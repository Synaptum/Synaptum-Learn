using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Persistence.Identity;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Users;
using SynaptumLearn.Domain.Assessments;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Domain.Analytics;
using SynaptumLearn.Domain.Curriculum;
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Sequences;

namespace SynaptumLearn.Persistence.Contexts;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, TimeProvider timeProvider, ICurrentUserService currentUserService) : base(options)
    {
        _timeProvider = timeProvider;
        _currentUserService = currentUserService;
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
    public DbSet<EntitySequence> EntitySequences => Set<EntitySequence>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.LastModifiedAt = utcNow;
                entry.Entity.CreatedByUserId = _currentUserService.UserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedAt = utcNow;
                entry.Entity.CreatedByUserId = _currentUserService.UserId;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
       
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}