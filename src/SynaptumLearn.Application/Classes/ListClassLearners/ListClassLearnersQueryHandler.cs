
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Classes.ListClassLearners;

public sealed class ListClassLearnersQueryHandler
    : IRequestHandler<
        ListClassLearnersQuery,
        IReadOnlyList<ClassLearnerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public ListClassLearnersQueryHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<IReadOnlyList<ClassLearnerDto>> Handle(
        ListClassLearnersQuery request,
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
                "The selected class does not belong to your school.");
        }

        return await _context.LearnerClasses
            .AsNoTracking()
            .Where(lc =>
                lc.SchoolId == schoolId &&
                lc.ClassId == request.ClassId &&
                lc.LeftAt == null &&
                lc.Learner.SchoolId == schoolId)
            .OrderBy(lc => lc.Learner.User.LastName)
            .ThenBy(lc => lc.Learner.User.FirstName)
            .Select(lc => new ClassLearnerDto(
                lc.Id,
                lc.LearnerId,
                lc.Learner.LearnerNumber,
                lc.Learner.User.FirstName,
                lc.Learner.User.LastName,
                lc.EnrolledAt
            ))
            .ToListAsync(cancellationToken);
    }
}
