
using MediatR;

namespace SynaptumLearn.Application.Classes.CreateClass;

public sealed record CreateClassCommand(
    string Name,
    Guid GradeId,
    int AcademicYear
) : IRequest<Guid>;
