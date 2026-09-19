# Cross-Cutting Concern Review

Review the repository without rewriting feature code.

Focus only on:
- authentication
- authorization
- IDOR/ownership
- CSRF/anti-forgery
- exception handling
- logging
- validation
- transaction boundaries
- concurrency
- database integrity
- configuration/secrets
- observability
- cancellation
- async correctness
- performance/N+1
- testability

For each issue:
Severity:
Evidence:
Risk:
Recommended fix:
How to test:

Pay special attention to whether business correctness depends on application code alone when the database could provide stronger integrity.
