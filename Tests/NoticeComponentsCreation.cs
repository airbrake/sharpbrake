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
            this.config = new AirbrakeConfiguration
            {
                ApiKey = "123456",
                EnvironmentName = "test"
            };
            this.builder = new AirbrakeNoticeBuilder(this.config);
        }

        #endregion

        private AirbrakeConfiguration config;
        private AirbrakeNoticeBuilder builder;


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

            AirbrakeError error = this.builder.ErrorFromException(exception);
            Assert.AreNotEqual(0, error.Backtrace.Length);

            AirbrakeTraceLine trace = error.Backtrace[0];
            Assert.AreEqual("Building_error_from_dotNET_exception", trace.Method);
            Assert.AreNotEqual(0, trace.LineNumber);
        }


        [Test]
        public void Notice_contains_ServerEnvironment_and_Notifier()
        {
            AirbrakeNotice notice = this.builder.Notice((AirbrakeError)null);
            Assert.IsNotNull(notice.ServerEnvironment);
            Assert.IsNotNull(notice.Notifier);
            Assert.IsNotEmpty(notice.ApiKey);
            Assert.IsNotEmpty(notice.Version);
        }


        [Test]
        public void Notifier_initialized_correctly()
        {
            AirbrakeNotifier notifier = this.builder.Notifier;
            Assert.AreEqual("SharpBrake", notifier.Name);
            Assert.AreEqual("https://github.com/asbjornu/SharpBrake", notifier.Url);
            Assert.AreEqual("2.1.2.0", notifier.Version);
        }


        [Test]
        public void Server_environment_read_from_Airbrake_config()
        {
            AirbrakeServerEnvironment environment = this.builder.ServerEnvironment;
            Assert.AreEqual(this.config.EnvironmentName, environment.EnvironmentName);
        }
    }
}