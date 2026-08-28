# SupportTicketAPI — Requirements and Scope

## Project Goal

Build a secure RESTful API for managing customer support tickets using ASP.NET Core Web API, ADO.NET, SQL Server, and stored procedures.

The project is designed as a learning and portfolio project that demonstrates authentication, authorization, layered architecture, database transactions, concurrency control, API response design, and business workflow enforcement.

## Technology Stack

- .NET 8 and ASP.NET Core Web API
- SQL Server, ADO.NET, and stored procedures
- JWT authentication and refresh tokens
- BCrypt password hashing
- Role-based and resource-based authorization
- Serilog file logging
- Swagger / OpenAPI
- Git and GitHub

## Roles

- Customer
- Agent
- Admin

## Account Rules

- Customers can register publicly.
- Agents are created by an active Admin.
- Admin registration is not publicly available.
- The initial demo Admin is created using a SQL seed script.
- User emails must be unique.
- Inactive accounts cannot complete protected account operations.

## Implemented Functional Scope

### Authentication

- Customer registration
- Login and JWT access-token generation
- Hashed refresh-token storage
- Atomic refresh-token rotation
- Refresh-token revocation
- Idempotent repeated token revocation
- Logout
- Current-user endpoint based on JWT claims
- Admin-created Agent accounts
- Duplicate-email conflict handling
- Duplicate-email race handling
- Active-Admin validation during Agent creation
- Authentication audit events

### Customer

- Create tickets
- View own paginated tickets
- View owned ticket details
- Read comments on owned tickets
- Add comments to owned tickets
- Close owned tickets
- Concurrency-safe ticket closing

### Agent

- View paginated assigned tickets
- View assigned ticket details
- Read comments on assigned tickets
- Add comments to assigned tickets
- Update ticket status using:

```text
Assigned → InProgress → Resolved
```

- Concurrency-safe status updates
- Idempotent repeated status updates

### Admin

- Create Agent accounts
- View all tickets with pagination
- View ticket details
- View unassigned tickets with pagination
- Assign or reassign tickets
- View tickets assigned to a specific Agent with pagination
- Update ticket status
- Read and add comments
- View paginated audit logs with optional filters
- Concurrency-safe ticket assignment and status updates
- Idempotent repeated assignment and status updates

## Ticket Workflow

```text
Open → Assigned → InProgress → Resolved → Closed
```

Current rules:

- Customers access only their own tickets.
- Agents access only tickets assigned to them.
- Closed tickets cannot receive new comments.
- Closed tickets cannot be assigned or updated.
- Agents cannot close tickets.
- An assigned ticket cannot return to `Open` through the Admin status endpoint.
- An unassigned ticket cannot move to `InProgress` or `Resolved`.
- Repeating an already completed assignment or status change is treated as a successful no-op.

## Comments Authorization

- Admins can read and add comments on any ticket.
- Customers can read and add comments only on owned tickets.
- Agents can read and add comments only on assigned tickets.
- Resource-based authorization is enforced in the API.
- Comment read access is revalidated atomically in the stored procedure.
- Comment creation rules are also validated inside the database operation.

## Standard API Responses

All completed API endpoints use a common response contract containing:

- `isSuccess`
- `message`
- `data`
- `errors`

Service results classify failures as:

- Validation error
- Unauthorized
- Forbidden
- Not found
- Conflict
- Internal failure

The API maps these classifications to consistent HTTP status codes.

Model-validation failures also use the shared response contract and include field-level errors.

## Pagination

Pagination is implemented for:

- Admin all-ticket list
- Customer own-ticket list
- Agent assigned-ticket list
- Admin unassigned-ticket list
- Admin tickets-by-Agent list
- Admin audit-log list

Pagination requirements:

- Default page number: `1`
- Default page size: `10`
- Maximum page size: `100`
- Stable ordering includes a unique ID as a tie-breaker.
- SQL offset arithmetic uses `BIGINT` to prevent overflow.

## Audit Logging

The system records:

- `TicketCreated`
- `TicketAssigned`
- `TicketStatusChanged`
- `TicketCommentAdded`
- `FailedLogin`
- `UserLoggedIn`
- `UserLoggedOut`

Audit data can include the actor, action, entity, details, client IP address, and creation time.

Audit-log filters:

- Action
- Actor user ID
- Entity name
- Entity ID
- Start date
- End date

Ticket operations that change business data write their audit record inside the same database transaction.

## Reliability and Security Requirements

Implemented safeguards:

- Passwords hashed with BCrypt
- Refresh tokens stored as hashes
- JWT signing key kept outside committed configuration
- Parameterized stored-procedure calls with explicit SQL types and sizes
- Role-based endpoint authorization
- Resource-based ticket authorization
- Database-level validation of important access rules
- Safe duplicate-email handling under concurrency
- Row-level locking for assignment and status transitions
- Transactions for related business and audit changes
- Sanitized refresh-token rotation failures
- Basic fixed-window rate limiting for public authentication endpoints
- Global exception handling
- Safe `500 Internal Server Error` response
- Trace ID returned in the `X-Trace-Id` response header
- Detailed exceptions appended to one Git-ignored log file

## Database Scope

- `Users`
- `RefreshTokens`
- `Tickets`
- `TicketComments`
- `AuditLogs`

All application database operations use stored procedures.

SQL scripts are currently maintained as ordered migrations in `SQL/Migrations`.

## Current Verification Approach

The current project is verified manually using:

- `dotnet build`
- Swagger endpoint tests
- SSMS stored-procedure tests
- SSMS concurrency tests using multiple query windows
- Git diff and status review before each commit

Automated tests and automated build workflows are not currently part of the project.

## Possible Future Features

- Automated unit and integration tests
- Automated build and continuous integration workflow
- Production-ready rate limiting for reverse-proxy and multi-instance deployments
- Production-safe initial Admin provisioning
- Stable database result codes independent of response messages
- Consolidated canonical stored-procedure files
- Query execution-plan review and index tuning using representative data
- Advanced ticket filtering and search
- Further ticket-workflow refinements
- Nested or threaded comments
- Resolved-ticket notifications
- Background notification processing

## Current Status

The MVP is complete.

The post-MVP response standardization, pagination, audit improvements, concurrency safeguards, authorization hardening, refresh-token hardening, global exception handling, and pagination-overflow protection are also complete.

Future features can be added incrementally as the project continues to evolve.
