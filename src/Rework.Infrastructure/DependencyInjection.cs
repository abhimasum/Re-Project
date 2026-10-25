using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rework.Application;

namespace Rework.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReworkInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ReworkDbContext>(o => o.UseSqlServer(connectionString, sql =>
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReworkItemRepository, ReworkItemRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IReworkService, ReworkService>();
        return services;
    }
}
