using FormBuilder.Dtos;
using FormBuilder.Entities;
using FormBuilder.Interfaces;
using System.Text.Json;

namespace FormBuilder.Services
{
    public class FormService : IFormService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlugService _slugService;

        public FormService(IUnitOfWork unitOfWork, ISlugService slugService)
        {
            _unitOfWork = unitOfWork;
            _slugService = slugService;
        }

        public async Task<FormDto> GetFormByIdAsync(Guid formId, Guid userId)
        {
            var form = await _unitOfWork.Forms.GetByIdAsync(formId);
            if (form == null || form.OwnerUserId != userId)
                throw new UnauthorizedAccessException("Form not found or access denied.");

            var fields = await _unitOfWork.FormFields.FindAsync(f => f.FormId == formId);
            return MapToDto(form, fields.OrderBy(f => f.SortOrder));
        }

        public async Task<FormDto> GetFormBySlugAsync(string slug)
        {
            var form = (await _unitOfWork.Forms.FindAsync(f => f.PublicSlug == slug && f.IsPublished)).FirstOrDefault();
            if (form == null)
                throw new Exception("Form not found or is not published.");

            var fields = await _unitOfWork.FormFields.FindAsync(f => f.FormId == form.Id);
            return MapToDto(form, fields.OrderBy(f => f.SortOrder));
        }

        public async Task<IEnumerable<FormDto>> GetFormsByUserAsync(Guid userId)
        {
            var forms = await _unitOfWork.Forms.FindAsync(f => f.OwnerUserId == userId);
            return forms.OrderByDescending(f => f.CreatedOn).Select(f => MapToDto(f, new List<FormField>()));
        }

        public async Task<FormDto> CreateFormAsync(CreateFormRequest request, Guid userId)
        {
            var slug = await _slugService.GenerateUniqueSlugAsync(request.Title);

            var form = new Form
            {
                Id = Guid.NewGuid(),
                OwnerUserId = userId,
                Title = request.Title,
                Description = request.Description ?? "",
                PublicSlug = slug,
                IsPublished = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _unitOfWork.Forms.AddAsync(form);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(form, new List<FormField>());
        }

        public async Task<FormDto> UpdateFormAsync(Guid formId, UpdateFormRequest request, Guid userId)
        {
            var form = await _unitOfWork.Forms.GetByIdAsync(formId);
            if (form == null || form.OwnerUserId != userId)
                throw new UnauthorizedAccessException("Form not found or access denied.");

            form.Title = request.Title;
            form.Description = request.Description ?? "";
            form.UpdatedOn = DateTime.UtcNow;
            form.UpdatedBy = userId;

            _unitOfWork.Forms.Update(form);
            await _unitOfWork.SaveChangesAsync();

            var fields = await _unitOfWork.FormFields.FindAsync(f => f.FormId == formId);
            return MapToDto(form, fields.OrderBy(f => f.SortOrder));
        }

        public async Task DeleteFormAsync(Guid formId, Guid userId)
        {
            var form = await _unitOfWork.Forms.GetByIdAsync(formId);
            if (form == null || form.OwnerUserId != userId)
                throw new UnauthorizedAccessException("Form not found or access denied.");

            _unitOfWork.Forms.Remove(form);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task PublishFormAsync(Guid formId, Guid userId)
        {
            var form = await _unitOfWork.Forms.GetByIdAsync(formId);
            if (form == null || form.OwnerUserId != userId)
                throw new UnauthorizedAccessException("Form not found or access denied.");

            form.IsPublished = true;
            form.UpdatedOn = DateTime.UtcNow;
            form.UpdatedBy = userId;

            _unitOfWork.Forms.Update(form);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UnpublishFormAsync(Guid formId, Guid userId)
        {
            var form = await _unitOfWork.Forms.GetByIdAsync(formId);
            if (form == null || form.OwnerUserId != userId)
                throw new UnauthorizedAccessException("Form not found or access denied.");

            form.IsPublished = false;
            form.UpdatedOn = DateTime.UtcNow;
            form.UpdatedBy = userId;

            _unitOfWork.Forms.Update(form);
            await _unitOfWork.SaveChangesAsync();
        }

        private FormDto MapToDto(Form form, IEnumerable<FormField> fields)
        {
            return new FormDto
            {
                Id = form.Id,
                Title = form.Title,
                Description = form.Description,
                PublicSlug = form.PublicSlug,
                IsPublished = form.IsPublished,
                Fields = fields.Select(f => new FormFieldDto
                {
                    Id = f.Id,
                    Label = f.Label,
                    FieldType = f.FieldType,
                    IsRequired = f.IsRequired,
                    Placeholder = f.Placeholder,
                    SortOrder = f.SortOrder,
                    MaxLength = f.MaxLength,
                    MinLength = f.MinLength,
                    MaxValue = f.MaxValue,
                    MinValue = f.MinValue,
                    Options = string.IsNullOrEmpty(f.OptionsJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(f.OptionsJson) ?? new List<string>()
                }).ToList()
            };
        }
    }
}
