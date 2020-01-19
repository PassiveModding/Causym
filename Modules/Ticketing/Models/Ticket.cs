using System;

namespace Causym.Modules.Ticketing.Models
{
    public class Ticket
    {
        public ulong MessageId { get; set; }

        public string TicketType { get; set; }

        public string Content { get; set; }

        public string ClosingResponse { get; set; }

        public ulong TicketChannelId { get; set; }

        public ulong GuildId { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdate { get; set; }
    }
}
