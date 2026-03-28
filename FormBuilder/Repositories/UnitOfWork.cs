using FormBuilder.Context;
using FormBuilder.Entities;
using FormBuilder.Interfaces;

namespace FormBuilder.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FormBuilderContext _context;
        private IRepository<User>? _users;
        private IRepository<Form>? _forms;
        private IRepository<FormField>? _formFields;
        private IRepository<FormSubmission>? _formSubmissions;
        private IRepository<FormSubmissionValue>? _formSubmissionValues;
        private IRepository<RefreshToken>? _refreshTokens;

        public UnitOfWork(FormBuilderContext context)
        {
            _context = context;
        }

        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<Form> Forms => _forms ??= new Repository<Form>(_context);
        public IRepository<FormField> FormFields => _formFields ??= new Repository<FormField>(_context);
        public IRepository<FormSubmission> FormSubmissions => _formSubmissions ??= new Repository<FormSubmission>(_context);
        public IRepository<FormSubmissionValue> FormSubmissionValues => _formSubmissionValues ??= new Repository<FormSubmissionValue>(_context);
        public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
