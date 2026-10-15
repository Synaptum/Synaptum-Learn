using MediatR;

namespace SynaptumLearn.Application.Curriculum.Subjects.GetSubjectDetails;

public sealed record GetSubjectDetailsQuery(Guid SubjectId)
    : IRequest<SubjectDetailsDto?>;