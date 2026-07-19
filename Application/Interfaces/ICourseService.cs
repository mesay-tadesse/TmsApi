using TmsApi.Entities;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<Course?> GetByCodeAsync(string code, CancellationToken ct);
}
