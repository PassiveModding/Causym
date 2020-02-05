﻿using System.Linq;
using System.Threading.Tasks;
using Causym.Services;
using Disqord;
using Disqord.Bot.Sharding;
using Disqord.Events;
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
        public EventHandler(DiscordBotSharder bot, Logger logger)
        {
            Bot = bot;
            Logger = logger;

            Bot.ShardReady += Bot_ShardReady;
            Bot.Ready += ReadyAsync;
            Bot.CommandExecuted += CommandExecutedAsync;
            Bot.CommandExecutionFailed += CommandExecutionFailedAsync;

#if DEBUG
            Bot.MessageReceived += Bot_MessageReceived;
#endif
        }

        private DiscordBotSharder Bot { get; }

        private Logger Logger { get; }

        private Task Bot_ShardReady(Disqord.Sharding.ShardReadyEventArgs e)
        {
            Logger.Log($"Shard {e.Shard.Id} Ready, Guilds: {e.Shard.Guilds.Count}", Logger.Source.Bot);
            e.Shard.SetPresenceAsync(new Disqord.LocalActivity("?help", Disqord.ActivityType.Watching));
            return Task.CompletedTask;
        }

        private void Logger_MessageLogged(object sender, Disqord.Logging.MessageLoggedEventArgs e)
        {
            Logger.Log(e.Message, Logger.Source.Bot, Logger.LogLevel.Verbose);
        }

        private Task CommandExecutionFailedAsync(CommandExecutionFailedEventArgs e)
        {
            Logger.Log(
                $"Command Failed: {e.Context.Command.Name} {e.Result.CommandExecutionStep} {e.Result.Reason}\n" +
                $"{e.Result.Exception}",
                Logger.Source.Cmd);
            return Task.CompletedTask;
        }

        private Task CommandExecutedAsync(CommandExecutedEventArgs e)
        {
            Logger.Log($"Command Executed: {e.Context.Command.Name}", Logger.Source.Cmd);
            return Task.CompletedTask;
        }

        private Task ReadyAsync(ReadyEventArgs e)
        {
            if (Bot.Shards.Count != 0)
            {
                Logger.Log($"All Shards Ready ({string.Join(',', Bot.Shards.Select(x => x.Id))})", Logger.Source.Bot);
            }
            else
            {
                Logger.Log($"All Shards Ready", Logger.Source.Bot);
            }

            Logger.Log($"Total Guilds: {e.Client.Guilds.Count}", Logger.Source.Bot);
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