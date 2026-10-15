using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Subjects.UpdateSubject;

public sealed class UpdateSubjectCommandHandler
    : IRequestHandler<UpdateSubjectCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateSubjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        UpdateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subject = await _context.Subjects
            .SingleOrDefaultAsync(
                x => x.Id == request.SubjectId,
                cancellationToken);

        if (subject is null)
            return false;

        subject.Name = request.Name.Trim();
        subject.Code = request.Code.Trim().ToUpperInvariant();

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}