using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.GitVersion;
using Cake.Common.Tools.NuGet;
using Cake.Common.Tools.NuGet.Push;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using Path = System.IO.Path;

namespace Sharpbrake.Build
{
    /// <summary>
    /// Sharpbrake extensions for Cake.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Setups list of projects that should be considered by build.
        /// </summary>
        [CakeMethodAlias]
        public static Projects SetProjects(this ICakeContext context, Options options, params Project[] projects)
        {
            var prs = new Projects(context, options);

            foreach (var project in projects)
            {
                project.AbsolutePath = context.MakeAbsolute(new FilePath(project.Path)).FullPath;
                prs.Add(project);
            }

            return prs;
        }

        /// <summary>
        /// Gets options that customize or provide info for build.
        /// </summary>
        [CakeMethodAlias]
        public static Options GetBuildOptions(this ICakeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var options = new Options();
            var buildSystem = context.BuildSystem();

            // Platform
            options.IsLocalBuild = buildSystem.IsLocalBuild;
            options.IsMonoPresent = IsMonoOnPath();
            options.IsRunningOnUnix = context.IsRunningOnUnix();
            options.IsRunningOnWindows = context.IsRunningOnWindows();
            options.IsRunningOnAppVeyor = buildSystem.AppVeyor.IsRunningOnAppVeyor;

            // Repository
            options.IsPullRequest = buildSystem.AppVeyor.Environment.PullRequest.IsPullRequest;
            options.IsMainBranch = StringComparer.OrdinalIgnoreCase.Equals("master", buildSystem.AppVeyor.Environment.Repository.Branch);
            options.IsTagged = IsBuildTagged(buildSystem);

            // Features on/off
            options.SkipGitVersion = StringComparer.OrdinalIgnoreCase.Equals("true", context.EnvironmentVariable("SKIP_GITVERSION"));
            options.SkipCoverage = StringComparer.OrdinalIgnoreCase.Equals("true", context.EnvironmentVariable("SKIP_COVERAGE"));

            // GitVersion tool is used to calculate semantic version based on repo history
            if (buildSystem.AppVeyor.IsRunningOnAppVeyor)
            {
                // AppVeyor provides GitVersion functionality that can be enabled via appveyor.yml:
                // before_build:
                //   -ps: gitversion $env:APPVEYOR_BUILD_FOLDER /l console /output buildserver /nofetch /b $env:APPVEYOR_REPO_BRANCH
                //
                // GitVersion from build script ("else branch" below) fails on AppVeyor with the next exception:
                // "An unexpected error occurred: LibGit2Sharp.LibGit2SharpException: this remote has never connected".
                // This issue can be tracked down to libgit2 library and its method "git_remote_ls" (https://github.com/libgit2/libgit2/blob/master/src/remote.c#L713).
                // TODO: Investigate why AppVeyor's GitVersion works but our one suffers from that problem while basically under the hood this is the same tool.
                options.AssemblySemVer = context.EnvironmentVariable("GitVersion_AssemblySemVer");
                options.MajorMinorPatch = context.EnvironmentVariable("GitVersion_MajorMinorPatch");
                options.InformationalVersion = context.EnvironmentVariable("GitVersion_InformationalVersion");
                options.FullSemVer = context.EnvironmentVariable("GitVersion_FullSemVer");
                options.SemVer = context.EnvironmentVariable("GitVersion_SemVer");
                options.NuGetVersion = context.EnvironmentVariable("GitVersion_NuGetVersion");
            }
            else
            {
                var version = RunGitVersion(context, options);

                options.AssemblySemVer = version.AssemblySemVer;
                options.MajorMinorPatch = version.MajorMinorPatch;
                options.InformationalVersion = version.InformationalVersion;
                options.FullSemVer = version.FullSemVer;
                options.SemVer = version.SemVer;
                options.NuGetVersion = version.NuGetVersion;
            }

            if (!string.IsNullOrEmpty(options.NuGetVersion))
            {
                var pos = options.NuGetVersion.IndexOf('-');
                if (pos >= 0)
                    options.NuGetVersionSuffix = options.NuGetVersion.Substring(pos + 1);
            }

            // Artifacts
            var artifactsDir = Path.Combine("artifacts", "v" + options.SemVer);
            var testResultsDir = Path.Combine(artifactsDir, "test-results");
            var nugetDir = Path.Combine(artifactsDir, "nuget");

            options.DirectoriesToClean = new[]
            {
                artifactsDir,
                testResultsDir,
                nugetDir
            };

            options.TestResultsDir = testResultsDir;
            options.OpenCoverXml = Path.Combine(testResultsDir, "OpenCover.xml");
            options.CoverageReportDir = Path.Combine(testResultsDir, "report");
            options.OutputPackagesDir = nugetDir;

            return options;
        }

