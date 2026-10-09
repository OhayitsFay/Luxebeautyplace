namespace LuxeBeautyPlace.Api.Models;

public enum UserRole
{
    Customer,
    Admin
}

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public List<Appointment> Appointments { get; set; } = [];
    public List<Testimonial> Testimonials { get; set; } = [];
}
