using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;

namespace Causym.Modules.Configure
{
    public class Config : DiscordModuleBase
    {
        [Command("SetPrefix")]
        [RequireMemberGuildPermissions(Disqord.Permission.Administrator)]
        public async Task SetPrefixAsync(string prefix = null)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("Command can only be used from within a discord server.");
                return;
            }

            using (var db = new DataContext())
            {
                var config = db.Guilds.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
                if (config == null)
                {
                    config = new GuildConfiguration
                    {
                        GuildId = Context.Guild.Id,
                        Prefix = prefix
                    };
                    db.Guilds.Add(config);
                }
                else
                {
                    config.Prefix = prefix;
                    db.Guilds.Update(config);
                }

                db.SaveChanges();
            }

            await ReplyAsync("Guild prefix set (or removed)");
        }
    }
}
