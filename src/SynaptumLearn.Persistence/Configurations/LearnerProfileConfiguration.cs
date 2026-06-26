
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Persistence.Configurations;

public class LearnerProfileConfiguration : IEntityTypeConfiguration<LearnerProfile>
{
    public void Configure(EntityTypeBuilder<LearnerProfile> builder)
    {
        builder.ToTable("LearnerProfiles");

        builder.Property(x => x.LearnerNumber)
            .HasMaxLength(50);
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.LearnerProfiles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}