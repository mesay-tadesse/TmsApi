using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
	private static bool IsHonorRoll(decimal gpa)
	{
		return gpa >= 3.5m;
	}

	[HttpGet("translation-fail")]
	public IActionResult TestTranslationFail()
	{
		Console.WriteLine("\n>>> STEP 1: running non-translatable query ...");
		try
		{
			// var students = context.Students
			// .Where(s => IsHonorRoll(s.GPA))
			// .ToList();

			//Server-Side Evaluation (Preferred):
			var students = context.Students
			    .Where(s => s.GPA >=3.5m)
				.ToList();
				
			// Client-Side Evaluation
			// var students = context.Students
			// 	.AsEnumerable()
			// 	.Where(s => IsHonorRoll(s.GPA))
			// 	.ToList();


			return Ok(students);
		}
		catch(Exception ex)
		{
			Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
			return BadRequest(new { Message = ex.Message});
		}	
	}

	[HttpGet("deferred")]
	public IActionResult TestDeferred()
	{
		Console.WriteLine("\n>>> STEP 1: building the query object (nodatabase contact)...");

		var query = context.Students.Where(s => s.GPA >= 3.0m);

		Console.WriteLine(">>> STEP 2: appending a sorting clause...");

		var orderedQuery = query.OrderBy(s => s.Name);

		Console.WriteLine(">>> STEP 3: Materializing query into a C#List...");

		var results = orderedQuery.ToList();

		Console.WriteLine(">>> STEP 4: Materialization finished. Listpopulated.\n");

		return Ok(results);
	}

	[HttpGet("getcount")]
	public async Task<IActionResult> GetCount()
	{
		var count = await context.Students
		.Where(s => s.IsActive && s.GPA >= 3.0m)
		.CountAsync();
		return Ok(count);

	}

	[HttpGet("courselist")]
	public async Task<IActionResult> GetCourseList()
	{
		var list = await context.Courses
		.Select(c => new
		{
			c.Title,
			EnromentCount = c.Enrollments.Count
		})
		.OrderByDescending(x => x.EnromentCount)
		.ToListAsync();

		return Ok(list);
	}

	[HttpGet("averagegpa")]
	public async Task<IActionResult> GetAverageGPA()
	{
		var list = await context.Enrollments
		.GroupBy(e => e.Course.Title)
		.Select(g => new
		{
			Course  = g.Key,
			AverageGPA = g.Average(e => e.Student.GPA)
		})
		.ToListAsync();

		return Ok(list);
	}


	[HttpGet("zeroenroll")]
	public async Task<IActionResult> GetZeroEnrollments()
	{ 
		//[A] Using Subquery
		// var list = await context.Students
		// .Where(s => !s.Enrollments.Any())
		// .Select(s => s.Name)
		// .ToListAsync();

		//[B] Using EF Core 10 LeftJoin
		var list = await context.Students
		.LeftJoin(context.Enrollments,
				s => s.Id,
				e => e.StudentId,
				(s, e) => new { s, e })
		.Where(x => x.e == null)
		.Select(x => x.s.Name)
		.ToListAsync();
		
		return Ok(list);
	}

    //student pagination
    [HttpGet("students-page")]
	public async Task<IActionResult> GetStudentsPage(
		int page, int pageSize,
		CancellationToken cancellationToken)
	{
		pageSize = 20;

		var students = await context.Students
			.OrderBy(student => student.Name)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return Ok(students);
	}

	//top5 enrolled courses
	[HttpGet("top-courses")]
	public async Task<IActionResult> GetTopCourses(
		CancellationToken cancellationToken)
	{
		var courses = await context.Enrollments
			.GroupBy(e => e.Course.Title)
			.Select(g => new
			{
				CourseTitle = g.Key,
				EnrollmentCount = g.Count()
			})
			.OrderByDescending(c => c.EnrollmentCount)
			.Take(5)
			.ToListAsync(cancellationToken);

		return Ok(courses);
	}
}