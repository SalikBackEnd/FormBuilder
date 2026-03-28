using FormBuilder.Dtos;

namespace FormBuilder.Interfaces
{
    public interface IFormFieldService
    {
        Task<FormFieldDto> AddFieldAsync(Guid formId, CreateFormFieldRequest request, Guid userId);
        Task<FormFieldDto> UpdateFieldAsync(Guid formId, Guid fieldId, UpdateFormFieldRequest request, Guid userId);
        Task DeleteFieldAsync(Guid formId, Guid fieldId, Guid userId);
        Task ReorderFieldsAsync(Guid formId, ReorderFieldsRequest request, Guid userId);
    }
}
