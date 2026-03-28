using FormBuilder.Entities;

namespace FormBuilder.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Form> Forms { get; }
        IRepository<FormField> FormFields { get; }
        IRepository<FormSubmission> FormSubmissions { get; }
        IRepository<FormSubmissionValue> FormSubmissionValues { get; }
        IRepository<RefreshToken> RefreshTokens { get; }

        Task<int> SaveChangesAsync();
    }
}
