﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Causym.Services.Help;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Qmmands;

namespace Causym.Modules.General
{
    public class General : DiscordModuleBase
    {
        public General(CommandService cmdService)
        {
            CmdService = cmdService;
        }

        public CommandService CmdService { get; }

        [Command("Avatar")]
        public async Task AvatarAsync(CachedUser user = null)
        {
            if (user == null) user = Context.User;

            await ReplyAsync("", false, new LocalEmbedBuilder().WithAuthor(user).WithImageUrl(user.GetAvatarUrl()).Build());
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
            await Context.Channel.StartMenuAsync(new HelpService.HelpMenu(CmdService));
        }
    }
}
