# Database and Admin Guide

## What stores what?

The React frontend displays the website. The ASP.NET API reads and writes SQL Server Express. Browser-only sample data is not your database.

Database: `LuxeBeautyPlace` on `.\SQLEXPRESS`.

- `Users`: admin/customer names, email addresses, phones, roles, and salted password hashes. Passwords are not stored as readable text.
- `Services`: service names, prices, price labels, duration in minutes, categories, and image URLs.
- `Appointments`: contact details, date, time, status, and the selected `ServiceId`. Guest bookings do not need a user account.
- `Testimonials`: reviews, ratings, and approval status. Only approved reviews appear publicly.
- `GalleryPhotos`: gallery titles, categories, and image URLs.
- `Products`: retail descriptions, prices, stock, and images. The current admin screen does not include a products editor.

`ServiceId` connects a booking to its service. A service with bookings cannot be deleted. `UserId` is optional for bookings and reviews.

## Start the website

In one PowerShell terminal, from the project folder:

```powershell
cd backend
dotnet run
```

In a second terminal:

```powershell
cd frontend
npm.cmd run dev
```

Open `http://127.0.0.1:5173`. Admin access is `http://127.0.0.1:5173/#/admin`, also available through the footer key icon. API health is `http://127.0.0.1:5117/api/health`.

The README previously referred to `start-frontend.bat`, but that file is not present. Use the commands above.

## Log in to the existing admin

The database check found one admin account. In `backend/.env`, find `Admin__DefaultEmail` and `Admin__DefaultPassword`. These are the configured initial login details. Do not share or commit this file.

Use those details only if the account still uses its initial password. An existing account's password is now preserved across API restarts. If the password was changed, use that changed password instead. The configuration is not a list of all current database passwords.

After signing in, use **Change Password** to set a strong, unique password. The dashboard manages services, booking statuses, review approvals, and additional admin accounts.

If you forgot the password while running locally in Development mode, choose **Forgot password?**, enter the admin email and server setup code from `Admin__SetupCode` in `backend/.env`, select **Get Reset Code**, then enter a new password and save. The reset code is filled in locally and expires after 20 minutes. Restarting the API invalidates reset codes. Keep the setup code private. This development feature must not be exposed publicly. Production recovery returns an explicit unavailable message because email delivery is not implemented.

## Create an admin account

With an existing admin: sign in, open **Admin Accounts**, fill in the new person's name, email, phone, and temporary password, and select **Create Admin**. That person can sign in with their email and temporary password and then change it.

On a fresh database with no admins, the API creates the initial admin from `Admin__Default*` settings if configured. To use the first-admin registration form instead, clear the default email/password settings in both configuration sources before first startup. Open the admin page and enter the setup code from `Admin__SetupCode` in `backend/.env`, plus your chosen account details. Registration closes once an admin exists.

Never put a plaintext password directly in `Users.PasswordHash`. The application must generate the hash. SQL Server's Windows login is separate from the website admin login.

## Configuration and safety

`backend/.env` overrides `backend/appsettings.json`; existing process environment variables take precedence. The connection string uses Windows authentication, so the Windows account running the API needs database access. `frontend/.env` contains `VITE_API_URL`, the public API address. Never put passwords or signing keys in a `VITE_` variable: those values are shipped to browsers.

Service images belong in `frontend/public/service-images`. Store their URLs as `/service-images/filename.jpg`, not a Windows filesystem path. The homepage and service listing now fetch database services so admin edits can appear after refresh.

The review aligned legacy generic database photos with the existing service-specific frontend photos, without changing prices or booking relationships. Newly seeded catalogs now use the same photo assignments. `npm.cmd test` checks both catalogs against each other and checks the public image files. Photos in `frontend/src/public` and `backend/Data/public` are unused duplicates, retained rather than deleted.

## What was verified locally

Frontend production build, backend build, public JPEG decoding, homepage/service image loading, configured admin login, protected dashboard access, creation of an additional test admin, password change and authorized local recovery, review approval/rejection, and booking creation/status changes were checked. Temporary accounts, reviews, and bookings were removed after testing. Existing admin credentials and the four original bookings were preserved.

The first-admin registration path on a completely empty database was reviewed but not exercised against your existing database. This review is not a production security certification.

`database/schema.sql` is only for a fresh database. It now refuses to proceed if salon tables already exist and no longer drops tables. It is not an update/migration script. Normal API startup creates a fresh database when needed and seeds an empty service catalog without overwriting existing services.

Before production: replace development signing keys/setup codes/default credentials, configure HTTPS and permitted frontend origins, add rate limiting and real password-reset delivery, back up SQL Server, define appointment availability/conflict rules, and replace contact placeholders. Email/WhatsApp booking notifications are not implemented. Products/gallery have API endpoints but no dashboard editors.
