using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Persistence.Identity;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Users;
using SynaptumLearn.Domain.Assessments;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Domain.Analytics;
using SynaptumLearn.Domain.Curriculum;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Persistence.Contexts;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<AssessmentAttempt> AssessmentAttempts => Set<AssessmentAttempt>();
    public DbSet<LessonApproval> LessonApprovals => Set<LessonApproval>();
    public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<GradeSubject> GradeSubjects => Set<GradeSubject>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonContent> LessonContents => Set<LessonContent>();
    public DbSet<User> BusinessUsers => Set<User>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<SchoolMembership> SchoolMemberships => Set<SchoolMembership>();
    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();
    public DbSet<LearnerProfile> LearnerProfiles => Set<LearnerProfile>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
       
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}