using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sharpbrake.Client.Impl
{
    /// <summary>
    /// Implementation of <see cref="ILogger"/> for logging into file.
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string logFile;
        private static object locker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogger"/> class.
        /// </summary>
        /// <param name="logFilePath">Path to the log file.</param>
        public FileLogger(string logFilePath)
        {
            logFile = Path.IsPathRooted(logFilePath)
                ? logFilePath
                : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location ?? string.Empty), logFilePath);
        }

        /// <summary>
        /// Gets the path to the log file.
        /// </summary>
        public string LogFile
        {
            get { return logFile; }
        }

        /// <summary>
        /// Logs response from the Airbrake endpoint.
        /// </summary>
        public void Log(AirbrakeResponse response)
        {
            if (response == null)
                return;

            using (var file = new FileStream(logFile, FileMode.Append, FileAccess.Write))
            {
                lock (locker)
                {
                    using (var writer = new StreamWriter(file, Encoding.UTF8))
                        writer.WriteLine("[{0}]\t{1}\t{2}\t{3}", DateTime.Now, response.Status, response.Id, response.Url);
                }
            }
        }

        /// <summary>
        /// Logs exception occurred during call to the Airbrake endpoint.
        /// </summary>
        public void Log(Exception exception)
        {
            if (exception == null)
                return;

            using (var file = new FileStream(logFile, FileMode.Append, FileAccess.Write))
            {
                lock (locker)
                {
                    using (var writer = new StreamWriter(file, Encoding.UTF8))
                        writer.WriteLine("[{0}]\t{1}\t{2}", DateTime.Now, exception.GetType().FullName, exception.Message);
                }
            }
        }
    }
}
