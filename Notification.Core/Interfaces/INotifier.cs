namespace Notification.Core.Interfaces;

public interface INotifier
{
    Task NotifyAsync(string message);
}