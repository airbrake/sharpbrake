using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
#if NET35 || NET45
using System.Reflection;
#endif

namespace Sharpbrake.Client.IntegrationTests
{
    /// <summary>
    /// Runs test http server to mock real Airbrake endpoint.
    /// </summary>
    public class HttpServer : IDisposable
    {
        public const string Host = "http://127.0.0.1:8001";

        static HttpServer()
        {
            var schemaFilePath =
#if NET35 || NET45
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location ?? string.Empty), "notice_schema.json");
#else
                Path.Combine(AppContext.BaseDirectory, "notice_schema.json");
#endif
            noticeSchema = JSchema.Parse(File.ReadAllText(schemaFilePath));
        }

        public HttpServer()
        {
            var uriBuilder = new UriBuilder(Host);
            var listener = new TcpListener(IPAddress.Parse(uriBuilder.Host), uriBuilder.Port);

            new Thread(() =>
            {
                listener.Start();
                while (!ServerStopRequested)
                {
#if NET35
                    listener.BeginAcceptTcpClient(result =>
                    {
                        var client = listener.EndAcceptTcpClient(result);
                        new Thread(() => HandleClient(client)).Start();
                        serverWaitHandle.Set();
                    }, listener);
#else
                    listener.AcceptTcpClientAsync().ContinueWith(task =>
                    {
                        new Thread(() => HandleClient(task.Result)).Start();
                        serverWaitHandle.Set();
                    });
#endif
                    serverWaitHandle.WaitOne();
                }
            }).Start();
        }

        public void Dispose()
        {
            lock (locker) serverStopRequested = true;
            serverWaitHandle.Set();
        }

#region Server maintenance vars

        private readonly EventWaitHandle serverWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private readonly object locker = new object();
        private bool serverStopRequested;

        private bool ServerStopRequested
        {
            get
            {
                lock (locker) return serverStopRequested;
            }
        }

