<<<<<<< HEAD
# Naistrings
=======
# Naistrings Conference Portal — Package 01

## Milestone 1: Foundation and runnable solution

This package provides the initial .NET 8 solution scaffold.

- `Naitrings.sln`: solution file.
- `NaistringsAPI`: ASP.NET Core 8 Web API, EF Core SQL Server configuration, health and diagnostics endpoints.
- `NaistringsCommunity`: Blazor WebAssembly community frontend.
- `NaistringsAdmin`: Blazor WebAssembly admin frontend scaffold.
- `Naistrings.Tests`: initial xUnit test project.

All project directories are directly under the solution root. The API follows the agreed structure: `Controllers`, `Managers`, `Services/Interfaces`, `Services/Implementations`, `Models/Entities`, `Models/DTOs`, and `Data/ApplicationDbContext.cs`.

## Prerequisites

Install the .NET 8 SDK and SQL Server LocalDB (or change the connection string for another SQL Server instance).

## Build and test

Open `Naitrings.sln` in Visual Studio 2022 and restore packages, or run:

```bash
dotnet restore Naitrings.sln
dotnet build Naitrings.sln
dotnet test Naitrings.sln
```

## Run

Run the API and both Blazor projects as separate startup projects. API defaults to `https://localhost:7100` / `http://localhost:5100`. Both frontend configurations point to `https://localhost:7100/`; ensure the frontend launch ports are included in the API CORS allowlist.

API checks:
- `/health`
- `/api/v1/diagnostics/ping`
- Swagger UI in Development at `/swagger`

## EF Core

The DbContext and provider are configured, but no migration is included because the initial domain entities have not yet been defined. After adding the first entity:

```bash
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef migrations add InitialCreate --project NaistringsAPI --startup-project NaistringsAPI
dotnet ef database update --project NaistringsAPI --startup-project NaistringsAPI
```

Use user-secrets or environment configuration for local credentials. Do not commit production secrets.

## Scope and security

This is a foundation scaffold, not a production-ready application. Authentication, authorisation, business modules, payment processing, production logging configuration, deployment automation and meaningful test coverage are later milestones. The admin frontend is not protected until the identity/access-control milestone is implemented.


## Milestone 2
See [README_MILESTONE_02.md](README_MILESTONE_02.md) for the identity foundation integration and migration steps.
>>>>>>> 49b73cb (Add project files.)
