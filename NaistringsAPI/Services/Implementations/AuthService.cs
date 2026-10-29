
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NaistringsAPI.Models.DTOs;
using NaistringsAPI.Models.Entities;
using NaistringsAPI.Services.Interfaces;

namespace NaistringsAPI.Services.Implementations
{
    public sealed class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn,
            IConfiguration configuration)
        {
            _users = users;
            _signIn = signIn;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterAsync(
            RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                DisplayName = request.DisplayName.Trim()
            };

            var result = await _users.CreateAsync(
                user,
                request.Password);

            if (!result.Succeeded)
            {
                return result;
            }

            var roleResult = await _users.AddToRoleAsync(
                user,
                "CommunityMember");

            if (!roleResult.Succeeded)
            {
                // Roll back the account if role assignment fails.
                var rollbackResult = await _users.DeleteAsync(user);

                if (!rollbackResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Registration role assignment failed, " +
                        "and the newly created account could not be rolled back.");
                }

                return roleResult;
            }

            return IdentityResult.Success;
        }

        public async Task<AuthResponse?> LoginAsync(
            LoginRequest request)
        {
            // ----------------------------------------------------
            // 1. Retrieve user
            // ----------------------------------------------------

            var user = await _users.FindByEmailAsync(
                request.Email.Trim());

            if (user is null || !user.EmailConfirmed)
            {
                return null;
            }

            // ----------------------------------------------------
            // 2. Validate password and account lockout
            // ----------------------------------------------------

            var check = await _signIn.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!check.Succeeded)
            {
                return null;
            }

            // ----------------------------------------------------
            // 3. Retrieve assigned Identity roles
            // ----------------------------------------------------

            var roles = (await _users.GetRolesAsync(user))
                .ToArray();

            // ----------------------------------------------------
            // 4. Retrieve JWT configuration
            // ----------------------------------------------------

            var jwt = _configuration.GetSection("Jwt");

            var key = jwt["Key"]
                ?? throw new InvalidOperationException(
                    "Jwt:Key is required.");

            var issuer = jwt["Issuer"]
                ?? throw new InvalidOperationException(
                    "Jwt:Issuer is required.");

            var audience = jwt["Audience"]
                ?? throw new InvalidOperationException(
                    "Jwt:Audience is required.");

            // ----------------------------------------------------
            // 5. Decode Base64 JWT signing key
            //    Must match Program.cs validation configuration
            // ----------------------------------------------------

            byte[] keyBytes;

            try
            {
                keyBytes = Convert.FromBase64String(key);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException(
                    "Jwt:Key must be a valid Base64-encoded value.",
                    ex);
            }

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:Key must contain at least 32 decoded bytes.");
            }

            // ----------------------------------------------------
            // 6. Set JWT expiration
            // ----------------------------------------------------

            var expiry = DateTimeOffset.UtcNow.AddMinutes(15);

            // ----------------------------------------------------
            // 7. Build JWT claims
            // ----------------------------------------------------

            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                new Claim(
                    JwtRegisteredClaimNames.Email,
                    user.Email!),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            // Include all assigned Identity roles.
            claims.AddRange(
                roles.Select(role =>
                    new Claim(ClaimTypes.Role, role)));

            // ----------------------------------------------------
            // 8. Generate signing credentials
            // ----------------------------------------------------

            var signingKey = new SymmetricSecurityKey(
                keyBytes);

            var signingCredentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

            // ----------------------------------------------------
            // 9. Generate JWT access token
            // ----------------------------------------------------

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry.UtcDateTime,
                signingCredentials: signingCredentials);

            var accessToken = new JwtSecurityTokenHandler()
                .WriteToken(token);

            // ----------------------------------------------------
            // 10. Return authentication response
            // ----------------------------------------------------

            return new AuthResponse(
                accessToken,
                expiry,
                user.Email!,
                user.DisplayName,
                roles);
        }
    }
}
