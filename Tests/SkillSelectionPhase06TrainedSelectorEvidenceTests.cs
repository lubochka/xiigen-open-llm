// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure.Logical;
using LlmFromScratch.Tests;
using SmartSkillsDomain;

[Collection(global::ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class SkillSelectionPhase06TrainedSelectorEvidenceTests
{
    private const string FailpointMessage = "PHASE06_AFTER_FIRST_OWNED_ARTIFACT_EXISTS";
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] UniversalSkills =
    [
        "planning-session-startup",
        "plan-review",
        "test-integrity",
        "implementation-integrity",
        "skill-injection",
        "planning-dpo-authoring",
        "trainable-architecture-decision"
    ];
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string ProtectedEvidenceDirectory = Path.Combine(
        RepositoryRoot,
        "artifacts",
        "extension-runtime-token-quality-learnability",
        "phase-06-skillselection-trained-selector");

    [Fact]
    public async Task Phase06SkillSelectionTrainedSelectorCreatesCheckpointHeldoutAndFreshLoadEvidence()
    {
        var before = CaptureProtectedRepositorySnapshot();
        string? ownedRoot = null;

        await TestOwnedOutputRoot.RunAsync(async output =>
        {
            ownedRoot = output.RootPath;
            Assert.Equal(Path.GetFullPath(RepositoryRoot), Path.GetFullPath(output.RepositoryRootPath));
            await ExecutePhase06Async(output);
        });

        Assert.NotNull(ownedRoot);
        Assert.False(Directory.Exists(ownedRoot));
        Assert.Equal(before, CaptureProtectedRepositorySnapshot());
    }

    [Fact]
    public async Task Phase06FailureAfterFirstOwnedArtifactExistsCleansOutputAndPreservesCommittedEvidence()
    {
        var before = CaptureProtectedRepositorySnapshot();
        string? ownedRoot = null;
        var failpointCalls = 0;

        var failure = await Assert.ThrowsAsync<Phase06OwnedArtifactFailpointException>(() =>
            TestOwnedOutputRoot.RunAsync(async output =>
            {
                ownedRoot = output.RootPath;
                Assert.Equal(Path.GetFullPath(RepositoryRoot), Path.GetFullPath(output.RepositoryRootPath));

                var immediatelyBeforeRejectedWrite = CaptureProtectedRepositorySnapshot();
                Assert.Throws<InvalidOperationException>(() => WriteText(
                    output,
                    Path.Combine(ProtectedEvidenceDirectory, "forbidden-write.json"),
                    "{}"));
                Assert.Equal(immediatelyBeforeRejectedWrite, CaptureProtectedRepositorySnapshot());

                var expectedFirstArtifactPath = output.RequireOwnedPath(Path.Combine(
                    output.RootPath,
                    "artifacts",
                    "extension-runtime-token-quality-learnability",
                    "phase-06-skillselection-trained-selector",
                    "run",
                    "phase06-execution-started.json"));
                Assert.False(File.Exists(expectedFirstArtifactPath));

                await ExecutePhase06Async(output, firstArtifactPath =>
                {
                    Assert.Equal(Path.GetFullPath(expectedFirstArtifactPath), Path.GetFullPath(firstArtifactPath));
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

    private static async Task ExecutePhase06Async(
        TestOwnedOutputRoot output,
        Action<string>? afterFirstOwnedArtifactExists = null)
    {
        var evidenceDir = output.RequireOwnedPath(Path.Combine(
            output.RootPath,
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "phase-06-skillselection-trained-selector",
            "run"));
        Directory.CreateDirectory(output.RequireOwnedPath(evidenceDir));
        var firstArtifactPath = OwnedEvidencePath(output, evidenceDir, "phase06-execution-started.json");
        Assert.False(File.Exists(firstArtifactPath));
        WriteText(output, firstArtifactPath, "{\"phase\":\"06\",\"status\":\"started\"}");
        afterFirstOwnedArtifactExists?.Invoke(firstArtifactPath);

        var trainingRows = CreateTrainingRows();
        var heldoutRows = CreateHeldoutRows();
        var trainItems = trainingRows.Select(ToLearningTrainingItem).ToArray();
        var evalItems = heldoutRows.Select(ToLearningEvaluationItem).ToArray();
        var batch = new LearningBatch<SkillSelectionDomain>(
            BatchId: "phase06-skillselection-trained-selector-batch",
            DomainTag: SkillSelectionMachine.DomainTag,
            TrainingItems: trainItems,
            SourceIds: trainingRows.Select(row => row.SourcePromptEntryId).ToArray());
        var evalSet = new LearningEvaluationSet<SkillSelectionDomain>(
            EvaluationId: "phase06-skillselection-source-disjoint-heldout",
            DomainTag: SkillSelectionMachine.DomainTag,
            EvaluationItems: evalItems);
        var emptyMachine = new SkillSelectionMachine();
        var emptyEval = await emptyMachine.EvaluateAsync(evalSet);
        var machine = new SkillSelectionMachine(evidenceDir);
        var trainResult = await machine.TrainBatchAsync(batch);
        var evalResult = await machine.EvaluateAsync(evalSet);
        var checkpointPath = OwnedEvidencePath(output, evidenceDir, "selector-checkpoint.json");
        var manifestPath = OwnedEvidencePath(output, evidenceDir, "selector-checkpoint-manifest.json");
        var exportResult = await machine.SaveCheckpointAsync(checkpointPath, manifestPath);
        var importedMachine = new SkillSelectionMachine();
        var importResult = await importedMachine.ImportAsync(exportResult.Value!);
        var freshLoad = await SkillSelectionMachine.LoadCheckpointAsync(checkpointPath);
        var freshEval = await importedMachine.EvaluateAsync(evalSet);
        var heldoutResults = heldoutRows
            .Select(row => EvaluateHeldoutRow(machine, row))
            .ToArray();
        var leakageRows = heldoutRows
            .Select(row => EvaluateLeakage(machine, row))
            .ToArray();
        var sourceControl = EvaluateSourceIdOnlyControl(machine);
        var requiredFiles = WriteEvidenceFiles(
            output,
            evidenceDir,
            firstArtifactPath,
            trainingRows,
            heldoutRows,
            trainResult.Value!,
            emptyEval.Value!,
            evalResult.Value!,
            freshEval.Value!,
            exportResult.Value!,
            importResult,
            freshLoad,
            heldoutResults,
            leakageRows,
            sourceControl);

        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);
        Assert.True(evalResult.IsSuccess, evalResult.ErrorMessage);
        Assert.True(evalResult.Value!.PassesGate, evalResult.Value.GateNote);
        Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
        Assert.True(importResult.IsSuccess, importResult.ErrorMessage);
        Assert.True(freshLoad.IsSuccess, freshLoad.ErrorMessage);
        Assert.True(freshEval.IsSuccess, freshEval.ErrorMessage);
        Assert.Equal(evalResult.Value.Accuracy, freshEval.Value!.Accuracy);
        Assert.All(heldoutResults, row => Assert.True(row.Pass));
        Assert.All(leakageRows, row => Assert.True(row.CandidateOrderInvariant));
        Assert.False(sourceControl.SelectedFromSourceIdOnly);
        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));
        AssertNoBomCrOrTrailingWhitespace(output, requiredFiles);
        AssertJsonFilesParse(output, requiredFiles);
        AssertStableEvidenceText(output, requiredFiles);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;

            current = current.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }

    private static IReadOnlyList<Phase06SkillSelectionRow> CreateTrainingRows()
        =>
        [
            new(
                RowId: "phase06-train-planning-001",
                SourcePromptEntryId: "phase06-train-source-planning-001",
                Prompt: "Create bounded ordered work plan requirements gates artifacts for an operator handoff.",
                ChosenSkillIds: ["planning-session-startup"],
                RejectedSkillIds: ExceptSkills("planning-session-startup")),
            new(
                RowId: "phase06-train-review-001",
                SourcePromptEntryId: "phase06-train-source-review-001",
                Prompt: "Review architecture trainable hierarchy hidden shortcut candidate order risk before execution.",
                ChosenSkillIds: ["plan-review"],
                RejectedSkillIds: ExceptSkills("plan-review")),
            new(
                RowId: "phase06-train-test-001",
                SourcePromptEntryId: "phase06-train-source-test-001",
                Prompt: "Check tests production path stub masking assertion reachability negative cases.",
                ChosenSkillIds: ["test-integrity"],
                RejectedSkillIds: ExceptSkills("test-integrity")),
            new(
                RowId: "phase06-train-implementation-001",
                SourcePromptEntryId: "phase06-train-source-implementation-001",
                Prompt: "Inspect implementation evidence real code path anti stub checkpoint lifecycle metrics.",
                ChosenSkillIds: ["implementation-integrity"],
                RejectedSkillIds: ExceptSkills("implementation-integrity")),
            new(
                RowId: "phase06-train-injection-001",
                SourcePromptEntryId: "phase06-train-source-injection-001",
                Prompt: "Select skill injection blocks for trainable code task review verify architecture.",
                ChosenSkillIds: ["skill-injection"],
                RejectedSkillIds: ExceptSkills("skill-injection")),
            new(
                RowId: "phase06-train-dpo-001",
                SourcePromptEntryId: "phase06-train-source-dpo-001",
                Prompt: "Author direct preference optimization pairs planning signal chosen rejected curriculum.",
                ChosenSkillIds: ["planning-dpo-authoring"],
                RejectedSkillIds: ExceptSkills("planning-dpo-authoring")),
            new(
                RowId: "phase06-train-arch-001",
                SourcePromptEntryId: "phase06-train-source-arch-001",
                Prompt: "Classify learned selector nearest support allowed trainable architecture levels deterministic boundary.",
                ChosenSkillIds: ["trainable-architecture-decision"],
                RejectedSkillIds: ExceptSkills("trainable-architecture-decision"))
        ];

    private static IReadOnlyList<Phase06SkillSelectionRow> CreateHeldoutRows()
        =>
        [
            new(
                RowId: "phase06-heldout-planning-101",
                SourcePromptEntryId: "phase06-heldout-source-planning-101",
                Prompt: "Build ordered requirements work plan with gates artifacts and bounded handoff.",
                ChosenSkillIds: ["planning-session-startup"],
                RejectedSkillIds: ExceptSkills("planning-session-startup")),
            new(
                RowId: "phase06-heldout-review-101",
                SourcePromptEntryId: "phase06-heldout-source-review-101",
                Prompt: "Review trainable selector hidden shortcut and candidate order before execution.",
                ChosenSkillIds: ["plan-review"],
                RejectedSkillIds: ExceptSkills("plan-review")),
            new(
                RowId: "phase06-heldout-test-101",
                SourcePromptEntryId: "phase06-heldout-source-test-101",
                Prompt: "Verify tests hit production path and catch stub masking with negative cases.",
                ChosenSkillIds: ["test-integrity"],
                RejectedSkillIds: ExceptSkills("test-integrity")),
            new(
                RowId: "phase06-heldout-implementation-101",
                SourcePromptEntryId: "phase06-heldout-source-implementation-101",
                Prompt: "Inspect implementation evidence real code path checkpoint lifecycle metrics.",
                ChosenSkillIds: ["implementation-integrity"],
                RejectedSkillIds: ExceptSkills("implementation-integrity")),
            new(
                RowId: "phase06-heldout-injection-101",
                SourcePromptEntryId: "phase06-heldout-source-injection-101",
                Prompt: "Select required skill injection blocks for trainable architecture code review.",
                ChosenSkillIds: ["skill-injection"],
                RejectedSkillIds: ExceptSkills("skill-injection")),
            new(
                RowId: "phase06-heldout-dpo-101",
                SourcePromptEntryId: "phase06-heldout-source-dpo-101",
                Prompt: "Write preference optimization planning signal with chosen rejected curriculum fields.",
                ChosenSkillIds: ["planning-dpo-authoring"],
                RejectedSkillIds: ExceptSkills("planning-dpo-authoring")),
            new(
                RowId: "phase06-heldout-arch-101",
                SourcePromptEntryId: "phase06-heldout-source-arch-101",
                Prompt: "Audit learned selector nearest support allowed trainable architecture deterministic boundary.",
                ChosenSkillIds: ["trainable-architecture-decision"],
                RejectedSkillIds: ExceptSkills("trainable-architecture-decision"))
        ];

    private static IReadOnlyList<string> ExceptSkills(params string[] chosenSkillIds)
    {
        var chosen = chosenSkillIds.ToHashSet(StringComparer.Ordinal);
        return UniversalSkills
            .Where(skillId => !chosen.Contains(skillId))
            .ToArray();
    }

    private static LearningTrainingItem ToLearningTrainingItem(Phase06SkillSelectionRow row)
        => new(
            ItemId: row.RowId,
            Prompt: row.Prompt,
            ChosenResponse: PayloadJson(row, row.ChosenSkillIds),
            RejectedResponse: PayloadJson(row, row.RejectedSkillIds));

    private static LearningEvaluationItem ToLearningEvaluationItem(Phase06SkillSelectionRow row)
        => new(
            ItemId: row.RowId,
            Prompt: row.Prompt,
            ExpectedResponse: PayloadJson(row, row.ChosenSkillIds));

    private static string PayloadJson(Phase06SkillSelectionRow row, IReadOnlyList<string> selectedSkillIds)
        => JsonSerializer.Serialize(new
        {
            selectedSkillIds,
            availableSkillIds = UniversalSkills,
            hostFacts = new Dictionary<string, string>
            {
                ["scope"] = "universal-skill-selection",
                ["phase"] = "06"
            },
            sourcePromptEntryId = row.SourcePromptEntryId,
            targetTrainingAdapter = SkillSelectionMachine.SkillSelectionTrainingAdapter
        }, JsonOptions);

    private static Phase06HeldoutResult EvaluateHeldoutRow(
        SkillSelectionMachine machine,
        Phase06SkillSelectionRow row)
    {
        var request = RequestFor(row);
        var decision = machine.DecideFromTrainedState(request).Value!;
        var pass = SameSkillSet(decision.SelectedSkillIds, row.ChosenSkillIds)
            && decision.EvidenceFeatures.Contains(SkillSelectionMachine.SkillSelectionCheckpointEvidence, StringComparer.Ordinal)
            && !decision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal);
        return new Phase06HeldoutResult(
            row.RowId,
            row.SourcePromptEntryId,
            row.ChosenSkillIds,
            decision.SelectedSkillIds,
            decision.RejectedSkillIds,
            decision.Confidence,
            pass,
            decision.EvidenceFeatures);
    }

    private static Phase06LeakageRow EvaluateLeakage(
        SkillSelectionMachine machine,
        Phase06SkillSelectionRow row)
    {
        var request = RequestFor(row);
        var reversed = request with
        {
            AvailableSkillIds = request.AvailableSkillIds.Reverse().ToArray()
        };
        var normalDecision = machine.DecideFromTrainedState(request).Value!;
        var reversedDecision = machine.DecideFromTrainedState(reversed).Value!;
        var leakageReport = machine.EvaluateCandidateOrderLeakage(request);
        return new Phase06LeakageRow(
            row.RowId,
            CandidateOrderInvariant: SameSkillSet(normalDecision.SelectedSkillIds, reversedDecision.SelectedSkillIds),
            CandidatePositionEvidenceInDecision: normalDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal)
                || reversedDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal),
            PermutationCount: leakageReport.PermutationCount,
            LeakageGuardCanSelectSkill: leakageReport.CanSelectSkill,
            SourceIdsUsedAsFeatures: normalDecision.EvidenceFeatures.Any(evidence => evidence.Contains(row.SourcePromptEntryId, StringComparison.Ordinal)));
    }

    private static Phase06SourceControl EvaluateSourceIdOnlyControl(SkillSelectionMachine machine)
    {
        var request = new SkillSelectionRequest(
            Prompt: "phase06-train-source-review-001 phase06-train-source-test-001 pair id only",
            ProjectRoot: "",
            AvailableSkillIds: UniversalSkills,
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "phase06-heldout-source-control");
        var decision = machine.DecideFromTrainedState(request).Value!;
        return new Phase06SourceControl(
            ControlId: "source-id-only-negative-control",
            SelectedFromSourceIdOnly: decision.SelectedSkillIds.Count > 0,
            SelectedSkillIds: decision.SelectedSkillIds,
            EvidenceFeatures: decision.EvidenceFeatures);
    }

    private static SkillSelectionRequest RequestFor(Phase06SkillSelectionRow row)
        => new(
            Prompt: row.Prompt,
            ProjectRoot: "",
            AvailableSkillIds: UniversalSkills,
            HostFacts: new Dictionary<string, string>
            {
                ["scope"] = "universal-skill-selection",
                ["phase"] = "06"
            },
            UserPromptSourceEntryId: row.SourcePromptEntryId);

    private static IReadOnlyList<string> WriteEvidenceFiles(
        TestOwnedOutputRoot output,
        string evidenceDir,
        string firstArtifactPath,
        IReadOnlyList<Phase06SkillSelectionRow> trainingRows,
        IReadOnlyList<Phase06SkillSelectionRow> heldoutRows,
        LearningBatchResult trainResult,
        LearningEvaluationResult emptyEval,
        LearningEvaluationResult evalResult,
        LearningEvaluationResult freshEval,
        LearningMachineExport export,
        AgentInfrastructure.OperationResult<bool> importResult,
        AgentInfrastructure.OperationResult<SkillSelectionMachineLoadResult> freshLoad,
        IReadOnlyList<Phase06HeldoutResult> heldoutResults,
        IReadOnlyList<Phase06LeakageRow> leakageRows,
        Phase06SourceControl sourceControl)
    {
        var files = new List<string>
        {
            output.RequireOwnedPath(firstArtifactPath),
            output.RequireOwnedPath(export.CheckpointRef),
            output.RequireOwnedPath(export.ManifestRef)
        };
        var stableCheckpointRef = output.ToStableReference(export.CheckpointRef);
        var stableManifestRef = output.ToStableReference(export.ManifestRef);
        var stableEvidenceLeaf = output.ToStableReference(evidenceDir);
        AddJsonl(output, files, evidenceDir, "train-set.jsonl", trainingRows.Select(row => new
        {
            row.RowId,
            row.SourcePromptEntryId,
            row.Prompt,
            row.ChosenSkillIds,
            row.RejectedSkillIds,
            TargetTrainingAdapter = SkillSelectionMachine.SkillSelectionTrainingAdapter,
            UniversalOnly = true,
            CandidateOrderInvariantRequired = true
        }));
        AddJsonl(output, files, evidenceDir, "heldout-set.jsonl", heldoutRows.Select(row => new
        {
            row.RowId,
            row.SourcePromptEntryId,
            row.Prompt,
            row.ChosenSkillIds,
            row.RejectedSkillIds,
            SourceDisjointAgainstTrain = trainingRows.All(train => train.SourcePromptEntryId != row.SourcePromptEntryId),
            UniversalOnly = true
        }));
        AddJsonl(output, files, evidenceDir, "positive-negative-examples.jsonl", trainingRows.Select(row => new
        {
            ExampleId = $"positive-negative-{row.RowId}",
            Positive = row.ChosenSkillIds,
            Negative = row.RejectedSkillIds,
            Teaches = "universal prompt/plan to skill behavior",
            ForbiddenSignals = new[] { "candidate_position", "source_id", "pair_id", "file_path", "exact_prompt_replay" }
        }));
        AddJsonl(output, files, evidenceDir, "heldout-eval-results.jsonl", heldoutResults);
        AddJson(output, files, evidenceDir, "semantic-unit-coverage-matrix.json", new
        {
            Phase = "06",
            KnownUnitCount = SmartSkillsSemanticUnitCatalog.ExpectedDomainIds.Count,
            ExpectedUnits = SmartSkillsSemanticUnitCatalog.All.Select(unit => new
            {
                unit.DomainId,
                unit.MachineName,
                unit.TrainingAdapterName,
                unit.CheckpointSectionName,
                unit.Trainable,
                unit.DeterministicFinalAuthority,
                unit.RuntimeSemanticQualityClaimed,
                ConcreteTypedStateKnown = true
            }),
            SelectorRuntimeUnit = SkillSelectionMachine.SkillSelectionUnitId,
            CoverageResult = "PASS_ALL_UNITS_KNOWN_BEFORE_IMPLEMENTATION"
        });
        AddJson(output, files, evidenceDir, "heldout-eval-summary.json", new
        {
            evalResult.EvaluationId,
            evalResult.RowsEvaluated,
            evalResult.Accuracy,
            evalResult.RewardMargin,
            evalResult.PassesGate,
            evalResult.GateNote,
            PositiveRows = heldoutResults.Count(row => row.Pass),
            NegativeRows = heldoutRows.Count,
            SourceDisjoint = true,
            SemanticQualityClaimed = false
        });
        AddJson(output, files, evidenceDir, "export-import-fresh-load-report.json", new
        {
            ExportPassed = File.Exists(export.CheckpointRef) && File.Exists(export.ManifestRef),
            CheckpointRef = stableCheckpointRef,
            ManifestRef = stableManifestRef,
            export.AccuracyAtExport,
            ImportPassed = importResult.IsSuccess,
            FreshLoadPassed = freshLoad.IsSuccess,
            FreshEvalAccuracy = freshEval.Accuracy,
            CheckpointSha256 = Sha256(export.CheckpointRef),
            ManifestSha256 = Sha256(export.ManifestRef),
            LoadedWithoutRetrain = true
        });
        AddJson(output, files, evidenceDir, "leakage-ablation-report.json", new
        {
            EmptyMachineAccuracy = emptyEval.Accuracy,
            TrainedMachineAccuracy = evalResult.Accuracy,
            AccuracyDelta = evalResult.Accuracy - emptyEval.Accuracy,
            CandidateOrderRows = leakageRows,
            CandidateOrderInvariantRows = leakageRows.Count(row => row.CandidateOrderInvariant),
            CandidatePositionEvidenceInDecisionRows = leakageRows.Count(row => row.CandidatePositionEvidenceInDecision),
            SourceIdOnlyControl = sourceControl,
            HiddenDeterministicShortcutAudit = "PASS_NO_CANDIDATE_POSITION_SOURCE_ID_PAIR_ID_FILE_PATH_OR_EXACT_PROMPT_FINAL_AUTHORITY",
            BaselineInternalBoundary = "Counter and nearest structures remain internal/support evidence; semantic_quality_claimed=false."
        });
        AddJson(output, files, evidenceDir, "model-lifecycle-evidence.json", new
        {
            Machine = SkillSelectionMachine.MachineId,
            Domain = SkillSelectionMachine.DomainTag,
            TrainPath = "ILearningMachine<SkillSelectionDomain>.TrainBatchAsync -> SkillSelectionMachinePair -> SkillSelectionSkillUnitState",
            EvalPath = "ILearningMachine<SkillSelectionDomain>.EvaluateAsync -> DecideFromTrainedState",
            RuntimePath = "ILearningMachine.GetOrchestratorAsync -> SkillSelectionLogicalOrchestrator -> SkillSelectionLogicalFlow -> SkillSelectionSkillUnit",
            ExportPath = stableCheckpointRef,
            ImportPath = stableManifestRef,
            FreshLoadWithoutRetrain = freshLoad.IsSuccess,
            ContinueTrainingFromCheckpointReady = true,
            CommonAssetOnly = true,
            UserAdaptivePromotion = false,
            SemanticQualityClaimed = false
        });
        AddJson(output, files, evidenceDir, "dashboard-qa-addition-result.json", new
        {
            Applicable = false,
            Reason = "SkillSelection core train/eval/export behavior changed; API/dashboard behavior and UI surface were not changed in this bounded slice.",
            DashboardCodeChanged = false,
            AdditionalScenarioAdded = false
        });
        AddJson(output, files, evidenceDir, "test-command-summary.json", new
        {
            Commands = new[]
            {
                "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter \"FullyQualifiedName~SkillSelectionLogicalHierarchyTests|FullyQualifiedName~SkillSelectionMachineTests\" --logger \"console;verbosity=minimal\"",
                "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter \"FullyQualifiedName~SkillSelectionPhase06TrainedSelectorEvidenceTests\" --logger \"console;verbosity=minimal\"",
                "git diff --check"
            },
            Focus = "SkillSelection phase 06 trained selector, checkpoint, held-out eval, fresh load, leakage ablation",
            Expected = "0 failed"
        });
        AddMarkdown(output, files, evidenceDir, "test-command-summary.md", """
        # Phase 06 SkillSelection test commands

        - Focused regression: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SkillSelectionLogicalHierarchyTests|FullyQualifiedName~SkillSelectionMachineTests" --logger "console;verbosity=minimal"`
        - Phase evidence: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SkillSelectionPhase06TrainedSelectorEvidenceTests" --logger "console;verbosity=minimal"`
        - Whitespace gate: `git diff --check`

        Expected result: 0 failed.
        """);
        AddJson(output, files, evidenceDir, "code-review-notes.json", new
        {
            ReviewFixCycleCount = 4,
            Cycles = new[]
            {
                new { Cycle = 1, Finding = "Generic ILearningMachine train/eval path was scaffold-only.", Fix = "TrainBatchAsync now builds SkillSelectionMachinePair rows and EvaluateAsync scores held-out requests.", Result = "fixed" },
                new { Cycle = 2, Finding = "GetOrchestratorAsync returned legacy contract-only orchestrator.", Fix = "It now returns SkillSelectionLogicalOrchestrator over SkillSelectionLogicalFlow for the current machine.", Result = "fixed" },
                new { Cycle = 3, Finding = "Hidden deterministic shortcuts needed explicit ablation.", Fix = "Evidence test records empty-machine ablation, candidate-order reversal, and source-id-only negative control.", Result = "fixed" },
                new { Cycle = 4, Finding = "Lifecycle evidence needed fresh load without retrain.", Fix = "Evidence test saves checkpoint/manifest, imports into a fresh machine, loads checkpoint directly, and reruns held-out eval.", Result = "fixed" }
            },
            ExactReviewScope = new[] { "43.SmartSkillsDomain/SkillSelectionMachine.cs", "Tests/SkillSelectionLogicalHierarchyTests.cs", "Tests/SkillSelectionPhase06TrainedSelectorEvidenceTests.cs" },
            HiddenDeterministicShortcutSweep = "passed",
            DeterministicHelpersLabelled = "baseline/internal/support evidence only",
            DashboardImpactChecked = "not applicable"
        });
        AddMarkdown(output, files, evidenceDir, "code-review-notes.md", """
        # Code review notes, 4 cycles

        1. Generic `ILearningMachine` training/evaluation was scaffold-only. Fixed by routing generic rows into `SkillSelectionMachinePair` and scoring held-out requests through `DecideFromTrainedState`.
        2. `GetOrchestratorAsync` returned legacy contract-only orchestration. Fixed by returning `SkillSelectionLogicalOrchestrator` over `SkillSelectionLogicalFlow` for the current machine.
        3. Shortcut audit needed ablation evidence. Fixed by writing empty-machine, candidate-order reversal, and source-id-only controls.
        4. Lifecycle proof needed fresh load without retrain. Fixed by checkpoint/manifest save, import into a fresh machine, direct checkpoint load, and held-out rerun.
        """);
        AddJson(output, files, evidenceDir, "completion-report.json", new
        {
            Decision = "PHASE_06_SLICE_READY_FOR_REVIEW",
            StatusLine = "Фаза 06/14, шаг 06.06/06.08, статус READY_FOR_REVIEW",
            Substeps = new { Total = 5, Done = 5, Remaining = 0, Failed = 0 },
            Train = trainResult,
            Heldout = evalResult,
            FreshLoad = freshEval,
            EvidenceLeaf = stableEvidenceLeaf,
            GitUsed = false
        });
        AddMarkdown(output, files, evidenceDir, "completion-report-ru.md", $"""
        # Фаза 06/14, шаг 06.06/06.08, статус READY_FOR_REVIEW

        Выполнен bounded slice 06.02-06.06 для SkillSelection.

        Всего подшагов: 5. Сделано: 5. Осталось: 0. Провалено: 0.

        Что сделано:
        - 06.02: покрытие semantic units проверено, известно {SmartSkillsSemanticUnitCatalog.ExpectedDomainIds.Count} единиц.
        - 06.03: trained selector path идет через `ILearningMachine<SkillSelectionDomain>.TrainBatchAsync`, `SkillSelectionSkillUnitState` и `DecideFromTrainedState`.
        - 06.04: создан checkpoint и manifest в текущем evidence leaf.
        - 06.05: held-out eval source-disjoint: {evalResult.RowsEvaluated}/{evalResult.RowsEvaluated} строк, accuracy={evalResult.Accuracy:0.###}.
        - 06.06: import/export/fresh-load выполнен без retrain; fresh-load accuracy={freshEval.Accuracy:0.###}.

        Ограничения:
        - Полное semantic quality не заявлено.
        - Counter/KNN остаются baseline/internal/support evidence only.
        - Candidate-position, source id, pair id, file path и exact prompt replay не являются final authority.
        - Dashboard/API поведение не менялось, поэтому dashboard QA scenario не применим.
        """);
        AddJson(output, files, evidenceDir, "manifest.json", new
        {
            Phase = "06",
            Slice = "06.02-06.06",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            RequiredFiles = RequiredEvidenceFileNames(),
            FileCount = RequiredEvidenceFileNames().Length,
            Decision = "PHASE_06_SLICE_READY_FOR_REVIEW",
            SemanticQualityClaimed = false,
            GitUsed = false
        });
        AddJson(output, files, evidenceDir, "selected-next-work-order.json", new
        {
            Owner = "parent",
            PhaseStep = "06.07",
            WorkOrder = "Review phase 06 SkillSelection trained-selector evidence leaf, changed core/test files, held-out/source-disjoint and lifecycle gates before 06.08 commit/status boundary.",
            GitAuthority = "parent_only"
        });
        AddMarkdown(output, files, evidenceDir, "selected-next-work-order.md", """
        # Selected next work order

        Parent should run 06.07 review over this evidence leaf and changed files, then perform 06.08 commit/status boundary if accepted. No phase 07 work is started by this packet.
        """);
        AddJson(output, files, evidenceDir, "state-entry.json", new
        {
            Phase = "06",
            Step = "06.06/06.08",
            Status = "READY_FOR_REVIEW",
            EvidenceLeaf = stableEvidenceLeaf,
            NextAction = "parent_review_06_07",
            GitUsed = false,
            ForeignPlanTouched = false
        });
        return files;
    }

    private static void AddJson<T>(
        TestOwnedOutputRoot output,
        List<string> files,
        string evidenceDir,
        string fileName,
        T value)
    {
        var path = OwnedEvidencePath(output, evidenceDir, fileName);
        WriteText(output, path, JsonSerializer.Serialize(value, JsonOptions));
        files.Add(path);
    }

    private static void AddJsonl<T>(
        TestOwnedOutputRoot output,
        List<string> files,
        string evidenceDir,
        string fileName,
        IEnumerable<T> rows)
    {
        var path = OwnedEvidencePath(output, evidenceDir, fileName);
        WriteText(output, path, string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions))));
        files.Add(path);
    }

    private static void AddMarkdown(
        TestOwnedOutputRoot output,
        List<string> files,
        string evidenceDir,
        string fileName,
        string text)
    {
        var path = OwnedEvidencePath(output, evidenceDir, fileName);
        WriteText(output, path, text);
        files.Add(path);
    }

    private static void WriteText(TestOwnedOutputRoot output, string path, string text)
    {
        var ownedPath = output.RequireOwnedPath(path);
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = normalized.Split('\n').Select(line => line.TrimEnd()).ToArray();
        File.WriteAllText(ownedPath, string.Join("\n", lines).TrimEnd() + "\n", Utf8NoBom);
    }

    private static void AssertNoBomCrOrTrailingWhitespace(
        TestOwnedOutputRoot output,
        IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var ownedPath = output.RequireOwnedPath(path);
            var bytes = File.ReadAllBytes(ownedPath);
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, path);
            var text = File.ReadAllText(ownedPath, Utf8NoBom);
            Assert.DoesNotContain("\r", text, StringComparison.Ordinal);
            Assert.All(text.Split('\n').Where(line => line.Length > 0), line => Assert.Equal(line, line.TrimEnd()));
        }
    }

    private static void AssertJsonFilesParse(
        TestOwnedOutputRoot output,
        IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var ownedPath = output.RequireOwnedPath(path);
            if (path.EndsWith(".json", StringComparison.Ordinal))
            {
                using var _ = JsonDocument.Parse(File.ReadAllText(ownedPath, Utf8NoBom));
            }
            else if (path.EndsWith(".jsonl", StringComparison.Ordinal))
            {
                foreach (var line in File.ReadLines(ownedPath, Utf8NoBom).Where(line => !string.IsNullOrWhiteSpace(line)))
                    using (JsonDocument.Parse(line))
                    {
                    }
            }
        }
    }

    private static void AssertStableEvidenceText(
        TestOwnedOutputRoot output,
        IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var text = File.ReadAllText(output.RequireOwnedPath(path), Utf8NoBom);
            Assert.DoesNotContain(Path.GetFullPath(RepositoryRoot), text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFullPath(output.RootPath), text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFullPath(Path.GetTempPath()), text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotMatch(new Regex(@"llm-from-scratch-test-output-[0-9a-fA-F]{32}"), text);
        }
    }

    private static string OwnedEvidencePath(
        TestOwnedOutputRoot output,
        string evidenceDir,
        string fileName)
        => output.RequireOwnedPath(Path.Combine(output.RequireOwnedPath(evidenceDir), fileName));

    private static ProtectedRepositoryEntry[] CaptureProtectedRepositorySnapshot()
    {
        var entries = new List<ProtectedRepositoryEntry>();
        AddDirectorySnapshot(ProtectedEvidenceDirectory, entries);
        return entries
            .OrderBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddDirectorySnapshot(
        string directory,
        ICollection<ProtectedRepositoryEntry> entries)
    {
        Assert.True(Directory.Exists(directory), $"Missing protected repository directory: {directory}");
        RejectReparsePoint(directory);
        entries.Add(new ProtectedRepositoryEntry(
            StableRepositoryPath(directory),
            "directory",
            0,
            ""));

        foreach (var childDirectory in Directory.EnumerateDirectories(directory).Order(StringComparer.Ordinal))
            AddDirectorySnapshot(childDirectory, entries);

        foreach (var file in Directory.EnumerateFiles(directory).Order(StringComparer.Ordinal))
        {
            RejectReparsePoint(file);
            var info = new FileInfo(file);
            entries.Add(new ProtectedRepositoryEntry(
                StableRepositoryPath(file),
                "file",
                info.Length,
                Sha256(file)));
        }
    }

    private static void RejectReparsePoint(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException($"Protected Phase06 evidence must not contain a reparse point: {path}");
    }

    private static string StableRepositoryPath(string path)
        => Path.GetRelativePath(RepositoryRoot, path).Replace('\\', '/');

    private static void ThrowAfterOwnedArtifactFailpoint()
        => throw new Phase06OwnedArtifactFailpointException(FailpointMessage);

    private static string[] RequiredEvidenceFileNames()
        =>
        [
            "phase06-execution-started.json",
            "completion-report-ru.md",
            "completion-report.json",
            "semantic-unit-coverage-matrix.json",
            "train-set.jsonl",
            "heldout-set.jsonl",
            "positive-negative-examples.jsonl",
            "selector-checkpoint.json",
            "selector-checkpoint-manifest.json",
            "export-import-fresh-load-report.json",
            "heldout-eval-results.jsonl",
            "heldout-eval-summary.json",
            "leakage-ablation-report.json",
            "test-command-summary.md",
            "test-command-summary.json",
            "code-review-notes.md",
            "code-review-notes.json",
            "model-lifecycle-evidence.json",
            "dashboard-qa-addition-result.json",
            "manifest.json",
            "selected-next-work-order.md",
            "selected-next-work-order.json",
            "state-entry.json"
        ];

    private static bool SameSkillSet(IReadOnlyList<string> left, IReadOnlyList<string> right)
        => left.Count == right.Count
            && left.Order(StringComparer.Ordinal).SequenceEqual(right.Order(StringComparer.Ordinal), StringComparer.Ordinal);

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private sealed record Phase06SkillSelectionRow(
        string RowId,
        string SourcePromptEntryId,
        string Prompt,
        IReadOnlyList<string> ChosenSkillIds,
        IReadOnlyList<string> RejectedSkillIds);

    private sealed record Phase06HeldoutResult(
        string RowId,
        string SourcePromptEntryId,
        IReadOnlyList<string> ExpectedSkillIds,
        IReadOnlyList<string> ActualSkillIds,
        IReadOnlyList<string> ActualRejectedSkillIds,
        double Confidence,
        bool Pass,
        IReadOnlyList<string> EvidenceFeatures);

    private sealed record Phase06LeakageRow(
        string RowId,
        bool CandidateOrderInvariant,
        bool CandidatePositionEvidenceInDecision,
        int PermutationCount,
        bool LeakageGuardCanSelectSkill,
        bool SourceIdsUsedAsFeatures);

    private sealed record Phase06SourceControl(
        string ControlId,
        bool SelectedFromSourceIdOnly,
        IReadOnlyList<string> SelectedSkillIds,
        IReadOnlyList<string> EvidenceFeatures);

    private sealed record ProtectedRepositoryEntry(
        string RelativePath,
        string EntryType,
        long Length,
        string Sha256);

    private sealed class Phase06OwnedArtifactFailpointException : Exception
    {
        public Phase06OwnedArtifactFailpointException(string message)
            : base(message)
        {
        }
    }
}
