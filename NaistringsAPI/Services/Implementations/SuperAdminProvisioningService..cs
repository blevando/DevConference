




using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NaistringsAPI.Models.Entities;
using NaistringsAPI.Services.Interfaces;

namespace NaistringsAPI.Services.Implementations
{

    public sealed class SuperAdminProvisioningService : ISuperAdminProvisioningService
    {
        private const string RoleName = "SuperAdmin";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SuperAdminProvisioningService> _logger;

        public SuperAdminProvisioningService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration,
            ILogger<SuperAdminProvisioningService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ProvisionAsync()
        {
            if (!_configuration.GetValue<bool>(
                    "Identity:ProvisionSuperAdmin"))
            {
                return;
            }

            // Provisioning must be performed in a controlled
            // environment and must never be a public API action.

            if (!await _roleManager.RoleExistsAsync(RoleName))
            {
                throw new InvalidOperationException(
                    "SuperAdmin role does not exist. Seed roles first.");
            }

            var existingAdmins =
                await _userManager.GetUsersInRoleAsync(RoleName);

            if (existingAdmins.Count > 0)
            {
                throw new InvalidOperationException(
                    "SuperAdmin provisioning refused: " +
                    "a SuperAdmin account already exists.");
            }

            var email = _configuration[
                "Identity:SuperAdmin:Email"]?.Trim();

            var displayName = _configuration[
                "Identity:SuperAdmin:DisplayName"]?.Trim();

            var password = _configuration[
                "Identity:SuperAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "SuperAdmin Email, DisplayName and Password " +
                    "must be configured.");
            }

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "SuperAdmin provisioning refused: " +
                    "the configured email already belongs to a user.");
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                DisplayName = displayName,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult =
                await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "SuperAdmin creation failed: " +
                    string.Join("; ",
                        createResult.Errors.Select(e => e.Description)));
            }

            var roleResult =
                await _userManager.AddToRoleAsync(user, RoleName);

            if (!roleResult.Succeeded)
            {
                // Compensating action for a failed role assignment.
                var deleteResult =
                    await _userManager.DeleteAsync(user);

                if (!deleteResult.Succeeded)
                {
                    _logger.LogCritical(
                        "Failed to remove partially provisioned " +
                        "SuperAdmin account {UserId}.",
                        user.Id);
                }

                throw new InvalidOperationException(
                    "SuperAdmin role assignment failed: " +
                    string.Join("; ",
                        roleResult.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation(
                "Initial SuperAdmin account provisioned. UserId: {UserId}",
                user.Id);
        }
    }
}