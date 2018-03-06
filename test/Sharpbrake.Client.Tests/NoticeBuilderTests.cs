using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Sharpbrake.Client.Model;
using Sharpbrake.Client.Tests.Mocks;
using Xunit;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="NoticeBuilder"/> class.
    /// </summary>
    public class NoticeBuilderTests
    {
        [Fact]
        public void BuildNotice_ShouldInitializeContextAndNotifierInfo()
        {
            var notice = NoticeBuilder.BuildNotice();

            Assert.NotNull(notice.Context);
            Assert.NotNull(notice.Context.Notifier);
        }

        [Fact]
        public void SetErrorEntries_ShouldSetExceptionsInCorrectOrder()
        {
            var ex = new Exception("Main exception",
                        new Exception("Inner exception 1",
                            new Exception("Inner exception 2")));

            var notice = NoticeBuilder.BuildNotice();
            notice.SetErrorEntries(ex, string.Empty);

            var errorEntries = notice.Errors;

            Assert.True(errorEntries.Count.Equals(3));

            Assert.Equal("Main exception", errorEntries[0].Message);
            Assert.Equal("Inner exception 1", errorEntries[1].Message);
            Assert.Equal("Inner exception 2", errorEntries[2].Message);
        }

        [Fact]
        public void SetErrorEntries_ShouldLimitInnerExceptionsToThree()
        {
            var ex = new Exception("Main exception",
                        new Exception("Inner exception 1",
                            new Exception("Inner exception 2",
                                new Exception("Inner exception 3",
                                    new Exception("Inner exception 4")))));

            var notice = NoticeBuilder.BuildNotice();
            notice.SetErrorEntries(ex, string.Empty);

            var errorEntries = notice.Errors;

            // main exception + no more than 3 inner exceptions
            Assert.True(errorEntries.Count.Equals(4));
        }

        [Fact]
        public void SetErrorEntries_ShouldSetMessageIfPresent()
        {
            var ex = new FakeException("error message from exception");

            var notice = NoticeBuilder.BuildNotice();
            notice.SetErrorEntries(ex, "message");

            var errorEntries = notice.Errors;

            Assert.NotNull(errorEntries);
            Assert.True(errorEntries.Count == 1);
            Assert.Equal("message", errorEntries.First().Message);
        }

        [Fact]
        public void SetErrorEntries_ShouldSetErrorMessageFromExceptionIfNoMessage()
        {
            var ex = new FakeException("error message from exception");

            var notice = NoticeBuilder.BuildNotice();
            notice.SetErrorEntries(ex, string.Empty);

            var errorEntries = notice.Errors;

            Assert.NotNull(errorEntries);
            Assert.True(errorEntries.Count == 1);
            Assert.Equal("error message from exception", errorEntries.First().Message);
        }

        [Fact]
        public void SetErrorEntries_ShouldSetExceptionProperty()
        {
            var notice = NoticeBuilder.BuildNotice();
            notice.SetErrorEntries(new Exception(), string.Empty);

            Assert.NotNull(notice.Exception);
        }

        [Fact]
        public void SetErrorEntries_ShouldSetErrorEntryUsingMessageIfExceptionEmpty()
        {
            var notice = NoticeBuilder.BuildNotice();
            notice.SetErrorEntries(null, "message");

            var errorEntries = notice.Errors;

            Assert.NotNull(errorEntries);
            Assert.True(errorEntries.Count == 1);
            Assert.Equal("message", errorEntries.First().Message);
        }

        [Fact]
        public void SetConfigurationContext_ShouldSetEnvironmentNameAndAppVersion()
        {
            var notice = NoticeBuilder.BuildNotice();
            notice.SetConfigurationContext(new AirbrakeConfig
            {
                Environment = "local",
                AppVersion = "1.2.3"
            });

            Assert.NotNull(notice.Context);
            Assert.Equal("local", notice.Context.EnvironmentName);
            Assert.Equal("1.2.3", notice.Context.AppVersion);
        }

        [Fact]
        public void SetConfigurationContext_ShouldNotSetEnvironmentNameAndAppVersionIfConfigIsNull()
        {
            var notice = NoticeBuilder.BuildNotice();
            notice.SetConfigurationContext(null);

            Assert.NotNull(notice.Context);
            Assert.True(string.IsNullOrEmpty(notice.Context.EnvironmentName));
            Assert.True(string.IsNullOrEmpty(notice.Context.AppVersion));
        }

        [Theory,
         InlineData("", "", ""),
         InlineData("host", "", ""),
         InlineData("host", "os", ""),
         InlineData("host", "os", "lang")]
        public void SetEnvironmentContext_ShouldSetEnvironmentContextAccordingToPassedParameters(string host, string os, string lang)
        {
            var notice = NoticeBuilder.BuildNotice();
            notice.SetEnvironmentContext(host, os, lang);

            if (string.IsNullOrEmpty(host) && string.IsNullOrEmpty(os) && string.IsNullOrEmpty(lang))
            {
                Assert.True(string.IsNullOrEmpty(notice.Context.Hostname));
                Assert.True(string.IsNullOrEmpty(notice.Context.Os));
                Assert.True(string.IsNullOrEmpty(notice.Context.Language));
            }
            else
            {
                Assert.True(string.IsNullOrEmpty(host) ? string.IsNullOrEmpty(notice.Context.Hostname) : notice.Context.Hostname.Equals(host));
                Assert.True(string.IsNullOrEmpty(os) ? string.IsNullOrEmpty(notice.Context.Os) : notice.Context.Os.Equals(os));
                Assert.True(string.IsNullOrEmpty(lang) ? string.IsNullOrEmpty(notice.Context.Language) : notice.Context.Language.Equals(lang));
            }
        }

        [Fact]
        public void SetHttpContext_ShouldNotSetHttpContextIfContextParamIsEmpty()
        {
            var notice = NoticeBuilder.BuildNotice();
            notice.SetHttpContext(null, null);

            Assert.Null(notice.HttpContext);
        }

        [Theory,
         InlineData("", ""),
         InlineData("url", ""),
         InlineData("url", "userAgent")]
        public void SetHttpContext_ContextHttpParameters(string url, string userAgent)
        {
            var httpContext = new FakeHttpContext
            {
                Url = url,
                UserAgent = userAgent
            };

            var notice = NoticeBuilder.BuildNotice();
            notice.SetHttpContext(httpContext, null);

            Assert.NotNull(notice.Context);
            Assert.True(string.IsNullOrEmpty(url) ? string.IsNullOrEmpty(notice.Context.Url) : notice.Context.Url.Equals(url));
            Assert.True(string.IsNullOrEmpty(userAgent) ? string.IsNullOrEmpty(notice.Context.UserAgent) : notice.Context.UserAgent.Equals(userAgent));
        }

        [Theory,
         InlineData("", "", ""),
         InlineData("id", "", ""),
         InlineData("id", "name", ""),
         InlineData("id", "name", "email")]
        public void SetHttpContext_ContextUserInfo(string id, string name, string email)
        {
            var httpContext = new FakeHttpContext
            {
                UserName = name,
                UserId = id,
                UserEmail = email
            };

            var notice = NoticeBuilder.BuildNotice();
            notice.SetHttpContext(httpContext, null);

            Assert.NotNull(notice.Context);
            Assert.NotNull(notice.Context.User);
            Assert.True(string.IsNullOrEmpty(id) ? string.IsNullOrEmpty(notice.Context.User.Id) : notice.Context.User.Id.Equals(id));
            Assert.True(string.IsNullOrEmpty(name) ? string.IsNullOrEmpty(notice.Context.User.Name) : notice.Context.User.Name.Equals(name));
            Assert.True(string.IsNullOrEmpty(email) ? string.IsNullOrEmpty(notice.Context.User.Email) : notice.Context.User.Email.Equals(email));
        }

        [Fact]
        public void SetHttpContext_ShouldSetParametersIfConfigIsNotDefined()
        {
            var httpContext = new FakeHttpContext
            {
                Parameters = new Dictionary<string, string>(),
                EnvironmentVars = new Dictionary<string, string>(),
                Session = new Dictionary<string, string>()
            };

            var notice = NoticeBuilder.BuildNotice();
            notice.SetHttpContext(httpContext, null);

            Assert.NotNull(notice.Params);
            Assert.NotNull(notice.EnvironmentVars);
            Assert.NotNull(notice.Session);
        }

        [Fact]
        public void SetHttpContext_ShouldSetParametersIfConfigIsDefined()
        {
            var httpContext = new FakeHttpContext
            {
                Parameters = new Dictionary<string, string>(),
                EnvironmentVars = new Dictionary<string, string>(),
                Session = new Dictionary<string, string>()
            };

            var config = new AirbrakeConfig();

            var notice = NoticeBuilder.BuildNotice();
            notice.SetHttpContext(httpContext, config);

            Assert.NotNull(notice.Params);
            Assert.NotNull(notice.EnvironmentVars);
            Assert.NotNull(notice.Session);
        }

        [Fact]
        public void SetHttpContext_ShouldSetHttpContextProperty()
        {
            var notice = NoticeBuilder.BuildNotice();
            notice.SetHttpContext(new FakeHttpContext(), null);

            Assert.NotNull(notice.HttpContext);
        }

        [Fact]
        public void SetHttpContext_ShouldSetActionAndContextIfProvided()
        {
            var httpContext = new FakeHttpContext
            {
                Action = "Action",
                Component = "Component"
            };

            var notice = NoticeBuilder.BuildNotice();
            notice.SetHttpContext(httpContext, null);

            Assert.True(!string.IsNullOrEmpty(notice.Context.Action));
            Assert.True(!string.IsNullOrEmpty(notice.Context.Component));
        }

        [Fact]
        public void SetSeverity_ShouldSetSeverityLowercase()
        {
            var notice = NoticeBuilder.BuildNotice();
            notice.SetSeverity(Severity.Critical);

            Assert.Equal("critical", notice.Context.Severity);
        }

        [Fact]
        public void ToJsonString()
        {
            var notice = NoticeBuilder.BuildNotice();
            notice.SetErrorEntries(new Exception(), string.Empty);

            var actualJson = notice.ToJsonString();

            var serializerSettings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true
            };

            var serializer = new DataContractJsonSerializer(typeof(Notice), serializerSettings);

            string expectedJson;
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, notice);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                    expectedJson = reader.ReadToEnd();
            }

            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void ToJsonString_ShouldNotSerializeExceptionAndHttpContext()
        {
            var notice = NoticeBuilder.BuildNotice();

            notice.SetErrorEntries(new Exception(), string.Empty);
            notice.SetHttpContext(new FakeHttpContext(), null);

            var json = notice.ToJsonString();

            Assert.True(json.IndexOf("\"Exception\":", StringComparison.OrdinalIgnoreCase) == -1);
            Assert.True(json.IndexOf("\"HttpContext\":", StringComparison.OrdinalIgnoreCase) == -1);
        }

        [Fact]
        public void ToJsonString_ShouldTruncateNoticeBigger64KB()
        {
            var httpContext = new FakeHttpContext
            {
                Parameters = new Dictionary<string, string>
                {
                    {"long_param", new string('x', 64001)}
                }
            };

            var notice = NoticeBuilder.BuildNotice();
            notice.SetHttpContext(httpContext, null);

            var json = notice.ToJsonString();

            Assert.NotNull(json);
            Assert.True(json.Length <= 64000);
        }

        [Fact]
        public void FromJsonString()
        {
            const string json = "{\"errors\":[{\"type\":\"System.Exception\",\"message\":\"Exception: Exception of type 'System.Exception' was thrown.\"}]}";

            var notice = NoticeBuilder.FromJsonString(json);

            Assert.NotNull(notice);
            Assert.NotNull(notice.Errors);
            Assert.True(notice.Errors.Count > 0);
        }
    }
}
