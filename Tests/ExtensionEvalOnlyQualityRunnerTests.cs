// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;

public sealed class ExtensionEvalOnlyQualityRunnerTests
{
    [Fact]
    public void Run_RejectsMalformedJsonlWithHumanReadableError()
    {
        using var workspace = TempWorkspace();
        var inputPath = Path.Combine(workspace, "rows.jsonl");
        File.WriteAllText(inputPath, "{ not-json", Encoding.UTF8);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.False(result.IsSuccess);
        Assert.Equal("MALFORMED_JSONL", result.ErrorCode);
        Assert.Contains("Malformed JSONL", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("line 1", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_RejectsDeserializedNullCollectionsWithOperationResultAndNoReports()
    {
        AssertNullCollectionRejected(
            "pair-null-layer-inputs",
            root => root["layer_inputs"] = null,
            "layer_inputs must be a JSON array, not null");

        AssertNullCollectionRejected(
            "pair-null-row-failure-codes",
            root => root["failure_codes"] = null,
            "failure_codes must be a JSON array, not null");

        AssertNullCollectionRejected(
            "pair-null-layer-evidence-refs",
            root => FirstLayer(root)["evidence_refs"] = null,
            "evidence_refs must be a JSON array, not null");

        AssertNullCollectionRejected(
            "pair-null-layer-failure-codes",
            root => FirstLayer(root)["failure_codes"] = null,
            "failure_codes must be a JSON array, not null");
    }

    [Fact]
    public void PublicMethods_ReturnOperationResultFailuresForNullInputs()
    {
        var runner = new ExtensionEvalOnlyQualityRunner();

        var run = runner.Run(null!);

        Assert.False(run.IsSuccess);
        Assert.Equal("OPTIONS_REQUIRED", run.ErrorCode);

        var validation = runner.ValidateRows(null);

        Assert.False(validation.IsSuccess);
        Assert.Equal("INPUT_ROWS_REQUIRED", validation.ErrorCode);

        using var workspace = TempWorkspace();
        var inputPath = WriteRows(workspace, ValidRow("pair-null-public-inputs", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute));
        var report = runner.Run(Options(inputPath, workspace) with { WriteReports = false }).Value!;

        var missingOptions = runner.WriteReportFiles(null, report);
        var missingReport = runner.WriteReportFiles(Options(inputPath, workspace), null);

        Assert.False(missingOptions.IsSuccess);
        Assert.Equal("OPTIONS_REQUIRED", missingOptions.ErrorCode);
        Assert.False(missingReport.IsSuccess);
        Assert.Equal("REPORT_REQUIRED", missingReport.ErrorCode);
    }

    [Fact]
    public void Run_RejectsUserAdaptiveRowWhenApprovalRouteHostMethodRefsRedactionOrMutationAttestationsAreMissing()
    {
        using var workspace = TempWorkspace();
        var inputPath = WriteRows(workspace, new ExtensionEvalOnlyQualityRunnerInputRow
        {
            ScenarioId = "TQ-001-prompt-task-documentation",
            PairId = "pair-missing-fields",
            Route = ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute,
            PrivacyRedactionStatus = "NOT_REDACTED",
            NoCommonMutationAttestation = false,
            NoTrainingMutationAttestation = false,
            NoModelCheckpointLocatorMutationAttestation = false
        });

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.False(result.IsSuccess);
        Assert.Equal("EVAL_ONLY_INPUT_VALIDATION_FAILED", result.ErrorCode);
        Assert.Contains("evidence_scope is required", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("prompt_sha256 is required", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("mode is required", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("approval_ref is required", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("host_name is required", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("method is required", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("baseline_output_ref is required", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("adaptive_output_ref is required", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("privacy_redaction_status must be PASS_REDACTED", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("no_common_mutation_attestation must be true", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_RejectsInvalidPromptModeAndEvidenceScopeBeforeScoring()
    {
        using var workspace = TempWorkspace();
        var row = ValidRow("pair-invalid-provenance", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute) with
        {
            EvidenceScope = "shared_product_claim",
            PromptSha256 = "",
            Mode = "single_output"
        };
        var inputPath = WriteRows(workspace, row);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.False(result.IsSuccess);
        Assert.Equal("EVAL_ONLY_INPUT_VALIDATION_FAILED", result.ErrorCode);
        Assert.Contains("prompt_sha256 is required", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("mode must be baseline_vs_adaptive_pair", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("evidence_scope must be local_private_eval_only", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("baseline_score", -1)]
    [InlineData("baseline_score", 6)]
    [InlineData("adaptive_score", -1)]
    [InlineData("adaptive_score", 6)]
    public void Run_RejectsLayerScoresOutsideZeroToFiveBeforeScoringAndReportWrites(string scoreField, int score)
    {
        using var workspace = TempWorkspace();
        var invalidLayer = scoreField == "baseline_score"
            ? Layer("prompt_to_task", score, 5, "reviewed")
            : Layer("prompt_to_task", 4, score, "reviewed");
        var row = ValidRow("pair-invalid-score-range", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute) with
        {
            LayerInputs =
            [
                invalidLayer,
                Layer("orchestration_reasoning", 4, 4, "reviewed"),
                Layer("skill_selection", 4, 4, "reviewed"),
                Layer("final_usefulness", 4, 5, "reviewed")
            ]
        };
        var inputPath = WriteRows(workspace, row);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.False(result.IsSuccess);
        Assert.Equal("EVAL_ONLY_INPUT_VALIDATION_FAILED", result.ErrorCode);
        Assert.Contains("pair-invalid-score-range:prompt_to_task", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains($"{scoreField} must be between 0 and 5 inclusive", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains($"actual value was {score}", result.ErrorMessage, StringComparison.Ordinal);
        AssertNoGeneratedReports(workspace);
    }

    [Fact]
    public void Run_AcceptsLayerScoreBoundariesZeroAndFive()
    {
        using var workspace = TempWorkspace();
        var row = ValidRow("pair-score-boundaries", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute) with
        {
            LayerInputs =
            [
                Layer("prompt_to_task", 0, 0, "reviewed") with { FailureCodes = ["LOW_SCORE_BOUNDARY_ACCEPTED"] },
                Layer("orchestration_reasoning", 5, 5, "reviewed"),
                Layer("skill_selection", 0, 5, "reviewed"),
                Layer("final_usefulness", 5, 5, "reviewed")
            ]
        };
        var inputPath = WriteRows(workspace, row);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var output = Assert.Single(result.Value!.OutputRows);
        Assert.Contains(output.PerLayerScores, layer => layer.BaselineScore == 0 && layer.AdaptiveScore == 0);
        Assert.Contains(output.PerLayerScores, layer => layer.BaselineScore == 5 && layer.AdaptiveScore == 5);
        Assert.True(File.Exists(Path.Combine(workspace, "quality-02-eval-only-runner-report.json")));
        Assert.True(File.Exists(Path.Combine(workspace, "quality-02-eval-only-runner-score-rows.jsonl")));
        Assert.True(File.Exists(Path.Combine(workspace, "quality-02-eval-only-runner-report.md")));
    }

    [Fact]
    public void Run_ScoresOnlyUserAdaptiveEvalOnlyRowsAndExcludesEvidenceOnlyAndCommonBlockedRows()
    {
        using var workspace = TempWorkspace();
        var inputPath = WriteRows(
            workspace,
            ValidRow("pair-user", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute),
            ValidRow("pair-evidence", ExtensionEvalOnlyQualityRunner.EvidenceOnlyRoute),
            ValidRow("pair-common", ExtensionEvalOnlyQualityRunner.CommonBlockedRoute));

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Equal(3, report.TotalInputRows);
        Assert.Equal(1, report.ScoredOutputRows);
        Assert.Equal(1, report.EvidenceOnlyExcludedRows);
        Assert.Equal(1, report.CommonBlockedExcludedRows);
        Assert.Equal(ExtensionEvalOnlyQualityRunner.LocalPrivateEvalOnlyEvidenceScope, report.EvidenceScope);
        Assert.True(report.SyntheticFixtureOnly);
        Assert.False(report.ActualApprovedEvalExecuted);
        Assert.Single(report.OutputRows);
        Assert.Equal("pair-user", report.OutputRows[0].PairId);
        Assert.Equal(ExtensionEvalOnlyQualityRunner.LocalPrivateEvalOnlyEvidenceScope, report.OutputRows[0].EvidenceScope);
        Assert.True(report.OutputRows[0].SyntheticFixtureOnly);
        Assert.False(report.OutputRows[0].ActualApprovedEvalExecuted);
        Assert.DoesNotContain(report.OutputRows, row => row.PairId == "pair-evidence");
        Assert.DoesNotContain(report.OutputRows, row => row.PairId == "pair-common");
        Assert.Contains(report.PairResults, row => row.ScoreStatus == ExtensionEvalOnlyQualityRunner.ScoreStatusEvidenceOnly);
        Assert.Contains(report.PairResults, row => row.ScoreStatus == ExtensionEvalOnlyQualityRunner.ScoreStatusCommonBlocked);
    }

    [Fact]
    public void Run_KeepsForbiddenClaimFieldsFalseInRowsPairsReportAndMarkdown()
    {
        using var workspace = TempWorkspace();
        var inputPath = WriteRows(workspace, ValidRow("pair-claims", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute));

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        AssertClaimLocksFalse(report.ClaimLocks);
        Assert.All(report.PairResults, pair => AssertClaimLocksFalse(pair.ClaimLocks));
        Assert.All(report.OutputRows, row =>
        {
            Assert.False(row.QualityImprovementClaimed);
            Assert.False(row.SemanticQualityClaimAllowed);
            Assert.False(row.RawPrivateTextCopied);
            AssertClaimLocksFalse(row.ClaimLocks);
        });
        var markdown = File.ReadAllText(Path.Combine(workspace, "quality-02-eval-only-runner-report.md"), Encoding.UTF8);
        Assert.Contains("token_savings_claimed: false", markdown, StringComparison.Ordinal);
        Assert.Contains("final_readiness_claimed: false", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_LabelsRubricScorerAsEvalOnlyAndNeverSemanticTrainableAuthority()
    {
        using var workspace = TempWorkspace();
        var inputPath = WriteRows(workspace, ValidRow("pair-label", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute));

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var row = Assert.Single(result.Value!.OutputRows);
        Assert.Contains("eval-only", row.DeterministicScorerLabel, StringComparison.Ordinal);
        Assert.Contains("not semantic", row.DeterministicScorerLabel, StringComparison.Ordinal);
        Assert.Contains("not semantic/trainable authority", row.HumanReviewerNote, StringComparison.Ordinal);
        Assert.False(row.SemanticQualityClaimAllowed);
        Assert.Contains("privacy helper", result.Value.WhatThisDoesNotProve, StringComparison.Ordinal);
        Assert.Contains("not semantic, trainable", result.Value.WhatThisDoesNotProve, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_WritesJsonJsonlAndMarkdownWithRequiredCountsAndHumanBoard()
    {
        using var workspace = TempWorkspace();
        var inputPath = WriteRows(workspace, ValidRow("pair-output", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute));

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var reportJson = Path.Combine(workspace, "quality-02-eval-only-runner-report.json");
        var rowsJsonl = Path.Combine(workspace, "quality-02-eval-only-runner-score-rows.jsonl");
        var markdown = Path.Combine(workspace, "quality-02-eval-only-runner-report.md");
        Assert.True(File.Exists(reportJson));
        Assert.True(File.Exists(rowsJsonl));
        Assert.True(File.Exists(markdown));
        using var document = JsonDocument.Parse(File.ReadAllText(reportJson, Encoding.UTF8));
        Assert.Equal(1, document.RootElement.GetProperty("total_input_rows").GetInt32());
        Assert.Equal("local_private_eval_only", document.RootElement.GetProperty("evidence_scope").GetString());
        Assert.True(document.RootElement.GetProperty("synthetic_fixture_only").GetBoolean());
        Assert.False(document.RootElement.GetProperty("actual_approved_eval_executed").GetBoolean());
        Assert.Contains("Total:", document.RootElement.GetProperty("human_board_ru").GetString(), StringComparison.Ordinal);
        var line = Assert.Single(File.ReadLines(rowsJsonl, Encoding.UTF8));
        using var rowDocument = JsonDocument.Parse(line);
        Assert.Equal(1, rowDocument.RootElement.GetProperty("total_input_rows").GetInt32());
        Assert.Equal("local_private_eval_only", rowDocument.RootElement.GetProperty("evidence_scope").GetString());
        Assert.True(rowDocument.RootElement.GetProperty("synthetic_fixture_only").GetBoolean());
        Assert.False(rowDocument.RootElement.GetProperty("actual_approved_eval_executed").GetBoolean());
        Assert.Contains("Counts", File.ReadAllText(markdown, Encoding.UTF8), StringComparison.Ordinal);
        Assert.Contains("Synthetic fixture only: true", File.ReadAllText(markdown, Encoding.UTF8), StringComparison.Ordinal);
        Assert.Contains("Privacy Helper Label", File.ReadAllText(markdown, Encoding.UTF8), StringComparison.Ordinal);
    }

    [Fact]
    public void Run_DoesNotOpenQualityImprovementWhenAdaptiveLayerRegressesOrReviewerNoteMissing()
    {
        using var workspace = TempWorkspace();
        var regressed = ValidRow("pair-regression", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute) with
        {
            LayerInputs =
            [
                Layer("prompt_to_task", 5, 4, "reviewed"),
                Layer("orchestration_reasoning", 5, 5, "reviewed"),
                Layer("skill_selection", 5, 5, ""),
                Layer("final_usefulness", 5, 5, "reviewed")
            ]
        };
        var inputPath = WriteRows(workspace, regressed);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var output = Assert.Single(result.Value!.OutputRows);
        Assert.True(output.QualityRegression);
        Assert.False(output.QualityImprovementClaimed);
        Assert.Contains(output.FailureCodes, code => code.Contains("FAIL_QUALITY_REGRESSION", StringComparison.Ordinal));
        Assert.Contains(output.FailureCodes, code => code.Contains("missing_human_reviewer_note", StringComparison.Ordinal));
        AssertClaimLocksFalse(output.ClaimLocks);
    }

    [Fact]
    public void Run_DoesNotCopyRawPrivateTextIntoCommonRepoArtifacts()
    {
        using var workspace = TempWorkspace();
        var secret = "PRIVATE_SECRET_DO_NOT_EXPORT";
        var row = ValidRow("pair-private", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute) with
        {
            RawPrivateInputRef = $"user-local://{secret}",
            RedactedSharedEvidenceRef = "artifact://redacted-only"
        };
        var inputPath = WriteRows(workspace, row);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.All(result.Value!.OutputRefs, outputPath =>
        {
            var text = File.ReadAllText(outputPath, Encoding.UTF8);
            Assert.DoesNotContain(secret, text, StringComparison.Ordinal);
        });
        Assert.All(result.Value.OutputRows, output => Assert.False(output.RawPrivateTextCopied));
    }

    [Fact]
    public void Run_RejectsRawPrivateTextInCopiedOutputRefsEvidenceRefsAndReviewerNotes()
    {
        using var workspace = TempWorkspace();
        var secret = "PRIVATE_SECRET_DO_NOT_EXPORT";
        var row = ValidRow("pair-private-copied-fields", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute) with
        {
            BaselineOutputRef = $"artifact://baseline-{secret}",
            AdaptiveOutputRef = $"artifact://adaptive-{secret}",
            LayerInputs =
            [
                Layer("prompt_to_task", 4, 5, $"review note {secret}") with
                {
                    EvidenceRefs = [$"artifact://evidence-{secret}"]
                }
            ]
        };
        var inputPath = WriteRows(workspace, row);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.False(result.IsSuccess);
        Assert.Equal("EVAL_ONLY_INPUT_VALIDATION_FAILED", result.ErrorCode);
        Assert.Contains("baseline_output_ref appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("adaptive_output_ref appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("reviewer_note appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("evidence_refs[0] appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(workspace, "quality-02-eval-only-runner-report.json")));
    }

    [Fact]
    public void Run_RejectsRawPrivateTextInRowAndLayerFailureCodesWithoutWritingArtifacts()
    {
        using var workspace = TempWorkspace();
        var secret = "PRIVATE_SECRET_DO_NOT_EXPORT";
        var row = ValidRow("pair-private-failure-codes", ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute) with
        {
            FailureCodes = [$"ROW_{secret}"],
            LayerInputs =
            [
                Layer("prompt_to_task", 4, 5, "reviewed") with
                {
                    FailureCodes = [$"LAYER_{secret}"]
                }
            ]
        };
        var inputPath = WriteRows(workspace, row);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.False(result.IsSuccess);
        Assert.Equal("EVAL_ONLY_INPUT_VALIDATION_FAILED", result.ErrorCode);
        Assert.Contains("failure_codes[0] appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        AssertNoGeneratedReports(workspace);
    }

    [Theory]
    [InlineData("EVIDENCE_ONLY")]
    [InlineData("COMMON_BLOCKED")]
    public void Run_RejectsExcludedRowsWithRawPrivateTextInSharedOutputPrivacyFieldsWithoutWritingArtifacts(string route)
    {
        using var workspace = TempWorkspace();
        var secret = "PRIVATE_SECRET_DO_NOT_EXPORT";
        var row = ValidRow("pair-excluded-private", route) with
        {
            ScenarioId = $"scenario-{secret}",
            PairId = $"pair-{secret}",
            EvidenceScope = $"scope-{secret}",
            FailureCodes = [$"ROW_{secret}"],
            LayerInputs =
            [
                Layer("prompt_to_task", 4, 4, $"excluded reviewer note {secret}") with
                {
                    EvidenceRefs = [$"artifact://excluded-{secret}"],
                    FailureCodes = [$"LAYER_{secret}"]
                }
            ]
        };
        var inputPath = WriteRows(workspace, row);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.False(result.IsSuccess);
        Assert.Equal("EVAL_ONLY_INPUT_VALIDATION_FAILED", result.ErrorCode);
        Assert.Contains("scenario_id appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("pair_id appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("evidence_scope appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("failure_codes[0] appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("reviewer_note appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("evidence_refs[0] appears to contain raw private text", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("privacy helper/shared-output guard", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("not semantic, trainable", result.ErrorMessage, StringComparison.Ordinal);
        AssertNoGeneratedReports(workspace);
    }

    private static ExtensionEvalOnlyQualityRunnerOptions Options(string inputPath, string outputDirectory)
        => new()
        {
            InputJsonlPath = inputPath,
            OutputDirectory = outputDirectory,
            RunId = "unit-test-run",
            GeneratedAtUtc = new DateTimeOffset(2026, 6, 18, 9, 0, 0, TimeSpan.Zero)
        };

    private static void AssertNullCollectionRejected(
        string pairId,
        Action<JsonObject> mutate,
        string expectedMessage)
    {
        using var workspace = TempWorkspace();
        var root = ValidRowJson(pairId);
        mutate(root);
        var inputPath = Path.Combine(workspace, "rows.jsonl");
        File.WriteAllText(inputPath, root.ToJsonString(SerializerOptions()), Encoding.UTF8);

        var result = new ExtensionEvalOnlyQualityRunner().Run(Options(inputPath, workspace));

        Assert.False(result.IsSuccess);
        Assert.Equal("EVAL_ONLY_INPUT_VALIDATION_FAILED", result.ErrorCode);
        Assert.Contains(expectedMessage, result.ErrorMessage, StringComparison.Ordinal);
        AssertNoGeneratedReports(workspace);
    }

    private static JsonObject ValidRowJson(string pairId)
        => JsonNode.Parse(JsonSerializer.Serialize(
            ValidRow(pairId, ExtensionEvalOnlyQualityRunner.UserAdaptiveEvalOnlyRoute),
            SerializerOptions()))!.AsObject();

    private static JsonObject FirstLayer(JsonObject root)
        => root["layer_inputs"]!.AsArray()[0]!.AsObject();

    private static ExtensionEvalOnlyQualityRunnerInputRow ValidRow(string pairId, string route)
        => new()
        {
            ScenarioId = "TQ-001-prompt-task-documentation",
            PairId = pairId,
            Route = route,
            EvidenceScope = ExtensionEvalOnlyQualityRunner.LocalPrivateEvalOnlyEvidenceScope,
            ApprovalRef = "approval://eval-only/current",
            UserLocalRootRef = "user-local://approved-root",
            RawPrivateInputRef = "user-local://raw-private-ref",
            RedactedSharedEvidenceRef = "artifact://redacted-evidence",
            SourceRef = "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md",
            DocId = "doc#section",
            PromptSha256 = "abc123",
            HostName = "Codex",
            Method = "prompt-to-task",
            Mode = "baseline_vs_adaptive_pair",
            BaselineOutputRef = "artifact://baseline-output",
            AdaptiveOutputRef = "artifact://adaptive-output",
            PrivacyRedactionStatus = "PASS_REDACTED",
            NoCommonMutationAttestation = true,
            NoTrainingMutationAttestation = true,
            NoModelCheckpointLocatorMutationAttestation = true,
            LayerInputs =
            [
                Layer("prompt_to_task", 4, 5, "reviewed"),
                Layer("orchestration_reasoning", 4, 4, "reviewed"),
                Layer("skill_selection", 4, 4, "reviewed"),
                Layer("final_usefulness", 4, 5, "reviewed")
            ]
        };

    private static ExtensionEvalOnlyQualityRunnerLayerInput Layer(string layerId, int baseline, int adaptive, string note)
        => new()
        {
            LayerId = layerId,
            BaselineScore = baseline,
            AdaptiveScore = adaptive,
            ReviewerNote = note,
            EvidenceRefs = ["artifact://evidence"],
            FailureCodes = []
        };

    private static string WriteRows(string workspace, params ExtensionEvalOnlyQualityRunnerInputRow[] rows)
    {
        var path = Path.Combine(workspace, "rows.jsonl");
        File.WriteAllLines(path, rows.Select(row => JsonSerializer.Serialize(row, SerializerOptions())), Encoding.UTF8);
        return path;
    }

    private static JsonSerializerOptions SerializerOptions()
        => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

    private static void AssertNoGeneratedReports(string workspace)
    {
        Assert.False(File.Exists(Path.Combine(workspace, "quality-02-eval-only-runner-report.json")));
        Assert.False(File.Exists(Path.Combine(workspace, "quality-02-eval-only-runner-score-rows.jsonl")));
        Assert.False(File.Exists(Path.Combine(workspace, "quality-02-eval-only-runner-report.md")));
    }

    private static TempDirectory TempWorkspace()
        => new();

    private static void AssertClaimLocksFalse(ExtensionEvalOnlyQualityRunnerClaimLocks locks)
    {
        Assert.False(locks.TokenSavingsClaimed);
        Assert.False(locks.QualityImprovementClaimed);
        Assert.False(locks.SemanticQualityClaimed);
        Assert.False(locks.SemanticReadinessClaimed);
        Assert.False(locks.LearnabilityClaimed);
        Assert.False(locks.LiveHostReadinessClaimed);
        Assert.False(locks.CommonReadinessClaimed);
        Assert.False(locks.StrictQuality03Ready);
        Assert.False(locks.TrainingReadyClaimed);
        Assert.False(locks.TargetPromotionClaimed);
        Assert.False(locks.LocatorUpdateClaimed);
        Assert.False(locks.FinalReadinessClaimed);
    }

    private sealed class TempDirectory : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"quality02-runner-tests-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(_path);
        }

        public static implicit operator string(TempDirectory directory)
            => directory._path;

        public void Dispose()
        {
            if (Directory.Exists(_path))
                Directory.Delete(_path, recursive: true);
        }
    }
}
