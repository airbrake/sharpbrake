using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Sharpbrake.Client.Tests.Mocks;
#if NET35
using System.Reflection;
using Xunit.Extensions;
#elif NET45
using System.Reflection;
using System.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Xunit;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="AirbrakeNotifier"/> class.
    /// </summary>
    public class AirbrakeNotifierTests
    {
        [Fact]
        public void Ctor_ShouldThrowExceptionIfConfigIsNotProvided()
        {
            var exception = Record.Exception(() => new AirbrakeNotifier(null));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
            Assert.True(((ArgumentNullException)exception).ParamName.Equals("config"));
        }

        [Fact]
        public void Ctor_ShouldSetFileLoggerIfCustomIsEmptyAndLogFileIsSet()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6",
                LogFile = Guid.NewGuid() + ".log"
            };

            var logFile =
#if NET35 || NET45
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location ?? string.Empty), config.LogFile);
#else
                Path.Combine(AppContext.BaseDirectory, config.LogFile);
#endif

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                requestHandler.HttpResponse.StatusCode = HttpStatusCode.Created;
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://airbrake.io/\"}";

                var notifier = new AirbrakeNotifier(config, null, requestHandler);
                notifier.Notify(new Exception());

                var resetEvent = new AutoResetEvent(false);
                while (!resetEvent.WaitOne(2000))
                    if (File.Exists(logFile))
                        resetEvent.Set();
#if NET35
                resetEvent.Close();
#else
                resetEvent.Dispose();
#endif
            }

            Assert.True(File.Exists(logFile));
            File.Delete(logFile);
        }

        [Theory,
         InlineData("", "e2046ca6e4e9214b24ad252e3c99a0f6"),
         InlineData("127348", "")]
        public void NotifyAsync_ShouldThrowExceptionIfProjectIdOrKeyIsNotSet(string projectId, string projectKey)
        {
            var config = new AirbrakeConfig
            {
                ProjectId = projectId,
                ProjectKey = projectKey
            };

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                var notifier = new AirbrakeNotifier(config, new FakeLogger(), requestHandler);
#if NET35
                var exception = Record.Exception(() => notifier.NotifyAsync(new Exception()));
#else
                var exceptionTask = Record.ExceptionAsync(() => Task.Run(() => notifier.NotifyAsync(new Exception())));

                Assert.NotNull(exceptionTask);
                var exception = exceptionTask.Result;
#endif
                Assert.IsType<Exception>(exception);
                Assert.True(exception.Message.Equals("Project " + (string.IsNullOrEmpty(projectId) ? "Id" : "Key") + " is required"));
            }
        }

        [Fact]
        public void NotifyAsync_ShouldSetStatusToIgnoredIfEnvironmentIsIgnored()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6",
                Environment = "test",
                IgnoreEnvironments = new List<string> {"test"}
            };

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                var notifier = new AirbrakeNotifier(config, new FakeLogger(), requestHandler);
#if NET35
                var resetEvent = new AutoResetEvent(false);
                AirbrakeResponse airbrakeResponse = null;
                notifier.NotifyCompleted += (sender, eventArgs) =>
                {
                    airbrakeResponse = eventArgs.Result;
                    resetEvent.Set();
                };

                notifier.NotifyAsync(new Exception());

                Assert.Equal(resetEvent.WaitOne(2000), true);
                resetEvent.Close();
#else
                var airbrakeResponse = notifier.NotifyAsync(new Exception()).Result;
#endif
                Assert.True(airbrakeResponse.Status == RequestStatus.Ignored);
            }
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public void NotifyAsync_ShouldInitializeHttpContextOnlyIfProvided(bool isHttpContextProvided)
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

                var notifier = new AirbrakeNotifier(config, new FakeLogger(), requestHandler);

                FakeHttpContext context = null;
                if (isHttpContextProvided)
                    context = new FakeHttpContext { UserAgent = "test" };

