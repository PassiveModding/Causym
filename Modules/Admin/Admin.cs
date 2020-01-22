﻿using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;

namespace Causym.Modules.Admin
{
    [BotOwnerOnly]
    public class Admin : DiscordModuleBase
    {
        [Command("SetUsername")]
        public async Task SetUserNameAsync([Remainder]string username)
        {
            await Context.Bot.ModifyCurrentUserAsync(x =>
            {
                x.Name = username;
            });
            await ReplyAsync("Username updated.");
        }
    }
}
