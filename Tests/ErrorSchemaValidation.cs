using System;

using NUnit.Framework;

using SharpBrake.Serialization;

namespace SharpBrake.Tests
{
    [TestFixture]
    public class ErrorSchemaValidation
    {
        [Test]
        public void Minimal_notice_generates_valid_XML()
        {
            var notice = new AirbrakeNotice
            {
                ApiKey = "123456",
                Error = Activator.CreateInstance<AirbrakeError>()
            };

            notice.Error.Class = "TestError";
            notice.Error.Message = "something blew up";
            notice.Error.Backtrace = new[]
            {
                new AirbrakeTraceLine("unknown.cs", 0) { Method = "unknown" }
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