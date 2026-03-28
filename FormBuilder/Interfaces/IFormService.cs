using FormBuilder.Dtos;

namespace FormBuilder.Interfaces
{
    public interface IFormService
    {
        Task<FormDto> GetFormByIdAsync(Guid formId, Guid userId);
        Task<FormDto> GetFormBySlugAsync(string slug);
        Task<IEnumerable<FormDto>> GetFormsByUserAsync(Guid userId);
        Task<FormDto> CreateFormAsync(CreateFormRequest request, Guid userId);
        Task<FormDto> UpdateFormAsync(Guid formId, UpdateFormRequest request, Guid userId);
        Task DeleteFormAsync(Guid formId, Guid userId);
        Task PublishFormAsync(Guid formId, Guid userId);
        Task UnpublishFormAsync(Guid formId, Guid userId);
    }
}
