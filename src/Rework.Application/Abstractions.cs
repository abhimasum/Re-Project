using Rework.Domain.Entities;

namespace Rework.Application;

public class NotFoundException(string message) : Exception(message) { }

public interface IReworkItemRepository
{
    Task<ReworkItem?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ReworkItem>> ListAsync(ReworkStatus? status, int take, CancellationToken ct);
    void Add(ReworkItem item);
}

public interface ISettingsRepository
{
    Task<IReadOnlyList<AppSetting>> ListAsync(string? category, CancellationToken ct);
    Task<AppSetting?> GetAsync(string key, CancellationToken ct);
    void Add(AppSetting setting);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
