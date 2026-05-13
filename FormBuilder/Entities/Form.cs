namespace FormBuilder.Entities
{
    public class Form:BaseEntity
    {
        public Guid OwnerUserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PublicSlug { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public bool CollectSubmitterName { get; set; } = true;
        public bool CollectSubmitterEmail { get; set; } = true;
        public bool SubmitterNameRequired { get; set; } = false;
        public bool SubmitterEmailRequired { get; set; } = false;
    }
}
