namespace FormBuilder.Entities
{
    public class FormSubmissionValue
    {
        public Guid Id { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid FieldId { get; set; }
        public string Value { get; set; }
    }
}
