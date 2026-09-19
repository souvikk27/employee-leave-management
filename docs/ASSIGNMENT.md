# Source Assignment — Employee Leave Management System

Source: supplied machine-test PDF.

## Objective
Develop an ASP.NET MVC web application to manage employee leave requests with role-based access for Admin and Employee.

## Roles

### Admin
- Manage employees: add, edit, deactivate
- View and manage all leave requests
- Approve/reject pending leaves
- View reports and dashboard summary

### Employee
- Apply for leave using From Date, To Date, Reason
- View own leave requests and approval status
- View dashboard with leave summary and pending requests

## Leave process
1. Employee submits leave request.
2. Validate overlapping dates.
3. New request is Pending.
4. Admin approves or rejects.
5. Employee sees the updated status.

## Functional requirements

### Employee management
- Add
- Edit
- Deactivate
- Search/list
- Display employee total leave count

### Leave management
- Apply leave
- Admin view/approve/reject
- Prevent overlapping dates for same employee
- Validate FromDate <= ToDate

### Authentication
- Simple login
- Session or cookie authentication
- Redirect by role

### Dashboards
Admin:
- total employees
- total pending requests
- summary counts

Employee:
- leave history
- approved/rejected/pending requests

## Technical requirements
- ASP.NET MVC, Core or Framework
- SQL Server / MySQL / SQLite
- Entity Framework / Dapper / ADO.NET
- Razor Views + Bootstrap/simple CSS
- DataAnnotations + server-side validation
- Exception handling + user-friendly messages

## Default test users
Admin: admin@example.com / admin123
Employee: employee@example.com / emp123

These credentials are assignment-provided test credentials. Store only password hashes in the database/application.

## Optional bonus
- SignalR notification when Admin approves/rejects
- Dashboard filters by department/date/status
- Excel/PDF export
- Audit logging: who approved/rejected and when

## Deliverables
- Source code
- SQL script or EF migrations
- README with setup and default credentials

## Requirement authority
This document is the source of truth for assignment scope. Where it is silent, do not invent requirements without documenting the assumption.
