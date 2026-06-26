using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Persistence.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("Grades");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.Order)
            .IsRequired();
        builder.HasIndex(x => x.Name)
            .IsUnique();
        builder.HasMany(x => x.GradeSubjects)
            .WithOne(x => x.Grade)
            .HasForeignKey(x => x.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}