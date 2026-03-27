namespace FormBuilder.Entities
{
    public class FormField:BaseEntity
    {
        public Guid FormId { get; set; }
        public string Label { get; set; }
        public string FieldType { get; set; }
        public string Placeholder { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public string OptionsJson { get; set; }
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
    }
}
