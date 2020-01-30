using System;

namespace Causym.Modules.Statistics
{
    public class StatisticsSnapshot
    {
        public ulong GuildId { get; set; }

        public int MemberCount { get; set; }

        public DateTime SnapshotTime { get; set; } = DateTime.Now;

        public int TotalMessageCount { get; set; } = 0;

        public int CachedMembers { get; set; }

        public int MembersOnline { get; set; }

        public int MembersDND { get; set; }

        public int MembersIdle { get; set; }
    }
}
