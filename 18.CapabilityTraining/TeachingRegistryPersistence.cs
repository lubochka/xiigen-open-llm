// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using AgentInfrastructure;
using LoRA;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

internal interface ITeachingRegistryArtifactLoadObserver
{
    void OnArtifactsVerified();
}

// This class persists and restores cumulative teaching registry state between rows.
public static class TeachingRegistryPersistence
{
    private sealed class ManifestPublicationCleanupArbiter
    {
        private const int MaximumCleanupFailureCount = 8;
        private const string CleanupFailureDataKey =
            "GENERATED_CAPABILITY_MANIFEST_CLEANUP_FAILURE";
        private readonly Exception?[] _cleanupFailures =
            new Exception?[MaximumCleanupFailureCount];
        private int _cleanupFailureCount;
        private Exception? _originalFailure;
        private Exception? _firstCleanupFailure;
        private Exception? _firstFatalCleanupFailure;

        internal bool HasCleanupFailure => _firstCleanupFailure is not null;
        internal void CaptureOriginal(Exception exception)
            => _originalFailure ??= exception;
        internal void CaptureCleanup(Exception exception)
        {
            _firstCleanupFailure ??= exception;
            if (_firstFatalCleanupFailure is null &&
                IsFatalConservatively(exception))
            {
                _firstFatalCleanupFailure = exception;
            }
            for (var index = 0; index < _cleanupFailureCount; index++)
            {
                if (ReferenceEquals(_cleanupFailures[index], exception))
                    return;
            }
            if (_cleanupFailureCount < _cleanupFailures.Length)
                _cleanupFailures[_cleanupFailureCount++] = exception;
        }

        internal Exception? Resolve()
        {
            var originalFatal = _originalFailure is not null &&
                IsFatalConservatively(_originalFailure);
            var winner = originalFatal
                ? _originalFailure
                : _firstFatalCleanupFailure ??
                  _originalFailure ??
                  _firstCleanupFailure;
            if (winner is null)
                return null;

            try
            {
                var attached = new List<Exception>();
                var seen = new HashSet<Exception>(
                    ReferenceEqualityComparer.Instance) { winner };
                AddExistingAttachment(winner, attached, seen);
                AddFlattened(_originalFailure, attached, seen);
                for (var index = 0; index < _cleanupFailureCount; index++)
                    AddFlattened(_cleanupFailures[index], attached, seen);
                if (attached.Count > 0)
                    winner.Data[CleanupFailureDataKey] = attached.Count == 1
                        ? attached[0]
                        : new AggregateException(attached);
            }
            catch { }
            return winner;
        }

        internal static bool IsFatalConservatively(Exception exception)
        {
            try
            {
                return !SnapshotExceptionGraphClassifier.IsNonFatal(exception);
            }
            catch
            {
                return true;
            }
        }

        private static void AddExistingAttachment(
            Exception exception,
            List<Exception> attached,
            HashSet<Exception> seen)
        {
            try
            {
                if (exception.Data[CleanupFailureDataKey] is Exception existing)
                    AddFlattened(existing, attached, seen);
            }
            catch { }
        }

        private static void AddFlattened(
            Exception? exception,
            List<Exception> attached,
            HashSet<Exception> seen)
        {
            if (exception is null || !seen.Add(exception))
                return;
            if (exception is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    AddFlattened(inner, attached, seen);
                AddExistingAttachment(aggregate, attached, seen);
                return;
            }
            attached.Add(exception);
            AddExistingAttachment(exception, attached, seen);
        }
    }

