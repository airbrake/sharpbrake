using System;
using HopSharp;
using HopSharp.Serialization;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class NoticeComponentsCreation
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _config = new HoptoadConfiguration
                         {
                             ApiKey = "123456",
                             EnvironmentName = "test"
                         };
            _builder = new HoptoadNoticeBuilder(_config);
        }

        #endregion

        private HoptoadConfiguration _config;
        private HoptoadNoticeBuilder _builder;

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

            HoptoadError error = _builder.ErrorFromException(exception);
            Assert.AreNotEqual(0, error.Backtrace.Length);

            TraceLine trace = error.Backtrace[0];
            Assert.AreEqual("Building_error_from_dotNET_exception", trace.Method);
            Assert.AreNotEqual(0, trace.LineNumber);
        }

        [Test]
        public void Notice_contains_ServerEnvironment_and_Notifier()
        {
            HoptoadNotice notice = _builder.Notice((HoptoadError) null);
            Assert.IsNotNull(notice.ServerEnvironment);
            Assert.IsNotNull(notice.Notifier);
            Assert.IsNotEmpty(notice.ApiKey);
            Assert.IsNotEmpty(notice.Version);
        }

        [Test]
        public void Notifier_initialized_correctly()
        {
            HoptoadNotifier notifier = _builder.Notifier;
            Assert.AreEqual("hopsharp", notifier.Name);
            Assert.AreEqual("http://github.com/krobertson/hopsharp", notifier.Url);
            Assert.AreEqual("2.0.0.0", notifier.Version);
        }

        [Test]
        public void Server_environment_read_from_Hoptoad_config()
        {
            HoptoadServerEnvironment environment = _builder.ServerEnvironment;
            Assert.AreEqual(_config.EnvironmentName, environment.EnvironmentName);
        }
    }
}