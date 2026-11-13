// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public sealed record ResultGapSourceDescriptor(
    string SourceKind,
    string LicenseExpression,
    string PrivacyScope,
    string SourceRef,
    string SourceSha256,
    string SourceVersionId,
    bool UserApprovalRecorded);

public sealed record ResultGapSourceRoute(
    string SourceKind,
    string PrivacyScope,
    string TrainingRoute,
    bool CommonTrainingAllowed,
    bool UserAdaptiveAllowed,
    bool Quarantined,
    string QuarantineReason,
    bool RawPayloadAllowedInChosenTarget,
    bool SourceIdUsedAsTrainingFeature,
    string RouteEvidence);

public sealed record ResultGapNearMissTaxonomyEntry(
    string TaxonomyCode,
    string DisplayName,
    string WhyPlausible,
    string RejectedTeaches,
    bool TrainReady);

public sealed record ResultGapDpoCandidateWriteRequest(
    string OutputPath,
    ResultGapResult ResultGap,
    ResultGapSourceDescriptor Source,
    string PromptRef,
    string OutputRef,
    string CandidateId,
    string NearMissTaxonomyCode,
    string RejectedWhyPlausible,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

public sealed record ResultGapCorrectionSignalCandidateWriteRequest(
    string OutputPath,
    string CorrectionId,
    IReadOnlyList<string> ChosenSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    string CorrectionClass,
    string PromptRef,
    string OutputRef,
    string AiResponseRef,
    string CorrectionRef,
    string SourcePromptLedgerId,
    ResultGapSourceDescriptor Source,
    string NearMissTaxonomyCode,
    string RejectedWhyPlausible,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

public sealed record ResultGapTrainableDpoCandidate(
    string CandidateId,
    string PairId,
    string CandidateKind,
    string ChosenRef,
    string RejectedRef,
    IReadOnlyList<string> ChosenSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    string MissingSkillId,
    string CorrectionClass,
    string PromptRef,
    string OutputRef,
    string AiResponseRef,
    string CorrectionRef,
    string SourcePromptLedgerId,
    string TargetTrainingAdapter,
    string StorageScope,
    string TrainingRoute,
    string UniversalBehaviorTag,
    string NearMissTaxonomyCode,
    string RejectedWhyPlausible,
    bool TrainReady,
    bool SemanticQualityClaimed,
    bool ContainsRawPrivateText,
    bool ContainsRawVendorOutput,
    bool ChosenTargetContainsRawPrivateOrVendorOutput,
    bool CommonCheckpointMutationClaimed,
    bool TechnicalExecutionRefsExcluded,
    bool SourceIdUsedAsTrainingFeature,
    string QuarantineState,
    string QuarantineReason,
    ResultGapSourceRoute SourceRoute,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

public sealed record ResultGapDpoCandidateWriteResult(
    string CandidatePath,
    ResultGapTrainableDpoCandidate Candidate,
    ResultGapSourceRoute SourceRoute,
    bool Written,
    bool Quarantined);

public sealed record ResultGapDpoSchemaValidationResult(
    bool Passed,
    IReadOnlyList<string> FailedFields,
    int CheckedFieldCount,
    string Verdict);

public sealed class ResultGapSourcePrivacyRouter
{
    public ResultGapSourceRoute Classify(ResultGapSourceDescriptor source)
    {
        if (string.IsNullOrWhiteSpace(source.SourceRef) || string.IsNullOrWhiteSpace(source.SourceKind))
            return Quarantined(source, "source_identity_missing");

        if (IsExternalOrVendor(source))
            return Quarantined(source, "external_vendor_or_unapproved_source");

        if (IsForbiddenPrivacy(source))
            return Quarantined(source, "privacy_scope_forbidden");

        if (IsPrivateAdaptive(source))
        {
            return new ResultGapSourceRoute(
                SourceKind: source.SourceKind,
                PrivacyScope: source.PrivacyScope,
                TrainingRoute: "adaptive_only",
                CommonTrainingAllowed: false,
                UserAdaptiveAllowed: true,
                Quarantined: false,
                QuarantineReason: "",
                RawPayloadAllowedInChosenTarget: false,
                SourceIdUsedAsTrainingFeature: false,
                RouteEvidence: "private_or_user_correction_routes_to_user_adaptive_signal_only");
        }

        if (IsCommonAllowed(source))
        {
            return new ResultGapSourceRoute(
                SourceKind: source.SourceKind,
                PrivacyScope: source.PrivacyScope,
                TrainingRoute: "trainable_common",
                CommonTrainingAllowed: true,
                UserAdaptiveAllowed: false,
                Quarantined: false,
                QuarantineReason: "",
                RawPayloadAllowedInChosenTarget: false,
                SourceIdUsedAsTrainingFeature: false,
                RouteEvidence: "approved_common_source_license_privacy_gate_passed");
        }

        return Quarantined(source, "source_license_or_privacy_not_train_ready");
    }

    private static ResultGapSourceRoute Quarantined(ResultGapSourceDescriptor source, string reason)
        => new(
            SourceKind: source.SourceKind,
            PrivacyScope: source.PrivacyScope,
            TrainingRoute: "quarantine",
            CommonTrainingAllowed: false,
            UserAdaptiveAllowed: false,
            Quarantined: true,
            QuarantineReason: reason,
            RawPayloadAllowedInChosenTarget: false,
            SourceIdUsedAsTrainingFeature: false,
            RouteEvidence: "source_route_quarantine_blocks_training");

    private static bool IsCommonAllowed(ResultGapSourceDescriptor source)
        => (string.Equals(source.SourceKind, "approved_common", StringComparison.Ordinal)
            || string.Equals(source.SourceKind, "repository_common", StringComparison.Ordinal)
            || string.Equals(source.SourceKind, "synthetic_fixture", StringComparison.Ordinal))
            && (string.Equals(source.PrivacyScope, "common_allowed", StringComparison.Ordinal)
                || string.Equals(source.PrivacyScope, "redacted_common_candidate", StringComparison.Ordinal))
            && IsAllowedCommonLicense(source.LicenseExpression);

    private static bool IsPrivateAdaptive(ResultGapSourceDescriptor source)
        => string.Equals(source.PrivacyScope, "personal_adaptive_only", StringComparison.Ordinal)
            || string.Equals(source.SourceKind, "private_user", StringComparison.Ordinal)
            || string.Equals(source.SourceKind, "manual_correction", StringComparison.Ordinal)
            || string.Equals(source.SourceKind, "user_correction", StringComparison.Ordinal);

    private static bool IsExternalOrVendor(ResultGapSourceDescriptor source)
        => source.SourceKind.Contains("vendor", StringComparison.OrdinalIgnoreCase)
            || source.SourceKind.Contains("external", StringComparison.OrdinalIgnoreCase)
            || source.LicenseExpression.Contains("vendor", StringComparison.OrdinalIgnoreCase)
            || source.LicenseExpression.Contains("unknown", StringComparison.OrdinalIgnoreCase);

    private static bool IsForbiddenPrivacy(ResultGapSourceDescriptor source)
        => string.Equals(source.PrivacyScope, "forbidden", StringComparison.Ordinal)
            || string.Equals(source.PrivacyScope, "raw_private", StringComparison.Ordinal);

    private static bool IsAllowedCommonLicense(string license)
        => string.Equals(license, "MIT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(license, "Apache-2.0", StringComparison.OrdinalIgnoreCase)
            || string.Equals(license, "ProjectInternal", StringComparison.OrdinalIgnoreCase)
            || string.Equals(license, "Synthetic", StringComparison.OrdinalIgnoreCase);
}

public sealed class ResultGapNearMissTaxonomy
{
    private static readonly IReadOnlyList<ResultGapNearMissTaxonomyEntry> Entries =
    [
        new ResultGapNearMissTaxonomyEntry(
            TaxonomyCode: "missing_required_capability_after_output",
            DisplayName: "Missing required capability after output",
            WhyPlausible: "The first pass selected a relevant startup capability but missed a second required review capability.",
            RejectedTeaches: "Rejected row teaches that a partially relevant skill bundle still fails when the missing capability is observable after output.",
            TrainReady: true),
        new ResultGapNearMissTaxonomyEntry(
            TaxonomyCode: "false_done_with_gap_remaining",
            DisplayName: "False done while gap remains",
            WhyPlausible: "The answer can look complete while a required skill-specific verification step is still absent.",
            RejectedTeaches: "Rejected row teaches that task completion must wait for the missing-pattern second pass.",
            TrainReady: true),
        new ResultGapNearMissTaxonomyEntry(
            TaxonomyCode: "correction_overfit_to_raw_answer",
            DisplayName: "Correction overfits raw answer",
            WhyPlausible: "A correction can appear useful while copying the host answer instead of storing redacted adaptive refs.",
            RejectedTeaches: "Rejected row teaches that user corrections become structured refs and taxonomy, not raw private target text.",
            TrainReady: true),
        new ResultGapNearMissTaxonomyEntry(
            TaxonomyCode: "generic_bad_output",
            DisplayName: "Generic bad output",
            WhyPlausible: "Generic badness does not identify a close failure boundary.",
            RejectedTeaches: "This entry is quarantine-only and must not train a DPO pair.",
            TrainReady: false)
    ];

    public IReadOnlyList<ResultGapNearMissTaxonomyEntry> All()
        => Entries;

    public bool IsTrainReady(string taxonomyCode)
        => Entries.Any(entry => string.Equals(entry.TaxonomyCode, taxonomyCode, StringComparison.Ordinal)
            && entry.TrainReady);
}

public sealed class ResultGapDpoSchemaValidator
{
    private readonly ResultGapNearMissTaxonomy _taxonomy = new();

    public OperationResult<ResultGapDpoSchemaValidationResult> Validate(ResultGapTrainableDpoCandidate candidate)
    {
        var failures = new List<string>();
        Require(!string.IsNullOrWhiteSpace(candidate.CandidateId), "candidate_id", failures);
        Require(!string.IsNullOrWhiteSpace(candidate.PairId), "pair_id", failures);
        Require(!string.IsNullOrWhiteSpace(candidate.ChosenRef), "chosen_ref", failures);
        Require(!string.IsNullOrWhiteSpace(candidate.RejectedRef), "rejected_ref", failures);
        Require(candidate.ChosenSkillIds.Count > 0, "chosen_skill_ids", failures);
        Require(candidate.RejectedSkillIds.Count > 0, "rejected_skill_ids", failures);
        Require(!candidate.ChosenTargetContainsRawPrivateOrVendorOutput, "chosen_target_privacy", failures);
        Require(!candidate.ContainsRawPrivateText, "raw_private_text", failures);
        Require(!candidate.ContainsRawVendorOutput, "raw_vendor_output", failures);
        Require(!candidate.CommonCheckpointMutationClaimed, "common_checkpoint_mutation", failures);
        Require(candidate.TechnicalExecutionRefsExcluded, "technical_execution_refs_excluded", failures);
        Require(!candidate.SourceIdUsedAsTrainingFeature, "source_id_not_feature", failures);
        Require(_taxonomy.IsTrainReady(candidate.NearMissTaxonomyCode), "near_miss_taxonomy", failures);
        Require(candidate.RejectedWhyPlausible.Length >= 24, "rejected_plausibility", failures);

        var result = new ResultGapDpoSchemaValidationResult(
            Passed: failures.Count == 0,
            FailedFields: failures,
            CheckedFieldCount: 14,
            Verdict: failures.Count == 0 ? "RESULT_GAP_DPO_SCHEMA_VALID" : "RESULT_GAP_DPO_SCHEMA_INVALID");

        return OperationResult<ResultGapDpoSchemaValidationResult>.Ok(result);
    }

    private static void Require(bool condition, string field, ICollection<string> failures)
    {
        if (!condition)
            failures.Add(field);
    }
}

public sealed class ResultGapDpoCandidateWriter
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly ResultGapSourcePrivacyRouter _router = new();
    private readonly ResultGapNearMissTaxonomy _taxonomy = new();

    public OperationResult<ResultGapDpoCandidateWriteResult> WriteMissingPatternCandidate(
        ResultGapDpoCandidateWriteRequest request)
    {
        if (request.ResultGap?.DpoRow is null || !request.ResultGap.SkillGapDetected)
            return OperationResult<ResultGapDpoCandidateWriteResult>.Fail(
                "A detected ResultGap DPO row is required.",
                "RESULT_GAP_DPO_ROW_REQUIRED");

        var dpo = request.ResultGap.DpoRow;
        var sourceRoute = _router.Classify(request.Source);
        var refsAreSafe = IsSafeRef(request.PromptRef) && IsSafeRef(request.OutputRef);
        var quarantineReason = GetQuarantineReason(
            sourceRoute,
            refsAreSafe,
            "unsafe_prompt_or_output_ref",
            request.NearMissTaxonomyCode);
        var trainReady = string.IsNullOrWhiteSpace(quarantineReason);
        var provenance = BuildProvenance(request.ProvenanceRefs, request.Source, sourceRoute)
            .Append(new KeyValuePair<string, string>("prompt_ref", request.PromptRef))
            .Append(new KeyValuePair<string, string>("output_ref", request.OutputRef))
            .Append(new KeyValuePair<string, string>("source_prompt_ledger_id", dpo.SourcePromptLedgerId))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        var candidate = new ResultGapTrainableDpoCandidate(
            CandidateId: request.CandidateId,
            PairId: dpo.PairId,
            CandidateKind: "missing_pattern",
            ChosenRef: $"result-gap-missing:chosen:{dpo.SourcePromptLedgerId}:{dpo.MissingSkillId}",
            RejectedRef: $"result-gap-missing:near-miss:{dpo.SourcePromptLedgerId}:{dpo.MissingSkillId}:{request.NearMissTaxonomyCode}",
            ChosenSkillIds: dpo.ChosenSkillIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            RejectedSkillIds: dpo.RejectedSkillIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            MissingSkillId: dpo.MissingSkillId,
            CorrectionClass: "",
            PromptRef: request.PromptRef,
            OutputRef: request.OutputRef,
            AiResponseRef: "",
            CorrectionRef: "",
            SourcePromptLedgerId: dpo.SourcePromptLedgerId,
            TargetTrainingAdapter: dpo.TargetTrainingAdapter,
            StorageScope: sourceRoute.TrainingRoute == "trainable_common"
                ? "repo-common:smart-skills/result-gap/missing-pattern-dpo-template"
                : "staged-user-adaptive:smart-skills/result-gap/missing-pattern-dpo-signal",
            TrainingRoute: sourceRoute.TrainingRoute,
            UniversalBehaviorTag: "resultgap.missing_pattern.requires_second_pass_capability",
            NearMissTaxonomyCode: request.NearMissTaxonomyCode,
            RejectedWhyPlausible: request.RejectedWhyPlausible,
            TrainReady: trainReady,
            SemanticQualityClaimed: false,
            ContainsRawPrivateText: false,
            ContainsRawVendorOutput: false,
            ChosenTargetContainsRawPrivateOrVendorOutput: false,
            CommonCheckpointMutationClaimed: false,
            TechnicalExecutionRefsExcluded: true,
            SourceIdUsedAsTrainingFeature: false,
            QuarantineState: trainReady ? "train_ready" : "quarantined",
            QuarantineReason: quarantineReason,
            SourceRoute: sourceRoute,
            ProvenanceRefs: provenance);

        return WriteCandidate(request.OutputPath, candidate, sourceRoute);
    }

    public OperationResult<ResultGapDpoCandidateWriteResult> WriteCorrectionSignalCandidate(
        ResultGapCorrectionSignalCandidateWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CorrectionId)
            || string.IsNullOrWhiteSpace(request.CorrectionClass)
            || string.IsNullOrWhiteSpace(request.SourcePromptLedgerId))
            return OperationResult<ResultGapDpoCandidateWriteResult>.Fail(
                "Correction identity fields are required.",
                "RESULT_GAP_CORRECTION_IDENTITY_REQUIRED");

        if (request.ChosenSkillIds.Count == 0 || request.RejectedSkillIds.Count == 0)
            return OperationResult<ResultGapDpoCandidateWriteResult>.Fail(
                "Correction chosen and rejected skill ids are required.",
                "RESULT_GAP_CORRECTION_CHOSEN_REJECTED_REQUIRED");

        var sourceRoute = _router.Classify(request.Source);
        var refsAreSafe = IsSafeRef(request.PromptRef)
            && IsSafeRef(request.OutputRef)
            && IsSafeRef(request.AiResponseRef)
            && IsSafeRef(request.CorrectionRef);
        var quarantineReason = GetQuarantineReason(
            sourceRoute,
            refsAreSafe,
            "unsafe_correction_ref",
            request.NearMissTaxonomyCode);
        var trainReady = string.IsNullOrWhiteSpace(quarantineReason);
        var provenance = BuildProvenance(request.ProvenanceRefs, request.Source, sourceRoute)
            .Append(new KeyValuePair<string, string>("prompt_ref", request.PromptRef))
            .Append(new KeyValuePair<string, string>("output_ref", request.OutputRef))
            .Append(new KeyValuePair<string, string>("ai_response_ref", request.AiResponseRef))
            .Append(new KeyValuePair<string, string>("correction_ref", request.CorrectionRef))
            .Append(new KeyValuePair<string, string>("source_prompt_ledger_id", request.SourcePromptLedgerId))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        var candidate = new ResultGapTrainableDpoCandidate(
            CandidateId: $"result-gap-correction-{request.SourcePromptLedgerId}-{request.CorrectionId}",
            PairId: $"result-gap-correction-pair-{request.CorrectionId}",
            CandidateKind: "user_correction",
            ChosenRef: $"result-gap-correction:chosen:{request.SourcePromptLedgerId}:{request.CorrectionId}",
            RejectedRef: $"result-gap-correction:near-miss:{request.SourcePromptLedgerId}:{request.CorrectionId}:{request.NearMissTaxonomyCode}",
            ChosenSkillIds: request.ChosenSkillIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            RejectedSkillIds: request.RejectedSkillIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            MissingSkillId: "",
            CorrectionClass: request.CorrectionClass,
            PromptRef: request.PromptRef,
            OutputRef: request.OutputRef,
            AiResponseRef: request.AiResponseRef,
            CorrectionRef: request.CorrectionRef,
            SourcePromptLedgerId: request.SourcePromptLedgerId,
            TargetTrainingAdapter: "IResultGapCorrectionTraining",
            StorageScope: "staged-user-adaptive:smart-skills/result-gap/correction-dpo-signal",
            TrainingRoute: sourceRoute.TrainingRoute == "trainable_common" ? "adaptive_only" : sourceRoute.TrainingRoute,
            UniversalBehaviorTag: "resultgap.user_correction.structured_adaptive_signal",
            NearMissTaxonomyCode: request.NearMissTaxonomyCode,
            RejectedWhyPlausible: request.RejectedWhyPlausible,
            TrainReady: trainReady,
            SemanticQualityClaimed: false,
            ContainsRawPrivateText: false,
            ContainsRawVendorOutput: false,
            ChosenTargetContainsRawPrivateOrVendorOutput: false,
            CommonCheckpointMutationClaimed: false,
            TechnicalExecutionRefsExcluded: true,
            SourceIdUsedAsTrainingFeature: false,
            QuarantineState: trainReady ? "train_ready" : "quarantined",
            QuarantineReason: quarantineReason,
            SourceRoute: sourceRoute,
            ProvenanceRefs: provenance);

        return WriteCandidate(request.OutputPath, candidate, sourceRoute);
    }

    private static OperationResult<ResultGapDpoCandidateWriteResult> WriteCandidate(
        string outputPath,
        ResultGapTrainableDpoCandidate candidate,
        ResultGapSourceRoute sourceRoute)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
            return OperationResult<ResultGapDpoCandidateWriteResult>.Fail(
                "Output path is required.",
                "RESULT_GAP_DPO_OUTPUT_PATH_REQUIRED");

        if (candidate.TrainReady)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            File.AppendAllText(
                outputPath,
                JsonSerializer.Serialize(candidate, JsonOptions) + "\n",
                Utf8NoBom);
        }

        return OperationResult<ResultGapDpoCandidateWriteResult>.Ok(
            new ResultGapDpoCandidateWriteResult(
                CandidatePath: outputPath,
                Candidate: candidate,
                SourceRoute: sourceRoute,
                Written: candidate.TrainReady,
                Quarantined: !candidate.TrainReady));
    }

    private static IEnumerable<KeyValuePair<string, string>> BuildProvenance(
        IReadOnlyDictionary<string, string> input,
        ResultGapSourceDescriptor source,
        ResultGapSourceRoute route)
    {
        foreach (var item in input.OrderBy(item => item.Key, StringComparer.Ordinal))
            yield return item;

        yield return new KeyValuePair<string, string>("source_ref", source.SourceRef);
        yield return new KeyValuePair<string, string>("source_sha256", source.SourceSha256);
        yield return new KeyValuePair<string, string>("source_version_id", source.SourceVersionId);
        yield return new KeyValuePair<string, string>("source_kind", source.SourceKind);
        yield return new KeyValuePair<string, string>("privacy_scope", source.PrivacyScope);
        yield return new KeyValuePair<string, string>("training_route", route.TrainingRoute);
    }

    private static bool IsSafeRef(string value)
        => !string.IsNullOrWhiteSpace(value)
            && value.Length <= 160
            && !value.Any(char.IsWhiteSpace)
            && !value.Contains(@":\", StringComparison.Ordinal)
            && !value.Contains("C:/Users/", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
            && value.Contains("-ref:", StringComparison.Ordinal);

    private string GetQuarantineReason(
        ResultGapSourceRoute sourceRoute,
        bool refsAreSafe,
        string unsafeRefReason,
        string nearMissTaxonomyCode)
    {
        if (sourceRoute.Quarantined)
            return sourceRoute.QuarantineReason;

        if (!refsAreSafe)
            return unsafeRefReason;

        return _taxonomy.IsTrainReady(nearMissTaxonomyCode)
            ? ""
            : "near_miss_taxonomy_not_train_ready";
    }
}
