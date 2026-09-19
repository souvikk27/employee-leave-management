# Database Guidelines

Version: 1.0

---

# 1. Purpose

This document defines the database design, modeling, persistence, and data-access standards for the **Employee Leave Management System**.

The objectives are to:

- Maintain data integrity.
- Prevent invalid leave states and data anomalies.
- Support reliable concurrent operations.
- Keep persistence concerns separate from business logic.
- Keep the database easy to evolve through EF Core migrations.
- Support auditing of important business operations.
- Keep queries efficient and maintainable.

These guidelines apply to **SQL Server, Entity Framework Core, and all future database changes**.

The database is the source of persisted truth, while business rules remain in the Domain/Application layers unless explicitly identified as database integrity constraints.

---

# 2. Database Technology

## Database Engine

SQL Server

## ORM

Entity Framework Core

## Migration Strategy

EF Core Migrations

Schema changes must be represented by migrations and committed to source control.

---

# 3. Design Principles

The database should:

- Maintain referential integrity.
- Prevent duplicate records where uniqueness is required.
- Preserve historical leave information.
- Use appropriate constraints for structural integrity.
- Support transactional consistency.
- Be easy to evolve through migrations.
- Avoid embedding application business workflows in database triggers.
- Support efficient querying.

Business workflows belong primarily in the Domain/Application layers.

The database should enforce **structural invariants** that can safely and consistently be represented as database constraints.

---

# 4. Normalization

The schema should generally follow **Third Normal Form (3NF)**.

Goals:

- Avoid unnecessary duplication.
- Prevent update anomalies.
- Keep relationships explicit.
- Maintain clear ownership of data.

Denormalization should only be introduced when there is a demonstrated performance requirement.

Do not denormalize merely for convenience.

---

# 5. Naming Conventions

## Tables

Use singular entity-oriented names.

Examples:

```text
Employees
LeaveRequests
```

Avoid inconsistent mixtures such as:

```text
employees
leave_requests
Employee
leaveRequests
```

The SQL Server schema naming should remain consistent with the EF Core model.

---

## Columns

Use PascalCase in the EF Core model.

Examples:

```text
Id
EmployeeId
FromDate
ToDate
CreatedAt
UpdatedAt
ReviewedBy
ReviewedAt
```

EF Core configuration is responsible for mapping the domain model to the database schema.

Do not introduce PostgreSQL-specific `snake_case` conventions into this project.

---

# 6. Primary Keys

Entities should use:

```text
Guid
```

with SQL Server:

```text
uniqueidentifier
```

Primary key property:

```csharp
public Guid Id { get; private set; }
```

All persistent entities should have a stable primary key.

Use generated IDs rather than exposing database-specific identity behavior to the Domain layer.

---

# 7. Foreign Keys

Foreign keys should follow:

```text
<EntityName>Id
```

Examples:

```text
EmployeeId
ReviewedBy
```

Where the relationship is to an Employee, use an explicit foreign key relationship.

Foreign keys must have appropriate referential-integrity behavior configured through EF Core.

---

# 8. Core Tables

The initial database should contain the entities required by the assignment.

Expected core entities:

```text
Employees
LeaveRequests
```

Authentication may use the application's own Employee credentials for this machine test.

Do not introduce a generalized Users/Roles/RefreshTokens model unless the implementation genuinely requires it.

The assignment only requires:

- Admin
- Employee

and simple cookie-based authentication.

---

# 9. Entity Relationships

Core relationship:

```text
Employee
   1
   |
   |
   *
LeaveRequest
```

A LeaveRequest belongs to exactly one Employee.

An Employee may have zero or many LeaveRequests.

Conceptually:

```text
Employee
---------
Id
Name
Email
PasswordHash
Role
IsActive
...

        1
        |
        |
        *
LeaveRequest
------------
Id
EmployeeId
FromDate
ToDate
Reason
Status
ReviewedBy
ReviewedAt
...
```

The actual model should be documented in `docs/IMPLEMENTATION_CONTEXT.md` once implemented.

