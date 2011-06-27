using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;

using Common.Logging;

using HopSharp.Serialization;

namespace HopSharp
{
   /// <summary>
   /// Responsible for building the notice that is sent to Hoptoad.
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
         get { return this._configuration; }
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
      /// Gets the server environment.
      /// </summary>
      public HoptoadServerEnvironment ServerEnvironment
      {
         get
         {
            var environment = new HoptoadServerEnvironment(Configuration.EnvironmentName)
            {
               ProjectRoot = Configuration.ProjectRoot
            };

            return environment;
         }
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

         if (HttpContext.Current != null)
         {
            Assembly assembly = Assembly.GetExecutingAssembly();

            notice.Request = new HoptoadRequest(HttpContext.Current.Request.Url, assembly.CodeBase)
            {
               Params = BuildParams().ToArray(),
               Session = BuildSession().ToArray(),
               CgiData = BuildCgiData().ToArray(),
            };
         }

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


      private static IEnumerable<HoptoadTraceLine> BuildBacktrace(Exception exception)
      {
         var stackTrace = new StackTrace(exception);
         StackFrame[] frames = stackTrace.GetFrames();

         if (frames == null || frames.Length == 0)
         {
            // Hoptoad requires that at least one line is present in the XML.
            yield return new HoptoadTraceLine("none", 0);
            yield break;
         }

         foreach (StackFrame frame in frames)
         {
            MethodBase method = frame.GetMethod();

            int lineNumber = frame.GetFileLineNumber();

            if (lineNumber == 0)
               lineNumber = frame.GetILOffset();

            string file = frame.GetFileName();

            if (string.IsNullOrEmpty(file))
               file = method.ReflectedType.FullName;

            yield return new HoptoadTraceLine(file, lineNumber)
            {
               Method = method.Name
            };
         }
      }


      private static IEnumerable<HoptoadVar> BuildCgiData()
      {
         return from key in HttpContext.Current.Request.ServerVariables.AllKeys
                let value = HttpContext.Current.Request.ServerVariables[key]
                select new HoptoadVar(key, value);
      }


      private static IEnumerable<HoptoadVar> BuildParams()
      {
         return from key in HttpContext.Current.Request.Params.AllKeys
                let value = HttpContext.Current.Request.Params[key]
                select new HoptoadVar(key, value);
      }


      private static IEnumerable<HoptoadVar> BuildSession()
      {
         return from key in HttpContext.Current.Session.Keys.Cast<string>()
                let v = HttpContext.Current.Session[key]
                let value = v != null ? v.ToString() : null
                select new HoptoadVar(key, value);
      }
   }
}