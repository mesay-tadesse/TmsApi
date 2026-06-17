using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreateStudent(CreateStudentRequest request)
    {
        var student = await studentService.CreateAsync(
                request.Name,
                request.Age,
                request.GPA);
        return Created($"/api/students/{student.Id}", student);
    }


    [HttpGet]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await studentService.GetAllAsync();
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudent(string id)
    {
        var student = await studentService.GetByIdAsync(id);
        return student is null
            ? NotFound()
            : Ok(student);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(string id)
    {
        var deleted = await studentService.DeleteAsync(id);

        return deleted
            ? NoContent()
            : NotFound();
    }

}


public record CreateStudentRequest(
    string Name,
    int Age,
    decimal GPA
);