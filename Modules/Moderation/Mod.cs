using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Causym.Services.Help;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Qmmands;

namespace Causym.Modules.Moderation
{
    [Group("Moderation", "Mod", "M")]
    [HelpMetadata("🔨", "#C70039")]
    [RequireMemberGuildPermissions(Permission.Administrator)]
    public class Mod : DiscordModuleBase
    {
        [Command("Prune")]
        public async Task PruneAsync(IUser user)
        {
            var messages = await Context.Channel.GetMessagesAsync();
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(x => x.Author.Id == user.Id).Select(x => x.Id));
        }

        [Command("Prune")]
        public async Task PruneAsync(IRole role)
        {
            var messages = await Context.Channel.GetMessagesAsync();
            var toRemove = new List<RestMessage>();
            foreach (var message in messages)
            {
                if (Context.Guild.Members[message.Author.Id]?.Roles.ContainsKey(role.Id) == true)
                {
                    toRemove.Add(message);
                }
            }

            await (Context.Channel as ITextChannel).DeleteMessagesAsync(toRemove.Select(x => x.Id));
        }

        [Command("Prune")]
        public async Task PruneAsync()
        {
            var messages = await Context.Channel.GetMessagesAsync();
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages.Select(x => x.Id));
        }
    }
}
