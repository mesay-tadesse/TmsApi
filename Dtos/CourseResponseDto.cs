namespace TmsApi.Dtos;

public record CourseResponseDto(
    int Id,
    string Code,
    String Title,
    int MaxCapacity,
    int EnrollmentCount

);
