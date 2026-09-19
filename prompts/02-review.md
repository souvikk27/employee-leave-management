# Coding Agent Prompt — Review Existing Implementation

Read:
- `AGENTS.md`
- `docs/ASSIGNMENT.md`
- `docs/ARCHITECTURE.md`
- `docs/DOMAIN_RULES.md`
- `docs/REVIEW_CHECKLIST.md`

Perform a code review only. Do not modify code.

Review in this order:
1. Security/authorization
2. Business-rule correctness
3. Concurrency/data integrity
4. Architecture boundaries
5. EF Core/database behavior
6. Error handling
7. Performance
8. Tests
9. Maintainability

For every finding provide:
- Severity: Critical / High / Medium / Low
- File and location
- Problem
- Why it matters
- Concrete remediation

Do not praise trivial things. Focus on defects, risks, missing tests, and questionable tradeoffs.

At the end provide:
- top 5 issues
- assumptions you think are unsafe
- questions an interviewer could challenge
