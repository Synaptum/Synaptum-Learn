using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Domain.Sequences;

public class EntitySequence : BaseAuditableEntity
{
    public string EntityName { get; set; } = null!;

    public string Prefix { get; set; } = null!;

    public long NextValue { get; set; }
}