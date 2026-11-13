// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// A safe, immutable, non-authenticating commitment to the legal-teacher
/// admission that authorized one DesignConversion DPO policy checkpoint. It is
/// carried inside the already trusted artifact-hash chain and detects accidental
/// or out-of-band mutation there. It is not a signature, MAC, or proof against
/// an untrusted writer that can rewrite both the artifact and its hashes.
/// </summary>
public sealed record DesignConversionDpoLegalTeacherAdmissionCommitment(
    string Schema,
    string PolicyIdentityRef,
    int ExternalTeacherInvocationCount,
    int ReceiptCount,
    string ReceiptSetSha256,
    string AdmissionEvidenceSetSha256,
    string CheckpointEvidenceSetSha256,
    bool RawIdentityPersisted,
    bool AbsolutePathPersisted,
    string CommitmentSha256);

/// <summary>
/// Opaque pre-publication snapshot. The producer captures it immediately after
/// legal admission, then finalizes it only after the complete checkpoint-ref
/// evidence set is known.
/// </summary>
public sealed class DesignConversionDpoLegalTeacherAdmissionCore
{
    internal DesignConversionDpoLegalTeacherAdmissionCore(
        string policyIdentityRef,
        int externalTeacherInvocationCount,
        int receiptCount,
        string receiptSetSha256,
        string admissionEvidenceSetSha256)
    {
        PolicyIdentityRef = policyIdentityRef;
        ExternalTeacherInvocationCount = externalTeacherInvocationCount;
        ReceiptCount = receiptCount;
        ReceiptSetSha256 = receiptSetSha256;
        AdmissionEvidenceSetSha256 = admissionEvidenceSetSha256;
    }

    internal string PolicyIdentityRef { get; }
    internal int ExternalTeacherInvocationCount { get; }
    internal int ReceiptCount { get; }
    internal string ReceiptSetSha256 { get; }
    internal string AdmissionEvidenceSetSha256 { get; }
}

/// <summary>
/// Canonical codec and validator for non-authenticating legal-teacher admission
/// commitments carried inside an already trusted artifact-hash chain.
/// </summary>
public static class DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
{
    public const string Schema =
        "design-conversion-dpo-legal-teacher-admission-commitment-v1";
    public const string InvalidAdmissionErrorCode =
        "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_ADMISSION_INVALID";
    public const string DuplicateReceiptErrorCode =
        "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_DUPLICATE_RECEIPT";
    public const string DuplicateEvidenceErrorCode =
        "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_DUPLICATE_EVIDENCE";
    public const string LimitExceededErrorCode =
        "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_LIMIT_EXCEEDED";
    public const string EnumerationFailedErrorCode =
        "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_ENUMERATION_FAILED";
    public const string FinalEvidenceInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_FINAL_EVIDENCE_INVALID";
    public const string CommitmentInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_INVALID";
    public const string CommitmentMismatchErrorCode =
        "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_MISMATCH";
    public const string SafeFailureMessage =
        "DesignConversion legal-teacher admission commitment is invalid.";

    private const int MaximumReceiptCount = LegalTrainingTeacherAdmissionAuthority.MaxBatchRows;
    private const int MaximumEvidenceCount =
        LegalTrainingTeacherAdmissionAuthority.MaxBatchRows * 32;
    private const int MaximumStringUtf8Bytes = 4 * 1024;
    private const int MaximumFrameBytes = 64 * 1024;
    private const int MaximumAggregateUtf8Bytes = 8 * 1024 * 1024;

    private const string ReceiptFrameDomain =
        "design-conversion-dpo-legal-teacher-admission-receipt-v1";
    private const string ReceiptSetDomain =
        "design-conversion-dpo-legal-teacher-admission-receipt-set-v1";
    private const string AdmissionEvidenceSetDomain =
        "design-conversion-dpo-legal-teacher-admission-evidence-set-v1";
    private const string CheckpointEvidenceSetDomain =
        "design-conversion-dpo-legal-teacher-checkpoint-evidence-set-v1";
    private const string CommitmentDomain =
        "design-conversion-dpo-legal-teacher-admission-commitment-digest-v1";

