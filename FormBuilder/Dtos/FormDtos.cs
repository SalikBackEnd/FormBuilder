namespace FormBuilder.Dtos
{
    public class FormDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PublicSlug { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public List<FormFieldDto> Fields { get; set; } = new();
    }

    public class CreateFormRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateFormRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
