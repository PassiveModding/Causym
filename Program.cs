using System;
using System.Linq;
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

        public static void Main(string[] args = null)
        {
            new Program().RunAsync(args).GetAwaiter().GetResult();
        }

        public async Task RunAsync(string[] args = null)
        {
            var config = ParseArguments(args);

            using (var db = new DataContext())
            {
                await db.Database.MigrateAsync();

                await db.SaveChangesAsync();
            }

            IServiceCollection botServiceCollection = new ServiceCollection()
                .AddSingleton<HttpClient>();

            var services = Extensions.GetServices(Assembly.GetEntryAssembly());
            foreach (var type in services)
            {
                botServiceCollection = botServiceCollection.AddSingleton(type);
            }

            var bot = new DiscordBotSharder(TokenType.Bot, config.Entries[Config.Defaults.Token.ToString()], new DatabasePrefixProvider(config.Entries[Config.Defaults.Prefix.ToString()]), new DiscordBotSharderConfiguration
            {
                ProviderFactory = bot => botServiceCollection
                    .AddDbContext<DataContext>(ServiceLifetime.Transient)
                    //.AddSingleton(bot)
                    .AddSingleton(bot as DiscordBotSharder)
                    .AddSingleton(config)
                    .AddSingleton(logger)
                    .BuildServiceProvider()
            });

            bot.AddTypeParser(new IEmojiParser(bot.GetRequiredService<HttpClient>()));
            await bot.AddExtensionAsync(new InteractivityExtension());

            foreach (var type in services)
            {
                var service = bot.GetService(type);
                if (service == null) logger.Log($"Service of type {type.Name} not found in bot service provider despite being marked with a service attribute", Logger.Source.Bot, Logger.LogLevel.Warn);
            }

            bot.AddModules(Assembly.GetEntryAssembly());
            await bot.RunAsync();
        }

        public Config ParseArguments(string[] args = null)
        {
            Config config = null;
            if (args != null)
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(o =>
                    {
                        config = Config.LoadFromFile(o.ConfigPath);
                    });
            }

            config ??= Config.LoadFromFile(null);

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

            return config;
        }
    }
}
