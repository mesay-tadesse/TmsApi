using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{


    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request)
    {
        var course = await courseService.CreateAsync(
                request.Code,
                request.Title,
                request.Capacity);
        return Created($"/api/courses/{course.Code}", course);
    }


    [HttpGet]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await courseService.GetAllAsync();
        return Ok(courses);
    }
    
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourse(string id)
    {
        var course = await courseService.GetByIdAsync(id);
        return course is null
            ? NotFound()
            : Ok(course);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(string id)
    {
        var deleted = await courseService.DeleteAsync(id);
        return deleted
            ? NoContent()
            : NotFound();
    }


}


public record CreateCourseRequest(
    string Code,
    string Title,
    int Capacity
);