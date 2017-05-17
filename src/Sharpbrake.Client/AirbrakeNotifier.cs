using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Sharpbrake.Client.Impl;
using Sharpbrake.Client.Model;
#if NET35
using System.ComponentModel;
#elif NET45
using System.Threading.Tasks;
#elif NETSTANDARD1_4
using System.Runtime.InteropServices;
using System.Threading.Tasks;
#endif

namespace Sharpbrake.Client
{
    /// <summary>
    /// Functionality for notifying Airbrake on exception.
    /// </summary>
    public class AirbrakeNotifier
    {
        private readonly AirbrakeConfig config;
        private readonly ILogger logger;
        private readonly IHttpRequestHandler httpRequestHandler;

        /// <summary>
        /// List of filters for applying to the <see cref="Notice"/> object.
        /// </summary>
        private readonly IList<Func<Notice, Notice>> filters = new List<Func<Notice, Notice>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeNotifier"/> class.
        /// </summary>
        /// <param name="config">The <see cref="AirbrakeConfig"/> instance to use.</param>
        /// <param name="logger">The <see cref="ILogger"/> implementation to use.</param>
        /// <param name="httpRequestHandler">The <see cref="IHttpRequestHandler"/> implementation to use.</param>
        public AirbrakeNotifier(AirbrakeConfig config, ILogger logger = null, IHttpRequestHandler httpRequestHandler = null)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            this.config = config;

            // use default FileLogger if no custom implementation has been provided
            // but config contains non-empty value for "LogFile" property
            if (logger != null)
                this.logger = logger;
            else if (!string.IsNullOrEmpty(config.LogFile))
                this.logger = new FileLogger(config.LogFile);

            // use default provider that returns HttpWebRequest from standard .NET library
            // if custom implementation has not been provided
            this.httpRequestHandler = httpRequestHandler ?? new HttpRequestHandler(config.ProjectId, config.ProjectKey, config.Host);
        }

        /// <summary>
        /// Adds filter to the list of filters for current notifier.
        /// </summary>
        public void AddFilter(Func<Notice, Notice> filter)
        {
            filters.Add(filter);
        }

        /// <summary>
        /// Notifies Airbrake on error in your app and logs response from Airbrake.
        /// </summary>
        /// <remarks>
        /// Call to Airbrake is made asynchronously. Logging is deferred and occurs only if constructor has been
        /// provided with logger implementation or config contains non-empty value for "LogFile" property.
        /// </remarks>
        public void Notify(Exception exception, IHttpContext context = null, Severity severity = Severity.Error)
        {
#if NET35
            if (logger != null)
            {
                NotifyCompleted += (sender, eventArgs) =>
                {
                    if (eventArgs.Error != null)
                        logger.Log(eventArgs.Error);
                    else
                        logger.Log(eventArgs.Result);
                };
            }

            NotifyAsync(exception, context);
#else
            var notifyTask = NotifyAsync(exception, context, severity);
            if (logger != null)
            {
                notifyTask.ContinueWith(response =>
                {
                    if (response.IsFaulted)
                        logger.Log(response.Exception);
                    else
                        logger.Log(response.Result);
                });
            }
#endif
        }

#if NET35
        /// <summary>
        /// Defines event for reporting Notify completion.
        /// </summary>
        public event NotifyCompletedEventHandler NotifyCompleted;

        /// <summary>
        /// Defines function called to complete async Notify operation.
        /// </summary>
        protected virtual void OnNotifyCompleted(NotifyCompletedEventArgs eventArgs)
        {
            var handler = NotifyCompleted;
            if (handler != null)
                handler(this, eventArgs);
        }
#endif

