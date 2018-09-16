using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TppkTool.Formats
{
    public class NarcArchiveEntry
    {
        /// <summary>
        /// Gets or sets the offset of the file data.
        /// </summary>
        internal int Offset { get; set; }

        /// <summary>
        /// Gets or sets the length of the file data.
        /// </summary>
        internal int Length { get; set; }
    }
}
