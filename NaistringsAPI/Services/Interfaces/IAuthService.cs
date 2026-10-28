using Microsoft.AspNetCore.Identity;
using NaistringsAPI.Models.DTOs;
using NaistringsAPI.Models.Entities;

namespace NaistringsAPI.Services.Interfaces
{
public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
}