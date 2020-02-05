using System;
using Newtonsoft.Json;

namespace Causym.Services.EmojiParser
{
    public class EmojiVersion
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("versionTimestamp")]
        public DateTimeOffset VersionTimestamp { get; set; }

        [JsonProperty("emojiDefinitions")]
        public EmojiDefinition[] EmojiDefinitions { get; set; }
    }
}