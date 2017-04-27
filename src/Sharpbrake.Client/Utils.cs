using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using Sharpbrake.Client.Model;

namespace Sharpbrake.Client
{
    public class Utils
    {
        /// <summary>
        /// Parses string with comma-separated values to the list of parameters.
        /// </summary>
        public static IList<string> ParseParameter(string value)
        {
            return string.IsNullOrEmpty(value)
                ? null
                : value.Replace(" ", "").Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Builds Uri to Airbrake endpoint.
        /// </summary>
        public static string GetRequestUri(string projectId, string projectKey, string host = null)
        {
            if (string.IsNullOrEmpty(projectId))
                throw new ArgumentNullException("projectId");

            if (string.IsNullOrEmpty(projectKey))
                throw new ArgumentNullException("projectKey");

            if (string.IsNullOrEmpty(host))
                host = "https://airbrake.io";

            return string.Format("{0}/api/v3/projects/{1}/notices?key={2}", host.Trim('/'), projectId, projectKey);
        }

        /// <summary>
        /// Configures the network proxy for making web requests.
        /// </summary>
        public static IWebProxy ConfigureProxy(string uri, string username = null, string password = null)
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var proxy = new WebProxy(new Uri(uri), true);

            if (!string.IsNullOrEmpty(username) &&
                !string.IsNullOrEmpty(password))
            {
                proxy.Credentials = new NetworkCredential(username, password);
            }

            return proxy;
        }

        /// <summary>
        /// Checks if environment should be ignored.
        /// </summary>
        public static bool IsIgnoredEnvironment(string environment, IList<string> ignoredEnvironments)
        {
            return ignoredEnvironments != null && ignoredEnvironments.Any(env =>
                string.Equals(env, environment, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns filtered dictionary of parameters based on black and white lists.
        /// </summary>
        public static IDictionary<string, string> FilterParameters(IDictionary<string, string> parameters,
            IList<Regex> blackList, IList<Regex> whiteList)
        {
            if (parameters == null) return null;

            var outputParameters = new Dictionary<string, string>();

            var blackListIsNotEmpty = blackList != null && blackList.Count > 0;
            var whiteListIsNotEmpty = whiteList != null && whiteList.Count > 0;

            foreach (var key in parameters.Keys)
            {
                var value = parameters[key];
                if ( // If blacklist is not empty and parameters for filtering exist
                    blackListIsNotEmpty && IndexOfRegex(blackList, key) != -1 ||
                    // or whitelist is not empty and has an approved parameters
                    whiteListIsNotEmpty && IndexOfRegex(whiteList, key) == -1)
                {
                    // value will be filtered.
                    // If white list is not empty - all other parameters will be filtered.
                    value = "[Filtered]";
                }

                outputParameters.Add(key, value);
            }

            return outputParameters;
        }

        /// <summary>
        /// Returns a dictionary of parameters with truncated values.
        /// </summary>
        public static IDictionary<string, string> TruncateParameters(IDictionary<string, string> parameters, int stringLimit)
        {
            if (parameters == null) return null;

            var outputParameters = new Dictionary<string, string>();

            foreach (var key in parameters.Keys)
            {
                var value = parameters[key];

                if (!string.IsNullOrEmpty(value) && value.Length > stringLimit)
                    value = value.Substring(0, stringLimit) + "...";

                outputParameters.Add(key, value);
            }

            return outputParameters;
        }

        /// <summary>
        /// Gets list of <see cref="Frame"/> (backtrace) for exception.
        /// </summary>
        public static IList<Frame> GetBacktrace(Exception exception)
        {
            var backtrace = new List<Frame>();

            // Airbrake requires at least one frame to be present in notice, so
            // in case of error or any other issue when stack frames become unavailable
            // this "blank" frame will be used
            var blankFrame = new Frame {File = "none", Column = 0, Line = 0};

            // It seems that obtaining stack frames may be dangerous operation and may even
            // throw "NotImplementedException" exception under some platforms.
            // There is a big thread on that question here https://github.com/dotnet/corefx/issues/1420
            // and https://github.com/dotnet/corefx/issues/1797
            try
            {
                var frames = new StackTrace(exception, true).GetFrames();
                if (frames == null || frames.Length == 0)
                    backtrace.Add(blankFrame);
                else
                {
                    foreach (var frame in frames)
                    {
                        var method = frame.GetMethod();

                        // Gets file line number or zero if line number can't be determined.
                        var lineNumber = frame.GetFileLineNumber();

                        // Gets file column number or zero if column number can't be determined.
                        var columnNumber = frame.GetFileColumnNumber();

                        // No line number found in method, using IL offset instead.
                        if (lineNumber == 0)
                            lineNumber = frame.GetILOffset();

                        var function = method.Name;

                        var file = frame.GetFileName();

                        if (method.DeclaringType != null && string.IsNullOrEmpty(file))
                            file = method.DeclaringType.FullName;

                        backtrace.Add(new Frame
                        {
                            File = file,
                            Line = lineNumber,
                            Column = columnNumber,
                            Function = function
                        });
                    }
                }
            }
            catch (Exception)
            {
                backtrace.Add(blankFrame);
            }

            return backtrace;
        }

        /// <summary>
        /// Applies filters to notice.
        /// </summary>
        /// <remarks>
        /// Filter is a lambda function that accepts and returns <see cref="Notice"/> object.
        /// Notice is already pre-populated with errors, context and params.
        /// These parameters can be modified in lambda, if null is returned Notice is filtered out.
        /// </remarks>
        public static Notice ApplyFilters(Notice notice, IList<Func<Notice, Notice>> filters)
        {
            if (filters == null || filters.Count == 0)
                return notice;

            return filters.Aggregate(notice, (current, filter) => filter(current));
        }

        /// <summary>
        /// Gets list of compiled <see cref="Regex"/> objects from list of string patterns.
        /// </summary>
        public static IList<Regex> CompileRegex(IList<string> stringPatterns)
        {
            return stringPatterns == null
                ? null
                : stringPatterns.Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase)).ToList();
        }

        private static int IndexOfRegex(IList<Regex> regexList, string key)
        {
            for (var i = 0; i < regexList.Count; i++)
                if (regexList[i].IsMatch(key))
                    return i;

            return -1;
        }
    }
}
