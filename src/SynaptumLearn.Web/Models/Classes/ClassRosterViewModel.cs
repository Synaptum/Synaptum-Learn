
using SynaptumLearn.Application.Classes.ListClassLearners;

namespace SynaptumLearn.Web.Models.Classes;

public sealed class ClassRosterViewModel
{
    public Guid ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string GradeName { get; set; } = string.Empty;

    public int AcademicYear { get; set; }

    public IReadOnlyList<ClassLearnerDto> Learners { get; set; }
        = Array.Empty<ClassLearnerDto>();
}
