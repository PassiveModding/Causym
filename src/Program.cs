using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Causym.Services;
using CommandLine;
using Disqord;
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

        public static void Main(string[] args = null)
        {
            new Program().RunAsync(args).GetAwaiter().GetResult();
        }

        public async Task RunAsync(string[] args = null)
        {
            var config = Config.ParseArguments(args);

            using (var db = new DataContext())
            {
                await db.Database.MigrateAsync();

                await db.SaveChangesAsync();
            }

            IServiceCollection botServiceCollection = new ServiceCollection()
                .AddSingleton<HttpClient>();

            // Gets services marked with the Service attribute, adds them to the service collection
            var services = Assembly.GetEntryAssembly().GetServices();
            foreach (var type in services)
            {
                botServiceCollection = botServiceCollection.AddSingleton(type);
            }

            var bot = new DiscordBotSharder(
                TokenType.Bot,
                config.GetOrAddEntry(Config.Defaults.Token.ToString(), () =>
                {
                    logger.Log(
                        $"Please input bot token, can be found at: " +
                        $"{Constants.DeveloperApplicationLink}",
                        Logger.Source.Bot);
                    return Console.ReadLine();
                }),
                new DatabasePrefixProvider(config.GetOrAddEntry(Config.Defaults.Prefix.ToString(), () =>
                {
                    logger.Log($"Please input bot default prefix:", Logger.Source.Bot);
                    return Console.ReadLine();
                })),
                new DiscordBotSharderConfiguration
                {
                    ProviderFactory = bot => botServiceCollection
                        .AddDbContext<DataContext>(ServiceLifetime.Transient)
                        .AddSingleton(bot as DiscordBotSharder)
                        .AddSingleton(config)
                        .AddSingleton(logger)
                        .BuildServiceProvider(),
                    ShardCount = int.Parse(config.GetOrAddEntry(Config.Defaults.ShardCount.ToString(), () => "1"))
                });

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
            await bot.RunAsync();
        }
    }
}