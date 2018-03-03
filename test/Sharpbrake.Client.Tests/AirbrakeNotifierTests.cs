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
        public void CreateNotice_ShouldSetDefaultSeverityToError()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());

            var notice1 = notifier.CreateNotice("message");
            var notice2 = notifier.CreateNotice(new Exception());

            Assert.Equal(Severity.Error.ToString().ToLowerInvariant(), notice1.Context.Severity);
            Assert.Equal(Severity.Error.ToString().ToLowerInvariant(), notice2.Context.Severity);
        }

        [Fact]
        public void CreateNotice_ShouldSetErrorEntriesIfExceptionProvided()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());

            var notice = notifier.CreateNotice(Severity.Error, new Exception());

            Assert.NotNull(notice.Errors);
        }

        [Fact]
        public void CreateNotice_ShouldSetErrorEntriesIfMessageProvided()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());

            var notice = notifier.CreateNotice(Severity.Error, "message {0}", 1);

            Assert.NotNull(notice.Errors);
        }

        [Fact]
        public void CreateNotice_ShouldUpdateNoticeAfterApplyingFilters()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());

            notifier.AddFilter(n =>
            {
                n.Context.Action = "modified action";
                return n;
            });

            var notice = notifier.CreateNotice("message");

            Assert.NotNull(notice.Context);
            Assert.True(notice.Context.Action == "modified action");
        }

        [Fact]
        public void CreateNotice_ShouldReturnNullIfFilterSetNoticeToNull()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());

            notifier.AddFilter(n => null);

            var notice = notifier.CreateNotice("message");

            Assert.Null(notice);
        }

        [Fact]
        public void SetHttpContext_ShouldSetHttpContext()
        {
            var notifier = new AirbrakeNotifier(new AirbrakeConfig());
            var notice = NoticeBuilder.CreateNotice();
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
                var exceptionTask = Record.ExceptionAsync(() => Task.Run(() => notifier.NotifyAsync(NoticeBuilder.CreateNotice())));

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
                var response = notifier.NotifyAsync(NoticeBuilder.CreateNotice()).Result;

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
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://airbrake.io/\"}";

                requestHandler.HttpRequest.IsFaultedGetRequestStream = faultedTask == "GetRequestStream";
                requestHandler.HttpRequest.IsFaultedGetResponse = faultedTask == "GetResponse";

                var notifier = new AirbrakeNotifier(config, requestHandler);
                var notifyTask = notifier.NotifyAsync(NoticeBuilder.CreateNotice());
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
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://airbrake.io/\"}";

                requestHandler.HttpRequest.IsCanceledGetRequestStream = canceledTask == "GetRequestStream";
                requestHandler.HttpRequest.IsCanceledGetResponse = canceledTask == "GetResponse";

                var notifier = new AirbrakeNotifier(config, requestHandler);
                var notifyTask = notifier.NotifyAsync(NoticeBuilder.CreateNotice());
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
                requestHandler.HttpResponse.ResponseJson = "{\"Id\":\"12345\",\"Url\":\"https://airbrake.io/\"}";

                var notifier = new AirbrakeNotifier(config, requestHandler);
                var airbrakeResponse = notifier.NotifyAsync(NoticeBuilder.CreateNotice()).Result;

                if (isStatusCodeCreated)
                    Assert.True(airbrakeResponse.Status == RequestStatus.Success);
                else
                    Assert.True(airbrakeResponse.Status == RequestStatus.RequestError);
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
    }
}
