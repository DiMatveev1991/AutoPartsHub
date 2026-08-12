using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AutoPartsHub.TelegramBot.Telegram;

public sealed class ConsoleBotWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TelegramOptions> options,
    IHostApplicationLifetime lifetime) : BackgroundService
{
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
