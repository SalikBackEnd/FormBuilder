namespace FormBuilder.Entities
{
    public class FormSubmission
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string SubmitterEmail { get; set; }
        public string SubmitterName { get; set; }
    }
}
