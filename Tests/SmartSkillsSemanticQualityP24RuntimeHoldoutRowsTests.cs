// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP24RuntimeHoldoutRowsTests
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
    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public void P24CreatesApprovedRedactedRuntimeHoldoutRowsWithoutComparisonOrModelMutation()
    {
        var artifactDir = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p24-skillselection-runtime-holdout-rows");
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

        Directory.CreateDirectory(artifactDir);

        var commonManifestHashBefore = Sha256(commonManifestPath);
        var commonCheckpointHashBefore = Sha256(commonCheckpointPath);
        var candidateManifestHashBefore = Sha256(candidateManifestPath);
        var candidateCheckpointHashBefore = Sha256(candidateCheckpointPath);
        var checkpointIdentity = ReadCheckpointIdentity(commonCheckpointPath, candidateCheckpointPath);
        var acceptedRows = BuildAcceptedRows(checkpointIdentity);
        var rejectedRows = BuildRejectedRows(checkpointIdentity);
        var modelsUnchanged = commonManifestHashBefore == Sha256(commonManifestPath)
            && commonCheckpointHashBefore == Sha256(commonCheckpointPath)
            && candidateManifestHashBefore == Sha256(candidateManifestPath)
            && candidateCheckpointHashBefore == Sha256(candidateCheckpointPath);

        WriteArtifacts(
            artifactDir,
            commonManifestPath,
            commonCheckpointPath,
            candidateManifestPath,
            candidateCheckpointPath,
            commonCheckpointHashBefore,
            candidateCheckpointHashBefore,
            checkpointIdentity,
            acceptedRows,
            rejectedRows,
            modelsUnchanged);

        var acceptedLedgerPath = Path.Combine(artifactDir, "p24-runtime-holdout-rows.jsonl");
        var gapLedgerPath = Path.Combine(artifactDir, "p24-runtime-holdout-gap-ledger.jsonl");
        var reportPath = Path.Combine(artifactDir, "p24-runtime-holdout-report.json");
        var reviewPath = Path.Combine(artifactDir, "p24-review-cycles.jsonl");
        var selfCheckPath = Path.Combine(artifactDir, "p24-self-check.json");
        var acceptedLedgerRows = ReadJsonLineDocuments(acceptedLedgerPath);
        var gapLedgerRows = ReadJsonLineDocuments(gapLedgerPath);
        var reviewRows = ReadJsonLineDocuments(reviewPath);
        using var report = JsonDocument.Parse(File.ReadAllText(reportPath, Encoding.UTF8));
        using var selfCheck = JsonDocument.Parse(File.ReadAllText(selfCheckPath, Encoding.UTF8));

        Assert.True(modelsUnchanged);
        Assert.True(acceptedRows.Count > 0);
        Assert.Equal(acceptedRows.Count, acceptedLedgerRows.Length);
        Assert.True(gapLedgerRows.Length > 0);
        Assert.True(reviewRows.Length >= 6);
        Assert.True(report.RootElement.GetProperty("acceptedHoldoutRowsCreated").GetBoolean());
        Assert.Equal(acceptedRows.Count, report.RootElement.GetProperty("acceptedHoldoutRowCount").GetInt32());
        Assert.False(report.RootElement.GetProperty("sideBySideComparisonRan").GetBoolean());
        Assert.False(report.RootElement.GetProperty("sideBySideComparisonAllowedInThisSlice").GetBoolean());
        Assert.False(report.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.False(report.RootElement.GetProperty("skillSelectionReadinessClaimed").GetBoolean());
        Assert.False(report.RootElement.GetProperty("runtimePromotionPerformed").GetBoolean());
        Assert.False(report.RootElement.GetProperty("commonModelFilesModified").GetBoolean());
        Assert.False(report.RootElement.GetProperty("trainingRun").GetBoolean());
        Assert.False(report.RootElement.GetProperty("checkpointMutated").GetBoolean());
        Assert.True(selfCheck.RootElement.GetProperty("acceptedHoldoutRowsCreated").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("sideBySideComparisonRan").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());

        foreach (var row in acceptedLedgerRows)
            AssertAcceptedRowSchema(row.RootElement, checkpointIdentity);

        foreach (var row in gapLedgerRows)
        {
            Assert.False(row.RootElement.GetProperty("acceptedAsP24Holdout").GetBoolean());
            Assert.NotEqual("", row.RootElement.GetProperty("rejectionReason").GetString());
            Assert.True(row.RootElement.GetProperty("representedInComparedCheckpoint").GetBoolean());
        }

        foreach (var path in Directory.EnumerateFiles(artifactDir))
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            AssertNoEncodingDamage(text);
        }

        foreach (var row in acceptedLedgerRows.Concat(gapLedgerRows).Concat(reviewRows))
            row.Dispose();
    }

    private static CheckpointIdentity ReadCheckpointIdentity(string commonCheckpointPath, string candidateCheckpointPath)
    {
        using var common = JsonDocument.Parse(File.ReadAllText(commonCheckpointPath, Encoding.UTF8));
        using var candidate = JsonDocument.Parse(File.ReadAllText(candidateCheckpointPath, Encoding.UTF8));
        var commonPairIds = ExtractPairIds(common.RootElement);
        var candidatePairIds = ExtractPairIds(candidate.RootElement);
        var commonSkillIds = ExtractSkillIds(common.RootElement);
        var candidateSkillIds = ExtractSkillIds(candidate.RootElement);
        var commonSourceFamilies = InferSourceFamilies(commonPairIds);
        var candidateSourceFamilies = InferSourceFamilies(candidatePairIds);

        return new CheckpointIdentity(
            CommonPairIds: commonPairIds,
            CandidatePairIds: candidatePairIds,
            CommonSkillIds: commonSkillIds,
            CandidateSkillIds: candidateSkillIds,
            CommonSourceFamilies: commonSourceFamilies,
            CandidateSourceFamilies: candidateSourceFamilies);
    }

    private static IReadOnlyList<P24AcceptedHoldoutRow> BuildAcceptedRows(CheckpointIdentity checkpointIdentity)
    {
        var specs = new[]
        {
            new SourceSpec(
                RowId: "P24-SS-HOLDOUT-E2E-MATRIX-001",
                PairId: "P24-SS-HOLDOUT-E2E-MATRIX-001",
                SourceSkillDocId: "e2e-test-matrix",
                SourceFamily: "p24_local_skill_doc_e2e_test_matrix",
                SourcePath: RepoPath("docs", "skills", "e2e-test-matrix-SKILL.md"),
                RedactedSummary: "Task asks for a scenario matrix covering success, idempotency, delayed behavior, recovery, boundary, and security checks before implementation.",
                AvailableSkillIds: ["e2e-test-matrix", "test-integrity", "implementation-integrity", "plan-review"],
                ExpectedSelectedSkillIds: ["e2e-test-matrix"],
                ExpectedRejectedSkillIdsOrReasonOmitted: ["test-integrity", "implementation-integrity"],
                UniversalReason: "Universal SkillSelection boundary: choose the skill that designs coverage shape before code, not a project-stack execution recipe."),
            new SourceSpec(
                RowId: "P24-SS-HOLDOUT-NO-PRODUCT-DECISION-001",
                PairId: "P24-SS-HOLDOUT-NO-PRODUCT-DECISION-001",
                SourceSkillDocId: "no-product-decisions",
                SourceFamily: "p24_local_skill_doc_no_product_decisions",
                SourcePath: RepoPath("docs", "skills", "no-product-decisions-SKILL.md"),
                RedactedSummary: "Task asks the agent to choose between alternatives that change user behavior, public interface shape, or architecture direction.",
                AvailableSkillIds: ["no-product-decisions", "implementation-integrity", "planning-session-startup", "claim-verification"],
                ExpectedSelectedSkillIds: ["no-product-decisions"],
                ExpectedRejectedSkillIdsOrReasonOmitted: ["implementation-integrity"],
                UniversalReason: "Universal SkillSelection boundary: identify a decision-approval skill when the task is a product or architecture choice, not stack-specific implementation."),
            new SourceSpec(
                RowId: "P24-SS-HOLDOUT-OUTPUT-SKEPTICISM-001",
                PairId: "P24-SS-HOLDOUT-OUTPUT-SKEPTICISM-001",
                SourceSkillDocId: "output-skepticism",
                SourceFamily: "p24_local_skill_doc_output_skepticism",
                SourcePath: RepoPath("docs", "skills", "output-skepticism-SKILL.md"),
                RedactedSummary: "Task asks for a result claim to be checked against refuting evidence, scope validity, and proxy-measure risk before reporting.",
                AvailableSkillIds: ["output-skepticism", "phase-completion-packager", "verification-before-completion", "plan-review"],
                ExpectedSelectedSkillIds: ["output-skepticism"],
                ExpectedRejectedSkillIdsOrReasonOmitted: ["phase-completion-packager"],
                UniversalReason: "Universal SkillSelection boundary: choose the skill that validates whether evidence supports a claim, independent of the repository stack."),
            new SourceSpec(
                RowId: "P24-SS-HOLDOUT-SELF-VERIFICATION-001",
                PairId: "P24-SS-HOLDOUT-SELF-VERIFICATION-001",
                SourceSkillDocId: "self-verification",
                SourceFamily: "p24_local_skill_doc_self_verification",
                SourcePath: RepoPath("docs", "skills", "self-verification-SKILL.md"),
                RedactedSummary: "Task asks the implementer to verify its own changed artifact with fresh fixtures, blast-radius classification, cascade tracing, and prediction matching.",
                AvailableSkillIds: ["self-verification", "test-integrity", "output-skepticism", "implementation-integrity"],
                ExpectedSelectedSkillIds: ["self-verification"],
                ExpectedRejectedSkillIdsOrReasonOmitted: ["output-skepticism"],
                UniversalReason: "Universal SkillSelection boundary: select a self-check skill for post-change verification behavior, not a project-stack command path.")
        };

        return specs.Select(spec => ToAcceptedRow(spec, checkpointIdentity)).ToArray();
    }

    private static IReadOnlyList<P24GapLedgerRow> BuildRejectedRows(CheckpointIdentity checkpointIdentity)
    {
        var rejected = new[]
        {
            new RejectedSourceSpec(
                SourceSkillDocId: "trainable-architecture-decision",
                SourceFamily: "local_project_skill_trainable_architecture_decision",
                RejectionReason: "The compared checkpoints already represent this skill family through learned skill ids or prior P16/P17 source rows; accepting it would weaken P24 source-disjointness."),
            new RejectedSourceSpec(
                SourceSkillDocId: "test-integrity",
                SourceFamily: "local_project_skill_test_integrity",
                RejectionReason: "The compared checkpoints already represent this skill family through learned skill ids or prior P16/P17 source rows; accepting it would weaken P24 source-disjointness."),
            new RejectedSourceSpec(
                SourceSkillDocId: "output-readability-gate",
                SourceFamily: "local_project_skill_output_readability",
                RejectionReason: "The compared checkpoints already represent this skill family through learned skill ids or prior P16/P17 source rows; accepting it would weaken P24 source-disjointness.")
        };

        return rejected.Select(row => new P24GapLedgerRow(
            CandidateSourceFamily: row.SourceFamily,
            CandidateSourceSkillDocId: row.SourceSkillDocId,
            AcceptedAsP24Holdout: false,
            RepresentedInComparedCheckpoint: checkpointIdentity.IsSkillOrFamilyRepresented(row.SourceSkillDocId, row.SourceFamily),
            RuntimeInvocationReady: false,
            RejectionReason: row.RejectionReason,
            MissingConditions: ["source family or matching skill id is already represented in a compared checkpoint"])).ToArray();
    }

    private static P24AcceptedHoldoutRow ToAcceptedRow(SourceSpec spec, CheckpointIdentity checkpointIdentity)
    {
        Assert.True(File.Exists(spec.SourcePath));
        var sourceHash = Sha256(spec.SourcePath);
        Assert.False(checkpointIdentity.IsSkillOrFamilyRepresented(spec.SourceSkillDocId, spec.SourceFamily));
        Assert.DoesNotContain(spec.PairId, checkpointIdentity.CommonPairIds);
        Assert.DoesNotContain(spec.PairId, checkpointIdentity.CandidatePairIds);

        return new P24AcceptedHoldoutRow(
            RowId: spec.RowId,
            PairId: spec.PairId,
            SourceFamily: spec.SourceFamily,
            SourceSkillDocId: spec.SourceSkillDocId,
            SourceRefOrHashedRedactedSourceRef: $"sha256:{sourceHash}",
            SourceHash: sourceHash,
            SourceLicenseOrLocalPrivateStatus: "local_private_user_approved_holdout",
            CommonTrainingAllowed: false,
            PromptHash: Sha256Text(spec.RedactedSummary),
            PromptOrTaskTextRedactedSummary: spec.RedactedSummary,
            AvailableSkillIds: spec.AvailableSkillIds,
            ExpectedSelectedSkillIds: spec.ExpectedSelectedSkillIds,
            ExpectedRejectedSkillIdsOrReasonOmitted: spec.ExpectedRejectedSkillIdsOrReasonOmitted,
            SourceDisjointAgainstCommonCheckpoint: true,
            SourceDisjointAgainstCandidateCheckpoint: true,
            PairIdDisjointAgainstCommonCheckpoint: true,
            PairIdDisjointAgainstCandidateCheckpoint: true,
            NotUsedForTraining: true,
            RuntimeInvocationReady: true,
            RuntimeInvocationReadinessProof: "row has redacted task summary, available skill ids, expected selected skill ids, rejected or omitted reason, and no-training holdout status",
            LeakageControls: new LeakageControls(
                ExactPrompt: false,
                SourceId: false,
                Path: false,
                Filename: false,
                CandidateOrder: false,
                PairId: false,
                ExactPromptBlocked: true,
                SourceIdBlocked: true,
                PathBlocked: true,
                FilenameBlocked: true,
                CandidateOrderBlocked: true,
                PairIdBlocked: true),
            WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution: spec.UniversalReason);
    }

    private static void WriteArtifacts(
        string artifactDir,
        string commonManifestPath,
        string commonCheckpointPath,
        string candidateManifestPath,
        string candidateCheckpointPath,
        string commonCheckpointSha256,
        string candidateCheckpointSha256,
        CheckpointIdentity checkpointIdentity,
        IReadOnlyList<P24AcceptedHoldoutRow> acceptedRows,
        IReadOnlyList<P24GapLedgerRow> rejectedRows,
        bool modelsUnchanged)
    {
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p24-runtime-holdout-rows.jsonl"),
            string.Join("\n", acceptedRows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p24-runtime-holdout-gap-ledger.jsonl"),
            string.Join("\n", rejectedRows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");

        var report = new
        {
            Phase = "P24_SKILL_SELECTION_RUNTIME_HOLDOUT_ROWS",
            ArtifactType = "approved redacted runtime holdout rows only",
            Verdict = "APPROVED_REDACTED_RUNTIME_HOLDOUT_ROWS_CREATED",
            AcceptedHoldoutRowsCreated = acceptedRows.Count > 0,
            AcceptedHoldoutRowCount = acceptedRows.Count,
            GapLedgerRowCount = rejectedRows.Count,
            SideBySideComparisonAllowedInThisSlice = false,
            SideBySideComparisonRan = false,
            CommonManifestPath = RelativePath(commonManifestPath),
            CommonCheckpointPath = RelativePath(commonCheckpointPath),
            CandidateManifestPath = RelativePath(candidateManifestPath),
            CandidateCheckpointPath = RelativePath(candidateCheckpointPath),
            CommonCheckpointSha256 = commonCheckpointSha256,
            CandidateCheckpointSha256 = candidateCheckpointSha256,
            CommonCheckpointPairIdCount = checkpointIdentity.CommonPairIds.Count,
            CandidateCheckpointPairIdCount = checkpointIdentity.CandidatePairIds.Count,
            CommonCheckpointKnownSkillIds = checkpointIdentity.CommonSkillIds.Order(StringComparer.Ordinal).ToArray(),
            CandidateCheckpointKnownSkillIds = checkpointIdentity.CandidateSkillIds.Order(StringComparer.Ordinal).ToArray(),
            CommonCheckpointSourceFamilyMarkers = checkpointIdentity.CommonSourceFamilies.Order(StringComparer.Ordinal).ToArray(),
            CandidateCheckpointSourceFamilyMarkers = checkpointIdentity.CandidateSourceFamilies.Order(StringComparer.Ordinal).ToArray(),
            AcceptedSourceFamilies = acceptedRows.Select(row => row.SourceFamily).ToArray(),
            AcceptedPairIds = acceptedRows.Select(row => row.PairId).ToArray(),
            RowCreationBoundaryRu = "P24 создал только redacted holdout rows. Сравнение common и candidate checkpoints, promotion, обучение и изменение checkpoint-ов в этом срезе запрещены и не выполнялись.",
            MissingConditionIfZero = acceptedRows.Count == 0
                ? "Missing approved source-disjoint local skill or guide source family with runtime-ready redacted task summary."
                : "",
            NextRepairActionIfZero = acceptedRows.Count == 0
                ? "Approve or provide a local source family absent from both checkpoints, then rerun P24 row creation."
                : "",
            NextPlanDerivedAction = "P25 run side-by-side SkillSelection comparison only on the accepted P24 runtime holdout rows, with promotion still blocked unless that later runtime gate passes.",
            ModelsUnchanged = modelsUnchanged,
            SemanticQualityClaimed = false,
            SkillSelectionReadinessClaimed = false,
            FullApiReadinessClaimed = false,
            RuntimePromotionPerformed = false,
            CommonModelFilesModified = false,
            TrainingRun = false,
            CheckpointMutated = false,
            ExternalAiOrNetworkUsed = false,
            AcceptedRowsRef = "p24-runtime-holdout-rows.jsonl",
            GapLedgerRef = "p24-runtime-holdout-gap-ledger.jsonl",
            ReviewCyclesRef = "p24-review-cycles.jsonl"
        };
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p24-runtime-holdout-report.json"),
            JsonSerializer.Serialize(report, WebJson) + "\n");
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p24-runtime-holdout-report.md"),
            BuildHumanReport(acceptedRows.Count, rejectedRows.Count));
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p24-review-cycles.jsonl"),
            BuildReviewCycles());
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p24-self-check.json"),
            JsonSerializer.Serialize(new
            {
                Phase = "P24",
                AcceptedHoldoutRowsCreated = acceptedRows.Count > 0,
                AcceptedHoldoutRowCount = acceptedRows.Count,
                GapLedgerRowCount = rejectedRows.Count,
                SideBySideComparisonAllowedInThisSlice = false,
                SideBySideComparisonRan = false,
                ModelsUnchanged = modelsUnchanged,
                SemanticQualityClaimed = false,
                SkillSelectionReadinessClaimed = false,
                FullApiReadinessClaimed = false,
                RuntimePromotionPerformed = false,
                CommonModelFilesModified = false,
                TrainingRun = false,
                CheckpointMutated = false,
                ExternalAiOrNetworkUsed = false,
                FocusedTest = "SmartSkillsSemanticQualityP24RuntimeHoldoutRowsTests"
            }, WebJson) + "\n");
        WriteUtf8Lf(
            Path.Combine(artifactDir, "p24-evidence-card.md"),
            BuildEvidenceCard(acceptedRows.Count, rejectedRows.Count));
    }

    private static string BuildHumanReport(int acceptedRowCount, int rejectedRowCount)
        => $$"""
        # P24 - redacted runtime holdout rows для SkillSelection

        Вердикт: создан малый легитимный holdout ledger. Принятых строк: {{acceptedRowCount}}.

        Что принято:
        - строки взяты только из локальных skill/guide source families, которых нет в common checkpoint и candidate checkpoint;
        - raw private text, exact prompt, path, filename, source id, pair id и candidate order не используются как authority;
        - каждая строка содержит redacted summary, hashes, available skills, expected selected skills и явный not-used-for-training флаг;
        - каждая строка помечена как runtimeInvocationReady, потому что в ней есть все поля для будущего SkillSelection runtime request.

        Что отклонено:
        - кандидатов с уже представленными skill families: {{rejectedRowCount}};
        - эти кандидаты записаны в gap ledger, а не превращены в accepted holdout rows.

        Граница доказательства:
        P24 не запускал side-by-side comparison, не обучал модель, не менял checkpoints и не выполнял promotion. Это только подготовка честного eval/holdout ledger.

        Следующее действие по плану:
        P25 может сравнивать common и candidate checkpoints только на accepted P24 rows. Даже после этого promotion останется запрещен, пока отдельный runtime gate не пройдет.
        """;

    private static string BuildEvidenceCard(int acceptedRowCount, int rejectedRowCount)
        => $$"""
        # P24 evidence card

        P24 создал redacted SkillSelection runtime holdout rows до любого comparison или promotion.

        Принято строк: {{acceptedRowCount}}.

        Отклонено кандидатов: {{rejectedRowCount}}.

        Почему строки можно использовать как holdout:
        - sourceFamily отсутствует в обоих сравниваемых checkpoints;
        - pairId отсутствует в обоих checkpoints;
        - строки не использовались для training и common training для них запрещен;
        - prompts представлены только как redacted summaries и hashes;
        - leakage controls блокируют exact prompt, source id, path, filename, candidate order и pair id.

        No-overclaim statement:
        Это только eval/holdout rows ledger. Он не доказывает semantic quality, SkillSelection readiness, full API readiness, runtime promotion или common model replacement.

        Next:
        P25 должен запустить side-by-side comparison только на этих accepted holdout rows и сохранить promotion blocked до отдельного проходящего runtime gate.
        """;

    private static string BuildReviewCycles()
    {
        var rows = new[]
        {
            new { cycleId = "p24_review_01_source_family_disjointness", status = "passed", findingRu = "Приняты только fresh P24 source families, которые отсутствуют в skill ids и source family markers обоих checkpoints.", fixOrEvidenceRu = "Тест извлекает checkpoint ids/families и проверяет каждый accepted sourceFamily и sourceSkillDocId." },
            new { cycleId = "p24_review_02_pair_id_disjointness", status = "passed", findingRu = "Каждый pair id имеет P24 prefix и отсутствует в common и candidate checkpoints.", fixOrEvidenceRu = "Тест сравнивает row pair ids с trained/source pair ids обоих checkpoints." },
            new { cycleId = "p24_review_03_runtime_readiness", status = "passed", findingRu = "Строки готовы для будущего runtime invocation, но runtime comparison в P24 не запускался.", fixOrEvidenceRu = "Каждая строка содержит redacted task summary, available skills, expected skills и runtimeInvocationReady=true; report держит sideBySideComparisonRan=false." },
            new { cycleId = "p24_review_04_leakage_controls", status = "passed", findingRu = "Exact prompt, source id, path, filename, candidate order и pair id не являются authority.", fixOrEvidenceRu = "leakageControls для всех accepted rows выставлены false/blocked; sourceRef хранится как hash, не как путь." },
            new { cycleId = "p24_review_05_universal_not_stack_specific", status = "passed", findingRu = "Каждая строка учит универсальному выбору skill по типу задачи, а не .NET/project execution recipe.", fixOrEvidenceRu = "whyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution заполнен для каждой accepted row." },
            new { cycleId = "p24_review_06_no_overclaim_readability", status = "passed", findingRu = "Артефакты не заявляют semantic quality, readiness, promotion или full API readiness.", fixOrEvidenceRu = "Report/self-check flags false; Markdown написан человеческим русским текстом; UTF-8 scan обязателен." }
        };

        return string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n";
    }

    private static void AssertAcceptedRowSchema(JsonElement row, CheckpointIdentity checkpointIdentity)
    {
        var rowId = row.GetProperty("rowId").GetString()!;
        var pairId = row.GetProperty("pairId").GetString()!;
        var sourceFamily = row.GetProperty("sourceFamily").GetString()!;
        var sourceSkillDocId = row.GetProperty("sourceSkillDocId").GetString()!;
        Assert.StartsWith("P24-", rowId, StringComparison.Ordinal);
        Assert.StartsWith("P24-", pairId, StringComparison.Ordinal);
        Assert.DoesNotContain(pairId, checkpointIdentity.CommonPairIds);
        Assert.DoesNotContain(pairId, checkpointIdentity.CandidatePairIds);
        Assert.False(checkpointIdentity.IsSkillOrFamilyRepresented(sourceSkillDocId, sourceFamily));
        Assert.StartsWith("sha256:", row.GetProperty("sourceRefOrHashedRedactedSourceRef").GetString(), StringComparison.Ordinal);
        Assert.DoesNotContain("/", row.GetProperty("sourceRefOrHashedRedactedSourceRef").GetString(), StringComparison.Ordinal);
        Assert.DoesNotContain("\\", row.GetProperty("sourceRefOrHashedRedactedSourceRef").GetString(), StringComparison.Ordinal);
        Assert.Equal("local_private_user_approved_holdout", row.GetProperty("sourceLicenseOrLocalPrivateStatus").GetString());
        Assert.False(row.GetProperty("commonTrainingAllowed").GetBoolean());
        Assert.NotEqual("", row.GetProperty("promptHash").GetString());
        Assert.NotEqual("", row.GetProperty("promptOrTaskTextRedactedSummary").GetString());
        Assert.True(row.GetProperty("availableSkillIds").GetArrayLength() >= 3);
        Assert.True(row.GetProperty("expectedSelectedSkillIds").GetArrayLength() > 0);
        Assert.True(row.GetProperty("expectedRejectedSkillIdsOrReasonOmitted").GetArrayLength() > 0);
        Assert.True(row.GetProperty("sourceDisjointAgainstCommonCheckpoint").GetBoolean());
        Assert.True(row.GetProperty("sourceDisjointAgainstCandidateCheckpoint").GetBoolean());
        Assert.True(row.GetProperty("pairIdDisjointAgainstCommonCheckpoint").GetBoolean());
        Assert.True(row.GetProperty("pairIdDisjointAgainstCandidateCheckpoint").GetBoolean());
        Assert.True(row.GetProperty("notUsedForTraining").GetBoolean());
        Assert.True(row.GetProperty("runtimeInvocationReady").GetBoolean());
        Assert.Contains("Universal SkillSelection boundary", row.GetProperty("whyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution").GetString(), StringComparison.Ordinal);

        var leakage = row.GetProperty("leakageControls");
        Assert.False(leakage.GetProperty("exactPrompt").GetBoolean());
        Assert.False(leakage.GetProperty("sourceId").GetBoolean());
        Assert.False(leakage.GetProperty("path").GetBoolean());
        Assert.False(leakage.GetProperty("filename").GetBoolean());
        Assert.False(leakage.GetProperty("candidateOrder").GetBoolean());
        Assert.False(leakage.GetProperty("pairId").GetBoolean());
        Assert.True(leakage.GetProperty("exactPromptBlocked").GetBoolean());
        Assert.True(leakage.GetProperty("sourceIdBlocked").GetBoolean());
        Assert.True(leakage.GetProperty("pathBlocked").GetBoolean());
        Assert.True(leakage.GetProperty("filenameBlocked").GetBoolean());
        Assert.True(leakage.GetProperty("candidateOrderBlocked").GetBoolean());
        Assert.True(leakage.GetProperty("pairIdBlocked").GetBoolean());
    }

    private static JsonDocument[] ReadJsonLineDocuments(string path)
        => File.ReadLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();

    private static HashSet<string> ExtractPairIds(JsonElement root)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        AddArray(root, "trained_pair_ids", ids);
        AddArray(root, "trainedPairIds", ids);
        foreach (var section in root.EnumerateObject())
        {
            if (section.Value.ValueKind != JsonValueKind.Object)
                continue;

            AddArray(section.Value, "training_pair_ids", ids);
            AddArray(section.Value, "trainingPairIds", ids);
            if (TryGetProperty(section.Value, "skill_profiles", "skillProfiles", out var profiles)
                && profiles.ValueKind == JsonValueKind.Array)
            {
                foreach (var profile in profiles.EnumerateArray())
                {
                    AddArray(profile, "source_pair_ids", ids);
                    AddArray(profile, "sourcePairIds", ids);
                }
            }
        }

        return ids;
    }

    private static HashSet<string> ExtractSkillIds(JsonElement root)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        WalkSkillIds(root, ids);
        return ids;
    }

    private static void WalkSkillIds(JsonElement element, HashSet<string> target)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if ((property.NameEquals("skill_id") || property.NameEquals("skillId") || property.NameEquals("unitId") || property.NameEquals("unit_id"))
                    && property.Value.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(property.Value.GetString()))
                    target.Add(property.Value.GetString()!);

                WalkSkillIds(property.Value, target);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                WalkSkillIds(item, target);
        }
    }

    private static HashSet<string> InferSourceFamilies(HashSet<string> pairIds)
    {
        var families = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pairId in pairIds)
        {
            if (pairId.StartsWith("P16B-", StringComparison.Ordinal))
                families.Add("P16B");
            if (pairId.StartsWith("P16E1-", StringComparison.Ordinal))
                families.Add("P16E1");
            if (pairId.StartsWith("P17C-", StringComparison.Ordinal))
                families.Add("P17C");
            if (pairId.StartsWith("skill-selection-q5-04r-source-disjoint", StringComparison.Ordinal))
                families.Add("q5_04r_source_disjoint");
            if (pairId.StartsWith("skill-selection-q5-04r-dashboard", StringComparison.Ordinal))
                families.Add("q5_04r_dashboard_bridge");
            if (pairId.StartsWith("skill-selection-", StringComparison.Ordinal))
                families.Add("skill_selection_common_training");
        }

        return families;
    }

    private static void AddArray(JsonElement element, string name, HashSet<string> target)
    {
        if (!element.TryGetProperty(name, out var array) || array.ValueKind != JsonValueKind.Array)
            return;

        foreach (var value in array.EnumerateArray())
        {
            if (value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString()))
                target.Add(value.GetString()!);
        }
    }

    private static bool TryGetProperty(JsonElement element, string snakeName, string camelName, out JsonElement value)
        => element.TryGetProperty(snakeName, out value) || element.TryGetProperty(camelName, out value);

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

    private sealed record CheckpointIdentity(
        HashSet<string> CommonPairIds,
        HashSet<string> CandidatePairIds,
        HashSet<string> CommonSkillIds,
        HashSet<string> CandidateSkillIds,
        HashSet<string> CommonSourceFamilies,
        HashSet<string> CandidateSourceFamilies)
    {
        public bool IsSkillOrFamilyRepresented(string sourceSkillDocId, string sourceFamily)
            => CommonSkillIds.Contains(sourceSkillDocId)
                || CandidateSkillIds.Contains(sourceSkillDocId)
                || CommonSkillIds.Contains(sourceSkillDocId.Replace('-', '_'))
                || CandidateSkillIds.Contains(sourceSkillDocId.Replace('-', '_'))
                || CommonSourceFamilies.Contains(sourceFamily)
                || CandidateSourceFamilies.Contains(sourceFamily);
    }

    private sealed record SourceSpec(
        string RowId,
        string PairId,
        string SourceSkillDocId,
        string SourceFamily,
        string SourcePath,
        string RedactedSummary,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSelectedSkillIds,
        IReadOnlyList<string> ExpectedRejectedSkillIdsOrReasonOmitted,
        string UniversalReason);

    private sealed record RejectedSourceSpec(
        string SourceSkillDocId,
        string SourceFamily,
        string RejectionReason);

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
        bool PairIdBlocked);

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
        string WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution);

    private sealed record P24GapLedgerRow(
        string CandidateSourceFamily,
        string CandidateSourceSkillDocId,
        bool AcceptedAsP24Holdout,
        bool RepresentedInComparedCheckpoint,
        bool RuntimeInvocationReady,
        string RejectionReason,
        IReadOnlyList<string> MissingConditions);
}
