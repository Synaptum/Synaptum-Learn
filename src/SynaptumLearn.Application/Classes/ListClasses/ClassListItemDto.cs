
namespace SynaptumLearn.Application.Classes.ListClasses;

public sealed record ClassListItemDto(
    Guid Id,
    string Name,
    Guid GradeId,
    string GradeName,
    int AcademicYear,
    int LearnerCount
);
