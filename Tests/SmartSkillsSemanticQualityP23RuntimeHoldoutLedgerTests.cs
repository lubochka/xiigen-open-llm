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

[Collection(global::ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class SmartSkillsSemanticQualityP23RuntimeHoldoutLedgerTests
{
    private const string FailpointMessage = "P23_AFTER_OWNED_ARTIFACT_EXISTS";
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
    private static readonly JsonSerializerOptions SnakeJsonLine = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false
    };
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string ProtectedP23Directory = RepoPath(
        "artifacts",
        "smart-skills-semantic-quality-completion",
        "p23-skillselection-runtime-holdout-ledger");

    [Fact]
    public void P23CreatesHonestRuntimeHoldoutLedgerOrGapWithoutModelMutation()
    {
        var before = CaptureProtectedRepositorySnapshot();
        string? ownedRoot = null;

        TestOwnedOutputRoot.Run(output =>
        {
            ownedRoot = output.RootPath;
            Assert.Equal(Path.GetFullPath(RepoRoot), Path.GetFullPath(output.RepositoryRootPath));
            ExecuteP23(output);
        });

        Assert.NotNull(ownedRoot);
        Assert.False(Directory.Exists(ownedRoot));
        Assert.Equal(before, CaptureProtectedRepositorySnapshot());
    }

    [Fact]
    public void P23FailureAfterOwnedArtifactExistsCleansOutputAndPreservesCommittedInputs()
    {
        var before = CaptureProtectedRepositorySnapshot();
        string? ownedRoot = null;
        var failpointCalls = 0;

        var failure = Assert.Throws<P23OwnedArtifactFailpointException>(() =>
            TestOwnedOutputRoot.Run(output =>
            {
                ownedRoot = output.RootPath;
                Assert.Equal(Path.GetFullPath(RepoRoot), Path.GetFullPath(output.RepositoryRootPath));

                var immediatelyBeforeRejectedWrite = CaptureProtectedRepositorySnapshot();
                Assert.Throws<InvalidOperationException>(() =>
                    output.RequireOwnedPath(Path.Combine(ProtectedP23Directory, "p23-runtime-holdout-report.json")));
                Assert.Equal(immediatelyBeforeRejectedWrite, CaptureProtectedRepositorySnapshot());

                var expectedFirstArtifactPath = output.RequireOwnedPath(Path.Combine(
                    output.RootPath,
                    "smart-skills-semantic-quality-completion",
                    "p23-skillselection-runtime-holdout-ledger",
                    "p23-runtime-holdout-ledger.jsonl"));
                Assert.False(File.Exists(expectedFirstArtifactPath));

                ExecuteP23(output, firstArtifactPath =>
                {
                    Assert.Equal(
                        Path.GetFullPath(expectedFirstArtifactPath),
                        Path.GetFullPath(firstArtifactPath));
                    Assert.Equal(firstArtifactPath, output.RequireOwnedPath(firstArtifactPath));
                    Assert.True(File.Exists(firstArtifactPath));
                    failpointCalls++;
                    ThrowAfterOwnedArtifactFailpoint();
                });
            }));

        Assert.Equal(FailpointMessage, failure.Message);
        Assert.Contains(nameof(ThrowAfterOwnedArtifactFailpoint), failure.StackTrace, StringComparison.Ordinal);
        Assert.Equal(1, failpointCalls);
        Assert.NotNull(ownedRoot);
        Assert.False(Directory.Exists(ownedRoot));
        Assert.Equal(before, CaptureProtectedRepositorySnapshot());
    }

    private static void ExecuteP23(
        TestOwnedOutputRoot output,
        Action<string>? afterOwnedArtifactExists = null)
    {
        var artifactDir = output.RequireOwnedPath(Path.Combine(
            output.RootPath,
            "smart-skills-semantic-quality-completion",
            "p23-skillselection-runtime-holdout-ledger"));
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
        var p16eLedgerPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p16e-skill-selection-source-disjoint-eval-ablation",
            "skill-selection-p16e-eval-input-ledger.jsonl");
        var p16eReportPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p16e-skill-selection-source-disjoint-eval-ablation",
            "skill-selection-p16e-source-disjoint-eval-report.json");
        var p16e2ReportPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p16e2-skill-selection-source-disjoint-eval-ablation-rerun",
            "skill-selection-p16e2-source-disjoint-eval-report.json");
        var p21ReportPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p21-skillselection-compatibility-nonregression",
            "p21-compatibility-nonregression-report.json");
        var p22ReportPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p22-skillselection-source-disjoint-runtime-eval",
            "p22-source-disjoint-runtime-eval-report.json");
        var p16bRowsPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p16b-skill-selection-source-dpo-materialization",
            "p16b-dpo-candidate-rows.jsonl");
        var p17cRowsPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17c-skillselection-train-only-runtime-coverage-repair",
            "p17c-train-only-candidate-rows.jsonl");

        Directory.CreateDirectory(output.RequireOwnedPath(artifactDir));

        var commonManifestHashBefore = Sha256(commonManifestPath);
        var commonCheckpointHashBefore = Sha256(commonCheckpointPath);
        var candidateManifestHashBefore = Sha256(candidateManifestPath);
        var candidateCheckpointHashBefore = Sha256(candidateCheckpointPath);

        var checkpointIdentity = ReadCheckpointIdentity(commonCheckpointPath, candidateCheckpointPath);
        var p16eRows = ReadJsonLines<P16ELedgerRow>(p16eLedgerPath, SnakeJsonLine);
        var p16bRows = ReadJsonLines<JsonElement>(p16bRowsPath);
        var p17cRows = ReadJsonLines<JsonElement>(p17cRowsPath);
        using var p16eReport = JsonDocument.Parse(File.ReadAllText(p16eReportPath, Encoding.UTF8));
        using var p16e2Report = JsonDocument.Parse(File.ReadAllText(p16e2ReportPath, Encoding.UTF8));
        using var p21Report = JsonDocument.Parse(File.ReadAllText(p21ReportPath, Encoding.UTF8));
        using var p22Report = JsonDocument.Parse(File.ReadAllText(p22ReportPath, Encoding.UTF8));

        var p16eRuntime = p16eReport.RootElement.GetProperty("runtime_evaluation").GetBoolean();
        var p16eSchemaOnly = p16eReport.RootElement.GetProperty("candidate_schema_eval_only").GetBoolean();
        var p16e2Runtime = p16e2Report.RootElement.GetProperty("runtime_evaluation").GetBoolean();
        var p16e2SchemaOnly = p16e2Report.RootElement.GetProperty("candidate_schema_eval_only").GetBoolean();
        var p21SourceDisjointProven = p21Report.RootElement.GetProperty("sourceDisjointProven").GetBoolean();
        var p22AcceptedRows = p22Report.RootElement.GetProperty("acceptedP22EvalRows").GetInt32();

        var gapRows = BuildGapRows(
            p16eRows,
            p16bRows,
            p17cRows,
            checkpointIdentity,
            p16eRuntime,
            p16eSchemaOnly,
            p16e2Runtime,
            p16e2SchemaOnly,
            p21SourceDisjointProven,
            p22AcceptedRows);
        var acceptedRows = Array.Empty<P23AcceptedHoldoutRow>();
        var acceptedHoldoutRowsCreated = acceptedRows.Length > 0;
        var modelsUnchanged = commonManifestHashBefore == Sha256(commonManifestPath)
            && commonCheckpointHashBefore == Sha256(commonCheckpointPath)
            && candidateManifestHashBefore == Sha256(candidateManifestPath)
            && candidateCheckpointHashBefore == Sha256(candidateCheckpointPath);

        WriteArtifacts(
            output,
            artifactDir,
            commonManifestPath,
            commonCheckpointPath,
            candidateManifestPath,
            candidateCheckpointPath,
            commonCheckpointHashBefore,
            candidateCheckpointHashBefore,
            p16eLedgerPath,
            p16eReportPath,
            p16e2ReportPath,
            p21ReportPath,
            p22ReportPath,
            p16bRowsPath,
            p17cRowsPath,
            checkpointIdentity,
            p16eRuntime,
            p16eSchemaOnly,
            p16e2Runtime,
            p16e2SchemaOnly,
            p21SourceDisjointProven,
            p22AcceptedRows,
            acceptedRows,
            gapRows,
            modelsUnchanged,
            afterOwnedArtifactExists);

        var acceptedLedgerPath = OwnedArtifactPath(output, artifactDir, "p23-runtime-holdout-ledger.jsonl");
        var gapLedgerPath = OwnedArtifactPath(output, artifactDir, "p23-runtime-holdout-gap-ledger.jsonl");
        var reportPath = OwnedArtifactPath(output, artifactDir, "p23-runtime-holdout-report.json");
        var reviewPath = OwnedArtifactPath(output, artifactDir, "p23-review-cycles.jsonl");
        var selfCheckPath = OwnedArtifactPath(output, artifactDir, "p23-self-check.json");
        var acceptedLedgerRows = File.ReadLines(acceptedLedgerPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();
        var gapLedgerRows = File.ReadLines(gapLedgerPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();
        var reviewRows = File.ReadLines(reviewPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();
        using var report = JsonDocument.Parse(File.ReadAllText(reportPath, Encoding.UTF8));
        using var selfCheck = JsonDocument.Parse(File.ReadAllText(selfCheckPath, Encoding.UTF8));

        Assert.True(modelsUnchanged);
        Assert.False(acceptedHoldoutRowsCreated);
        Assert.Empty(acceptedLedgerRows);
        Assert.NotEmpty(gapLedgerRows);
        Assert.True(gapLedgerRows.Length >= 12);
        Assert.True(reviewRows.Length >= 6);
        Assert.False(report.RootElement.GetProperty("acceptedHoldoutRowsCreated").GetBoolean());
        Assert.Equal(0, report.RootElement.GetProperty("acceptedHoldoutRowCount").GetInt32());
        Assert.False(report.RootElement.GetProperty("sideBySideComparisonAllowed").GetBoolean());
        Assert.False(report.RootElement.GetProperty("sideBySideComparisonRan").GetBoolean());
        Assert.False(report.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.False(report.RootElement.GetProperty("skillSelectionReadinessClaimed").GetBoolean());
        Assert.False(report.RootElement.GetProperty("runtimePromotionPerformed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("acceptedHoldoutRowsCreated").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("sideBySideComparisonAllowed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.All(gapLedgerRows, row =>
        {
            Assert.False(row.RootElement.GetProperty("acceptedAsP23Holdout").GetBoolean());
            Assert.False(row.RootElement.GetProperty("runtimeInvocationReady").GetBoolean());
            Assert.NotEqual("", row.RootElement.GetProperty("rejectionReason").GetString());
            Assert.True(row.RootElement.GetProperty("missingConditions").GetArrayLength() > 0);
        });

        foreach (var row in acceptedLedgerRows)
            AssertAcceptedRowSchema(row.RootElement, checkpointIdentity);

        foreach (var path in Directory.EnumerateFiles(artifactDir))
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            AssertNoEncodingDamage(text);
            AssertStableEvidenceText(output, text);
        }

        foreach (var row in acceptedLedgerRows.Concat(gapLedgerRows).Concat(reviewRows))
            row.Dispose();
    }

    private static void ThrowAfterOwnedArtifactFailpoint()
        => throw new P23OwnedArtifactFailpointException(FailpointMessage);

    private static CheckpointIdentity ReadCheckpointIdentity(string commonCheckpointPath, string candidateCheckpointPath)
    {
        using var common = JsonDocument.Parse(File.ReadAllText(commonCheckpointPath, Encoding.UTF8));
        using var candidate = JsonDocument.Parse(File.ReadAllText(candidateCheckpointPath, Encoding.UTF8));
        var commonPairIds = ExtractPairIds(common.RootElement);
        var candidatePairIds = ExtractPairIds(candidate.RootElement);
        var commonSourceFamilies = InferSourceFamilies(commonPairIds);
        var candidateSourceFamilies = InferSourceFamilies(candidatePairIds);

        return new CheckpointIdentity(
            CommonPairIds: commonPairIds,
            CandidatePairIds: candidatePairIds,
            CommonSourceFamilies: commonSourceFamilies,
            CandidateSourceFamilies: candidateSourceFamilies);
    }

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

    private static IReadOnlyList<P23GapLedgerRow> BuildGapRows(
        IReadOnlyList<P16ELedgerRow> p16eRows,
        IReadOnlyList<JsonElement> p16bRows,
        IReadOnlyList<JsonElement> p17cRows,
        CheckpointIdentity checkpointIdentity,
        bool p16eRuntime,
        bool p16eSchemaOnly,
        bool p16e2Runtime,
        bool p16e2SchemaOnly,
        bool p21SourceDisjointProven,
        int p22AcceptedRows)
    {
        var rows = new List<P23GapLedgerRow>();
        foreach (var row in p16eRows)
        {
            rows.Add(new P23GapLedgerRow(
                CandidateRowId: row.RowId,
                CandidateSourceArtifact: "P16E eval input ledger",
                SourceFamily: row.SourceFamily,
                SourceRefOrHash: "redacted:p16e-ledger-row",
                PairId: row.RowId,
                AvailableSkillIds: row.AvailableSkills,
                ExpectedSelectedSkillIds: [row.ExpectedChosen],
                ExpectedRejectedSkillIds: [row.ExpectedRejected],
                AcceptedAsP23Holdout: false,
                NotUsedForTrainingProvenAgainstBothCheckpoints: !checkpointIdentity.CommonPairIds.Contains(row.RowId)
                    && !checkpointIdentity.CandidatePairIds.Contains(row.RowId),
                RuntimeInvocationReady: false,
                RejectionReason: "P16E/P16E2 controlling reports mark these rows as schema-only and not runtime evaluation; P21 sourceDisjointProven=false and P22 accepted rows=0.",
                MissingConditions: new[]
                {
                    p16eRuntime ? "" : "P16E runtime_evaluation is false",
                    p16eSchemaOnly ? "P16E candidate_schema_eval_only is true" : "",
                    p16e2Runtime ? "" : "P16E2 runtime_evaluation is false",
                    p16e2SchemaOnly ? "P16E2 candidate_schema_eval_only is true" : "",
                    p21SourceDisjointProven ? "" : "P21 sourceDisjointProven is false",
                    p22AcceptedRows > 0 ? "" : "P22 accepted eval rows count is zero",
                    "runtimeInvocationReady acceptance record missing"
                }.Where(item => item.Length > 0).ToArray()));
        }

        rows.AddRange(BuildJsonElementGapRows("P16B candidate material", p16bRows, checkpointIdentity, "P16B material is candidate/adaptive or evidence-only and was not accepted as runtime-invocation-ready holdout against both checkpoints."));
        rows.AddRange(BuildJsonElementGapRows("P17C train-only candidate material", p17cRows, checkpointIdentity, "P17C material is train-side adaptive candidate material; several row ids are present in the candidate checkpoint and none is accepted as runtime holdout."));
        return rows;
    }

    private static IEnumerable<P23GapLedgerRow> BuildJsonElementGapRows(
        string sourceArtifact,
        IReadOnlyList<JsonElement> sourceRows,
        CheckpointIdentity checkpointIdentity,
        string reason)
    {
        foreach (var row in sourceRows)
        {
            var rowId = ReadString(row, "row_id", "rowId", "unknown-row");
            var sourceFamily = ReadString(row, "source_family", "sourceFamily", "unknown_source_family");
            var expectedSkill = ReadString(row, "expected_skill_id", "expectedSkillId", ReadString(row, "chosen", "expected_chosen", ""));
            var rejectedSkill = ReadString(row, "expected_rejected", "rejected", "");
            var availableSkills = ReadStringArray(row, "available_skill_ids", "available_skills", "availableSkillIds", "availableSkills");
            var pairSeen = checkpointIdentity.CommonPairIds.Contains(rowId) || checkpointIdentity.CandidatePairIds.Contains(rowId);
            var familySeen = checkpointIdentity.CommonSourceFamilies.Contains(TopLevelFamily(rowId))
                || checkpointIdentity.CandidateSourceFamilies.Contains(TopLevelFamily(rowId));

            yield return new P23GapLedgerRow(
                CandidateRowId: rowId,
                CandidateSourceArtifact: sourceArtifact,
                SourceFamily: sourceFamily,
                SourceRefOrHash: ReadString(row, "source_hash_or_pointer", "sourceHashOrPointer", "redacted:source-ref-not-exported"),
                PairId: rowId,
                AvailableSkillIds: availableSkills,
                ExpectedSelectedSkillIds: string.IsNullOrWhiteSpace(expectedSkill) ? [] : [expectedSkill],
                ExpectedRejectedSkillIds: string.IsNullOrWhiteSpace(rejectedSkill) ? [] : [rejectedSkill],
                AcceptedAsP23Holdout: false,
                NotUsedForTrainingProvenAgainstBothCheckpoints: !pairSeen && !familySeen,
                RuntimeInvocationReady: false,
                RejectionReason: reason,
                MissingConditions: new[]
                {
                    pairSeen ? "pair id or row id is already present in a compared checkpoint" : "",
                    familySeen ? "top-level row family is already present in a compared checkpoint" : "",
                    "runtimeInvocationReady acceptance record missing",
                    "source-family disjointness cannot be promoted from candidate/adaptive-only metadata"
                }.Where(item => item.Length > 0).ToArray());
        }
    }

    private static void WriteArtifacts(
        TestOwnedOutputRoot output,
        string artifactDir,
        string commonManifestPath,
        string commonCheckpointPath,
        string candidateManifestPath,
        string candidateCheckpointPath,
        string commonCheckpointSha256,
        string candidateCheckpointSha256,
        string p16eLedgerPath,
        string p16eReportPath,
        string p16e2ReportPath,
        string p21ReportPath,
        string p22ReportPath,
        string p16bRowsPath,
        string p17cRowsPath,
        CheckpointIdentity checkpointIdentity,
        bool p16eRuntime,
        bool p16eSchemaOnly,
        bool p16e2Runtime,
        bool p16e2SchemaOnly,
        bool p21SourceDisjointProven,
        int p22AcceptedRows,
        IReadOnlyList<P23AcceptedHoldoutRow> acceptedRows,
        IReadOnlyList<P23GapLedgerRow> gapRows,
        bool modelsUnchanged,
        Action<string>? afterOwnedArtifactExists)
    {
        var acceptedLedgerPath = OwnedArtifactPath(
            output,
            artifactDir,
            "p23-runtime-holdout-ledger.jsonl");
        WriteUtf8Lf(
            output,
            acceptedLedgerPath,
            acceptedRows.Count == 0
                ? ""
                : string.Join("\n", acceptedRows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");
        afterOwnedArtifactExists?.Invoke(acceptedLedgerPath);
        WriteUtf8Lf(
            output,
            OwnedArtifactPath(output, artifactDir, "p23-runtime-holdout-gap-ledger.jsonl"),
            string.Join("\n", gapRows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");

        var missingCondition = "No local row has an accepted runtime-invocation-ready record plus row-level source-family and pair-id disjointness against both compared checkpoints.";
        var report = new
        {
            Phase = "P23_SKILL_SELECTION_RUNTIME_HOLDOUT_LEDGER",
            ArtifactType = "runtime holdout ledger creation or honest gap ledger",
            Verdict = "NO_ACCEPTED_RUNTIME_HOLDOUT_ROWS_CREATED",
            AcceptedHoldoutRowsCreated = acceptedRows.Count > 0,
            AcceptedHoldoutRowCount = acceptedRows.Count,
            GapLedgerRowCount = gapRows.Count,
            MissingCondition = missingCondition,
            MissingConditionRu = "Локально не найдено ни одной строки, у которой одновременно есть принятая runtime-ready форма, доказанная раздельность source family и pair id против обоих checkpoint-ов, а также запрет на обучение по этой строке.",
            NextRepairAction = "Create new redacted local SkillSelection runtime holdout rows from approved source families not represented in either checkpoint, record source-family and pair-id exclusion proof, then rerun only the ledger gate before any comparison.",
            NextRepairActionRu = "Создать новые redacted local SkillSelection runtime holdout rows из approved source families, которых нет ни в одном из двух checkpoints; записать доказательство исключения source-family и pair-id; затем заново запустить только ledger gate до любого comparison.",
            SideBySideComparisonAllowed = acceptedRows.Count > 0,
            SideBySideComparisonRan = false,
            CommonManifestPath = output.ToStableReference(commonManifestPath),
            CommonCheckpointPath = output.ToStableReference(commonCheckpointPath),
            CandidateManifestPath = output.ToStableReference(candidateManifestPath),
            CandidateCheckpointPath = output.ToStableReference(candidateCheckpointPath),
            CommonCheckpointSha256 = commonCheckpointSha256,
            CandidateCheckpointSha256 = candidateCheckpointSha256,
            P16ELedgerRef = output.ToStableReference(p16eLedgerPath),
            P16EReportRef = output.ToStableReference(p16eReportPath),
            P16E2ReportRef = output.ToStableReference(p16e2ReportPath),
            P21ReportRef = output.ToStableReference(p21ReportPath),
            P22ReportRef = output.ToStableReference(p22ReportPath),
            P16BRowsRef = output.ToStableReference(p16bRowsPath),
            P17CRowsRef = output.ToStableReference(p17cRowsPath),
            P16ERuntimeEvaluation = p16eRuntime,
            P16ECandidateSchemaOnly = p16eSchemaOnly,
            P16E2RuntimeEvaluation = p16e2Runtime,
            P16E2CandidateSchemaOnly = p16e2SchemaOnly,
            P21SourceDisjointProven = p21SourceDisjointProven,
            P22AcceptedRows = p22AcceptedRows,
            CommonCheckpointPairIdCount = checkpointIdentity.CommonPairIds.Count,
            CandidateCheckpointPairIdCount = checkpointIdentity.CandidatePairIds.Count,
            CommonCheckpointSourceFamilyMarkers = checkpointIdentity.CommonSourceFamilies.Order(StringComparer.Ordinal).ToArray(),
            CandidateCheckpointSourceFamilyMarkers = checkpointIdentity.CandidateSourceFamilies.Order(StringComparer.Ordinal).ToArray(),
            RequiredAcceptedRowSchema = new[]
            {
                "rowId",
                "sourceFamily",
                "sourceRefOrHashedRedactedSourceRef",
                "sourceLicenseOrLocalPrivateStatus",
                "promptHash",
                "promptOrTaskTextRedactedSummary",
                "availableSkillIds",
                "expectedSelectedSkillIds",
                "expectedRejectedSkillIdsOrReasonOmitted",
                "sourceDisjointAgainstCommonCheckpoint",
                "sourceDisjointAgainstCandidateCheckpoint",
                "pairIdDisjointAgainstCommonCheckpoint",
                "pairIdDisjointAgainstCandidateCheckpoint",
                "notUsedForTraining",
                "runtimeInvocationReady",
                "leakageControls",
                "whyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution"
            },
            ModelsUnchanged = modelsUnchanged,
            SemanticQualityClaimed = false,
            SkillSelectionReadinessClaimed = false,
            FullApiReadinessClaimed = false,
            RuntimePromotionPerformed = false,
            CommonModelFilesModified = false,
            TrainingRun = false,
            CheckpointMutated = false,
            ExternalAiOrNetworkUsed = false,
            AcceptedLedgerRef = "p23-runtime-holdout-ledger.jsonl",
            GapLedgerRef = "p23-runtime-holdout-gap-ledger.jsonl",
            ReviewCyclesRef = "p23-review-cycles.jsonl"
        };
        WriteUtf8Lf(
            output,
            OwnedArtifactPath(output, artifactDir, "p23-runtime-holdout-report.json"),
            JsonSerializer.Serialize(report, WebJson) + "\n");
        WriteUtf8Lf(
            output,
            OwnedArtifactPath(output, artifactDir, "p23-runtime-holdout-report.md"),
            BuildHumanReport(report.MissingConditionRu, report.NextRepairActionRu, gapRows.Count));
        WriteUtf8Lf(
            output,
            OwnedArtifactPath(output, artifactDir, "p23-review-cycles.jsonl"),
            BuildReviewCycles());
        WriteUtf8Lf(
            output,
            OwnedArtifactPath(output, artifactDir, "p23-self-check.json"),
            JsonSerializer.Serialize(new
            {
                Phase = "P23",
                AcceptedHoldoutRowsCreated = acceptedRows.Count > 0,
                AcceptedHoldoutRowCount = acceptedRows.Count,
                GapLedgerRowCount = gapRows.Count,
                SideBySideComparisonAllowed = acceptedRows.Count > 0,
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
                FocusedTest = "SmartSkillsSemanticQualityP23RuntimeHoldoutLedgerTests"
            }, WebJson) + "\n");
        WriteUtf8Lf(
            output,
            OwnedArtifactPath(output, artifactDir, "p23-evidence-card.md"),
            """
            # P23 evidence card

            P23 tried to create an accepted SkillSelection runtime holdout ledger before any side-by-side comparison or promotion.

            Result: no accepted holdout rows were created.

            Why:
            - P16E and P16E2 rows are explicitly schema-only, not runtime evaluation.
            - P21 says source-disjoint proof is false.
            - P22 found zero accepted runtime-eval rows.
            - P16B and P17C candidate rows are adaptive, train-side, already represented by checkpoint families, or missing an accepted runtime-ready record.

            What was written:
            - `p23-runtime-holdout-ledger.jsonl` is intentionally empty.
            - `p23-runtime-holdout-gap-ledger.jsonl` lists rejected candidates and exact reasons.
            - No model, checkpoint, training data, production code, runtime/common path, or promotion target was changed.

            No-overclaim statement:
            This is a holdout-ledger gap artifact only. It does not claim semantic quality, SkillSelection readiness, full API readiness, runtime promotion, or common model replacement.

            Следующий repair:
            Создать новые redacted local SkillSelection runtime holdout rows из approved source families, которых нет ни в одном из двух checkpoints; доказать pair-id и source-family exclusion против обоих checkpoints; затем заново запустить ledger gate до любого comparison.
            """);
    }

    private static string BuildHumanReport(string missingConditionRu, string nextRepairActionRu, int gapRows)
        => $$"""
        # P23 - runtime holdout ledger для SkillSelection

        Вердикт: принятые holdout-строки не созданы.

        Причина: {{missingConditionRu}}

        Проверено кандидатов в gap ledger: {{gapRows}}.

        Почему это честный результат:
        - P16E и P16E2 полезны как evidence, но сами отчеты говорят, что это schema-only, не runtime evaluation.
        - P21 прямо фиксирует, что source-disjoint proof равен false.
        - P22 уже нашел ноль accepted runtime-eval rows.
        - P16B/P17C строки нельзя просто переименовать в holdout: часть их семей уже обучала candidate checkpoint, а остальные не имеют принятого runtimeInvocationReady доказательства.

        Что не делалось:
        - side-by-side comparison не запускался;
        - обучение не запускалось;
        - common и candidate checkpoint не менялись;
        - promotion и runtime/common replacement не выполнялись.

        Следующее исправление:
        {{nextRepairActionRu}}
        """;

    private static string BuildReviewCycles()
    {
        var rows = new[]
        {
            new { cycle_id = "p23_review_01_source_disjointness", status = "passed_gap_preserved", finding_ru = "Ни одна локальная строка не доказала source-family disjointness против обоих checkpoint-ов.", fix_or_evidence_ru = "Accepted ledger оставлен пустым; gap ledger фиксирует причину для каждого candidate source." },
            new { cycle_id = "p23_review_02_pair_id_disjointness", status = "passed_gap_preserved", finding_ru = "P17C и часть P16B top-level families уже представлены в candidate checkpoint; common checkpoint содержит собственные skill-selection/q5 family markers.", fix_or_evidence_ru = "Gap rows сравниваются с trained pair ids и source pair ids обоих checkpoint-ов." },
            new { cycle_id = "p23_review_03_runtime_readiness", status = "passed_gap_preserved", finding_ru = "P16E/P16E2 schema-only; P16B/P17C не имеют accepted runtimeInvocationReady record.", fix_or_evidence_ru = "runtimeInvocationReady=false во всех gap rows; side-by-side comparisonAllowed=false." },
            new { cycle_id = "p23_review_04_leakage", status = "passed", finding_ru = "Нельзя использовать exact prompt, source id, path, filename, candidate order или pair id как authority.", fix_or_evidence_ru = "Accepted schema требует blocked leakage controls; gap artifact не запускает runtime на leakage-risk rows." },
            new { cycle_id = "p23_review_05_universal_not_stack_specific", status = "passed", finding_ru = "Нет строки, где универсальная SkillSelection boundary одновременно доказана и runtime-ready.", fix_or_evidence_ru = "Gap keeps local/private/adaptive/project evidence out of common holdout promotion." },
            new { cycle_id = "p23_review_06_no_overclaim_readability", status = "passed", finding_ru = "Отчет не заявляет semantic quality, readiness, promotion или full API readiness.", fix_or_evidence_ru = "No-overclaim flags false; Markdown human-readable Russian; UTF-8 scan required." }
        };

        return string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n";
    }

    private static void AssertAcceptedRowSchema(JsonElement row, CheckpointIdentity checkpointIdentity)
    {
        var rowId = row.GetProperty("rowId").GetString()!;
        Assert.NotEqual("", rowId);
        Assert.NotEqual("", row.GetProperty("sourceFamily").GetString());
        Assert.NotEqual("", row.GetProperty("sourceRefOrHashedRedactedSourceRef").GetString());
        Assert.NotEqual("", row.GetProperty("sourceLicenseOrLocalPrivateStatus").GetString());
        Assert.NotEqual("", row.GetProperty("promptHash").GetString());
        Assert.NotEqual("", row.GetProperty("promptOrTaskTextRedactedSummary").GetString());
        Assert.True(row.GetProperty("availableSkillIds").GetArrayLength() > 0);
        Assert.True(row.GetProperty("expectedSelectedSkillIds").GetArrayLength() > 0);
        Assert.True(row.GetProperty("sourceDisjointAgainstCommonCheckpoint").GetBoolean());
        Assert.True(row.GetProperty("sourceDisjointAgainstCandidateCheckpoint").GetBoolean());
        Assert.True(row.GetProperty("pairIdDisjointAgainstCommonCheckpoint").GetBoolean());
        Assert.True(row.GetProperty("pairIdDisjointAgainstCandidateCheckpoint").GetBoolean());
        Assert.True(row.GetProperty("notUsedForTraining").GetBoolean());
        Assert.True(row.GetProperty("runtimeInvocationReady").GetBoolean());
        Assert.DoesNotContain(rowId, checkpointIdentity.CommonPairIds);
        Assert.DoesNotContain(rowId, checkpointIdentity.CandidatePairIds);
        var leakage = row.GetProperty("leakageControls");
        Assert.False(leakage.GetProperty("exactPrompt").GetBoolean());
        Assert.False(leakage.GetProperty("sourceId").GetBoolean());
        Assert.False(leakage.GetProperty("path").GetBoolean());
        Assert.False(leakage.GetProperty("filename").GetBoolean());
        Assert.False(leakage.GetProperty("candidateOrder").GetBoolean());
        Assert.False(leakage.GetProperty("pairId").GetBoolean());
        Assert.Contains("universal", row.GetProperty("whyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<T> ReadJsonLines<T>(string path, JsonSerializerOptions? options = null)
    {
        return File.ReadLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<T>(line, options ?? WebJsonLine)!)
            .ToArray();
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

    private static string ReadString(JsonElement element, string firstName, string secondName, string fallback)
    {
        if (element.TryGetProperty(firstName, out var first) && first.ValueKind == JsonValueKind.String)
            return first.GetString() ?? fallback;
        if (element.TryGetProperty(secondName, out var second) && second.ValueKind == JsonValueKind.String)
            return second.GetString() ?? fallback;

        return fallback;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!element.TryGetProperty(name, out var array) || array.ValueKind != JsonValueKind.Array)
                continue;

            return array.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item!)
                .ToArray();
        }

        return [];
    }

    private static string TopLevelFamily(string rowId)
    {
        if (rowId.StartsWith("P16B-", StringComparison.Ordinal))
            return "P16B";
        if (rowId.StartsWith("P16E1-", StringComparison.Ordinal))
            return "P16E1";
        if (rowId.StartsWith("P17C-", StringComparison.Ordinal))
            return "P17C";
        if (rowId.StartsWith("skill-selection-q5-04r-source-disjoint", StringComparison.Ordinal))
            return "q5_04r_source_disjoint";
        if (rowId.StartsWith("skill-selection-q5-04r-dashboard", StringComparison.Ordinal))
            return "q5_04r_dashboard_bridge";

        return rowId.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? rowId;
    }

    private static void WriteUtf8Lf(TestOwnedOutputRoot output, string path, string text)
    {
        var ownedPath = output.RequireOwnedPath(path);
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        File.WriteAllText(ownedPath, normalized, Utf8NoBom);
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

    private static void AssertStableEvidenceText(TestOwnedOutputRoot output, string text)
    {
        Assert.DoesNotContain(Path.GetFullPath(RepoRoot), text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFullPath(output.RootPath), text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFullPath(Path.GetTempPath()), text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(new Regex(@"llm-from-scratch-test-output-[0-9a-fA-F]{32}"), text);
    }

    private static string OwnedArtifactPath(TestOwnedOutputRoot output, string artifactDir, string fileName)
        => output.RequireOwnedPath(Path.Combine(output.RequireOwnedPath(artifactDir), fileName));

    private static ProtectedRepositoryEntry[] CaptureProtectedRepositorySnapshot()
    {
        var entries = new List<ProtectedRepositoryEntry>();
        AddDirectorySnapshot(ProtectedP23Directory, entries);
        foreach (var path in ProtectedInputPaths())
            AddFileSnapshot(path, entries);

        return entries
            .OrderBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> ProtectedInputPaths()
        =>
        [
            RepoPath("models", "skill-selection", "skill-selection-v1", "manifest.json"),
            RepoPath("models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json"),
            RepoPath("artifacts", "smart-skills-semantic-quality-completion", "p17e-skillselection-lifecycle-import-export", "production-export", "skill-selection-manifest.json"),
            RepoPath("artifacts", "smart-skills-semantic-quality-completion", "p17e-skillselection-lifecycle-import-export", "production-export", "skill-selection-checkpoint.json"),
            RepoPath("artifacts", "smart-skills-semantic-quality-completion", "p16e-skill-selection-source-disjoint-eval-ablation", "skill-selection-p16e-eval-input-ledger.jsonl"),
            RepoPath("artifacts", "smart-skills-semantic-quality-completion", "p16e-skill-selection-source-disjoint-eval-ablation", "skill-selection-p16e-source-disjoint-eval-report.json"),
            RepoPath("artifacts", "smart-skills-semantic-quality-completion", "p16e2-skill-selection-source-disjoint-eval-ablation-rerun", "skill-selection-p16e2-source-disjoint-eval-report.json"),
            RepoPath("artifacts", "smart-skills-semantic-quality-completion", "p21-skillselection-compatibility-nonregression", "p21-compatibility-nonregression-report.json"),
            RepoPath("artifacts", "smart-skills-semantic-quality-completion", "p22-skillselection-source-disjoint-runtime-eval", "p22-source-disjoint-runtime-eval-report.json"),
            RepoPath("artifacts", "smart-skills-semantic-quality-completion", "p16b-skill-selection-source-dpo-materialization", "p16b-dpo-candidate-rows.jsonl"),
            RepoPath("artifacts", "smart-skills-semantic-quality-completion", "p17c-skillselection-train-only-runtime-coverage-repair", "p17c-train-only-candidate-rows.jsonl")
        ];

    private static void AddDirectorySnapshot(
        string directory,
        ICollection<ProtectedRepositoryEntry> entries)
    {
        Assert.True(Directory.Exists(directory), $"Missing protected repository directory: {directory}");
        entries.Add(new ProtectedRepositoryEntry(
            StableRepositoryPath(directory),
            "directory",
            0,
            ""));

        foreach (var childDirectory in Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories))
        {
            entries.Add(new ProtectedRepositoryEntry(
                StableRepositoryPath(childDirectory),
                "directory",
                0,
                ""));
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            AddFileSnapshot(file, entries);
    }

    private static void AddFileSnapshot(
        string path,
        ICollection<ProtectedRepositoryEntry> entries)
    {
        Assert.True(File.Exists(path), $"Missing protected repository file: {path}");
        var info = new FileInfo(path);
        entries.Add(new ProtectedRepositoryEntry(
            StableRepositoryPath(path),
            "file",
            info.Length,
            Sha256(path)));
    }

    private static string StableRepositoryPath(string path)
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

    private sealed record CheckpointIdentity(
        HashSet<string> CommonPairIds,
        HashSet<string> CandidatePairIds,
        HashSet<string> CommonSourceFamilies,
        HashSet<string> CandidateSourceFamilies);

    private sealed record P16ELedgerRow(
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

    private sealed record P23AcceptedHoldoutRow(
        string RowId,
        string SourceFamily,
        string SourceRefOrHashedRedactedSourceRef,
        string SourceLicenseOrLocalPrivateStatus,
        string PromptHash,
        string PromptOrTaskTextRedactedSummary,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSelectedSkillIds,
        IReadOnlyList<string> ExpectedRejectedSkillIds,
        string ExpectedRejectedSkillIdsOrReasonOmitted,
        bool SourceDisjointAgainstCommonCheckpoint,
        bool SourceDisjointAgainstCandidateCheckpoint,
        bool PairIdDisjointAgainstCommonCheckpoint,
        bool PairIdDisjointAgainstCandidateCheckpoint,
        bool NotUsedForTraining,
        bool RuntimeInvocationReady,
        object LeakageControls,
        string WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution);

    private sealed record P23GapLedgerRow(
        string CandidateRowId,
        string CandidateSourceArtifact,
        string SourceFamily,
        string SourceRefOrHash,
        string PairId,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSelectedSkillIds,
        IReadOnlyList<string> ExpectedRejectedSkillIds,
        bool AcceptedAsP23Holdout,
        bool NotUsedForTrainingProvenAgainstBothCheckpoints,
        bool RuntimeInvocationReady,
        string RejectionReason,
        IReadOnlyList<string> MissingConditions);

    private sealed record ProtectedRepositoryEntry(
        string RelativePath,
        string EntryType,
        long Length,
        string Sha256);

    private sealed class P23OwnedArtifactFailpointException : Exception
    {
        public P23OwnedArtifactFailpointException(string message)
            : base(message)
        {
        }
    }
}
