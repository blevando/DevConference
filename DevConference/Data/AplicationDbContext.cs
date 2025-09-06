using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevConference.Shared.Models.Entities;

namespace DevConferenceAPI.Data
{
    public class AplicationDbContext : DbContext
    {
        public AplicationDbContext (DbContextOptions<AplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DevConference.Shared.Models.Entities.Conferences> Conferences { get; set; } = default!;
    }
}
