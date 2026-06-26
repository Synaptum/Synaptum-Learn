namespace SynaptumLearn.Domain.Common;

public abstract class TenantEntity : BaseEntity
{
    public Guid SchoolId { get; set; }
}