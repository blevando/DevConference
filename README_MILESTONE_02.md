# Naistrings — Milestone 2 Part A (reworked from the supplied working codebase)

## Source of truth
This package is rebuilt from the user-supplied `Naistrings(2).zip`, **not** from the earlier generated foundation archive. The solution file, both Blazor WebAssembly portals, diagnostic controller, launch profiles, CSS, frontend API URLs, SQL Server connection string (`Server=localhost;Database=NaistringsDb`), and configured CORS origins are retained. Only API identity components and their required registration are added.

## What changed
- `ApplicationDbContext` now inherits `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`.
- Added `ApplicationUser`, registration/login DTOs, `AuthController`, `AuthManager`, `IAuthService`, `AuthService`, and `IdentityRoleSeeder`.
- Added .NET 8 JWT bearer and Identity EF Core NuGet dependencies.
- Added Identity registration, JWT validation, authentication middleware and role authorization.
- Kept `Cors:AllowedOrigins` configuration from the uploaded codebase (including 14986 and 14989). This is an explicit allowlist; it does not use the unrestricted CORS option discussed previously.
- Added opt-in role seeding via `Identity:SeedRoles` configuration (defaults to false).
- Added `/health/database` alongside the existing `/health` and diagnostic controller.

## Local setup (run in this order)
1. **Back up** the current working solution and database. Extract this package to a separate folder first.
2. Review `NaistringsAPI/appsettings.json`; the supplied working SQL Server connection string and CORS origins are preserved. Update them only if your current environment differs.
3. From the `NaistringsAPI` folder, configure secrets (never commit JWT signing secrets):

   ```powershell
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_A_RANDOM_KEY_OF_AT_LEAST_32_UTF8_BYTES"
   dotnet user-secrets set "Jwt:Issuer" "NaistringsAPI"
   dotnet user-secrets set "Jwt:Audience" "NaistringsFrontends"
   ```

   Replace the example key with a securely generated random secret. `dotnet user-secrets init` updates the API csproj with a UserSecretsId.
4. From the solution root:

   ```powershell
   dotnet restore Naitrings.sln
   dotnet build Naitrings.sln
   dotnet test Naitrings.sln
   ```

5. From the API project folder, generate and inspect the first Identity migration before applying it. If you already have EF migrations in your local database, **do not** blindly generate a new initial migration; reconcile the existing migration history first.

   ```powershell
   dotnet tool install --global dotnet-ef --version 8.0.11
   dotnet ef migrations add InitialIdentity --context ApplicationDbContext --output-dir Migrations
   dotnet ef database update --context ApplicationDbContext
   ```

   `dotnet tool update --global dotnet-ef --version 8.0.11` may be used if already installed.
6. Seed the roles after migration (the registration service requires `CommunityMember` to exist):

   ```powershell
   dotnet user-secrets set "Identity:SeedRoles" "true"
   dotnet run
   ```

   After a successful startup, set `Identity:SeedRoles` to `false` again. This does not provision an administrator account.
7. Check `/health`, `/health/database`, `/api/v1/diagnostics/ping` and both existing frontend **Check API** buttons.

## API endpoints
- `POST /api/v1/auth/register`: creates community account with `CommunityMember` role. Email confirmation is required before login.
- `POST /api/v1/auth/login`: returns a 15-minute JWT for an existing, confirmed user.
- `GET /api/v1/auth/me`: bearer-authenticated identity check.
- `GET /api/v1/auth/admin-check`: bearer token with `SuperAdmin` or `Admin` role.

## Scope and limitations
This is **Part A only**: it deliberately does not include frontend login screens, email confirmation delivery/verification, password reset, refresh-token rotation, controlled admin bootstrap, or production-ready end-to-end authentication. Since email confirmation is required, new accounts cannot sign in until the confirmation workflow is built or a test account is manually confirmed in a controlled development environment. No migration is shipped because it must be generated against your verified SQL Server database/migration history. Do not expose the JWT key in source control. The project was not compiled here because a .NET SDK is not available; verify build and tests locally.
