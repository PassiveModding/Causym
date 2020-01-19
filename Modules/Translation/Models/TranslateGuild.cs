namespace Causym.Translation
{
    public class TranslateGuild
    {
        public enum FilterMode
        {
            Blacklist,
            Whitelist,
            None
        }

        public ulong GuildId { get; set; }

        public bool ReactionsEnabled { get; set; } = true;
    }
}
