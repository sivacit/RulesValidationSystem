using Microsoft.EntityFrameworkCore;
using RulesValidationSystem.Model;

namespace RulesValidationSystem.Data
{
    public class RulesDbContext : DbContext
    {
        public RulesDbContext(DbContextOptions<RulesDbContext> options) : base(options) { }

        public DbSet<RulesWorkflow> RulesWorkflows { get; set; }
    }
}
