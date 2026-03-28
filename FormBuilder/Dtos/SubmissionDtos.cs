namespace FormBuilder.Dtos
{
    public class SubmitFormRequest
    {
        public Guid FormId { get; set; }
        public string SubmitterEmail { get; set; } = string.Empty;
        public string SubmitterName { get; set; } = string.Empty;
        public List<SubmissionValueDto> Values { get; set; } = new();
    }

    public class SubmissionValueDto
    {
        public Guid FieldId { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class FormSubmissionDto
    {
        public Guid Id { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string SubmitterEmail { get; set; } = string.Empty;
        public string SubmitterName { get; set; } = string.Empty;
        public List<SubmissionValueDto> Values { get; set; } = new();
    }
}
