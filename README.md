# Luxe Beauty Place

React/Vite website with an ASP.NET Core 8 API and SQL Server Express database.

## Run locally

Prerequisites: Node.js/npm, .NET 8 SDK, and a running SQL Server instance. Keep secrets in the ignored `backend/.env` file.

From the project directory, open two PowerShell terminals:

```powershell
cd backend
dotnet run
```

```powershell
cd frontend
npm.cmd install
npm.cmd run dev
```

Website: http://127.0.0.1:5173

Admin: http://127.0.0.1:5173/#/admin

API health: http://127.0.0.1:5117/api/health

See [DATABASE-ADMIN-GUIDE.md](DATABASE-ADMIN-GUIDE.md) for account creation, sign-in, recovery, database tables, and configuration.

## Verify

```powershell
cd frontend
npm.cmd test
npm.cmd run build
```

```powershell
cd backend
dotnet build
```

With the API running, run `powershell -File frontend/scripts/check-api.ps1` from the project directory for read-only API, login, authorization, validation, and database image-path checks. It reads the initial admin credentials from the ignored `.env`; it does not change records. If you changed that account's password, the configured initial password will no longer pass this check.

Images served by Vite belong in `frontend/public`, not `frontend/src/public` or `backend/Data/public`. Service image URLs use `/service-images/filename.jpg`. The duplicate source folders are retained but are not the served asset directory.

Normal API startup creates an empty database and seeds services only when the service table is empty. Existing dashboard edits and admin passwords are preserved. `database/schema.sql` is an alternative fresh-database script and refuses existing salon tables; it is not a migration tool.

## Limits Before Launch

Contact email and opening hours still need the owner's real information. Booking email/WhatsApp notifications, automatic availability/conflict scheduling, production password-reset email delivery, and dashboard editors for products/gallery are not implemented. Admins review booking requests manually. Development password recovery requires the server setup code and must not be exposed publicly.

Before public deployment, replace local credentials and signing keys, configure permitted frontend origins and HTTPS, add request rate limiting, and plan database backups and migrations. The current configuration is for local development, not a production security guarantee.
