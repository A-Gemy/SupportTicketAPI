# SupportTicketAPI — Requirements and Scope

## Project Goal

Build a secure RESTful API for managing customer support tickets using ASP.NET Core Web API, ADO.NET, SQL Server, and Stored Procedures.

## Technology Stack

* .NET 8 and ASP.NET Core Web API
* SQL Server, ADO.NET, and Stored Procedures
* JWT Authentication and Refresh Tokens
* BCrypt password hashing
* Role-Based and Resource-Based Authorization
* Swagger and GitHub

## Roles

* Customer
* Agent
* Admin

## Account Rules

* Customers can register publicly.
* Agents are created by an Admin.
* Admin registration is not publicly available.
* The initial Admin is created using a SQL seed script.

## Implemented MVP

### Authentication

* Customer registration
* Login and JWT access tokens
* Refresh token rotation
* Logout and token revocation
* Admin creates Agent accounts
* JWT signing key stored using User Secrets

### Customer

* Create tickets
* View own tickets and details
* Read and add comments
* Close own tickets

### Agent

* View assigned tickets and details
* Read and add comments
* Update status using:

```text
Assigned → InProgress → Resolved
```

### Admin

* View all tickets and ticket details
* View unassigned tickets
* Assign or reassign tickets
* View tickets assigned to an Agent
* Update ticket status
* Read and add comments
* View audit logs with optional filters

## Ticket Workflow

```text
Open → Assigned → InProgress → Resolved → Closed
```

* Customers access only their own tickets.
* Agents access only tickets assigned to them.
* Closed tickets cannot receive comments.
* Only Admin or Customer closing rules can move a ticket to `Closed`.

## Audit Logging

The system records:

* `TicketCreated`
* `TicketAssigned`
* `TicketStatusChanged`
* `TicketCommentAdded`

Audit logs can be filtered by action, actor, entity, and date range.

## Database Tables

* Users
* RefreshTokens
* Tickets
* TicketComments
* AuditLogs

## Deferred Improvements

* Pagination and advanced ticket filters
* Automated tests
* Global exception handling
* Rate limiting
* Nested comments
* Improved reassignment workflow
* Resolved-ticket notifications
* Production secret management
