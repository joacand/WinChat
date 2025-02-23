using Microsoft.EntityFrameworkCore;

namespace WinChat.Infrastructure.Repository;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationData> ApplicationData { get; set; }
    public DbSet<ChatMessageEntry> ChatMessages { get; set; }
}
