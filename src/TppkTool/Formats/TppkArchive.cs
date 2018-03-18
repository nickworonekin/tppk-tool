using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TppkTool.Exceptions;
using TppkTool.IO;
using TppkTool.Resources;

namespace TppkTool.Formats
{
    public static class TppkArchive
    {
        /// <summary>
        /// Creates a TPPK archive.
        /// </summary>
        /// <param name="inputPaths">The DDS files to add.</param>
        /// <param name="outputPath">The output folder of the TPPK archive.</param>
        public static void Create(ICollection<string> inputPaths, string outputPath)
        {
            // Before we start, let's go through all the paths and make sure they are all DDS files
            foreach (var file in inputPaths)
            {
                if (!FileHelper.IsDds(file))
                {
                    throw new InvalidFileTypeException(string.Format(ErrorMessages.NotADdsFile, Path.GetFileName(file)));
                }
            }

            using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(output))
            {
                try
                {
                    writer.Write((byte)'t');
                    writer.Write((byte)'p');
                    writer.Write((byte)'p');
                    writer.Write((byte)'k');
                    writer.Write(0);
                    writer.Write(inputPaths.Count);

                    var position = (((12 * (inputPaths.Count + 1)) + 63) / 64) * 64;
                    var index = 0;
                    foreach (var inputPath in inputPaths)
                    {
                        // Check to see if this file is a DDS file
                        if (!FileHelper.IsDds(inputPath))
                        {
                            throw new InvalidFileTypeException(string.Format(ErrorMessages.NotADdsFile, Path.GetFileName(inputPath)));
                        }

                        var textureId = GetTextureId(inputPath);
                        var length = (int)new FileInfo(inputPath).Length;

                        writer.Write(textureId);
                        writer.Write(position - ((index + 1) * 12));
                        writer.Write(length);

                        position += ((length + 63) / 64) * 64;
                        index++;
                    }

                    foreach (var inputPath in inputPaths)
                    {
                        while (output.Length % 64 != 0)
                        {
                            writer.Write((byte)0);
                        }

                        using (var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
                        {
                            input.CopyTo(output);
                        }
                    }
                }
                catch (Exception e)
                {
                    output.SetLength(0);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Extracts a TPPK archive.
        /// </summary>
        /// <param name="inputPath">The TPPK archive.</param>
        /// <param name="outputPath">The folder to extract the TPPK archive to.</param>
        public static void Extract(string inputPath, string outputPath)
        {
            using (var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(input))
            {
                if (!(reader.ReadByte() == 't'
                    && reader.ReadByte() == 'p'
                    && reader.ReadByte() == 'p'
                    && reader.ReadByte() == 'k'))
                {
                    throw new InvalidFileTypeException(string.Format(ErrorMessages.NotATppkArchive, Path.GetFileName(inputPath)));
                }

                input.Position += 4;
                var entryCount = reader.ReadInt32();
                var entries = new List<TppkArchiveEntry>(entryCount);

                for (var i = 0; i < entryCount; i++)
                {
                    var textureId = reader.ReadInt32();
                    var position = reader.ReadInt32() + (12 * (i + 1));
                    var length = reader.ReadInt32();

                    entries.Add(new TppkArchiveEntry
                    {
                        TextureId = textureId,
                        Offset = position,
                        Length = length,
                    });
                }

                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                var numOfDigits = Math.Floor(Math.Log10(entryCount) + 1);
                var index = 0;
                foreach (var entry in entries)
                {
                    var outputFilename = $"{Path.GetFileNameWithoutExtension(inputPath)}_{index.ToString($"D{numOfDigits}")}_{entry.TextureId.ToString("x")}.dds";

                    using (var output = new FileStream(Path.Combine(outputPath, outputFilename), FileMode.Create, FileAccess.Write))
                    using (var entryStream = new SubReadStream(input, entry.Offset, entry.Length))
                    {
                        entryStream.CopyTo(output);
                    }

                    index++;
                }
            }
        }

        /// <summary>
        /// Gets the texture ID from the filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The texture ID.</returns>
        private static int GetTextureId(string filename)
        {
            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            var textureIdIndex = filenameWithoutExtension.LastIndexOf('_');

            if (textureIdIndex == -1)
            {
                throw new NoTextureIdException(string.Format(ErrorMessages.NoTextureId, Path.GetFileName(filename)));
            }

            if (int.TryParse(filenameWithoutExtension.Substring(textureIdIndex + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value))
            {
                return value;
            }

            throw new NoTextureIdException(string.Format(ErrorMessages.NoTextureId, Path.GetFileName(filename)));
        }
    }
}
