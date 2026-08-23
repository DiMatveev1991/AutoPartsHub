using System.Net;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Создаёт один Telegram-клиент с общим HTTP-соединением и настройками прокси.
/// </summary>
public sealed class TelegramBotClientProvider : IDisposable
{
    private readonly HttpClient? httpClient;

    /// <summary>
    /// Инициализирует клиент только при наличии Telegram-токена.
    /// </summary>
    public TelegramBotClientProvider(
        IOptions<TelegramOptions> telegramOptions,
        IOptions<ProxyOptions> proxyOptions)
    {
        var token = telegramOptions.Value.BotToken;
        if (string.IsNullOrWhiteSpace(token))
            return;

        if (!proxyOptions.Value.UseProxy)
        {
            Client = new TelegramBotClient(token);
            return;
        }

        var proxyUri = ParseProxyUri(proxyOptions.Value.Url);
        var proxy = new WebProxy(proxyUri);
        if (!string.IsNullOrWhiteSpace(proxyOptions.Value.Username))
        {
            proxy.Credentials = new NetworkCredential(
                proxyOptions.Value.Username,
                proxyOptions.Value.Password);
        }

        var handler = new SocketsHttpHandler
        {
            Proxy = proxy,
            UseProxy = true,
            PooledConnectionLifetime = TimeSpan.FromMinutes(3)
        };
        httpClient = new HttpClient(handler, disposeHandler: true);
        Client = new TelegramBotClient(token, httpClient);
        UsesProxy = true;
        ProxyDisplayName = proxyUri.GetLeftPart(UriPartial.Authority);
    }

    /// <summary>Получает общий Telegram-клиент или null без токена.</summary>
    public ITelegramBotClient? Client { get; }

    /// <summary>Указывает, что клиент направляет запросы через прокси.</summary>
    public bool UsesProxy { get; }

    /// <summary>Получает безопасное описание прокси без логина и пароля.</summary>
    public string? ProxyDisplayName { get; }

    /// <inheritdoc />
    public void Dispose() => httpClient?.Dispose();

    private static Uri ParseProxyUri(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "Для включённого прокси задайте абсолютный HTTP/HTTPS-адрес Proxy:Url.");
        }

        return uri;
    }
}
