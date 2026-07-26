// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;

public sealed class ExtensionEvalOnlyQualityRunner
{
    public const string ReportSchema = "xiigen.quality_02.eval_only_runner.report.v1";
    public const string OutputRowSchema = "xiigen.quality_02.eval_only_runner.score_row.v1";
    public const string UserAdaptiveEvalOnlyRoute = "USER_ADAPTIVE_EVAL_ONLY";
    public const string EvidenceOnlyRoute = "EVIDENCE_ONLY";
    public const string CommonBlockedRoute = "COMMON_BLOCKED";
    public const string ScoreStatusScored = "SCORED_EVAL_ONLY";
    public const string ScoreStatusEvidenceOnly = "NON_RUNNABLE_EVIDENCE_ONLY";
    public const string ScoreStatusCommonBlocked = "COMMON_BLOCKED";
    public const string LocalPrivateEvalOnlyEvidenceScope = "local_private_eval_only";
    public const string BaselineVsAdaptivePairMode = "baseline_vs_adaptive_pair";
    public const string DeterministicScorerLabel = "eval-only/helper/internal-detail/baseline; not semantic, trainable, runtime, or product authority";
    public const string PrivacyHelperLabel = "privacy helper/shared-output guard; not semantic, trainable, runtime, or product authority";

    private static readonly IReadOnlySet<string> AllowedHosts = new HashSet<string>(StringComparer.Ordinal)
    {
        "Claude",
        "Cursor",
        "Codex"
    };

    private static readonly IReadOnlySet<string> AllowedMethods = new HashSet<string>(StringComparer.Ordinal)
    {
        "prompt-to-task",
        "orchestration-plan",
        "skill-selection"
    };

    private static readonly IReadOnlySet<string> AllowedPrivacyStatuses = new HashSet<string>(StringComparer.Ordinal)
    {
        "PASS_REDACTED",
        "PRIVATE_ONLY_NO_SHARED_TEXT"
    };

    private static readonly IReadOnlySet<string> AllowedSharedRefPrefixes = new HashSet<string>(StringComparer.Ordinal)
    {
        "artifact://",
        "redacted://",
        "evidence://",
        "user-local://approved-",
        "user-local://synthetic-"
    };

    private static readonly string[] RawPrivateTextMarkers =
    [
        "PRIVATE_SECRET",
        "DO_NOT_EXPORT",
        "RAW_PRIVATE_TEXT",
        "raw private text",
        "raw-private-text",
        "BEGIN_PRIVATE",
        "private://",
        "secret://"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonOptions)
    {
        WriteIndented = false
    };

    public OperationResult<ExtensionEvalOnlyQualityRunnerReport> Run(ExtensionEvalOnlyQualityRunnerOptions options)
    {
        var optionValidation = ValidateOptions(options);
        if (!optionValidation.IsSuccess)
            return optionValidation.MapError<ExtensionEvalOnlyQualityRunnerReport>();

        var rowsResult = ReadInputRows(options.InputJsonlPath);
        if (!rowsResult.IsSuccess || rowsResult.Value is null)
            return rowsResult.MapError<ExtensionEvalOnlyQualityRunnerReport>();

        var validation = ValidateRows(rowsResult.Value);
        if (!validation.IsSuccess || validation.Value is null)
            return validation.MapError<ExtensionEvalOnlyQualityRunnerReport>();
        if (!validation.Value.Passed)
            return OperationResult<ExtensionEvalOnlyQualityRunnerReport>.Fail(
                $"Eval-only runner input failed validation: {string.Join("; ", validation.Value.Errors)}",
                "EVAL_ONLY_INPUT_VALIDATION_FAILED");

        var report = BuildReport(options, rowsResult.Value, validation.Value);
        if (options.WriteReports)
        {
            var writeResult = WriteReportFiles(options, report);
            if (!writeResult.IsSuccess || writeResult.Value is null)
                return writeResult.MapError<ExtensionEvalOnlyQualityRunnerReport>();

            report = report with { OutputRefs = writeResult.Value };
        }

        return OperationResult<ExtensionEvalOnlyQualityRunnerReport>.Ok(report);
    }

