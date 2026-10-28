
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NaistringsAPI.Data;
using NaistringsAPI.Managers;
using NaistringsAPI.Models.Entities;
using NaistringsAPI.Services.Implementations;
using NaistringsAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. API CONTROLLERS AND SWAGGER
// ============================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// 2. DATABASE CONFIGURATION
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is required."));
});

// ============================================================
// 3. ASP.NET CORE IDENTITY
// ============================================================

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // ============================================================
        // 4. JWT AUTHENTICATION
        // ============================================================

        var jwtSection = builder.Configuration.GetSection("Jwt");

        var jwtKey = jwtSection["Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key is required. Configure it using user-secrets.");

        var jwtIssuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException(
                "Jwt:Issuer is required.");

        var jwtAudience = jwtSection["Audience"]
            ?? throw new InvalidOperationException(
                "Jwt:Audience is required.");

        byte[] jwtKeyBytes;

        try
        {
            jwtKeyBytes = Convert.FromBase64String(jwtKey);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "Jwt:Key must be a valid Base64-encoded value.", ex);
        }

        if (jwtKeyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key must contain at least 32 decoded bytes.");
        }

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(jwtKeyBytes),

                ValidateLifetime = true,
                RequireExpirationTime = true,

                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = JwtRegisteredClaimNames.Email,
                RoleClaimType = ClaimTypes.Role
            };
    });

// ============================================================
// 5. AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization();

// ============================================================
// 6. APPLICATION DEPENDENCY INJECTION
// ============================================================

// Authentication
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISuperAdminProvisioningService, SuperAdminProvisioningService>();





// Managers
builder.Services.AddScoped<AuthManager>();





// Register additional Managers and Services here as
// subsequent Naistrings modules are implemented.

// ============================================================
// 7. CORS CONFIGURATION
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorFrontends", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (origins.Length > 0)
        {
            policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});
 

// ============================================================
// 8. BUILD APPLICATION
// ============================================================

var app = builder.Build();




// ============================================================
// 9. INITIAL IDENTITY ROLE SEEDING
// ============================================================

// Enable only after applying the Identity migration.
// Configuration: Identity:SeedRoles = true

//Console.WriteLine(
//    "SuperAdmin provisioning enabled: " +
//    app.Configuration.GetValue<bool>("Identity:ProvisionSuperAdmin"));

if (app.Configuration.GetValue<bool>("Identity:ProvisionSuperAdmin"))
{
    using var scope = app.Services.CreateScope();

    var provisioningService = scope.ServiceProvider
        .GetRequiredService<ISuperAdminProvisioningService>();

    await provisioningService.ProvisionAsync();
}

if (app.Configuration.GetValue<bool>("Identity:SeedRoles"))
{
    using var scope = app.Services.CreateScope();

    await IdentityRoleSeeder.SeedRolesAsync(
        scope.ServiceProvider);
}

// ============================================================
// 10. SWAGGER
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ============================================================
// 11. HTTP MIDDLEWARE PIPELINE
// ============================================================

app.UseHttpsRedirection();

app.UseCors("BlazorFrontends");

app.UseAuthentication();

app.UseAuthorization();

// ============================================================
// 12. CONTROLLER ENDPOINTS
// ============================================================

app.MapControllers();

// ============================================================
// 13. APPLICATION HEALTH CHECK
// ============================================================

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        application = "NaistringsAPI",
        utcTime = DateTimeOffset.UtcNow
    });
});

// ============================================================
// 14. DATABASE HEALTH CHECK
// ============================================================

app.MapGet(
    "/health/database",
    async (ApplicationDbContext db, CancellationToken ct) =>
    {
        try
        {
            var connected =
                await db.Database.CanConnectAsync(ct);

            if (!connected)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Database not reachable");
            }

            return Results.Ok(new
            {
                status = "Connected",
                database = "Reachable",
                utcTime = DateTimeOffset.UtcNow
            });
        }
        catch
        {
            return Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Database connectivity check failed");
        }
    });

// ============================================================
// 15. START APPLICATION
// ============================================================

app.Run();

// Required for integration testing.
public partial class Program
{
}


//using System.Text;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using NaistringsAPI.Data;
//using NaistringsAPI.Managers;
//using NaistringsAPI.Models.Entities;
//using NaistringsAPI.Services.Implementations;
//using NaistringsAPI.Services.Interfaces;

//var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddIdentityCore<ApplicationUser>(options =>
//{
//    options.Password.RequiredLength = 12;
//    options.Password.RequireDigit = true;
//    options.Password.RequireUppercase = true;
//    options.Password.RequireLowercase = true;
//    options.Password.RequireNonAlphanumeric = true;
//    options.User.RequireUniqueEmail = true;
//    options.SignIn.RequireConfirmedEmail = true;
//    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
//    options.Lockout.MaxFailedAccessAttempts = 5;
//}).AddRoles<IdentityRole<Guid>>().AddSignInManager().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
//var jwt = builder.Configuration.GetSection("Jwt");
//var jwtKey = jwt["Key"] ?? throw new InvalidOperationException("Configure Jwt:Key using user-secrets or environment variables.");
//if (Encoding.UTF8.GetByteCount(jwtKey) < 32) throw new InvalidOperationException("Jwt:Key must contain at least 32 UTF-8 bytes.");
//var issuer = jwt["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is required.");
//var audience = jwt["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is required.");
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true, ValidIssuer = issuer,
//        ValidateAudience = true, ValidAudience = audience,
//        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
//        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
//        NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email,
//        RoleClaimType = System.Security.Claims.ClaimTypes.Role
//    };
//});


//builder.Services.AddAuthorization();
//builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<AuthManager>();
//builder.Services.AddCors(options => options.AddPolicy("BlazorFrontends", policy =>
//{
//    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
//    if (origins.Length > 0) policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();

//}));
//var app = builder.Build();
//// Apply the Identity migration before enabling role seeding.
//// Set Identity:SeedRoles=true for one startup after migrations have been applied.
//if (app.Configuration.GetValue<bool>("Identity:SeedRoles"))
//{
//    using var scope = app.Services.CreateScope();
//    await IdentityRoleSeeder.SeedRolesAsync(scope.ServiceProvider);
//}
//if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
//app.UseHttpsRedirection();
//app.UseCors("BlazorFrontends");
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();
//app.MapGet("/health", () => Results.Ok(new { status = "Healthy", application = "NaistringsAPI", utcTime = DateTimeOffset.UtcNow }));
//app.MapGet("/health/database", async (ApplicationDbContext db, CancellationToken ct) =>
//{
//    try { return await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "Connected", database = "Reachable" }) : Results.Problem(statusCode: 503, title: "Database not reachable"); }
//    catch { return Results.Problem(statusCode: 503, title: "Database connectivity check failed"); }
//});
//app.Run();
//public partial class Program { 

//}
