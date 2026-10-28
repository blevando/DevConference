using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NaistringsAPI.Models.DTOs;
using NaistringsAPI.Models.Entities;
using NaistringsAPI.Services.Interfaces;

namespace NaistringsAPI.Services.Implementations;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, IConfiguration configuration)
    { _users = users; _signIn = signIn; _configuration = configuration; }

    public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email.Trim(), Email = request.Email.Trim(), DisplayName = request.DisplayName.Trim() };
        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded) return result;
        var roleResult = await _users.AddToRoleAsync(user, "CommunityMember");
        return roleResult;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.EmailConfirmed) return null;
        var check = await _signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!check.Succeeded) return null;
        var roles = (await _users.GetRolesAsync(user)).ToArray();
        var jwt = _configuration.GetSection("Jwt");
        var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
        if (Encoding.UTF8.GetByteCount(key) < 32) throw new InvalidOperationException("Jwt:Key must contain at least 32 UTF-8 bytes.");
        var issuer = jwt["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is required.");
        var audience = jwt["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is required.");
        var expiry = DateTimeOffset.UtcNow.AddMinutes(15);
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(JwtRegisteredClaimNames.Email, user.Email!), new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiry.UtcDateTime, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expiry, user.Email!, user.DisplayName, roles);
    }
}
