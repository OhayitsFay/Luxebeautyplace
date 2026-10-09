IF DB_ID(N'LuxeBeautyPlace') IS NULL
BEGIN
    CREATE DATABASE LuxeBeautyPlace;
END
GO

USE LuxeBeautyPlace;
GO

-- This reset script is for a NEW database only. Never erase existing salon data.
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.Services', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.Appointments', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.Testimonials', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.GalleryPhotos', N'U') IS NOT NULL
BEGIN
    ;THROW 51000, 'Existing salon tables detected. Use normal API startup; this script is only for a new database.', 1;
END

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(160) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    Phone NVARCHAR(40) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(40) NOT NULL DEFAULT 'Customer'
);

CREATE TABLE Services (
    ServiceID INT IDENTITY(1,1) PRIMARY KEY,
    ServiceName NVARCHAR(160) NOT NULL,
    Category NVARCHAR(120) NOT NULL DEFAULT '',
    Description NVARCHAR(MAX) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    PriceLabel NVARCHAR(80) NOT NULL DEFAULT '',
    Duration INT NOT NULL,
    ImageURL NVARCHAR(600) NOT NULL DEFAULT ''
);

CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(160) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    ImageURL NVARCHAR(600) NOT NULL DEFAULT ''
);

CREATE TABLE Appointments (
    AppointmentID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL,
    ServiceID INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    AppointmentTime TIME NOT NULL,
    FullName NVARCHAR(160) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(40) NOT NULL,
    Stylist NVARCHAR(120) NULL,
    Notes NVARCHAR(MAX) NULL,
    Status NVARCHAR(40) NOT NULL DEFAULT 'Pending',
    CONSTRAINT FK_Appointments_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Appointments_Services FOREIGN KEY (ServiceID) REFERENCES Services(ServiceID)
);

CREATE TABLE Testimonials (
    TestimonialID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL,
    Name NVARCHAR(160) NOT NULL,
    Review NVARCHAR(MAX) NOT NULL,
    Rating INT NOT NULL,
    Status NVARCHAR(40) NOT NULL DEFAULT 'Pending',
    CONSTRAINT FK_Testimonials_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE GalleryPhotos (
    GalleryPhotoID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(160) NOT NULL,
    Category NVARCHAR(80) NOT NULL,
    ImageURL NVARCHAR(600) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

INSERT INTO GalleryPhotos (Title, Category, ImageURL) VALUES
    (N'Braided finish', N'Hair', N'/service-images/photo_2026-08-20_00-29-03 (2).jpg'),
    (N'Fresh pedicure', N'Pedicure', N'/service-images/photo_2026-08-20_23-00-53.jpg'),
    (N'Ponytail finish', N'Hair', N'/service-images/photo_2026-08-20_00-28-58.jpg');

INSERT INTO Products (ProductName, Description, Price, StockQuantity, ImageURL) VALUES
    (N'Luxe Edge Gloss', N'Polishing edge control for smooth salon finishes.', 12000, 25, N'/photo_2026-08-20_00-29-04.jpg'),
    (N'Cuticle Oil', N'Daily nail and cuticle aftercare oil.', 9000, 30, N'/photo_2026-08-20_00-29-05.jpg');

INSERT INTO Testimonials (Name, Review, Rating, Status) VALUES
    (N'Amara K.', N'The booking experience was smooth and the service felt premium from start to finish.', 5, N'Approved'),
    (N'Chidinma O.', N'My nails lasted beautifully and the team was warm and professional.', 5, N'Approved');
