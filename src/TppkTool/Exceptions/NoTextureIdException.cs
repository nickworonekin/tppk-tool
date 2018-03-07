using System;
using System.Runtime.Serialization;

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
