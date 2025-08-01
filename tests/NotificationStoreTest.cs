using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Notification.Core.Models;
using Notification.Infrastructure.Data;
using Xunit;


public class NotificationStoreTests
{
    [Fact]
    public async Task SaveAsync_SavesNotificationToDb()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase("TestDb_SaveAsync").Options;

        var context = new NotificationDbContext(options);
        var store = new NotificationStore(context);

        var notification = new NotificationMessage { Text = "Test message" };

        // Act
        await store.SaveAsync(notification);

        // Assert
        var saved = await context.Notifications.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("Test message", saved.Text);
    }
}
