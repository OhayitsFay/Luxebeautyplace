namespace LuxeBeautyPlace.Api.Models;

public enum AppointmentStatus
{
    Pending,
    Approved,
    Rescheduled,
    Cancelled,
    Completed
}

public class Appointment
{
    public int AppointmentId { get; set; }
    public int? UserId { get; set; }
    public int ServiceId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Stylist { get; set; }
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public User? User { get; set; }
    public Service? Service { get; set; }
}
