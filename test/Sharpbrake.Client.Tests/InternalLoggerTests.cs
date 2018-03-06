using System;
using System.Net;
using System.Text;
using Sharpbrake.Client.Tests.Mocks;
using Xunit;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="InternalLogger"/> class.
    /// </summary>
    public class InternalLoggerTests
    {
        [Fact]
        public void Enable_ShouldThrowExceptionIfArgumentIsNull()
        {
            var exception = Record.Exception(() => InternalLogger.Enable(null));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("action", ((ArgumentNullException)exception).ParamName);
        }

        [Fact]
        public void Enable_Disable_ShouldStartAndStopTracing()
        {
            var traceOutput = new StringBuilder();
            InternalLogger.Enable(msg => traceOutput.AppendLine(msg));

            NotifyAsync();

            Assert.True(traceOutput.Length > 0, traceOutput.ToString());

            InternalLogger.Disable();
            traceOutput.Clear();

            NotifyAsync();

            Assert.True(traceOutput.Length == 0, traceOutput.ToString());
        }

        private void NotifyAsync()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6"
            };

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                requestHandler.HttpResponse.StatusCode = HttpStatusCode.Created;
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://airbrake.io/\"}";

                var notifier = new AirbrakeNotifier(config, requestHandler);
                var notice = notifier.BuildNotice(new Exception());
                var airbrakeResponse = notifier.NotifyAsync(notice).Result;
            }
        }
    }
}
