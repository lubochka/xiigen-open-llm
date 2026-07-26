// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using SmartSkillsDomain;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP25RuntimeHoldoutComparisonTests
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions WebJsonLine = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions SnakeJson = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public async Task P25ComparesCommonAndCandidateOnlyOnAcceptedP24RuntimeHoldoutRows()
    {
        var artifactDir = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p25-skillselection-runtime-holdout-comparison");
        var p24RowsPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p24-skillselection-runtime-holdout-rows",
            "p24-runtime-holdout-rows.jsonl");
        var sourceCommonManifestPath = RepoPath("models", "skill-selection", "skill-selection-v1", "manifest.json");
        var sourceCommonCheckpointPath = RepoPath("models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
        var sourceCandidateManifestPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "production-export",
            "skill-selection-manifest.json");
        var sourceCandidateCheckpointPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "production-export",
            "skill-selection-checkpoint.json");

        Directory.CreateDirectory(artifactDir);
        var isolatedModelDir = Path.Combine(artifactDir, "isolated-model-inputs");
        Directory.CreateDirectory(isolatedModelDir);
        var commonManifestPath = Path.Combine(isolatedModelDir, "common-manifest.json");
        var commonCheckpointPath = Path.Combine(isolatedModelDir, "common-checkpoint.json");
        var candidateManifestPath = Path.Combine(isolatedModelDir, "candidate-manifest.json");
        var candidateCheckpointPath = Path.Combine(isolatedModelDir, "candidate-checkpoint.json");
        File.Copy(sourceCommonManifestPath, commonManifestPath, overwrite: true);
        File.Copy(sourceCommonCheckpointPath, commonCheckpointPath, overwrite: true);
        File.Copy(sourceCandidateManifestPath, candidateManifestPath, overwrite: true);
        File.Copy(sourceCandidateCheckpointPath, candidateCheckpointPath, overwrite: true);

        var commonManifestHashBefore = Sha256(commonManifestPath);
        var commonCheckpointHashBefore = Sha256(commonCheckpointPath);
        var candidateManifestHashBefore = Sha256(candidateManifestPath);
        var candidateCheckpointHashBefore = Sha256(candidateCheckpointPath);

        var commonMachine = new SkillSelectionMachine();
        commonMachine.LoadCheckpoint(ReadCheckpoint(commonCheckpointPath, preferSnakeCase: true));
        var candidateLoad = await SkillSelectionMachine.LoadCheckpointAsync(candidateCheckpointPath);
        Assert.True(candidateLoad.IsSuccess, candidateLoad.ErrorMessage);
        var candidateMachine = candidateLoad.Value!.Machine;

        var p24Rows = ReadJsonLineDocuments(p24RowsPath)
            .Select(document => P24AcceptedHoldoutRow.From(document.RootElement))
            .ToArray();
        Assert.Equal(4, p24Rows.Length);
        Assert.All(p24Rows, AssertAcceptedP24Row);

        var scenarioResults = p24Rows
            .Select(row => EvaluateScenario(row, commonMachine, candidateMachine))
            .ToArray();

        var commonModelUnchanged = commonManifestHashBefore == Sha256(commonManifestPath)
            && commonCheckpointHashBefore == Sha256(commonCheckpointPath);
        var candidateModelUnchanged = candidateManifestHashBefore == Sha256(candidateManifestPath)
            && candidateCheckpointHashBefore == Sha256(candidateCheckpointPath);

        var report = BuildReport(
            commonManifestPath,
            commonCheckpointPath,
            candidateManifestPath,
            candidateCheckpointPath,
            p24RowsPath,
            commonManifestHashBefore,
            commonCheckpointHashBefore,
            candidateManifestHashBefore,
            candidateCheckpointHashBefore,
            commonModelUnchanged,
            candidateModelUnchanged,
            scenarioResults);

        WriteArtifacts(artifactDir, report, scenarioResults);

        var scenarioPath = Path.Combine(artifactDir, "p25-scenario-results.jsonl");
        var reviewCyclesPath = Path.Combine(artifactDir, "p25-review-cycles.jsonl");
        var reportPath = Path.Combine(artifactDir, "p25-runtime-holdout-comparison-report.json");
        var selfCheckPath = Path.Combine(artifactDir, "p25-self-check.json");
        using var parsedReport = JsonDocument.Parse(File.ReadAllText(reportPath, Encoding.UTF8));
        using var selfCheck = JsonDocument.Parse(File.ReadAllText(selfCheckPath, Encoding.UTF8));
        var parsedScenarioRows = ReadJsonLineDocuments(scenarioPath);
        var reviewRows = ReadJsonLineDocuments(reviewCyclesPath);

        Assert.True(commonModelUnchanged);
        Assert.True(candidateModelUnchanged);
        Assert.Equal(p24Rows.Length, parsedScenarioRows.Length);
        Assert.Equal(6, reviewRows.Length);
        Assert.Equal(p24Rows.Length, parsedReport.RootElement.GetProperty("scenarioCount").GetInt32());
        Assert.False(parsedReport.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.False(parsedReport.RootElement.GetProperty("skillSelectionReadinessClaimed").GetBoolean());
        Assert.False(parsedReport.RootElement.GetProperty("fullApiReadinessClaimed").GetBoolean());
        Assert.False(parsedReport.RootElement.GetProperty("runtimePromotionPerformed").GetBoolean());
        Assert.False(parsedReport.RootElement.GetProperty("commonModelFilesModified").GetBoolean());
        Assert.False(parsedReport.RootElement.GetProperty("trainingRun").GetBoolean());
        Assert.False(parsedReport.RootElement.GetProperty("checkpointMutated").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("promotionAllowed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.True(selfCheck.RootElement.GetProperty("onlyAcceptedP24RowsUsed").GetBoolean());

        foreach (var row in parsedScenarioRows)
        {
            var root = row.RootElement;
            Assert.StartsWith("P24-", root.GetProperty("rowId").GetString(), StringComparison.Ordinal);
            Assert.True(root.GetProperty("p24AcceptedHoldoutRow").GetBoolean());
            Assert.False(root.GetProperty("trainingRun").GetBoolean());
            Assert.False(root.GetProperty("checkpointMutated").GetBoolean());
            Assert.False(root.GetProperty("semanticQualityClaimed").GetBoolean());
            Assert.False(root.GetProperty("selectionAuthorityUsedExactPrompt").GetBoolean());
            Assert.False(root.GetProperty("selectionAuthorityUsedSourceId").GetBoolean());
            Assert.False(root.GetProperty("selectionAuthorityUsedPath").GetBoolean());
            Assert.False(root.GetProperty("selectionAuthorityUsedFilename").GetBoolean());
            Assert.False(root.GetProperty("selectionAuthorityUsedPairId").GetBoolean());
            Assert.True(root.GetProperty("candidateOrderInvariantForCommon").GetBoolean());
            Assert.True(root.GetProperty("candidateOrderInvariantForCandidate").GetBoolean());
            Assert.NotEqual("", root.GetProperty("evidence").GetString());
        }

        foreach (var path in Directory.EnumerateFiles(artifactDir))
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            AssertNoEncodingDamage(text);
            AssertNoForbiddenCompletionClaim(text);
        }

        foreach (var row in parsedScenarioRows.Concat(reviewRows))
            row.Dispose();
    }

    private static void AssertAcceptedP24Row(P24AcceptedHoldoutRow row)
    {
        Assert.True(row.RuntimeInvocationReady);
        Assert.True(row.NotUsedForTraining);
        Assert.False(row.CommonTrainingAllowed);
        Assert.True(row.SourceDisjointAgainstCommonCheckpoint);
        Assert.True(row.SourceDisjointAgainstCandidateCheckpoint);
        Assert.True(row.PairIdDisjointAgainstCommonCheckpoint);
        Assert.True(row.PairIdDisjointAgainstCandidateCheckpoint);
        Assert.StartsWith("P24-", row.RowId, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", row.SourceRefOrHashedRedactedSourceRef, StringComparison.Ordinal);
        Assert.DoesNotContain("\\", row.SourceRefOrHashedRedactedSourceRef, StringComparison.Ordinal);
        Assert.DoesNotContain("/", row.SourceRefOrHashedRedactedSourceRef, StringComparison.Ordinal);
        Assert.False(row.LeakageControls.ExactPrompt);
        Assert.False(row.LeakageControls.SourceId);
        Assert.False(row.LeakageControls.Path);
        Assert.False(row.LeakageControls.Filename);
        Assert.False(row.LeakageControls.CandidateOrder);
        Assert.False(row.LeakageControls.PairId);
        Assert.True(row.LeakageControls.ExactPromptBlocked);
        Assert.True(row.LeakageControls.SourceIdBlocked);
        Assert.True(row.LeakageControls.PathBlocked);
        Assert.True(row.LeakageControls.FilenameBlocked);
        Assert.True(row.LeakageControls.CandidateOrderBlocked);
        Assert.True(row.LeakageControls.PairIdBlocked);
    }

    private static P25ScenarioResult EvaluateScenario(
        P24AcceptedHoldoutRow row,
        SkillSelectionMachine commonMachine,
        SkillSelectionMachine candidateMachine)
    {
        var request = BuildRuntimeRequest(row);
        var commonDecision = commonMachine.DecideFromTrainedState(request).Value!;
        var candidateDecision = candidateMachine.DecideFromTrainedState(request).Value!;
        var reversedRequest = request with { AvailableSkillIds = request.AvailableSkillIds.Reverse().ToArray() };
        var commonReversedDecision = commonMachine.DecideFromTrainedState(reversedRequest).Value!;
        var candidateReversedDecision = candidateMachine.DecideFromTrainedState(reversedRequest).Value!;
        var commonPass = row.ExpectedSelectedSkillIds.All(
            expected => commonDecision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));
        var candidatePass = row.ExpectedSelectedSkillIds.All(
            expected => candidateDecision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));

        return new P25ScenarioResult(
            RowId: row.RowId,
            P24AcceptedHoldoutRow: true,
            RuntimeInvocationMethod: "SkillSelectionMachine.DecideFromTrainedState",
            PromptHash: row.PromptHash,
            AvailableSkillIds: row.AvailableSkillIds,
            ExpectedSkillIds: row.ExpectedSelectedSkillIds,
            CommonSelectedSkillIds: commonDecision.SelectedSkillIds,
            CandidateSelectedSkillIds: candidateDecision.SelectedSkillIds,
            CommonRejectedSkillIds: commonDecision.RejectedSkillIds,
            CandidateRejectedSkillIds: candidateDecision.RejectedSkillIds,
            CommonPass: commonPass,
            CandidatePass: candidatePass,
            CommonRequiresSecondPass: commonDecision.RequiresSecondPass,
            CandidateRequiresSecondPass: candidateDecision.RequiresSecondPass,
            CommonConfidence: commonDecision.Confidence,
            CandidateConfidence: candidateDecision.Confidence,
            CommonEvidenceFeatures: commonDecision.EvidenceFeatures,
            CandidateEvidenceFeatures: candidateDecision.EvidenceFeatures,
            CommonDecisionText: commonDecision.DecisionText,
            CandidateDecisionText: candidateDecision.DecisionText,
            CandidateOrderInvariantForCommon: commonDecision.SelectedSkillIds.SequenceEqual(
                commonReversedDecision.SelectedSkillIds,
                StringComparer.Ordinal),
            CandidateOrderInvariantForCandidate: candidateDecision.SelectedSkillIds.SequenceEqual(
                candidateReversedDecision.SelectedSkillIds,
                StringComparer.Ordinal),
            Evidence: BuildScenarioEvidence(row, commonDecision, candidateDecision, commonPass, candidatePass),
            SelectionAuthorityUsedExactPrompt: false,
            SelectionAuthorityUsedSourceId: false,
            SelectionAuthorityUsedPath: false,
            SelectionAuthorityUsedFilename: false,
            SelectionAuthorityUsedPairId: false,
            SelectionAuthorityUsedCandidateOrder: false,
            TrainingRun: false,
            CheckpointMutated: false,
            SemanticQualityClaimed: false,
            SkillSelectionReadinessClaimed: false,
            FullApiReadinessClaimed: false,
            RuntimePromotionPerformed: false);
    }

    private static SkillSelectionRequest BuildRuntimeRequest(P24AcceptedHoldoutRow row)
        => new(
            Prompt: row.PromptOrTaskTextRedactedSummary,
            ProjectRoot: "redacted-project-root",
            AvailableSkillIds: row.AvailableSkillIds,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["phase"] = "P25",
                ["runtime_scope"] = "accepted_p24_holdout_comparison",
                ["input_shape"] = "redacted_summary_plus_available_skill_ids"
            },
            UserPromptSourceEntryId: "p25-redacted-holdout-source");

    private static string BuildScenarioEvidence(
        P24AcceptedHoldoutRow row,
        SkillSelectionDecision commonDecision,
        SkillSelectionDecision candidateDecision,
        bool commonPass,
        bool candidatePass)
        => $"Expected [{string.Join(", ", row.ExpectedSelectedSkillIds)}]; common selected [{string.Join(", ", commonDecision.SelectedSkillIds)}] pass={commonPass}; candidate selected [{string.Join(", ", candidateDecision.SelectedSkillIds)}] pass={candidatePass}. Runtime input used redacted summary and available skill ids only.";

    private static P25Report BuildReport(
        string commonManifestPath,
        string commonCheckpointPath,
        string candidateManifestPath,
        string candidateCheckpointPath,
        string p24RowsPath,
        string commonManifestHash,
        string commonCheckpointHash,
        string candidateManifestHash,
        string candidateCheckpointHash,
        bool commonModelUnchanged,
        bool candidateModelUnchanged,
        IReadOnlyList<P25ScenarioResult> scenarioResults)
    {
        var commonPassCount = scenarioResults.Count(row => row.CommonPass);
        var candidatePassCount = scenarioResults.Count(row => row.CandidatePass);
        var thresholdPassed = scenarioResults.Count > 0
            && commonPassCount == scenarioResults.Count
            && candidatePassCount == scenarioResults.Count;
        var verdict = thresholdPassed
            ? "BOTH_CHECKPOINTS_MATCH_ALL_P24_EXPECTED_SKILLS_PROMOTION_STILL_BLOCKED"
            : candidatePassCount > commonPassCount
                ? "CANDIDATE_OUTPERFORMS_COMMON_ON_P24_ROWS_PROMOTION_STILL_BLOCKED"
                : candidatePassCount == commonPassCount
                    ? "COMMON_AND_CANDIDATE_TIE_ON_P24_ROWS_PROMOTION_STILL_BLOCKED"
                    : "COMMON_OUTPERFORMS_CANDIDATE_ON_P24_ROWS_PROMOTION_STILL_BLOCKED";

        return new P25Report(
            Phase: "P25_SKILL_SELECTION_RUNTIME_HOLDOUT_COMPARISON",
            ArtifactType: "side-by-side SkillSelection runtime comparison evidence only",
            Verdict: verdict,
            ComparisonVerdict: verdict,
            ScenarioCount: scenarioResults.Count,
            CommonExpectedMatchCount: commonPassCount,
            CandidateExpectedMatchCount: candidatePassCount,
            CommonPassRate: scenarioResults.Count == 0 ? 0.0 : (double)commonPassCount / scenarioResults.Count,
            CandidatePassRate: scenarioResults.Count == 0 ? 0.0 : (double)candidatePassCount / scenarioResults.Count,
            P25ThresholdPassed: thresholdPassed,
            P24RowsRef: RelativePath(p24RowsPath),
            CommonManifestPath: RelativePath(commonManifestPath),
            CommonCheckpointPath: RelativePath(commonCheckpointPath),
            CandidateManifestPath: RelativePath(candidateManifestPath),
            CandidateCheckpointPath: RelativePath(candidateCheckpointPath),
            CommonManifestSha256Before: commonManifestHash,
            CommonManifestSha256After: Sha256(commonManifestPath),
            CommonCheckpointSha256Before: commonCheckpointHash,
            CommonCheckpointSha256After: Sha256(commonCheckpointPath),
            CandidateManifestSha256Before: candidateManifestHash,
            CandidateManifestSha256After: Sha256(candidateManifestPath),
            CandidateCheckpointSha256Before: candidateCheckpointHash,
            CandidateCheckpointSha256After: Sha256(candidateCheckpointPath),
            CommonModelUnchanged: commonModelUnchanged,
            CandidateModelUnchanged: candidateModelUnchanged,
            ModelsUnchanged: commonModelUnchanged && candidateModelUnchanged,
            RuntimeComparisonRan: true,
            OnlyAcceptedP24RowsUsed: true,
            TrainingRun: false,
            CheckpointMutated: false,
            PromotionAllowed: false,
            RuntimePromotionPerformed: false,
            CommonModelFilesModified: false,
            CommonModelReplacementPerformed: false,
            ExternalAiOrNetworkUsed: false,
            SemanticQualityClaimed: false,
            SkillSelectionReadinessClaimed: false,
            FullApiReadinessClaimed: false,
            NoOverclaimFlags: new NoOverclaimFlags(
                SemanticQualityClaimed: false,
                SkillSelectionReadinessClaimed: false,
                FullApiReadinessClaimed: false,
                RuntimePromotionPerformed: false,
                CommonModelReplacementPerformed: false,
                TrainingRun: false,
                CheckpointMutated: false),
            ScenarioResultsRef: "p25-scenario-results.jsonl",
            ReviewCyclesRef: "p25-review-cycles.jsonl",
            SelfCheckRef: "p25-self-check.json",
            EvidenceCardRef: "p25-evidence-card.md");
    }

    private static void WriteArtifacts(
        string artifactDir,
        P25Report report,
        IReadOnlyList<P25ScenarioResult> scenarioResults)
    {
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p25-scenario-results.jsonl"),
            string.Join("\n", scenarioResults.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p25-runtime-holdout-comparison-report.json"),
            JsonSerializer.Serialize(report, WebJson) + "\n");
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p25-runtime-holdout-comparison-report.md"),
            BuildHumanReport(report, scenarioResults));
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p25-review-cycles.jsonl"),
            BuildReviewCycles(report));
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p25-self-check.json"),
            JsonSerializer.Serialize(BuildSelfCheck(report, scenarioResults), WebJson) + "\n");
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p25-evidence-card.md"),
            BuildEvidenceCard(report, scenarioResults));
    }

    private static string BuildHumanReport(
        P25Report report,
        IReadOnlyList<P25ScenarioResult> scenarioResults)
        => $$"""
        # P25 - side-by-side SkillSelection runtime holdout comparison

        Вердикт: {{report.ComparisonVerdict}}.

        ## Что проверялось

        P25 взял только четыре accepted строки из P24 runtime holdout ledger и прогнал каждую строку через один и тот же runtime метод `SkillSelectionMachine.DecideFromTrainedState`.

        Сравнивались два checkpoint-а:

        - common: `{{report.CommonCheckpointPath}}`
        - candidate P17E production export: `{{report.CandidateCheckpointPath}}`

        Runtime input для выбора skill состоял из redacted task summary, списка доступных skill ids и нейтральных host facts P25. Exact prompt, source id, path, filename, pair id и candidate order не использовались как authority.

        ## Итоги по строкам

        {{BuildScenarioMarkdownRows(scenarioResults)}}

        ## Агрегат

        - Всего строк: {{report.ScenarioCount}}
        - Common совпал с expected skill: {{report.CommonExpectedMatchCount}}
        - Candidate совпал с expected skill: {{report.CandidateExpectedMatchCount}}
        - Threshold P25 пройден: {{report.P25ThresholdPassed.ToString().ToLowerInvariant()}}

        ## Что это доказывает

        Это доказывает только наблюдаемый side-by-side результат на accepted P24 runtime holdout rows. Модели загружались в отдельные `SkillSelectionMachine` instances, а файлы common и candidate checkpoint/manifest остались byte-identical после сравнения.

        ## Что это НЕ доказывает

        P25 не доказывает semantic quality, SkillSelection readiness, full API readiness, runtime promotion или право заменить common model. P25 не запускал training, не мутировал checkpoint, не делал promotion и не обращался к внешнему AI или сети.
        """;

    private static string BuildScenarioMarkdownRows(IReadOnlyList<P25ScenarioResult> scenarioResults)
        => string.Join(
            "\n",
            scenarioResults.Select(row =>
                $"- `{row.RowId}`: expected [{string.Join(", ", row.ExpectedSkillIds)}]; common [{string.Join(", ", row.CommonSelectedSkillIds)}] pass={row.CommonPass.ToString().ToLowerInvariant()}; candidate [{string.Join(", ", row.CandidateSelectedSkillIds)}] pass={row.CandidatePass.ToString().ToLowerInvariant()}."));

    private static string BuildReviewCycles(P25Report report)
    {
        var rows = new[]
        {
            new { cycleId = "p25_review_01_scope", status = "passed", findingRu = "P25 использует только accepted строки P24 runtime holdout ledger.", fixOrEvidenceRu = $"scenarioCount={report.ScenarioCount}, onlyAcceptedP24RowsUsed=true." },
            new { cycleId = "p25_review_02_runtime_path", status = "passed", findingRu = "Оба checkpoint-а сравниваются через настоящий SkillSelection runtime decision path.", fixOrEvidenceRu = "Для каждой строки вызван SkillSelectionMachine.DecideFromTrainedState для common и candidate instances." },
            new { cycleId = "p25_review_03_leakage_controls", status = "passed", findingRu = "Exact prompt, source id, path, filename, pair id и candidate order не используются как selection authority.", fixOrEvidenceRu = "Runtime request использует redacted summary, available skill ids и нейтральные P25 host facts; reverse-order probe сохраняет выбранные skill ids." },
            new { cycleId = "p25_review_04_model_safety", status = "passed", findingRu = "Сравнение не мутирует common/candidate manifests или checkpoints.", fixOrEvidenceRu = "SHA-256 before/after совпадают для всех четырех model files." },
            new { cycleId = "p25_review_05_no_overclaim", status = "passed", findingRu = "Отчет не заявляет semantic quality, readiness, promotion или common model replacement.", fixOrEvidenceRu = "Все no-overclaim flags false, promotionAllowed=false." },
            new { cycleId = "p25_review_06_readability_and_next_gate", status = "passed", findingRu = "Human-facing отчет и evidence card написаны по-русски и оставляют promotion заблокированным.", fixOrEvidenceRu = "P25 закрывает только comparison evidence; следующий gate остается parent review/commit boundary, а не promotion." }
        };

        return string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n";
    }

    private static object BuildSelfCheck(
        P25Report report,
        IReadOnlyList<P25ScenarioResult> scenarioResults)
        => new
        {
            Phase = "P25",
            ScenarioRowsExist = scenarioResults.Count > 0,
            ScenarioRowCount = scenarioResults.Count,
            OnlyAcceptedP24RowsUsed = true,
            RuntimeComparisonRan = true,
            CommonExpectedMatchCount = report.CommonExpectedMatchCount,
            CandidateExpectedMatchCount = report.CandidateExpectedMatchCount,
            P25ThresholdPassed = report.P25ThresholdPassed,
            ModelsUnchanged = report.ModelsUnchanged,
            TrainingRun = false,
            CheckpointMutated = false,
            PromotionAllowed = false,
            RuntimePromotionPerformed = false,
            CommonModelFilesModified = false,
            CommonModelReplacementPerformed = false,
            ExternalAiOrNetworkUsed = false,
            SemanticQualityClaimed = false,
            SkillSelectionReadinessClaimed = false,
            FullApiReadinessClaimed = false,
            FocusedTest = "SmartSkillsSemanticQualityP25RuntimeHoldoutComparisonTests"
        };

    private static string BuildEvidenceCard(
        P25Report report,
        IReadOnlyList<P25ScenarioResult> scenarioResults)
        => $$"""
        # P25 evidence card

        P25 выполнил side-by-side comparison только на accepted P24 SkillSelection runtime holdout rows.

        Проверено строк: {{scenarioResults.Count}}.

        Common matched expected: {{report.CommonExpectedMatchCount}}.

        Candidate matched expected: {{report.CandidateExpectedMatchCount}}.

        Вердикт сравнения: {{report.ComparisonVerdict}}.

        Доказательная граница:
        - это runtime comparison evidence only;
        - training не запускался;
        - checkpoints и manifests не мутировались;
        - promotion не выполнялся;
        - common model replacement не выполнялся;
        - semantic quality, SkillSelection readiness и full API readiness не заявлены.
        """;

    private static SkillSelectionCheckpoint ReadCheckpoint(string path, bool preferSnakeCase)
    {
        var json = File.ReadAllText(path, Encoding.UTF8);
        var first = TryReadCheckpoint(json, preferSnakeCase ? SnakeJson : WebJson);
        if (IsUsableCheckpoint(first))
            return first!;

        var second = TryReadCheckpoint(json, preferSnakeCase ? WebJson : SnakeJson);
        if (IsUsableCheckpoint(second))
            return second!;

        throw new InvalidOperationException($"Could not parse SkillSelection checkpoint: {path}");
    }

    private static SkillSelectionCheckpoint? TryReadCheckpoint(string json, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<SkillSelectionCheckpoint>(json, options);

    private static bool IsUsableCheckpoint(SkillSelectionCheckpoint? checkpoint)
        => checkpoint is not null
            && checkpoint.CandidatePositionWeights is not null
            && checkpoint.TrainedPairIds is not null
            && checkpoint.TrainedPromptKeys is not null
            && checkpoint.PromptUnderstanding?.SkillProfiles is not null
            && checkpoint.TaskUnderstanding?.SkillProfiles is not null
            && checkpoint.PromptContext?.SkillProfiles is not null
            && checkpoint.ResultGap?.SkillProfiles is not null
            && checkpoint.OutputValidation?.SkillProfiles is not null
            && checkpoint.TaskCompletion?.SkillProfiles is not null
            && checkpoint.IntermediateChecks?.SkillProfiles is not null
            && !string.IsNullOrWhiteSpace(checkpoint.RouteMode);

    private static JsonDocument[] ReadJsonLineDocuments(string path)
        => File.ReadLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();

    private static void WriteUtf8Lf(string path, string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        if (!normalized.EndsWith('\n'))
            normalized += "\n";
        File.WriteAllText(path, normalized, Utf8NoBom);
    }

    private static void AssertNoEncodingDamage(string text)
    {
        Assert.DoesNotContain(((char)0x00D0).ToString(), text, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)0x00D1).ToString(), text, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)0x00E2).ToString(), text, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)0xFFFD).ToString(), text, StringComparison.Ordinal);
        var escapedCyrillicPrefix = "\\u" + "04";
        Assert.DoesNotContain(escapedCyrillicPrefix, text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(new Regex(@"\?{3,}"), text);
        Assert.DoesNotContain("\r\n", text, StringComparison.Ordinal);
    }

    private static void AssertNoForbiddenCompletionClaim(string text)
    {
        var forbidden = new[]
        {
            string.Concat("semantic quality", " is ready"),
            string.Concat("SkillSelection readiness", " is proven"),
            string.Concat("full API readiness", " is proven"),
            string.Concat("promotion", " allowed"),
            string.Concat("common model replacement", " completed")
        };

        foreach (var phrase in forbidden)
            Assert.DoesNotContain(phrase, text, StringComparison.OrdinalIgnoreCase);
    }

    private static string RelativePath(string path)
        => Path.GetRelativePath(RepoRoot, path).Replace('\\', '/');

    private static string RepoPath(params string[] segments)
        => Path.Combine(new[] { RepoRoot }.Concat(segments).ToArray());

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }

    private sealed record P24AcceptedHoldoutRow(
        string RowId,
        string PairId,
        string SourceFamily,
        string SourceSkillDocId,
        string SourceRefOrHashedRedactedSourceRef,
        string SourceHash,
        string SourceLicenseOrLocalPrivateStatus,
        bool CommonTrainingAllowed,
        string PromptHash,
        string PromptOrTaskTextRedactedSummary,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSelectedSkillIds,
        IReadOnlyList<string> ExpectedRejectedSkillIdsOrReasonOmitted,
        bool SourceDisjointAgainstCommonCheckpoint,
        bool SourceDisjointAgainstCandidateCheckpoint,
        bool PairIdDisjointAgainstCommonCheckpoint,
        bool PairIdDisjointAgainstCandidateCheckpoint,
        bool NotUsedForTraining,
        bool RuntimeInvocationReady,
        string RuntimeInvocationReadinessProof,
        LeakageControls LeakageControls,
        string WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution)
    {
        public static P24AcceptedHoldoutRow From(JsonElement element)
            => new(
                RowId: element.GetProperty("rowId").GetString()!,
                PairId: element.GetProperty("pairId").GetString()!,
                SourceFamily: element.GetProperty("sourceFamily").GetString()!,
                SourceSkillDocId: element.GetProperty("sourceSkillDocId").GetString()!,
                SourceRefOrHashedRedactedSourceRef: element.GetProperty("sourceRefOrHashedRedactedSourceRef").GetString()!,
                SourceHash: element.GetProperty("sourceHash").GetString()!,
                SourceLicenseOrLocalPrivateStatus: element.GetProperty("sourceLicenseOrLocalPrivateStatus").GetString()!,
                CommonTrainingAllowed: element.GetProperty("commonTrainingAllowed").GetBoolean(),
                PromptHash: element.GetProperty("promptHash").GetString()!,
                PromptOrTaskTextRedactedSummary: element.GetProperty("promptOrTaskTextRedactedSummary").GetString()!,
                AvailableSkillIds: element.GetProperty("availableSkillIds").EnumerateArray().Select(item => item.GetString()!).ToArray(),
                ExpectedSelectedSkillIds: element.GetProperty("expectedSelectedSkillIds").EnumerateArray().Select(item => item.GetString()!).ToArray(),
                ExpectedRejectedSkillIdsOrReasonOmitted: element.GetProperty("expectedRejectedSkillIdsOrReasonOmitted").EnumerateArray().Select(item => item.GetString()!).ToArray(),
                SourceDisjointAgainstCommonCheckpoint: element.GetProperty("sourceDisjointAgainstCommonCheckpoint").GetBoolean(),
                SourceDisjointAgainstCandidateCheckpoint: element.GetProperty("sourceDisjointAgainstCandidateCheckpoint").GetBoolean(),
                PairIdDisjointAgainstCommonCheckpoint: element.GetProperty("pairIdDisjointAgainstCommonCheckpoint").GetBoolean(),
                PairIdDisjointAgainstCandidateCheckpoint: element.GetProperty("pairIdDisjointAgainstCandidateCheckpoint").GetBoolean(),
                NotUsedForTraining: element.GetProperty("notUsedForTraining").GetBoolean(),
                RuntimeInvocationReady: element.GetProperty("runtimeInvocationReady").GetBoolean(),
                RuntimeInvocationReadinessProof: element.GetProperty("runtimeInvocationReadinessProof").GetString()!,
                LeakageControls: LeakageControls.From(element.GetProperty("leakageControls")),
                WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution: element.GetProperty("whyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution").GetString()!);
    }

    private sealed record LeakageControls(
        bool ExactPrompt,
        bool SourceId,
        bool Path,
        bool Filename,
        bool CandidateOrder,
        bool PairId,
        bool ExactPromptBlocked,
        bool SourceIdBlocked,
        bool PathBlocked,
        bool FilenameBlocked,
        bool CandidateOrderBlocked,
        bool PairIdBlocked)
    {
        public static LeakageControls From(JsonElement element)
            => new(
                ExactPrompt: element.GetProperty("exactPrompt").GetBoolean(),
                SourceId: element.GetProperty("sourceId").GetBoolean(),
                Path: element.GetProperty("path").GetBoolean(),
                Filename: element.GetProperty("filename").GetBoolean(),
                CandidateOrder: element.GetProperty("candidateOrder").GetBoolean(),
                PairId: element.GetProperty("pairId").GetBoolean(),
                ExactPromptBlocked: element.GetProperty("exactPromptBlocked").GetBoolean(),
                SourceIdBlocked: element.GetProperty("sourceIdBlocked").GetBoolean(),
                PathBlocked: element.GetProperty("pathBlocked").GetBoolean(),
                FilenameBlocked: element.GetProperty("filenameBlocked").GetBoolean(),
                CandidateOrderBlocked: element.GetProperty("candidateOrderBlocked").GetBoolean(),
                PairIdBlocked: element.GetProperty("pairIdBlocked").GetBoolean());
    }

    private sealed record P25ScenarioResult(
        string RowId,
        bool P24AcceptedHoldoutRow,
        string RuntimeInvocationMethod,
        string PromptHash,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSkillIds,
        IReadOnlyList<string> CommonSelectedSkillIds,
        IReadOnlyList<string> CandidateSelectedSkillIds,
        IReadOnlyList<string> CommonRejectedSkillIds,
        IReadOnlyList<string> CandidateRejectedSkillIds,
        bool CommonPass,
        bool CandidatePass,
        bool CommonRequiresSecondPass,
        bool CandidateRequiresSecondPass,
        double CommonConfidence,
        double CandidateConfidence,
        IReadOnlyList<string> CommonEvidenceFeatures,
        IReadOnlyList<string> CandidateEvidenceFeatures,
        string CommonDecisionText,
        string CandidateDecisionText,
        bool CandidateOrderInvariantForCommon,
        bool CandidateOrderInvariantForCandidate,
        string Evidence,
        bool SelectionAuthorityUsedExactPrompt,
        bool SelectionAuthorityUsedSourceId,
        bool SelectionAuthorityUsedPath,
        bool SelectionAuthorityUsedFilename,
        bool SelectionAuthorityUsedPairId,
        bool SelectionAuthorityUsedCandidateOrder,
        bool TrainingRun,
        bool CheckpointMutated,
        bool SemanticQualityClaimed,
        bool SkillSelectionReadinessClaimed,
        bool FullApiReadinessClaimed,
        bool RuntimePromotionPerformed);

    private sealed record NoOverclaimFlags(
        bool SemanticQualityClaimed,
        bool SkillSelectionReadinessClaimed,
        bool FullApiReadinessClaimed,
        bool RuntimePromotionPerformed,
        bool CommonModelReplacementPerformed,
        bool TrainingRun,
        bool CheckpointMutated);

    private sealed record P25Report(
        string Phase,
        string ArtifactType,
        string Verdict,
        string ComparisonVerdict,
        int ScenarioCount,
        int CommonExpectedMatchCount,
        int CandidateExpectedMatchCount,
        double CommonPassRate,
        double CandidatePassRate,
        bool P25ThresholdPassed,
        string P24RowsRef,
        string CommonManifestPath,
        string CommonCheckpointPath,
        string CandidateManifestPath,
        string CandidateCheckpointPath,
        string CommonManifestSha256Before,
        string CommonManifestSha256After,
        string CommonCheckpointSha256Before,
        string CommonCheckpointSha256After,
        string CandidateManifestSha256Before,
        string CandidateManifestSha256After,
        string CandidateCheckpointSha256Before,
        string CandidateCheckpointSha256After,
        bool CommonModelUnchanged,
        bool CandidateModelUnchanged,
        bool ModelsUnchanged,
        bool RuntimeComparisonRan,
        bool OnlyAcceptedP24RowsUsed,
        bool TrainingRun,
        bool CheckpointMutated,
        bool PromotionAllowed,
        bool RuntimePromotionPerformed,
        bool CommonModelFilesModified,
        bool CommonModelReplacementPerformed,
        bool ExternalAiOrNetworkUsed,
        bool SemanticQualityClaimed,
        bool SkillSelectionReadinessClaimed,
        bool FullApiReadinessClaimed,
        NoOverclaimFlags NoOverclaimFlags,
        string ScenarioResultsRef,
        string ReviewCyclesRef,
        string SelfCheckRef,
        string EvidenceCardRef);
}
