using FluentValidation;
using FormBuilder.Dtos;

namespace FormBuilder.Validators
{
    public class SubmitFormRequestValidator : AbstractValidator<SubmitFormRequest>
    {
        public SubmitFormRequestValidator()
        {
            RuleFor(x => x.FormId).NotEmpty();
            RuleFor(x => x.SubmitterEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.SubmitterName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Values).NotNull();
        }
    }
}
