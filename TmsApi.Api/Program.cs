using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TmsApi;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using TmsApi.Application.DTOs;
using TmsApi.Infrastructure.Services;
using TmsApi.Application.Interfaces;
using TmsApi.Api.Filters;
using Asp.Versioning; 
using TmsApi.Api.Middleware;
using FluentValidation;
using MediatR;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    // options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
    })
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentHandler).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
// LoggingBehavior FIRST—it must wrap ValidationBehavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<TmsDbContext>(
    options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")));
        // .LogTo(Console.WriteLine, LogLevel.Information)
        // .EnableSensitiveDataLogging());


// builder.Services
//     .AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions,
//         TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();



builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService,CourseService>();

builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// builder.Services.AddControllers();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<V1DeprecationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // app.MapScalarApiReference();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient);
        // Tell Scalar to pull both documents into its sidebar dropdown
        options
            .AddDocument("v1", "API Version 1.0")
            .AddDocument("v2", "API Version 2.0");
    });

    
    // using var scope = app.Services.CreateScope();

    // var context = scope.ServiceProvider
    //     .GetRequiredService<TmsDbContext>();

    // await DataSeeder.SeedAsync(context);
}
else{
    app.UseExceptionHandler();
}

app.MapGet("/api/assessments/results", () => Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    }))
.RequireAuthorization();

// app.MapGet("/api/enrollments/worker-smoke",
//     (EnrollmentWorker worker) =>
// {
//     worker.ProcessBatch();

//     return Results.Ok("processed");
// });

// app.MapGet("/api/error", () =>
// {
//     throw new TmsDatabaseException(
//         "Simulated database failure for ProblemDetails testing");
// });
// app.UseHttpsRedirection();
app.MapControllers();



// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
//     context.Database.Migrate();

//     if (!context.Students.Any())
//     {
//         var students = new List<Student>
//         {
//             new() {RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, Age = 19, IsActive = true},
//             new() {RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m , Age = 25, IsActive = true},
//             new() {RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m , Age = 21, IsActive = false},
//             new() {RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince", GPA = 3.9m , Age = 24, IsActive = true},
//             new() {RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright", GPA = 2.5m , Age = 22, IsActive = true},

//         };

//         context.Students.AddRange(students);
//         var courses = new List<Course>
//         {
//             new() { Code = "CS-101", Title = "Introduction to Computer Science", Capacity = 30 },
//             new() { Code = "CS-201", Title = "Data Structures and Algorithms", Capacity = 25 },
//             new() { Code = "MAT-101", Title = "Calculus I", Capacity = 40 }
//         };
//         context.Courses.AddRange(courses);
//         context.SaveChanges();

//        var enrollments = new List<Enrollment>
//        {
//             new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
//             new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
//             new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
//             new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
//         };
        
//         context.Enrollments.AddRange(enrollments);
//         context.SaveChanges();
//     }
// }
   
app.Run();
