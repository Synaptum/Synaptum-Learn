using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Persistence.Configurations;

public class LessonApprovalConfiguration : IEntityTypeConfiguration<LessonApproval>
{
    public void Configure(EntityTypeBuilder<LessonApproval> builder)
    {
        builder.ToTable("LessonApprovals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Lesson)
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SchoolId, x.LessonId })
            .IsUnique();
    }
}