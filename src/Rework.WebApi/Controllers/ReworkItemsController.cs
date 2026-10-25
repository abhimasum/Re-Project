using Microsoft.AspNetCore.Mvc;
using Rework.Application;
using Rework.Domain.Entities;

namespace Rework.WebApi.Controllers;

public record SubmitReworkRequest(string DefectDescription, string StationId);
public record ActorRequest(string Actor);
public record RejectRequest(string Reason);

public record ReworkItemDto(Guid Id, string DefectDescription, string StationId, ReworkStatus Status,
    string? AssignedTo, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static ReworkItemDto From(ReworkItem i) =>
        new(i.Id, i.DefectDescription, i.StationId, i.Status, i.AssignedTo, i.CreatedAt, i.UpdatedAt);
}

[ApiController]
[Route("api/rework-items")]
public class ReworkItemsController(IReworkService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReworkItemDto>>> List(
        [FromQuery] ReworkStatus? status, [FromQuery] int take = 100, CancellationToken ct = default)
    {
        var items = await service.ListAsync(status, take, ct);
        return Ok(items.Select(ReworkItemDto.From));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReworkItemDto>> Get(Guid id, CancellationToken ct) =>
        Ok(ReworkItemDto.From(await service.GetAsync(id, ct)));

    [HttpPost]
    public async Task<ActionResult<ReworkItemDto>> Submit([FromBody] SubmitReworkRequest req, CancellationToken ct)
    {
        var item = await service.SubmitAsync(req.DefectDescription, req.StationId, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ReworkItemDto.From(item));
    }

    [HttpPost("{id:guid}/inspect")]
    public async Task<ActionResult<ReworkItemDto>> Inspect(Guid id, [FromBody] ActorRequest req, CancellationToken ct) =>
        Ok(ReworkItemDto.From(await service.InspectAsync(id, req.Actor, ct)));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ReworkItemDto>> Approve(Guid id, [FromBody] ActorRequest req, CancellationToken ct) =>
        Ok(ReworkItemDto.From(await service.ApproveAsync(id, req.Actor, ct)));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ReworkItemDto>> Reject(Guid id, [FromBody] RejectRequest req, CancellationToken ct) =>
        Ok(ReworkItemDto.From(await service.RejectAsync(id, req.Reason, ct)));

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ReworkItemDto>> Complete(Guid id, CancellationToken ct) =>
        Ok(ReworkItemDto.From(await service.CompleteAsync(id, ct)));
}
