using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Schools.GetSchoolDetails;

public sealed class GetSchoolDetailsQueryHandler
    : IRequestHandler<GetSchoolDetailsQuery, SchoolDetailsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolDetailsQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolDetailsDto?> Handle(
        GetSchoolDetailsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Schools
            .AsNoTracking()
            .Where(x => x.Id == request.SchoolId)
            .Select(x => new SchoolDetailsDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                EMISNumber = x.EMISNumber,
                Email = x.Email,
                Phone = x.Phone,
                Province = x.Province,
                Status = x.Status,
                Slug = x.Slug,
                CreatedAt = x.CreatedAt,
                LastModifiedAt = x.LastModifiedAt
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}