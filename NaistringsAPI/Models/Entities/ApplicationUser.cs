using Microsoft.AspNetCore.Identity;

namespace NaistringsAPI.Models.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

}