---

# 10. Data Types

Use SQL Server-appropriate types through EF Core.

| Data       | C# Type          | SQL Server Type                          |
| ---------- | ---------------- | ---------------------------------------- |
| Identifier | `Guid`           | `uniqueidentifier`                       |
| Name       | `string`         | `nvarchar(...)`                          |
| Email      | `string`         | `nvarchar(...)`                          |
| Reason     | `string`         | `nvarchar(...)` / `nvarchar(max)`        |
| Date       | `DateOnly`       | `date`                                   |
| Timestamp  | `DateTimeOffset` | `datetimeoffset`                         |
| Boolean    | `bool`           | `bit`                                    |
| Status     | `enum`           | configured integer/string representation |

For monetary values, if monetary fields are ever introduced, use `decimal`, never floating-point types.

Do not use SQL Server-specific types directly in the Domain layer.

---

# 11. Required Audit Columns

Persistent business entities should generally include:

```text
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

where applicable.

For LeaveRequest, review-specific fields should include:

```text
ReviewedAt
ReviewedBy
```

These fields support the assignment's auditability requirement.

Not every technical/reference entity necessarily requires every audit column.

The actual requirement should be evaluated based on the entity's lifecycle.

---

# 12. Soft Delete

Soft deletion should be used for entities where historical references must remain valid.

For example:

```text
Employee
```

should not normally be physically deleted if the employee has historical LeaveRequests.

Instead, use:

```text
IsActive = false
```

The assignment explicitly requires employee deactivation.

This is preferable to deleting an employee and breaking historical leave records.

Do not automatically introduce `IsDeleted` into every table.

Use the lifecycle semantics appropriate to the entity.

---

# 13. Employee Rules

An Employee must have:

- unique email
- role
- active/inactive state

Only active employees may submit new leave requests.

Deactivating an employee must not remove historical LeaveRequests.

Historical LeaveRequests must remain queryable.

---

# 14. Leave Request Rules

A LeaveRequest must contain:

```text
EmployeeId
FromDate
ToDate
Reason
Status
```

Review information:

```text
ReviewedBy
ReviewedAt
```

where the request has been reviewed.

## Date constraint

The following must hold:

```text
FromDate <= ToDate
```

This should be validated in the Application/Domain layer.

A database check constraint may also be considered because this is a structural invariant.

---

# 15. Leave Status

Valid statuses:

```text
Pending
Approved
Rejected
```

Valid transitions:

```text
Pending -> Approved
Pending -> Rejected
```

Approved and Rejected are terminal states.

The state transition itself belongs to the Domain layer.

The database representation must prevent invalid persistence where practical, but the database should not become the workflow engine.

---

# 16. Leave Overlap

For the same Employee, a new leave overlaps an existing blocking leave when:

```text
existing.FromDate <= new.ToDate
AND
existing.ToDate >= new.FromDate
```

Blocking statuses:

```text
Pending
Approved
```

Rejected leave does not block future leave.

This is primarily an Application/Domain business rule.

However, the concurrency implications of this rule MUST be considered.

A simple:

```text
SELECT overlap
INSERT leave
```

is not sufficient to guarantee correctness under concurrent requests.

The implementation must treat:

```text
overlap check
+
leave insertion
```

as one consistency boundary.

The chosen SQL Server transaction/isolation strategy must be documented.

---

# 17. Constraints

The database should enforce structural integrity.

Examples:

### Primary Key

Every persistent entity must have a primary key.

### Foreign Key

Every LeaveRequest must reference an existing Employee.

### Unique Constraint

Employee email must be unique.

### Check Constraint

Where appropriate:

```text
FromDate <= ToDate
```

Database constraints should protect data integrity without moving the complete business workflow into SQL.

---

# 18. Indexing

Indexes should support actual query patterns.

Initial candidates include:

### Employees

```text
Email
IsActive
```

### LeaveRequests

```text
EmployeeId
Status
FromDate
ToDate
```

Composite indexes should only be added when they support real query patterns.

Do not blindly index every foreign key or column.

Indexing decisions should consider:

- query frequency
- selectivity
- ordering
- filtering
- write overhead

Execution plans should be reviewed when performance becomes relevant.

---

# 19. Transactions

Operations that modify multiple pieces of related state must execute within an appropriate transaction.

Examples:

- Leave approval with associated audit state
- Leave rejection with associated audit state
- Operations requiring multiple database writes
- Leave overlap check + insertion

A transaction should provide the required atomicity.

Do not introduce explicit transactions around every single database operation unnecessarily.

---

# 20. Concurrency

Concurrency must be considered explicitly.

## Leave creation

The overlap check and insertion must be protected against concurrent requests.

The implementation must document the selected SQL Server strategy, such as an appropriate transaction isolation level or locking strategy.

The choice should consider:

- correctness
- blocking
- throughput
- deadlock risk
- implementation complexity

## Leave approval/rejection

Approval/rejection operates on shared mutable state.

Only:

```text
Pending -> Approved
Pending -> Rejected
```

is valid.

The implementation should protect against two concurrent Admin requests attempting to transition the same Pending LeaveRequest.

Optimistic concurrency may be used where appropriate.

For example, a concurrency token can detect stale updates.

Do not introduce a concurrency mechanism without explaining what race it protects against.

---

# 21. Entity Type Configurations

Every entity must have its own:

```text
IEntityTypeConfiguration<TEntity>
```

configuration class.

Never place multiple entity configurations in one file.

Location:

```text
LeaveManagement.Infrastructure/
└── Persistence/
    └── Configurations/
        ├── EmployeeConfiguration.cs
        └── LeaveRequestConfiguration.cs
