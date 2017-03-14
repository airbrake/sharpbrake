using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Tools.DotNetCore.Pack;
using Cake.Common.Tools.DotNetCore.Restore;
using Cake.Common.Tools.DotNetCore.Test;
using Cake.Core;
using Cake.Core.IO;
using Path = System.IO.Path;

namespace Sharpbrake.Build
{
    /// <summary>
    /// Helpers around list of projects.
    /// </summary>
    public class Projects : List<Project>
    {
        private readonly ICakeContext context;
        private readonly Options options;

        public Projects(ICakeContext context, Options options)
        {
            this.context = context;
            this.options = options;
        }

        /// <summary>
        /// Patches project version info in AssemblyInfo.cs and project.json files.
        /// </summary>
        public void PatchVersionInfo(Func<Project, bool> projectsToUpdateVersion, Func<Project, bool> projectsToUpdateClientDependency)
        {
            // update version in "AssemblyInfo.cs" and "project.json" files
            ForEach(p =>
            {
                if (!projectsToUpdateVersion(p)) return;
                UpdateAssemblyInfoFile(Path.Combine(p.AbsolutePath, "Properties", "AssemblyInfo.cs"));
                UpdateProjectJsonFile(Path.Combine(p.AbsolutePath, "project.json"));
            });

            // update version of Sharpbrake.Client in projects that depend on it
            ForEach(p =>
            {
                if (!projectsToUpdateClientDependency(p)) return;
                UpdateDependenciesInProjectJsonFile(Path.Combine(p.AbsolutePath, "project.json"));
            });
        }

        /// <summary>
        /// Restores project packages.
        /// </summary>
        public void RestorePackages()
        {
            ForEach(p =>
            {
                context.DotNetCoreRestore(Path.Combine(p.AbsolutePath, "project.json"),
                    new DotNetCoreRestoreSettings
                    {
                        Verbosity = DotNetCoreRestoreVerbosity.Warning
                    });
            });
        }

        /// <summary>
        /// Builds projects.
        /// </summary>
        public void Build(string configuration)
        {
            ForEach(p =>
            {
                context.DotNetCoreBuild(Path.Combine(p.AbsolutePath, "project.json"),
                    new DotNetCoreBuildSettings
                    {
                        Configuration = configuration
                    });
            });
        }

        /// <summary>
        /// Gets list of test actions.
        /// <remarks>
        /// Test action, when invoked, runs tests for "project - framework" pair.
        /// If "Sharpbrake.Client.Tests" supports "netcoreapp1.0", "net45" and "net35"
        /// then three test actions are produced.
        /// </remarks>
        /// </summary>
        public List<Action<ICakeContext>> GetTestActions(Func<Project, bool> predicate, string configuration)
        {
            var testActions = new List<Action<ICakeContext>>();

            // dotnet cli (core) doesn't support .NET 4.5 and 3.5 so need to use
            // xunit console runner for testing under these frameworks
            var xunitConsole = context.GetFiles("tools/xunit.runner.console/*/tools/xunit.console.exe")
                .OrderByDescending(file => file.FullPath)
                .FirstOrDefault();

            string runner = null;
            string runnerOptions = null;

            if (xunitConsole != null)
            {
                if (options.IsRunningOnWindows)
                    runner = xunitConsole.FullPath;
                else if (options.IsMonoPresent)
                {
                    // xunit console can be run using Mono on non-Windows system
                    runner = "mono";
                    runnerOptions = xunitConsole.FullPath;
                }
            }

            ForEach(p =>
            {
                if (!p.IsTestProject || !predicate(p)) return;

                var projectTitle = p.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();

                foreach (var framework in p.SupportedFrameworks)
                {
                    var testResultsFile = options.TestResultsDir + "/" + projectTitle + "." + framework + ".xml";

                    if (framework.StartsWith("netcoreapp", StringComparison.OrdinalIgnoreCase))
                    {
                        var projectJsonFile = Path.Combine(p.AbsolutePath, "project.json");

                        testActions.Add(tool =>
                        {
                            tool.DotNetCoreTest(projectJsonFile, new DotNetCoreTestSettings
                            {
                                Framework = framework,
                                Configuration = configuration,
                                NoBuild = true,
                                Verbose = false,
                                ArgumentCustomization = arg => arg.Append("-xml").Append(testResultsFile)
                            });
                        });
                    }
                    else if (framework.Equals("net45", StringComparison.OrdinalIgnoreCase) ||
                             framework.Equals("net35", StringComparison.OrdinalIgnoreCase))
                    {
                        if (runner == null) continue;

                        // ".../Sharpbrake.Client.Tests/bin/Debug/net45/win7-x64/Sharpbrake.Client.Tests.dll"
                        var testFile = context.GetFiles(p.Path + "/bin/" + configuration + "/" + framework + "/*/" + projectTitle + ".dll").First().FullPath;

                        var args = new StringBuilder();

                        if (!string.IsNullOrEmpty(runnerOptions))
                            args.Append(runnerOptions).Append(" ");

                        args.Append(testFile).Append(" -xml ").Append(testResultsFile).Append(" -nologo -noshadow");

                        testActions.Add(tool =>
                        {
                            using (var process = tool.StartAndReturnProcess(runner, new ProcessSettings { Arguments = args.ToString() }))
                            {
                                process.WaitForExit();
                                if (process.GetExitCode() != 0)
                                    throw new Exception("Tests for " + projectTitle + " (" + framework + ") have failed!");
                            }
                        });
                    }
                }
            });

            return testActions;
        }

