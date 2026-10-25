using Rework.Domain.Entities;

namespace Rework.Application;

public interface IReworkService
{
    Task<ReworkItem> SubmitAsync(string defectDescription, string stationId, CancellationToken ct);
    Task<ReworkItem> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ReworkItem>> ListAsync(ReworkStatus? status, int take, CancellationToken ct);
    Task<ReworkItem> InspectAsync(Guid id, string inspector, CancellationToken ct);
    Task<ReworkItem> ApproveAsync(Guid id, string approver, CancellationToken ct);
    Task<ReworkItem> RejectAsync(Guid id, string reason, CancellationToken ct);
    Task<ReworkItem> CompleteAsync(Guid id, CancellationToken ct);
}

public class ReworkService(IReworkItemRepository items, IUnitOfWork uow) : IReworkService
{
    public async Task<ReworkItem> SubmitAsync(string defectDescription, string stationId, CancellationToken ct)
    {
        var item = ReworkItem.Submit(defectDescription, stationId);
        items.Add(item);
        await uow.SaveChangesAsync(ct);
        return item;
    }

    public async Task<ReworkItem> GetAsync(Guid id, CancellationToken ct) =>
        await items.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Rework item {id} was not found.");

    public Task<IReadOnlyList<ReworkItem>> ListAsync(ReworkStatus? status, int take, CancellationToken ct) =>
        items.ListAsync(status, Math.Clamp(take, 1, 500), ct);

    public Task<ReworkItem> InspectAsync(Guid id, string inspector, CancellationToken ct) =>
        Mutate(id, i => i.Inspect(inspector), ct);

    public Task<ReworkItem> ApproveAsync(Guid id, string approver, CancellationToken ct) =>
        Mutate(id, i => i.Approve(approver), ct);

    public Task<ReworkItem> RejectAsync(Guid id, string reason, CancellationToken ct) =>
        Mutate(id, i => i.Reject(reason), ct);

    public Task<ReworkItem> CompleteAsync(Guid id, CancellationToken ct) =>
        Mutate(id, i => i.Complete(), ct);

    private async Task<ReworkItem> Mutate(Guid id, Action<ReworkItem> change, CancellationToken ct)
    {
        var item = await GetAsync(id, ct);
        change(item);                    // domain rule runs here
        await uow.SaveChangesAsync(ct);  // RowVersion conflicts surface as DbUpdateConcurrencyException
        return item;
    }
}
