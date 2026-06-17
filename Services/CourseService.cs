using TmsApi.Entities;

public interface ICourseService
{
    Task<Course> CreateAsync(string code, string title, int capacity);
    Task<Course?> GetByIdAsync(string code);
    Task<IReadOnlyList<Course>> GetAllAsync();
    Task<bool> DeleteAsync(string code);

}


public class CourseService : ICourseService
{
    private readonly Dictionary<string, Course> _store = new();
    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public Task<Course> CreateAsync(
        string code,
        string title,
        int capacity)
    {
        var course = new Course
        {
            Code = code,
            Title = title,
            Capacity = capacity,
        };

        _store[code] = course;
        _logger.LogInformation("Created course {CourseCode}", code);
        return Task.FromResult(course);
    }

    public Task<Course?> GetByIdAsync(string code)
    {
        _store.TryGetValue(code, out var course);

        if (course is null)
        {
            _logger.LogWarning("Course {CourseCode} not found", code);
        }

        return Task.FromResult(course);
    }

    public Task<IReadOnlyList<Course>> GetAllAsync()
    {
        IReadOnlyList<Course> courses = _store.Values.ToList();
        return Task.FromResult(courses);
    }

    public Task<bool> DeleteAsync(string code)
    {
        var removed = _store.Remove(code);

        if (removed)
        {
            _logger.LogInformation("Deleted course {CourseCode}",code);
        }
        else
        {
            _logger.LogWarning("Delete failed course {CourseCode} not found", code);
        }

        return Task.FromResult(removed);
    }
}