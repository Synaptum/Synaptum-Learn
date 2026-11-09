
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Teachers.ListTeacherSubjects;

public sealed class ListTeacherSubjectsQueryHandler
    : IRequestHandler<ListTeacherSubjectsQuery,
        IReadOnlyList<TeacherSubjectListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public ListTeacherSubjectsQueryHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<IReadOnlyList<TeacherSubjectListItemDto>> Handle(
        ListTeacherSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        return await _context.TeacherSubjects
            .AsNoTracking()
            .Where(ts =>
                ts.SchoolId == schoolId &&
                ts.TeacherProfile.SchoolId == schoolId)
            .OrderBy(ts => ts.TeacherProfile.User.LastName)
            .ThenBy(ts => ts.GradeSubject.Grade.Order)
            .ThenBy(ts => ts.GradeSubject.Subject.Name)
            .Select(ts => new TeacherSubjectListItemDto(
                ts.Id,
                ts.TeacherProfileId,
                ts.TeacherProfile.User.FirstName + " " +
                    ts.TeacherProfile.User.LastName,
                ts.TeacherProfile.EmployeeNumber,
                ts.GradeSubject.Grade.Name,
                ts.GradeSubject.Subject.Name))
            .ToListAsync(cancellationToken);
    }
}
