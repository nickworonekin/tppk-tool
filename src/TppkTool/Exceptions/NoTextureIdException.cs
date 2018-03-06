using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TppkTool.Exceptions
{
    public class NoTextureIdException : Exception
    {
        public NoTextureIdException()
        {
        }

        public NoTextureIdException(string message) : base(message)
        {
        }

        public NoTextureIdException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoTextureIdException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
