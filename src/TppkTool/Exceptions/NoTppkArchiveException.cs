using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TppkTool.Exceptions
{
    public class NoTppkArchiveException : Exception
    {
        public NoTppkArchiveException()
        {
        }

        public NoTppkArchiveException(string message) : base(message)
        {
        }

        public NoTppkArchiveException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoTppkArchiveException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
