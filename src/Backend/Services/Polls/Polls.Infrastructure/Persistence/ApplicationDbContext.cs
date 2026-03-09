using Microsoft.EntityFrameworkCore;
using Polls.Domain.Cities;
using Polls.Domain.Ideas;
using Polls.Domain.Polls;

namespace Polls.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<City> Cities => Set<City>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<Idea> Ideas => Set<Idea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
