using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Causym.Modules.Statistics
{
    [Group("Statistics", "s")]
    [GuildOnly]
    public class Statistics : DiscordModuleBase
    {

        [Command("SetSnapshots")]
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

                await ReplyAsync($"Snapshots Enabled: {config.SnapshotsEnabled}");
                await db.SaveChangesAsync();
            }
        }

        [Command("CreateMemberChannel")]
        [RequireMemberGuildPermissions(Disqord.Permission.Administrator)]
        [RequireBotGuildPermissions(Disqord.Permission.ManageChannels)]
        public async Task MemberChannelAsync()
        {
            using (var db = new DataContext())
            {
                var config = db.StatServers.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
                if (config == null)
                {
                    var channel = await Context.Guild.CreateVoiceChannelAsync($"👥 Members: {Context.Guild.MemberCount}", x => x.Position = 0);
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

                    var channel = await Context.Guild.CreateVoiceChannelAsync($"👥 Members: {Context.Guild.MemberCount}");
                    config.MemberChannelId = channel.Id;
                    db.StatServers.Update(config);
                }

                await ReplyAsync($"Member Channel Created, it is recommended you set it so users cannot join.");
                await db.SaveChangesAsync();
            }
        }

        [Command("PlotChannelMessages")]
        public async Task PlotAsync(CachedTextChannel channel = null)
        {
            channel ??= (CachedTextChannel)Context.Channel;

            using (var db = new DataContext())
            {
                var snapshots = db.ChannelSnapshots.Where(x => x.ChannelId == channel.Id).OrderByDescending(x => x.SnapshotTime).Take(144);

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

                await PlotAsync(snapshots.Select(x => (double)x.SnapshotTime.Ticks).ToArray(), snapshots.Select(x => (double)x.MessageCount).ToArray(), "Messages", $"#{channel.Name} Messages");
            }
        }

        private async Task PlotAsync(double[] xValues, double[] yValues, string yTitle, string title)
        {
            var plt = new ScottPlot.Plot(1000, 500);
            plt.PlotScatter(xValues, yValues);

            // Calculate difference in ticks between first and last snapshots
            var difference = xValues.First() - xValues.Last();
            int steps = 6;
            long increment = (long)difference / steps;
            long value = (long)xValues.Last();

            var snapshotTicks = new List<double>();
            var snapshotLabels = new List<string>();

            // Add labels along x axis
            for (int i = 0; i <= steps; i++)
            {
                snapshotTicks.Add(value);
                snapshotLabels.Add(new DateTime(value).ToString("dd/MM HH:mm tt"));

                value += increment;
            }

            plt.XTicks(snapshotTicks.ToArray(), snapshotLabels.ToArray());

            // Label axes
            plt.YLabel("Messages");
            plt.XLabel("Time");

            // Set horizontal margin to be sloightly larger to accomodate for longer labels
            plt.AxisAuto(0.1);

            plt.Style(ScottPlot.Style.Gray2);
            plt.Title($"#{Context.Channel.Name} Messages");
            var map = plt.GetBitmap();

            using (var stream = new MemoryStream())
            {
                map.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendMessageAsync(new LocalAttachment(stream, "data.png"));
            }
        }
    }
}
