using MediatR;

namespace TmsApi.Application.Enrollments.Commands;

public class UpdateCourseCommand : IRequest<bool>
{
    public string Code { get; set; } = default!;
    public string Title { get; set; } = default!;
}