using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Causym.Services;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Disqord.Rest;

namespace Causym.Modules.Translation
{
    /// <summary>
    /// Contains general methods, properties and fields for the translate service.
    /// </summary>
    [Service]
    public partial class TranslateService
    {
        private readonly YandexTranslator translator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateService"/> class.
        /// </summary>
        /// <param name="bot">The discord bot.</param>
        /// <param name="config">The bot's default configuration.</param>
        /// <param name="logger">The log handler.</param>
        public TranslateService(DiscordBotBase bot, Config config, Logger logger)
        {
            Config = config;
            Logger = logger;
            if (!Config.Entries.ContainsKey("YandexKey"))
            {
                logger.Log("Enter Yandex api key", "TRANSLATE", Logger.LogLevel.Info);
                var key = Console.ReadLine();
                Config.Entries.Add("YandexKey", key);
                Config.Save();
            }

            var apiKey = Config.Entries["YandexKey"];
            translator = new YandexTranslator(apiKey);
            bot.ReactionAdded += ReactionAddedAsync;
        }

        public static readonly List<TranslationSet> DefaultMap =
            new List<TranslationSet>
            {
                        new TranslationSet { EmoteMatches = new List<string> { "🇦🇺", "🇺🇸", "🇪🇺", "🇳🇿" }, LanguageString = "en" },
                        new TranslationSet { EmoteMatches = new List<string> { "🇭🇺" }, LanguageString = "hu" },
                        new TranslationSet { EmoteMatches = new List<string> { "🇫🇷" }, LanguageString = "fr" },
                        new TranslationSet { EmoteMatches = new List<string> { "🇫🇮" }, LanguageString = "fi" },
                        new TranslationSet { EmoteMatches = new List<string> { "🇲🇽", "🇪🇸", "🇨🇴", "🇦🇷" }, LanguageString = "es" },
                        new TranslationSet { EmoteMatches = new List<string> { "🇧🇷", "🇵🇹", "🇲🇿", "🇦🇴" }, LanguageString = "pt" },
                        new TranslationSet { EmoteMatches = new List<string> { "🇩🇪", "🇦🇹", "🇨🇭", "🇧🇪", "🇱🇺", "🇱🇮" }, LanguageString = "de" },
                        new TranslationSet { EmoteMatches = new List<string> { "🇮🇹", "🇨🇭", "🇸🇲", "🇻🇦" }, LanguageString = "it" },
                        new TranslationSet { EmoteMatches = new List<string> { "🇨🇳", "🇸🇬", "🇹🇼" }, LanguageString = "zh" },
                        new TranslationSet { EmoteMatches = new List<string> { "🇯🇵" }, LanguageString = "ja" }
            };

        public Config Config { get; }

        public Logger Logger { get; }

        public SpecificCulture[] GetAvailableLanguages()
        {
            return translator.GetAvailableLanguages();
        }

        public bool IsValidLanguageCode(string code)
        {
            return translator.IsValidLanguageCode(code);
        }

        public static TranslationSet GetCode(string reaction)
        {
            var languageType = DefaultMap.FirstOrDefault(x => x.EmoteMatches.Any(val => val == reaction));
            if (languageType == null)
            {
                return null;
            }

            return languageType;
        }

        private async Task ReactionAddedAsync(ReactionAddedEventArgs e)
        {
            if (!e.User.HasValue) return;
            if (e.User.Value.IsBot) return;
            if (e.Emoji == null) return;

            // Check config to see if reactions are enabled, check reaction is a translate pair for the guild
            // Respond with translated message.
            string destLang;
            if (e.Channel is IGuildChannel gChannel)
            {
                var guildId = gChannel.GuildId;
                using (var db = new DataContext())
                {
                    var guild = db.TranslateGuilds.FirstOrDefault(x => x.GuildId == guildId);
                    if (guild == null) return;

                    if (guild.ReactionsEnabled == false) return;

                    var reactionOverride = db.TranslatePairs.FirstOrDefault(x => x.GuildId == guildId && x.Source == e.Emoji.ReactionFormat);
                    if (reactionOverride == null)
                    {
                        // Match against default map
                        destLang = GetCode(e.Emoji.ReactionFormat)?.LanguageString;
                    }
                    else
                    {
                        destLang = reactionOverride.DestLang;
                    }

                    // TODO: Implement guild backlist/whitelist roles
                }
            }
            else
            {
                // Context should be in dm channel, bot should just respond with the translated message
                destLang = GetCode(e.Emoji.ReactionFormat)?.LanguageString;
            }

            if (destLang == null) return;
            var msg = await e.Message.FetchAsync();
            if (msg == null) return;
            if (!(msg is RestUserMessage message)) return;

            if (message.Content.Length > 0)
            {
                var result = Translate(message.Content, destLang);
                if (result.ResponseResult == TranslateResponse.Result.Success)
                {
                    var embed = GetTranslationEmbed(result, message);
                    await e.Channel.SendMessageAsync("", false, embed.Build());
                }
            }

            foreach (var embed in message.Embeds)
            {
                if (embed.IsRich)
                {
                    var response = TranslateEmbed(embed, destLang, message);
                    if (response != null)
                    {
                        await e.Channel.SendMessageAsync("", false, response.Build());
                    }
                }
            }
        }
    }
}