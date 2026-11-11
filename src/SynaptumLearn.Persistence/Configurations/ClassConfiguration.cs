
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Persistence.Configurations;

public sealed class ClassConfiguration
    : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(x => x.Grade)
            .WithMany()
            .HasForeignKey(x => x.GradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.SchoolId,
            x.GradeId,
            x.AcademicYear,
            x.Name
        }).IsUnique();

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Classes_AcademicYear",
                "[AcademicYear] BETWEEN 2000 AND 2100"));
    }
}
