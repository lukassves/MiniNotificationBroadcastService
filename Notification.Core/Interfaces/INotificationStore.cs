using Notification.Core.Models;

namespace Notification.Core.Interfaces;

public interface INotificationStore
{
    Task SaveAsync(NotificationMessage notification);
    Task<List<NotificationMessage>> GetLastAsync(int limit);
}
