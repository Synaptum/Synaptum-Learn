using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Users;

namespace SynaptumLearn.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.IdentityUserId)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.IdentityUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(u => u.LastName)
        .HasMaxLength(100)
        .IsRequired();
    }
}