using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Help;
using Disqord.Extensions.Interactivity.Menus;
using Newtonsoft.Json.Linq;
using Passive;
using Passive.Discord;
using Qmmands;

namespace Causym.Modules.General
{
    [HelpMetadata("🎁", "#4fe25d")]
    public class General : DiscordModuleBase
    {
        public General(CommandService cmdService, HttpClient httpClient)
        {
            CmdService = cmdService;
            HttpClient = httpClient;
        }

        public CommandService CmdService { get; }

        public HttpClient HttpClient { get; }

        [Command("Avatar")]
        public async Task AvatarAsync(CachedUser user = null)
        {
            if (user == null) user = Context.User;

            await ReplyAsync(
                "",
                false,
                new LocalEmbedBuilder()
                .WithAuthor(user)
                .WithImageUrl(user.GetAvatarUrl())
                .Build());
        }

        [Command("Ping", "Latency")]
        public async Task PingAsync()
        {
            // Latency is null until the first heartbeat sent by the bot.
            var latency = Context.Bot.Latency.HasValue ? "heartbeat: " + Context.Bot.Latency.Value.TotalMilliseconds + "ms, " : "";

            var s = Stopwatch.StartNew();
            var m = await ReplyAsync($"{latency}init: ---, rtt: ---");
            var init = s.ElapsedMilliseconds;
            await m.ModifyAsync(x => x.Content = $"{latency}init: {init}ms, rtt: Calculating");
            s.Stop();
            await m.ModifyAsync(x => x.Content = $"{latency} init: {init}ms, rtt: {s.ElapsedMilliseconds}ms");
        }

        [Command("Help")]
        public async Task HelpAsync()
        {
            await Context.Channel.StartMenuAsync(new HelpMenu(CmdService));
        }

        [Command("Invite")]
        public async Task InviteAsync()
        {
            await ReplyAsync("", false, new LocalEmbedBuilder().WithColor(Color.DarkSlateGray)
                .WithDescription($"https://discordapp.com/oauth2/authorize" +
                $"?client_id={Context.Bot.CurrentUser.Id}" +
                $"&scope=bot&permissions={Constants.BotPermissionLevel}").Build());
        }

        [Command("Stats")]
        public async Task StatsAsync()
        {
            string changes;
            var request = new HttpRequestMessage(HttpMethod.Get, Constants.GithubApiCommitUrl);
            request.Headers
                .Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var response = await HttpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                changes = "There was an error fetching the latest changes.";
            }
            else
            {
                dynamic result = JArray.Parse(await response.Content.ReadAsStringAsync());
                changes =
                    $"[{((string)result[0].sha).Substring(0, 7)}]({result[0].html_url}) {result[0].commit.message}\n" +
                    $"[{((string)result[1].sha).Substring(0, 7)}]({result[1].html_url}) {result[1].commit.message}\n" +
                    $"[{((string)result[2].sha).Substring(0, 7)}]({result[2].html_url}) {result[2].commit.message}";
            }

            var embed = new LocalEmbedBuilder();

            embed.WithAuthor(
                x =>
                {
                    x.IconUrl = Context.Bot.CurrentUser.GetAvatarUrl();
                    x.Name = $"{Context.Bot.CurrentUser.Name}'s Official Invite";
                    x.Url = $"https://discordapp.com/oauth2/authorize" +
                    $"?client_id={Context.Bot.CurrentUser.Id}&scope=bot" +
                    $"&permissions={Constants.BotPermissionLevel}";
                });
            embed.AddField("Changes", changes.FixLength());

            int bots = Context.Bot.Guilds
                .Sum(x => x.Value.Members.Count(z => z.Value?.IsBot == true));
            int humans = Context.Bot.Guilds
                .Sum(x => x.Value.Members.Count(z => z.Value?.IsBot == false));
            int presentUsers = Context.Bot.Guilds
                .Sum(x => x.Value.Members.Count(u => u.Value?.Presence?.Status != UserStatus.Offline));

            embed.AddField(
                "Members",
                $"Bot: {bots}\n" +
                $"Human: {humans}\n" +
                $"Present: {presentUsers}",
                true);

            int online = Context.Bot.Guilds
                .Sum(x => x.Value.Members.Count(z => z.Value?.Presence?.Status == UserStatus.Online));
            int afk = Context.Bot.Guilds
                .Sum(x => x.Value.Members.Count(z => z.Value?.Presence?.Status == UserStatus.Idle));
            int dnd = Context.Bot.Guilds
                .Sum(x => x.Value.Members.Count(z => z.Value?.Presence?.Status == UserStatus.DoNotDisturb));

            embed.AddField(
                "Members",
                $"Online: {online}\n" +
                $"AFK: {afk}\n" +
                $"DND: {dnd}",
                true);

            embed.AddField(
                "Channels",
                $"Text: {Context.Bot.Guilds.Sum(x => x.Value.TextChannels.Count)}\n" +
                $"Voice: {Context.Bot.Guilds.Sum(x => x.Value.VoiceChannels.Count)}\n" +
                $"Total: {Context.Bot.Guilds.Sum(x => x.Value.Channels.Count)}",
                true);

            embed.AddField(
                "Guilds",
                $"Count: {Context.Bot.Guilds.Count}\n" +
                $"Total Users: {Context.Bot.Guilds.Sum(x => x.Value.MemberCount)}\n" +
                $"Total Cached: {Context.Bot.Guilds.Sum(x => x.Value.Members.Count)}\n",
                true);

            embed.AddField(
                "Commands",
                $"Commands: {CmdService.GetAllCommands().Count()}\n" +
                $"Aliases: {CmdService.GetAllCommands().Sum(x => x.Aliases.Count)}\n" +
                $"Modules: {CmdService.GetAllModules().Count()}",
                true);

            embed.AddField(
                ":hammer_pick:",
                $"Heap: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB\n" +
                $"Up: {(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\D\ hh\H\ mm\M\ ss\S")}",
                true);

            embed.AddField(":beginner:", $"Written by: [PassiveModding](https://github.com/PassiveModding)", true);

            await ReplyAsync("", false, embed.Build());
        }
    }
}