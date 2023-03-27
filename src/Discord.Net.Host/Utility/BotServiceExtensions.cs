using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
namespace Discord.Net.Host.Utility;

public static class BotServiceExtensions {
    /// <summary>
    /// Setup the Discord bot.
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="token">Token of the Discord bot</param>
    /// <param name="config">DiscordSocketConfig to use to initialize <see cref="DiscordSocketClient"/></param>
    /// <returns>Bot service builder</returns>
    public static BotServiceBuilder AddDiscordBot(this IServiceCollection services, string token, DiscordSocketConfig? config = null) {
        return BotServiceBuilder.CreateAndInject(services, token, config);
    }
}
