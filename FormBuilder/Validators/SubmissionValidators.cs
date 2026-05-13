using FluentValidation;
using FormBuilder.Dtos;

namespace FormBuilder.Validators
{
    public class SubmitFormRequestValidator : AbstractValidator<SubmitFormRequest>
    {
        public SubmitFormRequestValidator()
        {
            RuleFor(x => x.FormId).NotEmpty();
            RuleFor(x => x.SubmitterEmail)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.SubmitterEmail))
                .WithMessage("Please enter a valid email address.");
            RuleFor(x => x.SubmitterName)
                .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.SubmitterName))
                .WithMessage("Name cannot exceed 100 characters.");
            RuleFor(x => x.Values).NotNull();
        }
    }
}
