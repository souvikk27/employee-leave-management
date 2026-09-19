# Decision Log

Use this file to record decisions made during implementation.

Format:

## DEC-XXX — <decision>
Date:
Context:
Decision:
Alternatives:
Tradeoff:
Why this is appropriate for this assignment:
Tests/verification:

Initial decisions:

## DEC-001 — Lightweight layered monolith
Decision: Four-project layered monolith.
Tradeoff: More separation than a single MVC project, less ceremony than full enterprise Clean Architecture.
Reason: The assignment benefits from clear boundaries but has a small domain.

## DEC-002 — EF Core without generic repository
Decision: Use DbContext directly from application-facing persistence abstractions where appropriate.
Tradeoff: Less abstraction, but fewer meaningless wrappers and better EF query control.

## DEC-003 — Cookie authentication + roles
Decision: ASP.NET Core cookie authentication with Admin/Employee role claims.
Tradeoff: Simple and aligned with the MVC assignment.

## DEC-004 — Leave lifecycle as explicit state transitions
Decision: Pending -> Approved/Rejected.
Tradeoff: Slightly more domain code than setting an enum directly, but protects invariants.

## DEC-005 — Concurrency treated as a consistency boundary
Decision: Overlap detection and insertion must be considered together.
Tradeoff: More database/transaction complexity than a simple query-before-insert, but avoids a known race.

## DEC-006 — Serializable transaction for overlap check + insert
Date: 2026-09-19
Context: Two employees' concurrent submissions (or double-submits) could both pass an overlap pre-check and insert overlapping Pending/Approved leaves.
Decision: `LeaveCommands.ApplyLeaveAsync` runs the blocking-overlap `AnyAsync` check and the insert in one SQL Server `Serializable` transaction.
Alternatives: Snapshot isolation + retry loop (more throughput, more code); database exclusion constraint (not expressible for date ranges + status filter in SQL Server without triggers).
Tradeoff: Serializable range locks serialize concurrent applicants and can deadlock under contention; the failure surfaces as a database error the user can retry. Correctness (no overlapping blocking leaves) is prioritized over throughput for this low-contention domain.
Why this is appropriate for this assignment: small user base, correctness is the top quality bar, implementation stays readable.
Tests/verification: build + unit suite green; race covered by design review (no live concurrency harness in repo — see known gaps).

## DEC-007 — Optimistic concurrency for approval/rejection
Date: 2026-09-19
Context: Two admins could concurrently approve/reject the same Pending request, risking a double transition or lost update.
Decision: Single-row `SaveChanges` (atomic) guarded by the existing `Version` concurrency token; `DbUpdateConcurrencyException` is translated to a friendly "already been reviewed" message, and the domain still rejects non-Pending transitions.
Alternatives: Pessimistic row lock (`UPDLOCK`) per review (heavier, unnecessary for rare admin collisions).
Tradeoff: The loser of a race must retry manually, but no invalid state is ever persisted.
Why this is appropriate for this assignment: admin reviews are infrequent; optimistic handling keeps the write path simple.
Tests/verification: domain transition tests green; race covered by design review.
Update 2026-09-19 (Phase 8): SQLite-backed tests proved the token never persisted — `ValueGeneratedOnAddOrUpdate` makes EF Core skip the client-side `Version++` on UPDATE, and with no database trigger the column stayed at 1 forever, so no conflict could ever fire. Fixed by mapping `Version` as a client-owned concurrency token (`IsConcurrencyToken` + insert default, no `ValueGeneratedOnAddOrUpdate`) on all four entities; migration `FixVersionConcurrencyTokenValueGeneration` applied. A permanent `Approve_PersistsIncrementedVersionToken` regression test guards the mapping.
