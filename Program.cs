using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Causym.Services;
using CommandLine;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Sharding;
using Disqord.Extensions.Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Causym
{
    public class Program
    {
        private readonly Logger logger = new Logger();
        private Config config;

        public static void Main(string[] args = null)
        {
            new Program().RunAsync(args).GetAwaiter().GetResult();
        }

        public async Task RunAsync(string[] args = null)
        {
            ParseArguments(args);

            using (var db = new DataContext())
            {
                await db.Database.MigrateAsync();

                await db.SaveChangesAsync();
            }

            var bot = new DiscordBotSharder(TokenType.Bot, config.Entries[Config.Defaults.Token.ToString()], new DatabasePrefixProvider(config.Entries[Config.Defaults.Prefix.ToString()]), new DiscordBotConfiguration
            {
                ProviderFactory = bot => new ServiceCollection()
                    .AddSingleton(new Modules.Translation.TranslateService(bot, config, logger))
                    .AddDbContext<DataContext>(ServiceLifetime.Transient)
                    .AddSingleton(bot)
                    .AddSingleton(config)
                    .AddSingleton(logger)
                    .AddSingleton(new Modules.Translation.TranslateService(bot, config, logger))
                    .AddSingleton(new Modules.Statistics.StatisticsService(bot))
                    .BuildServiceProvider()
            });
            var client = new HttpClient();
            bot.AddTypeParser(new IEmojiParser(client));
            await bot.AddExtensionAsync(new InteractivityExtension());
            new EventHandler(bot, logger).Initialize();
            bot.AddModules(Assembly.GetEntryAssembly());
            await bot.RunAsync();
        }

        public void ParseArguments(string[] args = null)
        {
            if (args != null)
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(o =>
                    {
                        config = Config.LoadFromFile(o.ConfigPath);
                    });
            }
            else
            {
                config = Config.LoadFromFile(null);
            }

            if (!config.Entries.ContainsKey(Config.Defaults.Token.ToString()))
            {
                logger.Log($"Please input bot token, can be found at: {Constants.DeveloperApplicationLink}", Logger.Source.Bot);
                config.Entries[Config.Defaults.Token.ToString()] = Console.ReadLine();
                config.Save();
            }

            if (!config.Entries.ContainsKey(Config.Defaults.Prefix.ToString()))
            {
                logger.Log($"Please input bot default prefix:", Logger.Source.Bot);
                config.Entries[Config.Defaults.Prefix.ToString()] = Console.ReadLine();
                config.Save();
            }
        }
    }
}
