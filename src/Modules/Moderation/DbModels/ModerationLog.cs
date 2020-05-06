using Newtonsoft.Json;

namespace Causym.Modules.Moderation.DbModels
{
    public class ModerationLog
    {
        public enum ModerationLogType
        {
            Prune,
            Kick,
            Ban,
            SoftBan,
            Mute,
            Warn
        }

        public ulong ChannelId { get; set; }

        public ulong GuildId { get; set; }

        public ulong ModeratorId { get; set; }

        public string Reason { get; set; }

        public ModerationLogType LogType { get; set; }

        /// <summary>
        /// Json formatted additional metadata relevant to the specific log that was made.
        /// May remove in favor of tables with log specific data.
        /// </summary>
        public string MetaData { get; private set; }

        public void SetMetaData(object data)
        {
            MetaData = JsonConvert.SerializeObject(data);
        }

        public T GetMetaData<T>()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(MetaData);
            }
            catch
            {
                return default;
            }
        }
    }
}