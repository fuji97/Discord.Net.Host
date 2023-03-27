using System.Reflection;
using Discord.Interactions;
using Discord.Net.Host.Utility;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Discord.Net.Host; 

public class BotService : IHostedService {
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<BotService> _logger;
    private readonly BotServiceBuilder _builder;
    private readonly IServiceProvider _serviceProvider;
    
    private DiscordSocketClient? _client = null!;

    public BotService(ILoggerFactory loggerFactory, BotServiceBuilder builder, IServiceProvider serviceProvider) {
        _loggerFactory = loggerFactory;
        _builder = builder;
        _serviceProvider = serviceProvider;

        _logger = _loggerFactory.CreateLogger<BotService>();
    }

    private static Func<LogMessage, Task> LogMessage(ILogger logger) {
        return (log) => {
            logger.LogMessage(log);
            return Task.CompletedTask;
        };
    }

    private async Task RunBot(CancellationToken cancellationToken) {
        _client = await _builder.Build(_serviceProvider, _loggerFactory);

        // Start bot
        await _client.LoginAsync(TokenType.Bot, _builder.Token);
        await _client.StartAsync();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Application started");

        await RunBot(cancellationToken);
    }
    public async Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Application stopped");

        if (_client != null) await _client.StopAsync();
    }
}