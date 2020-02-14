using System;
using System.Collections.Generic;
using System.Net;
using Sharpbrake.Client.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;
using Sharpbrake.Client.Model;

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
            Assert.Equal("config", ((ArgumentNullException)exception).ParamName);
        }

        [Fact]
        public void BuildNotice_ShouldSetDefaultSeverityToError()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());
            var notice1 = notifier.BuildNotice("message");
            var notice2 = notifier.BuildNotice(new Exception());

            Assert.Equal(Severity.Error.ToString().ToLowerInvariant(), notice1.Context.Severity);
            Assert.Equal(Severity.Error.ToString().ToLowerInvariant(), notice2.Context.Severity);
        }

        [Fact]
        public void BuildNotice_ShouldSetErrorEntriesIfExceptionProvided()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());
            var notice = notifier.BuildNotice(Severity.Error, new Exception());

            Assert.NotNull(notice.Errors);
        }

        [Fact]
        public void BuildNotice_ShouldSetErrorEntriesIfMessageProvided()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());
            var notice = notifier.BuildNotice(Severity.Error, "message {0}", 1);

            Assert.NotNull(notice.Errors);
        }

        [Fact]
        public void BuildNotice_ShouldSetConfigurationContext()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6",
                Environment = "test",
                AppVersion = "1.0.0"
            };

            var notifier = new AirbrakeNotifier(config);
            var notice = notifier.BuildNotice(Severity.Error, "message");

            Assert.NotNull(notice.Context);
            Assert.NotNull(notice.Context.EnvironmentName);
            Assert.NotNull(notice.Context.AppVersion);
        }

        [Fact]
        public void BuildNotice_ShouldSetEnvironmentContext()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());
            var notice = notifier.BuildNotice(Severity.Error, new Exception(), "message");

            Assert.NotNull(notice.Context);
            Assert.NotNull(notice.Context.Hostname);
            Assert.NotNull(notice.Context.Os);
            Assert.NotNull(notice.Context.Language);
        }

        [Fact]
        public void SetHttpContext_ShouldSetHttpContext()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());
            var notice = NoticeBuilder.BuildNotice();
            var context = new FakeHttpContext();

            notifier.SetHttpContext(notice, context);

            Assert.NotNull(notice.HttpContext);
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
                var notifier = new AirbrakeNotifier(config, requestHandler);
                var exceptionTask = Record.ExceptionAsync(() => Task.Run(() => notifier.NotifyAsync(NoticeBuilder.BuildNotice())));

                Assert.NotNull(exceptionTask);

                var exception = exceptionTask.Result;

                Assert.IsType<Exception>(exception);
                Assert.Equal("Project " + (string.IsNullOrEmpty(projectId) ? "Id" : "Key") + " is required", exception.Message);
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
                IgnoreEnvironments = new List<string> { "test" }
            };

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                var notifier = new AirbrakeNotifier(config, requestHandler);
                var response = notifier.NotifyAsync(NoticeBuilder.BuildNotice()).Result;

                Assert.True(response.Status == RequestStatus.Ignored);
            }
        }

        [Fact]
        public void NotifyAsync_ShouldSetStatusToIgnoredIfNoticeIsNull()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = "127348",
                ProjectKey = "e2046ca6e4e9214b24ad252e3c99a0f6"
            };

            using (var requestHandler = new FakeHttpRequestHandler())
            {
                var notifier = new AirbrakeNotifier(config, requestHandler);
                var response = notifier.NotifyAsync(null).Result;

                Assert.True(response.Status == RequestStatus.Ignored);
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
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://api.airbrake.io/\"}";

                var notifier = new AirbrakeNotifier(config, requestHandler);

                notifier.AddFilter(n =>
                {
                    n.Context.Action = "modified action";
                    return n;
                });

                var notice = notifier.BuildNotice(new Exception());
                var response = notifier.NotifyAsync(notice).Result;
                var actualNotice = NoticeBuilder.FromJsonString(requestHandler.HttpRequest.GetRequestStreamContent());

                Assert.True(response.Status == RequestStatus.Success);
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
                var notifier = new AirbrakeNotifier(config, requestHandler);
                notifier.AddFilter(n => null);

                var notice = notifier.BuildNotice(new Exception());
                var response = notifier.NotifyAsync(notice).Result;

                Assert.True(response.Status == RequestStatus.Ignored);
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
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://api.airbrake.io/\"}";

                requestHandler.HttpRequest.IsFaultedGetRequestStream = faultedTask == "GetRequestStream";
                requestHandler.HttpRequest.IsFaultedGetResponse = faultedTask == "GetResponse";

                var notifier = new AirbrakeNotifier(config, requestHandler);
                var notifyTask = notifier.NotifyAsync(NoticeBuilder.BuildNotice());
                var exceptionTask = Record.ExceptionAsync(() => notifyTask);

                Assert.NotNull(exceptionTask);

                var exception = exceptionTask.Result;

                Assert.True(notifyTask.IsFaulted);
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
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://api.airbrake.io/\"}";

                requestHandler.HttpRequest.IsCanceledGetRequestStream = canceledTask == "GetRequestStream";
                requestHandler.HttpRequest.IsCanceledGetResponse = canceledTask == "GetResponse";

                var notifier = new AirbrakeNotifier(config, requestHandler);
                var notifyTask = notifier.NotifyAsync(NoticeBuilder.BuildNotice());
                var exceptionTask = Record.ExceptionAsync(() => notifyTask);

                Assert.NotNull(exceptionTask);

                var exception = exceptionTask.Result;

                Assert.True(notifyTask.IsCanceled);
                Assert.IsType<TaskCanceledException>(exception);
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
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://api.airbrake.io/\"}";

                var notifier = new AirbrakeNotifier(config, requestHandler);
                var airbrakeResponse = notifier.NotifyAsync(NoticeBuilder.BuildNotice()).Result;

                if (isStatusCodeCreated)
                    Assert.True(airbrakeResponse.Status == RequestStatus.Success);
                else
                    Assert.True(airbrakeResponse.Status == RequestStatus.RequestError);
            }
        }
    }
}
