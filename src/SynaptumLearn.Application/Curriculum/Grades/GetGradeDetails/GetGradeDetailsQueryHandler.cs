using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Grades.GetGradeDetails;

public sealed class GetGradeDetailsQueryHandler
    : IRequestHandler<GetGradeDetailsQuery, GradeDetailsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetGradeDetailsQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GradeDetailsDto?> Handle(
        GetGradeDetailsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Grades
            .AsNoTracking()
            .Where(x => x.Id == request.GradeId)
            .Select(x => new GradeDetailsDto
            {
                Id = x.Id,
                Name = x.Name,
                Order = x.Order,
                CreatedAt = x.CreatedAt,
                LastModifiedAt = x.LastModifiedAt
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}