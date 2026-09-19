# Implementation Context

## Purpose and maintenance

This is the authoritative, living record of the repository's implemented state. It complements, but does not replace, the assignment, architecture, and domain-rule documents.

Every implementation change must update this file in the same change set. Entries must state what exists now, not future intent. When work changes or replaces an earlier decision, revise the affected section so this file remains an accurate handoff for the next phase.

## Current baseline

**Last updated:** 2026-09-19

Phase 0 is complete. Phase 1 persistence, domain entities, migrations and seeds are implemented. Phase 2 Authentication & Authorization is implemented. Phase 3 Employee Management is implemented. Phase 4 Leave Application & Overlap Validation is implemented. Phase 5 Admin Approval Workflow is implemented. Phase 6 Dashboards is implemented. `ICurrentUserService`, `AuditSaveChangesInterceptor`, `ISqlConnectionFactory`, and `IDateTimeProvider` are in place. Domain entities for `Employee`, `LeaveRequest`, `User`, `Role`, `UserRole` with business invariants and audit base are implemented and verified with unit tests. Cookie authentication with role-based authorization, login/logout, and `IAuthenticationService` abstraction are implemented. Employee management use cases with EF Core writes, Dapper reads, Admin authorization, and atomic deactivation are implemented. Leave application uses `ICurrentUserService` for identity, enforces active employee, validates input, checks overlap with Serializable transaction, and creates Pending leave via domain factory using `IDateTimeProvider`.

## Implemented capabilities

- `LeaveManagement.slnx` contains the four planned projects under `src`:
  - `LeaveManagement.Domain`
  - `LeaveManagement.Application`
  - `LeaveManagement.Infrastructure`
  - `LeaveManagement.Web`
