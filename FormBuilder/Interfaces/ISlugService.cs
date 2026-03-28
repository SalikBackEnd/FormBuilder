namespace FormBuilder.Interfaces
{
    public interface ISlugService
    {
        Task<string> GenerateUniqueSlugAsync(string title);
    }
}
