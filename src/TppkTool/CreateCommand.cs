﻿using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using TppkTool.Formats;
using TppkTool.Validation;

namespace TppkTool
{
    [Command("create",
        Description = "Creates a TPPK archive.")]
    [HelpOption("-? | -h | --help",
            Description = "Show help information.")]
    class CreateCommand
    {
        [Required]
        [LegalFilePath]
        [Argument(0, "output", "The filename of the TPPK file to create.")]
        public string OutputPath { get; set; }

        [Required]
        [LegalFilePathOrSearchPattern]
        [Argument(1, "files", "The files to add. Can contain folders and/or wildcards.")]
        public string[] InputPaths { get; set; }

        public bool ShowHelp { get; set; }

        private int OnExecute(IConsole console)
        {
            var reporter = new ConsoleReporter(console);

            try
            {
                var files = new List<string>();
                foreach (var file in InputPaths)
                {
                    if (Directory.Exists(file))
                    {
                        // This is a directory, add all the DDS files in it
                        files.AddRange(Directory.EnumerateFiles(file, "*.dds"));
                    }
                    else
                    {
                        // This is a file (or has wildcards), add all the files that match it
                        var directory = Path.GetDirectoryName(file);
                        if (directory == string.Empty)
                        {
                            directory = Environment.CurrentDirectory;
                        }
                        files.AddRange(Directory.EnumerateFiles(directory, Path.GetFileName(file)));
                    }
                }

                TppkArchive.Create(files, OutputPath);

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
