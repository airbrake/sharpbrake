using System;
using Common.Logging;
using NUnit.Framework;

using SharpBrake.Serialization;

namespace SharpBrake.Tests
{
    [TestFixture]
    public class AirbrakeClientTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.client = new AirbrakeClient();
        }

        #endregion

        private AirbrakeClient client;


        [Test]
        [Ignore("This test needs to be rewritten for the 2.2 API")]
        public void Send_EndRequestEventIsInvoked_And_ResponseOnlyContainsApiError()
        {
            bool requestEndInvoked = false;
            AirbrakeResponseError[] errors = null;
            int i = 0;

            this.client.RequestEnd += (sender, e) =>
            {
                requestEndInvoked = true;
                errors = e.Response.Errors;
            };

            var configuration = new AirbrakeConfiguration
            {
                ApiKey = Guid.NewGuid().ToString("N"),
                EnvironmentName = "test",
            };

            var builder = new AirbrakeNoticeBuilder(configuration, new BacktraceBuilder(LogManager.GetLogger<BacktraceBuilder>()));

            AirbrakeNotice notice = builder.Notice(new Exception("Test"));

            notice.Request = new AirbrakeRequest("http://example.com", "Test")
            {
                Params = new[]
                {
                    new AirbrakeVar("TestKey", "TestValue")
                }
            };

            this.client.Send(notice);

            Assert.That(requestEndInvoked, Is.True.After(5000));
            Assert.That(errors, Is.Not.Null);
            Assert.That(errors, Has.Length.EqualTo(1));
        }
    }
}