using Microsoft.AspNetCore.Mvc;
using TmsApi.Entities;
using TmsApi.Services;
using TmsApi.Dtos;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(
    ICourseService courseService,
    LinkGenerator linkGenerator) : ControllerBase
{
    
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null) return NotFound();

        var selfHref = linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });
    
        var enrollmentsHref = linkGenerator.GetPathByAction(
            HttpContext,
            action: "ListCourseEnrollments",
            controller: "Enrollments",
            values: new { courseId = id }
            );

        var links = new List<LinkDto>
        {
            new LinkDto(selfHref!, "self", "GET"),
            new LinkDto(selfHref!, "update", "PUT"),
            new LinkDto(selfHref!, "delete", "DELETE"),
            new LinkDto(enrollmentsHref!, "enrollments", "GET")
        };

        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new LinkDto(enrollmentsHref!, "enroll", "POST"));
        }
    
        var detailDto = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(detailDto);

    }

  
    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        // var result = await courseService.CreateAsync(request, ct);
        // return CreatedAtAction(
        //     nameof(GetCourseById),
        //     new { id = result.Id },
        //     result);
  
        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }
        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(
            nameof(GetCourseById), 
            new { id = result.Id}, 
            result);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }
}