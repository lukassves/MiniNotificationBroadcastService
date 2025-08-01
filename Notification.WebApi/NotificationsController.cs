using Microsoft.AspNetCore.Mvc;
using Notification.Core.Interfaces;
using Notification.Core.Models;
namespace Notification.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationStore _store;
        private readonly INotifier _notifier;

        public NotificationsController(INotificationStore store, INotifier notifier)
        {
            _store = store;
            _notifier = notifier;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int limit = 10)
        {
            var messages = await _store.GetLastAsync(limit);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string text)
        {
            var message = new Notification.Core.Models.NotificationMessage { Text = text };
            await _store.SaveAsync(message);
            await _notifier.NotifyAsync(text);
            return Ok();
        }
    }
}
