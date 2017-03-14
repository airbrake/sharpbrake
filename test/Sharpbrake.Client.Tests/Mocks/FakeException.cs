using System;

namespace Sharpbrake.Client.Tests.Mocks
{
    public class FakeException : Exception
    {
        private readonly string message;
        private readonly string source;
        private readonly string stackTrace;

        public FakeException(string message = null, string source = null, string stackTrace = null)
        {
            this.message = message;
            this.source = source;
            this.stackTrace = stackTrace;
        }

        public override string Message
        {
            get { return message; }
        }

        public override string Source
        {
            get { return source; }
        }

        public override string StackTrace
        {
            get { return stackTrace; }
        }
    }
}