        /// <summary>
        /// Determines whether coverage report should be uploaded to codecov.io.
        /// <remarks>
        /// Report is uploaded when the next criteria are meet:
        /// 1. Coverage scanning is enabled (otherwise we don't have what to upload).
        /// 2. It's not local build (for local build report is generated manually so developer can monitor coverage without any external services).
        /// 3. It's not pull request (in case of PR, secure variables are not "unlocked" (on AppVeyor) so we don't have token for codecov.io).
        /// </remarks>
        /// </summary>
        [CakeMethodAlias]
        public static bool ShouldPublishCoverageReport(this ICakeContext context, Options options)
        {
            return !options.SkipCoverage && !options.IsLocalBuild && !options.IsPullRequest;
        }

        /// <summary>
        /// Uploads coverage report (OpenCover.xml) to codecov.io.
        /// </summary>
        [CakeMethodAlias]
        public static void UploadCoverageReport(this ICakeContext context, string openCoverXml)
        {
            const string url = "https://codecov.io/upload/v2";

            // query parameters: https://github.com/codecov/codecov-bash/blob/master/codecov#L1202
            var queryBuilder = new StringBuilder(url);
            queryBuilder.Append("?package=bash-tbd&service=appveyor");
            queryBuilder.Append("&branch=").Append(context.EnvironmentVariable("APPVEYOR_REPO_BRANCH"));
            queryBuilder.Append("&commit=").Append(context.EnvironmentVariable("APPVEYOR_REPO_COMMIT"));
            queryBuilder.Append("&build=").Append(context.EnvironmentVariable("APPVEYOR_JOB_ID"));
            queryBuilder.Append("&pr=").Append(context.EnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER"));
            queryBuilder.Append("&job=").Append(context.EnvironmentVariable("APPVEYOR_ACCOUNT_NAME"));
            queryBuilder.Append("%2F").Append(context.EnvironmentVariable("APPVEYOR_PROJECT_SLUG"));
            queryBuilder.Append("%2F").Append(context.EnvironmentVariable("APPVEYOR_BUILD_VERSION"));
            queryBuilder.Append("&token=").Append(context.EnvironmentVariable("CODECOV_TOKEN"));

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            using (var fileStream = new FileStream(openCoverXml, FileMode.Open, FileAccess.Read))
            using (var contentStream = new StreamContent(fileStream))
            {
                request.RequestUri = new Uri(queryBuilder.ToString());
                request.Method = HttpMethod.Post;
                request.Headers.Add("Accept", "text/plain");
                request.Content = contentStream;

                var task = client.SendAsync(request).ContinueWith(t =>
                {
                    using (var responseResult = t.Result)
                    {
                        var taskResponse = responseResult.Content.ReadAsStreamAsync().ContinueWith(s =>
                        {
                            using (var responseStream = s.Result)
                            using (var reader = new StreamReader(responseStream))
                                context.Information(reader.ReadToEnd());
                        });

                        taskResponse.Wait();
                    }
                });

                task.Wait();
            }
        }

        /// <summary>
        /// Determines whether packages should be uploaded to MyGet.
        /// <remarks>
        /// Packages are uploaded to MyGet when merging to master.
        /// </remarks>
        /// </summary>
        [CakeMethodAlias]
        public static bool ShouldPublishToMyGet(this ICakeContext context, Options options)
        {
            return !options.IsLocalBuild && !options.IsPullRequest && options.IsMainBranch && !options.IsTagged;
        }

        /// <summary>
        /// Uploads packages to MyGet.
        /// </summary>
        [CakeMethodAlias]
        public static void UploadPackagesToMyGet(this ICakeContext context, string outputPackagesDir)
        {
            var apiUrl = context.EnvironmentVariable("MYGET_API_URL");
            if (string.IsNullOrEmpty(apiUrl))
                throw new InvalidOperationException("Could not resolve MyGet API url");

            var apiKey = context.EnvironmentVariable("MYGET_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Could not resolve MyGet API key");

            foreach (var package in context.GetFiles(outputPackagesDir + "/*.nupkg"))
            {
                // symbols packages are pushed alongside regular ones so no need to push them explicitly
                if (package.FullPath.EndsWith("symbols.nupkg", StringComparison.OrdinalIgnoreCase))
                    continue;

                context.NuGetPush(package.FullPath, new NuGetPushSettings
                {
                    Source = apiUrl,
                    ApiKey = apiKey
                });
            }
        }

        /// <summary>
        /// Determines whether packages should be uploaded to NuGet.
        /// <remarks>
        /// Packages are uploaded to NuGet when master is tagged.
        /// </remarks>
        /// </summary>
        [CakeMethodAlias]
        public static bool ShouldPublishToNuGet(this ICakeContext context, Options options)
        {
            return !options.IsLocalBuild && !options.IsPullRequest && options.IsMainBranch && options.IsTagged;
        }

        /// <summary>
        /// Uploads packages to NuGet.
        /// </summary>
        [CakeMethodAlias]
        public static void UploadPackagesToNuGet(this ICakeContext context, string outputPackagesDir)
        {
            var apiUrl = context.EnvironmentVariable("NUGET_API_URL");
            if (string.IsNullOrEmpty(apiUrl))
                throw new InvalidOperationException("Could not resolve NuGet API url");

            var apiKey = context.EnvironmentVariable("NUGET_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Could not resolve NuGet API key");

            foreach (var package in context.GetFiles(outputPackagesDir + "/*.nupkg"))
            {
                // symbols packages are pushed alongside regular ones so no need to push them explicitly
                if (package.FullPath.EndsWith("symbols.nupkg", StringComparison.OrdinalIgnoreCase))
                    continue;

                context.NuGetPush(package.FullPath, new NuGetPushSettings
                {
                    Source = apiUrl,
                    ApiKey = apiKey
                });
            }
        }

        private static bool IsBuildTagged(BuildSystem buildSystem)
        {
            return buildSystem.AppVeyor.Environment.Repository.Tag.IsTag
                   && !string.IsNullOrWhiteSpace(buildSystem.AppVeyor.Environment.Repository.Tag.Name);
        }

        /// <summary>
        /// Checks whether Mono is present on build machine.
        /// </summary>
        private static bool IsMonoOnPath()
        {
            try
            {
                new Process
                {
                    StartInfo = { FileName = "mono", Arguments = "--version" }
                }
                .Start();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static GitVersion RunGitVersion(ICakeContext context, Options options)
        {
            var version = new GitVersion();

            var gitVersionConsole = context.GetFiles("tools/GitVersion.CommandLine/*/tools/GitVersion.exe")
                .OrderByDescending(file => file.FullPath)
                .FirstOrDefault();

            if (gitVersionConsole == null)
                throw new Exception("Missing GitVersion console in \"tools\" folder.");

            string runner;
            var argumentsBuilder = new StringBuilder();

            if (!options.IsRunningOnWindows && options.IsMonoPresent)
            {
                // TODO: Consider what to do if Mono is not available
                runner = "mono";
                argumentsBuilder.Append("\"");
                argumentsBuilder.Append(gitVersionConsole.FullPath);
                argumentsBuilder.Append("\" ");
            }
            else
                runner = gitVersionConsole.FullPath;

            argumentsBuilder.Append("-nofetch");

            using (var process = context.StartAndReturnProcess(runner, new ProcessSettings {
                Arguments = argumentsBuilder.ToString(),
                RedirectStandardOutput = true }))
            {
                process.WaitForExit();

                var errorCode = process.GetExitCode();
                if (errorCode != 0)
                    throw new Exception("GitVersion has failed with error code " + errorCode);

                var versionType = version.GetType();

                foreach (var line in process.GetStandardOutput())
                {
                    if (!line.Contains(':'))
                        continue;

                    // "SemVer":"0.2.1-build-maintenance.1", => value[0] = "SemVer", value[1] = "0.2.1-build-maintenance.1"
                    var value =
                        line.Split(':')
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(s => s.Trim('"', ' ', ','))
                            .ToList();

                    if (value.Count < 2)
                        continue;

                    var property = versionType.GetProperty(value[0]);
                    if (property != null)
                        property.SetValue(version, Convert.ChangeType(value[1], property.PropertyType));
                }
            }

            return version;
        }
    }
}
