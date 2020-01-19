using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;

namespace Causym.Services.ModuleExtender
{
    public class TranslateModuleBase : DiscordModuleBase
    {
        // TODO: Implement storage of user/server/channel language settings
        // Translate responses appropriately for each
        // User > Channel > Server
        protected override Task<RestUserMessage> ReplyAsync(LocalAttachment attachment, string content = null, bool isTts = false, LocalEmbed embed = null, RestRequestOptions options = null)
        {
            return base.ReplyAsync(attachment, content, isTts, embed, options);
        }

        protected override Task<RestUserMessage> ReplyAsync(IEnumerable<LocalAttachment> attachments, string content = null, bool isTts = false, LocalEmbed embed = null, RestRequestOptions options = null)
        {
            return base.ReplyAsync(attachments, content, isTts, embed, options);
        }

        protected override Task<RestUserMessage> ReplyAsync(string content = null, bool isTts = false, LocalEmbed embed = null, RestRequestOptions options = null)
        {
            return base.ReplyAsync(content, isTts, embed, options);
        }
    }
}
