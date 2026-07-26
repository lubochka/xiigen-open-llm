// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

internal static class ExtensionRuntimeTokenQualityArtifactPaths
{
    internal const string CollectionName = "ExtensionRuntimeTokenQualityArtifacts";

    private const string ArtifactDirectoryName = "extension-runtime-token-quality-learnability";
    private const string StateFileName = "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json";

    internal static readonly string RepoRoot = FindRepoRoot();

    internal static readonly string ArtifactDir = Path.Combine(
        RepoRoot,
        "artifacts",
        ArtifactDirectoryName);

    internal static readonly string StatePath = Path.Combine(
        RepoRoot,
        "docs",
        "plans",
        StateFileName);

    private static string FindRepoRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var current = new DirectoryInfo(Path.GetFullPath(start));
            while (current is not null)
            {
                if (IsRepoRoot(current.FullName))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate repository root containing Tests/LlmFromScratch.Tests.csproj and artifacts/{ArtifactDirectoryName}.");
    }

    private static bool IsRepoRoot(string path)
        => File.Exists(Path.Combine(path, "Tests", "LlmFromScratch.Tests.csproj"))
           && Directory.Exists(Path.Combine(path, "artifacts", ArtifactDirectoryName));
}

[CollectionDefinition(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName, DisableParallelization = true)]
public sealed class ExtensionRuntimeTokenQualityArtifactsCollection
{
}
