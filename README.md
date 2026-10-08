# Luxe Beauty Place

A service-based salon appointment booking site built with a React frontend and an ASP.NET Core Web API backend.

## Stack

- Frontend: React, React Router, Axios, Redux Toolkit
- Backend: ASP.NET Core Web API
- Database target: SQL Server through Entity Framework Core

## Project Structure

```text
frontend/   React client app
backend/    ASP.NET Core API
database/   SQL Server starter schema
```

## Run Locally

On Windows, you can start the frontend by double-clicking:

```text
start-frontend.bat
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Then open:

```text
http://127.0.0.1:5173
```

Backend:

```bash
cd backend
dotnet restore
dotnet run
```

The backend is configured for a local SQL Server Express instance:

```text
Server=.\SQLEXPRESS;Database=LuxeBeautyPlace;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True
```

To create and seed the database from Command Prompt, run:

```bat
"C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE" -S .\SQLEXPRESS -E -i database\schema.sql
```

Then start the API and test the database connection:

```bash
cd backend
dotnet run
```

Open:

```text
http://127.0.0.1:5117/api/health
http://127.0.0.1:5117/api/services
```

When the backend starts, it checks the `Services` table, adds the service category/price label columns if they are missing, and syncs the current salon price list into SQL Server.

## Admin Access

The admin page is hidden behind the small key icon in the footer.

If no admin exists yet, open the hidden admin page from the key icon in the footer and create the first admin account with this setup code:

```text
LUXE-ADMIN-SETUP-2026
```

After the first admin exists, only a logged-in admin can create additional admin accounts.

Admins can change their password from the dashboard. The forgot-password flow creates a temporary reset code for local development; in production, wire that code to email or SMS before launch.

## Contact Placeholders

- Logo: `[LOGO HERE]`
- Slogan: `[SLOGAN HERE]`
- Address: `[BUSINESS ADDRESS]`
- Phone: `[PHONE NUMBER]`
- WhatsApp: `[WHATSAPP NUMBER]`
- Email: `[EMAIL ADDRESS]`
- Opening hours: `[OPENING HOURS]`
