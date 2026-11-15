
using MediatR;

namespace SynaptumLearn.Application.Schools.GetCurriculumAccessMode;

public sealed record GetCurriculumAccessModeQuery()
    : IRequest<CurriculumSettingsDto>;
