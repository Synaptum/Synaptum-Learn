
using MediatR;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.UpdateCurriculumAccessMode;

public sealed record UpdateCurriculumAccessModeCommand(
    CurriculumAccessMode AccessMode,
    int CurrentTerm
) : IRequest;
