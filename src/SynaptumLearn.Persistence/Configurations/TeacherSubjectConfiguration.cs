using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Persistence.Configurations;

public class TeacherSubjectConfiguration : IEntityTypeConfiguration<TeacherSubject>
{
    public void Configure(EntityTypeBuilder<TeacherSubject> builder)
    {
        builder.ToTable("TeacherSubjects");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.TeacherProfile)
            .WithMany()
            .HasForeignKey(x => x.TeacherProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GradeSubject)
            .WithMany()
            .HasForeignKey(x => x.GradeSubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.TeacherProfileId,
            x.GradeSubjectId
        }).IsUnique();
    }
}