    public OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>> ReadInputRows(string inputJsonlPath)
    {
        if (string.IsNullOrWhiteSpace(inputJsonlPath))
            return OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>>.Fail("Input JSONL path is required.", "INPUT_JSONL_PATH_REQUIRED");
        if (!File.Exists(inputJsonlPath))
            return OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>>.Fail($"Input JSONL file not found: {inputJsonlPath}", "INPUT_JSONL_NOT_FOUND");

        var rows = new List<ExtensionEvalOnlyQualityRunnerInputRow>();
        var lineNumber = 0;
        try
        {
            foreach (var line in File.ReadLines(inputJsonlPath, Encoding.UTF8))
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var row = JsonSerializer.Deserialize<ExtensionEvalOnlyQualityRunnerInputRow>(line, JsonOptions);
                if (row is null)
                    return OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>>.Fail($"JSONL line {lineNumber} did not contain an input row.", "JSONL_NULL_ROW");

                rows.Add(row);
            }
        }
        catch (JsonException ex)
        {
            return OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>>.Fail($"Malformed JSONL at line {lineNumber}: {ex.Message}", "MALFORMED_JSONL");
        }
        catch (IOException ex)
        {
            return OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>>.Fail($"Could not read input JSONL: {ex.Message}", "INPUT_JSONL_READ_FAILED");
        }
        catch (UnauthorizedAccessException ex)
        {
            return OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>>.Fail($"Could not read input JSONL: {ex.Message}", "INPUT_JSONL_READ_FORBIDDEN");
        }

