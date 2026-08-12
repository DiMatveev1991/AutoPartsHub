using AutoPartsHub.BLL;
using AutoPartsHub.Core;
using AutoPartsHub.DAL;
using AutoPartsHub.TelegramBot;
using AutoPartsHub.TelegramBot.Background;
using AutoPartsHub.TelegramBot.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAutoPartsHubDal(builder.Configuration);
builder.Services.Configure<TelegramOptions>(
    builder.Configuration.GetSection(TelegramOptions.SectionName));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<BotCommandHandler>();
builder.Services.AddScoped<INotificationSender, TelegramNotificationSender>();

builder.Services.AddSingleton<TelegramUpdateHandler>();
builder.Services.AddHostedService<TelegramBotWorker>();
builder.Services.AddHostedService<ConsoleBotWorker>();
builder.Services.AddHostedService<NotificationWorker>();

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Database");
await DbSeeder.InitializeAsync(
    host.Services,
    builder.Configuration,
    logger,
    CancellationToken.None);
await host.RunAsync();

public partial class Program;
