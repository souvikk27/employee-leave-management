# Domain and Business Rules

## Leave status

```text
Pending
  |   |    v   v
Approved  Rejected
```

Only Pending can transition to Approved or Rejected.

Approved and Rejected are terminal.

## Date validity
`FromDate <= ToDate`

## Overlap
For the same employee:

```text
existing.FromDate <= requested.ToDate
AND
existing.ToDate >= requested.FromDate
```

Blocking statuses:
- Pending
- Approved

Rejected does not block.

## Employee eligibility
Only active employees can submit new leave requests.

## Authorization
Employee:
- may create leave for self
- may view own leaves
- must not approve/reject
- must not access another employee's leave data

Admin:
- may manage employees
- may view all leave requests
- may approve/reject Pending requests

## Approval/rejection
Approval/rejection must:
- be performed by an authorized Admin
- target a Pending request
- set ReviewedAt
- set ReviewedBy

If rejection reason is implemented, it is required when rejecting.

## Ownership
Employee owns the submission/viewing boundary for their own leave requests.
Admin owns the approval/rejection operation.
LeaveRequest owns its lifecycle state; controllers do not.
