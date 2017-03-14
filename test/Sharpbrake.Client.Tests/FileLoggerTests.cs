using System;
using System.IO;
using Sharpbrake.Client.Impl;
using Xunit;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="FileLogger"/> class.
    /// </summary>
    public class FileLoggerTests
    {
        [Fact]
        public void Ctor_ShouldUseCurrentDirectoryIfOnlyFilenameIsProvided()
        {
            var logger = new FileLogger("airbrake.log");

            Assert.True(!string.IsNullOrEmpty(Path.GetDirectoryName(logger.LogFile)));
        }

        [Fact]
        public void Ctor_ShouldUseProvidedFilePathIfPathIsAbsolute()
        {
            var windir = Environment.GetEnvironmentVariable("windir");
            var isWindows = !string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir);

            var absolutePath = isWindows ? "C:\\airbrake_log.txt" : "/Users/app/airbrake_log.txt";
            var logger = new FileLogger(absolutePath);

            Assert.True(logger.LogFile == absolutePath);
        }

        [Fact]
        public void Log_ShouldNotLogResponseIfEmpty()
        {
            var logFile = Guid.NewGuid() + ".log";
            var logger = new FileLogger(logFile);

            AirbrakeResponse response = null;
            logger.Log(response);

            Assert.True(!File.Exists(logger.LogFile));
            File.Delete(logger.LogFile);
        }

        [Fact]
        public void Log_ShouldLogResponseIfNotEmpty()
        {
            var logFile = Guid.NewGuid() + ".log";
            var logger = new FileLogger(logFile);

            var response = new AirbrakeResponse
            {
                Id = "0005488e-8947-223e-90ca-16fec30b6d72",
                Url = "https://airbrake.io/locate/0005488e-8947-223e-90ca-16fec30b6d72",
                Status = RequestStatus.Success
            };
            logger.Log(response);

            Assert.True(File.Exists(logger.LogFile));
            Assert.True(!string.IsNullOrEmpty(File.ReadAllText(logger.LogFile)));
            File.Delete(logger.LogFile);
        }

        [Fact]
        public void Log_ShouldNotLogExceptionIfEmpty()
        {
            var logFile = Guid.NewGuid() + ".log";
            var logger = new FileLogger(logFile);

            AirbrakeResponse response = null;
            logger.Log(response);

            Assert.True(!File.Exists(logger.LogFile));
            File.Delete(logger.LogFile);
        }

        [Fact]
        public void Log_ShouldLogExceptionIfNotEmpty()
        {
            var logFile = Guid.NewGuid() + ".log";
            var logger = new FileLogger(logFile);

            logger.Log(new Exception("Exception message"));

            Assert.True(File.Exists(logger.LogFile));
            Assert.True(!string.IsNullOrEmpty(File.ReadAllText(logger.LogFile)));
            File.Delete(logger.LogFile);
        }
    }
}
