using HopSharp.Serialization;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ErrorSchemaValidation
    {
        [Test]
        public void Minimal_notice_generates_valid_XML()
        {
            var notice = new HoptoadNotice();
            notice.ApiKey = "123456";
            notice.Error = new HoptoadError
                               {
                                   Class = "TestError",
                                   Message = "something blew up",
                                   Backtrace = new[]
                                                   {
                                                       new HoptoadTraceLine ("unknown.cs", 0) { Method = "unknown"}
                                                   }
                               };
            notice.Notifier = new HoptoadNotifier
                                  {
                                      Name = "hopsharp",
                                      Version = "2.0",
                                      Url = "http://github.com/krobertson/hopsharp"
                                  };
            notice.ServerEnvironment = new HoptoadServerEnvironment("staging")
                                           {
                                               ProjectRoot = "/test",
                                           };

            var serializer = new CleanXmlSerializer<HoptoadNotice>();
            string xml = serializer.ToXml(notice);

            HoptoadValidator.ValidateSchema(xml);
        }
    }
}