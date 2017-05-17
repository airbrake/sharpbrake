using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
        /// Sets exception data into notice errors list.
        /// </summary>
        public void SetErrorEntries(Exception exception)
        {
            notice.Exception = exception;

            var errors = new List<ErrorEntry>();

            // main exception + no more than 3 inner exceptions (to reduce JSON size)
            while (errors.Count < 4 && exception != null)
            {
                var exceptionType = exception.GetType();
                errors.Add(new ErrorEntry
                {
                    Message = !string.IsNullOrEmpty(exception.Message)
                        ? string.Format("{0}: {1}", exceptionType.Name, exception.Message)
                        : exceptionType.Name,
                    Type = exceptionType.FullName,
                    Backtrace = Utils.GetBacktrace(exception)
                });
                exception = exception.InnerException;
            }

            notice.Errors = errors;

            var error = errors.FirstOrDefault();
            if (error != null && error.Backtrace != null)
            {
                var backtrace = error.Backtrace.FirstOrDefault();
                if (backtrace != null)
                {
                    notice.Context.Action = backtrace.Function;
                    notice.Context.Component = backtrace.File;
                }
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

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(notice, jsonSerializerSettings);
            var level = 0;

            while (json.Length > noticeLengthMax && level < 8)
            {
                // each level reduces the string limit by a half
                var stringLimit = stringLengthMax / (int)Math.Pow(2, level);

                notice.EnvironmentVars = Utils.TruncateParameters(notice.EnvironmentVars, stringLimit);
                notice.Params = Utils.TruncateParameters(notice.Params, stringLimit);
                notice.Session = Utils.TruncateParameters(notice.Session, stringLimit);

                json = JsonConvert.SerializeObject(notice, jsonSerializerSettings);
                level++;
            }

            return json;
        }

        /// <summary>
        /// Gets the instance of <see cref="Notice"/> from JSON string.
        /// </summary>
        public static Notice FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<Notice>(json, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
