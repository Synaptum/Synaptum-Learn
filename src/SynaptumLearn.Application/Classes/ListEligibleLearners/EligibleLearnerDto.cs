
namespace SynaptumLearn.Application.Classes.ListEligibleLearners;

public sealed record EligibleLearnerDto(
    Guid LearnerProfileId,
    string FullName,
    string LearnerNumber
);
