using System.Linq;
using Disqord;
using Qmmands;

namespace Causym.Services.Help
{
    public class HelpService
    {
        public static LocalEmbedBuilder GetModuleHelp(Module module)
        {
            var commandInfos = module.Commands.Select(x => GetCommandHelp(x));

            // TODO: Split based on max embed length (or x amount of fields + specific max length)
            return new LocalEmbedBuilder().WithDescription(string.Join("\n", commandInfos)).WithTitle(module.Name);
        }

        public static string GetCommandHelp(Command command)
        {
            return $"{GetCommandInfo(command)}\n{FormatCommand(command)}";
        }

        private static string FormatCommand(Command command)
        {
            return $"`{command.FullAliases[0]} {string.Join(" ", command.Parameters.Select(x => FormatParameter(x)))}`";
        }

        private static string GetCommandInfo(Command command)
        {
            var response = $"**{command.Name}**";
            if (command.Description != null)
            {
                response += "\n" + command.Description;
            }

            if (command.Remarks != null)
            {
                response += "\n**[**Remarks**]**" + command.Remarks;
            }

            if (command.FullAliases.Count > 1)
            {
                response += "\n**[**Aliases**]**" + string.Join(", ", command.FullAliases.Select(x => $"`{x}`"));
            }

            if (command.Checks.Count > 0)
            {
                // TODO: Implement custom checkattribute with name and description
                response += "\n**[**Checks**]**" + string.Join(", ", command.Checks.Select(x => x.GetType().Name));
            }

            if (!command.IsEnabled)
            {
                response += "\n__Command is currently disabled__";
            }

            return response;
        }

        private static string FormatParameter(Parameter parameter)
        {
            var str = parameter.Name;
            if (parameter.IsMultiple)
            {
                str = str + "*";
            }

            if (parameter.IsOptional)
            {
                str = str + "?";
            }

            if (parameter.IsRemainder)
            {
                str = str + "...";
            }

            if (parameter.Description != null)
            {
                str = str + "(" + parameter.Description + ")";
            }

            if (parameter.Remarks != null)
            {
                str = str + "[" + parameter.Remarks + "]";
            }

            // TODO: DefaultValue & type parsing
            return str;
        }
    }
}
