using AutoPartsHub.BLL.RegistratorServices;
using AutoPartsHub.DAL.Registrator;
using AutoPartsHub.TelegramBot;
using AutoPartsHub.TelegramBot.Registrator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Generic Host собирает конфигурацию из appsettings, переменных окружения и
// аргументов запуска, а также настраивает логирование и корректное завершение.
var builder = Host.CreateApplicationBuilder(args);

// Как и в DeliveryApp, каждый слой сам знает, какие его реализации нужно
// зарегистрировать. Program остаётся composition root: он определяет только
// порядок сборки приложения и не знает конкретные классы репозиториев и сервисов.
// Порядок отражает направление зависимостей для чтения кода: сначала хранилище,
// затем использующая его бизнес-логика, после этого внешний интерфейс бота.
builder.Services
    .AddDatabase(builder.Configuration)
    .AddBusinessLogic()
    .AddBotPresentation(builder.Configuration);

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
