using LuxeBeautyPlace.Api.Data;
using LuxeBeautyPlace.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

LoadEnvironmentFile(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
LoadEnvironmentFile(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing DefaultConnection connection string.");

builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseSqlServer(connectionString));

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Missing Jwt:Issuer.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Missing Jwt:Audience.");
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Missing Jwt:SigningKey.");
if (Encoding.UTF8.GetByteCount(jwtSigningKey) < 32)
    throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 UTF-8 bytes.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().RequireRole("Admin").Build();
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://127.0.0.1:5173", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
    await db.Database.EnsureCreatedAsync();
    await EnsureDatabaseShapeAsync(db);
    await SyncServiceCatalogAsync(db);
    await EnsureDefaultAdminAsync(db, scope.ServiceProvider.GetRequiredService<IConfiguration>());
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");
var resetTokens = new ConcurrentDictionary<string, PasswordResetEntry>();

api.MapGet("/health", async (SalonDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return Results.Ok(new
    {
        status = canConnect ? "Healthy" : "Unhealthy",
        database = db.Database.GetDbConnection().Database,
        server = db.Database.GetDbConnection().DataSource
    });
});

api.MapGet("/admin/setup-status", async (SalonDbContext db) =>
{
    var adminCount = await db.Users.CountAsync(user => user.Role == UserRole.Admin);
    return Results.Ok(new { setupRequired = adminCount == 0, adminCount });
});

api.MapPost("/admin/register", async (AdminRegisterRequest request, SalonDbContext db, IConfiguration configuration) =>
{
    if (!AdminRequestValidator.IsValid(request.FullName, request.Email, request.Phone, request.Password))
    {
        return Results.BadRequest(new { message = "Full name, email, phone, and a password of at least 8 characters are required." });
    }

    var hasAdmin = await db.Users.AnyAsync(user => user.Role == UserRole.Admin);
    if (hasAdmin) return Results.Conflict(new { message = "An admin account already exists." });

    var setupCode = configuration["Admin:SetupCode"];
    if (string.IsNullOrWhiteSpace(setupCode) || request.SetupCode != setupCode)
    {
        return Results.Unauthorized();
    }

    var email = request.Email.Trim().ToLowerInvariant();
    var emailExists = await db.Users.AnyAsync(user => user.Email == email);
    if (emailExists) return Results.Conflict(new { message = "That email is already registered." });

    var user = new User
    {
        FullName = request.FullName.Trim(),
        Email = email,
        Phone = request.Phone.Trim(),
        PasswordHash = PasswordHasher.Hash(request.Password),
        Role = UserRole.Admin
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created("/api/admin/login", new { user.Email, user.FullName });
});

api.MapPost("/admin/setup-admin", async (AdminRegisterRequest request, SalonDbContext db, IConfiguration configuration, IWebHostEnvironment environment) =>
{
    if (!environment.IsDevelopment())
    {
        return Results.NotFound();
    }

    if (!AdminRequestValidator.IsValid(request.FullName, request.Email, request.Phone, request.Password))
    {
        return Results.BadRequest(new { message = "Full name, email, phone, and a password of at least 8 characters are required." });
    }

    var setupCode = configuration["Admin:SetupCode"];
    if (string.IsNullOrWhiteSpace(setupCode) || request.SetupCode != setupCode)
    {
        return Results.Unauthorized();
    }

    var email = request.Email.Trim().ToLowerInvariant();
    var firstAdmin = await db.Users
        .Where(user => user.Role == UserRole.Admin)
        .OrderBy(user => user.UserId)
        .FirstOrDefaultAsync();

    var existingEmailOwner = await db.Users.SingleOrDefaultAsync(user => user.Email == email);
    if (firstAdmin is not null)
        return Results.Conflict(new { message = "An admin already exists. Sign in or use password reset instead." });
    if (existingEmailOwner is not null)
    {
        return Results.Conflict(new { message = "That email is already registered to another account." });
    }

    if (firstAdmin is null)
    {
        firstAdmin = new User { Role = UserRole.Admin };
        db.Users.Add(firstAdmin);
    }

    firstAdmin.FullName = request.FullName.Trim();
    firstAdmin.Email = email;
    firstAdmin.Phone = request.Phone.Trim();
    firstAdmin.PasswordHash = PasswordHasher.Hash(request.Password);
    firstAdmin.Role = UserRole.Admin;

    await db.SaveChangesAsync();
    return Results.Ok(new { firstAdmin.Email, firstAdmin.FullName });
});

api.MapGet("/admin/accounts", async (SalonDbContext db) =>
    await db.Users
        .Where(user => user.Role == UserRole.Admin)
        .OrderBy(user => user.FullName)
        .Select(user => new { user.UserId, user.FullName, user.Email, user.Phone })
        .ToListAsync()).RequireAuthorization();

api.MapPost("/admin/accounts", async (CreateAdminRequest request, SalonDbContext db) =>
{
    if (!AdminRequestValidator.IsValid(request.FullName, request.Email, request.Phone, request.Password))
    {
        return Results.BadRequest(new { message = "Full name, email, phone, and a password of at least 8 characters are required." });
    }

    var email = request.Email.Trim().ToLowerInvariant();
    var emailExists = await db.Users.AnyAsync(user => user.Email == email);
    if (emailExists) return Results.Conflict(new { message = "That email is already registered." });

    var user = new User
    {
        FullName = request.FullName.Trim(),
        Email = email,
        Phone = request.Phone.Trim(),
        PasswordHash = PasswordHasher.Hash(request.Password),
        Role = UserRole.Admin
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/api/admin/accounts/{user.UserId}", new { user.UserId, user.FullName, user.Email, user.Phone });
}).RequireAuthorization();

api.MapPost("/admin/login", async (AdminLoginRequest login, SalonDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
        return Results.BadRequest(new { message = "Email and password are required." });
    var email = login.Email.Trim().ToLowerInvariant();
    var user = await db.Users.SingleOrDefaultAsync(user => user.Email == email && user.Role == UserRole.Admin);

    if (user is null || !PasswordHasher.Verify(login.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var token = CreateAdminToken(user.Email, user.FullName);

    return Results.Ok(new
    {
        token = new JwtSecurityTokenHandler().WriteToken(token),
        email = user.Email,
        fullName = user.FullName,
        expiresAt = DateTime.SpecifyKind(token.ValidTo, DateTimeKind.Utc)
    });
});

api.MapPost("/admin/change-password", async (ChangePasswordRequest request, ClaimsPrincipal principal, SalonDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        return Results.BadRequest(new { message = "The new password must be at least 8 characters." });
    var email = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
        principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (email is null) return Results.Unauthorized();

    var user = await db.Users.SingleOrDefaultAsync(user => user.Email == email && user.Role == UserRole.Admin);
    if (user is null || !PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapPost("/admin/forgot-password", async (ForgotPasswordRequest request, SalonDbContext db, IWebHostEnvironment environment, IConfiguration configuration) =>
{
    // Local recovery is not an email-delivery service. Require the server owner's setup code.
    if (!environment.IsDevelopment())
        return Results.Json(new { message = "Password recovery is not configured. Contact another administrator." }, statusCode: 503);
    var setupCode = configuration["Admin:SetupCode"];
    if (string.IsNullOrWhiteSpace(setupCode) || request.SetupCode != setupCode) return Results.Unauthorized();
    if (string.IsNullOrWhiteSpace(request.Email)) return Results.BadRequest();
    var email = request.Email.Trim().ToLowerInvariant();
    var user = await db.Users.SingleOrDefaultAsync(user => user.Email == email && user.Role == UserRole.Admin);

    if (user is null)
    {
        return Results.Ok(new { message = "If that admin email exists, a reset code has been created." });
    }

    var resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
    resetTokens[email] = new PasswordResetEntry(resetToken, DateTimeOffset.UtcNow.AddMinutes(20));

    return Results.Ok(new
    {
        message = "Reset code created. In production this should be emailed to the admin.",
        resetToken = environment.IsDevelopment() ? resetToken : null
    });
});

api.MapPost("/admin/reset-password", async (ResetPasswordRequest request, SalonDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.ResetToken))
        return Results.BadRequest();
    if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        return Results.BadRequest(new { message = "The new password must be at least 8 characters." });
    var email = request.Email.Trim().ToLowerInvariant();
    if (!resetTokens.TryGetValue(email, out var entry) ||
        entry.ExpiresAt < DateTimeOffset.UtcNow ||
        !CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(entry.Token),
            Encoding.UTF8.GetBytes(request.ResetToken.Trim().ToLowerInvariant())))
    {
        return Results.Unauthorized();
    }

    var user = await db.Users.SingleOrDefaultAsync(user => user.Email == email && user.Role == UserRole.Admin);
    if (user is null) return Results.Unauthorized();

    user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
    resetTokens.TryRemove(email, out _);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapGet("/services", async (SalonDbContext db) =>
    await db.Services
        .OrderBy(service => service.Category)
        .ThenBy(service => service.ServiceName)
        .ToListAsync());

api.MapGet("/services/{id:int}", async (int id, SalonDbContext db) =>
    await db.Services.FindAsync(id) is { } service ? Results.Ok(service) : Results.NotFound());

api.MapPost("/services", async (Service service, SalonDbContext db) =>
{
    if (!RequestValidator.ValidService(service))
        return Results.BadRequest(new { message = "Service name, nonnegative price, and positive duration are required." });
    service.Appointments = [];
    db.Services.Add(service);
    await db.SaveChangesAsync();
    return Results.Created($"/api/services/{service.ServiceId}", service);
}).RequireAuthorization();

api.MapPut("/services/{id:int}", async (int id, Service updated, SalonDbContext db) =>
{
    if (!RequestValidator.ValidService(updated))
        return Results.BadRequest(new { message = "Service name, nonnegative price, and positive duration are required." });
    var service = await db.Services.FindAsync(id);
    if (service is null) return Results.NotFound();

    service.ServiceName = updated.ServiceName;
    service.Category = updated.Category;
    service.Description = updated.Description;
    service.Price = updated.Price;
    service.PriceLabel = updated.PriceLabel;
    service.Duration = updated.Duration;
    service.ImageUrl = updated.ImageUrl;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapDelete("/services/{id:int}", async (int id, SalonDbContext db) =>
{
    var service = await db.Services.FindAsync(id);
    if (service is null) return Results.NotFound();

    if (await db.Appointments.AnyAsync(appointment => appointment.ServiceId == id))
        return Results.Conflict(new { message = "This service has bookings and cannot be deleted." });
    db.Services.Remove(service);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapGet("/products", async (SalonDbContext db) =>
    await db.Products.OrderBy(product => product.ProductName).ToListAsync());

api.MapPost("/products", async (Product product, SalonDbContext db) =>
{
    if (!RequestValidator.ValidProduct(product)) return Results.BadRequest(new { message = "Valid product details, nonnegative price and stock are required." });
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.ProductId}", product);
}).RequireAuthorization();

api.MapPut("/products/{id:int}", async (int id, Product updated, SalonDbContext db) =>
{
    if (!RequestValidator.ValidProduct(updated)) return Results.BadRequest(new { message = "Valid product details, nonnegative price and stock are required." });
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    product.ProductName = updated.ProductName;
    product.Description = updated.Description;
    product.Price = updated.Price;
    product.StockQuantity = updated.StockQuantity;
    product.ImageUrl = updated.ImageUrl;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapDelete("/products/{id:int}", async (int id, SalonDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapPost("/appointments", async (Appointment appointment, SalonDbContext db) =>
{
    if (!await db.Services.AnyAsync(service => service.ServiceId == appointment.ServiceId))
        return Results.BadRequest(new { message = "Select a valid service." });
    if (!RequestValidator.RequiredText(appointment.FullName, 160) || !RequestValidator.ValidEmail(appointment.Email) ||
        !RequestValidator.RequiredText(appointment.Phone, 40) || appointment.Stylist?.Length > 120 ||
        appointment.AppointmentDate < DateOnly.FromDateTime(DateTime.Today))
        return Results.BadRequest(new { message = "Contact details and a date today or later are required." });
    appointment.UserId = null;
    appointment.User = null;
    appointment.Service = null;
    appointment.Status = AppointmentStatus.Pending;
    db.Appointments.Add(appointment);
    await db.SaveChangesAsync();
    return Results.Created($"/api/appointments/{appointment.AppointmentId}", appointment);
});

api.MapGet("/appointments", async (SalonDbContext db) =>
    await db.Appointments
        .Include(appointment => appointment.User)
        .Include(appointment => appointment.Service)
        .OrderByDescending(appointment => appointment.AppointmentDate)
        .ToListAsync()).RequireAuthorization();

api.MapPatch("/appointments/{id:int}/status/{status}", async (int id, AppointmentStatus status, SalonDbContext db) =>
{
    if (!Enum.IsDefined(status)) return Results.BadRequest(new { message = "Invalid booking status." });
    var appointment = await db.Appointments.FindAsync(id);
    if (appointment is null) return Results.NotFound();

    appointment.Status = status;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapPost("/testimonials", async (Testimonial testimonial, SalonDbContext db) =>
{
    if (!RequestValidator.RequiredText(testimonial.Name, 160) || string.IsNullOrWhiteSpace(testimonial.Review) ||
        testimonial.Rating < 1 || testimonial.Rating > 5)
        return Results.BadRequest(new { message = "Name, review, and a rating from 1 to 5 are required." });
    testimonial.UserId = null;
    testimonial.User = null;
    testimonial.Status = ReviewStatus.Pending;
    db.Testimonials.Add(testimonial);
    await db.SaveChangesAsync();
    return Results.Created($"/api/testimonials/{testimonial.TestimonialId}", testimonial);
});

api.MapGet("/testimonials", async (SalonDbContext db) =>
    await db.Testimonials.Where(testimonial => testimonial.Status == ReviewStatus.Approved).ToListAsync());

api.MapGet("/admin/testimonials", async (SalonDbContext db) =>
    await db.Testimonials.OrderByDescending(testimonial => testimonial.TestimonialId).ToListAsync()).RequireAuthorization();

api.MapPatch("/testimonials/{id:int}/status/{status}", async (int id, ReviewStatus status, SalonDbContext db) =>
{
    if (!Enum.IsDefined(status)) return Results.BadRequest(new { message = "Invalid review status." });
    var testimonial = await db.Testimonials.FindAsync(id);
    if (testimonial is null) return Results.NotFound();

    testimonial.Status = status;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapDelete("/testimonials/{id:int}", async (int id, SalonDbContext db) =>
{
    var testimonial = await db.Testimonials.FindAsync(id);
    if (testimonial is null) return Results.NotFound();

    db.Testimonials.Remove(testimonial);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapGet("/gallery", async (SalonDbContext db) =>
    await db.GalleryPhotos.OrderByDescending(photo => photo.CreatedAt).ToListAsync());

api.MapPost("/gallery", async (GalleryPhoto photo, SalonDbContext db) =>
{
    if (!RequestValidator.ValidPhoto(photo)) return Results.BadRequest(new { message = "Title, category and image URL are required." });
    photo.CreatedAt = DateTime.UtcNow;
    db.GalleryPhotos.Add(photo);
    await db.SaveChangesAsync();
    return Results.Created($"/api/gallery/{photo.GalleryPhotoId}", photo);
}).RequireAuthorization();

api.MapPut("/gallery/{id:int}", async (int id, GalleryPhoto updated, SalonDbContext db) =>
{
    if (!RequestValidator.ValidPhoto(updated)) return Results.BadRequest(new { message = "Title, category and image URL are required." });
    var photo = await db.GalleryPhotos.FindAsync(id);
    if (photo is null) return Results.NotFound();

    photo.Title = updated.Title;
    photo.Category = updated.Category;
    photo.ImageUrl = updated.ImageUrl;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapDelete("/gallery/{id:int}", async (int id, SalonDbContext db) =>
{
    var photo = await db.GalleryPhotos.FindAsync(id);
    if (photo is null) return Results.NotFound();

    db.GalleryPhotos.Remove(photo);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.Run();

static async Task EnsureDatabaseShapeAsync(SalonDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('Services', 'Category') IS NULL
        BEGIN
            ALTER TABLE Services ADD Category NVARCHAR(120) NOT NULL CONSTRAINT DF_Services_Category DEFAULT '';
        END

        IF COL_LENGTH('Services', 'PriceLabel') IS NULL
        BEGIN
            ALTER TABLE Services ADD PriceLabel NVARCHAR(80) NOT NULL CONSTRAINT DF_Services_PriceLabel DEFAULT '';
        END

        IF OBJECT_ID(N'Products', N'U') IS NULL
        BEGIN
            CREATE TABLE Products (
                ProductId INT IDENTITY(1,1) PRIMARY KEY,
                ProductName NVARCHAR(160) NOT NULL,
                Description NVARCHAR(MAX) NOT NULL,
                Price DECIMAL(18,2) NOT NULL,
                StockQuantity INT NOT NULL DEFAULT 0,
                ImageUrl NVARCHAR(600) NOT NULL DEFAULT ''
            );
        END
        """);
}

static async Task SyncServiceCatalogAsync(SalonDbContext db)
{
    // Seed only an empty catalog; preserve dashboard edits on restart.
    if (await db.Services.AnyAsync()) return;
    var catalogNames = ServiceCatalog.Services.Select(service => service.ServiceName).ToHashSet();
    var existingServices = await db.Services.OrderBy(service => service.ServiceId).ToListAsync();
    var needsSync = existingServices.Count != ServiceCatalog.Services.Count ||
        existingServices.Any(service => !catalogNames.Contains(service.ServiceName)) ||
        existingServices.Any(service => string.IsNullOrWhiteSpace(service.Category) || string.IsNullOrWhiteSpace(service.PriceLabel)) ||
        existingServices.Take(ServiceCatalog.Services.Count)
            .Zip(ServiceCatalog.Services, (existing, catalog) => existing.ImageUrl != catalog.ImageUrl)
            .Any(mismatched => mismatched);

    if (!needsSync) return;

    for (var i = 0; i < ServiceCatalog.Services.Count; i++)
    {
        var catalogService = ServiceCatalog.Services[i];
        var service = i < existingServices.Count ? existingServices[i] : new Service();

        service.ServiceName = catalogService.ServiceName;
        service.Category = catalogService.Category;
        service.Description = catalogService.Description;
        service.Price = catalogService.Price;
        service.PriceLabel = catalogService.PriceLabel;
        service.Duration = catalogService.Duration;
        service.ImageUrl = catalogService.ImageUrl;

        if (service.ServiceId == 0) db.Services.Add(service);
    }

    var catalogSize = ServiceCatalog.Services.Count;
    var bookedServiceIds = await db.Appointments.Select(appointment => appointment.ServiceId).ToListAsync();
    var unusedExtraServices = existingServices
        .Skip(catalogSize)
        .Where(service => !bookedServiceIds.Contains(service.ServiceId));

    db.Services.RemoveRange(unusedExtraServices);
    await db.SaveChangesAsync();
}

static async Task EnsureDefaultAdminAsync(SalonDbContext db, IConfiguration configuration)
{
    if (await db.Users.AnyAsync(user => user.Role == UserRole.Admin)) return;
    if (string.IsNullOrWhiteSpace(configuration["Admin:DefaultEmail"]) ||
        string.IsNullOrWhiteSpace(configuration["Admin:DefaultPassword"])) return;
    var email = (configuration["Admin:DefaultEmail"] ?? "admin@luxebeautyplace.com").Trim().ToLowerInvariant();
    var password = configuration["Admin:DefaultPassword"] ?? "LuxeAdmin2026!";
    var fullName = configuration["Admin:DefaultFullName"] ?? "Luxe Beauty Admin";
    var phone = configuration["Admin:DefaultPhone"] ?? "+2348153144870";

    if (password.Length < 8)
    {
        throw new InvalidOperationException("Admin:DefaultPassword must be at least 8 characters.");
    }

    var admin = await db.Users.SingleOrDefaultAsync(user => user.Email == email);
    if (admin is null)
    {
        db.Users.Add(new User
        {
            FullName = fullName,
            Email = email,
            Phone = phone,
            PasswordHash = PasswordHasher.Hash(password),
            Role = UserRole.Admin
        });

        await db.SaveChangesAsync();
        return;
    }

    var changed = false;
    if (admin.Role != UserRole.Admin)
    {
        admin.Role = UserRole.Admin;
        changed = true;
    }

    if (string.IsNullOrWhiteSpace(admin.PasswordHash) || !PasswordHasher.Verify(password, admin.PasswordHash))
    {
        admin.PasswordHash = PasswordHasher.Hash(password);
        changed = true;
    }

    if (changed) await db.SaveChangesAsync();
}

JwtSecurityToken CreateAdminToken(string email, string fullName)
{
    var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim(ClaimTypes.NameIdentifier, email),
        new Claim(ClaimTypes.Name, fullName),
        new Claim(ClaimTypes.Role, "Admin")
    };

    return new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: credentials);
}

static void LoadEnvironmentFile(string path)
{
    if (!File.Exists(path)) return;

    foreach (var line in File.ReadAllLines(path))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;

        var separatorIndex = trimmed.IndexOf('=');
        if (separatorIndex <= 0) continue;

        var key = trimmed[..separatorIndex].Trim();
        var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(key)) continue;

        if (Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, value);
    }
}

public record AdminLoginRequest(string Email, string Password);
public record AdminRegisterRequest(string FullName, string Email, string Phone, string Password, string SetupCode);
public record CreateAdminRequest(string FullName, string Email, string Phone, string Password);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ForgotPasswordRequest(string Email, string? SetupCode);
public record ResetPasswordRequest(string Email, string ResetToken, string NewPassword);
public record PasswordResetEntry(string Token, DateTimeOffset ExpiresAt);

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"pbkdf2-sha256.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash)) return false;
        var parts = passwordHash.Split('.');
        if (parts.Length != 4 || parts[0] != "pbkdf2-sha256") return false;
        if (!int.TryParse(parts[1], out var iterations) || iterations < 1 || iterations > 1_000_000) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedKey = Convert.FromBase64String(parts[3]);
            if (salt.Length != SaltSize || expectedKey.Length != KeySize) return false;
            var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, KeySize);
            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
        catch (FormatException) { return false; }
    }
}

public static class AdminRequestValidator
{
    public static bool IsValid(string fullName, string email, string phone, string password) =>
        RequestValidator.RequiredText(fullName, 160) &&
        RequestValidator.ValidEmail(email) &&
        RequestValidator.RequiredText(phone, 40) &&
        !string.IsNullOrWhiteSpace(password) &&
        password.Length >= 8;
}

public static class RequestValidator
{
    public static bool RequiredText(string? value, int maxLength) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    public static bool ValidEmail(string? email) => RequiredText(email, 256) &&
        System.Net.Mail.MailAddress.TryCreate(email, out var address) && address.Address == email!.Trim();

    public static bool ValidService(Service service) => RequiredText(service.ServiceName, 160) &&
        service.Category is not null && service.Category.Length <= 120 &&
        service.Description is not null && service.PriceLabel is not null && service.PriceLabel.Length <= 80 &&
        service.ImageUrl is not null && service.ImageUrl.Length <= 600 && service.Price >= 0 && service.Duration > 0;

    public static bool ValidProduct(Product product) => RequiredText(product.ProductName, 160) &&
        product.Description is not null && product.Price >= 0 && product.StockQuantity >= 0 &&
        product.ImageUrl is not null && product.ImageUrl.Length <= 600;

    public static bool ValidPhoto(GalleryPhoto photo) => RequiredText(photo.Title, 160) &&
        RequiredText(photo.Category, 80) && RequiredText(photo.ImageUrl, 600);
}
