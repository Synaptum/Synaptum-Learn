using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynaptumLearn.Domain.Assessments;

namespace SynaptumLearn.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Assessment)
            .WithMany()
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}