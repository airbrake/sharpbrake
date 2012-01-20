using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;

using Common.Logging;

using SharpBrake.Serialization;

namespace SharpBrake
{
    /// <summary>
    /// Responsible for building the notice that is sent to Airbrake.
    /// </summary>
    public class AirbrakeNoticeBuilder
    {
        private readonly AirbrakeConfiguration configuration;
        private readonly ILog log;
        private AirbrakeServerEnvironment environment;
        private AirbrakeNotifier notifier;


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeNoticeBuilder"/> class.
        /// </summary>
        public AirbrakeNoticeBuilder()
            : this(new AirbrakeConfiguration())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeNoticeBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public AirbrakeNoticeBuilder(AirbrakeConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            this.configuration = configuration;
            this.log = LogManager.GetLogger(GetType());
        }


        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public AirbrakeConfiguration Configuration
        {
            get { return this.configuration; }
        }

        /// <summary>
        /// Gets the notifier.
        /// </summary>
        public AirbrakeNotifier Notifier
        {
            get
            {
                return this.notifier ?? (this.notifier = new AirbrakeNotifier
                {
                    Name = "SharpBrake",
                    Url = "https://github.com/asbjornu/SharpBrake",
                    Version = typeof(AirbrakeNotice).Assembly.GetName().Version.ToString()
                });
            }
        }

        /// <summary>
        /// Gets the server environment.
        /// </summary>
        public AirbrakeServerEnvironment ServerEnvironment
        {
            get
            {
                string env = this.configuration.EnvironmentName;
                return this.environment ?? (this.environment = new AirbrakeServerEnvironment(env)
                {
                    ProjectRoot = this.configuration.ProjectRoot
                });
            }
        }


        /// <summary>
        /// Creates a <see cref="AirbrakeError"/> from the the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// A <see cref="AirbrakeError"/>, created from the the specified exception.
        /// </returns>
        public AirbrakeError ErrorFromException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            this.log.Debug(f => f("{0}.Notice({1})", GetType(), exception.GetType()), exception);

            var error = new AirbrakeError
            {
                Class = exception.GetType().FullName,
                Message = exception.GetType().Name + ": " + exception.Message,
                Backtrace = BuildBacktrace(exception).ToArray(),
            };

            return error;
        }


        /// <summary>
        /// Creates a <see cref="AirbrakeNotice"/> from the the specified error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public AirbrakeNotice Notice(AirbrakeError error)
        {
            this.log.Debug(f => f("{0}.Notice({1})", GetType(), error));

            var notice = new AirbrakeNotice
            {
                ApiKey = Configuration.ApiKey,
                Error = error,
                Notifier = Notifier,
                ServerEnvironment = ServerEnvironment,
            };

            if (HttpContext.Current != null)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                notice.Request = new AirbrakeRequest(HttpContext.Current.Request.Url, assembly.CodeBase)
                {
                    Params = BuildParams().ToArray(),
                    Session = BuildSession().ToArray(),
                    CgiData = BuildCgiData().ToArray(),
                };
            }

            return notice;
        }


        /// <summary>
        /// Creates a <see cref="AirbrakeNotice"/> from the the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// A <see cref="AirbrakeNotice"/>, created from the the specified exception.
        /// </returns>
        public AirbrakeNotice Notice(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            this.log.Debug(f => f("{0}.Notice({1})", GetType(), exception.GetType()), exception);

            var notice = new AirbrakeNotice
            {
                ApiKey = Configuration.ApiKey,
                Error = ErrorFromException(exception),
                Notifier = Notifier,
                ServerEnvironment = ServerEnvironment,
            };

            return notice;
        }


        private static IEnumerable<AirbrakeTraceLine> BuildBacktrace(Exception exception)
        {
            var stackTrace = new StackTrace(exception);
            StackFrame[] frames = stackTrace.GetFrames();

            if (frames == null || frames.Length == 0)
            {
                // Airbrake requires that at least one line is present in the XML.
                yield return new AirbrakeTraceLine("none", 0);
                yield break;
            }

            foreach (StackFrame frame in frames)
            {
                MethodBase method = frame.GetMethod();

                int lineNumber = frame.GetFileLineNumber();

                if (lineNumber == 0)
                    lineNumber = frame.GetILOffset();

                string file = frame.GetFileName();

                if (string.IsNullOrEmpty(file) && method.ReflectedType != null)
                {
                    file = method.ReflectedType.FullName;
                }
                else
                {
                    file = "unknown";
                }

                yield return new AirbrakeTraceLine(file, lineNumber)
                {
                    Method = method.Name
                };
            }
        }


        private static IEnumerable<AirbrakeVar> BuildCgiData()
        {
            return from key in HttpContext.Current.Request.ServerVariables.AllKeys
                   let value = HttpContext.Current.Request.ServerVariables[key]
                   select new AirbrakeVar(key, value);
        }


        private static IEnumerable<AirbrakeVar> BuildParams()
        {
            return from key in HttpContext.Current.Request.Params.AllKeys
                   let value = HttpContext.Current.Request.Params[key]
                   select new AirbrakeVar(key, value);
        }


        private static IEnumerable<AirbrakeVar> BuildSession()
        {
            return from key in HttpContext.Current.Session.Keys.Cast<string>()
                   let v = HttpContext.Current.Session[key]
                   let value = v != null ? v.ToString() : null
                   select new AirbrakeVar(key, value);
        }
    }
}