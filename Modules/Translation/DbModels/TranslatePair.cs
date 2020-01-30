using System.ComponentModel.DataAnnotations;

namespace Causym.Modules.Translation
{
    public class TranslatePair
    {
        public ulong GuildId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Source { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string DestLang { get; set; }
    }
}
