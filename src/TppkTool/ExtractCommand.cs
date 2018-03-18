using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using TppkTool.Formats;

namespace TppkTool
{
    [Command("extract",
        Description = "Extracts a TPPK archive.")]
    [HelpOption("-? | -h | --help",
            Description = "Show help information.")]
    class ExtractCommand
    {
        [Required]
        [Argument(0, "input", "The TPPK archive to extract.")]
        public string InputPath { get; set; }

        [Option("-o | --output", "The output folder. Defaults to the current folder.", CommandOptionType.SingleValue)]
        public string OutputPath { get; set; }

        private void OnExecute(IConsole console)
        {
            var reporter = new ConsoleReporter(console);

            try
            {
                if (OutputPath == null)
                {
                    OutputPath = Environment.CurrentDirectory;
                }

                TppkArchive.Extract(InputPath, OutputPath);
            }
            catch (Exception e)
            {
                reporter.Error(e.Message);
            }
        }
    }
}