```

Example:

```csharp
public sealed class EmployeeConfiguration
    : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        // configuration
    }
}
```

`ApplicationDbContext.OnModelCreating` should load configurations automatically:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(ApplicationDbContext).Assembly);
```

New configuration classes should therefore not require manual registration.

---

# 22. EF Core Migrations

All schema changes must be managed through EF Core migrations.

Rules:

- Never modify an already-applied migration.
- Never manually modify the database schema as a replacement for a migration.
- Each migration should represent a coherent logical change.
- Migration names must describe the change.
- Migrations must be reviewed before application.
- Migration behavior should be tested against a clean database.

Examples:

```text
CreateInitialLeaveManagementSchema

AddLeaveReviewAuditFields

AddEmployeeIsActive

AddLeaveRequestConcurrencyToken
```

---

# 23. Seed Data

Seed only essential data required for the application.

The assignment requires:

```text
Admin
Employee
```

test users.

Seed data must be:

- deterministic
- idempotent
- safe to run repeatedly

Passwords must be stored as hashes.

Do not seed plaintext passwords into the database.

The assignment credentials should be documented in the README for testing purposes.

---

# 24. Auditing

Important leave operations should be attributable.

At minimum, LeaveRequest should be able to answer:

```text
Who reviewed this?
When was it reviewed?
What is its current status?
```

Therefore:

```text
ReviewedBy
ReviewedAt
```

should be persisted.

If a dedicated AuditLog entity is introduced, it should be justified by a concrete auditing requirement.

Do not introduce a generic audit subsystem merely because it is an enterprise pattern.

Audit history should be append-only where implemented.

---

# 25. Query Guidelines

Prefer LINQ queries translated by EF Core.

For read operations:

- Project only required columns.
- Avoid loading complete entities unnecessarily.
- Avoid unnecessary `Include`.
- Avoid N+1 queries.
- Prefer `AsNoTracking()` for read-only queries where appropriate.
- Use pagination for potentially large lists.
- Keep dashboard aggregation in application query/service code, not controllers.

Example:

```csharp
var employees = await dbContext.Employees
    .AsNoTracking()
    .Where(...)
    .Select(x => new EmployeeListItemDto
    {
        Id = x.Id,
        Name = x.Name,
        Email = x.Email
    })
    .ToListAsync(cancellationToken);
```

Do not retrieve entire entities when a projection is sufficient.

