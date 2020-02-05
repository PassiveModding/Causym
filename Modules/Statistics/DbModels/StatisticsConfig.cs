namespace Causym.Modules.Statistics
{
    public class StatisticsConfig
    {
        public ulong GuildId { get; set; }

        public ulong? MemberChannelId { get; set; } = null;

        public bool SnapshotsEnabled { get; set; } = false;
    }
}