using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Notification.Core.Interfaces;
using Notification.Core.Models;
using Notification.WebApi;
using Xunit;

public class NotificationsControllerTests
{
    [Fact]
    public async Task Post_CallsNotifierAndSavesToStore()
    {
        var storeMock = new Mock<INotificationStore>();
        var notifierMock = new Mock<INotifier>();

        var controller = new NotificationsController(storeMock.Object, notifierMock.Object);
        var payload = "test123";

        var result = await controller.Post(payload);

        storeMock.Verify(s => s.SaveAsync(It.IsAny<NotificationMessage>()), Times.Once);
        notifierMock.Verify(n => n.NotifyAsync("test123"), Times.Once);
        Assert.IsType<OkResult>(result);
    }
}
