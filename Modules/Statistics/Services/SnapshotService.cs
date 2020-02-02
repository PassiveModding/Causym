using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Causym.Services;
using Disqord.Events;

namespace Causym.Modules.Statistics
{
    [Service]
    public partial class StatisticsService
    {
        public Timer SnapshotTimer { get; }

        public Dictionary<ulong, Dictionary<ulong, int>> GuildMessageTracker { get; } = new Dictionary<ulong, Dictionary<ulong, int>>();

        public HashSet<ulong> SnapshotEnabledCache { get; internal set; } = new HashSet<ulong>();

        private Task Bot_MessageReceived(MessageReceivedEventArgs e)
        {
            lock (GuildMessageTracker)
            {
                // TODO: Ensure the guild is tracked, use locally cached list?
                if (e.Message.Guild == null) return Task.CompletedTask;
                if (SnapshotEnabledCache.Contains(e.Message.Guild.Id))
                {
                    if (!GuildMessageTracker.ContainsKey(e.Message.Guild.Id))
                    {
                        GuildMessageTracker[e.Message.Guild.Id] = new Dictionary<ulong, int>();
                    }

                    if (GuildMessageTracker[e.Message.Guild.Id].ContainsKey(e.Message.Channel.Id))
                    {
                        GuildMessageTracker[e.Message.Guild.Id][e.Message.Channel.Id]++;
                    }
                    else
                    {
                        GuildMessageTracker[e.Message.Guild.Id].Add(e.Message.Channel.Id, 1);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private async void SnapshotCallback(object state)
        {
            try
            {
                SnapshotTimer.Change(Timeout.Infinite, Timeout.Infinite);
                using (var db = new DataContext())
                {
                    lock (GuildMessageTracker)
                    {
                        var time = DateTime.UtcNow;
                        var configs = db.StatServers.Where(x => x.SnapshotsEnabled);
                        SnapshotEnabledCache = configs.Select(x => x.GuildId).ToHashSet();
                        foreach (var config in configs)
                        {
                            if (!Bot.Guilds.TryGetValue(config.GuildId, out var guild)) continue;

                            int messageCount = 0;
                            if (GuildMessageTracker.TryGetValue(config.GuildId, out var channelDictionary))
                            {
                                foreach (var channel in channelDictionary)
                                {
                                    var chanelSnapshot = new ChannelSnapshot
                                    {
                                        GuildId = config.GuildId,
                                        ChannelId = channel.Key,
                                        MessageCount = channel.Value,
                                        SnapshotTime = time
                                    };

                                    db.ChannelSnapshots.Add(chanelSnapshot);
                                    messageCount += channel.Value;
                                }
                            }

                            var snapshot = new StatisticsSnapshot
                            {
                                GuildId = config.GuildId,
                                MemberCount = guild.MemberCount,
                                MembersDND = guild.Members.Count(x => x.Value?.Presence?.Status == Disqord.UserStatus.DoNotDisturb),
                                MembersIdle = guild.Members.Count(x => x.Value?.Presence?.Status == Disqord.UserStatus.Idle),
                                MembersOnline = guild.Members.Count(x => x.Value?.Presence?.Status == Disqord.UserStatus.Online),
                                SnapshotTime = time,

                                TotalMessageCount = messageCount
                            };

                            db.StatSnapshots.Add(snapshot);
                        }
                    }

                    await db.SaveChangesAsync();
                }
            }
            finally
            {
#if DEBUG
                SnapshotTimer.Change(30 * 1000, Timeout.Infinite);
#else
                SnapshotTimer.Change(60 * 1000 * 10, Timeout.Infinite);
#endif
            }
        }
    }
}
