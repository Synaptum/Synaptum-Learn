
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Tests;

public sealed class TeacherProfileFailureInterceptor
    : SaveChangesInterceptor
{
    public bool Enabled { get; set; }

    public override ValueTask<InterceptionResult<int>>
        SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
    {
        if (Enabled &&
            eventData.Context?.ChangeTracker
                .Entries<TeacherProfile>()
                .Any(e => e.State == EntityState.Added) == true)
        {
            throw new InvalidOperationException(
                "Simulated teacher profile save failure.");
        }

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }
}
