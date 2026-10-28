
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NaistringsAPI.Models.Entities;

namespace NaistringsAPI.Data
{
    public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Default SQL Server schema for all application tables.
            modelBuilder.HasDefaultSchema("dbo");

            // Additional ApplicationUser configuration.
            modelBuilder.Entity<ApplicationUser>()
                .Property(x => x.DisplayName)
                .HasMaxLength(150);
        }
    }
}
