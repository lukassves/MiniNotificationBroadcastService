using Microsoft.EntityFrameworkCore;
using Notification.Core.Interfaces;
using Notification.TcpServer.Actors;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// PostgreSQL DB
builder.Services.AddDbContext<Notification.Infrastructure.Data.NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));


// Custom services

builder.Services.AddScoped<INotificationStore, NotificationStore>(); // ✅ teisingai

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<INotifier, AkkaNotifier>();


// Build the app
var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// 🔥 LABAI SVARBU:
app.Run();
