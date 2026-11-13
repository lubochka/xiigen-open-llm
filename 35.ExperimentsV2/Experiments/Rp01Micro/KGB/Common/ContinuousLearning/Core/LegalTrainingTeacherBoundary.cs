// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public enum TrainingTeacherSourceKind
{
    ExternalTeacherOutput,
    DeterministicNoExternalTeacher
}

public enum OpenWeightTeacherFamily
{
    Qwen,
    DeepSeek,
    Kimi,
    Glm,
    Mistral
}

public enum TrainingTeacherAccessMode
{
    LocalHostedOpenWeight,
    OnlineHostedOpenWeight
}

public enum TrainingTeacherHostingProvider
{
    Ollama,
    DashScope,
    DeepSeek,
    Moonshot,
    Zai,
    Mistral,
    Together,
    Fireworks,
    OpenRouter,
    Novita,
    HuggingFace
}

public enum TrainingTeacherModelLicense
{
    Apache20,
    MIT,
    ModifiedMIT
}

public sealed record TrainingTeacherProvenance(
    TrainingTeacherSourceKind SourceKind,
    OpenWeightTeacherFamily? Family,
    TrainingTeacherAccessMode? AccessMode,
    TrainingTeacherHostingProvider? HostingProvider,
    string ModelId,
    string ModelVersion,
    TrainingTeacherModelLicense? ModelLicense,
    bool ProviderTermsChecked,
    bool ProviderTermsAllowTraining,
    bool ClosedProprietaryModel,
    bool TeacherOutputUsedForTraining,
    string RequestId,
    int InvocationCount)
{
    public string ModelArtifactDigestKind { get; init; } = string.Empty;

    public string ModelArtifactDigest { get; init; } = string.Empty;

    /// <summary>
    /// Exact neutral-projector row digest. Metadata-only preflight provenance
    /// leaves this empty; every bound mutation admission requires an exact
    /// match against content recomputed by the server boundary.
    /// </summary>
    [System.Text.Json.Serialization.JsonInclude]
    public string TrainingContentSha256 { get; internal init; } = string.Empty;

    public static TrainingTeacherProvenance Deterministic(string requestId)
        => new(
            TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
            Family: null,
            AccessMode: null,
            HostingProvider: null,
            ModelId: string.Empty,
            ModelVersion: string.Empty,
            ModelLicense: null,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: false,
            requestId,
            InvocationCount: 0);
}

public sealed record LegalTrainingTeacherPolicyEntry(
    OpenWeightTeacherFamily Family,
    TrainingTeacherAccessMode AccessMode,
    TrainingTeacherHostingProvider HostingProvider,
    string ModelId,
    string ModelVersion,
    TrainingTeacherModelLicense ModelLicense,
    string LocalRetentionPolicyRef,
    string OutputLicenseRef)
{
    public string ModelArtifactDigestKind { get; init; } = string.Empty;

    public string ModelArtifactDigest { get; init; } = string.Empty;

    public LegalTrainingTeacherPolicyEntry(
        OpenWeightTeacherFamily Family,
        TrainingTeacherAccessMode AccessMode,
        TrainingTeacherHostingProvider HostingProvider,
        string ModelId,
        string ModelVersion,
        TrainingTeacherModelLicense ModelLicense)
        : this(
            Family,
            AccessMode,
            HostingProvider,
            ModelId,
            ModelVersion,
            ModelLicense,
            LocalRetentionPolicyRef: string.Empty,
            OutputLicenseRef: string.Empty)
    {
    }

    public void Deconstruct(
        out OpenWeightTeacherFamily Family,
        out TrainingTeacherAccessMode AccessMode,
        out TrainingTeacherHostingProvider HostingProvider,
        out string ModelId,
        out string ModelVersion,
        out TrainingTeacherModelLicense ModelLicense)
    {
        Family = this.Family;
        AccessMode = this.AccessMode;
        HostingProvider = this.HostingProvider;
        ModelId = this.ModelId;
        ModelVersion = this.ModelVersion;
        ModelLicense = this.ModelLicense;
    }
}

public sealed record LegalTrainingTeacherAdmissionReceipt(
    TrainingTeacherSourceKind SourceKind,
    OpenWeightTeacherFamily? Family,
    TrainingTeacherAccessMode? AccessMode,
    TrainingTeacherHostingProvider? HostingProvider,
    TrainingTeacherModelLicense? ModelLicense,
    bool ProviderTermsChecked,
    bool ProviderTermsAllowTraining,
    bool ClosedProprietaryModel,
    bool TeacherOutputUsedForTraining,
    int InvocationCount,
    string RequestRef,
    string ProviderRef,
    string ModelRef,
    string ModelVersionRef,
    string LicenseRef,
    string? LegalEvidenceMode,
    string? LocalRetentionPolicyRef,
    string? OutputLicenseRef)
{
    public string ModelArtifactDigestKind { get; init; } = string.Empty;

    public string ModelArtifactRef { get; init; } = string.Empty;

    public string TrainingContentRef { get; init; } = string.Empty;

    public LegalTrainingTeacherAdmissionReceipt(
        TrainingTeacherSourceKind SourceKind,
        OpenWeightTeacherFamily? Family,
        TrainingTeacherAccessMode? AccessMode,
        TrainingTeacherHostingProvider? HostingProvider,
        TrainingTeacherModelLicense? ModelLicense,
        bool ProviderTermsChecked,
        bool ProviderTermsAllowTraining,
        bool ClosedProprietaryModel,
        bool TeacherOutputUsedForTraining,
        int InvocationCount,
        string RequestRef,
        string ProviderRef,
        string ModelRef,
        string ModelVersionRef,
        string LicenseRef)
        : this(
            SourceKind,
            Family,
            AccessMode,
            HostingProvider,
            ModelLicense,
            ProviderTermsChecked,
            ProviderTermsAllowTraining,
            ClosedProprietaryModel,
            TeacherOutputUsedForTraining,
            InvocationCount,
            RequestRef,
            ProviderRef,
            ModelRef,
            ModelVersionRef,
            LicenseRef,
            LegalEvidenceMode: null,
            LocalRetentionPolicyRef: null,
            OutputLicenseRef: null)
    {
    }

    public void Deconstruct(
        out TrainingTeacherSourceKind SourceKind,
        out OpenWeightTeacherFamily? Family,
        out TrainingTeacherAccessMode? AccessMode,
        out TrainingTeacherHostingProvider? HostingProvider,
        out TrainingTeacherModelLicense? ModelLicense,
        out bool ProviderTermsChecked,
        out bool ProviderTermsAllowTraining,
        out bool ClosedProprietaryModel,
        out bool TeacherOutputUsedForTraining,
        out int InvocationCount,
        out string RequestRef,
        out string ProviderRef,
        out string ModelRef,
        out string ModelVersionRef,
        out string LicenseRef)
    {
        SourceKind = this.SourceKind;
        Family = this.Family;
        AccessMode = this.AccessMode;
        HostingProvider = this.HostingProvider;
        ModelLicense = this.ModelLicense;
        ProviderTermsChecked = this.ProviderTermsChecked;
        ProviderTermsAllowTraining = this.ProviderTermsAllowTraining;
        ClosedProprietaryModel = this.ClosedProprietaryModel;
        TeacherOutputUsedForTraining = this.TeacherOutputUsedForTraining;
        InvocationCount = this.InvocationCount;
        RequestRef = this.RequestRef;
        ProviderRef = this.ProviderRef;
        ModelRef = this.ModelRef;
        ModelVersionRef = this.ModelVersionRef;
        LicenseRef = this.LicenseRef;
    }
}

