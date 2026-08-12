using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Предоставляет консольный интерфейс с теми же командами, что и Telegram-бот.
/// </summary>
/// <param name="scopeFactory">Фабрика областей зависимостей для обработки команд.</param>
/// <param name="options">Настройки Telegram и консольного режима.</param>
/// <param name="lifetime">Управление временем жизни приложения.</param>
public sealed class ConsoleBotWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TelegramOptions> options,
    IHostApplicationLifetime lifetime) : BackgroundService
{
    /// <summary>
    /// Читает команды из стандартного ввода до отмены или команды <c>/exit</c>.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.EnableConsole)
            return;

        Console.WriteLine("AutoParts Hub запущен в консольном режиме.");
        Console.WriteLine("Введите /help для списка команд, /exit — для выхода.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var command = await Console.In.ReadLineAsync(stoppingToken);
            if (command is null || string.Equals(command.Trim(), "/exit", StringComparison.OrdinalIgnoreCase))
            {
                lifetime.StopApplication();
                return;
            }

            // Scoped-сервисы и DbContext создаются заново для каждой команды.
            await using var scope = scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<BotCommandHandler>();
            var response = await handler.HandleAsync(
                options.Value.ConsoleChatId,
                "Консольный пользователь",
                command,
                stoppingToken);
            Console.WriteLine(response);
        }
    }
}
