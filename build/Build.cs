using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitReleaseManager;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.GitReleaseManager.GitReleaseManagerTasks;


[GitHubActions("pull-request",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.PullRequest },
    InvokedTargets = new[] { nameof(Tests) },
    AutoGenerate = true)]
[GitHubActions("build-main",
    GitHubActionsImage.UbuntuLatest,
    InvokedTargets = new[] { nameof(Tests), nameof(Publish), nameof(Pack), nameof(CreateRelease) },
    ImportSecrets = new[] { nameof(NugetToken) },
    OnPushBranches = new []{ "main" },
    EnableGitHubToken = true,
    AutoGenerate = true)]
[UnsetVisualStudioEnvironmentVariables]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Secret] [Parameter("Nuget API key", Name = "api-key")] readonly string NugetToken;

    [Parameter("NuGet Source for Packages", Name = "nuget-source")]
    readonly string NugetSource = "https://api.nuget.org/v3/index.json";

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(NoFetch = true, Framework = "net5.0")] readonly GitVersion GitVersion;
    [CI] GitHubActions GitHubActions;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    AbsolutePath PackageDirectory => ArtifactsDirectory / "packages";

    AbsolutePath TestResultDirectory => ArtifactsDirectory / "test-results";

    AbsolutePath CoverageReportDirectory => ArtifactsDirectory / "coverage-report";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetClean(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration));
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion));
        });

    Target Coverage => _ => _
        .Produces(CoverageReportDirectory)
        .Executes(() => { });

    Target Tests => _ => _
        .DependsOn(Compile)
        .Produces(TestResultDirectory / "*.trx")
        .Produces(TestResultDirectory / "*.xml")
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetNoBuild(InvokedTargets.Contains(Compile))
                .SetResultsDirectory(TestResultDirectory)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .When(InvokedTargets.Contains(Coverage) || IsServerBuild, _ => _
                    .EnableCollectCoverage()
                    .SetCoverletOutputFormat(CoverletOutputFormat.opencover)
                    .When(IsServerBuild, _ => _.EnableUseSourceLink()))
                .CombineWith(Solution.GetProjects("*.Tests"), (_, projectFile) => _
                    .SetProjectFile(projectFile)
                    .SetLoggers($"trx;LogFileName={projectFile.Name}.trx")
                    .SetCoverletOutput(CoverageReportDirectory / $"{projectFile.Name}.xml")));
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Produces(PackageDirectory / "*.nupkg")
        .Produces(PackageDirectory / "*.snupkg")
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution)
                .SetNoBuild(InvokedTargets.Contains(Compile))
                .SetConfiguration(Configuration)
                .SetOutputDirectory(PackageDirectory)
                .SetVersion(GitVersion.NuGetVersionV2)
                .EnableIncludeSource()
                .EnableIncludeSymbols()
                .EnableNoRestore());
        });

    Target Publish => _ => _
        .After(Pack)
        .Consumes(Pack)
        .Requires(() => NugetToken)
        .Requires(() => Configuration.Equals(Configuration.Release))
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetSource(NugetSource)
                .SetApiKey(NugetToken)
                .EnableSkipDuplicate()
                .CombineWith(
                    PackageDirectory.GlobFiles("*.nupkg", "*.snupkg"),
                    (_, v) => _.SetTargetPath(v)));
        });

    Target CreateRelease => _ => _
        .After(Publish)
        .Requires(() => GitHubActions)
        .Executes(() =>
        {
            GitReleaseManagerCreate(r => r
                .SetName(GitVersion.AssemblySemVer)
                .SetRepositoryName(GitHubActions.Repository)
                .SetRepositoryOwner(GitHubActions.RepositoryOwner)
                .SetTargetCommitish(GitHubActions.Sha)
                .AddAssetPaths(PackageDirectory)
            );
        });
}
