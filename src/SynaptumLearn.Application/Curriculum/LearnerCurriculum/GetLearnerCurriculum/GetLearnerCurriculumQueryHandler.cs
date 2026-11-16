
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Curriculum.LearnerCurriculum;

public sealed class GetLearnerCurriculumQueryHandler
    : IRequestHandler<GetLearnerCurriculumQuery, LearnerCurriculumDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLearnerCurriculumQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<LearnerCurriculumDto> Handle(
        GetLearnerCurriculumQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAccessException(
                "An authenticated learner account is required.");
        }

        if (_currentUser.SchoolId is not Guid currentSchoolId)
        {
            throw new UnauthorizedAccessException(
                "An active school context is required.");
        }

        var learner = await (
            from profile in _context.LearnerProfiles.AsNoTracking()
            join membership in _context.SchoolMemberships.AsNoTracking()
                on new { profile.UserId, profile.SchoolId }
                equals new { membership.UserId, membership.SchoolId }
            join school in _context.Schools.AsNoTracking()
                on profile.SchoolId equals school.Id
            join user in _context.BusinessUsers.AsNoTracking()
                on profile.UserId equals user.Id
            where profile.UserId == userId
                && profile.SchoolId == currentSchoolId
                && membership.IsActive
                && membership.Role == SchoolRole.Learner
                && school.Status == SchoolStatus.Active
                && user.IsActive
            select new
            {
                profile.GradeId,
                profile.SchoolId,
                school.CurriculumAccessMode,
                school.CurrentTerm
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (learner is null)
        {
            throw new UnauthorizedAccessException(
                "No active learner profile and school membership were found.");
        }

        if (learner.CurrentTerm is < 1 or > 4 ||
            learner.CurriculumAccessMode is not (
                CurriculumAccessMode.Progressive or
                CurriculumAccessMode.FullYear))
        {
            throw new InvalidOperationException(
                "The school's curriculum settings are invalid.");
        }

        var visibleTerm = learner.CurriculumAccessMode ==
                          CurriculumAccessMode.FullYear
            ? 4
            : learner.CurrentTerm;

        var subjects = await _context.GradeSubjects
            .AsNoTracking()
            .Where(gs => gs.GradeId == learner.GradeId)
            .OrderBy(gs => gs.Subject.Name)
            .Select(gs => new LearnerSubjectDto(
                gs.Id,
                gs.SubjectId,
                gs.Subject.Name,
                gs.Subject.Code,
                gs.Topics
                    .Where(t => t.Term >= 1 && t.Term <= visibleTerm)
                    .OrderBy(t => t.Term)
                    .ThenBy(t => t.Order)
                    .Select(t => new LearnerTopicDto(
                        t.Id,
                        t.Title,
                        t.Description,
                        t.Term,
                        t.Order
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        return new LearnerCurriculumDto(
            learner.GradeId,
            learner.CurriculumAccessMode,
            learner.CurrentTerm,
            subjects
        );
    }
}
