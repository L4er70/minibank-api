# MiniBank API

Simple ASP.NET Core Web API for managing customers, accounts, and transactions using SQLite.

## Requirements
- .NET SDK 8.0
- SQLite (bundled via EF Core SQLite provider)

## Quick start
1. Restore and run:
   ```bash
   dotnet restore
   dotnet run
   ```
2. The API will start on the default ASP.NET Core ports.

## Configuration
- Connection string lives in `appsettings.json` under `ConnectionStrings:DefaultConnection`.
- The default database file is `banking.db` in the project root.

## API surface (high level)
- Customers
  - `POST /api/customers` register a customer
  - `GET /api/customers` list customers
  - `GET /api/customers/search?name=...` search by name
  - `GET /api/customers/{id}` get by id
- Accounts
  - `GET /api/account/customer/{customerId}` get account by customer id
  - `GET /api/account/{accountId}` get account details
  - `GET /api/account/{accountId}/transactions` get account transactions
  - `POST /api/account/transactions` create a transaction

## Swagger
In Development, Swagger UI is enabled at `/swagger`.

## Notes
- The app seeds the database on startup via `DbInitializer.Initialize(...)`.
- HTTPS redirection is currently disabled in `Program.cs`.