- `LeaveManagement.Domain.BaseEntity` defines `Id` (`Guid`), `CreatedAt` and `UpdatedAt` (`DateTime`), and `CreatedBy` and `UpdatedBy` (`Guid`) with encapsulated setters for future persistent entities.
- `LeaveManagement.Application.Services.ICurrentUserService` defines `UserId` and `IsAuthenticated` for current user context.
- `LeaveManagement.Web.Services.CurrentUserService` implements `ICurrentUserService` using `IHttpContextAccessor` and `ClaimTypes.NameIdentifier`.
- `LeaveManagement.Infrastructure.Persistence.Interceptors.AuditSaveChangesInterceptor` implements `SaveChangesInterceptor` to automatically set `CreatedAt/CreatedBy/UpdatedAt/UpdatedBy` on `BaseEntity` entries using `ICurrentUserService` and `IDateTimeProvider`.
- `LeaveManagement.Infrastructure.DependencyInjection.InfrastructureServiceCollectionExtensions.AddInfrastructure` registers `AuditSaveChangesInterceptor` and adds it to `AppDbContext` options.
- `LeaveManagement.Web.Extensions.ServiceExtensions.AddWebCoreServices` registers `IHttpContextAccessor` and `ICurrentUserService`.
- `LeaveManagement.Application.Interfaces.ISqlConnectionFactory` abstracts creation of SQL connections for Dapper queries, returning `DbConnection` and providing `CreateOpenConnectionAsync` for async I/O.
- `LeaveManagement.Infrastructure.Persistence.SqlConnectionFactory` implements `ISqlConnectionFactory` using `Microsoft.Data.SqlClient.SqlConnection` and the `DefaultConnection` connection string.
- `AddInfrastructure` registers `ISqlConnectionFactory` as singleton.
- `LeaveManagement.Application.Interfaces.IDateTimeProvider` abstracts current UTC time for testability.
- `LeaveManagement.Infrastructure.Persistence.DateTimeProvider` implements `IDateTimeProvider` using `DateTimeOffset.UtcNow`.
- `AddInfrastructure` registers `IDateTimeProvider` as singleton.
- `LeaveManagement.slnx` also contains `tests/LeaveManagement.Tests`, a .NET 10 xUnit project. It is under the solution's `tests` folder, so Visual Studio loads the source and test projects together. The project contains a passing test-infrastructure smoke test; behavior tests will be added in the planned test phase.
- All projects target `net10.0`, enable nullable reference types and implicit usings, and the repository pins SDK `10.0.401` through `global.json` (rolling forward only to a later patch).
- `LeaveManagement.Domain.Entities.BaseEntity` provides `Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `Version` for optimistic concurrency.
- `LeaveManagement.Domain.Entities.Employee`, `LeaveRequest`, `User`, `Role`, `UserRole` implement domain invariants and state transitions.
- `LeaveRequest.Create` enforces `FromDate <= ToDate` and non-empty reason; `Approve`/`Reject` enforce transition from `Pending`; `OverlapsWith` ignores `Rejected` leaves.
- `Employee.CanSubmitLeave` enforces `IsActive` on both `Employee` and linked `User`.
- `LeaveManagement.Infrastructure.Persistence.AppDbContext` exposes `DbSet<Employee>`, `LeaveRequests`, `Users`, `Roles`, `UserRoles` and applies EF configurations.
- EF Core entity configurations establish foreign keys, unique indexes, check constraint `FromDate <= ToDate`, and concurrency token `Version` on all entities.
- Migration `20260919103022_InitialCreate` creates schema for `Roles`, `Users`, `Employees`, `UserRoles`, `LeaveRequests` with indexes and seeds Admin and Employee roles, users `admin@example.com`/`employee@example.com` with BCrypt hashes, and corresponding employees.
- Database `LeaveManagementDb` on `192.168.0.104` created and migrated; `__EFMigrationsHistory` contains `InitialCreate`.
- Unit tests in `tests/LeaveManagement.Tests.Domain` cover `LeaveRequest` creation validation, state transitions, overlap logic and `Employee.CanSubmitLeave`.
- `Directory.Build.props` applies `TreatWarningsAsErrors=true` repository-wide. Changes must resolve underlying warnings; warning suppression or downgrades require explicit user authorization and documentation here.
- `Directory.Packages.props` enables Central Package Management. The test project's xUnit, Visual Studio runner, test SDK, and coverage-collector versions are declared there; all future package versions must also be declared there rather than in project files.
- Project references enforce the documented direction: Application -> Domain; Infrastructure -> Application and Domain; Web -> Application and Infrastructure.
- The Web project provides MVC routing, static-asset mapping, its template configuration files, and an unauthenticated `GET /health` endpoint for the baseline health check.
- Web UI layout shell and design system based on `reference.html` implemented:
  - `Views/Shared/_Layout.cshtml`: Application shell (`.app-shell`) with collapsible sidebar layout, CDN links for Bootstrap 5.3.2, Bootstrap Icons 1.11.1, and Google Font Inter, embedding `_Sidebar` and `_Topbar` partials.
  - `Views/Shared/_Sidebar.cshtml`: Navigation sidebar with Overview, Leave, People, Insights, and System sections, tooltip bindings, active route highlighting using MVC route data (`controller`/`action`), and user footer displaying current identity/role.
  - `Views/Shared/_Topbar.cshtml`: Header bar with sidebar collapse toggle (`#sidebarToggle`), breadcrumbs, dynamic page title/subtitle via `ViewData`, search box, notification icon, export action, dynamic new request button, and conditional logout button for authenticated users.
  - `wwwroot/css/site.css`: Complete design system tokens (`--colors-ink`, `--colors-canvas`, `--colors-accent`, etc.), Bootstrap card/table/button/pill overrides, custom stat cards, sparklines, charts, mini calendar, holiday lists, activity feeds, and form control styles.
  - `wwwroot/js/site.js`: Bootstrap tooltip initialization, sidebar collapse toggle logic with tooltip enable/disable, segmented pill control toggling, and table row approve/reject action handlers.
  - `Views/Home/AdminDashboard.cshtml` / `Views/Home/EmployeeDashboard.cshtml`: Role-specific dashboards in the design-system stat-card language with real data (the previous static `Index.cshtml` showcase with hardcoded numbers was removed). Admin shows total/active employees and pending/approved/rejected/total request counts; Employee shows own pending/approved/rejected/total counts plus the 5 most recent requests. `HomeController.Index` selects the view by role; all aggregation lives in Dapper `IDashboardQueries`, never in the controller.
  - `Views/Home/Privacy.cshtml` & `Views/Shared/Error.cshtml`: Styled to conform to the new design token card standards.
  - Removed template default `Views/Shared/_Layout.cshtml.css` to prevent obsolete scoped style collision.
