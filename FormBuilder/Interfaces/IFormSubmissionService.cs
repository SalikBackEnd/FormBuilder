using FormBuilder.Dtos;

namespace FormBuilder.Interfaces
{
    public interface IFormSubmissionService
    {
        Task SubmitFormAsync(string formSlug, SubmitFormRequest request);
        Task<IEnumerable<FormSubmissionDto>> GetSubmissionsAsync(Guid formId, Guid userId);
        Task<FormSubmissionDto> GetSubmissionByIdAsync(Guid formId, Guid submissionId, Guid userId);
    }
}
