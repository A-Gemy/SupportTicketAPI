# Support Ticket Management API

A secure RESTful API for managing customer support tickets, built with ASP.NET Core Web API, ADO.NET, SQL Server, and Stored Procedures.

## Overview

The project provides a complete support-ticket workflow for three roles:

* Customer
* Agent
* Admin

It includes JWT authentication, refresh token rotation, role-based authorization, resource-based authorization, ticket comments, status management, and audit logging.

## Technologies

* .NET 8
* ASP.NET Core Web API
* SQL Server
* ADO.NET
* Stored Procedures
* JWT Authentication
* Refresh Tokens
* BCrypt Password Hashing
* Swagger / OpenAPI
* Git and GitHub

## Architecture

```text
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

The project follows a layered architecture that separates API handling, business logic, database access, and SQL operations.

## Roles and Features

### Customer

Customers can:

* Register and log in
* Create support tickets
* View their own tickets
* View their ticket details
* Read and add comments
* Close their own tickets

### Agent

Agents can:

* Log in
* View tickets assigned to them
* View assigned ticket details
* Read and add comments
* Update ticket status using:

```text
Assigned → InProgress → Resolved
```

Agents cannot access or update tickets assigned to other Agents.

### Admin

Admins can:

* Create Agent accounts
* View all tickets
* View ticket details
* View unassigned tickets
* Assign and reassign tickets
* Update ticket status
* View tickets assigned to a specific Agent
* Read and add comments
* View audit logs with optional filters

## Ticket Workflow

```text
Open
  ↓
Assigned
  ↓
InProgress
  ↓
Resolved
  ↓
Closed
```

Authorization rules ensure that:

* Customers access only their own tickets.
* Agents access only tickets assigned to them.
* Closed tickets cannot receive new comments.
* Agents cannot close tickets.
* Admins manage ticket assignment and final status changes.

## Authentication

The API uses JWT access tokens and refresh tokens.

Implemented authentication features:

* Customer registration
* User login
* JWT access token generation
* Refresh token rotation
* Refresh token revocation
* Logout
* Hashed refresh token storage
* BCrypt password hashing
* Admin-created Agent accounts

JWT claims include:

* User ID
* Full name
* Email
* Role

## Authorization

The API uses both:

* Role-Based Authorization
* Resource-Based Authorization

Examples:

* Admin endpoints require the `Admin` role.
* Customers can access and comment only on owned tickets.
* Agents can access and comment only on assigned tickets.
* Unauthorized resource access returns `403 Forbidden`.
* Missing or invalid authentication returns `401 Unauthorized`.

## Audit Logging

The system records important ticket events:

* `TicketCreated`
* `TicketAssigned`
* `TicketStatusChanged`
* `TicketCommentAdded`

Audit records include:

* Actor user ID
* Actor name
* Actor role
* Action
* Entity name
* Entity ID
* Details
* Creation time

Admins can retrieve audit logs using:

```http
GET /api/admin/audit-logs
```

Optional filters include:

```text
action
actorUserId
entityName
entityId
fromDate
toDate
```

Example:

```http
GET /api/admin/audit-logs?action=TicketStatusChanged&actorUserId=1005
```

## Main API Endpoints

### Authentication

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
GET  /api/auth/me
POST /api/auth/agents
```

### Customer Tickets

```http
POST  /api/tickets
GET   /api/tickets/my
GET   /api/tickets/{ticketId}
PATCH /api/tickets/{ticketId}/close
```

### Shared Comments

```http
GET  /api/tickets/{ticketId}/comments
POST /api/tickets/{ticketId}/comments
```

### Agent Tickets

```http
GET   /api/tickets/assigned-to-me
GET   /api/tickets/assigned-to-me/{ticketId}
PATCH /api/tickets/assigned-to-me/{ticketId}/status
```

### Admin Tickets

```http
GET   /api/tickets
GET   /api/tickets/admin/{ticketId}
GET   /api/tickets/unassigned
PATCH /api/tickets/{ticketId}/assign
PATCH /api/tickets/{ticketId}/status
GET   /api/tickets/assigned-to-agent/{agentId}
```

### Audit Logs

```http
GET /api/admin/audit-logs
```

## Database

The main database tables are:

* `Users`
* `Tickets`
* `TicketComments`
* `RefreshTokens`
* `AuditLogs`

Database operations are implemented using Stored Procedures.

Transactions are used when an operation must update business data and create an audit record together.

## Project Structure

```text
Controllers/
Services/
    Interfaces/
DataAccess/
    Interfaces/
Models/
DTOs/
Security/
Authorization/
Constants/
SQL/
    Migrations/
docs/
```

## Local Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd SupportTicketAPI
```

### 2. Configure the Database Connection

The local SQL Server connection string is configured in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=SupportTicketDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Update the server name if your local SQL Server instance uses a different name.

For example:

```text
Server=.\SQLEXPRESS;
```

The current connection string uses Windows Authentication and does not contain a database username or password.

### 3. Configure the JWT Signing Key

Initialize .NET User Secrets:

```bash
dotnet user-secrets init
```

Add a secure JWT signing key:

```bash
dotnet user-secrets set "Jwt:Key" "YOUR_SECURE_JWT_KEY"
```

The JWT signing key must not be stored in `appsettings.json` or committed to source control.

A secure key can be generated using PowerShell:

```powershell
$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()
$jwtKey = [Convert]::ToBase64String($bytes)

dotnet user-secrets set "Jwt:Key" "$jwtKey"
```

### 4. Create the Database

Run the SQL scripts inside:

```text
SQL/Migrations
```

in numerical order.

After creating the database, run the initial Admin seed script.

### 5. Restore and Run the Project

```bash
dotnet restore
dotnet build
dotnet run
```

Open Swagger using the URL displayed in the terminal.

## API Testing

Swagger can be used to:

* Register a Customer
* Log in using each role
* Add the JWT access token using the `Authorize` button
* Test Customer, Agent, and Admin endpoints
* Test refresh token rotation and logout
* Test audit log filters
* Verify `401 Unauthorized` and `403 Forbidden` responses

## Security Features

* Passwords are hashed using BCrypt.
* Passwords are never stored as plain text.
* Refresh tokens are stored as hashes.
* Refresh tokens are revoked after use.
* Refresh tokens are rotated when refreshed.
* JWT signing keys are stored outside source control.
* SQL Parameters use explicit database types and sizes.
* Stored Procedures validate important user and role rules.
* Authorization is enforced at both role and resource levels.
* Important ticket changes are recorded in audit logs.

## Project Status

The MVP is complete.

Implemented modules:

* Authentication
* Customer ticket workflow
* Agent ticket workflow
* Admin ticket management
* Shared ticket comments
* Role-based authorization
* Resource-based authorization
* Refresh token rotation
* Audit logging
* Audit log filters

Future improvements are documented in:

```text
docs/Requirements.md
```
