using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MediatR;
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
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, TimeProvider timeProvider, ICurrentUserService currentUserService, IDomainEventDispatcher domainEventDispatcher) : base(options)
    {
        _timeProvider = timeProvider;
        _currentUserService = currentUserService;
        _domainEventDispatcher = domainEventDispatcher;
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

        //Auditing
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.LastModifiedAt = utcNow;

                entry.Entity.CreatedByUserId = _currentUserService.UserId;
                entry.Entity.LastModifiedByUserId = _currentUserService.UserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedAt = utcNow;

                entry.Entity.LastModifiedByUserId = _currentUserService.UserId;
            }
        }

        //Capture domain events before saving
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        //Save database changes first
        var result = await base.SaveChangesAsync(cancellationToken);

        //Clear before dispatching
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        //Publish after successful database save
        await _domainEventDispatcher.DispatchAsync(
            domainEvents,
            cancellationToken);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}