
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetAssessmentIdByQuestion;

public sealed class GetAssessmentIdByQuestionQueryHandler
    : IRequestHandler<GetAssessmentIdByQuestionQuery, Guid?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAssessmentIdByQuestionQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid?> Handle(
        GetAssessmentIdByQuestionQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can manage assessment questions.");
        }

        return await _context.Questions
            .AsNoTracking()
            .Where(x =>
                x.Id == request.QuestionId &&
                x.Assessment.IsRequiredForProgression)
            .Select(x => (Guid?)x.AssessmentId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
