namespace LuxeBeautyPlace.Api.Models;

public enum ReviewStatus
{
    Pending,
    Approved,
    Rejected
}

public class Testimonial
{
    public int TestimonialId { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Review { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public User? User { get; set; }
}
