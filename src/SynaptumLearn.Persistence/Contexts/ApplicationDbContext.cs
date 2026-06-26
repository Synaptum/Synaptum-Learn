using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Persistence.Identity;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Users;
using SynaptumLearn.Domain.Assessments;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Domain.Analytics;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Persistence.Contexts;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Assessment> Assessments { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<AssessmentAttempt> AssessmentAttempts { get; set; }
    public DbSet<LessonApproval> LessonApprovals { get ; set; }
    public DbSet<TeacherSubject> TeacherSubjects { get; set; }
    public DbSet<LessonProgress> LessonProgresses { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<GradeSubject> GradeSubjects { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<LessonContent> LessonContents { get; set; }
    public DbSet<User> BusinessUsers { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<SchoolMembership> SchoolMemberships {get;set;}
    public DbSet<TeacherProfile> TeacherProfiles { get; set; }
    public DbSet<LearnerProfile> LearnerProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}