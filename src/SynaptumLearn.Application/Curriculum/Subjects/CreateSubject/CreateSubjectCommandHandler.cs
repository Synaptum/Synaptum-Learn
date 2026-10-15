using MediatR;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Application.Curriculum.Subjects.CreateSubject;

public sealed class CreateSubjectCommandHandler
    : IRequestHandler<CreateSubjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateSubjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subject = new Subject
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpperInvariant()
        };

        _context.Subjects.Add(subject);

        await _context.SaveChangesAsync(cancellationToken);

        return subject.Id;
    }
}