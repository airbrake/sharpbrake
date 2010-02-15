using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

		public HoptoadConfiguration Configuration
		{
			get { return this.configuration; }
		}

		public HoptoadServerEnvironment ServerEnvironment()
		{
			var environment = new HoptoadServerEnvironment { 
				EnvironmentName = this.Configuration.EnvironmentName,
				ProjectRoot = this.Configuration.ProjectRoot
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
				ApiKey = this.Configuration.ApiKey,
				Error = error,
				Notifier = this.Notifier(),
				ServerEnvironment = this.ServerEnvironment(),
			};
			return notice;
		}

		public HoptoadNotice Notice(Exception exception)
		{
			var notice = new HoptoadNotice { 
				ApiKey = this.Configuration.ApiKey,
				Error = this.ErrorFromException(exception),
				Notifier = this.Notifier(),
				ServerEnvironment = this.ServerEnvironment(),
			};
			return notice;
		}

		public HoptoadError ErrorFromException(Exception exception)
		{
			var error = new HoptoadError { 
				Class = exception.GetType().FullName,
				Message = exception.GetType().Name + ": " + exception.Message,
				Backtrace = this.BuildBacktrace(exception).ToArray(),
			};
			return error;
		}

		private IEnumerable<TraceLine> BuildBacktrace(Exception exception)
		{
			var stackTrace = new StackTrace(exception);
			var frames = stackTrace.GetFrames();
			foreach (var frame in frames)
			{
				var method = frame.GetMethod();

				var lineNumber = frame.GetFileLineNumber();
				if (lineNumber == 0)
					lineNumber = frame.GetILOffset();

				var file = frame.GetFileName();
				if (string.IsNullOrEmpty(file))
					file = method.ReflectedType.FullName;

				yield return new TraceLine { 
					File = file,
					LineNumber = lineNumber,
					Method = method.Name
				};
			}
		}
	}
}