using FormBuilder.Dtos;
using FormBuilder.Entities;
using FormBuilder.Exceptions;
using FormBuilder.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

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
                throw new NotFoundException("Form not found or is not published.");

            var fields = (await _unitOfWork.FormFields.FindAsync(f => f.FormId == form.Id)).ToList();
            var valueMap = request.Values.ToDictionary(v => v.FieldId, v => v.Value ?? "");

            // Validate each field's submitted value against its configuration
            foreach (var field in fields)
            {
                valueMap.TryGetValue(field.Id, out var value);
                value ??= "";

                if (field.IsRequired && string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException($"'{field.Label}' is required.");

                if (string.IsNullOrWhiteSpace(value))
                    continue; // Optional field with no value — skip further checks

                switch (field.FieldType)
                {
                    case "Email":
                        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                            throw new ArgumentException($"'{field.Label}' must be a valid email address.");
                        break;

                    case "Number":
                        if (!decimal.TryParse(value, out decimal numVal))
                            throw new ArgumentException($"'{field.Label}' must be a number.");
                        if (field.MinValue != 0 && numVal < field.MinValue)
                            throw new ArgumentException($"'{field.Label}' must be at least {field.MinValue}.");
                        if (field.MaxValue != 0 && numVal > field.MaxValue)
                            throw new ArgumentException($"'{field.Label}' must be at most {field.MaxValue}.");
                        break;

                    case "Date":
                        if (!DateTime.TryParse(value, out _))
                            throw new ArgumentException($"'{field.Label}' must be a valid date.");
                        break;

                    case "Text":
                    case "TextArea":
                        if (field.MinLength > 0 && value.Length < field.MinLength)
                            throw new ArgumentException($"'{field.Label}' must be at least {field.MinLength} characters.");
                        if (field.MaxLength > 0 && value.Length > field.MaxLength)
                            throw new ArgumentException($"'{field.Label}' must be at most {field.MaxLength} characters.");
                        break;

                    case "Dropdown":
                    case "Radio":
                        var allowedOptions = string.IsNullOrEmpty(field.OptionsJson)
                            ? new List<string>()
                            : JsonSerializer.Deserialize<List<string>>(field.OptionsJson) ?? new List<string>();
                        if (!allowedOptions.Contains(value))
                            throw new ArgumentException($"'{field.Label}' contains an invalid option.");
                        break;

                    case "Checkbox":
                        var checkboxOptions = string.IsNullOrEmpty(field.OptionsJson)
                            ? new List<string>()
                            : JsonSerializer.Deserialize<List<string>>(field.OptionsJson) ?? new List<string>();
                        var selectedOptions = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        foreach (var selected in selectedOptions)
                        {
                            if (!checkboxOptions.Contains(selected))
                                throw new ArgumentException($"'{field.Label}' contains an invalid option: '{selected}'.");
                        }
                        break;
                }
            }

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
                throw new NotFoundException("Submission not found.");

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
