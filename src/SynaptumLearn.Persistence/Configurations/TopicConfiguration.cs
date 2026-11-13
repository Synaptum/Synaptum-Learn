using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Persistence.Configurations;

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.HasOne(x => x.GradeSubject)
            .WithMany(x => x.Topics)
            .HasForeignKey(x => x.GradeSubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lessons)
            .WithOne(x => x.Topic)
            .HasForeignKey(x => x.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.GradeSubjectId,
            x.Term,
            x.Order
        }).IsUnique();

        builder.Property(x => x.Term)
            .IsRequired();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Topics_Term",
                "[Term] BETWEEN 1 AND 4");

            t.HasCheckConstraint(
                "CK_Topics_Order",
                "[Order] > 0");
        });
    }
}