using System;
using System.Collections.Generic;

namespace Sharpbrake.Client.Tests.Mocks
{
    public class FakeLogger : ILogger
    {
        public IList<AirbrakeResponse> LoggedResponses { get; private set; }
        public IList<Exception> LoggedExceptions { get; private set; }

        public FakeLogger()
        {
            LoggedResponses = new List<AirbrakeResponse>();
            LoggedExceptions = new List<Exception>();
        }

        public void Log(AirbrakeResponse response)
        {
            LoggedResponses.Add(response);
        }

        public void Log(Exception exception)
        {
            LoggedExceptions.Add(exception);
        }
    }
}
