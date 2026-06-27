using MediatR;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Application.Schools.CreateSchool;

public sealed class CreateSchoolCommandHandler : IRequestHandler<CreateSchoolCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ISchoolCodeGenerator _schoolCodeGenerator;

    public CreateSchoolCommandHandler(IApplicationDbContext context, ISlugGenerator slugGenerator, ISchoolCodeGenerator schoolCodeGenerator)
    {
        _context = context;
        _slugGenerator = slugGenerator;
        _schoolCodeGenerator = schoolCodeGenerator;
    }

    public async Task<Guid> Handle(CreateSchoolCommand request,CancellationToken cancellationToken)
    {
        var school = new School
        {
            Name = request.Name.Trim(),
            EMISNumber = request.EMISNumber.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            //Province = request.Province.Trim(),

            Slug = await _slugGenerator.GenerateSchoolSlugAsync(request.Name),
            Code = await _schoolCodeGenerator.GenerateAsync(),
            Status = SchoolStatus.Pending
        };

        _context.Schools.Add(school);

        await _context.SaveChangesAsync(cancellationToken);

        return school.Id;
    }
}