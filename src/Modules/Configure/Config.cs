﻿using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Help;
using Qmmands;

namespace Causym.Modules.Configure
{
    [HelpMetadata("🛠️")]
    public class Config : DiscordModuleBase
    {
        [Command("SetPrefix")]
        [Description("Set's the bot's custom prefix for the current server")]
        [Disqord.Extensions.Checks.RequireMemberGuildPermissions(Disqord.Permission.Administrator)]
        [GuildOnly]
        public async Task SetPrefixAsync(string prefix = null)
        {
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