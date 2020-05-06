using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;

namespace Causym.Modules.Admin
{
    // This is selectively not included in the help commands as these commands are not to be shown to regular users
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