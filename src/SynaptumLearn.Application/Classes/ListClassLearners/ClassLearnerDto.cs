
namespace SynaptumLearn.Application.Classes.ListClassLearners;

public sealed record ClassLearnerDto(
    Guid EnrollmentId,
    Guid LearnerProfileId,
    string LearnerNumber,
    string FirstName,
    string LastName,
    DateTime EnrolledAt
);
