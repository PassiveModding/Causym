using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Prefixes;

namespace Causym.Services
{
    public class DatabasePrefixProvider : IPrefixProvider
    {
        public DatabasePrefixProvider(string defaultPrefix)
        {
            DefaultPrefix = defaultPrefix;
        }

        public string DefaultPrefix { get; }

        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(CachedUserMessage message)
        {
            var prefixes = new List<IPrefix>(1)
            {
                MentionPrefix.Instance
            };

            if (message.Guild != null)
            {
                using (var db = new DataContext())
                {
                    var guild = db.Guilds.FirstOrDefault(g => g.GuildId == message.Guild.Id);
                    if (guild != null && guild.Prefix != null)
                    {
                        prefixes.Add(new StringPrefix(guild.Prefix));
                    }
                }
            }

            if (prefixes.Count == 1)
            {
                // Prefix list size will only be 1 if there is no guild override prefix
                prefixes.Add(new StringPrefix(DefaultPrefix));
            }

            return new ValueTask<IEnumerable<IPrefix>>(prefixes);
        }
    }
}
