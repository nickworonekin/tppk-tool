using System.IO;
using TppkTool.IO;

namespace TppkTool.Formats
{
    public class TppkArchiveEntry
    {
        /// <summary>
        /// Gets or sets the texture ID.
        /// </summary>
        public int TextureId { get; set; }

        /// <summary>
        /// Gets or sets the offset of the entry within the archive.
        /// </summary>
        internal int Offset { get; set; }

        /// <summary>
        /// Gets or sets the length of the entry.
        /// </summary>
        internal int Length { get; set; }
    }
}