#if NET35
                var resetEvent = new AutoResetEvent(false);
                AirbrakeResponse airbrakeResponse = null;
                notifier.NotifyCompleted += (sender, eventArgs) =>
                {
                    airbrakeResponse = eventArgs.Result;
                    resetEvent.Set();
                };

                notifier.NotifyAsync(new Exception(), context);

                Assert.Equal(resetEvent.WaitOne(2000), true);
                resetEvent.Close();
#else
                var airbrakeResponse = notifier.NotifyAsync(new Exception(), context).Result;
#endif
                var notice = NoticeBuilder.FromJsonString(requestHandler.HttpRequest.GetRequestStreamContent());

                Assert.True(airbrakeResponse.Status == RequestStatus.Success);

                if (isHttpContextProvided)
                    Assert.True(notice.Context != null);
                else
                    Assert.True(notice.Context == null || string.IsNullOrEmpty(notice.Context.UserAgent));
            }
        }

        [Theory,
         InlineData("GetRequestStream"),
         InlineData("GetResponse")]
        public void NotifyAsync_ShouldSetExceptionIfRequestStreamOrResponseIsFaulted(string faultedTask)
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

                requestHandler.HttpRequest.IsFaultedGetRequestStream = faultedTask == "GetRequestStream";
                requestHandler.HttpRequest.IsFaultedGetResponse = faultedTask == "GetResponse";

                var notifier = new AirbrakeNotifier(config, new FakeLogger(), requestHandler);
#if NET35
                Exception exception = null;
                var resetEvent = new AutoResetEvent(false);
                notifier.NotifyCompleted += (sender, eventArgs) =>
                {
                    exception = eventArgs.Error;
                    resetEvent.Set();
                };

                notifier.NotifyAsync(new Exception());

                Assert.Equal(resetEvent.WaitOne(), true);
                resetEvent.Close();
#else
                var notifyTask = notifier.NotifyAsync(new Exception());
                var exceptionTask = Record.ExceptionAsync(() => notifyTask);

                Assert.NotNull(exceptionTask);
                var exception = exceptionTask.Result;
                Assert.True(notifyTask.IsFaulted);
#endif
                Assert.IsType<Exception>(exception);
            }
        }

        [Theory,
         InlineData("GetRequestStream"),
         InlineData("GetResponse")]
        public void NotifyAsync_ShouldSetCanceledIfRequestStreamOrResponseIsCanceled(string canceledTask)
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

                requestHandler.HttpRequest.IsCanceledGetRequestStream = canceledTask == "GetRequestStream";
                requestHandler.HttpRequest.IsCanceledGetResponse = canceledTask == "GetResponse";

                var notifier = new AirbrakeNotifier(config, new FakeLogger(), requestHandler);
#if NET35
#else
                var notifyTask = notifier.NotifyAsync(new Exception());
                var exceptionTask = Record.ExceptionAsync(() => notifyTask);

                Assert.NotNull(exceptionTask);
                var exception = exceptionTask.Result;
                Assert.True(notifyTask.IsCanceled);
                Assert.IsType<TaskCanceledException>(exception);
#endif
            }
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public void NotifyAsync_ShouldSetRequestStatusToSuccessOnlyIfStatusCodeCreated(bool isStatusCodeCreated)
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6"
            };

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                requestHandler.HttpResponse.StatusCode = isStatusCodeCreated
                    ? HttpStatusCode.Created
                    : HttpStatusCode.BadRequest;
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://airbrake.io/\"}";

                var notifier = new AirbrakeNotifier(config, new FakeLogger(), requestHandler);
#if NET35
                var resetEvent = new AutoResetEvent(false);
                AirbrakeResponse airbrakeResponse = null;
                notifier.NotifyCompleted += (sender, eventArgs) =>
                {
                    airbrakeResponse = eventArgs.Result;
                    resetEvent.Set();
                };

                notifier.NotifyAsync(new Exception());

                Assert.Equal(resetEvent.WaitOne(2000), true);
                resetEvent.Close();
