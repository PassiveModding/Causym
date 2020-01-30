using System;

namespace Causym.Modules.Statistics
{
    public class ChannelSnapshot
    {
        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }

        public int MessageCount { get; set; }

        public DateTime SnapshotTime { get; set; }
    }
}
