using Lavalink4NET;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using System.Text;
using Lavalink4NET.Events;

namespace ExampleBot.Core.Managers
{
    public class AudioManager
    {
        private static readonly LavalinkNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavalinkNode>();
        private static EventManager _eventManager = new EventManager();

        public static async Task<string> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel channel)
        {
            var player = _lavaNode.GetPlayer<QueuedLavalinkPlayer>(guild.Id);
            if (_lavaNode.HasPlayer(guild.Id))
                return "I'm already in the voice channel!";
            if (voiceState.VoiceChannel is null)
                return $"You are not in voice channel!";
            try
            {
                await _lavaNode.JoinAsync<QueuedLavalinkPlayer>(guild.Id, voiceState.VoiceChannel.Id); // Присоединение к голосовому каналу.
                return $"Joined {voiceState.VoiceChannel.Name}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public static async Task<string> PlayAsync(SocketGuildUser user, IGuild guild, string query)
        {
            if (user.VoiceChannel is null)
                return $"You are not in voice channel!";
            if (!_lavaNode.HasPlayer(guild.Id))
                return $"I'm not in voice channel!";
            try
            {
                var player = _lavaNode.GetPlayer<QueuedLavalinkPlayer>(guild.Id);

                var track = await _lavaNode.GetTrackAsync(query, SearchMode.YouTube);

                if (track == null)
                    return $"I couldn't find the song: {query}";

                var position = await player.PlayAsync(track, enqueue: true);

                if (position == 0)
                {
                    Console.WriteLine($"Now playing {track.Title}"); // Logs
                    return $"Now playing: [{track.Title}]";
                }
                else
                {
                    Console.WriteLine($"Added to Queue {track.Title}"); // Logs
                    return $"Song: [{track.Title}] was added to queue.";
                }

            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public static async Task<string> ListAsync(IGuild guild)
        {
            try
            {
                var descriptionBuilder = new StringBuilder();
                var player = _lavaNode.GetPlayer<QueuedLavalinkPlayer>(guild.Id);

                if (player is null) return "I'm not in voice channel!";
                if (player.State is PlayerState.Playing)
                {
                    if (player.Queue.Count < 1 && player.CurrentTrack != null)
                    {
                        return $"Now playing: [{player.CurrentTrack.Title}]\nQueue is empty.";
                    }
                    else
                    {
                        var trackNum = 1;
                        foreach (var track in player.Queue)
                        {
                            descriptionBuilder.Append($"{trackNum}: [{track.Title}]\n");
                            trackNum++;
                        }
                        return $"Now playing: [{player.CurrentTrack.Title}]\n{descriptionBuilder}";
                    }
                }
                else
                {
                    return $"Nothing is playing.";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public static async Task<string> SkipAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer<QueuedLavalinkPlayer>(guild.Id);
                if (player.Queue.Count < 1)
                {
                    return $"Can't skip the song because the queue is empty.";
                }
                else
                {
                    try
                    {
                        var currentTrack = player.CurrentTrack;
                        await player.SkipAsync();
                        return $"Skipped: [{currentTrack.Title}]";
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static async Task<string> LeaveAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer<QueuedLavalinkPlayer>(guild.Id);
                await player.StopAsync(true);

                Console.WriteLine($"[{DateTime.Now}]\t(AUDIO)\tLeft a VC."); // Логи
                return $"Left voice channel!";
            }
            catch (InvalidOperationException ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public static async Task<string> StopAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer<QueuedLavalinkPlayer>(guild.Id);

                if (player.State is PlayerState.Playing)
                {
                    await player.StopAsync(false);
                    return $"Playback stopped.";
                }
                else
                {
                    return $"Nothing is playing right now!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static async Task<string> TogglePauseAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer<QueuedLavalinkPlayer>(guild.Id);
                if (!(player.State is PlayerState.Playing) && player.CurrentTrack == null) return $"Nothing is playing.";

                if (!(player.State is PlayerState.Playing))
                {
                    await player.ResumeAsync();
                    return $"**Resumed**: [{player.CurrentTrack.Title}]";
                }
                await player.PauseAsync();
                return $"**Paused**: [{player.CurrentTrack.Title}]";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
        public static async Task<string> RemoveSong(IGuild guild, int trackNum)
        {
            if (trackNum == 0) return "Specify song number.";
            var player = _lavaNode.GetPlayer<QueuedLavalinkPlayer>(guild.Id);
            if (player.Queue.Count == 0)
                return "Queue is empty.";
            var toRemove = player.Queue.ElementAt(trackNum - 1);
            var currentTrack = player.CurrentTrack;
            if (toRemove == null)
            {
                return "Song not found.";
            }
            var toReplace = player.Queue?.ElementAt(player.Queue.IndexOf(currentTrack) + 1);
            if (currentTrack == toRemove && toReplace is not null)
                await player.PlayAsync(toReplace, enqueue: true);
            else if (currentTrack == toRemove && toReplace is null)
                await player.StopAsync();
            player.Queue.Remove(player.Queue.ElementAt(trackNum - 1));
            return "Song was removed from queue.";
        }
        /*private async Task OnTrackEndAsync(TrackEndEventArgs args)
        {
            if (args.Reason is TrackEndReason.Stopped or TrackEndReason.CleanUp or TrackEndReason.Replaced) return;
            var queue = _eventManager.GetQueue(args.Player.GuildId);
            if (queue.Count > 0)
            {
                var gid = args.Player.GuildId;
                var currentTrack = queue.Find(x => args.Player.CurrentTrack.Identifier == x.Identifier);
                var nextTrack = queue.ElementAt(queue.IndexOf(currentTrack) + 1);

                if (nextTrack is null) return;
                await args.Player.PlayAsync(nextTrack);
            }
        }*/
    }
}
