# Discord.Net Generic Host support
<p align="center">
    <img src="https://img.shields.io/github/license/fuji97/Discord.Net.Host" alt="License">
  <a href="https://www.nuget.org/packages/Discord.Net.Host">
    <img src="https://img.shields.io/nuget/v/Discord.Net.Host" alt="NuGet">
  </a>
</p>
Implementation to use <a href="https://github.com/discord-net/Discord.Net">Discord.Net</a> with the `IHost` Generic Host.

## 📥 Installation
Builds are available on NuGet: https://www.nuget.org/packages/Discord.Net.Host

```shell
dotnet add package Discord.Net.Host
```

## 📖 Usage
Example of a simple bot that replies to the `/hello` command with `Hello world!`.

### Program.cs
```csharp
using Discord.Net.Host.Utility;
using Microsoft.Extensions.Hosting;

var token = "<DISCORD-BOT-TOKEN>";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((host, services) => {
        services.AddDiscordBot(token)
            .AddInteractionModules<Program>()
            .RegisterCommandsGloballyOnStartup();
    })
    .Build();

await host.RunAsync();

```

### Modules/HelloWorldModule.cs
```csharp
using Discord.Interactions;
using Microsoft.Extensions.Logging;

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
```

## 📦 Docker
An example of a linux compatible Dockerfile is available in the sample project.