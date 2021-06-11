using System;

namespace PS2MapTool.Exceptions
{
    public class OptiPngException : Exception
    {
        public OptiPngException()
        {
        }

        public OptiPngException(string? message) : base(message)
        {
        }

        public OptiPngException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
