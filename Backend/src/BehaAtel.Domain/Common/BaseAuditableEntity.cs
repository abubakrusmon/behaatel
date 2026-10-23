namespace BehaAtel.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    protected BaseAuditableEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public void SetCreatedBy(Guid? userId) => CreatedBy = userId;

    public void SetUpdated(Guid? userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }
}
