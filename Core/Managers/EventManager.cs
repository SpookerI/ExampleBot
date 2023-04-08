using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Lavalink4NET.Player;
using Lavalink4NET;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace ExampleBot.Core.Managers
{
    public class EventManager
    {
        private static LavalinkNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavalinkNode>();
        private static DiscordSocketClient _client = ServiceManager.GetService<DiscordSocketClient>();
        private static CommandService _commandService = ServiceManager.GetService<CommandService>();
        public readonly ConcurrentDictionary<ulong, List<LavalinkTrack?>> Queues;

        public static async Task LoadCommands()
        {
            _client.Log += message =>
            {
                Console.WriteLine($"[{DateTime.Now}]\t({message.Source})\t{message.Message}"); // Logs
                return Task.CompletedTask;
            };

            _commandService.Log += message =>
            {
                Console.WriteLine($"[{DateTime.Now}]\t({message.Source})\t{message.Message}"); // Logs
                return Task.CompletedTask;
            };
            _client.Ready += OnReady; // Runs on startup
            _client.MessageReceived += OnMessageReceived;
        }
        private static async Task OnReady()
        {
            try
            {
                await _lavaNode.ConnectAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Console.WriteLine($"[{DateTime.Now}]\t(READY)\tI'm ready to rock!");
            await _client.SetStatusAsync(UserStatus.Online); // Sets status to "Online"
        }
        private static async Task OnMessageReceived(SocketMessage arg) // Commands manager
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            if (message.Author.IsBot || message.Channel is SocketDMChannel) return; // Check for bot or DM.
            var argPos = 0;

            if (!(message.HasStringPrefix(ConfigManager.Config.Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

            var result = await _commandService.ExecuteAsync(context, argPos, ServiceManager.Provider);

            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand) return; // Check for unknown Command
            }
        }
        public List<LavalinkTrack?> GetQueue(ulong guildid) =>
        !Queues.Select(x => x.Key).Contains(guildid)
            ? new List<LavalinkTrack>
            {
                Capacity = 0
            }
            : Queues.FirstOrDefault(x => x.Key == guildid).Value;
    }
}
