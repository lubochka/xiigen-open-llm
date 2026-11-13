// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using DPO;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

public sealed class LogicalCapabilityTrainingSourceBindingComposite :
    IEquatable<LogicalCapabilityTrainingSourceBindingComposite>
{
    public const int MaximumBindingCount = 256;

    private readonly ReadOnlyCollection<string> _values;
    private readonly bool _isCanonical;

    [JsonConstructor]
    public LogicalCapabilityTrainingSourceBindingComposite(
        IReadOnlyList<string>? values)
    {
        try
        {
            var snapshot = values?
                .Take(MaximumBindingCount + 1)
                .ToArray() ?? [];
            _isCanonical = snapshot.Length is > 0 and <= MaximumBindingCount &&
                snapshot.All(LogicalCapabilityTrainingReplayDescriptor
                    .IsCanonicalSha256) &&
                IsStrictlyIncreasing(snapshot);
            _values = Array.AsReadOnly(_isCanonical ? snapshot : []);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            _values = Array.AsReadOnly(Array.Empty<string>());
            _isCanonical = false;
        }
    }

    public IReadOnlyList<string> Values => _values;

    public bool IsCanonical => _isCanonical;

    public LogicalCapabilityTrainingSourceBindingComposite Snapshot()
        => new(_values);

    public bool Equals(
        LogicalCapabilityTrainingSourceBindingComposite? other)
        => other is not null && IsCanonical && other.IsCanonical &&
           _values.SequenceEqual(other._values, StringComparer.Ordinal);

    public override bool Equals(object? obj)
        => obj is LogicalCapabilityTrainingSourceBindingComposite other &&
           Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_values.Count);
        foreach (var value in _values)
            hash.Add(value, StringComparer.Ordinal);
        return hash.ToHashCode();
    }

    private static bool IsStrictlyIncreasing(IReadOnlyList<string> values)
    {
        for (var index = 1; index < values.Count; index++)
        {
            if (StringComparer.Ordinal.Compare(
                    values[index - 1],
                    values[index]) >= 0)
            {
                return false;
            }
        }
        return true;
    }
}

public sealed record LogicalCapabilityTrainingReplayDescriptor(
    int SchemaVersion,
    LogicalTrainingContentSourceKind OriginalSourceKind,
    string ReplaySealGroupSha256,
    LogicalCapabilityTrainingSourceBindingComposite SourceBindings,
    string ReplayPayloadSha256,
    string DpoPreferenceSha256)
{
    public const int CurrentSchemaVersion = 3;

    public bool IsCanonical =>
        SchemaVersion == CurrentSchemaVersion &&
        OriginalSourceKind == LogicalTrainingContentSourceKind
            .DeterministicSyntheticNoExternalTeacher &&
        IsCanonicalSha256(ReplaySealGroupSha256) &&
        SourceBindings is not null && SourceBindings.IsCanonical &&
        IsCanonicalSha256(ReplayPayloadSha256) &&
        IsCanonicalSha256(DpoPreferenceSha256);

    internal static bool IsCanonicalSha256(string? value)
        => value is not null && value.Length == 64 &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');
}

public sealed record LogicalCapabilityTrainingReplayAttestation(
    int SchemaVersion,
    LogicalTrainingContentSourceKind OriginalSourceKind,
    string ReplaySealGroupSha256,
    LogicalCapabilityTrainingSourceBindingComposite SourceBindings,
    string ReplayPayloadSha256,
    string DpoPreferenceSha256,
    string AuthorityKeyRef,
    string MacSha256)
{
    public bool IsCanonical =>
        Descriptor.IsCanonical &&
        IsCanonicalEvidenceRef(AuthorityKeyRef) &&
        LogicalCapabilityTrainingReplayDescriptor
            .IsCanonicalSha256(MacSha256);

    public LogicalCapabilityTrainingReplayDescriptor Descriptor => new(
        SchemaVersion,
        OriginalSourceKind,
        ReplaySealGroupSha256,
        SourceBindings?.Snapshot() ??
            new LogicalCapabilityTrainingSourceBindingComposite(null),
        ReplayPayloadSha256,
        DpoPreferenceSha256);

    public bool Matches(LogicalCapabilityTrainingReplayDescriptor descriptor)
        => descriptor is not null && descriptor.IsCanonical &&
           Descriptor == descriptor;

    private static bool IsCanonicalEvidenceRef(string? value)
        => value is not null && value.Length == 71 &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
               value[7..]);
}

