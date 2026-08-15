using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Schools.ChangeSchoolStatus;

public sealed class ChangeSchoolStatusCommandHandler
    : IRequestHandler<ChangeSchoolStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ChangeSchoolStatusCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        ChangeSchoolStatusCommand request,
        CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .SingleOrDefaultAsync(
                x => x.Id == request.SchoolId,
                cancellationToken);

        if (school is null)
        {
            return false;
        }

        var changed = school.ChangeStatus(request.NewStatus);

        if (!changed)
        {
            return false;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}