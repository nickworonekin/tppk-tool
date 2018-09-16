using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TppkTool.Exceptions;
using TppkTool.IO;
using TppkTool.Resources;

namespace TppkTool.Formats
{
    public class NarcArchive
    {
        /// <summary>
        /// Creates a NARC archive.
        /// </summary>
        /// <param name="inputStreams">The files to add.</param>
        /// <param name="outputPath">The output folder of the TPPK archive.</param>
        public static void Create(ICollection<Stream> inputStreams, string outputPath)
        {
            using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(output))
            {
                try
                {
                    // Write out the NARC header
                    writer.Write((byte)'N');
                    writer.Write((byte)'A');
                    writer.Write((byte)'R');
                    writer.Write((byte)'C');
                    writer.Write((byte)0xFE);
                    writer.Write((byte)0xFF);
                    writer.Write((byte)0);
                    writer.Write((byte)1);

                    writer.Write(0); // File length (will be written to later)

                    writer.Write((short)16); // Header length (always 16)
                    writer.Write((short)3); // Number of sections (always 3)

                    // Write out the FATB section
                    writer.Write((byte)'B');
                    writer.Write((byte)'T');
                    writer.Write((byte)'A');
                    writer.Write((byte)'F');

                    writer.Write(12 + (inputStreams.Count * 8)); // Section length
                    writer.Write(inputStreams.Count); // Number of file entries

                    var position = 0;
                    foreach (var inputStream in inputStreams)
                    {
                        var length = (int)inputStream.Length;

                        writer.Write(position); // Start position
                        writer.Write(position + length); // End position

                        position += ((length + 3) / 4) * 4; // Offsets must be a multiple of 4
                    }
                    var fimgLength = position;

                    // Write out the FNTB section
                    writer.Write((byte)'B');
                    writer.Write((byte)'T');
                    writer.Write((byte)'N');
                    writer.Write((byte)'F');
                    
                    // The FNTB section is always the same if there are no filenames
                    writer.Write(16); // Section length (always 16)
                    writer.Write(4); // Always 4
                    writer.Write((short)0); // First file index (always 0)
                    writer.Write((short)1); // Number of directories, including the root directory (always 1)

                    // Write out the FIMG section
                    writer.Write((byte)'G');
                    writer.Write((byte)'M');
                    writer.Write((byte)'I');
                    writer.Write((byte)'F');

                    writer.Write(fimgLength + 8); // Section length

                    foreach (var inputStream in inputStreams)
                    {
                        inputStream.CopyTo(output);

                        while (output.Length % 4 != 0)
                        {
                            writer.Write((byte)0xFF);
                        }
                    }

                    // Go back and write out the file length
                    output.Position = 8;
                    writer.Write((int)output.Length); // File length
                    output.Position = output.Length;
                }
                catch (Exception e)
                {
                    output.SetLength(0);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Extracts a NARC archive.
        /// </summary>
        /// <param name="inputPath">The NARC archive.</param>
        /// <param name="onExtract">The action to call when an entry is extracted.</param>
        public static void Extract(string inputPath, Action<Stream> onExtract)
        {
            using (var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(input))
            {
                // Read the NARC header
                if (!(reader.ReadByte() == 'N'
                    && reader.ReadByte() == 'A'
                    && reader.ReadByte() == 'R'
                    && reader.ReadByte() == 'C'
                    && reader.ReadByte() == 0xFE
                    && reader.ReadByte() == 0xFF
                    && reader.ReadByte() == 0
                    && reader.ReadByte() == 1
                    && reader.ReadInt32() == input.Length))
                {
                    throw new InvalidFileTypeException(string.Format(ErrorMessages.NotANarcFile, Path.GetFileName(inputPath)));
                }

                var headerLength = reader.ReadInt16();
                var fatbPosition = headerLength;

                // Read the FATB section
                input.Position = fatbPosition;
                if (!(reader.ReadByte() == 'B'
                    && reader.ReadByte() == 'T'
                    && reader.ReadByte() == 'A'
                    && reader.ReadByte() == 'F'))
                {
                    throw new InvalidFileTypeException(string.Format(ErrorMessages.NotANarcFile, Path.GetFileName(inputPath)));
                }

                var fatbLength = reader.ReadInt32();
                var fntbPosition = fatbPosition + fatbLength;

                var fileEntryCount = reader.ReadInt32();
                var fileEntries = new List<NarcArchiveEntry>(fileEntryCount);
                for (var i = 0; i < fileEntryCount; i++)
                {
                    var offset = reader.ReadInt32();
                    var length = reader.ReadInt32() - offset;
                    fileEntries.Add(new NarcArchiveEntry
                    {
                        Offset = offset,
                        Length = length,
                    });
                }

                // Read the FNTB section
                input.Position = fntbPosition;
                if (!(reader.ReadByte() == 'B'
                    && reader.ReadByte() == 'T'
                    && reader.ReadByte() == 'N'
                    && reader.ReadByte() == 'F'))
                {
                    throw new InvalidFileTypeException(string.Format(ErrorMessages.NotANarcFile, Path.GetFileName(inputPath)));
                }

                var fntbLength = reader.ReadInt32();
                var fimgPosition = fntbPosition + fntbLength;

                // Read the FIMG section
                input.Position = fimgPosition;
                if (!(reader.ReadByte() == 'G'
                    && reader.ReadByte() == 'M'
                    && reader.ReadByte() == 'I'
                    && reader.ReadByte() == 'F'))
                {
                    throw new InvalidFileTypeException(string.Format(ErrorMessages.NotANarcFile, Path.GetFileName(inputPath)));
                }

                // Read the entries
                foreach (var fileEntry in fileEntries)
                {
                    using (var entryStream = new SubReadStream(input, fimgPosition + 8 + fileEntry.Offset, fileEntry.Length))
                    {
                        onExtract(entryStream);
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the TPPK archive from a NARC archive.
        /// </summary>
        /// <param name="inputPath">The NARC archive.</param>
        /// <param name="outputPath">The folder to extract the TPPK archive to.</param>
        public static void ExtractTppk(string inputPath, string outputPath)
        {
            var isTppkFound = false;
            void OnExtract(Stream stream)
            {
                if (!isTppkFound && FileHelper.IsTppk(stream))
                {
                    TppkArchive.Extract(stream, outputPath, $"{Path.GetFileNameWithoutExtension(inputPath)}_");
                    isTppkFound = true;
                }
            }

            Extract(inputPath, OnExtract);

            if (!isTppkFound)
            {
                throw new NoTppkArchiveException(string.Format(ErrorMessages.NoTppkArchiveInFile, Path.GetFileName(inputPath)));
            }
        }

        /// <summary>
        /// Updates the TPPK archive in a NARC archive with a new TPPK archive.
        /// </summary>
        /// <param name="inputPaths">The DDS files to add to the new TPPK archive.</param>
        /// <param name="narcPath">The NARC archive.</param>
        public static void UpdateTppk(ICollection<string> inputPaths, string narcPath)
        {
            // Before we start, let's go through all the paths and make sure they are all DDS files
            foreach (var file in inputPaths)
            {
                if (!FileHelper.IsDds(file))
                {
                    throw new InvalidFileTypeException(string.Format(ErrorMessages.NotADdsFile, Path.GetFileName(file)));
                }
            }

            var entryStreams = new List<Stream>();
            var isTppkFound = false;
            void OnExtract(Stream stream)
            {
                var entryStream = new MemoryStream();
                if (!isTppkFound && FileHelper.IsTppk(stream))
                {
                    // Create a new TPPK archive
                    TppkArchive.Create(inputPaths, entryStream);
                    isTppkFound = true;
                }
                else
                {
                    // Just extract this file to add to the new NARC
                    stream.CopyTo(entryStream);
                }

                entryStream.Position = 0;
                entryStreams.Add(entryStream);
            }

            Extract(narcPath, OnExtract);

            if (!isTppkFound)
            {
                throw new NoTppkArchiveException(string.Format(ErrorMessages.NoTppkArchiveInFile, Path.GetFileName(narcPath)));
            }

            // Create the NARC archive. We'll create a temporary file first, then overwrite the original.
            var tempNarcPath = Path.GetTempFileName();
            Create(entryStreams, tempNarcPath);
            File.Delete(narcPath);
            File.Move(tempNarcPath, narcPath);
        }
    }
}
