// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace Tests;

using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;
using Xunit;

public sealed class DesignConversionTrainingIntakeDpoPairStoreHostIntegrationTests
{
    private const UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead |
        UnixFileMode.UserWrite |
        UnixFileMode.UserExecute;

    [Fact]
    public void PromotionHostOptionsToString_RedactsExplicitPairStoreRootAndShowsHumanOptionPresence()
    {
        var sensitiveRoot = Path.Combine(
            Path.GetTempPath(),
            "pair-store-host-integration-sensitive",
            Guid.NewGuid().ToString("N"));
        var pairStoreOptions = new DesignConversionTrainingIntakeDpoPairStoreOptions(
            sensitiveRoot);
        var pairStoreReport = pairStoreOptions.ToString();
        var options = new DesignConversionModelServerPromotionHostOptions(
            ArtifactRootDirectory: "artifact-root-for-redaction-test",
            TrainingIntakeDpoPairStoreOptions: pairStoreOptions);
        var report = options.ToString();

        // Проверяем реальный human-facing report, а не хрупкий текст исходника:
        // explicit state root чувствителен и обязан скрываться даже при наличии опции.
        Assert.Contains(
            "NativeStateRootOverride = [REDACTED]",
            pairStoreReport,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            sensitiveRoot,
            pairStoreReport,
            StringComparison.Ordinal);
        Assert.Contains(
            "TrainingIntakeDpoPairStoreOptions = [REDACTED]",
            report,
            StringComparison.Ordinal);
        Assert.Contains(
            "TrainingIntakeDpoPairStoreOptionsConfigured",
            report,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            sensitiveRoot,
            report,
            StringComparison.Ordinal);
    }

