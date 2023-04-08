using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ExampleBot.Core.Managers;

namespace ExampleBot.Core.Commands
{
    public class PrefixCommands : ModuleBase<SocketCommandContext>
    {
        private static CommandService _commandService = ServiceManager.GetService<CommandService>();
        private static DiscordSocketClient _client = ServiceManager.GetService<DiscordSocketClient>();
        [Command("random")]
        public async Task Random(int randomMin = 0, int randomMax = 101)
        {
            Random rnd = new Random();
            await Context.Channel.SendMessageAsync($"Number: {rnd.Next(randomMin, randomMax)}");
        }
        [Command("help")]
        public async Task Help()
        {
            await Context.Channel.SendMessageAsync("Commands list:```\n" +
                "random - A random number in a given range. If range is not set, then from 0 to 100. Example: a!random 727 1337\n" +
                "[Music Commands]\n" +
                "join - Connect to the current voice channel.\n" +
                "play - Play specified song. You can specify the title or video link. Example: a!play https://www.youtube.com/watch?v=dQw4w9WgXcQ\n" +
                "leave - Leave the voice channel.\n" +
                "remove - Remove song from queue. Example: a!remove 2\n" +
                "pause - Pause the playback.\n" +
                "stop - Stop playback and clear the queue.\n" +
                "list - Show all songs in the queue.\n" +
                "skip - Skip the current song.\n```");
        }
        [Command("join")]
        public async Task Join()
            => await Context.Channel.SendMessageAsync(await AudioManager.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("play")]
        public async Task Play([Remainder] string search)
            => await Context.Channel.SendMessageAsync(await AudioManager.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search));

        [Command("leave")]
        public async Task Leave()
            => await Context.Channel.SendMessageAsync(await AudioManager.LeaveAsync(Context.Guild));

        [Command("remove")]
        public async Task Remove(int trackNum = 0)
            => await Context.Channel.SendMessageAsync(await AudioManager.RemoveSong(Context.Guild, trackNum));

        [Command("pause")]
        [Alias("resume")]
        public async Task Pause()
            => await Context.Channel.SendMessageAsync(await AudioManager.TogglePauseAsync(Context.Guild));

        [Command("stop")]
        public async Task Stop()
            => await Context.Channel.SendMessageAsync(await AudioManager.StopAsync(Context.Guild));

        [Command("list")]
        public async Task List()
            => await Context.Channel.SendMessageAsync(await AudioManager.ListAsync(Context.Guild));

        [Command("skip")]
        public async Task Skip()
            => await Context.Channel.SendMessageAsync(await AudioManager.SkipAsync(Context.Guild));
    }
}
