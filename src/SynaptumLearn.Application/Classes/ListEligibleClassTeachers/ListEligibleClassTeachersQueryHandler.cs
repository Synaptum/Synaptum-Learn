
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Classes.ListEligibleClassTeachers;

public sealed class ListEligibleClassTeachersQueryHandler
    : IRequestHandler<
        ListEligibleClassTeachersQuery,
        IReadOnlyList<EligibleClassTeacherDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public ListEligibleClassTeachersQueryHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<IReadOnlyList<EligibleClassTeacherDto>> Handle(
        ListEligibleClassTeachersQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

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


        var eligibleTeachers = await _context.TeacherSubjects
            .AsNoTracking()
            .Where(ts =>
                ts.SchoolId == schoolId &&
                ts.TeacherProfile.SchoolId == schoolId &&
                ts.TeacherProfile.User.IsActive &&
                _context.SchoolMemberships.Any(m =>
                    m.SchoolId == schoolId &&
                    m.UserId == ts.TeacherProfile.UserId &&
                    m.Role == SchoolRole.Teacher &&
                    m.IsActive) &&
                ts.GradeSubject.GradeId == schoolClass.GradeId &&
                !_context.TeacherClassSubjects.Any(tcs =>
                    tcs.SchoolId == schoolId &&
                    tcs.ClassId == schoolClass.Id &&
                    tcs.TeacherSubjectId == ts.Id))
            .OrderBy(ts => ts.GradeSubject.Subject.Name)
            .ThenBy(ts => ts.TeacherProfile.User.FirstName)
            .ThenBy(ts => ts.TeacherProfile.User.LastName)
            .Select(ts => new EligibleClassTeacherDto(
                ts.Id,
                ts.TeacherProfile.User.FirstName + " " +
                    ts.TeacherProfile.User.LastName,
                ts.GradeSubject.Subject.Name))
            .ToListAsync(cancellationToken);


        return eligibleTeachers;
    }
}
