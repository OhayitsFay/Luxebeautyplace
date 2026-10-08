namespace LuxeBeautyPlace.Api.Models;

public class Service
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceLabel { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<Appointment> Appointments { get; set; } = [];
}
