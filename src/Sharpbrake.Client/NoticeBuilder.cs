using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sharpbrake.Client.Model;

namespace Sharpbrake.Client
{
    /// <summary>
    /// Builder for the <see cref="Notice"/> object.
    /// </summary>
    public class NoticeBuilder
    {
        private readonly Notice notice = new Notice();

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
        }

        /// <summary>
        /// Sets environment context (host, OS and C#/.NET info) into corresponding properties of notice.
        /// </summary>
        public void SetEnvironmentContext(string hostName, string osVersion, string langVersion)
        {
            if (string.IsNullOrEmpty(hostName) && string.IsNullOrEmpty(osVersion) && string.IsNullOrEmpty(langVersion))
                return;

            if (notice.Context == null)
                notice.Context = new Context();

            notice.Context.Hostname = hostName;
            notice.Context.Os = osVersion;
            notice.Context.Language = langVersion;
        }

        /// <summary>
        /// Sets the current Http context properties into corresponding properties of notice.
        /// </summary>
        public void SetHttpContext(IHttpContext httpContext, AirbrakeConfig config)
        {
            if (httpContext == null)
                return;

            notice.HttpContext = httpContext;

            if (notice.Context == null)
                notice.Context = new Context();

            notice.Context.Url = httpContext.Url;
            notice.Context.UserAgent = httpContext.UserAgent;

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
        /// Gets JSON string for the current instance of <see cref="Notice"/>.
        /// </summary>
        public static string ToJsonString(Notice notice)
        {
            return JsonConvert.SerializeObject(notice, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
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
