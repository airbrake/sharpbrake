using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Common.Logging;

using HopSharp.Serialization;

namespace HopSharp
{
   /// <summary>
   /// Responsible for building the notice that is sent to HopToad.
   /// </summary>
    public class HoptoadNoticeBuilder
    {
        private readonly HoptoadConfiguration _configuration;
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoptoadNoticeBuilder"/> class.
        /// </summary>
        public HoptoadNoticeBuilder()
           : this(new HoptoadConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HoptoadNoticeBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public HoptoadNoticeBuilder(HoptoadConfiguration configuration)
        {
           if (configuration == null)
              throw new ArgumentNullException("configuration");

           _configuration = configuration;
           _log = LogManager.GetCurrentClassLogger();
        }


      /// <summary>
        /// Gets the configuration.
        /// </summary>
        public HoptoadConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        /// Gets the server environment.
        /// </summary>
        public HoptoadServerEnvironment ServerEnvironment
        {
           get
           {
              var environment = new HoptoadServerEnvironment
              {
                 EnvironmentName = Configuration.EnvironmentName,
                 ProjectRoot = Configuration.ProjectRoot
              };
              return environment;
           }
        }

        /// <summary>
        /// Gets the notifier.
        /// </summary>
        public HoptoadNotifier Notifier
        {
           get
           {
              var notifer = new HoptoadNotifier
              {
                 Name = "hopsharp",
                 Url = "http://github.com/krobertson/hopsharp",
                 Version = typeof(HoptoadNotice).Assembly.GetName().Version.ToString()
              };

              return notifer;
           }
        }

        /// <summary>
        /// Creates a <see cref="HoptoadNotice"/> from the the specified error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public HoptoadNotice Notice(HoptoadError error)
        {
            _log.DebugFormat("{0}.Notice({1})", GetType(), error);

            var notice = new HoptoadNotice
                             {
                                 ApiKey = Configuration.ApiKey,
                                 Error = error,
                                 Notifier = Notifier,
                                 ServerEnvironment = ServerEnvironment,
                             };
            return notice;
        }

        /// <summary>
        /// Creates a <see cref="HoptoadNotice"/> from the the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// A <see cref="HoptoadNotice"/>, created from the the specified exception.
        /// </returns>
        public HoptoadNotice Notice(Exception exception)
        {
           if (exception == null)
              throw new ArgumentNullException("exception");

            _log.DebugFormat("{0}.Notice({1})", exception, GetType(), exception.GetType());

            var notice = new HoptoadNotice
                             {
                                 ApiKey = Configuration.ApiKey,
                                 Error = ErrorFromException(exception),
                                 Notifier = Notifier,
                                 ServerEnvironment = ServerEnvironment,
                             };
            return notice;
        }

        /// <summary>
        /// Creates a <see cref="HoptoadError"/> from the the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// A <see cref="HoptoadError"/>, created from the the specified exception.
        /// </returns>
        public HoptoadError ErrorFromException(Exception exception)
        {
           if (exception == null)
              throw new ArgumentNullException("exception");

           _log.DebugFormat("{0}.Notice({1})", exception, GetType(), exception.GetType());
           
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