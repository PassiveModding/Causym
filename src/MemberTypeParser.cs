using Disqord;
using Disqord.Bot.Parsers;
using Qmmands;
using System.Threading.Tasks;
using Disqord.Bot;

namespace Causym
{
    public class MemberTypeParser : TypeParser<IMember>
    {
        private readonly CachedMemberTypeParser _CachedMemberParser;

        public MemberTypeParser()
        {
            _CachedMemberParser = new CachedMemberTypeParser();
        }

        public override async ValueTask<TypeParserResult<IMember>> ParseAsync(
            Parameter parameter,
            string value,
            CommandContext context)
        {

            if (!(context is DiscordCommandContext ctx))
            {
                return TypeParserResult<IMember>.Unsuccessful("Context not of of type " + typeof(DiscordCommandContext) + " found " + context.GetType().Name);
            }

            if (ctx.Guild == null)
            {
                return TypeParserResult<IMember>.Unsuccessful("Context not within guild");
            }

            // Attempt pulling member from cache before requesting rest member info
            var cachedResult = await _CachedMemberParser.ParseAsync(parameter, value, context);

            if (cachedResult.IsSuccessful)
            {
                return TypeParserResult<IMember>.Successful(cachedResult.Value);
            }

            // Since cache parser had no hit, parse user id and request directly
            if (!Discord.TryParseUserMention(value, out var id) && !Snowflake.TryParse(value, out id))
            {
                return TypeParserResult<IMember>.Unsuccessful("Member snowflake not able to be parsed");
            }

            var member = await ctx.Guild.GetMemberAsync(id);
            if (member == null)
            {
                return TypeParserResult<IMember>.Unsuccessful("Member not found");
            }

            return TypeParserResult<IMember>.Successful(member);
        }
    }
}