using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Sharding;
using Disqord.Events;
using Disqord.Extensions.Passive;
using Disqord.Logging;
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

            Bot.Logger.Logged += Logger_MessageLogged;
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
            Logger.Log("Connection", $"Shard {e.Shard.Id} Ready, Guilds: {e.Shard.Guilds.Count}", LogSeverity.Information);
            var prefixResponse = await Bot.PrefixProvider.GetPrefixesAsync(null);
            await e.Shard.SetPresenceAsync(new Disqord.LocalActivity($"{prefixResponse.Last()}help", Disqord.ActivityType.Watching));
        }

        private void Logger_MessageLogged(object sender, Disqord.Logging.LogEventArgs e)
        {
            if (e.Severity == Disqord.Logging.LogSeverity.Warning)
            {
                if (e.Message.StartsWith("Close:"))
                {
                    State = ConnectionState.Closed;
                }
            }
            else if (e.Severity == Disqord.Logging.LogSeverity.Information)
            {
                if (e.Message.StartsWith("Resumed."))
                {
                    State = ConnectionState.Connected;
                }
            }
        }

        private async Task CommandExecutionFailedAsync(CommandExecutionFailedEventArgs e)
        {
            var context = e.Context as DiscordCommandContext;
            Logger.Log("Command",
                $"Failed: {e.Context.Command.Name} {e.Result.CommandExecutionStep} {e.Result.Reason}\n" +
                $"{e.Result.Exception.StackTrace}",
                LogSeverity.Warning);

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
            Logger.Log("Command", $"Command Executed: {e.Context.Command.Name}", LogSeverity.Information);
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
                    Logger.Log("Connection", $"All Shards Ready ({string.Join(',', sharder.Shards.Select(x => x.Id))})", LogSeverity.Information);
                }
                else
                {
                    Logger.Log("Connection", "All Shards Ready", LogSeverity.Information);
                }

                Logger.Log("Connection", $"Total Guilds: {e.Client.Guilds.Count}", LogSeverity.Information);
            }

            State = ConnectionState.Connected;
            return Task.CompletedTask;
        }

        private Task Bot_MessageReceived(MessageReceivedEventArgs e)
        {
            if (e.Message is CachedUserMessage cM)
            {
                Logger.Log(
                    "Bot", 
                    "Message: " + cM.Content + (cM.Embeds.Count > 0 ? cM.Embeds.Count + " Embed(s)" : ""),
                    LogSeverity.Trace);
            }
            else
            {
                Logger.Log("Bot", "Message: " + e.Message.Content, LogSeverity.Trace);
            }

            return Task.CompletedTask;
        }
    }
}