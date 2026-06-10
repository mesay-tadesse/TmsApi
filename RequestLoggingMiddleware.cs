using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate a short correlation id
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        // Set X-Correlation-Id header before calling next
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // Use Stopwatch to measure elapsed time
        var stopwatch = Stopwatch.StartNew();

        // Log request entry
        _logger.LogInformation(
            "Request Started | {Method} {Path} | CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        // Pass control to the next middleware
        await _next(context);

        stopwatch.Stop();

        // Log request exit
        _logger.LogInformation(
            "Request Finished | StatusCode: {StatusCode} | Elapsed: {ElapsedMs}ms | CorrelationId: {CorrelationId}",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}

/*

info: RequestLoggingMiddleware[0]
      Request Started | GET /api/assessments/results/ | CorrelationId: bc7269bd
info: RequestLoggingMiddleware[0]
      Request Finished | StatusCode: 200 | Elapsed: 278ms | CorrelationId: bc7269bd

*/