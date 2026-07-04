using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Sequences;

namespace SynaptumLearn.Persistence.Configurations;

public class EntitySequenceConfiguration
    : IEntityTypeConfiguration<EntitySequence>
{
    public void Configure(EntityTypeBuilder<EntitySequence> builder)
    {
        builder.ToTable("EntitySequences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Prefix)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(x => x.EntityName)
            .IsUnique();
        
        builder.HasData(
            new EntitySequence
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EntityName = SequenceNames.School,
                Prefix = SequencePrefixes.School,
                NextValue = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new EntitySequence
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                EntityName = SequenceNames.Teacher,
                Prefix = SequencePrefixes.Teacher,
                NextValue = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new EntitySequence
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                EntityName = SequenceNames.Learner,
                Prefix = SequencePrefixes.Learner,
                NextValue = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new EntitySequence
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                EntityName = SequenceNames.Lesson,
                Prefix = SequencePrefixes.Lesson,
                NextValue = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new EntitySequence
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                EntityName = SequenceNames.Assessment,
                Prefix = SequencePrefixes.Assessment,
                NextValue = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new EntitySequence
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                EntityName = SequenceNames.Topic,
                Prefix = SequencePrefixes.Topic,
                NextValue = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }
}