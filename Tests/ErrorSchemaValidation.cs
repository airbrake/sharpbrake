using NUnit.Framework;

using SharpBrake.Serialization;

namespace Tests
{
    [TestFixture]
    public class ErrorSchemaValidation
    {
        [Test]
        public void Minimal_notice_generates_valid_XML()
        {
            var notice = new AirbrakeNotice();
            notice.ApiKey = "123456";
            notice.Error = new AirbrakeError
            {
                Class = "TestError",
                Message = "something blew up",
                Backtrace = new[]
                {
                    new AirbrakeTraceLine("unknown.cs", 0) { Method = "unknown" }
                }
            };
            notice.Notifier = new AirbrakeNotifier
            {
                Name = "hopsharp",
                Version = "2.0",
                Url = "http://github.com/krobertson/hopsharp"
            };
            notice.ServerEnvironment = new AirbrakeServerEnvironment("staging")
            {
                ProjectRoot = "/test",
            };

            var serializer = new CleanXmlSerializer<AirbrakeNotice>();
            string xml = serializer.ToXml(notice);

            AirbrakeValidator.ValidateSchema(xml);
        }
    }
}