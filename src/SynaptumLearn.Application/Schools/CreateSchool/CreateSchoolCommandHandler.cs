using MediatR;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Schools.Events;
using SynaptumLearn.Domain.Sequences;

namespace SynaptumLearn.Application.Schools.CreateSchool;

public sealed class CreateSchoolCommandHandler : IRequestHandler<CreateSchoolCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ISequenceGenerator _sequenceGenerator;

    public CreateSchoolCommandHandler(IApplicationDbContext context, ISlugGenerator slugGenerator, ISequenceGenerator sequenceGenerator)
    {
        _context = context;
        _slugGenerator = slugGenerator;
        _sequenceGenerator = sequenceGenerator;
    }

    public async Task<Guid> Handle(CreateSchoolCommand request,CancellationToken cancellationToken)
    {
        var school = new School
        {
            Name = request.Name,
            EMISNumber = request.EMISNumber,
            Email = request.Email,
            Phone = request.Phone,
            Province = request.Province,

            Slug = await _slugGenerator.GenerateSchoolSlugAsync(request.Name, cancellationToken),
            Code = await _sequenceGenerator.GenerateAsync(SequenceNames.School, SequencePrefixes.School, cancellationToken),
            Status = SchoolStatus.Pending
        };

        school.AddDomainEvent(new SchoolCreatedEvent(school.Id));

        _context.Schools.Add(school);

        await _context.SaveChangesAsync(cancellationToken);

        return school.Id;
    }
}