using LuxeBeautyPlace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxeBeautyPlace.Api.Data;

public class SalonDbContext(DbContextOptions<SalonDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<GalleryPhoto> GalleryPhotos => Set<GalleryPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(40);

        modelBuilder.Entity<Service>()
            .Property(service => service.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(product => product.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Service>()
            .Property(service => service.Category)
            .HasMaxLength(120);

        modelBuilder.Entity<Service>()
            .Property(service => service.PriceLabel)
            .HasMaxLength(80);

        modelBuilder.Entity<Appointment>()
            .Property(appointment => appointment.Status)
            .HasConversion<string>()
            .HasMaxLength(40);

        modelBuilder.Entity<Testimonial>()
            .Property(testimonial => testimonial.Rating)
            .HasDefaultValue(5);

        modelBuilder.Entity<Testimonial>()
            .Property(testimonial => testimonial.Status)
            .HasConversion<string>()
            .HasMaxLength(40);
    }
}
