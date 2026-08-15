namespace SynaptumLearn.Web.Models.Grades;

public sealed class EditGradeViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }
}