- Dependency injection extension points are established:
  - `LeaveManagement.Application.DependencyInjection.ApplicationServiceCollectionExtensions.AddApplication(IServiceCollection)` for Application registration.
  - `LeaveManagement.Infrastructure.DependencyInjection.InfrastructureServiceCollectionExtensions.AddInfrastructure(IServiceCollection, IConfiguration)` for Infrastructure registration.
  - `LeaveManagement.Web.Extensions.ServiceExtensions` provides `AddWebCoreServices`, `AddApplicationServices`, `UseAppPipeline`, and `ConfigureCors`.
  - `LeaveManagement.Web.Program` is thin and delegates service registration and pipeline configuration to `ServiceExtensions`.
- Central Package Management includes `Microsoft.Extensions.DependencyInjection`, `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Dapper`, and `BCrypt.Net-Next` with versions pinned in `Directory.Packages.props`. Infrastructure project references EF Core, Dapper and BCrypt.Net-Next; Application project references Microsoft.Extensions.DependencyInjection.

## Established decisions and constraints

- The intended implementation is a layered ASP.NET Core MVC monolith with Web, Application, Domain, and Infrastructure projects.
- SQL Server and EF Core are the intended persistence stack.
- Cookie authentication and the `Admin` and `Employee` roles are required.
- The business rules in `DOMAIN_RULES.md`, including terminal leave statuses, date validity, ownership, and blocking-overlap behavior, remain binding.
- `DATABASE_GUIDELINES.md` is mandatory for all entity, DbContext, persistence, schema, migration, transaction, seed-data, and database-query work.
- Overlap check + leave insertion is one consistency boundary via a `Serializable` EF Core transaction (`LeaveCommands.ApplyLeaveAsync`); tradeoff is reduced concurrency under contention (range locks can deadlock, surfacing as a retriable database error) in exchange for no phantom overlapping inserts. Approval/rejection relies on single-row `SaveChanges` atomicity plus the `Version` optimistic-concurrency token: two concurrent reviews of the same Pending request resolve to one winner, the loser receives a friendly already-reviewed message. See DEC-006/DEC-007.

## Schema and migration state

EF Core packages are added to Infrastructure project via Central Package Management. `AppDbContext` and entity configurations are implemented. Migrations `20260919103022_InitialCreate` and `20260919125737_FixVersionConcurrencyTokenValueGeneration` exist and are applied to SQL Server `192.168.0.104\LeaveManagementDb`. The latter is a metadata-only fix (plus no-op `Version = 1` seed touch-ups): `Version` on all entities changed from `ValueGeneratedOnAddOrUpdate` to a client-owned concurrency token after Phase 8 tests proved the store never persisted the domain's `Version++`, silently disabling optimistic concurrency; no DDL change. Schema includes `Roles`, `Users`, `Employees`, `UserRoles`, `LeaveRequests` with check constraint `CK_LeaveRequest_Dates`, unique indexes and concurrency token `Version`. Seed data for Admin and Employee roles, users and employees is present. Database Review Checklist from `DATABASE_GUIDELINES.md` applied for entity design, configuration, migration and seeding.

## Verification state

`dotnet restore LeaveManagement.slnx`, `dotnet build LeaveManagement.slnx`, and `dotnet test LeaveManagement.slnx --no-build` succeeded on 2026-09-19 after adding `BaseEntity`. The build completed with zero warnings and zero errors under the repository-wide warnings-as-errors setting; the test suite reports one passing test.

`dotnet build LeaveManagement.slnx` succeeded on 2026-09-19 after adding `ICurrentUserService` and `AuditSaveChangesInterceptor` with zero warnings and zero errors.

`dotnet build LeaveManagement.slnx` succeeded on 2026-09-19 after adding `ISqlConnectionFactory` and `SqlConnectionFactory` with zero warnings and zero errors.

