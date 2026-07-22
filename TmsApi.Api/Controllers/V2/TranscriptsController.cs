[ApiController]
[Route("api/v2/transcripts")]
public class TranscriptsController : ControllerBase
{
	[HttpPost]
	[EnableRateLimiting("transcripts")]
	public IActionResult RequestTranscript([FromBody] object? _)
	{
	   return Ok();
	}
}


[HttpGet("search")]
[EnableRateLimiting("search")]
public async Task<IActionResult> SearchCourses(
	[FromQuery] string? term, CancellationToken ct)
{
	var results = await mediator.Send(new SearchCoursesQuery(term), ct);
	return Ok(results);
}