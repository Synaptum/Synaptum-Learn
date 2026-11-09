
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Teachers.RemoveTeacherSubject;

public sealed class RemoveTeacherSubjectCommandHandler
    : IRequestHandler<RemoveTeacherSubjectCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public RemoveTeacherSubjectCommandHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task Handle(
        RemoveTeacherSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        var assignment = await _context.TeacherSubjects
            .FirstOrDefaultAsync(ts =>
                ts.Id == request.AssignmentId &&
                ts.SchoolId == schoolId &&
                ts.TeacherProfile.SchoolId == schoolId,
                cancellationToken);

        if (assignment is null)
        {
            throw new InvalidOperationException(
                "The selected teacher assignment was not found.");
        }

        _context.TeacherSubjects.Remove(assignment);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
