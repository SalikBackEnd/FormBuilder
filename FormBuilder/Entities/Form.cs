namespace FormBuilder.Entities
{
    public class Form:BaseEntity
    {
        public Guid OwnerUserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PublicSlug { get; set; }
        public bool IsPublished { get; set; }
        
    }
}
