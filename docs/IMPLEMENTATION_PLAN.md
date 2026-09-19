# Agentic Implementation Plan

Implement in small reviewable phases.

## Phase 0 — Skeleton
Deliver:
- solution/projects
- references
- nullable enabled
- dependency injection
- configuration
- health/build baseline

Acceptance:
- solution builds cleanly.

## Phase 1 — Persistence
Deliver:
- Employee
- LeaveRequest
- LeaveStatus
- AppDbContext
- EF configurations
- migration
- seed Admin + Employee

Acceptance:
- database can be created/migrated
- seeded users work with assignment credentials.

## Phase 2 — Authentication / Authorization
Deliver:
- login/logout
- cookie auth
- role claims
- Admin/Employee authorization
- post-login redirects

Acceptance:
- unauthenticated users cannot access protected pages
- Employee cannot access Admin pages.

## Phase 3 — Employee management
Deliver:
- list/search
- add
- edit
- deactivate
- total leave count

Acceptance:
- inactive employees cannot submit leave.

## Phase 4 — Leave application
Deliver:
- apply page
- DataAnnotations
- server-side business validation
- overlap detection
- Pending creation
- employee's own leave list

Acceptance:
- invalid dates rejected
- overlap rejected
- only self-owned requests accessible to Employee.

## Phase 5 — Admin approval
Deliver:
- pending/all leave view
- approve
- reject
- ReviewedBy/ReviewedAt
- state transition enforcement

Acceptance:
- only Pending requests transition
- Employee cannot approve/reject.

## Phase 6 — Dashboards
Deliver:
- Admin counts
- Employee counts/history/pending
- efficient projection queries

Acceptance:
- no dashboard aggregation in controllers.

## Phase 7 — Cross-cutting hardening
Deliver:
- centralized error handling
- structured logging
- anti-forgery
- authorization review
- concurrency strategy
- friendly validation/error messages

Acceptance:
- no sensitive exception details leak
- concurrent overlap scenario is addressed/documented.

## Phase 8 — Tests
Prioritize:
- leave state transitions
- date validation
- overlap rule
- authorization/ownership
- inactive employee rule
- concurrent submission behavior where practical

## Phase 9 — Optional bonus
Only after core requirements are stable:
1. Audit logging
2. SignalR
3. Dashboard filters
4. Export

Do not allow bonus work to destabilize core requirements.
