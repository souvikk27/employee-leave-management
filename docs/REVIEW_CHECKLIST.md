# Code Review Checklist

## Correctness
- [ ] All assignment requirements implemented
- [ ] Pending -> Approved/Rejected only
- [ ] Invalid date range rejected
- [ ] Overlapping leave rejected
- [ ] Rejected leave does not block future leave
- [ ] Inactive employee cannot submit
- [ ] Dashboard counts are correct

## Security
- [ ] Cookie authentication configured securely
- [ ] Role authorization applied at controller/action boundary
- [ ] Anti-forgery enabled for state-changing MVC forms
- [ ] Employee ownership is derived from authenticated identity
- [ ] No IDOR through employee/leave IDs
- [ ] Passwords are hashed
- [ ] No secrets committed
- [ ] No sensitive exception details rendered

## Concurrency / integrity
- [ ] Overlap check + insert has a documented consistency strategy
- [ ] Approval/rejection cannot race into invalid state
- [ ] Invalid state transitions are rejected
- [ ] Database constraints support domain integrity where practical

## Architecture
- [ ] Controllers are thin
- [ ] Domain has no infrastructure dependencies
- [ ] Application has no MVC dependencies
- [ ] Infrastructure contains persistence concerns
- [ ] No generic repository
- [ ] No unnecessary MediatR/CQRS abstractions
- [ ] No business logic duplicated between controllers/services

## Performance
- [ ] Dashboard queries project only required fields
- [ ] No obvious N+1 queries
- [ ] Async I/O used
- [ ] Pagination considered for employee/leave lists if needed

## UX
- [ ] Validation messages are clear
- [ ] Success/error feedback is visible
- [ ] Unauthorized access has sensible behavior
- [ ] Empty states handled

## Tests
- [ ] Business rules have automated tests
- [ ] Authorization boundaries tested
- [ ] Important failure paths tested

## Machine-test hygiene
- [ ] README complete
- [ ] EF migrations included
- [ ] Setup works from clean checkout
- [ ] Seed/test credentials documented
- [ ] No dead code
- [ ] No unexplained TODOs
