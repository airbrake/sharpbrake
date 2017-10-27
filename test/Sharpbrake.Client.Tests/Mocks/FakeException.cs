using System;

namespace Sharpbrake.Client.Tests.Mocks
{
    public class FakeException : Exception
    {
        private readonly string source;

        public FakeException(string message, string source = null, string stackTrace = null)
        {
            Message = message;
            this.source = source;
            StackTrace = stackTrace;
        }

        public override string Message { get; }

        public override string Source => source;

        public override string StackTrace { get; }
    }
}
