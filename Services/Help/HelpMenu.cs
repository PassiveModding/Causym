using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Qmmands;

namespace Causym.Services.Help
{
    public class HelpMenu : MenuBase
    {
        private readonly Dictionary<string, (Module Module, HelpMetadataAttribute HelpData)> pages = new Dictionary<string, (Module, HelpMetadataAttribute)>();

        public HelpMenu(CommandService commandService)
        {
            CommandService = commandService;
        }

        public CommandService CommandService { get; }

        protected override async Task<IUserMessage> InitialiseAsync()
        {
            foreach (var module in CommandService.GetAllModules())
            {
                if (module.Attributes.FirstOrDefault(x => x.GetType().Equals(typeof(HelpMetadataAttribute))) is HelpMetadataAttribute attMatch)
                {
                    if (!pages.TryAdd(attMatch.ButtonCode, (module, attMatch)))
                    {
                        // TODO: warn about module being ignored due to duplicate button.
                    }
                }
            }

            var homePage = new LocalEmbedBuilder().WithTitle("Modules");
            foreach (var page in pages)
            {
                homePage.AddField(new LocalEmbedFieldBuilder
                {
                    Name = $"{page.Key} " + page.Value.Module.Name,
                    Value = string.Join(", ", page.Value.Module.Commands.Select(c => '`' + c.Name + '`'))
                });
            }

            var message = await Channel.SendMessageAsync("", false, homePage.Build());

            foreach (var page in pages)
            {
                await AddButtonAsync(new Button(new LocalEmoji(page.Key), x =>
                {
                    return Message.ModifyAsync(m => m.Embed = HelpService.GetModuleHelp(page.Value.Module).WithColor(page.Value.HelpData.Color).Build());
                }));
            }

            return message;
        }
    }
}
