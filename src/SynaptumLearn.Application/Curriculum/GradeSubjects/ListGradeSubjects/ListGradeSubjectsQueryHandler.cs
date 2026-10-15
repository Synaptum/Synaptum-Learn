using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.GradeSubjects.ListGradeSubjects;

public sealed class ListGradeSubjectsQueryHandler
    : IRequestHandler<ListGradeSubjectsQuery,
        IReadOnlyList<GradeSubjectListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public ListGradeSubjectsQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GradeSubjectListItemDto>> Handle(
        ListGradeSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.GradeSubjects
            .AsNoTracking()
            .OrderBy(x => x.Grade.Order)
            .ThenBy(x => x.Subject.Name)
            .Select(x => new GradeSubjectListItemDto
            {
                Id = x.Id,
                GradeId = x.GradeId,
                GradeName = x.Grade.Name,
                SubjectId = x.SubjectId,
                SubjectName = x.Subject.Name,
                SubjectCode = x.Subject.Code,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}