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
    
    [HttpGet("include")]
    public async Task<IActionResult> Include(CancellationToken cancellationToken)
    {
        var students = await db.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
            .ToListAsync(cancellationToken);

        var result = students.Select(s => new
        {
            s.Name,
            EnrollmentCount = s.Enrollments.Count
        });

        return Ok(result);
    }

    [HttpPost("softdelete")]
    public async Task<IActionResult> SoftDelete(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow;
        await db.Students 
            .Where(e => e.IsActive == true)
            .ExecuteUpdateAsync(s => s.SetProperty( e => e.IsDeleted, true), cancellationToken);
        return Ok(); 
    }

    [HttpPost("ISdeleted")]
    public async Task<IActionResult> Isdeleted(CancellationToken cancellationToken)
    {
        var allStudents = await db.Students
             .IgnoreQueryFilters()
             .ToListAsync();

         return Ok(allStudents);
    }     
    
    [HttpPost("archive")]
    public async Task<IActionResult> Archive(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow;
        await db.Enrollments 
            .Where(e => e.EnrolledAt < cutoff)
            .ExecuteUpdateAsync(s => s.SetProperty( e => e.IsArchived, true), cancellationToken);
        return Ok();
    }  
}
