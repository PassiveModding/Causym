namespace Causym.Modules.Configure
{
    /// <summary>
    /// Default guild configuration for settings used on a bot (non module) level.
    /// </summary>
    public class GuildConfiguration
    {
        public ulong GuildId { get; set; }

        public string Prefix { get; set; }
    }
}