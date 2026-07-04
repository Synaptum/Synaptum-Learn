namespace SynaptumLearn.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public Guid? LastModifiedByUserId { get; set; }
}