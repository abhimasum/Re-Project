using Microsoft.EntityFrameworkCore;
using Rework.Application;
using Rework.Domain.Entities;

namespace Rework.Infrastructure;

public class ReworkItemRepository(ReworkDbContext db) : IReworkItemRepository
{
    public Task<ReworkItem?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.ReworkItems.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<ReworkItem>> ListAsync(ReworkStatus? status, int take, CancellationToken ct)
    {
        var q = db.ReworkItems.AsNoTracking().AsQueryable();
        if (status is not null) q = q.Where(x => x.Status == status);
        return await q.OrderByDescending(x => x.CreatedAt).Take(take).ToListAsync(ct);
    }

    public void Add(ReworkItem item) => db.ReworkItems.Add(item);
}

public class SettingsRepository(ReworkDbContext db) : ISettingsRepository
{
    public async Task<IReadOnlyList<AppSetting>> ListAsync(string? category, CancellationToken ct)
    {
        var q = db.AppSettings.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(x => x.Category == category);
        return await q.OrderBy(x => x.Category).ThenBy(x => x.Key).ToListAsync(ct);
    }

    public Task<AppSetting?> GetAsync(string key, CancellationToken ct) =>
        db.AppSettings.FirstOrDefaultAsync(x => x.Key == key, ct);

    public void Add(AppSetting setting) => db.AppSettings.Add(setting);
}

public class UnitOfWork(ReworkDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
