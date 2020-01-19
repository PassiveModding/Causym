using System;
using System.Collections.Generic;
using System.Text;

namespace Causym.Modules.Ticketing.Models
{
    public class Vote
    {
        public ulong UserId { get; set; }

        public ulong TicketId { get; set; }

        public bool Upvote { get; set; }
    }
}
