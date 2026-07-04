using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Sequences;
using SynaptumLearn.Persistence.Contexts;

namespace SynaptumLearn.Persistence.Common;

public class SequenceGenerator : ISequenceGenerator
{
    private readonly ApplicationDbContext _context;

    public SequenceGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(string entityName, string prefix, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var sequence = await _context.EntitySequences
            .SingleAsync(
                x => x.EntityName == entityName,
                 cancellationToken
            );
            
            

        if (sequence is null)
        {
            sequence = new EntitySequence
            {
                EntityName = entityName,
                Prefix = prefix,
                NextValue = 1
            };
            _context.EntitySequences.Add(sequence);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var value = sequence.NextValue;
        sequence.NextValue++;

        await transaction.CommitAsync(cancellationToken);

        return $"{sequence.Prefix}{value:D6}"; // Format the value with leading zeros (e.g., 000001)
    }
}