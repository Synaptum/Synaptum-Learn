
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Persistence.Configurations;

public class TeacherProfileConfiguration : IEntityTypeConfiguration<TeacherProfile>
{
    public void Configure(EntityTypeBuilder<TeacherProfile> builder)
    {
        builder.ToTable("TeacherProfiles");

        builder.Property(x => x.EmployeeNumber)
            .HasMaxLength(50);

        builder.HasMany(x => x.TeacherSubjects)
    .WithOne(x => x.TeacherProfile)
    .HasForeignKey(x => x.TeacherProfileId)
    .OnDelete(DeleteBehavior.Restrict);
    }
}