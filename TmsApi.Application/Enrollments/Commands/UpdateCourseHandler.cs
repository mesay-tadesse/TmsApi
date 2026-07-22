using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Application.DTOs;
namespace TmsApi.Application.Enrollments.Commands;

public class UpdateCourseHandler(
    ICourseService service,
    ICachedCourseService cachedService)
    : IRequestHandler<UpdateCourseCommand, bool>
{
    public async Task<bool> Handle(UpdateCourseCommand command, CancellationToken ct)
    {
        await service.UpdateAsync(command, ct);
        await cachedService.InvalidateCourseCacheAsync(ct);
        return true;
    }
}