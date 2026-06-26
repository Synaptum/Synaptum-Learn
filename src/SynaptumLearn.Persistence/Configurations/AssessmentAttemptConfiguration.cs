using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Assessments;

namespace SynaptumLearn.Persistence.Configurations;

public class AssessmentAttemptConfiguration : IEntityTypeConfiguration<AssessmentAttempt>
{
    public void Configure(EntityTypeBuilder<AssessmentAttempt> builder)
    {
        builder.ToTable("AssessmentAttempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score)
            .HasPrecision(5, 2);

        builder.HasOne(x => x.Assessment)
            .WithMany()
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LearnerProfile)
            .WithMany()
            .HasForeignKey(x => x.LearnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}