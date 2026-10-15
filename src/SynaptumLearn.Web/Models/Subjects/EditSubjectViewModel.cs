namespace SynaptumLearn.Web.Models.Subjects;

public sealed class EditSubjectViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}