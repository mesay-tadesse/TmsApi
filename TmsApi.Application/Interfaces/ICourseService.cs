using TmsApi.Domain.Entities;
using TmsApi.Application.DTOs;
using TmsApi.Application.Enrollments.Commands;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);

    Task<Course?> GetByCodeAsync(string code, CancellationToken ct);
    // Task UpdateAsync(Course course);
    Task UpdateAsync(UpdateCourseCommand command, CancellationToken ct);
    Task<List<Course>> GetAllAsync(CancellationToken ct);
}