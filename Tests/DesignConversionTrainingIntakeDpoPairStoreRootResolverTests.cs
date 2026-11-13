// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace Tests;

using AgentInfrastructure;
using DesignConversionDomain;
using System.Text.Json.Nodes;
using Xunit;

public sealed class DesignConversionTrainingIntakeDpoPairStoreRootResolverTests
{
    private const UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead |
        UnixFileMode.UserWrite |
        UnixFileMode.UserExecute;
    private const UnixFileMode GroupOrOtherAccess =
        UnixFileMode.GroupRead |
        UnixFileMode.GroupWrite |
        UnixFileMode.GroupExecute |
        UnixFileMode.OtherRead |
        UnixFileMode.OtherWrite |
        UnixFileMode.OtherExecute;

    [PairStoreWslWindowsDriveFact]
    public void ExplicitNativeRoot_WslWindowsDriveArtifactRoot_CreatesStableRootAndManifest()
    {
        // Успешный путь фиксирует главную гарантию: пары уходят с `/mnt/c` в
        // owner-only native root, а manifest остаётся стабильным указателем.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-happy");
        var nativeBase = NewNativeBase("pair-store-resolver-happy-native");
        try
        {
            var options = new DesignConversionTrainingIntakeDpoPairStoreOptions(
                nativeBase);

            var first = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    options);
            var second = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    options);

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.NotNull(first.Value);
            Assert.NotNull(second.Value);
            Assert.True(first.Value!.UsesNativeWslStateRoot);
            Assert.Equal(first.Value.PairStoreRootDirectory,
                second.Value!.PairStoreRootDirectory);
            Assert.Equal(first.Value.ManifestPath, second.Value.ManifestPath);
            Assert.NotEqual(
                first.Value.LegacyPairStoreRootDirectory,
                first.Value.PairStoreRootDirectory);
            Assert.False(IsUnderDirectory(
                first.Value.PairStoreRootDirectory,
                artifactRoot));
            Assert.True(IsUnderDirectory(
                first.Value.PairStoreRootDirectory,
                nativeBase));
            Assert.True(File.Exists(first.Value.ManifestPath));
            AssertManifestBindsNativeDirectoryAndWslIdentity(first.Value);
            AssertOwnerOnlyDirectory(nativeBase);
            AssertOwnerOnlyDirectory(first.Value.PairStoreRootDirectory);
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void ManifestMismatch_FailsClosedInsteadOfSelectingSecondRoot()
    {
        // Manifest mismatch — это возможный split-brain сигнал; resolver не
        // должен молча выбрать другой native root поверх старого binding.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-mismatch");
        var nativeBase = NewNativeBase("pair-store-resolver-mismatch-native");
        try
        {
            var options = new DesignConversionTrainingIntakeDpoPairStoreOptions(
                nativeBase);
            var first = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    options);
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.NotNull(first.Value);
            File.WriteAllText(first.Value!.ManifestPath, "{\"schema\":\"other\"}");

            var second = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    options);

            Assert.False(second.IsSuccess);
            Assert.Equal(
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ManifestMismatchErrorCode,
                second.ErrorCode);
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void MismatchedExistingManifest_DoesNotCreateAlternativeNativeRoot()
    {
        // Existing manifest проверяется до создания native dirs: при mismatch
        // resolver не должен оставлять второй root как след неудачного выбора.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-mismatch-before-create");
        var nativeBase = NewNativeBase("pair-store-resolver-mismatch-before-create-native");
        try
        {
            File.WriteAllText(
                Path.Combine(
                    artifactRoot,
                    DesignConversionTrainingIntakeDpoPairStoreRootResolver
                        .ManifestFileName),
                "{\"schemaVersion\":\"different\"}");

            var result = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    new DesignConversionTrainingIntakeDpoPairStoreOptions(
                        nativeBase));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ManifestMismatchErrorCode,
                result.ErrorCode);
            Assert.False(Directory.Exists(Path.Combine(nativeBase, "xiigen")));
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void ManifestRestart_MissingNativeRoot_FailsClosed()
    {
        // Manifest хранит device/inode/effective user binding; если native root
        // исчез между запусками, resolver не пересоздаёт его молча.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-missing-native-root");
        var nativeBase = NewNativeBase("pair-store-resolver-missing-native-root-native");
        try
        {
            var first = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    new DesignConversionTrainingIntakeDpoPairStoreOptions(
                        nativeBase));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.NotNull(first.Value);
            Directory.Delete(first.Value!.PairStoreRootDirectory);

            var second = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    new DesignConversionTrainingIntakeDpoPairStoreOptions(
                        nativeBase));

