using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;

namespace Sharpbrake.Build
{
    public static class Coverity
    {
        /// <summary>
        /// Runs coverity scan tool and uploads results to "scan.coverity.com" for analysis.
        /// </summary>
        [CakeMethodAlias]
        public static void RunCoverityScan(this ICakeContext context, Options options, string configuration)
        {
            // 1. Clean-up any artifacts from previous builds
            context.Information("Cleaning up directories...");

            context.CleanDirectories("coverity/Sharpbrake");
            context.CleanDirectories("coverity/**/bin");
            context.CleanDirectories("coverity/**/obj");

            // 2. Restore packages
            var nugetConsole = context.GetFiles("tools/nuget.exe").FirstOrDefault();
            if (nugetConsole == null)
                throw new InvalidOperationException("Could not find nuget.exe in tools folder.");

            using (var process = context.StartAndReturnProcess(nugetConsole.FullPath, new ProcessSettings {
                Arguments = "restore coverity/Sharpbrake.Coverity.sln" }))
            {
                process.WaitForExit();
                if (process.GetExitCode() != 0)
                    throw new Exception("Restoring packages has failed for Sharpbrake.Coverity.sln.");
            }

            // 3. Run Coverity scan process

            // cov-build --dir cov-int <build command>
            // where <build command> = msbuild coverity/Sharpbrake.Coverity.sln /p:Configuration=Release|Debug
            // path to "cov-build.exe" should be in PATH

            // TODO: Check "cov-int/build-log.txt" for build results.
            // From Scan Coverity Build Tool instructions:
            // IMPORTANT - Your build will be rejected if at least 85% units of code are not compiled.
            // * tail cov-int/build-log.txt
            // * The last few lines of cov-int/build-log.txt should look as follows to indicate that at least 85% of the compilation units are compiled successfully to avoid false-positives:
            //   ----------------------------------------------
            //   Compilation units (85%) are ready for analysis
            //   The cov-build utility completed successfully
            //   ----------------------------------------------

            using (var process = context.StartAndReturnProcess("cov-build.exe", new ProcessSettings {
                Arguments = "--dir coverity/Sharpbrake/cov-int msbuild coverity/Sharpbrake.Coverity.sln /p:Configuration=" + configuration }))
            {
                process.WaitForExit();
                if (process.GetExitCode() != 0)
                    throw new Exception("Coverity scan has failed!");
            }

            // 4. Upload coverity scan results to "scan.coverity.com" for analysis
            UploadCoverityScanResults(context, options.SemVer);
        }

        private static void UploadCoverityScanResults(ICakeContext context, string semVersion)
        {
            var token = context.EnvironmentVariable("COVERITY_PROJECT_TOKEN");
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("Could not resolve Coverity project token. Set COVERITY_PROJECT_TOKEN.");

            // email to which Coverity Scan should send a notification about analysis results
            var email = context.EnvironmentVariable("COVERITY_NOTIFICATION_EMAIL");
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("Could not resolve Coverity notification email. Set COVERITY_NOTIFICATION_EMAIL.");

            // 1. Zip "coverity/Sharpbrake" folder with scan results
            context.Zip("coverity/Sharpbrake", "coverity/Sharpbrake.zip");

            // 2. Submit archive to "https://scan.coverity.com/builds?project=sharpbrake" for analysis
            if (context.FileExists("coverity/Sharpbrake.zip"))
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(20);

                    using (var form = new MultipartFormDataContent())
                    using (var tokenField = CreateStringContent("token", token))
                    using (var emailField = CreateStringContent("email", email))
                    using (var versionField = CreateStringContent("version", semVersion))
                    using (var descriptionField = CreateStringContent("description", context.EnvironmentVariable("APPVEYOR_REPO_COMMIT_MESSAGE") ?? "Sharpbrake"))

                    using (var fs = new FileStream("coverity/Sharpbrake.zip", FileMode.Open, FileAccess.Read))
                    using (var fileField = CreateFileContent(fs, "file", "Sharpbrake.zip"))
                    {
                        form.Add(tokenField);
                        form.Add(emailField);
                        form.Add(versionField);
                        form.Add(descriptionField);
                        form.Add(fileField);

                        try
                        {
                            var response = client.PostAsync("https://scan.coverity.com/builds?project=sharpbrake", form);
                            response.Wait();

                            if (response.Result.IsSuccessStatusCode)
                            {
                                context.Information("Results have been submitted.");
                            }
                            else
                                throw new InvalidOperationException(response.Result.ReasonPhrase);
                        }
                        catch (AggregateException ex)
                        {
                            throw ex.InnerException;
                        }
                    }
                }
            }
            else
                throw new Exception("Can't find \"coverity/Sharpbrake.zip\" archive with scan results.");
        }

        private static HttpContent CreateFileContent(Stream stream, string name, string fileName)
        {
            var streamContent = new StreamContent(stream);
            streamContent.Headers.Clear();
            streamContent.Headers.Add("Content-Type", "application/x-zip-compressed");
            streamContent.Headers.Add("Content-Disposition", "form-data; name=\"" + name + "\"; filename=\"" + fileName + "\"");
            return streamContent;
        }

        private static HttpContent CreateStringContent(string name, string value)
        {
            var stringContent = new StringContent(value);
            stringContent.Headers.Clear();
            stringContent.Headers.Add("Content-Disposition", "form-data; name=\"" + name + "\"");
            return stringContent;
        }
    }
}
