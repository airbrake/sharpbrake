using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Sharpbrake.Client.Impl;
using Sharpbrake.Client.Model;
#if NET452
using System.Threading.Tasks;
#elif NETSTANDARD1_4 || NETSTANDARD2_0
using System.Runtime.InteropServices;
using System.Threading.Tasks;
#endif

namespace Sharpbrake.Client
{
    /// <summary>
    /// Functionality for reporting errors to the Airbrake dashboard.
    /// </summary>
    public class AirbrakeNotifier
    {
        private readonly AirbrakeConfig config;
        private readonly IHttpRequestHandler httpRequestHandler;

        /// <summary>
        /// List of filters for applying to the <see cref="Notice"/> object.
        /// </summary>
        private readonly IList<Func<Notice, Notice>> filters = new List<Func<Notice, Notice>>();

        /// <summary>
        /// Logs responses from Airbrake into the file.
        /// </summary>
        private readonly Action<AirbrakeResponse> logResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeNotifier"/> class.
        /// </summary>
        /// <param name="config">The <see cref="AirbrakeConfig"/> instance to use.</param>
        /// <param name="httpRequestHandler">The <see cref="IHttpRequestHandler"/> implementation to use.</param>
        public AirbrakeNotifier(AirbrakeConfig config, IHttpRequestHandler httpRequestHandler = null)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            if (!string.IsNullOrEmpty(config.LogFile))
                logResponse = response => Utils.LogResponse(config.LogFile, response);

            // use default provider that returns HttpWebRequest from standard .NET library
            // if custom implementation has not been provided
            this.httpRequestHandler = httpRequestHandler ?? new HttpRequestHandler(config.ProjectId, config.ProjectKey, config.Host);
        }

        /// <summary>
        /// Adds a filter to the list of filters for the current notifier.
        /// </summary>
        public void AddFilter(Func<Notice, Notice> filter)
        {
            filters.Add(filter);
        }

        /// <summary>
        /// Builds a notice for the exception with <see cref="Severity.Error"/> severity.
        /// </summary>
        /// <param name="exception">Exception to report on.</param>
        public Notice BuildNotice(Exception exception)
        {
            return BuildNotice(Severity.Error, exception, null, null);
        }

        /// <summary>
        /// Builds a notice for the error with <see cref="Severity.Error"/> severity.
        /// </summary>
        /// <param name="message">Message describing the error.</param>
        /// <param name="args">Objects positionally formatted into the error message.</param>
        public Notice BuildNotice(string message, params object[] args)
        {
            return BuildNotice(Severity.Error, null, message, args);
        }

        /// <summary>
        /// Builds a notice for the error with <see cref="Severity.Error"/> severity and associated exception.
        /// </summary>
        /// <param name="exception">Exception associated with the error.</param>
        /// <param name="message">Message describing the error.</param>
        /// <param name="args">Objects positionally formatted into the error message.</param>
        public Notice BuildNotice(Exception exception, string message, params object[] args)
        {
            return BuildNotice(Severity.Error, exception, message, args);
        }

        /// <summary>
        /// Builds a notice for the exception with specified severity.
        /// </summary>
        /// <param name="severity">Severity level of the error.</param>
        /// <param name="exception">Exception to report on.</param>
        public Notice BuildNotice(Severity severity, Exception exception)
        {
            return BuildNotice(severity, exception, null, null);
        }

        /// <summary>
        /// Builds a notice for the error with specified severity.
        /// </summary>
        /// <param name="severity">Severity level of the error.</param>
        /// <param name="message">Message describing the error.</param>
        /// <param name="args">Objects positionally formatted into the error message.</param>
        public Notice BuildNotice(Severity severity, string message, params object[] args)
        {
            return BuildNotice(severity, null, message, args);
        }

        /// <summary>
        /// Builds a notice for the error with specified severity and associated exception.
        /// </summary>
        /// <param name="severity">Severity level of the error.</param>
        /// <param name="exception">Exception associated with the error.</param>
        /// <param name="message">Message describing the error.</param>
        /// <param name="args">Objects positionally formatted into the error message.</param>
        public Notice BuildNotice(Severity severity, Exception exception, string message, params object[] args)
        {
            var log = InternalLogger.CreateInstance();
            var notice = NoticeBuilder.BuildNotice();

            log.Trace("Setting error entries");
            notice.SetErrorEntries(exception,
                Utils.GetMessage(config.FormatProvider, message, args));

            log.Trace("Setting configuration context");
            notice.SetConfigurationContext(config);

            log.Trace("Setting severity to {0}", severity);
            notice.SetSeverity(severity);

            log.Trace("Setting environment context");
#if NET452
            notice.SetEnvironmentContext(Dns.GetHostName(), Environment.OSVersion.VersionString, "C#/NET45");
#elif NETSTANDARD1_4
            // TODO: check https://github.com/dotnet/corefx/issues/4306 for "Environment.MachineName"
            notice.SetEnvironmentContext("", RuntimeInformation.OSDescription, "C#/NETCORE");
#elif NETSTANDARD2_0
            notice.SetEnvironmentContext(Dns.GetHostName(), RuntimeInformation.OSDescription, "C#/NETCORE2");
#endif

            log.Trace("Notice was created");
            return notice;
        }