        /// <summary>
        /// Produces NuGet packages.
        /// </summary>
        public void Pack(Func<Project, bool> predicate, string configuration)
        {
            ForEach(p =>
            {
                if (!predicate(p)) return;

                context.DotNetCorePack(Path.Combine(p.AbsolutePath, "project.json"),
                    new DotNetCorePackSettings
                    {
                        Configuration = configuration,
                        OutputDirectory = options.OutputPackagesDir,
                        NoBuild = true,
                        Verbose = false,
                        VersionSuffix = options.NuGetVersionSuffix
                    });
            });
        }

        /// <summary>
        /// Updates AssemblyVersion, AssemblyFileVersion and AssemblyInformationalVersion tags
        /// in AssemblyInfo.cs file.
        /// </summary>
        /// <param name="path">Path to AssemblyInfo.cs file.</param>
        private void UpdateAssemblyInfoFile(string path)
        {
            UpdateFileContent(path, line =>
            {
                // [assembly: AssemblyVersion("0.1.0.0")]
                if (line.IndexOf("AssemblyVersion", StringComparison.OrdinalIgnoreCase) > 0)
                    return ReplaceQuotedValue(line, options.AssemblySemVer);

                // [assembly: AssemblyFileVersion("0.1.0.0")]
                if (line.IndexOf("AssemblyFileVersion", StringComparison.OrdinalIgnoreCase) > 0)
                    return ReplaceQuotedValue(line, options.MajorMinorPatch + ".0");

                // [assembly: AssemblyInformationalVersion("0.1.0.0")]
                if (line.IndexOf("AssemblyInformationalVersion", StringComparison.OrdinalIgnoreCase) > 0)
                    return ReplaceQuotedValue(line, options.InformationalVersion);

                return line;
            });
        }

        /// <summary>
        /// Updates "version" tag in project.json file.
        /// </summary>
        /// <param name="path">Path to project.json file.</param>
        private void UpdateProjectJsonFile(string path)
        {
            UpdateFileContent(path, line =>
            {
                if (line.IndexOf("\"version\"", StringComparison.OrdinalIgnoreCase) < 0)
                    return line;

                // "version": "0.1.2-*",
                var colonPos = line.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                return line.Substring(0, colonPos) + ReplaceQuotedValue(line.Substring(colonPos), options.MajorMinorPatch + "-*");
            });
        }

        /// <summary>
        /// Updates version number for "Sharpbrake.Client" dependency to the current one.
        /// </summary>
        /// <param name="path">Path to project.json file.</param>
        private void UpdateDependenciesInProjectJsonFile(string path)
        {
            var dependenciesContext = -1;

            UpdateFileContent(path, line =>
            {
                // is "dependencies" node reached?
                if (line.IndexOf("\"dependencies\"", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    dependenciesContext = 0;
                    if (line.IndexOf("{", StringComparison.OrdinalIgnoreCase) >= 0)
                        dependenciesContext += 1;
                }

                if (dependenciesContext < 0)
                    return line;

                // within "dependencies" node
                if (line.IndexOf("\"Sharpbrake.Client\"", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // "Sharpbrake.Client": "0.1.2",
                    var colonPos = line.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                    return line.Substring(0, colonPos) + ReplaceQuotedValue(line.Substring(colonPos), options.MajorMinorPatch);
                }

                if (line.IndexOf("}", StringComparison.OrdinalIgnoreCase) >= 0)
                    dependenciesContext -= 1;

                return line;
            });
        }

        /// <summary>
        /// Updates file content by rewriting lines with processLine logic.
        /// </summary>
        private static void UpdateFileContent(string path, Func<string, string> processLine)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var streamNew = new FileStream(path + "_new", FileMode.Create, FileAccess.Write))
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(streamNew))
                while (!reader.EndOfStream)
                    // by using "WriteLine" method empty line is added to the end of file regardless
                    // whether "original" file ends with empty line or not
                    // TODO: Consider on how this can be fixed [low priority]
                    writer.WriteLine(processLine(reader.ReadLine()));

            File.Delete(path);
            File.Move(path + "_new", path);
        }

        /// <summary>
        /// Replaces value between first two double-quotes with new one.
        /// </summary>
        private static string ReplaceQuotedValue(string str, string newValue)
        {
            // place to start insert from
            var pos = str.IndexOf('"') + 1;
            return new StringBuilder(str)
                .Remove(pos, str.IndexOf('"', pos) - pos)
                .Insert(pos, newValue)
                .ToString();
        }
    }

    /// <summary>
    /// Project details that matter for build.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Relative path to project root.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Absolute path to project root.
        /// </summary>
        public string AbsolutePath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTestProject { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string[] SupportedFrameworks { get; set; }
    }
}
