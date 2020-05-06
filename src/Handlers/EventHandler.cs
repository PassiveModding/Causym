using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Sharding;
using Disqord.Events;
using Disqord.Extensions.Passive;
using Passive;
using Passive.Logging;
using Qmmands;

namespace Causym
{
    /// <summary>
    /// Causym eventhandler, handles initial subscriptions to events for logging purposes.
    /// </summary>
    [Service]
    public class EventHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandler"/> class.
        /// </summary>
        /// <param name="bot">The discord bot events should be logged for.</param>
        /// <param name="logger">The log handler, used for handling generic logging.</param>
        public EventHandler(DiscordBotBase bot, Logger logger)
        {
            Bot = bot;
            Logger = logger;

            Bot.Logger.MessageLogged += Logger_MessageLogged;
            if (bot is DiscordBotSharder sharder)
            {
                sharder.ShardReady += Bot_ShardReady;
            }
            Bot.Ready += ReadyAsync;
            Bot.CommandExecuted += CommandExecutedAsync;
            Bot.CommandExecutionFailed += CommandExecutionFailedAsync;

#if DEBUG
            Bot.MessageReceived += Bot_MessageReceived;
#endif
        }

        private DiscordBotBase Bot { get; }

        private Logger Logger { get; }

        private async Task Bot_ShardReady(Disqord.Sharding.ShardReadyEventArgs e)
        {
            Logger.Log($"Shard {e.Shard.Id} Ready, Guilds: {e.Shard.Guilds.Count}", Logger.Source.Bot);
            var prefixResponse = await Bot.PrefixProvider.GetPrefixesAsync(null);
            await e.Shard.SetPresenceAsync(new Disqord.LocalActivity($"{prefixResponse.Last()}help", Disqord.ActivityType.Watching));
        }

        private void Logger_MessageLogged(object sender, Disqord.Logging.MessageLoggedEventArgs e)
        {
            if (e.Severity == Disqord.Logging.LogMessageSeverity.Warning)
            {
                if (e.Message.StartsWith("Close:"))
                {
                    State = ConnectionState.Closed;
                }
            }
            else if (e.Severity == Disqord.Logging.LogMessageSeverity.Information)
            {
                if (e.Message.StartsWith("Resumed."))
                {
                    State = ConnectionState.Connected;
                }
            }

            if (e.Exception != null)
            {
                Logger.Log(e.Message + "\n" + e.Exception.ToString(), e.Source, e.Severity.GetLevel());
                return;
            }

            Logger.Log(e.Message, e.Source, e.Severity.GetLevel());
        }

        private async Task CommandExecutionFailedAsync(CommandExecutionFailedEventArgs e)
        {
            var context = e.Context as DiscordCommandContext;
            Logger.Log(
                $"Command Failed: {e.Context.Command.Name} {e.Result.CommandExecutionStep} {e.Result.Reason}\n" +
                $"{e.Result.Exception.StackTrace}",
                Logger.Source.Cmd);

            bool response = true;

#if DEBUG

#else
            if (context.Guild != null)
            {
                using (var db = new DataContext())
                {
                    var guildConfig = db.Guilds.FirstOrDefault(x => x.GuildId == context.Guild.Id);
                    if (guildConfig != null)
                    {
                        response = guildConfig.RespondOnCommandFailure;
                    }
                }
            }
#endif

            if (!response) return;

            await context.Channel.SendMessageAsync(
                "",
                false,
                new LocalEmbedBuilder()
                .WithTitle($"Command Failed: {e.Context.Command.Name}")
                .AddField("Reason", e.Result.Exception.Message)
                .WithColor(Color.Red)
                .Build());
        }

        private Task CommandExecutedAsync(CommandExecutedEventArgs e)
        {
            Logger.Log($"Command Executed: {e.Context.Command.Name}", Logger.Source.Cmd);
            return Task.CompletedTask;
        }

        public ConnectionState State = ConnectionState.Connected;

        public enum ConnectionState
        {
            Closed,

            Connected
        }

        private Task ReadyAsync(ReadyEventArgs e)
        {
            if (Bot is DiscordBotSharder sharder)
            {
                if (sharder.Shards.Count != 0)
                {
                    Logger.Log($"All Shards Ready ({string.Join(',', sharder.Shards.Select(x => x.Id))})", Logger.Source.Bot);
                }
                else
                {
                    Logger.Log($"All Shards Ready", Logger.Source.Bot);
                }

                Logger.Log($"Total Guilds: {e.Client.Guilds.Count}", Logger.Source.Bot);
            }

            State = ConnectionState.Connected;
            return Task.CompletedTask;
        }

        private Task Bot_MessageReceived(MessageReceivedEventArgs e)
        {
            if (e.Message is CachedUserMessage cM)
            {
                Logger.Log(
                    "Message: " + cM.Content + (cM.Embeds.Count > 0 ? cM.Embeds.Count + " Embed(s)" : ""),
                    Logger.Source.Bot,
                    Logger.LogLevel.Debug);
            }
            else
            {
                Logger.Log("Message: " + e.Message.Content, Logger.Source.Bot, Logger.LogLevel.Debug);
            }

            return Task.CompletedTask;
        }
    }
}