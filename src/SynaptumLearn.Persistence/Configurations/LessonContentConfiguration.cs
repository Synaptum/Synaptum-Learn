using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Persistence.Configurations;

public class LessonContentConfiguration : IEntityTypeConfiguration<LessonContent>
{
    public void Configure(EntityTypeBuilder<LessonContent> builder)
    {
        builder.ToTable("LessonContents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Type)
            .HasConversion<int>();

        builder.HasOne(x => x.Lesson)
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}