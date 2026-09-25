# SQL Organization

The database scripts are separated by purpose.

## Migrations

`Migrations` contains ordered scripts for database structure and one-time data changes.

Current setup scripts:

1. `01_CreateInitialSchema.sql`
2. `02_SeedInitialAdmin.sql`

Run migrations in numerical order when creating a new local database.

Future table, constraint, index, or one-time data changes should be added as new numbered migrations. Previously applied migrations should not be rewritten.

## Stored Procedures

`StoredProcedures` contains one canonical file for every active stored procedure, grouped by responsibility:

- `Authentication`
- `RefreshTokens`
- `Tickets/Customer`
- `Tickets/Agent`
- `Tickets/Admin`
- `Comments`
- `Audit`

Each file contains the latest complete definition and uses `CREATE OR ALTER PROCEDURE`.

When a stored procedure changes, update its existing canonical file instead of creating another migration that repeats the whole procedure definition.

When a stored procedure is permanently removed:

1. Remove its canonical file.
2. Add a numbered migration that drops it from existing databases.

## Fresh Database Setup

1. Run `Migrations/01_CreateInitialSchema.sql`.
2. Run `Migrations/02_SeedInitialAdmin.sql`.
3. Run every `.sql` file under `StoredProcedures`.

The stored-procedure files can be rerun safely because they use `CREATE OR ALTER PROCEDURE`.

## Existing Database

Do not rerun the schema or seed migrations against an existing configured database.

Run the canonical stored-procedure files to synchronize procedure definitions with the repository.

Historical procedure migrations removed during SQL organization remain available in Git history and in the `v2.0.0` tag.