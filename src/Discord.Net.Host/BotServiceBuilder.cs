using System.Reflection;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net.Host.Utility;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
namespace Discord.Net.Host;

public class BotServiceBuilder {
    internal readonly string Token;
    private readonly IServiceCollection _services;
    
    private Func<DiscordSocketClient, IServiceProvider, Task>? _configureDelegate;

    private List<Assembly> _interactionModuleAssemblies = new List<Assembly>();
    private List<Type> _interactionModuleTypes = new List<Type>();

    private List<Assembly> _commandModuleAssemblies = new List<Assembly>();
    private List<Type> _commandModuleTypes = new List<Type>();
    
    private bool _registerCommandsGloballyOnStartup = false;
    private List<ulong> _registerCommandsToGuildsOnStartup = new List<ulong>();

    private Func<DiscordSocketClient, IServiceProvider, Task>? _onReadyDelegate = null;
    
    private Func<InteractionService, IServiceProvider, Task>? _interactionServiceConfigurator;
    private Func<CommandService, IServiceProvider, Task>? _commandServiceConfigurator;

    private bool IsInteractionServiceRegistered => _interactionModuleAssemblies.Count > 0 || _interactionModuleTypes.Count > 0;
    private bool IsCommandServiceRegistered => _commandModuleAssemblies.Count > 0 || _commandModuleTypes.Count > 0;
    
    private string _prefix = "!";


    public static BotServiceBuilder CreateAndInject(IServiceCollection services, string token, DiscordSocketConfig? config = null) {
        config ??= new DiscordSocketConfig();

        services.AddSingleton<DiscordSocketConfig>(config);
        services.AddSingleton<DiscordSocketClient>(new DiscordSocketClient(config));
        
        var builder = new BotServiceBuilder(services, token);
        services.AddSingleton<BotServiceBuilder>(builder);
        services.AddHostedService<BotService>();

        return builder;
    }
    public BotServiceBuilder(IServiceCollection services, string token) {
        _services = services;
        Token = token;
    }

    #region Builder methods

    /// <summary>
    /// Configures the <see cref="DiscordSocketClient"/> before it is started.
    /// </summary>
    /// <param name="configureDelegate"></param>
    /// <returns></returns>
    public BotServiceBuilder ConfigureClient(Func<DiscordSocketClient, IServiceProvider, Task> configureDelegate) {
        _configureDelegate = configureDelegate;
        return this;
    }
    
