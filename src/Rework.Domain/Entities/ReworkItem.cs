namespace Rework.Domain.Entities;

public enum ReworkStatus { Submitted, UnderInspection, Approved, Rejected, Completed }

public class ReworkItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string DefectDescription { get; private set; } = string.Empty;
    public string StationId { get; private set; } = string.Empty;
    public ReworkStatus Status { get; private set; } = ReworkStatus.Submitted;
    public string? AssignedTo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Optimistic concurrency token. Two people acting on the same item get a conflict, not a silent overwrite.</summary>
    public byte[] RowVersion { get; private set; } = [];

    private ReworkItem() { } // EF Core

    public static ReworkItem Submit(string defectDescription, string stationId)
    {
        if (string.IsNullOrWhiteSpace(defectDescription))
            throw new DomainRuleException("A defect description is required.");
        if (string.IsNullOrWhiteSpace(stationId))
            throw new DomainRuleException("A station ID is required.");

        var now = DateTimeOffset.UtcNow;
        return new ReworkItem
        {
            DefectDescription = defectDescription.Trim(),
            StationId = stationId.Trim(),
            Status = ReworkStatus.Submitted,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Inspect(string inspector)
    {
        RequireStatus(ReworkStatus.Submitted, ReworkStatus.UnderInspection);
        AssignedTo = inspector;
    }

    public void Approve(string approver) =>
        RequireStatus(ReworkStatus.UnderInspection, ReworkStatus.Approved);

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainRuleException("A rejection reason is required.");
        RequireStatus(ReworkStatus.UnderInspection, ReworkStatus.Rejected);
    }

    public void Complete() =>
        RequireStatus(ReworkStatus.Approved, ReworkStatus.Completed);

    private void RequireStatus(ReworkStatus expected, ReworkStatus next)
    {
        if (Status != expected)
            throw new DomainRuleException($"Cannot move from {Status} to {next}. Expected {expected}.");
        Status = next;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
