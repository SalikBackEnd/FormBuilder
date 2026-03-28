namespace FormBuilder.Dtos
{
    public class FormFieldDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string Placeholder { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<string> Options { get; set; } = new();
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
    }

    public class CreateFormFieldRequest
    {
        public string Label { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string Placeholder { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
    }

    public class UpdateFormFieldRequest : CreateFormFieldRequest
    {
    }

    public class ReorderFieldsRequest
    {
        public Dictionary<Guid, int> FieldOrders { get; set; } = new();
    }
}
