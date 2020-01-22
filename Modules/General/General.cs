using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Causym.Modules.General
{
    public class General : DiscordModuleBase
    {
        [Command("Avatar")]
        public async Task AvatarAsync(CachedUser user)
        {
            await ReplyAsync("", false, new LocalEmbedBuilder().WithAuthor(user).WithImageUrl(user.GetAvatarUrl()).Build());
        }
    }
}
