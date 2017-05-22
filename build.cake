#tool nuget:?package=OpenCover&version=4.6.519
#tool nuget:?package=ReportGenerator&version=2.5.8

var target = Context.Argument("target", "Default");

var configuration =
    HasArgument("Configuration") ? Argument<string>("Configuration") :
    EnvironmentVariable("Configuration") != null ? EnvironmentVariable("Configuration") : "Release";

var buildSystem = Context.BuildSystem();
var isLocalBuild = buildSystem.IsLocalBuild;
var isRunningOnAppVeyor = buildSystem.AppVeyor.IsRunningOnAppVeyor;
var isRunningOnWindows = Context.IsRunningOnWindows();

var buildNumber =
    HasArgument("BuildNumber") ? Argument<int>("BuildNumber") :
    isRunningOnAppVeyor ? AppVeyor.Environment.Build.Number :
    EnvironmentVariable("BuildNumber") != null ? int.Parse(EnvironmentVariable("BuildNumber")) : 0;

var artifactsDir = Directory("./artifacts");
var testResultsDir = Directory("./artifacts/test-results");
var nugetDir = System.IO.Path.Combine(artifactsDir, "nuget");

//
// Tasks
//

Task("Info")
    .Does(() =>
{
    Information("Target: {0}", target);
    Information("Configuration: {0}", configuration);
    Information("Build number: {0}", buildNumber);

    var projects = GetFiles("./src/**/*.csproj");
    foreach (var project in projects) {
        Information("{0} version: {1}", project.GetFilenameWithoutExtension(), GetValueFromCsproj(project.FullPath, "Version"));
    }
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
});

Task("Restore-Packages")
    .Does(() =>
{
    DotNetCoreRestore();
});

Task("Build")
    .IsDependentOn("Info")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-Packages")
    .Does(() =>
{
    var projects = GetFiles("./src/**/*.csproj");
    projects.Add(GetFiles("./test/**/*.csproj"));

    foreach(var project in projects)
    {
        DotNetCoreBuild(project.FullPath,
            new DotNetCoreBuildSettings
            {
                Configuration = configuration,
                ArgumentCustomization = args => args.Append("/p:DebugType=full /p:DebugSymbols=True")
            }
        );
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testProject = new FilePath("./test/Sharpbrake.Client.Tests/Sharpbrake.Client.Tests.csproj");
    var workingDirectory = MakeAbsolute(new DirectoryPath("./test/Sharpbrake.Client.Tests")).FullPath;

    var testActions = new List<Action<ICakeContext>>();
    var dotnetCmd = isRunningOnWindows ? "dotnet.exe" : "dotnet";

    testActions.Add(tool => {
        using (var process = tool.StartAndReturnProcess(
            dotnetCmd,
            new ProcessSettings {
                Arguments = "xunit -f netcoreapp1.1 -nobuild -c " + configuration,
                WorkingDirectory = workingDirectory
            }
        ))
        {
            process.WaitForExit();
            if (process.GetExitCode() != 0)
                throw new Exception("Tests for netcoreapp1.1 have failed!");
        }
    });

    testActions.Add(tool => {
        using (var process = tool.StartAndReturnProcess(
            dotnetCmd,
            new ProcessSettings {
                Arguments = "xunit -f net452 -nobuild -noshadow -c " + configuration,
                WorkingDirectory = workingDirectory
            }
        ))
        {
            process.WaitForExit();
            if (process.GetExitCode() != 0)
                throw new Exception("Tests for net452 have failed!");
        }
    });

    EnsureDirectoryExists(testResultsDir);

    // OpenCover works only on Windows
    if (isRunningOnWindows)
    {
        var openCoverXml = MakeAbsolute(testResultsDir.Path.CombineWithFilePath("OpenCover").AppendExtension("xml"));;
        var coverageReportDir = System.IO.Path.Combine(testResultsDir, "report");

        var settings = new OpenCoverSettings
        {
            Register = "user",
            ReturnTargetCodeOffset = 0,
            WorkingDirectory = workingDirectory,
            ArgumentCustomization =
                args =>
                    args.Append(
                        "-skipautoprops -mergebyhash -mergeoutput -oldstyle -hideskipped:All")
        }
        .WithFilter("+[*]* -[xunit.*]* -[*.Tests]*")
        .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
        .ExcludeByFile("*/*Designer.cs;*/*.g.cs;*/*.g.i.cs");

        foreach (var testAction in testActions)
            OpenCover(testAction, openCoverXml, settings);

        // for non-local build coverage is uploaded to codecov.io so no need to generate the report
        if (FileExists(openCoverXml) && isLocalBuild)
        {
            ReportGenerator(openCoverXml, coverageReportDir,
                new ReportGeneratorSettings {
                    ArgumentCustomization = args => args.Append("-reporttypes:html")
                }
            );
        }
    }
    else
    {
        foreach (var testAction in testActions)
            testAction(Context);
    }
});

Task("Create-Packages")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    var revision = buildNumber.ToString("D4");
    foreach (var project in GetFiles("./src/**/*.csproj"))
    {
        DotNetCorePack(
            project.GetDirectory().FullPath,
            new DotNetCorePackSettings()
            {
                Configuration = configuration,
                NoBuild = true,
                OutputDirectory = nugetDir,
                VersionSuffix = revision
            });
    }
});

//
// Targets
//

Task("Default")
    .IsDependentOn("Create-Packages");

//
// Run build
//

RunTarget(target);


// **********************************************
// ***               Utilities                ***
// **********************************************

/// <summary>
/// Reads node's value in the csproj file.
/// </summary>
public static string GetValueFromCsproj(string csproj, string node)
{
    using (var reader = System.Xml.XmlReader.Create(csproj))
    {
        reader.MoveToContent();
        while (reader.Read())
            if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                reader.LocalName.Equals(node, StringComparison.OrdinalIgnoreCase))
                return reader.ReadElementContentAsString();
    }
    return null;
}
