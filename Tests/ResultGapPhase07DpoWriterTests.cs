// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using SmartSkillsDomain;

public sealed class ResultGapPhase07DpoWriterTests
{
    [Fact]
    public void ResultGapDpoCandidateWriter_WritesStructuredMissingPatternCandidate()
    {
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"phase07-missing-pattern-{Guid.NewGuid():N}",
            "missing-pattern-dpo-candidates.jsonl");
        var resultGap = new ResultGapMachine().Evaluate(new ResultGapRequest(
            Prompt: "Review output and recover when a required capability is missing.",
            SelectedSkillIds: ["planning-session-startup"],
            AiOutput: "The answer planned startup only and skipped review.",
            ExpectedMissingSkillId: "plan-review",
            ObservedMissingSkillId: "plan-review",
            UserPromptSourceEntryId: "UPS-P07-MISSING-001"));
        var writer = new ResultGapDpoCandidateWriter();

        try
        {
            var write = writer.WriteMissingPatternCandidate(new ResultGapDpoCandidateWriteRequest(
                OutputPath: outputPath,
                ResultGap: resultGap,
                Source: ApprovedCommonSource(),
                PromptRef: "prompt-ref:UPS-P07-MISSING-001:redacted",
                OutputRef: "output-ref:UPS-P07-MISSING-001:redacted",
                CandidateId: "p07-missing-pattern-001",
                NearMissTaxonomyCode: "missing_required_capability_after_output",
                RejectedWhyPlausible: "The rejected bundle starts the work but plausibly misses the required review capability.",
                ProvenanceRefs: new Dictionary<string, string>
                {
                    ["case_ref"] = "case-ref:p07-missing-pattern"
                }));
            var persisted = JsonSerializer.Deserialize<ResultGapTrainableDpoCandidate>(
                File.ReadAllLines(outputPath).Single(),
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var validation = new ResultGapDpoSchemaValidator().Validate(write.Value!.Candidate);

            Assert.True(write.IsSuccess, write.ErrorMessage);
            Assert.True(write.Value.Written);
            Assert.False(write.Value.Quarantined);
            Assert.NotNull(persisted);
            Assert.Equal("missing_pattern", persisted!.CandidateKind);
            Assert.Equal("trainable_common", persisted.TrainingRoute);
            Assert.Equal("repo-common:smart-skills/result-gap/missing-pattern-dpo-template", persisted.StorageScope);
            Assert.Equal("resultgap.missing_pattern.requires_second_pass_capability", persisted.UniversalBehaviorTag);
            Assert.Equal("IResultGapTraining", persisted.TargetTrainingAdapter);
            Assert.Equal("train_ready", persisted.QuarantineState);
            Assert.False(persisted.ContainsRawPrivateText);
            Assert.False(persisted.ChosenTargetContainsRawPrivateOrVendorOutput);
            Assert.False(persisted.CommonCheckpointMutationClaimed);
            Assert.False(persisted.SourceIdUsedAsTrainingFeature);
            Assert.True(persisted.TechnicalExecutionRefsExcluded);
            Assert.True(validation.IsSuccess, validation.ErrorMessage);
            Assert.True(validation.Value!.Passed);
            Assert.DoesNotContain(resultGap.DpoRow!.Prompt, File.ReadAllText(outputPath), StringComparison.Ordinal);
        }
        finally
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (directory is not null && Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ResultGapDpoCandidateWriter_WritesStructuredAdaptiveCorrectionSignal()
    {
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"phase07-correction-{Guid.NewGuid():N}",
            "user-correction-dpo-signals.jsonl");
        var writer = new ResultGapDpoCandidateWriter();
        var rawPrompt = "Please replace the selected startup skill with review.";
        var rawVendorAnswer = "Vendor model text selected planning-session-startup.";

        try
        {
            var write = writer.WriteCorrectionSignalCandidate(new ResultGapCorrectionSignalCandidateWriteRequest(
                OutputPath: outputPath,
                CorrectionId: "P07-CORRECTION-001",
                ChosenSkillIds: ["plan-review"],
                RejectedSkillIds: ["planning-session-startup"],
                CorrectionClass: "user_changed_missing_result_gap_skill",
                PromptRef: "prompt-ref:UPS-P07-CORRECTION-001:redacted",
                OutputRef: "output-ref:UPS-P07-CORRECTION-001:redacted",
                AiResponseRef: "ai-response-ref:UPS-P07-CORRECTION-001:redacted",
                CorrectionRef: "correction-ref:UPS-P07-CORRECTION-001:redacted",
                SourcePromptLedgerId: "UPS-P07-CORRECTION-001",
                Source: PrivateUserCorrectionSource(),
                NearMissTaxonomyCode: "correction_overfit_to_raw_answer",
                RejectedWhyPlausible: "The rejected correction appears helpful but overfits to raw answer text instead of redacted adaptive refs.",
                ProvenanceRefs: new Dictionary<string, string>
                {
                    ["raw_prompt_hash_ref"] = "hash-ref:prompt:UPS-P07-CORRECTION-001"
                }));
            var line = File.ReadAllLines(outputPath).Single();
            var persisted = JsonSerializer.Deserialize<ResultGapTrainableDpoCandidate>(
                line,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var validation = new ResultGapDpoSchemaValidator().Validate(write.Value!.Candidate);

            Assert.True(write.IsSuccess, write.ErrorMessage);
            Assert.True(write.Value.Written);
            Assert.False(write.Value.Quarantined);
            Assert.NotNull(persisted);
            Assert.Equal("user_correction", persisted!.CandidateKind);
            Assert.Equal("adaptive_only", persisted.TrainingRoute);
            Assert.Equal("IResultGapCorrectionTraining", persisted.TargetTrainingAdapter);
            Assert.Equal("staged-user-adaptive:smart-skills/result-gap/correction-dpo-signal", persisted.StorageScope);
            Assert.Equal("resultgap.user_correction.structured_adaptive_signal", persisted.UniversalBehaviorTag);
            Assert.Equal("user_changed_missing_result_gap_skill", persisted.CorrectionClass);
            Assert.False(persisted.SourceRoute.CommonTrainingAllowed);
            Assert.True(persisted.SourceRoute.UserAdaptiveAllowed);
            Assert.False(persisted.ContainsRawPrivateText);
            Assert.False(persisted.ContainsRawVendorOutput);
            Assert.True(validation.Value!.Passed);
            Assert.DoesNotContain(rawPrompt, line, StringComparison.Ordinal);
            Assert.DoesNotContain(rawVendorAnswer, line, StringComparison.Ordinal);
        }
        finally
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (directory is not null && Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ResultGapDpoCandidateWriter_QuarantinesGenericBadTaxonomyBeforeTrainReadyPersistence()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"phase07-generic-taxonomy-{Guid.NewGuid():N}");
        var missingPath = Path.Combine(directory, "missing-pattern-dpo-candidates.jsonl");
        var correctionPath = Path.Combine(directory, "user-correction-dpo-signals.jsonl");
        var resultGap = new ResultGapMachine().Evaluate(new ResultGapRequest(
            Prompt: "Review output and recover when a required capability is missing.",
            SelectedSkillIds: ["planning-session-startup"],
            AiOutput: "The answer planned startup only and skipped review.",
            ExpectedMissingSkillId: "plan-review",
            ObservedMissingSkillId: "plan-review",
            UserPromptSourceEntryId: "UPS-P07-GENERIC-TAXONOMY-001"));
        var writer = new ResultGapDpoCandidateWriter();
        var validator = new ResultGapDpoSchemaValidator();

        try
        {
            var missingWrite = writer.WriteMissingPatternCandidate(new ResultGapDpoCandidateWriteRequest(
                OutputPath: missingPath,
                ResultGap: resultGap,
                Source: ApprovedCommonSource(),
                PromptRef: "prompt-ref:UPS-P07-GENERIC-TAXONOMY-001:redacted",
                OutputRef: "output-ref:UPS-P07-GENERIC-TAXONOMY-001:redacted",
                CandidateId: "p07-generic-taxonomy-missing-001",
                NearMissTaxonomyCode: "generic_bad_output",
                RejectedWhyPlausible: "The rejected row has safe refs but uses generic bad output, which is not a train-ready near miss.",
                ProvenanceRefs: new Dictionary<string, string>
                {
                    ["case_ref"] = "case-ref:p07-generic-taxonomy-missing"
                }));
            var correctionWrite = writer.WriteCorrectionSignalCandidate(new ResultGapCorrectionSignalCandidateWriteRequest(
                OutputPath: correctionPath,
                CorrectionId: "P07-GENERIC-TAXONOMY-CORRECTION-001",
                ChosenSkillIds: ["plan-review"],
                RejectedSkillIds: ["planning-session-startup"],
                CorrectionClass: "user_changed_missing_result_gap_skill",
                PromptRef: "prompt-ref:UPS-P07-GENERIC-TAXONOMY-CORRECTION-001:redacted",
                OutputRef: "output-ref:UPS-P07-GENERIC-TAXONOMY-CORRECTION-001:redacted",
                AiResponseRef: "ai-response-ref:UPS-P07-GENERIC-TAXONOMY-CORRECTION-001:redacted",
                CorrectionRef: "correction-ref:UPS-P07-GENERIC-TAXONOMY-CORRECTION-001:redacted",
                SourcePromptLedgerId: "UPS-P07-GENERIC-TAXONOMY-CORRECTION-001",
                Source: PrivateUserCorrectionSource(),
                NearMissTaxonomyCode: "generic_bad_output",
                RejectedWhyPlausible: "The rejected correction has safe refs but uses generic bad output, which is not a train-ready near miss.",
                ProvenanceRefs: new Dictionary<string, string>
                {
                    ["case_ref"] = "case-ref:p07-generic-taxonomy-correction"
                }));
            var missingValidation = validator.Validate(missingWrite.Value!.Candidate).Value!;
            var correctionValidation = validator.Validate(correctionWrite.Value!.Candidate).Value!;

            Assert.True(missingWrite.IsSuccess, missingWrite.ErrorMessage);
            Assert.True(correctionWrite.IsSuccess, correctionWrite.ErrorMessage);
            Assert.False(missingWrite.Value.Written);
            Assert.False(correctionWrite.Value.Written);
            Assert.True(missingWrite.Value.Quarantined);
            Assert.True(correctionWrite.Value.Quarantined);
            Assert.False(missingWrite.Value.Candidate.TrainReady);
            Assert.False(correctionWrite.Value.Candidate.TrainReady);
            Assert.Equal("quarantined", missingWrite.Value.Candidate.QuarantineState);
            Assert.Equal("quarantined", correctionWrite.Value.Candidate.QuarantineState);
            Assert.Equal("near_miss_taxonomy_not_train_ready", missingWrite.Value.Candidate.QuarantineReason);
            Assert.Equal("near_miss_taxonomy_not_train_ready", correctionWrite.Value.Candidate.QuarantineReason);
            Assert.False(File.Exists(missingPath));
            Assert.False(File.Exists(correctionPath));
            Assert.False(missingValidation.Passed);
            Assert.False(correctionValidation.Passed);
            Assert.Contains("near_miss_taxonomy", missingValidation.FailedFields);
            Assert.Contains("near_miss_taxonomy", correctionValidation.FailedFields);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ResultGapSourcePrivacyRouter_ClassifiesCommonAdaptiveAndVendorRoutes()
    {
        var router = new ResultGapSourcePrivacyRouter();
        var common = router.Classify(ApprovedCommonSource());
        var adaptive = router.Classify(PrivateUserCorrectionSource());
        var vendor = router.Classify(new ResultGapSourceDescriptor(
            SourceKind: "external_vendor_output",
            LicenseExpression: "VendorTerms",
            PrivacyScope: "forbidden",
            SourceRef: "vendor-ref:p07:redacted",
            SourceSha256: "vendor-sha",
            SourceVersionId: "vendor-v1",
            UserApprovalRecorded: false));

        Assert.Equal("trainable_common", common.TrainingRoute);
        Assert.True(common.CommonTrainingAllowed);
        Assert.False(common.RawPayloadAllowedInChosenTarget);
        Assert.False(common.SourceIdUsedAsTrainingFeature);
        Assert.Equal("adaptive_only", adaptive.TrainingRoute);
        Assert.True(adaptive.UserAdaptiveAllowed);
        Assert.False(adaptive.CommonTrainingAllowed);
        Assert.Equal("quarantine", vendor.TrainingRoute);
        Assert.True(vendor.Quarantined);
        Assert.False(vendor.CommonTrainingAllowed);
        Assert.False(vendor.UserAdaptiveAllowed);
    }

    [Fact]
    public void ResultGapNearMissTaxonomy_AllowsPlausibleFailuresAndRejectsGenericBadRows()
    {
        var taxonomy = new ResultGapNearMissTaxonomy();
        var validator = new ResultGapDpoSchemaValidator();
        var baseCandidate = new ResultGapTrainableDpoCandidate(
            CandidateId: "p07-taxonomy-001",
            PairId: "p07-taxonomy-pair",
            CandidateKind: "missing_pattern",
            ChosenRef: "chosen-ref:p07-taxonomy:redacted",
            RejectedRef: "rejected-ref:p07-taxonomy:redacted",
            ChosenSkillIds: ["plan-review"],
            RejectedSkillIds: ["planning-session-startup"],
            MissingSkillId: "plan-review",
            CorrectionClass: "",
            PromptRef: "prompt-ref:p07-taxonomy:redacted",
            OutputRef: "output-ref:p07-taxonomy:redacted",
            AiResponseRef: "",
            CorrectionRef: "",
            SourcePromptLedgerId: "UPS-P07-TAXONOMY-001",
            TargetTrainingAdapter: "IResultGapTraining",
            StorageScope: "repo-common:smart-skills/result-gap/missing-pattern-dpo-template",
            TrainingRoute: "trainable_common",
            UniversalBehaviorTag: "resultgap.missing_pattern.requires_second_pass_capability",
            NearMissTaxonomyCode: "missing_required_capability_after_output",
            RejectedWhyPlausible: "The rejected row starts the work and plausibly misses the required review capability.",
            TrainReady: true,
            SemanticQualityClaimed: false,
            ContainsRawPrivateText: false,
            ContainsRawVendorOutput: false,
            ChosenTargetContainsRawPrivateOrVendorOutput: false,
            CommonCheckpointMutationClaimed: false,
            TechnicalExecutionRefsExcluded: true,
            SourceIdUsedAsTrainingFeature: false,
            QuarantineState: "train_ready",
            QuarantineReason: "",
            SourceRoute: new ResultGapSourcePrivacyRouter().Classify(ApprovedCommonSource()),
            ProvenanceRefs: new Dictionary<string, string>());
        var genericBad = baseCandidate with
        {
            NearMissTaxonomyCode = "generic_bad_output",
            RejectedWhyPlausible = "bad output"
        };

        Assert.True(taxonomy.IsTrainReady("missing_required_capability_after_output"));
        Assert.True(taxonomy.IsTrainReady("correction_overfit_to_raw_answer"));
        Assert.False(taxonomy.IsTrainReady("generic_bad_output"));
        Assert.True(validator.Validate(baseCandidate).Value!.Passed);
        Assert.False(validator.Validate(genericBad).Value!.Passed);
        Assert.Contains("near_miss_taxonomy", validator.Validate(genericBad).Value!.FailedFields);
        Assert.Contains("rejected_plausibility", validator.Validate(genericBad).Value!.FailedFields);
    }

    [Fact]
    public void ResultGapAdaptiveSignalWriter_RejectsRawPrivatePromptAndOutputRefs()
    {
        var writer = new ResultGapAdaptiveSignalWriter();
        var resultGap = new ResultGapMachine().Evaluate(new ResultGapRequest(
            Prompt: "Use planning and plan-review skills.",
            SelectedSkillIds: ["planning-session-startup"],
            AiOutput: "The answer contains planning only.",
            ExpectedMissingSkillId: "plan-review",
            ObservedMissingSkillId: "plan-review",
            UserPromptSourceEntryId: "UPS-P07-RAW-REF-001"));
        var write = writer.WriteUserLocalSignal(new ResultGapAdaptiveSignalWriteRequest(
            UserLocalRoot: Path.Combine(Path.GetTempPath(), $"phase07-raw-ref-{Guid.NewGuid():N}"),
            ResultGap: resultGap,
            PromptRef: @"C:\Users\private-user\private-prompt.txt",
            OutputRef: "raw output text",
            SourcePromptLedgerId: "UPS-P07-RAW-REF-001"));

        Assert.False(write.IsSuccess);
        Assert.Equal("RESULT_GAP_REDACTED_REFS_REQUIRED", write.ErrorCode);
    }

    private static ResultGapSourceDescriptor ApprovedCommonSource()
        => new(
            SourceKind: "approved_common",
            LicenseExpression: "MIT",
            PrivacyScope: "common_allowed",
            SourceRef: "source-ref:common-resultgap-template",
            SourceSha256: "sha256-common-resultgap-template",
            SourceVersionId: "p07-common-v1",
            UserApprovalRecorded: true);

    private static ResultGapSourceDescriptor PrivateUserCorrectionSource()
        => new(
            SourceKind: "user_correction",
            LicenseExpression: "PrivateUserAdaptive",
            PrivacyScope: "personal_adaptive_only",
            SourceRef: "source-ref:user-correction-redacted",
            SourceSha256: "sha256-user-correction-redacted",
            SourceVersionId: "p07-user-v1",
            UserApprovalRecorded: true);
}