        return OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>>.Ok(rows);
    }

    public OperationResult<ExtensionEvalOnlyQualityRunnerValidation> ValidateRows(
        IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>? rows)
    {
        if (rows is null)
            return OperationResult<ExtensionEvalOnlyQualityRunnerValidation>.Fail("Input rows are required.", "INPUT_ROWS_REQUIRED");

        var errors = new List<string>();
        var warnings = new List<string>();
        if (rows.Count == 0)
            errors.Add("input_jsonl_has_no_rows");

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var rowName = BuildValidationRowName(row.PairId, index);
            RequireText(errors, rowName, "scenario_id", row.ScenarioId);
            RequireText(errors, rowName, "pair_id", row.PairId);
            RequireText(errors, rowName, "route", row.Route);
            if (!IsKnownRoute(row.Route))
                errors.Add($"{rowName}: route must be USER_ADAPTIVE_EVAL_ONLY, EVIDENCE_ONLY, or COMMON_BLOCKED.");

            ValidateSharedOutputPrivacyHelperFields(errors, rowName, row);
            if (string.Equals(row.Route, UserAdaptiveEvalOnlyRoute, StringComparison.Ordinal))
                ValidateRunnableRow(errors, warnings, rowName, row);
        }

        return OperationResult<ExtensionEvalOnlyQualityRunnerValidation>.Ok(
            new ExtensionEvalOnlyQualityRunnerValidation(errors.Count == 0, errors, warnings));
    }

    public OperationResult<IReadOnlyList<string>> WriteReportFiles(
        ExtensionEvalOnlyQualityRunnerOptions? options,
        ExtensionEvalOnlyQualityRunnerReport? report)
    {
        if (options is null)
            return OperationResult<IReadOnlyList<string>>.Fail("Runner options are required.", "OPTIONS_REQUIRED");
        if (report is null)
            return OperationResult<IReadOnlyList<string>>.Fail("Runner report is required.", "REPORT_REQUIRED");
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
            return OperationResult<IReadOnlyList<string>>.Fail("Output directory is required.", "OUTPUT_DIRECTORY_REQUIRED");

        try
        {
            Directory.CreateDirectory(options.OutputDirectory);
            var reportJsonPath = Path.Combine(options.OutputDirectory, "quality-02-eval-only-runner-report.json");
            var rowsJsonlPath = Path.Combine(options.OutputDirectory, "quality-02-eval-only-runner-score-rows.jsonl");
            var markdownPath = Path.Combine(options.OutputDirectory, "quality-02-eval-only-runner-report.md");
            IReadOnlyList<string> outputRefs =
            [
                NormalizePath(reportJsonPath),
                NormalizePath(rowsJsonlPath),
                NormalizePath(markdownPath)
            ];
            var reportWithRefs = report with { OutputRefs = outputRefs };
            File.WriteAllText(reportJsonPath, JsonSerializer.Serialize(reportWithRefs, JsonOptions) + Environment.NewLine, Encoding.UTF8);
            File.WriteAllLines(rowsJsonlPath, report.OutputRows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions)), Encoding.UTF8);
            File.WriteAllText(markdownPath, BuildMarkdown(reportWithRefs), Encoding.UTF8);
            return OperationResult<IReadOnlyList<string>>.Ok(outputRefs);
        }
        catch (IOException ex)
        {
            return OperationResult<IReadOnlyList<string>>.Fail($"Could not write eval-only runner reports: {ex.Message}", "REPORT_WRITE_FAILED");
        }
        catch (UnauthorizedAccessException ex)
        {
            return OperationResult<IReadOnlyList<string>>.Fail($"Could not write eval-only runner reports: {ex.Message}", "REPORT_WRITE_FORBIDDEN");
        }
    }

    private static OperationResult<bool> ValidateOptions(ExtensionEvalOnlyQualityRunnerOptions? options)
    {
        if (options is null)
            return OperationResult<bool>.Fail("Runner options are required.", "OPTIONS_REQUIRED");
        if (string.IsNullOrWhiteSpace(options.InputJsonlPath))
            return OperationResult<bool>.Fail("Input JSONL path is required.", "INPUT_JSONL_PATH_REQUIRED");
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
            return OperationResult<bool>.Fail("Output directory is required.", "OUTPUT_DIRECTORY_REQUIRED");
        if (!string.Equals(options.EvidenceScope, LocalPrivateEvalOnlyEvidenceScope, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Evidence scope must be local_private_eval_only.", "EVIDENCE_SCOPE_REQUIRED");
        if (options.SyntheticFixtureOnly && options.ActualApprovedEvalExecuted)
            return OperationResult<bool>.Fail("Synthetic fixture and actual approved eval boundaries cannot both be true.", "EVAL_BOUNDARY_CONFLICT");
        if (!options.SyntheticFixtureOnly && !options.ActualApprovedEvalExecuted)
            return OperationResult<bool>.Fail("Runner evidence must declare either synthetic fixture only or actual approved eval execution.", "EVAL_BOUNDARY_REQUIRED");
        return OperationResult<bool>.Ok(true);
    }

    private static void ValidateRunnableRow(
        List<string> errors,
        List<string> warnings,
        string rowName,
        ExtensionEvalOnlyQualityRunnerInputRow row)
    {
        RequireText(errors, rowName, "evidence_scope", row.EvidenceScope);
        RequireText(errors, rowName, "prompt_sha256", row.PromptSha256);
        RequireText(errors, rowName, "mode", row.Mode);
        RequireText(errors, rowName, "approval_ref", row.ApprovalRef);
        RequireText(errors, rowName, "user_local_root_ref", row.UserLocalRootRef);
        RequireText(errors, rowName, "raw_private_input_ref", row.RawPrivateInputRef);
        RequireText(errors, rowName, "redacted_shared_evidence_ref", row.RedactedSharedEvidenceRef);
        RequireText(errors, rowName, "source_ref", row.SourceRef);
        RequireText(errors, rowName, "doc_id", row.DocId);
        RequireText(errors, rowName, "host_name", row.HostName);
        RequireText(errors, rowName, "method", row.Method);
        RequireText(errors, rowName, "baseline_output_ref", row.BaselineOutputRef);
        RequireText(errors, rowName, "adaptive_output_ref", row.AdaptiveOutputRef);
        RequireText(errors, rowName, "privacy_redaction_status", row.PrivacyRedactionStatus);
        if (!AllowedHosts.Contains(row.HostName))
            errors.Add($"{rowName}: host_name must be Claude, Cursor, or Codex.");
        if (!AllowedMethods.Contains(row.Method))
            errors.Add($"{rowName}: method must be prompt-to-task, orchestration-plan, or skill-selection.");
        if (!AllowedPrivacyStatuses.Contains(row.PrivacyRedactionStatus))
            errors.Add($"{rowName}: privacy_redaction_status must be PASS_REDACTED or PRIVATE_ONLY_NO_SHARED_TEXT.");
        if (!string.Equals(row.EvidenceScope, LocalPrivateEvalOnlyEvidenceScope, StringComparison.Ordinal))
            errors.Add($"{rowName}: evidence_scope must be local_private_eval_only.");
        if (!string.Equals(row.Mode, BaselineVsAdaptivePairMode, StringComparison.Ordinal))
            errors.Add($"{rowName}: mode must be baseline_vs_adaptive_pair.");
        if (!row.NoCommonMutationAttestation)
            errors.Add($"{rowName}: no_common_mutation_attestation must be true.");
        if (!row.NoTrainingMutationAttestation)
            errors.Add($"{rowName}: no_training_mutation_attestation must be true.");
        if (!row.NoModelCheckpointLocatorMutationAttestation)
            errors.Add($"{rowName}: no_model_checkpoint_locator_mutation_attestation must be true.");
        ValidateCopiedText(errors, rowName, "prompt_sha256", row.PromptSha256);
        ValidateSharedRef(errors, rowName, "redacted_shared_evidence_ref", row.RedactedSharedEvidenceRef);
        ValidateSharedRef(errors, rowName, "baseline_output_ref", row.BaselineOutputRef);
        ValidateSharedRef(errors, rowName, "adaptive_output_ref", row.AdaptiveOutputRef);
        ValidateCopiedTextCollection(errors, rowName, "failure_codes", row.FailureCodes);
        if (row.LayerInputs is null)
        {
            errors.Add($"{rowName}: layer_inputs must be a JSON array, not null.");
            return;
        }

        if (row.LayerInputs.Count == 0)
            warnings.Add($"{rowName}: layer_inputs is empty, so scoring will fail rather than opening a quality claim.");
        for (var layerIndex = 0; layerIndex < row.LayerInputs.Count; layerIndex++)
        {
            var layer = row.LayerInputs[layerIndex];
            if (layer is null)
            {
                errors.Add($"{rowName}: layer_inputs[{layerIndex}] must be an object, not null.");
                continue;
            }

            var layerName = string.IsNullOrWhiteSpace(layer.LayerId)
                ? $"{rowName}:layer_index:{layerIndex}"
                : BuildValidationLayerName(rowName, layer.LayerId, layerIndex);
            ValidateCopiedText(errors, layerName, "layer_id", layer.LayerId);
            ValidateLayerScoreRange(errors, layerName, "baseline_score", layer.BaselineScore);
            ValidateLayerScoreRange(errors, layerName, "adaptive_score", layer.AdaptiveScore);
            ValidateCopiedText(errors, layerName, "reviewer_note", layer.ReviewerNote);
            ValidateSharedRefCollection(errors, layerName, "evidence_refs", layer.EvidenceRefs);
            ValidateCopiedTextCollection(errors, layerName, "failure_codes", layer.FailureCodes);
        }
    }

    private static ExtensionEvalOnlyQualityRunnerReport BuildReport(
        ExtensionEvalOnlyQualityRunnerOptions options,
        IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow> rows,
        ExtensionEvalOnlyQualityRunnerValidation validation)
    {
        var generatedAt = options.GeneratedAtUtc.ToString("O");
        var runId = string.IsNullOrWhiteSpace(options.RunId)
            ? $"quality-02-eval-only-runner-{options.GeneratedAtUtc:yyyyMMdd-HHmmss}"
            : options.RunId;
        var pairResults = rows.Select(row => BuildPairResult(options, runId, generatedAt, row, rows)).ToArray();
        var outputRows = pairResults.Where(result => result.OutputRow is not null).Select(result => result.OutputRow!).ToArray();
        var report = new ExtensionEvalOnlyQualityRunnerReport(
            Schema: ReportSchema,
            PhaseId: options.PhaseId,
            RunId: runId,
            GeneratedAtUtc: generatedAt,
            EvidenceScope: options.EvidenceScope,
            SyntheticFixtureOnly: options.SyntheticFixtureOnly,
            ActualApprovedEvalExecuted: options.ActualApprovedEvalExecuted,
            TotalInputRows: rows.Count,
            UserAdaptiveEvalOnlyRows: rows.Count(row => string.Equals(row.Route, UserAdaptiveEvalOnlyRoute, StringComparison.Ordinal)),
            ScoredOutputRows: outputRows.Length,
            EvidenceOnlyExcludedRows: rows.Count(row => string.Equals(row.Route, EvidenceOnlyRoute, StringComparison.Ordinal)),
            CommonBlockedExcludedRows: rows.Count(row => string.Equals(row.Route, CommonBlockedRoute, StringComparison.Ordinal)),
            ValidationErrorCount: validation.Errors.Count,
            PairResults: pairResults,
            OutputRows: outputRows,
            Validation: validation,
            ClaimLocks: new ExtensionEvalOnlyQualityRunnerClaimLocks(),
            OutputRefs: [],
            HumanBoardRu: BuildHumanBoard(rows, outputRows.Length),
            WhatThisProves: options.SyntheticFixtureOnly
                ? "The runner can validate the eval-only contract and write score-shaped evidence from a synthetic fixture only."
                : "The runner can validate approved eval-only rows and write eval-only score evidence for USER_ADAPTIVE_EVAL_ONLY rows only.",
            WhatThisDoesNotProve: $"This does not prove product quality, semantic readiness, learnability, token savings, live host readiness, common readiness, actual approved eval when synthetic_fixture_only=true, target promotion, locator update, or final readiness. Deterministic privacy validation is a {PrivacyHelperLabel}.",
            NextIndependentTrack: "Parent review, commit/push boundary, then a separately approved eval run or next bounded quality phase.",
            EvalRunnerExecuted: true,
            HostRuntimeStarted: false,
            TrainingStarted: false,
            ModelCheckpointLocatorMutated: false);
        return report;
    }

    private static ExtensionEvalOnlyQualityRunnerPairResult BuildPairResult(
        ExtensionEvalOnlyQualityRunnerOptions options,
        string runId,
        string generatedAt,
        ExtensionEvalOnlyQualityRunnerInputRow row,
        IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow> allRows)
    {
        if (string.Equals(row.Route, EvidenceOnlyRoute, StringComparison.Ordinal))
            return BuildExcludedPair(options, row, ScoreStatusEvidenceOnly, "Evidence-only rows are context only and are not scored.");
        if (string.Equals(row.Route, CommonBlockedRoute, StringComparison.Ordinal))
            return BuildExcludedPair(options, row, ScoreStatusCommonBlocked, "Common-blocked rows are excluded from scoring and common training.");
        if (!string.Equals(row.Route, UserAdaptiveEvalOnlyRoute, StringComparison.Ordinal))
            return BuildExcludedPair(options, row, "BLOCKED_BY_ROUTE", "Unknown route is not scored.");

        var normalizedLayers = row.LayerInputs.Select(ToQualityLayerScore).ToArray();
        var regressionFailures = row.LayerInputs
            .Where(layer => layer.AdaptiveScore < layer.BaselineScore)
            .Select(layer => $"FAIL_QUALITY_REGRESSION:{NormalizePublicLayerId(layer.LayerId)}")
            .ToArray();
        var forbiddenFailures = row.FailureCodes
            .Concat(regressionFailures)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var scoreResult = ExtensionQualityScoring.Score(new ExtensionQualityScoreInput(
            ScenarioId: row.ScenarioId,
            PairId: row.PairId,
            LayerScores: normalizedLayers,
            ForbiddenFailures: forbiddenFailures,
            PrivacyAndRedactionPassed: AllowedPrivacyStatuses.Contains(row.PrivacyRedactionStatus),
            TokenComparison: null));
        var allFailureCodes = scoreResult.Reasons
            .Concat(forbiddenFailures)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var qualityRegression = regressionFailures.Length > 0;
        var outputRow = new ExtensionEvalOnlyQualityRunnerOutputRow(
            Schema: OutputRowSchema,
            RecordType: "quality_02_eval_only_score_row",
            PhaseId: options.PhaseId,
            RunId: runId,
            GeneratedAtUtc: generatedAt,
            ScenarioId: row.ScenarioId,
            PairId: row.PairId,
            Route: row.Route,
            ScoreStatus: ScoreStatusScored,
            EvidenceScope: row.EvidenceScope,
            Mode: row.Mode,
            HostName: row.HostName,
            Method: row.Method,
            PromptSha256: row.PromptSha256,
            BaselineOutputRef: row.BaselineOutputRef,
            AdaptiveOutputRef: row.AdaptiveOutputRef,
            PerLayerScores: row.LayerInputs.Select(ToLayerScoreRow).ToArray(),
            FailureCodes: allFailureCodes,
            HumanReviewerNote: BuildOutputReviewerNote(scoreResult, qualityRegression),
            InterRaterStatus: "EVAL_ONLY_SINGLE_REVIEWER_HELPER",
            QualityRegression: qualityRegression,
            QualityImprovementClaimed: false,
            SemanticQualityClaimAllowed: false,
            DeterministicScorerLabel: DeterministicScorerLabel,
            ClaimLocks: new ExtensionEvalOnlyQualityRunnerClaimLocks(),
            RawPrivateTextCopied: false,
            SyntheticFixtureOnly: options.SyntheticFixtureOnly,
            ActualApprovedEvalExecuted: options.ActualApprovedEvalExecuted,
            NoModelHostTrainingExecution: true,
            TotalInputRows: allRows.Count,
            ScoredOutputRows: allRows.Count(item => string.Equals(item.Route, UserAdaptiveEvalOnlyRoute, StringComparison.Ordinal)),
            EvidenceOnlyExcludedRows: allRows.Count(item => string.Equals(item.Route, EvidenceOnlyRoute, StringComparison.Ordinal)),
            CommonBlockedExcludedRows: allRows.Count(item => string.Equals(item.Route, CommonBlockedRoute, StringComparison.Ordinal)));
        return new ExtensionEvalOnlyQualityRunnerPairResult(
            ScenarioId: row.ScenarioId,
            PairId: row.PairId,
            Route: row.Route,
            Scored: true,
            ScoreStatus: ScoreStatusScored,
            EvidenceScope: row.EvidenceScope,
            SyntheticFixtureOnly: options.SyntheticFixtureOnly,
            ActualApprovedEvalExecuted: options.ActualApprovedEvalExecuted,
            Verdict: scoreResult.Verdict,
            AverageAdaptiveScore: scoreResult.AverageScore,
            QualityRegression: qualityRegression,
            FailureCodes: allFailureCodes,
            HumanReviewerNote: outputRow.HumanReviewerNote,
            DeterministicScorerLabel: DeterministicScorerLabel,
            ClaimLocks: new ExtensionEvalOnlyQualityRunnerClaimLocks(),
            OutputRow: outputRow);
    }

    private static ExtensionEvalOnlyQualityRunnerPairResult BuildExcludedPair(
        ExtensionEvalOnlyQualityRunnerOptions options,
        ExtensionEvalOnlyQualityRunnerInputRow row,
        string scoreStatus,
        string note)
        => new(
            ScenarioId: row.ScenarioId,
            PairId: row.PairId,
            Route: row.Route,
            Scored: false,
            ScoreStatus: scoreStatus,
            EvidenceScope: string.IsNullOrWhiteSpace(row.EvidenceScope) ? options.EvidenceScope : row.EvidenceScope,
            SyntheticFixtureOnly: options.SyntheticFixtureOnly,
            ActualApprovedEvalExecuted: options.ActualApprovedEvalExecuted,
            Verdict: null,
            AverageAdaptiveScore: null,
            QualityRegression: false,
            FailureCodes: scoreStatus is ScoreStatusEvidenceOnly ? ["NON_RUNNABLE_EVIDENCE_ONLY"] : ["COMMON_BLOCKED_NO_PERMISSIVE_SOURCE"],
            HumanReviewerNote: note,
            DeterministicScorerLabel: DeterministicScorerLabel,
            ClaimLocks: new ExtensionEvalOnlyQualityRunnerClaimLocks(),
            OutputRow: null);

    private static ExtensionQualityLayerScore ToQualityLayerScore(ExtensionEvalOnlyQualityRunnerLayerInput layer)
        => new(
            LayerId: NormalizeScoringLayerId(layer.LayerId),
            HumanName: HumanizeLayer(layer.LayerId),
            Score: layer.AdaptiveScore,
            EvidenceRefs: layer.EvidenceRefs,
            FailureCategories: layer.FailureCodes,
            HumanReviewerNote: layer.ReviewerNote);

    private static ExtensionEvalOnlyQualityRunnerLayerScoreRow ToLayerScoreRow(ExtensionEvalOnlyQualityRunnerLayerInput layer)
    {
        var regressed = layer.AdaptiveScore < layer.BaselineScore;
        var passFail = string.IsNullOrWhiteSpace(layer.ReviewerNote)
            ? "FAIL"
            : regressed
                ? "FAIL"
                : layer.AdaptiveScore >= ExtensionQualityScoring.PassingLayerScoreMin
                    ? "PASS"
                    : "FAIL";
        var failureCodes = layer.FailureCodes
            .Concat(regressed ? [$"FAIL_QUALITY_REGRESSION:{NormalizePublicLayerId(layer.LayerId)}"] : [])
            .Concat(string.IsNullOrWhiteSpace(layer.ReviewerNote) ? [$"MISSING_REVIEWER_NOTE:{NormalizePublicLayerId(layer.LayerId)}"] : [])
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return new ExtensionEvalOnlyQualityRunnerLayerScoreRow(
            LayerId: NormalizePublicLayerId(layer.LayerId),
            BaselineScore: layer.BaselineScore,
            AdaptiveScore: layer.AdaptiveScore,
            PassFail: passFail,
            QualityRegressed: regressed,
            FailureCodes: failureCodes,
            EvidenceRefs: layer.EvidenceRefs,
            ReviewerNote: layer.ReviewerNote,
            DeterministicScorerLabel: DeterministicScorerLabel);
    }

    private static string BuildOutputReviewerNote(ExtensionQualityScoreResult scoreResult, bool qualityRegression)
    {
        var regressionNote = qualityRegression
            ? " At least one adaptive layer regressed against baseline, so quality improvement remains closed."
            : string.Empty;
        return $"{scoreResult.HumanReviewerNote}{regressionNote} Deterministic rubric scoring is eval-only helper evidence, not semantic/trainable authority.";
    }

    private static string BuildHumanBoard(
        IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow> rows,
        int outputRowCount)
        => string.Join(
            Environment.NewLine,
            "Total: 1 bounded code slice for the QUALITY-02 eval-only runner.",
            "Done: runner validation, all-route shared-output privacy helper validation, provenance/scope gates, USER_ADAPTIVE_EVAL_ONLY scoring path, exclusion counts, JSON/JSONL/Markdown reports, and claim locks.",
            "In progress now: bounded code worker evidence packet for parent review.",
            $"Remaining: parent review plus commit/push boundary; actual product eval remains separately approved. Scored rows now: {outputRowCount}.",
            "Type of current result: code and eval-only evidence, not product readiness.",
            $"Accepted evidence: evidence_scope={LocalPrivateEvalOnlyEvidenceScope}; input rows {rows.Count}; evidence-only excluded {rows.Count(row => string.Equals(row.Route, EvidenceOnlyRoute, StringComparison.Ordinal))}; common-blocked excluded {rows.Count(row => string.Equals(row.Route, CommonBlockedRoute, StringComparison.Ordinal))}.",
            "Closed claims and why: token savings, quality improvement, semantic readiness, learnability, live host readiness, common readiness, training, target promotion, locator update and final readiness remain false because this runner writes eval-only helper evidence only.",
            "Open gaps: no live host runtime, no training, no checkpoint mutation, no common promotion, no final readiness review.",
            "Next action: parent reviews this slice and handles the commit/push boundary.",
            "Execution paused for plan-review visibility: no.");

    private static string BuildMarkdown(ExtensionEvalOnlyQualityRunnerReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-02 Eval-only Runner Report");
        builder.AppendLine();
        builder.AppendLine(report.HumanBoardRu);
        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine($"- Evidence scope: {report.EvidenceScope}");
        builder.AppendLine($"- Synthetic fixture only: {report.SyntheticFixtureOnly.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Actual approved eval executed: {report.ActualApprovedEvalExecuted.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Total input rows: {report.TotalInputRows}");
        builder.AppendLine($"- USER_ADAPTIVE_EVAL_ONLY rows scored: {report.ScoredOutputRows}");
        builder.AppendLine($"- EVIDENCE_ONLY rows excluded: {report.EvidenceOnlyExcludedRows}");
        builder.AppendLine($"- COMMON_BLOCKED rows excluded: {report.CommonBlockedExcludedRows}");
        builder.AppendLine();
        builder.AppendLine("## Claim Locks");
        builder.AppendLine();
        builder.AppendLine($"- token_savings_claimed: {report.ClaimLocks.TokenSavingsClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- quality_improvement_claimed: {report.ClaimLocks.QualityImprovementClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- semantic_quality_claimed: {report.ClaimLocks.SemanticQualityClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- semantic_readiness_claimed: {report.ClaimLocks.SemanticReadinessClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- learnability_claimed: {report.ClaimLocks.LearnabilityClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- live_host_readiness_claimed: {report.ClaimLocks.LiveHostReadinessClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- common_readiness_claimed: {report.ClaimLocks.CommonReadinessClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- strict_quality03_ready: {report.ClaimLocks.StrictQuality03Ready.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- training_ready_claimed: {report.ClaimLocks.TrainingReadyClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- target_promotion_claimed: {report.ClaimLocks.TargetPromotionClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- locator_update_claimed: {report.ClaimLocks.LocatorUpdateClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- final_readiness_claimed: {report.ClaimLocks.FinalReadinessClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine();
        builder.AppendLine("## Scope");
        builder.AppendLine();
        builder.AppendLine($"What this proves: {report.WhatThisProves}");
        builder.AppendLine();
        builder.AppendLine($"What this does not prove: {report.WhatThisDoesNotProve}");
        builder.AppendLine();
        builder.AppendLine($"Next independent track: {report.NextIndependentTrack}");
        builder.AppendLine();
        builder.AppendLine("## Deterministic Scorer Label");
        builder.AppendLine();
        builder.AppendLine(DeterministicScorerLabel);
        builder.AppendLine();
        builder.AppendLine("## Privacy Helper Label");
        builder.AppendLine();
        builder.AppendLine(PrivacyHelperLabel);
        builder.AppendLine();
        builder.AppendLine("## Rows");
        builder.AppendLine();
        foreach (var result in report.PairResults)
            builder.AppendLine($"- {result.PairId}: {result.ScoreStatus}; scored={result.Scored.ToString().ToLowerInvariant()}; evidence_scope={result.EvidenceScope}; synthetic_fixture_only={result.SyntheticFixtureOnly.ToString().ToLowerInvariant()}; actual_approved_eval_executed={result.ActualApprovedEvalExecuted.ToString().ToLowerInvariant()}; claims_final_readiness={result.ClaimLocks.FinalReadinessClaimed.ToString().ToLowerInvariant()}");
        return builder.ToString();
    }

    private static void RequireText(List<string> errors, string rowName, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            errors.Add($"{rowName}: {fieldName} is required.");
    }

    private static void ValidateSharedRef(List<string> errors, string rowName, string fieldName, string? value)
    {
        ValidateCopiedText(errors, rowName, fieldName, value);
        if (string.IsNullOrWhiteSpace(value))
            return;
        if (!AllowedSharedRefPrefixes.Any(prefix => value.StartsWith(prefix, StringComparison.Ordinal)))
            errors.Add($"{rowName}: {fieldName} must be an approved redacted/shared evidence ref.");
    }

    private static void ValidateSharedRefCollection(
        List<string> errors,
        string rowName,
        string fieldName,
        IReadOnlyList<string>? values)
    {
        if (values is null)
        {
            errors.Add($"{rowName}: {fieldName} must be a JSON array, not null.");
            return;
        }

        for (var index = 0; index < values.Count; index++)
        {
            var itemName = $"{fieldName}[{index}]";
            if (values[index] is null)
            {
                errors.Add($"{rowName}: {itemName} must be a string when present.");
                continue;
            }

            ValidateSharedRef(errors, rowName, itemName, values[index]);
        }
    }

    private static void ValidateSharedOutputPrivacyHelperFields(
        List<string> errors,
        string rowName,
        ExtensionEvalOnlyQualityRunnerInputRow row)
    {
        ValidateCopiedText(errors, rowName, "scenario_id", row.ScenarioId);
        ValidateCopiedText(errors, rowName, "pair_id", row.PairId);
        ValidateCopiedText(errors, rowName, "evidence_scope", row.EvidenceScope);
        if (string.Equals(row.Route, UserAdaptiveEvalOnlyRoute, StringComparison.Ordinal))
            return;

        ValidateCopiedTextCollectionIfPresent(errors, rowName, "failure_codes", row.FailureCodes);
        if (row.LayerInputs is null)
            return;

        for (var layerIndex = 0; layerIndex < row.LayerInputs.Count; layerIndex++)
        {
            var layer = row.LayerInputs[layerIndex];
            if (layer is null)
                continue;

            var layerName = string.IsNullOrWhiteSpace(layer.LayerId)
                ? $"{rowName}:layer_index:{layerIndex}"
                : BuildValidationLayerName(rowName, layer.LayerId, layerIndex);
            ValidateCopiedText(errors, layerName, "layer_id", layer.LayerId);
            ValidateCopiedText(errors, layerName, "reviewer_note", layer.ReviewerNote);
            ValidateCopiedTextCollectionIfPresent(errors, layerName, "evidence_refs", layer.EvidenceRefs);
            ValidateCopiedTextCollectionIfPresent(errors, layerName, "failure_codes", layer.FailureCodes);
        }
    }

    private static void ValidateCopiedTextCollection(
        List<string> errors,
        string rowName,
        string fieldName,
        IReadOnlyList<string>? values)
    {
        if (values is null)
        {
            errors.Add($"{rowName}: {fieldName} must be a JSON array, not null.");
            return;
        }

        for (var index = 0; index < values.Count; index++)
        {
            var itemName = $"{fieldName}[{index}]";
            if (values[index] is null)
            {
                errors.Add($"{rowName}: {itemName} must be a string when present.");
                continue;
            }

            ValidateCopiedText(errors, rowName, itemName, values[index]);
        }
    }

    private static void ValidateCopiedTextCollectionIfPresent(
        List<string> errors,
        string rowName,
        string fieldName,
        IReadOnlyList<string>? values)
    {
        if (values is null)
            return;

        for (var index = 0; index < values.Count; index++)
        {
            if (values[index] is null)
                continue;

            ValidateCopiedText(errors, rowName, $"{fieldName}[{index}]", values[index]);
        }
    }

    private static void ValidateCopiedText(List<string> errors, string rowName, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        if (ContainsRawPrivateText(value))
            errors.Add($"{rowName}: {fieldName} appears to contain raw private text and cannot be copied into shared evidence ({PrivacyHelperLabel}).");
    }

    private static void ValidateLayerScoreRange(List<string> errors, string layerName, string fieldName, int value)
    {
        if (value is < 0 or > 5)
            errors.Add($"{layerName}: {fieldName} must be between 0 and 5 inclusive; actual value was {value}.");
    }

    private static bool ContainsRawPrivateText(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           RawPrivateTextMarkers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static string BuildValidationRowName(string? pairId, int index)
        => string.IsNullOrWhiteSpace(pairId) || ContainsRawPrivateText(pairId)
            ? $"row_index:{index}"
            : pairId;

    private static string BuildValidationLayerName(string rowName, string? layerId, int layerIndex)
        => string.IsNullOrWhiteSpace(layerId) || ContainsRawPrivateText(layerId)
            ? $"{rowName}:layer_index:{layerIndex}"
            : $"{rowName}:{NormalizePublicLayerId(layerId)}";

    private static bool IsKnownRoute(string route)
        => string.Equals(route, UserAdaptiveEvalOnlyRoute, StringComparison.Ordinal) ||
           string.Equals(route, EvidenceOnlyRoute, StringComparison.Ordinal) ||
           string.Equals(route, CommonBlockedRoute, StringComparison.Ordinal);

    private static string NormalizeScoringLayerId(string layerId)
        => string.Equals(layerId, "final_usefulness", StringComparison.Ordinal)
            ? "final_output_usefulness"
            : layerId;

    private static string NormalizePublicLayerId(string layerId)
        => string.Equals(layerId, "final_output_usefulness", StringComparison.Ordinal)
            ? "final_usefulness"
            : layerId;

    private static string HumanizeLayer(string layerId)
        => NormalizePublicLayerId(layerId).Replace('_', ' ');

    private static string NormalizePath(string path)
        => Path.GetFullPath(path).Replace('\\', '/');
}
