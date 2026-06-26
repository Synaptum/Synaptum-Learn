
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

        builder.HasMany(x => x.Topics)
            .WithOne(x => x.GradeSubject)
            .HasForeignKey(x => x.GradeSubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.GradeId, x.SubjectId })
            .IsUnique();
    }
}