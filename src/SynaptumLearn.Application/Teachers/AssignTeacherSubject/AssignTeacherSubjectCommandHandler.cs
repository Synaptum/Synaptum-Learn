
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Teachers.AssignTeacherSubject;

public sealed class AssignTeacherSubjectCommandHandler
    : IRequestHandler<AssignTeacherSubjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public AssignTeacherSubjectCommandHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<Guid> Handle(
        AssignTeacherSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);


        var teacherExists = await _context.TeacherProfiles
            .AsNoTracking()
            .AnyAsync(t =>
                t.Id == request.TeacherProfileId &&
                t.SchoolId == schoolId &&
                t.User.IsActive &&
                _context.SchoolMemberships.Any(m =>
                    m.SchoolId == schoolId &&
                    m.UserId == t.UserId &&
                    m.Role == SchoolRole.Teacher &&
                    m.IsActive),
                cancellationToken);


        if (!teacherExists)
        {
            throw new InvalidOperationException(
                "The selected teacher does not belong to your active school.");
        }

        var gradeSubjectExists = await _context.GradeSubjects
            .AsNoTracking()
            .AnyAsync(gs => gs.Id == request.GradeSubjectId,
                cancellationToken);

        if (!gradeSubjectExists)
        {
            throw new InvalidOperationException(
                "The selected grade-subject combination does not exist.");
        }

        var alreadyAssigned = await _context.TeacherSubjects
            .AsNoTracking()
            .AnyAsync(ts =>
                ts.SchoolId == schoolId &&
                ts.TeacherProfileId == request.TeacherProfileId &&
                ts.GradeSubjectId == request.GradeSubjectId,
                cancellationToken);

        if (alreadyAssigned)
        {
            throw new InvalidOperationException(
                "This teacher is already assigned to the selected subject and grade.");
        }

        var assignment = new TeacherSubject
        {
            SchoolId = schoolId,
            TeacherProfileId = request.TeacherProfileId,
            GradeSubjectId = request.GradeSubjectId
        };

        _context.TeacherSubjects.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return assignment.Id;
    }
}
