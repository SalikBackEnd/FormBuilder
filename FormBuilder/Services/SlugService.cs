using FormBuilder.Interfaces;
using System.Text.RegularExpressions;

namespace FormBuilder.Services
{
    public class SlugService : ISlugService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SlugService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateUniqueSlugAsync(string title)
        {
            string baseSlug = Slugify(title);
            string slug = $"{baseSlug}-{GenerateRandomCode(6)}";

            // Ensure uniqueness
            while (true)
            {
                var exists = (await _unitOfWork.Forms.FindAsync(f => f.PublicSlug == slug)).Any();
                if (!exists)
                    return slug;

                // Collision occurred (unlikely), generate another random code
                slug = $"{baseSlug}-{GenerateRandomCode(6)}";
            }
        }

        private string Slugify(string text)
        {
            string str = text.ToLowerInvariant();
            
            // Remove invalid characters
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            
            // Convert multiple spaces to a single sequence
            str = Regex.Replace(str, @"\s+", " ").Trim();
            
            // Replace space with hyphen
            str = Regex.Replace(str, @"\s", "-");
            
            return str;
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
