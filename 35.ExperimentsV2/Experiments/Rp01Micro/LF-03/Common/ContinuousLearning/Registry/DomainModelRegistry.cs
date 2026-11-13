// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using TransformerModel;

/// <summary>
/// Path-free retained adapter metadata and immutable exact-read evidence.
/// </summary>
internal sealed record ResolvedRetainedAdapterArtifact(
    string Domain,
    int TrainingExamples,
    DateTimeOffset TrainedAt,
    ModelAdapterProvenance Provenance,
    AdapterArtifactReference Reference,
    ServerOwnedContinuousLearningAdapterSnapshot Snapshot);

// DomainModelRegistry: durably stores and retrieves LoRA adapters per domain.
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class DomainModelRegistry : BaseService,
    IDomainModelRegistry,
    IDpoRegistryPublicationTransaction
{
    private const int RegistrySchemaVersion = 1;
    private const int RegistrySchemaVersion2 = 2;
    private const int LegacyAbsolutePathStorageKind = 1;
    private const int RetainedReferenceStorageKind = 2;
    private const string AdapterReferenceResolverRequiredErrorCode =
        "ADAPTER_REFERENCE_RESOLVER_REQUIRED";
    private const string AdapterReferenceNotRetainedErrorCode =
        "ADAPTER_REFERENCE_NOT_RETAINED";
    private const string AdapterReferenceUntrustedErrorCode =
        "ADAPTER_REFERENCE_UNTRUSTED";
    private const string AdapterReferenceChangedErrorCode =
        "ADAPTER_REFERENCE_CHANGED_DURING_RESOLUTION";
    private const string RetainedReferenceWriteUnsupportedErrorCode =
        "REGISTRY_RETAINED_REFERENCE_WRITE_UNSUPPORTED";
    internal const long MaximumRegistryBytes = 16L * 1024 * 1024;
    private const long MaximumAdapterBytes = 2L * 1024 * 1024 * 1024;
    private const string RegistryFileName = "registry.json";
    private const string RegistryLockFileName = ".registry.json.lock";
    private const string RegistryOwnerLockFileName =
        ".continuous-learning.registry-owner.lock";
    private const long RegistryLockOffset = 0;
    private const long RegistryLockLength = 1;
    private static readonly ConcurrentDictionary<string, object> PathGates = new(
        OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);
    private static readonly ConditionalWeakTable<
        ServerOwnedContinuousLearningRegistryRootCapability,
        object> RetainedCapabilityGates = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    // One adapter per canonical domain string — latest training always wins.
    private readonly Dictionary<string, ModelAdapter> _registry = new(StringComparer.Ordinal);

    // Directory where registry.json and adapter files live.
    private readonly string? _registryDir;
    private readonly string? _registryPath;
    private readonly string? _lockPath;
    private readonly string? _ownerLockPath;
    private readonly object _pathGate;
    private readonly ServerOwnedContinuousLearningRegistryRootCapability?
        _retainedRegistryRoot;
    private readonly ServerOwnedContinuousLearningAdapterRootCapability?
        _retainedAdapterRoot;
    private readonly Action? _beforeFinalPublicationValidation;
    // Test-only seam used by registry recovery tests to make rollback failure
    // deterministic across Windows and Linux; production constructors keep it null.
    private readonly Action? _beforeRollbackForTesting;
    private readonly TimeSpan _processLeaseTimeout;
    private bool _retainedRecoveryConfirmed;

    public DomainModelRegistry(ISessionStorage storage, string registryDir = "adapters")
        : this(
            storage,
            registryDir,
            beforeFinalPublicationValidation: null,
            processLeaseTimeout: TimeSpan.FromSeconds(10))
    {
    }

    internal DomainModelRegistry(
        ISessionStorage storage,
        string registryDir,
        Action? beforeFinalPublicationValidation,
        TimeSpan? processLeaseTimeout = null,
        Action? beforeRollbackForTesting = null)
        : base(storage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registryDir);
        _registryDir = Path.TrimEndingDirectorySeparator(Path.GetFullPath(registryDir));
        _registryPath = Path.Combine(_registryDir, RegistryFileName);
        _lockPath = Path.Combine(_registryDir, RegistryLockFileName);
        _ownerLockPath = Path.Combine(_registryDir, RegistryOwnerLockFileName);
        _pathGate = PathGates.GetOrAdd(_registryPath, static _ => new object());
        _beforeFinalPublicationValidation = beforeFinalPublicationValidation;
        _beforeRollbackForTesting = beforeRollbackForTesting;
        _processLeaseTimeout = processLeaseTimeout ?? TimeSpan.FromSeconds(10);
        if (_processLeaseTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(processLeaseTimeout));

        var load = ExecuteUnderRegistryBoundary(LoadRegistryStateCore);
        if (!load.IsSuccess || load.Value is null)
            throw new InvalidDataException($"{load.ErrorCode}: {load.ErrorMessage}");
        foreach (var adapter in load.Value.ModelAdapters)
            _registry.Add(adapter.Domain, adapter);
    }

    internal DomainModelRegistry(
        ISessionStorage storage,
        ServerOwnedContinuousLearningRegistryRootCapability registryRoot,
        Action? beforeFinalPublicationValidation = null)
        : base(storage)
    {
        _retainedRegistryRoot = registryRoot
            ?? throw new ArgumentNullException(nameof(registryRoot));
        _pathGate = RetainedCapabilityGates.GetValue(
            registryRoot,
            static _ => new object());
        _beforeFinalPublicationValidation = beforeFinalPublicationValidation;
        _beforeRollbackForTesting = null;
        _processLeaseTimeout = TimeSpan.FromSeconds(10);

        var load = ExecuteUnderRegistryBoundary(LoadRegistryStateCore);
        if (!load.IsSuccess || load.Value is null)
            throw new InvalidDataException($"{load.ErrorCode}: {load.ErrorMessage}");
        foreach (var adapter in load.Value.ModelAdapters)
            _registry.Add(adapter.Domain, adapter);
    }

    internal DomainModelRegistry(
        ISessionStorage storage,
        ServerOwnedContinuousLearningRegistryRootCapability registryRoot,
        ServerOwnedContinuousLearningAdapterRootCapability adapterRoot,
        Action? beforeFinalPublicationValidation = null)
        : base(storage)
    {
        _retainedRegistryRoot = registryRoot
            ?? throw new ArgumentNullException(nameof(registryRoot));
        _retainedAdapterRoot = adapterRoot
            ?? throw new ArgumentNullException(nameof(adapterRoot));
        _pathGate = RetainedCapabilityGates.GetValue(
            registryRoot,
            static _ => new object());
        _beforeFinalPublicationValidation = beforeFinalPublicationValidation;
        _beforeRollbackForTesting = null;
        _processLeaseTimeout = TimeSpan.FromSeconds(10);

        var load = ExecuteUnderRegistryBoundary(LoadRegistryStateCore);
        if (!load.IsSuccess || load.Value is null)
            throw new InvalidDataException($"{load.ErrorCode}: {load.ErrorMessage}");
        foreach (var adapter in load.Value.ModelAdapters)
            _registry.Add(adapter.Domain, adapter);
    }

    // Register or overwrite the adapter for adapter.Domain.
    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        return ExecuteUnderRegistryBoundary(() =>
        {
            var latest = LoadRegistryStateCore();
            if (!latest.IsSuccess || latest.Value is null)
            {
                if (StringComparer.Ordinal.Equals(
                        latest.ErrorCode,
                        AdapterReferenceResolverRequiredErrorCode))
                {
                    return OperationResult<bool>.Fail(
                        "A registry containing retained references cannot be written by the legacy registration API.",
                        RetainedReferenceWriteUnsupportedErrorCode);
                }
                return RegistryWriteFailure();
            }
            if (latest.Value.RetainedAdapters.Count != 0)
            {
                return OperationResult<bool>.Fail(
                    "A registry containing retained references cannot be written by the legacy registration API.",
                    RetainedReferenceWriteUnsupportedErrorCode);
            }

            var validation = ValidateAdapter(adapter);
            if (!validation.IsSuccess || validation.Value is null)
                return validation.MapError<bool>();
            var normalized = validation.Value;
            var heldResult = HeldAdapterArtifact.Open(
                normalized.AdapterPath,
                normalized.Provenance!.AdapterSha256);
            if (!heldResult.IsSuccess || heldResult.Value is null)
                return heldResult.MapError<bool>();
            using var heldAdapter = heldResult.Value;
            var candidate = new Dictionary<string, ModelAdapter>(
                latest.Value.ModelAdapters.ToDictionary(value => value.Domain, StringComparer.Ordinal),
                StringComparer.Ordinal)
            {
                [normalized.Domain] = normalized,
            };
            var persistence = PersistRegistryState(
                latest.Value.SchemaVersion,
                candidate.Values,
                () =>
                {
                    _beforeFinalPublicationValidation?.Invoke();
                    return heldAdapter.VerifyUnchangedAndCurrentPath();
                });
            if (!persistence.IsSuccess)
                return persistence;

            ReplaceMemory(candidate.Values);
            return OperationResult<bool>.Ok(true);
        }, "REGISTRY_PERSISTENCE_FAILED");
    }

    // Return the adapter for domain, or null Value when none trained yet (still IsSuccess).
    public OperationResult<ModelAdapter?> GetAdapter(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return OperationResult<ModelAdapter?>.Fail(
                "Adapter domain cannot be empty.",
                "INVALID_ADAPTER_DOMAIN");
        }

        return ExecuteUnderRegistryBoundary(() =>
        {
            var latest = LoadRegistryStateCore();
            if (!latest.IsSuccess || latest.Value is null)
                return latest.MapError<ModelAdapter?>();
            ReplaceMemory(latest.Value.ModelAdapters);
            if (latest.Value.FindRetained(domain) is not null)
            {
                return OperationResult<ModelAdapter?>.Fail(
                    "The requested adapter is retained and requires exact reference resolution.",
                    AdapterReferenceResolverRequiredErrorCode);
            }
            if (!_registry.TryGetValue(domain, out var adapter))
                return OperationResult<ModelAdapter?>.Ok(null);

            var validation = ValidateAdapter(adapter, domain);
            if (!validation.IsSuccess)
            {
                return OperationResult<ModelAdapter?>.Fail(
                    validation.ErrorMessage,
                    validation.ErrorCode);
            }

            return OperationResult<ModelAdapter?>.Ok(validation.Value);
        });
    }

    // Return all registered adapters as an ordered list.
    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
    {
        return ExecuteUnderRegistryBoundary(() =>
        {
            var latest = LoadRegistryStateCore();
            if (!latest.IsSuccess || latest.Value is null)
                return latest.MapError<IReadOnlyList<ModelAdapter>>();
            ReplaceMemory(latest.Value.ModelAdapters);
            if (latest.Value.RetainedAdapters.Count != 0)
            {
                return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                    "Registry contains retained adapter references that require exact resolution.",
                    AdapterReferenceResolverRequiredErrorCode);
            }
            return OperationResult<IReadOnlyList<ModelAdapter>>.Ok(
                Array.AsReadOnly(latest.Value.ModelAdapters.ToArray()));
        });
    }

    OperationResult<IReadOnlyList<ModelAdapter>>
        IDpoRegistryPublicationTransaction.PublishBatchAndCommit(
            IReadOnlyList<ModelAdapter> adapters,
            Func<IReadOnlyList<ModelAdapter>, OperationResult<bool>> commit)
        => ExecuteUnderRegistryBoundary(
            () => PublishBatchAndCommitCore(adapters, commit),
            "REGISTRY_PERSISTENCE_FAILED");

    private OperationResult<IReadOnlyList<ModelAdapter>> PublishBatchAndCommitCore(
        IReadOnlyList<ModelAdapter> adapters,
        Func<IReadOnlyList<ModelAdapter>, OperationResult<bool>> commit)
    {
        if (adapters is null
            || commit is null
            || adapters.Count is <= 0
                or > FileDpoPairStore.MaximumTransactionDomainCount)
        {
            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                "The DPO registry publication batch is invalid.",
                "DPO_REGISTRY_PUBLICATION_INVALID");
        }

        ModelAdapter[] snapshot;
        try
        {
            snapshot = adapters.ToArray();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                "The DPO registry publication batch could not be snapshotted.",
                "DPO_REGISTRY_PUBLICATION_INVALID");
        }
        if (snapshot.Any(static value => value is null)
            || snapshot.Select(static value => value.Domain)
                    .Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
        {
            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                "The DPO registry publication batch contains duplicate or missing adapters.",
                "DPO_REGISTRY_PUBLICATION_INVALID");
        }

        var latest = LoadRegistryStateCore();
        if (!latest.IsSuccess || latest.Value is null)
            return latest.MapError<IReadOnlyList<ModelAdapter>>();
        if (latest.Value.RetainedAdapters.Count != 0)
        {
            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                "A registry containing retained references cannot be written by the legacy DPO publication API.",
                RetainedReferenceWriteUnsupportedErrorCode);
        }

        var held = new List<HeldAdapterArtifact>(snapshot.Length);
        try
        {
            var normalized = new ModelAdapter[snapshot.Length];
            for (var index = 0; index < snapshot.Length; index++)
            {
                var validation = ValidateAdapter(snapshot[index]);
                if (!validation.IsSuccess || validation.Value is null)
                    return validation.MapError<IReadOnlyList<ModelAdapter>>();
                if (validation.Value.Provenance?.DpoPublication is null)
                {
                    return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                        "Every adapter in a DPO registry transaction requires an exact publication receipt.",
                        "DPO_REGISTRY_PUBLICATION_INVALID");
                }
                var heldResult = HeldAdapterArtifact.Open(
                    validation.Value.AdapterPath,
                    validation.Value.Provenance.AdapterSha256);
                if (!heldResult.IsSuccess || heldResult.Value is null)
                    return heldResult.MapError<IReadOnlyList<ModelAdapter>>();
                held.Add(heldResult.Value);
                normalized[index] = validation.Value;
            }

            var candidate = new Dictionary<string, ModelAdapter>(
                latest.Value.ModelAdapters.ToDictionary(
                    static value => value.Domain,
                    StringComparer.Ordinal),
                StringComparer.Ordinal);
            foreach (var adapter in normalized)
                candidate[adapter.Domain] = adapter;
            var orderedCandidate = candidate.Values
                .OrderBy(static value => value.Domain, StringComparer.Ordinal)
                .ToArray();
            var persistence = PersistRegistryState(
                latest.Value.SchemaVersion,
                orderedCandidate,
                () => held.All(static value =>
                    value.VerifyUnchangedAndCurrentPath()));
            if (!persistence.IsSuccess || persistence.Value is not true)
                return persistence.MapError<IReadOnlyList<ModelAdapter>>();

            var durable = LoadRegistryStateCore();
            if (!durable.IsSuccess
                || durable.Value is null
                || durable.Value.RetainedAdapters.Count != 0
                || !CanonicalAdapterCollectionsEqual(
                    orderedCandidate,
                    durable.Value.ModelAdapters)
                || held.Any(static value =>
                    !value.VerifyUnchangedAndCurrentPath()))
            {
                return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                    "The exact DPO registry publication batch could not be read back.",
                    "DPO_REGISTRY_PUBLICATION_INCOMPLETE");
            }

            ReplaceMemory(durable.Value.ModelAdapters);
            var publishedDomains = normalized
                .Select(static value => value.Domain)
                .ToHashSet(StringComparer.Ordinal);
            var published = Array.AsReadOnly(durable.Value.ModelAdapters
                .Where(value => publishedDomains.Contains(value.Domain))
                .OrderBy(static value => value.Domain, StringComparer.Ordinal)
                .ToArray());
            if (published.Count != normalized.Length)
            {
                return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                    "The exact DPO registry publication batch could not be read back.",
                    "DPO_REGISTRY_PUBLICATION_INCOMPLETE");
            }

            // The registry lock and process lease remain held while this
            // callback acquires the pair-store transaction lock. This is the
            // sole supported lock order: registry -> DPO transaction.
            var committed = commit(published);
            if (!committed.IsSuccess || committed.Value is not true)
                return committed.MapError<IReadOnlyList<ModelAdapter>>();
            return OperationResult<IReadOnlyList<ModelAdapter>>.Ok(published);
        }
        finally
        {
            foreach (var artifact in held)
                artifact.Dispose();
        }
    }

    internal OperationResult<ResolvedRetainedAdapterArtifact?> ResolveRetainedAdapter(
        string domain,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return OperationResult<ResolvedRetainedAdapterArtifact?>.Fail(
                "Adapter domain cannot be empty.",
                "INVALID_ADAPTER_DOMAIN");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var capture = ExecuteUnderRegistryBoundary(() => CaptureRetainedAdapter(domain));
        if (!capture.IsSuccess)
            return capture.MapError<ResolvedRetainedAdapterArtifact?>();
        if (capture.Value is null)
            return OperationResult<ResolvedRetainedAdapterArtifact?>.Ok(null);
        if (_retainedAdapterRoot is null)
        {
            return OperationResult<ResolvedRetainedAdapterArtifact?>.Fail(
                "Retained adapter resolution requires an AdapterRoot capability.",
                AdapterReferenceResolverRequiredErrorCode);
        }

        var retained = capture.Value;
        ServerOwnedContinuousLearningAdapterPublication expectedPublication;
        ServerOwnedContinuousLearningAdapterSnapshot snapshot;
        try
        {
            expectedPublication = retained.Reference.ToPublication();
            snapshot = _retainedAdapterRoot.ReadExact(
                retained.Reference,
                cancellationToken);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return UntrustedRetainedAdapter();
        }

        if (!ExactSnapshotMatches(retained, expectedPublication, snapshot))
            return UntrustedRetainedAdapter();

        cancellationToken.ThrowIfCancellationRequested();
        var final = ExecuteUnderRegistryBoundary(() => VerifyRetainedAdapterUnchanged(retained));
        if (!final.IsSuccess || final.Value is null)
            return ChangedRetainedAdapter();
        cancellationToken.ThrowIfCancellationRequested();

        return OperationResult<ResolvedRetainedAdapterArtifact?>.Ok(
            new ResolvedRetainedAdapterArtifact(
                retained.Domain,
                retained.TrainingExamples,
                retained.TrainedAt,
                retained.Provenance,
                retained.Reference,
                snapshot));
    }

    private OperationResult<ParsedRetainedRegistryAdapter?> CaptureRetainedAdapter(
        string domain)
    {
        var latest = LoadRegistryStateCore();
        if (!latest.IsSuccess || latest.Value is null)
            return latest.MapError<ParsedRetainedRegistryAdapter?>();

        var retained = latest.Value.FindRetained(domain);
        if (retained is not null)
            return OperationResult<ParsedRetainedRegistryAdapter?>.Ok(retained);
        if (latest.Value.FindLegacy(domain) is not null)
        {
            return OperationResult<ParsedRetainedRegistryAdapter?>.Fail(
                "The requested adapter uses legacy path-backed storage.",
                AdapterReferenceNotRetainedErrorCode);
        }
        return OperationResult<ParsedRetainedRegistryAdapter?>.Ok(null);
    }

    private OperationResult<ParsedRetainedRegistryAdapter?>
        VerifyRetainedAdapterUnchanged(ParsedRetainedRegistryAdapter captured)
    {
        var latest = LoadRegistryStateCore();
        if (!latest.IsSuccess || latest.Value is null)
            return ChangedRetainedEntry();

        var current = latest.Value.FindRetained(captured.Domain);
        return current is not null && CanonicalRetainedAdapterEquals(captured, current)
            ? OperationResult<ParsedRetainedRegistryAdapter?>.Ok(current)
            : ChangedRetainedEntry();
    }

    private static bool ExactSnapshotMatches(
        ParsedRetainedRegistryAdapter retained,
        ServerOwnedContinuousLearningAdapterPublication expectedPublication,
        ServerOwnedContinuousLearningAdapterSnapshot snapshot)
        => snapshot.Publication == expectedPublication
           && StringComparer.Ordinal.Equals(
               snapshot.CanonicalLeafName,
               retained.Reference.CanonicalLeafName)
           && snapshot.RetainedRootIdentity == retained.Reference.RetainedRootIdentity
           && snapshot.FinalArtifactIdentity == retained.Reference.FinalArtifactIdentity
           && snapshot.Length == retained.Reference.Length
           && StringComparer.Ordinal.Equals(
               snapshot.Sha256Hex,
               retained.Reference.Sha256Hex)
           && StringComparer.Ordinal.Equals(
               retained.Provenance.AdapterSha256,
               retained.Reference.Sha256Hex);

    private static OperationResult<ResolvedRetainedAdapterArtifact?>
        UntrustedRetainedAdapter()
        => OperationResult<ResolvedRetainedAdapterArtifact?>.Fail(
            "The retained adapter artifact failed exact trust validation.",
            AdapterReferenceUntrustedErrorCode);

    private static OperationResult<ResolvedRetainedAdapterArtifact?>
        ChangedRetainedAdapter()
        => OperationResult<ResolvedRetainedAdapterArtifact?>.Fail(
            "The retained adapter registry entry changed during resolution.",
            AdapterReferenceChangedErrorCode);

    private static OperationResult<ParsedRetainedRegistryAdapter?> ChangedRetainedEntry()
        => OperationResult<ParsedRetainedRegistryAdapter?>.Fail(
            "The retained adapter registry entry changed during resolution.",
            AdapterReferenceChangedErrorCode);

    private OperationResult<ParsedRegistryState> LoadRegistryStateCore()
    {
        if (_retainedRegistryRoot is not null)
            return LoadRetainedRegistryStateCore();

        if (!File.Exists(_registryPath!))
        {
            if (Directory.Exists(_registryPath!))
                return RegistryStateFailure();
            if (Directory.Exists(_registryDir!))
            {
                try
                {
                    EnsureNoReparseDirectoryChain(_registryDir!);
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return RegistryStateFailure();
                }
            }
            return EmptyRegistryState();
        }

        try
        {
            EnsureSafeRegistryPath(_registryPath!);
            using var stream = new FileStream(
                _registryPath!,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                FileOptions.SequentialScan);
            if (stream.Length <= 0 || stream.Length > MaximumRegistryBytes)
                return RegistryStateFailure();
            var bytes = new byte[checked((int)stream.Length)];
            stream.ReadExactly(bytes);
            return ParseRegistryStateForCurrentMode(bytes);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RegistryStateFailure();
        }
    }

    private OperationResult<ParsedRegistryState> LoadRetainedRegistryStateCore()
    {
        try
        {
            var canonical = _retainedRegistryRoot!.ReadCanonical();
            return canonical.State ==
                   ServerOwnedContinuousLearningRegistryCanonicalState.Absent
                ? EmptyRegistryState()
                : canonical.Snapshot is null
                    ? RegistryStateFailure()
                    : ParseRegistryStateForCurrentMode(canonical.Snapshot.CopyBytes());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RegistryStateFailure();
        }
    }

    private OperationResult<ParsedRegistryState> ParseRegistryStateForCurrentMode(
        ReadOnlySpan<byte> bytes)
        => ParseRegistryState(
            bytes,
            quarantineRetainedReferences: _retainedAdapterRoot is null);

    private static OperationResult<ParsedRegistryState> ParseRegistryState(
        ReadOnlySpan<byte> bytes,
        bool quarantineRetainedReferences)
    {
        try
        {
            var preflight = PreflightRegistryState(bytes);
            if (!preflight.IsSuccess || preflight.Value is null)
                return preflight.MapError<ParsedRegistryState>();
            if (quarantineRetainedReferences && preflight.Value.ContainsRetainedReference)
                return RetainedReferenceResolverRequiredState();

            return preflight.Value.SchemaVersion == RegistrySchemaVersion
                ? MaterializeVersion1RegistryState(preflight.Value.Version1Adapters)
                : MaterializeVersion2RegistryState(preflight.Value);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RegistryStateFailure();
        }
    }

    private static OperationResult<RegistryStructuralPreflight> PreflightRegistryState(
        ReadOnlySpan<byte> bytes)
    {
        try
        {
            if (bytes.IsEmpty || bytes.Length > MaximumRegistryBytes)
                return RegistryStateFailure().MapError<RegistryStructuralPreflight>();
            ValidateStrictJson(bytes);
            using var document = JsonDocument.Parse(bytes.ToArray());
            var schemaVersion = ReadRegistrySchemaVersion(document.RootElement);
            if (schemaVersion == RegistrySchemaVersion2)
                return PreflightVersion2RegistryState(document.RootElement);
            if (schemaVersion != RegistrySchemaVersion)
                return RegistryStateFailure().MapError<RegistryStructuralPreflight>();

            var envelope = JsonSerializer.Deserialize<RegistryEnvelope>(bytes, JsonOptions);
            if (envelope is null
                || envelope.SchemaVersion != RegistrySchemaVersion
                || envelope.Adapters is null)
            {
                return RegistryStateFailure().MapError<RegistryStructuralPreflight>();
            }

            var domains = new HashSet<string>(StringComparer.Ordinal);
            string? previousDomain = null;
            foreach (var stored in envelope.Adapters)
            {
                if (stored is null || !domains.Add(stored.Domain))
                {
                    return RegistryStateFailure(
                            "Registry contains a duplicate adapter domain.")
                        .MapError<RegistryStructuralPreflight>();
                }
                if (previousDomain is not null
                    && StringComparer.Ordinal.Compare(previousDomain, stored.Domain) >= 0)
                {
                    return RegistryStateFailure(
                            "Registry entries are not canonically ordered.")
                        .MapError<RegistryStructuralPreflight>();
                }
                if (!TryCanonicalizeVersion2AdapterMetadata(
                        new RegistryVersion2StoredAdapter(
                            stored.Domain,
                            stored.TrainingExamples,
                            stored.TrainedAt,
                            stored.Provenance,
                            LegacyAbsolutePathStorageKind,
                            stored.AdapterPath,
                            RetainedReference: null),
                        out _))
                {
                    return RegistryStateFailure(
                            "Registry contains non-canonical adapter metadata.")
                        .MapError<RegistryStructuralPreflight>();
                }
                previousDomain = stored.Domain;
            }

            return OperationResult<RegistryStructuralPreflight>.Ok(
                new RegistryStructuralPreflight(
                    RegistrySchemaVersion,
                    Array.AsReadOnly(envelope.Adapters.ToArray()),
                    Array.AsReadOnly(Array.Empty<RegistryVersion2StoredAdapter>()),
                    new Dictionary<string, ParsedRetainedRegistryAdapter>(
                        StringComparer.Ordinal)));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RegistryStateFailure().MapError<RegistryStructuralPreflight>();
        }
    }

    private static OperationResult<ParsedRegistryState> MaterializeVersion1RegistryState(
        IReadOnlyList<ModelAdapter> storedAdapters)
    {
        var validated = new List<ModelAdapter>(storedAdapters.Count);
        foreach (var stored in storedAdapters)
        {
            var validation = ValidateAdapter(stored);
            if (!validation.IsSuccess || validation.Value is null)
                return validation.MapError<ParsedRegistryState>();
            if (!CanonicalAdapterEquals(stored, validation.Value))
                return RegistryStateFailure("Registry contains non-canonical adapter metadata.");
            validated.Add(validation.Value);
        }
        return OperationResult<ParsedRegistryState>.Ok(
            new ParsedRegistryState(
                RegistrySchemaVersion,
                Array.AsReadOnly<ParsedRegistryAdapter>(validated
                    .Select(static adapter => new ParsedLegacyRegistryAdapter(adapter))
                    .ToArray())));
    }

    private static int ReadRegistrySchemaVersion(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("Registry root must be an object.");

        var hasSchemaVersion = false;
        var hasAdapters = false;
        var schemaVersion = 0;
        foreach (var property in root.EnumerateObject())
        {
            switch (property.Name)
            {
                case nameof(RegistryEnvelope.SchemaVersion):
                    if (hasSchemaVersion
                        || property.Value.ValueKind != JsonValueKind.Number
                        || !property.Value.TryGetInt32(out schemaVersion))
                    {
                        throw new JsonException("Registry schema version is invalid.");
                    }
                    hasSchemaVersion = true;
                    break;
                case nameof(RegistryEnvelope.Adapters):
                    if (hasAdapters || property.Value.ValueKind != JsonValueKind.Array)
                        throw new JsonException("Registry adapters are invalid.");
                    hasAdapters = true;
                    break;
                default:
                    throw new JsonException("Registry contains an unknown property.");
            }
        }
        if (!hasSchemaVersion || !hasAdapters)
            throw new JsonException("Registry is missing a required property.");
        return schemaVersion;
    }

    private static OperationResult<RegistryStructuralPreflight>
        PreflightVersion2RegistryState(JsonElement root)
    {
        try
        {
            var adaptersElement = root.GetProperty(nameof(RegistryEnvelope.Adapters));
            var storedAdapters = new List<RegistryVersion2StoredAdapter>();
            var retainedByDomain = new Dictionary<
                string,
                ParsedRetainedRegistryAdapter>(StringComparer.Ordinal);
            var domains = new HashSet<string>(StringComparer.Ordinal);
            string? previousDomain = null;
            foreach (var adapterElement in adaptersElement.EnumerateArray())
            {
                var stored = ParseVersion2Adapter(adapterElement);
                if (!domains.Add(stored.Domain))
                    throw new JsonException("Registry contains a duplicate adapter domain.");
                if (previousDomain is not null
                    && StringComparer.Ordinal.Compare(previousDomain, stored.Domain) >= 0)
                {
                    throw new JsonException("Registry entries are not canonically ordered.");
                }

                if (stored.StorageKind == RetainedReferenceStorageKind)
                {
                    if (!TryCanonicalizeVersion2AdapterMetadata(
                            stored,
                            out var canonicalProvenance)
                        || stored.RetainedReference is null
                        || !StringComparer.Ordinal.Equals(
                            stored.RetainedReference.Sha256Hex,
                            canonicalProvenance!.AdapterSha256))
                    {
                        throw new JsonException("Retained adapter reference is invalid.");
                    }
                    retainedByDomain.Add(stored.Domain, new ParsedRetainedRegistryAdapter(
                        stored.Domain,
                        stored.TrainingExamples,
                        stored.TrainedAt,
                        canonicalProvenance!,
                        stored.RetainedReference));
                }
                else if (!TryCanonicalizeVersion2AdapterMetadata(stored, out _))
                {
                    throw new JsonException("Legacy adapter metadata is invalid.");
                }
                storedAdapters.Add(stored);
                previousDomain = stored.Domain;
            }

            return OperationResult<RegistryStructuralPreflight>.Ok(
                new RegistryStructuralPreflight(
                    RegistrySchemaVersion2,
                    Array.AsReadOnly(Array.Empty<ModelAdapter>()),
                    Array.AsReadOnly(storedAdapters.ToArray()),
                    retainedByDomain));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RegistryStateFailure().MapError<RegistryStructuralPreflight>();
        }
    }

    private static OperationResult<ParsedRegistryState> MaterializeVersion2RegistryState(
        RegistryStructuralPreflight preflight)
    {
        var parsedAdapters = new List<ParsedRegistryAdapter>(
            preflight.Version2Adapters.Count);
        foreach (var stored in preflight.Version2Adapters)
        {
            if (stored.StorageKind == RetainedReferenceStorageKind)
            {
                parsedAdapters.Add(preflight.RetainedByDomain[stored.Domain]);
                continue;
            }

            var adapter = new ModelAdapter(
                stored.Domain,
                stored.LegacyAbsolutePath!,
                stored.TrainingExamples,
                stored.TrainedAt,
                stored.Provenance);
            var validation = ValidateAdapter(adapter);
            if (!validation.IsSuccess || validation.Value is null)
                return validation.MapError<ParsedRegistryState>();
            if (!CanonicalVersion2LegacyAdapterEquals(stored, validation.Value))
            {
                return RegistryStateFailure(
                    "Registry contains non-canonical adapter metadata.");
            }
            parsedAdapters.Add(new ParsedLegacyRegistryAdapter(validation.Value));
        }

        return OperationResult<ParsedRegistryState>.Ok(
            new ParsedRegistryState(
                RegistrySchemaVersion2,
                Array.AsReadOnly(parsedAdapters.ToArray())));
    }

    private static RegistryVersion2StoredAdapter ParseVersion2Adapter(
        JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new JsonException("Registry adapter must be an object.");

        string? domain = null;
        string? legacyAbsolutePath = null;
        var trainingExamples = 0;
        var trainedAt = default(DateTimeOffset);
        ModelAdapterProvenance? provenance = null;
        AdapterArtifactReference? retainedReference = null;
        var storageKind = 0;
        var hasDomain = false;
        var hasTrainingExamples = false;
        var hasTrainedAt = false;
        var hasProvenance = false;
        var hasStorageKind = false;
        var hasLegacyAbsolutePath = false;
        var hasRetainedReference = false;

        foreach (var property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case nameof(ModelAdapter.Domain):
                    RejectDuplicate(hasDomain);
                    hasDomain = true;
                    domain = DeserializeRequired<string>(property.Value);
                    break;
                case nameof(ModelAdapter.TrainingExamples):
                    RejectDuplicate(hasTrainingExamples);
                    hasTrainingExamples = true;
                    trainingExamples = DeserializeRequired<int>(property.Value);
                    break;
                case nameof(ModelAdapter.TrainedAt):
                    RejectDuplicate(hasTrainedAt);
                    hasTrainedAt = true;
                    trainedAt = DeserializeRequired<DateTimeOffset>(property.Value);
                    break;
                case nameof(ModelAdapter.Provenance):
                    RejectDuplicate(hasProvenance);
                    hasProvenance = true;
                    provenance = DeserializeRequired<ModelAdapterProvenance>(property.Value);
                    break;
                case nameof(RegistryVersion2StoredAdapter.StorageKind):
                    RejectDuplicate(hasStorageKind);
                    hasStorageKind = true;
                    storageKind = DeserializeRequired<int>(property.Value);
                    break;
                case nameof(RegistryVersion2StoredAdapter.LegacyAbsolutePath):
                    RejectDuplicate(hasLegacyAbsolutePath);
                    hasLegacyAbsolutePath = true;
                    legacyAbsolutePath = DeserializeRequired<string>(property.Value);
                    break;
                case nameof(RegistryVersion2StoredAdapter.RetainedReference):
                    RejectDuplicate(hasRetainedReference);
                    hasRetainedReference = true;
                    retainedReference = DeserializeRequired<AdapterArtifactReference>(
                        property.Value);
                    break;
                default:
                    throw new JsonException("Registry adapter contains an unknown property.");
            }
        }

        if (!hasDomain || !hasTrainingExamples || !hasTrainedAt
            || !hasProvenance || !hasStorageKind)
        {
            throw new JsonException("Registry adapter is missing a required property.");
        }
        if (storageKind == LegacyAbsolutePathStorageKind)
        {
            if (!hasLegacyAbsolutePath || hasRetainedReference)
                throw new JsonException("Legacy path storage union is invalid.");
        }
        else if (storageKind == RetainedReferenceStorageKind)
        {
            if (!hasRetainedReference || hasLegacyAbsolutePath)
                throw new JsonException("Retained reference storage union is invalid.");
        }
        else
        {
            throw new JsonException("Registry adapter storage kind is unsupported.");
        }

        return new RegistryVersion2StoredAdapter(
            domain!,
            trainingExamples,
            trainedAt,
            provenance,
            storageKind,
            legacyAbsolutePath,
            retainedReference);
    }

    private static T DeserializeRequired<T>(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
            throw new JsonException("Registry property cannot be null.");
        return JsonSerializer.Deserialize<T>(element.GetRawText(), JsonOptions)
               ?? throw new JsonException("Registry property is invalid.");
    }

    private static void RejectDuplicate(bool alreadyPresent)
    {
        if (alreadyPresent)
            throw new JsonException("Registry property is duplicated.");
    }

    private static bool CanonicalVersion2LegacyAdapterEquals(
        RegistryVersion2StoredAdapter stored,
        ModelAdapter canonical)
        => stored.StorageKind == LegacyAbsolutePathStorageKind
           && StringComparer.Ordinal.Equals(stored.Domain, canonical.Domain)
           && StringComparer.Ordinal.Equals(stored.LegacyAbsolutePath, canonical.AdapterPath)
           && stored.TrainingExamples == canonical.TrainingExamples
           && stored.TrainedAt.Offset == TimeSpan.Zero
           && stored.TrainedAt.Equals(canonical.TrainedAt)
           && CanonicalProvenanceEquals(stored.Provenance, canonical.Provenance);

    private static bool CanonicalProvenanceEquals(
        ModelAdapterProvenance? stored,
        ModelAdapterProvenance? canonical)
        => stored is not null
           && canonical is not null
           && stored.TrainingSources is not null
           && canonical.TrainingSources is not null
           && stored.TrainingSources.SourceIds is not null
           && canonical.TrainingSources.SourceIds is not null
           && StringComparer.Ordinal.Equals(
               stored.TrainingSources.Sha256,
               canonical.TrainingSources.Sha256)
           && stored.TrainingSources.SourceIds.SequenceEqual(
               canonical.TrainingSources.SourceIds,
               StringComparer.Ordinal)
           && StringComparer.Ordinal.Equals(
               stored.AdapterSha256,
               canonical.AdapterSha256)
           && StringComparer.Ordinal.Equals(
               stored.BaseCheckpointSha256,
               canonical.BaseCheckpointSha256)
           && StringComparer.Ordinal.Equals(
               stored.TokenizerSha256,
               canonical.TokenizerSha256)
           && stored.Model == canonical.Model
           && stored.Lora is not null
           && canonical.Lora is not null
           && stored.Lora.OrderedTargetModules is not null
           && canonical.Lora.OrderedTargetModules is not null
           && stored.Lora.Rank == canonical.Lora.Rank
           && stored.Lora.Alpha.Equals(canonical.Lora.Alpha)
           && stored.Lora.Scaling.Equals(canonical.Lora.Scaling)
           && stored.Lora.Dropout.Equals(canonical.Lora.Dropout)
           && StringComparer.Ordinal.Equals(
               stored.Lora.Sha256,
               canonical.Lora.Sha256)
           && stored.Lora.OrderedTargetModules.SequenceEqual(
               canonical.Lora.OrderedTargetModules,
               StringComparer.Ordinal)
           && DpoPublicationEquals(
               stored.DpoPublication,
               canonical.DpoPublication);

    private static bool DpoPublicationEquals(
        DpoAdapterPublicationReceipt? left,
        DpoAdapterPublicationReceipt? right)
        => left is null && right is null
           || left is not null
              && right is not null
              && left.OrderedPairIds is not null
              && right.OrderedPairIds is not null
              && left.OrderedDomains is not null
              && right.OrderedDomains is not null
              && StringComparer.Ordinal.Equals(
                  left.TransactionId,
                  right.TransactionId)
              && StringComparer.Ordinal.Equals(
                  left.PairBatchSha256,
                  right.PairBatchSha256)
              && left.OrderedPairIds.SequenceEqual(
                  right.OrderedPairIds,
                  StringComparer.Ordinal)
              && left.OrderedDomains.SequenceEqual(
                  right.OrderedDomains,
                  StringComparer.Ordinal);

    private static bool TryCanonicalizeVersion2AdapterMetadata(
        RegistryVersion2StoredAdapter stored,
        out ModelAdapterProvenance? canonicalProvenance)
    {
        canonicalProvenance = null;
        if (string.IsNullOrWhiteSpace(stored.Domain)
            || !StringComparer.Ordinal.Equals(stored.Domain, stored.Domain.Trim())
            || stored.TrainingExamples <= 0
            || stored.TrainedAt == default
            || stored.TrainedAt.Offset != TimeSpan.Zero
            || stored.Provenance is null
            || stored.Provenance.TrainingSources is null
            || stored.Provenance.Model is null
            || stored.Provenance.Lora is null
            || stored.Provenance.Lora.OrderedTargetModules is null)
        {
            return false;
        }

        try
        {
            var manifest = TrainingSourceProvenance.BuildManifest(
                stored.Provenance.TrainingSources.SourceIds);
            if (!manifest.IsSuccess || manifest.Value is null
                || !StringComparer.Ordinal.Equals(
                    manifest.Value.Sha256,
                    stored.Provenance.TrainingSources.Sha256)
                || !TrainingSourceProvenance.IsCanonicalSha256(
                    stored.Provenance.AdapterSha256)
                || !TrainingSourceProvenance.IsCanonicalSha256(
                    stored.Provenance.BaseCheckpointSha256)
                || !TrainingSourceProvenance.IsCanonicalSha256(
                    stored.Provenance.TokenizerSha256))
            {
                return false;
            }

            var model = TrainingSourceProvenance.Identify(
                new TransformerModel.Config.ModelConfig
                {
                    VocabSize = stored.Provenance.Model.VocabSize,
                    ContextLen = stored.Provenance.Model.ContextLen,
                    HiddenDim = stored.Provenance.Model.HiddenDim,
                    NumLayers = stored.Provenance.Model.NumLayers,
                    NumHeads = stored.Provenance.Model.NumHeads,
                    NumKvHeads = stored.Provenance.Model.NumKvHeads,
                    FfnHiddenDim = stored.Provenance.Model.FfnHiddenDim,
                    RopeTheta = stored.Provenance.Model.RopeTheta,
                    RmsNormEps = stored.Provenance.Model.RmsNormEps,
                });
            var lora = TrainingSourceProvenance.Identify(new LoRA.LoraConfig
            {
                Rank = stored.Provenance.Lora.Rank,
                Alpha = stored.Provenance.Lora.Alpha,
                Dropout = stored.Provenance.Lora.Dropout,
                TargetModules = stored.Provenance.Lora.OrderedTargetModules.ToArray(),
            });
            var dpoReceipt = CanonicalizeDpoPublicationReceipt(
                stored.Provenance.DpoPublication);
            if (!dpoReceipt.IsSuccess)
                return false;
            canonicalProvenance = new ModelAdapterProvenance(
                manifest.Value,
                stored.Provenance.AdapterSha256,
                stored.Provenance.BaseCheckpointSha256,
                stored.Provenance.TokenizerSha256,
                model,
                lora,
                dpoReceipt.Value);
            return CanonicalProvenanceEquals(stored.Provenance, canonicalProvenance);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            canonicalProvenance = null;
            return false;
        }
    }

    private static RegistrySerializationPlan PreflightRegistrySerialization(
        int schemaVersion,
        IReadOnlyList<ModelAdapter> ordered)
    {
        if (!HasBoundedRegistrySerializationFields(ordered))
            throw new RegistrySerializationCapacityException();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var bounded = new BoundedRegistryWriteStream(
            destination: null,
            hash,
            MaximumRegistryBytes);
        using (var writer = new Utf8JsonWriter(bounded))
        {
            WriteRegistryStateJson(writer, schemaVersion, ordered);
            writer.Flush();
        }
        if (bounded.BytesWritten <= 0)
            throw new RegistrySerializationCapacityException();
        return new RegistrySerializationPlan(
            bounded.BytesWritten,
            Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    private static bool HasBoundedRegistrySerializationFields(
        IReadOnlyList<ModelAdapter> ordered)
    {
        long aggregateBytes = 0;
        bool Add(string? value)
        {
            if (value is null)
                return false;
            var length = Encoding.UTF8.GetByteCount(value);
            if (length > MaximumRegistryBytes - aggregateBytes)
            {
                return false;
            }
            aggregateBytes += length;
            return true;
        }

        try
        {
            foreach (var adapter in ordered)
            {
                if (adapter is null
                    || !Add(adapter.Domain)
                    || !Add(adapter.AdapterPath))
                {
                    return false;
                }
                if (adapter.Provenance is not { } provenance)
                    continue;
                if (provenance.TrainingSources is null
                    || provenance.TrainingSources.SourceIds is null
                    || provenance.Model is null
                    || provenance.Lora is null
                    || provenance.Lora.OrderedTargetModules is null
                    || !Add(provenance.TrainingSources.Sha256)
                    || !Add(provenance.AdapterSha256)
                    || !Add(provenance.BaseCheckpointSha256)
                    || !Add(provenance.TokenizerSha256)
                    || !Add(provenance.Model.Sha256)
                    || !Add(provenance.Lora.Sha256))
                {
                    return false;
                }
                foreach (var sourceId in provenance.TrainingSources.SourceIds)
                {
                    if (!Add(sourceId))
                        return false;
                }
                foreach (var module in provenance.Lora.OrderedTargetModules)
                {
                    if (!Add(module))
                        return false;
                }
                if (provenance.DpoPublication is not { } receipt)
                    continue;
                if (receipt.OrderedPairIds is null
                    || receipt.OrderedDomains is null
                    || !Add(receipt.TransactionId)
                    || !Add(receipt.PairBatchSha256))
                {
                    return false;
                }
                foreach (var pairId in receipt.OrderedPairIds)
                {
                    if (!Add(pairId))
                        return false;
                }
                foreach (var domain in receipt.OrderedDomains)
                {
                    if (!Add(domain))
                        return false;
                }
            }
            return true;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static void WriteRegistryStateJson(
        Utf8JsonWriter writer,
        int schemaVersion,
        IReadOnlyList<ModelAdapter> ordered)
    {
        if (schemaVersion is not (
                RegistrySchemaVersion or RegistrySchemaVersion2))
        {
            throw new InvalidDataException(
                "Registry schema version is unsupported.");
        }
        writer.WriteStartObject();
        writer.WriteNumber(nameof(RegistryEnvelope.SchemaVersion), schemaVersion);
        writer.WriteStartArray(nameof(RegistryEnvelope.Adapters));
        foreach (var adapter in ordered)
        {
            if (schemaVersion == RegistrySchemaVersion)
            {
                JsonSerializer.Serialize(writer, adapter, JsonOptions);
                continue;
            }
            JsonSerializer.Serialize(
                writer,
                new RegistryVersion2LegacyAdapterEnvelope(
                    adapter.Domain,
                    adapter.TrainingExamples,
                    adapter.TrainedAt,
                    adapter.Provenance,
                    LegacyAbsolutePathStorageKind,
                    adapter.AdapterPath),
                JsonOptions);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteRegistryState(
        Stream destination,
        int schemaVersion,
        IReadOnlyList<ModelAdapter> ordered,
        RegistrySerializationPlan expected)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var bounded = new BoundedRegistryWriteStream(
            destination,
            hash,
            MaximumRegistryBytes);
        using (var writer = new Utf8JsonWriter(bounded))
        {
            WriteRegistryStateJson(writer, schemaVersion, ordered);
            writer.Flush();
        }
        var digest = Convert.ToHexString(hash.GetHashAndReset())
            .ToLowerInvariant();
        if (bounded.BytesWritten != expected.Length
            || !StringComparer.Ordinal.Equals(digest, expected.Sha256))
        {
            throw new InvalidDataException(
                "Registry serialization changed after bounded preflight.");
        }
    }

    private static byte[] MaterializeRegistryState(
        int schemaVersion,
        IReadOnlyList<ModelAdapter> ordered,
        RegistrySerializationPlan expected)
    {
        var bytes = GC.AllocateUninitializedArray<byte>(
            checked((int)expected.Length));
        using var stream = new MemoryStream(
            bytes,
            index: 0,
            count: bytes.Length,
            writable: true,
            publiclyVisible: true);
        stream.SetLength(0);
        WriteRegistryState(stream, schemaVersion, ordered, expected);
        if (stream.Length != bytes.LongLength)
            throw new InvalidDataException("Registry serialization was incomplete.");
        return bytes;
    }

    internal static OperationResult<long> PreflightRegistrySerializationForTesting(
        int schemaVersion,
        IReadOnlyList<ModelAdapter> adapters)
    {
        try
        {
            var ordered = adapters
                .OrderBy(static value => value.Domain, StringComparer.Ordinal)
                .ToArray();
            return OperationResult<long>.Ok(
                PreflightRegistrySerialization(schemaVersion, ordered).Length);
        }
        catch (RegistrySerializationCapacityException)
        {
            return RegistryWriteFailure().MapError<long>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RegistryWriteFailure().MapError<long>();
        }
    }

    private OperationResult<bool> PersistRegistryState(
        int schemaVersion,
        IEnumerable<ModelAdapter> adapters,
        Func<bool> finalAdapterValidation)
    {
        if (_retainedRegistryRoot is not null)
            return PersistRetainedRegistryState(
                schemaVersion,
                adapters,
                finalAdapterValidation);

        string? temporaryPath = null;
        string? backupPath = null;
        byte[]? priorBytes = null;
        var published = false;
        var committed = false;
        var rollbackConfirmed = false;
        try
        {
            EnsureSafeRegistryDirectory();
            var ordered = adapters.OrderBy(value => value.Domain, StringComparer.Ordinal).ToArray();
            var serialization = PreflightRegistrySerialization(
                schemaVersion,
                ordered);

            if (File.Exists(_registryPath))
            {
                EnsureSafeRegistryPath(_registryPath);
                priorBytes = ReadBoundedRegistryBytes(_registryPath);
            }
            backupPath = Path.Combine(
                _registryDir!,
                $".{RegistryFileName}.{Guid.NewGuid():N}.bak");

            // For a first publication an empty backup is an explicit
            // "previously absent" marker. A crash before commit can therefore
            // restore absence just as File.Replace restores prior bytes.
            if (priorBytes is null)
                WriteRecoveryArtifact(backupPath, priorBytes);

            temporaryPath = Path.Combine(
                _registryDir!,
                $".{RegistryFileName}.{Guid.NewGuid():N}.tmp");
            using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                FileOptions.WriteThrough))
            {
                WriteRegistryState(
                    stream,
                    schemaVersion,
                    ordered,
                    serialization);
                stream.Flush(flushToDisk: true);
            }

            EnsureSafeRegistryPath(temporaryPath);
            if (priorBytes is null)
            {
                File.Move(temporaryPath, _registryPath!);
            }
            else
            {
                File.Replace(temporaryPath, _registryPath!, backupPath);
            }
            temporaryPath = null;
            published = true;
            EnsureSafeRegistryPath(_registryPath!);

            var durable = LoadRegistryStateCore();
            if (!durable.IsSuccess
                || durable.Value is null
                || durable.Value.SchemaVersion != schemaVersion
                || durable.Value.RetainedAdapters.Count != 0
                || !CanonicalAdapterCollectionsEqual(ordered, durable.Value.ModelAdapters)
                || !finalAdapterValidation())
            {
                throw new InvalidDataException("Published registry verification failed.");
            }

            if (backupPath is not null)
            {
                File.Delete(backupPath);
                backupPath = null;
            }
            if (!finalAdapterValidation())
            {
                backupPath = Path.Combine(
                    _registryDir!,
                    $".{RegistryFileName}.{Guid.NewGuid():N}.bak");
                WriteRecoveryArtifact(backupPath, priorBytes);
                throw new InvalidDataException(
                    "Adapter changed while registry commit was finalized.");
            }
            committed = true;
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (published)
            {
                try
                {
                    // Tests can stop rollback at this exact boundary to prove
                    // that the durable recovery artifact survives until restart.
                    _beforeRollbackForTesting?.Invoke();
                    RestorePriorRegistryState(priorBytes);
                    rollbackConfirmed = true;
                }
                catch (Exception rollbackException)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(rollbackException))
                {
                    return RegistryWriteFailure();
                }
            }
            return RegistryWriteFailure();
        }
        finally
        {
            if (temporaryPath is not null && !published)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    // Publication failed; preserve the stable persistence error.
                }
            }
            if (backupPath is not null
                && (committed || rollbackConfirmed || !published))
            {
                try
                {
                    File.Delete(backupPath);
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    // A failed publication remains failed; the next boundary
                    // treats this exact backup residue as mandatory cleanup.
                }
            }
        }
    }

    private OperationResult<bool> PersistRetainedRegistryState(
        int schemaVersion,
        IEnumerable<ModelAdapter> adapters,
        Func<bool> finalAdapterValidation)
    {
        ServerOwnedContinuousLearningRegistrySnapshot? rejectedArtifact = null;
        ServerOwnedContinuousLearningRegistryPriorStateEvidence? priorEvidence = null;
        ServerOwnedContinuousLearningRegistryRollbackEvidence? rollbackEvidence = null;
        var commitAttempted = false;
        try
        {
            var ordered = adapters
                .OrderBy(value => value.Domain, StringComparer.Ordinal)
                .ToArray();
            var serialization = PreflightRegistrySerialization(
                schemaVersion,
                ordered);
            var bytes = MaterializeRegistryState(
                schemaVersion,
                ordered,
                serialization);

            var prior = _retainedRegistryRoot!.ReadCanonical();
            priorEvidence = prior.State ==
                            ServerOwnedContinuousLearningRegistryCanonicalState.Absent
                ? ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent()
                : prior.Snapshot is null
                    ? throw new InvalidDataException("Canonical registry evidence is missing.")
                    : ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(
                        prior.Snapshot);

            var written = _retainedRegistryRoot.CreateWriteTemp(bytes);
            rejectedArtifact = new ServerOwnedContinuousLearningRegistrySnapshot(
                written.Identity,
                bytes);
            var exactTemp = _retainedRegistryRoot.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                written.Identity);
            if (!exactTemp.Matches(rejectedArtifact))
                throw new InvalidDataException("The registry write temporary changed.");

            // RollbackRequired is file- and root-durable before either the
            // prior canonical object or the candidate's activation name can
            // change. Every pre-commit failure therefore has durable exact
            // prior-or-absence provenance.
            rollbackEvidence = _retainedRegistryRoot.PreparePublication(
                rejectedArtifact,
                priorEvidence);

            if (priorEvidence.State ==
                ServerOwnedContinuousLearningRegistryCanonicalState.Present)
            {
                _retainedRegistryRoot.RenameNoReplace(
                    ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                    priorEvidence.Snapshot!.Identity,
                    ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
                _retainedRegistryRoot.FlushRootDirectory();
            }

            _retainedRegistryRoot.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedArtifact.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            _retainedRegistryRoot.FlushFile(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                rejectedArtifact.Identity);
            _retainedRegistryRoot.FlushRootDirectory();

            var durableArtifact = _retainedRegistryRoot.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                rejectedArtifact.Identity);
            var durableState = ParseRegistryStateForCurrentMode(
                durableArtifact.CopyBytes());
            if (!durableArtifact.Matches(rejectedArtifact)
                || !durableState.IsSuccess
                || durableState.Value is null
                || durableState.Value.SchemaVersion != schemaVersion
                || durableState.Value.RetainedAdapters.Count != 0
                || !CanonicalAdapterCollectionsEqual(ordered, durableState.Value.ModelAdapters)
                || !finalAdapterValidation())
            {
                throw new InvalidDataException("Published registry verification failed.");
            }

            // The held adapter blocks mutation while final acceptance runs.
            // Perform the final caller-controlled validation while the exact
            // prior backup is still durable; no fallible adapter callback is
            // permitted after finalization starts removing rollback evidence.
            if (!finalAdapterValidation())
                throw new InvalidDataException("Final adapter validation failed.");

            // The atomic AcceptedStaging -> AcceptedEvidence rename followed
            // by its root-directory flush is the commit point. Cleanup is
            // deliberately deferred so it cannot turn an acknowledged commit
            // into an ordinary failed registration.
            commitAttempted = true;
            var accepted = _retainedRegistryRoot.CommitPublication(
                rejectedArtifact,
                priorEvidence,
                rollbackEvidence);
            if (!accepted.RollbackEvidence.MarkerArtifact.Matches(
                    rollbackEvidence.MarkerArtifact) ||
                !accepted.RollbackEvidence.RejectedArtifactMatches(rejectedArtifact))
            {
                throw new InvalidDataException("Accepted commit evidence changed.");
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (commitAttempted)
                return RegistryCommitOutcomeIndeterminate();
            if (rejectedArtifact is not null && priorEvidence is not null)
            {
                try
                {
                    _retainedRegistryRoot!.ReconcileRejectedPublication(
                        rejectedArtifact,
                        priorEvidence);
                }
                catch (Exception reconciliationException)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(
                        reconciliationException))
                {
                    // Exact fixed-leaf evidence is deliberately left for a
                    // fresh retained owner to reconcile or reject at restart.
                }
            }
            return RegistryWriteFailure();
        }
    }

    private static void WriteRecoveryArtifact(string path, byte[]? priorBytes)
    {
        using var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            FileOptions.WriteThrough);
        if (priorBytes is not null)
            stream.Write(priorBytes);
        stream.Flush(flushToDisk: true);
    }

    private OperationResult<T> ExecuteUnderRegistryBoundary<T>(
        Func<OperationResult<T>> action,
        string failureCode = "REGISTRY_STATE_CORRUPT")
    {
        lock (_pathGate)
        {
            try
            {
                if (_retainedRegistryRoot is not null)
                {
                    RecoverRetainedRegistryState();
                    return action();
                }

                EnsureSafeRegistryDirectory();
                using var processLease = AcquireProcessLease();
                CleanupStalePublicationArtifacts();
                return action();
            }
            catch (RetainedReferenceResolverRequiredException)
            {
                return OperationResult<T>.Fail(
                    "Registry contains a retained adapter reference that requires an AdapterRoot capability.",
                    AdapterReferenceResolverRequiredErrorCode);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<T>.Fail(
                    "Registry persistence boundary could not be established.",
                    failureCode);
            }
        }
    }

    private void RecoverRetainedRegistryState()
    {
        var inventory = _retainedRegistryRoot!.Inventory();
        var canonical = inventory.CanonicalRegistry.Snapshot;
        var writeTemp = inventory.WriteTemp.Snapshot;
        var backup = inventory.PublicationBackup.Snapshot;
        var rollback = inventory.RollbackEvidence.Snapshot;
        var acceptedStaging = inventory.AcceptedStaging.Snapshot;
        var accepted = inventory.AcceptedEvidence.Snapshot;
        if (accepted is not null)
        {
            RecoverAcceptedPublication(inventory, accepted);
            _retainedRecoveryConfirmed = true;
            return;
        }
        if (acceptedStaging is not null && rollback is null)
        {
            throw new InvalidDataException(
                "Accepted staging exists without rollback-required evidence.");
        }
        if (rollback is not null)
        {
            RecoverRollbackReconstruction(inventory, rollback);
            _retainedRecoveryConfirmed = true;
            return;
        }

        var presentCount = (canonical is null ? 0 : 1)
                           + (writeTemp is null ? 0 : 1)
                           + (backup is null ? 0 : 1);
        if (presentCount == 0)
        {
            if (!_retainedRecoveryConfirmed)
            {
                _retainedRegistryRoot.FlushRootDirectory();
                var confirmed = _retainedRegistryRoot.Inventory();
                EnsureRetainedNamespace(
                    confirmed,
                    canonicalExpected: null,
                    rollbackExpected: null);
                _retainedRecoveryConfirmed = true;
            }
            return;
        }
        if (canonical is not null && presentCount == 1)
        {
            PreflightRecoveryBranch(
            [
                new RecoveryRegistryPreflightInput(
                    canonical.CopyBytes(),
                    "Canonical registry is invalid."),
            ]);
            if (!_retainedRecoveryConfirmed)
            {
                _retainedRegistryRoot.FlushFile(
                    ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                    canonical.Identity);
                _retainedRegistryRoot.FlushRootDirectory();
                var confirmed = _retainedRegistryRoot.Inventory();
                EnsureRetainedNamespace(
                    confirmed,
                    canonical,
                    rollbackExpected: null);
                _retainedRecoveryConfirmed = true;
            }
            return;
        }
        if (presentCount == 3)
        {
            throw new InvalidDataException("The retained registry namespace is ambiguous.");
        }

        var branchPreflight = new List<RecoveryRegistryPreflightInput>(presentCount);
        if (canonical is not null)
        {
            branchPreflight.Add(new RecoveryRegistryPreflightInput(
                canonical.CopyBytes(),
                "Canonical recovery registry is invalid."));
        }
        if (writeTemp is not null)
        {
            branchPreflight.Add(new RecoveryRegistryPreflightInput(
                writeTemp.CopyBytes(),
                "Temporary recovery registry is invalid."));
        }
        if (backup is not null)
        {
            branchPreflight.Add(new RecoveryRegistryPreflightInput(
                backup.CopyBytes(),
                "Backup recovery registry is invalid."));
        }
        PreflightRecoveryBranch(branchPreflight);

        var canonicalState = canonical is null ? null : RequireValidRegistry(canonical);
        var tempState = writeTemp is null ? null : RequireValidRegistry(writeTemp);
        var backupState = backup is null ? null : RequireValidRegistry(backup);

        if (backup is not null && canonical is null && writeTemp is null)
        {
            _retainedRegistryRoot.RestoreBackupOnly(backup);
            _retainedRecoveryConfirmed = true;
            return;
        }

        ServerOwnedContinuousLearningRegistrySnapshot rejected;
        ServerOwnedContinuousLearningRegistryPriorStateEvidence prior;
        if (writeTemp is not null && canonical is null && backup is null)
        {
            if (tempState!.Entries.Count != 1)
                throw new InvalidDataException(
                    "A first-publication temporary is not a single registration.");
            rejected = writeTemp;
            prior = ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent();
        }
        else if (canonical is not null && writeTemp is not null && backup is null)
        {
            EnsureSingleRegistrationTransition(canonicalState!, tempState!);
            rejected = writeTemp;
            prior = ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(
                canonical);
        }
        else if (canonical is null && writeTemp is not null && backup is not null)
        {
            EnsureSingleRegistrationTransition(backupState!, tempState!);
            rejected = writeTemp;
            prior = ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(
                backup);
        }
        else if (canonical is not null && writeTemp is null && backup is not null)
        {
            EnsureSingleRegistrationTransition(backupState!, canonicalState!);
            rejected = canonical;
            prior = ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(
                backup);
        }
        else
        {
            throw new InvalidDataException("The retained registry namespace is ambiguous.");
        }

        _retainedRegistryRoot.ReconcileRejectedPublication(rejected, prior);
        _retainedRecoveryConfirmed = true;
    }

    private void RecoverAcceptedPublication(
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory,
        ServerOwnedContinuousLearningRegistrySnapshot acceptedMarker)
    {
        var accepted = _retainedRegistryRoot!.DecodeAcceptedEvidence(acceptedMarker);
        var transaction = accepted.RollbackEvidence;
        var canonical = inventory.CanonicalRegistry.Snapshot;
        var rollback = inventory.RollbackEvidence.Snapshot;
        var backup = inventory.PublicationBackup.Snapshot;
        if (!transaction.HasRejectedArtifact || canonical is null ||
            !transaction.RejectedArtifactMatches(canonical) ||
            inventory.WriteTemp.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedStaging.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            (rollback is not null && !rollback.Matches(transaction.MarkerArtifact)) ||
            (backup is not null && !transaction.PriorArtifactMatches(backup)))
        {
            throw new InvalidDataException(
                "Accepted transaction evidence is foreign or incomplete.");
        }
        if (transaction.PriorState ==
                ServerOwnedContinuousLearningRegistryCanonicalState.Absent &&
            backup is not null)
        {
            throw new InvalidDataException("Accepted first publication has a prior backup.");
        }

        byte[]? embeddedPriorBytes = null;
        var branchPreflight = new List<RecoveryRegistryPreflightInput>
        {
            new(canonical.CopyBytes(), "Accepted candidate registry is invalid."),
        };
        if (transaction.PriorState ==
            ServerOwnedContinuousLearningRegistryCanonicalState.Present)
        {
            embeddedPriorBytes = transaction.CopyPriorBytes();
            branchPreflight.Add(new RecoveryRegistryPreflightInput(
                embeddedPriorBytes,
                "Accepted prior registry is invalid."));
        }
        PreflightRecoveryBranch(branchPreflight);

        var candidateState = RequireValidRegistry(canonical);
        if (transaction.PriorState ==
            ServerOwnedContinuousLearningRegistryCanonicalState.Absent)
        {
            if (candidateState.Entries.Count != 1)
                throw new InvalidDataException(
                    "Accepted first publication is not one registration.");
        }
        else
        {
            var priorState = RequireParsedRegistryState(
                ParseRegistryStateForCurrentMode(embeddedPriorBytes!),
                "Accepted prior registry is invalid.");
            EnsureSingleRegistrationTransition(priorState, candidateState);
        }

        var final = _retainedRegistryRoot.ResumeAcceptedCleanup(accepted);
        EnsureRetainedNamespace(final, canonical, rollbackExpected: null);
    }

    private void RecoverRollbackReconstruction(
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory,
        ServerOwnedContinuousLearningRegistrySnapshot marker)
    {
        var evidence = _retainedRegistryRoot!.DecodeRollbackEvidence(marker);
        var canonical = inventory.CanonicalRegistry.Snapshot;
        var writeTemp = inventory.WriteTemp.Snapshot;
        var backup = inventory.PublicationBackup.Snapshot;
        var acceptedStaging = inventory.AcceptedStaging.Snapshot;
        if (inventory.AcceptedEvidence.State !=
            ServerOwnedContinuousLearningRegistryLeafState.Absent)
        {
            throw new InvalidDataException("Committed evidence cannot be rolled back.");
        }
        if (acceptedStaging is not null)
        {
            var staged = _retainedRegistryRoot.DecodeAcceptedEvidence(acceptedStaging);
            if (!staged.RollbackEvidence.MarkerArtifact.Matches(marker))
            {
                throw new InvalidDataException(
                    "Accepted staging belongs to a foreign transaction.");
            }
        }

        byte[]? embeddedPriorBytes = null;
        if (evidence.PriorState ==
            ServerOwnedContinuousLearningRegistryCanonicalState.Present)
        {
            embeddedPriorBytes = evidence.CopyPriorBytes();
        }
        else if (evidence.PriorLength != 0)
        {
            throw new InvalidDataException("Rollback absence contains prior bytes.");
        }

        if (backup is not null && !evidence.PriorArtifactMatches(backup))
            throw new InvalidDataException("Rollback publication backup is foreign.");

        var canonicalIsPrior = canonical is not null &&
                               evidence.PriorContentMatches(canonical);
        var canonicalIsRejected = canonical is not null &&
                                  evidence.RejectedArtifactMatches(canonical);
        var tempIsPrior = writeTemp is not null &&
                          evidence.PriorContentMatches(writeTemp);
        var tempIsRejected = writeTemp is not null &&
                             evidence.RejectedArtifactMatches(writeTemp);
        if ((canonical is not null && !canonicalIsPrior && !canonicalIsRejected) ||
            (writeTemp is not null && !tempIsPrior && !tempIsRejected) ||
            (canonicalIsPrior && writeTemp is not null && !tempIsRejected) ||
            (canonicalIsRejected && tempIsRejected))
        {
            throw new InvalidDataException("Rollback fixed-leaf evidence is foreign or reversed.");
        }

        var rejected = canonicalIsRejected
            ? canonical
            : tempIsRejected
                ? writeTemp
                : null;
        if (rejected is null
            && evidence.HasRejectedArtifact
            && embeddedPriorBytes is not null
            && !canonicalIsPrior
            && !tempIsPrior
            && backup is null)
        {
            throw new InvalidDataException("Rollback has no reachable prior state.");
        }

        byte[]? rejectedBytes = rejected?.CopyBytes();
        var branchPreflight = new List<RecoveryRegistryPreflightInput>(2);
        if (embeddedPriorBytes is not null)
        {
            branchPreflight.Add(new RecoveryRegistryPreflightInput(
                embeddedPriorBytes,
                "Rollback prior registry is invalid."));
        }
        if (rejectedBytes is not null)
        {
            branchPreflight.Add(new RecoveryRegistryPreflightInput(
                rejectedBytes,
                "Rollback rejected registry is invalid."));
        }
        PreflightRecoveryBranch(branchPreflight);

        ParsedRegistryState? priorState = embeddedPriorBytes is null
            ? null
            : RequireParsedRegistryState(
                ParseRegistryStateForCurrentMode(embeddedPriorBytes),
                "Rollback prior registry is invalid.");
        if (rejectedBytes is not null)
        {
            var parsedRejected = RequireParsedRegistryState(
                ParseRegistryStateForCurrentMode(rejectedBytes),
                "Rollback rejected registry is invalid.");
            if (priorState is null)
            {
                if (parsedRejected.Entries.Count != 1)
                    throw new InvalidDataException(
                        "First-publication rollback is not one registration.");
            }
            else
            {
                EnsureSingleRegistrationTransition(priorState, parsedRejected);
            }
        }

        var final = _retainedRegistryRoot.ResumeRollbackReconstruction(evidence);
        if (priorState is null)
        {
            EnsureRetainedNamespace(
                final,
                canonicalExpected: null,
                rollbackExpected: null);
            return;
        }

        if (final.CanonicalRegistry.Snapshot is null ||
            !evidence.PriorContentMatches(final.CanonicalRegistry.Snapshot))
        {
            throw new InvalidDataException("Rollback did not restore exact prior bytes.");
        }
        EnsureRetainedNamespace(
            final,
            final.CanonicalRegistry.Snapshot,
            rollbackExpected: null);
    }

    private static void EnsureRetainedNamespace(
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory,
        ServerOwnedContinuousLearningRegistrySnapshot? canonicalExpected,
        ServerOwnedContinuousLearningRegistrySnapshot? rollbackExpected)
    {
        var canonicalMatches = canonicalExpected is null
            ? inventory.CanonicalRegistry.State ==
              ServerOwnedContinuousLearningRegistryLeafState.Absent
            : inventory.CanonicalRegistry.Matches(canonicalExpected);
        var rollbackMatches = rollbackExpected is null
            ? inventory.RollbackEvidence.State ==
              ServerOwnedContinuousLearningRegistryLeafState.Absent
            : inventory.RollbackEvidence.Matches(rollbackExpected);
        if (!canonicalMatches || !rollbackMatches ||
            inventory.WriteTemp.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.PublicationBackup.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedStaging.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedEvidence.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent)
        {
            throw new InvalidDataException("The retained registry namespace is unstable.");
        }
    }

    private ParsedRegistryState RequireValidRegistry(
        ServerOwnedContinuousLearningRegistrySnapshot snapshot)
    {
        var parsed = ParseRegistryStateForCurrentMode(snapshot.CopyBytes());
        return RequireParsedRegistryState(
            parsed,
            $"{parsed.ErrorCode}: {parsed.ErrorMessage}");
    }

    private void PreflightRecoveryBranch(
        IReadOnlyList<RecoveryRegistryPreflightInput> inputs)
    {
        var containsRetainedReference = false;
        foreach (var input in inputs)
        {
            var preflight = PreflightRegistryState(input.Bytes);
            if (!preflight.IsSuccess || preflight.Value is null)
                throw new InvalidDataException(input.InvalidMessage);
            containsRetainedReference |= preflight.Value.ContainsRetainedReference;
        }

        if (containsRetainedReference && _retainedAdapterRoot is null)
            throw new RetainedReferenceResolverRequiredException();
    }

    private static ParsedRegistryState RequireParsedRegistryState(
        OperationResult<ParsedRegistryState> parsed,
        string invalidMessage)
    {
        if (parsed.IsSuccess && parsed.Value is not null)
            return parsed.Value;
        if (StringComparer.Ordinal.Equals(
                parsed.ErrorCode,
                AdapterReferenceResolverRequiredErrorCode))
        {
            throw new RetainedReferenceResolverRequiredException();
        }
        throw new InvalidDataException(invalidMessage);
    }

    private static void EnsureSingleRegistrationTransition(
        ParsedRegistryState prior,
        ParsedRegistryState candidate)
    {
        if (candidate.SchemaVersion != prior.SchemaVersion)
            throw new InvalidDataException("Registry recovery evidence changes schema version.");
        if (candidate.Entries.Count != prior.Entries.Count
            && candidate.Entries.Count != prior.Entries.Count + 1)
            throw new InvalidDataException("Registry recovery evidence is not one registration.");

        var priorByDomain = prior.Entries.ToDictionary(
            value => value.Domain,
            StringComparer.Ordinal);
        var differences = 0;
        foreach (var adapter in candidate.Entries)
        {
            if (!priorByDomain.TryGetValue(adapter.Domain, out var priorAdapter))
            {
                differences++;
                continue;
            }
            if (!CanonicalParsedAdapterEquals(priorAdapter, adapter))
                differences++;
        }
        if (differences > 1
            || prior.Entries.Any(adapter => candidate.Entries.All(candidateAdapter =>
                !StringComparer.Ordinal.Equals(candidateAdapter.Domain, adapter.Domain))))
        {
            throw new InvalidDataException("Registry recovery evidence is not one registration.");
        }
    }

    private RegistryProcessLease AcquireProcessLease()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException(
                "Registry process locks require Windows or Linux.");
        var timeout = Stopwatch.StartNew();
        while (true)
        {
            if (timeout.Elapsed >= _processLeaseTimeout)
                throw new IOException("Registry process lock acquisition timed out.");
            try
            {
                FileStream? lease = null;
                FileStream? ownerLease = null;
                try
                {
                    ownerLease = new FileStream(
                        _ownerLockPath!,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None,
                        bufferSize: 1,
                        FileOptions.WriteThrough);
                    EnsureSafeRegistryPath(_ownerLockPath!);
                    lease = new FileStream(
                        _lockPath!,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite,
                        bufferSize: 1,
                        FileOptions.WriteThrough);
                    EnsureSafeRegistryPath(_lockPath!);
                    if (lease.Length == 0)
                    {
                        lease.SetLength(RegistryLockLength);
                        lease.Flush(flushToDisk: true);
                    }
                    if (lease.Length != RegistryLockLength
                        || !OpenedHandleMatchesPath(lease, _lockPath!))
                    {
                        throw new InvalidDataException(
                            "Registry lock file identity is invalid.");
                    }
                    if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                        lease.Lock(RegistryLockOffset, RegistryLockLength);
                    else
                        throw new PlatformNotSupportedException(
                            "Registry process locks require Windows or Linux.");
                    if (!OpenedHandleMatchesPath(lease, _lockPath!)
                        || lease.Length != RegistryLockLength)
                    {
                        try
                        {
                            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                                lease.Unlock(RegistryLockOffset, RegistryLockLength);
                        }
                        finally
                        {
                            throw new InvalidDataException(
                                "Registry lock file changed during acquisition.");
                        }
                    }
                    return new RegistryProcessLease(lease, ownerLease);
                }
                catch
                {
                    lease?.Dispose();
                    ownerLease?.Dispose();
                    throw;
                }
            }
            catch (IOException) when (timeout.Elapsed < _processLeaseTimeout)
            {
                var remaining = _processLeaseTimeout - timeout.Elapsed;
                Thread.Sleep(
                    remaining < TimeSpan.FromMilliseconds(20)
                        ? remaining
                        : TimeSpan.FromMilliseconds(20));
            }
        }
    }

    private void CleanupStalePublicationArtifacts()
    {
        var backups = Directory
            .EnumerateFiles(_registryDir!, ".registry.json.*.bak")
            .Where(path => IsPublicationArtifactName(Path.GetFileName(path)))
            .ToArray();
        if (backups.Length > 1)
            throw new InvalidDataException("Multiple incomplete registry publications were found.");
        if (backups.Length == 1)
        {
            var backupPath = backups[0];
            EnsureSafeRegistryPath(backupPath);
            if (new FileInfo(backupPath).Length == 0)
            {
                File.Delete(_registryPath!);
                if (File.Exists(_registryPath!))
                    throw new IOException("Incomplete first registry publication could not be rolled back.");
                File.Delete(backupPath);
            }
            else
            {
                File.Move(backupPath, _registryPath!, overwrite: true);
            }
            if (File.Exists(backupPath))
                throw new IOException("Stale registry backup could not be recovered.");
        }

        foreach (var path in Directory.EnumerateFiles(_registryDir!, ".registry.json.*"))
        {
            var name = Path.GetFileName(path);
            if (!IsPublicationArtifactName(name)
                || name.EndsWith(".bak", StringComparison.Ordinal))
                continue;
            EnsureSafeRegistryPath(path);
            File.Delete(path);
            if (File.Exists(path))
                throw new IOException("Stale registry publication artifact could not be removed.");
        }
    }

    private static bool IsPublicationArtifactName(string name)
    {
        const string prefix = ".registry.json.";
        if (!name.StartsWith(prefix, StringComparison.Ordinal))
            return false;
        var extension = name.EndsWith(".tmp", StringComparison.Ordinal)
            ? ".tmp"
            : name.EndsWith(".bak", StringComparison.Ordinal)
                ? ".bak"
                : null;
        if (extension is null)
            return false;
        var identifier = name[prefix.Length..^extension.Length];
        return Guid.TryParseExact(identifier, "N", out _);
    }

    private void RestorePriorRegistryState(byte[]? priorBytes)
    {
        if (priorBytes is null)
        {
            File.Delete(_registryPath!);
            if (File.Exists(_registryPath!))
                throw new IOException("Registry rollback failed.");
            return;
        }

        var rollbackPath = Path.Combine(
            _registryDir!,
            $".{RegistryFileName}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(
                rollbackPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                FileOptions.WriteThrough))
            {
                stream.Write(priorBytes);
                stream.Flush(flushToDisk: true);
            }
            File.Move(rollbackPath, _registryPath!, overwrite: true);
            if (!ReadBoundedRegistryBytes(_registryPath!).AsSpan().SequenceEqual(priorBytes))
                throw new IOException("Registry rollback verification failed.");
        }
        finally
        {
            if (File.Exists(rollbackPath))
                File.Delete(rollbackPath);
        }
    }

    private static byte[] ReadBoundedRegistryBytes(string path)
    {
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            FileOptions.SequentialScan);
        if (stream.Length <= 0 || stream.Length > MaximumRegistryBytes)
            throw new InvalidDataException("Registry state has invalid size.");
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        return bytes;
    }

    private static void ValidateStrictJson(ReadOnlySpan<byte> bytes)
    {
        var reader = new Utf8JsonReader(
            bytes,
            new JsonReaderOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 64,
            });
        var objectProperties = new Stack<HashSet<string>>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                objectProperties.Push(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (objectProperties.Count == 0)
                    throw new JsonException("Unbalanced JSON object.");
                objectProperties.Pop();
            }
            else if (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (objectProperties.Count == 0
                    || !objectProperties.Peek().Add(reader.GetString()!))
                    throw new JsonException("Duplicate JSON property name.");
            }
        }
        if (objectProperties.Count != 0 || reader.BytesConsumed != bytes.Length)
            throw new JsonException("Registry JSON is incomplete or has trailing data.");
    }

    private void ReplaceMemory(IEnumerable<ModelAdapter> adapters)
    {
        _registry.Clear();
        foreach (var adapter in adapters)
            _registry.Add(adapter.Domain, adapter);
    }

    private static bool CanonicalAdapterCollectionsEqual(
        IReadOnlyList<ModelAdapter> expected,
        IReadOnlyList<ModelAdapter> actual)
    {
        if (expected.Count != actual.Count)
            return false;
        for (var index = 0; index < expected.Count; index++)
        {
            if (!CanonicalAdapterEquals(expected[index], actual[index]))
                return false;
        }
        return true;
    }

    private static bool OpenedHandleMatchesPath(FileStream stream, string expectedPath)
    {
        try
        {
            return PathIdentityEquals(
                ResolveOpenedIdentity(stream),
                Path.GetFullPath(expectedPath));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool PathIdentityEquals(string left, string right)
        => (OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal)
            .Equals(left, right);

    private static string ResolveOpenedIdentity(FileStream stream)
    {
        if (OperatingSystem.IsWindows())
        {
            var buffer = new StringBuilder(32_768);
            var length = GetFinalPathNameByHandle(
                stream.SafeFileHandle.DangerousGetHandle(),
                buffer,
                buffer.Capacity,
                0);
            if (length == 0 || length >= buffer.Capacity)
                throw new IOException("REGISTRY_FILE_IDENTITY_UNAVAILABLE");
            var path = buffer.ToString();
            if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
                path = @"\\" + path[8..];
            else if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                path = path[4..];
            return Path.GetFullPath(path);
        }

        if (OperatingSystem.IsLinux())
        {
            var descriptorPath =
                $"/proc/self/fd/{stream.SafeFileHandle.DangerousGetHandle().ToInt64()}";
            var target = File.ResolveLinkTarget(descriptorPath, returnFinalTarget: true)
                ?? throw new IOException("REGISTRY_FILE_IDENTITY_UNAVAILABLE");
            return Path.GetFullPath(target.FullName);
        }

        throw new PlatformNotSupportedException(
            "REGISTRY_FILE_IDENTITY_UNSUPPORTED");
    }

    private sealed class RegistryProcessLease(
        FileStream stream,
        FileStream ownerStream) : IDisposable
    {
        private FileStream? _stream = stream;
        private FileStream? _ownerStream = ownerStream;

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref _stream, null);
            if (owned is null)
                return;
            try
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                    owned.Unlock(RegistryLockOffset, RegistryLockLength);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Closing the handle releases the OS range lock even when an
                // explicit unlock fails.
            }
            finally
            {
                owned.Dispose();
                Interlocked.Exchange(ref _ownerStream, null)?.Dispose();
            }
        }
    }

    private sealed class HeldAdapterArtifact : IDisposable
    {
        private readonly FileStream _stream;
        private readonly string _canonicalPath;
        private readonly string _openedIdentity;
        private readonly long _length;
        private readonly string _sha256;

        private HeldAdapterArtifact(
            FileStream stream,
            string canonicalPath,
            string openedIdentity,
            long length,
            string sha256)
        {
            _stream = stream;
            _canonicalPath = canonicalPath;
            _openedIdentity = openedIdentity;
            _length = length;
            _sha256 = sha256;
        }

        public static OperationResult<HeldAdapterArtifact> Open(
            string path,
            string expectedSha256)
        {
            FileStream? stream = null;
            try
            {
                var canonicalPath = Path.GetFullPath(path);
                stream = new FileStream(
                    canonicalPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    FileOptions.SequentialScan);
                if (stream.Length <= 0 || stream.Length > MaximumAdapterBytes)
                    throw new InvalidDataException("Adapter checkpoint has invalid size.");
                var openedIdentity = ResolveOpenedIdentity(stream);
                if (!PathIdentityEquals(openedIdentity, canonicalPath))
                    throw new InvalidDataException("Adapter checkpoint identity changed while opening.");
                var length = stream.Length;
                var sha256 = TrainingSourceProvenance.Sha256(stream);
                if (!StringComparer.Ordinal.Equals(sha256, expectedSha256))
                    throw new InvalidDataException("Adapter checkpoint hash changed while opening.");
                stream.Position = 0;
                LlmModel.ValidateAdapterCheckpointArtifact(stream);
                stream.Position = 0;
                return OperationResult<HeldAdapterArtifact>.Ok(
                    new HeldAdapterArtifact(
                        stream,
                        canonicalPath,
                        openedIdentity,
                        length,
                        sha256));
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                stream?.Dispose();
                return OperationResult<HeldAdapterArtifact>.Fail(
                    "Adapter checkpoint could not be held through registry publication.",
                    "ADAPTER_CHECKPOINT_INVALID");
            }
        }

        public bool VerifyUnchangedAndCurrentPath()
        {
            try
            {
                if (_stream.Length != _length
                    || !PathIdentityEquals(
                        ResolveOpenedIdentity(_stream),
                        _openedIdentity)
                    || !StringComparer.Ordinal.Equals(
                        TrainingSourceProvenance.Sha256(_stream),
                        _sha256))
                {
                    return false;
                }

                using var current = new FileStream(
                    _canonicalPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    FileOptions.SequentialScan);
                return current.Length == _length
                       && PathIdentityEquals(
                           ResolveOpenedIdentity(current),
                           _openedIdentity)
                       && StringComparer.Ordinal.Equals(
                           TrainingSourceProvenance.Sha256(current),
                           _sha256);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return false;
            }
        }

        public void Dispose() => _stream.Dispose();
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandle(
        IntPtr fileHandle,
        StringBuilder filePath,
        int filePathLength,
        uint flags);

    private void EnsureSafeRegistryDirectory()
    {
        if (!Directory.Exists(_registryDir!))
            Directory.CreateDirectory(_registryDir!);
        EnsureNoReparseDirectoryChain(_registryDir!);
    }

    private static void EnsureSafeRegistryPath(string path)
    {
        if (!Path.IsPathFullyQualified(path)
            || !StringComparer.OrdinalIgnoreCase.Equals(path, Path.GetFullPath(path)))
            throw new InvalidDataException("Registry path is non-canonical.");
        EnsureNoReparseDirectoryChain(Path.GetDirectoryName(path)!);
        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            throw new InvalidDataException("Registry path is not a regular file.");
    }

    private static void EnsureNoReparseDirectoryChain(string directoryPath)
    {
        for (var directory = new DirectoryInfo(directoryPath);
             directory is not null;
             directory = directory.Parent)
        {
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Registry path contains a reparse point.");
        }
    }

    private static OperationResult<ParsedRegistryState> EmptyRegistryState()
        => OperationResult<ParsedRegistryState>.Ok(
            new ParsedRegistryState(
                RegistrySchemaVersion,
                Array.AsReadOnly(Array.Empty<ParsedRegistryAdapter>())));

    private static OperationResult<ParsedRegistryState> RegistryStateFailure(
        string message = "Registry state is corrupt or incomplete.")
        => OperationResult<ParsedRegistryState>.Fail(
            message,
            "REGISTRY_STATE_CORRUPT");

    private static OperationResult<ParsedRegistryState>
        RetainedReferenceResolverRequiredState()
        => OperationResult<ParsedRegistryState>.Fail(
            "Registry contains a retained adapter reference that requires an AdapterRoot capability.",
            AdapterReferenceResolverRequiredErrorCode);

    private static OperationResult<bool> RegistryWriteFailure()
        => OperationResult<bool>.Fail(
            "Registry state could not be published atomically.",
            "REGISTRY_PERSISTENCE_FAILED");

    private static OperationResult<bool> RegistryCommitOutcomeIndeterminate()
        => OperationResult<bool>.Fail(
            "Registry commit acknowledgement is indeterminate; restart recovery decides the exact outcome.",
            "REGISTRY_COMMIT_OUTCOME_INDETERMINATE");

    private static bool CanonicalAdapterEquals(ModelAdapter stored, ModelAdapter canonical)
        => JsonSerializer.SerializeToUtf8Bytes(stored, JsonOptions)
            .AsSpan()
            .SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(canonical, JsonOptions));

    private static bool CanonicalParsedAdapterEquals(
        ParsedRegistryAdapter left,
        ParsedRegistryAdapter right)
        => (left, right) switch
        {
            (ParsedLegacyRegistryAdapter leftLegacy,
                ParsedLegacyRegistryAdapter rightLegacy) =>
                CanonicalAdapterEquals(leftLegacy.Adapter, rightLegacy.Adapter),
            (ParsedRetainedRegistryAdapter leftRetained,
                ParsedRetainedRegistryAdapter rightRetained) =>
                CanonicalRetainedAdapterEquals(leftRetained, rightRetained),
            _ => false,
        };

    private static bool CanonicalRetainedAdapterEquals(
        ParsedRetainedRegistryAdapter left,
        ParsedRetainedRegistryAdapter right)
        => StringComparer.Ordinal.Equals(left.Domain, right.Domain)
           && left.TrainingExamples == right.TrainingExamples
           && left.TrainedAt.Equals(right.TrainedAt)
           && CanonicalProvenanceEquals(left.Provenance, right.Provenance)
           && left.Reference == right.Reference;

    private sealed record RegistryEnvelope(
        int SchemaVersion,
        IReadOnlyList<ModelAdapter> Adapters);

    private sealed record RegistrySerializationPlan(
        long Length,
        string Sha256);

    private sealed class RegistrySerializationCapacityException : Exception;

    private sealed class BoundedRegistryWriteStream(
        Stream? destination,
        IncrementalHash hash,
        long maximumBytes) : IBufferWriter<byte>, IDisposable
    {
        private const int MaximumPooledBufferBytes = 4 * 1024 * 1024 + 1024;
        private const int MaximumJsonEscapeExpansionFactor = 6;
        private const int JsonWriterGrowthSlackBytes = 1024;
        private byte[] _buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
        private bool _bufferIsPooled = true;
        internal long BytesWritten { get; private set; }

        public void Advance(int count)
        {
            if (count < 0
                || count > _buffer.Length
                || count > maximumBytes - BytesWritten)
            {
                throw new RegistrySerializationCapacityException();
            }
            hash.AppendData(_buffer.AsSpan(0, count));
            destination?.Write(_buffer.AsSpan(0, count));
            BytesWritten += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            var required = Math.Max(sizeHint, 1);
            var remaining = maximumBytes - BytesWritten;
            var maximumBufferBytes = checked(
                (int)Math.Min(
                    int.MaxValue,
                    checked(
                        maximumBytes * MaximumJsonEscapeExpansionFactor +
                        JsonWriterGrowthSlackBytes)));
            if (remaining <= 0 || required > maximumBufferBytes)
                throw new RegistrySerializationCapacityException();
            if (_buffer.Length < required)
            {
                var replacement = required <= MaximumPooledBufferBytes
                    ? ArrayPool<byte>.Shared.Rent(required)
                    : GC.AllocateUninitializedArray<byte>(required);
                var replaced = _buffer;
                var replacedWasPooled = _bufferIsPooled;
                _buffer = replacement;
                _bufferIsPooled = required <= MaximumPooledBufferBytes;
                if (replacedWasPooled)
                    ArrayPool<byte>.Shared.Return(replaced);
            }

            // IBufferWriter must honor the writer's size hint even when its
            // conservative escape reservation exceeds the remaining output.
            // Advance is the authority that rejects actual bytes past the
            // public registry maximum.
            return _buffer.AsMemory();
        }

        public Span<byte> GetSpan(int sizeHint = 0)
            => GetMemory(sizeHint).Span;

        public void Dispose()
        {
            var bufferWasPooled = _bufferIsPooled;
            _bufferIsPooled = false;
            var buffer = Interlocked.Exchange(ref _buffer, []);
            if (bufferWasPooled && buffer.Length != 0)
                ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private sealed record RecoveryRegistryPreflightInput(
        byte[] Bytes,
        string InvalidMessage);

    private sealed record RegistryStructuralPreflight(
        int SchemaVersion,
        IReadOnlyList<ModelAdapter> Version1Adapters,
        IReadOnlyList<RegistryVersion2StoredAdapter> Version2Adapters,
        IReadOnlyDictionary<string, ParsedRetainedRegistryAdapter> RetainedByDomain)
    {
        internal bool ContainsRetainedReference => RetainedByDomain.Count != 0;
    }

    private sealed class RetainedReferenceResolverRequiredException : Exception
    {
    }

    private abstract record ParsedRegistryAdapter(string Domain);

    private sealed record ParsedLegacyRegistryAdapter(ModelAdapter Adapter)
        : ParsedRegistryAdapter(Adapter.Domain);

    private sealed record ParsedRetainedRegistryAdapter(
        string Domain,
        int TrainingExamples,
        DateTimeOffset TrainedAt,
        ModelAdapterProvenance Provenance,
        AdapterArtifactReference Reference)
        : ParsedRegistryAdapter(Domain);

    private sealed record ParsedRegistryState(
        int SchemaVersion,
        IReadOnlyList<ParsedRegistryAdapter> Entries)
    {
        internal IReadOnlyList<ModelAdapter> ModelAdapters { get; } =
            Array.AsReadOnly(Entries
                .OfType<ParsedLegacyRegistryAdapter>()
                .Select(static value => value.Adapter)
                .ToArray());

        internal IReadOnlyList<ParsedRetainedRegistryAdapter> RetainedAdapters { get; } =
            Array.AsReadOnly(Entries
                .OfType<ParsedRetainedRegistryAdapter>()
                .ToArray());

        internal ModelAdapter? FindLegacy(string domain)
            => ModelAdapters.FirstOrDefault(value =>
                StringComparer.Ordinal.Equals(value.Domain, domain));

        internal ParsedRetainedRegistryAdapter? FindRetained(string domain)
            => RetainedAdapters.FirstOrDefault(value =>
                StringComparer.Ordinal.Equals(value.Domain, domain));
    }

    private sealed record RegistryVersion2Envelope(
        int SchemaVersion,
        IReadOnlyList<RegistryVersion2LegacyAdapterEnvelope> Adapters);

    private sealed record RegistryVersion2LegacyAdapterEnvelope(
        string Domain,
        int TrainingExamples,
        DateTimeOffset TrainedAt,
        ModelAdapterProvenance? Provenance,
        int StorageKind,
        string LegacyAbsolutePath);

    private sealed record RegistryVersion2StoredAdapter(
        string Domain,
        int TrainingExamples,
        DateTimeOffset TrainedAt,
        ModelAdapterProvenance? Provenance,
        int StorageKind,
        string? LegacyAbsolutePath,
        AdapterArtifactReference? RetainedReference);

    private static OperationResult<ModelAdapter> ValidateAdapter(
        ModelAdapter? adapter,
        string? expectedDomain = null)
    {
        if (adapter is null)
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter cannot be null.",
                "NULL_ADAPTER");
        }

        if (string.IsNullOrWhiteSpace(adapter.Domain)
            || !StringComparer.Ordinal.Equals(adapter.Domain, adapter.Domain.Trim()))
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter domain cannot be empty.",
                "INVALID_ADAPTER_DOMAIN");
        }

        if (expectedDomain is not null
            && !StringComparer.Ordinal.Equals(adapter.Domain, expectedDomain))
        {
            return OperationResult<ModelAdapter>.Fail(
                "Registered adapter domain does not match the requested domain.",
                "ADAPTER_DOMAIN_MISMATCH");
        }

        if (string.IsNullOrWhiteSpace(adapter.AdapterPath)
            || !Path.IsPathFullyQualified(adapter.AdapterPath))
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter path must be fully qualified.",
                "ADAPTER_PATH_NOT_ABSOLUTE");
        }

        if (adapter.TrainingExamples <= 0
            || adapter.TrainedAt == default)
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter training metadata is invalid.",
                "ADAPTER_TRAINING_METADATA_INVALID");
        }

        if (adapter.Provenance is null)
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter provenance is required for registry publication.",
                "ADAPTER_PROVENANCE_REQUIRED");
        }

        if (adapter.Provenance.TrainingSources is null
            || adapter.Provenance.Model is null
            || adapter.Provenance.Lora is null
            || adapter.Provenance.Lora.OrderedTargetModules is null)
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter provenance is invalid.",
                "ADAPTER_PROVENANCE_MISMATCH");
        }

        var manifest = TrainingSourceProvenance.BuildManifest(
            adapter.Provenance.TrainingSources.SourceIds);
        if (!manifest.IsSuccess || manifest.Value is null
            || !StringComparer.Ordinal.Equals(
                manifest.Value.Sha256,
                adapter.Provenance.TrainingSources.Sha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(adapter.Provenance.AdapterSha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(adapter.Provenance.BaseCheckpointSha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(adapter.Provenance.TokenizerSha256))
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter provenance is invalid.",
                "ADAPTER_PROVENANCE_MISMATCH");
        }
        if (adapter.Provenance.DpoPublication is not null
            && !TrainingSourceProvenance.IsCanonicalDpoPublicationReceipt(
                adapter.Provenance.DpoPublication))
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter DPO publication receipt is invalid.",
                "ADAPTER_PROVENANCE_MISMATCH");
        }
        var canonicalDpoReceipt = CanonicalizeDpoPublicationReceipt(
            adapter.Provenance.DpoPublication);
        if (!canonicalDpoReceipt.IsSuccess)
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter DPO publication receipt is invalid.",
                "ADAPTER_PROVENANCE_MISMATCH");
        }

        ModelConfigurationIdentity expectedModelIdentity;
        LoraConfigurationIdentity expectedLoraIdentity;
        try
        {
            expectedModelIdentity = TrainingSourceProvenance.Identify(new TransformerModel.Config.ModelConfig
            {
                VocabSize = adapter.Provenance.Model.VocabSize,
                ContextLen = adapter.Provenance.Model.ContextLen,
                HiddenDim = adapter.Provenance.Model.HiddenDim,
                NumLayers = adapter.Provenance.Model.NumLayers,
                NumHeads = adapter.Provenance.Model.NumHeads,
                NumKvHeads = adapter.Provenance.Model.NumKvHeads,
                FfnHiddenDim = adapter.Provenance.Model.FfnHiddenDim,
                RopeTheta = adapter.Provenance.Model.RopeTheta,
                RmsNormEps = adapter.Provenance.Model.RmsNormEps,
            });
            expectedLoraIdentity = TrainingSourceProvenance.Identify(new LoRA.LoraConfig
            {
                Rank = adapter.Provenance.Lora.Rank,
                Alpha = adapter.Provenance.Lora.Alpha,
                Dropout = adapter.Provenance.Lora.Dropout,
                TargetModules = adapter.Provenance.Lora.OrderedTargetModules.ToArray(),
            });
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter configuration provenance is invalid.",
                "ADAPTER_PROVENANCE_MISMATCH");
        }
        if (expectedModelIdentity != adapter.Provenance.Model
            || expectedLoraIdentity.Rank != adapter.Provenance.Lora.Rank
            || !expectedLoraIdentity.Alpha.Equals(adapter.Provenance.Lora.Alpha)
            || !expectedLoraIdentity.Scaling.Equals(adapter.Provenance.Lora.Scaling)
            || !expectedLoraIdentity.Dropout.Equals(adapter.Provenance.Lora.Dropout)
            || !StringComparer.Ordinal.Equals(
                expectedLoraIdentity.Sha256,
                adapter.Provenance.Lora.Sha256)
            || !expectedLoraIdentity.OrderedTargetModules.SequenceEqual(
                adapter.Provenance.Lora.OrderedTargetModules,
                StringComparer.Ordinal))
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter configuration provenance is invalid.",
                "ADAPTER_PROVENANCE_MISMATCH");
        }

        string normalizedPath;
        try
        {
            normalizedPath = Path.GetFullPath(adapter.AdapterPath);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter path must be a valid fully-qualified path.",
                "ADAPTER_PATH_NOT_ABSOLUTE");
        }

        try
        {
            var attributes = File.GetAttributes(normalizedPath);
            var fileInfo = new FileInfo(normalizedPath);
            if ((attributes
                    & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            {
                return OperationResult<ModelAdapter>.Fail(
                    "Adapter checkpoint is not a valid regular file.",
                    "ADAPTER_CHECKPOINT_INVALID");
            }

            if (fileInfo.Length <= 0)
            {
                return OperationResult<ModelAdapter>.Fail(
                    "Adapter checkpoint is empty.",
                    "ADAPTER_CHECKPOINT_INVALID");
            }

            for (var directory = new FileInfo(normalizedPath).Directory;
                 directory is not null;
                 directory = directory.Parent)
            {
                if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    return OperationResult<ModelAdapter>.Fail(
                        "Adapter path contains a reparse point.",
                        "ADAPTER_CHECKPOINT_INVALID");
                }
            }

            using var stream = new FileStream(
                normalizedPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                FileOptions.SequentialScan);
            if (stream.Length > 2L * 1024 * 1024 * 1024)
            {
                return OperationResult<ModelAdapter>.Fail(
                    "Adapter checkpoint exceeds the registry size limit.",
                    "ADAPTER_CHECKPOINT_INVALID");
            }
            var adapterSha256 = TrainingSourceProvenance.Sha256(stream);
            if (!StringComparer.Ordinal.Equals(
                    adapterSha256,
                    adapter.Provenance.AdapterSha256))
            {
                return OperationResult<ModelAdapter>.Fail(
                    "Adapter bytes do not match registered provenance.",
                    "ADAPTER_PROVENANCE_MISMATCH");
            }
            stream.Position = 0;
            LlmModel.ValidateAdapterCheckpointArtifact(stream);
        }
        catch (Exception exception)
            when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter checkpoint was not found.",
                "ADAPTER_CHECKPOINT_NOT_FOUND");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<ModelAdapter>.Fail(
                "Adapter checkpoint is invalid.",
                "ADAPTER_CHECKPOINT_INVALID");
        }

        return OperationResult<ModelAdapter>.Ok(
            adapter with
            {
                AdapterPath = normalizedPath,
                TrainedAt = adapter.TrainedAt.ToUniversalTime(),
                Provenance = new ModelAdapterProvenance(
                    manifest.Value!,
                    adapter.Provenance.AdapterSha256,
                    adapter.Provenance.BaseCheckpointSha256,
                    adapter.Provenance.TokenizerSha256,
                    expectedModelIdentity,
                    expectedLoraIdentity,
                    canonicalDpoReceipt.Value),
            });
    }

    private static OperationResult<DpoAdapterPublicationReceipt?>
        CanonicalizeDpoPublicationReceipt(
            DpoAdapterPublicationReceipt? receipt)
    {
        if (receipt is null)
            return OperationResult<DpoAdapterPublicationReceipt?>.Ok(null);
        if (receipt.OrderedPairIds is null || receipt.OrderedDomains is null)
        {
            return OperationResult<DpoAdapterPublicationReceipt?>.Fail(
                "Adapter DPO publication receipt is invalid.",
                "ADAPTER_PROVENANCE_MISMATCH");
        }
        var rebuilt = TrainingSourceProvenance.BuildDpoPublicationReceipt(
            receipt.OrderedPairIds,
            receipt.OrderedDomains);
        if (!rebuilt.IsSuccess || rebuilt.Value is null
            || !StringComparer.Ordinal.Equals(
                rebuilt.Value.TransactionId,
                receipt.TransactionId)
            || !StringComparer.Ordinal.Equals(
                rebuilt.Value.PairBatchSha256,
                receipt.PairBatchSha256))
        {
            return OperationResult<DpoAdapterPublicationReceipt?>.Fail(
                "Adapter DPO publication receipt is invalid.",
                "ADAPTER_PROVENANCE_MISMATCH");
        }
        return OperationResult<DpoAdapterPublicationReceipt?>.Ok(rebuilt.Value);
    }
}
