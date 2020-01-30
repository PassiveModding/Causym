using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Events;

namespace Causym.Modules.Statistics
{
    public partial class StatisticsService
    {
        public Timer SnapshotTimer { get; }

        public Dictionary<ulong, Dictionary<ulong, int>> GuildMessageTracker { get; set; } = new Dictionary<ulong, Dictionary<ulong, int>>();

        public HashSet<ulong> SnapshotEnabledCache { get; set; } = new HashSet<ulong>();

        private async Task Bot_MessageReceived(MessageReceivedEventArgs e)
        {
            lock (GuildMessageTracker)
            {
                // TODO: Ensure the guild is tracked, use locally cached list?
                if (e.Message.Guild == null) return;
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
                            if (!GuildMessageTracker.TryGetValue(config.GuildId, out var channelDictionary))
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
                                MembersDND = guild.Members.Count(x => x.Value.Presence.Status == Disqord.UserStatus.DoNotDisturb),
                                MembersIdle = guild.Members.Count(x => x.Value.Presence.Status == Disqord.UserStatus.Idle),
                                MembersOnline = guild.Members.Count(x => x.Value.Presence.Status == Disqord.UserStatus.Online),
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
                SnapshotTimer.Change(60 * 1000 * 10, Timeout.Infinite);
            }
        }
    }
}
