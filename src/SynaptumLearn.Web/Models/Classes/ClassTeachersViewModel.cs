
using SynaptumLearn.Application.Classes.ListClassTeachers;

namespace SynaptumLearn.Web.Models.Classes;

public sealed class ClassTeachersViewModel
{
    public Guid ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string GradeName { get; set; } = string.Empty;

    public int AcademicYear { get; set; }

    public IReadOnlyList<ClassTeacherDto> Teachers { get; set; }
        = Array.Empty<ClassTeacherDto>();
}
