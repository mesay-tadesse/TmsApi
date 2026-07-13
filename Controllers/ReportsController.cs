using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TmsApi.Data;
using TmsApi.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace TmsApi.Controllers;


[ApiController]
[Route("api/report")]
public class ReportsController : ControllerBase
{
    private readonly TmsDbContext db;
    public ReportsController(TmsDbContext context) => db = context;

    [HttpGet("nplus1")]
    public async Task<IActionResult> NPlus1(CancellationToken ct)
    {
        var students = await db.Students.AsNoTracking().ToListAsync(ct);

        foreach (var s in students)
        {
            var count = await db.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id, ct);

            Console.WriteLine($"{s.Name}: {count} enrollments");
        }

        return Ok();
    }

    [HttpGet("shaped")]
    public async Task<IActionResult> NPlus1Fix(CancellationToken cancellationToken)
    {
        var report = await db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

            foreach (var r in report)
            {
                Console.WriteLine(
                    $"{r.Name}: {r.EnrollmentCount} enrollments");
            }

        return Ok(report);
    } 
}
