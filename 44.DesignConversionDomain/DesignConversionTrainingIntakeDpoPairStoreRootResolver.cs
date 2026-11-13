// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace DesignConversionDomain;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;

public sealed record DesignConversionTrainingIntakeDpoPairStoreOptions(
    string? NativeStateRootOverride = null)
{
    public static DesignConversionTrainingIntakeDpoPairStoreOptions Default { get; } = new();

    public override string ToString()
        => $"{nameof(DesignConversionTrainingIntakeDpoPairStoreOptions)} {{ " +
           $"{nameof(NativeStateRootOverride)}Configured = {(!string.IsNullOrWhiteSpace(NativeStateRootOverride)).ToString()}, " +
           $"{nameof(NativeStateRootOverride)} = [REDACTED] }}";
}

internal sealed record DesignConversionTrainingIntakeDpoPairStoreRootResolution(
    string PairStoreRootDirectory,
    string LegacyPairStoreRootDirectory,
    string ManifestPath,
    string ArtifactRootCanonicalPath,
    string ArtifactRootIdentity,
    string ArtifactRootFingerprintSha256,
    string NativeStateBaseRootDirectory,
    string NativePairStoreRootDirectory,
    string NativePairStoreFingerprintSha256,
    bool UsesNativeWslStateRoot,
    string NativePairStoreDirectoryDevice = "",
    string NativePairStoreDirectoryInode = "",
    string NativePairStoreDirectoryEffectiveUserId = "",
    string NativePairStoreDirectoryFileSystemType = "",
    string NativeStateRootSource = "",
    string WslDistroIdentitySha256 = "");

internal static class DesignConversionTrainingIntakeDpoPairStoreRootResolver
{
    internal const string ManifestFileName =
        "training-intake-dpo-pair-store-root.manifest.json";
    internal const string ManifestTempFilePrefix =
        ManifestFileName + ".publishing.";
    internal const string ManifestSchemaVersion =
        "design-conversion-training-intake-dpo-pair-store-root-manifest-v1";
    internal const string MigrationRequiredErrorCode =
        "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_PAIR_STORE_MIGRATION_REQUIRED";
    internal const string ManifestMismatchErrorCode =
        "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_PAIR_STORE_MANIFEST_MISMATCH";
    internal const string ManifestUnavailableErrorCode =
        "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_PAIR_STORE_MANIFEST_UNAVAILABLE";
    internal const string NativeRootUnavailableErrorCode =
        "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_PAIR_STORE_NATIVE_ROOT_UNAVAILABLE";

    private const string XdgStateHomeEnvironmentVariable = "XDG_STATE_HOME";
    private const string HomeEnvironmentVariable = "HOME";
    private const int MaxManifestBytes = 64 * 1024;
    private const long LinuxV9FsMagic = 0x01021997;
    private const int LinuxOpenReadOnly = 0;
    private const int LinuxOpenNoFollow = 0x20000;
    private const int LinuxOpenDirectory = 0x10000;
    private const int LinuxOpenCloseOnExec = 0x80000;
    private const uint LinuxFileTypeMask = 0xF000;
    private const uint LinuxDirectoryFileType = 0x4000;
    private const uint LinuxPermissionBitsMask = 0x1FF;
    private const uint LinuxOwnerDirectoryMode = 0x1C0;
    private const uint LinuxGroupOrOtherPermissionMask = 0x3F;

    private static readonly JsonSerializerOptions JsonOptions =
        new(DesignConversionCheckpointJsonOptions.Standard)
        {
            WriteIndented = false
        };

    internal static OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>
        Resolve(
            string artifactRootDirectory,
            string legacyPairStoreDirectoryName,
            DesignConversionTrainingIntakeDpoPairStoreOptions? options = null)
        => Resolve(
            artifactRootDirectory,
            legacyPairStoreDirectoryName,
            options,
            OperatingSystem.IsLinux(),
            ReadProcVersion(),
            ReadMountInfoLines(),
            Environment.GetEnvironmentVariable);

    internal static OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>
        ResolveForTesting(
            string artifactRootDirectory,
            string legacyPairStoreDirectoryName,
            DesignConversionTrainingIntakeDpoPairStoreOptions? options,
            bool isLinux,
            string procVersion,
            IReadOnlyList<string> mountInfoLines,
            Func<string, string?> getEnvironmentVariable,
            Action<string, string, byte[]>? afterNativeRootCreatedForTesting = null)
        => Resolve(
            artifactRootDirectory,
            legacyPairStoreDirectoryName,
            options,
            isLinux,
            procVersion,
            mountInfoLines,
            getEnvironmentVariable,
            afterNativeRootCreatedForTesting);

