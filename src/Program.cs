using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Causym.Services;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Disqord.Bot.Sharding;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Passive;
using Passive.Discord;
using Passive.Discord.Setup;
using Passive.Logging;

namespace Causym
{
    public class Program
    {
        private readonly Logger logger = new Logger(Logger.LogLevel.Info, Path.Combine(AppContext.BaseDirectory, "logs"));

        private string[] args;

        private DiscordBotBase bot;

        private int closedCounter = 0;

        private int closeInterval = 30000;

        private Timer internalWatchdog;

        private int watchDogInit = 60000;

        private int watchdogInterval = 30000;

        private bool WatchDogManaging = false;

        public Program()
        {
            internalWatchdog = new Timer(RunWatchDog, null, -1, watchdogInterval);
        }

        public static void Main(string[] args = null)
        {
            new Program()
            {
                args = args
            }.RunAsync().GetAwaiter().GetResult();
        }

        public async Task<DiscordBotBase> ConfigureAsync()
        {
            var config = Config.ParseArguments(args);
            DbConnection.Initialize(config);

            using (var db = new DataContext())
            {
                await db.Database.MigrateAsync();

                await db.SaveChangesAsync();
            }

            var shardCount = int.Parse(config.GetOrAddEntry(Config.Defaults.ShardCount.ToString(), () =>
            {
                logger.Log(
                    $"Please input desired shard count (discord allows a maximum of 2500 guilds per shard): ",
                    Logger.Source.Bot);

                string value;

                // Iterate until valid integer is provided.
                do
                {
                    value = Console.ReadLine();
                }
                while (!int.TryParse(value, out var x) || x <= 0);

                return value;
            }));

            bool multiProcessSharding = false;
            if (shardCount > 1)
            {
                multiProcessSharding = bool.Parse(config.GetOrAddEntry("MultiProcess", () =>
                {
                    logger.Log(
                        $"Using multi-process sharding? (true/false): ",
                        Logger.Source.Bot);

                    string value;

                    do
                    {
                        value = Console.ReadLine();
                    }
                    while (!bool.TryParse(value, out var x));

                    return value;
                }));
            }

            var token = config.GetOrAddEntry(Config.Defaults.Token.ToString(), () =>
            {
                logger.Log(
                    $"Please input bot token, can be found at: " +
                    $"{Constants.DeveloperApplicationLink}",
                    Logger.Source.Bot);
                return Console.ReadLine();
            });
            var prefix = config.GetOrAddEntry(Config.Defaults.Prefix.ToString(), () =>
            {
                logger.Log("Please input bot prefix",
                    Logger.Source.Bot);
                return Console.ReadLine();
            });

            IServiceCollection botServiceCollection = new ServiceCollection()
                .AddSingleton<HttpClient>();

            // Gets services marked with the Service attribute, adds them to the service collection
            var services = Assembly.GetEntryAssembly().GetServices();
            foreach (var type in services)
            {
                botServiceCollection = botServiceCollection.AddSingleton(type);
            }

            var prefixProvider = new DefaultPrefixProvider().AddPrefix(prefix);
            DiscordBotBase bot;
            if (!multiProcessSharding)
            {
                bot = new DiscordBotSharder(
                    TokenType.Bot,
                    token,
                    prefixProvider,
                    new DiscordBotSharderConfiguration
                    {
                        ProviderFactory = bot => botServiceCollection
                            .AddSingleton(bot)
                            .AddSingleton(config)
                            .AddSingleton(logger)
                            .BuildServiceProvider(),
                        ShardCount = shardCount
                    });
            }
            else
            {
                bot = new DiscordBot(TokenType.Bot, token, prefixProvider, new DiscordBotConfiguration()
                {
                    ProviderFactory = bot => botServiceCollection
                       .AddSingleton(bot)
                       .AddSingleton(config)
                       .AddSingleton(logger)
                       .BuildServiceProvider(),
                    ShardCount = shardCount,
                    ShardId = int.Parse(config.GetOrAddEntry("ShardId", () =>
                    {
                        logger.Log(
                            $"Please input shard ID:",
                            Logger.Source.Bot);

                        string value;

                        // Iterate until valid integer is provided.
                        do
                        {
                            value = Console.ReadLine();
                        }
                        while (!int.TryParse(value, out var x) || x < 0);

                        return value;
                    }))
                });
            }

            bot.AddTypeParser(new IEmojiParser(bot.GetRequiredService<HttpClient>()));
            bot.AddTypeParser(new Disqord.Extensions.Parsers.TimeSpanParser());
            await bot.AddExtensionAsync(new InteractivityExtension());

            // Initializes services marked with the Service attribute from the
            // service collection in order to initialize them
            foreach (var type in services)
            {
                var service = bot.GetService(type);
                if (service == null)
                {
                    logger.Log(
                        $"Service of type {type.Name} " +
                      $"not found in bot service provider despite being marked " +
                      $"with a service attribute",
                        Logger.Source.Bot,
                        Logger.LogLevel.Warn);
                }
            }

            bot.AddModules(Assembly.GetEntryAssembly());

            return bot;
        }

        public async Task RunAsync()
        {
            bot = await ConfigureAsync();
            internalWatchdog.Change(watchDogInit, -1);
            try
            {
                await bot.RunAsync();
            }
            catch (ObjectDisposedException ex)
            {
                //
            }
            catch (TaskCanceledException)
            {
                //
            }

            await Task.Delay(5000);
            while (WatchDogManaging)
            {
                try
                {
                    await bot.RunAsync();
                }
                catch (ObjectDisposedException)
                {
                    //
                }
                catch (TaskCanceledException)
                {
                    //
                }
                catch (Exception e)
                {
                    logger.Log(e.ToString(), "Connection", Logger.LogLevel.Error);
                    break;
                }
                await Task.Delay(5000);
            }
        }

        private async void RunWatchDog(object stateInfo = null)
        {
            if (bot.GetRequiredService<EventHandler>().State == EventHandler.ConnectionState.Closed)
            {
                closedCounter++;

                // Check every 30 seconds for closed.
                internalWatchdog.Change(closeInterval, -1);
                logger.Log($"Watchdog close counter incremented {closedCounter}", "Watchdog", Logger.LogLevel.Warn);
            }
            else
            {
                closedCounter = 0;
                internalWatchdog.Change(watchdogInterval, -1);
            }

            if (closedCounter >= 5)
            {
                logger.Log($"Watchdog disposing bot.", "Watchdog", Logger.LogLevel.Error);
                internalWatchdog.Change(-1, -1);

                // reboot bot.
                try
                {
                    await bot.StopAsync();
                }
                catch
                {
                    //
                }
                await bot.DisposeAsync();
                closedCounter = 0;
                bot = await ConfigureAsync();
                WatchDogManaging = true;
                internalWatchdog.Change(watchdogInterval, -1);
            }
        }
    }
}