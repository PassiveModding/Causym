using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Bot.Sharding;
using Disqord.Events;

namespace Causym.Modules.Statistics
{
    public partial class StatisticsService
    {
        private readonly Dictionary<ulong, (ulong ChannelId, int MemberCount)> updateQueue =
            new Dictionary<ulong, (ulong ChannelId, int MemberCount)>();

        public StatisticsService(DiscordBotSharder bot)
        {
            using (var db = new DataContext())
            {
                SnapshotEnabledCache = db.StatServers.Where(x => x.SnapshotsEnabled).Select(x => x.GuildId).ToHashSet();
            }

            Bot = bot;
            Bot.MemberJoined += Bot_MemberJoined;
            Bot.MemberLeft += Bot_MemberLeft;
            Bot.MessageReceived += Bot_MessageReceived;
            ChannelTimer = new Timer(ChannelCallback, null, 0, 60 * 1000);

            // 10 minutes vs 30 sec in debug mode.
#if DEBUG
            SnapshotTimer = new Timer(SnapshotCallback, null, 0, 30 * 1000);
#else
            SnapshotTimer = new Timer(SnapshotCallback, null, 0, 60 * 1000 * 10);
#endif
        }

        public DiscordBotSharder Bot { get; }

        public Timer ChannelTimer { get; }

        private async void ChannelCallback(object state)
        {
            try
            {
                // Prevent the timer from running until this method ends
                ChannelTimer.Change(Timeout.Infinite, Timeout.Infinite);
                var copy = updateQueue.ToDictionary(x => x.Key, x => x.Value);
                foreach (var item in copy)
                {
                    updateQueue.Remove(item.Key);

                    try
                    {
                        await Bot.ModifyVoiceChannelAsync(
                            item.Value.ChannelId,
                            x => x.Name = $"👥 Members: {item.Value.MemberCount}");
                    }
                    catch
                    {
                        using (var db = new DataContext())
                        {
                            var config = db.StatServers.FirstOrDefault(x => x.GuildId == item.Key);
                            if (config == null) return;

                            config.MemberChannelId = null;
                            db.Update(config);
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
            finally
            {
                ChannelTimer.Change(60000, Timeout.Infinite);
            }
        }

        private Task Bot_MemberLeft(MemberLeftEventArgs e)
        {
            using (var db = new DataContext())
            {
                var config = db.StatServers.FirstOrDefault(x => x.GuildId == e.Guild.Id);
                if (config == null || config.MemberChannelId == null) return Task.CompletedTask;
                if (!e.Guild.VoiceChannels.TryGetValue(config.MemberChannelId.Value, out var channel))
                    return Task.CompletedTask;

                updateQueue[e.Guild.Id] = (channel.Id, e.Guild.MemberCount);
            }

            return Task.CompletedTask;
        }

        private Task Bot_MemberJoined(MemberJoinedEventArgs e)
        {
            using (var db = new DataContext())
            {
                var config = db.StatServers.FirstOrDefault(x => x.GuildId == e.Member.Guild.Id);
                if (config == null || config.MemberChannelId == null) return Task.CompletedTask;
                if (!e.Member.Guild.VoiceChannels.TryGetValue(config.MemberChannelId.Value, out var channel))
                    return Task.CompletedTask;

                updateQueue[e.Member.Guild.Id] = (channel.Id, e.Member.Guild.MemberCount);
            }

            return Task.CompletedTask;
        }
    }
}