using System.Threading.Tasks;
using Disqord.Bot.Sharding;
using Disqord.Events;
using Qmmands;

namespace Causym
{
    /// <summary>
    /// Causym eventhandler, handles initial subscriptions to events for logging purposes.
    /// </summary>
    public class EventHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandler"/> class.
        /// </summary>
        /// <param name="bot">The discord bot events should be logged for.</param>
        /// <param name="logger">The log handler, used for handling generic logging.</param>
        public EventHandler(DiscordBotSharder bot, Logger logger)
        {
            Bot = bot;
            Logger = logger;
        }

        private DiscordBotSharder Bot { get; }

        private Logger Logger { get; }

        /// <summary>
        /// Subscribes the classes event handlers.
        /// </summary>
        public void Initialize()
        {
            Bot.Ready += ReadyAsync;
            Bot.CommandExecuted += CommandExecutedAsync;
            Bot.CommandExecutionFailed += CommandExecutionFailedAsync;

            // Bot.MessageReceived += Bot_MessageReceived;
            // Bot.Logger.MessageLogged += Logger_MessageLogged;
            Logger.Log($"Initialized", Logger.Source.Bot);
        }

        private void Logger_MessageLogged(object sender, Disqord.Logging.MessageLoggedEventArgs e)
        {
            Logger.Log(e.Message, Logger.Source.Bot);
        }

        private Task CommandExecutionFailedAsync(CommandExecutionFailedEventArgs e)
        {
            Logger.Log($"Command Failed: {e.Context.Command.Name} {e.Result.CommandExecutionStep} {e.Result.Reason}\n{e.Result.Exception}", Logger.Source.Cmd);
            return Task.CompletedTask;
        }

        private Task CommandExecutedAsync(CommandExecutedEventArgs e)
        {
            Logger.Log($"Command Executed: {e.Context.Command.Name}", Logger.Source.Cmd);
            return Task.CompletedTask;
        }

        private Task ReadyAsync(ReadyEventArgs e)
        {
            /*if (e is ShardReadyEventArgs s)
            {
                Logger.Log($"Shard {s.ShardId} Ready", Logger.Source.Bot);
            }
            else
            {*/
            Logger.Log($"Ready", Logger.Source.Bot);
            Logger.Log($"Guilds: {e.Client.Guilds.Count}", Logger.Source.Bot);
            Bot.SetPresenceAsync(new Disqord.LocalActivity("?help", Disqord.ActivityType.Watching));
            return Task.CompletedTask;
        }

        private Task Bot_MessageReceived(MessageReceivedEventArgs e)
        {
            Logger.Log(e.Message.Content, Logger.Source.Bot);
            return Task.CompletedTask;
        }
    }
}
