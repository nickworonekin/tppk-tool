using System.IO;

namespace TppkTool.IO
{
    public static class FileHelper
    {
        public static bool IsDds(string path)
        {
            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return source.ReadByte() == 'D'
                    && source.ReadByte() == 'D'
                    && source.ReadByte() == 'S'
                    && source.ReadByte() == ' ';
            }
        }

        public static bool IsNarc(string path)
        {
            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source))
            {
                return source.Length >= 12
                    && reader.ReadByte() == 'N'
                    && reader.ReadByte() == 'A'
                    && reader.ReadByte() == 'R'
                    && reader.ReadByte() == 'C'
                    && reader.ReadByte() == 0xFE
                    && reader.ReadByte() == 0xFF
                    && reader.ReadByte() == 0
                    && reader.ReadByte() == 1
                    && reader.ReadInt32() == source.Length;
            }
        }

        public static bool IsTppk(string path)
        {
            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return IsTppk(source);
            }
        }

        public static bool IsTppk(Stream source)
        {
            var position = source.Position;
            var result = source.ReadByte() == 't'
                && source.ReadByte() == 'p'
                && source.ReadByte() == 'p'
                && source.ReadByte() == 'k';
            source.Position = position;

            return result;
        }
    }
}
