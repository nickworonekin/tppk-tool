using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TppkTool.IO;

namespace TppkTool.Formats
{
    public class TppkArchiveEntry
    {
        private readonly Stream stream;
        private readonly int position;
        private readonly int length;

        public int TextureId { get; }

        internal TppkArchiveEntry(int textureId, Stream stream, int position, int length)
        {
            TextureId = textureId;
            this.stream = stream;
            this.position = position;
            this.length = length;
        }

        public Stream Open()
        {
            return new SubReadStream(stream, position, length);
        }
    }
}
