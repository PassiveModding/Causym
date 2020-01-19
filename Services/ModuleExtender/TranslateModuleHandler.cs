using System.Linq;

namespace Causym.Services.ModuleExtender
{
    public class TranslateModuleHandler
    {
        public void GetLanguage(Disqord.Bot.DiscordCommandContext context)
        {
            using (var db = new DataContext())
            {
                Language languageOverride = db.LanguageOverrides.FirstOrDefault(x => x.Id == context.User.Id.RawValue);

                if (languageOverride != null)
                {
                    languageOverride = db.LanguageOverrides.FirstOrDefault(x => x.Id == context.Channel.Id.RawValue);

                    if (languageOverride == null && context.Guild != null)
                    {
                        languageOverride = db.LanguageOverrides.FirstOrDefault(x => x.Id == context.Guild.Id);
                    }
                }

                if (languageOverride == null)
                {
                }
            }
        }

        public class Language
        {
            public enum OverrideType
            {
                User,
                Channel,
                Guild
            }

            public ulong Id { get; set; }

            public OverrideType IdType { get; set; }

            public string LanguageIdentifier { get; set; }
        }
    }
}
