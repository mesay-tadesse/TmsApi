// using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace TmsApi.Infrastructure.Services;

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
    
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
          => context.Courses
               .AsNoTracking()
               .AnyAsync(c => c.Code == code, ct);
    
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        IQueryable<Course> query = context.Courses.AsNoTracking();

         if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
                EF.Functions.ILike(c.Code, $"%{request.Search}%"));
        }

        var totalCount = await query.CountAsync(ct);
     
        IOrderedQueryable<Course> sortedQuery = request.OrderBy switch
        {
            "Code" => request.Descending
                ? query.OrderByDescending(c => c.Code)
                : query.OrderBy(c => c.Code),

            "MaxCapacity" => request.Descending
                ? query.OrderByDescending(c => c.MaxCapacity)
                : query.OrderBy(c => c.MaxCapacity),

            _ => request.Descending
                ? query.OrderByDescending(c => c.Title)
                : query.OrderBy(c => c.Title)
        };

      
        var items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .ToListAsync(ct);

        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
 
    public async Task<Course?> GetByCodeAsync(
        string code,
        CancellationToken ct)
    {
        return await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task UpdateAsync(
        UpdateCourseCommand command,
        CancellationToken ct)
    {
        var course = await context.Courses
            .FirstOrDefaultAsync(c => c.Code == command.Code, ct);

        if (course is null)
        {
            throw new KeyNotFoundException($"Course '{command.Code}' was not found.");
        }

        course.Title = command.Title;

        await context.SaveChangesAsync(ct);
    }
    
    public async Task<List<Course>> GetAllAsync(
        CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .ToListAsync(ct);
    }
}