public sealed record LogicalCapabilityTrainingAdmissionPreference(
    string Prompt,
    string Chosen,
    string Rejected,
    IReadOnlyList<float>? TeachingVector,
    LogicalTrainingContentProvenance? TrainingContentProvenance,
    LogicalCapabilityTrainingReplayDescriptor? ReplayDescriptor,
    LogicalCapabilityTrainingReplayAttestation? ReplayAttestation);

/// <summary>
/// Server-sealed commitment joining an executable manifest to the exact
/// replay attestation that authorized its persisted training state.
/// </summary>
public sealed record LogicalCapabilityExecutableManifestAttestation(
    int SchemaVersion,
    string ExecutableManifestSha256,
    string ReplayAuthorityKeyRef,
    string ReplayMacSha256,
    string AuthorityKeyRef,
    string MacSha256)
{
    public const int CurrentSchemaVersion = 1;

    public bool IsCanonical =>
        SchemaVersion == CurrentSchemaVersion &&
        LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
            ExecutableManifestSha256) &&
        IsCanonicalEvidenceRef(ReplayAuthorityKeyRef) &&
        LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
            ReplayMacSha256) &&
        IsCanonicalEvidenceRef(AuthorityKeyRef) &&
        LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
            MacSha256);

    public bool Matches(
        string executableManifestSha256,
        LogicalCapabilityTrainingReplayAttestation replayAttestation)
        => IsCanonical &&
           replayAttestation is { IsCanonical: true } &&
           LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
               executableManifestSha256) &&
           StringComparer.Ordinal.Equals(
               ExecutableManifestSha256,
               executableManifestSha256) &&
           StringComparer.Ordinal.Equals(
               ReplayAuthorityKeyRef,
               replayAttestation.AuthorityKeyRef) &&
           StringComparer.Ordinal.Equals(
               ReplayMacSha256,
               replayAttestation.MacSha256);

    private static bool IsCanonicalEvidenceRef(string? value)
        => value is not null && value.Length == 71 &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
               value[7..]);
}

internal sealed record LogicalCapabilityTrainingReplayAuthorization(
    string ReplaySealGroupSha256,
    LogicalCapabilityTrainingSourceBindingComposite SourceBindings,
    string ReplayPayloadSha256,
    string DpoPreferenceSha256)
{
    internal bool IsCanonical =>
        LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
            ReplaySealGroupSha256) &&
        SourceBindings is { IsCanonical: true } &&
        LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
            ReplayPayloadSha256) &&
        LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
            DpoPreferenceSha256);

    internal bool Matches(LogicalCapabilityTrainingReplayDescriptor descriptor)
        => IsCanonical && descriptor is { IsCanonical: true } &&
           StringComparer.Ordinal.Equals(
               ReplaySealGroupSha256,
               descriptor.ReplaySealGroupSha256) &&
           SourceBindings.Equals(descriptor.SourceBindings) &&
           StringComparer.Ordinal.Equals(
               ReplayPayloadSha256,
               descriptor.ReplayPayloadSha256) &&
           StringComparer.Ordinal.Equals(
               DpoPreferenceSha256,
               descriptor.DpoPreferenceSha256);
}

public enum LogicalCapabilityTrainingAdmissionRowKind
{
    Untrusted,
    Current,
    Replay
}

