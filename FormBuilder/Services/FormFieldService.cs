using FormBuilder.Dtos;
using FormBuilder.Entities;
using FormBuilder.Interfaces;
using System.Text.Json;

namespace FormBuilder.Services
{
    public class FormFieldService : IFormFieldService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FormFieldService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private async Task ValidateFormOwnership(Guid formId, Guid userId)
        {
            var form = await _unitOfWork.Forms.GetByIdAsync(formId);
            if (form == null || form.OwnerUserId != userId)
                throw new UnauthorizedAccessException("Form not found or access denied.");
        }

        public async Task<FormFieldDto> AddFieldAsync(Guid formId, CreateFormFieldRequest request, Guid userId)
        {
            await ValidateFormOwnership(formId, userId);

            var existingFields = await _unitOfWork.FormFields.FindAsync(f => f.FormId == formId);
            int nextSortOrder = existingFields.Any() ? existingFields.Max(f => f.SortOrder) + 1 : 0;

            var field = new FormField
            {
                Id = Guid.NewGuid(),
                FormId = formId,
                Label = request.Label,
                FieldType = request.FieldType,
                IsRequired = request.IsRequired,
                Placeholder = request.Placeholder ?? "",
                SortOrder = nextSortOrder,
                OptionsJson = request.Options != null && request.Options.Any() ? JsonSerializer.Serialize(request.Options) : "",
                MinLength = request.MinLength,
                MaxLength = request.MaxLength,
                MinValue = request.MinValue,
                MaxValue = request.MaxValue,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _unitOfWork.FormFields.AddAsync(field);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(field);
        }

        public async Task<FormFieldDto> UpdateFieldAsync(Guid formId, Guid fieldId, UpdateFormFieldRequest request, Guid userId)
        {
            await ValidateFormOwnership(formId, userId);

            var field = await _unitOfWork.FormFields.GetByIdAsync(fieldId);
            if (field == null || field.FormId != formId)
                throw new Exception("Field not found.");

            field.Label = request.Label;
            field.FieldType = request.FieldType;
            field.IsRequired = request.IsRequired;
            field.Placeholder = request.Placeholder ?? "";
            field.OptionsJson = request.Options != null && request.Options.Any() ? JsonSerializer.Serialize(request.Options) : "";
            field.MinLength = request.MinLength;
            field.MaxLength = request.MaxLength;
            field.MinValue = request.MinValue;
            field.MaxValue = request.MaxValue;
            field.UpdatedOn = DateTime.UtcNow;
            field.UpdatedBy = userId;

            _unitOfWork.FormFields.Update(field);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(field);
        }

        public async Task DeleteFieldAsync(Guid formId, Guid fieldId, Guid userId)
        {
            await ValidateFormOwnership(formId, userId);

            var field = await _unitOfWork.FormFields.GetByIdAsync(fieldId);
            if (field == null || field.FormId != formId)
                throw new Exception("Field not found.");

            _unitOfWork.FormFields.Remove(field);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ReorderFieldsAsync(Guid formId, ReorderFieldsRequest request, Guid userId)
        {
            await ValidateFormOwnership(formId, userId);

            var fields = await _unitOfWork.FormFields.FindAsync(f => f.FormId == formId);
            foreach (var field in fields)
            {
                if (request.FieldOrders.TryGetValue(field.Id, out int newSortOrder))
                {
                    field.SortOrder = newSortOrder;
                    _unitOfWork.FormFields.Update(field);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private FormFieldDto MapToDto(FormField f)
        {
            return new FormFieldDto
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
            };
        }
    }
}
