using System;
using System.Collections.Generic;
using Sharpbrake.Client.Model;
using Xunit;
using Xunit.Extensions;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="Utils"/> class.
    /// </summary>
    public class UtilsTests
    {
        [Theory,
        InlineData("value1,value2,value3"),
        InlineData(" value1, value2 ,value3 "),
        InlineData(",value1, value2,value3,,,")]
        public void ParseParameter_ShouldSplitByComma(string csv)
        {
            var expected = new List<string> { "value1", "value2", "value3" };
            var actual = Utils.ParseParameter(csv);

            Assert.Equal(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
                Assert.True(string.Equals(expected[i], actual[i]));
        }

        [Theory,
        InlineData("https://airbrake.io"),
        InlineData("https://airbrake.io/")]
        public void GetRequestUri_ShouldProduceValidEndpointRegardlessHostEnding(string host)
        {
            const string projectId = "127348";
            const string projectKey = "e2046ca6e4e9214b24ad252e3c99a0f6";

            const string expected = "https://airbrake.io/api/v3/projects/127348/notices?key=e2046ca6e4e9214b24ad252e3c99a0f6";

            var actual = Utils.GetRequestUri(projectId, projectKey, host);

            Assert.True(string.Equals(expected, actual));
        }

        [Theory,
        InlineData("", "127348", "e2046ca6e4e9214b24ad252e3c99a0f6"),
        InlineData(null, "127348", "e2046ca6e4e9214b24ad252e3c99a0f6"),
        InlineData("", "e2046ca6e4e9214b24ad252e3c99a0f6", "https://airbrake.io/"),
        InlineData("127348", "", "https://airbrake.io/")]
        public void GetRequestUri_ShouldThrowArgumentNullExceptionIfProjectIdOrKeyAreEmptyOrNull(string projectId, string projectKey, string host)
        {
            var exception = Record.Exception(() => Utils.GetRequestUri(projectId, projectKey, host));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory,
        InlineData(null),
        InlineData("")]
        public void GetRequestUri_ShouldSetDefaultAirbrakeHostIfParameterIsEmptyOrNull(string host)
        {
            const string projectId = "127348";
            const string projectKey = "e2046ca6e4e9214b24ad252e3c99a0f6";

            const string expected = "https://airbrake.io/api/v3/projects/127348/notices?key=e2046ca6e4e9214b24ad252e3c99a0f6";

            var actual = Utils.GetRequestUri(projectId, projectKey, host);

            Assert.True(string.Equals(expected, actual));
        }

        [Fact]
        public void ConfigureProxy_ShouldReturnProxyIfUriIsNotEmpty()
        {
            const string uri = "http://proxy-example.com:9090";

            var proxy = Utils.ConfigureProxy(uri);

            Assert.NotNull(proxy);
        }

        [Fact]
        public void ConfigureProxy_ShouldReturnNullIfUriIsEmpty()
        {
            const string uri = "";
            const string username = "username";
            const string password = "password";

            var proxy = Utils.ConfigureProxy(uri, username, password);

            Assert.Null(proxy);
        }

        [Fact]
        public void ConfigureProxy_ShouldSetCredentialsIfUsernameAndPasswordIsSet()
        {
            const string uri = "http://proxy-example.com:9090";
            const string username = "username";
            const string password = "password";

            var proxyUri = new Uri(uri);
            // basic auth type is Name and Password pair
            const string authType = "basic";

            var proxy = Utils.ConfigureProxy(uri, username, password);

            Assert.NotNull(proxy.Credentials);
            Assert.True(string.Equals(proxy.Credentials.GetCredential(proxyUri, authType).UserName, username));
            Assert.True(string.Equals(proxy.Credentials.GetCredential(proxyUri, authType).Password, password));
        }

        [Theory,
        InlineData("", "password"),
        InlineData("username", ""),
        InlineData("", "")]
        public void ConfigureProxy_ShouldNotSetCredentialsIfUsernameOrPasswordOrBothAreEmpty(string username, string password)
        {
            const string uri = "http://proxy-example.com:9090";

            var proxy = Utils.ConfigureProxy(uri, username, password);

            Assert.NotNull(proxy);
            Assert.Null(proxy.Credentials);
        }

        [Theory,
        InlineData("Test", null),
        InlineData("Test", ""),
        InlineData("Test", "Development")]
        public void IsIgnoredEnvironment_ShouldReturnFalseIfEnvironmentShouldNotBeIgnored(string environment, string environmentsToIgnore)
        {
            IList<string> ignoredEnvironments = null;
            if (environmentsToIgnore != null)
                ignoredEnvironments = environmentsToIgnore.Split(',');

            var shouldIgnore = Utils.IsIgnoredEnvironment(environment, ignoredEnvironments);

            Assert.False(shouldIgnore);
        }

        [Theory,
        InlineData("Test", "Test, Development"),
        InlineData("TEST", "Test, Development")]
        public void IsIgnoredEnvironment_ShouldReturnTrueIfEnvironmentShouldBeIgnored(string environment, string environmentsToIgnore)
        {
            IList<string> ignoredEnvironments = null;
            if (environmentsToIgnore != null)
                ignoredEnvironments = environmentsToIgnore.Split(',');

            var shouldIgnore = Utils.IsIgnoredEnvironment(environment, ignoredEnvironments);

            Assert.True(shouldIgnore);
        }

        [Fact]
        public void FilterParameters_ShouldKeepOnlyParamsFromWhiteList()
        {
            var whiteList = new List<string> { @"\bname\b", "email" };
            var blackList = new List<string>();

            var parameters = new Dictionary<string, string>
            {
                {"user_name", "John"},
                {"password", "s3kr3t"},
                {"email", "john@example.com"},
                {"account_id", "42"}
            };

            var result = Utils.FilterParameters(parameters, Utils.CompileRegex(blackList), Utils.CompileRegex(whiteList));

            Assert.True(result["password"].Equals("[Filtered]"));
            Assert.True(result["account_id"].Equals("[Filtered]"));
            Assert.True(result["user_name"].Equals("[Filtered]"));
            Assert.True(result["email"].Equals(parameters["email"]));
        }

        [Fact]
        public void FilterParameters_ShouldFilterParamsFromBlacklist()
        {
            var blackList = new List<string> { "password", "email", @"account\b" };
            var whiteList = new List<string>();

            var parameters = new Dictionary<string, string>
            {
                {"user", "John"},
                {"Password", "s3kr3t"},
                {"email", "john@example.com"},
                {"account_id", "42"}
            };

            var result = Utils.FilterParameters(parameters, Utils.CompileRegex(blackList), Utils.CompileRegex(whiteList));

            Assert.True(result["user"].Equals(parameters["user"]));
            Assert.True(result["Password"].Equals("[Filtered]"));
            Assert.True(result["email"].Equals("[Filtered]"));
            Assert.True(result["account_id"].Equals(parameters["account_id"]));
        }

        [Fact]
        public void FilterParameters_ShouldKeepParamsFromWhitelistAndIgnoreTheRest()
        {
            var blackList = new List<string> { "Password" };
            var whiteList = new List<string> { "user", "Email" };

            var parameters = new Dictionary<string, string>
            {
                {"user", "John"},
                {"password", "s3kr3t"},
                {"email", "john@example.com"},
                {"account_id", "42"}
            };

            var result = Utils.FilterParameters(parameters, Utils.CompileRegex(blackList), Utils.CompileRegex(whiteList));

            Assert.True(result["user"].Equals(parameters["user"]));
            Assert.True(result["email"].Equals(parameters["email"]));
            Assert.True(result["password"].Equals("[Filtered]"));
            Assert.True(result["account_id"].Equals("[Filtered]"));
        }

        [Fact]
        public void FilterParameters_ShouldKeepParamsWhenBlacklistAndWhitelistAreEmpty()
        {
            var blackList = new List<string>();
            var whiteList = new List<string>();

            var parameters = new Dictionary<string, string>
            {
                {"user", "John"},
                {"password", "s3kr3t"},
                {"email", "john@example.com"},
                {"account_id", "42"}
            };

            var result = Utils.FilterParameters(parameters, Utils.CompileRegex(blackList), Utils.CompileRegex(whiteList));

            Assert.True(result["user"].Equals(parameters["user"]));
            Assert.True(result["password"].Equals(parameters["password"]));
            Assert.True(result["email"].Equals(parameters["email"]));
            Assert.True(result["account_id"].Equals(parameters["account_id"]));
        }

        [Fact]
        public void TruncateParameters_ShouldNotModifyParametersIfInputIsNull()
        {
            var result = Utils.TruncateParameters(null, 0);
            Assert.Null(result);
        }

        [Fact]
        public void TruncateParameters_ShouldNotModifyParametersWithinStringLimit()
        {
            var parameters = new Dictionary<string, string>
            {
                {"email", "john@example.com"},
                {"account_id", ""}
            };

            var result = Utils.TruncateParameters(parameters, 16);

            Assert.NotNull(result);
            Assert.True(result["email"] == "john@example.com");
        }

        [Fact]
        public void TruncateParameters_ShouldModifyParametersLongerThanStringLimit()
        {
            var parameters = new Dictionary<string, string>
            {
                {"email", "john@example.com"},
                {"account_id", ""}
            };

            var result = Utils.TruncateParameters(parameters, 4);

            Assert.NotNull(result);
            Assert.True(result["email"] == "john...");
        }

        [Fact]
        public void GetBacktrace_ShouldReturnBlankFrameIfStackTraceNotAvailable()
        {
            var backtrace = Utils.GetBacktrace(new Exception());

            Assert.NotNull(backtrace);
            Assert.True(backtrace.Count == 1);
            Assert.True(backtrace[0].File == "none");
            Assert.True(backtrace[0].Column == 0);
            Assert.True(backtrace[0].Line == 0);
        }

        [Fact]
        public void GetBacktrace_ShouldReturnBlankFrameIfExceptionOccurs()
        {
            var backtrace = Utils.GetBacktrace(null);

            Assert.NotNull(backtrace);
            Assert.True(backtrace.Count == 1);
            Assert.True(backtrace[0].File == "none");
            Assert.True(backtrace[0].Column == 0);
            Assert.True(backtrace[0].Line == 0);
        }

        [Fact]
        public void GetBacktrace_ShouldGetNonEmptyFrameProperties()
        {
            // Currently throwing the real exception is the only way
            // to get non-empty stack trace.
            // TODO: Consider on more reliable way for testing GetBacktrace functionality
            try
            {
                throw new Exception("test message");
            }
            catch (Exception ex)
            {
                var backtrace = Utils.GetBacktrace(ex);

                Assert.NotNull(backtrace);
                Assert.True(backtrace.Count == 1);
                Assert.True(!string.IsNullOrEmpty(backtrace[0].File));
                //Assert.True(backtrace[0].Column != 0);
                //Assert.True(backtrace[0].Line != 0);
            }
        }

        [Fact]
        public void ApplyFilters_ShouldReturnNoticeIfFiltersIsNull()
        {
            var noticeIn = new Notice
            {
                Context = new Context
                {
                    Action = "test"
                }
            };

            var noticeOut = Utils.ApplyFilters(noticeIn, null);

            Assert.NotNull(noticeOut);
            Assert.NotNull(noticeOut.Context);
            Assert.True(noticeOut.Context.Action == "test");
        }

        [Fact]
        public void ApplyFilters_ShouldReturnNoticeIfFiltersIsEmpty()
        {
            var noticeIn = new Notice
            {
                Context = new Context
                {
                    Action = "test action"
                }
            };

            var noticeOut = Utils.ApplyFilters(noticeIn, new List<Func<Notice, Notice>>());

            Assert.NotNull(noticeOut);
            Assert.NotNull(noticeOut.Context);
            Assert.True(noticeOut.Context.Action == "test action");
        }

        [Fact]
        public void ApplyFilters_ShouldReturnModifiedNotice()
        {
            var noticeIn = new Notice
            {
                Context = new Context
                {
                    Action = "test action"
                }
            };

            var noticeOut = Utils.ApplyFilters(noticeIn, new List<Func<Notice, Notice>>
            {
                notice =>
                {
                    notice.Context.Action = "updated action";
                    return notice;
                }
            });

            Assert.NotNull(noticeOut);
            Assert.NotNull(noticeOut.Context);
            Assert.True(noticeOut.Context.Action == "updated action");
        }

        [Fact]
        public void ApplyFilters_ShouldApplyFiltersInOrder()
        {
            var noticeIn = new Notice
            {
                Context = new Context
                {
                    Action = "test action"
                }
            };

            var noticeOut = Utils.ApplyFilters(noticeIn, new List<Func<Notice, Notice>>
            {
                notice =>
                {
                    notice.Context.Action += " update 1";
                    return notice;
                },
                notice =>
                {
                    notice.Context.Action += " update 2";
                    return notice;
                }
            });

            Assert.NotNull(noticeOut);
            Assert.NotNull(noticeOut.Context);
            Assert.True(noticeOut.Context.Action == "test action update 1 update 2");
        }
    }
}
