# Architecture Decision Record — Lightweight Layered Monolith

## Chosen shape

```text
LeaveManagement.Web
        |
        v
LeaveManagement.Application
        |
        v
LeaveManagement.Domain

LeaveManagement.Web ---> Infrastructure (composition/startup)
Infrastructure -------> Application
Infrastructure -------> Domain
```

### Projects

#### Domain
Owns:
- Employee
- LeaveRequest
- LeaveStatus
- domain invariants and transitions

Must not reference:
- ASP.NET Core
- EF Core
- MVC
- SQL Server
- HTTP

#### Application
Owns:
- Employee use cases
- Leave use cases
- Dashboard queries
- authentication-related application abstractions
- DTOs
- interfaces

#### Infrastructure
Owns:
- AppDbContext
- EF configurations
- migrations
- persistence implementations
- password hashing integration if not kept in Web/Application composition

#### Web
Owns:
- Controllers
- Razor views
- ViewModels
- authentication/authorization configuration
- exception presentation
- Bootstrap assets

## Why not full Clean Architecture?
The assignment is small. Four projects provide useful boundaries without excessive ceremony.

## Why not microservices?
There is one small transactional domain, no independently deployable bounded contexts, and no assignment requirement for distributed processing.

## Why not CQRS/MediatR?
The use cases are straightforward CRUD + transactional business rules. Extra indirection would increase implementation and review cost without solving a demonstrated problem.

## Why not generic repository?
EF Core already supplies Unit of Work/change tracking and DbSet abstractions. Generic repositories tend to obscure useful query semantics.

## Why SQL Server?
The assignment explicitly permits SQL Server and the concurrency requirements are a useful opportunity to demonstrate transactional reasoning.

## Architectural quality bar
Optimize for:
1. Correctness
2. Security/authorization
3. Business-rule integrity
4. Testability
5. Maintainability
6. Simplicity

Not for number of patterns used.
