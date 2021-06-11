using System;

namespace PS2MapTool.Exceptions
{
    public class OptiPngNotFoundException : Exception
    {
        public OptiPngNotFoundException()
        {
        }

        public OptiPngNotFoundException(string? message) : base(message)
        {
        }

        public OptiPngNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
