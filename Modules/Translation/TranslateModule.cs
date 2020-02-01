using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Causym.Services.Help;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Causym.Modules.Translation
{
    /// <summary>
    /// Translate module, handles configuration and translation of messages.
    /// </summary>
    [Group("Translate", "T")]
    [ModuleButton("🌐")]
    public class TranslateModule : DiscordModuleBase
    {
        /// <summary>
        /// Gets or sets the translate service, used for translating text and embeds.
        /// </summary>
        public TranslateService TranslateService { get; set; }

        [Command]
        [Description("Translates the given message into the specified language")]
        [Remarks("The target language must be a valid language identifier")]
        public async Task TranslateAsync(string targetLang, [Remainder]string message)
        {
            var response = TranslateService.Translate(message, targetLang);
            if (response.ResponseResult == TranslateResponse.Result.Success)
            {
                await ReplyAsync("", false, TranslateService.GetTranslationEmbed(response, Context.Message).Build());
            }
        }

        [Command("Languages", "Lang")]
        [Description("Displays all language codes the bot accepts")]
        public async Task LanguagesAsync()
        {
            var embed = new LocalEmbedBuilder();
            foreach (var languageGroup in TranslateService.GetAvailableLanguages().GroupBy(x => x.BaseCulture.Name.ToCharArray().First()).OrderBy(x => x.Key))
            {
                var resContent = languageGroup.Select(x => $"`{x.BaseCulture.Name}` - **{x.BaseCulture.DisplayName}** {x.BaseCulture.NativeName}").ToArray();
                embed.AddField(languageGroup.Key.ToString(CultureInfo.CurrentCulture), string.Join("\n", resContent));
            }

            await ReplyAsync("", false, embed.Build());
        }

        [Command("Help", "HelpMe", "Commands")]
        public async Task HelpAsync()
        {
            await ReplyAsync("", false, HelpService.GetModuleHelp(Context.Command.Module).Build());
        }

        [Command("Reactions")]
        [Description("Updates or displays the current setting for Reaction translations")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        [GuildOnly]
        public async Task ReactionsAsync(bool? setting = null)
        {
            using (var db = new DataContext())
            {
                var match = db.TranslateGuilds.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
                if (match == null)
                {
                    match = new TranslateGuild()
                    {
                        GuildId = Context.Guild.Id,

                        // Enable by default.
                        ReactionsEnabled = setting ?? true
                    };
                    db.TranslateGuilds.Add(match);
                }
                else
                {
                    if (setting.HasValue)
                    {
                        match.ReactionsEnabled = setting.Value;
                        db.TranslateGuilds.Update(match);
                    }
                    else
                    {
                        // Display setting value
                        await ReplyAsync("", false, new LocalEmbedBuilder().WithDescription($"Reaction Translations Enabled: {match.ReactionsEnabled}").Build());
                        return;
                    }
                }

                await db.SaveChangesAsync();
                await ReplyAsync("", false, new LocalEmbedBuilder().WithDescription($"Reaction Translations Enabled: {match.ReactionsEnabled}").Build());
            }
        }

        [Command("AddEmoji")]
        [Description("Adds a pair for reaction translations")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        [GuildOnly]
        public async Task AddEmoteAsync(string code, IEmoji emote)
        {
            await AddEmoteAsync(emote, code);
        }

        [Command("AddEmoji")]
        [Description("Adds a pair for reaction translations")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        [GuildOnly]
        public async Task AddEmoteAsync(IEmoji emote, string code)
        {
            if (!TranslateService.IsValidLanguageCode(code))
            {
                await ReplyAsync("Language code is not valid.");
                return;
            }

            using (var db = new DataContext())
            {
                var match = db.TranslatePairs.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Source == emote.ReactionFormat);
                if (match != null)
                {
                    await ReplyAsync("This emote is already configured to translate to another language.");
                    return;
                }
                else
                {
                    db.TranslatePairs.Add(new TranslatePair
                    {
                        GuildId = Context.Guild.Id,
                        Source = emote.ReactionFormat,
                        DestLang = code
                    });
                    await db.SaveChangesAsync();

                    await ReplyAsync($"{emote.MessageFormat} reactions will now translate messages to {code}");
                }
            }
        }

        [Command("RemoveEmoji")]
        [Description("Removes a configured emoji for reaction translations")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        [GuildOnly]
        public async Task RemoveEmojiAsync(IEmoji emote)
        {
            using (var db = new DataContext())
            {
                var match = db.TranslatePairs.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Source == emote.ReactionFormat);
                if (match == null)
                {
                    await ReplyAsync("This emoji does not have a matching lanuage.");
                    return;
                }
                else
                {
                    db.TranslatePairs.Remove(match);
                    await db.SaveChangesAsync();

                    await ReplyAsync($"Emoji removed.");
                }
            }
        }

        [Command("Emojis")]
        [Description("Displays all configured emojis for reaction translations")]
        [GuildOnly]
        public async Task Emojis()
        {
            using (var db = new DataContext())
            {
                var embed = new LocalEmbedBuilder();

                if (Context.Guild != null)
                {
                    var matches = db.TranslatePairs.Where(x => x.GuildId == Context.Guild.Id).ToArray().GroupBy(x => x.DestLang);
                    embed.Description = string.Join("\n", matches.Select(x => $"{x.Key}: {string.Join(" ", x.Select(p => p.Source))}")).FixLength(2047);
                }

                embed.AddField("Defaults", string.Join("\n", TranslateService.DefaultMap.Select(x => $"{x.LanguageString}: {string.Join(" ", x.EmoteMatches)}")));
                await ReplyAsync("", false, embed.Build());
            }
        }
    }
}
