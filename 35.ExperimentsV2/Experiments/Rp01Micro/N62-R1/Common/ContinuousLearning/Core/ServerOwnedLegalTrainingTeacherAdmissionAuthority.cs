// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;

internal interface IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting
{
    void OnFinalMutationPermitGrantUnderGateForTesting();

    void OnMutationPermitActiveCountChangedForTesting(int activePermitCount);

    void OnLeaseDisposeAttemptedBeforeGateForTesting();
}

/// <summary>
/// Opaque, revocable authority to perform mutations for one admitted legal
/// teacher batch. The policy and revocation epoch remain server-owned.
/// </summary>
public sealed class ServerOwnedLegalTrainingTeacherAdmissionLease
    : ILogicalCapabilityTrainingAdmissionLease,
      ILogicalDpoMutationAdmissionIssuer,
      IDpoMutationAdmissionIssuer,
      ILogicalCapabilityExecutableManifestAttestationIssuer
{
    internal sealed class Lifetime
    {
        internal int Disposed;
    }

    private readonly Func<OperationResult<IDisposable>> _acquireMutationPermit;
    private readonly Func<
        LogicalCapabilityTrainingReplayDescriptor,
        OperationResult<LogicalCapabilityTrainingReplayAttestation>>
        _sealReplay;
    private readonly Func<
        string,
        LogicalCapabilityTrainingReplayAttestation,
        OperationResult<LogicalCapabilityExecutableManifestAttestation>>
        _sealExecutableManifest;
    private readonly Func<bool> _isLeaseLive;
    private readonly Action _dispose;
    private readonly Lifetime _lifetime;
    private readonly string? _trainingContentSha256;
    private readonly ReadOnlyCollection<string>? _trainingRowSha256;

    internal ServerOwnedLegalTrainingTeacherAdmissionLease(
        LegalTrainingTeacherAdmission admission,
        Func<OperationResult<IDisposable>> acquireMutationPermit,
        Func<
            LogicalCapabilityTrainingReplayDescriptor,
            OperationResult<LogicalCapabilityTrainingReplayAttestation>>
            sealReplay,
        Func<
            string,
            LogicalCapabilityTrainingReplayAttestation,
            OperationResult<LogicalCapabilityExecutableManifestAttestation>>
            sealExecutableManifest,
        Func<bool> isLeaseLive,
        Action dispose,
        Lifetime lifetime,
        DpoTrainingContentBatch? trainingContent)
    {
        Admission = admission ?? throw new ArgumentNullException(nameof(admission));
        _acquireMutationPermit = acquireMutationPermit
            ?? throw new ArgumentNullException(nameof(acquireMutationPermit));
        _sealReplay = sealReplay
            ?? throw new ArgumentNullException(nameof(sealReplay));
        _sealExecutableManifest = sealExecutableManifest
            ?? throw new ArgumentNullException(nameof(sealExecutableManifest));
        _isLeaseLive = isLeaseLive
            ?? throw new ArgumentNullException(nameof(isLeaseLive));
        _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        if (trainingContent is not null)
        {
            _trainingContentSha256 = trainingContent.Sha256;
            _trainingRowSha256 = Array.AsReadOnly(
                trainingContent.Rows
                    .Select(static row => row.Sha256)
                    .ToArray());
        }
    }

    public LegalTrainingTeacherAdmission Admission { get; }

    public OperationResult<IDisposable> AcquireMutationPermit()
        => Volatile.Read(ref _lifetime.Disposed) != 0
            ? ServerOwnedLegalTrainingTeacherAdmissionAuthority.RevokedPermit()
            : _acquireMutationPermit();

    internal bool MatchesExactTrainingContent(
        DpoTrainingContentBatch? trainingContent)
    {
        if (trainingContent is null ||
            Volatile.Read(ref _lifetime.Disposed) != 0 ||
            _trainingContentSha256 is null ||
            _trainingRowSha256 is null ||
            !string.Equals(
                _trainingContentSha256,
                trainingContent.Sha256,
                StringComparison.Ordinal) ||
            _trainingRowSha256.Count != trainingContent.Rows.Count)
        {
            return false;
        }

        for (var index = 0; index < _trainingRowSha256.Count; index++)
        {
            if (!string.Equals(
                    _trainingRowSha256[index],
                    trainingContent.Rows[index].Sha256,
                    StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    OperationResult<IDisposable>
        ILogicalDpoMutationAdmissionIssuer.AcquireDpoMutationPermit()
        => AcquireMutationPermit();

    public OperationResult<LogicalDpoMutationAdmissionCapability>
        IssueDpoMutationAdmissionCapability()
        => !_isLeaseLive()
            ? OperationResult<LogicalDpoMutationAdmissionCapability>.Fail(
                "The legal teacher training admission was revoked.",
                ServerOwnedLegalTrainingTeacherAdmissionAuthority
                    .AdmissionRevokedErrorCode)
            : LogicalDpoMutationAdmissionCapability.Issue(this);

    public OperationResult<DpoMutationAdmissionCapability>
        IssueDpoTrainerMutationAdmissionCapability()
        => !_isLeaseLive()
            ? OperationResult<DpoMutationAdmissionCapability>.Fail(
                "The legal teacher training admission was revoked.",
                ServerOwnedLegalTrainingTeacherAdmissionAuthority
                    .AdmissionRevokedErrorCode)
            : OperationResult<DpoMutationAdmissionCapability>.Ok(
                DpoMutationAdmissionCapability.Issue(this));

    public OperationResult<LogicalCapabilityTrainingAdmissionCapabilities>
        IssueAdmissionCapabilities()
    {
        if (!_isLeaseLive())
        {
            return OperationResult<
                LogicalCapabilityTrainingAdmissionCapabilities>.Fail(
                "The legal teacher training admission was revoked.",
                ServerOwnedLegalTrainingTeacherAdmissionAuthority
                    .AdmissionRevokedErrorCode);
        }

        var logicalMutationAdmission =
            LogicalDpoMutationAdmissionCapability.Issue(this);
        if (!logicalMutationAdmission.IsSuccess ||
            logicalMutationAdmission.Value is null)
        {
            return logicalMutationAdmission.MapError<
                LogicalCapabilityTrainingAdmissionCapabilities>();
        }

        var executableManifestAttestation =
            LogicalCapabilityExecutableManifestAttestationCapability.Issue(
                this);
        if (!executableManifestAttestation.IsSuccess ||
            executableManifestAttestation.Value is null)
        {
            return executableManifestAttestation.MapError<
                LogicalCapabilityTrainingAdmissionCapabilities>();
        }

        if (!_isLeaseLive())
        {
            return OperationResult<
                LogicalCapabilityTrainingAdmissionCapabilities>.Fail(
                "The legal teacher training admission was revoked.",
                ServerOwnedLegalTrainingTeacherAdmissionAuthority
                    .AdmissionRevokedErrorCode);
        }

        var dpoTrainerMutationAdmissionCapability =
            DpoMutationAdmissionCapability.Issue(this);

        return OperationResult<
            LogicalCapabilityTrainingAdmissionCapabilities>.Ok(
            new LogicalCapabilityTrainingAdmissionCapabilities(
                logicalMutationAdmission.Value,
                dpoTrainerMutationAdmissionCapability,
                executableManifestAttestation.Value,
                this));
    }

    IDisposable IDpoMutationAdmissionIssuer
        .AcquireDpoTrainerMutationPermit()
    {
        var permit = AcquireMutationPermit();
        if (!permit.IsSuccess || permit.Value is null)
        {
            throw new DpoMutationAdmissionException(
                permit.ErrorMessage,
                permit.ErrorCode);
        }
        return permit.Value;
    }

    public OperationResult<LogicalCapabilityTrainingReplayAttestation>
        SealReplay(LogicalCapabilityTrainingReplayDescriptor descriptor)
        => Volatile.Read(ref _lifetime.Disposed) != 0
            ? OperationResult<
                LogicalCapabilityTrainingReplayAttestation>.Fail(
                "The legal teacher training admission was revoked.",
                ServerOwnedLegalTrainingTeacherAdmissionAuthority
                    .AdmissionRevokedErrorCode)
            : _sealReplay(descriptor);

    OperationResult<LogicalCapabilityExecutableManifestAttestation>
        ILogicalCapabilityExecutableManifestAttestationIssuer
            .SealExecutableManifest(
                string executableManifestSha256,
                LogicalCapabilityTrainingReplayAttestation
                    replayAttestation)
        => Volatile.Read(ref _lifetime.Disposed) != 0
            ? OperationResult<
                LogicalCapabilityExecutableManifestAttestation>.Fail(
                    "The legal teacher training admission was revoked.",
                    ServerOwnedLegalTrainingTeacherAdmissionAuthority
                        .AdmissionRevokedErrorCode)
            : _sealExecutableManifest(
                executableManifestSha256,
                replayAttestation);

    public void Dispose()
        => _dispose();
}

/// <summary>
/// Opaque ModelServer capability for issuing server-owned DPO training leases
/// and revocable mutation admission after legal-teacher policy admission. The
/// underlying <see cref="LegalTrainingTeacherAdmissionAuthority"/> remains the
/// separate teacher policy, call-budget, and provenance admission role.
/// </summary>
public sealed class ServerOwnedLegalTrainingTeacherAdmissionAuthority
    : ILogicalCapabilityTrainingAdmissionAuthority,
      ILogicalCapabilityReplayVerificationIssuer,
      IServerOwnedDpoTrainingLeaseBroker
{
    public const string AuthorityRole =
        "server-owned-dpo-training-lease-revocable-mutation-admission";
    public const string AdmissionRevokedErrorCode =
        "LEGAL_TRAINING_ADMISSION_REVOKED";
    public const string RequestLedgerCapacityExceededErrorCode =
        "LEGAL_TRAINING_REQUEST_LEDGER_CAPACITY_EXCEEDED";

    // Never evict a consumed request id: eviction would turn a memory bound
    // into replay authorization. Once the authority-lifetime ledger is full,
    // all otherwise-new requests fail closed until the authority is replaced.
    internal const int MaximumConsumedTeacherRequestIds = 65_536;

    private const string ConfigurationRequiredSafeMessage =
        "Server-owned legal teacher admission configuration is required.";
    private const string AdmissionRevokedSafeMessage =
        "The legal teacher training admission was revoked.";

    private readonly Func<
        IEnumerable<TrainingTeacherProvenance?>?,
        OperationResult<LegalTrainingTeacherAdmission>>? _validateBatch;
    private readonly Func<
        DpoTrainingContentBatch?,
        IEnumerable<TrainingTeacherProvenance?>?,
        OperationResult<LegalTrainingTeacherAdmission>>? _validateBoundBatch;
    private readonly Func<bool>? _isAdmissionLive;
    private readonly Func<
        LogicalCapabilityTrainingReplayDescriptor,
        OperationResult<LogicalCapabilityTrainingReplayAttestation>>?
        _sealReplay;
    private readonly Func<
        LogicalCapabilityTrainingReplayAttestation,
        OperationResult<bool>>? _verifyReplay;
    private readonly Func<
        string,
        LogicalCapabilityTrainingReplayAttestation,
        OperationResult<LogicalCapabilityExecutableManifestAttestation>>?
        _sealExecutableManifest;
    private readonly Func<
        string,
        LogicalCapabilityTrainingReplayAttestation,
        LogicalCapabilityExecutableManifestAttestation,
        OperationResult<bool>>? _verifyExecutableManifest;
    private readonly IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting?
        _concurrencyObserverForTesting;
    private readonly RevocationState _state;

    private ServerOwnedLegalTrainingTeacherAdmissionAuthority(
        Func<
            IEnumerable<TrainingTeacherProvenance?>?,
            OperationResult<LegalTrainingTeacherAdmission>>? validateBatch,
        Func<
            DpoTrainingContentBatch?,
            IEnumerable<TrainingTeacherProvenance?>?,
            OperationResult<LegalTrainingTeacherAdmission>>?
            validateBoundBatch,
        Func<bool>? isAdmissionLive,
        Func<
            LogicalCapabilityTrainingReplayDescriptor,
            OperationResult<LogicalCapabilityTrainingReplayAttestation>>?
            sealReplay,
        Func<
            LogicalCapabilityTrainingReplayAttestation,
            OperationResult<bool>>? verifyReplay,
        Func<
            string,
            LogicalCapabilityTrainingReplayAttestation,
            OperationResult<LogicalCapabilityExecutableManifestAttestation>>?
            sealExecutableManifest,
        Func<
            string,
            LogicalCapabilityTrainingReplayAttestation,
            LogicalCapabilityExecutableManifestAttestation,
            OperationResult<bool>>? verifyExecutableManifest,
        IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting?
            concurrencyObserverForTesting)
    {
        _validateBatch = validateBatch;
        _validateBoundBatch = validateBoundBatch;
        _isAdmissionLive = isAdmissionLive;
        _sealReplay = sealReplay;
        _verifyReplay = verifyReplay;
        _sealExecutableManifest = sealExecutableManifest;
        _verifyExecutableManifest = verifyExecutableManifest;
        _concurrencyObserverForTesting = concurrencyObserverForTesting;
        _state = new RevocationState();
    }

    /// <summary>
    /// Validates typed provenance without exposing policy, allowlist, budget,
    /// or audit material.
    /// </summary>
    public OperationResult<LegalTrainingTeacherAdmission> ValidateBatch(
        IEnumerable<TrainingTeacherProvenance?>? provenance)
    {
        var validateBatch = _validateBatch;
        var isAdmissionLive = _isAdmissionLive;
        if (validateBatch is null || isAdmissionLive is null)
            return ConfigurationRequired();

        try
        {
            if (!isAdmissionLive())
                return ConfigurationRequired();
            return validateBatch(provenance);
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return ConfigurationRequired();
        }
    }

    /// <summary>
    /// Validates an exact, content-bound evaluation batch without issuing a
    /// mutation lease and without consuming teacher request ids.
    /// </summary>
    internal OperationResult<LegalTrainingTeacherAdmission> ValidateEvaluationBatch(
        DpoTrainingContentBatch? evaluationContent,
        IReadOnlyList<TrainingTeacherProvenance?>? provenance)
    {
        var validateBoundBatch = _validateBoundBatch;
        var isAdmissionLive = _isAdmissionLive;
        if (validateBoundBatch is null || isAdmissionLive is null)
            return ConfigurationRequired();
        if (evaluationContent is null || provenance is null ||
            evaluationContent.Rows.Count != provenance.Count ||
            evaluationContent.Rows.Count is <= 0 or >
                BoundedDpoTrainingContentProjector.MaximumRowCount)
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "Exact bounded DPO evaluation content and matching provenance are required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }

        TrainingTeacherProvenance?[] provenanceSnapshot;
        try
        {
            provenanceSnapshot = provenance
                .Select(static row => row is null ? null : row with { })
                .ToArray();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "Exact bounded DPO evaluation provenance could not be snapshotted.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }

        long epoch;
        lock (_state.Gate)
        {
            if (_state.Revoked)
                return RevokedAdmission();
            epoch = _state.Epoch;
        }

        OperationResult<LegalTrainingTeacherAdmission> admission;
        try
        {
            if (!isAdmissionLive())
                return ConfigurationRequired();
            admission = validateBoundBatch(
                evaluationContent,
                provenanceSnapshot);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ConfigurationRequired();
        }

        if (!admission.IsSuccess || admission.Value is null)
            return admission;

        lock (_state.Gate)
        {
            if (_state.Revoked || _state.Epoch != epoch)
                return RevokedAdmission();
        }

        try
        {
            if (!isAdmissionLive())
                return ConfigurationRequired();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ConfigurationRequired();
        }

        lock (_state.Gate)
        {
            return _state.Revoked || _state.Epoch != epoch
                ? RevokedAdmission()
                : admission;
        }
    }

    public OperationResult<bool> VerifyReplay(
        LogicalCapabilityTrainingReplayAttestation attestation)
    {
        long epoch;
        lock (_state.Gate)
        {
            if (_state.Revoked)
                return RevokedReplayVerification();
            epoch = _state.Epoch;
        }
        var verifyReplay = _verifyReplay;
        var isAdmissionLive = _isAdmissionLive;
        if (attestation is null || !attestation.IsCanonical ||
            verifyReplay is null || isAdmissionLive is null)
        {
            return OperationResult<bool>.Fail(
                "Logical capability replay attestation is invalid.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);
        }

        try
        {
            if (!isAdmissionLive())
            {
                return OperationResult<bool>.Fail(
                    ConfigurationRequiredSafeMessage,
                    LegalTrainingTeacherAdmissionAuthority
                        .ConfigurationRequiredErrorCode);
            }
            var verified = verifyReplay(attestation);
            lock (_state.Gate)
            {
                if (_state.Revoked || _state.Epoch != epoch)
                    return RevokedReplayVerification();
            }
            return verified.IsSuccess && verified.Value
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    verified.ErrorMessage,
                    verified.ErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "Logical capability replay attestation is invalid.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);
        }
    }

    OperationResult<bool>
        ILogicalCapabilityReplayVerificationIssuer.VerifyReplayAttestation(
            LogicalCapabilityTrainingReplayAttestation attestation)
        => VerifyReplay(attestation);

    public OperationResult<bool> VerifyExecutableManifest(
        string executableManifestSha256,
        LogicalCapabilityTrainingReplayAttestation replayAttestation,
        LogicalCapabilityExecutableManifestAttestation
            executableAttestation)
    {
        long epoch;
        lock (_state.Gate)
        {
            if (_state.Revoked)
                return RevokedReplayVerification();
            epoch = _state.Epoch;
        }

        var verifyExecutableManifest = _verifyExecutableManifest;
        var isAdmissionLive = _isAdmissionLive;
        if (verifyExecutableManifest is null || isAdmissionLive is null ||
            replayAttestation is not { IsCanonical: true } ||
            executableAttestation is not { IsCanonical: true } ||
            !executableAttestation.Matches(
                executableManifestSha256,
                replayAttestation))
        {
            return InvalidExecutableManifestVerification();
        }

        var replayVerification = VerifyReplay(replayAttestation);
        if (!replayVerification.IsSuccess || !replayVerification.Value)
            return replayVerification;

        try
        {
            if (!isAdmissionLive())
            {
                return OperationResult<bool>.Fail(
                    ConfigurationRequiredSafeMessage,
                    LegalTrainingTeacherAdmissionAuthority
                        .ConfigurationRequiredErrorCode);
            }
            var verified = verifyExecutableManifest(
                executableManifestSha256,
                replayAttestation,
                executableAttestation);
            lock (_state.Gate)
            {
                if (_state.Revoked || _state.Epoch != epoch)
                    return RevokedReplayVerification();
            }
            return verified.IsSuccess && verified.Value
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    verified.ErrorMessage,
                    verified.ErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidExecutableManifestVerification();
        }
    }

    OperationResult<bool> ILogicalCapabilityReplayVerificationIssuer
        .VerifyExecutableManifestAttestation(
            string executableManifestSha256,
            LogicalCapabilityTrainingReplayAttestation replayAttestation,
            LogicalCapabilityExecutableManifestAttestation
                executableAttestation)
        => VerifyExecutableManifest(
            executableManifestSha256,
            replayAttestation,
            executableAttestation);

    public OperationResult<LogicalCapabilityReplayVerificationCapability>
        IssueReplayVerificationCapability()
    {
        lock (_state.Gate)
        {
            if (_state.Revoked)
            {
                return OperationResult<
                    LogicalCapabilityReplayVerificationCapability>.Fail(
                    AdmissionRevokedSafeMessage,
                    AdmissionRevokedErrorCode);
            }
        }
        return LogicalCapabilityReplayVerificationCapability.Issue(this);
    }

    internal OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireTrainingLease(
            IEnumerable<TrainingTeacherProvenance?>? provenance,
            IReadOnlyDictionary<string,
                LogicalCapabilityTrainingReplayAuthorization>?
                replaySealAuthorizations = null)
    {
        var validateBatch = _validateBatch;
        var isAdmissionLive = _isAdmissionLive;
        if (validateBatch is null || isAdmissionLive is null)
        {
            return ConfigurationRequired()
                .MapError<ServerOwnedLegalTrainingTeacherAdmissionLease>();
        }

        long epoch;
        lock (_state.Gate)
        {
            if (_state.Revoked)
                return RevokedLease();
            epoch = _state.Epoch;
        }

        OperationResult<LegalTrainingTeacherAdmission> admission;
        try
        {
            if (!isAdmissionLive())
            {
                return ConfigurationRequired()
                    .MapError<ServerOwnedLegalTrainingTeacherAdmissionLease>();
            }

            admission = validateBatch(provenance);
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return ConfigurationRequired()
                .MapError<ServerOwnedLegalTrainingTeacherAdmissionLease>();
        }

        if (!admission.IsSuccess || admission.Value is null)
            return admission.MapError<ServerOwnedLegalTrainingTeacherAdmissionLease>();

        lock (_state.Gate)
        {
            if (_state.Revoked || _state.Epoch != epoch)
                return RevokedLease();

            var lifetime = new ServerOwnedLegalTrainingTeacherAdmissionLease
                .Lifetime();
            return OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>.Ok(
                new ServerOwnedLegalTrainingTeacherAdmissionLease(
                    admission.Value,
                    () => AcquireMutationPermit(epoch, lifetime),
                    descriptor => SealReplay(
                        epoch,
                        lifetime,
                        replaySealAuthorizations,
                        descriptor),
                    (executableManifestSha256, replayAttestation) =>
                        SealExecutableManifest(
                            epoch,
                            lifetime,
                            replaySealAuthorizations,
                            executableManifestSha256,
                            replayAttestation),
                    () => IsLeaseLive(epoch, lifetime),
                    () => DisposeLease(lifetime),
                    lifetime,
                    trainingContent: null));
        }
    }

    internal OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireTrainingLease(
            DpoTrainingContentBatch? trainingContent,
            IReadOnlyList<TrainingTeacherProvenance?>? provenance,
            FileDpoPairStore.TransactionHandle? transaction = null,
            string? transactionDomain = null)
    {
        var validateBoundBatch = _validateBoundBatch;
        var isAdmissionLive = _isAdmissionLive;
        if (validateBoundBatch is null || isAdmissionLive is null)
        {
            return ConfigurationRequired()
                .MapError<ServerOwnedLegalTrainingTeacherAdmissionLease>();
        }
        if (trainingContent is null || provenance is null ||
            trainingContent.Rows.Count != provenance.Count ||
            trainingContent.Rows.Count is <= 0 or >
                BoundedDpoTrainingContentProjector.MaximumRowCount)
        {
            return OperationResult<
                ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
                "Exact bounded DPO training content and matching provenance are required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }

        var provenanceSnapshot =
            new TrainingTeacherProvenance?[provenance.Count];
        try
        {
            for (var index = 0; index < provenanceSnapshot.Length; index++)
                provenanceSnapshot[index] = provenance[index] is { } row
                    ? row with { }
                    : null;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
                "Exact bounded DPO training provenance could not be snapshotted.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }

        long epoch;
        lock (_state.Gate)
        {
            if (_state.Revoked)
                return RevokedLease();
            epoch = _state.Epoch;
        }

        OperationResult<LegalTrainingTeacherAdmission> admission;
        try
        {
            if (!isAdmissionLive())
            {
                return ConfigurationRequired()
                    .MapError<ServerOwnedLegalTrainingTeacherAdmissionLease>();
            }
            admission = validateBoundBatch(
                trainingContent,
                provenanceSnapshot);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ConfigurationRequired()
                .MapError<ServerOwnedLegalTrainingTeacherAdmissionLease>();
        }
        if (!admission.IsSuccess || admission.Value is null)
            return admission
                .MapError<ServerOwnedLegalTrainingTeacherAdmissionLease>();

        if ((transaction is null) != (transactionDomain is null))
        {
            return OperationResult<
                ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
                "Exact durable DPO transaction authority is invalid.",
                "DPO_TRANSACTION_TEACHER_BINDING_INVALID");
        }
        if (transaction is not null)
        {
            var durableReservation = transaction.ValidateTeacherBatch(
                transactionDomain!,
                trainingContent,
                provenanceSnapshot);
            if (!durableReservation.IsSuccess)
            {
                return durableReservation
                    .MapError<ServerOwnedLegalTrainingTeacherAdmissionLease>();
            }
        }

        lock (_state.Gate)
        {
            if (_state.Revoked || _state.Epoch != epoch)
                return RevokedLease();

            for (var index = 0;
                 transaction is null && index < provenanceSnapshot.Length;
                 index++)
            {
                var requestId = provenanceSnapshot[index]!.RequestId;
                if (_state.ConsumedTeacherRequestIds.ContainsKey(requestId))
                {
                    return OperationResult<
                        ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
                        "A teacher request id has already been consumed by training.",
                        LegalTrainingTeacherAdmissionAuthority
                            .DuplicateRequestIdErrorCode);
                }
            }
            if (transaction is null && provenanceSnapshot.Length >
                MaximumConsumedTeacherRequestIds -
                _state.ConsumedTeacherRequestIds.Count)
            {
                return OperationResult<
                    ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
                    "The bounded teacher request replay ledger is full; no new training admission can be issued by this authority.",
                    RequestLedgerCapacityExceededErrorCode);
            }
            for (var index = 0;
                 transaction is null && index < provenanceSnapshot.Length;
                 index++)
            {
                _state.ConsumedTeacherRequestIds.Add(
                    provenanceSnapshot[index]!.RequestId,
                    trainingContent.Rows[index].Sha256);
            }

            var lifetime = new ServerOwnedLegalTrainingTeacherAdmissionLease
                .Lifetime();
            return OperationResult<
                ServerOwnedLegalTrainingTeacherAdmissionLease>.Ok(
                new ServerOwnedLegalTrainingTeacherAdmissionLease(
                    admission.Value,
                    () => AcquireMutationPermit(epoch, lifetime),
                    descriptor => SealReplay(
                        epoch,
                        lifetime,
                        allowedReplayAuthorizations: null,
                        descriptor: descriptor),
                    (executableManifestSha256, replayAttestation) =>
                        SealExecutableManifest(
                            epoch,
                            lifetime,
                            allowedReplayAuthorizations: null,
                            executableManifestSha256,
                            replayAttestation),
                    () => IsLeaseLive(epoch, lifetime),
                    () => DisposeLease(lifetime),
                    lifetime,
                    trainingContent));
        }
    }

    /// <summary>
    /// Friend-only ProjectWiki seam for a deterministic, no-external-teacher
    /// batch. The public surface cannot turn arbitrary caller content into
    /// provenance; the server authority still validates and consumes the exact
    /// projected content identities before issuing one revocable lease.
    /// </summary>
    internal OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireDeterministicDpoTrainingLease(
            DpoTrainingContentBatch? trainingContent)
        => AcquireDeterministicDpoTrainingLease(
            trainingContent,
            requestNamespace: "project-wiki-dpo",
            requestScopeSha256: null);

    OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        IServerOwnedDpoTrainingLeaseBroker.AcquireTranslationTrainingLease(
            DpoTrainingContentBatch trainingContent,
            string trainingRunId)
    {
        if (!IsCanonicalDpoTrainingRunId(trainingRunId))
        {
            return OperationResult<
                ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
                "A bounded canonical Translation DPO training run id is required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }

        return AcquireDeterministicDpoTrainingLease(
            trainingContent,
            requestNamespace: "translation-dpo",
            requestScopeSha256: ComputeRequestScopeSha256(trainingRunId));
    }

    OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        IServerOwnedDpoTrainingLeaseBroker.AcquireCodeDocumentationTrainingLease(
            DpoTrainingContentBatch trainingContent,
            string trainingRunId)
    {
        if (!IsCanonicalDpoTrainingRunId(trainingRunId))
        {
            return OperationResult<
                ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
                "A bounded canonical CodeDocumentation DPO training run id is required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }

        // CodeDocumentation already owns the DPO rows it wants to train on,
        // but it must still enter through this server-owned door. The runner
        // cannot safely reuse Translation's namespace or forge provenance for
        // itself; the authority derives deterministic, content-bound
        // provenance here and then uses the same revocable lease path as every
        // other admitted DPO training mutation.
        return AcquireDeterministicDpoTrainingLease(
            trainingContent,
            requestNamespace: "code-documentation-dpo",
            requestScopeSha256: ComputeRequestScopeSha256(trainingRunId));
    }

    private OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireDeterministicDpoTrainingLease(
            DpoTrainingContentBatch? trainingContent,
            string requestNamespace,
            string? requestScopeSha256)
    {
        if (trainingContent is null ||
            trainingContent.Rows.Count is <= 0 or >
                BoundedDpoTrainingContentProjector.MaximumRowCount)
        {
            return OperationResult<
                ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
                    "Exact bounded DPO training content is required.",
                    LegalTrainingTeacherAdmissionAuthority
                        .TrainingContentInvalidErrorCode);
        }

        var provenance = trainingContent.Rows
            .Select((row, ordinal) =>
                (TrainingTeacherProvenance?)
                (TrainingTeacherProvenance.Deterministic(
                    requestScopeSha256 is null
                        ? $"{requestNamespace}:{trainingContent.Sha256}:{ordinal:D4}"
                        : $"{requestNamespace}:{requestScopeSha256}:{trainingContent.Sha256}:{ordinal:D4}")
                 with
                 {
                     TrainingContentSha256 = row.Sha256
                 }))
            .ToArray();
        return AcquireTrainingLease(trainingContent, provenance);
    }

    private static bool IsCanonicalDpoTrainingRunId(string? value)
        => value is not null
           && value.Length is > 0 and <= 256
           && !string.IsNullOrWhiteSpace(value)
           && value.All(static character =>
               character is >= 'a' and <= 'z'
                   or >= 'A' and <= 'Z'
                   or >= '0' and <= '9'
                   or '-' or '_' or '.' or ':');

    private static string ComputeRequestScopeSha256(string value)
        => Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    OperationResult<ILogicalCapabilityTrainingAdmissionLease>
        ILogicalCapabilityTrainingAdmissionAuthority.AcquireTrainingLease(
            LogicalCapabilityTrainingAdmissionBatch batch)
    {
        if (batch is null ||
            batch.Rows.Count is 0 or > LogicalCapabilityTrainingAdmissionBatch.MaximumRowCount ||
            batch.Rows.Any(static row => !row.IsCanonical))
        {
            return OperationResult<ILogicalCapabilityTrainingAdmissionLease>.Fail(
                "Logical capability training admission batch is invalid.",
                "LOGICAL_CAPABILITY_TRAINING_ADMISSION_BATCH_INVALID");
        }

        if (batch.Rows.Any(static row =>
                row.TrainingContentSourceKind ==
                    LogicalTrainingContentSourceKind.Unattested))
        {
            return OperationResult<ILogicalCapabilityTrainingAdmissionLease>.Fail(
                "Typed provenance for every logical training row is required.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceRequiredErrorCode);
        }
        if (batch.Rows.Any(static row =>
                !row.HasCanonicalTrainingContentProvenance))
        {
            return OperationResult<ILogicalCapabilityTrainingAdmissionLease>.Fail(
                "Logical training row provenance is invalid.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);
        }
        if (!batch.IsTrustedConstruction)
        {
            return OperationResult<ILogicalCapabilityTrainingAdmissionLease>.Fail(
                "Logical training row provenance is invalid.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);
        }

        var verifyReplay = _verifyReplay;
        foreach (var row in batch.Rows)
        {
            if (row.RowKind !=
                LogicalCapabilityTrainingAdmissionRowKind.Replay)
            {
                continue;
            }

            var attestation = row.ReplayAttestation;
            if (attestation is null || verifyReplay is null)
            {
                return OperationResult<
                    ILogicalCapabilityTrainingAdmissionLease>.Fail(
                    "Logical capability replay attestation is invalid.",
                    LogicalCapabilityTrainingAdmissionBatch
                        .ProvenanceInvalidErrorCode);
            }

            OperationResult<bool> verified;
            try
            {
                verified = verifyReplay(attestation);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<
                    ILogicalCapabilityTrainingAdmissionLease>.Fail(
                    "Logical capability replay attestation is invalid.",
                    LogicalCapabilityTrainingAdmissionBatch
                        .ProvenanceInvalidErrorCode);
            }
            if (!verified.IsSuccess || !verified.Value)
            {
                return OperationResult<
                    ILogicalCapabilityTrainingAdmissionLease>.Fail(
                    verified.ErrorMessage,
                    verified.ErrorCode);
            }
        }

        var provenance = batch.Rows
            .Select(static row => row.TrainingContentSourceKind switch
            {
                LogicalTrainingContentSourceKind
                        .DeterministicSyntheticNoExternalTeacher =>
                    TrainingTeacherProvenance.Deterministic(row.Sha256),
                _ => null
            })
            .Cast<TrainingTeacherProvenance?>()
            .ToArray();
        if (provenance.Any(static row => row is null))
        {
            return OperationResult<ILogicalCapabilityTrainingAdmissionLease>.Fail(
                "Typed provenance for every logical training row is required.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);
        }
        var derivedAuthorizations = new Dictionary<string,
            LogicalCapabilityTrainingReplayAuthorization>(
            StringComparer.Ordinal);
        foreach (var group in batch.Rows
                     .Where(static row => row.RowKind ==
                         LogicalCapabilityTrainingAdmissionRowKind.Current)
                     .GroupBy(static row => row.ReplaySealGroupSha256,
                         StringComparer.Ordinal))
        {
            if (!LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
                    group.Key))
                return InvalidLogicalTrainingProvenance();
            var exactBindings = new List<string>();
            foreach (var row in group)
            {
                if (row.SourceBindings is not { IsCanonical: true } bindings ||
                    bindings.Values.Count != 1)
                {
                    return InvalidLogicalTrainingProvenance();
                }
                exactBindings.Add(bindings.Values[0]);
            }
            var derived = new LogicalCapabilityTrainingSourceBindingComposite(
                exactBindings
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(static value => value, StringComparer.Ordinal)
                    .ToArray());
            if (!derived.IsCanonical)
                return InvalidLogicalTrainingProvenance();
            var authorizations = group
                .Select(static row => row.AuthorizedReplay)
                .ToArray();
            if (authorizations.Any(static authorization =>
                    authorization is null || !authorization.IsCanonical) ||
                authorizations[0] is not { } authorization ||
                !StringComparer.Ordinal.Equals(
                    authorization.ReplaySealGroupSha256,
                    group.Key) ||
                !authorization.SourceBindings.Equals(derived) ||
                authorizations.Skip(1).Any(candidate =>
                    candidate != authorization) ||
                !derivedAuthorizations.TryAdd(group.Key!, authorization))
            {
                return InvalidLogicalTrainingProvenance();
            }
        }
        var lease = AcquireTrainingLease(
            provenance,
            derivedAuthorizations);
        return lease.IsSuccess && lease.Value is not null
            ? OperationResult<ILogicalCapabilityTrainingAdmissionLease>.Ok(lease.Value)
            : lease.MapError<ILogicalCapabilityTrainingAdmissionLease>();
    }

    private static OperationResult<ILogicalCapabilityTrainingAdmissionLease>
        InvalidLogicalTrainingProvenance()
        => OperationResult<ILogicalCapabilityTrainingAdmissionLease>.Fail(
            "Logical training row provenance is invalid.",
            LogicalCapabilityTrainingAdmissionBatch.ProvenanceInvalidErrorCode);

    internal Task BeginRevocation()
    {
        TaskCompletionSource<object?>? completeOutsideGate = null;
        Task drainTask;
        lock (_state.Gate)
        {
            if (!_state.Revoked)
            {
                _state.Revoked = true;
                _state.Epoch = unchecked(_state.Epoch + 1);
            }

            _state.Drain ??= new TaskCompletionSource<object?>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            if (_state.ActivePermitCount == 0)
                completeOutsideGate = _state.Drain;
            drainTask = _state.Drain.Task;
        }

        completeOutsideGate?.TrySetResult(null);
        return drainTask;
    }

    internal static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        IssueFromAdmittedConfiguration(
            ContinuousLearningTp1DpoConfiguration configuration,
            Func<bool> isAdmissionLive)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(isAdmissionLive);
        var authority = configuration.CreateTeacherPolicyBudgetAuthority();
        return authority.IsSuccess && authority.Value is not null
            ? Issue(
                authority.Value.ValidateBatch,
                isAdmissionLive,
                concurrencyObserverForTesting: null,
                sealReplay: authority.Value.SealLogicalCapabilityReplay,
                verifyReplay: authority.Value.VerifyLogicalCapabilityReplay,
                sealExecutableManifest:
                    authority.Value.SealLogicalCapabilityExecutableManifest,
                verifyExecutableManifest:
                    authority.Value.VerifyLogicalCapabilityExecutableManifest,
                validateBoundBatch: authority.Value.ValidateBatch)
            : Unavailable();
    }

    internal static ServerOwnedLegalTrainingTeacherAdmissionAuthority Issue(
        Func<
            IEnumerable<TrainingTeacherProvenance?>?,
            OperationResult<LegalTrainingTeacherAdmission>> validateBatch,
        Func<bool> isAdmissionLive,
        IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting?
            concurrencyObserverForTesting = null,
        Func<
            LogicalCapabilityTrainingReplayDescriptor,
            OperationResult<LogicalCapabilityTrainingReplayAttestation>>?
            sealReplay = null,
        Func<
            LogicalCapabilityTrainingReplayAttestation,
            OperationResult<bool>>? verifyReplay = null,
        Func<
            string,
            LogicalCapabilityTrainingReplayAttestation,
            OperationResult<LogicalCapabilityExecutableManifestAttestation>>?
            sealExecutableManifest = null,
        Func<
            string,
            LogicalCapabilityTrainingReplayAttestation,
            LogicalCapabilityExecutableManifestAttestation,
            OperationResult<bool>>? verifyExecutableManifest = null)
        => Issue(
            validateBatch,
            isAdmissionLive,
            concurrencyObserverForTesting,
            sealReplay,
            verifyReplay,
            sealExecutableManifest,
            verifyExecutableManifest,
            validateBoundBatch: null);

    internal static ServerOwnedLegalTrainingTeacherAdmissionAuthority Issue(
        Func<
            IEnumerable<TrainingTeacherProvenance?>?,
            OperationResult<LegalTrainingTeacherAdmission>> validateBatch,
        Func<bool> isAdmissionLive,
        IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting?
            concurrencyObserverForTesting,
        Func<
            LogicalCapabilityTrainingReplayDescriptor,
            OperationResult<LogicalCapabilityTrainingReplayAttestation>>?
            sealReplay,
        Func<
            LogicalCapabilityTrainingReplayAttestation,
            OperationResult<bool>>? verifyReplay,
        Func<
            string,
            LogicalCapabilityTrainingReplayAttestation,
            OperationResult<LogicalCapabilityExecutableManifestAttestation>>?
            sealExecutableManifest,
        Func<
            string,
            LogicalCapabilityTrainingReplayAttestation,
            LogicalCapabilityExecutableManifestAttestation,
            OperationResult<bool>>? verifyExecutableManifest,
        Func<
            DpoTrainingContentBatch?,
            IEnumerable<TrainingTeacherProvenance?>?,
            OperationResult<LegalTrainingTeacherAdmission>>?
            validateBoundBatch)
    {
        ArgumentNullException.ThrowIfNull(validateBatch);
        ArgumentNullException.ThrowIfNull(isAdmissionLive);
        return new ServerOwnedLegalTrainingTeacherAdmissionAuthority(
            validateBatch,
            validateBoundBatch,
            isAdmissionLive,
            sealReplay,
            verifyReplay,
            sealExecutableManifest,
            verifyExecutableManifest,
            concurrencyObserverForTesting);
    }

    internal static ServerOwnedLegalTrainingTeacherAdmissionAuthority Unavailable()
        => new(
            validateBatch: null,
            validateBoundBatch: null,
            isAdmissionLive: null,
            sealReplay: null,
            verifyReplay: null,
            sealExecutableManifest: null,
            verifyExecutableManifest: null,
            concurrencyObserverForTesting: null);

    internal bool IsAvailable =>
        _validateBatch is not null && _isAdmissionLive is not null;

    private OperationResult<LogicalCapabilityTrainingReplayAttestation>
        SealReplay(
            long leaseEpoch,
            ServerOwnedLegalTrainingTeacherAdmissionLease.Lifetime lifetime,
            IReadOnlyDictionary<string,
                LogicalCapabilityTrainingReplayAuthorization>?
                allowedReplayAuthorizations,
            LogicalCapabilityTrainingReplayDescriptor descriptor)
    {
        if (descriptor is null || !descriptor.IsCanonical ||
            allowedReplayAuthorizations is null ||
            !allowedReplayAuthorizations.TryGetValue(
                descriptor.ReplaySealGroupSha256,
                out var authorization) ||
            !authorization.Matches(descriptor))
        {
            return OperationResult<
                LogicalCapabilityTrainingReplayAttestation>.Fail(
                "Logical capability replay descriptor is invalid.",
                LogicalCapabilityTrainingAdmissionBatch
                .ProvenanceInvalidErrorCode);
    }

        if (Volatile.Read(ref lifetime.Disposed) != 0)
            return RevokedReplaySeal();

        long sampledEpoch;
        lock (_state.Gate)
        {
            if (_state.Revoked || _state.Epoch != leaseEpoch ||
                Volatile.Read(ref lifetime.Disposed) != 0)
            {
                return RevokedReplaySeal();
            }
            sampledEpoch = _state.Epoch;
        }

        var isAdmissionLive = _isAdmissionLive;
        var sealReplay = _sealReplay;
        if (isAdmissionLive is null || sealReplay is null)
        {
            return OperationResult<
                LogicalCapabilityTrainingReplayAttestation>.Fail(
                "Logical capability replay sealing is unavailable.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ReplaySealUnavailableErrorCode);
        }

        OperationResult<LogicalCapabilityTrainingReplayAttestation> sealedReplay;
        try
        {
            if (!isAdmissionLive())
                return RevokedReplaySeal();
            sealedReplay = sealReplay(descriptor);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                LogicalCapabilityTrainingReplayAttestation>.Fail(
                "Logical capability replay sealing is unavailable.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ReplaySealUnavailableErrorCode);
        }

        lock (_state.Gate)
        {
            if (_state.Revoked || _state.Epoch != sampledEpoch ||
                sampledEpoch != leaseEpoch ||
                Volatile.Read(ref lifetime.Disposed) != 0)
            {
                return RevokedReplaySeal();
            }
        }
        return sealedReplay.IsSuccess && sealedReplay.Value is not null &&
               sealedReplay.Value.IsCanonical &&
               sealedReplay.Value.Matches(descriptor)
            ? sealedReplay
            : OperationResult<
                LogicalCapabilityTrainingReplayAttestation>.Fail(
                "Logical capability replay attestation is invalid.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);
    }

    private OperationResult<
        LogicalCapabilityExecutableManifestAttestation>
        SealExecutableManifest(
            long leaseEpoch,
            ServerOwnedLegalTrainingTeacherAdmissionLease.Lifetime lifetime,
            IReadOnlyDictionary<string,
                LogicalCapabilityTrainingReplayAuthorization>?
                allowedReplayAuthorizations,
            string executableManifestSha256,
            LogicalCapabilityTrainingReplayAttestation replayAttestation)
    {
        if (!LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
                executableManifestSha256) ||
            replayAttestation is not { IsCanonical: true } ||
            allowedReplayAuthorizations is null ||
            !allowedReplayAuthorizations.TryGetValue(
                replayAttestation.ReplaySealGroupSha256,
                out var authorization) ||
            !authorization.Matches(replayAttestation.Descriptor))
        {
            return InvalidExecutableManifestAttestation();
        }
        if (Volatile.Read(ref lifetime.Disposed) != 0)
            return RevokedExecutableManifestSeal();

        long sampledEpoch;
        lock (_state.Gate)
        {
            if (_state.Revoked || _state.Epoch != leaseEpoch ||
                Volatile.Read(ref lifetime.Disposed) != 0)
            {
                return RevokedExecutableManifestSeal();
            }
            sampledEpoch = _state.Epoch;
        }

        var isAdmissionLive = _isAdmissionLive;
        var sealExecutableManifest = _sealExecutableManifest;
        var verifyReplay = _verifyReplay;
        if (isAdmissionLive is null || sealExecutableManifest is null ||
            verifyReplay is null)
            return InvalidExecutableManifestAttestation();

        OperationResult<LogicalCapabilityExecutableManifestAttestation>
            sealedAttestation;
        try
        {
            if (!isAdmissionLive())
                return RevokedExecutableManifestSeal();
            var replayVerification = verifyReplay(replayAttestation);
            if (!replayVerification.IsSuccess || !replayVerification.Value)
            {
                return replayVerification.MapError<
                    LogicalCapabilityExecutableManifestAttestation>();
            }
            sealedAttestation = sealExecutableManifest(
                executableManifestSha256,
                replayAttestation);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidExecutableManifestAttestation();
        }

        lock (_state.Gate)
        {
            if (_state.Revoked || _state.Epoch != sampledEpoch ||
                sampledEpoch != leaseEpoch ||
                Volatile.Read(ref lifetime.Disposed) != 0)
            {
                return RevokedExecutableManifestSeal();
            }
        }
        return sealedAttestation.IsSuccess &&
               sealedAttestation.Value is { IsCanonical: true } sealedValue &&
               sealedValue.Matches(
                   executableManifestSha256,
                   replayAttestation)
            ? sealedAttestation
            : InvalidExecutableManifestAttestation();
    }

    private static OperationResult<
        LogicalCapabilityExecutableManifestAttestation>
        InvalidExecutableManifestAttestation()
        => OperationResult<
            LogicalCapabilityExecutableManifestAttestation>.Fail(
                "Logical capability executable manifest attestation is invalid.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);

    private static OperationResult<bool>
        InvalidExecutableManifestVerification()
        => OperationResult<bool>.Fail(
            "Logical capability executable manifest attestation is invalid.",
            LogicalCapabilityTrainingAdmissionBatch
                .ProvenanceInvalidErrorCode);

    private static OperationResult<
        LogicalCapabilityExecutableManifestAttestation>
        RevokedExecutableManifestSeal()
        => OperationResult<
            LogicalCapabilityExecutableManifestAttestation>.Fail(
                AdmissionRevokedSafeMessage,
                AdmissionRevokedErrorCode);

    private static OperationResult<
        LogicalCapabilityTrainingReplayAttestation> RevokedReplaySeal()
        => OperationResult<LogicalCapabilityTrainingReplayAttestation>.Fail(
            AdmissionRevokedSafeMessage,
            AdmissionRevokedErrorCode);

    private static OperationResult<bool> RevokedReplayVerification()
        => OperationResult<bool>.Fail(
            AdmissionRevokedSafeMessage,
            AdmissionRevokedErrorCode);

    internal static OperationResult<IDisposable> RevokedPermit()
        => OperationResult<IDisposable>.Fail(
            AdmissionRevokedSafeMessage,
            AdmissionRevokedErrorCode);

    private OperationResult<IDisposable> AcquireMutationPermit(
        long leaseEpoch,
        ServerOwnedLegalTrainingTeacherAdmissionLease.Lifetime lifetime)
    {
        if (Volatile.Read(ref lifetime.Disposed) != 0)
            return RevokedPermit();

        long sampledEpoch;
        lock (_state.Gate)
        {
            if (_state.Revoked
                || _state.Epoch != leaseEpoch
                || Volatile.Read(ref lifetime.Disposed) != 0)
            {
                return RevokedPermit();
            }
            sampledEpoch = _state.Epoch;
        }

        var isAdmissionLive = _isAdmissionLive;
        if (isAdmissionLive is null)
            return RevokedPermit();

        bool isLive;
        try
        {
            isLive = isAdmissionLive();
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return RevokedPermit();
        }

        if (!isLive || Volatile.Read(ref lifetime.Disposed) != 0)
            return RevokedPermit();

        lock (_state.Gate)
        {
            if (_state.Revoked
                || _state.Epoch != sampledEpoch
                || sampledEpoch != leaseEpoch
                || Volatile.Read(ref lifetime.Disposed) != 0)
            {
                return RevokedPermit();
            }

            _concurrencyObserverForTesting?
                .OnFinalMutationPermitGrantUnderGateForTesting();
            if (_state.Revoked
                || _state.Epoch != sampledEpoch
                || sampledEpoch != leaseEpoch
                || Volatile.Read(ref lifetime.Disposed) != 0)
            {
                return RevokedPermit();
            }

            _state.ActivePermitCount++;
            _concurrencyObserverForTesting?
                .OnMutationPermitActiveCountChangedForTesting(
                    _state.ActivePermitCount);
            return OperationResult<IDisposable>.Ok(
                new MutationPermit(_state, _concurrencyObserverForTesting));
        }
    }

    private bool IsLeaseLive(
        long leaseEpoch,
        ServerOwnedLegalTrainingTeacherAdmissionLease.Lifetime lifetime)
    {
        if (Volatile.Read(ref lifetime.Disposed) != 0)
            return false;

        lock (_state.Gate)
        {
            return !_state.Revoked
                && _state.Epoch == leaseEpoch
                && Volatile.Read(ref lifetime.Disposed) == 0;
        }
    }

    private static OperationResult<LegalTrainingTeacherAdmission>
        ConfigurationRequired()
        => OperationResult<LegalTrainingTeacherAdmission>.Fail(
            ConfigurationRequiredSafeMessage,
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);

    private static OperationResult<LegalTrainingTeacherAdmission>
        RevokedAdmission()
        => OperationResult<LegalTrainingTeacherAdmission>.Fail(
            AdmissionRevokedSafeMessage,
            AdmissionRevokedErrorCode);

    private static OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        RevokedLease()
        => OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
            AdmissionRevokedSafeMessage,
            AdmissionRevokedErrorCode);

    private static void ReleaseMutationPermit(
        RevocationState state,
        IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting?
            concurrencyObserverForTesting)
    {
        TaskCompletionSource<object?>? completeOutsideGate = null;
        var activePermitCount = 0;
        lock (state.Gate)
        {
            if (state.ActivePermitCount <= 0)
                return;

            state.ActivePermitCount--;
            activePermitCount = state.ActivePermitCount;
            if (state.Revoked && state.ActivePermitCount == 0)
                completeOutsideGate = state.Drain;
        }

        concurrencyObserverForTesting?
            .OnMutationPermitActiveCountChangedForTesting(activePermitCount);
        completeOutsideGate?.TrySetResult(null);
    }

    private void DisposeLease(
        ServerOwnedLegalTrainingTeacherAdmissionLease.Lifetime lifetime)
    {
        _concurrencyObserverForTesting?
            .OnLeaseDisposeAttemptedBeforeGateForTesting();
        lock (_state.Gate)
        {
            if (lifetime.Disposed == 0)
                Volatile.Write(ref lifetime.Disposed, 1);
        }
    }

    private sealed class MutationPermit : IDisposable
    {
        private RevocationState? _state;
        private readonly IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting?
            _concurrencyObserverForTesting;

        internal MutationPermit(
            RevocationState state,
            IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting?
                concurrencyObserverForTesting)
        {
            _state = state;
            _concurrencyObserverForTesting = concurrencyObserverForTesting;
        }

        public void Dispose()
        {
            var state = Interlocked.Exchange(ref _state, null);
            if (state is not null)
            {
                ReleaseMutationPermit(
                    state,
                    _concurrencyObserverForTesting);
            }
        }
    }

    private sealed class RevocationState
    {
        internal object Gate { get; } = new();
        internal Dictionary<string, string> ConsumedTeacherRequestIds { get; } =
            new(StringComparer.Ordinal);
        internal long Epoch { get; set; }
        internal bool Revoked { get; set; }
        internal int ActivePermitCount { get; set; }
        internal TaskCompletionSource<object?>? Drain { get; set; }
    }
}
