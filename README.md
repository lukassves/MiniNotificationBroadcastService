# Mini Notification Broadcast Service

This is a .NET Core-based notification broadcast system using TCP, REST API, PostgreSQL, and Akka.NET actors. Clients can send `NOTIFY:` messages via TCP – messages are stored in the database and broadcasted to all connected TCP clients. REST API allows reading and posting notifications.

---

## 🏗️ Build & Run Instructions

### 1. Start PostgreSQL

**Option A – Docker:**
```bash
docker run --name pg-notify -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=notifications -p 5432:5432 -d postgres
Option B – Local PostgreSQL:
Create DB notifications
Username: postgres, Password: postgres

Update appsettings.json in both Notification.WebApi and Notification.Infrastructure:

json
Copy
Edit
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=notifications;Username=postgres;Password=postgres"
}
2. Run EF Core migrations
bash
Copy
Edit
dotnet ef database update --project Notification.Infrastructure
3. Run Web API
bash
Copy
Edit
dotnet run --project Notification.WebApi
Endpoints:

GET https://localhost:5001/api/notifications?limit=10

POST https://localhost:5001/api/notifications

4. Run TCP Server
bash
Copy
Edit
dotnet run --project Notification.TcpServer
Server listens on port 9000.

🧪 Sample TCP Client
Using telnet:
bash
Copy
Edit
telnet localhost 9000
NOTIFY:Hello from telnet
📘 Design Note
We use a single actor (NotiActor) for all messages. This keeps the logic centralized and avoids actor-to-actor messaging overhead. Since the app uses a single channel and shared state (broadcast to all), there's no need for per-connection actors.

In production, we would consider per-connection actors if clients had isolated state or load balancing needs. To handle back-pressure, we'd implement bounded mailboxes, message rate throttling, or stream-based input. Actor failures are managed via Akka.NET’s built-in supervision: the actor is restarted or escalated based on error type.

For full resilience: externalize actor logic (e.g. Kafka), use horizontally scalable services, and isolate persistence failures from live sessions.

🔁 Sequence Diagram (text form)
css
Copy
Edit
TCP Client
    ↓
TCP Server (Console App)
    ↓
Akka.NET Actor (NotiActor)
   → Save message to PostgreSQL
   → Broadcast to all connected TCP clients
   → REST API exposes data from DB
📦 Project Structure
Notification.Core – Models & interfaces

Notification.Infrastructure – EF Core, DbContext, Store

Notification.TcpServer – TCP listener, actor pipeline

Notification.WebApi – REST endpoints (GET/POST)

✅ Technologies
.NET 7+

ASP.NET Core Web API

TCP sockets

Akka.NET actors

EF Core + PostgreSQL