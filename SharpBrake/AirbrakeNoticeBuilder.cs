using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.SessionState;

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

            Type catchingType;
            var backtrace = BuildBacktrace(exception, out catchingType);

            var error = Activator.CreateInstance<AirbrakeError>();

            error.CatchingType = catchingType;
            error.Class = exception.GetType().FullName;
            error.Message = exception.GetType().Name + ": " + exception.Message;
            error.Backtrace = backtrace;

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

            Type catchingType = (error != null)
                                    ? error.CatchingType
                                    : null;

            AddInfoFromHttpContext(notice, catchingType);

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

            AirbrakeError error = ErrorFromException(exception);

            return Notice(error);
        }


        private static void AddInfoFromHttpContext(AirbrakeNotice notice, Type throwingType)
        {
            HttpContext httpContext = HttpContext.Current;

            if (httpContext == null)
                return;

            HttpRequest request = httpContext.Request;

            string component = (throwingType != null)
                                   ? throwingType.FullName
                                   : String.Empty;

            notice.Request = new AirbrakeRequest(request.Url, component)
            {
                CgiData = BuildVars(request.Headers).ToArray(),
                Params = BuildVars(request.Params).ToArray(),
                Session = BuildVars(httpContext.Session).ToArray(),
            };
        }


        private static IEnumerable<AirbrakeVar> BuildVars(NameValueCollection formData)
        {
            return from key in formData.AllKeys
                   where !String.IsNullOrEmpty(key)
                   let value = formData[key]
                   where !String.IsNullOrEmpty(value)
                   select new AirbrakeVar(key, value);
        }


        private static IEnumerable<AirbrakeVar> BuildVars(HttpSessionState httpSessionState)
        {
            return from key in httpSessionState.Keys.Cast<string>()
                   where !String.IsNullOrEmpty(key)
                   let v = httpSessionState[key]
                   let value = v != null ? v.ToString() : null
                   where !String.IsNullOrEmpty(value)
                   select new AirbrakeVar(key, value);
        }


        private AirbrakeTraceLine[] BuildBacktrace(Exception exception, out Type catchingType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            if (assembly.EntryPoint == null)
                assembly = Assembly.GetCallingAssembly();

            if (assembly.EntryPoint == null)
                assembly = Assembly.GetEntryAssembly();

            catchingType = assembly == null || assembly.EntryPoint == null
                               ? null
                               : assembly.EntryPoint.DeclaringType;

            List<AirbrakeTraceLine> lines = new List<AirbrakeTraceLine>();
            var stackTrace = new StackTrace(exception);
            StackFrame[] frames = stackTrace.GetFrames();

            if (frames == null || frames.Length == 0)
            {
                // Airbrake requires that at least one line is present in the XML.
                AirbrakeTraceLine line = new AirbrakeTraceLine("none", 0);
                lines.Add(line);
                return lines.ToArray();
            }

            foreach (StackFrame frame in frames)
            {
                MethodBase method = frame.GetMethod();

                catchingType = method.DeclaringType;

                int lineNumber = frame.GetFileLineNumber();

                if (lineNumber == 0)
                {
                    this.log.Debug(f => f("No line number found in {0}, using IL offset instead.", method));
                    lineNumber = frame.GetILOffset();
                }

                string file = frame.GetFileName();

                /*if (String.IsNullOrEmpty(file))
                    file = method.ReflectedType != null ? method.ReflectedType.FullName : "(unknown)";*/

                if (String.IsNullOrEmpty(file))
                    file = method.ReflectedType.FullName;

                AirbrakeTraceLine line = new AirbrakeTraceLine(file, lineNumber)
                {
                    Method = method.Name
                };

                lines.Add(line);
            }

            return lines.ToArray();
        }
    }
}