            Assert.False(second.IsSuccess);
            Assert.Equal(
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ManifestMismatchErrorCode,
                second.ErrorCode);
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void ManifestRestart_ChangedNativeIdentityBinding_FailsClosed()
    {
        // Manifest хранит не только путь: device/inode/effective user и WSL
        // identity должны совпасть с реально открытым native directory.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-native-identity-tamper");
        var nativeBase = NewNativeBase("pair-store-resolver-native-identity-tamper-native");
        try
        {
            var first = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    new DesignConversionTrainingIntakeDpoPairStoreOptions(
                        nativeBase));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.NotNull(first.Value);
            ReplaceManifestString(
                first.Value!.ManifestPath,
                "nativePairStoreDirectoryEffectiveUserId",
                "999999");

            var second = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    new DesignConversionTrainingIntakeDpoPairStoreOptions(
                        nativeBase));

            Assert.False(second.IsSuccess);
            Assert.Equal(
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ManifestMismatchErrorCode,
                second.ErrorCode);
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void ConcurrentFirstStart_SameConfigAcceptsSingleManifestBinding()
    {
        // Два одинаковых first-start resolver-а могут столкнуться на publish;
        // проигравший принимает только exact manifest, temp не остаётся authority.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-race");
        var nativeBase = NewNativeBase("pair-store-resolver-race-native");
        try
        {
            var tasks = Enumerable.Range(0, 2)
                .Select(_ => Task.Run(() =>
                    DesignConversionTrainingIntakeDpoPairStoreRootResolver
                        .Resolve(
                            artifactRoot,
                            JsonlDesignConversionLocalArtifactDb
                                .TrainingIntakeDpoPairStoreDirectoryName,
                            new DesignConversionTrainingIntakeDpoPairStoreOptions(
                                nativeBase))))
                .ToArray();
            var results = Task.WhenAll(tasks).GetAwaiter().GetResult();

            Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
            Assert.Single(results
                .Select(result => result.Value!.PairStoreRootDirectory)
                .Distinct(StringComparer.Ordinal));
            Assert.Single(results
                .Select(result => result.Value!.ManifestPath)
                .Distinct(StringComparer.Ordinal));
            Assert.True(File.Exists(results[0].Value!.ManifestPath));
            Assert.Empty(Directory.EnumerateFiles(
                artifactRoot,
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ManifestTempFilePrefix + "*.tmp"));
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void FirstStartCollisionAfterNativeRootCreate_AcceptsOnlyExactManifest()
    {
        // Коллизия после создания native root проверяется через повторное
        // открытие manifest-а: exact binding принимается, чужой binding нет.
        var exactArtifactRoot = NewArtifactRoot("pair-store-resolver-exact-collision");
        var exactNativeBase = NewNativeBase("pair-store-resolver-exact-collision-native");
        try
        {
            string? exactNativePairStoreRoot = null;
            var exact = ResolveWithAfterNativeRootCreated(
                exactArtifactRoot,
                exactNativeBase,
                (nativePairStoreRoot, manifestPath, expectedManifestBytes) =>
                {
                    exactNativePairStoreRoot = nativePairStoreRoot;
                    File.WriteAllBytes(manifestPath, expectedManifestBytes);
                });

            Assert.True(exact.IsSuccess, exact.ErrorMessage);
            Assert.NotNull(exact.Value);
            Assert.Equal(exactNativePairStoreRoot, exact.Value!.PairStoreRootDirectory);
            Assert.True(File.Exists(exact.Value.ManifestPath));
        }
        finally
        {
            DeleteDirectoryWithRetry(exactNativeBase);
            DeleteDirectoryWithRetry(exactArtifactRoot);
        }

        var mismatchedArtifactRoot = NewArtifactRoot("pair-store-resolver-mismatch-collision");
        var mismatchedNativeBase = NewNativeBase("pair-store-resolver-mismatch-collision-native");
        try
        {
            string? mismatchedNativePairStoreRoot = null;
            var mismatched = ResolveWithAfterNativeRootCreated(
                mismatchedArtifactRoot,
                mismatchedNativeBase,
                (nativePairStoreRoot, manifestPath, _) =>
                {
                    mismatchedNativePairStoreRoot = nativePairStoreRoot;
                    File.WriteAllText(
                        manifestPath,
                        "{\"schemaVersion\":\"different\"}");
                });

            Assert.False(mismatched.IsSuccess);
            Assert.Equal(
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ManifestMismatchErrorCode,
                mismatched.ErrorCode);
            Assert.True(File.Exists(Path.Combine(
                mismatchedArtifactRoot,
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ManifestFileName)));
            Assert.True(Directory.Exists(mismatchedNativePairStoreRoot));
        }
        finally
        {
            DeleteDirectoryWithRetry(mismatchedNativeBase);
            DeleteDirectoryWithRetry(mismatchedArtifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void ManifestSymlink_FailsClosedBeforeNativeRootSelection()
    {
        // Manifest является authority pointer-ом; symlink на его месте может
        // вывести binding из artifact root, поэтому это fail-closed случай.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-symlink");
        var nativeBase = NewNativeBase("pair-store-resolver-symlink-native");
        var manifestPath = Path.Combine(
            artifactRoot,
            DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .ManifestFileName);
        var target = Path.Combine(nativeBase, "manifest-target.json");
        try
        {
            File.WriteAllText(target, "{}");
            File.CreateSymbolicLink(manifestPath, target);

            var result = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    new DesignConversionTrainingIntakeDpoPairStoreOptions(
                        nativeBase));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ManifestUnavailableErrorCode,
                result.ErrorCode);
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void NativeRootOnCustomNinePDrvfsOrV9fsMount_FailsBeforeCreatingStateTree()
    {
        // Native root запрещён на любом 9p/drvfs/v9fs mount, не только под
        // `/mnt/<drive>`: mountinfo classification важнее path prefix.
        foreach (var fileSystem in new[] { "9p", "drvfs", "v9fs" })
        {
            var artifactRoot = NewArtifactRoot(
                "pair-store-resolver-custom-" + fileSystem);
            var nativeMount = "/custom-" + fileSystem + "-" +
                              Guid.NewGuid().ToString("N");
            var nativeBase = nativeMount + "/state";
            try
            {
                var result = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ResolveForTesting(
                        artifactRoot,
                        JsonlDesignConversionLocalArtifactDb
                            .TrainingIntakeDpoPairStoreDirectoryName,
                        new DesignConversionTrainingIntakeDpoPairStoreOptions(
                            nativeBase),
                        isLinux: true,
                        procVersion: "Linux version 6.9.0 microsoft-standard-WSL2",
                        mountInfoLines: CustomNativeMountInfo(
                            nativeMount,
                            fileSystem),
                        getEnvironmentVariable: _ => null);

                Assert.False(result.IsSuccess);
                Assert.Equal(
                    DesignConversionTrainingIntakeDpoPairStoreRootResolver
                        .NativeRootUnavailableErrorCode,
                    result.ErrorCode);
                Assert.False(Directory.Exists(nativeBase));
            }
            finally
            {
                DeleteDirectoryWithRetry(artifactRoot);
            }
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void RelativeOverride_FailsBeforeNormalization()
    {
        // Относительное значение нельзя сначала расширять через GetFullPath:
        // fail-closed должен увидеть исходный override ввод.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-relative-root");
        try
        {
            var overrideResult = ResolveWithInjectedEnvironment(
                artifactRoot,
                new DesignConversionTrainingIntakeDpoPairStoreOptions(
                    "relative-override"),
                _ => null);

            AssertRelativeRootFailed(overrideResult);
        }
        finally
        {
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void RelativeXdgStateHome_FailsBeforeNormalization()
    {
        // XDG_STATE_HOME — отдельный источник operator input; resolver не
        // должен превращать relative env в текущий абсолютный каталог.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-relative-xdg");
        try
        {
            var xdgResult = ResolveWithInjectedEnvironment(
                artifactRoot,
                options: null,
                name => string.Equals(name, "XDG_STATE_HOME", StringComparison.Ordinal)
                    ? "relative-xdg"
                    : null);

            AssertRelativeRootFailed(xdgResult);
        }
        finally
        {
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void RelativeHome_FailsBeforeNormalization()
    {
        // HOME проверяется до добавления `.local/state`, иначе относительный
        // HOME стал бы похож на валидный state root после GetFullPath.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-relative-home");
        try
        {
            var homeResult = ResolveWithInjectedEnvironment(
                artifactRoot,
                options: null,
                name => string.Equals(name, "HOME", StringComparison.Ordinal)
                    ? "relative-home"
                    : null);

            AssertRelativeRootFailed(homeResult);
        }
        finally
        {
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void NonEmptyLegacyArtifactRoot_FailsMigrationRequired()
    {
        // Старые байты пар под artifact root нельзя разделять с новым native
        // store без миграции: тест ловит именно этот split-brain риск.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-legacy-data");
        var nativeBase = NewNativeBase("pair-store-resolver-legacy-data-native");
        try
        {
            var legacyRoot = Path.Combine(
                artifactRoot,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeDpoPairStoreDirectoryName);
            Directory.CreateDirectory(legacyRoot);
            File.WriteAllText(
                Path.Combine(legacyRoot, "old.dpo-pair.json"),
                "{}");

            var result = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    new DesignConversionTrainingIntakeDpoPairStoreOptions(
                        nativeBase));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .MigrationRequiredErrorCode,
                result.ErrorCode);
            Assert.False(File.Exists(Path.Combine(
                artifactRoot,
                DesignConversionTrainingIntakeDpoPairStoreRootResolver
                    .ManifestFileName)));
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [PairStoreWslWindowsDriveFact]
    public void EmptyLegacyArtifactRoot_AllowsNativeSelectionAndStaysEmpty()
    {
        // Пустой legacy каталог — безопасный след старого запуска; после native
        // выбора он остаётся пустым и не становится вторым журналом пар.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-empty-legacy");
        var nativeBase = NewNativeBase("pair-store-resolver-empty-legacy-native");
        try
        {
            var legacyRoot = Path.Combine(
                artifactRoot,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeDpoPairStoreDirectoryName);
            Directory.CreateDirectory(legacyRoot);

            var result = DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .Resolve(
                    artifactRoot,
                    JsonlDesignConversionLocalArtifactDb
                        .TrainingIntakeDpoPairStoreDirectoryName,
                    new DesignConversionTrainingIntakeDpoPairStoreOptions(
                        nativeBase));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.NotNull(result.Value);
            Assert.True(result.Value!.UsesNativeWslStateRoot);
            Assert.Empty(Directory.EnumerateFileSystemEntries(legacyRoot));
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    [Fact]
    public void NormalLinuxNonWsl_KeepsLegacyPairStoreRoot()
    {
        // Этот срез не меняет обычный Linux: relocation включается только для WSL
        // Windows-drive mount, чтобы не расширять product contract незаметно.
        var artifactRoot = Path.Combine(
            Path.GetTempPath(),
            "pair-store-resolver-normal-linux-" + Guid.NewGuid().ToString("N"));
        var result = DesignConversionTrainingIntakeDpoPairStoreRootResolver
            .ResolveForTesting(
                artifactRoot,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeDpoPairStoreDirectoryName,
                options: null,
                isLinux: true,
                procVersion: "Linux version 6.9.0 generic",
                mountInfoLines:
                [
                    "1 0 8:1 / / rw,relatime - ext4 /dev/sda1 rw"
                ],
                getEnvironmentVariable: _ => null);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        Assert.False(result.Value!.UsesNativeWslStateRoot);
        Assert.Equal(
            Path.Combine(
                Path.GetFullPath(artifactRoot),
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeDpoPairStoreDirectoryName),
            result.Value.PairStoreRootDirectory);
    }

    [PairStoreWslWindowsDriveFact]
    public void ProductConstructor_ExplicitNativeRoot_UsesResolverWithoutInjectedPairStore()
    {
        // Конструктор продукта покрыт отдельно, чтобы зелёная модульная
        // проверка resolver-а не маскировала обход через подменённый pair-store.
        var artifactRoot = NewArtifactRoot("pair-store-resolver-product-ctor");
        var nativeBase = NewNativeBase("pair-store-resolver-product-ctor-native");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(
                artifactRoot,
                trainingIntakeDpoPairStoreOptions:
                new DesignConversionTrainingIntakeDpoPairStoreOptions(
                    nativeBase));

            var resolution = db.TrainingIntakeDpoPairStoreRootResolutionForTesting;
            Assert.True(resolution.UsesNativeWslStateRoot);
            Assert.True(IsUnderDirectory(
                resolution.PairStoreRootDirectory,
                nativeBase));
            Assert.False(IsUnderDirectory(
                resolution.PairStoreRootDirectory,
                artifactRoot));
            Assert.True(File.Exists(resolution.ManifestPath));
        }
        finally
        {
            DeleteDirectoryWithRetry(nativeBase);
            DeleteDirectoryWithRetry(artifactRoot);
        }
    }

    private static string NewArtifactRoot(string scope)
    {
        var root = DesignConversionPostEvaluationEligibilityTestFixture
            .CreateHandleBoundTempRoot(scope);
        Directory.CreateDirectory(root);
        return root;
    }

    private static string NewNativeBase(string scope)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            scope + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root, OwnerDirectoryMode);
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
    }

    private static void AssertOwnerOnlyDirectory(string path)
    {
        Assert.True(OperatingSystem.IsLinux());
        var mode = File.GetUnixFileMode(path);
        Assert.Equal(0, (int)(mode & GroupOrOtherAccess));
        Assert.Equal(
            OwnerDirectoryMode,
            mode & OwnerDirectoryMode);
    }

    private static bool IsUnderDirectory(string candidate, string parent)
    {
        var fullCandidate = EnsureTrailingSeparator(Path.GetFullPath(candidate));
        var fullParent = EnsureTrailingSeparator(Path.GetFullPath(parent));
        return fullCandidate.Length > fullParent.Length &&
               fullCandidate.StartsWith(fullParent, StringComparison.Ordinal);
    }

    private static string EnsureTrailingSeparator(string path)
        => Path.EndsInDirectorySeparator(path)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static OperationResult<
        DesignConversionTrainingIntakeDpoPairStoreRootResolution>
        ResolveWithInjectedEnvironment(
            string artifactRoot,
            DesignConversionTrainingIntakeDpoPairStoreOptions? options,
            Func<string, string?> getEnvironmentVariable,
            Action<string, string, byte[]>? afterNativeRootCreatedForTesting =
                null)
        => DesignConversionTrainingIntakeDpoPairStoreRootResolver
            .ResolveForTesting(
                artifactRoot,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeDpoPairStoreDirectoryName,
                options,
                isLinux: true,
                procVersion: "Linux version 6.9.0 microsoft-standard-WSL2",
                mountInfoLines: File.ReadAllLines("/proc/self/mountinfo"),
                getEnvironmentVariable,
                afterNativeRootCreatedForTesting);

    private static OperationResult<
        DesignConversionTrainingIntakeDpoPairStoreRootResolution>
        ResolveWithAfterNativeRootCreated(
            string artifactRoot,
            string nativeBase,
            Action<string, string, byte[]> afterNativeRootCreated)
        => ResolveWithInjectedEnvironment(
            artifactRoot,
            new DesignConversionTrainingIntakeDpoPairStoreOptions(nativeBase),
            _ => null,
            afterNativeRootCreated);

    private static void AssertManifestBindsNativeDirectoryAndWslIdentity(
        DesignConversionTrainingIntakeDpoPairStoreRootResolution resolution)
    {
        var manifest = ReadManifestObject(resolution.ManifestPath);
        Assert.Equal(
            resolution.NativePairStoreRootDirectory,
            RequiredManifestString(manifest, "nativePairStoreRootDirectory"));
        Assert.Equal(
            resolution.NativePairStoreDirectoryDevice,
            RequiredManifestString(
                manifest,
                "nativePairStoreDirectoryDevice"));
        Assert.Equal(
            resolution.NativePairStoreDirectoryInode,
            RequiredManifestString(
                manifest,
                "nativePairStoreDirectoryInode"));
        Assert.False(string.IsNullOrWhiteSpace(RequiredManifestString(
            manifest,
            "nativePairStoreDirectoryOwnerUserId")));
        Assert.Equal(
            resolution.NativePairStoreDirectoryEffectiveUserId,
            RequiredManifestString(
                manifest,
                "nativePairStoreDirectoryEffectiveUserId"));
        Assert.Equal(
            resolution.NativePairStoreDirectoryFileSystemType,
            RequiredManifestString(
                manifest,
                "nativePairStoreDirectoryFileSystemType"));
        Assert.False(string.IsNullOrWhiteSpace(RequiredManifestString(
            manifest,
            "wslMachineIdSha256")));
        Assert.False(string.IsNullOrWhiteSpace(RequiredManifestString(
            manifest,
            "wslDistroName")));
        Assert.Equal(
            resolution.WslDistroIdentitySha256,
            RequiredManifestString(manifest, "wslDistroIdentitySha256"));
    }

    private static void ReplaceManifestString(
        string manifestPath,
        string propertyName,
        string value)
    {
        var manifest = ReadManifestObject(manifestPath);
        manifest[propertyName] = value;
        File.WriteAllText(
            manifestPath,
            manifest.ToJsonString(DesignConversionCheckpointJsonOptions.Standard));
    }

    private static JsonObject ReadManifestObject(string manifestPath)
        => Assert.IsType<JsonObject>(
            JsonNode.Parse(File.ReadAllText(manifestPath)));

    private static string RequiredManifestString(
        JsonObject manifest,
        string propertyName)
    {
        Assert.True(
            manifest.TryGetPropertyValue(propertyName, out var node),
            "Manifest must contain " + propertyName + ".");
        // Manifest parse может вернуть внутренний JsonValue subtype; важно, что
        // поле остаётся scalar string, а не конкретный runtime класс.
        var value = Assert.IsAssignableFrom<JsonValue>(node);
        return value.GetValue<string>();
    }

    private static void AssertRelativeRootFailed(
        OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>
            result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionTrainingIntakeDpoPairStoreRootResolver
                .NativeRootUnavailableErrorCode,
            result.ErrorCode);
    }

    private static IReadOnlyList<string> CustomNativeMountInfo(
        string nativeMountPoint,
        string nativeFileSystem)
        =>
        [
            "1 0 0:1 / / rw,relatime - ext4 /dev/root rw",
            "2 1 0:2 / /mnt/c rw,relatime - 9p C: rw",
            $"3 1 0:3 / {nativeMountPoint} rw,relatime - {nativeFileSystem} custom-native rw"
        ];

    private static void DeleteDirectoryWithRetry(string root)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 4)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(50);
            }
            catch (UnauthorizedAccessException) when (attempt < 4)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(50);
            }
        }
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
            }
        }
    }
}
