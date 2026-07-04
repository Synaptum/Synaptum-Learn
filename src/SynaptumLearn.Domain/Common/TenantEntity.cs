namespace SynaptumLearn.Domain.Common;

public abstract class TenantEntity : BaseAuditableEntity
{
    public Guid SchoolId { get; set; }
}