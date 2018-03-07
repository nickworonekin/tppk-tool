using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using TppkTool.Formats;

namespace TppkTool
{
    [Command("extract",
        Description = "Extracts a TPPK archive.")]
    class ExtractCommand
    {
        [Required]
        [Argument(0, "input",
            Description = "The TPPK archive to extract.")]
        public string InputPath { get; set; }

        [Option("-o | --output",
            Description = "The output folder. Defaults to the current folder.")]
        public string OutputPath { get; set; }

        [HelpOption("-? | -h | --help",
            Description = "Show help information.")]
        public bool ShowHelp { get; set; }

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
