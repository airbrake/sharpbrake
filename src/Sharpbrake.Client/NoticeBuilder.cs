using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Sharpbrake.Client.Model;

namespace Sharpbrake.Client
{
    /// <summary>
    /// Builder for the <see cref="Notice"/> object.
    /// </summary>
    public class NoticeBuilder
    {
        private readonly Notice notice;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoticeBuilder"/> class.
        /// </summary>
        public NoticeBuilder()
        {
            notice = new Notice
            {
                Context = new Context
                {
                    Notifier = new NotifierInfo()
                }
            };
        }

        /// <summary>
        /// Sets error entries into notice errors list.
        /// </summary>
        public void SetErrorEntries(Exception exception, string message)
        {
            notice.Exception = exception;
            notice.Message = message;

            var errors = new List<ErrorEntry>();
            var ex = exception;

            if (ex != null)
            {
                errors.Add(new ErrorEntry
                {
                    Message = !string.IsNullOrEmpty(message) ? message : ex.Message,
                    Type = ex.GetType().FullName,
                    Backtrace = Utils.GetBacktrace(new StackTrace(ex, true))
                });

                ex = ex.InnerException;
            }
            else
            {
                errors.Add(new ErrorEntry
                {
                    Message = message,
#if !NETSTANDARD1_4
                    // skip 2 stack frames (for NotifyAsync and SetErrorEntries methods)
                    Backtrace = Utils.GetBacktrace(new StackTrace(3, true))
#endif
                });
            }

            // to reduce JSON size no more than 3 inner exceptions are processed
            while (ex != null && errors.Count < 4)
            {
                errors.Add(new ErrorEntry
                {
                    Message = ex.Message,
                    Type = ex.GetType().FullName,
                    Backtrace = Utils.GetBacktrace(new StackTrace(ex, true))
                });

                ex = ex.InnerException;
            }

            notice.Errors = errors;

            var error = errors.FirstOrDefault();
            var backtrace = error?.Backtrace?.FirstOrDefault();
            if (backtrace != null)
            {
                notice.Context.Action = backtrace.Function;
                notice.Context.Component = backtrace.File;
            }
        }

        /// <summary>
        /// Sets configuration context into corresponding properties of notice.
        /// </summary>
        public void SetConfigurationContext(AirbrakeConfig config)
        {
            if (config == null)
                return;

            notice.Context.EnvironmentName = config.Environment;
            notice.Context.AppVersion = config.AppVersion;
        }

        /// <summary>
        /// Sets environment context (host, OS and C#/.NET info) into corresponding properties of notice.
        /// </summary>
        public void SetEnvironmentContext(string hostName, string osVersion, string langVersion)
        {
            if (string.IsNullOrEmpty(hostName) && string.IsNullOrEmpty(osVersion) && string.IsNullOrEmpty(langVersion))
                return;

            notice.Context.Hostname = hostName;
            notice.Context.Os = osVersion;
            notice.Context.Language = langVersion;
        }

        /// <summary>
        /// Sets error severity.
        /// </summary>
        public void SetSeverity(Severity severity)
        {
            notice.Context.Severity = severity.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Sets the current Http context properties into corresponding properties of notice.
        /// </summary>
        public void SetHttpContext(IHttpContext httpContext, AirbrakeConfig config)
        {
            if (httpContext == null)
                return;

            notice.HttpContext = httpContext;

            notice.Context.Url = httpContext.Url;
            notice.Context.UserAgent = httpContext.UserAgent;

            if (httpContext.Action != null && httpContext.Component != null)
            {
                notice.Context.Action = httpContext.Action;
                notice.Context.Component = httpContext.Component;
            }

            notice.Context.User = new UserInfo
            {
                Name = httpContext.UserName,
                Id = httpContext.UserId,
                Email = httpContext.UserEmail
            };

            if (config == null)
            {
                notice.Params = httpContext.Parameters;
                notice.EnvironmentVars = httpContext.EnvironmentVars;
                notice.Session = httpContext.Session;
            }
            else
            {
                var blackListRegex = Utils.CompileRegex(config.BlacklistKeys);
                var whiteListRegex = Utils.CompileRegex(config.WhitelistKeys);

                notice.Params = Utils.FilterParameters(httpContext.Parameters, blackListRegex, whiteListRegex);
                notice.EnvironmentVars = Utils.FilterParameters(httpContext.EnvironmentVars, blackListRegex, whiteListRegex);
                notice.Session = Utils.FilterParameters(httpContext.Session, blackListRegex, whiteListRegex);
            }
        }

        /// <summary>
        /// Gets the current instance of <see cref="Notice"/>.
        /// </summary>
        public Notice ToNotice()
        {
            return notice;
        }

        /// <summary>
        /// Gets JSON string for the instance of <see cref="Notice"/>.
        /// </summary>
        /// <remarks>
        /// Notice that exceeds 64 KB is truncated.
        /// </remarks>
        public static string ToJsonString(Notice notice)
        {
            const int noticeLengthMax = 64000;
            const int stringLengthMax = 1024;

            var serializerSettings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true
            };

            var serializer = new DataContractJsonSerializer(typeof(Notice), serializerSettings);

            string json;
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, notice);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                    json = reader.ReadToEnd();
            }

            var level = 0;
            while (json.Length > noticeLengthMax && level < 8)
            {
                // each level reduces the string limit by a half
                var stringLimit = stringLengthMax / (int) Math.Pow(2, level);

                notice.EnvironmentVars = Utils.TruncateParameters(notice.EnvironmentVars, stringLimit);
                notice.Params = Utils.TruncateParameters(notice.Params, stringLimit);
                notice.Session = Utils.TruncateParameters(notice.Session, stringLimit);

                using (var memoryStream = new MemoryStream())
                {
                    serializer.WriteObject(memoryStream, notice);
                    memoryStream.Position = 0;

                    using (var reader = new StreamReader(memoryStream))
                        json = reader.ReadToEnd();
                }

                level++;
            }

            return json;
        }

        /// <summary>
        /// Gets the instance of <see cref="Notice"/> from JSON string.
        /// </summary>
        public static Notice FromJsonString(string json)
        {
            var serializerSettings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true
            };

            var serializer = new DataContractJsonSerializer(typeof(Notice), serializerSettings);

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                memoryStream.Position = 0;
                return serializer.ReadObject(memoryStream) as Notice;
            }
        }
    }
}
