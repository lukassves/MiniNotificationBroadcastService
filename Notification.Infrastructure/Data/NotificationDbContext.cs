using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Notification.Core.Interfaces;
using Notification.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Notification.Infrastructure.Data;



namespace Notification.Infrastructure.Data
{
    public class NotificationDbContext : DbContext
    {
        public DbSet<NotificationMessage> Notifications { get; set; }

        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationMessage>().HasKey(n => n.Id);
        }
    }

    public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=notifications;Username=postgres;Password=pass");

            return new NotificationDbContext(optionsBuilder.Options);
        }
    }
}

public class NotificationStore : INotificationStore
{
    private readonly NotificationDbContext _db;

    public NotificationStore(NotificationDbContext db)
    {
        _db = db;
    }

    public async Task SaveAsync(NotificationMessage notification)
    {
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }

    public async Task<List<NotificationMessage>> GetLastAsync(int limit)
    {
        return await _db.Notifications
            .OrderByDescending(n => n.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}

