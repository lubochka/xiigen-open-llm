// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningR17IntermediateHonestyTests
{
    [Fact]
    public void ActiveModelIndex_DoesNotAdvertiseLegacyOrchestrationReasoningModel()
    {
        var indexPath = RepoPath("models", "index.json");
        using var document = JsonDocument.Parse(File.ReadAllText(indexPath));

        Assert.DoesNotContain(
            document.RootElement.GetProperty("models").EnumerateArray(),
            model => model.TryGetProperty("model_id", out var id)
                && string.Equals(id.GetString(), "orchestration-reasoning-v1", StringComparison.Ordinal));
    }

    [Fact]
    public void Phase08_CommonLegIsFalseFromRegistryEvenWhenLegacyFilesRemain()
    {
        var repo = RepoPath();
        var legacyDirectory = RepoPath(
            "models",
            "orchestration-reasoning",
            "orchestration-reasoning-v1");
        Assert.True(Directory.Exists(legacyDirectory));
        var userRoot = Path.Combine(Path.GetTempPath(), $"phase08-r17-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(userRoot);
            var rows = new Phase08CommonUserTwoLegLifecycleService()
                .BuildTwoLegLifecycleRows(repo, userRoot);

            Assert.True(rows.IsSuccess, rows.ErrorMessage);
            var orchestration = Assert.Single(
                rows.Value!,
                row => row.SphereId == "orchestration-reasoning");
            Assert.False(orchestration.CommonLegRepoOwned);
        }
        finally
        {
            Directory.Delete(userRoot, recursive: true);
        }
    }

    private static string RepoPath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. parts]);
    }
}
