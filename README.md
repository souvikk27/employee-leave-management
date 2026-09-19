# Employee Leave Management System

ASP.NET Core MVC web application for managing employee leave requests with
role-based access (Admin and Employee). Layered monolith: Web → Application →
Domain, with Infrastructure for EF Core / SQL Server persistence.

## Prerequisites

- .NET 10 SDK (`10.0.401`, pinned in `global.json`)
- SQL Server (2019+; LocalDB or full instance both work)
- `dotnet-ef` CLI — optional, only needed for the manual migration/schema
  commands below (`dotnet tool install -g dotnet-ef`); the app migrates
  itself on startup

## Configuration (no secrets in this repo)

The app reads `ConnectionStrings:DefaultConnection` from the standard
ASP.NET Core configuration chain. Pick one option:

1. **Copy the template** (quick local setup):
   ```powershell
   Copy-Item src\LeaveManagement.Web\appsettings.example.json src\LeaveManagement.Web\appsettings.json
   ```
   then edit `DefaultConnection` inside `appsettings.json` (this file is yours —
   never commit real credentials).
2. **User secrets** (recommended for development):
   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=LeaveManagementDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true" --project src\LeaveManagement.Web\LeaveManagement.Web.csproj
   ```
3. **Environment variable** (CI / production):
   ```powershell
   $env:ConnectionStrings__DefaultConnection = "Server=...;Database=LeaveManagementDb;..."
   ```

Example connection string (SQL authentication):

```text
Server=localhost;Database=LeaveManagementDb;User Id=leavemgmt_app;Password=<strong-password>;TrustServerCertificate=True;MultipleActiveResultSets=true
```

## Setup from a clean checkout

```powershell
# 1. Restore and build (0 warnings expected — warnings are errors repo-wide)
dotnet restore LeaveManagement.slnx
dotnet build LeaveManagement.slnx -c Release

# 2. Run the test suite
dotnet test LeaveManagement.slnx

# 3. Run the app (http://localhost:5026 by default, see launchSettings.json)
dotnet run --project src\LeaveManagement.Web\LeaveManagement.Web.csproj
```

**No manual migration step is needed.** On startup, the app calls
`Database.Migrate()` against the configured connection string, which creates
the database if it doesn't exist, applies every pending EF Core migration,
and seeds the default Admin/Employee accounts (see below) — all automatically
on first launch. Point the connection string (see Configuration above) at an
empty database and just `dotnet run`.

To apply migrations manually instead (e.g. to inspect the schema, or to
migrate a database before the app itself ever runs):

```powershell
dotnet ef database update --project src\LeaveManagement.Infrastructure\LeaveManagement.Infrastructure.csproj --startup-project src\LeaveManagement.Web\LeaveManagement.Web.csproj
```

To inspect the schema without a database:

```powershell
dotnet ef migrations script --project src\LeaveManagement.Infrastructure\LeaveManagement.Infrastructure.csproj --startup-project src\LeaveManagement.Web\LeaveManagement.Web.csproj
```

## Default test credentials

Seeded automatically by the initial migration on first launch (passwords
stored as BCrypt hashes only):

| Role     | Email                | Password |
| -------- | -------------------- | -------- |
| Admin    | admin@example.com    | admin123 |
| Employee | employee@example.com | emp123   |

These are the assignment-provided test accounts.

## What each role can do

**Admin**

- Dashboard: total/active employees, pending/approved/rejected request counts
- Employees: list, search, Active/Inactive filter, pagination, create, edit,
  deactivate, and reactivate (a reactivated employee's account and login are
  both restored)
- Leave Requests: view all requests (status filter, date range filter,
  employee/reason search, pagination), approve or reject pending requests
- Export: download the current (filtered) leave request list as an Excel
  (`.xlsx`) file, including who reviewed each request and when

**Employee**

- Dashboard: own pending/approved/rejected counts plus 5 most recent requests
- My Leaves: own leave history with status filter, reason search, pagination
- Apply for Leave: From/To dates (today or later) + reason; overlapping
  Pending/Approved leaves are rejected; new requests start as Pending
- Real-time notification: a toast appears the moment an admin approves or
  rejects a request, naming who reviewed it and when — no page refresh needed
  to find out

## Optional (bonus) features implemented

All four bonus items from the assignment brief are implemented:

- **Real-time notifications (SignalR)** — `LeaveNotificationsHub` pushes an
  update to the requesting employee's browser the instant an admin
  approves/rejects; it renders as a toast naming the reviewer and timestamp,
  then refreshes the page data. Delivery failures are caught and logged
  rather than failing the underlying approval.
- **Reporting enhancements** — the admin Leave Requests view supports status
  and date-range filters (department filtering was intentionally not added:
  there is no department concept anywhere in the assignment's data model.
- **Excel export** — `Export` on the Leave Requests page generates a real
  `.xlsx` workbook (via ClosedXML) of the current filtered results, not a
  CSV-with-an-`.xlsx`-extension.
- **Audit logging** — every leave request records `ReviewedBy` and
  `ReviewedAt`, surfaced in the admin list and the Excel export; every entity
  also carries `CreatedBy/At` and `UpdatedBy/At` via a shared EF Core
  interceptor.

## Project structure

```text
src/
  LeaveManagement.Domain/         Entities, enums, invariants (no dependencies)
  LeaveManagement.Application/    Use cases/services, DTOs, interfaces
  LeaveManagement.Infrastructure/ EF Core, SQL Server, Dapper reads, migrations
  LeaveManagement.Web/            Controllers, Razor views, auth, Bootstrap UI
tests/
  LeaveManagement.Tests/          xUnit suite (domain + SQLite-backed command tests)
```

## Security notes

- Cookie authentication with `Admin` / `Employee` role claims; employee identity
  is always derived from the login, never from request data.
- All state-changing forms carry anti-forgery tokens.
- Deactivating a user revokes their live session immediately — the auth
  cookie is re-validated against `Users.IsActive` on every request, so a
  deactivated employee is signed out mid-session, not just blocked at the
  next login.
- The SignalR hub (`/hubs/leave-notifications`) requires authentication and
  only ever pushes to the group matching the authenticated caller's own user
  id — a client cannot subscribe to another user's notifications.
- Never commit real connection strings or passwords — see Configuration above.
  (A development SA password appeared in this repo's early history; rotate it and
  treat it as compromised.)
