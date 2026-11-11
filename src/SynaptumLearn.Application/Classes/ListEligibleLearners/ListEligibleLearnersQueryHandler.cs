
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Classes.ListEligibleLearners;

public sealed class ListEligibleLearnersQueryHandler
    : IRequestHandler<
        ListEligibleLearnersQuery,
        IReadOnlyList<EligibleLearnerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public ListEligibleLearnersQueryHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<IReadOnlyList<EligibleLearnerDto>> Handle(
        ListEligibleLearnersQuery request,
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
                "The selected class does not belong to your school.");
        }

        return await _context.LearnerProfiles
            .AsNoTracking()
            .Where(l =>
                l.SchoolId == schoolId &&
                l.GradeId == schoolClass.GradeId &&
                l.User.IsActive &&
                _context.SchoolMemberships.Any(m =>
                    m.SchoolId == schoolId &&
                    m.UserId == l.UserId &&
                    m.Role == SchoolRole.Learner &&
                    m.IsActive) &&
                !_context.LearnerClasses.Any(lc =>
                    lc.SchoolId == schoolId &&
                    lc.LearnerId == l.Id &&
                    lc.AcademicYear == schoolClass.AcademicYear &&
                    lc.LeftAt == null))
            .OrderBy(l => l.User.LastName)
            .ThenBy(l => l.User.FirstName)
            .Select(l => new EligibleLearnerDto(
                l.Id,
                l.User.FirstName + " " + l.User.LastName,
                l.LearnerNumber
            ))
            .ToListAsync(cancellationToken);
    }
}
