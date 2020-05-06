using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Prefixes;

namespace Causym.Services
{
    /// <summary>
    /// A prefix provider that utilizes <see cref="DataContext"/> to get guild specific prefixes.
    /// </summary>
    public class DatabasePrefixProvider : IPrefixProvider
    {
        public DatabasePrefixProvider(string defaultPrefix)
        {
            DefaultPrefix = defaultPrefix;
        }

        public string DefaultPrefix { get; }

        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(CachedUserMessage message)
        {
            // By default always accept a bot mention as a valid prefix.
            var prefixes = new List<IPrefix>(1)
            {
                MentionPrefix.Instance
            };

            // Ensure the message exists and was sent within a guild.
            if (message?.Guild != null)
            {
                using (var db = new DataContext())
                {
                    // Attempt to find a matching guild config from the db
                    var guild = db.Guilds.FirstOrDefault(g => g.GuildId == message.Guild.Id);

                    // Check to ensure the guild has a valid prefix override set.
                    if (guild?.Prefix != null)
                    {
                        // Add the overridden prefix to the prefix list.
                        prefixes.Add(new StringPrefix(guild.Prefix));
                    }
                }
            }

            // Prefix list size will only be 1 if there is no guild override prefix
            if (prefixes.Count == 1)
            {
                prefixes.Add(new StringPrefix(DefaultPrefix));
            }

            return new ValueTask<IEnumerable<IPrefix>>(prefixes);
        }
    }
}