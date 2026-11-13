
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;

namespace SynaptumLearn.Application.Curriculum.Topics.ListTopics;

public sealed class ListTopicsQueryHandler
    : IRequestHandler<ListTopicsQuery,
        IReadOnlyList<TopicListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListTopicsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TopicListItemDto>> Handle(
        ListTopicsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can manage curriculum topics.");
        }

        return await _context.Topics
            .AsNoTracking()
            .OrderBy(x => x.GradeSubject.Grade.Order)
            .ThenBy(x => x.GradeSubject.Subject.Name)
            .ThenBy(x => x.Term)
            .ThenBy(x => x.Order)
            .Select(x => new TopicListItemDto
            {
                Id = x.Id,
                GradeSubjectId = x.GradeSubjectId,
                GradeName = x.GradeSubject.Grade.Name,
                SubjectName = x.GradeSubject.Subject.Name,
                SubjectCode = x.GradeSubject.Subject.Code,
                Title = x.Title,
                Description = x.Description,
                Term = x.Term,
                Order = x.Order,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
