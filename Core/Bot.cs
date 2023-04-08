using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Discord.Interactions;
using ExampleBot.Core.Managers;

namespace ExampleBot.Core
{
    public class Bot
    {
        private DiscordSocketClient _client;
        private CommandService _commandService;
        private LavalinkNodeOptions _lavalinkOptions;
        private IDiscordClientWrapper _discordClientWrapper;

        public Bot()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All
            });

            _commandService = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = false,
                DefaultRunMode = Discord.Commands.RunMode.Async,
                IgnoreExtraArgs = true
            });
            _lavalinkOptions = new LavalinkNodeOptions()
            {
                Password = "12341234",
                BufferSize = 2048,
                Label = "ExampleBot"
            };
            _discordClientWrapper = new DiscordClientWrapper(_client);

            var collection = new ServiceCollection();
            collection.AddSingleton(_client);
            collection.AddSingleton(_commandService);
            collection.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
            collection.AddSingleton<LavalinkNode>();
            collection.AddSingleton(_lavalinkOptions);
            collection.AddSingleton(_discordClientWrapper);
            ServiceManager.SetProvider(collection);
        }
        public async Task MainAsync()
        {

            if (string.IsNullOrWhiteSpace(ConfigManager.Config.Token)) return;

            await CommandManager.LoadCommandsAsync();
            await EventManager.LoadCommands();
            await _client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
            await _client.StartAsync();

            Console.ReadLine();

        }
    }
}