#endregion

        private static readonly JSchema noticeSchema;

        /// <summary>
        /// Parses request from client and prepares response.
        /// </summary>
        private static void HandleClient(TcpClient client)
        {
            using (client)
            using (var networkStream = client.GetStream())
            {
                // process request from client
                var requestBuilder = new StringBuilder();
                var data = new byte[1024];

                var bytesRead = 0;
                while (networkStream.DataAvailable && (bytesRead = networkStream.Read(data, 0, data.Length)) > 0)
                    requestBuilder.Append(Encoding.UTF8.GetString(data, 0, bytesRead));

                // read "Content-Length" header
                string contentLengthHeader;
                using (var reader = new StringReader(requestBuilder.ToString()))
                    while (!string.IsNullOrEmpty(contentLengthHeader = reader.ReadLine()))
                        if (contentLengthHeader.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                            break;

                if (!string.IsNullOrEmpty(contentLengthHeader))
                {
                    var bodyLength = int.Parse(contentLengthHeader.Replace("Content-Length:", string.Empty).Trim());
                    if (bodyLength > 0 && bytesRead < bodyLength)
                    {
                        var bodyBytesRead = 0;
                        do
                        {
                            bytesRead = networkStream.Read(data, 0, data.Length);
                            bodyBytesRead += bytesRead;
                            requestBuilder.Append(Encoding.UTF8.GetString(data, 0, bytesRead));

                        } while (bodyBytesRead < bodyLength);
                    }
                }

                string errorMessage;
                ValidateRequest(requestBuilder.ToString(), out errorMessage);
                PrepareResponse(networkStream, errorMessage);
            }
        }

        /// <summary>
        /// Validates request.
        /// </summary>
        /// <remarks>
        /// Requirements to check:
        /// 1. Method should be "POST".
        /// 2. Post URL should be of form "/api/v3/projects/PROJECT_ID/notices?key=PROJECT_KEY".
        /// 3. Content-Type header should be "application/json".
        /// 4. Request body should pass notice schema checks.
        /// </remarks>
        private static void ValidateRequest(string requestContent, out string errorMessage)
        {
            using (var reader = new StringReader(requestContent))
            {
                // first line should be the "Request-Line"
                var line = reader.ReadLine();
                if (!IsValidRequestLine(line, out errorMessage))
                    return;

                var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                while ((line = reader.ReadLine()) != null)
                {
                    // request body starts after double CRLF (https://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html)
                    if (string.IsNullOrEmpty(line))
                    {
                        var requestBody = reader.ReadToEnd();

                        if (string.IsNullOrEmpty(requestBody))
                        {
                            errorMessage = "Empty request body.";
                            return;
                        }

                        // check if body conforms to schema expected by Airbrake endpoint
                        IList<string> errors;
                        if (!JObject.Parse(requestBody).IsValid(noticeSchema, out errors))
                        {
                            errorMessage = string.Join(Environment.NewLine, errors.ToArray());
                            return;
                        }
                    }

                    // remove whitespaces and split by colon to get header name and value
                    var header = new string(line.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray()).Split(':');
                    headers.Add(header[0], header.Length > 1 ? header[1] : null);
                }

                if (!headers.ContainsKey("Content-Type") ||
                    !headers["Content-Type"].Equals("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "Invalid Content-Type header.";
                }
            }
        }

        /// <summary>
        /// Validates whether method is 'POST' and URL is of expected format.
        /// </summary>
        private static bool IsValidRequestLine(string requestLine, out string errorMessage)
        {
            errorMessage = null;

            // Request-Line = Method <SP> Request-URI <SP> HTTP-Version <CRLF>
            var tokens = requestLine.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != 3)
            {
                errorMessage = "Invalid Request-Line format.";
                return false;
            }

            if (!tokens[0].Equals("POST"))
            {
                errorMessage = "Invalid HTTP method.";
                return false;
            }

            // URL should be kind of "/api/v3/projects/PROJECT_ID/notices?key=PROJECT_KEY"
            const string expectedUrlFormat = @"\/api\/v3\/projects\/(\w+)\/notices\?key=(\w+)";
            if (!Regex.Match(tokens[1], expectedUrlFormat, RegexOptions.Compiled | RegexOptions.IgnoreCase).Success)
            {
                errorMessage = "Invalid URL format.";
                return false;
            }

            return true;
        }

        private static void PrepareResponse(Stream networkStream, string errorMessage)
        {
            // lines should be separated by CRLF https://www.w3.org/Protocols/rfc2616/rfc2616-sec2.html#sec2.2
            const string CRLF = "\r\n";

            var responseBuilder = new StringBuilder();
            string responseContent;
            HttpStatusCode statusCode;

            if (string.IsNullOrEmpty(errorMessage))
            {
                var id = Guid.NewGuid().ToString();
                var url = string.Format(CultureInfo.InvariantCulture, "{0}/locate/{1}/", Host, id);
                responseContent = string.Format("{{\"id\": \"{0}\", \"url\": \"{1}\"}}", id, url);
                statusCode = HttpStatusCode.Created;
            }
            else
            {
                responseContent = string.Format("{{\"error\": \"{0}\"}}", errorMessage);
                statusCode = HttpStatusCode.BadRequest;
            }

            responseBuilder.AppendFormat("HTTP/1.1 {0} {1}{2}", (int)statusCode, statusCode, CRLF);
            responseBuilder.AppendFormat("Content-Type: application/json", CRLF);
            responseBuilder.AppendFormat("Content-Length: {0}{1}", Encoding.UTF8.GetByteCount(responseContent), CRLF);
            responseBuilder.Append(CRLF);
            responseBuilder.Append(responseContent);

            var responseData = Encoding.UTF8.GetBytes(responseBuilder.ToString());
            networkStream.Write(responseData, 0, responseData.Length);
        }
    }
}
