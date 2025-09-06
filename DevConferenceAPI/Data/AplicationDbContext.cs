
using DevConference.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace DevConferenceAPI.Data
{
    public class AplicationDbContext: DbContext
    {
        public AplicationDbContext( DbContextOptions options) : base(options)
        {
                
        }

        public DbSet<Conferences> Conferences { get; set; }
    }
}
