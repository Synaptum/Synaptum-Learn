
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Persistence.Configurations;

public sealed class TeacherClassSubjectConfiguration
    : IEntityTypeConfiguration<TeacherClassSubject>
{
    public void Configure(
        EntityTypeBuilder<TeacherClassSubject> builder)
    {
        builder.ToTable("TeacherClassSubjects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SchoolId)
            .IsRequired();

        builder.Property(x => x.TeacherSubjectId)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.HasOne(x => x.TeacherSubject)
            .WithMany(x => x.TeacherClassSubjects)
            .HasForeignKey(x => x.TeacherSubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Class)
            .WithMany(x => x.TeacherClassSubjects)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.SchoolId,
            x.TeacherSubjectId,
            x.ClassId
        }).IsUnique();

        builder.HasIndex(x => new
        {
            x.SchoolId,
            x.ClassId
        });
    }
}
