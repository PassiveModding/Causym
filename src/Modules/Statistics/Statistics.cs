using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Help;
using Qmmands;

namespace Causym.Modules.Statistics
{
    [Group("Statistics", "s")]
    [Disqord.Extensions.Checks.GuildOnly]
    [HelpMetadata("📊")]
    public class Statistics : DiscordModuleBase
    {
        [Command("SetSnapshots")]
        [Disqord.Extensions.Checks.RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetSnapshotsAsync(bool? enabled = null)
        {
            using (var db = new DataContext())
            {
                var config = db.StatServers.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
                if (config == null)
                {
                    if (enabled == null)
                    {
                        await ReplyAsync("Snapshots are not enabled");
                        return;
                    }

                    config = new StatisticsConfig
                    {
                        GuildId = Context.Guild.Id,
                        MemberChannelId = null,
                        SnapshotsEnabled = enabled.Value
                    };
                    db.StatServers.Add(config);
                }
                else
                {
                    if (enabled == null)
                    {
                        await ReplyAsync($"Snapshots Enabled: {config.SnapshotsEnabled}");
                        return;
                    }

                    config.SnapshotsEnabled = enabled.Value;
                    db.StatServers.Update(config);
                }

                if (config.SnapshotsEnabled)
                {
                    // Add initial snapshot for the server
                    var snapshot = new StatisticsSnapshot
                    {
                        GuildId = config.GuildId,
                        MemberCount = Context.Guild.MemberCount,
                        MembersDND = Context.Guild.Members
                            .Count(x => x.Value?.Presence?.Status == Disqord.UserStatus.DoNotDisturb),
                        MembersIdle = Context.Guild.Members
                            .Count(x => x.Value?.Presence?.Status == Disqord.UserStatus.Idle),
                        MembersOnline = Context.Guild.Members
                            .Count(x => x.Value?.Presence?.Status == Disqord.UserStatus.Online),
                        SnapshotTime = DateTime.UtcNow,

                        TotalMessageCount = 0
                    };
                    db.StatSnapshots.Add(snapshot);
                }

                await ReplyAsync($"Snapshots Enabled: {config.SnapshotsEnabled}");
                await db.SaveChangesAsync();
            }
        }

        [Command("CreateMemberChannel")]
        [Disqord.Extensions.Checks.RequireMemberGuildPermissions(Permission.Administrator)]
        [Disqord.Extensions.Checks.RequireBotGuildPermissions(Permission.ManageChannels)]
        public async Task MemberChannelAsync()
        {
            using (var db = new DataContext())
            {
                var config = db.StatServers.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
                if (config == null)
                {
                    var channel = await Context.Guild
                        .CreateVoiceChannelAsync($"👥 Members: {Context.Guild.MemberCount}", x => x.Position = 0);
                    config = new StatisticsConfig
                    {
                        GuildId = Context.Guild.Id,
                        MemberChannelId = channel.Id
                    };
                    db.StatServers.Add(config);
                }
                else
                {
                    if (Context.Guild.VoiceChannels.Any(x => x.Key == config.MemberChannelId))
                    {
                        await ReplyAsync($"You already have a configured member channel.");
                        return;
                    }

                    var channel = await Context.Guild
                        .CreateVoiceChannelAsync($"👥 Members: {Context.Guild.MemberCount}");
                    config.MemberChannelId = channel.Id;
                    db.StatServers.Update(config);
                }

                await ReplyAsync($"Member Channel Created, it is recommended you set it so users cannot join.");
                await db.SaveChangesAsync();
            }
        }

        [Command("PlotAll")]
        public async Task PlotAllAsync(TimeSpan? time = null)
        {
            if (time == null)
            {
                time = TimeSpan.FromDays(7);
            }
            else if (time > TimeSpan.FromDays(7))
            {
                await ReplyAsync("Currently snapshots are limited to 7 days of data.");
                return;
            }

            var minTime = DateTime.UtcNow - time;

            using (var db = new DataContext())
            {
                var snapshots = db.StatSnapshots
                    .Where(x => x.GuildId == Context.Guild.Id)
                    .OrderByDescending(x => x.SnapshotTime)
                    .Where(x => x.SnapshotTime >= minTime)
                    .Take(144);

                int count = snapshots.Count();
                if (count == 0)
                {
                    await ReplyAsync("No Captured Data Found.");
                    return;
                }
                else if (count == 1)
                {
                    await ReplyAsync("Not Enough Captured Data Found.");
                    return;
                }

                var plt = GetDefaultPlot();
                var xValues = snapshots.Select(x => x.SnapshotTime.ToOADate()).ToArray();
                plt.PlotScatter(
                    xValues,
                    snapshots.Select(x => (double)x.MemberCount).ToArray(),
                    Color.Blue,
                    label: "Members");
                plt.PlotScatter(
                    xValues,
                    snapshots.Select(x => (double)x.MembersDND).ToArray(),
                    Color.Red,
                    label: "Members DND");
                plt.PlotScatter(
                    xValues,
                    snapshots.Select(x => (double)x.MembersIdle).ToArray(),
                    Color.Orange,
                    label: "Members Idle");
                plt.PlotScatter(
                    xValues,
                    snapshots.Select(x => (double)x.MembersOnline).ToArray(),
                    Color.Green,
                    label: "Members Online");
                plt.PlotScatter(
                    xValues,
                    snapshots.Select(x => (double)x.TotalMessageCount).ToArray(),
                    Color.Honeydew,
                    label: "Messages");

                plt.YLabel("Members");
                plt.Title("Server Stats");
                await SendPlot(Context.Channel, plt);
            }
        }

        public ScottPlot.Plot GetDefaultPlot()
        {
            var plt = new ScottPlot.Plot(1000, 500);
            plt.Style(ScottPlot.Style.Gray2);
            return plt;
        }

        [Command("PlotChannelMessages")]
        public async Task PlotAsync(CachedTextChannel channel = null, TimeSpan? time = null)
        {
            if (time == null)
            {
                time = TimeSpan.FromDays(7);
            }
            else if (time > TimeSpan.FromDays(7))
            {
                await ReplyAsync("Currently snapshots are limited to 7 days of data.");
                return;
            }

            var minTime = DateTime.UtcNow - time;

            channel ??= (CachedTextChannel)Context.Channel;

            using (var db = new DataContext())
            {
                var snapshots = db.ChannelSnapshots
                    .Where(x => x.ChannelId == channel.Id)
                    .OrderByDescending(x => x.SnapshotTime)
                    .Where(x => x.SnapshotTime >= minTime).Take(144);

                int count = snapshots.Count();
                if (count == 0)
                {
                    await ReplyAsync("No Captured Data Found.");
                    return;
                }
                else if (count == 1)
                {
                    await ReplyAsync("Not Enough Captured Data Found.");
                    return;
                }

                var xValues = snapshots.Select(x => x.SnapshotTime.ToOADate()).ToArray();
                var yValues = snapshots.Select(x => (double)x.MessageCount).ToArray();

                var plt = GetDefaultPlot();
                plt.PlotScatter(xValues, yValues);
                plt.YLabel("Messages");

                plt.Title($"#{channel.Name} Messages");
                await SendPlot(Context.Channel, plt);
            }
        }

        [Command("PlotMembers")]
        public async Task PlotMembersAsync(TimeSpan? time = null)
        {
            if (time == null)
            {
                time = TimeSpan.FromDays(7);
            }
            else if (time > TimeSpan.FromDays(7))
            {
                await ReplyAsync("Currently snapshots are limited to 7 days of data.");
                return;
            }

            var minTime = DateTime.UtcNow - time;

            using (var db = new DataContext())
            {
                var snapshots = db.StatSnapshots.Where(x => x.GuildId == Context.Guild.Id)
                    .Where(x => x.SnapshotTime >= minTime).OrderByDescending(x => x.SnapshotTime).Take(144);

                int count = snapshots.Count();
                if (count == 0)
                {
                    await ReplyAsync("No Captured Data Found.");
                    return;
                }
                else if (count == 1)
                {
                    await ReplyAsync("Not Enough Captured Data Found.");
                    return;
                }

                var plt = GetDefaultPlot();
                var xValues = snapshots.Select(x => x.SnapshotTime.ToOADate()).ToArray();
                plt.PlotScatter(xValues, snapshots.Select(x => (double)x.MemberCount).ToArray(), Color.Blue, label: "Members");
                plt.PlotScatter(xValues, snapshots.Select(x => (double)x.MembersDND).ToArray(), Color.Red, label: "Members DND");
                plt.PlotScatter(xValues, snapshots.Select(x => (double)x.MembersIdle).ToArray(), Color.Orange, label: "Members Idle");
                plt.PlotScatter(xValues, snapshots.Select(x => (double)x.MembersOnline).ToArray(), Color.Green, label: "Members Online");

                plt.YLabel("Members");

                plt.Title("Member Stats");
                await SendPlot(Context.Channel, plt);
            }
        }

        public async Task SendPlot(ICachedMessageChannel channel, ScottPlot.Plot plot)
        {
            // Set horizontal margin to be slightly larger to accomodate for longer labels
            plot.AxisAuto(0.1);

            plot.XLabel("Time");
            plot.Ticks(useExponentialNotation: false, useMultiplierNotation: false, useOffsetNotation: false, dateTimeX: true);

            // TODO: sort out location.
            plot.Legend();

            var map = plot.GetBitmap();

            using (var stream = new MemoryStream())
            {
                map.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                using (var attachment = new LocalAttachment(stream, "data.png"))
                {
                    await channel.SendMessageAsync(attachment);
                }
            }
        }
    }
}