namespace NaistringsAPI.Models.DTOs;

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, string Email, string DisplayName, string[] Roles);