`dotnet build LeaveManagement.slnx` succeeded on 2026-09-19 after adding `IDateTimeProvider` and `DateTimeProvider` with zero warnings and zero errors.

`dotnet build LeaveManagement.slnx` succeeded on 2026-09-19 after updating `ISqlConnectionFactory` to return `DbConnection` and add `CreateOpenConnectionAsync` with zero warnings and zero errors.

`dotnet build LeaveManagement.slnx` succeeded on 2026-09-19 after injecting `IDateTimeProvider` into `AuditSaveChangesInterceptor` with zero warnings and zero errors.

`dotnet ef database update` applied migration `20260919103022_InitialCreate` to `192.168.0.104\LeaveManagementDb` on 2026-09-19. Database tables `Roles`, `Users`, `Employees`, `UserRoles`, `LeaveRequests` exist with seed rows for admin and employee users verified.

Unit tests for domain rules added and pass. `dotnet test LeaveManagement.slnx` succeeds with zero warnings/errors.

`dotnet build LeaveManagement.slnx` and `dotnet test LeaveManagement.slnx --no-build` succeeded on 2026-09-19 after implementing the UI layout shell, `_Sidebar`, `_Topbar`, `site.css`, `site.js`, and `Index.cshtml` views with 0 warnings and 0 errors under the repository-wide warnings-as-errors setting; all 13 unit tests passed.

`dotnet build LeaveManagement.slnx` and `dotnet test LeaveManagement.slnx --no-build` succeeded on 2026-09-19 after Phase 2 Authentication & Authorization implementation with 0 warnings and 0 errors; cookie authentication, `AccountController`, `IAuthenticationService`, `IUserAuthenticationStore`, `BCryptPasswordHasher` registered; tests pass.

`dotnet build LeaveManagement.slnx` and `dotnet test LeaveManagement.slnx --no-build` succeeded on 2026-09-19 after refactoring `Employees/Index.cshtml`, `Create.cshtml`, and `Edit.cshtml` to align with the design tokens in `site.css` and the table design of `Home/Index.cshtml`, and pruning `_Sidebar.cshtml` to retain only Dashboard and Employees navigation. 0 warnings and 0 errors; all 13 tests passed.

`dotnet build LeaveManagement.slnx` and `dotnet test LeaveManagement.slnx --no-build` succeeded on 2026-09-19 after Phase 4 Leave Application & Overlap Validation implementation with 0 warnings and 0 errors; `LeaveRequest.Create` updated to use `IDateTimeProvider`, `ILeaveCommands` implements Serializable transaction overlap check + insert, `LeavesController` with antiforgery and Employee role authorization added; all 13 unit tests passed.

`dotnet build LeaveManagement.slnx` and `dotnet test LeaveManagement.slnx` succeeded on 2026-09-19 after adding future-date guardrail, employee leave history (`ILeaveQueries`/Dapper + `Leaves/Index` with filter/pagination), employee-only `My Leaves` sidebar entry without Apply link, and client-side past-date validation in `Views/Leaves/Apply.cshtml`; 0 warnings and 0 errors; all 13 tests passed.

`dotnet test LeaveManagement.slnx` (Debug) and `dotnet build LeaveManagement.slnx -c Release` succeeded on 2026-09-19 after fixing the leave-history status mapping (stored `LeaveStatus` int projected to display names, UI strings normalized to stored ints, empty filters treated as no-filter) and adding Previous/Next pagination preserving filters; 0 warnings and 0 errors; all 13 tests passed.

`dotnet test LeaveManagement.slnx` (Debug) and `dotnet build LeaveManagement.slnx -c Release` succeeded on 2026-09-19 after verifying/fixing the employee list (`IEmployeeQueries` gained an Active/Inactive status filter with blank-as-all normalization, blank search treated as no-search, page clamping, Previous/Next pagination preserving filters in `Views/Employees/Index.cshtml`) and gating the Employees sidebar section to the `Admin` role; 0 warnings and 0 errors; all 13 tests passed.

`dotnet test LeaveManagement.slnx` (Debug) and `dotnet build LeaveManagement.slnx -c Release` succeeded on 2026-09-19 after restyling `Views/Leaves/Apply.cshtml` to the `Employees/Create` design language and pre-filling From/To dates with the current date in `LeavesController.Apply` (GET); 0 warnings and 0 errors; all 13 tests passed.

