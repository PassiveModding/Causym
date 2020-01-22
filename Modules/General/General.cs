using System.Diagnostics;
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

        [Command("Ping", "Latency")]
        public async Task PingAsync()
        {
            var latency = Context.Bot.Latency;
            var s = Stopwatch.StartNew();
            var m = await ReplyAsync($"heartbeat: {latency}ms, init: ---, rtt: ---");
            var init = s.ElapsedMilliseconds;
            await m.ModifyAsync(x => x.Content = $"heartbeat: {latency}ms, init: {init}ms, rtt: Calculating");
            s.Stop();
            await m.ModifyAsync(x => x.Content = $"heartbeat: {latency}ms, init: {init}ms, rtt: {s.ElapsedMilliseconds}ms");
        }
    }
}
