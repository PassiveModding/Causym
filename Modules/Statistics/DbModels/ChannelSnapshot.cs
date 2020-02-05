using System;
using System.ComponentModel.DataAnnotations;

namespace Causym.Modules.Statistics
{
    public class ChannelSnapshot
    {
        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }

        public int MessageCount { get; set; }

        [Required]
        public DateTime SnapshotTime { get; set; } = DateTime.UtcNow;
    }
}