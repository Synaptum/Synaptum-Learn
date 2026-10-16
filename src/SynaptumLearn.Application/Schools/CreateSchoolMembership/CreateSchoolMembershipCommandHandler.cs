using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Application.Schools.CreateSchoolMembership;

public sealed class CreateSchoolMembershipCommandHandler
    : IRequestHandler<CreateSchoolMembershipCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateSchoolMembershipCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateSchoolMembershipCommand request,
        CancellationToken cancellationToken)
    {
        var schoolExists = await _context.Schools
            .AnyAsync(
                s => s.Id == request.SchoolId,
                cancellationToken);

        if (!schoolExists)
        {
            throw new InvalidOperationException(
                "The specified school does not exist.");
        }

        var userExists = await _context.BusinessUsers
            .AnyAsync(
                u => u.Id == request.UserId,
                cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException(
                "The specified user does not exist.");
        }

        var membershipExists = await _context.SchoolMemberships
            .AnyAsync(
                m => m.SchoolId == request.SchoolId
                     && m.UserId == request.UserId,
                cancellationToken);

        if (membershipExists)
        {
            throw new InvalidOperationException(
                "The user already belongs to this school.");
        }

        var membership = new SchoolMembership
        {
            SchoolId = request.SchoolId,
            UserId = request.UserId,
            Role = request.Role,
            IsActive = true
        };

        _context.SchoolMemberships.Add(membership);

        await _context.SaveChangesAsync(cancellationToken);

        return membership.Id;
    }
}