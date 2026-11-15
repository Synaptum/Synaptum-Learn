
using FluentValidation;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.UpdateCurriculumAccessMode;

public sealed class UpdateCurriculumAccessModeCommandValidator
    : AbstractValidator<UpdateCurriculumAccessModeCommand>
{
    public UpdateCurriculumAccessModeCommandValidator()
    {
        RuleFor(x => x.AccessMode)
            .Must(mode =>
                mode == CurriculumAccessMode.Progressive ||
                mode == CurriculumAccessMode.FullYear)
            .WithMessage(
                "Select a valid curriculum access mode.");

        RuleFor(x => x.CurrentTerm)
            .InclusiveBetween(1, 4)
            .WithMessage("The current academic term must be between 1 and 4.");
    }
}