    private static OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>
        Resolve(
            string artifactRootDirectory,
            string legacyPairStoreDirectoryName,
            DesignConversionTrainingIntakeDpoPairStoreOptions? options,
            bool isLinux,
            string procVersion,
            IReadOnlyList<string> mountInfoLines,
            Func<string, string?> getEnvironmentVariable,
            Action<string, string, byte[]>? afterNativeRootCreatedForTesting = null)
    {
        if (string.IsNullOrWhiteSpace(artifactRootDirectory))
        {
            return OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>.Fail(
                "DesignConversion training-intake DPO pair-store artifact root is required.",
                NativeRootUnavailableErrorCode);
        }
        if (string.IsNullOrWhiteSpace(legacyPairStoreDirectoryName) ||
            legacyPairStoreDirectoryName.IndexOfAny(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) >= 0)
        {
            return OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>.Fail(
                "DesignConversion training-intake DPO pair-store legacy directory name is invalid.",
                NativeRootUnavailableErrorCode);
        }

        var artifactRoot = Path.GetFullPath(artifactRootDirectory);
        var legacyPairStoreRoot = Path.Combine(
            artifactRoot,
            legacyPairStoreDirectoryName);
        var mountEntries = mountInfoLines
            .Select(TryParseMountInfo)
            .Where(static entry => entry is not null)
            .Select(static entry => entry!)
            .ToArray();
        var windowsDriveMount = FindWindowsDriveMount(
            artifactRoot,
            mountEntries);
        if (!isLinux ||
            !IsWslProcess(procVersion, getEnvironmentVariable) ||
            windowsDriveMount is null)
        {
            // Не-WSL и обычный Linux сохраняют старую семантику: этот срез
            // намеренно решает только WSL Windows-drive artifact root.
            return OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>.Ok(
                new DesignConversionTrainingIntakeDpoPairStoreRootResolution(
                    legacyPairStoreRoot,
                    legacyPairStoreRoot,
                    string.Empty,
                    artifactRoot,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    legacyPairStoreRoot,
                    string.Empty,
                    UsesNativeWslStateRoot: false));
        }

        var legacyState = InspectLegacyPairStoreRoot(legacyPairStoreRoot);
        if (!legacyState.IsSuccess)
            return legacyState.MapError<DesignConversionTrainingIntakeDpoPairStoreRootResolution>();
        if (legacyState.Value!.HasExistingData)
        {
            // Старые legacy bytes нельзя молча разделить с новым native state:
            // перед выбором второго root нужна явная миграция.
            return OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>.Fail(
                "DesignConversion training-intake DPO pair-store has existing artifact-root data and requires an explicit migration before using a native WSL state root.",
                MigrationRequiredErrorCode);
        }

        // Порядок root детерминирован и управляется оператором: explicit option,
        // затем XDG state, затем HOME state; production не придумывает `/tmp`.
        var nativeBase = ResolveNativeStateBaseRoot(
            options ?? DesignConversionTrainingIntakeDpoPairStoreOptions.Default,
            getEnvironmentVariable);
        if (!nativeBase.IsSuccess)
            return nativeBase.MapError<DesignConversionTrainingIntakeDpoPairStoreRootResolution>();

        var artifactIdentity = BuildArtifactRootIdentity(
            artifactRoot,
            windowsDriveMount);
        var artifactFingerprint = Sha256Ref(string.Join(
            "\n",
            ManifestSchemaVersion,
            "artifact-root",
            artifactIdentity));
        var nativeStoreFingerprint = Sha256Ref(string.Join(
            "\n",
            ManifestSchemaVersion,
            "native-root",
            artifactFingerprint,
            nativeBase.Value!.RootDirectory));
        // Fingerprint даёт один стабильный native subdirectory на canonical
        // artifact-root identity, чтобы два checkout не делили pair state по имени.
        var nativePairStoreRoot = Path.Combine(
            nativeBase.Value.RootDirectory,
            "xiigen",
            "design-conversion",
            "training-intake-dpo-pairs",
            nativeStoreFingerprint["sha256:".Length..]);

        var manifestPath = Path.Combine(artifactRoot, ManifestFileName);
        var nativeMount = EnsureNativeRootIsNotWindowsDriveFileSystem(
            nativeBase.Value.RootDirectory,
            nativePairStoreRoot,
            mountEntries);
        if (!nativeMount.IsSuccess)
            return nativeMount.MapError<DesignConversionTrainingIntakeDpoPairStoreRootResolution>();

        var wslIdentity = ResolveWslDistroIdentity(getEnvironmentVariable);
        if (!wslIdentity.IsSuccess)
            return wslIdentity.MapError<DesignConversionTrainingIntakeDpoPairStoreRootResolution>();

        var expectedBinding = new ExpectedManifestBinding(
            ArtifactRootCanonicalPath: artifactRoot,
            ArtifactRootIdentity: artifactIdentity,
            ArtifactRootFingerprintSha256: artifactFingerprint,
            LegacyPairStoreRootDirectory: legacyPairStoreRoot,
            NativeStateBaseRootDirectory: nativeBase.Value.RootDirectory,
            NativePairStoreRootDirectory: nativePairStoreRoot,
            NativePairStoreFingerprintSha256: nativeStoreFingerprint,
            WindowsDriveMountPoint: windowsDriveMount.MountPoint,
            WindowsDriveSource: windowsDriveMount.Source,
            WindowsDriveFileSystemType: windowsDriveMount.FileSystemType,
            NativeStateRootSource: nativeBase.Value.Source,
            NativeStateRootConfiguredValueSha256:
                nativeBase.Value.ConfiguredValueSha256,
            WslMachineIdSha256: wslIdentity.Value!.MachineIdSha256,
            WslDistroName: wslIdentity.Value.DistroName,
            WslDistroIdentitySha256:
                wslIdentity.Value.DistroIdentitySha256);

        var existingManifest = TryReadExistingManifest(artifactRoot);
        if (!existingManifest.IsSuccess)
            return existingManifest.MapError<DesignConversionTrainingIntakeDpoPairStoreRootResolution>();
        if (existingManifest.Value is not null)
        {
            // Existing manifest сначала связывается с ожидаемой конфигурацией,
            // и только потом открывается уже существующий native root.
            var existingReady = ValidateExistingManifestAndNativeRoot(
                existingManifest.Value,
                expectedBinding);
            if (!existingReady.IsSuccess)
                return existingReady.MapError<DesignConversionTrainingIntakeDpoPairStoreRootResolution>();

            return CreateResolution(
                nativePairStoreRoot,
                legacyPairStoreRoot,
                manifestPath,
                artifactRoot,
                artifactIdentity,
                artifactFingerprint,
                nativeBase.Value,
                nativeStoreFingerprint,
                existingReady.Value!);
        }

        var rootReady = EnsureNativeRoot(
            nativeBase.Value.RootDirectory,
            nativePairStoreRoot);
        if (!rootReady.IsSuccess)
            return rootReady.MapError<DesignConversionTrainingIntakeDpoPairStoreRootResolution>();

        var manifest = CreateManifest(expectedBinding, rootReady.Value!);
        // Тестовый seam срабатывает после создания native root: так проверяется
        // реальная first-start коллизия manifest-а, а не заранее созданный файл.
        afterNativeRootCreatedForTesting?.Invoke(
            nativePairStoreRoot,
            manifestPath,
            JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
        var manifestReady = EnsureManifestCreatedOrAcceptExactRace(
            artifactRoot,
            manifest);
        if (!manifestReady.IsSuccess)
            return manifestReady.MapError<DesignConversionTrainingIntakeDpoPairStoreRootResolution>();

        var finalReady = ValidateExistingManifestAndNativeRoot(
            manifestReady.Value!,
            expectedBinding);
        if (!finalReady.IsSuccess)
            return finalReady.MapError<DesignConversionTrainingIntakeDpoPairStoreRootResolution>();

        return CreateResolution(
            nativePairStoreRoot,
            legacyPairStoreRoot,
            manifestPath,
            artifactRoot,
            artifactIdentity,
            artifactFingerprint,
            nativeBase.Value,
            nativeStoreFingerprint,
            finalReady.Value!);
    }

    private static OperationResult<LegacyPairStoreState> InspectLegacyPairStoreRoot(
        string legacyPairStoreRoot)
    {
        try
        {
            if (File.Exists(legacyPairStoreRoot) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    legacyPairStoreRoot))
            {
                return OperationResult<LegacyPairStoreState>.Fail(
                    "DesignConversion training-intake DPO pair-store legacy artifact-root path is occupied by unsafe or migrated state.",
                    MigrationRequiredErrorCode);
            }
            if (!Directory.Exists(legacyPairStoreRoot))
                return OperationResult<LegacyPairStoreState>.Ok(new(false));

            // Пустой legacy-каталог допустим как след от старого запуска; любые
            // байты внутри означают риск split-brain, поэтому миграция явная.
            var hasEntries = Directory
                .EnumerateFileSystemEntries(legacyPairStoreRoot)
                .Any();
            return OperationResult<LegacyPairStoreState>.Ok(new(hasEntries));
        }
        catch (Exception exception) when (IsSafeStorageException(exception))
        {
            return OperationResult<LegacyPairStoreState>.Fail(
                "DesignConversion training-intake DPO pair-store legacy artifact-root state could not be inspected safely.",
                MigrationRequiredErrorCode);
        }
    }

