namespace Notification.TcpServer.Actors
{
    using Akka.Actor;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Notification.Core.Models;
    using System.Threading.Tasks;
    using Notification.Core.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Notification.Core;
    using Notification.Infrastructure.Data; // ← būtinai
    #region ─── Actors & Messages ──────────────────────────────────────────────

    public record RegisterClient(IActorRef Client);
    public record UnregisterClient(IActorRef Client);
    public record NewNotification(string Text);


    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        public DbSet<NotificationMessage> Notifications => Set<NotificationMessage>();
    }

    public class NotiActor : ReceiveActor
    {
        private readonly List<IActorRef> _clients = new();
        private readonly List<string> _messages = new();
        private readonly NotificationDbContext _db;

        public NotiActor(NotificationDbContext db)
        {
            _db = db;

            Receive<RegisterClient>(msg => _clients.Add(msg.Client));
            Receive<UnregisterClient>(msg => _clients.Remove(msg.Client));

            ReceiveAsync<NewNotification>(async msg =>
            {
                _messages.Add(msg.Text);
                Console.WriteLine($"Gauta žinutė: {msg.Text}");

                // Save to DB
                var entity = new NotificationMessage
                {
                    Text = msg.Text,
                    Timestamp = DateTime.UtcNow
                };

                _db.Notifications.Add(entity);
                await _db.SaveChangesAsync();

                // Broadcast to all clients
                foreach (var client in _clients)
                {
                    client.Tell(msg.Text);
                }
            });
        }
    }


    public class SupervisorActor : ReceiveActor
    {
        public static IActorRef? NotiActorRef;

        public SupervisorActor(NotificationDbContext db)
        {
            NotiActorRef = Context.ActorOf(Props.Create(() => new NotiActor(db)), "notification-actor");
        }
    }


    #endregion

    #region ─── TCP Server ─────────────────────────────────────────────────────

    public class TcpServer
    {
        private readonly int _port = 9000;

        public async Task StartAsync(Func<string, Task> onMessageReceived)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            Console.WriteLine($"TCP server listening on port {_port}...");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client, onMessageReceived);
            }
        }

        private async Task HandleClientAsync(TcpClient client, Func<string, Task> onMessageReceived)
        {
            using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("NOTIFY:"))
                {
                    var message = line.Substring("NOTIFY:".Length).Trim();
                    await onMessageReceived(message);
                }
            }
        }
    }

    #endregion

    #region ─── Akka Notifier ──────────────────────────────────────────────────

    public class AkkaNotifier : INotifier
    {
        public Task NotifyAsync(string text)
        {
            SupervisorActor.NotiActorRef?.Tell(new NewNotification(text));
            return Task.CompletedTask;
        }
    }

    #endregion

    #region ─── Main Program ───────────────────────────────────────────────────

    class Program
    {
        static async Task Main(string[] args)
        {
            // 🔧 Create DbContext manually
            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseNpgsql("Host=localhost;Database=notification;Username=postgres;Password=YOUR_PASSWORD")
                .Options;

            var dbContext = new NotificationDbContext(options);

            // 🎭 Create ActorSystem and Supervisor with dbContext
            var actorSystem = ActorSystem.Create("MySystem");
            var supervisor = actorSystem.ActorOf(Props.Create(() => new SupervisorActor(dbContext)), "supervisor");

            // 🌐 Start TCP server and forward messages to NotiActor
            var tcpServer = new TcpServer();
            await tcpServer.StartAsync(async message =>
            {
                if (SupervisorActor.NotiActorRef != null)
                    SupervisorActor.NotiActorRef.Tell(new NewNotification(message));
            });

            Console.WriteLine("System running. Press Enter to exit.");
            Console.ReadLine();
        }
    }

    #endregion
}