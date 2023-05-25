using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {            
        }

        public DbSet<ContractBasicInfo> ContractBasicInfo { get; set; }
        public DbSet<LaborCategory> LaborCategory { get; set; }
    }
}
