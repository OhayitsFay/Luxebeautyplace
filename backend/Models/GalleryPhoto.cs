namespace LuxeBeautyPlace.Api.Models;

public class GalleryPhoto
{
    public int GalleryPhotoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
