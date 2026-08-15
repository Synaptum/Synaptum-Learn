namespace SynaptumLearn.Web.Models.Grades;

public sealed class CreateGradeViewModel
{
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }
}