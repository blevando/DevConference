using Microsoft.AspNetCore.Identity;
using NaistringsAPI.Models.DTOs;
using NaistringsAPI.Services.Interfaces;

namespace NaistringsAPI.Managers;

public sealed class AuthManager
{
    private readonly IAuthService _auth;
    public AuthManager(IAuthService auth) => _auth = auth;
    public Task<IdentityResult> RegisterAsync(RegisterRequest request) => _auth.RegisterAsync(request);
    public Task<AuthResponse?> LoginAsync(LoginRequest request) => _auth.LoginAsync(request);
}
