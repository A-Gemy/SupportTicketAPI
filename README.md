# Support Ticket Management API

A RESTful API for managing customer support tickets, built with ASP.NET Core Web API, ADO.NET, SQL Server, and stored procedures.

The project implements a complete ticket lifecycle for Customers, Agents, and Admins, with authentication, authorization, comments, pagination, audit logging, concurrency-safe database operations, and standardized API responses.

## Project Highlights

- Layered Controller, Service, Data Access, and Stored Procedure architecture.
- JWT authentication with rotating refresh tokens.
- Role-based and resource-based authorization.
- Complete Customer, Agent, and Admin ticket workflows.
- Standard response contract across the API.
- Server-side pagination for ticket lists and audit logs.
- Transactional audit logging for important ticket operations.
- Concurrency-safe and idempotent status and assignment operations.
- Global exception handling with server-side error logging.
- Basic rate limiting on public authentication endpoints.

## Technology Stack

- .NET 8
- ASP.NET Core Web API
- SQL Server
- ADO.NET
- Stored Procedures
- JWT Bearer Authentication
- BCrypt password hashing
- Serilog
- Swagger / OpenAPI
- Git and GitHub

## Architecture

```text
HTTP Request
    ↓
Controller
   ↓
Service
   ↓
Data Access
   ↓
Stored Procedure
   ↓
SQL Server
```

The layers have separate responsibilities:

- **Controllers** handle HTTP concerns, authentication claims, and response creation.
- **Services** validate input and coordinate business operations.
- **Data Access** executes stored procedures and maps their results.
- **Stored Procedures** enforce important data, workflow, transaction, and concurrency rules.

## Roles and Features

### Customer

Customers can:

- Register and log in.
- Create support tickets.
- View their own paginated ticket list.
- View their own ticket details.
- Read and add comments on their own tickets.
- Close their own tickets.

### Agent

Agents can:

- Log in.
- View tickets assigned to them.
- View assigned ticket details.
- Read and add comments on assigned tickets.
- Move assigned tickets through the Agent workflow.

```text
Assigned → InProgress → Resolved
```

Agents cannot access or update tickets assigned to another Agent.

### Admin

Admins can:

- Create Agent accounts.
- View all tickets.
- View ticket details.
- View unassigned tickets.
- Assign or reassign tickets.
- Update ticket status.
- View tickets assigned to a specific Agent.
- Read and add comments.
- View and filter audit logs.

## Ticket Workflow

The normal ticket lifecycle is:

```text
Open → Assigned → InProgress → Resolved → Closed
```

Important workflow rules:

- A newly created ticket starts as `Open`.
- Assigning an open ticket moves it to `Assigned`.
- An Agent can move `Assigned` to `InProgress`.
- An Agent can move `InProgress` to `Resolved`.
- Agents cannot close tickets.
- Closed tickets cannot be assigned, updated, or commented on.
- An assigned ticket cannot be moved back to `Open` by an Admin.
- An unassigned ticket cannot be moved to `InProgress` or `Resolved`.
- Repeating the same Agent or Admin status update succeeds without changing `UpdatedAt` or creating another audit entry.
- Assigning a ticket again to the same Agent succeeds without another update or duplicate audit entry.

## Standard API Response

API endpoints return a shared response contract:

```json
{
  "isSuccess": true,
  "message": "Tickets retrieved successfully.",
  "data": {},
  "errors": null
}
```

An error response follows the same shape:

```json
{
  "isSuccess": false,
  "message": "Validation failed.",
  "data": null,
  "errors": {
    "email": [
      "Invalid email format."
    ]
  }
}
```

Service results are mapped consistently to HTTP status codes such as:

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `429 Too Many Requests`
- `500 Internal Server Error`

## Pagination

Ticket list endpoints and the audit log endpoint support:

```text
pageNumber=1
pageSize=10
```

Rules:

- `pageNumber` must be at least `1`.
- `pageSize` must be between `1` and `100`.
- Pagination offset calculations use `BIGINT` in SQL to prevent integer overflow.

