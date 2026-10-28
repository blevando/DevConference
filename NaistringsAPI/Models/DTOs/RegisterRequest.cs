using System.ComponentModel.DataAnnotations;

namespace NaistringsAPI.Models.DTOs;

public sealed class RegisterRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(12)] public string Password { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string DisplayName { get; set; } = string.Empty;
}
