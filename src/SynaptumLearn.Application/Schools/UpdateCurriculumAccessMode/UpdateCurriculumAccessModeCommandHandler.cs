
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Schools.UpdateCurriculumAccessMode;

public sealed class UpdateCurriculumAccessModeCommandHandler
    : IRequestHandler<UpdateCurriculumAccessModeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _schoolAdminAccessService;

    public UpdateCurriculumAccessModeCommandHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService schoolAdminAccessService)
    {
        _context = context;
        _schoolAdminAccessService = schoolAdminAccessService;
    }

    public async Task Handle(
        UpdateCurriculumAccessModeCommand request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _schoolAdminAccessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        if (request.AccessMode is not (
            CurriculumAccessMode.Progressive or
            CurriculumAccessMode.FullYear))
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.AccessMode),
                "Unsupported curriculum access mode.");
        }

        if (request.CurrentTerm is < 1 or > 4)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.CurrentTerm),
                "The current academic term must be between 1 and 4.");
        }

        var school = await _context.Schools
            .SingleOrDefaultAsync(
                x => x.Id == schoolId,
                cancellationToken);

        if (school is null)
        {
            throw new InvalidOperationException(
                "The authorized school could not be found.");
        }

        school.CurriculumAccessMode = request.AccessMode;
        school.CurrentTerm = request.CurrentTerm;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
