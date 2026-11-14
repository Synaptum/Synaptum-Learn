
using MediatR;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.GetCurriculumAccessMode;

public sealed record GetCurriculumAccessModeQuery()
    : IRequest<CurriculumAccessMode>;
