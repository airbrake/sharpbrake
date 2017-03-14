#reference "tools/Sharpbrake.Build.dll"

//
// Setup
//

var target = Context.Argument("target", "Default");
var configuration = Context.Argument("configuration", "Release");

var options = GetBuildOptions();

var projects = SetProjects(options,
    new Project
    {
        Path = "src/Sharpbrake.Client",
        IsTestProject = false,
        SupportedFrameworks = new[] {"netstandard1.4", "net45", "net35"}
    },
    new Project
    {
        Path = "src/Sharpbrake.Http.Module",
        IsTestProject = false,
        SupportedFrameworks = new[] {"net45", "net35"}
    },
    new Project
    {
        Path = "src/Sharpbrake.Http.Middleware",
        IsTestProject = false,
        SupportedFrameworks = new[] {"netstandard1.4", "net451"}
    },
    new Project
    {
        Path = "test/Sharpbrake.Client.Tests",
        IsTestProject = true,
        SupportedFrameworks = new[] {"netcoreapp1.0", "net45", "net35"}
    },
    new Project
    {
        Path = "test/Sharpbrake.Client.IntegrationTests",
        IsTestProject = true,
        SupportedFrameworks = new[] {"netcoreapp1.0", "net45", "net35"}
    }
);

//
// Tasks
//

Task("Info")
    .Does(() =>
{
    Information("Building Sharpbrake of version {0}", options.SemVer);
    Information("Configuration: {0}", configuration);
    Information("Target: {0}", target);
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories(options.DirectoriesToClean);
});

Task("Patch-Version")
    .Does(() =>
{
    // patch the "Sharpbrake.Client" version and update dependent projects
    projects.PatchVersionInfo(
        p => p.Path.Equals("src/Sharpbrake.Client", StringComparison.OrdinalIgnoreCase),
        p => !p.IsTestProject
    );
});

Task("Restore-Packages")
    .Does(() =>
{
    projects.RestorePackages();
});

Task("Build")
    .IsDependentOn("Info")
    .IsDependentOn("Clean")
    .IsDependentOn("Patch-Version")
    .IsDependentOn("Restore-Packages")
    .Does(() =>
{
    projects.Build(configuration);
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    projects.GetTestActions(
        // produce test actions only for unit tests (ie. skip integration tests)
        p => p.Path.IndexOf(".IntegrationTests", StringComparison.OrdinalIgnoreCase) < 0,
        configuration
    ).ForEach(action =>
    {
        if (options.IsRunningOnWindows && !options.SkipCoverage)
        {
            OpenCover(action, options.OpenCoverXml, new OpenCoverSettings
                {
                    Register = "user",
                    ReturnTargetCodeOffset = 0,
                    ArgumentCustomization =
                        args =>
                            args.Append(
                                "-skipautoprops -mergebyhash -mergeoutput -oldStyle -hideskipped:All")
                }
                .WithFilter("+[*]* -[xunit.*]* -[*.Tests]* -[*.IntegrationTests]*")
                .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
                .ExcludeByFile("*/*Designer.cs;*/*.g.cs;*/*.g.i.cs")
            );
        }
        else
            action(Context);
    });

    // in case of non-local build coverage is uploaded to codecov.io so no need to generate report
    if (FileExists(options.OpenCoverXml) && options.IsLocalBuild)
    {
        ReportGenerator(options.OpenCoverXml, options.CoverageReportDir,
            new ReportGeneratorSettings {
                ArgumentCustomization = args => args.Append("-reporttypes:html")
            }
        );
    }
});

Task("Publish-Coverage")
    .IsDependentOn("Run-Unit-Tests")
    .WithCriteria(() => ShouldPublishCoverageReport(options))
    .Does(() =>
{
    if (!FileExists(options.OpenCoverXml))
        throw new Exception("Missing \"" + options.OpenCoverXml + "\" file");

    UploadCoverageReport(MakeAbsolute(File(options.OpenCoverXml)).FullPath);
})
.OnError(exception =>
{
    Information("Error: " + exception.Message);
});

Task("Create-Packages")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    projects.Pack(p => !p.IsTestProject, configuration);
});

Task("Publish-MyGet")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => ShouldPublishToMyGet(options))
    .Does(() =>
{
    UploadPackagesToMyGet(options.OutputPackagesDir);
})
.OnError(exception =>
{
    Information("Error: " + exception.Message);
});

Task("Publish-NuGet")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => ShouldPublishToNuGet(options))
    .Does(() =>
{
    UploadPackagesToNuGet(options.OutputPackagesDir);
})
.OnError(exception =>
{
    Information("Error: " + exception.Message);
});

Task("Run-Coverity-Scan")
    .Does(() =>
{
    RunCoverityScan(options, configuration);
})
.OnError(exception =>
{
    Information("Error: " + exception.Message);
});

//
// Targets
//

Task("Default")
    .IsDependentOn("Create-Packages")
    .IsDependentOn("Publish-Coverage")
    .IsDependentOn("Publish-MyGet")
    .IsDependentOn("Publish-NuGet");

// Coverity scan is going to be run "on-demand" and can be triggered on AppVeyor
// by setting "TARGET" environment variable to "Coverity"
Task("Coverity")
    .IsDependentOn("Run-Coverity-Scan");

//
// Run build
//

RunTarget(target);