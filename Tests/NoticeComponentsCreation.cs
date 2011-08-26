using System;
using NUnit.Framework;

using SharpBrake;
using SharpBrake.Serialization;

namespace Tests
{
    [TestFixture]
    public class NoticeComponentsCreation
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _config = new AirbrakeConfiguration
                         {
                             ApiKey = "123456",
                             EnvironmentName = "test"
                         };
            _builder = new AirbrakeNoticeBuilder(_config);
        }

        #endregion

        private AirbrakeConfiguration _config;
        private AirbrakeNoticeBuilder _builder;

        [Test]
        public void Building_error_from_dotNET_exception()
        {
            Exception exception;

            try
            {
                throw new InvalidOperationException("test error");
            }
            catch (Exception testException)
            {
                exception = testException;
            }

            AirbrakeError error = _builder.ErrorFromException(exception);
            Assert.AreNotEqual(0, error.Backtrace.Length);

            AirbrakeTraceLine trace = error.Backtrace[0];
            Assert.AreEqual("Building_error_from_dotNET_exception", trace.Method);
            Assert.AreNotEqual(0, trace.LineNumber);
        }

        [Test]
        public void Notice_contains_ServerEnvironment_and_Notifier()
        {
            AirbrakeNotice notice = _builder.Notice((AirbrakeError) null);
            Assert.IsNotNull(notice.ServerEnvironment);
            Assert.IsNotNull(notice.Notifier);
            Assert.IsNotEmpty(notice.ApiKey);
            Assert.IsNotEmpty(notice.Version);
        }

        [Test]
        public void Notifier_initialized_correctly()
        {
            AirbrakeNotifier notifier = _builder.Notifier;
            Assert.AreEqual("hopsharp", notifier.Name);
            Assert.AreEqual("http://github.com/krobertson/hopsharp", notifier.Url);
            Assert.AreEqual("2.1.0.0", notifier.Version);
        }

        [Test]
        public void Server_environment_read_from_Airbrake_config()
        {
            AirbrakeServerEnvironment environment = _builder.ServerEnvironment;
            Assert.AreEqual(_config.EnvironmentName, environment.EnvironmentName);
        }
    }
}