`dotnet test LeaveManagement.slnx` (Debug) and `dotnet build LeaveManagement.slnx -c Release` succeeded on 2026-09-19 after Phase 5 Admin Approval Workflow (`LeaveReviewService`, `ILeaveCommands` approve/reject with `Version` optimistic-concurrency handling, Dapper `ListAllLeavesAsync`, `LeaveReviewsController` + `Views/LeaveReviews/Index.cshtml`, admin-only sidebar entry); 0 warnings and 0 errors; all 13 tests passed.

`dotnet test LeaveManagement.slnx` (Debug) and `dotnet build LeaveManagement.slnx -c Release` succeeded on 2026-09-19 after replacing the `Items.Count >= PageSize` Next-button heuristic with exact `COUNT(*)` queries (`CountOwnLeavesAsync`, `CountAllLeavesAsync`, `CountEmployeesAsync` sharing each list query's filter normalization) and `Page X of Y (N total)` footers on all three lists; 0 warnings and 0 errors; all 13 tests passed.

`dotnet test LeaveManagement.slnx` (Debug) and `dotnet build LeaveManagement.slnx -c Release` succeeded on 2026-09-19 after Phase 6 Dashboards (`DashboardApplicationService`, Dapper `IDashboardQueries` single-query summaries, role-specific `AdminDashboard`/`EmployeeDashboard` views replacing the static showcase, deleted `Views/Home/Index.cshtml`); 0 warnings and 0 errors; all 13 tests passed.

`dotnet test LeaveManagement.slnx` (Debug) and `dotnet build LeaveManagement.slnx -c Release` succeeded on 2026-09-19 after Phase 7 hardening: centralized error logging in `HomeController.Error` (view still renders RequestId only), `UseStatusCodePagesWithReExecute("/Home/Error")` for friendly 4xx/5xx pages, structured `ILogger` outcomes in Account/Employees/Leaves/LeaveReviews controllers (no credentials or exception internals logged), dead-redirect + magic-string cleanup in login, and documented concurrency strategies (DEC-006/DEC-007); authorization/antiforgery review passed with no gaps (all POSTs carry antiforgery tokens, role gates verified per controller); 0 warnings and 0 errors; all 13 tests passed.

`dotnet test LeaveManagement.slnx` (Debug) and `dotnet build LeaveManagement.slnx -c Release` succeeded on 2026-09-19 after Phase 8 tests: 22 new tests (21 SQLite-backed `LeaveCommands`/service tests covering apply validation, overlap/boundary/adjacent rules, inactive-user blocking, ownership scoping, review transitions + audit, unauthenticated rejection, and the stale-version second-reviewer path; plus a same-day domain overlap test), `Microsoft.EntityFrameworkCore.Sqlite` added via Central Package Management; suite now 35/35. Testing exposed that `ValueGeneratedOnAddOrUpdate` silently dropped the domain's `Version++`, disabling optimistic concurrency — fixed in all four entity configurations with migration `FixVersionConcurrencyTokenValueGeneration` (applied to the test database; seed updates were no-ops); 0 warnings and 0 errors.

Submission readiness verified on 2026-09-19 and committed as `a0aa11b`: README documents setup/migrations/credentials; `appsettings.json` holds only a placeholder connection string with the real value in local user secrets (`appsettings.example.json` is the committed template); `dotnet restore` + Release build (0 warnings) + `dotnet test -c Release` (35/35) all green from the committed tree; `dotnet ef migrations script` generates tables plus seed inserts; the app booted with the secret-based connection and live HTTP checks passed (admin login → dashboard + LeaveReviews table, employee login → dashboard + My Leaves, employee blocked from LeaveReviews).

## Known gaps and next implementation work

- Phase 2 Authentication & Authorization implemented. Login/logout, cookie authentication, role claims, `IAuthenticationService`, `IUserAuthenticationStore`, `IPasswordHasher` in place.
- Phase 3 Employee Management implemented: Admin create/edit/deactivate employee, list/search/Active-Inactive-filter with Dapper projection and leave count plus Previous/Next pagination preserving filters (blank search treated as no-search, blank/unknown status treated as all), atomic User+Employee+Role creation with `IPasswordHasher`, deactivation sets both `Employee.IsActive` and `User.IsActive` false, authorization requires Admin role, server-side validation and antiforgery applied. UI aligned with design system tokens. `_Sidebar.cshtml` shows the Employees section only for the `Admin` role (controller was already `[Authorize(Roles = Admin)]`).
- Phase 4 Leave Application & Overlap Validation implemented: Employee can submit leave via `LeavesController.Apply`, `LeaveApplicationService` + `ILeaveCommands` implementation uses `ICurrentUserService` for employee identity, enforces `Employee.CanSubmitLeave()`, validates dates/reason via DataAnnotations and domain invariants, rejects `FromDate` before today (`IDateTimeProvider`-derived date) in addition to `FromDate <= ToDate`, overlap check for Pending/Approved leaves performed server-side with efficient EF Core `AnyAsync` query, check and insert executed in a Serializable transaction to provide consistency boundary. `LeaveRequest.Create` accepts `DateTimeOffset now` supplied by `IDateTimeProvider`. UI view `Views/Leaves/Apply.cshtml` follows the `Employees/Create` design language (centered narrow card, `card-head`/`card-title`/`card-sub`, `rounded-pill` inputs, `btn-ink` submit + `Cancel` back to the list, pill-style success alert), has antiforgery, `min=today` date inputs pre-filled with the current date by `LeavesController.Apply` (GET), a live day-count pill, and client-side JS that pins both pickers to today and keeps `ToDate >= FromDate`. Employee leave history is available at `LeavesController.Index` / `Views/Leaves/Index.cshtml` (styled like `Employees/Index.cshtml` with `Apply Leave` button to the creation page), backed by Dapper `ILeaveQueries.ListOwnLeavesAsync` scoped to the current user with status/reason filter and page/pageSize pagination. The query normalizes UI status strings to the stored `LeaveStatus` int values, treats empty filters as no-filter, and projects the stored int back to `Pending`/`Approved`/`Rejected` display names; the view renders status pills from those names and provides Previous/Next pagination with exact total counts (`CountOwnLeavesAsync`) preserving the active filters. `_Sidebar.cshtml` exposes only `Leaves/Index` (`My Leaves`) and only for `Employee` role; no Apply entry is in the sidebar. Domain `LeaveRequest` invariants unchanged.
- Phase 5 Admin Approval Workflow implemented: `LeaveReviewsController` (`[Authorize(Roles = Admin)]`) lists all requests via Dapper `ILeaveQueries.ListAllLeavesAsync` (employee name/email + status/reason filter, page pagination) and approves/rejects via `LeaveReviewService` + `ILeaveCommands` review methods, which enforce the domain Pending-only transition, stamp `ReviewedBy` (admin UserId) / `ReviewedAt`, and translate `DbUpdateConcurrencyException` on the `Version` token into a friendly already-reviewed message. Single-row `SaveChanges` is the atomicity boundary; no schema change, no migration. Admin sidebar gained an employee-hidden `Leave Requests` entry.
- Phase 6 Dashboards implemented: `DashboardApplicationService` + Dapper `IDashboardQueries` serve single-query conditional-aggregation summaries (admin: total/active employees, pending/approved/rejected/total requests; employee: own status counts plus 5 most recent requests), rendered by role-specific `AdminDashboard`/`EmployeeDashboard` views; no aggregation in controllers. No schema change, no migration.
- Concurrency strategy documented as Serializable transaction for overlap check + insert; tradeoffs recorded.
- Integration tests for authentication, authorization, ownership are still absent; unit tests for domain remain passing.
- Phase 1, Phase 2, Phase 3, Phase 4, Phase 5, Phase 6, Phase 7, Phase 8 complete.
- Known accepted risk: `appsettings.json` commits a local SQL Server SA password for the shared test database so the machine test runs from a clean checkout; production credentials must differ and come from secure configuration per `DATABASE_GUIDELINES.md` §30.

