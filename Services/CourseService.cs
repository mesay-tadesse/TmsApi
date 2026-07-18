using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CourseService(
    TmsDbContext context, 
    ILogger<CourseService> logger) : ICourseService
{
    // public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)
    // { 
    //     return await context.Courses
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(c => c.Id == id, ct);  

    // }
    public Task<CourseResponseDto?> GetByIdAsync(
        int id, CancellationToken ct) =>
        context.Courses
           .AsNoTracking()
           .Where(c => c.Id == id)
           .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
           .FirstOrDefaultAsync(ct);

    // public async Task<Course> CreateAsync(Course course, CancellationToken ct)
    // {

    //     context.Courses.Add(course);
    //     await context.SaveChangesAsync(ct);
    //     logger.LogInformation("Course created {CourseId}", course.Id);

    //     return course;

    // }
    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        }; 
        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);
        return (await GetByIdAsync(course.Id, ct))!;
    }

}