    /// <summary>
    /// Configures the <see cref="DiscordSocketClient"/> before it is started.
    /// </summary>
    /// <param name="configureDelegate"></param>
    /// <returns></returns>
    public BotServiceBuilder ConfigureClient(Action<DiscordSocketClient, IServiceProvider> configureDelegate) {
        _configureDelegate = (client, provider) => {
            configureDelegate(client, provider);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <summary>
    /// Add the interaction module to the <see cref="InteractionService"/>.
    /// </summary>
    /// <typeparam name="T">Interaction module type</typeparam>
    /// <returns></returns>
    public BotServiceBuilder AddInteractionModule<T>() where T : InteractionModuleBase {
        _services.TryAddSingleton<InteractionService>();
        _interactionModuleTypes.Add(typeof(T));
        return this;
    }
    
    /// <summary>
    /// Add the interaction module to the <see cref="InteractionService"/>.
    /// </summary>
    /// <param name="type">Interaction module type</param>
    /// <returns></returns>
    public BotServiceBuilder AddInteractionModule(Type type) {
        _services.TryAddSingleton<InteractionService>();
        _interactionModuleTypes.Add(type);
        return this;
    }

    /// <summary>
    /// Add all the interaction modules in an assembly to the <see cref="InteractionService"/>.
    /// </summary>
    /// <typeparam name="T">Type with from the assembly to use</typeparam>
    /// <returns></returns>
    public BotServiceBuilder AddInteractionModules<T>() {
        _services.TryAddSingleton<InteractionService>();
        _interactionModuleAssemblies.Add(typeof(T).Assembly);
        return this;
    }
    
    /// <summary>
    /// Add all the interaction modules in an assembly to the <see cref="InteractionService"/>.
    /// </summary>
    /// <param name="assembly">Assembly to use</param>
    /// <returns></returns>
    public BotServiceBuilder AddInteractionModules(Assembly assembly) {
        _services.TryAddSingleton<InteractionService>();
        _interactionModuleAssemblies.Add(assembly);
        return this;
    }
    
    /// <summary>
    /// Add the command module to the <see cref="CommandService"/>.
    /// </summary>
    /// <typeparam name="T">Command module type</typeparam>
    /// <returns></returns>
    public BotServiceBuilder AddCommandModule<T>() where T : ModuleBase {
        _services.TryAddSingleton<CommandService>();
        _commandModuleTypes.Add(typeof(T));
        return this;
    }
    
    /// <summary>
    /// Add the command module to the <see cref="CommandService"/>.
    /// </summary>
    /// <param name="type">Command module type</param>
    /// <returns></returns>
    public BotServiceBuilder AddCommandModule(Type type) {
        _services.TryAddSingleton<CommandService>();
        _commandModuleTypes.Add(type);
        return this;
    }
    
    /// <summary>
    /// Add all the command modules in an assembly to the <see cref="CommandService"/>.
    /// </summary>
    /// <typeparam name="T">Type with from the assembly to use</typeparam>
    /// <returns></returns>
    public BotServiceBuilder AddCommandModules<T>() {
        _services.TryAddSingleton<CommandService>();
        _commandModuleAssemblies.Add(typeof(T).Assembly);
        return this;
    }
    
    /// <summary>
    /// Add all the command modules in an assembly to the <see cref="CommandService"/>.
    /// </summary>
    /// <param name="assembly">Assembly to use</param>
    /// <returns></returns>
    public BotServiceBuilder AddCommandModules(Assembly assembly) {
        _services.TryAddSingleton<CommandService>();
        _commandModuleAssemblies.Add(assembly);
        return this;
    }

    /// <summary>
    /// Calls <see>
    ///     <cref>InteractionService.RegisterCommandsAsync</cref>
    /// </see>
    /// on startup.
    /// </summary>
    /// <returns></returns>
    public BotServiceBuilder RegisterCommandsGloballyOnStartup() {
        _registerCommandsGloballyOnStartup = true;
        return this;
    }
    
    /// <summary>
    /// Calls <see>
    ///     <cref>InteractionService.RegisterCommandsToGuildAsync</cref>
    /// </see>
    /// for each guild on startup.
    /// </summary>
    /// <param name="guildIds">IDs of the guilds</param>
    /// <returns></returns>
    public BotServiceBuilder RegisterCommandsToGuildsOnStartup(params ulong[] guildIds) {
        _registerCommandsToGuildsOnStartup.AddRange(guildIds);
        return this;
    }
    
    /// <summary>
    /// Configure the <see cref="InteractionService"/> before it is started.
    /// </summary>
    /// <param name="configureDelegate"></param>
    /// <returns></returns>
    public BotServiceBuilder ConfigureInteractionService(Func<InteractionService, IServiceProvider, Task> configureDelegate) {
        _interactionServiceConfigurator = configureDelegate;
        return this;
    }
    
    /// <summary>
    /// Configure the <see cref="InteractionService"/> before it is started.
    /// </summary>
    /// <param name="configureDelegate"></param>
    /// <returns></returns>
    public BotServiceBuilder ConfigureInteractionService(Action<InteractionService, IServiceProvider> configureDelegate) {
        _interactionServiceConfigurator = (service, provider) => {
            configureDelegate(service, provider);
            return Task.CompletedTask;
        };
        return this;
    }
    
    /// <summary>
    /// Configure the <see cref="CommandService"/> before it is started.
    /// </summary>
    /// <param name="configureDelegate"></param>
    /// <returns></returns>
    public BotServiceBuilder ConfigureCommandService(Func<CommandService, IServiceProvider, Task> configureDelegate) {
        _commandServiceConfigurator = configureDelegate;
        return this;
    }
    
    /// <summary>
    /// Configure the <see cref="CommandService"/> before it is started.
    /// </summary>
    /// <param name="configureDelegate"></param>
    /// <returns></returns>
    public BotServiceBuilder ConfigureCommandService(Action<CommandService, IServiceProvider> configureDelegate) {
        _commandServiceConfigurator = (service, provider) => {
            configureDelegate(service, provider);
            return Task.CompletedTask;
        };
        return this;
    }
    
    /// <summary>
    /// Code to run on the <see cref="DiscordSocketClient.Ready"/> event.
    /// </summary>
    /// <param name="onReadyDelegate"></param>
    /// <returns></returns>
    public BotServiceBuilder OnReady(Func<DiscordSocketClient, IServiceProvider, Task> onReadyDelegate) {
        _onReadyDelegate = onReadyDelegate;
        return this;
    }

    /// <summary>
    /// Code to run on the <see cref="DiscordSocketClient.Ready"/> event.
    /// </summary>
    /// <param name="onReadyDelegate"></param>
    /// <returns></returns>
    public BotServiceBuilder OnReady(Action<DiscordSocketClient, IServiceProvider> onReadyDelegate) {
        _onReadyDelegate = (client, provider) => {
            onReadyDelegate(client, provider);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <summary>
    /// Set the prefix for the <see cref="CommandService"/>.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public BotServiceBuilder WithPrefix(string prefix) {
        _prefix = prefix;
        return this;
    }

  #endregion

    /// <summary>
    /// Build the <see cref="BotService"/>.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="loggerFactory"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task<DiscordSocketClient> Build(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) {
        if (string.IsNullOrEmpty(Token)) {
            throw new InvalidOperationException("Token is null or empty.");
        }
        
        var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        var interactionService = IsInteractionServiceRegistered ? serviceProvider.GetRequiredService<InteractionService>() : null;
        var commandService = IsCommandServiceRegistered ? serviceProvider.GetRequiredService<CommandService>() : null;
        
        // Configure logging
        client.Log += LogMessage(loggerFactory.CreateLogger<DiscordSocketClient>());
        if (interactionService != null) {
            interactionService.Log += LogMessage(loggerFactory.CreateLogger<InteractionService>());
        }
        if (commandService != null) {
            commandService.Log += LogMessage(loggerFactory.CreateLogger<CommandService>());
        }
        
        // Setup interaction service
        if (interactionService != null) {
            foreach (var assembly in _interactionModuleAssemblies) {
                await interactionService.AddModulesAsync(assembly, serviceProvider);
            }
            foreach (var type in _interactionModuleTypes) {
                await interactionService.AddModuleAsync(type, serviceProvider);
            }
            if (_interactionServiceConfigurator != null) {
                await _interactionServiceConfigurator(interactionService, serviceProvider);
            }
        }
        
        // Setup command service
        if (commandService != null) {
            foreach (var assembly in _commandModuleAssemblies) {
                await commandService.AddModulesAsync(assembly, serviceProvider);
            }
            foreach (var type in _commandModuleTypes) {
                await commandService.AddModuleAsync(type, serviceProvider);
            }
            if (_commandServiceConfigurator != null) {
                await _commandServiceConfigurator(commandService, serviceProvider);
            }
        }
        
        // Configure on interaction
        if (interactionService != null) {
            client.InteractionCreated += async (x) => {
                var ctx = new SocketInteractionContext(client, x);
                var scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;
                await interactionService.ExecuteCommandAsync(ctx, scopedServiceProvider );
            };
        }
        
        // Configure on command
        if (commandService != null) {
            client.MessageReceived += async (x) => {
                var msg = x as SocketUserMessage;
                if (msg == null) {
                    return;
                }
                var argPos = 0;
                if (!msg.HasStringPrefix(_prefix, ref argPos)) {
                    return;
                }
                var ctx = new SocketCommandContext(client, msg);
                var scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;
                await commandService.ExecuteAsync(ctx, argPos, scopedServiceProvider);
            };
        }

        // Do things on ready
        client.Ready += async () => {
            // Call on ready delegate
            if (_onReadyDelegate != null) {
                await _onReadyDelegate(client, serviceProvider);
            }
            
            // Register commands
            if (interactionService != null) {
                if (_registerCommandsGloballyOnStartup) {
                    await interactionService.RegisterCommandsGloballyAsync();
                }
                foreach (var guildId in _registerCommandsToGuildsOnStartup) {
                    await interactionService.RegisterCommandsToGuildAsync(guildId);
                }
            }
        };
        
        // Call configure delegate
        if (_configureDelegate != null) {
            await _configureDelegate(client, serviceProvider);
        }

        return client;
    }
    
    private static Func<LogMessage, Task> LogMessage(ILogger logger) {
        return (log) => {
            logger.LogMessage(log);
            return Task.CompletedTask;
        };
    }
}
