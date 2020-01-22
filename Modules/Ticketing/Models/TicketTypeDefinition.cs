using System;
using System.Collections.Generic;
using System.Text;

namespace Causym.Modules.Ticketing.Models
{
    public class TicketTypeDefinition
    {
        public ulong GuildId { get; set; }

        public ulong? ChannelId { get; set; }

        public string TicketType { get; set; }
    }
}
