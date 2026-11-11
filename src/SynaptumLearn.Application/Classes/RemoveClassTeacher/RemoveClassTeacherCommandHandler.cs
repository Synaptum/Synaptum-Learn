
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Classes.RemoveClassTeacher;

public sealed class RemoveClassTeacherCommandHandler
    : IRequestHandler<RemoveClassTeacherCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public RemoveClassTeacherCommandHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task Handle(
        RemoveClassTeacherCommand request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        var assignment = await _context.TeacherClassSubjects
            .FirstOrDefaultAsync(x =>
                x.Id == request.AssignmentId &&
                x.SchoolId == schoolId &&
                x.Class.SchoolId == schoolId &&
                x.TeacherSubject.SchoolId == schoolId,
                cancellationToken);

        if (assignment is null)
        {
            throw new InvalidOperationException(
                "The teacher-class assignment was not found in your school.");
        }

        _context.TeacherClassSubjects.Remove(assignment);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
