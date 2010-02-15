using HopSharp;
using HopSharp.Serialization;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class NoticeComponentsCreation
	{
		private HoptoadConfiguration config;
		private HoptoadNoticeBuilder builder;

		[SetUp]
		public void SetUp()
		{
			this.config = new HoptoadConfiguration() {
				ApiKey = "123456",
				EnvironmentName = "test"
			};
			this.builder = new HoptoadNoticeBuilder(this.config);
		}

		[Test]
		public void Server_environment_read_from_Hoptoad_config()
		{
			var environment = this.builder.ServerEnvironment();
			Assert.AreEqual(this.config.EnvironmentName, environment.EnvironmentName);
		}

		[Test]
		public void Notifier_initialized_correctly()
		{
			var notifier = this.builder.Notifier();
			Assert.AreEqual("hopsharp", notifier.Name);
			Assert.AreEqual("http://github.com/krobertson/hopsharp", notifier.Url);
			Assert.AreEqual("2.0.0.0", notifier.Version);
		}

		[Test]
		public void Notice_contains_ServerEnvironment_and_Notifier()
		{
			var notice = this.builder.Notice(null);
			Assert.IsNotNull(notice.ServerEnvironment);
			Assert.IsNotNull(notice.Notifier);
			Assert.IsNotEmpty(notice.ApiKey);
			Assert.IsNotEmpty(notice.Version);
		}
	}
}