        /// <summary>
        /// Sets HTTP context properties into the notice.
        /// </summary>
        public Notice SetHttpContext(Notice notice, IHttpContext context)
        {
            notice.SetHttpContext(context, config);
            return notice;
        }

        /// <summary>
        /// Notifies Airbrake on the error using the <see cref="Notice"/> object.
        /// </summary>
        /// <param name="notice">The notice describing the error.</param>
        /// <returns>Task which represents an asynchronous operation to Airbrake.</returns>
        public Task<AirbrakeResponse> NotifyAsync(Notice notice)
        {
            var log = InternalLogger.CreateInstance();

            if (string.IsNullOrEmpty(config.ProjectId))
            {
                log.Trace("Project Id is required");
                throw new Exception("Project Id is required");
            }

            if (string.IsNullOrEmpty(config.ProjectKey))
            {
                log.Trace("Project Key is required");
                throw new Exception("Project Key is required");
            }

            // Task-based Asynchronous Pattern (https://msdn.microsoft.com/en-us/library/hh873177.aspx)
            var tcs = new TaskCompletionSource<AirbrakeResponse>();
            try
            {
                if (notice == null)
                {
                    var response = new AirbrakeResponse {Status = RequestStatus.Ignored};
                    tcs.SetResult(response);
                    log.Trace("Notice is empty");
                    return tcs.Task;
                }

                if (filters.Count > 0)
                {
                    log.Trace("Applying filters");
                    notice = Utils.ApplyFilters(notice, filters);
                }

                if (notice == null)
                {
                    var response = new AirbrakeResponse {Status = RequestStatus.Ignored};
                    tcs.SetResult(response);
                    log.Trace("Ignoring notice because of filters");
                    return tcs.Task;
                }

                if (Utils.IsIgnoredEnvironment(config.Environment, config.IgnoreEnvironments))
                {
                    var response = new AirbrakeResponse {Status = RequestStatus.Ignored};
                    tcs.SetResult(response);
                    log.Trace("Ignoring notice for environment: {0}", config.Environment);
                    return tcs.Task;
                }

                var request = httpRequestHandler.Get();

                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Method = "POST";

                request.GetRequestStreamAsync().ContinueWith(requestStreamTask =>
                {
                    if (requestStreamTask.IsFaulted)
                    {
                        if (requestStreamTask.Exception != null)
                        {
                            var exceptions = requestStreamTask.Exception.InnerExceptions;
                            tcs.SetException(exceptions);
                            log.Trace("Exception occurred in request stream: {0}", exceptions.First().Message);
                        }

                        log.Trace("Request stream is faulted");
                    }
                    else if (requestStreamTask.IsCanceled)
                    {
                        tcs.SetCanceled();
                        log.Trace("Request stream is canceled");
                    }
                    else
                    {
                        using (var requestStream = requestStreamTask.Result)
                        using (var requestWriter = new StreamWriter(requestStream))
                        {
                            log.Trace("Writing to request stream");
                            requestWriter.Write(notice.ToJsonString());
                        }

                        request.GetResponseAsync().ContinueWith(responseTask =>
                        {
                            if (responseTask.IsFaulted)
                            {
                                if (responseTask.Exception != null)
                                {
                                    var exceptions = responseTask.Exception.InnerExceptions;
                                    tcs.SetException(exceptions);
                                    log.Trace("Exception occurred in response task: {0}", exceptions.First().Message);
                                }

                                log.Trace("Response task is faulted");
                            }
                            else if (responseTask.IsCanceled)
                            {
                                tcs.SetCanceled();
                                log.Trace("Response task is canceled");
                            }
                            else
                            {
                                IHttpResponse httpResponse = null;
                                try
                                {
                                    httpResponse = responseTask.Result;

                                    using (var responseStream = httpResponse.GetResponseStream())
                                    using (var responseReader = new StreamReader(responseStream))
                                    {
                                        var serializerSettings = new DataContractJsonSerializerSettings
                                        {
                                            UseSimpleDictionaryFormat = true
                                        };

                                        var serializer = new DataContractJsonSerializer(typeof(AirbrakeResponse), serializerSettings);

                                        AirbrakeResponse airbrakeResponse;
                                        using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(responseReader.ReadToEnd())))
                                        {
                                            memoryStream.Position = 0;
                                            airbrakeResponse = (AirbrakeResponse)serializer.ReadObject(memoryStream);
                                        }

                                        // Note: a success response means that the data has been received and accepted for processing.
                                        // Use the URL or id from the response to query the status of an error. This will tell you if the error has been processed,
                                        // or if it has been rejected for reasons including invalid JSON and rate limiting.
                                        airbrakeResponse.Status = httpResponse.StatusCode == HttpStatusCode.Created
                                            ? RequestStatus.Success
                                            : RequestStatus.RequestError;

                                        tcs.SetResult(airbrakeResponse);
                                        log.Trace("Notice was registered: {0}", airbrakeResponse.Url);

                                        logResponse?.Invoke(airbrakeResponse);
                                    }
                                }
                                finally
                                {
                                    var disposableResponse = httpResponse as IDisposable;
                                    disposableResponse?.Dispose();
                                }
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                log.Trace("Exception occurred when sending notice: {0}", ex.Message);
                return tcs.Task;
            }

            return tcs.Task;
        }
    }
}
