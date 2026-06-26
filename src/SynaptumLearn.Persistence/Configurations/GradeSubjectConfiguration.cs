
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Persistence.Configurations;

public class GradeSubjectConfiguration : IEntityTypeConfiguration<GradeSubject>
{
    public void Configure(EntityTypeBuilder<GradeSubject> builder)
    {
        builder.ToTable("GradeSubjects");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Grade)
            .WithMany()
            .HasForeignKey(x => x.GradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.GradeId, x.SubjectId })
            .IsUnique();
    }
}