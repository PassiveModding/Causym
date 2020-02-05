using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;

namespace Causym.Modules.Admin
{
    [BotOwnerOnly]
    [Group("Admin")]
    public class AdminModule : DiscordModuleBase
    {
        [Command("SetUsername")]
        [Description("Sets the bot's display username")]
        public async Task SetUserNameAsync([Remainder]string username)
        {
            await Context.Bot.ModifyCurrentUserAsync(x => x.Name = username);
            await ReplyAsync("Username updated.");
        }
    }
}