    private static OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>
        CreateResolution(
            string nativePairStoreRoot,
            string legacyPairStoreRoot,
            string manifestPath,
            string artifactRoot,
            string artifactIdentity,
            string artifactFingerprint,
            NativeStateBaseRootSelection nativeBase,
            string nativeStoreFingerprint,
            NativeDirectoryIdentity nativeIdentity)
        => OperationResult<DesignConversionTrainingIntakeDpoPairStoreRootResolution>.Ok(
            new DesignConversionTrainingIntakeDpoPairStoreRootResolution(
                nativePairStoreRoot,
                legacyPairStoreRoot,
                manifestPath,
                artifactRoot,
                artifactIdentity,
                artifactFingerprint,
                nativeBase.RootDirectory,
                nativePairStoreRoot,
                nativeStoreFingerprint,
                UsesNativeWslStateRoot: true,
                nativeIdentity.Device,
                nativeIdentity.Inode,
                nativeIdentity.EffectiveUserId,
                nativeIdentity.FileSystemType,
                nativeBase.Source,
                nativeIdentity.WslDistroIdentitySha256));

    private static OperationResult<NativeStateBaseRootSelection> ResolveNativeStateBaseRoot(
        DesignConversionTrainingIntakeDpoPairStoreOptions options,
        Func<string, string?> getEnvironmentVariable)
    {
        string? configured = null;
        string? configuredOriginal = null;
        string source = "unconfigured";
        if (options.NativeStateRootOverride is not null)
        {
            configured = options.NativeStateRootOverride;
            configuredOriginal = options.NativeStateRootOverride;
            source = "NativeStateRootOverride";
        }
        else
        {
            configured = getEnvironmentVariable(XdgStateHomeEnvironmentVariable);
            configuredOriginal = configured;
            source = XdgStateHomeEnvironmentVariable;
            if (string.IsNullOrWhiteSpace(configured))
            {
                var home = getEnvironmentVariable(HomeEnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(home))
                {
                    if (!Path.IsPathFullyQualified(home.Trim()))
                    {
                        return OperationResult<NativeStateBaseRootSelection>.Fail(
                            "DesignConversion training-intake native DPO pair-store HOME root must be absolute before normalization.",
                            NativeRootUnavailableErrorCode);
                    }

                    configured = Path.Combine(home, ".local", "state");
                    configuredOriginal = home;
                    source = HomeEnvironmentVariable + "/.local/state";
                }
            }
        }

        if (string.IsNullOrWhiteSpace(configured))
        {
            return OperationResult<NativeStateBaseRootSelection>.Fail(
                "DesignConversion training-intake native DPO pair-store state root is unavailable.",
                NativeRootUnavailableErrorCode);
        }

        var trimmed = configured.Trim();
        if (!Path.IsPathFullyQualified(trimmed))
        {
            return OperationResult<NativeStateBaseRootSelection>.Fail(
                "DesignConversion training-intake native DPO pair-store state root must be absolute before normalization.",
                NativeRootUnavailableErrorCode);
        }

        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(trimmed));
        return OperationResult<NativeStateBaseRootSelection>.Ok(
            new NativeStateBaseRootSelection(
                fullPath,
                source,
                Sha256Ref(string.Join(
                    "\n",
                    ManifestSchemaVersion,
                    "native-state-root-configured-value",
                    source,
                    configuredOriginal ?? trimmed))));
    }

    private static OperationResult<NativeDirectoryIdentity> EnsureNativeRoot(
        string nativeBaseRoot,
        string nativePairStoreRoot)
    {
        try
        {
            // В WSL пары не пишутся в /mnt/c: v9fs не доказывает нужную
            // Linux owner-only границу для lock/state. Native root проверяется
            // retained handle-ом и режимом 0700 без обещаний приватности на v9fs.
            EnsureNativeDirectory(nativeBaseRoot);
            EnsureNativeDirectory(Path.Combine(nativeBaseRoot, "xiigen"));
            EnsureNativeDirectory(Path.Combine(
                nativeBaseRoot,
                "xiigen",
                "design-conversion"));
            EnsureNativeDirectory(Path.Combine(
                nativeBaseRoot,
                "xiigen",
                "design-conversion",
                "training-intake-dpo-pairs"));
            EnsureNativeDirectory(nativePairStoreRoot);
            return ReadValidatedNativeDirectoryIdentity(nativePairStoreRoot);
        }
        catch (Exception exception) when (IsSafeStorageException(exception))
        {
            return OperationResult<NativeDirectoryIdentity>.Fail(
                "DesignConversion training-intake native DPO pair-store root could not be prepared with an owner-only Linux boundary.",
                NativeRootUnavailableErrorCode);
        }
    }

    private static void EnsureNativeDirectory(string directory)
    {
        if (File.Exists(directory) ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                directory))
        {
            throw new IOException(
                "Native DPO pair-store directory path is occupied or unsafe.");
        }

        try
        {
            using var handleBound = DesignConversionHandleBoundDirectory
                .AcquireOrCreate(directory, beforeIo: null);
            handleBound.ValidateLinuxCustodyRootTrustContract();
        }
        catch (DesignConversionHandleBoundConcurrentCreateException)
        {
            // При первом старте два resolver-а могут одновременно увидеть один
            // сегмент отсутствующим; принимаем только заново открытый каталог с
            // тем же owner-only Linux contract, manifest всё равно сверит identity.
            using var handleBound = DesignConversionHandleBoundDirectory
                .AcquireExisting(directory, beforeIo: null);
            handleBound.ValidateLinuxCustodyRootTrustContract();
        }
    }

    private static OperationResult<TrainingIntakeDpoPairStoreRootManifest?>
        TryReadExistingManifest(string artifactRoot)
    {
        try
        {
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    artifactRoot))
            {
                return OperationResult<TrainingIntakeDpoPairStoreRootManifest?>.Fail(
                    "DesignConversion training-intake DPO pair-store manifest root is unsafe.",
                    ManifestUnavailableErrorCode);
            }

            Directory.CreateDirectory(artifactRoot);
            if (IsExistingReparsePoint(Path.Combine(
                    artifactRoot,
                    ManifestFileName)))
            {
                return OperationResult<TrainingIntakeDpoPairStoreRootManifest?>.Fail(
                    "DesignConversion training-intake DPO pair-store manifest is an unsafe link.",
                    ManifestUnavailableErrorCode);
            }

            using var directory = DesignConversionHandleBoundDirectory
                .AcquireOrCreate(artifactRoot, beforeIo: null);
            using var existing = directory.TryOpenExistingReadOnly(
                ManifestFileName);
            if (existing is null)
                return OperationResult<TrainingIntakeDpoPairStoreRootManifest?>.Ok(null);

            var manifest = ReadManifest(existing);
            return manifest.IsSuccess
                ? OperationResult<TrainingIntakeDpoPairStoreRootManifest?>.Ok(
                    manifest.Value)
                : manifest.MapError<TrainingIntakeDpoPairStoreRootManifest?>();
        }
        catch (Exception exception) when (IsSafeStorageException(exception))
        {
            return OperationResult<TrainingIntakeDpoPairStoreRootManifest?>.Fail(
                "DesignConversion training-intake DPO pair-store manifest could not be read safely.",
                ManifestUnavailableErrorCode);
        }
    }

    private static OperationResult<TrainingIntakeDpoPairStoreRootManifest>
        EnsureManifestCreatedOrAcceptExactRace(
            string artifactRoot,
            TrainingIntakeDpoPairStoreRootManifest manifest)
    {
        try
        {
            var expectedBytes = JsonSerializer.SerializeToUtf8Bytes(
                manifest,
                JsonOptions);
            using var directory = DesignConversionHandleBoundDirectory
                .AcquireOrCreate(artifactRoot, beforeIo: null);
            using var existing = directory.TryOpenExistingReadOnly(
                ManifestFileName);
            if (existing is not null)
            {
                return ExistingManifestExactlyMatches(
                    existing,
                    expectedBytes,
                    manifest);
            }

            var tempLeafName = CreateManifestTempLeafName();
            FileStream? temp = null;
            DesignConversionFileIdentity? tempIdentity = null;
            var tempPublished = false;
            try
            {
                // Manifest публикуется через temp в том же handle-bound root:
                // финальное имя появляется только после полного write+fsync.
                temp = directory.CreateNew(
                    tempLeafName,
                    FileShare.Read,
                    writeThrough: true);
                tempIdentity = directory.GetIdentity(temp, tempLeafName);
                temp.Write(expectedBytes);
                temp.Flush(flushToDisk: true);

                try
                {
                    // No-replace rename оставляет чужой manifest нетронутым;
                    // проигравший resolver принимает только exact bytes.
                    directory.RenameNoReplaceAndFlush(
                        temp,
                        tempLeafName,
                        ManifestFileName);
                    tempPublished = true;
                }
                catch (DesignConversionHandleBoundNameCollisionException)
                {
                    using var raced = directory.TryOpenExistingReadOnly(
                        ManifestFileName);
                    return raced is null
                        ? OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Fail(
                            "DesignConversion training-intake DPO pair-store manifest readback is unavailable.",
                            ManifestUnavailableErrorCode)
                        : ExistingManifestExactlyMatches(
                            raced,
                            expectedBytes,
                            manifest);
                }
            }
            catch (Exception exception) when (IsSafeStorageException(exception))
            {
                return OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Fail(
                    "DesignConversion training-intake DPO pair-store manifest could not be created safely.",
                    ManifestUnavailableErrorCode);
            }
            finally
            {
                if (!tempPublished)
                {
                    TryDeleteOwnManifestTempLeaf(
                        directory,
                        temp,
                        tempLeafName,
                        tempIdentity);
                }
                temp?.Dispose();
            }

            using var readback = directory.TryOpenExistingReadOnly(
                ManifestFileName);
            return readback is null
                ? OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Fail(
                    "DesignConversion training-intake DPO pair-store manifest readback is unavailable.",
                    ManifestUnavailableErrorCode)
                : ExistingManifestExactlyMatches(readback, expectedBytes, manifest);
        }
        catch (Exception exception) when (IsSafeStorageException(exception))
        {
            return OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Fail(
                "DesignConversion training-intake DPO pair-store manifest could not be read or written safely.",
                ManifestUnavailableErrorCode);
        }
    }

    private static string CreateManifestTempLeafName()
        => ManifestTempFilePrefix +
           Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) +
           ".tmp";

    private static void TryDeleteOwnManifestTempLeaf(
        DesignConversionHandleBoundDirectory directory,
        FileStream? temp,
        string tempLeafName,
        DesignConversionFileIdentity? tempIdentity)
    {
        if (temp is null ||
            tempIdentity is null)
            return;

        try
        {
            // Cleanup доказывает identity через удержанный temp handle, поэтому
            // чужой final/root или подменённый leaf не удаляются.
            directory.DeleteHeldRegular(temp, tempLeafName, tempIdentity.Value);
        }
        catch (Exception exception) when (IsSafeStorageException(exception))
        {
            // Crash/cleanup leftovers не становятся authority: resolver читает
            // только canonical manifest name, а temp имена уникальны.
        }
    }

    private static OperationResult<TrainingIntakeDpoPairStoreRootManifest>
        ReadManifest(FileStream existing)
    {
        if (existing.Length <= 0 ||
            existing.Length > MaxManifestBytes)
        {
            return OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Fail(
                "DesignConversion training-intake DPO pair-store manifest does not match the expected native root binding.",
                ManifestMismatchErrorCode);
        }

        var retained = new byte[checked((int)existing.Length)];
        existing.ReadExactly(retained);
        var manifest = JsonSerializer.Deserialize<
            TrainingIntakeDpoPairStoreRootManifest>(
            retained,
            JsonOptions);
        return manifest is null
            ? OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Fail(
                "DesignConversion training-intake DPO pair-store manifest could not be decoded.",
                ManifestMismatchErrorCode)
            : OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Ok(manifest);
    }

    private static OperationResult<TrainingIntakeDpoPairStoreRootManifest>
        ExistingManifestExactlyMatches(
            FileStream existing,
            byte[] expectedBytes,
            TrainingIntakeDpoPairStoreRootManifest expected)
    {
        if (existing.Length <= 0 ||
            existing.Length > MaxManifestBytes ||
            existing.Length != expectedBytes.Length)
        {
            return OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Fail(
                "DesignConversion training-intake DPO pair-store manifest does not match the expected native root binding.",
                ManifestMismatchErrorCode);
        }

        var retained = new byte[expectedBytes.Length];
        existing.ReadExactly(retained);
        if (!retained.AsSpan().SequenceEqual(expectedBytes))
        {
            return OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Fail(
                "DesignConversion training-intake DPO pair-store manifest does not match the expected native root binding.",
                ManifestMismatchErrorCode);
        }

        return OperationResult<TrainingIntakeDpoPairStoreRootManifest>.Ok(expected);
    }

    private static OperationResult<NativeDirectoryIdentity>
        ValidateExistingManifestAndNativeRoot(
            TrainingIntakeDpoPairStoreRootManifest manifest,
            ExpectedManifestBinding expected)
    {
        var binding = ManifestBindingMatches(manifest, expected);
        if (!binding.IsSuccess)
            return binding.MapError<NativeDirectoryIdentity>();
        var manifestIdentity = ReadManifestIdentity(manifest);
        if (!manifestIdentity.IsSuccess)
            return manifestIdentity.MapError<NativeDirectoryIdentity>();
        if (!Directory.Exists(expected.NativePairStoreRootDirectory))
        {
            return OperationResult<NativeDirectoryIdentity>.Fail(
                "DesignConversion training-intake DPO pair-store manifest points to a missing native root.",
                ManifestMismatchErrorCode);
        }

        var liveIdentity = ReadValidatedNativeDirectoryIdentity(
            expected.NativePairStoreRootDirectory);
        if (!liveIdentity.IsSuccess)
            return liveIdentity.MapError<NativeDirectoryIdentity>();

        return NativeIdentityMatches(liveIdentity.Value!, manifestIdentity.Value!)
            ? liveIdentity
            : OperationResult<NativeDirectoryIdentity>.Fail(
                "DesignConversion training-intake DPO pair-store native root identity changed after manifest binding.",
                ManifestMismatchErrorCode);
    }

    private static OperationResult<bool> ManifestBindingMatches(
        TrainingIntakeDpoPairStoreRootManifest manifest,
        ExpectedManifestBinding expected)
    {
        if (!string.Equals(manifest.SchemaVersion, ManifestSchemaVersion, StringComparison.Ordinal) ||
            manifest.Version != 1 ||
            !TextEquals(manifest.ArtifactRootCanonicalPath, expected.ArtifactRootCanonicalPath) ||
            !TextEquals(manifest.ArtifactRootIdentity, expected.ArtifactRootIdentity) ||
            !TextEquals(manifest.ArtifactRootFingerprintSha256, expected.ArtifactRootFingerprintSha256) ||
            !TextEquals(manifest.LegacyPairStoreRootDirectory, expected.LegacyPairStoreRootDirectory) ||
            !TextEquals(manifest.NativeStateBaseRootDirectory, expected.NativeStateBaseRootDirectory) ||
            !TextEquals(manifest.NativePairStoreRootDirectory, expected.NativePairStoreRootDirectory) ||
            !TextEquals(manifest.NativePairStoreFingerprintSha256, expected.NativePairStoreFingerprintSha256) ||
            !TextEquals(manifest.WindowsDriveMountPoint, expected.WindowsDriveMountPoint) ||
            !TextEquals(manifest.WindowsDriveSource, expected.WindowsDriveSource) ||
            !TextEquals(manifest.WindowsDriveFileSystemType, expected.WindowsDriveFileSystemType) ||
            !TextEquals(manifest.NativeStateRootSource, expected.NativeStateRootSource) ||
            !TextEquals(manifest.NativeStateRootConfiguredValueSha256, expected.NativeStateRootConfiguredValueSha256) ||
            !TextEquals(manifest.WslMachineIdSha256, expected.WslMachineIdSha256) ||
            !TextEquals(manifest.WslDistroName, expected.WslDistroName) ||
            !TextEquals(manifest.WslDistroIdentitySha256, expected.WslDistroIdentitySha256) ||
            !TextEquals(manifest.LegacyNonEmptyPolicy, "migration_required_before_native_pair_store_selection") ||
            !TextEquals(manifest.EmptyLegacyDirectoryPolicy, "allowed_only_when_empty_and_never_used_for_pairs_after_native_selection"))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion training-intake DPO pair-store manifest binding does not match the current artifact, native root, WSL distro, or configured source.",
                ManifestMismatchErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static TrainingIntakeDpoPairStoreRootManifest CreateManifest(
        ExpectedManifestBinding expected,
        NativeDirectoryIdentity nativeIdentity)
        => new(
            ManifestSchemaVersion,
            Version: 1,
            ArtifactRootCanonicalPath: expected.ArtifactRootCanonicalPath,
            ArtifactRootIdentity: expected.ArtifactRootIdentity,
            ArtifactRootFingerprintSha256: expected.ArtifactRootFingerprintSha256,
            LegacyPairStoreRootDirectory: expected.LegacyPairStoreRootDirectory,
            NativeStateBaseRootDirectory: expected.NativeStateBaseRootDirectory,
            NativePairStoreRootDirectory: expected.NativePairStoreRootDirectory,
            NativePairStoreFingerprintSha256:
                expected.NativePairStoreFingerprintSha256,
            WindowsDriveMountPoint: expected.WindowsDriveMountPoint,
            WindowsDriveSource: expected.WindowsDriveSource,
            WindowsDriveFileSystemType: expected.WindowsDriveFileSystemType,
            NativeStateRootSource: expected.NativeStateRootSource,
            NativeStateRootConfiguredValueSha256:
                expected.NativeStateRootConfiguredValueSha256,
            WslMachineIdSha256: expected.WslMachineIdSha256,
            WslDistroName: expected.WslDistroName,
            WslDistroIdentitySha256: expected.WslDistroIdentitySha256,
            NativePairStoreDirectoryDevice: nativeIdentity.Device,
            NativePairStoreDirectoryInode: nativeIdentity.Inode,
            NativePairStoreDirectoryOwnerUserId: nativeIdentity.OwnerUserId,
            NativePairStoreDirectoryEffectiveUserId:
                nativeIdentity.EffectiveUserId,
            NativePairStoreDirectoryFileSystemType:
                nativeIdentity.FileSystemType,
            LegacyNonEmptyPolicy:
                "migration_required_before_native_pair_store_selection",
            EmptyLegacyDirectoryPolicy:
                "allowed_only_when_empty_and_never_used_for_pairs_after_native_selection");

    private static OperationResult<NativeDirectoryIdentity> ReadManifestIdentity(
        TrainingIntakeDpoPairStoreRootManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.NativePairStoreDirectoryDevice) ||
            string.IsNullOrWhiteSpace(manifest.NativePairStoreDirectoryInode) ||
            string.IsNullOrWhiteSpace(manifest.NativePairStoreDirectoryOwnerUserId) ||
            string.IsNullOrWhiteSpace(manifest.NativePairStoreDirectoryEffectiveUserId) ||
            string.IsNullOrWhiteSpace(manifest.NativePairStoreDirectoryFileSystemType) ||
            string.IsNullOrWhiteSpace(manifest.WslDistroIdentitySha256))
        {
            return OperationResult<NativeDirectoryIdentity>.Fail(
                "DesignConversion training-intake DPO pair-store manifest is missing native root identity.",
                ManifestMismatchErrorCode);
        }

        return OperationResult<NativeDirectoryIdentity>.Ok(
            new NativeDirectoryIdentity(
                manifest.NativePairStoreDirectoryDevice,
                manifest.NativePairStoreDirectoryInode,
                manifest.NativePairStoreDirectoryOwnerUserId,
                manifest.NativePairStoreDirectoryEffectiveUserId,
                manifest.NativePairStoreDirectoryFileSystemType,
                manifest.WslDistroIdentitySha256));
    }

    private static bool NativeIdentityMatches(
        NativeDirectoryIdentity left,
        NativeDirectoryIdentity right)
        => TextEquals(left.Device, right.Device) &&
           TextEquals(left.Inode, right.Inode) &&
           TextEquals(left.OwnerUserId, right.OwnerUserId) &&
           TextEquals(left.EffectiveUserId, right.EffectiveUserId) &&
           TextEquals(left.FileSystemType, right.FileSystemType) &&
           TextEquals(
               left.WslDistroIdentitySha256,
               right.WslDistroIdentitySha256);

    private static bool TextEquals(string? left, string? right)
        => string.Equals(left, right, StringComparison.Ordinal);

    private static OperationResult<bool> EnsureNativeRootIsNotWindowsDriveFileSystem(
        string nativeBaseRoot,
        string nativePairStoreRoot,
        IReadOnlyList<LinuxMountInfoEntry> mountEntries)
    {
        var baseMount = FindBestMount(nativeBaseRoot, mountEntries);
        var pairMount = FindBestMount(nativePairStoreRoot, mountEntries);
        if ((baseMount is not null && IsWindowsDriveFileSystem(baseMount)) ||
            (pairMount is not null && IsWindowsDriveFileSystem(pairMount)))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion training-intake native DPO pair-store root cannot live on a 9p, drvfs, or v9fs mount.",
                NativeRootUnavailableErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<NativeDirectoryIdentity>
        ReadValidatedNativeDirectoryIdentity(string nativePairStoreRoot)
    {
        try
        {
            if (!OperatingSystem.IsLinux())
            {
                return OperationResult<NativeDirectoryIdentity>.Fail(
                    "DesignConversion training-intake native DPO pair-store identity is available only on Linux.",
                    NativeRootUnavailableErrorCode);
            }

            var fileDescriptor = OpenLinux(
                nativePairStoreRoot,
                LinuxOpenReadOnly |
                LinuxOpenNoFollow |
                LinuxOpenDirectory |
                LinuxOpenCloseOnExec,
                mode: 0);
            if (fileDescriptor < 0)
            {
                return OperationResult<NativeDirectoryIdentity>.Fail(
                    "DesignConversion training-intake native DPO pair-store root could not be opened for identity validation.",
                    NativeRootUnavailableErrorCode);
            }

            try
            {
                if (FStatLinux(fileDescriptor, out var stat) != 0 ||
                    FStatFsLinux(fileDescriptor, out var statFs) != 0)
                {
                    return OperationResult<NativeDirectoryIdentity>.Fail(
                        "DesignConversion training-intake native DPO pair-store root identity could not be read.",
                        NativeRootUnavailableErrorCode);
                }

                var effectiveUserId = GetEffectiveUserIdLinux();
                if ((stat.Mode & LinuxFileTypeMask) != LinuxDirectoryFileType ||
                    stat.Uid != effectiveUserId ||
                    (stat.Mode & LinuxGroupOrOtherPermissionMask) != 0 ||
                    (stat.Mode & LinuxOwnerDirectoryMode) != LinuxOwnerDirectoryMode ||
                    (stat.Mode & LinuxPermissionBitsMask) != LinuxOwnerDirectoryMode ||
                    statFs.Type == LinuxV9FsMagic)
                {
                    return OperationResult<NativeDirectoryIdentity>.Fail(
                        "DesignConversion training-intake native DPO pair-store root failed owner-only Linux identity validation.",
                        NativeRootUnavailableErrorCode);
                }

                var wslIdentity = ResolveWslDistroIdentity(
                    Environment.GetEnvironmentVariable);
                if (!wslIdentity.IsSuccess)
                    return wslIdentity.MapError<NativeDirectoryIdentity>();

                return OperationResult<NativeDirectoryIdentity>.Ok(
                    new NativeDirectoryIdentity(
                        stat.Device.ToString(CultureInfo.InvariantCulture),
                        stat.Inode.ToString(CultureInfo.InvariantCulture),
                        stat.Uid.ToString(CultureInfo.InvariantCulture),
                        effectiveUserId.ToString(CultureInfo.InvariantCulture),
                        "0x" + statFs.Type.ToString("x", CultureInfo.InvariantCulture),
                        wslIdentity.Value!.DistroIdentitySha256));
            }
            finally
            {
                _ = CloseLinux(fileDescriptor);
            }
        }
        catch (Exception exception) when (IsSafeInteropException(exception))
        {
            return OperationResult<NativeDirectoryIdentity>.Fail(
                "DesignConversion training-intake native DPO pair-store root identity could not be validated safely.",
                NativeRootUnavailableErrorCode);
        }
    }

    private static OperationResult<WslDistroIdentity> ResolveWslDistroIdentity(
        Func<string, string?> getEnvironmentVariable)
    {
        var machineId = ReadFirstMachineId();
        if (string.IsNullOrWhiteSpace(machineId))
        {
            return OperationResult<WslDistroIdentity>.Fail(
                "DesignConversion training-intake DPO pair-store requires /etc/machine-id for WSL distro identity.",
                NativeRootUnavailableErrorCode);
        }

        var distroName = ReadDistroName() ??
                         getEnvironmentVariable("WSL_DISTRO_NAME");
        if (string.IsNullOrWhiteSpace(distroName))
        {
            return OperationResult<WslDistroIdentity>.Fail(
                "DesignConversion training-intake DPO pair-store requires a human WSL distro name in addition to machine-id.",
                NativeRootUnavailableErrorCode);
        }

        var safeDistroName = distroName.Trim();
        var machineIdSha = Sha256Ref(string.Join(
            "\n",
            ManifestSchemaVersion,
            "wsl-machine-id",
            machineId.Trim()));
        var distroIdentitySha = Sha256Ref(string.Join(
            "\n",
            ManifestSchemaVersion,
            "wsl-distro-identity",
            machineIdSha,
            safeDistroName));
        return OperationResult<WslDistroIdentity>.Ok(
            new WslDistroIdentity(
                machineIdSha,
                safeDistroName,
                distroIdentitySha));
    }

    private static string? ReadFirstMachineId()
        => ReadSmallTextFile("/etc/machine-id") ??
           ReadSmallTextFile("/var/lib/dbus/machine-id");

    private static string? ReadDistroName()
    {
        var osRelease = ReadSmallTextFile("/etc/os-release");
        if (string.IsNullOrWhiteSpace(osRelease))
            return null;

        foreach (var line in osRelease.Split('\n'))
        {
            if (line.StartsWith("PRETTY_NAME=", StringComparison.Ordinal))
                return DecodeOsReleaseValue(line["PRETTY_NAME=".Length..]);
            if (line.StartsWith("NAME=", StringComparison.Ordinal))
                return DecodeOsReleaseValue(line["NAME=".Length..]);
        }

        return null;
    }

    private static string DecodeOsReleaseValue(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length >= 2 &&
               trimmed[0] == '"' &&
               trimmed[^1] == '"'
            ? trimmed[1..^1]
            : trimmed;
    }

    private static string? ReadSmallTextFile(string path)
    {
        try
        {
            if (!File.Exists(path))
                return null;
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize: 4096,
                FileOptions.SequentialScan);
            if (stream.Length <= 0 || stream.Length > 4096)
                return null;
            using var reader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 4096,
                leaveOpen: false);
            return reader.ReadToEnd().Trim();
        }
        catch (Exception exception) when (IsSafeStorageException(exception))
        {
            return null;
        }
    }

    private static bool IsWslProcess(
        string procVersion,
        Func<string, string?> getEnvironmentVariable)
        => procVersion.Contains("microsoft", StringComparison.OrdinalIgnoreCase) ||
           !string.IsNullOrWhiteSpace(getEnvironmentVariable("WSL_DISTRO_NAME")) ||
           !string.IsNullOrWhiteSpace(getEnvironmentVariable("WSL_INTEROP"));

    private static LinuxMountInfoEntry? FindWindowsDriveMount(
        string candidatePath,
        IReadOnlyList<LinuxMountInfoEntry> mountEntries)
    {
        var best = mountEntries
            .Where(entry =>
                IsSingleDriveMountPoint(entry.MountPoint) &&
                IsWindowsDriveFileSystem(entry) &&
                IsUnderOrEqualMountPoint(candidatePath, entry.MountPoint))
            .OrderByDescending(static entry => entry.MountPoint.Length)
            .FirstOrDefault();
        return best;
    }

    private static LinuxMountInfoEntry? FindBestMount(
        string candidatePath,
        IReadOnlyList<LinuxMountInfoEntry> mountEntries)
        => mountEntries
            .Where(entry => IsUnderOrEqualMountPoint(candidatePath, entry.MountPoint))
            .OrderByDescending(static entry => entry.MountPoint.Length)
            .FirstOrDefault();

    private static bool IsSingleDriveMountPoint(string mountPoint)
    {
        var segments = SplitAbsoluteLinuxPath(mountPoint);
        return segments.Length == 2 &&
               string.Equals(segments[0], "mnt", StringComparison.Ordinal) &&
               segments[1].Length == 1 &&
               char.IsAsciiLetter(segments[1][0]);
    }

    private static bool IsWindowsDriveFileSystem(LinuxMountInfoEntry entry)
        => string.Equals(entry.FileSystemType, "9p", StringComparison.Ordinal) ||
           string.Equals(entry.FileSystemType, "drvfs", StringComparison.Ordinal) ||
           string.Equals(entry.FileSystemType, "v9fs", StringComparison.Ordinal);

    private static bool IsUnderOrEqualMountPoint(
        string candidatePath,
        string mountPoint)
    {
        var candidateSegments = SplitAbsoluteLinuxPath(candidatePath);
        var mountSegments = SplitAbsoluteLinuxPath(mountPoint);
        if (mountSegments.Length == 0 ||
            candidateSegments.Length < mountSegments.Length)
        {
            return false;
        }

        for (var index = 0; index < mountSegments.Length; index++)
        {
            if (!string.Equals(
                    candidateSegments[index],
                    mountSegments[index],
                    StringComparison.Ordinal))
            {
                return false;
            }
        }
        return true;
    }

    private static string BuildArtifactRootIdentity(
        string artifactRoot,
        LinuxMountInfoEntry windowsDriveMount)
    {
        var drive = windowsDriveMount.MountPoint.TrimEnd('/')[^1];
        var relativeSegments = SplitAbsoluteLinuxPath(artifactRoot)
            .Skip(SplitAbsoluteLinuxPath(windowsDriveMount.MountPoint).Length);
        return string.Concat(
            char.ToUpperInvariant(drive),
            ":/",
            string.Join("/", relativeSegments));
    }

    private static string[] SplitAbsoluteLinuxPath(string path)
        => path
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

    private static LinuxMountInfoEntry? TryParseMountInfo(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var separatorIndex = Array.IndexOf(parts, "-");
        if (separatorIndex < 0 || parts.Length <= separatorIndex + 2 ||
            parts.Length <= 4)
        {
            return null;
        }

        return new LinuxMountInfoEntry(
            DecodeMountInfoField(parts[4]),
            parts[separatorIndex + 1],
            DecodeMountInfoField(parts[separatorIndex + 2]));
    }

    private static string DecodeMountInfoField(string value)
    {
        var builder = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] == '\\' &&
                index + 3 < value.Length &&
                IsOctalDigit(value[index + 1]) &&
                IsOctalDigit(value[index + 2]) &&
                IsOctalDigit(value[index + 3]))
            {
                var decoded =
                    ((value[index + 1] - '0') << 6) +
                    ((value[index + 2] - '0') << 3) +
                    (value[index + 3] - '0');
                builder.Append((char)decoded);
                index += 3;
            }
            else
            {
                builder.Append(value[index]);
            }
        }
        return builder.ToString();
    }

    private static bool IsOctalDigit(char value)
        => value is >= '0' and <= '7';

    private static bool IsExistingReparsePoint(string path)
    {
        try
        {
            return File.Exists(path) &&
                   (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
        }
        catch (Exception exception) when (IsSafeStorageException(exception))
        {
            return true;
        }
    }

    private static string ReadProcVersion()
    {
        try
        {
            return File.Exists("/proc/version")
                ? File.ReadAllText("/proc/version")
                : string.Empty;
        }
        catch (Exception exception) when (IsSafeStorageException(exception))
        {
            return string.Empty;
        }
    }

    private static IReadOnlyList<string> ReadMountInfoLines()
    {
        try
        {
            return File.Exists("/proc/self/mountinfo")
                ? File.ReadAllLines("/proc/self/mountinfo")
                : [];
        }
        catch (Exception exception) when (IsSafeStorageException(exception))
        {
            return [];
        }
    }

    private static string Sha256Ref(string value)
        => "sha256:" + Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static bool IsSafeStorageException(Exception exception)
        => exception is IOException or UnauthorizedAccessException or
            SecurityException or InvalidDataException or
            PlatformNotSupportedException or ArgumentException or JsonException;

    private static bool IsSafeInteropException(Exception exception)
        => IsSafeStorageException(exception) ||
           exception is EntryPointNotFoundException or DllNotFoundException or
           BadImageFormatException;

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int OpenLinux(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int CloseLinux(int fileDescriptor);

    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int FStatLinux(
        int fileDescriptor,
        out LinuxStat stat);

    [DllImport("libc", EntryPoint = "fstatfs", SetLastError = true)]
    private static extern int FStatFsLinux(
        int fileDescriptor,
        out LinuxStatFs stat);

    [DllImport("libc", EntryPoint = "geteuid")]
    private static extern uint GetEffectiveUserIdLinux();

    [StructLayout(LayoutKind.Sequential)]
    private struct LinuxStat
    {
        public ulong Device;
        public ulong Inode;
        public ulong LinkCount;
        public uint Mode;
        public uint Uid;
        public uint Gid;
        public int Padding0;
        public ulong DeviceId;
        public long Size;
        public long BlockSize;
        public long Blocks;
        public long AccessTimeSeconds;
        public long AccessTimeNanoseconds;
        public long ModificationTimeSeconds;
        public long ModificationTimeNanoseconds;
        public long ChangeTimeSeconds;
        public long ChangeTimeNanoseconds;
        public long Unused0;
        public long Unused1;
        public long Unused2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LinuxStatFs
    {
        public long Type;
        public long BlockSize;
        public ulong Blocks;
        public ulong BlocksFree;
        public ulong BlocksAvailable;
        public ulong Files;
        public ulong FilesFree;
        public int FileSystemId0;
        public int FileSystemId1;
        public long NameLength;
        public long FragmentSize;
        public long Flags;
        public long Spare0;
        public long Spare1;
        public long Spare2;
        public long Spare3;
    }

    private sealed record LegacyPairStoreState(bool HasExistingData);

    private sealed record NativeStateBaseRootSelection(
        string RootDirectory,
        string Source,
        string ConfiguredValueSha256);

    private sealed record WslDistroIdentity(
        string MachineIdSha256,
        string DistroName,
        string DistroIdentitySha256);

    private sealed record NativeDirectoryIdentity(
        string Device,
        string Inode,
        string OwnerUserId,
        string EffectiveUserId,
        string FileSystemType,
        string WslDistroIdentitySha256);

    private sealed record ExpectedManifestBinding(
        string ArtifactRootCanonicalPath,
        string ArtifactRootIdentity,
        string ArtifactRootFingerprintSha256,
        string LegacyPairStoreRootDirectory,
        string NativeStateBaseRootDirectory,
        string NativePairStoreRootDirectory,
        string NativePairStoreFingerprintSha256,
        string WindowsDriveMountPoint,
        string WindowsDriveSource,
        string WindowsDriveFileSystemType,
        string NativeStateRootSource,
        string NativeStateRootConfiguredValueSha256,
        string WslMachineIdSha256,
        string WslDistroName,
        string WslDistroIdentitySha256);

    private sealed record LinuxMountInfoEntry(
        string MountPoint,
        string FileSystemType,
        string Source);

    private sealed record TrainingIntakeDpoPairStoreRootManifest(
        string SchemaVersion,
        int Version,
        string ArtifactRootCanonicalPath,
        string ArtifactRootIdentity,
        string ArtifactRootFingerprintSha256,
        string LegacyPairStoreRootDirectory,
        string NativeStateBaseRootDirectory,
        string NativePairStoreRootDirectory,
        string NativePairStoreFingerprintSha256,
        string WindowsDriveMountPoint,
        string WindowsDriveSource,
        string WindowsDriveFileSystemType,
        string NativeStateRootSource,
        string NativeStateRootConfiguredValueSha256,
        string WslMachineIdSha256,
        string WslDistroName,
        string WslDistroIdentitySha256,
        string NativePairStoreDirectoryDevice,
        string NativePairStoreDirectoryInode,
        string NativePairStoreDirectoryOwnerUserId,
        string NativePairStoreDirectoryEffectiveUserId,
        string NativePairStoreDirectoryFileSystemType,
        string LegacyNonEmptyPolicy,
        string EmptyLegacyDirectoryPolicy);
}
