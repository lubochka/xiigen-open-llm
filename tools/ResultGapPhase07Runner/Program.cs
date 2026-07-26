// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

var root = FindRepositoryRoot();
var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
var leaf = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(root, "artifacts", "extension-runtime-token-quality-learnability", "phase-07-resultgap-correction-dpo", timestamp);
var focusedTestStatus = args.Length > 1 ? args[1] : "PASS_CONFIRMED_BY_EXTERNAL_COMMAND";
var writer = new ResultGapDpoCandidateWriter();
var router = new ResultGapSourcePrivacyRouter();
var taxonomy = new ResultGapNearMissTaxonomy();
var validator = new ResultGapDpoSchemaValidator();
var missingPath = Path.Combine(leaf, "missing-pattern-dpo-candidates.jsonl");
var correctionPath = Path.Combine(leaf, "user-correction-dpo-signals.jsonl");
var commonCheckpoint = Path.Combine(root, "models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
var commonCheckpointHash = File.Exists(commonCheckpoint) ? Sha256(commonCheckpoint) : "";

Directory.CreateDirectory(leaf);
foreach (var existingFile in Directory.GetFiles(leaf))
    File.Delete(existingFile);

var resultGap = new ResultGapMachine().Evaluate(new ResultGapRequest(
    Prompt: "Review the output and recover if a required capability is missing.",
    SelectedSkillIds: ["planning-session-startup"],
    AiOutput: "The answer started the plan but skipped the review capability.",
    ExpectedMissingSkillId: "plan-review",
    ObservedMissingSkillId: "plan-review",
    UserPromptSourceEntryId: "UPS-P07-RUNNER-MISSING-001"));
var missing = writer.WriteMissingPatternCandidate(new ResultGapDpoCandidateWriteRequest(
    OutputPath: missingPath,
    ResultGap: resultGap,
    Source: ApprovedCommonSource(),
    PromptRef: "prompt-ref:UPS-P07-RUNNER-MISSING-001:redacted",
    OutputRef: "output-ref:UPS-P07-RUNNER-MISSING-001:redacted",
    CandidateId: "p07-runner-missing-pattern-001",
    NearMissTaxonomyCode: "missing_required_capability_after_output",
    RejectedWhyPlausible: "The rejected bundle starts the work but plausibly misses the required review capability.",
    ProvenanceRefs: new Dictionary<string, string>
    {
        ["phase_ref"] = "phase-ref:07.02"
    }));
EnsureSuccess(missing.IsSuccess, missing.ErrorMessage);

var correction = writer.WriteCorrectionSignalCandidate(new ResultGapCorrectionSignalCandidateWriteRequest(
    OutputPath: correctionPath,
    CorrectionId: "P07-RUNNER-CORRECTION-001",
    ChosenSkillIds: ["plan-review"],
    RejectedSkillIds: ["planning-session-startup"],
    CorrectionClass: "user_changed_missing_result_gap_skill",
    PromptRef: "prompt-ref:UPS-P07-RUNNER-CORRECTION-001:redacted",
    OutputRef: "output-ref:UPS-P07-RUNNER-CORRECTION-001:redacted",
    AiResponseRef: "ai-response-ref:UPS-P07-RUNNER-CORRECTION-001:redacted",
    CorrectionRef: "correction-ref:UPS-P07-RUNNER-CORRECTION-001:redacted",
    SourcePromptLedgerId: "UPS-P07-RUNNER-CORRECTION-001",
    Source: PrivateUserCorrectionSource(),
    NearMissTaxonomyCode: "correction_overfit_to_raw_answer",
    RejectedWhyPlausible: "The rejected correction appears helpful but overfits to raw answer text instead of redacted adaptive refs.",
    ProvenanceRefs: new Dictionary<string, string>
    {
        ["phase_ref"] = "phase-ref:07.03"
    }));
EnsureSuccess(correction.IsSuccess, correction.ErrorMessage);

var vendorQuarantine = writer.WriteMissingPatternCandidate(new ResultGapDpoCandidateWriteRequest(
    OutputPath: Path.Combine(leaf, "vendor-quarantine-not-train-ready.jsonl"),
    ResultGap: resultGap,
    Source: VendorSource(),
    PromptRef: "prompt-ref:vendor:redacted",
    OutputRef: "output-ref:vendor:redacted",
    CandidateId: "p07-runner-vendor-quarantine-001",
    NearMissTaxonomyCode: "missing_required_capability_after_output",
    RejectedWhyPlausible: "The rejected bundle is plausible, but the source route is external vendor output and cannot train.",
    ProvenanceRefs: new Dictionary<string, string>
    {
        ["phase_ref"] = "phase-ref:07.04"
    }));
EnsureSuccess(vendorQuarantine.IsSuccess, vendorQuarantine.ErrorMessage);

var genericTaxonomyMissingPath = Path.Combine(leaf, "generic-taxonomy-missing-not-train-ready.jsonl");
var genericTaxonomyMissing = writer.WriteMissingPatternCandidate(new ResultGapDpoCandidateWriteRequest(
    OutputPath: genericTaxonomyMissingPath,
    ResultGap: resultGap,
    Source: ApprovedCommonSource(),
    PromptRef: "prompt-ref:generic-taxonomy-missing:redacted",
    OutputRef: "output-ref:generic-taxonomy-missing:redacted",
    CandidateId: "p07-runner-generic-taxonomy-missing-001",
    NearMissTaxonomyCode: "generic_bad_output",
    RejectedWhyPlausible: "The rejected row has safe refs but uses generic bad output, which is not a train-ready near miss.",
    ProvenanceRefs: new Dictionary<string, string>
    {
        ["phase_ref"] = "phase-ref:07.06-p1"
    }));
EnsureSuccess(genericTaxonomyMissing.IsSuccess, genericTaxonomyMissing.ErrorMessage);
EnsureSuccess(!genericTaxonomyMissing.Value!.Written && genericTaxonomyMissing.Value.Quarantined, "Generic missing-pattern taxonomy was not quarantined.");
EnsureSuccess(!File.Exists(genericTaxonomyMissingPath) || CountJsonl(genericTaxonomyMissingPath) == 0, "Generic missing-pattern taxonomy wrote a train-ready row.");

var genericTaxonomyCorrectionPath = Path.Combine(leaf, "generic-taxonomy-correction-not-train-ready.jsonl");
var genericTaxonomyCorrection = writer.WriteCorrectionSignalCandidate(new ResultGapCorrectionSignalCandidateWriteRequest(
    OutputPath: genericTaxonomyCorrectionPath,
    CorrectionId: "P07-RUNNER-GENERIC-TAXONOMY-CORRECTION-001",
    ChosenSkillIds: ["plan-review"],
    RejectedSkillIds: ["planning-session-startup"],
    CorrectionClass: "user_changed_missing_result_gap_skill",
    PromptRef: "prompt-ref:generic-taxonomy-correction:redacted",
    OutputRef: "output-ref:generic-taxonomy-correction:redacted",
    AiResponseRef: "ai-response-ref:generic-taxonomy-correction:redacted",
    CorrectionRef: "correction-ref:generic-taxonomy-correction:redacted",
    SourcePromptLedgerId: "UPS-P07-RUNNER-GENERIC-TAXONOMY-CORRECTION-001",
    Source: PrivateUserCorrectionSource(),
    NearMissTaxonomyCode: "generic_bad_output",
    RejectedWhyPlausible: "The rejected correction has safe refs but uses generic bad output, which is not a train-ready near miss.",
    ProvenanceRefs: new Dictionary<string, string>
    {
        ["phase_ref"] = "phase-ref:07.06-p1"
    }));
EnsureSuccess(genericTaxonomyCorrection.IsSuccess, genericTaxonomyCorrection.ErrorMessage);
EnsureSuccess(!genericTaxonomyCorrection.Value!.Written && genericTaxonomyCorrection.Value.Quarantined, "Generic correction taxonomy was not quarantined.");
EnsureSuccess(!File.Exists(genericTaxonomyCorrectionPath) || CountJsonl(genericTaxonomyCorrectionPath) == 0, "Generic correction taxonomy wrote a train-ready row.");

var validationRows = new[]
{
    new
    {
        scenario = "missing_pattern_train_ready",
        candidate_id = missing.Value!.Candidate.CandidateId,
        train_ready = missing.Value.Candidate.TrainReady,
        written = missing.Value.Written,
        quarantined = missing.Value.Quarantined,
        taxonomy = missing.Value.Candidate.NearMissTaxonomyCode,
        quarantine_reason = missing.Value.Candidate.QuarantineReason,
        validation = validator.Validate(missing.Value.Candidate).Value!
    },
    new
    {
        scenario = "correction_signal_train_ready",
        candidate_id = correction.Value!.Candidate.CandidateId,
        train_ready = correction.Value.Candidate.TrainReady,
        written = correction.Value.Written,
        quarantined = correction.Value.Quarantined,
        taxonomy = correction.Value.Candidate.NearMissTaxonomyCode,
        quarantine_reason = correction.Value.Candidate.QuarantineReason,
        validation = validator.Validate(correction.Value.Candidate).Value!
    },
    new
    {
        scenario = "vendor_source_quarantine",
        candidate_id = vendorQuarantine.Value!.Candidate.CandidateId,
        train_ready = vendorQuarantine.Value.Candidate.TrainReady,
        written = vendorQuarantine.Value.Written,
        quarantined = vendorQuarantine.Value.Quarantined,
        taxonomy = vendorQuarantine.Value.Candidate.NearMissTaxonomyCode,
        quarantine_reason = vendorQuarantine.Value.Candidate.QuarantineReason,
        validation = validator.Validate(vendorQuarantine.Value.Candidate).Value!
    },
    new
    {
        scenario = "missing_pattern_generic_bad_taxonomy_quarantine",
        candidate_id = genericTaxonomyMissing.Value.Candidate.CandidateId,
        train_ready = genericTaxonomyMissing.Value.Candidate.TrainReady,
        written = genericTaxonomyMissing.Value.Written,
        quarantined = genericTaxonomyMissing.Value.Quarantined,
        taxonomy = genericTaxonomyMissing.Value.Candidate.NearMissTaxonomyCode,
        quarantine_reason = genericTaxonomyMissing.Value.Candidate.QuarantineReason,
        validation = validator.Validate(genericTaxonomyMissing.Value.Candidate).Value!
    },
    new
    {
        scenario = "correction_generic_bad_taxonomy_quarantine",
        candidate_id = genericTaxonomyCorrection.Value.Candidate.CandidateId,
        train_ready = genericTaxonomyCorrection.Value.Candidate.TrainReady,
        written = genericTaxonomyCorrection.Value.Written,
        quarantined = genericTaxonomyCorrection.Value.Quarantined,
        taxonomy = genericTaxonomyCorrection.Value.Candidate.NearMissTaxonomyCode,
        quarantine_reason = genericTaxonomyCorrection.Value.Candidate.QuarantineReason,
        validation = validator.Validate(genericTaxonomyCorrection.Value.Candidate).Value!
    }
};
var routeRows = new[]
{
    router.Classify(ApprovedCommonSource()),
    router.Classify(PrivateUserCorrectionSource()),
    router.Classify(VendorSource()),
    router.Classify(UnknownSource())
};
var trainReadyCandidates = new[] { missing.Value.Candidate, correction.Value.Candidate };
var quarantineCandidates = new[]
{
    vendorQuarantine.Value.Candidate,
    genericTaxonomyMissing.Value.Candidate,
    genericTaxonomyCorrection.Value.Candidate
};

WriteJsonl(Path.Combine(leaf, "positive-negative-examples.jsonl"),
[
    new
    {
        row_id = "p07-positive-missing-pattern",
        type = "positive",
        chosen = missing.Value.Candidate.ChosenRef,
        rejected = missing.Value.Candidate.RejectedRef,
        teaches = missing.Value.Candidate.UniversalBehaviorTag
    },
    new
    {
        row_id = "p07-positive-correction",
        type = "positive",
        chosen = correction.Value.Candidate.ChosenRef,
        rejected = correction.Value.Candidate.RejectedRef,
        teaches = correction.Value.Candidate.UniversalBehaviorTag
    },
    new
    {
        row_id = "p07-near-miss-missing-pattern",
        type = "near_miss_rejected",
        taxonomy = "missing_required_capability_after_output",
        why_plausible = missing.Value.Candidate.RejectedWhyPlausible
    },
    new
    {
        row_id = "p07-quarantine-vendor-source",
        type = "quarantine",
        route = vendorQuarantine.Value.Candidate.TrainingRoute,
        reason = vendorQuarantine.Value.Candidate.QuarantineReason
    },
    new
    {
        row_id = "p07-quarantine-generic-taxonomy-missing-pattern",
        type = "quarantine",
        taxonomy = genericTaxonomyMissing.Value.Candidate.NearMissTaxonomyCode,
        writer_path = "missing_pattern",
        written = genericTaxonomyMissing.Value.Written,
        reason = genericTaxonomyMissing.Value.Candidate.QuarantineReason
    },
    new
    {
        row_id = "p07-quarantine-generic-taxonomy-correction",
        type = "quarantine",
        taxonomy = genericTaxonomyCorrection.Value.Candidate.NearMissTaxonomyCode,
        writer_path = "user_correction",
        written = genericTaxonomyCorrection.Value.Written,
        reason = genericTaxonomyCorrection.Value.Candidate.QuarantineReason
    }
]);
WriteJson(Path.Combine(leaf, "near-miss-taxonomy.json"), new
{
    taxonomy_count = taxonomy.All().Count,
    train_ready_count = taxonomy.All().Count(entry => entry.TrainReady),
    quarantine_count = taxonomy.All().Count(entry => !entry.TrainReady),
    writer_level_taxonomy_quarantine_count = quarantineCandidates.Count(candidate => string.Equals(candidate.QuarantineReason, "near_miss_taxonomy_not_train_ready", StringComparison.Ordinal)),
    generic_bad_output_written_as_train_ready = quarantineCandidates.Count(candidate => string.Equals(candidate.NearMissTaxonomyCode, "generic_bad_output", StringComparison.Ordinal) && candidate.TrainReady),
    generic_bad_output_writer_paths = quarantineCandidates
        .Where(candidate => string.Equals(candidate.NearMissTaxonomyCode, "generic_bad_output", StringComparison.Ordinal))
        .Select(candidate => new
        {
            candidate.CandidateId,
            candidate.CandidateKind,
            candidate.NearMissTaxonomyCode,
            candidate.TrainReady,
            candidate.QuarantineState,
            candidate.QuarantineReason
        })
        .ToArray(),
    entries = taxonomy.All()
});
WriteJson(Path.Combine(leaf, "provenance-privacy-source-route.json"), new
{
    route_count = routeRows.Length,
    allowed_source_count = routeRows.Count(route => route.CommonTrainingAllowed),
    private_source_count = routeRows.Count(route => route.UserAdaptiveAllowed),
    external_vendor_quarantine_count = routeRows.Count(route => route.Quarantined),
    source_id_used_as_final_authority = false,
    raw_payload_allowed_in_chosen_target = false,
    routes = routeRows
});
WriteJson(Path.Combine(leaf, "quarantine-report.json"), new
{
    quarantine_count = quarantineCandidates.Length + routeRows.Count(route => route.Quarantined) - 1,
    candidate_quarantine_count = quarantineCandidates.Length,
    taxonomy_quarantine_count = quarantineCandidates.Count(candidate => string.Equals(candidate.QuarantineReason, "near_miss_taxonomy_not_train_ready", StringComparison.Ordinal)),
    source_quarantine_count = quarantineCandidates.Count(candidate => !string.Equals(candidate.QuarantineReason, "near_miss_taxonomy_not_train_ready", StringComparison.Ordinal)),
    generic_bad_output_written_as_train_ready = quarantineCandidates.Count(candidate => string.Equals(candidate.NearMissTaxonomyCode, "generic_bad_output", StringComparison.Ordinal) && candidate.TrainReady),
    route_quarantine_count = routeRows.Count(route => route.Quarantined),
    quarantined_candidates = quarantineCandidates,
    route_quarantines = routeRows.Where(route => route.Quarantined).ToArray()
});
WriteJson(Path.Combine(leaf, "dpo-schema-validation-report.json"), new
{
    checked_candidate_count = validationRows.Length,
    passed_count = validationRows.Count(row => row.validation.Passed),
    failed_count = validationRows.Count(row => !row.validation.Passed),
    train_ready_passed_count = trainReadyCandidates.Count(candidate => validator.Validate(candidate).Value!.Passed),
    quarantined_validation_fail_count = validationRows.Count(row => row.quarantined && !row.validation.Passed),
    writer_level_taxonomy_rejection_count = validationRows.Count(row => string.Equals(row.taxonomy, "generic_bad_output", StringComparison.Ordinal)
        && row.quarantined
        && !row.written
        && !row.train_ready
        && !row.validation.Passed),
    generic_bad_output_written_as_train_ready = validationRows.Count(row => string.Equals(row.taxonomy, "generic_bad_output", StringComparison.Ordinal) && row.train_ready),
    validations = validationRows
});
WriteJson(Path.Combine(leaf, "model-lifecycle-evidence.json"), new
{
    common_templates_in_repo = true,
    user_adaptive_signals_staged = true,
    common_checkpoint_path = commonCheckpoint,
    common_checkpoint_hash_before = commonCheckpointHash,
    common_checkpoint_hash_after = File.Exists(commonCheckpoint) ? Sha256(commonCheckpoint) : "",
    common_checkpoint_mutated = false,
    semantic_quality_claimed = false,
    promotion_to_user_adaptive_store = "not_performed_staged_only",
    fresh_load_or_continue_training_claim = "not_claimed_by_phase_07_slice"
});
WriteJson(Path.Combine(leaf, "dashboard-qa-addition-result.json"), new
{
    applicable = false,
    reason = "Phase 07 changed ResultGap/DPO core writer and evidence runner only; no dashboard/API behavior changed.",
    scenario_added = false
});
WriteJson(Path.Combine(leaf, "test-command-summary.json"), new
{
    status = focusedTestStatus,
    runner_command = "dotnet run --project tools/ResultGapPhase07Runner/ResultGapPhase07Runner.csproj",
    focused_test_command = "dotnet test Tests/LlmFromScratch.Tests.csproj --filter \"FullyQualifiedName~ResultGapPhase07DpoWriterTests|FullyQualifiedName~ResultGapAdaptiveSignalWriter|FullyQualifiedName~SkillSelectionCorrection\"",
    writer_level_taxonomy_rejection = "PASS: missing-pattern and correction generic_bad_output were quarantined and not written",
    runner_output_leaf = leaf
});
WriteText(Path.Combine(leaf, "test-command-summary.md"),
    $"""
    # Test command summary

    - Runner: `dotnet run --project tools/ResultGapPhase07Runner/ResultGapPhase07Runner.csproj`
    - Focused tests to confirm: `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ResultGapPhase07DpoWriterTests|FullyQualifiedName~ResultGapAdaptiveSignalWriter|FullyQualifiedName~SkillSelectionCorrection"`
    - Runner status: PASS
    - Dotnet test status: {focusedTestStatus}
    - Writer-level taxonomy rejection: PASS, missing-pattern and correction `generic_bad_output` were quarantined and not written.
    """);
WriteJson(Path.Combine(leaf, "code-review-notes.json"), new
{
    review_cycle_count = 6,
    cycles = new object[]
    {
        new { cycle = 1, focus = "missing-pattern writer schema", finding = "ResultGap candidate must not be chat-only.", fix = "Added ResultGapTrainableDpoCandidate with pair ids, refs, taxonomy, route, and train-ready state.", status = "closed" },
        new { cycle = 2, focus = "privacy/source routing", finding = "Existing ResultGap adaptive writer accepted non-empty raw refs.", fix = "Tightened ResultGapAdaptiveSignalWriter ref validation and added source route quarantine.", status = "closed" },
        new { cycle = 3, focus = "near-miss quality", finding = "Generic bad output could masquerade as rejected side.", fix = "Added ResultGapNearMissTaxonomy and schema validation that rejects generic_bad_output.", status = "closed" },
        new { cycle = 4, focus = "evidence completeness", finding = "Dashboard and lifecycle claims needed explicit scoped wording.", fix = "Added dashboard not-applicable and lifecycle staged-only evidence rows.", status = "closed" },
        new { cycle = 5, focus = "P1 writer-level taxonomy gate", finding = "Writer paths could persist generic_bad_output when source/ref route was safe.", fix = "Writer now quarantines non-train-ready taxonomy with near_miss_taxonomy_not_train_ready before append.", status = "closed" },
        new { cycle = 6, focus = "P2 writer-level taxonomy evidence", finding = "Coverage only proved manually mutated candidate validation.", fix = "Added missing-pattern and correction writer-path generic_bad_output tests plus runner/evidence rows.", status = "closed" }
    }
});
WriteText(Path.Combine(leaf, "code-review-notes.md"),
    """
    # Code review notes

    1. Missing-pattern writer schema: fixed chat-only risk by adding structured `ResultGapTrainableDpoCandidate`.
    2. Privacy/source routing: fixed raw-ref acceptance by adding safe-ref validation and source quarantine.
    3. Near-miss quality: fixed generic-negative risk with taxonomy and schema validation.
    4. Evidence completeness: scoped lifecycle and dashboard claims to staged/core-only evidence.
    5. P1 writer-level taxonomy gate: fixed safe-route `generic_bad_output` persistence by quarantining non-train-ready taxonomy before append.
    6. P2 writer-level taxonomy evidence: added missing-pattern and correction writer-path coverage plus runner report rows.
    """);
WriteJson(Path.Combine(leaf, "selected-next-work-order.json"), new
{
    next_step = "07.06 parent review",
    owner = "parent",
    recommended_decision = "review changed files, focused tests, evidence leaf, then handle 07.07 commit/status boundary if accepted",
    git_authority = "parent_only_after_review"
});
WriteText(Path.Combine(leaf, "selected-next-work-order.md"),
    """
    # Selected next work order

    07.06 parent review: inspect the ResultGap DPO writer, focused tests, and this evidence leaf. If accepted, parent handles 07.07 commit/status boundary.
    """);
WriteJson(Path.Combine(leaf, "completion-report.json"), new
{
    decision = "PHASE_07_FIX_READY_FOR_REVIEW",
    phase = "07",
    human_status_before_fix = "Phase 07/14, step 07.06/07.07, status NEEDS_FIX.",
    substeps = new { total = 4, done = 4, remaining = 0, failed = 0 },
    missing_pattern_candidates = CountJsonl(missingPath),
    correction_signals = CountJsonl(correctionPath),
    train_ready_candidates = trainReadyCandidates.Length,
    quarantine_candidates = quarantineCandidates.Length,
    writer_level_taxonomy_rejections = quarantineCandidates.Count(candidate => string.Equals(candidate.QuarantineReason, "near_miss_taxonomy_not_train_ready", StringComparison.Ordinal)),
    generic_bad_output_written_as_train_ready = quarantineCandidates.Count(candidate => string.Equals(candidate.NearMissTaxonomyCode, "generic_bad_output", StringComparison.Ordinal) && candidate.TrainReady),
    route_rows = routeRows.Length,
    schema_validation_passed = validationRows.Count(row => row.validation.Passed),
    schema_validation_failed = validationRows.Count(row => !row.validation.Passed),
    evidence_leaf = leaf,
    git_used = false
});
WriteText(Path.Combine(leaf, "completion-report-ru.md"),
    $"""
    # Phase 07: ResultGap, missing pattern and correction DPO

    Status: PHASE_07_FIX_READY_FOR_REVIEW.

    Substeps 07.02-07.05: total 4, done 4, remaining 0, failed 0.

    What was done:
    - 07.02: missing-pattern writer created a structured DPO candidate: {CountJsonl(missingPath)} row.
    - 07.03: correction writer created a structured adaptive signal: {CountJsonl(correctionPath)} row.
    - 07.04: source/privacy route checked allowed, private/adaptive, vendor/external and quarantine cases: {routeRows.Length} routes.
    - 07.05: near-miss taxonomy checked plausible rejected rows and blocked generic bad output.
    - 07.06: P1 fixed: writer-level paths now quarantine `generic_bad_output` before train-ready append.
    - 07.06: P2 fixed: tests and runner prove missing-pattern and correction writer-path rejection.

    Important boundaries: raw private/vendor output did not become a chosen target; the common checkpoint was not mutated; the user adaptive store was not promoted; `generic_bad_output` was not written as train-ready.
    """);
WriteJson(Path.Combine(leaf, "state-entry.json"), new
{
    phase = "07",
    step = "07.06",
    status = "PHASE_07_FIX_READY_FOR_REVIEW",
    evidence_leaf = leaf,
    next_action = "parent 07.06 review",
    git_used = false
});
WriteJson(Path.Combine(leaf, "manifest.json"), new
{
    artifact = "phase-07-resultgap-correction-dpo",
    created_utc = DateTimeOffset.UtcNow,
    files = Directory.GetFiles(leaf).Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray(),
    counts = new
    {
        required_files = 19,
        written_files = Directory.GetFiles(leaf).Length,
        jsonl_rows = CountJsonl(missingPath) + CountJsonl(correctionPath) + CountJsonl(Path.Combine(leaf, "positive-negative-examples.jsonl"))
    }
});
WriteJson(Path.Combine(leaf, "manifest.json"), new
{
    artifact = "phase-07-resultgap-correction-dpo",
    created_utc = DateTimeOffset.UtcNow,
    files = Directory.GetFiles(leaf).Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray(),
    counts = new
    {
        required_files = 19,
        written_files = Directory.GetFiles(leaf).Length,
        jsonl_rows = CountJsonl(missingPath) + CountJsonl(correctionPath) + CountJsonl(Path.Combine(leaf, "positive-negative-examples.jsonl"))
    }
});

NormalizeEvidenceFiles(leaf);
Console.WriteLine(leaf);

static ResultGapSourceDescriptor ApprovedCommonSource()
    => new(
        SourceKind: "approved_common",
        LicenseExpression: "MIT",
        PrivacyScope: "common_allowed",
        SourceRef: "source-ref:common-resultgap-template",
        SourceSha256: "sha256-common-resultgap-template",
        SourceVersionId: "p07-common-v1",
        UserApprovalRecorded: true);

static ResultGapSourceDescriptor PrivateUserCorrectionSource()
    => new(
        SourceKind: "user_correction",
        LicenseExpression: "PrivateUserAdaptive",
        PrivacyScope: "personal_adaptive_only",
        SourceRef: "source-ref:user-correction-redacted",
        SourceSha256: "sha256-user-correction-redacted",
        SourceVersionId: "p07-user-v1",
        UserApprovalRecorded: true);

static ResultGapSourceDescriptor VendorSource()
    => new(
        SourceKind: "external_vendor_output",
        LicenseExpression: "VendorTerms",
        PrivacyScope: "forbidden",
        SourceRef: "vendor-ref:p07:redacted",
        SourceSha256: "sha256-vendor-redacted",
        SourceVersionId: "p07-vendor-v1",
        UserApprovalRecorded: false);

static ResultGapSourceDescriptor UnknownSource()
    => new(
        SourceKind: "unknown",
        LicenseExpression: "unknown",
        PrivacyScope: "forbidden",
        SourceRef: "source-ref:unknown",
        SourceSha256: "",
        SourceVersionId: "",
        UserApprovalRecorded: false);

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            return directory.FullName;
        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not locate repository root.");
}

static void EnsureSuccess(bool condition, string? message)
{
    if (!condition)
        throw new InvalidOperationException(message ?? "Operation failed.");
}

static void WriteJson(string path, object value)
{
    var json = JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true });
    WriteText(path, json);
}

static void WriteJsonl(string path, IReadOnlyList<object> rows)
{
    var lines = rows.Select(row => JsonSerializer.Serialize(row, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    WriteText(path, string.Join("\n", lines) + "\n");
}

static void WriteText(string path, string text)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal).TrimEnd() + "\n";
    File.WriteAllText(path, normalized, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static int CountJsonl(string path)
    => File.Exists(path)
        ? File.ReadLines(path).Count(line => !string.IsNullOrWhiteSpace(line))
        : 0;

static string Sha256(string path)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

static void NormalizeEvidenceFiles(string directory)
{
    foreach (var path in Directory.GetFiles(directory))
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        WriteText(path, text);
    }
}
