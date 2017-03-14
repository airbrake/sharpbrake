Developer's How-To
==========

## Build from command-line on Windows

* build with `Default` target

  ```powershell
  PS> .\build.ps1
  ```

  Target `Default` includes the next steps (Cake tasks):

  * `Info` - outputs the current version of Sharpbrake (version is calculated using GitVersion), configuration (`Debug` or `Release`) and target (`Default` or `Coverity`)
  * `Clean` - cleanups directories for artifacts (artifacts/VERSION), test results (artifacts/VERSION/test-results) and NuGet packages (artifacts/VERSION/nuget)
  * `Patch-Version`- sets calculated SemVer into AssemblyInfo.cs and project.json files for non-test projects and updates version number for "Sharpbrake.Client" dependency
  * `Restore-Packages` - restores packages for all projects using "project.json" files
  * `Build` - builds all projects using "project.json" files
  * `Run-Unit-Tests` - runs unit tests and calculates code coverage (OpenCover tool is used for code coverage)
  * `Publish-Coverage` - publishes coverage report to codecov.io (if coverage is not skipped and this is neither a local build nor a pull request)
  * `Create-Packages` - creates NuGet packages for uploading to NuGet-based repositories
  * `Publish-MyGet` - publishes packages to MyGet repo if this is Merge Request to master
  * `Publish-NuGet` - publishes packages to NuGet repo if master is tagged

  Locally test coverage results can be checked here: _artifacts/VERSION/test-results/report/index.htm_

  NuGet packages can be found in this folder: _artifacts/VERSION/nuget_

  Build with this target is performed automatically each time when new code is pushed to the repo or master branch is tagged. A build can be forced manually by using AppVeyor's "New Build" command. If a build on AppVeyor should be performed with some other target like `Coverity` for static code analysis, then the TARGET environment variable should be defined with the desired target (AppVeyor => Settings => Environment => Add variable).

* build with `Coverity` target

  ```powershell
  PS> .\build.ps1 -Target Coverity
  ```

  Build with `Coverity` target can be used when you need to analyze code (static analysis) with Coverity Scan.
  The next steps should be performed to use AppVeyor for such build:

  1. Set TARGET environment variable to `Coverity` on AppVeyor.
  2. Run build manually using "New Build" command on AppVeyor or push some code changes to trigger build automatically.
  3. Go to https://scan.coverity.com/projects/sharpbrake and check static analysis results.
  4. Remove TARGET environment variable to "restore" normal build ("Default" target).