public sealed record LegalTrainingTeacherAdmission(
    IReadOnlyList<LegalTrainingTeacherAdmissionReceipt> Receipts,
    int ExternalTeacherInvocationCount,
    string PolicyIdentityRef,
    IReadOnlyList<string> EvidenceRefs)
{
    public string TrainingContentBatchRef { get; init; } = string.Empty;
}

/// <summary>
/// Teacher policy, call-budget, and provenance admission authority. It never
/// issues DPO mutation leases; see
/// <see cref="ServerOwnedLegalTrainingTeacherAdmissionAuthority"/> for the
/// server-owned DPO training lease and revocable mutation-admission role.
/// </summary>
public sealed class LegalTrainingTeacherAdmissionAuthority
{
    public const string AuthorityRole =
        "teacher-policy-call-budget-provenance-admission-never-dpo-mutation-or-lease-issuance";
    public const int MaxBatchRows = 4_096;
    public const int MaxAllowlistEntries = 64;
    public const int HardMaximumTeacherCalls = 10_000;
    public const string SelfHostedNoProviderTermsEvidenceMode =
        "SELF_HOSTED_NO_PROVIDER_TERMS";
    public const string HostedProviderTermsEvidenceMode =
        "HOSTED_PROVIDER_TERMS";

    public const string ConfigurationRequiredErrorCode = "LEGAL_TEACHER_CONFIGURATION_REQUIRED";
    public const string AllowlistLimitExceededErrorCode = "LEGAL_TEACHER_ALLOWLIST_LIMIT_EXCEEDED";
    public const string BatchLimitExceededErrorCode = "LEGAL_TEACHER_BATCH_LIMIT_EXCEEDED";
    public const string EnumerationFailedErrorCode = "LEGAL_TEACHER_ENUMERATION_FAILED";
    public const string ProvenanceRequiredErrorCode = "LEGAL_TEACHER_PROVENANCE_REQUIRED";
    public const string ProvenanceInvalidErrorCode = "LEGAL_TEACHER_PROVENANCE_INVALID";
    public const string ClosedProprietaryDeniedErrorCode = "LEGAL_TEACHER_CLOSED_PROPRIETARY_DENIED";
    public const string TermsNotApprovedErrorCode = "LEGAL_TEACHER_TERMS_NOT_APPROVED";
    public const string NotAllowlistedErrorCode = "LEGAL_TEACHER_NOT_ALLOWLISTED";
    public const string CallBudgetExceededErrorCode = "LEGAL_TEACHER_CALL_BUDGET_EXCEEDED";
    public const string TrainingContentInvalidErrorCode =
        "LEGAL_TEACHER_TRAINING_CONTENT_INVALID";
    public const string DuplicateRequestIdErrorCode =
        "LEGAL_TEACHER_REQUEST_ID_REUSED";

    private const int MinimumAuditSaltBytes = 32;
    private const int MaximumIdentityLength = 256;
    public const string OllamaModelLayerSha256DigestKind = "ollama_model_layer_sha256";

    private readonly ReadOnlyCollection<LegalTrainingTeacherPolicyEntry> _allowlist;
    private readonly int _maxTeacherCalls;
    private readonly byte[] _auditHashSalt;

    private LegalTrainingTeacherAdmissionAuthority(
        LegalTrainingTeacherPolicyEntry[] allowlist,
        int maxTeacherCalls,
        byte[] auditHashSalt)
    {
        _allowlist = Array.AsReadOnly(allowlist.ToArray());
        _maxTeacherCalls = maxTeacherCalls;
        _auditHashSalt = auditHashSalt.ToArray();
        PolicyIdentityRef = HashRef(
            "legal-teacher-policy",
            string.Join("\n", _allowlist.Select(CanonicalPolicyIdentity))
            + $"\nmax_teacher_calls={_maxTeacherCalls}");
    }

    public string PolicyIdentityRef { get; }

