using System;
using System.Reflection;
using HopSharp.Serialization;

namespace HopSharp
{
	public class HoptoadNoticeBuilder
	{
		private readonly HoptoadConfiguration configuration;

		public HoptoadNoticeBuilder() : this(new HoptoadConfiguration())
		{	
		}

		public HoptoadNoticeBuilder(HoptoadConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public HoptoadServerEnvironment ServerEnvironment()
		{
			var environment = new HoptoadServerEnvironment { 
				EnvironmentName = this.configuration.EnvironmentName,
				ProjectRoot = this.configuration.ProjectRoot
			};
			return environment;
		}

		public HoptoadNotifier Notifier()
		{
			var notifer = new HoptoadNotifier { 
				Name = "hopsharp",
				Url = "http://github.com/krobertson/hopsharp",
				Version = typeof(HoptoadNotice).Assembly.GetName().Version.ToString()
			};
			return notifer;
		}

		public HoptoadNotice Notice(HoptoadError error)
		{
			var notice = new HoptoadNotice { 
				ApiKey = this.configuration.ApiKey,
				Error = error,
				Notifier = this.Notifier(),
				ServerEnvironment = this.ServerEnvironment(),
			};
			return notice;
		}
	}
}