---

# 26. Database vs Application Responsibilities

The database should enforce:

- primary keys
- foreign keys
- uniqueness
- structural check constraints
- transactional atomicity

The Application/Domain layers should enforce:

- leave state transitions
- employee authorization
- ownership
- leave eligibility
- leave overlap semantics
- application workflows

Do not rely exclusively on either layer when both can provide useful protection.

For example:

```text
Application:
"Can this employee submit this leave?"

Database:
"Can this row reference a non-existent employee?"
```

---

# 27. Cascade Deletes

Cascade deletes must be used cautiously.

LeaveRequest records are historical business records.

Deleting an Employee should therefore NOT cascade-delete historical LeaveRequests.

Recommended behavior:

```text
Employee
   |
   | restrict/delete protection
   |
LeaveRequest
```

Employee deactivation should be used instead of physical deletion where historical records exist.

---

# 28. Referential Integrity

All foreign key relationships must be enforced.

Examples:

LeaveRequest:

- Employee must exist.
- Reviewer must exist when `ReviewedBy` is populated.

The database must not contain orphaned LeaveRequests.

---

# 29. Performance

Database performance guidelines:

- Use appropriate indexes.
- Avoid N+1 queries.
- Project only required columns.
- Paginate large result sets.
- Avoid unnecessary `Include`.
- Use `AsNoTracking()` for read-only queries where appropriate.
- Review SQL generated by EF Core for important queries.
- Use execution plans when investigating performance problems.

Do not optimize prematurely.

Measure before introducing complexity.

---

# 30. Security

Database security requirements:

- Connection strings must not be committed to source control.
- Secrets must come from secure configuration.
- Application database access should follow least privilege.
- Production credentials must differ from development/test credentials.
- The database must not be directly exposed to clients.
- Sensitive information must not be written to logs.

The application should never expose raw database exceptions to users.

---

# 31. Backup and Recovery

Production deployment should eventually provide:

- automated backups
- appropriate retention
- point-in-time recovery where supported
- restore testing

Backup/recovery is deployment infrastructure rather than application code and is outside the core machine-test implementation unless explicitly requested.

---

# 32. Schema Evolution

Schema changes must preserve existing historical data wherever possible.

When introducing breaking schema changes:

1. Identify affected application code.
2. Create an explicit migration.
3. Consider backward compatibility.
4. Test migration from the current schema.
5. Test application behavior after migration.

Never silently modify historical LeaveRequest data to accommodate a new feature.

---

# 33. Database Review Checklist

Before accepting a database/schema change, verify:

- [ ] Project builds with zero warnings
- [ ] Entity has its own `IEntityTypeConfiguration<TEntity>`
- [ ] Configuration is under `Persistence/Configurations/`
- [ ] Naming conventions are consistent
- [ ] Appropriate SQL Server data types are used
- [ ] Primary keys are defined
- [ ] Foreign keys are defined
- [ ] Referential integrity is maintained
- [ ] Required unique constraints exist
- [ ] Appropriate indexes exist
- [ ] Indexes have a query-driven justification
- [ ] EF Core migration is created
- [ ] Migration has a descriptive name
- [ ] Migration does not modify historical data incorrectly
- [ ] Concurrency implications are considered
- [ ] Transaction boundaries are appropriate
- [ ] Leave overlap semantics are preserved
- [ ] Leave state transitions remain valid
- [ ] Employee deactivation preserves historical leaves
- [ ] Audit information is preserved
- [ ] No secrets are committed
- [ ] Important queries avoid unnecessary entity loading
- [ ] N+1 queries are avoided
- [ ] Tests cover important data-integrity behavior

---

# 34. Guiding Principle

The database should be **boring, predictable, and protective**.

Use the database to guarantee structural integrity.

Use Domain/Application code to express business behavior.

Use EF Core to provide maintainable persistence.

Do not add database complexity simply to demonstrate architectural sophistication.

Every constraint, index, transaction, concurrency mechanism, and abstraction should have a reason that can be explained during code review or an interview.
