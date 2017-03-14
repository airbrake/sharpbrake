using System;
using System.Threading;
using Sharpbrake.Client.Impl;
using Xunit;

namespace Sharpbrake.Client.IntegrationTests
{
    public class NotifyTests : HttpServerFixture
    {
        private readonly object locker = new object();

        [Fact]
        public void NotifyAsync_1xRequest()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6",
                Host = HttpServer.Host
            };

            var httpRequestHandler = new HttpRequestHandler(config.ProjectId, config.ProjectKey, config.Host);
            var notifier = new AirbrakeNotifier(config, null, httpRequestHandler);

            try
            {
                var zero = 0;
                var result = 1 / zero;
            }
            catch (Exception ex)
            {
#if NET35
                var resetEvent = new AutoResetEvent(false);
                AirbrakeResponse response = null;
                notifier.NotifyCompleted += (sender, eventArgs) =>
                {
                    response = eventArgs.Result;
                    resetEvent.Set();
                };

                notifier.NotifyAsync(ex);

                Assert.Equal(resetEvent.WaitOne(2000), true);
                resetEvent.Close();
#else
                var response = notifier.NotifyAsync(ex).Result;
#endif
                Assert.True(response.Status.Equals(RequestStatus.Success));
            }
        }

        [Fact]
        public void NotifyAsync_10xRequests()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6",
                LogFile = "airbrake.log",
                Host = HttpServer.Host
            };

            var httpRequestHandler = new HttpRequestHandler(config.ProjectId, config.ProjectKey, config.Host);
            var notifier = new AirbrakeNotifier(config, null, httpRequestHandler);

            try
            {
                var zero = 0;
                var result = 1 / zero;
            }
            catch (Exception ex)
            {

                var resetEvent = new AutoResetEvent(false);
                var successfulRequests = 0;
#if NET35
                notifier.NotifyCompleted += (sender, eventArgs) =>
                {
                    var response = eventArgs.Result;
                    lock (locker)
                    {
                        if (response.Status == RequestStatus.Success)
                            successfulRequests += 1;

                        if (successfulRequests == 10)
                            resetEvent.Set();
                    }
                };

                for (var i = 0; i < 10; i++)
                    notifier.NotifyAsync(ex);

                Assert.Equal(resetEvent.WaitOne(10000), true);
                resetEvent.Close();
#else
                for (var i = 0; i < 10; i++)
                {
                    notifier.NotifyAsync(ex).ContinueWith(responseTask =>
                    {
                        var response = responseTask.Result;
                        lock (locker)
                        {
                            if (response.Status == RequestStatus.Success)
                                successfulRequests += 1;

                            if (successfulRequests == 10)
                                resetEvent.Set();
                        }
                    });
                }

                Assert.Equal(resetEvent.WaitOne(10000), true);
                resetEvent.Dispose();
#endif
            }
        }
    }
}
