using FormBuilder.Dtos;
using FormBuilder.Entities;
using FormBuilder.Interfaces;

namespace FormBuilder.Services
{
    public class FormSubmissionService : IFormSubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FormSubmissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SubmitFormAsync(string formSlug, SubmitFormRequest request)
        {
            var form = (await _unitOfWork.Forms.FindAsync(f => f.PublicSlug == formSlug && f.IsPublished)).FirstOrDefault();
            if (form == null || form.Id != request.FormId)
                throw new Exception("Form not found, not published, or ID mismatch.");

            var submission = new FormSubmission
            {
                Id = Guid.NewGuid(),
                FormId = form.Id,
                SubmittedOn = DateTime.UtcNow,
                SubmitterEmail = request.SubmitterEmail,
                SubmitterName = request.SubmitterName
            };

            await _unitOfWork.FormSubmissions.AddAsync(submission);

            foreach (var val in request.Values)
            {
                var submissionValue = new FormSubmissionValue
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    FieldId = val.FieldId,
                    Value = val.Value ?? ""
                };
                await _unitOfWork.FormSubmissionValues.AddAsync(submissionValue);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<FormSubmissionDto>> GetSubmissionsAsync(Guid formId, Guid userId)
        {
            var form = await _unitOfWork.Forms.GetByIdAsync(formId);
            if (form == null || form.OwnerUserId != userId)
                throw new UnauthorizedAccessException("Form not found or access denied.");

            var submissions = await _unitOfWork.FormSubmissions.FindAsync(s => s.FormId == formId);
            
            var dtos = new List<FormSubmissionDto>();
            foreach(var sub in submissions)
            {
                var values = await _unitOfWork.FormSubmissionValues.FindAsync(v => v.SubmissionId == sub.Id);
                dtos.Add(MapToDto(sub, values));
            }

            return dtos.OrderByDescending(d => d.SubmittedOn);
        }

        public async Task<FormSubmissionDto> GetSubmissionByIdAsync(Guid formId, Guid submissionId, Guid userId)
        {
            var form = await _unitOfWork.Forms.GetByIdAsync(formId);
            if (form == null || form.OwnerUserId != userId)
                throw new UnauthorizedAccessException("Form not found or access denied.");

            var submission = await _unitOfWork.FormSubmissions.GetByIdAsync(submissionId);
            if (submission == null || submission.FormId != formId)
                throw new Exception("Submission not found.");

            var values = await _unitOfWork.FormSubmissionValues.FindAsync(v => v.SubmissionId == submissionId);
            return MapToDto(submission, values);
        }

        private FormSubmissionDto MapToDto(FormSubmission sub, IEnumerable<FormSubmissionValue> values)
        {
            return new FormSubmissionDto
            {
                Id = sub.Id,
                SubmittedOn = sub.SubmittedOn,
                SubmitterEmail = sub.SubmitterEmail,
                SubmitterName = sub.SubmitterName,
                Values = values.Select(v => new SubmissionValueDto
                {
                    FieldId = v.FieldId,
                    Value = v.Value
                }).ToList()
            };
        }
    }
}
