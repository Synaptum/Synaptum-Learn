using MediatR;

namespace SynaptumLearn.Application.Curriculum.LearnerCurriculum;

public sealed record GetLearnerCurriculumQuery()
    : IRequest<LearnerCurriculumDto>;