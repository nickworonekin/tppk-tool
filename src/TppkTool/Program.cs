using McMaster.Extensions.CommandLineUtils;

namespace TppkTool
{
    [Command(Name = "TppkTool",
        Description = "Creates and extracts TPPK archives for Puyo Puyo Tetris PC.",
        ThrowOnUnexpectedArgument = false)]
    [Subcommand("create", typeof(CreateCommand))]
    [Subcommand("extract", typeof(ExtractCommand))]
    [HelpOption("-? | -h | --help",
            Description = "Show help information.")]
    class Program
    {
        static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private void OnExecute(CommandLineApplication app) => app.ShowHelp();
    }
}
