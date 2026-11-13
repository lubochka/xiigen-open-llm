// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Reflection;
using System.Text.Json;
using DesignConversionDomain;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionLiveLocalBootstrapCheckpointTests
{
    private const string CheckpointIdentity = "pass143-bootstrap-v1";
    private const string CheckpointRelativePath =
        "models/design-conversion/pass143-bootstrap-v1/pass143-bootstrap-v1-checkpoint.json";
    private const string ManifestRelativePath =
        "models/design-conversion/pass143-bootstrap-v1/manifest.json";

    [Fact]
    public async Task RepositoryBootstrapCheckpoint_LoadsFreshWithPinnedDigestRealWeightsAndNoDpoPolicy()
    {
        var repoRoot = FindRepoRoot();
        var checkpointPath = Path.Combine(repoRoot, CheckpointRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var checkpointBytes = File.ReadAllBytes(checkpointPath);
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(
            Path.Combine(repoRoot, ManifestRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        var checkpoint = JsonSerializer.Deserialize<DesignConversionMachineCheckpoint>(
            checkpointBytes,
            DesignConversionCheckpointJsonOptions.Standard);

        Assert.NotNull(checkpoint);
        Assert.Equal(
            manifest.RootElement.GetProperty("checkpoint_sha256").GetString(),
            Convert.ToHexString(SHA256.HashData(checkpointBytes)).ToLowerInvariant());
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint));
        Assert.True(DesignConversionMachine.CountRealLearnedWeightArtifacts(checkpoint) > 0);
        Assert.Null(checkpoint!.DpoPolicyCheckpointRef);

        await WithBootstrapBytesInAppBaseAsync(checkpointBytes, async () =>
        {
            var first = await InvokeBootstrapLoaderAsync(repoRoot);
            var second = await InvokeBootstrapLoaderAsync(repoRoot);
            var firstMachine = Assert.IsType<DesignConversionMachine>(ReadProperty(first, "Machine"));
            var secondMachine = Assert.IsType<DesignConversionMachine>(ReadProperty(second, "Machine"));
            var firstExport = firstMachine.ExportCheckpoint();

            Assert.NotSame(firstMachine, secondMachine);
            Assert.True(firstExport.IsSuccess, firstExport.ErrorMessage);
            Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(firstExport.Value));
            Assert.Equal(
                DesignConversionMachine.CountRealLearnedWeightArtifacts(checkpoint),
                DesignConversionMachine.CountRealLearnedWeightArtifacts(firstExport.Value));
            Assert.Equal(CheckpointIdentity, ReadProperty(first, "CheckpointIdentity"));
            Assert.Equal(
                Convert.ToHexString(SHA256.HashData(checkpointBytes)).ToLowerInvariant(),
                ReadProperty(first, "CheckpointSha256"));
            Assert.Equal(1, ReadProperty(first, "TrainingRows"));
            Assert.True((int)ReadProperty(first, "LearnedWeightArtifactCount")! > 0);
            Assert.False((bool)ReadProperty(first, "DeterministicLogicClaimedAsLearning")!);
            Assert.False((bool)ReadProperty(first, "MachineTrainingExecutedAtStartup")!);
            Assert.False((bool)ReadProperty(first, "ProductReady")!);
        });
    }

    [Fact]
    public async Task RepositoryBootstrapCheckpoint_TamperedBytesFailBeforeMachineCreation()
    {
        var repoRoot = FindRepoRoot();
        var checkpointBytes = File.ReadAllBytes(Path.Combine(
            repoRoot,
            CheckpointRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        checkpointBytes[^1] ^= 0x01;
        Exception? failure = null;
        await WithBootstrapBytesInAppBaseAsync(checkpointBytes, async () =>
        {
            failure = await Record.ExceptionAsync(() => InvokeBootstrapLoaderAsync(repoRoot));
        });

        Assert.NotNull(failure);
        Assert.Equal(
            "PASS143_MACHINE_BOOTSTRAP_CHECKPOINT_SHA256_MISMATCH",
            ReadProperty(failure!, "Code"));
        Assert.Contains(
            "PASS143_MACHINE_BOOTSTRAP_CHECKPOINT_SHA256_MISMATCH",
            failure!.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(AppContext.BaseDirectory, failure.Message, StringComparison.OrdinalIgnoreCase);

        Exception? missingFailure = null;
        await WithoutBootstrapFileInAppBaseAsync(async () =>
        {
            missingFailure = await Record.ExceptionAsync(() => InvokeBootstrapLoaderAsync(repoRoot));
        });
        Assert.NotNull(missingFailure);
        Assert.Equal(
            "PASS143_MACHINE_BOOTSTRAP_CHECKPOINT_MISSING",
            ReadProperty(missingFailure!, "Code"));
        Assert.Contains(
            "PASS143_MACHINE_BOOTSTRAP_CHECKPOINT_MISSING",
            missingFailure!.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            AppContext.BaseDirectory,
            missingFailure.Message,
            StringComparison.OrdinalIgnoreCase);

        var source = ReadLoaderSource(repoRoot);
        var digestIndex = source.IndexOf("SHA256.HashData(checkpointBytes)", StringComparison.Ordinal);
        var comparisonIndex = source.IndexOf("CryptographicOperations.FixedTimeEquals", StringComparison.Ordinal);
        var deserializeIndex = source.IndexOf("JsonSerializer.Deserialize<DesignConversionMachineCheckpoint>", StringComparison.Ordinal);
        var machineIndex = source.IndexOf("new DesignConversionMachine()", StringComparison.Ordinal);

        Assert.True(digestIndex >= 0);
        Assert.True(comparisonIndex > digestIndex);
        Assert.True(deserializeIndex > comparisonIndex);
        Assert.True(machineIndex > deserializeIndex);
        Assert.Contains("CheckpointSha256MismatchCode", source, StringComparison.Ordinal);
        Assert.DoesNotContain("checkpointPath +", source, StringComparison.Ordinal);
    }

    [Fact]
    public void RepositoryBootstrapCheckpoint_ManifestAndModelsIndexBindExactCheckpointWithoutReadinessOverclaim()
    {
        var repoRoot = FindRepoRoot();
        var checkpointBytes = File.ReadAllBytes(Path.Combine(
            repoRoot,
            CheckpointRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var manifestBytes = File.ReadAllBytes(Path.Combine(
            repoRoot,
            ManifestRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        using var manifest = JsonDocument.Parse(manifestBytes);
        using var index = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(repoRoot, "models", "index.json")));
        var entry = Assert.Single(
            index.RootElement.GetProperty("models").EnumerateArray(),
            candidate => string.Equals(
                candidate.GetProperty("model_id").GetString(),
                CheckpointIdentity,
                StringComparison.Ordinal));
        var checkpointSha = Convert.ToHexString(SHA256.HashData(checkpointBytes)).ToLowerInvariant();
        var manifestSha = Convert.ToHexString(SHA256.HashData(manifestBytes)).ToLowerInvariant();

        Assert.Equal(CheckpointIdentity, manifest.RootElement.GetProperty("model_id").GetString());
        Assert.Equal(CheckpointRelativePath, entry.GetProperty("checkpoint_path").GetString());
        Assert.Equal(ManifestRelativePath, entry.GetProperty("manifest_path").GetString());
        Assert.Equal(checkpointSha, manifest.RootElement.GetProperty("checkpoint_sha256").GetString());
        Assert.Equal(checkpointSha, entry.GetProperty("checkpoint_sha256").GetString());
        Assert.Equal(checkpointSha, entry.GetProperty("manifest_checkpoint_sha256").GetString());
        Assert.Equal(manifestSha, entry.GetProperty("manifest_sha256").GetString());
        Assert.False(manifest.RootElement.GetProperty("product_ready").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("deterministic_logic_claimed_as_learning").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("machine_training_executed_at_startup").GetBoolean());
    }

    [Fact]
    public void ProgramSource_MachineBackedStartupLoadsPinnedCheckpointWithoutTrainingAuthorityPath()
    {
        var repoRoot = FindRepoRoot();
        var program = File.ReadAllText(Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "Program.cs"));

        Assert.Contains("Pass143MachineBootstrapCheckpoint.LoadAsync()", program, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateTinyMachineBackedLocalMachineAsync", program, StringComparison.Ordinal);
        Assert.DoesNotContain("DesignConversionMachineTrainingAuthority.ExecuteExactAsync", program, StringComparison.Ordinal);
        Assert.DoesNotContain("new LearningTrainingItem(", program, StringComparison.Ordinal);
        Assert.Contains("MachineTrainingExecutedAtStartup: false", program, StringComparison.Ordinal);
        Assert.Contains("MachineBackedApiProductReady:", program, StringComparison.Ordinal);
        Assert.Contains("body[\"productReady\"] = false", program, StringComparison.Ordinal);
    }

    [Fact]
    public void ProgramSource_MachineBackedBootstrapCannotMintOrPropagateTrainingCapability()
    {
        var repoRoot = FindRepoRoot();
        var loader = ReadLoaderSource(repoRoot);
        var program = File.ReadAllText(Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "Program.cs"));

        Assert.DoesNotContain("LegalTrainingTeacherAdmission", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("TrainingAuthority", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("LearningBatch", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("DpoPolicy", loader, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Pass143MachineBootstrapCheckpoint.LoadAsync(legalTeacherAdmissionAuthority",
            program,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task MachineBackedBootstrap_IsIndependentOfProcessWorkingDirectory()
    {
        var repoRoot = FindRepoRoot();
        var checkpointBytes = File.ReadAllBytes(Path.Combine(
            repoRoot,
            CheckpointRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var loader = ReadLoaderSource(repoRoot);
        var project = File.ReadAllText(Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj"));

        var originalWorkingDirectory = Environment.CurrentDirectory;
        const string overrideVariable = "PASS143_MACHINE_BOOTSTRAP_BASE_DIRECTORY";
        var originalOverride = Environment.GetEnvironmentVariable(overrideVariable);
        var unrelatedWorkingDirectory = Path.Combine(
            Path.GetTempPath(),
            "pass143-bootstrap-cwd",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(unrelatedWorkingDirectory);
        try
        {
            await WithBootstrapBytesInAppBaseAsync(checkpointBytes, async () =>
            {
                Environment.CurrentDirectory = unrelatedWorkingDirectory;
                Environment.SetEnvironmentVariable(overrideVariable, unrelatedWorkingDirectory);
                var loaded = await InvokeBootstrapLoaderAsync(repoRoot);
                Assert.IsType<DesignConversionMachine>(ReadProperty(loaded, "Machine"));
            });
        }
        finally
        {
            Environment.CurrentDirectory = originalWorkingDirectory;
            Environment.SetEnvironmentVariable(overrideVariable, originalOverride);
            Directory.Delete(unrelatedWorkingDirectory, recursive: true);
        }

        Assert.Contains("AppContext.BaseDirectory", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("Environment.CurrentDirectory", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.GetCurrentDirectory", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("Environment.GetEnvironmentVariable", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("args", loader, StringComparison.Ordinal);
        Assert.Contains("TargetPath=\"models\\design-conversion\\pass143-bootstrap-v1\\pass143-bootstrap-v1-checkpoint.json\"", project, StringComparison.Ordinal);
        Assert.Contains("CopyToOutputDirectory=\"PreserveNewest\"", project, StringComparison.Ordinal);
    }

    private static async Task<object> InvokeBootstrapLoaderAsync(string repoRoot)
    {
        var assemblyPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "bin",
            "Release",
            "net9.0",
            "DesignConversionLiveLocalServer.dll");
        Assert.True(
            File.Exists(assemblyPath),
            "Build DesignConversionLiveLocalServer in Release before running bootstrap tests.");
        var assembly = Assembly.LoadFrom(assemblyPath);
        var loaderType = assembly.GetType(
            "Pass143MachineBootstrapCheckpoint",
            throwOnError: true)!;
        var loadAsync = loaderType.GetMethod(
            "LoadAsync",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var task = Assert.IsAssignableFrom<Task>(loadAsync.Invoke(
            null,
            [CancellationToken.None]));
        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private static object? ReadProperty(object instance, string name)
        => instance.GetType().GetProperty(
            name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(instance);

    private static async Task WithBootstrapBytesInAppBaseAsync(
        byte[] checkpointBytes,
        Func<Task> action)
    {
        var outputCheckpointPath = Path.Combine(
            AppContext.BaseDirectory,
            CheckpointRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var hadPrior = File.Exists(outputCheckpointPath);
        var priorBytes = hadPrior ? await File.ReadAllBytesAsync(outputCheckpointPath) : null;
        Directory.CreateDirectory(Path.GetDirectoryName(outputCheckpointPath)!);
        await File.WriteAllBytesAsync(outputCheckpointPath, checkpointBytes);
        try
        {
            await action();
        }
        finally
        {
            if (hadPrior)
                await File.WriteAllBytesAsync(outputCheckpointPath, priorBytes!);
            else if (File.Exists(outputCheckpointPath))
                File.Delete(outputCheckpointPath);
        }
    }

    private static async Task WithoutBootstrapFileInAppBaseAsync(Func<Task> action)
    {
        var outputCheckpointPath = Path.Combine(
            AppContext.BaseDirectory,
            CheckpointRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var hadPrior = File.Exists(outputCheckpointPath);
        var priorBytes = hadPrior ? await File.ReadAllBytesAsync(outputCheckpointPath) : null;
        if (hadPrior)
            File.Delete(outputCheckpointPath);
        try
        {
            await action();
        }
        finally
        {
            if (hadPrior)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputCheckpointPath)!);
                await File.WriteAllBytesAsync(outputCheckpointPath, priorBytes!);
            }
        }
    }

    private static string ReadLoaderSource(string repoRoot)
        => File.ReadAllText(Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "Pass143MachineBootstrapCheckpoint.cs"));

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}
