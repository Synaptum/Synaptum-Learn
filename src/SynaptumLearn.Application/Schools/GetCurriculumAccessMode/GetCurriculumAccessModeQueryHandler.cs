
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.GetCurriculumAccessMode;

public sealed class GetCurriculumAccessModeQueryHandler
    : IRequestHandler<GetCurriculumAccessModeQuery, CurriculumAccessMode>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _schoolAdminAccessService;

    public GetCurriculumAccessModeQueryHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService schoolAdminAccessService)
    {
        _context = context;
        _schoolAdminAccessService = schoolAdminAccessService;
    }

    public async Task<CurriculumAccessMode> Handle(
        GetCurriculumAccessModeQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _schoolAdminAccessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        var school = await _context.Schools
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.Id == schoolId,
                cancellationToken);

        if (school is null)
        {
            throw new InvalidOperationException(
                "The authorized school could not be found.");
        }

        return school.CurriculumAccessMode;
    }
}
