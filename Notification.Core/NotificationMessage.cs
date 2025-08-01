using Notification.Core.Models;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Notification.Core.Models
{
    public class NotificationMessage
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

