using Discord.Net.Host.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(builder => {
        builder.AddCommandLine(args);
    })
    .ConfigureAppConfiguration((host, conf) => {
        conf.AddEnvironmentVariables();
        conf.AddUserSecrets<Program>();
    })
    .ConfigureServices((host, services) => {
        var discordToken = host.Configuration["Discord:Token"]!;
        
        services.AddDiscordBot(discordToken)
            .AddInteractionModules<Program>()
            .RegisterCommandsGloballyOnStartup();
    })
    .Build();

await host.RunAsync();
