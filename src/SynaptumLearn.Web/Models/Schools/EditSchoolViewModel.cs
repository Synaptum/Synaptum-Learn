using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Web.Models.Schools;

public sealed class EditSchoolViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string EMISNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public Province Province { get; set; }
}