#else
                var airbrakeResponse = notifier.NotifyAsync(new Exception()).Result;
#endif
                if (isStatusCodeCreated)
                    Assert.True(airbrakeResponse.Status == RequestStatus.Success);
                else
                    Assert.True(airbrakeResponse.Status == RequestStatus.RequestError);
            }
        }

        [Fact]
        public void NotifyAsync_ShouldUpdateNoticeAfterApplyingFilters()
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

                var notifier = new AirbrakeNotifier(config, new FakeLogger(), requestHandler);

                notifier.AddFilter(notice =>
                {
                    notice.Context.Action = "modified action";
                    return notice;
                });
#if NET35
                var resetEvent = new AutoResetEvent(false);
                AirbrakeResponse airbrakeResponse = null;
                notifier.NotifyCompleted += (sender, eventArgs) =>
                {
                    airbrakeResponse = eventArgs.Result;
                    resetEvent.Set();
                };

                notifier.NotifyAsync(new Exception());

                Assert.Equal(resetEvent.WaitOne(2000), true);
                resetEvent.Close();
#else
                var airbrakeResponse = notifier.NotifyAsync(new Exception()).Result;
#endif
                var actualNotice = NoticeBuilder.FromJsonString(requestHandler.HttpRequest.GetRequestStreamContent());

                Assert.True(airbrakeResponse.Status == RequestStatus.Success);
                Assert.NotNull(actualNotice.Context);
                Assert.True(actualNotice.Context.Action == "modified action");
            }
        }

        [Fact]
        public void NotifyAsync_ShouldSetStatusToIgnoredIfNoticeIsNullAfterApplyingFilters()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6"
            };

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                var notifier = new AirbrakeNotifier(config, new FakeLogger(), requestHandler);
                notifier.AddFilter(notice => null);
#if NET35
                var resetEvent = new AutoResetEvent(false);
                AirbrakeResponse airbrakeResponse = null;
                notifier.NotifyCompleted += (sender, eventArgs) =>
                {
                    airbrakeResponse = eventArgs.Result;
                    resetEvent.Set();
                };

                notifier.NotifyAsync(new Exception());

                Assert.Equal(resetEvent.WaitOne(2000), true);
                resetEvent.Close();
#else
                var airbrakeResponse = notifier.NotifyAsync(new Exception()).Result;
#endif
                Assert.True(airbrakeResponse.Status == RequestStatus.Ignored);
            }
        }

        [Fact]
        public void Notify_ShouldLogResponseIfResponseIsOk()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6"
            };

            var logger = new FakeLogger();

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                requestHandler.HttpResponse.StatusCode = HttpStatusCode.Created;
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://airbrake.io/\"}";

                var notifier = new AirbrakeNotifier(config, logger, requestHandler);
                notifier.Notify(new Exception());

                var resetEvent = new AutoResetEvent(false);
                while (!resetEvent.WaitOne(2000))
                    if (logger.LoggedResponses.Count > 0)
                        resetEvent.Set();
#if NET35
                resetEvent.Close();
#else
                resetEvent.Dispose();
#endif
            }

            Assert.True(logger.LoggedResponses.Count > 0);
        }

        [Fact]
        public void Notify_ShouldLogExceptionIfResponseIsFaulted()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6"
            };

            var logger = new FakeLogger();

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                requestHandler.HttpRequest.IsFaultedGetResponse = true;

                var notifier = new AirbrakeNotifier(config, logger, requestHandler);
                notifier.Notify(new Exception());

                var resetEvent = new AutoResetEvent(false);
                while (!resetEvent.WaitOne(2000))
                    if (logger.LoggedExceptions.Count > 0)
                        resetEvent.Set();
#if NET35
                resetEvent.Close();
#else
                resetEvent.Dispose();
#endif
            }

            Assert.True(logger.LoggedExceptions.Count > 0);
        }
    }
}
