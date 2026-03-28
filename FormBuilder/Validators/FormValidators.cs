using FluentValidation;
using FormBuilder.Dtos;

namespace FormBuilder.Validators
{
    public class CreateFormRequestValidator : AbstractValidator<CreateFormRequest>
    {
        public CreateFormRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        }
    }

    public class CreateFormFieldRequestValidator : AbstractValidator<CreateFormFieldRequest>
    {
        public CreateFormFieldRequestValidator()
        {
            RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
            RuleFor(x => x.FieldType).NotEmpty();
        }
    }
}
