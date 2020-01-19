using CommandLine;

namespace Causym
{
    public class Options
    {
        [Option('p', "path", Required = false, HelpText = "Path to a config file, generated if file does not exist")]
        public string ConfigPath { get; set; }
    }
}