    [PairStoreWslWindowsDriveFact]
    public void PromotionHostOptions_ExplicitNativeRootReachesLocalHostJsonlDbUnderWslWindowsDrive()
    {
        var artifactRoot = NewRepoArtifactRoot(
            "promotion-host-pair-store-options-artifact");
        var nativeBase = NewNativeTempRoot(
            "promotion-host-pair-store-options-native");
        try
        {
            var hostOptions = new DesignConversionModelServerPromotionHostOptions(
                artifactRoot,
                TrainingIntakeDpoPairStoreOptions:
                    new DesignConversionTrainingIntakeDpoPairStoreOptions(
                        nativeBase));
            // Это интеграция host-а, а не модульный мок: проверяем настоящий
            // путь вызовов от options до JsonlDesignConversionLocalArtifactDb.
            var registry = DesignConversionModelServerAspNetHostStartup
                .CreateLocalRouteRegistry(
                    DesignConversionLocalServiceComposition
                        .CreateDeterministicLocalForLiveHostTesting(),
                    new UnexpectedCandidateStrategy(),
                    hostOptions);

            var db = ReadTrainingIntakeArtifactDb(registry);
            var resolution =
                db.TrainingIntakeDpoPairStoreRootResolutionForTesting;
            var expectedNativeBase = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(nativeBase));
            var expectedNativePrefix = Path.Combine(
                expectedNativeBase,
                "xiigen",
                "design-conversion",
                "training-intake-dpo-pairs");

            Assert.True(resolution.UsesNativeWslStateRoot);
            Assert.Equal(expectedNativeBase, resolution.NativeStateBaseRootDirectory);
            Assert.StartsWith(
                expectedNativePrefix + Path.DirectorySeparatorChar,
                resolution.PairStoreRootDirectory + Path.DirectorySeparatorChar,
                StringComparison.Ordinal);
            Assert.Equal(
                Path.Combine(
                    Path.GetFullPath(artifactRoot),
                    DesignConversionTrainingIntakeDpoPairStoreRootResolver
                        .ManifestFileName),
                resolution.ManifestPath);
            Assert.True(File.Exists(resolution.ManifestPath));
        }
        finally
        {
            DeleteOwnedRoot(artifactRoot);
            DeleteOwnedRoot(nativeBase);
        }
    }

    [Fact]
    public void LiveLocalSource_ExposesExplicitPairStoreStateRootWithoutTempDefault()
    {
        var source = ReadRepoFile(
            "tools",
            "DesignConversionLiveLocalServer",
            "Program.cs");

        // Live-local должен иметь видимый оператору explicit root и split-brain
        // отказ; отсутствие `/tmp` default защищает production от случайной потери.
        Assert.Contains(
            "--training-intake-dpo-state-root",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_STATE_ROOT",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_STATE_ROOT_SPLIT_BRAIN",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "Path.IsPathFullyQualified(trimmedStateRoot)",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "must be absolute before normalization",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "options.TrainingIntakeDpoPairStoreOptions",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "new DesignConversionTrainingIntakeDpoPairStoreOptions(Path.GetTempPath()",
            source,
            StringComparison.Ordinal);
    }

    private static JsonlDesignConversionLocalArtifactDb ReadTrainingIntakeArtifactDb(
        DesignConversionModelServerEndpointRegistry registry)
    {
        var registration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        var handler = Assert.IsType<DesignConversionModelServerTrainingIntakeHandler>(
            registration.Value!.Handler.Target);
        var api = Assert.IsType<DesignConversionDpoUsefulnessApi>(
            typeof(DesignConversionModelServerTrainingIntakeHandler)
                .GetField(
                    "_api",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(handler));
        return Assert.IsType<JsonlDesignConversionLocalArtifactDb>(
            typeof(DesignConversionDpoUsefulnessApi)
                .GetField(
                    "_artifactDb",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(api));
    }

    private static string ReadRepoFile(params string[] relativeSegments)
        => File.ReadAllText(Path.Combine(FindRepoRoot(), Path.Combine(relativeSegments)));

    private static string NewRepoArtifactRoot(string leafName)
    {
        var root = Path.Combine(
            FindRepoRoot(),
            "artifacts",
            "pair-store-wsl",
            "worker",
            "after-integration",
            "test-state",
            $"{leafName}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private static string NewNativeTempRoot(string leafName)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pair-store-host-integration",
            $"{leafName}-{Guid.NewGuid():N}");
        if (OperatingSystem.IsLinux())
        {
            // Runtime-путь проверяет production owner-only guard, поэтому test
            // native root создаётся тем же режимом 0700, что и resolver tests.
            Directory.CreateDirectory(root, OwnerDirectoryMode);
        }
        else
        {
            Directory.CreateDirectory(root);
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
    }

    private static void DeleteOwnedRoot(string root)
    {
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            return;

        Directory.Delete(root, recursive: true);
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repository root from the current test directory.");
    }

    private sealed class PairStoreWslWindowsDriveFactAttribute : FactAttribute
    {
        public PairStoreWslWindowsDriveFactAttribute()
        {
            if (!OperatingSystem.IsLinux())
            {
                Skip = "Requires Linux WSL with a mounted Windows drive.";
                return;
            }
            if (!File.Exists("/proc/version") ||
                !File.ReadAllText("/proc/version").Contains(
                    "microsoft",
                    StringComparison.OrdinalIgnoreCase))
            {
                Skip = "Requires WSL.";
                return;
            }
            if (!File.Exists("/proc/self/mountinfo") ||
                !File.ReadLines("/proc/self/mountinfo").Any(
                    static line => line.Contains(" /mnt/c ", StringComparison.Ordinal) &&
                                   line.Contains(" - 9p ", StringComparison.Ordinal)))
            {
                Skip = "Requires a WSL /mnt/c Windows-drive v9fs mount.";
                return;
            }

            var repoRoot = TryFindRepoRoot(Directory.GetCurrentDirectory());
            if (repoRoot is null ||
                !repoRoot.StartsWith("/mnt/c/", StringComparison.Ordinal))
            {
                Skip = "Requires this checkout to run from the WSL /mnt/c mount.";
            }
        }

        private static string? TryFindRepoRoot(string startDirectory)
        {
            for (var directory = new DirectoryInfo(startDirectory);
                 directory is not null;
                 directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                    return directory.FullName;
            }

            return null;
        }
    }

    private sealed class UnexpectedCandidateStrategy :
        IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>>
            CreateNextCandidateAsync(
                DesignConversionCandidateUpdateRequest request,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "Training-intake pair-store host integration test does not request candidate generation.",
                    "TRAINING_INTAKE_PAIR_STORE_HOST_TEST_UNEXPECTED_CANDIDATE_REQUEST"));
    }
}
