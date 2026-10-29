using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NaistringsAPI.Data;
using NaistringsAPI.Managers;
using NaistringsAPI.Models.Entities;
using NaistringsAPI.Services.Implementations;
using NaistringsAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. API controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Naistrings API",
        Version = "v1",
        Description = "Naistrings Software Developer Conference API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the JWT access token only (without the Bearer prefix).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 2. Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is required."));
});

// 3. ASP.NET Core Identity
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
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddSignInManager()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 4. JWT authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("Jwt:Key is required. Configure it using user-secrets.");
var jwtIssuer = jwtSection["Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is required.");
var jwtAudience = jwtSection["Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is required.");

byte[] jwtKeyBytes;
try
{
    jwtKeyBytes = Convert.FromBase64String(jwtKey);
}
catch (FormatException ex)
{
    throw new InvalidOperationException("Jwt:Key must be a valid Base64-encoded value.", ex);
}

if (jwtKeyBytes.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must contain at least 32 decoded bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Email,
            RoleClaimType = ClaimTypes.Role
        };

        // Temporary local-development diagnostics. Remove after resolving the 401.
        if (builder.Environment.IsDevelopment())
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    Console.WriteLine("JWT Authorization header present: " +
                        context.Request.Headers.ContainsKey("Authorization"));
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine("JWT authentication failed: " +
                        context.Exception.GetType().Name);
                    Console.WriteLine(context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("JWT token successfully validated.");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Console.WriteLine("JWT challenge: " +
                        (context.Error ?? "Unauthorized"));
                    Console.WriteLine("JWT challenge description: " +
                        (context.ErrorDescription ?? "Not supplied"));
                    return Task.CompletedTask;
                }
            };
        }
    });

// 5. Authorization
builder.Services.AddAuthorization();

// 6. Application services and managers
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISuperAdminProvisioningService, SuperAdminProvisioningService>();
builder.Services.AddScoped<AuthManager>();

// 7. CORS for Blazor frontends
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorFrontends", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// 8. Initial role seeding and SuperAdmin provisioning
// Both flags should normally be false after initial setup.
if (app.Configuration.GetValue<bool>("Identity:SeedRoles"))
{
    using var scope = app.Services.CreateScope();
    await IdentityRoleSeeder.SeedRolesAsync(scope.ServiceProvider);
}

if (app.Configuration.GetValue<bool>("Identity:ProvisionSuperAdmin"))
{
    using var scope = app.Services.CreateScope();
    var provisioningService = scope.ServiceProvider
        .GetRequiredService<ISuperAdminProvisioningService>();
    await provisioningService.ProvisionAsync();
}

// 9. HTTP middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Naistrings API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("BlazorFrontends");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 10. Health checks
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    application = "NaistringsAPI",
    utcTime = DateTimeOffset.UtcNow
}));

app.MapGet("/health/database", async (ApplicationDbContext db, CancellationToken ct) =>
{
    try
    {
        if (!await db.Database.CanConnectAsync(ct))
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

app.Run();

// Required for integration testing.
public partial class Program
{
}
