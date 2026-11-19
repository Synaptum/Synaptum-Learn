
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetMasteryAssessmentByLesson;

public sealed class GetMasteryAssessmentByLessonQueryHandler
    : IRequestHandler<GetMasteryAssessmentByLessonQuery, Guid?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMasteryAssessmentByLessonQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid?> Handle(
        GetMasteryAssessmentByLessonQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can manage mastery assessments.");
        }

        if (request.LessonId == Guid.Empty)
            return null;

        return await _context.Assessments
            .AsNoTracking()
            .Where(x =>
                x.LessonId == request.LessonId &&
                x.IsRequiredForProgression)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
