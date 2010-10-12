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
        private readonly HoptoadConfiguration _configuration;

        public HoptoadNoticeBuilder() : this(new HoptoadConfiguration())
        {
        }

        public HoptoadNoticeBuilder(HoptoadConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HoptoadConfiguration Configuration
        {
            get { return _configuration; }
        }

        public HoptoadServerEnvironment ServerEnvironment()
        {
            var environment = new HoptoadServerEnvironment
                                  {
                                      EnvironmentName = Configuration.EnvironmentName,
                                      ProjectRoot = Configuration.ProjectRoot
                                  };
            return environment;
        }

        public HoptoadNotifier Notifier()
        {
            var notifer = new HoptoadNotifier
                              {
                                  Name = "hopsharp",
                                  Url = "http://github.com/krobertson/hopsharp",
                                  Version = typeof (HoptoadNotice).Assembly.GetName().Version.ToString()
                              };
            return notifer;
        }

        public HoptoadNotice Notice(HoptoadError error)
        {
            var notice = new HoptoadNotice
                             {
                                 ApiKey = Configuration.ApiKey,
                                 Error = error,
                                 Notifier = Notifier(),
                                 ServerEnvironment = ServerEnvironment(),
                             };
            return notice;
        }

        public HoptoadNotice Notice(Exception exception)
        {
            var notice = new HoptoadNotice
                             {
                                 ApiKey = Configuration.ApiKey,
                                 Error = ErrorFromException(exception),
                                 Notifier = Notifier(),
                                 ServerEnvironment = ServerEnvironment(),
                             };
            return notice;
        }

        public HoptoadError ErrorFromException(Exception exception)
        {
            var error = new HoptoadError
                            {
                                Class = exception.GetType().FullName,
                                Message = exception.GetType().Name + ": " + exception.Message,
                                Backtrace = BuildBacktrace(exception).ToArray(),
                            };
            return error;
        }

        private static IEnumerable<TraceLine> BuildBacktrace(Exception exception)
        {
            var stackTrace = new StackTrace(exception);
            StackFrame[] frames = stackTrace.GetFrames();
            if (frames != null)
                foreach (StackFrame frame in frames)
                {
                    MethodBase method = frame.GetMethod();

                    int lineNumber = frame.GetFileLineNumber();
                    if (lineNumber == 0)
                        lineNumber = frame.GetILOffset();

                    string file = frame.GetFileName();
                    if (string.IsNullOrEmpty(file))
                        file = method.ReflectedType.FullName;

                    yield return new TraceLine
                                     {
                                         File = file,
                                         LineNumber = lineNumber,
                                         Method = method.Name
                                     };
                }
        }
    }
}