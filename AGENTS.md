# Employee Leave Management — Agent Coding Contract

## Goal
Build the assignment in `docs/ASSIGNMENT.md` as a small, production-minded ASP.NET Core MVC layered monolith.

## Living implementation context
`docs/IMPLEMENTATION_CONTEXT.md` is mandatory operational state, not optional documentation.

Use the project documents for distinct purposes:
- `AGENTS.md`: how an agent must behave
- `docs/ASSIGNMENT.md`: what must be built
- `docs/ARCHITECTURE.md`: why the system is structured this way
- `docs/DOMAIN_RULES.md`: business rules that must remain true
- `docs/DATABASE_GUIDELINES.md`: mandatory standards for entities, DbContext, persistence, schema, migrations, and database access
- `docs/IMPLEMENTATION_CONTEXT.md`: what is implemented and verified right now

Before changing implementation code, read the implementation context in addition to the relevant requirements and design documents. Treat it as the handoff record for prior phases; do not assume earlier work is absent or complete without checking it.

For any prompt or change involving an entity, `DbContext`, EF Core configuration, database access, schema, migration, transaction, seed data, or persistence query, read and follow `docs/DATABASE_GUIDELINES.md` before modifying code. Apply its database review checklist before considering that work complete.

After every implementation change, update `docs/IMPLEMENTATION_CONTEXT.md` in the same change set. Record the completed capability, affected projects/files, decisions or assumptions made, verification performed, schema/migration state, and known gaps or risks. Keep it factual and current; remove or revise superseded entries rather than preserving stale status.

## Architecture
Projects:
- `LeaveManagement.Web`
- `LeaveManagement.Application`
- `LeaveManagement.Domain`
- `LeaveManagement.Infrastructure`

Dependency direction:
Web -> Application
Web -> Infrastructure only for composition/startup
Infrastructure -> Application
Infrastructure -> Domain
Application -> Domain
Domain -> nothing

Do not introduce microservices, CQRS/MediatR, event sourcing, generic repositories, or unnecessary abstractions.

## Core domain
Leave lifecycle:
Pending -> Approved
Pending -> Rejected

Rejected and Approved are terminal states.

Leave overlap rule:
For the same employee, a new leave overlaps an existing blocking leave when:
`existing.FromDate <= new.ToDate && existing.ToDate >= new.FromDate`

Default blocking statuses:
- Pending
- Approved

Rejected leaves do not block future leave.

Date rule:
FromDate <= ToDate.

Only active employees may submit leave.

## Authentication / authorization
Use ASP.NET Core cookie authentication and role-based authorization:
- Admin
- Employee

Never trust an employee ID supplied by a browser for employee-owned operations. Derive the current employee from the authenticated identity.

## Layer responsibilities
### Domain
Entities, enums, domain invariants/state transitions. No EF Core, MVC, HTTP, logging, or UI dependencies.

### Application
Use cases/services, DTOs, validation orchestration, interfaces. No MVC-specific code.

### Infrastructure
EF Core, SQL Server, persistence implementations, migrations.

### Web
Controllers, Razor views, view models, authentication setup, authorization, Bootstrap/simple CSS.

Controllers should remain thin.

## Validation
Use DataAnnotations for UI/input validation and server-side validation.
Business rules belong in Application/Domain and must not depend on ModelState.

## Persistence
Use EF Core + SQL Server.
Prefer direct DbContext usage from application services unless a domain-specific repository genuinely improves the design.
Do not create a generic repository.

## Concurrency
Treat overlap check + leave insertion as one consistency boundary.
Do not claim that a simple pre-check alone prevents concurrent overlapping submissions.
Document the chosen SQL Server transaction/isolation strategy and its tradeoffs.

## Error handling
Use centralized exception handling where practical.
Show user-friendly messages.
Do not expose stack traces, SQL, secrets, or internal exception details to users.

## Auditability
Track:
- ReviewedBy
- ReviewedAt
- status transition information as appropriate

Approval/rejection must be attributable to an Admin.

## Dashboards
Admin dashboard:
- total employees
- pending requests
- approved/rejected summary counts

Employee dashboard:
- own leave history
- approved/rejected/pending summary

Keep dashboard aggregation out of controllers.

## Default users
The assignment requires one Admin and one Employee static database record for testing.
Do not hard-code plaintext passwords into application logic. Seed password hashes and document the test credentials only where appropriate.

## Coding style
Prefer:
- async/await for I/O
- cancellation tokens on application/database operations where practical
- explicit names
- small methods
- nullable reference types
- dependency injection
- no magic strings for statuses/roles
- no unnecessary comments

## Build quality
`Directory.Build.props` treats warnings as errors for every project in this repository. Keep the build warning-free: address the underlying warning before considering work complete. Do not disable, suppress, or downgrade warnings merely to make a build pass unless the user explicitly authorizes it and the exception is documented in `docs/IMPLEMENTATION_CONTEXT.md`.

## Package management
`Directory.Packages.props` enables NuGet Central Package Management for the entire repository. When adding or updating a package, declare each project's dependency with `<PackageReference Include="Package.Id" />` and add or update its single `<PackageVersion Include="Package.Id" Version="x.y.z" />` entry in `Directory.Packages.props`. Never put a `Version` attribute on a project-level `PackageReference`, add ad-hoc version overrides, or introduce duplicate central package-version entries. Restore and build after every package change; resolve dependency/version conflicts at the central version definition rather than suppressing related warnings.

## Agent behavior
Before modifying code:
1. Read relevant docs, including `docs/IMPLEMENTATION_CONTEXT.md`; for entity or database-related work, also read `docs/DATABASE_GUIDELINES.md`. For package changes, read `Directory.Packages.props`.
2. Inspect existing project structure.
3. Identify impacted boundaries.
4. State assumptions if requirements are ambiguous.
5. Make the smallest coherent change.

After modifying code:
1. Build with the repository defaults and resolve every warning or error.
2. Run tests.
3. Review authorization boundaries.
4. Review validation and concurrency behavior.
5. Check migrations/schema impact.
6. Update `docs/IMPLEMENTATION_CONTEXT.md` with the current implementation state and verification results.
7. Report files changed and remaining risks.

Never silently invent business rules when the assignment is ambiguous.
