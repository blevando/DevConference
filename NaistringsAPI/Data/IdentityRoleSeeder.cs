using Microsoft.AspNetCore.Identity;

namespace NaistringsAPI.Data;

public static class IdentityRoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var manager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var name in new[] { "SuperAdmin", "Admin", "EventManager", "Speaker", "CommunityMember" })
            if (!await manager.RoleExistsAsync(name))
            {
                var result = await manager.CreateAsync(new IdentityRole<Guid>(name));
                if (!result.Succeeded) throw new InvalidOperationException($"Failed to seed role {name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
    }
}
