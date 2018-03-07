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
        /// <param name="paths">The DDS files to add.</param>
        /// <param name="outputPath">The output folder of the TPPK archive.</param>
        public static void Create(ICollection<string> paths, string outputPath)
        {
            // Before we start, let's go through all the paths and make sure they are all DDS files
            foreach (var file in paths)
            {
                if (!FileHelper.IsDds(file))
                {
                    throw new InvalidFileTypeException(string.Format(ErrorMessages.NotADdsFile, Path.GetFileName(file)));
                }
            }

            using (var destination = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(destination))
            {
                try
                {
                    writer.Write((byte)'t');
                    writer.Write((byte)'p');
                    writer.Write((byte)'p');
                    writer.Write((byte)'k');
                    writer.Write(0);
                    writer.Write(paths.Count);

                    var position = (((12 * (paths.Count + 1)) + 63) / 64) * 64;
                    var index = 0;
                    foreach (var path in paths)
                    {
                        // Check to see if this file is a DDS file
                        if (!FileHelper.IsDds(path))
                        {
                            throw new InvalidFileTypeException(string.Format(ErrorMessages.NotADdsFile, Path.GetFileName(path)));
                        }

                        var textureId = GetTextureId(path);
                        var length = (int)new FileInfo(path).Length;

                        writer.Write(textureId);
                        writer.Write(position - ((index + 1) * 12));
                        writer.Write(length);

                        position += ((length + 63) / 64) * 64;
                        index++;
                    }

                    foreach (var path in paths)
                    {
                        while (destination.Length % 64 != 0)
                        {
                            writer.Write((byte)0);
                        }

                        using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            source.CopyTo(destination);
                        }
                    }
                }
                catch (Exception e)
                {
                    destination.SetLength(0);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Extracts a TPPK archive.
        /// </summary>
        /// <param name="path">The TPPK archive.</param>
        /// <param name="outputPath">The folder to extract the TPPK archive to.</param>
        public static void Extract(string path, string outputPath)
        {
            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source))
            {
                if (!(reader.ReadByte() == 't' && reader.ReadByte() == 'p' && reader.ReadByte() == 'p' && reader.ReadByte() == 'k'))
                {
                    throw new InvalidFileTypeException(string.Format(ErrorMessages.NotATppkArchive, Path.GetFileName(path)));
                }

                source.Position += 4;
                var entryCount = reader.ReadInt32();
                var entries = new List<TppkArchiveEntry>(entryCount);

                for (var i = 0; i < entryCount; i++)
                {
                    var textureId = reader.ReadInt32();
                    var position = reader.ReadInt32() + (12 * (i + 1));
                    var length = reader.ReadInt32();

                    entries.Add(new TppkArchiveEntry(textureId, source, position, length));
                }

                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                var numOfDigits = Math.Floor(Math.Log10(entryCount) + 1);
                var index = 0;
                foreach (var entry in entries)
                {
                    var outputFilename = $"{index.ToString($"D{numOfDigits}")}_{entry.TextureId.ToString("x")}.dds";

                    using (var destination = new FileStream(Path.Combine(outputPath, outputFilename), FileMode.Create, FileAccess.Write))
                    {
                        entry.Open().CopyTo(destination);
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
