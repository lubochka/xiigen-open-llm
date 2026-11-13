// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;

public sealed class RepositoryModelIndexIntegrityGuardTests
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    [Fact]
    public void RepositoryModelIndexIntegrityGuard_CommittedIndex_BindsAllRowsToPhysicalBytes()
    {
        var repoRoot = GetRepoRoot();

        var validation = RepositoryModelIndexIntegrityGuard.Validate(repoRoot);

        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        Assert.True(validation.Value!.Models.Count >= 5);
        Assert.All(validation.Value.Models, row =>
        {
            Assert.Equal(row.ManifestSha256, Sha256File(row.ManifestFullPath));
            Assert.Equal(row.CheckpointSha256, Sha256File(row.CheckpointFullPath));
            Assert.Equal(row.CheckpointSha256, row.ManifestCheckpointSha256);
        });

        var lookup = RepositoryModelIndexIntegrityGuard.FindValidatedRowByManifestPath(
            repoRoot,
            @"models\orchestration\orchestration-v1\manifest.json");
        Assert.True(lookup.IsSuccess, lookup.ErrorMessage);
        Assert.Equal("orchestration-v1", lookup.Value!.ModelId);
    }

    [Fact]
    public void RepositoryModelIndexIntegrityGuard_ValidateLoadedIndexUsesSuppliedJsonSnapshot()
        => WithValidRegistry(paths =>
        {
            var loaded = RepositoryModelIndexIntegrityGuard.ReadBoundedIndexJson(paths.Root, paths.IndexPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);

            var laterFileBytes = """
                {
                  "models": [
                    {
                      "model_id": "manifest-only-after-read",
                      "manifest_path": "models/test/test-model-v1/manifest.json"
                    }
                  ]
                }
                """.Replace("\r\n", "\n", StringComparison.Ordinal);
            File.WriteAllText(paths.IndexPath, laterFileBytes, Utf8NoBom);

            var loadedSnapshotValidation = RepositoryModelIndexIntegrityGuard.ValidateLoadedIndex(
                loaded.Value!.RepositoryRoot,
                loaded.Value.IndexPath,
                loaded.Value.Json);
            var currentFileValidation = RepositoryModelIndexIntegrityGuard.Validate(paths.Root);

            Assert.True(loadedSnapshotValidation.IsSuccess, loadedSnapshotValidation.ErrorMessage);
            Assert.False(currentFileValidation.IsSuccess);
            Assert.Equal(
                RepositoryModelIndexIntegrityGuard.RequiredFieldMissingErrorCode,
                currentFileValidation.ErrorCode);
        });

    [Theory]
    [InlineData("manifest_sha256")]
    [InlineData("checkpoint_sha256")]
    public void RepositoryModelIndexIntegrityGuard_RejectsOneCharacterFileHashMutation(
        string hashField)
        => WithValidRegistry(paths =>
        {
            var index = ReadIndex(paths.IndexPath);
            var row = index["models"]!.AsArray()[0]!.AsObject();
            var original = row[hashField]!.GetValue<string>();
            var mutated = FlipLastHexCharacter(original);
            row[hashField] = mutated;
            if (string.Equals(hashField, "checkpoint_sha256", StringComparison.Ordinal))
                row["manifest_checkpoint_sha256"] = mutated;
            WriteIndex(paths.IndexPath, index);

            var validation = RepositoryModelIndexIntegrityGuard.Validate(paths.Root);

            Assert.False(validation.IsSuccess);
            Assert.Equal(RepositoryModelIndexIntegrityGuard.HashMismatchErrorCode, validation.ErrorCode);
        });

    [Fact]
    public void RepositoryModelIndexIntegrityGuard_RejectsMissingBoundFile()
        => WithValidRegistry(paths =>
        {
            File.Delete(paths.CheckpointFullPath);

            var validation = RepositoryModelIndexIntegrityGuard.Validate(paths.Root);

            Assert.False(validation.IsSuccess);
            Assert.Equal(RepositoryModelIndexIntegrityGuard.FileMissingErrorCode, validation.ErrorCode);
        });

    [Fact]
    public void RepositoryModelIndexIntegrityGuard_RejectsPathEscapeBeforeFileTrust()
        => WithValidRegistry(paths =>
        {
            var index = ReadIndex(paths.IndexPath);
            var row = index["models"]!.AsArray()[0]!.AsObject();
            row["manifest_path"] = "../outside-manifest.json";
            WriteIndex(paths.IndexPath, index);

            var validation = RepositoryModelIndexIntegrityGuard.Validate(paths.Root);

            Assert.False(validation.IsSuccess);
            Assert.Equal(RepositoryModelIndexIntegrityGuard.PathOutsideRepositoryErrorCode, validation.ErrorCode);
        });

    [Fact]
    public void RepositoryModelIndexIntegrityGuard_RejectsDuplicateModelId()
        => WithValidRegistry(paths =>
        {
            var index = ReadIndex(paths.IndexPath);
            index["models"]!.AsArray().Add(new JsonObject
            {
                ["model_id"] = "test-model-v1"
            });
            WriteIndex(paths.IndexPath, index);

            var validation = RepositoryModelIndexIntegrityGuard.Validate(paths.Root);

            Assert.False(validation.IsSuccess);
            Assert.Equal(RepositoryModelIndexIntegrityGuard.DuplicateModelIdErrorCode, validation.ErrorCode);
        });

    [Fact]
    public void RepositoryModelIndexIntegrityGuard_RejectsManifestOnlyRow()
        => WithValidRegistry(paths =>
        {
            var index = new JsonObject
            {
                ["models"] = new JsonArray(
                    new JsonObject
                    {
                        ["model_id"] = "manifest-only-v1",
                        ["manifest_path"] = "models/test/manifest-only-v1/manifest.json"
                    })
            };
            WriteIndex(paths.IndexPath, index);

            var validation = RepositoryModelIndexIntegrityGuard.Validate(paths.Root);

            Assert.False(validation.IsSuccess);
            Assert.Equal(RepositoryModelIndexIntegrityGuard.RequiredFieldMissingErrorCode, validation.ErrorCode);
        });

    [Fact]
    public void RepositoryModelIndexIntegrityGuard_RejectsReparsePointSegmentWhenPortable()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "xiigen-model-index-reparse-test",
            Guid.NewGuid().ToString("N"));
        var outside = Path.Combine(
            Path.GetTempPath(),
            "xiigen-model-index-reparse-outside",
            Guid.NewGuid().ToString("N"));
        var linkPath = Path.Combine(root, "models", "linked-manifest-root");

        try
        {
            Directory.CreateDirectory(Path.Combine(root, "models"));
            Directory.CreateDirectory(outside);
            File.WriteAllText(Path.Combine(outside, "manifest.json"), "{\"manifest\":true}\n", Utf8NoBom);

            try
            {
                Directory.CreateSymbolicLink(linkPath, outside);
            }
            catch (Exception ex) when (
                ex is IOException
                || ex is UnauthorizedAccessException
                || ex is PlatformNotSupportedException)
            {
                return;
            }

            var checkpointRef = "models/test/reparse-v1/checkpoint.json";
            var checkpointPath = Path.Combine(root, checkpointRef.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(checkpointPath)!);
            File.WriteAllText(checkpointPath, "{\"checkpoint\":true}\n", Utf8NoBom);

            var indexPath = Path.Combine(root, "models", "index.json");
            WriteIndex(indexPath, CreateIndex(
                "reparse-v1",
                "models/linked-manifest-root/manifest.json",
                checkpointRef,
                Sha256File(Path.Combine(outside, "manifest.json")),
                Sha256File(checkpointPath)));

            var validation = RepositoryModelIndexIntegrityGuard.Validate(root);

            Assert.False(validation.IsSuccess);
            Assert.Equal(RepositoryModelIndexIntegrityGuard.UnsafePathErrorCode, validation.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(linkPath))
                Directory.Delete(linkPath);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
            if (Directory.Exists(outside))
                Directory.Delete(outside, recursive: true);
        }
    }

    private static void WithValidRegistry(Action<TestRegistryPaths> test)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "xiigen-model-index-integrity-test",
            Guid.NewGuid().ToString("N"));

        try
        {
            var paths = CreateValidRegistry(root);
            test(paths);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static TestRegistryPaths CreateValidRegistry(string root)
    {
        var manifestRef = "models/test/test-model-v1/manifest.json";
        var checkpointRef = "models/test/test-model-v1/test-model-v1-checkpoint.json";
        var manifestPath = Path.Combine(root, manifestRef.Replace('/', Path.DirectorySeparatorChar));
        var checkpointPath = Path.Combine(root, checkpointRef.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(checkpointPath)!);
        File.WriteAllText(manifestPath, "{\"manifest\":true}\n", Utf8NoBom);
        File.WriteAllText(checkpointPath, "{\"checkpoint\":true}\n", Utf8NoBom);

        var indexPath = Path.Combine(root, "models", "index.json");
        WriteIndex(indexPath, CreateIndex(
            "test-model-v1",
            manifestRef,
            checkpointRef,
            Sha256File(manifestPath),
            Sha256File(checkpointPath)));

        return new TestRegistryPaths(
            root,
            indexPath,
            manifestPath,
            checkpointPath);
    }

    private static JsonObject CreateIndex(
        string modelId,
        string manifestRef,
        string checkpointRef,
        string manifestSha256,
        string checkpointSha256)
        => new()
        {
            ["models"] = new JsonArray(
                new JsonObject
                {
                    ["model_id"] = modelId,
                    ["domain"] = "TestDomain",
                    ["manifest_path"] = manifestRef,
                    ["checkpoint_path"] = checkpointRef,
                    ["version"] = "test-v1",
                    ["manifest_sha256"] = manifestSha256,
                    ["checkpoint_sha256"] = checkpointSha256,
                    ["manifest_checkpoint_sha256"] = checkpointSha256
                })
        };

    private static JsonObject ReadIndex(string indexPath)
        => JsonNode.Parse(File.ReadAllText(indexPath, Utf8NoBom))!.AsObject();

    private static void WriteIndex(string indexPath, JsonObject index)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
        File.WriteAllText(
            indexPath,
            index.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n",
            Utf8NoBom);
    }

    private static string FlipLastHexCharacter(string value)
        => value[..^1] + (value[^1] == 'a' ? 'b' : 'a');

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root with LlmFromScratch.sln was not found.");
    }

    private sealed record TestRegistryPaths(
        string Root,
        string IndexPath,
        string ManifestFullPath,
        string CheckpointFullPath);
}
