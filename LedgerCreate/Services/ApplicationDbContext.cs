using Microsoft.EntityFrameworkCore;

namespace LedgerCreate.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
    }
    
}
