
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Classes.EnrollLearner;

public sealed class EnrollLearnerCommandHandler
    : IRequestHandler<EnrollLearnerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public EnrollLearnerCommandHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<Guid> Handle(
        EnrollLearnerCommand request,
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

        var learner = await _context.LearnerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(l =>
                l.Id == request.LearnerProfileId &&
                l.SchoolId == schoolId &&
                l.User.IsActive &&
                _context.SchoolMemberships.Any(m =>
                    m.SchoolId == schoolId &&
                    m.UserId == l.UserId &&
                    m.Role == SchoolRole.Learner &&
                    m.IsActive),
                cancellationToken);

        if (learner is null)
        {
            throw new InvalidOperationException(
                "The selected learner is not active in your school.");
        }

        if (learner.GradeId != schoolClass.GradeId)
        {
            throw new InvalidOperationException(
                "The learner's grade does not match the selected class.");
        }

        var alreadyEnrolled = await _context.LearnerClasses
            .AsNoTracking()
            .AnyAsync(lc =>
                lc.SchoolId == schoolId &&
                lc.LearnerId == learner.Id &&
                lc.AcademicYear == schoolClass.AcademicYear &&
                lc.LeftAt == null,
                cancellationToken);

        if (alreadyEnrolled)
        {
            throw new InvalidOperationException(
                "This learner already has an active class enrollment for the selected academic year.");
        }

        var enrollment = new LearnerClass
        {
            SchoolId = schoolId,
            LearnerId = learner.Id,
            ClassId = schoolClass.Id,
            AcademicYear = schoolClass.AcademicYear,
            EnrolledAt = DateTime.UtcNow
        };

        _context.LearnerClasses.Add(enrollment);

        await _context.SaveChangesAsync(cancellationToken);

        return enrollment.Id;
    }
}
