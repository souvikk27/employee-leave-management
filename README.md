# Employee Leave Management System

ASP.NET Core MVC web application for managing employee leave requests with
role-based access (Admin and Employee). Layered monolith: Web → Application →
Domain, with Infrastructure for EF Core / SQL Server persistence.

## Prerequisites

- .NET 10 SDK (`10.0.401`, pinned in `global.json`)
- SQL Server (2019+; LocalDB or full instance both work)
- `dotnet-ef` CLI for applying migrations: `dotnet tool install -g dotnet-ef`

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

# 2. Create / migrate the database (runs all EF Core migrations, incl. seed data)
dotnet ef database update --project src\LeaveManagement.Infrastructure\LeaveManagement.Infrastructure.csproj --startup-project src\LeaveManagement.Web\LeaveManagement.Web.csproj

# 3. Run the test suite
dotnet test LeaveManagement.slnx

# 4. Run the app (http://localhost:5026 by default, see launchSettings.json)
dotnet run --project src\LeaveManagement.Web\LeaveManagement.Web.csproj
```

To inspect the schema without a database:

```powershell
dotnet ef migrations script --project src\LeaveManagement.Infrastructure\LeaveManagement.Infrastructure.csproj --startup-project src\LeaveManagement.Web\LeaveManagement.Web.csproj
```

## Default test credentials

Seeded by the initial migration (passwords stored as BCrypt hashes only):

| Role     | Email                | Password |
| -------- | -------------------- | -------- |
| Admin    | admin@example.com    | admin123 |
| Employee | employee@example.com | emp123   |

These are the assignment-provided test accounts.

## What each role can do

**Admin**

- Dashboard: total/active employees, pending/approved/rejected request counts
- Employees: list, search, Active/Inactive filter, pagination, create, edit, deactivate
- Leave Requests: view all requests (status filter, employee/reason search,
  pagination), approve or reject pending requests (audited with reviewer + timestamp)

**Employee**

- Dashboard: own pending/approved/rejected counts plus 5 most recent requests
- My Leaves: own leave history with status filter, reason search, pagination
- Apply for Leave: From/To dates (today or later) + reason; overlapping
  Pending/Approved leaves are rejected; new requests start as Pending

## Project structure

```text
src/
  LeaveManagement.Domain/         Entities, enums, invariants (no dependencies)
  LeaveManagement.Application/    Use cases/services, DTOs, interfaces
  LeaveManagement.Infrastructure/ EF Core, SQL Server, Dapper reads, migrations
  LeaveManagement.Web/            Controllers, Razor views, auth, Bootstrap UI
tests/
  LeaveManagement.Tests/          xUnit suite (domain + SQLite-backed command tests)
docs/
  ASSIGNMENT.md                   Source requirements (authority on scope)
  ARCHITECTURE.md                 Layered-monolith decisions
  DOMAIN_RULES.md                 Leave lifecycle / overlap / ownership rules
  DATABASE_GUIDELINES.md          Mandatory persistence standards
  IMPLEMENTATION_CONTEXT.md       Living record of what is implemented now
  DECISION_LOG.md                 Key decisions incl. concurrency strategy
```

Key design points: overlap check + leave insert run in one `Serializable`
transaction; approval/rejection uses the `Version` optimistic-concurrency token
so two concurrent admin reviews resolve to a single winner; read lists use
Dapper projections with exact `COUNT(*)` pagination.

## Security notes

- Cookie authentication with `Admin` / `Employee` role claims; employee identity
  is always derived from the login, never from request data.
- All state-changing forms carry anti-forgery tokens.
- Never commit real connection strings or passwords — see Configuration above.
  (A development SA password appeared in this repo's early history; rotate it and
  treat it as compromised.)
