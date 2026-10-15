using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Subjects.GetSubjectDetails;

public sealed class GetSubjectDetailsQueryHandler
    : IRequestHandler<GetSubjectDetailsQuery, SubjectDetailsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSubjectDetailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubjectDetailsDto?> Handle(
        GetSubjectDetailsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Subjects
            .AsNoTracking()
            .Where(x => x.Id == request.SubjectId)
            .Select(x => new SubjectDetailsDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                CreatedAt = x.CreatedAt,
                LastModifiedAt = x.LastModifiedAt
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}