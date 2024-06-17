using JWT.Entites;
using Microsoft.EntityFrameworkCore;

namespace JWT.Entities;

public class AppUserDbContext : DbContext
{
    
    public virtual DbSet<AppUser> AppUsers { get; set; }

    public AppUserDbContext()
    {
    }

    public AppUserDbContext(DbContextOptions<AppUserDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        optionsBuilder.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppUserDbContext).Assembly);
    }
}