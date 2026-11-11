
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Application.Common.Interfaces;


namespace SynaptumLearn.Application.Classes.AssignTeacher;

public sealed class AssignTeacherToClassCommandHandler
    : IRequestHandler<AssignTeacherToClassCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public AssignTeacherToClassCommandHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<Guid> Handle(
        AssignTeacherToClassCommand request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        var teacherSubject = await _context.TeacherSubjects
            .AsNoTracking()
            .Include(ts => ts.GradeSubject)
            .FirstOrDefaultAsync(ts =>
                ts.Id == request.TeacherSubjectId &&
                ts.SchoolId == schoolId &&
                ts.TeacherProfile.SchoolId == schoolId &&
                ts.TeacherProfile.User.IsActive &&
                _context.SchoolMemberships.Any(m =>
                    m.SchoolId == schoolId &&
                    m.UserId == ts.TeacherProfile.UserId &&
                    m.Role == SchoolRole.Teacher &&
                    m.IsActive),
                cancellationToken);

        if (teacherSubject is null)
        {
            throw new InvalidOperationException(
                "The teacher-subject assignment was not found in your school.");
        }

        var schoolClass = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == request.ClassId &&
                c.SchoolId == schoolId,
                cancellationToken);

        if (schoolClass is null)
        {
            throw new InvalidOperationException(
                "The class was not found in your school.");
        }

        if (teacherSubject.GradeSubject.GradeId != schoolClass.GradeId)
        {
            throw new InvalidOperationException(
                "The teacher's assigned subject does not match the class grade.");
        }

        var alreadyAssigned = await _context.TeacherClassSubjects
            .AnyAsync(x =>
                x.SchoolId == schoolId &&
                x.TeacherSubjectId == request.TeacherSubjectId &&
                x.ClassId == request.ClassId,
                cancellationToken);

        if (alreadyAssigned)
        {
            throw new InvalidOperationException(
                "This teacher is already assigned to the selected class and subject.");
        }

        var assignment = new TeacherClassSubject
        {
            SchoolId = schoolId,
            TeacherSubjectId = request.TeacherSubjectId,
            ClassId = request.ClassId
        };

        _context.TeacherClassSubjects.Add(assignment);

        await _context.SaveChangesAsync(cancellationToken);

        return assignment.Id;
    }
}
