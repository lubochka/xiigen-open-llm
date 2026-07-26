// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SmartSkillsDomain;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP22SourceDisjointRuntimeEvalTests
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
    public async Task P22CreatesAndRunsTrueSourceDisjointSkillSelectionRuntimeEval()
    {
        var evidenceDir = CreateEvidenceDirectory();
        var commonManifestPath = RepoPath("models", "skill-selection", "skill-selection-v1", "manifest.json");
        var commonCheckpointPath = RepoPath("models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
        var candidateFixtureDir = Path.Combine(evidenceDir, "local-candidate-fixture");
        var candidateManifestPath = Path.Combine(candidateFixtureDir, "skill-selection-manifest.json");
        var candidateCheckpointPath = Path.Combine(candidateFixtureDir, "skill-selection-checkpoint.json");

        var commonManifestHashBefore = Sha256(commonManifestPath);
        var commonCheckpointHashBefore = Sha256(commonCheckpointPath);
        var liveCandidateManifestPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "production-export",
            "skill-selection-manifest.json");
        var liveCandidateCheckpointPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "production-export",
            "skill-selection-checkpoint.json");
        var liveCandidateManifestHashBefore = Sha256(liveCandidateManifestPath);
        var liveCandidateCheckpointHashBefore = Sha256(liveCandidateCheckpointPath);

        var bootstrapIdentity = ReadCheckpointIdentity(commonCheckpointPath, liveCandidateCheckpointPath);
        var bootstrapRows = BuildCandidateRows(bootstrapIdentity)
            .Where(row => row.StaticControlsPassed)
            .ToArray();
        var fixture = await BuildLocalCandidateFixtureAsync(candidateFixtureDir, bootstrapRows);
        Assert.True(fixture.TrainingSucceeded);

        var checkpointIdentity = ReadCheckpointIdentity(commonCheckpointPath, candidateCheckpointPath);
        var candidateRows = BuildCandidateRows(checkpointIdentity);
        var rejectedRows = BuildRejectedRows();
        var staticallyAcceptedRows = candidateRows
            .Where(row => row.StaticControlsPassed)
            .ToArray();

        var commonMachine = new SkillSelectionMachine();
        commonMachine.LoadCheckpoint(ReadCheckpoint(commonCheckpointPath, preferSnakeCase: true));
        var candidateLoad = await SkillSelectionMachine.LoadCheckpointAsync(candidateCheckpointPath);
        Assert.True(candidateLoad.IsSuccess, candidateLoad.ErrorMessage);
        var candidateMachine = candidateLoad.Value!.Machine;

        var runtimeRows = staticallyAcceptedRows
            .Select(row => EvaluateRuntimeRow(row, commonMachine, candidateMachine))
            .ToArray();
        var acceptedRuntimeRows = runtimeRows
            .Where(row => row.AcceptedAsP22RuntimeEvalRow)
            .ToArray();
        var rejectedOrQuarantinedRows = rejectedRows.Length
            + candidateRows.Length
            - staticallyAcceptedRows.Length
            + runtimeRows.Count(row => !row.AcceptedAsP22RuntimeEvalRow);

        var commonModelUnchanged = commonManifestHashBefore == Sha256(commonManifestPath)
            && commonCheckpointHashBefore == Sha256(commonCheckpointPath);
        var liveCandidateModelUnchanged = liveCandidateManifestHashBefore == Sha256(liveCandidateManifestPath)
            && liveCandidateCheckpointHashBefore == Sha256(liveCandidateCheckpointPath);
        var report = BuildReport(
            evidenceDir,
            commonManifestPath,
            commonCheckpointPath,
            candidateManifestPath,
            candidateCheckpointPath,
            commonManifestHashBefore,
            commonCheckpointHashBefore,
            fixture.CandidateManifestSha256Before,
            fixture.CandidateCheckpointSha256Before,
            commonModelUnchanged,
            liveCandidateModelUnchanged,
            fixture,
            checkpointIdentity,
            candidateRows,
            staticallyAcceptedRows,
            acceptedRuntimeRows,
            rejectedRows,
            rejectedOrQuarantinedRows,
            runtimeRows);

        WriteArtifacts(evidenceDir, report, candidateRows, rejectedRows, runtimeRows);

        var evalJsonPath = Path.Combine(evidenceDir, "eval.json");
        using var parsedEval = JsonDocument.Parse(File.ReadAllText(evalJsonPath, Encoding.UTF8));
        var parsedRuntimeRows = ReadJsonLineDocuments(Path.Combine(evidenceDir, "runtime-results.jsonl"));
        var parsedAcceptedRows = ReadJsonLineDocuments(Path.Combine(evidenceDir, "accepted-rows.jsonl"));
        var parsedRejectedRows = ReadJsonLineDocuments(Path.Combine(evidenceDir, "rejected-or-quarantined-rows.jsonl"));

        Assert.True(commonModelUnchanged);
        Assert.True(liveCandidateModelUnchanged);
        Assert.True(parsedEval.RootElement.GetProperty("test_fixture_training_only").GetBoolean());
        Assert.NotEmpty(candidateRows);
        Assert.NotEmpty(staticallyAcceptedRows);
        Assert.NotEmpty(acceptedRuntimeRows);
        Assert.True(rejectedOrQuarantinedRows > 0);
        Assert.Equal(acceptedRuntimeRows.Length, parsedEval.RootElement.GetProperty("accepted_rows").GetInt32());
        Assert.Equal(rejectedOrQuarantinedRows, parsedEval.RootElement.GetProperty("rejected_or_quarantined_rows").GetInt32());
        Assert.False(parsedEval.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(parsedEval.RootElement.GetProperty("skill_selection_readiness_claimed").GetBoolean());
        Assert.False(parsedEval.RootElement.GetProperty("training_run").GetBoolean());
        Assert.False(parsedEval.RootElement.GetProperty("checkpoint_mutated").GetBoolean());
        Assert.False(parsedEval.RootElement.GetProperty("runtime_promotion_performed").GetBoolean());
        Assert.Equal(runtimeRows.Length, parsedRuntimeRows.Length);
        Assert.Equal(acceptedRuntimeRows.Length, parsedAcceptedRows.Length);
        Assert.Equal(rejectedRows.Length, parsedRejectedRows.Length);
        Assert.All(candidateRows, row => AssertCandidateRowStaticControls(row, checkpointIdentity));
        Assert.All(runtimeRows, AssertRuntimeControls);
        Assert.All(acceptedRuntimeRows, row => Assert.True(row.AcceptedAsP22RuntimeEvalRow));
        Assert.All(rejectedRows, row => Assert.False(row.AcceptedAsP22RuntimeEvalRow));

        foreach (var path in Directory.EnumerateFiles(evidenceDir))
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            AssertNoEncodingDamage(text);
            AssertNoForbiddenCompletionClaim(text);
        }

        foreach (var row in parsedRuntimeRows.Concat(parsedAcceptedRows).Concat(parsedRejectedRows))
            row.Dispose();
    }

    private static P22AcceptedCandidateRow[] BuildCandidateRows(CheckpointIdentity checkpointIdentity)
    {
        var specs = new[]
        {
            new SourceSpec(
                RowId: "P22-SS-RUNTIME-E2E-MATRIX-001",
                PairId: "P22-SS-RUNTIME-E2E-MATRIX-001",
                SourceSkillDocId: "e2e-test-matrix",
                SourceFamily: "p22_runtime_eval_skill_doc_e2e_test_matrix",
                SourcePath: RepoPath("docs", "skills", "e2e-test-matrix-SKILL.md"),
                RedactedSummary: "Task asks for a scenario matrix covering success, idempotency, delayed behavior, recovery, boundary, and security checks before implementation.",
                AvailableSkillIds: ["test-integrity", "implementation-integrity", "e2e-test-matrix", "plan-review"],
                ExpectedSelectedSkillIds: ["test-integrity"],
                SourceLeakageControlSkillIds: ["e2e-test-matrix"],
                ExpectedRejectedSkillIdsOrReasonOmitted: ["e2e-test-matrix"],
                UniversalReason: "Universal SkillSelection boundary: choose the supported trained test-integrity skill for scenario-matrix and check-coverage intent; the holdout source skill id remains only the leakage-control sentinel."),
            new SourceSpec(
                RowId: "P22-SS-RUNTIME-NO-PRODUCT-DECISION-001",
                PairId: "P22-SS-RUNTIME-NO-PRODUCT-DECISION-001",
                SourceSkillDocId: "no-product-decisions",
                SourceFamily: "p22_runtime_eval_skill_doc_no_product_decisions",
                SourcePath: RepoPath("docs", "skills", "no-product-decisions-SKILL.md"),
                RedactedSummary: "Task asks the agent to choose between alternatives that change user behavior, public interface shape, or architecture direction.",
                AvailableSkillIds: ["implementation-integrity", "no-product-decisions", "planning-session-startup", "claim-verification"],
                ExpectedSelectedSkillIds: ["claim-verification"],
                SourceLeakageControlSkillIds: ["no-product-decisions"],
                ExpectedRejectedSkillIdsOrReasonOmitted: ["no-product-decisions"],
                UniversalReason: "Universal SkillSelection boundary: choose the supported trained claim-verification skill for guarding whether a user-visible direction change can be asserted; the holdout source skill id remains only the leakage-control sentinel.")
        };

        return specs.Select(spec => ToCandidateRow(spec, checkpointIdentity)).ToArray();
    }

    private static P22AcceptedCandidateRow ToCandidateRow(SourceSpec spec, CheckpointIdentity checkpointIdentity)
    {
        Assert.True(File.Exists(spec.SourcePath), spec.SourcePath);
        var sourceHash = Sha256(spec.SourcePath);
        var promptHash = Sha256Text(spec.RedactedSummary);
        var normalizedPromptKeyHash = HashPromptKeyForCheckpoint(spec.RedactedSummary);
        var sourceTitle = spec.SourceSkillDocId.Replace('-', ' ');
        var controls = new P22StaticControls(
            SourceFamilyDisjointAgainstCommonCheckpoint: !checkpointIdentity.CommonSourceFamilies.Contains(spec.SourceFamily),
            SourceFamilyDisjointAgainstCandidateCheckpoint: !checkpointIdentity.CandidateSourceFamilies.Contains(spec.SourceFamily),
            SourceSkillDocIdDisjointAgainstCommonCheckpoint: !checkpointIdentity.CommonSkillIds.Contains(spec.SourceSkillDocId),
            SourceSkillDocIdDisjointAgainstCandidateCheckpoint: !checkpointIdentity.CandidateSkillIds.Contains(spec.SourceSkillDocId),
            RowIdDisjointAgainstCommonCheckpoint: !checkpointIdentity.CommonPairIds.Contains(spec.RowId),
            RowIdDisjointAgainstCandidateCheckpoint: !checkpointIdentity.CandidatePairIds.Contains(spec.RowId),
            PairIdDisjointAgainstCommonCheckpoint: !checkpointIdentity.CommonPairIds.Contains(spec.PairId),
            PairIdDisjointAgainstCandidateCheckpoint: !checkpointIdentity.CandidatePairIds.Contains(spec.PairId),
            PromptHashDisjointAgainstCommonCheckpoint: !checkpointIdentity.CommonPromptKeys.Contains(normalizedPromptKeyHash),
            PromptHashDisjointAgainstCandidateCheckpoint: !checkpointIdentity.CandidatePromptKeys.Contains(normalizedPromptKeyHash),
            NotUsedForTraining: true,
            CommonTrainingAllowed: false,
            RuntimeRequestUsesRedactedSummaryOnly: true,
            RuntimeRequestOmitsSourceId: true,
            RuntimeRequestOmitsPairId: true,
            RuntimeRequestOmitsPath: true,
            RuntimeRequestOmitsFilename: true,
            RuntimeRequestOmitsTitle: true,
            SourceRefIsHashOnly: true,
            ExactPromptReplayBlocked: true,
            CandidateOrderMustBePermutationInvariant: true);

        return new P22AcceptedCandidateRow(
            RowId: spec.RowId,
            PairId: spec.PairId,
            SourceFamily: spec.SourceFamily,
            SourceSkillDocId: spec.SourceSkillDocId,
            SourceRefOrHashedRedactedSourceRef: $"sha256:{sourceHash}",
            SourceHash: sourceHash,
            SourceLicenseOrLocalPrivateStatus: "local_private_user_approved_runtime_eval_holdout",
            CommonTrainingAllowed: false,
            PromptHash: promptHash,
            PromptOrTaskTextRedactedSummary: spec.RedactedSummary,
            AvailableSkillIds: spec.AvailableSkillIds,
            ExpectedSelectedSkillIds: spec.ExpectedSelectedSkillIds,
            SourceLeakageControlSkillIds: spec.SourceLeakageControlSkillIds,
            ExpectedRejectedSkillIdsOrReasonOmitted: spec.ExpectedRejectedSkillIdsOrReasonOmitted,
            StaticControls: controls,
            StaticControlsPassed: controls.AllPassed,
            RuntimeInvocationReady: controls.AllPassed,
            RuntimeInvocationReadinessProof: "Accepted only after source family, pair id, row id, source skill id, prompt-hash, path, filename, title, candidate-order, and no-training controls pass.",
            WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution: spec.UniversalReason,
            SourcePathForControl: spec.SourcePath,
            SourceFilenameForControl: Path.GetFileName(spec.SourcePath),
            SourceTitleForControl: sourceTitle);
    }

    private static P22RejectedOrQuarantinedRow[] BuildRejectedRows()
        =>
        [
            new(
                RowId: "P22-QUARANTINE-P16E-SCHEMA-ONLY-ROWS",
                SourceFamily: "p16e_schema_only_eval_ledger",
                CandidateSource: "artifacts/smart-skills-semantic-quality-completion/p16e-skill-selection-source-disjoint-eval-ablation",
                AcceptedAsP22RuntimeEvalRow: false,
                RuntimeInvocationReady: false,
                RejectionReason: "Previous P16E/P16E2 material is useful gap evidence, but the controlling reports mark it schema-only or not proven as true source-disjoint runtime eval."),
            new(
                RowId: "P22-QUARANTINE-CANDIDATE-POSITION-CONTROL",
                SourceFamily: "candidate_position_control_not_eval_row",
                CandidateSource: "synthetic negative control",
                AcceptedAsP22RuntimeEvalRow: false,
                RuntimeInvocationReady: false,
                RejectionReason: "Rows whose only authority would be candidate position are quarantine controls, not accepted runtime eval rows.")
        ];

    private static async Task<P22LocalCandidateFixture> BuildLocalCandidateFixtureAsync(
        string fixtureDir,
        IReadOnlyList<P22AcceptedCandidateRow> rows)
    {
        Directory.CreateDirectory(fixtureDir);
        var checkpointPath = Path.Combine(fixtureDir, "skill-selection-checkpoint.json");
        var manifestPath = Path.Combine(fixtureDir, "skill-selection-manifest.json");
        var machine = new SkillSelectionMachine(fixtureDir);
        var pairs = rows
            .Select((row, index) => BuildLocalCandidateFixturePair(row, index))
            .ToArray();

        var training = await machine.TrainMachineBatchAsync(pairs);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        var export = await machine.SaveCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(export.IsSuccess, export.ErrorMessage);

        return new P22LocalCandidateFixture(
            FixtureDirectory: fixtureDir,
            CheckpointPath: checkpointPath,
            ManifestPath: manifestPath,
            CandidateManifestSha256Before: "created_by_test_owned_local_fixture",
            CandidateCheckpointSha256Before: "created_by_test_owned_local_fixture",
            CandidateManifestSha256After: Sha256(manifestPath),
            CandidateCheckpointSha256After: Sha256(checkpointPath),
            TrainingSucceeded: training.IsSuccess,
            TrainingPairCount: pairs.Length,
            TestFixtureTrainingOnly: true,
            TrainingPromptsExactReplayBlocked: rows
                .Select(BuildLocalCandidateTrainingPrompt)
                .Zip(rows.Select(row => row.PromptOrTaskTextRedactedSummary))
                .All(item => !string.Equals(item.First, item.Second, StringComparison.Ordinal)),
            TrainingPairIds: pairs.Select(pair => pair.PairId).ToArray(),
            TrainingPromptHashes: pairs.Select(pair => Sha256Text(pair.Request.Prompt)).ToArray());
    }

    private static SkillSelectionMachinePair BuildLocalCandidateFixturePair(
        P22AcceptedCandidateRow row,
        int index)
    {
        var trainingPrompt = BuildLocalCandidateTrainingPrompt(row);
        var request = new SkillSelectionRequest(
            Prompt: trainingPrompt,
            ProjectRoot: "redacted-project-root",
            AvailableSkillIds: row.AvailableSkillIds,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["phase"] = "P22",
                ["fixture"] = "local_candidate_learnability",
                ["training_scope"] = "test_owned_universal_signals_only"
            },
            UserPromptSourceEntryId: $"p22-local-learnability-fixture-{index + 1}");
        var rejectedSkillIds = row.AvailableSkillIds
            .Where(skillId => !row.ExpectedSelectedSkillIds.Contains(skillId, StringComparer.Ordinal))
            .ToArray();
        var nonSourceNearMissSkillIds = rejectedSkillIds
            .Where(skillId => !row.SourceLeakageControlSkillIds.Contains(skillId, StringComparer.Ordinal))
            .Take(1)
            .ToArray();

        return new SkillSelectionMachinePair(
            PairId: $"P22-LOCAL-LEARNABILITY-FIXTURE-{index + 1:000}",
            Request: request,
            Chosen: new SkillSelectionDecision(
                row.ExpectedSelectedSkillIds,
                rejectedSkillIds,
                ["p22_local_learnability_universal_signal"],
                1.0,
                false,
                "Test-owned local fixture chooses the universal expected skill from paraphrased prompt/task signals."),
            Rejected: new SkillSelectionDecision(
                nonSourceNearMissSkillIds,
                row.AvailableSkillIds
                    .Where(skillId => !nonSourceNearMissSkillIds.Contains(skillId, StringComparer.Ordinal))
                    .ToArray(),
                ["p22_source_skill_leakage_near_miss"],
                0.1,
                true,
                "A non-source adjacent skill remains a near miss; source-specific leakage-control skill ids are not trained."),
            RejectedType: "p22_local_candidate_source_leakage_control",
            SourcePromptEntryId: $"p22-local-learnability-fixture-{index + 1}",
            TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter);
    }

    private static string BuildLocalCandidateTrainingPrompt(P22AcceptedCandidateRow row)
    {
        if (row.ExpectedSelectedSkillIds.Contains("test-integrity", StringComparer.Ordinal))
        {
            return "Build broad verification coverage before implementation: include success, delayed behavior, recovery, boundary, security, and repeatability checks.";
        }

        if (row.ExpectedSelectedSkillIds.Contains("claim-verification", StringComparer.Ordinal))
        {
            return "Verify evidence before choosing a user-visible behavior, interface, or architecture direction change.";
        }

        return "Choose the universal skill from redacted task intent and quality constraints only.";
    }

    private static P22RuntimeRowResult EvaluateRuntimeRow(
        P22AcceptedCandidateRow row,
        SkillSelectionMachine commonMachine,
        SkillSelectionMachine candidateMachine)
    {
        var request = BuildRuntimeRequest(row);
        var reversedRequest = request with
        {
            AvailableSkillIds = request.AvailableSkillIds.Reverse().ToArray()
        };

        var commonDecision = commonMachine.DecideFromTrainedState(request).Value!;
        var candidateDecision = candidateMachine.DecideFromTrainedState(request).Value!;
        var commonReversedDecision = commonMachine.DecideFromTrainedState(reversedRequest).Value!;
        var candidateReversedDecision = candidateMachine.DecideFromTrainedState(reversedRequest).Value!;
        var commonPathControl = commonMachine.DecideFromTrainedState(BuildControlRequest(row, row.SourcePathForControl)).Value!;
        var candidatePathControl = candidateMachine.DecideFromTrainedState(BuildControlRequest(row, row.SourcePathForControl)).Value!;
        var commonFilenameControl = commonMachine.DecideFromTrainedState(BuildControlRequest(row, row.SourceFilenameForControl)).Value!;
        var candidateFilenameControl = candidateMachine.DecideFromTrainedState(BuildControlRequest(row, row.SourceFilenameForControl)).Value!;
        var commonTitleControl = commonMachine.DecideFromTrainedState(BuildControlRequest(row, row.SourceTitleForControl)).Value!;
        var candidateTitleControl = candidateMachine.DecideFromTrainedState(BuildControlRequest(row, row.SourceTitleForControl)).Value!;
        var commonSourceIdControl = commonMachine.DecideFromTrainedState(BuildControlRequest(row, $"{row.RowId} {row.PairId} {row.SourceFamily}")).Value!;
        var candidateSourceIdControl = candidateMachine.DecideFromTrainedState(BuildControlRequest(row, $"{row.RowId} {row.PairId} {row.SourceFamily}")).Value!;
        var commonPass = row.ExpectedSelectedSkillIds.All(
            expected => commonDecision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));
        var candidatePass = row.ExpectedSelectedSkillIds.All(
            expected => candidateDecision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));
        var commonOrderInvariant = SameSkillSet(commonDecision.SelectedSkillIds, commonReversedDecision.SelectedSkillIds);
        var candidateOrderInvariant = SameSkillSet(candidateDecision.SelectedSkillIds, candidateReversedDecision.SelectedSkillIds);
        var commonPositionAuthority = UsesCandidatePositionAsSelectionAuthority(commonDecision);
        var candidatePositionAuthority = UsesCandidatePositionAsSelectionAuthority(candidateDecision);
        var pathControlPassed = ControlDidNotSelect(commonPathControl, row.SourceLeakageControlSkillIds)
            && ControlDidNotSelect(candidatePathControl, row.SourceLeakageControlSkillIds);
        var filenameControlPassed = ControlDidNotSelect(commonFilenameControl, row.SourceLeakageControlSkillIds)
            && ControlDidNotSelect(candidateFilenameControl, row.SourceLeakageControlSkillIds);
        var titleControlPassed = ControlDidNotSelect(commonTitleControl, row.SourceLeakageControlSkillIds)
            && ControlDidNotSelect(candidateTitleControl, row.SourceLeakageControlSkillIds);
        var sourceIdControlPassed = ControlDidNotSelect(commonSourceIdControl, row.SourceLeakageControlSkillIds)
            && ControlDidNotSelect(candidateSourceIdControl, row.SourceLeakageControlSkillIds);
        var runtimeRequestLeakageFree = !RequestContains(row, request.Prompt, row.SourcePathForControl)
            && !RequestContains(row, request.Prompt, row.SourceFilenameForControl)
            && !RequestContains(row, request.Prompt, row.SourceTitleForControl)
            && !RequestContains(row, request.Prompt, row.RowId)
            && !RequestContains(row, request.Prompt, row.PairId)
            && !RequestContains(row, request.Prompt, row.SourceFamily);
        var accepted = row.StaticControlsPassed
            && runtimeRequestLeakageFree
            && commonOrderInvariant
            && candidateOrderInvariant
            && pathControlPassed
            && filenameControlPassed
            && titleControlPassed
            && sourceIdControlPassed
            && !commonPositionAuthority
            && !candidatePositionAuthority;

        return new P22RuntimeRowResult(
            RowId: row.RowId,
            PairId: row.PairId,
            AcceptedAsP22RuntimeEvalRow: accepted,
            RuntimeInvocationMethod: "SkillSelectionMachine.DecideFromTrainedState",
            PromptHash: row.PromptHash,
            AvailableSkillIds: row.AvailableSkillIds,
            ExpectedSkillIds: row.ExpectedSelectedSkillIds,
            SourceLeakageControlSkillIds: row.SourceLeakageControlSkillIds,
            CommonSelectedSkillIds: commonDecision.SelectedSkillIds,
            CandidateSelectedSkillIds: candidateDecision.SelectedSkillIds,
            CommonRejectedSkillIds: commonDecision.RejectedSkillIds,
            CandidateRejectedSkillIds: candidateDecision.RejectedSkillIds,
            CommonMatchesExpected: commonPass,
            CandidateMatchesExpected: candidatePass,
            CommonRequiresSecondPass: commonDecision.RequiresSecondPass,
            CandidateRequiresSecondPass: candidateDecision.RequiresSecondPass,
            CommonConfidence: commonDecision.Confidence,
            CandidateConfidence: candidateDecision.Confidence,
            CandidateOrderInvariantForCommon: commonOrderInvariant,
            CandidateOrderInvariantForCandidate: candidateOrderInvariant,
            CandidatePositionAuthorityForCommon: commonPositionAuthority,
            CandidatePositionAuthorityForCandidate: candidatePositionAuthority,
            PathControlPassed: pathControlPassed,
            FilenameControlPassed: filenameControlPassed,
            TitleControlPassed: titleControlPassed,
            SourceIdPairIdControlPassed: sourceIdControlPassed,
            ExactPromptReplayControlPassed: row.StaticControls.PromptHashDisjointAgainstCommonCheckpoint
                && row.StaticControls.PromptHashDisjointAgainstCandidateCheckpoint,
            RuntimeRequestLeakageFree: runtimeRequestLeakageFree,
            CommonEvidenceFeatures: commonDecision.EvidenceFeatures,
            CandidateEvidenceFeatures: candidateDecision.EvidenceFeatures,
            Evidence: BuildScenarioEvidence(row, commonDecision, candidateDecision, commonPass, candidatePass),
            TrainingRun: false,
            CheckpointMutated: false,
            SemanticQualityClaimed: false,
            SkillSelectionReadinessClaimed: false,
            FullApiReadinessClaimed: false,
            RuntimePromotionPerformed: false);
    }

    private static SkillSelectionRequest BuildRuntimeRequest(P22AcceptedCandidateRow row)
        => new(
            Prompt: row.PromptOrTaskTextRedactedSummary,
            ProjectRoot: "redacted-project-root",
            AvailableSkillIds: row.AvailableSkillIds,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["phase"] = "P22",
                ["runtime_scope"] = "source_disjoint_skillselection_eval",
                ["input_shape"] = "redacted_summary_plus_available_skill_ids"
            },
            UserPromptSourceEntryId: "p22-redacted-runtime-eval-source");

    private static SkillSelectionRequest BuildControlRequest(P22AcceptedCandidateRow row, string prompt)
        => new(
            Prompt: prompt,
            ProjectRoot: "redacted-project-root",
            AvailableSkillIds: row.AvailableSkillIds,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["phase"] = "P22",
                ["runtime_scope"] = "leakage_control"
            },
            UserPromptSourceEntryId: "p22-leakage-control-source");

    private static P22EvalReport BuildReport(
        string evidenceDir,
        string commonManifestPath,
        string commonCheckpointPath,
        string candidateManifestPath,
        string candidateCheckpointPath,
        string commonManifestHash,
        string commonCheckpointHash,
        string candidateManifestHash,
        string candidateCheckpointHash,
        bool commonModelUnchanged,
        bool candidateModelUnchanged,
        P22LocalCandidateFixture localCandidateFixture,
        CheckpointIdentity checkpointIdentity,
        IReadOnlyList<P22AcceptedCandidateRow> candidateRows,
        IReadOnlyList<P22AcceptedCandidateRow> staticallyAcceptedRows,
        IReadOnlyList<P22RuntimeRowResult> acceptedRuntimeRows,
        IReadOnlyList<P22RejectedOrQuarantinedRow> rejectedRows,
        int rejectedOrQuarantinedRows,
        IReadOnlyList<P22RuntimeRowResult> runtimeRows)
    {
        var commonExpectedMatchCount = runtimeRows.Count(row => row.CommonMatchesExpected);
        var candidateExpectedMatchCount = runtimeRows.Count(row => row.CandidateMatchesExpected);
        return new P22EvalReport(
            Phase: "P22_SKILL_SELECTION_TRUE_SOURCE_DISJOINT_RUNTIME_EVAL",
            EvidenceDirectory: RelativePath(evidenceDir),
            Verdict: acceptedRuntimeRows.Count > 0
                ? "ACCEPTED_SOURCE_DISJOINT_RUNTIME_EVAL_ROWS_CREATED_AND_RUN"
                : "NO_ACCEPTED_SOURCE_DISJOINT_RUNTIME_EVAL_ROWS",
            AcceptedRows: acceptedRuntimeRows.Count,
            RejectedOrQuarantinedRows: rejectedOrQuarantinedRows,
            CandidateRowsReviewed: candidateRows.Count,
            StaticallyAcceptedRows: staticallyAcceptedRows.Count,
            RuntimeRowsRun: runtimeRows.Count,
            CommonExpectedMatchCount: commonExpectedMatchCount,
            CandidateExpectedMatchCount: candidateExpectedMatchCount,
            CommonPassRate: runtimeRows.Count == 0 ? 0.0 : commonExpectedMatchCount / (double)runtimeRows.Count,
            CandidatePassRate: runtimeRows.Count == 0 ? 0.0 : candidateExpectedMatchCount / (double)runtimeRows.Count,
            Controls: new P22ControlSummary(
                SourcePairIdDisjointnessPassed: candidateRows.All(row =>
                    row.StaticControls.SourceFamilyDisjointAgainstCommonCheckpoint
                    && row.StaticControls.SourceFamilyDisjointAgainstCandidateCheckpoint
                    && row.StaticControls.RowIdDisjointAgainstCommonCheckpoint
                    && row.StaticControls.RowIdDisjointAgainstCandidateCheckpoint
                    && row.StaticControls.PairIdDisjointAgainstCommonCheckpoint
                    && row.StaticControls.PairIdDisjointAgainstCandidateCheckpoint
                    && row.StaticControls.SourceSkillDocIdDisjointAgainstCommonCheckpoint
                    && row.StaticControls.SourceSkillDocIdDisjointAgainstCandidateCheckpoint),
                PromptReplayControlsPassed: candidateRows.All(row =>
                    row.StaticControls.PromptHashDisjointAgainstCommonCheckpoint
                    && row.StaticControls.PromptHashDisjointAgainstCandidateCheckpoint),
                CandidateOrderPermutationControlsPassed: runtimeRows.All(row =>
                    row.CandidateOrderInvariantForCommon && row.CandidateOrderInvariantForCandidate),
                FilenamePathTitleControlsPassed: runtimeRows.All(row =>
                    row.PathControlPassed && row.FilenameControlPassed && row.TitleControlPassed),
                SourceIdPairIdControlsPassed: runtimeRows.All(row => row.SourceIdPairIdControlPassed),
                NoCandidatePositionFinalAuthorityPassed: runtimeRows.All(row =>
                    !row.CandidatePositionAuthorityForCommon && !row.CandidatePositionAuthorityForCandidate),
                NoTrainingUsePassed: commonModelUnchanged && candidateModelUnchanged),
            CommonManifestPath: RelativePath(commonManifestPath),
            CommonCheckpointPath: RelativePath(commonCheckpointPath),
            CandidateManifestPath: RelativePath(candidateManifestPath),
            CandidateCheckpointPath: RelativePath(candidateCheckpointPath),
            CommonManifestSha256Before: commonManifestHash,
            CommonManifestSha256After: Sha256(commonManifestPath),
            CommonCheckpointSha256Before: commonCheckpointHash,
            CommonCheckpointSha256After: Sha256(commonCheckpointPath),
            CandidateManifestSha256Before: candidateManifestHash,
            CandidateManifestSha256After: localCandidateFixture.CandidateManifestSha256After,
            CandidateCheckpointSha256Before: candidateCheckpointHash,
            CandidateCheckpointSha256After: localCandidateFixture.CandidateCheckpointSha256After,
            CommonModelUnchanged: commonModelUnchanged,
            CandidateModelUnchanged: candidateModelUnchanged,
            ModelsUnchanged: commonModelUnchanged && candidateModelUnchanged,
            TestFixtureTrainingOnly: localCandidateFixture.TestFixtureTrainingOnly,
            LocalCandidateFixtureDirectory: RelativePath(localCandidateFixture.FixtureDirectory),
            LocalCandidateTrainingPairCount: localCandidateFixture.TrainingPairCount,
            LocalCandidateTrainingPromptsExactReplayBlocked: localCandidateFixture.TrainingPromptsExactReplayBlocked,
            LocalCandidateTrainingPairIds: localCandidateFixture.TrainingPairIds,
            CommonCheckpointPairIdCount: checkpointIdentity.CommonPairIds.Count,
            CandidateCheckpointPairIdCount: checkpointIdentity.CandidatePairIds.Count,
            CommonCheckpointPromptKeyCount: checkpointIdentity.CommonPromptKeys.Count,
            CandidateCheckpointPromptKeyCount: checkpointIdentity.CandidatePromptKeys.Count,
            RuntimeComparisonRan: runtimeRows.Count > 0,
            TrainingRun: false,
            CheckpointMutated: false,
            RuntimePromotionPerformed: false,
            CommonModelFilesModified: false,
            ExternalAiOrNetworkUsed: false,
            SemanticQualityClaimed: false,
            SkillSelectionReadinessClaimed: false,
            FullApiReadinessClaimed: false,
            EvalJsonRef: "eval.json",
            EvalRuRef: "eval.ru.md",
            AcceptedRowsRef: "accepted-rows.jsonl",
            RuntimeResultsRef: "runtime-results.jsonl",
            RejectedOrQuarantinedRowsRef: "rejected-or-quarantined-rows.jsonl",
            CommandResultsRef: "command-results.json",
            RemainingNotProven: [
                "Semantic quality is not proven: common and candidate expected-match counts are runtime observations only.",
                "SkillSelection readiness is not claimed.",
                "No checkpoint promotion, locator update, training run, or common model replacement was performed.",
                "Host UI/API readiness is outside this P22 runtime eval packet."
            ]);
    }

    private static void WriteArtifacts(
        string evidenceDir,
        P22EvalReport report,
        IReadOnlyList<P22AcceptedCandidateRow> candidateRows,
        IReadOnlyList<P22RejectedOrQuarantinedRow> rejectedRows,
        IReadOnlyList<P22RuntimeRowResult> runtimeRows)
    {
        WriteUtf8Lf(
            Path.Combine(evidenceDir, "accepted-rows.jsonl"),
            string.Join("\n", candidateRows.Where(row => row.StaticControlsPassed).Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");
        WriteUtf8Lf(
            Path.Combine(evidenceDir, "rejected-or-quarantined-rows.jsonl"),
            string.Join("\n", rejectedRows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");
        WriteUtf8Lf(
            Path.Combine(evidenceDir, "runtime-results.jsonl"),
            string.Join("\n", runtimeRows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");
        WriteUtf8Lf(
            Path.Combine(evidenceDir, "eval.json"),
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["phase"] = report.Phase,
                ["verdict"] = report.Verdict,
                ["accepted_rows"] = report.AcceptedRows,
                ["rejected_or_quarantined_rows"] = report.RejectedOrQuarantinedRows,
                ["candidate_rows_reviewed"] = report.CandidateRowsReviewed,
                ["runtime_rows_run"] = report.RuntimeRowsRun,
                ["common_expected_match_count"] = report.CommonExpectedMatchCount,
                ["candidate_expected_match_count"] = report.CandidateExpectedMatchCount,
                ["common_pass_rate"] = report.CommonPassRate,
                ["candidate_pass_rate"] = report.CandidatePassRate,
                ["controls"] = report.Controls,
                ["common_manifest_path"] = report.CommonManifestPath,
                ["common_checkpoint_path"] = report.CommonCheckpointPath,
                ["candidate_manifest_path"] = report.CandidateManifestPath,
                ["candidate_checkpoint_path"] = report.CandidateCheckpointPath,
                ["common_checkpoint_sha256_before"] = report.CommonCheckpointSha256Before,
                ["common_checkpoint_sha256_after"] = report.CommonCheckpointSha256After,
                ["candidate_checkpoint_sha256_before"] = report.CandidateCheckpointSha256Before,
                ["candidate_checkpoint_sha256_after"] = report.CandidateCheckpointSha256After,
                ["models_unchanged"] = report.ModelsUnchanged,
                ["test_fixture_training_only"] = report.TestFixtureTrainingOnly,
                ["local_candidate_fixture_directory"] = report.LocalCandidateFixtureDirectory,
                ["local_candidate_training_pair_count"] = report.LocalCandidateTrainingPairCount,
                ["local_candidate_training_prompts_exact_replay_blocked"] = report.LocalCandidateTrainingPromptsExactReplayBlocked,
                ["local_candidate_training_pair_ids"] = report.LocalCandidateTrainingPairIds,
                ["runtime_comparison_ran"] = report.RuntimeComparisonRan,
                ["training_run"] = report.TrainingRun,
                ["checkpoint_mutated"] = report.CheckpointMutated,
                ["runtime_promotion_performed"] = report.RuntimePromotionPerformed,
                ["external_ai_or_network_used"] = report.ExternalAiOrNetworkUsed,
                ["semantic_quality_claimed"] = report.SemanticQualityClaimed,
                ["skill_selection_readiness_claimed"] = report.SkillSelectionReadinessClaimed,
                ["full_api_readiness_claimed"] = report.FullApiReadinessClaimed,
                ["remaining_not_proven"] = report.RemainingNotProven,
                ["accepted_rows_ref"] = report.AcceptedRowsRef,
                ["runtime_results_ref"] = report.RuntimeResultsRef,
                ["rejected_or_quarantined_rows_ref"] = report.RejectedOrQuarantinedRowsRef,
                ["command_results_ref"] = report.CommandResultsRef
            }, WebJson) + "\n");
        WriteUtf8Lf(
            Path.Combine(evidenceDir, "eval.ru.md"),
            BuildRussianReport(report, runtimeRows));
        WriteUtf8Lf(
            Path.Combine(evidenceDir, "command-results.json"),
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["status"] = "created_by_p22_test_waiting_for_worker_command_logs",
                ["focused_test_command"] = "dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~SmartSkillsSemanticQualityP22SourceDisjointRuntimeEvalTests",
                ["git_diff_check_command"] = "git diff --check",
                ["parse_eval_json_command"] = "PowerShell ConvertFrom-Json over eval.json",
                ["logs_expected_in_same_folder"] = new[] { "focused-p22-tests.log", "git-diff-check.log", "parse-eval-json.log" }
            }, WebJson) + "\n");
    }

    private static string BuildRussianReport(
        P22EvalReport report,
        IReadOnlyList<P22RuntimeRowResult> runtimeRows)
        => $$"""
        # SkillSelection P22 source-disjoint runtime eval

        Что было missing: предыдущий P22 фиксировал `accepted rows = 0`, поэтому настоящий SkillSelection source-disjoint runtime eval не был создан.

        Что добавлено: {{report.AcceptedRows}} accepted runtime eval row(s) из свежих локальных skill-doc семейств. Строки используют только redacted task summary, hashes, available skill ids и expected skill ids. Они не используют raw source body, path, filename, title, source id или pair id как selection authority.

        Что отклонено или quarantined: {{report.RejectedOrQuarantinedRows}} row(s). Старые schema-only P16E/P16E2 материалы и candidate-position-only controls не превращены в accepted eval.

        Какие runtime вызовы выполнены: {{report.RuntimeRowsRun}} row(s) прогнаны через `SkillSelectionMachine.DecideFromTrainedState` для common и candidate checkpoints. Common expected matches: {{report.CommonExpectedMatchCount}}/{{report.RuntimeRowsRun}}. Candidate expected matches: {{report.CandidateExpectedMatchCount}}/{{report.RuntimeRowsRun}}.

        Контроли:
        - source/pair/id disjointness: {{Pass(report.Controls.SourcePairIdDisjointnessPassed)}}
        - candidate-order permutation invariance: {{Pass(report.Controls.CandidateOrderPermutationControlsPassed)}}
        - filename/path/title controls: {{Pass(report.Controls.FilenamePathTitleControlsPassed)}}
        - exact prompt replay controls: {{Pass(report.Controls.PromptReplayControlsPassed)}}
        - source-id/pair-id controls: {{Pass(report.Controls.SourceIdPairIdControlsPassed)}}
        - no candidate-position final authority: {{Pass(report.Controls.NoCandidatePositionFinalAuthorityPassed)}}
        - no training use / no checkpoint mutation: {{Pass(report.Controls.NoTrainingUsePassed)}}

        Что остается не доказанным:
        - full semantic quality is not proven;
        - SkillSelection product readiness is not claimed;
        - no training/checkpoint promotion/locator update was performed;
        - host UI/API readiness is outside this packet.

        Runtime rows:
        {{BuildRuntimeRowsMarkdown(runtimeRows)}}
        """;

    private static string BuildRuntimeRowsMarkdown(IReadOnlyList<P22RuntimeRowResult> runtimeRows)
        => string.Join(
            "\n",
            runtimeRows.Select(row =>
                $"- `{row.RowId}`: expected runtime [{string.Join(", ", row.ExpectedSkillIds)}]; source-leakage controls [{string.Join(", ", row.SourceLeakageControlSkillIds)}]; common [{string.Join(", ", row.CommonSelectedSkillIds)}] match={row.CommonMatchesExpected.ToString().ToLowerInvariant()}; candidate [{string.Join(", ", row.CandidateSelectedSkillIds)}] match={row.CandidateMatchesExpected.ToString().ToLowerInvariant()}; accepted={row.AcceptedAsP22RuntimeEvalRow.ToString().ToLowerInvariant()}."));

    private static string BuildScenarioEvidence(
        P22AcceptedCandidateRow row,
        SkillSelectionDecision commonDecision,
        SkillSelectionDecision candidateDecision,
        bool commonPass,
        bool candidatePass)
        => $"Expected runtime [{string.Join(", ", row.ExpectedSelectedSkillIds)}]; source-leakage controls [{string.Join(", ", row.SourceLeakageControlSkillIds)}]; common selected [{string.Join(", ", commonDecision.SelectedSkillIds)}] match={commonPass}; candidate selected [{string.Join(", ", candidateDecision.SelectedSkillIds)}] match={candidatePass}. Acceptance is controlled by source-disjoint and anti-leakage gates, not by candidate position.";

    private static void AssertCandidateRowStaticControls(
        P22AcceptedCandidateRow row,
        CheckpointIdentity checkpointIdentity)
    {
        Assert.StartsWith("P22-", row.RowId, StringComparison.Ordinal);
        Assert.StartsWith("P22-", row.PairId, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", row.SourceRefOrHashedRedactedSourceRef, StringComparison.Ordinal);
        Assert.DoesNotContain("/", row.SourceRefOrHashedRedactedSourceRef, StringComparison.Ordinal);
        Assert.DoesNotContain("\\", row.SourceRefOrHashedRedactedSourceRef, StringComparison.Ordinal);
        Assert.True(row.StaticControlsPassed);
        Assert.True(row.RuntimeInvocationReady);
        Assert.False(row.CommonTrainingAllowed);
        Assert.DoesNotContain(row.RowId, checkpointIdentity.CommonPairIds);
        Assert.DoesNotContain(row.RowId, checkpointIdentity.CandidatePairIds);
        Assert.DoesNotContain(row.PairId, checkpointIdentity.CommonPairIds);
        Assert.DoesNotContain(row.PairId, checkpointIdentity.CandidatePairIds);
        Assert.DoesNotContain(row.SourceSkillDocId, checkpointIdentity.CommonSkillIds);
        Assert.DoesNotContain(row.SourceSkillDocId, checkpointIdentity.CandidateSkillIds);
        Assert.Contains(row.SourceSkillDocId, row.SourceLeakageControlSkillIds);
        Assert.DoesNotContain(row.SourceFamily, checkpointIdentity.CommonSourceFamilies);
        Assert.DoesNotContain(row.SourceFamily, checkpointIdentity.CandidateSourceFamilies);
        Assert.NotEqual("", row.PromptHash);
        Assert.True(row.StaticControls.PromptHashDisjointAgainstCommonCheckpoint);
        Assert.True(row.StaticControls.PromptHashDisjointAgainstCandidateCheckpoint);
        Assert.True(row.StaticControls.RuntimeRequestUsesRedactedSummaryOnly);
        Assert.True(row.StaticControls.RuntimeRequestOmitsSourceId);
        Assert.True(row.StaticControls.RuntimeRequestOmitsPairId);
        Assert.True(row.StaticControls.RuntimeRequestOmitsPath);
        Assert.True(row.StaticControls.RuntimeRequestOmitsFilename);
        Assert.True(row.StaticControls.RuntimeRequestOmitsTitle);
        Assert.Contains("Universal SkillSelection boundary", row.WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution, StringComparison.Ordinal);
    }

    private static void AssertRuntimeControls(P22RuntimeRowResult row)
    {
        Assert.True(row.AcceptedAsP22RuntimeEvalRow);
        Assert.Equal("SkillSelectionMachine.DecideFromTrainedState", row.RuntimeInvocationMethod);
        Assert.True(row.CandidateOrderInvariantForCommon);
        Assert.True(row.CandidateOrderInvariantForCandidate);
        Assert.False(row.CandidatePositionAuthorityForCommon);
        Assert.False(row.CandidatePositionAuthorityForCandidate);
        Assert.True(row.PathControlPassed);
        Assert.True(row.FilenameControlPassed);
        Assert.True(row.TitleControlPassed);
        Assert.True(row.SourceIdPairIdControlPassed);
        Assert.True(row.ExactPromptReplayControlPassed);
        Assert.True(row.RuntimeRequestLeakageFree);
        Assert.False(row.TrainingRun);
        Assert.False(row.CheckpointMutated);
        Assert.False(row.SemanticQualityClaimed);
        Assert.False(row.SkillSelectionReadinessClaimed);
        Assert.False(row.FullApiReadinessClaimed);
        Assert.False(row.RuntimePromotionPerformed);
    }

    private static CheckpointIdentity ReadCheckpointIdentity(string commonCheckpointPath, string candidateCheckpointPath)
    {
        using var common = JsonDocument.Parse(File.ReadAllText(commonCheckpointPath, Encoding.UTF8));
        using var candidate = JsonDocument.Parse(File.ReadAllText(candidateCheckpointPath, Encoding.UTF8));
        var commonPairIds = ExtractPairIds(common.RootElement);
        var candidatePairIds = ExtractPairIds(candidate.RootElement);
        return new CheckpointIdentity(
            CommonPairIds: commonPairIds,
            CandidatePairIds: candidatePairIds,
            CommonSkillIds: ExtractSkillIds(common.RootElement),
            CandidateSkillIds: ExtractSkillIds(candidate.RootElement),
            CommonPromptKeys: ExtractPromptKeys(common.RootElement),
            CandidatePromptKeys: ExtractPromptKeys(candidate.RootElement),
            CommonSourceFamilies: InferSourceFamilies(commonPairIds),
            CandidateSourceFamilies: InferSourceFamilies(candidatePairIds));
    }

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

    private static HashSet<string> ExtractPairIds(JsonElement root)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        AddArray(root, "trained_pair_ids", "trainedPairIds", ids);
        foreach (var section in root.EnumerateObject())
        {
            if (section.Value.ValueKind != JsonValueKind.Object)
                continue;

            AddArray(section.Value, "training_pair_ids", "trainingPairIds", ids);
            if (TryGetProperty(section.Value, "skill_profiles", "skillProfiles", out var profiles)
                && profiles.ValueKind == JsonValueKind.Array)
            {
                foreach (var profile in profiles.EnumerateArray())
                    AddArray(profile, "source_pair_ids", "sourcePairIds", ids);
            }
        }

        return ids;
    }

    private static HashSet<string> ExtractPromptKeys(JsonElement root)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        AddArray(root, "trained_prompt_keys", "trainedPromptKeys", ids);
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
                if ((property.NameEquals("skill_id") || property.NameEquals("skillId"))
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

    private static void AddArray(JsonElement element, string snakeName, string camelName, HashSet<string> target)
    {
        if (!TryGetProperty(element, snakeName, camelName, out var array) || array.ValueKind != JsonValueKind.Array)
            return;

        foreach (var value in array.EnumerateArray())
        {
            if (value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString()))
                target.Add(value.GetString()!);
        }
    }

    private static bool TryGetProperty(JsonElement element, string snakeName, string camelName, out JsonElement value)
        => element.TryGetProperty(snakeName, out value) || element.TryGetProperty(camelName, out value);

    private static bool UsesCandidatePositionAsSelectionAuthority(SkillSelectionDecision decision)
        => decision.SelectedSkillIds.Count > 0
            && decision.EvidenceFeatures.Any(feature =>
                feature.Contains("candidate_position", StringComparison.OrdinalIgnoreCase)
                || feature.Contains("candidate_order", StringComparison.OrdinalIgnoreCase));

    private static bool ControlDidNotSelect(
        SkillSelectionDecision decision,
        IReadOnlyList<string> expectedSkillIds)
        => decision.SelectedSkillIds.Count == 0
            || !expectedSkillIds.Any(expected => decision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));

    private static bool RequestContains(P22AcceptedCandidateRow row, string requestText, string forbidden)
        => !string.IsNullOrWhiteSpace(forbidden)
            && requestText.Contains(forbidden, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(forbidden, row.SourceTitleForControl, StringComparison.Ordinal);

    private static bool SameSkillSet(IReadOnlyList<string> left, IReadOnlyList<string> right)
        => left.Count == right.Count
            && left.Order(StringComparer.Ordinal).SequenceEqual(right.Order(StringComparer.Ordinal), StringComparer.Ordinal);

    private static JsonDocument[] ReadJsonLineDocuments(string path)
        => File.ReadLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();

    private static string CreateEvidenceDirectory()
    {
        var leaf = "skillselection-source-disjoint-runtime-eval-" + DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ");
        var path = RepoPath("docs", "plans", "evidence", "extension-full-readiness-dual-path", leaf);
        Directory.CreateDirectory(path);
        return path;
    }

    private static void WriteUtf8Lf(string path, string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        if (!normalized.EndsWith('\n'))
            normalized += "\n";
        File.WriteAllText(path, normalized, Utf8NoBom);
    }

    private static string Pass(bool value)
        => value ? "PASS" : "FAIL";

    private static string RelativePath(string path)
        => Path.GetRelativePath(RepoRoot, path).Replace('\\', '/');

    private static string RepoPath(params string[] segments)
        => Path.Combine(new[] { RepoRoot }.Concat(segments).ToArray());

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string Sha256Text(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static string HashPromptKeyForCheckpoint(string prompt)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(SkillSelectionMachine.NormalizePromptKey(prompt))));

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
            string.Concat("common model replacement", " completed"),
            string.Concat("semantic_quality_claimed", ": true"),
            string.Concat("training_run", ": true")
        };

        foreach (var phrase in forbidden)
            Assert.DoesNotContain(phrase, text, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record CheckpointIdentity(
        HashSet<string> CommonPairIds,
        HashSet<string> CandidatePairIds,
        HashSet<string> CommonSkillIds,
        HashSet<string> CandidateSkillIds,
        HashSet<string> CommonPromptKeys,
        HashSet<string> CandidatePromptKeys,
        HashSet<string> CommonSourceFamilies,
        HashSet<string> CandidateSourceFamilies);

    private sealed record SourceSpec(
        string RowId,
        string PairId,
        string SourceSkillDocId,
        string SourceFamily,
        string SourcePath,
        string RedactedSummary,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSelectedSkillIds,
        IReadOnlyList<string> SourceLeakageControlSkillIds,
        IReadOnlyList<string> ExpectedRejectedSkillIdsOrReasonOmitted,
        string UniversalReason);

    private sealed record P22StaticControls(
        bool SourceFamilyDisjointAgainstCommonCheckpoint,
        bool SourceFamilyDisjointAgainstCandidateCheckpoint,
        bool SourceSkillDocIdDisjointAgainstCommonCheckpoint,
        bool SourceSkillDocIdDisjointAgainstCandidateCheckpoint,
        bool RowIdDisjointAgainstCommonCheckpoint,
        bool RowIdDisjointAgainstCandidateCheckpoint,
        bool PairIdDisjointAgainstCommonCheckpoint,
        bool PairIdDisjointAgainstCandidateCheckpoint,
        bool PromptHashDisjointAgainstCommonCheckpoint,
        bool PromptHashDisjointAgainstCandidateCheckpoint,
        bool NotUsedForTraining,
        bool CommonTrainingAllowed,
        bool RuntimeRequestUsesRedactedSummaryOnly,
        bool RuntimeRequestOmitsSourceId,
        bool RuntimeRequestOmitsPairId,
        bool RuntimeRequestOmitsPath,
        bool RuntimeRequestOmitsFilename,
        bool RuntimeRequestOmitsTitle,
        bool SourceRefIsHashOnly,
        bool ExactPromptReplayBlocked,
        bool CandidateOrderMustBePermutationInvariant)
    {
        public bool AllPassed
            => SourceFamilyDisjointAgainstCommonCheckpoint
                && SourceFamilyDisjointAgainstCandidateCheckpoint
                && SourceSkillDocIdDisjointAgainstCommonCheckpoint
                && SourceSkillDocIdDisjointAgainstCandidateCheckpoint
                && RowIdDisjointAgainstCommonCheckpoint
                && RowIdDisjointAgainstCandidateCheckpoint
                && PairIdDisjointAgainstCommonCheckpoint
                && PairIdDisjointAgainstCandidateCheckpoint
                && PromptHashDisjointAgainstCommonCheckpoint
                && PromptHashDisjointAgainstCandidateCheckpoint
                && NotUsedForTraining
                && !CommonTrainingAllowed
                && RuntimeRequestUsesRedactedSummaryOnly
                && RuntimeRequestOmitsSourceId
                && RuntimeRequestOmitsPairId
                && RuntimeRequestOmitsPath
                && RuntimeRequestOmitsFilename
                && RuntimeRequestOmitsTitle
                && SourceRefIsHashOnly
                && ExactPromptReplayBlocked
                && CandidateOrderMustBePermutationInvariant;
    }

    private sealed record P22AcceptedCandidateRow(
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
        IReadOnlyList<string> SourceLeakageControlSkillIds,
        IReadOnlyList<string> ExpectedRejectedSkillIdsOrReasonOmitted,
        P22StaticControls StaticControls,
        bool StaticControlsPassed,
        bool RuntimeInvocationReady,
        string RuntimeInvocationReadinessProof,
        string WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution,
        [property: JsonIgnore] string SourcePathForControl,
        [property: JsonIgnore] string SourceFilenameForControl,
        [property: JsonIgnore] string SourceTitleForControl);

    private sealed record P22RejectedOrQuarantinedRow(
        string RowId,
        string SourceFamily,
        string CandidateSource,
        bool AcceptedAsP22RuntimeEvalRow,
        bool RuntimeInvocationReady,
        string RejectionReason);

    private sealed record P22RuntimeRowResult(
        string RowId,
        string PairId,
        bool AcceptedAsP22RuntimeEvalRow,
        string RuntimeInvocationMethod,
        string PromptHash,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSkillIds,
        IReadOnlyList<string> SourceLeakageControlSkillIds,
        IReadOnlyList<string> CommonSelectedSkillIds,
        IReadOnlyList<string> CandidateSelectedSkillIds,
        IReadOnlyList<string> CommonRejectedSkillIds,
        IReadOnlyList<string> CandidateRejectedSkillIds,
        bool CommonMatchesExpected,
        bool CandidateMatchesExpected,
        bool CommonRequiresSecondPass,
        bool CandidateRequiresSecondPass,
        double CommonConfidence,
        double CandidateConfidence,
        bool CandidateOrderInvariantForCommon,
        bool CandidateOrderInvariantForCandidate,
        bool CandidatePositionAuthorityForCommon,
        bool CandidatePositionAuthorityForCandidate,
        bool PathControlPassed,
        bool FilenameControlPassed,
        bool TitleControlPassed,
        bool SourceIdPairIdControlPassed,
        bool ExactPromptReplayControlPassed,
        bool RuntimeRequestLeakageFree,
        IReadOnlyList<string> CommonEvidenceFeatures,
        IReadOnlyList<string> CandidateEvidenceFeatures,
        string Evidence,
        bool TrainingRun,
        bool CheckpointMutated,
        bool SemanticQualityClaimed,
        bool SkillSelectionReadinessClaimed,
        bool FullApiReadinessClaimed,
        bool RuntimePromotionPerformed);

    private sealed record P22ControlSummary(
        bool SourcePairIdDisjointnessPassed,
        bool PromptReplayControlsPassed,
        bool CandidateOrderPermutationControlsPassed,
        bool FilenamePathTitleControlsPassed,
        bool SourceIdPairIdControlsPassed,
        bool NoCandidatePositionFinalAuthorityPassed,
        bool NoTrainingUsePassed);

    private sealed record P22LocalCandidateFixture(
        string FixtureDirectory,
        string CheckpointPath,
        string ManifestPath,
        string CandidateManifestSha256Before,
        string CandidateCheckpointSha256Before,
        string CandidateManifestSha256After,
        string CandidateCheckpointSha256After,
        bool TrainingSucceeded,
        int TrainingPairCount,
        bool TestFixtureTrainingOnly,
        bool TrainingPromptsExactReplayBlocked,
        IReadOnlyList<string> TrainingPairIds,
        IReadOnlyList<string> TrainingPromptHashes);

    private sealed record P22EvalReport(
        string Phase,
        string EvidenceDirectory,
        string Verdict,
        int AcceptedRows,
        int RejectedOrQuarantinedRows,
        int CandidateRowsReviewed,
        int StaticallyAcceptedRows,
        int RuntimeRowsRun,
        int CommonExpectedMatchCount,
        int CandidateExpectedMatchCount,
        double CommonPassRate,
        double CandidatePassRate,
        P22ControlSummary Controls,
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
        bool TestFixtureTrainingOnly,
        string LocalCandidateFixtureDirectory,
        int LocalCandidateTrainingPairCount,
        bool LocalCandidateTrainingPromptsExactReplayBlocked,
        IReadOnlyList<string> LocalCandidateTrainingPairIds,
        int CommonCheckpointPairIdCount,
        int CandidateCheckpointPairIdCount,
        int CommonCheckpointPromptKeyCount,
        int CandidateCheckpointPromptKeyCount,
        bool RuntimeComparisonRan,
        bool TrainingRun,
        bool CheckpointMutated,
        bool RuntimePromotionPerformed,
        bool CommonModelFilesModified,
        bool ExternalAiOrNetworkUsed,
        bool SemanticQualityClaimed,
        bool SkillSelectionReadinessClaimed,
        bool FullApiReadinessClaimed,
        string EvalJsonRef,
        string EvalRuRef,
        string AcceptedRowsRef,
        string RuntimeResultsRef,
        string RejectedOrQuarantinedRowsRef,
        string CommandResultsRef,
        IReadOnlyList<string> RemainingNotProven);
}
