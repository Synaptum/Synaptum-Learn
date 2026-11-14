
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
    }
}
