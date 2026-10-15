using MediatR;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Application.Curriculum.GradeSubjects.AssignSubjectToGrade;

public sealed class AssignSubjectToGradeCommandHandler
    : IRequestHandler<AssignSubjectToGradeCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AssignSubjectToGradeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        AssignSubjectToGradeCommand request,
        CancellationToken cancellationToken)
    {
        var gradeSubject = new GradeSubject
        {
            GradeId = request.GradeId,
            SubjectId = request.SubjectId
        };

        _context.GradeSubjects.Add(gradeSubject);

        await _context.SaveChangesAsync(cancellationToken);

        return gradeSubject.Id;
    }
}