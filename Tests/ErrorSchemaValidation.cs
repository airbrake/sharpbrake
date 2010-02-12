using HopSharp;
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
			notice.Error = new HoptoadError {
				Class = "TestError",
				Message = "something blew up",
				Backtrace = new[] { 
					new TraceLine() { File = "unknown.cs", LineNumber = 0, Method = "unknown" } 
				}
			};
			notice.Notifier = new HoptoadNotifier { 
				Name = "hopsharp",
				Version = "2.0",
				Url = "http://github.com/krobertson/hopsharp"
			};
			notice.ServerEnvironment = new HoptoadServerEnvironment { 
				ProjectRoot = "/test",
				EnvironmentName = "staging"
			};

			var serializer = new CleanXmlSerializer<HoptoadNotice>();
			var xml = serializer.ToXml(notice);

			HoptoadValidator.ValidateSchema(xml);
		}
	}
}