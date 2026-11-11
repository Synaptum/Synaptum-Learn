
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Application.Classes.TransferLearner;

public sealed class TransferLearnerCommandHandler
    : IRequestHandler<TransferLearnerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public TransferLearnerCommandHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<Guid> Handle(
        TransferLearnerCommand request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        await using var transaction =
            await _context.BeginTransactionAsync(cancellationToken);

        var enrollment =
            await _context.GetActiveEnrollmentForUpdateAsync(
                request.EnrollmentId,
                schoolId,
                cancellationToken);

        if (enrollment is null)
        {
            throw new InvalidOperationException(
                "The active enrollment was not found.");
        }

        var learner = await _context.LearnerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(l =>
                l.Id == enrollment.LearnerId &&
                l.SchoolId == schoolId,
                cancellationToken);

        if (learner is null)
        {
            throw new InvalidOperationException(
                "The learner does not belong to your school.");
        }

        var destination = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == request.DestinationClassId &&
                c.SchoolId == schoolId,
                cancellationToken);

        if (destination is null)
        {
            throw new InvalidOperationException(
                "The destination class was not found in your school.");
        }

        if (destination.Id == enrollment.ClassId)
        {
            throw new InvalidOperationException(
                "The learner is already enrolled in this class.");
        }

        if (destination.AcademicYear != enrollment.AcademicYear)
        {
            throw new InvalidOperationException(
                "Transfers must remain within the same academic year.");
        }

        if (destination.GradeId != learner.GradeId)
        {
            throw new InvalidOperationException(
                "The destination class must match the learner's grade.");
        }

        var transferTime = DateTime.UtcNow;

        enrollment.LeftAt = transferTime;

        // Close the old enrollment before inserting the new one.
        await _context.SaveChangesAsync(cancellationToken);

        var newEnrollment = new LearnerClass
        {
            SchoolId = schoolId,
            LearnerId = enrollment.LearnerId,
            ClassId = destination.Id,
            AcademicYear = enrollment.AcademicYear,
            EnrolledAt = transferTime
        };

        _context.LearnerClasses.Add(newEnrollment);

        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return newEnrollment.Id;
    }
}
