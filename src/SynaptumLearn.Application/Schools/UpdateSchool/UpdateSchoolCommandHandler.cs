using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Schools.UpdateSchool;

public sealed class UpdateSchoolCommandHandler
    : IRequestHandler<UpdateSchoolCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugGenerator _slugGenerator;

    public UpdateSchoolCommandHandler(
        IApplicationDbContext context,
        ISlugGenerator slugGenerator)
    {
        _context = context;
        _slugGenerator = slugGenerator;
    }

    public async Task<bool> Handle(
        UpdateSchoolCommand request,
        CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .SingleOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (school is null)
        {
            return false;
        }

        var normalizedName = request.Name.Trim();

        var nameChanged =
            !string.Equals(
                school.Name,
                normalizedName,
                StringComparison.Ordinal);

        school.Name = normalizedName;
        school.EMISNumber = request.EMISNumber.Trim();
        school.Email = request.Email.Trim();
        school.Phone = request.Phone.Trim();
        school.Province = request.Province;

        if (nameChanged)
        {
            school.Slug =
                await _slugGenerator.GenerateSchoolSlugAsync(
                    normalizedName,
                    cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}