using System;
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
			var notice = this.builder.Notice((HoptoadError) null);
			Assert.IsNotNull(notice.ServerEnvironment);
			Assert.IsNotNull(notice.Notifier);
			Assert.IsNotEmpty(notice.ApiKey);
			Assert.IsNotEmpty(notice.Version);
		}

		[Test]
		public void Building_error_from_dotNET_exception()
		{
			Exception exception = null;
			try
			{
				throw new InvalidOperationException("test error");
			}
			catch (Exception testException)
			{
				exception = testException;
			}

			var error = this.builder.ErrorFromException(exception);
			Assert.AreNotEqual(0, error.Backtrace.Length);
			
			var trace = error.Backtrace[0];
			Assert.AreEqual("Building_error_from_dotNET_exception", trace.Method);
			Assert.AreNotEqual(0, trace.LineNumber);
		}
	}
}