Paged responses contain:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0
}
```

## Authentication and Security

Implemented authentication features:

- Customer registration.
- Login with JWT access tokens.
- Hashed refresh token storage.
- Atomic refresh token rotation.
- Refresh token revocation and logout.
- BCrypt password hashing.
- JWT signing key stored outside source control.
- Active-user validation for protected database operations.
- Duplicate email conflicts handled safely, including concurrent registration attempts.
- Failed login, successful login, and logout audit events.

JWT claims include:

* User ID
* Full name
* Email
* Role

Public registration, login, and refresh-token endpoints currently share a fixed-window rate limit of five requests per minute for each client IP address.

## Authorization

The API uses both role-based and resource-based authorization.

Examples:

- Admin endpoints require the `Admin` role.
- Customer ticket endpoints require the `Customer` role.
- Agent ticket endpoints require the `Agent` role.
- Customers can access comments only for their own tickets.
- Agents can access comments only for tickets assigned to them.
- Comment read authorization is also validated atomically inside the database operation.
- Missing or invalid authentication returns `401 Unauthorized`.
- Authenticated users without access return `403 Forbidden`.

## Concurrency and Data Integrity

Important database operations use transactions and locking where necessary.

Implemented safeguards include:

- Concurrency-safe Agent ticket status updates.
- Concurrency-safe Admin ticket status updates.
- Concurrency-safe ticket assignment and reassignment.
- Idempotent repeated status updates.
- Idempotent repeated assignment to the same Agent.
- Idempotent refresh token revocation.
- Atomic refresh token rotation.
- Atomic ticket-comment read authorization.
- Duplicate email race handling.
- Business changes and their audit records committed in the same transaction.

## Audit Logging

The system records ticket and authentication events, including:

- `TicketCreated`
- `TicketAssigned`
- `TicketStatusChanged`
- `TicketCommentAdded`
- `FailedLogin`
- `UserLoggedIn`
- `UserLoggedOut`

Audit records can include:

- Actor user ID
- Actor name
- Actor role
- Action
- Entity name
- Entity ID
- Details
- Client IP address
- Creation time

Admins can filter audit logs using:

- `action`
- `actorUserId`
- `entityName`
- `entityId`
- `fromDate`
- `toDate`
- `pageNumber`
- `pageSize`

Example:

```http
GET /api/admin/audit-logs?action=TicketStatusChanged&pageNumber=1&pageSize=10
```

## Global Error Handling

Unhandled exceptions are processed by a global exception handler.

The client receives a safe response:

```json
{
  "isSuccess": false,
  "message": "An unexpected error occurred.",
  "data": null,
  "errors": null
}
```

The response includes an `X-Trace-Id` header that can be matched with the server log entry.

Server-side exception details are appended to:

```text
Logs/support-ticket-api-errors.log
```

Runtime `.log` files are excluded from Git.

## API Endpoints

### Authentication

| Method | Endpoint | Access | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register a Customer |
| `POST` | `/api/auth/login` | Public | Log in and receive access and refresh tokens |
| `POST` | `/api/auth/refresh-token` | Public | Rotate a valid refresh token |
| `POST` | `/api/auth/logout` | Refresh token | Revoke a refresh token |
| `GET` | `/api/auth/me` | Authenticated | Return the current JWT user |
| `POST` | `/api/auth/agents` | Admin | Create an Agent account |

### Customer Tickets

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/tickets` | Create a ticket |
| `GET` | `/api/tickets/my?pageNumber=1&pageSize=10` | View the current Customer's tickets |
| `GET` | `/api/tickets/{ticketId}` | View an owned ticket |
| `PATCH` | `/api/tickets/{ticketId}/close` | Close an owned ticket |

### Shared Comments

| Method | Endpoint | Access rule |
|---|---|---|
| `GET` | `/api/tickets/{ticketId}/comments` | Admin, owning Customer, or assigned Agent |
| `POST` | `/api/tickets/{ticketId}/comments` | Admin, owning Customer, or assigned Agent |

### Agent Tickets

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/tickets/assigned-to-me?pageNumber=1&pageSize=10` | View assigned tickets |
| `GET` | `/api/tickets/assigned-to-me/{ticketId}` | View assigned ticket details |
| `PATCH` | `/api/tickets/assigned-to-me/{ticketId}/status` | Update an assigned ticket's status |

### Admin Tickets

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/tickets?pageNumber=1&pageSize=10` | View all tickets |
| `GET` | `/api/tickets/admin/{ticketId}` | View ticket details |
| `GET` | `/api/tickets/unassigned?pageNumber=1&pageSize=10` | View unassigned tickets |
| `PATCH` | `/api/tickets/{ticketId}/assign` | Assign or reassign a ticket |
| `PATCH` | `/api/tickets/{ticketId}/status` | Update ticket status |
| `GET` | `/api/tickets/assigned-to-agent/{agentId}?pageNumber=1&pageSize=10` | View tickets assigned to an Agent |

### Audit Logs

| Method | Endpoint | Access |
|---|---|---|
| `GET` | `/api/admin/audit-logs` | Admin |

## Database

The main database tables are:

- `Users`
- `RefreshTokens`
- `Tickets`
- `TicketComments`
- `AuditLogs`

Database operations are implemented through stored procedures located in `SQL/Migrations`.

Migrations must be executed in numerical order because later scripts update procedures created by earlier scripts.

## Project Structure

```text
Authorization/
Common/
Constants/
Controllers/
DataAccess/
    Interfaces/
DTOs/
ExceptionHandling/
Extensions/
Models/
Security/
Services/
    Interfaces/
SQL/
    Migrations/
docs/
```

## Local Setup

### Prerequisites

- .NET 8 SDK
- SQL Server
- SQL Server Management Studio or another SQL client
- Git

### 1. Clone the Repository

```bash
git clone <repository-url>
cd SupportTicketAPI
```

### 2. Configure the Database Connection

The default connection string uses Windows Authentication:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=SupportTicketDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Update `Server=.` if your SQL Server instance uses another name, such as:

```text
Server=.\SQLEXPRESS;
```

### 3. Configure the JWT Signing Key

Set the JWT key using .NET User Secrets:

```bash
dotnet user-secrets set "Jwt:Key" "YOUR_SECURE_JWT_KEY"
```

PowerShell example for generating a random key:

```powershell
$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()
$jwtKey = [Convert]::ToBase64String($bytes)

dotnet user-secrets set "Jwt:Key" "$jwtKey"
```

### 4. Create the Database

Run every script in `SQL/Migrations` in numerical order, starting with:

```text
01_CreateInitialSchema.sql
```

and ending with:

```text
63_PreventPaginationOffsetOverflow.sql
```

The seed script creates this demo Admin account:

```text
Email:    admin@support.com
Password: admin123
```

These credentials are intended only for local demonstration and learning. Do not reuse them in a production environment.

### 5. Restore and Run

```bash
dotnet restore
dotnet build
dotnet run
```

Open the Swagger URL displayed in the terminal. Swagger is enabled in the Development environment.

### 6. Authenticate in Swagger

1. Call `POST /api/auth/login`.
2. Copy the returned access token.
3. Select **Authorize** in Swagger.
4. Enter the token value.
5. Test endpoints allowed for the authenticated role.

## Project Status

The MVP and the planned post-MVP reliability and response-standardization work are complete.

The repository currently uses manual Swagger and SSMS testing. Additional ideas for future development are documented in [docs/Requirements.md](docs/Requirements.md).
