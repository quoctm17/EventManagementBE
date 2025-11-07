# Database Scripts

This folder stores raw SQL scripts (e.g. `EventManagementDB.sql`) for recreating and seeding the EventManagementDB database.

They are not referenced by any project (`.csproj`) so they do not affect builds or runtime. Safe to push to GitHub.

## Usage (Manual)
1. Open SQL Server Management Studio (SSMS) or Azure Data Studio.
2. Copy the contents of `EventManagementDB.sql`.
3. Execute against your local SQL Server instance.

> NOTE: The script drops and recreates the `EventManagementDB` database. Do **not** run in production.

## Future Ideas
- Add migration diff scripts.
- Split seed data into separate files.
- Provide a lightweight version without large seed data.
