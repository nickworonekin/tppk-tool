using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static bool IsDirectory(string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);
    }
}
