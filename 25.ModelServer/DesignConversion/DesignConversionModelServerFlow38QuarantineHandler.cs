// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]

namespace ModelServer.DesignConversion;

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32.SafeHandles;

public sealed record DesignConversionFlow38QuarantinedSourceRow(
    string EventId,
    string CorrelationId,
    string TenantScope,
    string CycleId,
    string PatternId,
    string RetrievalId,
    string PromptHash,
    string ChosenOutputRef,
    string ChosenOutputHash,
    string RejectedOutputRef,
    string RejectedOutputHash,
    string SourceCategory,
    string TermsSnapshotRef,
    string OutputLicenseRef,
    IReadOnlyList<string> ProvenanceRefs,
    string RedactionPolicyRef,
    string ChosenOrigin,
    string RejectedOrigin,
    string LegalVerdict,
    string AllowedUse,
    string Route,
    bool SemanticQualityClaimed,
    bool DpoTrainReady);

internal enum DesignConversionFlow38SourceCategory
{
    Local,
    Human,
    OpenWeight,
    Hosted,
    DeterministicBaseline,
    SelfJudge,
    ClientUnverified,
    ThirdParty
}

internal static class DesignConversionFlow38SourceCategoryCanon
{
    internal const string Local = "local";
    internal const string Human = "human";
    internal const string OpenWeight = "open_weight";
    internal const string Hosted = "hosted";
    internal const string DeterministicBaseline = "deterministic_baseline";
    internal const string SelfJudge = "self_judge";
    internal const string ClientUnverified = "client_unverified";
    internal const string ThirdParty = "third_party";

    private static readonly string[] Values =
    [
        Local,
        Human,
        OpenWeight,
    Hosted,
    DeterministicBaseline,
    SelfJudge,
    ClientUnverified,
    ThirdParty
];

    internal static IReadOnlyList<string> CanonicalValues => Values;

    internal static DesignConversionFlow38SourceCategory ParseExact(string? value)
    {
        if (TryParseExact(value, out var category))
            return category;

        throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown Flow38 source category.");
    }

    internal static bool TryParseExact(
        string? value,
        out DesignConversionFlow38SourceCategory category)
    {
        category = default;
        switch (value)
        {
            case Local:
                category = DesignConversionFlow38SourceCategory.Local;
                return true;
            case Human:
                category = DesignConversionFlow38SourceCategory.Human;
                return true;
            case OpenWeight:
                category = DesignConversionFlow38SourceCategory.OpenWeight;
                return true;
            case Hosted:
                category = DesignConversionFlow38SourceCategory.Hosted;
                return true;
            case DeterministicBaseline:
                category = DesignConversionFlow38SourceCategory.DeterministicBaseline;
                return true;
            case SelfJudge:
                category = DesignConversionFlow38SourceCategory.SelfJudge;
                return true;
            case ClientUnverified:
                category = DesignConversionFlow38SourceCategory.ClientUnverified;
                return true;
            case ThirdParty:
                category = DesignConversionFlow38SourceCategory.ThirdParty;
                return true;
            default:
                return false;
        }
    }

    internal static bool IsQuarantineIntakeEligible(
        DesignConversionFlow38SourceCategory category)
        => category is DesignConversionFlow38SourceCategory.ClientUnverified or
            DesignConversionFlow38SourceCategory.ThirdParty;
}

public sealed record DesignConversionFlow38QuarantineStoreResult(
    string EventId,
    string RecordRef,
    string Fingerprint,
    bool Created,
    bool Replayed,
    bool Conflict);

public interface IDesignConversionFlow38QuarantineStore
{
    Task<DesignConversionFlow38QuarantineStoreResult> AdmitAsync(
        DesignConversionFlow38QuarantinedSourceRow row,
        CancellationToken cancellationToken = default);
}

public sealed record DesignConversionFlow38QuarantineStoreOptions
{
    public const long DefaultMaximumStoreBytes = 64L * 1024 * 1024;
    public const int DefaultMaximumRecordCount = 100_000;

    public long MaximumStoreBytes { get; init; } = DefaultMaximumStoreBytes;

    public int MaximumRecordCount { get; init; } = DefaultMaximumRecordCount;

    internal Action<DesignConversionFlow38SafeOpenStage, string>? BeforeSafeOpenForTesting { get; init; }

    internal DesignConversionFlow38StoragePlatform? StoragePlatformForTesting { get; init; }
}

internal enum DesignConversionFlow38SafeOpenStage
{
    RootLease,
    RootSegmentCreate,
    LockFile,
    RecordRead,
    RecordAppend
}

internal enum DesignConversionFlow38StoragePlatform
{
    Windows,
    Linux,
    MacOS,
    Unsupported
}

internal readonly record struct DesignConversionFlow38UnixNativeFlags(
    int OpenReadOnly,
    int OpenReadWrite,
    int OpenCreate,
    int OpenExclusive,
    int OpenDirectory,
    int OpenNoFollow,
    int OpenCloseOnExec,
    int FlockExclusive,
    int FlockNonBlocking)
{
    internal int DirectoryOpenFlags => OpenReadOnly | OpenDirectory | OpenNoFollow | OpenCloseOnExec;

    internal int FileOpenFlags => OpenReadWrite | OpenNoFollow | OpenCloseOnExec;

    internal int ExclusiveNonBlockingLock => FlockExclusive | FlockNonBlocking;
}

public sealed class JsonlDesignConversionFlow38QuarantineStore : IDesignConversionFlow38QuarantineStore
{
    public const string SchemaVersion = "xiigen.flow38.quarantined-source-store.v1";
    public const string FileName = "flow38-quarantined-source-rows.jsonl";
    public const string LockFileName = "flow38-quarantined-source-rows.lock";
    public const int MaximumPersistedLineBytes = 64 * 1024;

