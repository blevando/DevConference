using Microsoft.EntityFrameworkCore;

namespace Naistring.Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
             : base(options)
        {
        }
       // public DbSet<Naistring> Naistrings => Set<Naistring>(); //
    }
}