    public static OperationResult<DesignConversionDpoLegalTeacherAdmissionCore>
        CaptureAdmission(LegalTrainingTeacherAdmission? admission)
    {
        if (admission is null ||
            !IsCanonicalEvidenceRef(admission.PolicyIdentityRef) ||
            !IsCanonicalEvidenceRef(admission.TrainingContentBatchRef) ||
            admission.ExternalTeacherInvocationCount < 0 ||
            admission.Receipts is null ||
            admission.EvidenceRefs is null)
        {
            return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                InvalidAdmissionErrorCode);
        }

        var receipts = SnapshotBounded(
            admission.Receipts,
            MaximumReceiptCount,
            InvalidAdmissionErrorCode);
        if (!receipts.IsSuccess || receipts.Value is null)
            return receipts.MapError<DesignConversionDpoLegalTeacherAdmissionCore>();
        if (receipts.Value.Count == 0)
        {
            return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                InvalidAdmissionErrorCode);
        }

        var receiptFrames = new List<byte[]>(receipts.Value.Count);
        var requestRefs = new HashSet<string>(StringComparer.Ordinal);
        long receiptInvocationCount = 0;
        try
        {
            foreach (var receipt in receipts.Value)
            {
                var frame = BuildReceiptFrame(receipt);
                if (frame is null)
                {
                    return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                        InvalidAdmissionErrorCode);
                }
                if (!requestRefs.Add(receipt.RequestRef))
                {
                    return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                        DuplicateReceiptErrorCode);
                }
                receiptFrames.Add(frame);
                if (receipt.SourceKind ==
                    TrainingTeacherSourceKind.ExternalTeacherOutput)
                {
                    receiptInvocationCount = checked(
                        receiptInvocationCount + receipt.InvocationCount);
                }
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                InvalidAdmissionErrorCode);
        }

        if (receiptInvocationCount != admission.ExternalTeacherInvocationCount ||
            receiptInvocationCount >
                LegalTrainingTeacherAdmissionAuthority.HardMaximumTeacherCalls)
        {
            return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                InvalidAdmissionErrorCode);
        }

        var receiptSet = HashFrameSet(
            ReceiptSetDomain,
            receiptFrames,
            DuplicateReceiptErrorCode);
        if (!receiptSet.IsSuccess || receiptSet.Value is null)
            return receiptSet.MapError<DesignConversionDpoLegalTeacherAdmissionCore>();

        var admissionEvidence = HashStringSet(
            AdmissionEvidenceSetDomain,
            admission.EvidenceRefs,
            MaximumEvidenceCount,
            DuplicateEvidenceErrorCode);
        if (!admissionEvidence.IsSuccess || admissionEvidence.Value is null)
            return admissionEvidence.MapError<DesignConversionDpoLegalTeacherAdmissionCore>();

        return OperationResult<DesignConversionDpoLegalTeacherAdmissionCore>.Ok(
            new DesignConversionDpoLegalTeacherAdmissionCore(
                admission.PolicyIdentityRef,
                admission.ExternalTeacherInvocationCount,
                receiptFrames.Count,
                receiptSet.Value,
                admissionEvidence.Value));
    }

    public static OperationResult<DesignConversionDpoLegalTeacherAdmissionCore>
        CaptureAdmissions(
            LegalTrainingTeacherAdmission? trainAdmission,
            LegalTrainingTeacherAdmission? heldOutEvaluationAdmission)
    {
        if (!HasCanonicalAdmissionShape(trainAdmission) ||
            !HasCanonicalAdmissionShape(heldOutEvaluationAdmission) ||
            !string.Equals(
                trainAdmission!.PolicyIdentityRef,
                heldOutEvaluationAdmission!.PolicyIdentityRef,
                StringComparison.Ordinal) ||
            string.Equals(
                trainAdmission.TrainingContentBatchRef,
                heldOutEvaluationAdmission.TrainingContentBatchRef,
                StringComparison.Ordinal))
        {
            return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                InvalidAdmissionErrorCode);
        }

        var admissions = new[]
        {
            (Role: "train", Admission: trainAdmission),
            (Role: "heldout_evaluation", Admission: heldOutEvaluationAdmission)
        };
        var receiptFrames = new List<byte[]>();
        var evidenceFrames = new List<string>();
        var uniqueRequestReceipts = new Dictionary<
            string,
            LegalTrainingTeacherAdmissionReceipt>(StringComparer.Ordinal);
        long invocationCount = 0;
        try
        {
            foreach (var (role, admission) in admissions)
            {
                var receipts = SnapshotBounded(
                    admission.Receipts,
                    MaximumReceiptCount,
                    InvalidAdmissionErrorCode);
                if (!receipts.IsSuccess || receipts.Value is null ||
                    receipts.Value.Count == 0)
                {
                    return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                        InvalidAdmissionErrorCode);
                }

                long roleInvocationCount = 0;
                foreach (var receipt in receipts.Value)
                {
                    var receiptFrame = BuildReceiptFrame(receipt);
                    if (receiptFrame is null)
                    {
                        return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                            InvalidAdmissionErrorCode);
                    }

                    using var roleFrame = new CanonicalFrame(MaximumFrameBytes);
                    roleFrame.WriteString("design-conversion-dpo-legal-teacher-admission-role-v1");
                    roleFrame.WriteString(role);
                    roleFrame.WriteString(admission.TrainingContentBatchRef);
                    roleFrame.WriteString(receipt.RequestRef);
                    roleFrame.WriteString(receipt.TrainingContentRef);
                    roleFrame.WriteBytes(receiptFrame);
                    receiptFrames.Add(roleFrame.ToArray());
                    if (uniqueRequestReceipts.TryGetValue(
                            receipt.RequestRef,
                            out var existingReceipt))
                    {
                        var normalized = receipt with
                        {
                            TrainingContentRef =
                                existingReceipt.TrainingContentRef
                        };
                        if (normalized != existingReceipt)
                        {
                            return Fail<
                                DesignConversionDpoLegalTeacherAdmissionCore>(
                                InvalidAdmissionErrorCode);
                        }
                    }
                    else
                    {
                        uniqueRequestReceipts.Add(
                            receipt.RequestRef,
                            receipt with { });
                        if (receipt.SourceKind ==
                            TrainingTeacherSourceKind.ExternalTeacherOutput)
                        {
                            invocationCount = checked(
                                invocationCount + receipt.InvocationCount);
                        }
                    }
                    if (receipt.SourceKind ==
                        TrainingTeacherSourceKind.ExternalTeacherOutput)
                    {
                        roleInvocationCount = checked(
                            roleInvocationCount + receipt.InvocationCount);
                    }
                }

                if (roleInvocationCount != admission.ExternalTeacherInvocationCount)
                {
                    return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                        InvalidAdmissionErrorCode);
                }
                evidenceFrames.Add($"role:{role}:content:{admission.TrainingContentBatchRef}");
                evidenceFrames.AddRange(admission.EvidenceRefs.Select(
                    evidence => $"role:{role}:evidence:{evidence}"));
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                InvalidAdmissionErrorCode);
        }

        if (receiptFrames.Count > MaximumReceiptCount ||
            invocationCount >
                LegalTrainingTeacherAdmissionAuthority.HardMaximumTeacherCalls)
        {
            return Fail<DesignConversionDpoLegalTeacherAdmissionCore>(
                LimitExceededErrorCode);
        }

        var receiptSet = HashFrameSet(
            ReceiptSetDomain,
            receiptFrames,
            DuplicateReceiptErrorCode);
        if (!receiptSet.IsSuccess || receiptSet.Value is null)
            return receiptSet.MapError<DesignConversionDpoLegalTeacherAdmissionCore>();
        var evidenceSet = HashStringSet(
            AdmissionEvidenceSetDomain,
            evidenceFrames,
            MaximumEvidenceCount,
            DuplicateEvidenceErrorCode);
        if (!evidenceSet.IsSuccess || evidenceSet.Value is null)
            return evidenceSet.MapError<DesignConversionDpoLegalTeacherAdmissionCore>();

        return OperationResult<DesignConversionDpoLegalTeacherAdmissionCore>.Ok(
            new DesignConversionDpoLegalTeacherAdmissionCore(
                trainAdmission.PolicyIdentityRef,
                checked((int)invocationCount),
                receiptFrames.Count,
                receiptSet.Value,
                evidenceSet.Value));
    }

    private static bool HasCanonicalAdmissionShape(
        LegalTrainingTeacherAdmission? admission)
        => admission is not null &&
           IsCanonicalEvidenceRef(admission.PolicyIdentityRef) &&
           IsCanonicalEvidenceRef(admission.TrainingContentBatchRef) &&
           admission.ExternalTeacherInvocationCount >= 0 &&
           admission.Receipts is not null &&
           admission.EvidenceRefs is not null;

    public static OperationResult<DesignConversionDpoLegalTeacherAdmissionCommitment> FinalizeCommitment(
        DesignConversionDpoLegalTeacherAdmissionCore? admissionCore,
        IEnumerable<string>? checkpointEvidenceRefs)
    {
        if (admissionCore is null)
        {
            return Fail<DesignConversionDpoLegalTeacherAdmissionCommitment>(
                InvalidAdmissionErrorCode);
        }

        var checkpointEvidence = HashStringSet(
            CheckpointEvidenceSetDomain,
            checkpointEvidenceRefs,
            MaximumEvidenceCount,
            DuplicateEvidenceErrorCode);
        if (!checkpointEvidence.IsSuccess || checkpointEvidence.Value is null)
            return checkpointEvidence.MapError<DesignConversionDpoLegalTeacherAdmissionCommitment>();

        var unsigned = new DesignConversionDpoLegalTeacherAdmissionCommitment(
            Schema,
            admissionCore.PolicyIdentityRef,
            admissionCore.ExternalTeacherInvocationCount,
            admissionCore.ReceiptCount,
            admissionCore.ReceiptSetSha256,
            admissionCore.AdmissionEvidenceSetSha256,
            checkpointEvidence.Value,
            RawIdentityPersisted: false,
            AbsolutePathPersisted: false,
            CommitmentSha256: string.Empty);
        var committedValue = unsigned with { CommitmentSha256 = ComputeCommitmentSha256(unsigned) };
        return Validate(committedValue)
            ? OperationResult<DesignConversionDpoLegalTeacherAdmissionCommitment>.Ok(committedValue)
            : Fail<DesignConversionDpoLegalTeacherAdmissionCommitment>(CommitmentInvalidErrorCode);
    }

    public static bool Validate(DesignConversionDpoLegalTeacherAdmissionCommitment? commitment)
    {
        if (commitment is null ||
            !string.Equals(commitment.Schema, Schema, StringComparison.Ordinal) ||
            !IsCanonicalEvidenceRef(commitment.PolicyIdentityRef) ||
            commitment.ExternalTeacherInvocationCount < 0 ||
            commitment.ExternalTeacherInvocationCount >
                LegalTrainingTeacherAdmissionAuthority.HardMaximumTeacherCalls ||
            commitment.ReceiptCount <= 0 ||
            commitment.ReceiptCount > MaximumReceiptCount ||
            !IsCanonicalSha256(commitment.ReceiptSetSha256) ||
            !IsCanonicalSha256(commitment.AdmissionEvidenceSetSha256) ||
            !IsCanonicalSha256(commitment.CheckpointEvidenceSetSha256) ||
            commitment.RawIdentityPersisted ||
            commitment.AbsolutePathPersisted ||
            !IsCanonicalSha256(commitment.CommitmentSha256))
        {
            return false;
        }

        return string.Equals(
            commitment.CommitmentSha256,
            ComputeCommitmentSha256(commitment with { CommitmentSha256 = string.Empty }),
            StringComparison.Ordinal);
    }

    public static bool MatchesCheckpointEvidence(
        DesignConversionDpoLegalTeacherAdmissionCommitment? commitment,
        IEnumerable<string>? checkpointEvidenceRefs)
    {
        if (!Validate(commitment))
            return false;
        var expected = HashStringSet(
            CheckpointEvidenceSetDomain,
            checkpointEvidenceRefs,
            MaximumEvidenceCount,
            DuplicateEvidenceErrorCode);
        return expected.IsSuccess &&
               string.Equals(
                   commitment!.CheckpointEvidenceSetSha256,
                   expected.Value,
                   StringComparison.Ordinal);
    }

    public static bool MatchesAdmissionAndCheckpoint(
        DesignConversionDpoLegalTeacherAdmissionCommitment? commitment,
        LegalTrainingTeacherAdmission? admission,
        IEnumerable<string>? checkpointEvidenceRefs)
    {
        if (!Validate(commitment))
            return false;
        var core = CaptureAdmission(admission);
        if (!core.IsSuccess || core.Value is null)
            return false;
        var expected = FinalizeCommitment(core.Value, checkpointEvidenceRefs);
        return expected.IsSuccess && expected.Value == commitment;
    }

    public static DesignConversionDpoLegalTeacherAdmissionCommitment? Snapshot(
        DesignConversionDpoLegalTeacherAdmissionCommitment? commitment)
        => commitment is null ? null : commitment with { };

    private static byte[]? BuildReceiptFrame(LegalTrainingTeacherAdmissionReceipt? receipt)
    {
        if (receipt is null ||
            !Enum.IsDefined(receipt.SourceKind) ||
            !IsDefined(receipt.Family) ||
            !IsDefined(receipt.AccessMode) ||
            !IsDefined(receipt.HostingProvider) ||
            !IsDefined(receipt.ModelLicense) ||
            receipt.InvocationCount < 0 ||
            !IsCanonicalEvidenceRef(receipt.RequestRef) ||
            !IsCanonicalEvidenceRef(receipt.ProviderRef) ||
            !IsCanonicalEvidenceRef(receipt.ModelRef) ||
            !IsCanonicalEvidenceRef(receipt.ModelVersionRef) ||
            !IsCanonicalEvidenceRef(receipt.LicenseRef) ||
            !IsSafeOptionalScalar(receipt.LegalEvidenceMode) ||
            !IsSafeOptionalEvidenceRef(receipt.LocalRetentionPolicyRef) ||
            !IsSafeOptionalEvidenceRef(receipt.OutputLicenseRef) ||
            !IsSafeScalar(receipt.ModelArtifactDigestKind) ||
            !(receipt.ModelArtifactRef.Length == 0 ||
              IsCanonicalEvidenceRef(receipt.ModelArtifactRef)) ||
            !IsCanonicalEvidenceRef(receipt.TrainingContentRef) ||
            !HasAuthorityReceiptSemantics(receipt))
        {
            return null;
        }

        using var frame = new CanonicalFrame(MaximumFrameBytes);
        frame.WriteString(ReceiptFrameDomain);
        frame.WriteEnum(receipt.SourceKind);
        frame.WriteNullableEnum(receipt.Family);
        frame.WriteNullableEnum(receipt.AccessMode);
        frame.WriteNullableEnum(receipt.HostingProvider);
        frame.WriteNullableEnum(receipt.ModelLicense);
        frame.WriteBool(receipt.ProviderTermsChecked);
        frame.WriteBool(receipt.ProviderTermsAllowTraining);
        frame.WriteBool(receipt.ClosedProprietaryModel);
        frame.WriteBool(receipt.TeacherOutputUsedForTraining);
        frame.WriteInt32(receipt.InvocationCount);
        frame.WriteString(receipt.RequestRef);
        frame.WriteString(receipt.ProviderRef);
        frame.WriteString(receipt.ModelRef);
        frame.WriteString(receipt.ModelVersionRef);
        frame.WriteString(receipt.LicenseRef);
        frame.WriteString(receipt.LegalEvidenceMode);
        frame.WriteString(receipt.LocalRetentionPolicyRef);
        frame.WriteString(receipt.OutputLicenseRef);
        frame.WriteString(receipt.ModelArtifactDigestKind);
        frame.WriteString(receipt.ModelArtifactRef);
        frame.WriteString(receipt.TrainingContentRef);
        return frame.ToArray();
    }

    private static bool HasAuthorityReceiptSemantics(
        LegalTrainingTeacherAdmissionReceipt receipt)
    {
        if (receipt.SourceKind ==
            TrainingTeacherSourceKind.DeterministicNoExternalTeacher)
        {
            return receipt.Family is null &&
                   receipt.AccessMode is null &&
                   receipt.HostingProvider is null &&
                   receipt.ModelLicense is null &&
                   !receipt.ProviderTermsChecked &&
                   !receipt.ProviderTermsAllowTraining &&
                   !receipt.ClosedProprietaryModel &&
                   !receipt.TeacherOutputUsedForTraining &&
                   receipt.InvocationCount == 0 &&
                   receipt.LegalEvidenceMode is null &&
                   receipt.LocalRetentionPolicyRef is null &&
                   receipt.OutputLicenseRef is null &&
                   receipt.ModelArtifactDigestKind.Length == 0 &&
                   receipt.ModelArtifactRef.Length == 0;
        }

        if (receipt.SourceKind != TrainingTeacherSourceKind.ExternalTeacherOutput ||
            receipt.Family is null ||
            receipt.AccessMode is null ||
            receipt.HostingProvider is null ||
            receipt.ModelLicense is null ||
            receipt.InvocationCount <= 0 ||
            receipt.ClosedProprietaryModel ||
            !receipt.TeacherOutputUsedForTraining ||
            !IsCanonicalEvidenceRef(receipt.LocalRetentionPolicyRef) ||
            !IsCanonicalEvidenceRef(receipt.OutputLicenseRef))
        {
            return false;
        }

        var localOllama = receipt.AccessMode ==
                TrainingTeacherAccessMode.LocalHostedOpenWeight &&
            receipt.HostingProvider == TrainingTeacherHostingProvider.Ollama;
        var hosted = receipt.AccessMode ==
                TrainingTeacherAccessMode.OnlineHostedOpenWeight &&
            receipt.HostingProvider != TrainingTeacherHostingProvider.Ollama;
        if (!localOllama && !hosted)
            return false;

        if (localOllama)
        {
            return !receipt.ProviderTermsChecked &&
                   !receipt.ProviderTermsAllowTraining &&
                   string.Equals(
                       receipt.LegalEvidenceMode,
                       LegalTrainingTeacherAdmissionAuthority
                           .SelfHostedNoProviderTermsEvidenceMode,
                       StringComparison.Ordinal) &&
                   string.Equals(
                       receipt.ModelArtifactDigestKind,
                       LegalTrainingTeacherAdmissionAuthority
                           .OllamaModelLayerSha256DigestKind,
                       StringComparison.Ordinal) &&
                   IsCanonicalEvidenceRef(receipt.ModelArtifactRef);
        }

        return receipt.ProviderTermsChecked &&
               receipt.ProviderTermsAllowTraining &&
               string.Equals(
                   receipt.LegalEvidenceMode,
                   LegalTrainingTeacherAdmissionAuthority
                       .HostedProviderTermsEvidenceMode,
                   StringComparison.Ordinal) &&
               receipt.ModelArtifactDigestKind.Length == 0 &&
               receipt.ModelArtifactRef.Length == 0;
    }

    private static OperationResult<string> HashFrameSet(
        string domain,
        IReadOnlyList<byte[]> frames,
        string duplicateErrorCode)
    {
        long aggregateBytes = 0;
        try
        {
            foreach (var frame in frames)
                aggregateBytes = checked(aggregateBytes + frame.Length);
        }
        catch (OverflowException)
        {
            return Fail<string>(LimitExceededErrorCode);
        }
        if (aggregateBytes > MaximumAggregateUtf8Bytes)
            return Fail<string>(LimitExceededErrorCode);

        var ordered = frames.Order(ByteArrayOrdinalComparer.Instance).ToArray();
        if (ordered.Zip(ordered.Skip(1), static (left, right) =>
                left.AsSpan().SequenceEqual(right)).Any(static duplicate => duplicate))
        {
            return Fail<string>(duplicateErrorCode);
        }

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendHashString(hash, domain);
        AppendHashInt32(hash, ordered.Length);
        foreach (var item in ordered)
            AppendHashBytes(hash, item);
        return OperationResult<string>.Ok(
            Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    private static OperationResult<string> HashStringSet(
        string domain,
        IEnumerable<string>? values,
        int maximumCount,
        string duplicateErrorCode)
    {
        var snapshot = SnapshotBounded(values, maximumCount, FinalEvidenceInvalidErrorCode);
        if (!snapshot.IsSuccess || snapshot.Value is null)
            return snapshot.MapError<string>();
        if (snapshot.Value.Any(static value =>
                string.IsNullOrWhiteSpace(value) ||
                !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
                value.Any(char.IsControl) ||
                Encoding.UTF8.GetByteCount(value) > MaximumStringUtf8Bytes))
        {
            return Fail<string>(FinalEvidenceInvalidErrorCode);
        }

        long aggregateBytes = 0;
        try
        {
            foreach (var value in snapshot.Value)
                aggregateBytes = checked(
                    aggregateBytes + Encoding.UTF8.GetByteCount(value));
        }
        catch (OverflowException)
        {
            return Fail<string>(LimitExceededErrorCode);
        }
        if (aggregateBytes > MaximumAggregateUtf8Bytes)
            return Fail<string>(LimitExceededErrorCode);

        var ordered = snapshot.Value.Order(StringComparer.Ordinal).ToArray();
        if (ordered.Distinct(StringComparer.Ordinal).Count() != ordered.Length)
            return Fail<string>(duplicateErrorCode);

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendHashString(hash, domain);
        AppendHashInt32(hash, ordered.Length);
        foreach (var value in ordered)
            AppendHashString(hash, value);
        return OperationResult<string>.Ok(
            Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    private static string ComputeCommitmentSha256(
        DesignConversionDpoLegalTeacherAdmissionCommitment commitment)
    {
        using var frame = new CanonicalFrame(MaximumFrameBytes);
        frame.WriteString(CommitmentDomain);
        frame.WriteString(commitment.Schema);
        frame.WriteString(commitment.PolicyIdentityRef);
        frame.WriteInt32(commitment.ExternalTeacherInvocationCount);
        frame.WriteInt32(commitment.ReceiptCount);
        frame.WriteString(commitment.ReceiptSetSha256);
        frame.WriteString(commitment.AdmissionEvidenceSetSha256);
        frame.WriteString(commitment.CheckpointEvidenceSetSha256);
        frame.WriteBool(commitment.RawIdentityPersisted);
        frame.WriteBool(commitment.AbsolutePathPersisted);
        return HashBytes(frame.ToArray());
    }

    private static OperationResult<IReadOnlyList<T>> SnapshotBounded<T>(
        IEnumerable<T>? source,
        int maximumCount,
        string invalidErrorCode)
    {
        if (source is null)
            return Fail<IReadOnlyList<T>>(invalidErrorCode);
        var snapshot = new List<T>(Math.Min(maximumCount, 256));
        try
        {
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (snapshot.Count == maximumCount)
                    return Fail<IReadOnlyList<T>>(LimitExceededErrorCode);
                snapshot.Add(enumerator.Current);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Fail<IReadOnlyList<T>>(EnumerationFailedErrorCode);
        }
        return OperationResult<IReadOnlyList<T>>.Ok(Array.AsReadOnly(snapshot.ToArray()));
    }

    private static bool IsDefined<TEnum>(TEnum? value)
        where TEnum : struct, Enum
        => value is null || Enum.IsDefined(value.Value);

    private static bool IsSafeScalar(string? value)
        => value is not null &&
           Encoding.UTF8.GetByteCount(value) <= MaximumStringUtf8Bytes &&
           !value.Any(char.IsControl);

    private static bool IsSafeOptionalScalar(string? value)
        => value is null || IsSafeScalar(value);

    private static bool IsSafeOptionalEvidenceRef(string? value)
        => value is null || IsCanonicalEvidenceRef(value);

    private static bool IsCanonicalEvidenceRef(string? value)
        => value is { Length: 71 } &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           IsCanonicalSha256(value[7..]);

    private static bool IsCanonicalSha256(string? value)
        => value is { Length: 64 } &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static string HashBytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void AppendHashString(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        AppendHashInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendHashBytes(IncrementalHash hash, byte[] value)
    {
        AppendHashInt32(hash, value.Length);
        hash.AppendData(value);
    }

    private static void AppendHashInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static OperationResult<T> Fail<T>(string errorCode)
        => OperationResult<T>.Fail(SafeFailureMessage, errorCode);

    private sealed class ByteArrayOrdinalComparer : IComparer<byte[]>
    {
        internal static readonly ByteArrayOrdinalComparer Instance = new();

        public int Compare(byte[]? left, byte[]? right)
        {
            if (ReferenceEquals(left, right))
                return 0;
            if (left is null)
                return -1;
            if (right is null)
                return 1;
            var sharedLength = Math.Min(left.Length, right.Length);
            for (var index = 0; index < sharedLength; index++)
            {
                var comparison = left[index].CompareTo(right[index]);
                if (comparison != 0)
                    return comparison;
            }
            return left.Length.CompareTo(right.Length);
        }
    }

    private sealed class CanonicalFrame : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly int _maximumBytes;

        internal CanonicalFrame(int maximumBytes)
        {
            _maximumBytes = maximumBytes;
            _stream = new MemoryStream(Math.Min(maximumBytes, 16 * 1024));
        }

        internal void WriteString(string? value)
        {
            if (value is null)
            {
                WriteInt32(-1);
                return;
            }
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > MaximumStringUtf8Bytes)
                throw new InvalidDataException("Canonical string exceeds its bound.");
            WriteInt32(bytes.Length);
            WriteRaw(bytes);
        }

        internal void WriteBytes(byte[] value)
        {
            WriteInt32(value.Length);
            WriteRaw(value);
        }

        internal void WriteEnum<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(value))
                throw new InvalidDataException("Canonical enum value is undefined.");
            WriteInt32(Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture));
        }

        internal void WriteNullableEnum<TEnum>(TEnum? value)
            where TEnum : struct, Enum
        {
            if (value is null)
            {
                WriteInt32(-1);
                return;
            }
            WriteEnum(value.Value);
        }

        internal void WriteBool(bool value)
            => WriteRaw([value ? (byte)1 : (byte)0]);

        internal void WriteInt32(int value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(bytes, value);
            WriteRaw(bytes);
        }

        internal byte[] ToArray() => _stream.ToArray();

        private void WriteRaw(ReadOnlySpan<byte> bytes)
        {
            if (_stream.Length + bytes.Length > _maximumBytes)
                throw new InvalidDataException("Canonical frame exceeds its bound.");
            _stream.Write(bytes);
        }

        public void Dispose() => _stream.Dispose();
    }
}
