using System;
using System.ComponentModel.DataAnnotations;

namespace Causym.Modules.Statistics
{
    public class StatisticsSnapshot
    {
        public ulong GuildId { get; set; }

        public int MemberCount { get; set; }

        [Required]
        public DateTime SnapshotTime { get; set; } = DateTime.UtcNow;

        public int TotalMessageCount { get; set; } = 0;

        public int CachedMembers { get; set; }

        public int MembersOnline { get; set; }

        public int MembersDND { get; set; }

        public int MembersIdle { get; set; }
    }
}