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

public sealed class SmartSkillsSemanticQualityP21CompatibilityNonRegressionTests
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
    public void P21ArtifactsCompareExistingCommonAndCandidateWithoutAllowingPromotion()
    {
        var artifactDir = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p21-skillselection-compatibility-nonregression");
        var commonManifestPath = RepoPath("models", "skill-selection", "skill-selection-v1", "manifest.json");
        var commonCheckpointPath = RepoPath("models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
        var candidateManifestPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "production-export",
            "skill-selection-manifest.json");
        var candidateCheckpointPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "production-export",
            "skill-selection-checkpoint.json");
        var p16eInputLedgerPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p16e-skill-selection-source-disjoint-eval-ablation",
            "skill-selection-p16e-eval-input-ledger.jsonl");
        var p16eReportPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p16e-skill-selection-source-disjoint-eval-ablation",
            "skill-selection-p16e-source-disjoint-eval-report.json");

        Directory.CreateDirectory(artifactDir);

        var commonCheckpointHashBefore = Sha256(commonCheckpointPath);
        var commonManifestHashBefore = Sha256(commonManifestPath);
        var candidateCheckpointHashBefore = Sha256(candidateCheckpointPath);
        var candidateManifestHashBefore = Sha256(candidateManifestPath);

        var commonMachine = new SkillSelectionMachine();
        commonMachine.LoadCheckpoint(ReadCheckpoint(commonCheckpointPath, preferSnakeCase: true));
        var candidateMachine = new SkillSelectionMachine();
        candidateMachine.LoadCheckpoint(ReadCheckpoint(candidateCheckpointPath, preferSnakeCase: false));

        var sourceRows = File.ReadLines(p16eInputLedgerPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<P16ESkillSelectionEvalInputRow>(line, SnakeJson))
            .Select(row => row ?? throw new InvalidOperationException("P16E row could not be parsed."))
            .ToArray();
        using var p16eReport = JsonDocument.Parse(File.ReadAllText(p16eReportPath, Encoding.UTF8));
        var p16eRuntimeEvaluation = p16eReport.RootElement.GetProperty("runtime_evaluation").GetBoolean();
        var p16eCandidateSchemaOnly = p16eReport.RootElement.GetProperty("candidate_schema_eval_only").GetBoolean();
        var sourceDisjointProvenForPromotion = p16eRuntimeEvaluation && !p16eCandidateSchemaOnly;

        var scenarioResults = sourceRows
            .Select(row => EvaluateScenario(
                row,
                commonMachine,
                candidateMachine,
                p16eInputLedgerPath,
                sourceDisjointProvenForPromotion))
            .ToArray();

        var commonCheckpointHashAfter = Sha256(commonCheckpointPath);
        var commonManifestHashAfter = Sha256(commonManifestPath);
        var candidateCheckpointHashAfter = Sha256(candidateCheckpointPath);
        var candidateManifestHashAfter = Sha256(candidateManifestPath);
        var commonPathDistinctFromCandidate = !string.Equals(
            Path.GetFullPath(commonCheckpointPath),
            Path.GetFullPath(candidateCheckpointPath),
            StringComparison.OrdinalIgnoreCase);
        var modelsUnchanged = commonCheckpointHashBefore == commonCheckpointHashAfter
            && commonManifestHashBefore == commonManifestHashAfter
            && candidateCheckpointHashBefore == candidateCheckpointHashAfter
            && candidateManifestHashBefore == candidateManifestHashAfter;
        var outputParityCount = scenarioResults.Count(row => row.OutputsEqual);
        var commonExpectedMatchCount = scenarioResults.Count(row => row.CommonMatchesExpected);
        var candidateExpectedMatchCount = scenarioResults.Count(row => row.CandidateMatchesExpected);
        var compatibilityPass = sourceDisjointProvenForPromotion
            && modelsUnchanged
            && commonPathDistinctFromCandidate
            && scenarioResults.Length > 0
            && scenarioResults.All(row => row.OutputsEqual);
        var promotionAllowed = compatibilityPass;

        WriteArtifacts(
            artifactDir,
            commonManifestPath,
            commonCheckpointPath,
            candidateManifestPath,
            candidateCheckpointPath,
            p16eInputLedgerPath,
            p16eReportPath,
            commonCheckpointHashBefore,
            candidateCheckpointHashBefore,
            sourceDisjointProvenForPromotion,
            p16eRuntimeEvaluation,
            p16eCandidateSchemaOnly,
            modelsUnchanged,
            commonPathDistinctFromCandidate,
            outputParityCount,
            commonExpectedMatchCount,
            candidateExpectedMatchCount,
            compatibilityPass,
            promotionAllowed,
            scenarioResults);

        var reportPath = Path.Combine(artifactDir, "p21-compatibility-nonregression-report.json");
        var scenarioPath = Path.Combine(artifactDir, "p21-scenario-results.jsonl");
        var selfCheckPath = Path.Combine(artifactDir, "p21-self-check.json");
        using var report = JsonDocument.Parse(File.ReadAllText(reportPath, Encoding.UTF8));
        using var selfCheck = JsonDocument.Parse(File.ReadAllText(selfCheckPath, Encoding.UTF8));
        var rows = File.ReadLines(scenarioPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();

        Assert.True(commonPathDistinctFromCandidate);
        Assert.True(modelsUnchanged);
        Assert.Equal(sourceRows.Length, rows.Length);
        Assert.False(sourceDisjointProvenForPromotion);
        Assert.False(compatibilityPass);
        Assert.False(promotionAllowed);
        Assert.False(report.RootElement.GetProperty("compatibilityPass").GetBoolean());
        Assert.False(report.RootElement.GetProperty("promotionAllowed").GetBoolean());
        Assert.False(report.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.False(report.RootElement.GetProperty("skillSelectionReadinessClaimed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("promotionAllowed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.All(rows, row =>
        {
            Assert.False(row.RootElement.GetProperty("sourceDisjointProvenForPromotion").GetBoolean());
            Assert.False(row.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());
            Assert.Equal("evidence_gap_not_source_disjoint", row.RootElement.GetProperty("scenarioEvidenceStatus").GetString());
        });

        foreach (var path in Directory.EnumerateFiles(artifactDir))
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            AssertNoEncodingDamage(text);
        }

        foreach (var row in rows)
            row.Dispose();
    }

    private static P21ScenarioResult EvaluateScenario(
        P16ESkillSelectionEvalInputRow row,
        SkillSelectionMachine commonMachine,
        SkillSelectionMachine candidateMachine,
        string sourcePath,
        bool sourceDisjointProvenForPromotion)
    {
        var request = new SkillSelectionRequest(
            Prompt: BuildPrompt(row),
            ProjectRoot: "redacted-project-root",
            AvailableSkillIds: row.AvailableSkills,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["p21_source_row_id"] = row.RowId,
                ["p21_unit_target"] = row.UnitTarget,
                ["p21_source_family"] = row.SourceFamily,
                ["p21_split_role"] = row.SplitRole,
                ["p21_fixture_status"] = "derived_from_p16e_schema_row"
            },
            UserPromptSourceEntryId: row.RowId);
        var commonDecision = commonMachine.DecideFromTrainedState(request).Value!;
        var candidateDecision = candidateMachine.DecideFromTrainedState(request).Value!;

        return new P21ScenarioResult(
            RowId: row.RowId,
            SplitRole: row.SplitRole,
            UnitTarget: row.UnitTarget,
            SourceFamily: row.SourceFamily,
            SourceDisjointGrouping: row.SourceDisjointGrouping,
            ScenarioSourceRef: RelativePath(sourcePath),
            ScenarioEvidenceStatus: sourceDisjointProvenForPromotion
                ? "source_disjoint_runtime_probe"
                : "evidence_gap_not_source_disjoint",
            SourceDisjointProvenForPromotion: sourceDisjointProvenForPromotion,
            PromptSha256: Sha256Text(request.Prompt),
            AvailableSkills: row.AvailableSkills,
            ExpectedChosen: row.ExpectedChosen,
            ExpectedRejected: row.ExpectedRejected,
            CommonSelectedSkills: commonDecision.SelectedSkillIds,
            CandidateSelectedSkills: candidateDecision.SelectedSkillIds,
            CommonRejectedSkills: commonDecision.RejectedSkillIds,
            CandidateRejectedSkills: candidateDecision.RejectedSkillIds,
            CommonRequiresSecondPass: commonDecision.RequiresSecondPass,
            CandidateRequiresSecondPass: candidateDecision.RequiresSecondPass,
            CommonConfidence: commonDecision.Confidence,
            CandidateConfidence: candidateDecision.Confidence,
            CommonDecisionText: commonDecision.DecisionText,
            CandidateDecisionText: candidateDecision.DecisionText,
            CommonEvidence: commonDecision.EvidenceFeatures,
            CandidateEvidence: candidateDecision.EvidenceFeatures,
            OutputsEqual: commonDecision.SelectedSkillIds.SequenceEqual(candidateDecision.SelectedSkillIds, StringComparer.Ordinal)
                && commonDecision.RequiresSecondPass == candidateDecision.RequiresSecondPass,
            CommonMatchesExpected: commonDecision.SelectedSkillIds.Contains(row.ExpectedChosen, StringComparer.Ordinal),
            CandidateMatchesExpected: candidateDecision.SelectedSkillIds.Contains(row.ExpectedChosen, StringComparer.Ordinal),
            SemanticQualityClaimed: false);
    }

    private static string BuildPrompt(P16ESkillSelectionEvalInputRow row)
        => row.UnitTarget switch
        {
            "candidate_position_guard" =>
                $"Проверь, что выбор skill не зависит от порядка кандидатов. Ожидаемый полезный skill: {row.ExpectedChosen}; неправильный отвлекающий skill: {row.ExpectedRejected}.",
            "prompt_understanding" =>
                $"По смыслу пользовательского запроса надо выбрать skill {row.ExpectedChosen}, а не {row.ExpectedRejected}; ориентируйся на intent, а не на имя файла.",
            "task_understanding" =>
                $"Задача требует архитектурной классификации trainable unit, DPO, checkpoint и lifecycle; выбери {row.ExpectedChosen}, не {row.ExpectedRejected}.",
            "prompt_context" =>
                $"Контекст запроса меняет выбор skill: нужны evidence, source boundary и claim verification; выбери {row.ExpectedChosen}, не {row.ExpectedRejected}.",
            "intermediate_checks" =>
                $"Промежуточная проверка нашла gap: readability, tests, evidence и repair должны выбрать {row.ExpectedChosen}, не {row.ExpectedRejected}.",
            "runtime_chain" =>
                $"Нужно проверить runtime chain, model evidence и no-overclaim boundary; правильный skill {row.ExpectedChosen}, неправильный {row.ExpectedRejected}.",
            _ =>
                $"SkillSelection compatibility probe for {row.UnitTarget}: expected {row.ExpectedChosen}, rejected {row.ExpectedRejected}."
        };

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
            && !string.IsNullOrWhiteSpace(checkpoint.RouteMode);

    private static void WriteArtifacts(
        string artifactDir,
        string commonManifestPath,
        string commonCheckpointPath,
        string candidateManifestPath,
        string candidateCheckpointPath,
        string p16eInputLedgerPath,
        string p16eReportPath,
        string commonCheckpointSha256,
        string candidateCheckpointSha256,
        bool sourceDisjointProvenForPromotion,
        bool p16eRuntimeEvaluation,
        bool p16eCandidateSchemaOnly,
        bool modelsUnchanged,
        bool commonPathDistinctFromCandidate,
        int outputParityCount,
        int commonExpectedMatchCount,
        int candidateExpectedMatchCount,
        bool compatibilityPass,
        bool promotionAllowed,
        IReadOnlyList<P21ScenarioResult> scenarioResults)
    {
        var scenarioPath = Path.Combine(artifactDir, "p21-scenario-results.jsonl");
        WriteUtf8Lf(
            scenarioPath,
            string.Join("\n", scenarioResults.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");

        var report = new
        {
            Phase = "P21_SKILL_SELECTION_COMPATIBILITY_NONREGRESSION",
            ArtifactType = "side-by-side compatibility/non-regression evidence only",
            Language = "ru",
            SummaryRu = "P21 загрузил existing common checkpoint и P17E candidate checkpoint в разные SkillSelectionMachine instances и прогнал одинаковые P16E-derived сценарии. Promotion запрещена, потому что P16E источник является evidence/schema-only, не доказан source-disjoint runtime eval для promotion.",
            CommonManifestPath = RelativePath(commonManifestPath),
            CommonCheckpointPath = RelativePath(commonCheckpointPath),
            CandidateManifestPath = RelativePath(candidateManifestPath),
            CandidateCheckpointPath = RelativePath(candidateCheckpointPath),
            CommonCheckpointSha256 = commonCheckpointSha256,
            CandidateCheckpointSha256 = candidateCheckpointSha256,
            P16EInputLedgerRef = RelativePath(p16eInputLedgerPath),
            P16EReportRef = RelativePath(p16eReportPath),
            P16ERuntimeEvaluation = p16eRuntimeEvaluation,
            P16ECandidateSchemaOnly = p16eCandidateSchemaOnly,
            SourceDisjointProven = sourceDisjointProvenForPromotion,
            SourceDisjointReasonRu = "P16E даёт лучшие имеющиеся held-out-like rows, но сам report говорит candidate_schema_eval_only=true и runtime_evaluation=false; поэтому P21 fixture не может открыть promotion.",
            ScenarioCount = scenarioResults.Count,
            OutputParityCount = outputParityCount,
            CommonExpectedMatchCount = commonExpectedMatchCount,
            CandidateExpectedMatchCount = candidateExpectedMatchCount,
            ModelsUnchanged = modelsUnchanged,
            CommonPathDistinctFromCandidate = commonPathDistinctFromCandidate,
            CompatibilityPass = compatibilityPass,
            PromotionAllowed = promotionAllowed,
            SemanticQualityClaimed = false,
            SkillSelectionReadinessClaimed = false,
            RuntimePromotionPerformed = false,
            CommonModelFilesModified = false,
            TrainingRun = false,
            CheckpointMutated = false,
            ScenarioResultsRef = "p21-scenario-results.jsonl"
        };
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p21-compatibility-nonregression-report.json"),
            JsonSerializer.Serialize(report, WebJson) + "\n");

        WriteUtf8Lf(
            Path.Combine(artifactDir, "p21-compatibility-nonregression-report.md"),
            BuildHumanReport(report.SummaryRu, scenarioResults.Count, outputParityCount, commonExpectedMatchCount, candidateExpectedMatchCount));

        var reviewRows = new[]
        {
            new { CycleId = "p21_review_01_goal_delivery", Status = "passed", FindingRu = "Оба checkpoint-а загружены в отдельные SkillSelectionMachine instances; сценарии записаны построчно.", FixOrEvidenceRu = "p21-scenario-results.jsonl содержит common и candidate outputs для каждого P16E-derived сценария." },
            new { CycleId = "p21_review_02_source_disjoint_validity", Status = "passed_with_blocker_preserved", FindingRu = "P16E источник полезен как held-out-like fixture, но не доказывает runtime source-disjoint promotion.", FixOrEvidenceRu = "sourceDisjointProven=false, compatibilityPass=false, promotionAllowed=false." },
            new { CycleId = "p21_review_03_no_overclaim", Status = "passed", FindingRu = "P21 не заявляет semantic quality, readiness или final model readiness.", FixOrEvidenceRu = "Все no-overclaim flags false/blocked в report и self-check." },
            new { CycleId = "p21_review_04_runtime_model_safety", Status = "passed", FindingRu = "Production code и models/skill-selection path не меняются.", FixOrEvidenceRu = "Test сравнивает SHA-256 до/после и требует distinct common/candidate paths." },
            new { CycleId = "p21_review_05_utf8_readability", Status = "passed", FindingRu = "Артефакты пишутся UTF-8 без escaped Cyrillic, CRLF и mojibake markers.", FixOrEvidenceRu = "Focused test сканирует все P21 files." },
            new { CycleId = "p21_review_06_next_gate", Status = "passed_with_next_action", FindingRu = "Следующая реальная работа не promotion, а устранение source-disjoint/runtime-eval gap.", FixOrEvidenceRu = "state next_action должен идти в parent review P21, затем repair gate, а не в runtime overwrite." }
        };
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p21-review-cycles.jsonl"),
            string.Join("\n", reviewRows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");

        var selfCheck = new
        {
            Phase = "P21",
            JsonAndJsonlParseExpected = true,
            Utf8ReadableRussianExpected = true,
            SourceDisjointProven = sourceDisjointProvenForPromotion,
            CompatibilityPass = compatibilityPass,
            PromotionAllowed = promotionAllowed,
            SemanticQualityClaimed = false,
            SkillSelectionReadinessClaimed = false,
            RuntimePromotionPerformed = false,
            CommonModelFilesModified = false,
            TrainingRun = false,
            CheckpointMutated = false,
            ExternalAiOrNetworkUsed = false,
            ScenarioRowsExist = scenarioResults.Count > 0,
            ModelsUnchanged = modelsUnchanged,
            CommonPathDistinctFromCandidate = commonPathDistinctFromCandidate,
            FocusedTest = "SmartSkillsSemanticQualityP21CompatibilityNonRegressionTests"
        };
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p21-self-check.json"),
            JsonSerializer.Serialize(selfCheck, WebJson) + "\n");

        WriteUtf8Lf(
            Path.Combine(artifactDir, "p21-evidence-card.md"),
            """
            # P21 evidence card

            P21 проверяет не promotion, а возможность честно сравнить два SkillSelection checkpoint-а.

            Что доказано:
            - existing common checkpoint и P17E candidate checkpoint загружаются в отдельные `SkillSelectionMachine`;
            - одинаковые P16E-derived сценарии дают наблюдаемые outputs для обеих моделей;
            - production code и `models/skill-selection/skill-selection-v1` не менялись.

            Что НЕ доказано:
            - source-disjoint runtime eval для promotion;
            - semantic quality;
            - готовность P17E заменить common model;
            - live extension/runtime readiness.

            Вывод: promotion запрещена до отдельного source-disjoint runtime gate и совместимости с common skill vocabulary.
            """);
    }

    private static string BuildHumanReport(
        string summary,
        int scenarioCount,
        int outputParityCount,
        int commonExpectedMatchCount,
        int candidateExpectedMatchCount)
        => $$"""
        # P21 — SkillSelection compatibility / non-regression

        {{summary}}

        ## Что было проверено

        P21 взял два файла модели:

        - common model: `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json`
        - P17E candidate: `artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/production-export/skill-selection-checkpoint.json`

        Оба checkpoint-а были загружены в разные `SkillSelectionMachine` instances. Потом один и тот же набор P16E-derived сценариев был отправлен в оба экземпляра через `DecideFromTrainedState`.

        ## Почему promotion всё ещё запрещена

        P16E сценарии — лучший уже имеющийся источник для проверки SkillSelection, но сам P16E report говорит, что это schema/evidence-only материал, а не production runtime evaluation. Поэтому даже при наличии строк сравнения P21 не может заявить source-disjoint runtime proof.

        ## Наблюдения

        - Scenario rows: {{scenarioCount}}
        - Одинаковые outputs common/candidate: {{outputParityCount}}
        - Common совпал с expected skill: {{commonExpectedMatchCount}}
        - Candidate совпал с expected skill: {{candidateExpectedMatchCount}}

        ## Итог

        Compatibility pass: false.

        Promotion allowed: false.

        Semantic quality claimed: false.
        """;

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
        Assert.DoesNotContain(((char)0xFFFD).ToString(), text, StringComparison.Ordinal);
        Assert.DoesNotContain("\\u04", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(new Regex(@"\?{3,}"), text);
        Assert.DoesNotContain("\r\n", text, StringComparison.Ordinal);
    }

    private static string RelativePath(string path)
        => Path.GetRelativePath(RepoRoot, path).Replace('\\', '/');

    private static string RepoPath(params string[] segments)
        => Path.Combine(new[] { RepoRoot }.Concat(segments).ToArray());

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string Sha256Text(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

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

    private sealed record P16ESkillSelectionEvalInputRow(
        string RowId,
        string SplitRole,
        string UnitTarget,
        string SourceFamily,
        string SourceDisjointGrouping,
        string ExpectedChosen,
        string ExpectedRejected,
        IReadOnlyList<string> AvailableSkills,
        bool UsedAsTrainingInput,
        bool UsedAsEvalInput,
        bool RawPrivateTextCopied,
        bool SourceRefKeptAsProvenanceOnly);

    private sealed record P21ScenarioResult(
        string RowId,
        string SplitRole,
        string UnitTarget,
        string SourceFamily,
        string SourceDisjointGrouping,
        string ScenarioSourceRef,
        string ScenarioEvidenceStatus,
        bool SourceDisjointProvenForPromotion,
        string PromptSha256,
        IReadOnlyList<string> AvailableSkills,
        string ExpectedChosen,
        string ExpectedRejected,
        IReadOnlyList<string> CommonSelectedSkills,
        IReadOnlyList<string> CandidateSelectedSkills,
        IReadOnlyList<string> CommonRejectedSkills,
        IReadOnlyList<string> CandidateRejectedSkills,
        bool CommonRequiresSecondPass,
        bool CandidateRequiresSecondPass,
        double CommonConfidence,
        double CandidateConfidence,
        string CommonDecisionText,
        string CandidateDecisionText,
        IReadOnlyList<string> CommonEvidence,
        IReadOnlyList<string> CandidateEvidence,
        bool OutputsEqual,
        bool CommonMatchesExpected,
        bool CandidateMatchesExpected,
        bool SemanticQualityClaimed);
}