/// <summary>
/// Canonical deterministic identity for one final logical capability-training row.
/// The value is always a lower-case SHA-256 digest and carries no teacher-policy data.
/// </summary>
public readonly record struct LogicalCapabilityTrainingRowIdentity
{
    public const int Sha256HexLength = 64;

    private readonly LogicalTrainingContentSourceKind _trainingContentSourceKind;
    private readonly LogicalCapabilityTrainingSourceBindingComposite?
        _sourceBindings;
    private readonly LogicalCapabilityTrainingAdmissionRowKind _rowKind;
    private readonly LogicalCapabilityTrainingReplayDescriptor? _replayDescriptor;
    private readonly LogicalCapabilityTrainingReplayAttestation? _replayAttestation;
    private readonly string? _replaySealGroupSha256;
    private readonly string? _currentDpoPreferenceSha256;
    private readonly LogicalCapabilityTrainingSourceBindingComposite?
        _authorizedReplaySourceBindings;
    private readonly string? _authorizedReplayPayloadSha256;
    private readonly string? _authorizedReplayDpoPreferenceSha256;

    private LogicalCapabilityTrainingRowIdentity(
        string sha256,
        LogicalCapabilityTrainingAdmissionRowKind rowKind,
        LogicalTrainingContentSourceKind trainingContentSourceKind,
        LogicalCapabilityTrainingSourceBindingComposite? sourceBindings,
        LogicalCapabilityTrainingReplayDescriptor? replayDescriptor,
        LogicalCapabilityTrainingReplayAttestation? replayAttestation,
        string? replaySealGroupSha256,
        string? currentDpoPreferenceSha256,
        LogicalCapabilityTrainingSourceBindingComposite?
            authorizedReplaySourceBindings,
        string? authorizedReplayPayloadSha256,
        string? authorizedReplayDpoPreferenceSha256)
    {
        Sha256 = sha256;
        _rowKind = rowKind;
        _trainingContentSourceKind = trainingContentSourceKind;
        _sourceBindings = sourceBindings?.Snapshot();
        _replayDescriptor = replayDescriptor is null
            ? null
            : replayDescriptor with { };
        _replayAttestation = replayAttestation is null
            ? null
            : replayAttestation with { };
        _replaySealGroupSha256 = replaySealGroupSha256;
        _currentDpoPreferenceSha256 = currentDpoPreferenceSha256;
        _authorizedReplaySourceBindings =
            authorizedReplaySourceBindings?.Snapshot();
        _authorizedReplayPayloadSha256 = authorizedReplayPayloadSha256;
        _authorizedReplayDpoPreferenceSha256 =
            authorizedReplayDpoPreferenceSha256;
    }

    public string Sha256 { get; }

    public LogicalTrainingContentSourceKind TrainingContentSourceKind
        => _trainingContentSourceKind;

    public LogicalCapabilityTrainingSourceBindingComposite? SourceBindings
        => _sourceBindings?.Snapshot();

    public LogicalCapabilityTrainingAdmissionRowKind RowKind => _rowKind;

    public LogicalCapabilityTrainingReplayDescriptor? ReplayDescriptor
        => _replayDescriptor is null ? null : _replayDescriptor with { };

    public LogicalCapabilityTrainingReplayAttestation? ReplayAttestation
        => _replayAttestation is null ? null : _replayAttestation with { };

    internal string? ReplaySealGroupSha256 => _replaySealGroupSha256;

    internal string? CurrentDpoPreferenceSha256 =>
        _currentDpoPreferenceSha256;

    internal LogicalCapabilityTrainingReplayAuthorization?
        AuthorizedReplay =>
        _authorizedReplaySourceBindings is null ||
        _replaySealGroupSha256 is null ||
        _authorizedReplayPayloadSha256 is null ||
        _authorizedReplayDpoPreferenceSha256 is null
            ? null
            : new LogicalCapabilityTrainingReplayAuthorization(
                _replaySealGroupSha256,
                _authorizedReplaySourceBindings.Snapshot(),
                _authorizedReplayPayloadSha256,
                _authorizedReplayDpoPreferenceSha256);

    public static OperationResult<LogicalCapabilityTrainingRowIdentity> Create(
        string? sha256)
        => IsCanonicalSha256(sha256)
            ? OperationResult<LogicalCapabilityTrainingRowIdentity>.Ok(
                new LogicalCapabilityTrainingRowIdentity(
                    sha256!,
                    LogicalCapabilityTrainingAdmissionRowKind.Untrusted,
                    LogicalTrainingContentSourceKind.Unattested,
                    sourceBindings: null,
                    replayDescriptor: null,
                    replayAttestation: null,
                    replaySealGroupSha256: null,
                    currentDpoPreferenceSha256: null,
                    authorizedReplaySourceBindings: null,
                    authorizedReplayPayloadSha256: null,
                    authorizedReplayDpoPreferenceSha256: null))
            : OperationResult<LogicalCapabilityTrainingRowIdentity>.Fail(
                "Logical capability training row identity is invalid.",
                "LOGICAL_CAPABILITY_TRAINING_ROW_IDENTITY_INVALID");

    internal static OperationResult<LogicalCapabilityTrainingRowIdentity>
        CreateTrustedCurrent(
        string? sha256,
        string? dpoPreferenceSha256,
        LogicalTrainingContentProvenance? trainingContentProvenance)
        => IsCanonicalSha256(sha256) &&
           IsCanonicalSha256(dpoPreferenceSha256) &&
           trainingContentProvenance is not null &&
           trainingContentProvenance.IsCanonical &&
           trainingContentProvenance.AuthorizesCurrentPreference(
               dpoPreferenceSha256)
            ? OperationResult<LogicalCapabilityTrainingRowIdentity>.Ok(
                new LogicalCapabilityTrainingRowIdentity(
                    sha256!,
                    LogicalCapabilityTrainingAdmissionRowKind.Current,
                    trainingContentProvenance.SourceKind,
                    new LogicalCapabilityTrainingSourceBindingComposite(
                        [trainingContentProvenance.BindingSha256]),
                    replayDescriptor: null,
                     replayAttestation: null,
                     replaySealGroupSha256:
                        trainingContentProvenance.ReplaySealGroupSha256,
                    currentDpoPreferenceSha256: dpoPreferenceSha256,
                    authorizedReplaySourceBindings:
                        new LogicalCapabilityTrainingSourceBindingComposite(
                            trainingContentProvenance
                                .AuthorizedReplaySourceBindingSha256s),
                    authorizedReplayPayloadSha256:
                        trainingContentProvenance
                            .AuthorizedReplayPayloadSha256,
                    authorizedReplayDpoPreferenceSha256:
                        trainingContentProvenance
                            .AuthorizedReplayDpoPreferenceSha256))
            : Invalid();

    internal static OperationResult<LogicalCapabilityTrainingRowIdentity>
        CreateTrustedReplay(
            string? sha256,
            LogicalCapabilityTrainingReplayDescriptor? descriptor,
            LogicalCapabilityTrainingReplayAttestation? attestation)
        => IsCanonicalSha256(sha256) &&
           descriptor is not null && descriptor.IsCanonical &&
           attestation is not null && attestation.IsCanonical &&
           attestation.Matches(descriptor)
            ? OperationResult<LogicalCapabilityTrainingRowIdentity>.Ok(
                new LogicalCapabilityTrainingRowIdentity(
                    sha256!,
                    LogicalCapabilityTrainingAdmissionRowKind.Replay,
                    descriptor.OriginalSourceKind,
                    descriptor.SourceBindings,
                    descriptor,
                    attestation,
                    replaySealGroupSha256:
                        descriptor.ReplaySealGroupSha256,
                    currentDpoPreferenceSha256: null,
                    authorizedReplaySourceBindings: null,
                    authorizedReplayPayloadSha256: null,
                    authorizedReplayDpoPreferenceSha256: null))
            : Invalid();

    public bool IsCanonical => IsCanonicalSha256(Sha256);

    public bool HasCanonicalTrainingContentProvenance =>
        _rowKind switch
        {
            LogicalCapabilityTrainingAdmissionRowKind.Current =>
                _trainingContentSourceKind == LogicalTrainingContentSourceKind
                    .DeterministicSyntheticNoExternalTeacher &&
                _sourceBindings is not null &&
                _sourceBindings.IsCanonical &&
                LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
                    _replaySealGroupSha256) &&
                IsCanonicalSha256(_currentDpoPreferenceSha256) &&
                _authorizedReplaySourceBindings is { IsCanonical: true } &&
                _authorizedReplaySourceBindings.Values.Contains(
                    _sourceBindings.Values[0],
                    StringComparer.Ordinal) &&
                IsCanonicalSha256(_authorizedReplayPayloadSha256) &&
                IsCanonicalSha256(_authorizedReplayDpoPreferenceSha256) &&
                _replayDescriptor is null &&
                _replayAttestation is null,
            LogicalCapabilityTrainingAdmissionRowKind.Replay =>
                _trainingContentSourceKind == LogicalTrainingContentSourceKind
                    .DeterministicSyntheticNoExternalTeacher &&
                _replayDescriptor is not null &&
                _replayAttestation is not null &&
                _replayDescriptor.IsCanonical &&
                _replayAttestation.IsCanonical &&
                _replayAttestation.Matches(_replayDescriptor) &&
                _sourceBindings is not null &&
                _sourceBindings.Equals(_replayDescriptor.SourceBindings) &&
                StringComparer.Ordinal.Equals(
                    _replaySealGroupSha256,
                    _replayDescriptor.ReplaySealGroupSha256) &&
                _currentDpoPreferenceSha256 is null &&
                _authorizedReplaySourceBindings is null &&
                _authorizedReplayPayloadSha256 is null &&
                _authorizedReplayDpoPreferenceSha256 is null,
            _ => false
        };

    private static OperationResult<LogicalCapabilityTrainingRowIdentity> Invalid()
        => OperationResult<LogicalCapabilityTrainingRowIdentity>.Fail(
            "Logical capability training row identity is invalid.",
            "LOGICAL_CAPABILITY_TRAINING_ROW_IDENTITY_INVALID");

    private static bool IsCanonicalSha256(string? value)
    {
        if (value is null || value.Length != Sha256HexLength)
            return false;

        foreach (var character in value)
        {
            if (character is not (>= '0' and <= '9') and
                not (>= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Bounded, invocation-owned set of exact rows presented for one admission decision.
/// </summary>
public sealed class LogicalCapabilityTrainingAdmissionBatch
{
    public const int MaximumRowCount = 4096;
    public const string ProvenanceRequiredErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_PROVENANCE_REQUIRED";
    public const string ProvenanceInvalidErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_PROVENANCE_INVALID";
    public const string ReplayKeyMismatchErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_REPLAY_KEY_MISMATCH";
    public const string ReplaySealUnavailableErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_REPLAY_SEAL_UNAVAILABLE";
    public const string ReplayVerificationUnavailableErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_REPLAY_VERIFICATION_UNAVAILABLE";

    private readonly ReadOnlyCollection<LogicalCapabilityTrainingRowIdentity> _rows;
    private readonly bool _isTrustedConstruction;

    private LogicalCapabilityTrainingAdmissionBatch(
        LogicalCapabilityTrainingRowIdentity[] rows,
        bool isTrustedConstruction)
    {
        _rows = Array.AsReadOnly(rows);
        _isTrustedConstruction = isTrustedConstruction;
    }

    public IReadOnlyList<LogicalCapabilityTrainingRowIdentity> Rows => _rows;

    public bool IsTrustedConstruction => _isTrustedConstruction;

    public static OperationResult<LogicalCapabilityTrainingAdmissionBatch> Create(
        IEnumerable<LogicalCapabilityTrainingRowIdentity>? rows)
        => CreateCore(rows, isTrustedConstruction: false);

    internal static OperationResult<LogicalCapabilityTrainingAdmissionBatch>
        CreateTrusted(
            IEnumerable<LogicalCapabilityTrainingRowIdentity>? rows)
        => CreateCore(rows, isTrustedConstruction: true);

    private static OperationResult<LogicalCapabilityTrainingAdmissionBatch>
        CreateCore(
            IEnumerable<LogicalCapabilityTrainingRowIdentity>? rows,
            bool isTrustedConstruction)
    {
        if (rows is null)
            return Invalid();

        LogicalCapabilityTrainingRowIdentity[] snapshot;
        try
        {
            snapshot = rows.Take(MaximumRowCount + 1).ToArray();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid();
        }

        if (snapshot.Length is 0 or > MaximumRowCount ||
            snapshot.Any(static row => !row.IsCanonical) ||
            snapshot.Select(static row => row.Sha256)
                .Distinct(StringComparer.Ordinal).Count() != snapshot.Length ||
            (isTrustedConstruction && snapshot.Any(static row =>
                !row.HasCanonicalTrainingContentProvenance)))
        {
            return Invalid();
        }

        return OperationResult<LogicalCapabilityTrainingAdmissionBatch>.Ok(
            new LogicalCapabilityTrainingAdmissionBatch(
                snapshot.ToArray(),
                isTrustedConstruction));
    }

    private static OperationResult<LogicalCapabilityTrainingAdmissionBatch> Invalid()
        => OperationResult<LogicalCapabilityTrainingAdmissionBatch>.Fail(
            "Logical capability training admission batch is invalid.",
            "LOGICAL_CAPABILITY_TRAINING_ADMISSION_BATCH_INVALID");
}

public static class LogicalCapabilityTrainingAdmissionBatchFactory
{
    private const int MaximumTrainingVectorLength = 65_536;

    public static OperationResult<LogicalCapabilityTrainingAdmissionBatch>
        CreateAttested(
            string? runId,
            string? domainAdapterTag,
            IReadOnlyList<LogicalCapabilityTrainingAdmissionPreference>?
                preferences)
    {
        try
        {
            var ownedPreferences = preferences?
                .Take(LogicalCapabilityTrainingAdmissionBatch.MaximumRowCount + 1)
                .ToArray() ?? [];
            if (ownedPreferences.Length is 0 or >
                LogicalCapabilityTrainingAdmissionBatch.MaximumRowCount)
            {
                return InvalidAdmissionBatch();
            }

            var rows = new LogicalCapabilityTrainingRowIdentity[
                ownedPreferences.Length];
            for (var ordinal = 0; ordinal < ownedPreferences.Length; ordinal++)
            {
                var preference = ownedPreferences[ordinal];
                var contentProvenance =
                    preference.TrainingContentProvenance?.Snapshot();
                var replayDescriptor = preference.ReplayDescriptor is null
                    ? null
                    : preference.ReplayDescriptor with { };
                var replayAttestation = preference.ReplayAttestation is null
                    ? null
                    : preference.ReplayAttestation with { };
                var isCurrent = contentProvenance is not null &&
                    replayDescriptor is null &&
                    replayAttestation is null;
                var isReplay = contentProvenance is null &&
                    replayDescriptor is not null &&
                    replayAttestation is not null &&
                    replayAttestation.Matches(replayDescriptor);
                if (!isCurrent && !isReplay)
                    return MissingProvenance();

                if (isCurrent && (!contentProvenance!.IsCanonical ||
                    !contentProvenance.BindsTrainingInputs ||
                    contentProvenance.SourceKind !=
                        LogicalTrainingContentSourceKind
                            .DeterministicSyntheticNoExternalTeacher))
                {
                    return InvalidProvenance();
                }

                var vector = preference.TeachingVector?
                    .Take(MaximumTrainingVectorLength + 1)
                    .ToArray();
                var preferenceDigest =
                    LogicalCapabilityTrainingPreferenceProjector
                        .ComputeDpoPreferenceSha256(
                            preference.Prompt,
                            preference.Chosen,
                            preference.Rejected,
                            vector);
                if (!preferenceDigest.IsSuccess || vector is null ||
                    vector.Length is 0 or > MaximumTrainingVectorLength ||
                    vector.Any(static value => !float.IsFinite(value)) ||
                    (isReplay && !StringComparer.Ordinal.Equals(
                        replayDescriptor!.DpoPreferenceSha256,
                        preferenceDigest.Value)))
                {
                    return InvalidProvenance();
                }

                using var payload = new MemoryStream();
                WriteCanonicalString(
                    payload,
                    "logical-capability-training-row-v1");
                WriteCanonicalString(payload, runId!);
                WriteCanonicalString(payload, domainAdapterTag!);
                WriteCanonicalInt32(payload, ordinal);
                WriteCanonicalString(payload, preference.Prompt);
                WriteCanonicalString(payload, preference.Chosen);
                WriteCanonicalString(payload, preference.Rejected);
                WriteCanonicalInt32(payload, vector.Length);
                foreach (var value in vector)
                {
                    WriteCanonicalInt32(
                        payload,
                        BitConverter.SingleToInt32Bits(value));
                }
                var digest = Convert.ToHexString(
                    SHA256.HashData(payload.ToArray())).ToLowerInvariant();
                var identity = isCurrent
                    ? LogicalCapabilityTrainingRowIdentity.CreateTrustedCurrent(
                        digest,
                        preferenceDigest.Value,
                        contentProvenance)
                    : LogicalCapabilityTrainingRowIdentity.CreateTrustedReplay(
                        digest,
                        replayDescriptor,
                        replayAttestation);
                if (!identity.IsSuccess)
                    return identity.MapError<
                        LogicalCapabilityTrainingAdmissionBatch>();
                rows[ordinal] = identity.Value;
            }

            return LogicalCapabilityTrainingAdmissionBatch.CreateTrusted(rows);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<LogicalCapabilityTrainingAdmissionBatch>.Fail(
                "Logical capability training row identity could not be computed.",
                "LOGICAL_CAPABILITY_TRAINING_ROW_IDENTITY_INVALID");
        }
    }

    private static OperationResult<LogicalCapabilityTrainingAdmissionBatch>
        InvalidAdmissionBatch()
        => OperationResult<LogicalCapabilityTrainingAdmissionBatch>.Fail(
            "Logical capability training admission batch is invalid.",
            "LOGICAL_CAPABILITY_TRAINING_ADMISSION_BATCH_INVALID");

    private static OperationResult<LogicalCapabilityTrainingAdmissionBatch>
        InvalidProvenance()
        => OperationResult<LogicalCapabilityTrainingAdmissionBatch>.Fail(
            "Logical training row provenance is invalid.",
            LogicalCapabilityTrainingAdmissionBatch
                .ProvenanceInvalidErrorCode);

    private static OperationResult<LogicalCapabilityTrainingAdmissionBatch>
        MissingProvenance()
        => OperationResult<LogicalCapabilityTrainingAdmissionBatch>.Fail(
            "Typed provenance for every logical training row is required.",
            LogicalCapabilityTrainingAdmissionBatch
                .ProvenanceRequiredErrorCode);

    private static void WriteCanonicalString(Stream destination, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteCanonicalInt32(destination, bytes.Length);
        destination.Write(bytes, 0, bytes.Length);
    }

    private static void WriteCanonicalInt32(Stream destination, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        destination.Write(bytes);
    }
}

/// <summary>
/// Neutral revocable mutation authority retained by a single admitted invocation.
/// </summary>
public interface ILogicalCapabilityTrainingAdmissionLease : IDisposable
{
    OperationResult<IDisposable> AcquireMutationPermit();

    OperationResult<LogicalCapabilityTrainingReplayAttestation> SealReplay(
        LogicalCapabilityTrainingReplayDescriptor descriptor)
        => OperationResult<LogicalCapabilityTrainingReplayAttestation>.Fail(
            "Logical capability replay sealing is unavailable.",
            LogicalCapabilityTrainingAdmissionBatch
                .ReplaySealUnavailableErrorCode);

    OperationResult<LogicalCapabilityTrainingAdmissionCapabilities>
        IssueAdmissionCapabilities()
        => OperationResult<LogicalCapabilityTrainingAdmissionCapabilities>.Fail(
            "A server-issued logical capability training admission bundle is required.",
            LogicalCapabilityTrainingAdmissionCapabilities.RequiredErrorCode);
}

/// <summary>
/// Internal proof that a lease was issued by a trusted server admission
/// authority. Public implementations of the neutral lease contract cannot
/// opt into the DPO mutation boundary.
/// </summary>
internal interface ILogicalDpoMutationAdmissionIssuer
{
    OperationResult<IDisposable> AcquireDpoMutationPermit();
}

/// <summary>
/// Opaque public bundle of the exact mutation capabilities issued by one
/// admitted logical capability-training lease.
/// </summary>
public sealed class LogicalCapabilityTrainingAdmissionCapabilities
{
    public const string RequiredErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_ADMISSION_CAPABILITIES_REQUIRED";

    private readonly LogicalDpoMutationAdmissionCapability
        _logicalMutationAdmission;
    private readonly ILogicalCapabilityTrainingAdmissionLease?
        _admissionLease;
    private readonly
        LogicalCapabilityExecutableManifestAttestationCapability
        _executableManifestAttestation;

    internal LogicalCapabilityTrainingAdmissionCapabilities(
        LogicalDpoMutationAdmissionCapability logicalMutationAdmission,
        DpoMutationAdmissionCapability dpoTrainerMutationAdmissionCapability,
        LogicalCapabilityExecutableManifestAttestationCapability
            executableManifestAttestation,
        ILogicalCapabilityTrainingAdmissionLease? admissionLease = null)
    {
        _logicalMutationAdmission = logicalMutationAdmission ??
            throw new ArgumentNullException(nameof(logicalMutationAdmission));
        _admissionLease = admissionLease;
        DpoTrainerMutationAdmissionCapability =
            dpoTrainerMutationAdmissionCapability ??
            throw new ArgumentNullException(
                nameof(dpoTrainerMutationAdmissionCapability));
        _executableManifestAttestation =
            executableManifestAttestation ??
            throw new ArgumentNullException(
                nameof(executableManifestAttestation));
    }

    public DpoMutationAdmissionCapability
        DpoTrainerMutationAdmissionCapability { get; }

    internal LogicalDpoMutationAdmissionCapability
        LogicalMutationAdmissionCapability => _logicalMutationAdmission;

    public OperationResult<IDisposable> AcquireMutationPermit()
        => _admissionLease?.AcquireMutationPermit() ??
            _logicalMutationAdmission.AcquireMutationPermit();

    public OperationResult<LogicalCapabilityExecutableManifestAttestation>
        SealExecutableManifest(
            string executableManifestSha256,
            LogicalCapabilityTrainingReplayAttestation replayAttestation)
        => _executableManifestAttestation.Seal(
            executableManifestSha256,
            replayAttestation);
}

/// <summary>
/// Opaque admission capability required by the public logical DPO unit.
/// Only trusted friend assemblies can mint one from a server-issued lease.
/// </summary>
public sealed class LogicalDpoMutationAdmissionCapability
{
    public const string RequiredErrorCode =
        "DPO_MUTATION_ADMISSION_CAPABILITY_REQUIRED";

    private readonly ILogicalDpoMutationAdmissionIssuer _issuer;

    private LogicalDpoMutationAdmissionCapability(
        ILogicalDpoMutationAdmissionIssuer issuer)
    {
        _issuer = issuer;
    }

    internal static OperationResult<LogicalDpoMutationAdmissionCapability>
        Issue(ILogicalCapabilityTrainingAdmissionLease? lease)
        => lease is ILogicalDpoMutationAdmissionIssuer issuer
            ? OperationResult<LogicalDpoMutationAdmissionCapability>.Ok(
                new LogicalDpoMutationAdmissionCapability(issuer))
            : OperationResult<LogicalDpoMutationAdmissionCapability>.Fail(
                "A server-issued DPO mutation admission capability is required.",
                RequiredErrorCode);

    internal OperationResult<IDisposable> AcquireMutationPermit()
        => _issuer.AcquireDpoMutationPermit();
}

/// <summary>
/// Internal proof that executable-manifest sealing is backed by the same
/// genuine lease that admitted the logical capability training mutation.
/// </summary>
internal interface
    ILogicalCapabilityExecutableManifestAttestationIssuer
{
    OperationResult<LogicalCapabilityExecutableManifestAttestation>
        SealExecutableManifest(
            string executableManifestSha256,
            LogicalCapabilityTrainingReplayAttestation replayAttestation);
}

/// <summary>
/// Opaque lease-issued capability for sealing an executable manifest to its
/// replay authorization.
/// </summary>
public sealed class
    LogicalCapabilityExecutableManifestAttestationCapability
{
    public const string RequiredErrorCode =
        "LOGICAL_CAPABILITY_EXECUTABLE_MANIFEST_ATTESTATION_CAPABILITY_REQUIRED";

    private readonly
        ILogicalCapabilityExecutableManifestAttestationIssuer _issuer;

    private LogicalCapabilityExecutableManifestAttestationCapability(
        ILogicalCapabilityExecutableManifestAttestationIssuer issuer)
    {
        _issuer = issuer;
    }

    internal static OperationResult<
        LogicalCapabilityExecutableManifestAttestationCapability> Issue(
            ILogicalCapabilityTrainingAdmissionLease? lease)
        => lease is
            ILogicalCapabilityExecutableManifestAttestationIssuer issuer
            ? OperationResult<
                LogicalCapabilityExecutableManifestAttestationCapability>.Ok(
                    new(issuer))
            : OperationResult<
                LogicalCapabilityExecutableManifestAttestationCapability>.Fail(
                    "A server-issued executable manifest attestation capability is required.",
                    RequiredErrorCode);

    internal OperationResult<
        LogicalCapabilityExecutableManifestAttestation> Seal(
            string executableManifestSha256,
            LogicalCapabilityTrainingReplayAttestation replayAttestation)
        => _issuer.SealExecutableManifest(
            executableManifestSha256,
            replayAttestation);
}

/// <summary>
/// Internal proof that replay verification is backed by a trusted server
/// authority rather than a public caller implementation.
/// </summary>
internal interface ILogicalCapabilityReplayVerificationIssuer
{
    OperationResult<bool> VerifyReplayAttestation(
        LogicalCapabilityTrainingReplayAttestation attestation);

    OperationResult<bool> VerifyExecutableManifestAttestation(
        string executableManifestSha256,
        LogicalCapabilityTrainingReplayAttestation replayAttestation,
        LogicalCapabilityExecutableManifestAttestation
            executableAttestation);
}

/// <summary>
/// Opaque capability for verifying persisted replay attestations during
/// executable registry restore.
/// </summary>
public sealed class LogicalCapabilityReplayVerificationCapability
{
    public const string RequiredErrorCode =
        "LOGICAL_CAPABILITY_REPLAY_VERIFICATION_CAPABILITY_REQUIRED";

    private readonly ILogicalCapabilityReplayVerificationIssuer _issuer;

    private LogicalCapabilityReplayVerificationCapability(
        ILogicalCapabilityReplayVerificationIssuer issuer)
    {
        _issuer = issuer;
    }

    internal static OperationResult<
        LogicalCapabilityReplayVerificationCapability> Issue(
            ILogicalCapabilityReplayVerificationIssuer? issuer)
        => issuer is not null
            ? OperationResult<
                LogicalCapabilityReplayVerificationCapability>.Ok(new(issuer))
            : OperationResult<
                LogicalCapabilityReplayVerificationCapability>.Fail(
                    "A server-issued replay verification capability is required.",
                    RequiredErrorCode);

    public OperationResult<bool> VerifyReplay(
        LogicalCapabilityTrainingReplayAttestation attestation)
        => _issuer.VerifyReplayAttestation(attestation);

    public OperationResult<bool> VerifyExecutableManifest(
        string executableManifestSha256,
        LogicalCapabilityTrainingReplayAttestation replayAttestation,
        LogicalCapabilityExecutableManifestAttestation
            executableAttestation)
        => _issuer.VerifyExecutableManifestAttestation(
            executableManifestSha256,
            replayAttestation,
            executableAttestation);
}

/// <summary>
/// Low-dependency admission seam for deterministic logical capability training.
/// </summary>
public interface ILogicalCapabilityTrainingAdmissionAuthority
{
    OperationResult<bool> VerifyReplay(
        LogicalCapabilityTrainingReplayAttestation attestation)
        => OperationResult<bool>.Fail(
            "Logical capability replay verification is unavailable.",
            LogicalCapabilityTrainingAdmissionBatch
                .ReplayVerificationUnavailableErrorCode);

    OperationResult<ILogicalCapabilityTrainingAdmissionLease> AcquireTrainingLease(
        LogicalCapabilityTrainingAdmissionBatch batch);
}
