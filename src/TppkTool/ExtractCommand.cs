using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using TppkTool.Formats;
using TppkTool.IO;

namespace TppkTool
{
    [Command("extract",
        Description = "Extracts a TPPK archive, optionally from a NARC archive.")]
    [HelpOption("-? | -h | --help",
            Description = "Show help information.")]
    class ExtractCommand
    {
        [Required]
        [FileExists]
        [Argument(0, "input", "The TPPK archive to extract, or the NARC archive containing the TPPK archive to extract.")]
        public string InputPath { get; set; }

        [LegalFilePath]
        [Option("-o | --output", "The output folder. Defaults to the current folder.", CommandOptionType.SingleValue)]
        public string OutputPath { get; set; }

        private int OnExecute(IConsole console)
        {
            var reporter = new ConsoleReporter(console);

            try
            {
                if (OutputPath == null)
                {
                    OutputPath = Environment.CurrentDirectory;
                }

                if (FileHelper.IsNarc(InputPath))
                {
                    NarcArchive.ExtractTppk(InputPath, OutputPath);
                }
                else
                {
                    TppkArchive.Extract(InputPath, OutputPath);
                }

                return 0;
            }
            catch (Exception e)
            {
                reporter.Error(e.Message);

                return e.HResult;
            }
        }
    }
}
