# **Assignment: Employee Leave Management System** 

## **Objective** 

Develop an ASP.NET MVC web application to manage employee leave requests with role-based access for Admin and Employee. 

## **Application Flow** 

1. User Roles 

   - a) Admin Login: 

      - i. Manage employees (add, edit, deactivate). 

      - ii. View and manage all leave requests. 

      - iii. Approve or reject pending leaves. 

      - iv. View reports and dashboard summary. 

   - b) Employee Login: 

      - i. Apply for leave by selecting From Date, To Date, and Reason. 

      - ii. View their own leave requests and approval status. 

      - iii. Access their dashboard showing leave summary and pending requests. 

2. Leave Process 

   - a) Employee submits a leave request (validated for overlapping dates). 

   - b) The system marks the request as Pending. 

   - c) Admin reviews and updates the status to Approved or Rejected. 

   - d) Employee sees the update on their dashboard. 

3. Dashboard 

   - a) Admin Dashboard: Shows total employees, total pending requests, and summary counts. 

   - b) Employee Dashboard: Shows personal leave history, approved/rejected leaves, and pending ones. 

## **Functional Requirements** 

1. Employee Management 

   - a) Add, edit, and deactivate employees. 

   - b) Search or list all employees. 

   - c) Display each employee’s total leave count. 

2. Leave Management 

   - a) Employees can apply for leave (FromDate, ToDate, Reason). 

   - b) Admin can view and approve/reject requests. 

   - c) Prevent overlapping leave dates for the same employee. 

   - d) Validate date ranges (FromDate ≤ ToDate). 

3. Authentication & Roles 

   - a) Create a simple login page. 

   - b) Use session or cookie-based authentication. 

   - c) Redirect users to their respective dashboards after login. 

4. Reports & Dashboard 

   - a) Admin: show overall statistics, pending and approved counts. 

   - b) Employee: show leave history and request status. 

## **Technical Requirements** 

1. Framework: ASP.NET MVC (Core or Framework version). 

2. Database: Microsoft SQL Server / MySQL / SQLite . 

3. Data Access: Entity Framework, Dapper, or ADO.NET. 

4. Frontend: Razor Views with Bootstrap or any simple CSS styling. 

5. Validation: Use DataAnnotations and server-side validation. 

6. Error Handling: Handle exceptions and show user-friendly messages. 

7. Default Users 

   - a) Create one Admin and one Employee as static records in the database for testing. 

   - b) Example 

      - i. Admin: admin@example.com / admin123 

      - ii. Employee: employee@example.com / emp123 

## **Optional (Bonus Features)** 

1. Real-Time Notifications (SignalR) 

Implement SignalR to notify employees in real time when: 

- a) An Admin approves or rejects a leave request. 

   - b) The notification can appear as a small popup or message on the employee’s dashboard. 

2. Reporting Enhancements 

   - a) Add filters for department, date range, or status on the Admin dashboard. 

   - b) Add an export to Excel or PDF option (optional). 

3. Audit Logging 

   - a) Track who approved/rejected a request and when it was done. 

## **Deliverables** 

1. Complete working source code (GitHub link or ZIP). 

2. SQL script or EF migrations for database setup. 

3. README file including: 

   - a) Setup instructions 

   - b) Default login credentials (Admin/Employee) 

