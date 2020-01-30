using System.Linq;
using System.Threading.Tasks;
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
    }
}
