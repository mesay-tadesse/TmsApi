namespace TmsApi.Domain.Entities;

public class Student
{
    public int Id { get; set;}
    public required string RegistrationNumber { get; set;}
    public required string Name { get; set;}
    public decimal GPA { get; set;}
    public bool IsActive {get; set; }
    public bool IsDeleted { get; set; }
    public uint Version { get; set; }
    public int Age { get; set;}

    public ICollection<Enrollment> Enrollments {get; set;} = new List<Enrollment>(); 
    
    public ICollection<Assessment> Assessments {get; set;} = new List<Assessment>();
    public ICollection<Certificate> Certificates {get; set;} = new List<Certificate>();

}