        /// <summary>
        /// Notifies Airbrake on error in your app using asynchronous call.
        /// </summary>
#if NET35
        public void NotifyAsync(Exception exception, IHttpContext context = null, Severity severity = Severity.Error)
        {
            if (string.IsNullOrEmpty(config.ProjectId))
                throw new Exception("Project Id is required");

            if (string.IsNullOrEmpty(config.ProjectKey))
                throw new Exception("Project Key is required");

            try
            {
                if (Utils.IsIgnoredEnvironment(config.Environment, config.IgnoreEnvironments))
                {
                    var response = new AirbrakeResponse {Status = RequestStatus.Ignored};

                    OnNotifyCompleted(new NotifyCompletedEventArgs(response, null, false, null));
                    return;
                }

                var noticeBuilder = new NoticeBuilder();
                noticeBuilder.SetErrorEntries(exception);
                noticeBuilder.SetConfigurationContext(config);
                noticeBuilder.SetSeverity(severity);

                if (context != null)
                    noticeBuilder.SetHttpContext(context, config);

                noticeBuilder.SetEnvironmentContext(Dns.GetHostName(), Environment.OSVersion.VersionString, "C#/NET35");

                var notice = noticeBuilder.ToNotice();

                if (filters.Count > 0)
                    notice = Utils.ApplyFilters(notice, filters);

                if (notice == null)
                {
                    var response = new AirbrakeResponse {Status = RequestStatus.Ignored};

                    OnNotifyCompleted(new NotifyCompletedEventArgs(response, null, false, null));
                    return;
                }

                var request = httpRequestHandler.Get();

                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Method = "POST";

                request.BeginGetRequestStream(requestStreamResult =>
                {
                    try
                    {
                        var req = (IHttpRequest) requestStreamResult.AsyncState;
                        var requestStream = req.EndGetRequestStream(requestStreamResult);

                        using (var requestWriter = new StreamWriter(requestStream))
                            requestWriter.Write(NoticeBuilder.ToJsonString(notice));

                        req.BeginGetResponse(respRes =>
                        {
                            IHttpResponse httpResponse = null;
                            try
                            {
                                var req2 = (IHttpRequest) respRes.AsyncState;
                                httpResponse = req2.EndGetResponse(respRes);

                                using (var respStream = httpResponse.GetResponseStream())
                                using (var responseReader = new StreamReader(respStream))
                                {
                                    var airbrakeResponse = JsonConvert.DeserializeObject<AirbrakeResponse>(responseReader.ReadToEnd());
                                    airbrakeResponse.Status = httpResponse.StatusCode == HttpStatusCode.Created
                                        ? RequestStatus.Success
                                        : RequestStatus.RequestError;

                                    OnNotifyCompleted(new NotifyCompletedEventArgs(airbrakeResponse, null, false, null));
                                }
                            }
                            catch (Exception respException)
                            {
                                OnNotifyCompleted(new NotifyCompletedEventArgs(null, respException, false, null));
                            }
                            finally
                            {
                                var disposableResponse = httpResponse as IDisposable;
                                if (disposableResponse != null)
                                    disposableResponse.Dispose();
                            }
                        }, req);
                    }
                    catch (Exception reqException)
                    {
                        OnNotifyCompleted(new NotifyCompletedEventArgs(null, reqException, false, null));
                    }
                }, request);
            }
            catch (Exception ex)
            {
                OnNotifyCompleted(new NotifyCompletedEventArgs(null, ex, false, null));
            }
        }
#else
        public Task<AirbrakeResponse> NotifyAsync(Exception exception, IHttpContext context = null, Severity severity = Severity.Error)
        {
            if (string.IsNullOrEmpty(config.ProjectId))
                throw new Exception("Project Id is required");

            if (string.IsNullOrEmpty(config.ProjectKey))
                throw new Exception("Project Key is required");

            // Task-based Asynchronous Pattern (https://msdn.microsoft.com/en-us/library/hh873177.aspx)
            var tcs = new TaskCompletionSource<AirbrakeResponse>();
            try
            {
                if (Utils.IsIgnoredEnvironment(config.Environment, config.IgnoreEnvironments))
                {
                    var response = new AirbrakeResponse {Status = RequestStatus.Ignored};
                    tcs.SetResult(response);
                    return tcs.Task;
                }

                var noticeBuilder = new NoticeBuilder();
                noticeBuilder.SetErrorEntries(exception);
                noticeBuilder.SetConfigurationContext(config);
                noticeBuilder.SetSeverity(severity);

                if (context != null)
                    noticeBuilder.SetHttpContext(context, config);

#if NET45
                noticeBuilder.SetEnvironmentContext(Dns.GetHostName(), Environment.OSVersion.VersionString, "C#/NET45");
#elif NETSTANDARD1_4
                // TODO: check https://github.com/dotnet/corefx/issues/4306 for "Environment.MachineName"
                noticeBuilder.SetEnvironmentContext("", RuntimeInformation.OSDescription, "C#/NETCORE");
#endif
                var notice = noticeBuilder.ToNotice();

                if (filters.Count > 0)
                    notice = Utils.ApplyFilters(notice, filters);

                if (notice == null)
                {
                    var response = new AirbrakeResponse {Status = RequestStatus.Ignored};
                    tcs.SetResult(response);
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
                            tcs.SetException(requestStreamTask.Exception.InnerExceptions);
                    }
                    else if (requestStreamTask.IsCanceled)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        using (var requestStream = requestStreamTask.Result)
                        using (var requestWriter = new StreamWriter(requestStream))
                            requestWriter.Write(NoticeBuilder.ToJsonString(notice));

                        request.GetResponseAsync().ContinueWith(responseTask =>
                        {
                            if (responseTask.IsFaulted)
                            {
                                if (responseTask.Exception != null)
                                    tcs.SetException(responseTask.Exception.InnerExceptions);
                            }
                            else if (responseTask.IsCanceled)
                            {
                                tcs.SetCanceled();
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
                                        var airbrakeResponse = JsonConvert.DeserializeObject<AirbrakeResponse>(responseReader.ReadToEnd());
                                        // Note: a success response means that the data has been received and accepted for processing.
                                        // Use the url or id in the response to query the status of an error. This will tell you if the error has been processed,
                                        // or if it has been rejected for reasons including invalid JSON and rate limiting.
                                        // If the request body size exceeds 64KB, the API will reject the notice and return a 413 Request Entity Too Large status.
                                        // TODO: Investigate response in case when request body size exceeds 64KB.
                                        airbrakeResponse.Status = httpResponse.StatusCode == HttpStatusCode.Created
                                            ? RequestStatus.Success
                                            : RequestStatus.RequestError;

                                        tcs.SetResult(airbrakeResponse);
                                    }
                                }
                                finally
                                {
                                    var disposableResponse = httpResponse as IDisposable;
                                    if (disposableResponse != null)
                                        disposableResponse.Dispose();
                                }
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                return tcs.Task;
            }

            return tcs.Task;
        }
#endif
    }

#if NET35
    /// <summary>
    /// Defines delegate for Notify completion event.
    /// </summary>
    public delegate void NotifyCompletedEventHandler(object sender, NotifyCompletedEventArgs eventArgs);

    /// <summary>
    /// Holds the result of async call to Airbrake endpoint.
    /// </summary>
    public class NotifyCompletedEventArgs : AsyncCompletedEventArgs
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="NotifyCompletedEventArgs"/> class.
        /// </summary>
        public NotifyCompletedEventArgs(AirbrakeResponse response, Exception error, bool cancelled, object state)
            : base(error, cancelled, state)
        {
            Result = response;
        }

        /// <summary>
        /// Gets result that was returned from async call to Airbrake endpoint.
        /// </summary>
        public AirbrakeResponse Result { get; }
    }
#endif
}
