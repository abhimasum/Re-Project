using Microsoft.AspNetCore.Mvc;
using Rework.Application;
using Rework.Domain.Entities;

namespace Rework.WebApi.Controllers;

public record SettingDto(string Key, string Value, string Category, DateTimeOffset UpdatedAt);
public record CreateSettingRequest(string Key, string Value, string Category);
public record UpdateSettingRequest(string Value);

[ApiController]
[Route("api/settings")]
public class SettingsController(ISettingsRepository settings, IUnitOfWork uow) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SettingDto>>> List([FromQuery] string? category, CancellationToken ct)
    {
        var list = await settings.ListAsync(category, ct);
        return Ok(list.Select(s => new SettingDto(s.Key, s.Value, s.Category, s.UpdatedAt)));
    }

    [HttpPost]
    public async Task<ActionResult<SettingDto>> Create([FromBody] CreateSettingRequest req, CancellationToken ct)
    {
        if (await settings.GetAsync(req.Key, ct) is not null)
            return Conflict(new { detail = $"Setting {req.Key} already exists." });

        var setting = new AppSetting(req.Key, req.Value, req.Category);
        settings.Add(setting);
        await uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(List), new { category = setting.Category },
            new SettingDto(setting.Key, setting.Value, setting.Category, setting.UpdatedAt));
    }

    [HttpPut("{key}")]
    public async Task<ActionResult<SettingDto>> Update(string key, [FromBody] UpdateSettingRequest req, CancellationToken ct)
    {
        var setting = await settings.GetAsync(key, ct);
        if (setting is null) return NotFound(new { detail = $"Setting {key} was not found." });

        setting.Update(req.Value);
        await uow.SaveChangesAsync(ct);
        return Ok(new SettingDto(setting.Key, setting.Value, setting.Category, setting.UpdatedAt));
    }
}
