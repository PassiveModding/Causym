using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Help;
using Disqord.Rest;
using Qmmands;

namespace Causym.Modules.Moderation
{
    [Group("Moderation", "Mod", "M")]
    [HelpMetadata("🔨", "#C70039")]
    [Disqord.Extensions.Checks.RequireMemberGuildPermissions(Permission.Administrator)]
    public class Mod : DiscordModuleBase
    {
        [Command("PruneBan")]
        [Description("Bans all members who have the specified content in their message.")]
        [Disqord.Extensions.Checks.RequireMemberGuildPermissions(Permission.Administrator)]
        [Disqord.Extensions.Checks.RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task PruneBanAsync([Remainder]string messageMatch)
        {
            if (messageMatch.Length < 5)
            {
                await ReplyAsync("Minimum message length is 5 to avoid accidental bans.");
                return;
            }

            int counter = 0;
            var messages = await Context.Channel.GetMessagesAsync();
            foreach (var message in messages)
            {
                if (message.Author is RestMember mem)
                {
                    // Skips members with roles.
                    if (mem.RoleIds.Any()) continue;
                }

                // Skip attempting to ban the current user.
                if (message.Author.Id == Context.User.Id) continue;

                if (message.Content.Contains(messageMatch, System.StringComparison.OrdinalIgnoreCase))
                {
                    await Context.Guild.BanMemberAsync(message.Author.Id, $"PruneBan for message match: {messageMatch}", 7);
                    counter++;
                }
            }

            await ReplyAsync($"PruneBanned {counter} messages and their authors.");
        }

        [Group("Prune")]
        [HelpMetadata("🗄️", "#C70039")]
        [Disqord.Extensions.Checks.RequireBotChannelPermissions(Permission.ManageMessages)]
        public class Prune : DiscordModuleBase
        {
            [Command]
            public async Task PruneAsync(ulong countOrUserOrRoleId = 100)
            {
                IEnumerable<RestMessage> messages;
                if (countOrUserOrRoleId <= 1000)
                {
                    messages = await Context.Channel.GetMessagesAsync((int)countOrUserOrRoleId);
                }
                else if (countOrUserOrRoleId < 1000000000)
                {
                    await ReplyAsync(
                        "",
                        false,
                        new LocalEmbedBuilder()
                        .WithDescription("Maximum prune count is 1000")
                        .Build());
                    return;
                }
                else
                {
                    // Value is arbitrarily large, check if it is a role or user.
                    messages = await Context.Channel.GetMessagesAsync(100);
                    int count = messages.Count();

                    if (Context.Guild.Roles.ContainsKey(countOrUserOrRoleId))
                    {
                        // Check against member roles.
                        messages = messages
                            .Where(x => (x.Author as RestMember)?.RoleIds.Contains(countOrUserOrRoleId) == true);
                        if (messages.Count() == count)
                        {
                            // No matches found.
                            await ReplyAsync(
                                "",
                                false,
                                new LocalEmbedBuilder()
                                .WithDescription("No messages matching the search found.")
                                .Build());
                            return;
                        }
                    }
                    else
                    {
                        // Check against user id.
                        messages = messages.Where(x => x.Author.Id == countOrUserOrRoleId);
                        if (messages.Count() == count)
                        {
                            // No matches found.
                            await ReplyAsync(
                                "",
                                false,
                                new LocalEmbedBuilder()
                                .WithDescription("No messages matching the search found.")
                                .Build());
                            return;
                        }
                    }
                }

                await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages.Select(x => x.Id));
            }

            [Command]
            public async Task PruneAsync(CachedUser user)
            {
                var messages = await Context.Channel.GetMessagesAsync();
                await (Context.Channel as ITextChannel)
                    .DeleteMessagesAsync(messages.Where(x => x.Author.Id == user.Id).Select(x => x.Id));
            }

            [Command]
            public async Task PruneAsync(CachedRole role)
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
        }
    }
}