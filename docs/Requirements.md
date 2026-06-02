# Support Ticket Management API - Requirements

## Project Goal

Build a small RESTful API for managing support tickets.

The project will focus on applying:

- ASP.NET Core Web API
- ADO.NET
- SQL Server
- Stored Procedures
- JWT Authentication
- Role-Based Authorization
- Ownership-Based Authorization
- Policy-Based Authorization
- Audit Logs
- GitHub tracking

The first version will be small and focused. More features can be added later.

---

## Main Roles

The system will have three roles:

- Customer
- Agent
- Admin

---

## Account Rules

Customers can register using the public register endpoint.

Agents cannot register themselves.

Admins cannot register from the public register endpoint.

The first Admin account will be created using a SQL seed script.

Later, Admin can create Agent accounts.

---

## Main Ticket Rules

Customers can create support tickets.

Each ticket belongs to one Customer.

A ticket can be assigned to one Agent.

Customers can view only their own tickets.

Agents can view only tickets assigned to them.

Admins can view all tickets.

Admins can assign tickets to Agents.

Admins can view tickets assigned to a specific Agent.

Admins can view unassigned tickets.

Closed tickets cannot receive new comments.

---

## MVP Features

### Customer

- Register
- Login
- Create ticket
- View own tickets
- View own ticket details
- Add comment to own open ticket
- Close own ticket

### Agent

- Login
- View assigned tickets
- View assigned ticket details
- Add comment to assigned open ticket
- Update assigned ticket status

### Admin

- Login
- Create Agent account
- View all tickets
- View tickets assigned to a specific Agent
- View unassigned tickets
- Assign ticket to Agent
- Update ticket status
- Add comment to any open ticket
- View audit logs

---

## Initial Database Tables

The first database design will start with:

- Users
- RefreshTokens
- Tickets
- TicketComments
- AuditLogs

This design may change during development.

---

## Notes

This document describes the initial agreed scope.

Details may be updated as the project grows.