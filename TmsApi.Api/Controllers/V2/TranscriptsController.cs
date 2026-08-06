using System.Threading.Channels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using tmsapi.application.transcripts;
using TmsApi.Infrastructure.Transcripts;
// using MediatR;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiversion}/transcripts")]
[ApiVersion("2.0")]

public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore) :ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscriptAsync(
        TranscriptRequest request,
        [FromHeader(Name = "Idempotency-key")] string? idempotencyKey, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await statusStore.GetReportIdForIdempotencyKeyAsync(idempotencyKey, ct);
            if (existing is not null)
            {
                var existingStatus = await statusStore.GetAsync(existing, ct);
                return Accepted(
                    Url.Action(nameof(GetStatus), new { id = existing }),
                    existingStatus
                );
            }
        }

        var reportId = Guid.NewGuid().ToString("N")[..12];
        var status = await statusStore.CreateAsync(reportId, request.StudentId, ct);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await statusStore.LinkIdempotencyKeyAsync(idempotencyKey, reportId, ct);
        }

        await channel.Writer.WriteAsync(request.WithReportId(reportId), ct);

        Response.Headers.RetryAfter = "5";
        return Accepted(
            Url.Action(nameof(GetStatus), new { id = reportId }),
            status
        );

    }
    
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(string id, CancellationToken ct)
    {
        var status = await statusStore.GetAsync(id, ct);
        return status is null 
            ? NotFound(new ProblemDetails
            {
                Title = "Transcript not found",
                Detail = $"No transcript request with id '{id}'.",
                Status = StatusCodes.Status404NotFound 
            })
            : Ok(status);
    }

	[HttpGet("search")]
	[EnableRateLimiting("search")]
	public async Task<IActionResult> SearchCourses(
		[FromQuery] string? term, CancellationToken ct)
	{
		var results = await mediator.Send(new SearchCoursesQuery(term), ct);
		return Ok(results);
	}
}