    public static OperationResult<LegalTrainingTeacherAdmissionAuthority> Create(
        IEnumerable<LegalTrainingTeacherPolicyEntry?>? allowedTeachers,
        int maxTeacherCalls,
        ReadOnlySpan<byte> auditHashSalt)
    {
        if (maxTeacherCalls < 0
            || maxTeacherCalls > HardMaximumTeacherCalls
            || auditHashSalt.Length < MinimumAuditSaltBytes)
        {
            return OperationResult<LegalTrainingTeacherAdmissionAuthority>.Fail(
                "A valid server-owned teacher call budget and audit hash salt are required.",
                ConfigurationRequiredErrorCode);
        }

        var snapshot = SnapshotBounded(
            allowedTeachers,
            MaxAllowlistEntries,
            AllowlistLimitExceededErrorCode);
        if (!snapshot.IsSuccess || snapshot.Value is null)
            return snapshot.MapError<LegalTrainingTeacherAdmissionAuthority>();

        var entries = snapshot.Value;
        if (entries.Any(static entry => entry is null))
        {
            return OperationResult<LegalTrainingTeacherAdmissionAuthority>.Fail(
                "The server-owned teacher allowlist contains an invalid entry.",
                ConfigurationRequiredErrorCode);
        }

        var exactEntries = entries.Select(static entry => entry!).ToArray();
        if (exactEntries.Any(static entry => !IsValidPolicyEntry(entry))
            || exactEntries
                .Select(CanonicalPolicyBaseIdentity)
                .Distinct(StringComparer.Ordinal)
                .Count() != exactEntries.Length)
        {
            return OperationResult<LegalTrainingTeacherAdmissionAuthority>.Fail(
                "The server-owned teacher allowlist contains an invalid or duplicate entry.",
                ConfigurationRequiredErrorCode);
        }

        return OperationResult<LegalTrainingTeacherAdmissionAuthority>.Ok(
            new LegalTrainingTeacherAdmissionAuthority(
                exactEntries,
                maxTeacherCalls,
                auditHashSalt.ToArray()));
    }

    /// <summary>
    /// Admits one exact allowlisted local open-weight teacher observation for
    /// evaluation only. This boundary never converts evaluation provenance into
    /// training provenance and does not create a training admission receipt.
    /// </summary>
    public OperationResult<bool> ValidateLocalEvaluationOutput(
        TrainingTeacherProvenance? provenance)
    {
        if (provenance is null)
        {
            return OperationResult<bool>.Fail(
                "Typed teacher provenance is required for the evaluation output.",
                ProvenanceRequiredErrorCode);
        }

        if (!Enum.IsDefined(provenance.SourceKind)
            || provenance.SourceKind != TrainingTeacherSourceKind.ExternalTeacherOutput
            || provenance.Family is null
            || provenance.AccessMode is null
            || provenance.HostingProvider is null
            || provenance.ModelLicense is null
            || !Enum.IsDefined(provenance.Family.Value)
            || !Enum.IsDefined(provenance.AccessMode.Value)
            || !Enum.IsDefined(provenance.HostingProvider.Value)
            || !Enum.IsDefined(provenance.ModelLicense.Value)
            || !IsSafeIdentity(provenance.RequestId)
            || !IsSafeIdentity(provenance.ModelId)
            || !IsSafeIdentity(provenance.ModelVersion)
            || !IsValidArtifactIdentity(provenance)
            || provenance.InvocationCount <= 0)
        {
            return OperationResult<bool>.Fail(
                "Evaluation teacher provenance is incomplete or invalid.",
                ProvenanceInvalidErrorCode);
        }

        if (provenance.InvocationCount > _maxTeacherCalls)
        {
            return OperationResult<bool>.Fail(
                "Teacher invocation count exceeds the configured evaluation budget.",
                CallBudgetExceededErrorCode);
        }

        if (provenance.ClosedProprietaryModel
            || ContainsClosedIdentity(provenance.ModelId)
            || ContainsClosedIdentity(provenance.ModelVersion))
        {
            return OperationResult<bool>.Fail(
                "Closed proprietary models are not permitted as evaluation teachers.",
                ClosedProprietaryDeniedErrorCode);
        }

        var matchedPolicy = _allowlist.SingleOrDefault(entry =>
            PolicyBaseIdentityMatches(entry, provenance));
        if (matchedPolicy is null
            || matchedPolicy.AccessMode != TrainingTeacherAccessMode.LocalHostedOpenWeight
            || matchedPolicy.HostingProvider != TrainingTeacherHostingProvider.Ollama)
        {
            return OperationResult<bool>.Fail(
                "The evaluation teacher is not an exact configured local open-weight identity.",
                NotAllowlistedErrorCode);
        }

        if (provenance.ProviderTermsChecked
            || provenance.ProviderTermsAllowTraining
            || provenance.TeacherOutputUsedForTraining)
        {
            return OperationResult<bool>.Fail(
                "Evaluation output must retain explicit self-hosted, nontraining semantics.",
                TermsNotApprovedErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<LegalTrainingTeacherAdmission> ValidateBatch(
        IEnumerable<TrainingTeacherProvenance?>? provenance)
    {
        var snapshot = SnapshotBounded(
            provenance,
            MaxBatchRows,
            BatchLimitExceededErrorCode);
        if (!snapshot.IsSuccess || snapshot.Value is null)
            return snapshot.MapError<LegalTrainingTeacherAdmission>();

        var rows = snapshot.Value;
        if (rows.Count == 0 || rows.Any(static row => row is null))
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "Typed teacher provenance is required for every training record.",
                ProvenanceRequiredErrorCode);
        }

        var exactRows = rows.Select(static row => row!).ToArray();
        return ValidateBatchCore(exactRows, trainingContent: null);
    }

    /// <summary>
    /// Validates provenance against the exact immutable DPO content that will
    /// be consumed. The authority recomputes audit references from the bounded
    /// projection; no caller-supplied digest claim is accepted.
    /// </summary>
    public OperationResult<LegalTrainingTeacherAdmission> ValidateBatch(
        DpoTrainingContentBatch? trainingContent,
        IEnumerable<TrainingTeacherProvenance?>? provenance)
    {
        if (trainingContent is null ||
            trainingContent.Rows.Count is <= 0 or > MaxBatchRows ||
            !IsCanonicalSha256Digest(trainingContent.Sha256) ||
            trainingContent.Rows.Any(static row =>
                row is null || !IsCanonicalSha256Digest(row.Sha256)))
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "Exact bounded DPO training content is required.",
                TrainingContentInvalidErrorCode);
        }

