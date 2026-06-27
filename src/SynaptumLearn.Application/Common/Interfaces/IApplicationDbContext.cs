using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Domain.Analytics;
using SynaptumLearn.Domain.Assessments;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Domain.Curriculum;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Users;

namespace SynaptumLearn.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> BusinessUsers { get; }

    DbSet<School> Schools { get; }
    DbSet<SchoolMembership> SchoolMemberships { get; }
    DbSet<TeacherProfile> TeacherProfiles { get; }
    DbSet<LearnerProfile> LearnerProfiles { get; }
    DbSet<TeacherSubject> TeacherSubjects { get; }

    DbSet<Grade> Grades { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<GradeSubject> GradeSubjects { get; }

    DbSet<Topic> Topics { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<LessonContent> LessonContents { get; }
    DbSet<LessonApproval> LessonApprovals { get; }

    DbSet<Assessment> Assessments { get; }
    DbSet<Question> Questions { get; }
    DbSet<AnswerOption> AnswerOptions { get; }
    DbSet<AssessmentAttempt> AssessmentAttempts { get; }

    DbSet<LessonProgress> LessonProgresses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}