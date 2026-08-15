using AutoPartsHub.BLL;
using AutoPartsHub.Models;
using AutoPartsHub.DAL;
using AutoPartsHub.TelegramBot;
using AutoPartsHub.TelegramBot.Background;
using AutoPartsHub.TelegramBot.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Generic Host собирает конфигурацию из appsettings, переменных окружения и
// аргументов запуска, а также настраивает логирование и корректное завершение.
var builder = Host.CreateApplicationBuilder(args);

// DAL скрывает настройку Npgsql и регистрирует DbContext как Scoped: один
// контекст используется в рамках одной команды, но не разделяется между потоками.
builder.Services.AddAutoPartsHubDal(builder.Configuration);

// Настройки привязываются через Options pattern, чтобы классы Telegram-слоя
// не зависели напрямую от IConfiguration и строковых ключей.
builder.Services.Configure<TelegramOptions>(
    builder.Configuration.GetSection(TelegramOptions.SectionName));

// Бизнес-сервисы и обработчик команд имеют Scoped lifetime, потому что они
// используют Scoped-репозиторий и DbContext. Отдельный scope создаётся для
// каждой Telegram-команды, консольной команды и итерации фоновой обработки.
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<BotCommandHandler>();

// Отправщик не хранит изменяемого состояния и зависит только от потокобезопасных
// IOptions и ILogger, поэтому один Singleton-экземпляр безопасно переиспользуется.
builder.Services.AddSingleton<INotificationSender, TelegramNotificationSender>();

// Telegram.Bot переиспользует один callback-обработчик. Сам обработчик не хранит
// DbContext: на каждое обновление он создаёт новый scope, поэтому Singleton безопасен.
builder.Services.AddSingleton<TelegramUpdateHandler>();

// Hosted services сами являются Singleton. Scoped-зависимости нельзя захватывать
// в их конструкторах, поэтому внутри workers используется IServiceScopeFactory.
builder.Services.AddHostedService<TelegramBotWorker>();
builder.Services.AddHostedService<ConsoleBotWorker>();
builder.Services.AddHostedService<NotificationWorker>();

var host = builder.Build();

// Схема и демонстрационные данные готовятся до запуска workers, чтобы ни одна
// команда не обратилась к ещё не мигрированной базе. Ошибка старта завершает
// приложение сразу, вместо запуска частично работоспособного бота.
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Database");
await DbSeeder.InitializeAsync(
    host.Services,
    builder.Configuration,
    logger,
    CancellationToken.None);

// RunAsync удерживает приложение и передаёт сигнал остановки всем BackgroundService.
await host.RunAsync();

/// <summary>
/// Служит точкой привязки для интеграционных тестов и средств запуска приложения.
/// </summary>
public partial class Program;