        var snapshot = SnapshotBounded(
            provenance,
            MaxBatchRows,
            BatchLimitExceededErrorCode);
        if (!snapshot.IsSuccess || snapshot.Value is null)
            return snapshot.MapError<LegalTrainingTeacherAdmission>();
        if (snapshot.Value.Count != trainingContent.Rows.Count ||
            snapshot.Value.Any(static row => row is null))
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "Every exact DPO training row requires matching provenance.",
                TrainingContentInvalidErrorCode);
        }

        for (var index = 0; index < snapshot.Value.Count; index++)
        {
            if (!StringComparer.Ordinal.Equals(
                    snapshot.Value[index]!.TrainingContentSha256,
                    trainingContent.Rows[index].Sha256))
            {
                return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                    "Teacher provenance is not bound to the exact DPO training row.",
                    TrainingContentInvalidErrorCode);
            }
        }

        return ValidateBatchCore(
            snapshot.Value.Select(static row => row!).ToArray(),
            trainingContent);
    }

    private OperationResult<LegalTrainingTeacherAdmission> ValidateBatchCore(
        TrainingTeacherProvenance[] exactRows,
        DpoTrainingContentBatch? trainingContent)
    {
        var matchedPolicies = new Dictionary<TrainingTeacherProvenance, LegalTrainingTeacherPolicyEntry?>();
        foreach (var row in exactRows)
        {
            var rowFailure = ValidateRow(row, out var matchedPolicy);
            if (rowFailure is not null)
                return rowFailure;
            matchedPolicies[row] = matchedPolicy;
        }

        var requests = exactRows
            .GroupBy(static row => row.RequestId, StringComparer.Ordinal)
            .ToArray();
        if (requests.Any(static request => request.Count() != 1))
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "A teacher request id may authorize exactly one training row.",
                DuplicateRequestIdErrorCode);
        }

        var externalRequests = requests
            .Where(static request =>
                request.First().SourceKind == TrainingTeacherSourceKind.ExternalTeacherOutput)
            .ToArray();

        int invocationCount;
        try
        {
            invocationCount = 0;
            foreach (var request in externalRequests)
                invocationCount = checked(invocationCount + request.First().InvocationCount);
        }
        catch (OverflowException)
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "Teacher invocation count exceeds the configured batch budget.",
                CallBudgetExceededErrorCode);
        }

        if (invocationCount > _maxTeacherCalls)
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "Teacher invocation count exceeds the configured batch budget.",
                CallBudgetExceededErrorCode);
        }

        var contentByRequest = trainingContent is null
            ? null
            : exactRows
                .Select((row, index) => new
                {
                    row.RequestId,
                    ContentSha256 = trainingContent.Rows[index].Sha256
                })
                .ToDictionary(
                    static entry => entry.RequestId,
                    static entry => entry.ContentSha256,
                    StringComparer.Ordinal);
        var receipts = exactRows
            .OrderBy(static row => row.RequestId, StringComparer.Ordinal)
            .Select(row => BuildReceipt(
                row,
                matchedPolicies[row],
                contentByRequest is null
                    ? null
                    : contentByRequest[row.RequestId]))
            .ToArray();
        var ownedReceipts = Array.AsReadOnly(receipts);
        var evidence = receipts
            .SelectMany(BuildEvidenceRefs)
            .Append($"legal_teacher_admission:external_invocation_count={invocationCount}")
            .Append($"legal_teacher_admission:max_teacher_calls={_maxTeacherCalls}")
            .Append($"legal_teacher_admission:policy_ref={PolicyIdentityRef}")
            .Concat(trainingContent is null
                ? []
                : [
                    $"legal_teacher_admission:training_content_batch_ref={HashRef("teacher-training-content-batch", trainingContent.Sha256)}"
                ])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return OperationResult<LegalTrainingTeacherAdmission>.Ok(
            new LegalTrainingTeacherAdmission(
                ownedReceipts,
                invocationCount,
                PolicyIdentityRef,
                Array.AsReadOnly(evidence))
            {
                TrainingContentBatchRef = trainingContent is null
                    ? string.Empty
                    : HashRef(
                        "teacher-training-content-batch",
                        trainingContent.Sha256)
            });
    }

    internal OperationResult<LogicalCapabilityTrainingReplayAttestation>
        SealLogicalCapabilityReplay(
            LogicalCapabilityTrainingReplayDescriptor? descriptor)
    {
        if (descriptor is null || !descriptor.IsCanonical)
            return InvalidReplayAttestation();

        try
        {
            var keyRef = BuildLogicalCapabilityReplayAuthorityKeyRef();
            var mac = ComputeLogicalCapabilityReplayMac(descriptor);
            return OperationResult<
                LogicalCapabilityTrainingReplayAttestation>.Ok(
                new LogicalCapabilityTrainingReplayAttestation(
                    descriptor.SchemaVersion,
                    descriptor.OriginalSourceKind,
                    descriptor.ReplaySealGroupSha256,
                    descriptor.SourceBindings.Snapshot(),
                    descriptor.ReplayPayloadSha256,
                    descriptor.DpoPreferenceSha256,
                    keyRef,
                    mac));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidReplayAttestation();
        }
    }

    internal OperationResult<bool> VerifyLogicalCapabilityReplay(
        LogicalCapabilityTrainingReplayAttestation? attestation)
    {
        if (attestation is null || !attestation.IsCanonical)
            return InvalidReplayVerification();

        try
        {
            var expectedKeyRef =
                BuildLogicalCapabilityReplayAuthorityKeyRef();
            if (!FixedTimeEqualsHex(
                    expectedKeyRef.AsSpan(7),
                    attestation.AuthorityKeyRef.AsSpan(7)))
            {
                return OperationResult<bool>.Fail(
                    "Logical capability replay authority key does not match.",
                    LogicalCapabilityTrainingAdmissionBatch
                        .ReplayKeyMismatchErrorCode);
            }

            var expectedMac = ComputeLogicalCapabilityReplayMac(
                attestation.Descriptor);
            return FixedTimeEqualsHex(expectedMac, attestation.MacSha256)
                ? OperationResult<bool>.Ok(true)
                : InvalidReplayVerification();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidReplayVerification();
        }
    }

    internal OperationResult<
        LogicalCapabilityExecutableManifestAttestation>
        SealLogicalCapabilityExecutableManifest(
            string executableManifestSha256,
            LogicalCapabilityTrainingReplayAttestation replayAttestation)
    {
        if (!LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
                executableManifestSha256) ||
            replayAttestation is not { IsCanonical: true })
        {
            return InvalidExecutableManifestAttestation();
        }

        var replayVerification =
            VerifyLogicalCapabilityReplay(replayAttestation);
        if (!replayVerification.IsSuccess || !replayVerification.Value)
            return replayVerification.MapError<
                LogicalCapabilityExecutableManifestAttestation>();

        try
        {
            var keyRef = BuildLogicalCapabilityReplayAuthorityKeyRef();
            var mac = LogicalCapabilityReplayHmacCodec
                .ComputeExecutableManifestMac(
                    _auditHashSalt,
                    executableManifestSha256,
                    replayAttestation);
            return OperationResult<
                LogicalCapabilityExecutableManifestAttestation>.Ok(
                    new LogicalCapabilityExecutableManifestAttestation(
                        LogicalCapabilityExecutableManifestAttestation
                            .CurrentSchemaVersion,
                        executableManifestSha256,
                        replayAttestation.AuthorityKeyRef,
                        replayAttestation.MacSha256,
                        keyRef,
                        mac));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidExecutableManifestAttestation();
        }
    }

    internal OperationResult<bool>
        VerifyLogicalCapabilityExecutableManifest(
            string executableManifestSha256,
            LogicalCapabilityTrainingReplayAttestation replayAttestation,
            LogicalCapabilityExecutableManifestAttestation
                executableAttestation)
    {
        if (replayAttestation is not { IsCanonical: true } ||
            executableAttestation is not { IsCanonical: true } ||
            !executableAttestation.Matches(
                executableManifestSha256,
                replayAttestation))
        {
            return InvalidReplayVerification();
        }

        var replayVerification =
            VerifyLogicalCapabilityReplay(replayAttestation);
        if (!replayVerification.IsSuccess || !replayVerification.Value)
            return replayVerification;

        try
        {
            var expectedKeyRef =
                BuildLogicalCapabilityReplayAuthorityKeyRef();
            if (!FixedTimeEqualsHex(
                    expectedKeyRef.AsSpan(7),
                    executableAttestation.AuthorityKeyRef.AsSpan(7)))
            {
                return OperationResult<bool>.Fail(
                    "Logical capability executable manifest authority key does not match.",
                    LogicalCapabilityTrainingAdmissionBatch
                        .ReplayKeyMismatchErrorCode);
            }

            var expectedMac = LogicalCapabilityReplayHmacCodec
                .ComputeExecutableManifestMac(
                    _auditHashSalt,
                    executableManifestSha256,
                    replayAttestation);
            return FixedTimeEqualsHex(
                    expectedMac,
                    executableAttestation.MacSha256)
                ? OperationResult<bool>.Ok(true)
                : InvalidReplayVerification();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidReplayVerification();
        }
    }

    private string BuildLogicalCapabilityReplayAuthorityKeyRef()
        => LogicalCapabilityReplayHmacCodec.BuildAuthorityKeyRef(
            _auditHashSalt);

    private string ComputeLogicalCapabilityReplayMac(
        LogicalCapabilityTrainingReplayDescriptor descriptor)
        => LogicalCapabilityReplayHmacCodec.ComputeReplayMac(
            _auditHashSalt,
            descriptor);

    private static bool FixedTimeEqualsHex(
        ReadOnlySpan<char> left,
        ReadOnlySpan<char> right)
        => LogicalCapabilityReplayHmacCodec.FixedTimeEqualsHex(
            left,
            right);

    private static OperationResult<
        LogicalCapabilityTrainingReplayAttestation>
        InvalidReplayAttestation()
        => OperationResult<LogicalCapabilityTrainingReplayAttestation>.Fail(
            "Logical capability replay attestation is invalid.",
            LogicalCapabilityTrainingAdmissionBatch
                .ProvenanceInvalidErrorCode);

    private static OperationResult<
        LogicalCapabilityExecutableManifestAttestation>
        InvalidExecutableManifestAttestation()
        => OperationResult<
            LogicalCapabilityExecutableManifestAttestation>.Fail(
                "Logical capability executable manifest attestation is invalid.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);

    private static OperationResult<bool> InvalidReplayVerification()
        => OperationResult<bool>.Fail(
            "Logical capability replay attestation is invalid.",
            LogicalCapabilityTrainingAdmissionBatch
                .ProvenanceInvalidErrorCode);

    private OperationResult<LegalTrainingTeacherAdmission>? ValidateRow(
        TrainingTeacherProvenance row,
        out LegalTrainingTeacherPolicyEntry? matchedPolicy)
    {
        matchedPolicy = null;
        if (!Enum.IsDefined(row.SourceKind))
            return InvalidProvenance("Teacher provenance contains an invalid source kind.");

        if (row.SourceKind == TrainingTeacherSourceKind.DeterministicNoExternalTeacher)
        {
            if (!IsExactDeterministicNoExternalTeacherProvenance(row))
            {
                return InvalidProvenance(
                    "Deterministic provenance must explicitly prove that no external teacher output was used.");
            }

            return null;
        }

        if (row.Family is null
            || row.AccessMode is null
            || row.HostingProvider is null
            || row.ModelLicense is null
            || !Enum.IsDefined(row.Family.Value)
            || !Enum.IsDefined(row.AccessMode.Value)
            || !Enum.IsDefined(row.HostingProvider.Value)
            || !Enum.IsDefined(row.ModelLicense.Value)
            || !IsSafeIdentity(row.RequestId)
            || !IsSafeIdentity(row.ModelId)
            || !IsSafeIdentity(row.ModelVersion)
            || !IsValidArtifactIdentity(row)
            || row.InvocationCount <= 0)
        {
            return InvalidProvenance("External teacher provenance is incomplete or invalid.");
        }

        if (row.ClosedProprietaryModel
            || ContainsClosedIdentity(row.ModelId)
            || ContainsClosedIdentity(row.ModelVersion))
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "Closed proprietary models are not permitted as training teachers.",
                ClosedProprietaryDeniedErrorCode);
        }

        matchedPolicy = _allowlist.SingleOrDefault(entry => PolicyBaseIdentityMatches(entry, row));
        if (matchedPolicy is null)
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "The teacher is not an exact configured open-weight, training-permitted identity.",
                NotAllowlistedErrorCode);
        }

        var isLocalSelfHosted =
            matchedPolicy.AccessMode == TrainingTeacherAccessMode.LocalHostedOpenWeight
            && matchedPolicy.HostingProvider == TrainingTeacherHostingProvider.Ollama;
        if ((!isLocalSelfHosted
             && (!row.ProviderTermsChecked || !row.ProviderTermsAllowTraining))
            || (isLocalSelfHosted
                && (row.ProviderTermsChecked || row.ProviderTermsAllowTraining))
            || !row.TeacherOutputUsedForTraining)
        {
            return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "Hosted-provider terms or explicit self-hosted semantics and training use are required.",
                TermsNotApprovedErrorCode);
        }

        return null;
    }

    internal static bool IsExactDeterministicNoExternalTeacherProvenance(
        TrainingTeacherProvenance? row)
        // Детерминированный паспорт является безопасным только как полный
        // отрицательный сертификат: ни модели, ни provider, ни артефакта, ни
        // teacher-вызова. Все хранилища должны ссылаться на этот один якорь.
        => row is
           {
               SourceKind: TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
               Family: null,
               AccessMode: null,
               HostingProvider: null,
               ModelLicense: null,
               ModelId: "",
               ModelVersion: "",
               ModelArtifactDigestKind: "",
               ModelArtifactDigest: "",
               ProviderTermsChecked: false,
               ProviderTermsAllowTraining: false,
               ClosedProprietaryModel: false,
               TeacherOutputUsedForTraining: false,
               InvocationCount: 0
           }
           && IsSafeIdentity(row.RequestId);

    private LegalTrainingTeacherAdmissionReceipt BuildReceipt(
        TrainingTeacherProvenance row,
        LegalTrainingTeacherPolicyEntry? matchedPolicy,
        string? trainingContentSha256 = null)
    {
        var providerIdentity = row.HostingProvider?.ToString() ?? "none";
        var modelIdentity = row.ModelId.Length == 0 ? "none" : row.ModelId;
        var versionIdentity = row.ModelVersion.Length == 0 ? "none" : row.ModelVersion;
        var licenseIdentity = row.ModelLicense?.ToString() ?? "none";
        return new LegalTrainingTeacherAdmissionReceipt(
            row.SourceKind,
            row.Family,
            row.AccessMode,
            row.HostingProvider,
            row.ModelLicense,
            row.ProviderTermsChecked,
            row.ProviderTermsAllowTraining,
            row.ClosedProprietaryModel,
            row.TeacherOutputUsedForTraining,
            row.InvocationCount,
            HashRef("teacher-request", row.RequestId),
            HashRef("teacher-provider", providerIdentity),
            HashRef("teacher-model", modelIdentity),
            HashRef("teacher-version", versionIdentity),
            HashRef("teacher-license", licenseIdentity),
            matchedPolicy is null
                ? null
                : matchedPolicy.AccessMode == TrainingTeacherAccessMode.LocalHostedOpenWeight
                    ? SelfHostedNoProviderTermsEvidenceMode
                    : HostedProviderTermsEvidenceMode,
            matchedPolicy?.LocalRetentionPolicyRef,
            matchedPolicy?.OutputLicenseRef)
        {
            ModelArtifactDigestKind = row.ModelArtifactDigestKind,
            ModelArtifactRef = row.ModelArtifactDigest.Length == 0
                ? string.Empty
                : HashRef("teacher-model-artifact-digest", row.ModelArtifactDigest),
            TrainingContentRef = trainingContentSha256 is null
                ? string.Empty
                : HashRef("teacher-training-content-row", trainingContentSha256)
        };
    }

    private static IEnumerable<string> BuildEvidenceRefs(
        LegalTrainingTeacherAdmissionReceipt receipt)
    {
        yield return $"teacher_provenance:source_kind={receipt.SourceKind}";
        yield return $"teacher_provenance:family={receipt.Family?.ToString() ?? "none"}";
        yield return $"teacher_provenance:access_mode={receipt.AccessMode?.ToString() ?? "none"}";
        yield return $"teacher_provenance:hosting_provider={receipt.HostingProvider?.ToString() ?? "none"}";
        yield return $"teacher_provenance:model_license={receipt.ModelLicense?.ToString() ?? "none"}";
        yield return $"teacher_provenance:provider_terms_checked={receipt.ProviderTermsChecked}";
        yield return $"teacher_provenance:provider_terms_allow_training={receipt.ProviderTermsAllowTraining}";
        yield return $"teacher_provenance:closed_proprietary_model={receipt.ClosedProprietaryModel}";
        yield return $"teacher_provenance:output_used_for_training={receipt.TeacherOutputUsedForTraining}";
        yield return $"teacher_provenance:invocation_count={receipt.InvocationCount}";
        yield return $"teacher_provenance:request_ref={receipt.RequestRef}";
        yield return $"teacher_provenance:provider_ref={receipt.ProviderRef}";
        yield return $"teacher_provenance:model_ref={receipt.ModelRef}";
        yield return $"teacher_provenance:model_version_ref={receipt.ModelVersionRef}";
        yield return $"teacher_provenance:license_ref={receipt.LicenseRef}";
        if (receipt.ModelArtifactDigestKind.Length != 0)
            yield return $"teacher_provenance:model_artifact_digest_kind={receipt.ModelArtifactDigestKind}";
        if (receipt.ModelArtifactRef.Length != 0)
            yield return $"teacher_provenance:model_artifact_ref={receipt.ModelArtifactRef}";
        if (receipt.TrainingContentRef.Length != 0)
            yield return $"teacher_provenance:training_content_ref={receipt.TrainingContentRef}";
        if (receipt.LegalEvidenceMode is not null)
            yield return $"teacher_legal_evidence:mode={receipt.LegalEvidenceMode}";
        if (receipt.LocalRetentionPolicyRef is not null)
        {
            yield return $"teacher_legal_evidence:local_retention_policy_ref={receipt.LocalRetentionPolicyRef}";
            yield return $"teacher_legal_evidence:terms_snapshot_ref={receipt.LocalRetentionPolicyRef}";
        }
        if (receipt.OutputLicenseRef is not null)
            yield return $"teacher_legal_evidence:output_license_ref={receipt.OutputLicenseRef}";
    }

    private static bool IsValidPolicyEntry(LegalTrainingTeacherPolicyEntry entry)
        => Enum.IsDefined(entry.Family)
           && Enum.IsDefined(entry.AccessMode)
           && Enum.IsDefined(entry.HostingProvider)
           && Enum.IsDefined(entry.ModelLicense)
           && IsSafeIdentity(entry.ModelId)
           && IsSafeIdentity(entry.ModelVersion)
           && IsValidArtifactIdentity(entry)
           && IsCanonicalEvidenceRef(entry.LocalRetentionPolicyRef)
           && IsCanonicalEvidenceRef(entry.OutputLicenseRef)
           && !ContainsClosedIdentity(entry.ModelId)
           && !ContainsClosedIdentity(entry.ModelVersion)
           && IsCanonicalModelForFamily(entry)
           && IsProviderAllowedForFamily(entry.Family, entry.HostingProvider)
           && IsLicenseAllowedForFamily(entry.Family, entry.ModelLicense)
           && (entry.AccessMode != TrainingTeacherAccessMode.LocalHostedOpenWeight
               || entry.HostingProvider == TrainingTeacherHostingProvider.Ollama)
           && (entry.AccessMode != TrainingTeacherAccessMode.OnlineHostedOpenWeight
               || entry.HostingProvider != TrainingTeacherHostingProvider.Ollama);

    private static bool IsCanonicalModelForFamily(
        LegalTrainingTeacherPolicyEntry entry)
    {
        if (entry.HostingProvider == TrainingTeacherHostingProvider.Ollama)
        {
            return IsCanonicalLocalOllamaModelIdForFamily(
                entry.Family,
                entry.ModelId,
                requireTaggedModelId: false);
        }

        return entry.Family switch
        {
            OpenWeightTeacherFamily.Qwen => HasCanonicalPrefix(entry.ModelId, "qwen"),
            OpenWeightTeacherFamily.DeepSeek => HasCanonicalPrefix(entry.ModelId, "deepseek"),
            OpenWeightTeacherFamily.Kimi => HasCanonicalPrefix(entry.ModelId, "kimi"),
            OpenWeightTeacherFamily.Glm => HasCanonicalPrefix(entry.ModelId, "glm"),
            OpenWeightTeacherFamily.Mistral =>
                HasCanonicalPrefix(entry.ModelId, "mistral")
                || HasCanonicalPrefix(entry.ModelId, "mixtral"),
            _ => false
        };
    }

    internal static bool IsCanonicalLocalOllamaModelIdForFamily(
        OpenWeightTeacherFamily family,
        string? modelId,
        bool requireTaggedModelId = true)
    {
        if (modelId is null
            || !IsSafeIdentity(modelId)
            || ContainsClosedIdentity(modelId))
        {
            return false;
        }

        var tagSeparator = modelId.IndexOf(':', StringComparison.Ordinal);
        if (tagSeparator < 0)
        {
            return !requireTaggedModelId
                   && IsCanonicalLocalOllamaModelNameForFamily(
                       family,
                       modelId);
        }

        if (tagSeparator <= 0
            || tagSeparator != modelId.LastIndexOf(':')
            || tagSeparator == modelId.Length - 1)
        {
            return false;
        }

        var modelName = modelId[..tagSeparator];
        var tag = modelId[(tagSeparator + 1)..];
        return IsCanonicalLocalOllamaModelNameForFamily(family, modelName)
               && IsSafeLocalOllamaTag(tag);
    }

    private static bool IsCanonicalLocalOllamaModelNameForFamily(
        OpenWeightTeacherFamily family,
        string modelName)
    {
        if (!StringComparer.Ordinal.Equals(modelName, modelName.ToLowerInvariant())
            || modelName[0] == '/'
            || modelName[^1] == '/'
            || modelName.Any(static character =>
                character is not (>= 'a' and <= 'z')
                    and not (>= '0' and <= '9')
                    and not '.' and not '_' and not '-' and not '/')
            || modelName.Split('/').Any(static segment =>
                segment.Length == 0
                || StringComparer.Ordinal.Equals(segment, ".")
                || StringComparer.Ordinal.Equals(segment, "..")))
        {
            return false;
        }

        return family switch
        {
            OpenWeightTeacherFamily.Qwen => HasFamilyPrefix(modelName, "qwen"),
            OpenWeightTeacherFamily.DeepSeek => HasFamilyPrefix(modelName, "deepseek"),
            OpenWeightTeacherFamily.Kimi => HasFamilyPrefix(modelName, "kimi"),
            OpenWeightTeacherFamily.Glm => HasFamilyPrefix(modelName, "glm"),
            OpenWeightTeacherFamily.Mistral =>
                HasFamilyPrefix(modelName, "mistral")
                || HasFamilyPrefix(modelName, "mixtral"),
            _ => false
        };
    }

    private static bool IsSafeLocalOllamaTag(string tag)
        => tag.Length > 0
           && tag.All(static character =>
               character is >= 'a' and <= 'z'
                   or >= 'A' and <= 'Z'
                   or >= '0' and <= '9'
                   or '.' or '_' or '-');

    private static bool HasCanonicalPrefix(string value, string prefix)
        => StringComparer.Ordinal.Equals(value, value.ToLowerInvariant())
           && HasFamilyPrefix(value, prefix);

    private static bool HasFamilyPrefix(string value, string prefix)
        => value.StartsWith(prefix, StringComparison.Ordinal)
           && (value.Length == prefix.Length
               || char.IsDigit(value[prefix.Length])
               || value[prefix.Length] is '-' or ':' or '/' or '.');

    private static bool IsProviderAllowedForFamily(
        OpenWeightTeacherFamily family,
        TrainingTeacherHostingProvider provider)
    {
        if (provider == TrainingTeacherHostingProvider.Ollama)
            return true;
        if (provider is TrainingTeacherHostingProvider.Together
            or TrainingTeacherHostingProvider.Fireworks
            or TrainingTeacherHostingProvider.OpenRouter
            or TrainingTeacherHostingProvider.Novita
            or TrainingTeacherHostingProvider.HuggingFace)
        {
            return true;
        }

        return family switch
        {
            OpenWeightTeacherFamily.Qwen => provider == TrainingTeacherHostingProvider.DashScope,
            OpenWeightTeacherFamily.DeepSeek => provider == TrainingTeacherHostingProvider.DeepSeek,
            OpenWeightTeacherFamily.Kimi => provider == TrainingTeacherHostingProvider.Moonshot,
            OpenWeightTeacherFamily.Glm => provider == TrainingTeacherHostingProvider.Zai,
            OpenWeightTeacherFamily.Mistral => provider == TrainingTeacherHostingProvider.Mistral,
            _ => false
        };
    }

    private static bool IsLicenseAllowedForFamily(
        OpenWeightTeacherFamily family,
        TrainingTeacherModelLicense license)
        => family switch
        {
            OpenWeightTeacherFamily.Qwen => license == TrainingTeacherModelLicense.Apache20,
            OpenWeightTeacherFamily.DeepSeek => license == TrainingTeacherModelLicense.MIT,
            OpenWeightTeacherFamily.Kimi => license == TrainingTeacherModelLicense.ModifiedMIT,
            OpenWeightTeacherFamily.Glm => license == TrainingTeacherModelLicense.MIT,
            OpenWeightTeacherFamily.Mistral => license == TrainingTeacherModelLicense.Apache20,
            _ => false
        };

    private static bool ContainsClosedIdentity(string value)
        => ForbiddenModelIdentityRegistry.ContainsForbiddenIdentity(value);

    private static bool IsSafeIdentity(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= MaximumIdentityLength
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !value.Any(char.IsControl);

    private static string CanonicalPolicyIdentity(LegalTrainingTeacherPolicyEntry entry)
        => string.Join(
            "|",
            entry.Family,
            entry.AccessMode,
            entry.HostingProvider,
            entry.ModelId,
            entry.ModelVersion,
            entry.ModelLicense,
            entry.ModelArtifactDigestKind,
            entry.ModelArtifactDigest,
            entry.LocalRetentionPolicyRef,
            entry.OutputLicenseRef);

    private static string CanonicalPolicyBaseIdentity(LegalTrainingTeacherPolicyEntry entry)
        => string.Join(
            "|",
            entry.Family,
            entry.AccessMode,
            entry.HostingProvider,
            entry.ModelId,
            entry.ModelVersion,
            entry.ModelLicense,
            entry.ModelArtifactDigestKind,
            entry.ModelArtifactDigest);

    private static bool PolicyBaseIdentityMatches(
        LegalTrainingTeacherPolicyEntry entry,
        TrainingTeacherProvenance row)
        => entry.Family == row.Family
           && entry.AccessMode == row.AccessMode
           && entry.HostingProvider == row.HostingProvider
           && StringComparer.Ordinal.Equals(entry.ModelId, row.ModelId)
           && StringComparer.Ordinal.Equals(entry.ModelVersion, row.ModelVersion)
           && StringComparer.Ordinal.Equals(entry.ModelArtifactDigestKind, row.ModelArtifactDigestKind)
           && StringComparer.Ordinal.Equals(entry.ModelArtifactDigest, row.ModelArtifactDigest)
           && entry.ModelLicense == row.ModelLicense;

    private static bool IsValidArtifactIdentity(TrainingTeacherProvenance provenance)
    {
        var isLocalOllama = provenance.AccessMode == TrainingTeacherAccessMode.LocalHostedOpenWeight
                            && provenance.HostingProvider == TrainingTeacherHostingProvider.Ollama;
        return IsValidArtifactIdentity(
            isLocalOllama,
            provenance.ModelVersion,
            provenance.ModelArtifactDigestKind,
            provenance.ModelArtifactDigest);
    }

    private static bool IsValidArtifactIdentity(LegalTrainingTeacherPolicyEntry entry)
    {
        var isLocalOllama = entry.AccessMode == TrainingTeacherAccessMode.LocalHostedOpenWeight
                            && entry.HostingProvider == TrainingTeacherHostingProvider.Ollama;
        return IsValidArtifactIdentity(
            isLocalOllama,
            entry.ModelVersion,
            entry.ModelArtifactDigestKind,
            entry.ModelArtifactDigest);
    }

    private static bool IsValidArtifactIdentity(
        bool isLocalOllama,
        string? modelVersion,
        string? digestKind,
        string? digest)
        => isLocalOllama
            ? IsCanonicalLocalOllamaArtifactIdentity(
                modelVersion,
                digestKind,
                digest)
            : digestKind is not null
              && digestKind.Length == 0
              && digest is not null
              && digest.Length == 0;

    public static bool IsCanonicalLocalOllamaArtifactIdentity(
        string? modelVersion,
        string? digestKind,
        string? digest)
        => IsCanonicalSha256Digest(modelVersion)
           && StringComparer.Ordinal.Equals(
               digestKind,
               OllamaModelLayerSha256DigestKind)
           && IsCanonicalSha256Digest(digest);

    internal static bool IsCanonicalSha256Digest(string? value)
        => value is not null
           && value.Length == 64
           && value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    internal static bool IsCanonicalEvidenceRef(string? value)
        => value is not null
           && value.Length == 71
           && value.StartsWith("sha256:", StringComparison.Ordinal)
           && value.Skip(7).All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private string HashRef(string scope, string value)
    {
        using var hmac = new HMACSHA256(_auditHashSalt);
        var payload = Encoding.UTF8.GetBytes($"{scope}\0{value}");
        return $"sha256:{Convert.ToHexString(hmac.ComputeHash(payload)).ToLowerInvariant()}";
    }

    private static OperationResult<LegalTrainingTeacherAdmission> InvalidProvenance(
        string message)
        => OperationResult<LegalTrainingTeacherAdmission>.Fail(
            message,
            ProvenanceInvalidErrorCode);

    private static OperationResult<IReadOnlyList<T?>> SnapshotBounded<T>(
        IEnumerable<T?>? source,
        int maximumCount,
        string limitErrorCode)
    {
        if (source is null)
        {
            return OperationResult<IReadOnlyList<T?>>.Fail(
                "A bounded enumerable is required.",
                EnumerationFailedErrorCode);
        }

        var snapshot = new List<T?>(Math.Min(maximumCount, 256));
        try
        {
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (snapshot.Count == maximumCount)
                {
                    return OperationResult<IReadOnlyList<T?>>.Fail(
                        "The server-owned collection limit was exceeded.",
                        limitErrorCode);
                }

                snapshot.Add(enumerator.Current);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<IReadOnlyList<T?>>.Fail(
                "The server-owned collection could not be enumerated.",
                EnumerationFailedErrorCode);
        }

        return OperationResult<IReadOnlyList<T?>>.Ok(
            Array.AsReadOnly(snapshot.ToArray()));
    }
}
