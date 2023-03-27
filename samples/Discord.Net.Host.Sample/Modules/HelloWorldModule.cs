using Discord.Interactions;
using Microsoft.Extensions.Logging;
namespace Discord.Net.Host.Sample.Modules; 

public class HelloWorldModule : InteractionModuleBase {
    private readonly ILogger<HelloWorldModule> _logger;
    
    public HelloWorldModule(ILogger<HelloWorldModule> logger) {
        _logger = logger;
    }
    
    [SlashCommand("hello", "Send an hello world message.")]
    public async Task HelloWorldAsync() {
        _logger.LogInformation("Sending Hello world!");
        
        await RespondAsync("Hello world!");
    }
}
