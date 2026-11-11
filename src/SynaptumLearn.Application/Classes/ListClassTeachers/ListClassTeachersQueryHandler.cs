
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Classes.ListClassTeachers;

public sealed class ListClassTeachersQueryHandler
    : IRequestHandler<ListClassTeachersQuery, IReadOnlyList<ClassTeacherDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public ListClassTeachersQueryHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<IReadOnlyList<ClassTeacherDto>> Handle(
        ListClassTeachersQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        var classExists = await _context.Classes
            .AsNoTracking()
            .AnyAsync(c =>
                c.Id == request.ClassId &&
                c.SchoolId == schoolId,
                cancellationToken);

        if (!classExists)
        {
            throw new InvalidOperationException(
                "The selected class was not found in your school.");
        }

        return await _context.TeacherClassSubjects
            .AsNoTracking()
            .Where(tcs =>
                tcs.SchoolId == schoolId &&
                tcs.ClassId == request.ClassId &&
                tcs.TeacherSubject.SchoolId == schoolId &&
                tcs.TeacherSubject.TeacherProfile.SchoolId == schoolId)
            .OrderBy(tcs =>
                tcs.TeacherSubject.GradeSubject.Subject.Name)
            .ThenBy(tcs =>
                tcs.TeacherSubject.TeacherProfile.User.LastName)
            .Select(tcs => new ClassTeacherDto(
                tcs.Id,
                tcs.TeacherSubject.TeacherProfile.User.FirstName + " " +
                    tcs.TeacherSubject.TeacherProfile.User.LastName,
                tcs.TeacherSubject.GradeSubject.Subject.Name))
            .ToListAsync(cancellationToken);
    }
}
