namespace FormBuilder.Entities
{
    public class FormField:BaseEntity
    {
        public Guid FormId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public string OptionsJson { get; set; } = string.Empty;
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
    }
}
