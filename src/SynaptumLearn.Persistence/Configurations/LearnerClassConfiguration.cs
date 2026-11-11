
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Persistence.Configurations;

public sealed class LearnerClassConfiguration
    : IEntityTypeConfiguration<LearnerClass>
{
    public void Configure(EntityTypeBuilder<LearnerClass> builder)
    {
        builder.ToTable("LearnerClasses");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Learner)
            .WithMany()
            .HasForeignKey(x => x.LearnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Class)
            .WithMany(x => x.LearnerClasses)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.EnrolledAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.SchoolId,
            x.LearnerId,
            x.AcademicYear
        })
        .IsUnique()
        .HasFilter("[LeftAt] IS NULL");

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_LearnerClasses_AcademicYear",
                "[AcademicYear] BETWEEN 2000 AND 2100"));

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_LearnerClasses_Dates",
                "[LeftAt] IS NULL OR [LeftAt] >= [EnrolledAt]"));
    }
}
