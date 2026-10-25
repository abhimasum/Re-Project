using System.Net.Http.Json;

namespace Rework.Desktop.Services;

public record ReworkItemDto(Guid Id, string DefectDescription, string StationId, string Status,
    string? AssignedTo, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

/// <summary>Typed client for the shared rework API. Used by the Razor components, which call it from C#.</summary>
public class ReworkApiClient(HttpClient http)
{
    public async Task<List<ReworkItemDto>> ListAsync(string? status = null, int take = 100, CancellationToken ct = default)
    {
        var url = $"api/rework-items?take={take}" + (status is null ? "" : $"&status={status}");
        return await http.GetFromJsonAsync<List<ReworkItemDto>>(url, ct) ?? [];
    }

    public Task<ReworkItemDto?> SubmitAsync(string defectDescription, string stationId, CancellationToken ct = default) =>
        PostAsync("api/rework-items", new { defectDescription, stationId }, ct);

    public Task<ReworkItemDto?> InspectAsync(Guid id, string actor, CancellationToken ct = default) =>
        PostAsync($"api/rework-items/{id}/inspect", new { actor }, ct);

    public Task<ReworkItemDto?> ApproveAsync(Guid id, string actor, CancellationToken ct = default) =>
        PostAsync($"api/rework-items/{id}/approve", new { actor }, ct);

    public Task<ReworkItemDto?> RejectAsync(Guid id, string reason, CancellationToken ct = default) =>
        PostAsync($"api/rework-items/{id}/reject", new { reason }, ct);

    public Task<ReworkItemDto?> CompleteAsync(Guid id, CancellationToken ct = default) =>
        PostAsync($"api/rework-items/{id}/complete", new { }, ct);

    private async Task<ReworkItemDto?> PostAsync(string url, object body, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync(url, body, ct);
        if (!response.IsSuccessStatusCode)
        {
            // ProblemDetails carries the business reason in "detail"; surface it to the UI.
            var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
            throw new InvalidOperationException(problem?.GetValueOrDefault("detail") ?? "The request was rejected.");
        }
        return await response.Content.ReadFromJsonAsync<ReworkItemDto>(cancellationToken: ct);
    }
}
