using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Analytics;

namespace SynaptumLearn.Persistence.Configurations;

public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.ToTable("LessonProgress");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProgressPercentage)
            .HasPrecision(5, 2);

        builder.HasOne(x => x.LearnerProfile)
            .WithMany()
            .HasForeignKey(x => x.LearnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Lesson)
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.LearnerProfileId,
            x.LessonId
        }).IsUnique();
    }
}