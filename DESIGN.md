## 📘 Design Note

We use a **single actor (`NotiActor`)** instead of spawning one actor per connection to keep the architecture simple and centralized. Since the system maintains a single shared channel (not multi-room), all messages are handled by one actor that maintains the active TCP sessions and is responsible for broadcasting. This avoids unnecessary overhead of supervising and messaging between thousands of per-connection actors.

In a production-grade system, per-connection actors would make sense if each client needed isolated processing state (e.g. rate limiting, authentication, individual queues). But for a single-room broadcast system, one actor is optimal.

To handle **back-pressure**, bounded mailboxes could be introduced, or messages could be dropped, batched, or throttled based on traffic. Akka.NET provides back-pressure-aware streaming tools that could be used if needed. The actor can also reject messages when load is too high.

For **failure handling**, Akka.NET provides supervision strategies. Our `NotiActor` would be restarted on failure, and state (like connected sessions) could be restored. Critical failures could escalate to a higher-level supervisor. Messages are also persisted in PostgreSQL, so even if the actor crashes, no data is lost.

For high availability, a distributed actor system or pub/sub (e.g. Redis or Kafka) could replace the single actor.