    private const int ReadBufferBytes = 4096;
    private const int LockRetryDelayMilliseconds = 20;
    private const int MaximumLockAttempts = 250;

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> ProcessGates = new(
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    private static readonly HashSet<string> ExpectedEnvelopeProperties = new(StringComparer.Ordinal)
    {
        "schemaVersion", "eventId", "tenantScope", "recordRef", "fingerprint", "admittedAtUtc",
        "legalVerdict", "allowedUse", "route", "semanticQualityClaimed", "dpoTrainReady", "source"
    };
    private static readonly HashSet<string> ExpectedSourceProperties = new(StringComparer.Ordinal)
    {
        "eventId", "correlationId", "tenantScope", "cycleId", "patternId", "retrievalId",
        "promptHash", "chosenOutputRef", "chosenOutputHash", "rejectedOutputRef",
        "rejectedOutputHash", "sourceCategory", "termsSnapshotRef", "outputLicenseRef",
        "provenanceRefs", "redactionPolicyRef", "chosenOrigin", "rejectedOrigin",
        "legalVerdict", "allowedUse", "route", "semanticQualityClaimed", "dpoTrainReady"
    };

    private readonly string _rootDirectory;
    private readonly string _recordsPath;
    private readonly string _lockPath;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly DesignConversionFlow38QuarantineStoreOptions _options;
    private readonly SemaphoreSlim _processGate;

    public JsonlDesignConversionFlow38QuarantineStore(
        string rootDirectory,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionFlow38QuarantineStoreOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("FLOW-38 quarantine store root is required.", nameof(rootDirectory));

        _rootDirectory = Path.GetFullPath(rootDirectory);
        _recordsPath = Path.Combine(_rootDirectory, FileName);
        _lockPath = Path.Combine(_rootDirectory, LockFileName);
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _options = options ?? new DesignConversionFlow38QuarantineStoreOptions();
        if (_options.MaximumStoreBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum FLOW-38 quarantine store bytes must be positive.");
        if (_options.MaximumRecordCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum FLOW-38 quarantine record count must be positive.");

        EnsureCanonicalContainment(_recordsPath);
        EnsureCanonicalContainment(_lockPath);
        _processGate = ProcessGates.GetOrAdd(_recordsPath, static _ => new SemaphoreSlim(1, 1));
    }

    public async Task<DesignConversionFlow38QuarantineStoreResult> AdmitAsync(
        DesignConversionFlow38QuarantinedSourceRow row,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(row);
        cancellationToken.ThrowIfCancellationRequested();

        // Capture caller-owned collections before the first await. All subsequent work uses only
        // this owned immutable snapshot, so caller mutation cannot alter identity or persistence.
        var snapshot = Snapshot(row);
        if (!DesignConversionModelServerFlow38QuarantineHandler.IsPersistableQuarantineRow(snapshot))
            throw new InvalidDataException("FLOW-38 quarantine row is not safe to persist.");

        var fingerprint = Fingerprint(snapshot);
        var recordRef = RecordRef(snapshot);
        await _processGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            EnsureCanonicalContainment(_recordsPath);
            EnsureCanonicalContainment(_lockPath);
            InvokeBeforeSafeOpen(DesignConversionFlow38SafeOpenStage.RootLease, _rootDirectory);
            using var rootLease = SafeStoreDirectoryLease.AcquireOrCreate(
                _rootDirectory,
                _options.StoragePlatformForTesting,
                _options.BeforeSafeOpenForTesting);
            await using var crossProcessLock = await AcquireCrossProcessLockAsync(rootLease, cancellationToken).ConfigureAwait(false);

            var scan = await ScanStoreAsync(rootLease, snapshot, cancellationToken).ConfigureAwait(false);
            if (scan.Existing is not null)
            {
                var matches = string.Equals(scan.Existing.Fingerprint, fingerprint, StringComparison.Ordinal);
                return new DesignConversionFlow38QuarantineStoreResult(
                    snapshot.EventId,
                    scan.Existing.RecordRef,
                    fingerprint,
                    Created: false,
                    Replayed: matches,
                    Conflict: !matches);
            }

            if (scan.RecordCount >= _options.MaximumRecordCount)
                throw new InvalidDataException("FLOW-38 quarantine store record capacity is exhausted.");

            var envelope = new PersistedEnvelope(
                SchemaVersion,
                snapshot.EventId,
                snapshot.TenantScope,
                recordRef,
                fingerprint,
                _utcNow().ToUniversalTime(),
                LegalVerdict: DesignConversionModelServerFlow38QuarantineHandler.QuarantineVerdict,
                AllowedUse: DesignConversionModelServerFlow38QuarantineHandler.PersonalAdaptiveUse,
                Route: DesignConversionModelServerFlow38QuarantineHandler.AdaptiveOrQuarantineRoute,
                SemanticQualityClaimed: false,
                DpoTrainReady: false,
                Source: snapshot);
            var payload = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);
            if (payload.Length > MaximumPersistedLineBytes)
                throw new InvalidDataException("FLOW-38 quarantine record exceeds the persisted line limit.");

            var frame = new byte[payload.Length + 1];
            payload.CopyTo(frame, 0);
            frame[^1] = (byte)'\n';
            if (scan.TotalBytes > _options.MaximumStoreBytes - frame.LongLength)
                throw new InvalidDataException("FLOW-38 quarantine store byte capacity is exhausted.");

            await AppendFrameAsync(rootLease, frame, scan.TotalBytes, cancellationToken).ConfigureAwait(false);

            return new DesignConversionFlow38QuarantineStoreResult(
                snapshot.EventId,
                recordRef,
                fingerprint,
                Created: true,
                Replayed: false,
                Conflict: false);
        }
        finally
        {
            _processGate.Release();
        }
    }

    private async Task<StoreScanResult> ScanStoreAsync(
        SafeStoreDirectoryLease rootLease,
        DesignConversionFlow38QuarantinedSourceRow row,
        CancellationToken cancellationToken)
    {
        InvokeBeforeSafeOpen(DesignConversionFlow38SafeOpenStage.RecordRead, _recordsPath);
        await using var stream = rootLease.TryOpenExistingFile(FileName, FileShare.Read, writeThrough: false);
        if (stream is null)
            return new StoreScanResult(0, 0, null);

        var validatedLength = stream.Length;
        if (validatedLength > _options.MaximumStoreBytes)
            throw new InvalidDataException("FLOW-38 quarantine store exceeds its byte capacity.");

        var readBuffer = new byte[ReadBufferBytes];
        var lineBuffer = new byte[MaximumPersistedLineBytes];
        var lineLength = 0;
        var recordCount = 0;
        long totalRead = 0;
        PersistedEnvelope? existing = null;
        while (true)
        {
            var read = await stream.ReadAsync(readBuffer, cancellationToken).ConfigureAwait(false);
            if (read == 0)
                break;
            totalRead += read;
            if (totalRead > validatedLength || totalRead > _options.MaximumStoreBytes)
                throw new InvalidDataException("FLOW-38 quarantine store changed or exceeded capacity during validation.");

            for (var index = 0; index < read; index++)
            {
                var value = readBuffer[index];
                if (value != (byte)'\n')
                {
                    if (lineLength >= MaximumPersistedLineBytes)
                        throw new InvalidDataException("FLOW-38 quarantine store contains an oversized record.");
                    lineBuffer[lineLength++] = value;
                    continue;
                }

                var persisted = ParseAndValidateEnvelope(lineBuffer.AsSpan(0, lineLength));
                lineLength = 0;
                recordCount = IncrementRecordCount(recordCount, _options.MaximumRecordCount);

                if (string.Equals(persisted.EventId, row.EventId, StringComparison.Ordinal) &&
                    string.Equals(persisted.TenantScope, row.TenantScope, StringComparison.Ordinal))
                {
                    if (existing is not null)
                        throw new InvalidDataException("FLOW-38 quarantine store contains a duplicate event identity.");
                    existing = persisted;
                }
            }
        }

        if (totalRead != validatedLength || stream.Length != validatedLength)
            throw new InvalidDataException("FLOW-38 quarantine store changed during validation.");
        if (lineLength != 0)
            throw new InvalidDataException("FLOW-38 quarantine store contains an incomplete terminal frame.");

        return new StoreScanResult(validatedLength, recordCount, existing);
    }

    private static PersistedEnvelope ParseAndValidateEnvelope(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            throw new InvalidDataException("FLOW-38 quarantine store contains an empty record.");

        string json;
        try
        {
            json = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException("FLOW-38 quarantine store contains invalid UTF-8.", exception);
        }

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidDataException("FLOW-38 quarantine store contains a blank record.");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!HasExactProperties(root, ExpectedEnvelopeProperties) ||
            !root.TryGetProperty("source", out var source) ||
            !HasExactProperties(source, ExpectedSourceProperties))
        {
            throw new InvalidDataException("FLOW-38 quarantine store record schema is invalid.");
        }

        var persisted = root.Deserialize<PersistedEnvelope>(JsonOptions)
            ?? throw new InvalidDataException("FLOW-38 quarantine store contains an empty record.");
        if (!string.Equals(persisted.SchemaVersion, SchemaVersion, StringComparison.Ordinal) ||
            persisted.Source is null ||
            !DesignConversionModelServerFlow38QuarantineHandler.IsPersistableQuarantineRow(persisted.Source) ||
            string.IsNullOrWhiteSpace(persisted.Fingerprint) ||
            string.IsNullOrWhiteSpace(persisted.RecordRef) ||
            !string.Equals(persisted.EventId, persisted.Source.EventId, StringComparison.Ordinal) ||
            !string.Equals(persisted.TenantScope, persisted.Source.TenantScope, StringComparison.Ordinal) ||
            !string.Equals(persisted.RecordRef, RecordRef(persisted.Source), StringComparison.Ordinal) ||
            !string.Equals(persisted.Fingerprint, Fingerprint(persisted.Source), StringComparison.Ordinal) ||
            !string.Equals(persisted.LegalVerdict, DesignConversionModelServerFlow38QuarantineHandler.QuarantineVerdict, StringComparison.Ordinal) ||
            !string.Equals(persisted.AllowedUse, DesignConversionModelServerFlow38QuarantineHandler.PersonalAdaptiveUse, StringComparison.Ordinal) ||
            !string.Equals(persisted.Route, DesignConversionModelServerFlow38QuarantineHandler.AdaptiveOrQuarantineRoute, StringComparison.Ordinal) ||
            persisted.SemanticQualityClaimed ||
            persisted.DpoTrainReady)
        {
            throw new InvalidDataException("FLOW-38 quarantine store contains an invalid record.");
        }

        return persisted;
    }

