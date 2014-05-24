using System;
using System.IO;
using System.Linq.Expressions;
using Common.Logging;
using NUnit.Framework;

using SharpBrake.Serialization;

using Subtext.TestLibrary;

namespace SharpBrake.Tests
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
            this.builder = new AirbrakeNoticeBuilder(this.config,new BacktraceBuilder(LogManager.GetLogger<BacktraceBuilder>()));
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
            Assert.That(error.Backtrace, Has.Length.GreaterThan(0));

            AirbrakeTraceLine trace = error.Backtrace[0];
            Assert.That(trace.Method, Is.EqualTo("Building_error_from_dotNET_exception"));
            Assert.That(trace.LineNumber, Is.GreaterThan(0));
        }


        [Test]
        public void Notice_contains_Request()
        {
            AirbrakeNotice notice = null;
            const string url = "http://example.com/?Query.Key1=Query.Value1&Query.Key2=Query.Value2";
            const string referer = "http://github.com/";
            string physicalApplicationPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar;
            var httpSimulator = new HttpSimulator("/", physicalApplicationPath)
                .SetFormVariable("Form.Key1", "Form.Value1")
                .SetFormVariable("Form.Key2", "Form.Value2")
                .SetHeader("Header.Key1", "Header.Value1")
                .SetHeader("Header.Key2", "Header.Value2")
                .SetReferer(new Uri(referer))
                .SimulateRequest(new Uri(url));

            using (httpSimulator)
            {
                try
                {
                    Thrower.Throw(new Exception("Halp!"));
                }
                catch (Exception exception)
                {
                    AirbrakeError error = this.builder.ErrorFromException(exception);
                    notice = this.builder.Notice(error);
                }
            }

            Console.WriteLine(CleanXmlSerializer.ToXml(notice));

            Assert.That(notice, Is.Not.Null);
            Assert.That(notice.Error, Is.Not.Null);

#if !NET35
            // We have defined a NET35 constant in the Visual Studio 2008 project so the below code isn't executed,
            // since it requires HttpSimulator which in turn requires .NET 4.0, which in turn requires Visual Studio 2010.
            Assert.That(notice.Request, Is.Not.Null);
            Assert.That(notice.Request.Url, Is.EqualTo(url));
            Assert.That(notice.Request.Component, Is.EqualTo(typeof(Thrower).FullName));
            Assert.That(notice.Request.Action, Is.EqualTo("Throw"));

            Assert.That(notice.Request.CgiData,
                        Contains.Item(new AirbrakeVar("Content-Type", "application/x-www-form-urlencoded")));
            Assert.That(notice.Request.CgiData,
                        Contains.Item(new AirbrakeVar("Header.Key1", "Header.Value1")));
            Assert.That(notice.Request.CgiData,
                        Contains.Item(new AirbrakeVar("Header.Key2", "Header.Value2")));
            Assert.That(notice.Request.CgiData, Contains.Item(new AirbrakeVar("Referer", referer)));

            Assert.That(notice.Request.Params,
                        Contains.Item(new AirbrakeVar("APPL_PHYSICAL_PATH", physicalApplicationPath)));
            Assert.That(notice.Request.Params,
                        Contains.Item(new AirbrakeVar("QUERY_STRING", "Query.Key1=Query.Value1&Query.Key2=Query.Value2")));
            Assert.That(notice.Request.Params, Contains.Item(new AirbrakeVar("Form.Key1", "Form.Value1")));
            Assert.That(notice.Request.Params, Contains.Item(new AirbrakeVar("Form.Key2", "Form.Value2")));
            Assert.That(notice.Request.Params, Contains.Item(new AirbrakeVar("Query.Key1", "Query.Value1")));
            Assert.That(notice.Request.Params, Contains.Item(new AirbrakeVar("Query.Key2", "Query.Value2")));
#endif
        }


        [Test]
        public void Notice_contains_ServerEnvironment_and_Notifier()
        {
            AirbrakeNotice notice = this.builder.Notice((AirbrakeError)null);
            Assert.That(notice.ServerEnvironment, Is.Not.Null);
            Assert.That(notice.ServerEnvironment.ProjectRoot, Is.Not.Null);
            Assert.That(notice.ServerEnvironment.EnvironmentName, Is.Not.Null);
            Assert.That(notice.Notifier, Is.Not.Null);
            Assert.That(notice.ApiKey, Is.Not.Empty);
            Assert.That(notice.Version, Is.Not.Null);
        }


        [Test]
        public void Notifier_initialized_correctly()
        {
            AirbrakeNotifier notifier = this.builder.Notifier;
            Assert.That(notifier.Name, Is.EqualTo("SharpBrake"));
            Assert.That(notifier.Url, Is.EqualTo("https://github.com/asbjornu/SharpBrake"));
            Assert.That(notifier.Version, Is.EqualTo("2.2.1.0"));
        }


        [Test]
        public void Server_environment_read_from_Airbrake_config()
        {
            AirbrakeServerEnvironment environment = this.builder.ServerEnvironment;
            Assert.That(environment.EnvironmentName, Is.EqualTo(this.config.EnvironmentName));
        }


        [Test]
        public void StackTrace_contains_lambda_expression()
        {
            Exception exception = null;

            try
            {
                Expression<Func<int>> inner = () => ((string)null).Length;

                inner.Compile()();
            }
            catch (Exception testException)
            {
                exception = testException;
            }

            AirbrakeError error = this.builder.ErrorFromException(exception);

            Assert.That(error, Is.Not.Null);
        }
    }
}