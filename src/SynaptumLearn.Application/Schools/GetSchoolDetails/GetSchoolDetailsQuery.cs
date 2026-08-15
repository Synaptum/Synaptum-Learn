using MediatR;

namespace SynaptumLearn.Application.Schools.GetSchoolDetails;

public sealed record GetSchoolDetailsQuery(Guid SchoolId)
    : IRequest<SchoolDetailsDto?>;