    private static bool HasExactProperties(JsonElement element, IReadOnlySet<string> expected)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            // JsonProperty.Name is already JSON-unescaped, so escaped duplicate-equivalent names
            // collide in this ordinal set just like literal duplicates.
            if (!expected.Contains(property.Name) || !seen.Add(property.Name))
                return false;
        }

        return seen.SetEquals(expected);
    }

    internal static int IncrementRecordCount(int currentCount, int maximumRecordCount)
    {
        if (currentCount < 0 || maximumRecordCount <= 0 || currentCount >= maximumRecordCount)
            throw new InvalidDataException("FLOW-38 quarantine store exceeds its record capacity.");
        return checked(currentCount + 1);
    }

    internal static DesignConversionFlow38UnixNativeFlags UnixNativeFlagsFor(
        DesignConversionFlow38StoragePlatform platform) =>
        platform switch
        {
            DesignConversionFlow38StoragePlatform.Linux => new DesignConversionFlow38UnixNativeFlags(
                OpenReadOnly: 0,
                OpenReadWrite: 0x0002,
                OpenCreate: 0x0040,
                OpenExclusive: 0x0080,
                OpenDirectory: 0x10000,
                OpenNoFollow: 0x20000,
                OpenCloseOnExec: 0x80000,
                FlockExclusive: 0x02,
                FlockNonBlocking: 0x04),
            DesignConversionFlow38StoragePlatform.MacOS => new DesignConversionFlow38UnixNativeFlags(
                OpenReadOnly: 0,
                OpenReadWrite: 0x0002,
                OpenCreate: 0x0200,
                OpenExclusive: 0x0800,
                OpenDirectory: 0x100000,
                OpenNoFollow: 0x0100,
                OpenCloseOnExec: 0x01000000,
                FlockExclusive: 0x02,
                FlockNonBlocking: 0x04),
            _ => throw new PlatformNotSupportedException(
                $"FLOW-38 quarantine has no Unix native flag profile for '{platform}'.")
        };

    private async Task AppendFrameAsync(
        SafeStoreDirectoryLease rootLease,
        byte[] frame,
        long expectedExistingBytes,
        CancellationToken cancellationToken)
    {
        InvokeBeforeSafeOpen(DesignConversionFlow38SafeOpenStage.RecordAppend, _recordsPath);
        await using var stream = rootLease.OpenOrCreateFile(FileName, FileShare.Read, writeThrough: true);

        if (stream.Length != expectedExistingBytes)
            throw new InvalidDataException("FLOW-38 quarantine store changed before append.");
        if (stream.Length > 0)
        {
            stream.Position = stream.Length - 1;
            if (stream.ReadByte() != (byte)'\n')
                throw new InvalidDataException("FLOW-38 quarantine store terminal frame is incomplete.");
        }

        if (stream.Length > _options.MaximumStoreBytes - frame.LongLength)
            throw new InvalidDataException("FLOW-38 quarantine store byte capacity is exhausted.");

        stream.Position = stream.Length;
        await stream.WriteAsync(frame.AsMemory(), cancellationToken).ConfigureAwait(false);
        stream.Flush(flushToDisk: true);
    }

    private async Task<FileStream> AcquireCrossProcessLockAsync(
        SafeStoreDirectoryLease rootLease,
        CancellationToken cancellationToken)
    {
        IOException? lastFailure = null;
        for (var attempt = 0; attempt < MaximumLockAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            InvokeBeforeSafeOpen(DesignConversionFlow38SafeOpenStage.LockFile, _lockPath);
            try
            {
                var stream = rootLease.OpenOrCreateFile(LockFileName, FileShare.None, writeThrough: true);
                try
                {
                    if (!rootLease.TryAcquireExclusiveProcessLock(stream))
                        throw new IOException("FLOW-38 quarantine store lock is held by another process.");
                    return stream;
                }
                catch
                {
                    await stream.DisposeAsync().ConfigureAwait(false);
                    throw;
                }
            }
            catch (IOException exception)
            {
                lastFailure = exception;
                await Task.Delay(LockRetryDelayMilliseconds, cancellationToken).ConfigureAwait(false);
            }
        }

        throw new IOException("FLOW-38 quarantine store lock could not be acquired.", lastFailure);
    }

    private void InvokeBeforeSafeOpen(DesignConversionFlow38SafeOpenStage stage, string path) =>
        _options.BeforeSafeOpenForTesting?.Invoke(stage, path);

    private void EnsureCanonicalContainment(string path)
    {
        var relative = Path.GetRelativePath(_rootDirectory, Path.GetFullPath(path));
        if (Path.IsPathRooted(relative) ||
            string.Equals(relative, "..", StringComparison.Ordinal) ||
            relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidDataException("FLOW-38 quarantine store path escapes its canonical root.");
        }
    }

    private static DesignConversionFlow38QuarantinedSourceRow Snapshot(
        DesignConversionFlow38QuarantinedSourceRow row)
    {
        var provenance = row.ProvenanceRefs is null
            ? null!
            : Array.AsReadOnly(row.ProvenanceRefs.Select(CopyString).ToArray());
        return new DesignConversionFlow38QuarantinedSourceRow(
            CopyString(row.EventId),
            CopyString(row.CorrelationId),
            CopyString(row.TenantScope),
            CopyString(row.CycleId),
            CopyString(row.PatternId),
            CopyString(row.RetrievalId),
            CopyString(row.PromptHash),
            CopyString(row.ChosenOutputRef),
            CopyString(row.ChosenOutputHash),
            CopyString(row.RejectedOutputRef),
            CopyString(row.RejectedOutputHash),
            CopyString(row.SourceCategory),
            CopyString(row.TermsSnapshotRef),
            CopyString(row.OutputLicenseRef),
            provenance,
            CopyString(row.RedactionPolicyRef),
            CopyString(row.ChosenOrigin),
            CopyString(row.RejectedOrigin),
            CopyString(row.LegalVerdict),
            CopyString(row.AllowedUse),
            CopyString(row.Route),
            row.SemanticQualityClaimed,
            row.DpoTrainReady);
    }

    private static string CopyString(string? value) => value is null ? null! : new string(value.AsSpan());

    private static string Fingerprint(DesignConversionFlow38QuarantinedSourceRow row)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(row, JsonOptions);
        return "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static string RecordRef(DesignConversionFlow38QuarantinedSourceRow row)
    {
        var tenantHash = Convert.ToHexString(SHA256.HashData(StrictUtf8.GetBytes(row.TenantScope)))
            .ToLowerInvariant();
        return $"flow38-quarantine://sha256:{tenantHash}/{row.EventId}";
    }

    private static JsonSerializerOptions CreateJsonOptions() => new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        WriteIndented = false
    };

    private sealed class SafeStoreDirectoryLease : IDisposable
    {
        private const uint GenericRead = 0x80000000;
        private const uint GenericWrite = 0x40000000;
        private const uint Synchronize = 0x00100000;
        private const uint FileTraverse = 0x00000020;
        private const uint FileReadAttributes = 0x00000080;
        private const uint FileAttributeNormal = 0x00000080;
        private const uint FileAttributeDirectory = 0x00000010;
        private const uint FileAttributeReparsePoint = 0x00000400;
        private const uint FileFlagBackupSemantics = 0x02000000;
        private const uint FileFlagOpenReparsePoint = 0x00200000;
        private const uint OpenExisting = 3;
        private const uint FileOpen = 1;
        private const uint FileCreate = 2;
        private const uint FileDirectoryFile = 0x00000001;
        private const uint FileWriteThrough = 0x00000002;
        private const uint FileSynchronousIoNonAlert = 0x00000020;
        private const uint FileNonDirectoryFile = 0x00000040;
        private const uint FileOpenReparsePoint = 0x00200000;
        private const uint ObjectCaseInsensitive = 0x00000040;
        private const uint ObjectDontReparse = 0x00001000;
        private const int ErrorFileNotFound = 2;
        private const int ErrorPathNotFound = 3;
        private const int StatusNoSuchFile = unchecked((int)0xC000000F);
        private const int StatusObjectNameNotFound = unchecked((int)0xC0000034);
        private const int StatusObjectNameCollision = unchecked((int)0xC0000035);
        private const int StatusObjectPathNotFound = unchecked((int)0xC000003A);
        private const int UnixEntryNotFound = 2;
        private const int UnixEntryExists = 17;

        private readonly string _rootDirectory;
        private readonly IReadOnlyList<SafeFileHandle> _directoryHandles;
        private readonly SafeFileHandle _rootHandle;
        private readonly DesignConversionFlow38StoragePlatform _platform;

        private SafeStoreDirectoryLease(
            string rootDirectory,
            IReadOnlyList<SafeFileHandle> directoryHandles,
            DesignConversionFlow38StoragePlatform platform)
        {
            _rootDirectory = rootDirectory;
            _directoryHandles = directoryHandles;
            _rootHandle = directoryHandles[^1];
            _platform = platform;
        }

        public static SafeStoreDirectoryLease AcquireOrCreate(
            string rootDirectory,
            DesignConversionFlow38StoragePlatform? platformOverride,
            Action<DesignConversionFlow38SafeOpenStage, string>? beforeSafeOpen)
        {
            var platform = platformOverride ?? DetectStoragePlatform();
            return platform switch
            {
                DesignConversionFlow38StoragePlatform.Windows
                    => AcquireOrCreateWindows(rootDirectory, platform, beforeSafeOpen),
                DesignConversionFlow38StoragePlatform.Linux or DesignConversionFlow38StoragePlatform.MacOS
                    => AcquireOrCreateUnix(rootDirectory, platform, beforeSafeOpen),
                _ => throw new PlatformNotSupportedException(
                    "FLOW-38 quarantine safe no-follow storage is supported only on Windows, Linux, and macOS.")
            };
        }

        public FileStream? TryOpenExistingFile(string leafName, FileShare share, bool writeThrough)
        {
            EnsureLeafName(leafName);
            return _platform switch
            {
                DesignConversionFlow38StoragePlatform.Windows
                    => OpenWindowsLeaf(leafName, share, writeThrough, createIfMissing: false),
                DesignConversionFlow38StoragePlatform.Linux or DesignConversionFlow38StoragePlatform.MacOS
                    => OpenUnixLeaf(leafName, writeThrough, createIfMissing: false),
                _ => throw new PlatformNotSupportedException("FLOW-38 quarantine safe file open is unsupported on this OS.")
            };
        }

        public FileStream OpenOrCreateFile(string leafName, FileShare share, bool writeThrough)
        {
            EnsureLeafName(leafName);
            return (_platform switch
                    {
                        DesignConversionFlow38StoragePlatform.Windows
                            => OpenWindowsLeaf(leafName, share, writeThrough, createIfMissing: true),
                        DesignConversionFlow38StoragePlatform.Linux or DesignConversionFlow38StoragePlatform.MacOS
                            => OpenUnixLeaf(leafName, writeThrough, createIfMissing: true),
                        _ => throw new PlatformNotSupportedException("FLOW-38 quarantine safe file creation is unsupported on this OS.")
                    })
                ?? throw new IOException("FLOW-38 quarantine safe file creation returned no handle.");
        }

        public bool TryAcquireExclusiveProcessLock(FileStream stream)
        {
            if (_platform == DesignConversionFlow38StoragePlatform.Windows)
                return true; // CreateFileW FileShare.None is the exclusive cross-process lock.
            if (_platform is not DesignConversionFlow38StoragePlatform.Linux and
                not DesignConversionFlow38StoragePlatform.MacOS)
            {
                throw new PlatformNotSupportedException("FLOW-38 quarantine file locking is unsupported on this OS.");
            }

            var flags = UnixNativeFlagsFor(_platform);
            var descriptor = checked((int)stream.SafeFileHandle.DangerousGetHandle());
            if (Flock(descriptor, flags.ExclusiveNonBlockingLock) == 0)
                return true;
            var error = Marshal.GetLastPInvokeError();
            if (error is 11 or 35)
                return false;
            throw new IOException($"FLOW-38 quarantine flock failed with errno {error}.");
        }

        public void Dispose()
        {
            for (var index = _directoryHandles.Count - 1; index >= 0; index--)
                _directoryHandles[index].Dispose();
        }

        private static SafeStoreDirectoryLease AcquireOrCreateWindows(
            string rootDirectory,
            DesignConversionFlow38StoragePlatform platform,
            Action<DesignConversionFlow38SafeOpenStage, string>? beforeSafeOpen)
        {
            var fullRoot = Path.GetFullPath(rootDirectory);
            var pathRoot = Path.GetPathRoot(fullRoot)
                ?? throw new InvalidDataException("FLOW-38 quarantine root is not fully qualified.");
            var segments = fullRoot[pathRoot.Length..]
                .Split(
                    [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                    StringSplitOptions.RemoveEmptyEntries);
            var handles = new List<SafeFileHandle>(segments.Length + 1);
            try
            {
                var anchor = OpenWindowsDirectory(pathRoot, out var anchorError);
                if (anchor.IsInvalid)
                {
                    anchor.Dispose();
                    throw WindowsIOException("open filesystem root anchor", pathRoot, anchorError);
                }

                try
                {
                    ValidateWindowsHandle(anchor, pathRoot, requireDirectory: true);
                }
                catch
                {
                    anchor.Dispose();
                    throw;
                }

                handles.Add(anchor);
                var parentPath = pathRoot;
                foreach (var segment in segments)
                {
                    var childPath = Path.Combine(parentPath, segment);
                    var parentHandle = handles[^1];
                    ValidateWindowsHandle(parentHandle, parentPath, requireDirectory: true);
                    var childHandle = NtOpenWindowsDirectory(
                        parentHandle,
                        segment,
                        FileOpen,
                        out var status);
                    if (status < 0)
                    {
                        childHandle?.Dispose();
                        if (status is not StatusNoSuchFile and
                            not StatusObjectNameNotFound and
                            not StatusObjectPathNotFound)
                        {
                            throw WindowsNtIOException(
                                "open root-relative directory without following links",
                                childPath,
                                status);
                        }

                        beforeSafeOpen?.Invoke(
                            DesignConversionFlow38SafeOpenStage.RootSegmentCreate,
                            childPath);
                        ValidateWindowsHandle(parentHandle, parentPath, requireDirectory: true);
                        childHandle = NtOpenWindowsDirectory(
                            parentHandle,
                            segment,
                            FileCreate,
                            out status);
                        if (status < 0)
                        {
                            childHandle?.Dispose();
                            if (status == StatusObjectNameCollision)
                            {
                                throw new InvalidDataException(
                                    "FLOW-38 quarantine root segment changed during safe creation.");
                            }

                            throw WindowsNtIOException(
                                "create root-relative directory without following links",
                                childPath,
                                status);
                        }
                    }

                    try
                    {
                        ValidateWindowsHandle(childHandle, childPath, requireDirectory: true);
                    }
                    catch
                    {
                        childHandle.Dispose();
                        throw;
                    }

                    handles.Add(childHandle);
                    parentPath = childPath;
                }

                return new SafeStoreDirectoryLease(fullRoot, handles, platform);
            }
            catch
            {
                for (var index = handles.Count - 1; index >= 0; index--)
                    handles[index].Dispose();
                throw;
            }
        }

        private static SafeStoreDirectoryLease AcquireOrCreateUnix(
            string rootDirectory,
            DesignConversionFlow38StoragePlatform platform,
            Action<DesignConversionFlow38SafeOpenStage, string>? beforeSafeOpen)
        {
            var fullRoot = Path.GetFullPath(rootDirectory);
            if (!Path.IsPathRooted(fullRoot))
                throw new InvalidDataException("FLOW-38 quarantine Unix root is not fully qualified.");
            var nativeFlags = UnixNativeFlagsFor(platform);

            var handles = new List<SafeFileHandle>();
            try
            {
                var rootDescriptor = OpenUnix("/", nativeFlags.DirectoryOpenFlags, 0);
                if (rootDescriptor < 0)
                    throw UnixIOException("open filesystem root", "/", Marshal.GetLastPInvokeError());
                handles.Add(new SafeFileHandle((IntPtr)rootDescriptor, ownsHandle: true));

                var parentPath = "/";
                foreach (var segment in fullRoot[1..]
                             .Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    var childPath = Path.Combine(parentPath, segment);
                    var parentDescriptor = checked((int)handles[^1].DangerousGetHandle());
                    var descriptor = OpenAtUnix(parentDescriptor, segment, nativeFlags.DirectoryOpenFlags, 0);
                    if (descriptor < 0)
                    {
                        var error = Marshal.GetLastPInvokeError();
                        if (error != UnixEntryNotFound)
                            throw UnixIOException("open directory without following links", segment, error);

                        beforeSafeOpen?.Invoke(
                            DesignConversionFlow38SafeOpenStage.RootSegmentCreate,
                            childPath);
                        if (MakeDirectoryAtUnix(parentDescriptor, segment, Convert.ToUInt32("700", 8)) != 0)
                        {
                            error = Marshal.GetLastPInvokeError();
                            if (error != UnixEntryExists)
                                throw UnixIOException("create directory", segment, error);
                        }

                        descriptor = OpenAtUnix(parentDescriptor, segment, nativeFlags.DirectoryOpenFlags, 0);
                        if (descriptor < 0)
                            throw UnixIOException("open created directory", segment, Marshal.GetLastPInvokeError());
                    }

                    handles.Add(new SafeFileHandle((IntPtr)descriptor, ownsHandle: true));
                    parentPath = childPath;
                }

                return new SafeStoreDirectoryLease(fullRoot, handles, platform);
            }
            catch
            {
                for (var index = handles.Count - 1; index >= 0; index--)
                    handles[index].Dispose();
                throw;
            }
        }

        private FileStream? OpenWindowsLeaf(
            string leafName,
            FileShare share,
            bool writeThrough,
            bool createIfMissing)
        {
            var path = Path.Combine(_rootDirectory, leafName);
            ValidateWindowsHandle(_rootHandle, _rootDirectory, requireDirectory: true);
            var handle = NtOpenWindowsLeaf(
                leafName,
                share,
                writeThrough,
                FileOpen,
                out var status);
            if (status < 0)
            {
                handle?.Dispose();
                if (status is not StatusNoSuchFile and not StatusObjectNameNotFound and not StatusObjectPathNotFound)
                    throw WindowsNtIOException("open root-relative file without following links", path, status);
                if (!createIfMissing)
                    return null;

                handle = NtOpenWindowsLeaf(
                    leafName,
                    share,
                    writeThrough,
                    FileCreate,
                    out status);
                if (status < 0)
                {
                    handle?.Dispose();
                    if (status == StatusObjectNameCollision)
                        throw new InvalidDataException("FLOW-38 quarantine file changed during safe creation.");
                    throw WindowsNtIOException("create root-relative file without following links", path, status);
                }
            }

            try
            {
                ValidateWindowsHandle(handle, path, requireDirectory: false);
                return new FileStream(handle, FileAccess.ReadWrite, ReadBufferBytes, isAsync: false);
            }
            catch
            {
                handle.Dispose();
                throw;
            }
        }

        private SafeFileHandle NtOpenWindowsLeaf(
            string leafName,
            FileShare share,
            bool writeThrough,
            uint createDisposition,
            out int status) =>
            NtOpenWindowsRelative(
                _rootHandle,
                leafName,
                GenericRead | GenericWrite | Synchronize,
                share,
                createDisposition,
                FileAttributeNormal,
                FileSynchronousIoNonAlert |
                FileNonDirectoryFile |
                FileOpenReparsePoint |
                (writeThrough ? FileWriteThrough : 0),
                out status);

        private static SafeFileHandle NtOpenWindowsDirectory(
            SafeFileHandle parentHandle,
            string segment,
            uint createDisposition,
            out int status) =>
            NtOpenWindowsRelative(
                parentHandle,
                segment,
                FileReadAttributes | FileTraverse | Synchronize,
                FileShare.Read | FileShare.Write,
                createDisposition,
                FileAttributeDirectory,
                FileSynchronousIoNonAlert | FileDirectoryFile | FileOpenReparsePoint,
                out status);

        private static SafeFileHandle NtOpenWindowsRelative(
            SafeFileHandle rootHandle,
            string relativeName,
            uint desiredAccess,
            FileShare share,
            uint createDisposition,
            uint fileAttributes,
            uint createOptions,
            out int status)
        {
            var nameBuffer = Marshal.StringToHGlobalUni(relativeName);
            var unicodeStringPointer = Marshal.AllocHGlobal(Marshal.SizeOf<UnicodeString>());
            var rootHandleAddRef = false;
            try
            {
                var nameLength = checked((ushort)(relativeName.Length * sizeof(char)));
                var unicodeString = new UnicodeString
                {
                    Length = nameLength,
                    MaximumLength = checked((ushort)(nameLength + sizeof(char))),
                    Buffer = nameBuffer
                };
                Marshal.StructureToPtr(unicodeString, unicodeStringPointer, fDeleteOld: false);

                rootHandle.DangerousAddRef(ref rootHandleAddRef);
                var objectAttributes = new ObjectAttributes
                {
                    Length = Marshal.SizeOf<ObjectAttributes>(),
                    RootDirectory = rootHandle.DangerousGetHandle(),
                    ObjectName = unicodeStringPointer,
                    Attributes = ObjectCaseInsensitive | ObjectDontReparse,
                    SecurityDescriptor = IntPtr.Zero,
                    SecurityQualityOfService = IntPtr.Zero
                };
                status = NtCreateFileWindows(
                    out var handle,
                    desiredAccess,
                    ref objectAttributes,
                    out _,
                    IntPtr.Zero,
                    fileAttributes,
                    (uint)share,
                    createDisposition,
                    createOptions,
                    IntPtr.Zero,
                    0);
                return handle;
            }
            finally
            {
                if (rootHandleAddRef)
                    rootHandle.DangerousRelease();
                Marshal.FreeHGlobal(unicodeStringPointer);
                Marshal.FreeHGlobal(nameBuffer);
            }
        }

        private FileStream? OpenUnixLeaf(string leafName, bool writeThrough, bool createIfMissing)
        {
            _ = writeThrough; // Flush(flushToDisk:true) supplies durable persistence after the framed write.
            var rootDescriptor = checked((int)_rootHandle.DangerousGetHandle());
            var nativeFlags = UnixNativeFlagsFor(_platform);
            var flags = nativeFlags.FileOpenFlags;
            var descriptor = OpenAtUnix(rootDescriptor, leafName, flags, 0);
            if (descriptor < 0)
            {
                var error = Marshal.GetLastPInvokeError();
                if (error != UnixEntryNotFound)
                    throw UnixIOException("open file without following links", leafName, error);
                if (!createIfMissing)
                    return null;

                descriptor = OpenAtUnix(
                    rootDescriptor,
                    leafName,
                    flags | nativeFlags.OpenCreate | nativeFlags.OpenExclusive,
                    Convert.ToUInt32("600", 8));
                if (descriptor < 0)
                {
                    error = Marshal.GetLastPInvokeError();
                    if (error == UnixEntryExists)
                        throw new InvalidDataException("FLOW-38 quarantine file changed during safe creation.");
                    throw UnixIOException("create file without following links", leafName, error);
                }
            }

            var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
            try
            {
                return new FileStream(handle, FileAccess.ReadWrite, ReadBufferBytes, isAsync: false);
            }
            catch
            {
                handle.Dispose();
                throw;
            }
        }

        private static SafeFileHandle OpenWindowsDirectory(string path, out int error)
        {
            var handle = CreateFileWindows(
                path,
                FileReadAttributes | FileTraverse,
                (uint)(FileShare.Read | FileShare.Write),
                IntPtr.Zero,
                OpenExisting,
                FileFlagBackupSemantics | FileFlagOpenReparsePoint,
                IntPtr.Zero);
            error = handle.IsInvalid ? Marshal.GetLastPInvokeError() : 0;
            return handle;
        }

        private static void ValidateWindowsHandle(SafeFileHandle handle, string expectedPath, bool requireDirectory)
        {
            if (!GetFileInformationByHandleExWindows(
                    handle,
                    FileInfoByHandleClass.FileAttributeTagInfo,
                    out var info,
                    (uint)Marshal.SizeOf<FileAttributeTagInformation>()))
            {
                throw WindowsIOException("inspect handle", expectedPath, Marshal.GetLastPInvokeError());
            }

            if ((info.FileAttributes & FileAttributeReparsePoint) != 0)
                throw new InvalidDataException("FLOW-38 quarantine safe open rejected a reparse-point handle.");
            var isDirectory = (info.FileAttributes & FileAttributeDirectory) != 0;
            if (isDirectory != requireDirectory)
                throw new InvalidDataException("FLOW-38 quarantine safe open resolved an unexpected handle kind.");

            var finalPath = GetWindowsFinalPath(handle, expectedPath);
            if (!string.Equals(
                    NormalizeWindowsPath(finalPath),
                    NormalizeWindowsPath(expectedPath),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("FLOW-38 quarantine safe open resolved outside the expected path.");
            }
        }

        private static string GetWindowsFinalPath(SafeFileHandle handle, string expectedPath)
        {
            var capacity = 512;
            while (capacity <= 32 * 1024)
            {
                var buffer = new StringBuilder(capacity);
                var length = GetFinalPathNameByHandleWindows(handle, buffer, (uint)capacity, 0);
                if (length == 0)
                    throw WindowsIOException("resolve final handle path", expectedPath, Marshal.GetLastPInvokeError());
                if (length < capacity)
                    return buffer.ToString();
                capacity = checked((int)length + 1);
            }

            throw new InvalidDataException("FLOW-38 quarantine final handle path exceeds the safe limit.");
        }

        private static string NormalizeWindowsPath(string path)
        {
            if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
                path = @"\\" + path[8..];
            else if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                path = path[4..];

            var fullPath = Path.GetFullPath(path);
            var root = Path.GetPathRoot(fullPath);
            return string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase)
                ? fullPath
                : Path.TrimEndingDirectorySeparator(fullPath);
        }

        private static void EnsureLeafName(string leafName)
        {
            if (string.IsNullOrWhiteSpace(leafName) ||
                !string.Equals(Path.GetFileName(leafName), leafName, StringComparison.Ordinal) ||
                leafName is "." or "..")
            {
                throw new InvalidDataException("FLOW-38 quarantine safe-open leaf name is invalid.");
            }
        }

        private static DesignConversionFlow38StoragePlatform DetectStoragePlatform()
        {
            if (OperatingSystem.IsWindows())
                return DesignConversionFlow38StoragePlatform.Windows;
            if (OperatingSystem.IsLinux())
                return DesignConversionFlow38StoragePlatform.Linux;
            if (OperatingSystem.IsMacOS())
                return DesignConversionFlow38StoragePlatform.MacOS;
            return DesignConversionFlow38StoragePlatform.Unsupported;
        }

        private static IOException WindowsIOException(string operation, string path, int error) =>
            new($"FLOW-38 quarantine failed to {operation} '{path}' (Win32 error {error}).");

        private static IOException WindowsNtIOException(string operation, string path, int status)
        {
            var error = RtlNtStatusToDosErrorWindows(status);
            return new IOException(
                $"FLOW-38 quarantine failed to {operation} '{path}' " +
                $"(NTSTATUS 0x{unchecked((uint)status):X8}, Win32 error {error}).");
        }

        private static IOException UnixIOException(string operation, string path, int error) =>
            new($"FLOW-38 quarantine failed to {operation} '{path}' (errno {error}).");

        private enum FileInfoByHandleClass
        {
            FileAttributeTagInfo = 9
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileAttributeTagInformation
        {
            public uint FileAttributes;
            public uint ReparseTag;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UnicodeString
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ObjectAttributes
        {
            public int Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IoStatusBlock
        {
            public IntPtr Status;
            public IntPtr Information;
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFileWindows(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            IntPtr securityAttributes,
            uint creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandleExWindows(
            SafeFileHandle file,
            FileInfoByHandleClass fileInformationClass,
            out FileAttributeTagInformation fileInformation,
            uint bufferSize);

        [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetFinalPathNameByHandleWindows(
            SafeFileHandle file,
            StringBuilder filePath,
            uint filePathLength,
            uint flags);

        [DllImport("ntdll.dll", EntryPoint = "NtCreateFile")]
        private static extern int NtCreateFileWindows(
            out SafeFileHandle fileHandle,
            uint desiredAccess,
            ref ObjectAttributes objectAttributes,
            out IoStatusBlock ioStatusBlock,
            IntPtr allocationSize,
            uint fileAttributes,
            uint shareAccess,
            uint createDisposition,
            uint createOptions,
            IntPtr extendedAttributes,
            uint extendedAttributesLength);

        [DllImport("ntdll.dll", EntryPoint = "RtlNtStatusToDosError")]
        private static extern uint RtlNtStatusToDosErrorWindows(int status);

        [DllImport("libc", EntryPoint = "open", SetLastError = true)]
        private static extern int OpenUnix(string path, int flags, uint mode);

        [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
        private static extern int OpenAtUnix(int directoryFileDescriptor, string path, int flags, uint mode);

        [DllImport("libc", EntryPoint = "mkdirat", SetLastError = true)]
        private static extern int MakeDirectoryAtUnix(int directoryFileDescriptor, string path, uint mode);

        [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
        private static extern int Flock(int fileDescriptor, int operation);
    }

    private sealed record PersistedEnvelope(
        string SchemaVersion,
        string EventId,
        string TenantScope,
        string RecordRef,
        string Fingerprint,
        DateTimeOffset AdmittedAtUtc,
        string LegalVerdict,
        string AllowedUse,
        string Route,
        bool SemanticQualityClaimed,
        bool DpoTrainReady,
        DesignConversionFlow38QuarantinedSourceRow Source);

    private sealed record StoreScanResult(
        long TotalBytes,
        int RecordCount,
        PersistedEnvelope? Existing);
}

public sealed class DesignConversionModelServerFlow38QuarantineHandler
{
    public const string EndpointPath = "/api/design-conversion/flow38/quarantined-feedback";
    public const string EndpointMethod = "POST";
    public const string SchemaVersion = "xiigen.flow38.graph-dpo-source-row.quarantine-intake.v1";
    public const string BearerTokenEnvironmentName = "DESIGN_CONVERSION_FLOW38_QUARANTINE_BEARER_TOKEN";
    public const int MaximumRequestBodyBytes = 32 * 1024;
    public const string QuarantineVerdict = "QUARANTINE";
    public const string PersonalAdaptiveUse = "PersonalAdaptive";
    public const string AdaptiveOrQuarantineRoute = "adaptive_or_quarantine";
    public const string UnverifiedOrigin = "client_supplied_unverified";

    private const string AuthorizationHeaderName = "Authorization";
    private const string TenantHeaderName = "X-Tenant-Id";
    private const string CorrelationHeaderName = "X-Correlation-Id";
    private const string IdempotencyHeaderName = "X-Idempotency-Key";

    private static readonly HashSet<string> ExpectedProperties = new(StringComparer.Ordinal)
    {
        "eventId", "correlationId", "tenantScope", "cycleId", "patternId", "retrievalId",
        "promptHash", "chosenOutputRef", "chosenOutputHash", "rejectedOutputRef",
        "rejectedOutputHash", "sourceCategory", "termsSnapshotRef", "outputLicenseRef",
        "provenanceRefs", "redactionPolicyRef", "chosenOrigin", "rejectedOrigin",
        "legalVerdict", "allowedUse", "route", "semanticQualityClaimed", "dpoTrainReady"
    };
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        WriteIndented = false
    };

    private readonly IDesignConversionFlow38QuarantineStore? _store;
    private readonly string? _expectedBearerToken;

    public DesignConversionModelServerFlow38QuarantineHandler(
        IDesignConversionFlow38QuarantineStore? store,
        string? expectedBearerToken)
    {
        _store = store;
        _expectedBearerToken = string.IsNullOrWhiteSpace(expectedBearerToken)
            ? null
            : expectedBearerToken.Trim();
    }

    public bool IsConfigured => _store is not null && _expectedBearerToken is not null;

    public async Task<DesignConversionModelServerHttpResponse> HandleAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Error(400, "REQUEST_REQUIRED", "FLOW-38 quarantine intake request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method)
            ? string.Empty
            : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!string.Equals(path, EndpointPath, StringComparison.Ordinal))
            return Error(404, "ROUTE_NOT_FOUND", "FLOW-38 quarantine intake route is not registered.");
        if (!string.Equals(method, EndpointMethod, StringComparison.Ordinal))
            return Error(405, "METHOD_NOT_ALLOWED", "FLOW-38 quarantine intake endpoint requires POST.");
        if (_store is not { } store || _expectedBearerToken is null)
            return Error(503, "FLOW38_QUARANTINE_INTAKE_NOT_CONFIGURED", "FLOW-38 quarantine intake is not configured.");
        if (!HasJsonContentType(request.ContentType))
            return Error(415, "UNSUPPORTED_MEDIA_TYPE", "FLOW-38 quarantine intake requires application/json.");
        if (string.IsNullOrWhiteSpace(request.BodyJson))
            return Error(400, "REQUEST_BODY_REQUIRED", "FLOW-38 quarantine intake requires a JSON request body.");
        if (Encoding.UTF8.GetByteCount(request.BodyJson) > MaximumRequestBodyBytes)
            return Error(413, "REQUEST_BODY_TOO_LARGE", "FLOW-38 quarantine intake request body is too large.");
        if (!Authenticate(request.Headers))
            return Error(401, "FLOW38_QUARANTINE_AUTHENTICATION_FAILED", "FLOW-38 quarantine intake authentication failed.");

        DesignConversionFlow38QuarantinedSourceRow? row;
        try
        {
            using var document = JsonDocument.Parse(request.BodyJson);
            if (!HasExactProperties(document.RootElement))
                return Error(400, "FLOW38_QUARANTINE_SCHEMA_INVALID", "FLOW-38 quarantine intake payload schema is invalid.");
            row = document.RootElement.Deserialize<DesignConversionFlow38QuarantinedSourceRow>(JsonOptions);
        }
        catch (JsonException)
        {
            return Error(400, "REQUEST_JSON_INVALID", "FLOW-38 quarantine intake received invalid JSON.");
        }

        if (row is null)
            return Error(400, "REQUEST_BODY_REQUIRED", "FLOW-38 quarantine intake received an empty JSON request.");

        var validationError = Validate(row, request.Headers);
        if (validationError is not null)
            return Error(422, validationError.Value.Code, validationError.Value.Message);

        try
        {
            var stored = await store.AdmitAsync(row, cancellationToken).ConfigureAwait(false);
            if (stored.Conflict)
            {
                return Error(
                    409,
                    "FLOW38_QUARANTINE_EVENT_CONFLICT",
                    "FLOW-38 quarantine event identity already exists with different evidence.",
                    row.EventId);
            }

            return Success(row, stored);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or DecoderFallbackException or NotSupportedException)
        {
            return Error(
                503,
                "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE",
                "FLOW-38 quarantine evidence could not be persisted.",
                row.EventId);
        }
    }

    private bool Authenticate(IReadOnlyDictionary<string, string>? headers)
    {
        var authorization = ReadHeader(headers, AuthorizationHeaderName);
        const string prefix = "Bearer ";
        if (authorization is null ||
            !authorization.StartsWith(prefix, StringComparison.Ordinal) ||
            authorization.Length <= prefix.Length ||
            authorization.Contains(',', StringComparison.Ordinal))
        {
            return false;
        }

        var supplied = authorization[prefix.Length..];
        var expectedBytes = Encoding.UTF8.GetBytes(_expectedBearerToken!);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        return expectedBytes.Length == suppliedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }

    private static (string Code, string Message)? Validate(
        DesignConversionFlow38QuarantinedSourceRow row,
        IReadOnlyDictionary<string, string>? headers)
    {
        var rowError = ValidateRow(row);
        if (rowError is not null)
            return rowError;

        if (!string.Equals(ReadHeader(headers, TenantHeaderName), row.TenantScope, StringComparison.Ordinal) ||
            !string.Equals(ReadHeader(headers, CorrelationHeaderName), row.CorrelationId, StringComparison.Ordinal) ||
            !string.Equals(ReadHeader(headers, IdempotencyHeaderName), row.EventId, StringComparison.Ordinal))
        {
            return (
                "FLOW38_TRANSPORT_IDENTITY_MISMATCH",
                "FLOW-38 transport tenant, correlation, and idempotency identities must match the quarantined source row.");
        }

        return null;
    }

    internal static bool IsPersistableQuarantineRow(DesignConversionFlow38QuarantinedSourceRow row) =>
        row is not null && ValidateRow(row) is null;

    private static (string Code, string Message)? ValidateRow(DesignConversionFlow38QuarantinedSourceRow row)
    {
        if (!IsFlow38EventId(row.EventId))
            return ("FLOW38_EVENT_ID_INVALID", "eventId must be flow38-feedback:<64-lowercase-hex>.");
        if (!IsHeaderSafeIdentifier(row.CorrelationId, 160) ||
            !IsDownstreamTokenSafeTenant(row.TenantScope) ||
            !IsIdempotencyComponent(row.CycleId, 160) ||
            !IsIdempotencyComponent(row.PatternId, 160) ||
            !IsSafeText(row.RetrievalId, 160))
        {
            return ("FLOW38_IDENTITY_INVALID", "FLOW-38 quarantine identity fields are invalid.");
        }
        if (!IsSha256(row.PromptHash) || !IsSha256(row.ChosenOutputHash) || !IsSha256(row.RejectedOutputHash))
            return ("FLOW38_DIGEST_INVALID", "FLOW-38 quarantine digest fields must use exact sha256:<64-lowercase-hex> values.");
        if (!IsSafeReference(row.ChosenOutputRef) ||
            !IsSafeReference(row.RejectedOutputRef) ||
            !IsSafeReference(row.TermsSnapshotRef) ||
            !IsSafeReference(row.OutputLicenseRef) ||
            !IsSafeReference(row.RedactionPolicyRef))
        {
            return ("FLOW38_REFERENCE_INVALID", "FLOW-38 quarantine reference fields are invalid.");
        }
        if (row.ProvenanceRefs is null || row.ProvenanceRefs.Count is < 1 or > 16 || row.ProvenanceRefs.Any(value => !IsSafeReference(value)))
            return ("FLOW38_PROVENANCE_INVALID", "provenanceRefs must contain between one and sixteen safe references.");
        if (!DesignConversionFlow38SourceCategoryCanon.TryParseExact(
                row.SourceCategory,
                out var sourceCategory) ||
            !DesignConversionFlow38SourceCategoryCanon.IsQuarantineIntakeEligible(sourceCategory))
        {
            return ("FLOW38_SOURCE_CATEGORY_INVALID", "sourceCategory is not eligible for the quarantine-only intake.");
        }
        if (!string.Equals(row.ChosenOrigin, UnverifiedOrigin, StringComparison.Ordinal) ||
            !string.Equals(row.RejectedOrigin, UnverifiedOrigin, StringComparison.Ordinal) ||
            !string.Equals(row.LegalVerdict, QuarantineVerdict, StringComparison.Ordinal) ||
            !string.Equals(row.AllowedUse, PersonalAdaptiveUse, StringComparison.Ordinal) ||
            !string.Equals(row.Route, AdaptiveOrQuarantineRoute, StringComparison.Ordinal) ||
            row.SemanticQualityClaimed ||
            row.DpoTrainReady)
        {
            return (
                "FLOW38_QUARANTINE_INVARIANT_VIOLATION",
                "FLOW-38 client evidence must remain unverified, quarantined, personal/adaptive, non-semantic, and not train-ready.");
        }

        return null;
    }

    private static bool HasExactProperties(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return false;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in root.EnumerateObject())
        {
            if (!ExpectedProperties.Contains(property.Name) || !seen.Add(property.Name))
                return false;
        }

        return seen.SetEquals(ExpectedProperties);
    }

    private static bool IsFlow38EventId(string? value) =>
        value is { Length: 80 } &&
        value.StartsWith("flow38-feedback:", StringComparison.Ordinal) &&
        value[16..].All(IsLowerHex);

    private static bool IsSha256(string? value) =>
        value is { Length: 71 } &&
        value.StartsWith("sha256:", StringComparison.Ordinal) &&
        value[7..].All(IsLowerHex);

    private static bool IsLowerHex(char value) => value is >= '0' and <= '9' or >= 'a' and <= 'f';

    private static bool IsSafeText(string? value, int maximumLength) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= maximumLength &&
        value.All(character => !char.IsControl(character));

    private static bool IsHeaderSafeIdentifier(string? value, int maximumLength) =>
        !string.IsNullOrEmpty(value) &&
        value.Length <= maximumLength &&
        value.All(character => character is >= '!' and <= '~' && character != ',');

    private static bool IsIdempotencyComponent(string? value, int maximumLength) =>
        IsHeaderSafeIdentifier(value, maximumLength) &&
        !value!.Contains(':', StringComparison.Ordinal);

    private static bool IsDownstreamTokenSafeTenant(string? value) =>
        value is { Length: >= 1 and <= 128 } &&
        value.All(character =>
            character is >= 'a' and <= 'z'
            or >= 'A' and <= 'Z'
            or >= '0' and <= '9'
            or '.' or '_' or '-' or '~');

    private static bool IsSafeReference(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= 512 &&
        value.All(character => !char.IsControl(character));

    private static bool HasJsonContentType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        var separator = value.IndexOf(';', StringComparison.Ordinal);
        var mediaType = separator >= 0 ? value[..separator] : value;
        return string.Equals(mediaType.Trim(), "application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ReadHeader(IReadOnlyDictionary<string, string>? headers, string name)
    {
        if (headers is null)
            return null;
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }

        return null;
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";
        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? trimmed[..queryIndex] : trimmed;
    }

    private static DesignConversionModelServerHttpResponse Success(
        DesignConversionFlow38QuarantinedSourceRow row,
        DesignConversionFlow38QuarantineStoreResult stored)
    {
        var statusCode = stored.Created ? 202 : 200;
        var body = new
        {
            schemaVersion = SchemaVersion,
            endpoint = EndpointPath,
            method = EndpointMethod,
            statusCode,
            eventId = row.EventId,
            recordRef = stored.RecordRef,
            fingerprint = stored.Fingerprint,
            legalVerdict = QuarantineVerdict,
            allowedUse = PersonalAdaptiveUse,
            route = AdaptiveOrQuarantineRoute,
            semanticQualityClaimed = false,
            dpoTrainReady = false,
            dpoPairCreated = false,
            trainingTriggered = false,
            quarantinePersisted = true,
            created = stored.Created,
            replayed = stored.Replayed,
            boundaryMetadata = BoundaryMetadata(configured: true, persisted: true)
        };
        return new DesignConversionModelServerHttpResponse(
            statusCode,
            "application/json; charset=utf-8",
            JsonSerializer.Serialize(body, JsonOptions),
            ResponseHeaders(row.EventId));
    }

    private DesignConversionModelServerHttpResponse Error(
        int statusCode,
        string code,
        string message,
        string? eventId = null)
    {
        var body = new
        {
            schemaVersion = SchemaVersion,
            endpoint = EndpointPath,
            method = EndpointMethod,
            statusCode,
            errorCode = code,
            errorMessage = message,
            eventId,
            semanticQualityClaimed = false,
            dpoTrainReady = false,
            dpoPairCreated = false,
            trainingTriggered = false,
            boundaryMetadata = BoundaryMetadata(configured: IsConfigured, persisted: false)
        };
        return new DesignConversionModelServerHttpResponse(
            statusCode,
            "application/json; charset=utf-8",
            JsonSerializer.Serialize(body, JsonOptions),
            ResponseHeaders(eventId));
    }

    private static IReadOnlyDictionary<string, string> ResponseHeaders(string? eventId)
    {
        var headers = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["x-design-conversion-endpoint"] = EndpointPath,
            ["x-flow38-quarantine-only"] = bool.TrueString,
            ["x-flow38-dpo-train-ready"] = bool.FalseString,
            ["x-flow38-training-triggered"] = bool.FalseString,
            ["Cache-Control"] = "no-store"
        };
        if (!string.IsNullOrWhiteSpace(eventId))
            headers["x-flow38-event-id"] = eventId;
        return headers;
    }

    internal static IReadOnlyDictionary<string, string> BoundaryMetadata(bool configured, bool persisted) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["flow38_quarantine_contract_schema_version"] = SchemaVersion,
            ["flow38_quarantine_only"] = bool.TrueString,
            ["flow38_quarantine_store_configured"] = configured.ToString(),
            ["flow38_quarantine_persisted"] = persisted.ToString(),
            ["semantic_quality_claimed"] = bool.FalseString,
            ["dpo_train_ready"] = bool.FalseString,
            ["dpo_pair_created"] = bool.FalseString,
            ["training_triggered"] = bool.FalseString,
            ["common_training_allowed"] = bool.FalseString,
            ["raw_output_accepted"] = bool.FalseString,
            ["refs_only_public_response"] = bool.TrueString,
            ["product_ready"] = bool.FalseString
        };
}
