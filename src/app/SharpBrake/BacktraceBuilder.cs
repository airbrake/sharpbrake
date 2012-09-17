using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Common.Logging;
using SharpBrake.Serialization;
namespace SharpBrake
{
    /// <summary>
    /// Builds a Backtrace from an exception
    /// </summary>
    public class BacktraceBuilder : IBuilder<Exception,Backtrace>
    {
        private readonly ILog _log;

        /// <summary>
        /// Constructor with dependencies
        /// </summary>
        /// <param name="log"></param>
        public BacktraceBuilder(ILog log)
        {
            _log = log;
        }

        public Backtrace Build(Exception exception)
        {
            var messageBuilder = new StringBuilder();
            var stackFrames= new List<StackFrame>();

            // Get all the exceptions that caused this to bubble up
            var exceptionStack = GetExceptionStack(exception);
            
            foreach (var exceptionToExamine in exceptionStack)
            {
                // Build the message
                messageBuilder.AppendFormat("{0}: {1} | ", exceptionToExamine.GetType().Name, exceptionToExamine.Message);

                // Get the stack trace for the exception
                var stackTrace = new StackTrace(exceptionToExamine);
                var stackFramesInTrace = stackTrace.GetFrames();
                if (stackFramesInTrace != null)
                {
                    var reversedFrames = stackFramesInTrace.Reverse();
                    stackFrames.AddRange(reversedFrames);
                }
            }

            // Clean up the message
            var message = messageBuilder
                .ToString()
                .Trim()
                .TrimEnd('|')
                .Trim();

            // Convert the stack trace
            var airbrakeStackTrace = GetTraceLines(stackFrames);

            return new Backtrace
                {
                    CatchingMethod = airbrakeStackTrace.CatchingMethod,
                    Message = message,
                    Trace = airbrakeStackTrace.TraceLines
                };
        }
        private class TraceLineResult
        {
            public List<AirbrakeTraceLine> TraceLines { get; set; }

            public MethodBase CatchingMethod { get; set; }
        }

        private TraceLineResult GetTraceLines(IEnumerable<StackFrame> stackFrames)
        {
            var lines = new List<AirbrakeTraceLine>();

            MethodBase method;
            MethodBase catchingMethod = null;

            foreach (StackFrame frame in stackFrames)
            {
                method = frame.GetMethod();

                if(catchingMethod == null)
                    catchingMethod = method;

                int lineNumber = frame.GetFileLineNumber();

                if (lineNumber == 0)
                {
                    _log.Debug(f => f("No line number found in {0}, using IL offset instead.", method));
                    lineNumber = frame.GetILOffset();
                }

                string file = frame.GetFileName();

                if (String.IsNullOrEmpty(file))
                {
                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                    file = method.ReflectedType != null
                               ? method.ReflectedType.FullName
                               : "(unknown)";
                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                }

                var line = new AirbrakeTraceLine(file, lineNumber)
                {
                    Method = method.Name
                };

                lines.Add(line);
            }

            if(!lines.Any())
            {
                lines.Add(new AirbrakeTraceLine("none", 0));
            }

            if (catchingMethod == null)
                catchingMethod = GetCatchingMethod();

            return new TraceLineResult { CatchingMethod = catchingMethod, TraceLines = lines };
        }

        private Queue<Exception> GetExceptionStack(Exception exception)
        {
            var exceptionQueue = new Queue<Exception>();

            var exceptionToExamine = exception;

            do
            {
                exceptionQueue.Enqueue(exceptionToExamine);
                exceptionToExamine = exceptionToExamine.InnerException;

            } while (exceptionToExamine != null);

            return exceptionQueue;
        }

        private MethodBase GetCatchingMethod()
        {
            var assembly = Assembly.GetExecutingAssembly();

            if (assembly.EntryPoint == null)
                assembly = Assembly.GetCallingAssembly();

            if (assembly.EntryPoint == null)
                assembly = Assembly.GetEntryAssembly();

            MethodBase catchingMethod = assembly == null
                                            ? null
                                            : assembly.EntryPoint;

            return catchingMethod;
        }
    }
}