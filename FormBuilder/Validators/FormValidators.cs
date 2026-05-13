using FluentValidation;
using FormBuilder.Dtos;

namespace FormBuilder.Validators
{
    public class CreateFormRequestValidator : AbstractValidator<CreateFormRequest>
    {
        public CreateFormRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Form title is required.")
                .MaximumLength(200).WithMessage("Form title cannot exceed 200 characters (you entered {TotalLength}).");
        }
    }

    public class CreateFormFieldRequestValidator : AbstractValidator<CreateFormFieldRequest>
    {
        public CreateFormFieldRequestValidator()
        {
            RuleFor(x => x.Label)
                .NotEmpty().WithMessage("Field label is required.")
                .MaximumLength(200).WithMessage("Field label cannot exceed 200 characters (you entered {TotalLength}).");

            RuleFor(x => x.FieldType)
                .NotEmpty().WithMessage("Field type is required.");

            RuleFor(x => x.Options)
                .Must(opts => opts == null || opts.Count <= 20)
                .WithMessage("A field can have at most 20 options.");

            RuleForEach(x => x.Options)
                .MaximumLength(200)
                .WithMessage("Each option cannot exceed 200 characters. Please shorten the option text.");
        }
    }

    public class UpdateFormFieldRequestValidator : AbstractValidator<UpdateFormFieldRequest>
    {
        public UpdateFormFieldRequestValidator()
        {
            RuleFor(x => x.Label)
                .NotEmpty().WithMessage("Field label is required.")
                .MaximumLength(200).WithMessage("Field label cannot exceed 200 characters (you entered {TotalLength}).");

            RuleFor(x => x.FieldType)
                .NotEmpty().WithMessage("Field type is required.");

            RuleFor(x => x.Options)
                .Must(opts => opts == null || opts.Count <= 20)
                .WithMessage("A field can have at most 20 options.");

            RuleForEach(x => x.Options)
                .MaximumLength(200)
                .WithMessage("Each option cannot exceed 200 characters. Please shorten the option text.");
        }
    }
}
