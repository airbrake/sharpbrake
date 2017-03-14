using System.Collections.Generic;

namespace Sharpbrake.Build
{
    /// <summary>
    /// Options used during build process.
    /// </summary>
    public class Options
    {
        #region Platform
        public bool IsLocalBuild { get; set; }
        public bool IsMonoPresent { get; set; }

        public bool IsRunningOnUnix { get; set; }
        public bool IsRunningOnWindows { get; set; }
        public bool IsRunningOnAppVeyor { get; set; }
        #endregion

        #region Repository
        public bool IsPullRequest { get; set; }
        public bool IsMainBranch { get; set; }
        public bool IsTagged { get; set; }
        #endregion

        #region Features on/off
        public bool SkipGitVersion { get; set; }
        public bool SkipCoverage { get; set; }
        #endregion

        #region Version
        public string AssemblySemVer { get; set; }
        public string MajorMinorPatch { get; set; }
        public string InformationalVersion { get; set; }
        public string FullSemVer { get; set; }
        public string SemVer { get; set; }
        public string NuGetVersion { get; set; }
        public string NuGetVersionSuffix { get; set; }
        #endregion

        #region Artifacts
        public ICollection<string> DirectoriesToClean { get; set; }
        public string TestResultsDir { get; set; }
        public string OpenCoverXml { get; set; }
        public string CoverageReportDir { get; set; }
        public string OutputPackagesDir { get; set; }
        #endregion
    }
}
