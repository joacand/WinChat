using Microsoft.EntityFrameworkCore;

namespace WinChat.Infrastructure.Repository;

public class AppDbContext : DbContext
{
    public DbSet<ApplicationData> ApplicationData { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }
}

public class ApplicationData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
}