    private static readonly AsyncLocal<Action<string>?>
        ManifestAfterReadBeforeIdentityReopenObserver = new();
    internal static Action<string>?
        ManifestAfterReadBeforeIdentityReopenObserverForTesting
    {
        get => ManifestAfterReadBeforeIdentityReopenObserver.Value;
        set => ManifestAfterReadBeforeIdentityReopenObserver.Value = value;
    }
    private static readonly AsyncLocal<Action<string, string>?>
        ManifestBeforeHandleRenameObserver = new();
    internal static Action<string, string>?
        ManifestBeforeHandleRenameObserverForTesting
    {
        get => ManifestBeforeHandleRenameObserver.Value;
        set => ManifestBeforeHandleRenameObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        RestorePublicationLeaseAcquiredObserver = new();
    internal static Action?
        RestorePublicationLeaseAcquiredObserverForTesting
    {
        get => RestorePublicationLeaseAcquiredObserver.Value;
        set => RestorePublicationLeaseAcquiredObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        ManifestPayloadAllocationObserver = new();
    internal static Action?
        ManifestPayloadAllocationObserverForTesting
    {
        get => ManifestPayloadAllocationObserver.Value;
        set => ManifestPayloadAllocationObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        ManifestReadPayloadAllocationObserver = new();
    internal static Action?
        ManifestReadPayloadAllocationObserverForTesting
    {
        get => ManifestReadPayloadAllocationObserver.Value;
        set => ManifestReadPayloadAllocationObserver.Value = value;
    }
    internal const int MaximumManifestDirectoryCount = 4096;
    internal const int MaximumManifestCount = 4096;
    internal const int MaximumManifestTraversalDepth = 32;
    internal const int MaximumManifestEntryCount = 16384;
    internal const long MaximumManifestBytes = 1_048_576;
    internal const long MaximumManifestInventoryBytes = 64L * 1_048_576;
    internal const int MaximumManifestJsonTokenCount = 32768;
    internal const int MaximumManifestJsonDepth = 64;
    internal const int MaximumManifestStringLength = 16384;
    internal const int MaximumManifestArrayCount = 256;
    private static readonly TimeSpan RegistryPublicationLeaseTimeout =
        TimeSpan.FromSeconds(30);
    private const int MaximumManifestPublicationPathLength = 32767;
    private const int MaximumManifestPublicationComponentLength = 255;
    private const uint ManifestDirectoryReadAccess =
        0x00100000 | 0x00000020 | 0x00000080;
    private const uint ManifestDirectoryAddFileAccess = 0x00000002;
    private const uint ManifestDirectoryAddSubdirectoryAccess = 0x00000004;
    private const uint ManifestShareReadWriteDenyDelete =
        0x00000001 | 0x00000002;
    private const uint ManifestShareAll =
        0x00000001 | 0x00000002 | 0x00000004;
    private const uint ManifestDirectoryOpenOptions =
        0x00000001 | 0x00000020 | 0x00200000;
    public const string GeneratedCapabilityManifestFileName = "generated_capability_manifest.json";
    public const string GeneratedCapabilityManifestIdentityCustodyRequiredErrorCode =
        "GENERATED_CAPABILITY_MANIFEST_WINDOWS_IDENTITY_CUSTODY_REQUIRED";
    public const string GeneratedCapabilityExecutableManifestInvalidErrorCode =
        "GENERATED_CAPABILITY_EXECUTABLE_MANIFEST_INVALID";
    public const string GeneratedCapabilityReplayVerifierRequiredErrorCode =
        "GENERATED_CAPABILITY_REPLAY_VERIFIER_REQUIRED";
    public const string GeneratedCapabilityReplayAttestationInvalidErrorCode =
        "GENERATED_CAPABILITY_REPLAY_ATTESTATION_INVALID";

    internal static void WriteGeneratedCapabilityManifest(
        GeneratedCapabilityCheckpointManifest manifest,
        Stream destination)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(destination);
        var payload = SerializeBoundedGeneratedCapabilityManifest(manifest);
        destination.Write(payload, 0, payload.Length);
    }

    private static byte[] SerializeBoundedGeneratedCapabilityManifest(
        GeneratedCapabilityCheckpointManifest manifest)
    {
        if (!TrySnapshotManifest(manifest, out var ownedManifest))
            throw new InvalidOperationException(
                "Generated capability manifest exceeds its bounds.");

        // Count the exact UTF-8 JSON representation, including escaping and
        // structural bytes, into a discarding bounded stream before asking the
        // serializer to allocate the complete payload.
        using (var counter = new BoundedCountingWriteStream(
                   MaximumManifestBytes))
        {
            JsonSerializer.Serialize(counter, ownedManifest, JsonOptions);
        }

        ManifestPayloadAllocationObserver.Value?.Invoke();
        var payload = JsonSerializer.SerializeToUtf8Bytes(
            ownedManifest,
            JsonOptions);
        if (payload.LongLength > MaximumManifestBytes)
            throw new InvalidOperationException(
                "Generated capability manifest exceeds its serialized bound.");
        return payload;
    }

    private sealed class BoundedCountingWriteStream(long maximumLength) : Stream
    {
        private long _length;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _length;
        public override long Position
        {
            get => _length;
            set => throw new NotSupportedException();
        }

        public override void Flush() { }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (offset > buffer.Length - count)
                throw new ArgumentException("The write range is invalid.");
            Add(count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
            => Add(buffer.Length);

        public override void WriteByte(byte value)
        {
            _ = value;
            Add(1);
        }

        private void Add(int count)
        {
            if (count > maximumLength - _length)
                throw new InvalidOperationException(
                    "Generated capability manifest exceeds its serialized bound.");
            _length += count;
        }

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();
        public override void SetLength(long value)
            => throw new NotSupportedException();
    }
    private const string GeneratedCapabilityCaptureInvalid = "GENERATED_CAPABILITY_CAPTURE_INVALID";
    // Этим кодом сохранение честно отказывает модулю с настроенным каталогом правил ранжирования:
    // сериализуемого договора для такого каталога ещё нет, поэтому снимок нельзя объявить восстановимым.
    private const string UnsupportedPrincipleCatalogueMode = "UNSUPPORTED_PRINCIPLE_CATALOGUE_MODE";
    private const string GeneratedCapabilityRestoreInvalid = "GENERATED_CAPABILITY_RESTORE_INVALID";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
    private static readonly JsonSerializerOptions BoundedReadJsonOptions = new(JsonOptions)
    {
        MaxDepth = MaximumManifestJsonDepth,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    internal sealed record GeneratedCapabilityManifestSource(
        int SourceOrdinal,
        string CanonicalPath,
        GeneratedCapabilityCheckpointManifest Manifest);

    internal sealed record GeneratedCapabilityManifestInventory(
        GeneratedCapabilityManifestSource[] Sources,
        IReadOnlyDictionary<string, GeneratedCapabilityCheckpointManifest> ByUnitId);

    // Capture registry descriptors and generated capability manifests into one snapshot.
    public static OperationResult<TeachingRegistrySnapshot> Capture(
        string bundleId,
        int rowIndex,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        string checkpointRoot,
        string? snapshotId = null)
    {
        if (units is null)
            return OperationResult<TeachingRegistrySnapshot>.Fail("Unit registry is required.", "UNIT_REGISTRY_REQUIRED");
        if (flows is null)
            return OperationResult<TeachingRegistrySnapshot>.Fail("Flow registry is required.", "FLOW_REGISTRY_REQUIRED");
        if (string.IsNullOrWhiteSpace(bundleId))
            return OperationResult<TeachingRegistrySnapshot>.Fail("Bundle id is required.", "BUNDLE_ID_REQUIRED");

        var inventory = ReadGeneratedCapabilityManifestInventory(checkpointRoot);
        if (!inventory.IsSuccess)
            return OperationResult<TeachingRegistrySnapshot>.Fail(inventory.ErrorMessage, inventory.ErrorCode);

        try
        {
            var capturedUnits = CaptureUnitEntries(units, inventory.Value!.ByUnitId);
            if (!capturedUnits.IsSuccess)
                return OperationResult<TeachingRegistrySnapshot>.Fail(capturedUnits.ErrorMessage, capturedUnits.ErrorCode);

            var manifests = inventory.Value!.Sources
                .Select(source => source.Manifest)
                .ToArray();
            var capturedFlows = flows.ListDescriptors()
                .Select(ToSnapshotEntry)
                .ToArray();
            if (!HasCompleteGeneratedCapabilityBindings(
                    capturedUnits.Value!,
                    capturedFlows,
                    manifests))
            {
                return OperationResult<TeachingRegistrySnapshot>.Fail(
                    "Generated capability registry tuple is invalid.",
                    GeneratedCapabilityCaptureInvalid);
            }
            var checkpointPaths = manifests
                .SelectMany(manifest => new[] { manifest.PolicyCheckpointPath, manifest.TokenizerPath })
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            var snapshot = new TeachingRegistrySnapshot(
                string.IsNullOrWhiteSpace(snapshotId) ? $"teaching-snapshot-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}" : snapshotId,
                bundleId.Trim(),
                rowIndex,
                DateTimeOffset.UtcNow,
                capturedUnits.Value!,
                capturedFlows,
                manifests,
                checkpointPaths);

            return OperationResult<TeachingRegistrySnapshot>.Ok(snapshot);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<TeachingRegistrySnapshot>.Fail(
                "Teaching registry snapshot capture failed.",
                "SNAPSHOT_CAPTURE_FAILED");
        }
    }

    // Save one generated capability manifest beside the checkpoint files that rebuild it.
    public static OperationResult<string> SaveGeneratedCapabilityManifest(
        GeneratedCapabilityCheckpointManifest manifest,
        string manifestPath)
    {
        if (manifest is null)
            return OperationResult<string>.Fail("Generated capability manifest is required.", "MANIFEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<string>.Fail("Manifest path is required.", "MANIFEST_PATH_REQUIRED");

        string canonicalManifestPath;
        string manifestFileName;
        string temporaryFileName;
        string temporaryPath;
        byte[] payload;
        try
        {
            payload = SerializeBoundedGeneratedCapabilityManifest(manifest);
            if (!LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
                return OperationResult<string>.Fail(
                    "Generated capability manifest publication requires Windows file identity custody.",
                    GeneratedCapabilityManifestIdentityCustodyRequiredErrorCode);
            canonicalManifestPath = Path.GetFullPath(manifestPath);
            if (canonicalManifestPath.Length >
                MaximumManifestPublicationPathLength)
            {
                return OperationResult<string>.Fail(
                    "Generated capability manifest path is invalid.",
                    "GENERATED_CAPABILITY_MANIFEST_WRITE_FAILED");
            }
            var directory = Path.GetDirectoryName(canonicalManifestPath);
            if (string.IsNullOrWhiteSpace(directory))
                return OperationResult<string>.Fail(
                    "Generated capability manifest path is invalid.",
                    "GENERATED_CAPABILITY_MANIFEST_WRITE_FAILED");
            manifestFileName = Path.GetFileName(canonicalManifestPath);
            ValidateManifestRelativeName(manifestFileName);
            temporaryFileName =
                manifestFileName + $".tmp-{Guid.NewGuid():N}";
            ValidateManifestRelativeName(temporaryFileName);
            temporaryPath = Path.Combine(directory, temporaryFileName);

        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<string>.Fail(
                "Generated capability manifest could not be prepared.",
                "GENERATED_CAPABILITY_MANIFEST_WRITE_FAILED");
        }

        if (!OperatingSystem.IsWindows())
        {
            // Linux публикует manifest через общий custody-helper: временный
            // файл создаётся эксклюзивно, родительский каталог удерживается
            // через fd, а откат удаляет только доказанный inode.
            return LogicalCapabilityFileCustody.SavePayloadNoOverwrite(
                canonicalManifestPath,
                temporaryFileName,
                manifestFileName,
                payload,
                GeneratedCapabilityManifestIdentityCustodyRequiredErrorCode,
                "GENERATED_CAPABILITY_MANIFEST_ALREADY_EXISTS",
                "GENERATED_CAPABILITY_MANIFEST_WRITE_FAILED");
        }

        ManifestPublicationDirectoryCustody? directoryCustody = null;
        FileStream? ownedManifestTemp = null;
        SafeFileHandle? ownedManifestTempHandle = null;
        ByHandleFileInformation? ownedManifestTempIdentity = null;
        var publicationCommitted = false;
        OperationResult<string>? outcome = null;
        var failures = new ManifestPublicationCleanupArbiter();
        try
        {
            var directory = Path.GetDirectoryName(canonicalManifestPath)!;
            directoryCustody =
                ManifestPublicationDirectoryCustody.OpenOrCreate(directory);
            ownedManifestTempHandle = OpenManifestRelativeHandle(
                directoryCustody.DirectoryHandle,
                temporaryFileName,
                0x40000000 | 0x00010000 | 0x00100000 | 0x00000080,
                0,
                2,
                0x00000020 | 0x00000040 | 0x00200000,
                "Generated capability manifest temporary file could not be created.");
            ownedManifestTemp = new FileStream(
                ownedManifestTempHandle,
                FileAccess.Write,
                bufferSize: 4096,
                isAsync: false);
            if (!GetFileInformationByHandle(
                    ownedManifestTemp.SafeFileHandle, out var tempIdentity))
                throw new IOException(
                    "Generated capability manifest temporary identity could not be captured.",
                    new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()));
            ownedManifestTempIdentity = tempIdentity;
            ownedManifestTemp.Write(payload, 0, payload.Length);
            ownedManifestTemp.Flush(flushToDisk: true);
            ManifestBeforeHandleRenameObserver.Value?.Invoke(
                temporaryPath, canonicalManifestPath);
            directoryCustody.EnsurePathIdentityUnchanged();
            RenameManifestHandleWithoutOverwrite(
                ownedManifestTemp.SafeFileHandle,
                directoryCustody.DirectoryHandle,
                manifestFileName);
            publicationCommitted = true;
            outcome = OperationResult<string>.Ok(canonicalManifestPath);
        }
        catch (IOException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)
                  && IsManifestPublicationCollision(exception))
        {
            failures.CaptureOriginal(exception);
            outcome = OperationResult<string>.Fail(
                "Generated capability manifest already exists.",
                "GENERATED_CAPABILITY_MANIFEST_ALREADY_EXISTS");
        }
        catch (Exception exception)
        {
            failures.CaptureOriginal(exception);
            if (!ManifestPublicationCleanupArbiter
                    .IsFatalConservatively(exception))
            {
                outcome = OperationResult<string>.Fail(
                    "Generated capability manifest publication failed.",
                    "GENERATED_CAPABILITY_MANIFEST_WRITE_FAILED");
            }
        }
        finally
        {
            var exactHandleDeleteScheduled = false;
            var exactHandleDeleteAttempted = false;
            int legacyDeleteError = 0;
            int extendedDeleteError = 0;
            if (!publicationCommitted &&
                ownedManifestTempHandle is { IsInvalid: false, IsClosed: false })
            {
                try
                {
                    exactHandleDeleteAttempted = true;
                    exactHandleDeleteScheduled = TryScheduleManifestDeletion(
                        ownedManifestTempHandle,
                        out legacyDeleteError,
                        out extendedDeleteError);
                }
                catch (Exception exception)
                {
                    failures.CaptureCleanup(exception);
                }
            }

            try { ownedManifestTemp?.Dispose(); }
            catch (Exception exception) { failures.CaptureCleanup(exception); }
            try { ownedManifestTempHandle?.Dispose(); }
            catch (Exception exception) { failures.CaptureCleanup(exception); }

            if (!publicationCommitted &&
                !exactHandleDeleteScheduled &&
                ownedManifestTempIdentity is { } expectedIdentity &&
                directoryCustody is not null)
            {
                TryDeleteManifestTempByRevalidatedHandle(
                    directoryCustody.DirectoryHandle,
                    temporaryFileName,
                    expectedIdentity,
                    legacyDeleteError,
                    extendedDeleteError,
                    failures);
            }
            else if (!publicationCommitted &&
                     exactHandleDeleteAttempted &&
                     !exactHandleDeleteScheduled &&
                     ownedManifestTempIdentity is null)
            {
                failures.CaptureCleanup(CreateManifestCleanupException(
                    legacyDeleteError,
                    extendedDeleteError,
                    0));
            }
            directoryCustody?.DisposeInto(failures);
        }

        var winner = failures.Resolve();
        if (winner is not null &&
            (ManifestPublicationCleanupArbiter
                 .IsFatalConservatively(winner) ||
             outcome is null))
        {
            throw winner;
        }
        if (failures.HasCleanupFailure)
            ReportManifestTempCleanupFailure();
        return outcome ?? OperationResult<string>.Fail(
            "Generated capability manifest publication failed.",
            "GENERATED_CAPABILITY_MANIFEST_WRITE_FAILED");
    }

    // Save the cumulative teaching snapshot after a row completes.
    public static OperationResult<string> Save(TeachingRegistrySnapshot snapshot, string snapshotPath)
    {
        if (snapshot is null)
            return OperationResult<string>.Fail("Snapshot is required.", "SNAPSHOT_REQUIRED");
        if (string.IsNullOrWhiteSpace(snapshotPath))
            return OperationResult<string>.Fail("Snapshot path is required.", "SNAPSHOT_PATH_REQUIRED");

        try
        {
            var directory = Path.GetDirectoryName(snapshotPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(snapshotPath, JsonSerializer.Serialize(snapshot, JsonOptions));
            return OperationResult<string>.Ok(snapshotPath);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail(ex.Message, "SNAPSHOT_WRITE_FAILED");
        }
    }

    // Load the cumulative teaching snapshot. Null means row one has no prior state.
    public static OperationResult<TeachingRegistrySnapshot?> Load(string snapshotPath)
    {
        if (string.IsNullOrWhiteSpace(snapshotPath))
            return OperationResult<TeachingRegistrySnapshot?>.Fail("Snapshot path is required.", "SNAPSHOT_PATH_REQUIRED");
        if (!File.Exists(snapshotPath))
            return OperationResult<TeachingRegistrySnapshot?>.Ok(null);

        try
        {
            var json = File.ReadAllText(snapshotPath);
            var snapshot = JsonSerializer.Deserialize<TeachingRegistrySnapshot>(json, JsonOptions);
            return OperationResult<TeachingRegistrySnapshot?>.Ok(snapshot);
        }
        catch (Exception ex)
        {
            return OperationResult<TeachingRegistrySnapshot?>.Fail(ex.Message, "SNAPSHOT_READ_FAILED");
        }
    }

    // Serving callers can bind deserialization to one already-opened, size-bounded handle.
    // The legacy path loader above intentionally retains its existing behavior.
    public static OperationResult<TeachingRegistrySnapshot> LoadBounded(
        Stream stream,
        long maximumBytes)
    {
        if (stream is null || !stream.CanRead)
        {
            return OperationResult<TeachingRegistrySnapshot>.Fail(
                "Snapshot stream is required.",
                "SNAPSHOT_STREAM_REQUIRED");
        }
        if (maximumBytes <= 0)
        {
            return OperationResult<TeachingRegistrySnapshot>.Fail(
                "Snapshot size limit is invalid.",
                "SNAPSHOT_SIZE_LIMIT_INVALID");
        }

        try
        {
            if (!stream.CanSeek ||
                stream.Length <= 0 ||
                stream.Length > maximumBytes)
            {
                return OperationResult<TeachingRegistrySnapshot>.Fail(
                    "Snapshot exceeds the serving size boundary.",
                    "SNAPSHOT_SIZE_INVALID");
            }

            stream.Position = 0;
            var snapshot = JsonSerializer.Deserialize<TeachingRegistrySnapshot>(
                stream,
                BoundedReadJsonOptions);
            if (snapshot is null || stream.Position > maximumBytes)
            {
                return OperationResult<TeachingRegistrySnapshot>.Fail(
                    "Snapshot is invalid.",
                    "SNAPSHOT_READ_FAILED");
            }

            return OperationResult<TeachingRegistrySnapshot>.Ok(snapshot);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<TeachingRegistrySnapshot>.Fail(
                "Snapshot could not be read.",
                "SNAPSHOT_READ_FAILED");
        }
    }

    // Load generated retrieval units and generated capability flows into registries without re-registering them.
    // deliveryLedger — журнал состоявшихся выдач ответа. Параметр стоит последним и
    // необязателен, поэтому все прежние вызывающие продолжают работать без правок. Он нужен
    // ровно для одного: чтобы восстановленный поток запомнил выбранного получателя и будущее
    // подтверждённое исправление нашло адресата. Без журнала восстановление ведёт себя как прежде.
    public static OperationResult<int> LoadIntoRegistries(
        TeachingRegistrySnapshot snapshot,
        ILogicalTopologyRegistry topology,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        IAnsweredDeliveryLedger<string>? deliveryLedger = null)
        => LoadIntoRegistriesCore(
            snapshot,
            topology,
            units,
            flows,
            artifactObserver: null,
            heldArtifacts: null,
            replayVerificationCapability: null,
            deliveryLedger);

    // Та же штатная загрузка с проверкой воспроизведения: журнал выдач и здесь необязателен.
    public static OperationResult<int> LoadIntoRegistries(
        TeachingRegistrySnapshot snapshot,
        ILogicalTopologyRegistry topology,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        LogicalCapabilityReplayVerificationCapability
            replayVerificationCapability,
        IAnsweredDeliveryLedger<string>? deliveryLedger = null)
        => LoadIntoRegistriesCore(
            snapshot,
            topology,
            units,
            flows,
            artifactObserver: null,
            heldArtifacts: null,
            replayVerificationCapability,
            deliveryLedger);

    public static OperationResult<int> LoadIntoRegistriesFromHeldArtifacts(
        TeachingRegistrySnapshot snapshot,
        ILogicalTopologyRegistry topology,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        IReadOnlyDictionary<string, TeachingRegistryArtifactSource> heldArtifacts)
        => heldArtifacts is null
            ? OperationResult<int>.Fail(
                "Held artifacts are required.",
                "HELD_ARTIFACTS_REQUIRED")
            : LoadIntoRegistriesCore(
                snapshot,
                topology,
                units,
                flows,
                artifactObserver: null,
                heldArtifacts,
                replayVerificationCapability: null,
                // Эта перегрузка журнал выдач не принимает: её путь переносом не затронут.
                deliveryLedger: null);

    public static OperationResult<int> LoadIntoRegistriesFromHeldArtifacts(
        TeachingRegistrySnapshot snapshot,
        ILogicalTopologyRegistry topology,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        IReadOnlyDictionary<string, TeachingRegistryArtifactSource>
            heldArtifacts,
        LogicalCapabilityReplayVerificationCapability
            replayVerificationCapability)
        => heldArtifacts is null
            ? OperationResult<int>.Fail(
                "Held artifacts are required.",
                "HELD_ARTIFACTS_REQUIRED")
            : LoadIntoRegistriesCore(
                snapshot,
                topology,
                units,
                flows,
                artifactObserver: null,
                heldArtifacts,
                replayVerificationCapability,
                // Эта перегрузка журнал выдач не принимает: её путь переносом не затронут.
                deliveryLedger: null);

    internal static OperationResult<int> LoadIntoRegistriesForTesting(
        TeachingRegistrySnapshot snapshot,
        ILogicalTopologyRegistry topology,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        ITeachingRegistryArtifactLoadObserver artifactObserver,
        LogicalCapabilityReplayVerificationCapability?
            replayVerificationCapability = null)
        => LoadIntoRegistriesCore(
            snapshot,
            topology,
            units,
            flows,
            artifactObserver,
            heldArtifacts: null,
            replayVerificationCapability,
            // Наблюдатель артефактов журнал выдач не проверяет: его путь переносом не затронут.
            deliveryLedger: null);

    private static OperationResult<int> LoadIntoRegistriesCore(
        TeachingRegistrySnapshot snapshot,
        ILogicalTopologyRegistry topology,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        ITeachingRegistryArtifactLoadObserver? artifactObserver,
        IReadOnlyDictionary<string, TeachingRegistryArtifactSource>? heldArtifacts,
        LogicalCapabilityReplayVerificationCapability?
            replayVerificationCapability,
        // Журнал выдач доносится до фабрики восстановленного потока как есть; отсутствие
        // журнала — законный прежний случай, а не отказ загрузки.
        IAnsweredDeliveryLedger<string>? deliveryLedger)
    {
        if (snapshot is null)
            return OperationResult<int>.Fail("Snapshot is required.", "SNAPSHOT_REQUIRED");
        if (topology is null)
            return OperationResult<int>.Fail("Topology registry is required.", "TOPOLOGY_REGISTRY_REQUIRED");
        if (units is null)
            return OperationResult<int>.Fail("Unit registry is required.", "UNIT_REGISTRY_REQUIRED");
        if (flows is null)
            return OperationResult<int>.Fail("Flow registry is required.", "FLOW_REGISTRY_REQUIRED");

        if (snapshot.Units is null || snapshot.Flows is null || snapshot.GeneratedCapabilities is null)
            return OperationResult<int>.Fail(
                "Teaching registry snapshot is invalid.",
                "TEACHING_REGISTRY_SNAPSHOT_INVALID");

        if (snapshot.GeneratedCapabilities.Length > MaximumManifestCount)
        {
            return OperationResult<int>.Fail(
                "Generated capability manifest inventory exceeds its bound.",
                "GENERATED_CAPABILITY_MANIFEST_INVENTORY_TOO_LARGE");
        }

        var generatedManifests = new List<
            GeneratedCapabilityCheckpointManifest>(
            snapshot.GeneratedCapabilities.Length);
        var manifestsByUnitId = new Dictionary<string, GeneratedCapabilityCheckpointManifest>(StringComparer.Ordinal);
        foreach (var manifest in snapshot.GeneratedCapabilities)
        {
            if (!TrySnapshotManifest(manifest, out var ownedManifest))
                return OperationResult<int>.Fail(
                    "Generated capability manifest is invalid.",
                    "GENERATED_CAPABILITY_MANIFEST_INVALID");
            if (!manifestsByUnitId.TryAdd(
                    ownedManifest.UnitId,
                    ownedManifest))
                return OperationResult<int>.Fail(
                    "Generated capability manifest UnitId is duplicated.",
                    "GENERATED_CAPABILITY_MANIFEST_DUPLICATE_UNIT_ID");
            generatedManifests.Add(ownedManifest);
        }

        var unitEntries = new Dictionary<string, TeachingUnitRegistryEntry>(StringComparer.Ordinal);
        foreach (var entry in snapshot.Units)
        {
            if (entry is null || string.IsNullOrWhiteSpace(entry.UnitId) || !unitEntries.TryAdd(entry.UnitId, entry))
                return OperationResult<int>.Fail(
                    "Teaching unit registry identity is invalid or duplicated.",
                    "TEACHING_UNIT_REGISTRY_IDENTITY_INVALID");
        }

        if (snapshot.Flows.Any(entry => entry is null))
            return OperationResult<int>.Fail(
                "Teaching flow registry identity is invalid.",
                "TEACHING_FLOW_REGISTRY_IDENTITY_INVALID");

        var generatedUnitEntries = new List<(
            TeachingUnitRegistryEntry Entry,
            GeneratedCapabilityCheckpointManifest Manifest,
            LogicalUnitTrustState TrustState)>();
        foreach (var unitEntry in snapshot.Units)
        {
            var hasManifest = manifestsByUnitId.TryGetValue(unitEntry.UnitId, out var manifest);
            var hasGeneratedKind = string.Equals(
                unitEntry.UnitKind,
                TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit,
                StringComparison.Ordinal);

            // A manifest-backed tuple is always the generated retrieval contract, and the
            // generated kind is never valid without its exact manifest binding.
            if (!hasManifest && !hasGeneratedKind)
                continue;
            if (!hasManifest || !hasGeneratedKind)
                return GeneratedCapabilityRestoreFailure();

            var unitTrustState = ParseTrustState(unitEntry.TrustState);
            if (!unitTrustState.IsSuccess)
                return OperationResult<int>.Fail(
                    "Teaching unit trust state is invalid.",
                    "TEACHING_UNIT_TRUST_STATE_INVALID");

            var manifestTrustState = ParseTrustState(manifest!.TrustState);
            if (!manifestTrustState.IsSuccess)
                return OperationResult<int>.Fail(
                    "Generated capability trust state is invalid.",
                    "GENERATED_CAPABILITY_TRUST_STATE_INVALID");

            if (unitTrustState.Value != manifestTrustState.Value)
                return OperationResult<int>.Fail(
                    "Generated capability trust state does not match its teaching unit.",
                    "GENERATED_CAPABILITY_TRUST_STATE_MISMATCH");

            if (!IsExactGeneratedCapabilityBinding(
                    unitEntry,
                    manifest,
                    unitTrustState.Value,
                    manifestTrustState.Value))
                return GeneratedCapabilityRestoreFailure();

            generatedUnitEntries.Add((unitEntry, manifest, unitTrustState.Value));
        }

        if (generatedUnitEntries.Count != manifestsByUnitId.Count)
            return GeneratedCapabilityRestoreFailure();

        var resolvedFlowManifests = new Dictionary<TeachingFlowRegistryEntry, GeneratedCapabilityCheckpointManifest>(
            ReferenceEqualityComparer.Instance);
        var manifestFlowConsumption = generatedManifests
            .ToDictionary(
                manifest => manifest.UnitId,
                _ => 0,
                StringComparer.Ordinal);
        foreach (var flowEntry in snapshot.Flows
            .Where(entry => entry.FlowKind == TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow))
        {
            var candidates = generatedManifests
                .Where(manifest =>
                    IsExactGeneratedCapabilityFlowBinding(flowEntry, manifest))
                .ToArray();
            if (candidates.Length == 0)
                return OperationResult<int>.Fail(
                    "Generated capability flow has no matching manifest.",
                    "GENERATED_CAPABILITY_FLOW_MANIFEST_NOT_FOUND");
            if (candidates.Length > 1)
                return OperationResult<int>.Fail(
                    "Generated capability flow manifest identity is ambiguous.",
                    "GENERATED_CAPABILITY_FLOW_MANIFEST_AMBIGUOUS");

            var resolvedManifest = candidates[0];
            if (!resolvedFlowManifests.TryAdd(flowEntry, resolvedManifest))
                return OperationResult<int>.Fail(
                    "Teaching flow registry identity is invalid or duplicated.",
                    "TEACHING_FLOW_REGISTRY_IDENTITY_INVALID");
            manifestFlowConsumption[resolvedManifest.UnitId] =
                checked(manifestFlowConsumption[resolvedManifest.UnitId] + 1);
        }

        if (manifestFlowConsumption.Values.Any(count => count != 1))
            return GeneratedCapabilityRestoreFailure();

        foreach (var manifest in generatedManifests)
        {
            var executableManifest = ValidateExecutableManifest(
                manifest,
                replayVerificationCapability);
            if (!executableManifest.IsSuccess)
            {
                return OperationResult<int>.Fail(
                    executableManifest.ErrorMessage,
                    executableManifest.ErrorCode);
            }
        }

        var loadedUnits = new List<LogicalUnitRegistryLoadedEntry>();
        var loadedFlows = new List<LogicalFlowRegistryLoadedEntry>();

        foreach (var generatedEntry in generatedUnitEntries
            .OrderBy(candidate => candidate.Manifest.CreatedAtUtc)
            .ThenBy(candidate => candidate.Entry.RegistrationOrder))
        {
            var unitEntry = generatedEntry.Entry;
            var manifest = generatedEntry.Manifest;
            var unit = LoadGeneratedCapabilityUnit(
                manifest,
                artifactObserver,
                heldArtifacts,
                replayVerificationCapability);
            if (!unit.IsSuccess)
                return OperationResult<int>.Fail(unit.ErrorMessage, unit.ErrorCode);
            if (unit.Value!.GetType() != typeof(TransformerEmbeddingKnnLogicalUnit<string>))
                return GeneratedCapabilityRestoreFailure();

            var registrationOrder = unitEntry.RegistrationOrder;
            loadedUnits.Add(new LogicalUnitRegistryLoadedEntry(
                new LogicalUnitDescriptor(
                unitEntry.UnitId,
                unitEntry.UnitRole,
                unitEntry.DomainTag,
                unitEntry.Version,
                generatedEntry.TrustState,
                false,
                typeof(string),
                typeof(string),
                registrationOrder),
                unit.Value!));
        }

        foreach (var flowEntry in snapshot.Flows
            .Where(entry => entry.FlowKind == TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow)
            .OrderBy(entry => entry.RegistrationOrder))
        {
            var subPhase = topology.ResolveSubPhase(flowEntry.SubPhaseId);
            if (!subPhase.IsSuccess)
                return OperationResult<int>.Fail(subPhase.ErrorMessage, subPhase.ErrorCode);

            if (!resolvedFlowManifests.TryGetValue(flowEntry, out var manifest))
                return OperationResult<int>.Fail(
                    "Generated capability flow has no matching manifest.",
                    "GENERATED_CAPABILITY_FLOW_MANIFEST_NOT_FOUND");
            // Executable v2 manifests carry the signed runtime routing values.
            // The live topology may validate that the sub-phase still exists,
            // but it cannot supply replacement predicates or domain tags.
            var descriptorDomainTags = manifest.DomainTags.ToArray();
            var factoryDomainTags = manifest.DomainTags.ToArray();
            var predicateIds = manifest.PredicateIds.ToArray();

            loadedFlows.Add(new LogicalFlowRegistryLoadedEntry(
                new LogicalFlowDescriptor(
                    flowEntry.FlowId,
                    flowEntry.ScenarioId,
                    flowEntry.Version,
                    typeof(string),
                    flowEntry.PhaseId,
                    flowEntry.SubPhaseId,
                    flowEntry.TaskType,
                    descriptorDomainTags,
                    flowEntry.RegistrationOrder),
                new GeneratedCapabilityFlowFactory(
                    flowEntry.PhaseId,
                    flowEntry.SubPhaseId,
                    flowEntry.TaskType,
                    factoryDomainTags,
                    predicateIds,
                    manifest.DomainTag,
                    manifest.OriginalPrompt,
                    // Журнал выдач доходит до штатно восстановленного потока здесь: иначе у
                    // обычной выдачи после загрузки сохранённого состояния не было бы способа
                    // запомнить адресата будущего исправления. Отсутствие журнала оставляет
                    // восстановленный поток в точности прежним.
                    deliveryLedger)));
        }

        // The loaded executable objects and factories are fully prepared before taking the
        // synchronous cross-registry lease. Hold the sole unit-to-flow lock order across both
        // exact preflights and both appends so no concurrent publisher can invalidate the
        // checked live order/count state or observe only one side of the restored capability.
        var registryPublicationLeaseResult =
            LogicalRegistryPublicationLease.TryAcquireUnitThenFlow(
                units,
                flows,
                RegistryPublicationLeaseTimeout);
        if (!registryPublicationLeaseResult.IsSuccess ||
            registryPublicationLeaseResult.Value is null)
        {
            return OperationResult<int>.Fail(
                registryPublicationLeaseResult.ErrorMessage,
                registryPublicationLeaseResult.ErrorCode);
        }
        using var registryPublicationLease =
            registryPublicationLeaseResult.Value;
        RestorePublicationLeaseAcquiredObserver.Value?.Invoke();

        // Exercise both exact public append contracts against one locked live order/count
        // state before either live registry can be changed.
        var unitPreflight = PreflightUnitAppend(units, loadedUnits);
        if (!unitPreflight.IsSuccess)
            return OperationResult<int>.Fail(unitPreflight.ErrorMessage, unitPreflight.ErrorCode);

        var flowPreflight = PreflightFlowAppend(flows, loadedFlows);
        if (!flowPreflight.IsSuccess)
            return OperationResult<int>.Fail(flowPreflight.ErrorMessage, flowPreflight.ErrorCode);

        var unitLoad = units.LoadEntries(loadedUnits, replaceExisting: false);
        if (!unitLoad.IsSuccess)
            return OperationResult<int>.Fail(unitLoad.ErrorMessage, unitLoad.ErrorCode);

        var flowLoad = flows.LoadEntries(loadedFlows, replaceExisting: false);
        if (!flowLoad.IsSuccess)
            return OperationResult<int>.Fail(flowLoad.ErrorMessage, flowLoad.ErrorCode);

        return OperationResult<int>.Ok(unitLoad.Value + flowLoad.Value);
    }

    private static OperationResult<int> PreflightUnitAppend(
        InMemoryLogicalUnitRegistry liveRegistry,
        IReadOnlyList<LogicalUnitRegistryLoadedEntry> candidateEntries)
    {
        // First validate the candidate set itself through the real registry contract.
        var candidateValidationRegistry = new InMemoryLogicalUnitRegistry();
        var candidateValidation = candidateValidationRegistry.LoadEntries(
            candidateEntries,
            replaceExisting: true);
        if (!candidateValidation.IsSuccess || candidateEntries.Count == 0)
            return candidateValidation;

        // LoadEntries observes existing state only through tuple count and registration orders.
        // Recreate exactly those observable values with valid reservations, then append the
        // original candidate objects through the same public contract.
        var prototype = candidateEntries[0];
        var existingReservations = liveRegistry.ListDescriptors()
            .Select((descriptor, index) => new LogicalUnitRegistryLoadedEntry(
                prototype.Descriptor with
                {
                    UnitId = $"load-preflight-existing-unit-{index + 1}",
                    UnitRole = "load-preflight-existing-unit",
                    DomainTag = "load-preflight",
                    Version = "1.0.0",
                    RegistrationOrder = descriptor.RegistrationOrder
                },
                prototype.Unit))
            .ToArray();

        var liveStateValidationRegistry = new InMemoryLogicalUnitRegistry();
        var existingLoad = liveStateValidationRegistry.LoadEntries(
            existingReservations,
            replaceExisting: true);
        if (!existingLoad.IsSuccess)
            return existingLoad;

        return liveStateValidationRegistry.LoadEntries(candidateEntries, replaceExisting: false);
    }

    private static OperationResult<int> PreflightFlowAppend(
        InMemoryLogicalFlowRegistry liveRegistry,
        IReadOnlyList<LogicalFlowRegistryLoadedEntry> candidateEntries)
    {
        // First validate the candidate set itself through the real registry contract.
        var candidateValidationRegistry = new InMemoryLogicalFlowRegistry();
        var candidateValidation = candidateValidationRegistry.LoadEntries(
            candidateEntries,
            replaceExisting: true);
        if (!candidateValidation.IsSuccess || candidateEntries.Count == 0)
            return candidateValidation;

        // Preserve the live tuple count and every existing order while using the already
        // validated candidate boundary for inert reservations. The second append therefore
        // executes the exact production conflict and capacity rules without touching live data.
        var prototype = candidateEntries[0];
        var existingReservations = liveRegistry.ListDescriptors()
            .Select((descriptor, index) => new LogicalFlowRegistryLoadedEntry(
                prototype.Descriptor with
                {
                    FlowId = $"load-preflight-existing-flow-{index + 1}",
                    ScenarioId = $"load-preflight-existing-scenario-{index + 1}",
                    Version = "1.0.0",
                    RegistrationOrder = descriptor.RegistrationOrder
                },
                prototype.FlowOrFactory))
            .ToArray();

        var liveStateValidationRegistry = new InMemoryLogicalFlowRegistry();
        var existingLoad = liveStateValidationRegistry.LoadEntries(
            existingReservations,
            replaceExisting: true);
        if (!existingLoad.IsSuccess)
            return existingLoad;

        return liveStateValidationRegistry.LoadEntries(candidateEntries, replaceExisting: false);
    }

    private static OperationResult<TeachingUnitRegistryEntry[]> CaptureUnitEntries(
        InMemoryLogicalUnitRegistry units,
        IReadOnlyDictionary<string, GeneratedCapabilityCheckpointManifest> manifestsByUnitId)
    {
        var entries = new List<TeachingUnitRegistryEntry>();
        foreach (var descriptor in units.ListDescriptors())
        {
            if (!manifestsByUnitId.TryGetValue(descriptor.UnitId, out var manifest))
            {
                var unresolvedGeneratedRetrieval = string.Equals(
                        descriptor.UnitRole,
                        DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
                        StringComparison.Ordinal) &&
                    descriptor.InputType == typeof(string) &&
                    descriptor.OutputType == typeof(string)
                    ? units.ResolveLatestById<string, string>(descriptor.UnitId)
                    : null;
                if (unresolvedGeneratedRetrieval is { IsSuccess: true } &&
                    Equals(unresolvedGeneratedRetrieval.Value!.Descriptor, descriptor) &&
                    unresolvedGeneratedRetrieval.Value.Unit.GetType() ==
                        typeof(TransformerEmbeddingKnnLogicalUnit<string>))
                {
                    // A real generated retrieval executable without its manifest cannot be
                    // reconstructed durably and must not be downgraded to deterministic.
                    return OperationResult<TeachingUnitRegistryEntry[]>.Fail(
                        "Generated capability registry tuple is invalid.",
                        GeneratedCapabilityCaptureInvalid);
                }

                // A role/type lookalike without a generated manifest remains an ordinary
                // deterministic registration; role names alone never confer generated status.
                entries.Add(ToSnapshotEntry(
                    descriptor,
                    descriptor.IsLearned,
                    TeachingRegistrySnapshotKinds.DeterministicUnit));
                continue;
            }

            var manifestTrustState = ParseTrustState(manifest.TrustState);
            var resolved = units.ResolveLatestById<string, string>(descriptor.UnitId);
            if (!manifestTrustState.IsSuccess ||
                !IsExactGeneratedCapabilityBinding(descriptor, manifest, manifestTrustState.Value) ||
                !resolved.IsSuccess ||
                !Equals(resolved.Value!.Descriptor, descriptor) ||
                resolved.Value.Unit.GetType() != typeof(TransformerEmbeddingKnnLogicalUnit<string>))
            {
                return OperationResult<TeachingUnitRegistryEntry[]>.Fail(
                    "Generated capability registry tuple is invalid.",
                    GeneratedCapabilityCaptureInvalid);
            }

            // Опись снимка хранит только модель, токенизатор и учебные примеры; каталога правил
            // ранжирования в ней нет, поэтому восстановление всегда собирает модуль без каталога.
            // Настроенный непустой каталог меняет сам алгоритм выбора ответа, и молча записать такой
            // модуль как восстановимый нельзя: восстановленная модель отвечала бы по другому правилу.
            // Пустой и отсутствующий каталог сюда не попадают и сохраняют прежнее поведение байт в байт.
            if (resolved.Value!.Unit is TransformerEmbeddingKnnLogicalUnit<string>
                {
                    CandidatePrincipleCount: > 0
                })
            {
                // Отказ выдаётся здесь, до постройки снимка, поэтому частичный снимок не создаётся.
                return OperationResult<TeachingUnitRegistryEntry[]>.Fail(
                    // Человеческое пояснение прямо называет неподдержанный режим, а не общий сбой.
                    "Generated capability candidate principle catalogue cannot be captured durably yet.",
                    // Машинный код отличает неподдержанный режим от испорченной связки снимка.
                    UnsupportedPrincipleCatalogueMode);
            }

            // Legacy registries may still carry IsLearned=true. The durable snapshot records
            // the honest retrieval classification after the executable boundary is proven.
            entries.Add(ToSnapshotEntry(
                descriptor,
                isLearned: false,
                TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit));
        }

        return OperationResult<TeachingUnitRegistryEntry[]>.Ok(entries.ToArray());
    }

    private static TeachingUnitRegistryEntry ToSnapshotEntry(
        LogicalUnitDescriptor descriptor,
        bool isLearned,
        string unitKind)
    {
        return new TeachingUnitRegistryEntry(
            descriptor.UnitId,
            descriptor.UnitRole,
            descriptor.DomainTag,
            descriptor.Version,
            descriptor.TrustState.ToString(),
            isLearned,
            TypeName(descriptor.InputType),
            TypeName(descriptor.OutputType),
            descriptor.RegistrationOrder,
            unitKind);
    }

    private static TeachingFlowRegistryEntry ToSnapshotEntry(LogicalFlowDescriptor descriptor)
    {
        return new TeachingFlowRegistryEntry(
            descriptor.FlowId,
            descriptor.ScenarioId,
            descriptor.Version,
            descriptor.InputType.AssemblyQualifiedName ?? descriptor.InputType.FullName ?? descriptor.InputType.Name,
            descriptor.PhaseId,
            descriptor.SubPhaseId,
            descriptor.TaskType,
            descriptor.DomainTags,
            descriptor.RegistrationOrder,
            DetectFlowKind(descriptor));
    }

    internal static OperationResult<GeneratedCapabilityManifestInventory> ReadGeneratedCapabilityManifestInventory(
        string checkpointRoot)
    {
        var pathComparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (string.IsNullOrWhiteSpace(checkpointRoot))
            return EmptyManifestInventory(pathComparer);

        string canonicalRoot;
        string rootPrefix;
        try
        {
            canonicalRoot = CanonicalizeRoot(checkpointRoot);
            rootPrefix = canonicalRoot.EndsWith(Path.DirectorySeparatorChar)
                ? canonicalRoot
                : canonicalRoot + Path.DirectorySeparatorChar;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                "Generated capability manifest root path is invalid.",
                "GENERATED_CAPABILITY_MANIFEST_PATH_INVALID");
        }

        if (!Directory.Exists(canonicalRoot))
            return EmptyManifestInventory(pathComparer);

        var pending = new Queue<(string Path, int Depth)>();
        var visitedDirectories = new HashSet<string>(pathComparer);
        var manifestPaths = new HashSet<string>(pathComparer);
        pending.Enqueue((canonicalRoot, 0));
        var entryCount = 0;
        long manifestByteCount = 0;

        while (pending.Count > 0)
        {
            var pendingDirectory = pending.Dequeue();
            var directory = pendingDirectory.Path;
            try
            {
                directory = Path.GetFullPath(directory);
                if (!IsContained(directory, canonicalRoot, rootPrefix, pathComparison) ||
                    (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                {
                    return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                        "Generated capability manifest path is invalid.",
                        "GENERATED_CAPABILITY_MANIFEST_PATH_INVALID");
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                    "Generated capability manifest path is invalid.",
                    "GENERATED_CAPABILITY_MANIFEST_PATH_INVALID");
            }

            if (!visitedDirectories.Add(directory))
                continue;
            if (visitedDirectories.Count > MaximumManifestDirectoryCount)
                return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                    "Generated capability manifest inventory exceeds its bound.",
                    "GENERATED_CAPABILITY_MANIFEST_INVENTORY_TOO_LARGE");

            IEnumerable<string> entries;
            try
            {
                entries = Directory.EnumerateFileSystemEntries(directory, "*",
                    new EnumerationOptions { RecurseSubdirectories = false,
                        ReturnSpecialDirectories = false, AttributesToSkip = 0,
                        IgnoreInaccessible = false });
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                    "Generated capability manifest inventory could not be read.",
                    "GENERATED_CAPABILITY_MANIFEST_READ_FAILED");
            }

            try
            {
              foreach (var entry in entries)
              {
                if (++entryCount > MaximumManifestEntryCount)
                    return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                        "Generated capability manifest inventory exceeds its bound.",
                        "GENERATED_CAPABILITY_MANIFEST_INVENTORY_TOO_LARGE");
                var child = Path.GetFullPath(entry);
                var attributes = File.GetAttributes(child);
                if (!IsContained(child, canonicalRoot, rootPrefix, pathComparison) ||
                    (attributes & FileAttributes.ReparsePoint) != 0)
                {
                    return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                        "Generated capability manifest path is invalid.",
                        "GENERATED_CAPABILITY_MANIFEST_PATH_INVALID");
                }
                if ((attributes & FileAttributes.Directory) != 0)
                {
                    if (pendingDirectory.Depth >= MaximumManifestTraversalDepth)
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            "Generated capability manifest traversal exceeds its bound.",
                            "GENERATED_CAPABILITY_MANIFEST_INVENTORY_TOO_LARGE");
                    pending.Enqueue((child, pendingDirectory.Depth + 1));
                    continue;
                }
                if (!StringComparer.Ordinal.Equals(Path.GetFileName(child),
                        GeneratedCapabilityManifestFileName))
                    continue;
                var length = new FileInfo(child).Length;
                if (length > MaximumManifestBytes ||
                    manifestByteCount > MaximumManifestInventoryBytes - length)
                {
                    return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                        "Generated capability manifest inventory exceeds its bound.",
                        "GENERATED_CAPABILITY_MANIFEST_INVENTORY_TOO_LARGE");
                }
                manifestByteCount += length;
                manifestPaths.Add(child);
                if (manifestPaths.Count > MaximumManifestCount)
                    return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                        "Generated capability manifest inventory exceeds its bound.",
                        "GENERATED_CAPABILITY_MANIFEST_INVENTORY_TOO_LARGE");
              }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                    "Generated capability manifest inventory could not be read.",
                    "GENERATED_CAPABILITY_MANIFEST_READ_FAILED");
            }
        }

        var orderedPaths = manifestPaths.OrderBy(path => path, pathComparer).ToArray();
        if (orderedPaths.Length > 0 &&
            !LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                "Generated capability manifest inventory requires Windows file identity custody.",
                GeneratedCapabilityManifestIdentityCustodyRequiredErrorCode);
        var sources = new List<GeneratedCapabilityManifestSource>(orderedPaths.Length);
        var byUnitId = new Dictionary<string, GeneratedCapabilityCheckpointManifest>(StringComparer.Ordinal);
        var sourceOrdinalByUnitId = new Dictionary<string, int>(StringComparer.Ordinal);
        long openedManifestByteCount = 0;
        for (var index = 0; index < orderedPaths.Length; index++)
        {
            var sourceOrdinal = index + 1;
            FileStream stream;
            try
            {
                var attributes = File.GetAttributes(orderedPaths[index]);
                if (!IsContained(orderedPaths[index], canonicalRoot, rootPrefix, pathComparison) ||
                    (attributes & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0)
                    throw new IOException();
                stream = LogicalCapabilityFileCustody.OpenIdentityStream(
                    orderedPaths[index],
                    LogicalCapabilityFileCustody.GenericReadAccess,
                    FileShare.Read | FileShare.Delete) ??
                    throw new IOException();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                    $"Generated capability manifest source #{sourceOrdinal:0000} could not be read.",
                    "GENERATED_CAPABILITY_MANIFEST_READ_FAILED");
            }

            GeneratedCapabilityCheckpointManifest? manifest;
            using (stream)
            {
                try
                {
                    var openedLength = stream.Length;
                    if (!OpenedHandleMatchesCanonicalPath(
                            stream.SafeFileHandle, orderedPaths[index]))
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            $"Generated capability manifest source #{sourceOrdinal:0000} path identity is invalid.",
                            "GENERATED_CAPABILITY_MANIFEST_PATH_INVALID");
                    if (openedLength > MaximumManifestBytes)
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            $"Generated capability manifest source #{sourceOrdinal:0000} exceeds its bound.",
                            "GENERATED_CAPABILITY_MANIFEST_TOO_LARGE");
                    if (openedManifestByteCount > MaximumManifestInventoryBytes - openedLength)
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            "Generated capability manifest inventory exceeds its bound.",
                            "GENERATED_CAPABILITY_MANIFEST_INVENTORY_TOO_LARGE");
                    openedManifestByteCount += openedLength;
                    var complexity = PreflightManifestComplexity(
                        stream,
                        openedLength);
                    if (complexity == ManifestComplexityPreflightStatus.TooComplex)
                    {
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            $"Generated capability manifest source #{sourceOrdinal:0000} exceeds its complexity bound.",
                            "GENERATED_CAPABILITY_MANIFEST_TOO_LARGE");
                    }
                    if (complexity == ManifestComplexityPreflightStatus.ChangedDuringRead ||
                        stream.Length != openedLength ||
                        stream.Position != openedLength)
                    {
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            $"Generated capability manifest source #{sourceOrdinal:0000} changed while being read.",
                            "GENERATED_CAPABILITY_MANIFEST_CHANGED_DURING_READ");
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    if (stream.Position != 0 || stream.Length != openedLength ||
                        !OpenedHandleMatchesCanonicalPath(
                            stream.SafeFileHandle,
                            orderedPaths[index]))
                    {
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            $"Generated capability manifest source #{sourceOrdinal:0000} changed while being read.",
                            "GENERATED_CAPABILITY_MANIFEST_CHANGED_DURING_READ");
                    }
                    ManifestReadPayloadAllocationObserver.Value?.Invoke();
                    var bytes = new byte[checked((int)openedLength)];
                    stream.ReadExactly(bytes);
                    if (stream.Length != openedLength || stream.Position != openedLength)
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            $"Generated capability manifest source #{sourceOrdinal:0000} changed while being read.",
                            "GENERATED_CAPABILITY_MANIFEST_CHANGED_DURING_READ");
                    var finalAttributes = File.GetAttributes(orderedPaths[index]);
                    if ((finalAttributes & (FileAttributes.ReparsePoint |
                            FileAttributes.Directory)) != 0)
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            $"Generated capability manifest source #{sourceOrdinal:0000} changed while being read.",
                            "GENERATED_CAPABILITY_MANIFEST_CHANGED_DURING_READ");
                    ManifestAfterReadBeforeIdentityReopenObserver.Value?.Invoke(
                        orderedPaths[index]);
                    using var verificationStream =
                        LogicalCapabilityFileCustody.OpenIdentityStream(
                            orderedPaths[index],
                            LogicalCapabilityFileCustody.GenericReadAccess,
                            FileShare.ReadWrite | FileShare.Delete) ??
                        throw new IOException();
                    if (!SameFileIdentity(stream.SafeFileHandle,
                            verificationStream.SafeFileHandle) ||
                        !OpenedHandleMatchesCanonicalPath(
                            verificationStream.SafeFileHandle,
                            orderedPaths[index]))
                        return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                            $"Generated capability manifest source #{sourceOrdinal:0000} changed while being read.",
                            "GENERATED_CAPABILITY_MANIFEST_CHANGED_DURING_READ");
                    manifest = JsonSerializer.Deserialize<GeneratedCapabilityCheckpointManifest>(bytes, BoundedReadJsonOptions);
                }
                catch (IOException exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                        $"Generated capability manifest source #{sourceOrdinal:0000} could not be read.",
                        "GENERATED_CAPABILITY_MANIFEST_READ_FAILED");
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                        $"Generated capability manifest source #{sourceOrdinal:0000} is invalid.",
                        "GENERATED_CAPABILITY_MANIFEST_INVALID");
                }
            }

            if (!HasRequiredManifestIdentity(manifest))
                return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                    $"Generated capability manifest source #{sourceOrdinal:0000} is invalid.",
                    "GENERATED_CAPABILITY_MANIFEST_INVALID");

            var validManifest = manifest!;
            if (!byUnitId.TryAdd(validManifest.UnitId, validManifest))
            {
                var previousOrdinal = sourceOrdinalByUnitId[validManifest.UnitId];
                return OperationResult<GeneratedCapabilityManifestInventory>.Fail(
                    $"Generated capability manifest source #{sourceOrdinal:0000} conflicts with source #{previousOrdinal:0000}.",
                    "GENERATED_CAPABILITY_MANIFEST_DUPLICATE_UNIT_ID");
            }

            sourceOrdinalByUnitId.Add(validManifest.UnitId, sourceOrdinal);
            sources.Add(new GeneratedCapabilityManifestSource(sourceOrdinal, orderedPaths[index], validManifest));
        }

        return OperationResult<GeneratedCapabilityManifestInventory>.Ok(
            new GeneratedCapabilityManifestInventory(sources.ToArray(), byUnitId));
    }

    private enum ManifestComplexityPreflightStatus
    {
        Complete,
        TooComplex,
        ChangedDuringRead
    }

    // Count the token/depth shape directly from the retained bounded file
    // handle. JsonSerializer remains the syntax validator after this preflight;
    // for every valid JSON document this scanner counts the same structural,
    // string, and primitive token starts without retaining string payloads.
    private static ManifestComplexityPreflightStatus
        PreflightManifestComplexity(Stream stream, long expectedLength)
    {
        Span<byte> buffer = stackalloc byte[4096];
        var remaining = expectedLength;
        var tokenCount = 0;
        var depth = 0;
        var inString = false;
        var escapeNext = false;
        var inPrimitive = false;

        while (remaining > 0)
        {
            var requested = checked((int)Math.Min(buffer.Length, remaining));
            var read = stream.Read(buffer[..requested]);
            if (read <= 0)
                return ManifestComplexityPreflightStatus.ChangedDuringRead;
            remaining -= read;

            foreach (var value in buffer[..read])
            {
                if (inString)
                {
                    if (escapeNext)
                    {
                        escapeNext = false;
                        continue;
                    }
                    if (value == (byte)'\\')
                    {
                        escapeNext = true;
                        continue;
                    }
                    if (value == (byte)'"')
                        inString = false;
                    continue;
                }

                if (inPrimitive)
                {
                    if (!IsManifestJsonDelimiter(value))
                        continue;
                    inPrimitive = false;
                }

                switch (value)
                {
                    case (byte)'{':
                    case (byte)'[':
                        if (++tokenCount > MaximumManifestJsonTokenCount ||
                            ++depth > BoundedReadJsonOptions.MaxDepth)
                        {
                            return ManifestComplexityPreflightStatus.TooComplex;
                        }
                        break;
                    case (byte)'}':
                    case (byte)']':
                        if (++tokenCount > MaximumManifestJsonTokenCount)
                            return ManifestComplexityPreflightStatus.TooComplex;
                        if (depth > 0)
                            depth--;
                        break;
                    case (byte)'"':
                        if (++tokenCount > MaximumManifestJsonTokenCount)
                            return ManifestComplexityPreflightStatus.TooComplex;
                        inString = true;
                        break;
                    case (byte)',':
                    case (byte)':':
                    case (byte)' ':
                    case (byte)'\t':
                    case (byte)'\r':
                    case (byte)'\n':
                        break;
                    default:
                        if (++tokenCount > MaximumManifestJsonTokenCount)
                            return ManifestComplexityPreflightStatus.TooComplex;
                        inPrimitive = true;
                        break;
                }
            }
        }

        return ManifestComplexityPreflightStatus.Complete;
    }

    private static bool IsManifestJsonDelimiter(byte value)
        => value is (byte)'{' or (byte)'}' or (byte)'[' or (byte)']' or
            (byte)'"' or (byte)',' or (byte)':' or (byte)' ' or
            (byte)'\t' or (byte)'\r' or (byte)'\n';

    private static OperationResult<TransformerEmbeddingKnnLogicalUnit<string>> LoadGeneratedCapabilityUnit(
        GeneratedCapabilityCheckpointManifest manifest,
        ITeachingRegistryArtifactLoadObserver? artifactObserver,
        IReadOnlyDictionary<string, TeachingRegistryArtifactSource>? heldArtifacts,
        LogicalCapabilityReplayVerificationCapability?
            replayVerificationCapability)
    {
        if (heldArtifacts is null && !File.Exists(manifest.PolicyCheckpointPath))
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                "Generated capability policy checkpoint was not found.",
                "POLICY_CHECKPOINT_NOT_FOUND");
        if (heldArtifacts is null && !File.Exists(manifest.TokenizerPath))
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                "Generated capability tokenizer checkpoint was not found.",
                "TOKENIZER_CHECKPOINT_NOT_FOUND");

        var identityResult = BuildManifestRuntimeIdentity(manifest);
        if (!identityResult.IsSuccess)
        {
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                identityResult.ErrorMessage,
                identityResult.ErrorCode);
        }
        var knnCorpus = ProjectBoundKnnCorpus(manifest);
        if (!knnCorpus.IsSuccess || knnCorpus.Value is null)
        {
            return OperationResult<
                TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                    knnCorpus.ErrorMessage,
                    knnCorpus.ErrorCode);
        }

        try
        {
            using var tokenizerArtifact = OpenRuntimeArtifact(
                manifest.TokenizerPath,
                heldArtifacts);
            using var checkpointArtifact = OpenRuntimeArtifact(
                manifest.PolicyCheckpointPath,
                heldArtifacts);
            var executableBinding = ValidateExecutableArtifactBinding(
                manifest,
                replayVerificationCapability,
                tokenizerArtifact.Sha256,
                checkpointArtifact.Sha256);
            if (!executableBinding.IsSuccess)
            {
                return OperationResult<
                    TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                        executableBinding.ErrorMessage,
                        executableBinding.ErrorCode);
            }
            tokenizerArtifact.Reset();
            var tokenizer = BpeTokenizer.Load(tokenizerArtifact.Stream);
            var identity = identityResult.Value!;
            var actualRuntimeProvenance = RuntimeProvenance.ComputeSha256(
                identity.ModelConfig,
                identity.LoraConfig,
                tokenizerArtifact.Sha256,
                checkpointArtifact.Sha256);
            if (!actualRuntimeProvenance.IsSuccess
                || !RuntimeProvenance.FixedTimeSha256Equals(
                    manifest.TokenizerSha256!,
                    tokenizerArtifact.Sha256)
                || !RuntimeProvenance.FixedTimeSha256Equals(
                    manifest.TokenizerSha256!,
                    tokenizer.ComputeStateSha256())
                || !RuntimeProvenance.FixedTimeSha256Equals(
                    manifest.PolicyCheckpointSha256!,
                    checkpointArtifact.Sha256)
                || !RuntimeProvenance.FixedTimeSha256Equals(
                    manifest.RuntimeProvenanceSha256!,
                    actualRuntimeProvenance.Value!)
                || tokenizer.VocabSize != identity.ModelConfig.VocabSize)
            {
                return OperationResult<TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                    "Generated capability provenance is invalid.",
                    "GENERATED_CAPABILITY_PROVENANCE_INVALID");
            }

            artifactObserver?.OnArtifactsVerified();
            if (!tokenizerArtifact.StillMatches() || !checkpointArtifact.StillMatches())
            {
                return OperationResult<TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                    "Generated capability provenance is invalid.",
                    "GENERATED_CAPABILITY_PROVENANCE_INVALID");
            }

            var model = new LlmModel(identity.ModelConfig);

            LoraApplicator.Apply(model, identity.LoraConfig);
            checkpointArtifact.Reset();
            model.LoadCheckpoint(checkpointArtifact.Stream);

            var examples = knnCorpus.Value.Examples
                .Select(static example =>
                    new TransformerKnnTrainingExample<string>(
                        example.ExampleId,
                        example.TrainingText,
                        example.OutputText,
                        example.PredicateIds.ToArray(),
                        example.DomainTags.ToArray()))
                .ToArray();

            return TransformerEmbeddingKnnLogicalUnit<string>.Train(
                manifest.UnitId,
                manifest.UnitRole,
                manifest.DomainTag,
                model,
                tokenizer,
                examples);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                "Generated capability could not be loaded.",
                "GENERATED_CAPABILITY_LOAD_FAILED");
        }
    }

    private static HeldRuntimeArtifact OpenRuntimeArtifact(
        string path,
        IReadOnlyDictionary<string, TeachingRegistryArtifactSource>? heldArtifacts)
    {
        if (heldArtifacts is null)
            return HeldRuntimeArtifact.Open(path);

        var canonicalPath = Path.GetFullPath(path);
        if (!heldArtifacts.TryGetValue(canonicalPath, out var source) ||
            source?.Stream is null ||
            source.MaximumBytes <= 0)
        {
            throw new InvalidDataException("A required held runtime artifact is missing.");
        }

        return HeldRuntimeArtifact.OpenHeld(
            source.Stream,
            source.MaximumBytes);
    }

    private static OperationResult<ManifestRuntimeIdentity> BuildManifestRuntimeIdentity(
        GeneratedCapabilityCheckpointManifest manifest)
    {
        if (manifest.SchemaVersion !=
                GeneratedCapabilityCheckpointManifest.CurrentSchemaVersion
            || manifest.RopeTheta is null
                || manifest.RmsNormEps is null
                || !float.IsFinite(manifest.RopeTheta.Value)
                || manifest.RopeTheta <= 0f
                || !float.IsFinite(manifest.RmsNormEps.Value)
                || manifest.RmsNormEps <= 0f
                || !RuntimeProvenance.IsSha256Hex(manifest.TokenizerSha256)
                || !RuntimeProvenance.IsSha256Hex(manifest.PolicyCheckpointSha256)
                || !RuntimeProvenance.IsSha256Hex(
                    manifest.RuntimeProvenanceSha256))
        {
            return OperationResult<ManifestRuntimeIdentity>.Fail(
                "Generated capability provenance is invalid.",
                "GENERATED_CAPABILITY_PROVENANCE_INVALID");
        }

        var modelConfig = new ModelConfig
        {
            VocabSize = manifest.VocabSize,
            ContextLen = manifest.ContextLen,
            HiddenDim = manifest.HiddenDim,
            NumLayers = manifest.NumLayers,
            NumHeads = manifest.NumHeads,
            NumKvHeads = manifest.NumKvHeads,
            FfnHiddenDim = manifest.FfnHiddenDim,
            RopeTheta = manifest.RopeTheta ?? new ModelConfig().RopeTheta,
            RmsNormEps = manifest.RmsNormEps ?? new ModelConfig().RmsNormEps
        };
        var loraConfig = new LoraConfig
        {
            Rank = manifest.LoraRank,
            Alpha = manifest.LoraAlpha,
            Dropout = manifest.LoraDropout,
            TargetModules = manifest.LoraTargetModules
        };

        var canonicalLora = RuntimeProvenance.CanonicalizeLora(loraConfig);
        var declaredProvenance = canonicalLora.IsSuccess
            ? RuntimeProvenance.ComputeSha256(
                modelConfig,
                canonicalLora.Value!,
                manifest.TokenizerSha256!,
                manifest.PolicyCheckpointSha256!)
            : OperationResult<string>.Fail(
                "Generated capability provenance is invalid.",
                "GENERATED_CAPABILITY_PROVENANCE_INVALID");
        if (!canonicalLora.IsSuccess
            || !declaredProvenance.IsSuccess
            || !RuntimeProvenance.FixedTimeSha256Equals(
                manifest.RuntimeProvenanceSha256!,
                declaredProvenance.Value!))
        {
            return OperationResult<ManifestRuntimeIdentity>.Fail(
                "Generated capability provenance is invalid.",
                "GENERATED_CAPABILITY_PROVENANCE_INVALID");
        }

        loraConfig = canonicalLora.Value!;

        return OperationResult<ManifestRuntimeIdentity>.Ok(
            new ManifestRuntimeIdentity(
                modelConfig,
                loraConfig));
    }

    private sealed record ManifestRuntimeIdentity(
        ModelConfig ModelConfig,
        LoraConfig LoraConfig);

    private static OperationResult<bool> ValidateExecutableManifest(
        GeneratedCapabilityCheckpointManifest manifest,
        LogicalCapabilityReplayVerificationCapability?
            replayVerificationCapability)
    {
        var runtimeIdentity = BuildManifestRuntimeIdentity(manifest);
        var knnCorpus = ProjectBoundKnnCorpus(manifest);
        var attestation = manifest.ReplayAttestation;
        var executableAttestation =
            manifest.ExecutableManifestAttestation;
        if (!runtimeIdentity.IsSuccess ||
            !knnCorpus.IsSuccess ||
            !HasBoundedExecutablePredicateIds(manifest.PredicateIds) ||
            attestation is null ||
            !attestation.IsCanonical ||
            executableAttestation is null ||
            !executableAttestation.IsCanonical ||
            manifest.UnitRegistrationOrder is null or <= 0 ||
            manifest.FlowRegistrationOrder is null or <= 0)
        {
            return InvalidExecutableManifest();
        }

        try
        {
            var payload = LogicalCapabilityTrainingPreferenceProjector
                .ProjectReplayPayload(
                    attestation.SourceBindings.Values,
                    attestation.ReplaySealGroupSha256,
                    manifest.DomainTag,
                    manifest.SubPhaseId,
                    manifest.ExampleId,
                    manifest.TrainingText,
                    manifest.OutputText,
                    manifest.PredicateIds,
                    manifest.DomainTags);
            if (!payload.IsSuccess || payload.Value is null ||
                !attestation.Matches(payload.Value.Descriptor))
            {
                return InvalidExecutableManifest();
            }

            if (replayVerificationCapability is null)
            {
                return OperationResult<bool>.Fail(
                    "A server replay verification authority is required to restore executable generated capabilities.",
                    GeneratedCapabilityReplayVerifierRequiredErrorCode);
            }

            var verified = replayVerificationCapability.VerifyReplay(
                attestation with
                {
                    SourceBindings =
                        attestation.SourceBindings.Snapshot()
                });
            return verified.IsSuccess && verified.Value
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    "Generated capability replay attestation verification failed.",
                    GeneratedCapabilityReplayAttestationInvalidErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "Generated capability replay attestation verification failed.",
                GeneratedCapabilityReplayAttestationInvalidErrorCode);
        }
    }

    private static OperationResult<GeneratedCapabilityKnnCorpusProjection>
        ProjectBoundKnnCorpus(
            GeneratedCapabilityCheckpointManifest manifest)
    {
        var projected = GeneratedCapabilityKnnCorpusProjector.Project(
            manifest.KnnExamples);
        if (!projected.IsSuccess || projected.Value is null ||
            !RuntimeProvenance.IsSha256Hex(manifest.KnnCorpusSha256) ||
            !RuntimeProvenance.FixedTimeSha256Equals(
                manifest.KnnCorpusSha256!,
                projected.Value.Sha256))
        {
            return OperationResult<GeneratedCapabilityKnnCorpusProjection>.Fail(
                "Generated capability executable manifest schema or provenance is invalid.",
                GeneratedCapabilityExecutableManifestInvalidErrorCode);
        }

        return projected;
    }

    private static OperationResult<bool> ValidateExecutableArtifactBinding(
        GeneratedCapabilityCheckpointManifest manifest,
        LogicalCapabilityReplayVerificationCapability?
            replayVerificationCapability,
        string exactTokenizerArtifactSha256,
        string exactPolicyArtifactSha256)
    {
        if (replayVerificationCapability is null ||
            manifest.ReplayAttestation is null ||
            manifest.ExecutableManifestAttestation is null)
        {
            return InvalidExecutableManifest();
        }

        var executableManifestSha256 =
            GeneratedCapabilityExecutableManifestProjector.ProjectSha256(
                manifest,
                exactTokenizerArtifactSha256,
                exactPolicyArtifactSha256);
        if (!executableManifestSha256.IsSuccess ||
            executableManifestSha256.Value is null)
        {
            return InvalidExecutableManifest();
        }

        try
        {
            var verified = replayVerificationCapability
                .VerifyExecutableManifest(
                    executableManifestSha256.Value,
                    manifest.ReplayAttestation,
                    manifest.ExecutableManifestAttestation);
            return verified.IsSuccess && verified.Value
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    "Generated capability executable manifest attestation verification failed.",
                    GeneratedCapabilityReplayAttestationInvalidErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "Generated capability executable manifest attestation verification failed.",
                GeneratedCapabilityReplayAttestationInvalidErrorCode);
        }
    }

    private static OperationResult<bool> InvalidExecutableManifest()
        => OperationResult<bool>.Fail(
            "Generated capability executable manifest schema or provenance is invalid.",
            GeneratedCapabilityExecutableManifestInvalidErrorCode);

    private static OperationResult<GeneratedCapabilityManifestInventory> EmptyManifestInventory(
        StringComparer pathComparer)
    {
        _ = pathComparer;
        return OperationResult<GeneratedCapabilityManifestInventory>.Ok(
            new GeneratedCapabilityManifestInventory(
                [],
                new Dictionary<string, GeneratedCapabilityCheckpointManifest>(StringComparer.Ordinal)));
    }

    private static string CanonicalizeRoot(string checkpointRoot)
    {
        var fullPath = Path.GetFullPath(checkpointRoot);
        var root = Path.GetPathRoot(fullPath) ?? string.Empty;
        var trimmed = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(
            trimmed,
            root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
            ? root
            : trimmed;
    }

    private static bool IsContained(
        string candidate,
        string canonicalRoot,
        string rootPrefix,
        StringComparison comparison)
    {
        return string.Equals(candidate, canonicalRoot, comparison) ||
               candidate.StartsWith(rootPrefix, comparison);
    }

    private static bool HasRequiredManifestIdentity(GeneratedCapabilityCheckpointManifest? manifest)
    {
        return manifest is not null &&
               ManifestStringsAreBounded(manifest) &&
               ManifestArrayIsBounded(manifest.PredicateIds) &&
               ManifestArrayIsBounded(manifest.DomainTags) &&
               ManifestArrayIsBounded(manifest.LoraTargetModules) &&
               HasRequiredKnnCorpusIdentity(manifest) &&
               !string.IsNullOrWhiteSpace(manifest.UnitId) &&
               !string.IsNullOrWhiteSpace(manifest.DomainTag) &&
               !string.IsNullOrWhiteSpace(manifest.SubPhaseId) &&
               !string.IsNullOrWhiteSpace(manifest.PolicyCheckpointPath) &&
               !string.IsNullOrWhiteSpace(manifest.TokenizerPath);
    }

    private static bool TrySnapshotManifest(
        GeneratedCapabilityCheckpointManifest? source,
        out GeneratedCapabilityCheckpointManifest snapshot)
    {
        snapshot = null!;
        try
        {
            if (!HasRequiredManifestIdentity(source))
                return false;

            var replayAttestation = source!.ReplayAttestation;
            IReadOnlyList<GeneratedCapabilityKnnExample>? knnExamples = null;
            if (source.KnnExamples is not null)
            {
                var knnCorpus = GeneratedCapabilityKnnCorpusProjector.Project(
                    source.KnnExamples);
                if (!knnCorpus.IsSuccess || knnCorpus.Value is null)
                    return false;
                knnExamples = knnCorpus.Value.Examples;
            }
            snapshot = source with
            {
                PredicateIds = source.PredicateIds.ToArray(),
                DomainTags = source.DomainTags.ToArray(),
                LoraTargetModules = source.LoraTargetModules.ToArray(),
                KnnExamples = knnExamples,
                ReplayAttestation = replayAttestation is null
                    ? null
                    : replayAttestation with
                    {
                        SourceBindings =
                            replayAttestation.SourceBindings?.Snapshot() ??
                            new LogicalCapabilityTrainingSourceBindingComposite(
                                null)
                    },
                ExecutableManifestAttestation =
                    source.ExecutableManifestAttestation is null
                        ? null
                        : source.ExecutableManifestAttestation with { }
            };
            return HasRequiredManifestIdentity(snapshot);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            snapshot = null!;
            return false;
        }
    }

    private static bool HasRequiredKnnCorpusIdentity(
        GeneratedCapabilityCheckpointManifest manifest)
    {
        if (manifest.KnnExamples is null ||
            manifest.KnnCorpusSha256 is null)
        {
            return manifest.SchemaVersion !=
                       GeneratedCapabilityCheckpointManifest
                           .CurrentSchemaVersion &&
                   manifest.KnnExamples is null &&
                   manifest.KnnCorpusSha256 is null;
        }

        var projected = GeneratedCapabilityKnnCorpusProjector.Project(
            manifest.KnnExamples);
        return projected.IsSuccess &&
               projected.Value is not null &&
               RuntimeProvenance.IsSha256Hex(manifest.KnnCorpusSha256) &&
               RuntimeProvenance.FixedTimeSha256Equals(
                   manifest.KnnCorpusSha256,
                   projected.Value.Sha256);
    }

    private static bool OpenedHandleMatchesCanonicalPath(
        SafeFileHandle handle, string canonicalPath)
    {
        if (!OperatingSystem.IsWindows())
            return LogicalCapabilityFileCustody.OpenedHandleMatchesCanonicalPath(
                handle,
                canonicalPath);
        var buffer = new StringBuilder(32768);
        var length = GetFinalPathNameByHandle(handle, buffer,
            (uint)buffer.Capacity, 0);
        if (length == 0 || length >= buffer.Capacity)
            return false;
        var value = buffer.ToString();
        if (value.StartsWith("\\\\?\\UNC\\", StringComparison.OrdinalIgnoreCase))
            value = "\\\\" + value[8..];
        else if (value.StartsWith("\\\\?\\", StringComparison.OrdinalIgnoreCase))
            value = value[4..];
        return StringComparer.OrdinalIgnoreCase.Equals(
            Path.GetFullPath(value), Path.GetFullPath(canonicalPath));
    }

    private static bool SameFileIdentity(SafeFileHandle left,
        SafeFileHandle right)
        => !OperatingSystem.IsWindows()
            ? LogicalCapabilityFileCustody.StreamIdentitiesMatch(left, right)
            : GetFileInformationByHandle(left, out var leftInfo) &&
           GetFileInformationByHandle(right, out var rightInfo) &&
           SameFileIdentity(leftInfo, rightInfo);

    private static bool SameFileIdentity(
        ByHandleFileInformation left,
        ByHandleFileInformation right)
        => left.VolumeSerialNumber == right.VolumeSerialNumber &&
           left.FileIndexHigh == right.FileIndexHigh &&
           left.FileIndexLow == right.FileIndexLow;

    private sealed class ManifestPublicationDirectoryCustody
    {
        private readonly string _canonicalRoot;
        private readonly string[] _segments;
        private readonly List<SafeFileHandle> _handles;

        private ManifestPublicationDirectoryCustody(
            string canonicalRoot,
            string[] segments,
            List<SafeFileHandle> handles)
        {
            _canonicalRoot = canonicalRoot;
            _segments = segments;
            _handles = handles;
        }

        internal SafeFileHandle DirectoryHandle => _handles[^1];

        internal static ManifestPublicationDirectoryCustody OpenOrCreate(
            string canonicalDirectory)
        {
            var root = Path.GetPathRoot(canonicalDirectory);
            if (string.IsNullOrWhiteSpace(root))
                throw ManifestDirectoryCustodyFailure();
            root = Path.GetFullPath(root);
            var relative = Path.GetRelativePath(root, canonicalDirectory);
            if (Path.IsPathRooted(relative) ||
                string.Equals(relative, "..", StringComparison.Ordinal) ||
                relative.StartsWith(
                    ".." + Path.DirectorySeparatorChar,
                    StringComparison.Ordinal) ||
                relative.StartsWith(
                    ".." + Path.AltDirectorySeparatorChar,
                    StringComparison.Ordinal))
            {
                throw ManifestDirectoryCustodyFailure();
            }

            var segments = string.Equals(relative, ".", StringComparison.Ordinal)
                ? []
                : relative.Split(
                    [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                    StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > MaximumManifestTraversalDepth)
                throw ManifestDirectoryCustodyFailure();
            var provisional = new List<SafeFileHandle>(segments.Length + 1);
            var handles = new List<SafeFileHandle>(segments.Length + 1);
            try
            {
                var provisionalRoot = OpenManifestDirectoryRoot(
                    root,
                    ManifestDirectoryReadAccess,
                    ManifestShareAll);
                try
                {
                    VerifyManifestDirectoryHandle(provisionalRoot);
                    provisional.Add(provisionalRoot);
                }
                catch
                {
                    provisionalRoot.Dispose();
                    throw;
                }

                var firstMissingIndex = segments.Length;
                for (var index = 0; index < segments.Length; index++)
                {
                    SafeFileHandle child;
                    try
                    {
                        child = OpenManifestRelativeHandle(
                            provisional[^1],
                            segments[index],
                            ManifestDirectoryReadAccess,
                            ManifestShareAll,
                            1,
                            ManifestDirectoryOpenOptions,
                            "Generated capability manifest directory discovery failed.");
                    }
                    catch (IOException exception)
                        when (IsManifestObjectNotFound(exception))
                    {
                        firstMissingIndex = index;
                        break;
                    }
                    try
                    {
                        VerifyManifestDirectoryHandle(child);
                        provisional.Add(child);
                    }
                    catch
                    {
                        child.Dispose();
                        throw;
                    }
                }

                var rootAccess = ManifestDirectoryReadAccess;
                if (segments.Length == 0)
                    rootAccess |= ManifestDirectoryAddFileAccess;
                else if (firstMissingIndex == 0)
                    rootAccess |= ManifestDirectoryAddSubdirectoryAccess;
                var rootHandle = OpenManifestDirectoryRoot(
                    root,
                    rootAccess,
                    ManifestShareReadWriteDenyDelete);
                try
                {
                    VerifyManifestDirectoryHandle(rootHandle);
                    if (!SameFileIdentity(provisional[0], rootHandle))
                        throw ManifestDirectoryCustodyFailure();
                    handles.Add(rootHandle);
                }
                catch
                {
                    rootHandle.Dispose();
                    throw;
                }

                for (var index = 0; index < firstMissingIndex; index++)
                {
                    var desiredAccess = ManifestDirectoryReadAccess;
                    if (firstMissingIndex < segments.Length &&
                        index == firstMissingIndex - 1)
                    {
                        desiredAccess |= ManifestDirectoryAddSubdirectoryAccess;
                    }
                    else if (firstMissingIndex == segments.Length &&
                             index == segments.Length - 1)
                    {
                        desiredAccess |= ManifestDirectoryAddFileAccess;
                    }
                    var child = OpenManifestRelativeHandle(
                        handles[^1],
                        segments[index],
                        desiredAccess,
                        ManifestShareReadWriteDenyDelete,
                        1,
                        ManifestDirectoryOpenOptions,
                        "Generated capability manifest directory custody failed.");
                    try
                    {
                        VerifyManifestDirectoryHandle(child);
                        if (!SameFileIdentity(provisional[index + 1], child))
                            throw ManifestDirectoryCustodyFailure();
                        handles.Add(child);
                    }
                    catch
                    {
                        child.Dispose();
                        throw;
                    }
                }

                for (var index = firstMissingIndex;
                     index < segments.Length;
                     index++)
                {
                    var desiredAccess = ManifestDirectoryReadAccess |
                        (index == segments.Length - 1
                            ? ManifestDirectoryAddFileAccess
                            : ManifestDirectoryAddSubdirectoryAccess);
                    var child = OpenManifestRelativeHandle(
                        handles[^1],
                        segments[index],
                        desiredAccess,
                        ManifestShareReadWriteDenyDelete,
                        3,
                        ManifestDirectoryOpenOptions,
                        "Generated capability manifest directory custody failed.");
                    try
                    {
                        VerifyManifestDirectoryHandle(child);
                        handles.Add(child);
                    }
                    catch
                    {
                        child.Dispose();
                        throw;
                    }
                }
                return new ManifestPublicationDirectoryCustody(
                    root,
                    segments,
                    handles);
            }
            catch
            {
                for (var index = handles.Count - 1; index >= 0; index--)
                {
                    try { handles[index].Dispose(); }
                    catch { }
                }
                throw;
            }
            finally
            {
                for (var index = provisional.Count - 1; index >= 0; index--)
                    provisional[index].Dispose();
            }
        }

        internal void EnsurePathIdentityUnchanged()
        {
            var reopened = new List<SafeFileHandle>(_handles.Count);
            try
            {
                var rootHandle = OpenManifestDirectoryRoot(
                    _canonicalRoot,
                    ManifestDirectoryReadAccess,
                    ManifestShareAll);
                try
                {
                    VerifyManifestDirectoryHandle(rootHandle);
                    if (!SameFileIdentity(_handles[0], rootHandle))
                        throw ManifestDirectoryCustodyFailure();
                    reopened.Add(rootHandle);
                }
                catch
                {
                    rootHandle.Dispose();
                    throw;
                }

                for (var index = 0; index < _segments.Length; index++)
                {
                    var child = OpenManifestRelativeHandle(
                        reopened[^1],
                        _segments[index],
                        ManifestDirectoryReadAccess,
                        ManifestShareAll,
                        1,
                        ManifestDirectoryOpenOptions,
                        "Generated capability manifest directory identity changed.");
                    try
                    {
                        VerifyManifestDirectoryHandle(child);
                        reopened.Add(child);
                    }
                    catch
                    {
                        child.Dispose();
                        throw;
                    }
                    if (!SameFileIdentity(_handles[index + 1], child))
                        throw ManifestDirectoryCustodyFailure();
                }
            }
            finally
            {
                for (var index = reopened.Count - 1; index >= 0; index--)
                    reopened[index].Dispose();
            }
        }

        internal void DisposeInto(ManifestPublicationCleanupArbiter failures)
        {
            for (var index = _handles.Count - 1; index >= 0; index--)
            {
                try { _handles[index].Dispose(); }
                catch (Exception exception) { failures.CaptureCleanup(exception); }
            }
            _handles.Clear();
        }
    }

    private static SafeFileHandle OpenManifestDirectoryRoot(
        string root,
        uint desiredAccess,
        uint shareAccess)
    {
        var handle = CreateManifestCleanupHandle(
            root,
            desiredAccess,
            shareAccess,
            IntPtr.Zero,
            3,
            0x02000000 | 0x00200000,
            IntPtr.Zero);
        if (!handle.IsInvalid)
            return handle;
        var error = Marshal.GetLastWin32Error();
        handle.Dispose();
        throw new IOException(
            "Generated capability manifest root custody failed.",
            new System.ComponentModel.Win32Exception(error));
    }

    private static void VerifyManifestDirectoryHandle(SafeFileHandle handle)
    {
        if (!GetFileInformationByHandle(handle, out var information) ||
            (information.FileAttributes & 0x00000010) == 0 ||
            (information.FileAttributes & 0x00000400) != 0)
        {
            throw ManifestDirectoryCustodyFailure();
        }
    }

    private static IOException ManifestDirectoryCustodyFailure()
        => new("Generated capability manifest directory custody failed.");

    private static bool IsManifestPublicationCollision(IOException exception)
        => exception.InnerException is System.ComponentModel.Win32Exception
               { NativeErrorCode: 80 or 183 };

    private static bool IsManifestObjectNotFound(IOException exception)
        => exception.InnerException is System.ComponentModel.Win32Exception
               { NativeErrorCode: 2 or 3 };

    private static void RenameManifestHandleWithoutOverwrite(
        SafeFileHandle handle,
        SafeFileHandle directoryHandle,
        string fileName)
    {
        ValidateManifestRelativeName(fileName);
        var fileNameBytes = Encoding.Unicode.GetBytes(fileName);
        var rootDirectoryOffset = IntPtr.Size == 8 ? 8 : 4;
        var fileNameLengthOffset = rootDirectoryOffset + IntPtr.Size;
        var fileNameOffset = fileNameLengthOffset + sizeof(uint);
        var bufferSize = checked(fileNameOffset + fileNameBytes.Length);
        var buffer = Marshal.AllocHGlobal(bufferSize);
        var directoryHandleAddRef = false;
        try
        {
            for (var offset = 0; offset < bufferSize; offset++)
                Marshal.WriteByte(buffer, offset, 0);
            directoryHandle.DangerousAddRef(ref directoryHandleAddRef);
            Marshal.WriteIntPtr(
                buffer,
                rootDirectoryOffset,
                directoryHandle.DangerousGetHandle());
            Marshal.WriteInt32(
                buffer,
                fileNameLengthOffset,
                fileNameBytes.Length);
            Marshal.Copy(fileNameBytes, 0, IntPtr.Add(buffer, fileNameOffset),
                fileNameBytes.Length);
            var status = NtSetManifestInformationFile(
                handle,
                out _,
                buffer,
                checked((uint)bufferSize),
                10);
            if (status < 0)
            {
                var error = checked((int)RtlNtStatusToDosError(status));
                throw new IOException(
                    "Generated capability manifest handle publication failed.",
                    new System.ComponentModel.Win32Exception(error));
            }
        }
        finally
        {
            if (directoryHandleAddRef)
                directoryHandle.DangerousRelease();
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static SafeFileHandle OpenManifestRelativeHandle(
        SafeFileHandle directoryHandle,
        string fileName,
        uint desiredAccess,
        uint shareAccess,
        uint createDisposition,
        uint createOptions,
        string failureMessage)
    {
        ValidateManifestRelativeName(fileName);
        var fileNameBuffer = Marshal.StringToHGlobalUni(fileName);
        var unicodeStringBuffer = IntPtr.Zero;
        IntPtr rawHandle = IntPtr.Zero;
        try
        {
            var nameLength = checked((ushort)(fileName.Length * sizeof(char)));
            var unicodeName = new ManifestUnicodeString
            {
                Length = nameLength,
                MaximumLength = checked((ushort)(nameLength + sizeof(char))),
                Buffer = fileNameBuffer,
            };
            unicodeStringBuffer = Marshal.AllocHGlobal(
                Marshal.SizeOf<ManifestUnicodeString>());
            Marshal.StructureToPtr(
                unicodeName,
                unicodeStringBuffer,
                fDeleteOld: false);
            var attributes = new ManifestObjectAttributes
            {
                Length = checked((uint)Marshal.SizeOf<ManifestObjectAttributes>()),
                RootDirectory = directoryHandle.DangerousGetHandle(),
                ObjectName = unicodeStringBuffer,
                Attributes = 0x00000040,
            };
            var status = NtCreateManifestFile(
                out rawHandle,
                desiredAccess,
                ref attributes,
                out _,
                IntPtr.Zero,
                0x00000080,
                shareAccess,
                createDisposition,
                createOptions,
                IntPtr.Zero,
                0);
            if (status < 0 ||
                rawHandle == IntPtr.Zero ||
                rawHandle == new IntPtr(-1))
            {
                if (rawHandle != IntPtr.Zero && rawHandle != new IntPtr(-1))
                {
                    using var unexpected = new SafeFileHandle(
                        rawHandle,
                        ownsHandle: true);
                    rawHandle = IntPtr.Zero;
                }
                var error = status < 0
                    ? checked((int)RtlNtStatusToDosError(status))
                    : 6;
                throw new IOException(
                    failureMessage,
                    new System.ComponentModel.Win32Exception(error));
            }

            var handle = new SafeFileHandle(rawHandle, ownsHandle: true);
            rawHandle = IntPtr.Zero;
            return handle;
        }
        finally
        {
            if (rawHandle != IntPtr.Zero && rawHandle != new IntPtr(-1))
                new SafeFileHandle(rawHandle, ownsHandle: true).Dispose();
            if (unicodeStringBuffer != IntPtr.Zero)
                Marshal.FreeHGlobal(unicodeStringBuffer);
            Marshal.FreeHGlobal(fileNameBuffer);
        }
    }

    private static void ValidateManifestRelativeName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Length > MaximumManifestPublicationComponentLength ||
            string.Equals(fileName, ".", StringComparison.Ordinal) ||
            string.Equals(fileName, "..", StringComparison.Ordinal) ||
            fileName.EndsWith(' ') ||
            fileName.EndsWith('.') ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            fileName.Contains(Path.DirectorySeparatorChar) ||
            fileName.Contains(Path.AltDirectorySeparatorChar))
        {
            throw ManifestDirectoryCustodyFailure();
        }
    }

    private static bool TryScheduleManifestDeletion(
        SafeFileHandle handle,
        out int legacyError,
        out int extendedError)
    {
        var disposition = new FileDispositionInformation { DeleteFile = 1 };
        if (SetFileInformationByHandle(
                handle,
                4,
                ref disposition,
                checked((uint)Marshal.SizeOf<FileDispositionInformation>())))
        {
            legacyError = 0;
            extendedError = 0;
            return true;
        }

        legacyError = Marshal.GetLastWin32Error();
        var extendedDisposition = new FileDispositionInformationEx
        {
            Flags = 0x00000001 | 0x00000010
        };
        if (SetFileInformationByHandleExtended(
                handle,
                21,
                ref extendedDisposition,
                checked((uint)Marshal.SizeOf<FileDispositionInformationEx>())))
        {
            extendedError = 0;
            return true;
        }
        extendedError = Marshal.GetLastWin32Error();
        return false;
    }

    private static void TryDeleteManifestTempByRevalidatedHandle(
        SafeFileHandle directoryHandle,
        string temporaryFileName,
        ByHandleFileInformation expectedIdentity,
        int priorLegacyError,
        int priorExtendedError,
        ManifestPublicationCleanupArbiter failures)
    {
        try
        {
            using var current = OpenManifestRelativeHandle(
                directoryHandle,
                temporaryFileName,
                0x00010000 | 0x00100000 | 0x00000080,
                0x00000001 | 0x00000002,
                1,
                0x00000020 | 0x00000040 | 0x00200000,
                "Generated capability manifest temporary cleanup reopen failed.");
            if (!GetFileInformationByHandle(current, out var currentIdentity))
            {
                failures.CaptureCleanup(CreateManifestCleanupException(
                    priorLegacyError,
                    priorExtendedError,
                    Marshal.GetLastWin32Error()));
                return;
            }
            if (!SameFileIdentity(expectedIdentity, currentIdentity))
                return;
            if (!TryScheduleManifestDeletion(
                    current, out var legacyError, out var extendedError))
            {
                failures.CaptureCleanup(CreateManifestCleanupException(
                    legacyError,
                    extendedError,
                    0));
            }
        }
        catch (IOException exception)
            when (exception.InnerException is System.ComponentModel.Win32Exception
                  { NativeErrorCode: 2 or 3 })
        {
            // The exact retained temporary name is already absent.
        }
        catch (Exception exception)
        {
            failures.CaptureCleanup(exception);
        }
    }

    private static IOException CreateManifestCleanupException(
        int legacyError,
        int extendedError,
        int reopenError)
        => new(
            "Generated capability manifest temporary cleanup failed safely.",
            new AggregateException(
                new System.ComponentModel.Win32Exception(legacyError),
                new System.ComponentModel.Win32Exception(extendedError),
                new System.ComponentModel.Win32Exception(reopenError)));

    private static void ReportManifestTempCleanupFailure()
    {
        try
        {
            System.Diagnostics.Trace.TraceWarning(
                "GENERATED_CAPABILITY_MANIFEST_TEMP_CLEANUP_FAILED_SAFE");
        }
        catch
        {
            // Diagnostics cannot replace the stable publication outcome.
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ManifestUnicodeString
    {
        internal ushort Length;
        internal ushort MaximumLength;
        internal IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ManifestObjectAttributes
    {
        internal uint Length;
        internal IntPtr RootDirectory;
        internal IntPtr ObjectName;
        internal uint Attributes;
        internal IntPtr SecurityDescriptor;
        internal IntPtr SecurityQualityOfService;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ManifestIoStatusBlock
    {
        internal IntPtr Status;
        internal IntPtr Information;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ByHandleFileInformation
    {
        internal uint FileAttributes;
        internal System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
        internal uint VolumeSerialNumber;
        internal uint FileSizeHigh;
        internal uint FileSizeLow;
        internal uint NumberOfLinks;
        internal uint FileIndexHigh;
        internal uint FileIndexLow;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandle(SafeFileHandle handle,
        StringBuilder path, uint pathLength, uint flags);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(SafeFileHandle handle,
        out ByHandleFileInformation information);

    [StructLayout(LayoutKind.Sequential)]
    private struct FileDispositionInformation
    {
        internal byte DeleteFile;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileDispositionInformationEx
    {
        internal uint Flags;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateManifestCleanupHandle(
        string fileName, uint desiredAccess, uint shareMode,
        IntPtr securityAttributes, uint creationDisposition,
        uint flagsAndAttributes, IntPtr templateFile);

    [DllImport("ntdll.dll", EntryPoint = "NtCreateFile")]
    private static extern int NtCreateManifestFile(
        out IntPtr fileHandle,
        uint desiredAccess,
        ref ManifestObjectAttributes objectAttributes,
        out ManifestIoStatusBlock ioStatusBlock,
        IntPtr allocationSize,
        uint fileAttributes,
        uint shareAccess,
        uint createDisposition,
        uint createOptions,
        IntPtr eaBuffer,
        uint eaLength);

    [DllImport("ntdll.dll", EntryPoint = "NtSetInformationFile")]
    private static extern int NtSetManifestInformationFile(
        SafeFileHandle fileHandle,
        out ManifestIoStatusBlock ioStatusBlock,
        IntPtr fileInformation,
        uint length,
        int fileInformationClass);

    [DllImport("ntdll.dll")]
    private static extern uint RtlNtStatusToDosError(int status);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandle(
        SafeFileHandle file, int fileInformationClass,
        ref FileDispositionInformation fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "SetFileInformationByHandle",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandleExtended(
        SafeFileHandle file, int fileInformationClass,
        ref FileDispositionInformationEx fileInformation,
        uint bufferSize);

    private static bool ManifestArrayIsBounded(string[]? values)
        => values is not null && values.Length <= MaximumManifestArrayCount &&
           values.All(static value => value is not null &&
               value.Length <= MaximumManifestStringLength);

    internal static bool HasBoundedExecutablePredicateIds(
        IReadOnlyList<string>? predicateIds)
        => predicateIds is { Count: > 0 } &&
           predicateIds.Count <= MaximumManifestArrayCount &&
           predicateIds.All(static value =>
               !string.IsNullOrWhiteSpace(value) &&
               value.Length <= MaximumManifestStringLength);

    private static bool ManifestStringsAreBounded(
        GeneratedCapabilityCheckpointManifest manifest)
    {
        string?[] values =
        [
            manifest.UnitId, manifest.UnitRole, manifest.DomainTag,
            manifest.Version, manifest.TrustState, manifest.FlowId,
            manifest.ScenarioId, manifest.PhaseId, manifest.SubPhaseId,
            manifest.TaskType,
            manifest.ExampleId, manifest.PolicyCheckpointPath,
            manifest.TokenizerPath, manifest.UnitFamilyId,
            manifest.ParentUnitId, manifest.ParentPolicyCheckpointPath,
            manifest.ParentTokenizerPath,
            manifest.ServerBaseUrl, manifest.ServerUnitEndpoint,
            manifest.ManifestSha256, manifest.TrainingBranch,
            manifest.TrainingRunId, manifest.SourceDatasetSha256,
            manifest.TokenizerSha256, manifest.PolicyCheckpointSha256,
            manifest.RuntimeProvenanceSha256, manifest.KnnCorpusSha256
        ];
        return values.All(static value => value is null ||
                   value.Length <= MaximumManifestStringLength) &&
               manifest.TrainingText is not null &&
               manifest.TrainingText.Length <= MaximumManifestBytes &&
               manifest.OutputText is not null &&
               manifest.OutputText.Length <= MaximumManifestBytes &&
               manifest.OriginalPrompt is not null &&
               manifest.OriginalPrompt.Length <= MaximumManifestBytes;
    }

    private static OperationResult<LogicalUnitTrustState> ParseTrustState(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return OperationResult<LogicalUnitTrustState>.Fail(
                "Trust state is invalid.",
                "TRUST_STATE_INVALID");
        }

        foreach (var name in Enum.GetNames<LogicalUnitTrustState>())
        {
            if (string.Equals(value, name, StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse<LogicalUnitTrustState>(name, out var state) &&
                Enum.IsDefined(state))
            {
                return OperationResult<LogicalUnitTrustState>.Ok(state);
            }
        }

        return OperationResult<LogicalUnitTrustState>.Fail(
            "Trust state is invalid.",
            "TRUST_STATE_INVALID");
    }

    private static bool IsExactGeneratedCapabilityBinding(
        LogicalUnitDescriptor descriptor,
        GeneratedCapabilityCheckpointManifest manifest,
        LogicalUnitTrustState manifestTrustState)
    {
        return string.Equals(descriptor.UnitId, manifest.UnitId, StringComparison.Ordinal) &&
               string.Equals(descriptor.UnitRole, DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole, StringComparison.Ordinal) &&
               string.Equals(manifest.UnitRole, DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole, StringComparison.Ordinal) &&
               string.Equals(descriptor.UnitRole, manifest.UnitRole, StringComparison.Ordinal) &&
               string.Equals(descriptor.DomainTag, manifest.DomainTag, StringComparison.Ordinal) &&
               string.Equals(descriptor.Version, manifest.Version, StringComparison.Ordinal) &&
               descriptor.TrustState == manifestTrustState &&
               manifest.UnitRegistrationOrder is not null &&
               descriptor.RegistrationOrder ==
                   manifest.UnitRegistrationOrder.Value &&
               !descriptor.IsLearned &&
               descriptor.InputType == typeof(string) &&
               descriptor.OutputType == typeof(string);
    }

    private static bool IsExactGeneratedCapabilityBinding(
        TeachingUnitRegistryEntry entry,
        GeneratedCapabilityCheckpointManifest manifest,
        LogicalUnitTrustState entryTrustState,
        LogicalUnitTrustState manifestTrustState)
    {
        return string.Equals(entry.UnitId, manifest.UnitId, StringComparison.Ordinal) &&
               string.Equals(entry.UnitRole, DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole, StringComparison.Ordinal) &&
               string.Equals(manifest.UnitRole, DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole, StringComparison.Ordinal) &&
               string.Equals(entry.UnitRole, manifest.UnitRole, StringComparison.Ordinal) &&
               string.Equals(entry.DomainTag, manifest.DomainTag, StringComparison.Ordinal) &&
               string.Equals(entry.Version, manifest.Version, StringComparison.Ordinal) &&
               entryTrustState == manifestTrustState &&
               manifest.UnitRegistrationOrder is not null &&
               entry.RegistrationOrder ==
                   manifest.UnitRegistrationOrder.Value &&
               !entry.IsLearned &&
               string.Equals(entry.InputTypeName, TypeName(typeof(string)), StringComparison.Ordinal) &&
               string.Equals(entry.OutputTypeName, TypeName(typeof(string)), StringComparison.Ordinal);
    }

    private static bool IsExactGeneratedCapabilityFlowBinding(
        TeachingFlowRegistryEntry entry,
        GeneratedCapabilityCheckpointManifest manifest)
    {
        return entry.DomainTags is not null &&
               manifest.DomainTags is not null &&
               string.Equals(entry.FlowId, manifest.FlowId, StringComparison.Ordinal) &&
               string.Equals(entry.ScenarioId, manifest.ScenarioId, StringComparison.Ordinal) &&
               string.Equals(entry.Version, manifest.Version, StringComparison.Ordinal) &&
               string.Equals(entry.InputTypeName, TypeName(typeof(string)), StringComparison.Ordinal) &&
               string.Equals(entry.PhaseId, manifest.PhaseId, StringComparison.Ordinal) &&
               string.Equals(entry.SubPhaseId, manifest.SubPhaseId, StringComparison.Ordinal) &&
               string.Equals(entry.TaskType, manifest.TaskType, StringComparison.Ordinal) &&
               manifest.FlowRegistrationOrder is not null &&
               entry.RegistrationOrder ==
                   manifest.FlowRegistrationOrder.Value &&
               entry.DomainTags.SequenceEqual(
                   manifest.DomainTags,
                   StringComparer.Ordinal) &&
               string.Equals(
                   entry.FlowKind,
                   TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow,
                   StringComparison.Ordinal);
    }

    private static bool HasCompleteGeneratedCapabilityBindings(
        IReadOnlyList<TeachingUnitRegistryEntry> units,
        IReadOnlyList<TeachingFlowRegistryEntry> flows,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> manifests)
    {
        var generatedUnits = units
            .Where(entry => string.Equals(
                entry.UnitKind,
                TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit,
                StringComparison.Ordinal))
            .ToArray();
        var generatedFlows = flows
            .Where(entry => string.Equals(
                entry.FlowKind,
                TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow,
                StringComparison.Ordinal))
            .ToArray();
        if (generatedUnits.Length != manifests.Count ||
            generatedFlows.Length != manifests.Count)
        {
            return false;
        }

        foreach (var manifest in manifests)
        {
            var manifestTrustState = ParseTrustState(manifest.TrustState);
            if (!manifestTrustState.IsSuccess)
                return false;
            var exactUnits = generatedUnits.Count(entry =>
            {
                var entryTrustState = ParseTrustState(entry.TrustState);
                return entryTrustState.IsSuccess &&
                       IsExactGeneratedCapabilityBinding(
                           entry,
                           manifest,
                           entryTrustState.Value,
                           manifestTrustState.Value);
            });
            var exactFlows = generatedFlows.Count(entry =>
                IsExactGeneratedCapabilityFlowBinding(entry, manifest));
            if (exactUnits != 1 || exactFlows != 1)
                return false;
        }

        return true;
    }

    private static string TypeName(Type type)
    {
        return type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
    }

    private static OperationResult<int> GeneratedCapabilityRestoreFailure()
    {
        return OperationResult<int>.Fail(
            "Generated capability snapshot binding is invalid.",
            GeneratedCapabilityRestoreInvalid);
    }

    private static string DetectFlowKind(LogicalFlowDescriptor descriptor)
    {
        if (descriptor.FlowId.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, StringComparison.Ordinal))
            return TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow;
        if (descriptor.FlowId.StartsWith(UniversalNineNodeDevelopmentFlow.FlowIdPrefix, StringComparison.Ordinal))
            return TeachingRegistrySnapshotKinds.UniversalNineNodeFlow;

        return TeachingRegistrySnapshotKinds.RegisteredFlow;
    }
}
