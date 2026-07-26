// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class SkillSelectionMachine :
    ILearningMachine<SkillSelectionDomain>,
    IMachineTraining<SkillSelectionDomain>
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public const string MachineId = "skill-selection-machine";
    public const string DomainTag = "SmartSkillsDomain.SkillSelection";
    public const string Version = "1.0.0";
    public const string SemanticPipelineBoundaryEvidence = "SEMANTIC_SKILL_SELECTION_PIPELINE_BOUNDARY";
    public const string CandidatePositionScaffoldEvidence = "CANDIDATE_POSITION_WEIGHT_SCAFFOLD_ONLY";
    public const string PromptUnderstandingUnitId = "prompt_understanding";
    public const string PromptUnderstandingCheckpointEvidence = "PROMPT_UNDERSTANDING_CHECKPOINT_SECTION";
    public const string PromptUnderstandingTrainingAdapter = "IPromptUnderstandingTraining";
    public const string TaskUnderstandingUnitId = "task_understanding";
    public const string TaskUnderstandingCheckpointEvidence = "TASK_UNDERSTANDING_CHECKPOINT_SECTION";
    public const string TaskUnderstandingTrainingAdapter = "ITaskUnderstandingTraining";
    public const string TaskTypeUnitId = "task_type";
    public const string TaskTypeCheckpointEvidence = "TASK_TYPE_CHECKPOINT_SECTION";
    public const string TaskTypeTrainingAdapter = "ITaskTypeTraining";
    public const string RoutingUnitId = "routing";
    public const string RoutingCheckpointEvidence = "ROUTING_CHECKPOINT_SECTION";
    public const string RoutingTrainingAdapter = "IRoutingTraining";
    public const string SkillCandidatesUnitId = "skill_candidates";
    public const string SkillCandidatesCheckpointEvidence = "SKILL_CANDIDATES_CHECKPOINT_SECTION";
    public const string SkillCandidatesTrainingAdapter = "ISkillCandidateTraining";
    public const string SkillSelectionUnitId = "skill_selection";
    public const string SkillSelectionCheckpointEvidence = "SKILL_SELECTION_CHECKPOINT_SECTION";
    public const string SkillSelectionTrainingAdapter = "ISkillSelectionTraining";
    public const string PromptContextUnitId = "prompt_context";
    public const string PromptContextCheckpointEvidence = "PROMPT_CONTEXT_CHECKPOINT_SECTION";
    public const string PromptContextTrainingAdapter = "IPromptContextTraining";
    public const string ResultGapUnitId = "result_gap";
    public const string ResultGapCheckpointEvidence = "RESULT_GAP_CHECKPOINT_SECTION";
    public const string ResultGapTrainingAdapter = "IResultGapTraining";
    public const string OutputValidationUnitId = "output_validation";
    public const string OutputValidationCheckpointEvidence = "OUTPUT_VALIDATION_CHECKPOINT_SECTION";
    public const string OutputValidationTrainingAdapter = "IOutputValidationTraining";
    public const string TaskCompletionUnitId = "task_completion";
    public const string TaskCompletionCheckpointEvidence = "TASK_COMPLETION_CHECKPOINT_SECTION";
    public const string TaskCompletionTrainingAdapter = "ITaskCompletionTraining";
    public const string IntermediateChecksUnitId = "intermediate_checks";
    public const string IntermediateChecksCheckpointEvidence = "INTERMEDIATE_CHECKS_CHECKPOINT_SECTION";
    public const string IntermediateChecksTrainingAdapter = "IIntermediateCheckTraining";

    private const string CheckpointFileName = "skill-selection-checkpoint.json";
    private const string ManifestFileName = "skill-selection-manifest.json";
    private const string RouteMode = "candidate_position_scaffold_not_final_semantic_authority";
    private const double EvaluationAccuracyGateThreshold = 0.8;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly Dictionary<int, int> _candidatePositionWeights = new();
    private readonly PromptUnderstandingSkillTrainingState _promptUnderstandingSkillState = new();
    private readonly TaskUnderstandingSkillTrainingState _taskUnderstandingSkillState = new();
    private readonly TaskTypeSkillTrainingState _taskTypeSkillState = new();
    private readonly RoutingSkillTrainingState _routingSkillState = new();
    private readonly SkillCandidatesSkillTrainingState _skillCandidatesSkillState = new();
    private readonly SkillSelectionSkillTrainingState _skillSelectionSkillState = new();
    private readonly PromptContextSkillTrainingState _promptContextSkillState = new();
    private readonly ResultGapSkillTrainingState _resultGapSkillState = new();
    private readonly OutputValidationSkillTrainingState _outputValidationSkillState = new();
    private readonly TaskCompletionSkillTrainingState _taskCompletionSkillState = new();
    private readonly IntermediateChecksSkillTrainingState _intermediateChecksSkillState = new();
    private readonly HashSet<string> _trainedPairIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _trainedPromptKeys = new(StringComparer.Ordinal);
    private readonly string? _persistenceDirectory;
    private double _lastEvaluationAccuracy;

    public SkillSelectionMachine(string? persistenceDirectory = null)
    {
        _persistenceDirectory = string.IsNullOrWhiteSpace(persistenceDirectory)
            ? null
            : persistenceDirectory;
    }

    public IMachineTraining<SkillSelectionDomain> TrainingAdapter => this;

    public Task<OperationResult<SkillSelectionTrainingResult>> TrainMachineBatchAsync(
        IReadOnlyList<SkillSelectionMachinePair> pairs,
        CancellationToken ct = default)
    {
        foreach (var pair in pairs)
            LearnPair(pair);

        var result = new SkillSelectionTrainingResult(
            BatchId: $"skill-selection-contract-batch-{pairs.Count}",
            PairCount: pairs.Count,
            ContractOnly: false,
            GateNote: "Candidate-position scaffold weights updated from SkillSelectionMachinePair DPO rows; SS-01 forbids using them as final semantic authority.");

        return Task.FromResult(OperationResult<SkillSelectionTrainingResult>.Ok(result));
    }

    public OperationResult<SkillSelectionDecision> DecideFromTrainedState(SkillSelectionRequest request)
    {
        var semanticDecision = TryDecideWithSemanticUnitsOnly(request);
        if (semanticDecision is not null)
            return OperationResult<SkillSelectionDecision>.Ok(semanticDecision);

        if (_candidatePositionWeights.Count == 0 || request.AvailableSkillIds.Count == 0)
        {
            return OperationResult<SkillSelectionDecision>.Ok(new SkillSelectionDecision(
                SelectedSkillIds: [],
                RejectedSkillIds: request.AvailableSkillIds,
                EvidenceFeatures:
                [
                    "NO_TRAINED_STATE_AVAILABLE",
                    SemanticPipelineBoundaryEvidence
                ],
                Confidence: 0.0,
                RequiresSecondPass: true,
                DecisionText: "No trained SkillSelection state is available."));
        }

        var leakageReport = EvaluateCandidateOrderLeakage(request);
        return OperationResult<SkillSelectionDecision>.Ok(new SkillSelectionDecision(
            SelectedSkillIds: [],
            RejectedSkillIds: request.AvailableSkillIds,
            EvidenceFeatures: new[]
                {
                    SemanticPipelineBoundaryEvidence,
                    "SS_02_TO_SS_12_REQUIRED_FOR_FINAL_AUTHORITY"
                }
                .Concat(leakageReport.EvidenceFeatures)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "Candidate-position weights are diagnostics and leakage-guard evidence only; final semantic skill selection requires the SS-02..SS-12 semantic pipeline."));
    }

    public CandidateOrderLeakageReport EvaluateCandidateOrderLeakage(SkillSelectionRequest request)
        => new CandidateOrderLeakageGuardUnit(
            ExportCandidateOrderLeakageGuardState(),
            ProbeWithoutCandidatePositionAuthority).Evaluate(request);

    public SkillSelectionDecision ProbeWithoutCandidatePositionAuthority(SkillSelectionRequest request)
        => TryDecideWithSemanticUnitsOnly(request) ?? new SkillSelectionDecision(
            SelectedSkillIds: [],
            RejectedSkillIds: request.AvailableSkillIds,
            EvidenceFeatures:
            [
                SemanticPipelineBoundaryEvidence,
                "CANDIDATE_POSITION_FREE_SEMANTIC_PROBE",
                "NO_SEMANTIC_UNIT_DECISION_AVAILABLE"
            ],
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "Candidate-position-free semantic probe found no trained semantic unit decision.");

    public CandidateOrderLeakageGuardState ExportCandidateOrderLeakageGuardState()
        => new(
            UnitId: CandidateOrderLeakageGuardUnit.UnitId,
            SchemaVersion: 1,
            CandidatePositionWeights: _candidatePositionWeights
                .OrderBy(item => item.Key)
                .ToDictionary(item => item.Key, item => item.Value),
            SemanticQualityClaimed: false);

    private SkillSelectionDecision? TryDecideWithSemanticUnitsOnly(SkillSelectionRequest request)
    {
        var skillSelectionDecision = TryDecideWithSkillSelection(request);
        if (skillSelectionDecision is not null)
            return skillSelectionDecision;

        var promptContextDecision = TryDecideWithPromptContext(request);
        if (promptContextDecision is not null)
            return promptContextDecision;

        var resultGapDecision = TryDecideWithResultGap(request);
        if (resultGapDecision is not null)
            return resultGapDecision;

        var outputValidationDecision = TryDecideWithOutputValidation(request);
        if (outputValidationDecision is not null)
            return outputValidationDecision;

        var taskCompletionDecision = TryDecideWithTaskCompletion(request);
        if (taskCompletionDecision is not null)
            return taskCompletionDecision;

        var intermediateChecksDecision = TryDecideWithIntermediateChecks(request);
        if (intermediateChecksDecision is not null)
            return intermediateChecksDecision;

        var skillCandidatesDecision = TryDecideWithSkillCandidates(request);
        if (skillCandidatesDecision is not null)
            return skillCandidatesDecision;

        var routingDecision = TryDecideWithRouting(request);
        if (routingDecision is not null)
            return routingDecision;

        var taskTypeDecision = TryDecideWithTaskType(request);
        if (taskTypeDecision is not null)
            return taskTypeDecision;

        var taskUnderstandingDecision = TryDecideWithTaskUnderstanding(request);
        if (taskUnderstandingDecision is not null)
            return taskUnderstandingDecision;

        return TryDecideWithPromptUnderstanding(request);
    }

    public SkillSelectionCheckpoint ExportCheckpoint()
        => new(
            SchemaVersion: 1,
            CandidatePositionWeights: _candidatePositionWeights
                .OrderBy(item => item.Key)
                .ToDictionary(item => item.Key, item => item.Value),
            TrainedPairIds: _trainedPairIds.Order(StringComparer.Ordinal).ToArray(),
            TrainedPromptKeys: _trainedPromptKeys.Order(StringComparer.Ordinal).ToArray(),
            PromptUnderstanding: ExportPromptUnderstandingState(),
            TaskUnderstanding: ExportTaskUnderstandingState(),
            TaskType: ExportTaskTypeState(),
            Routing: ExportRoutingState(),
            SkillCandidates: ExportSkillCandidatesState(),
            SkillSelection: ExportSkillSelectionState(),
            PromptContext: ExportPromptContextState(),
            RouteMode: RouteMode,
            SavedAtUtc: DateTimeOffset.UtcNow)
        {
            ResultGap = ExportResultGapState(),
            OutputValidation = ExportOutputValidationState(),
            TaskCompletion = ExportTaskCompletionState(),
            IntermediateChecks = ExportIntermediateChecksState()
        };

    public void LoadCheckpoint(SkillSelectionCheckpoint checkpoint)
    {
        _candidatePositionWeights.Clear();
        _promptUnderstandingSkillState.Clear();
        _taskUnderstandingSkillState.Clear();
        _taskTypeSkillState.Clear();
        _routingSkillState.Clear();
        _skillCandidatesSkillState.Clear();
        _skillSelectionSkillState.Clear();
        _promptContextSkillState.Clear();
        _resultGapSkillState.Clear();
        _outputValidationSkillState.Clear();
        _taskCompletionSkillState.Clear();
        _intermediateChecksSkillState.Clear();
        _trainedPairIds.Clear();
        _trainedPromptKeys.Clear();

        foreach (var weight in checkpoint.CandidatePositionWeights)
            _candidatePositionWeights[weight.Key] = weight.Value;

        foreach (var pairId in checkpoint.TrainedPairIds)
            _trainedPairIds.Add(pairId);

        foreach (var promptKey in checkpoint.TrainedPromptKeys)
            _trainedPromptKeys.Add(promptKey);

        if (checkpoint.PromptUnderstanding?.SkillProfiles is not null)
            _promptUnderstandingSkillState.LoadProfiles(checkpoint.PromptUnderstanding.SkillProfiles);

        if (checkpoint.TaskUnderstanding?.SkillProfiles is not null)
            _taskUnderstandingSkillState.LoadProfiles(checkpoint.TaskUnderstanding.SkillProfiles);

        if (checkpoint.TaskType?.SkillProfiles is not null)
            _taskTypeSkillState.LoadProfiles(checkpoint.TaskType.SkillProfiles);

        if (checkpoint.Routing?.SkillProfiles is not null)
            _routingSkillState.LoadProfiles(checkpoint.Routing.SkillProfiles);

        if (checkpoint.SkillCandidates?.SkillProfiles is not null)
            _skillCandidatesSkillState.LoadProfiles(checkpoint.SkillCandidates.SkillProfiles);

        if (checkpoint.SkillSelection?.SkillProfiles is not null)
            _skillSelectionSkillState.LoadProfiles(checkpoint.SkillSelection.SkillProfiles);

        if (checkpoint.PromptContext?.SkillProfiles is not null)
            _promptContextSkillState.LoadProfiles(checkpoint.PromptContext.SkillProfiles);

        if (checkpoint.ResultGap?.SkillProfiles is not null)
            _resultGapSkillState.LoadProfiles(checkpoint.ResultGap.SkillProfiles);

        if (checkpoint.OutputValidation?.SkillProfiles is not null)
            _outputValidationSkillState.LoadProfiles(checkpoint.OutputValidation.SkillProfiles);

        if (checkpoint.TaskCompletion?.SkillProfiles is not null)
            _taskCompletionSkillState.LoadProfiles(checkpoint.TaskCompletion.SkillProfiles);

        if (checkpoint.IntermediateChecks?.SkillProfiles is not null)
            _intermediateChecksSkillState.LoadProfiles(checkpoint.IntermediateChecks.SkillProfiles);
    }

    public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<SkillSelectionDomain> batch,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Training cancelled.", "CANCELLED"));
        if (batch is null)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Training batch is required.", "BATCH_REQUIRED"));
        if (!string.Equals(batch.DomainTag, DomainTag, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                "Training batch domain tag does not match SkillSelection.",
                "SKILL_SELECTION_DOMAIN_TAG_MISMATCH"));

        var sw = Stopwatch.StartNew();
        var accuracyBefore = EvaluateLearningItemsAccuracy(batch.TrainingItems);
        var pairResult = BuildMachinePairs(batch.TrainingItems);
        if (!pairResult.IsSuccess)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(pairResult.ErrorMessage, pairResult.ErrorCode));

        foreach (var pair in pairResult.Value!)
            LearnPair(pair);

        var accuracyAfter = EvaluateLearningItemsAccuracy(batch.TrainingItems);
        sw.Stop();

        var result = new LearningBatchResult(
            BatchId: batch.BatchId,
            RowsTrained: pairResult.Value!.Count,
            AccuracyBefore: accuracyBefore,
            AccuracyAfter: accuracyAfter,
            RewardMargin: accuracyAfter - accuracyBefore,
            CpuMs: sw.ElapsedMilliseconds,
            PeakRamMb: GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024),
            ElapsedMs: sw.ElapsedMilliseconds,
            PromotedUnitIds: pairResult.Value
                .Select(pair => MapTrainingAdapterToUnitId(pair.TargetTrainingAdapter))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());

        return Task.FromResult(OperationResult<LearningBatchResult>.Ok(result));
    }

    public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<SkillSelectionDomain> evalSet,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail("Evaluation cancelled.", "CANCELLED"));
        if (evalSet is null)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail("Evaluation set is required.", "EVAL_SET_REQUIRED"));
        if (!string.Equals(evalSet.DomainTag, DomainTag, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(
                "Evaluation set domain tag does not match SkillSelection.",
                "SKILL_SELECTION_EVAL_DOMAIN_TAG_MISMATCH"));
        if (evalSet.EvaluationItems.Count == 0)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(
                "Evaluation set must contain held-out rows.",
                "SKILL_SELECTION_EVAL_SET_EMPTY"));

        var correct = 0;
        var confidenceSum = 0.0;
        foreach (var item in evalSet.EvaluationItems)
        {
            var expected = BuildEvaluationExpectation(item);
            if (!expected.IsSuccess)
                return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(expected.ErrorMessage, expected.ErrorCode));

            var decision = DecideFromTrainedState(expected.Value!.Request);
            if (!decision.IsSuccess)
                return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(decision.ErrorMessage, decision.ErrorCode));

            var isCorrect = SameSkillSet(decision.Value!.SelectedSkillIds, expected.Value.ExpectedSkillIds);
            if (isCorrect)
                correct++;

            confidenceSum += isCorrect ? decision.Value.Confidence : -decision.Value.Confidence;
        }

        var accuracy = correct / (double)evalSet.EvaluationItems.Count;
        _lastEvaluationAccuracy = accuracy;
        var passesGate = accuracy >= EvaluationAccuracyGateThreshold;
        var result = new LearningEvaluationResult(
            EvaluationId: evalSet.EvaluationId,
            RowsEvaluated: evalSet.EvaluationItems.Count,
            Accuracy: accuracy,
            RewardMargin: confidenceSum / evalSet.EvaluationItems.Count,
            PassesGate: passesGate,
            GateNote: passesGate
                ? $"SkillSelection held-out gate passed: accuracy={accuracy:F4} >= {EvaluationAccuracyGateThreshold:F4}; semantic_quality_claimed=false."
                : $"SkillSelection held-out gate failed: accuracy={accuracy:F4} < {EvaluationAccuracyGateThreshold:F4}; semantic_quality_claimed=false.");

        return Task.FromResult(OperationResult<LearningEvaluationResult>.Ok(result));
    }

    public Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_persistenceDirectory))
        {
            return Task.FromResult(OperationResult<LearningMachineExport>.Fail(
                "SkillSelection export requires a persistence directory so CheckpointRef and ManifestRef point to stored files.",
                "SKILL_SELECTION_EXPORT_DIRECTORY_REQUIRED"));
        }

        return SaveCheckpointAsync(
            Path.Combine(_persistenceDirectory, CheckpointFileName),
            Path.Combine(_persistenceDirectory, ManifestFileName),
            ct);
    }

    public async Task<OperationResult<bool>> ImportAsync(LearningMachineExport export, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<bool>.Fail("Import cancelled.", "CANCELLED");
        if (export is null)
            return OperationResult<bool>.Fail("Export is required.", "EXPORT_REQUIRED");
        if (!string.Equals(export.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Machine id mismatch.", "MACHINE_ID_MISMATCH");
        if (!string.Equals(export.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Domain tag mismatch.", "DOMAIN_TAG_MISMATCH");
        if (string.IsNullOrWhiteSpace(export.CheckpointRef))
            return OperationResult<bool>.Fail("CheckpointRef is required.", "CHECKPOINT_REF_REQUIRED");
        if (string.IsNullOrWhiteSpace(export.ManifestRef))
            return OperationResult<bool>.Fail("ManifestRef is required.", "MANIFEST_REF_REQUIRED");

        var checkpointPath = Path.GetFullPath(export.CheckpointRef);
        var manifestPath = Path.GetFullPath(export.ManifestRef);
        if (!File.Exists(checkpointPath))
            return OperationResult<bool>.Fail("Checkpoint file not found.", "CHECKPOINT_NOT_FOUND");
        if (!File.Exists(manifestPath))
            return OperationResult<bool>.Fail("Manifest file not found.", "MANIFEST_NOT_FOUND");

        var manifestResult = await ReadManifestAsync(manifestPath, ct);
        if (!manifestResult.IsSuccess)
            return OperationResult<bool>.Fail(manifestResult.ErrorMessage, manifestResult.ErrorCode);

        var manifestValidation = ValidateManifest(manifestResult.Value!, export, checkpointPath, manifestPath);
        if (!manifestValidation.IsSuccess)
            return manifestValidation;

        var checkpointResult = await ReadCheckpointAsync(checkpointPath, ct);
        if (!checkpointResult.IsSuccess)
            return OperationResult<bool>.Fail(checkpointResult.ErrorMessage, checkpointResult.ErrorCode);

        LoadCheckpoint(checkpointResult.Value!);
        return OperationResult<bool>.Ok(true);
    }

    public async Task<OperationResult<LearningMachineExport>> SaveCheckpointAsync(
        string checkpointPath,
        string? manifestPath = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<LearningMachineExport>.Fail("Checkpoint path is required.", "CHECKPOINT_PATH_REQUIRED");

        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        var fullManifestPath = Path.GetFullPath(string.IsNullOrWhiteSpace(manifestPath)
            ? Path.Combine(Path.GetDirectoryName(fullCheckpointPath) ?? "", ManifestFileName)
            : manifestPath);
        if (string.Equals(fullCheckpointPath, fullManifestPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<LearningMachineExport>.Fail("Manifest path must be separate from checkpoint path.", "MANIFEST_PATH_MUST_BE_SEPARATE");

        var exportedAtUtc = DateTimeOffset.UtcNow;
        var checkpoint = ExportCheckpoint();
        var accuracyAtExport = _lastEvaluationAccuracy;
        var manifestDirectory = Path.GetDirectoryName(fullManifestPath) ?? string.Empty;
        var export = new LearningMachineExport(
            MachineId,
            DomainTag,
            Version,
            fullCheckpointPath,
            fullManifestPath,
            accuracyAtExport,
            exportedAtUtc);
        var manifest = new SkillSelectionCheckpointManifest(
            SchemaVersion: 1,
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: Version,
            CheckpointRef: CreatePortableManifestReference(manifestDirectory, fullCheckpointPath),
            ManifestRef: CreatePortableManifestReference(manifestDirectory, fullManifestPath),
            CheckpointSchemaVersion: checkpoint.SchemaVersion,
            RouteMode: checkpoint.RouteMode,
            AccuracyAtExport: accuracyAtExport,
            SemanticQualityClaimed: false,
            ExportedAtUtc: exportedAtUtc);

        try
        {
            EnsureDirectoryFor(fullCheckpointPath);
            EnsureDirectoryFor(fullManifestPath);
            await WriteJsonAtomicallyAsync(fullCheckpointPath, checkpoint, ct);
            await WriteJsonAtomicallyAsync(fullManifestPath, manifest, ct);
        }
        catch (Exception ex)
        {
            return OperationResult<LearningMachineExport>.Fail(ex.Message, "SKILL_SELECTION_CHECKPOINT_WRITE_FAILED");
        }

        return OperationResult<LearningMachineExport>.Ok(export);
    }

    public static async Task<OperationResult<SkillSelectionMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        CancellationToken ct = default)
    {
        var checkpointResult = await ReadCheckpointAsync(checkpointPath, ct);
        if (!checkpointResult.IsSuccess)
            return OperationResult<SkillSelectionMachineLoadResult>.Fail(checkpointResult.ErrorMessage, checkpointResult.ErrorCode);

        var machine = new SkillSelectionMachine();
        machine.LoadCheckpoint(checkpointResult.Value!);
        return OperationResult<SkillSelectionMachineLoadResult>.Ok(new SkillSelectionMachineLoadResult(
            machine,
            checkpointResult.Value!,
            Path.GetFullPath(checkpointPath)));
    }

    public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(CancellationToken ct = default)
        => Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(
            new SkillSelectionLogicalOrchestrator(new SkillSelectionLogicalFlow(this))));

    private OperationResult<IReadOnlyList<SkillSelectionMachinePair>> BuildMachinePairs(
        IReadOnlyList<LearningTrainingItem> trainingItems)
    {
        var pairs = new List<SkillSelectionMachinePair>();
        foreach (var item in trainingItems)
        {
            var pair = BuildMachinePair(item);
            if (!pair.IsSuccess)
                return OperationResult<IReadOnlyList<SkillSelectionMachinePair>>.Fail(pair.ErrorMessage, pair.ErrorCode);

            pairs.Add(pair.Value!);
        }

        return OperationResult<IReadOnlyList<SkillSelectionMachinePair>>.Ok(pairs);
    }

    private OperationResult<SkillSelectionMachinePair> BuildMachinePair(LearningTrainingItem item)
    {
        var chosenPayload = ParseSkillPayload(item.ChosenResponse);
        var rejectedPayload = ParseSkillPayload(item.RejectedResponse);
        var chosenSkillIds = ResolveSelectedSkillIds(chosenPayload, item.ChosenResponse);
        var rejectedSelectedSkillIds = ResolveSelectedSkillIds(rejectedPayload, item.RejectedResponse);
        var availableSkillIds = ResolveAvailableSkillIds(chosenPayload, rejectedPayload, chosenSkillIds, rejectedSelectedSkillIds);
        if (chosenSkillIds.Count == 0)
            return OperationResult<SkillSelectionMachinePair>.Fail("Chosen skill ids are required.", "SKILL_SELECTION_CHOSEN_SKILLS_REQUIRED");
        if (availableSkillIds.Count == 0)
            return OperationResult<SkillSelectionMachinePair>.Fail("Available skill ids are required.", "SKILL_SELECTION_AVAILABLE_SKILLS_REQUIRED");

        var sourcePromptEntryId = FirstNonEmpty(chosenPayload?.SourcePromptEntryId, rejectedPayload?.SourcePromptEntryId, item.ItemId);
        var targetTrainingAdapter = FirstNonEmpty(chosenPayload?.TargetTrainingAdapter, rejectedPayload?.TargetTrainingAdapter, SkillSelectionTrainingAdapter);
        var request = new SkillSelectionRequest(
            Prompt: item.Prompt,
            ProjectRoot: "",
            AvailableSkillIds: availableSkillIds,
            HostFacts: chosenPayload?.HostFacts ?? rejectedPayload?.HostFacts ?? new Dictionary<string, string>(),
            UserPromptSourceEntryId: sourcePromptEntryId);
        var chosen = new SkillSelectionDecision(
            SelectedSkillIds: chosenSkillIds,
            RejectedSkillIds: availableSkillIds
                .Where(skillId => !chosenSkillIds.Contains(skillId, StringComparer.Ordinal))
                .ToArray(),
            EvidenceFeatures: ["ILMACHINE_GENERIC_TRAINING_CHOSEN"],
            Confidence: 1.0,
            RequiresSecondPass: false,
            DecisionText: "Generic ILearningMachine SkillSelection DPO row chose these universal skills.");
        var rejected = new SkillSelectionDecision(
            SelectedSkillIds: rejectedSelectedSkillIds,
            RejectedSkillIds: availableSkillIds
                .Where(skillId => !rejectedSelectedSkillIds.Contains(skillId, StringComparer.Ordinal))
                .ToArray(),
            EvidenceFeatures: ["ILMACHINE_GENERIC_TRAINING_REJECTED"],
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "Generic ILearningMachine SkillSelection DPO row rejected these skills.");

        return OperationResult<SkillSelectionMachinePair>.Ok(new SkillSelectionMachinePair(
            PairId: item.ItemId,
            Request: request,
            Chosen: chosen,
            Rejected: rejected,
            RejectedType: "generic_ilmmachine_skill_selection_dpo",
            SourcePromptEntryId: sourcePromptEntryId,
            TargetTrainingAdapter: targetTrainingAdapter));
    }

    private double EvaluateLearningItemsAccuracy(IReadOnlyList<LearningTrainingItem> trainingItems)
    {
        if (trainingItems.Count == 0)
            return 0.0;

        var correct = 0;
        foreach (var item in trainingItems)
        {
            var expectation = BuildTrainingExpectation(item);
            if (!expectation.IsSuccess)
                continue;

            var decision = DecideFromTrainedState(expectation.Value!.Request);
            if (decision.IsSuccess && SameSkillSet(decision.Value!.SelectedSkillIds, expectation.Value.ExpectedSkillIds))
                correct++;
        }

        return correct / (double)trainingItems.Count;
    }

    private OperationResult<SkillSelectionEvaluationExpectation> BuildTrainingExpectation(LearningTrainingItem item)
    {
        var pair = BuildMachinePair(item);
        if (!pair.IsSuccess)
            return OperationResult<SkillSelectionEvaluationExpectation>.Fail(pair.ErrorMessage, pair.ErrorCode);

        return OperationResult<SkillSelectionEvaluationExpectation>.Ok(new SkillSelectionEvaluationExpectation(
            pair.Value!.Request,
            pair.Value.Chosen.SelectedSkillIds));
    }

    private OperationResult<SkillSelectionEvaluationExpectation> BuildEvaluationExpectation(LearningEvaluationItem item)
    {
        var payload = ParseSkillPayload(item.ExpectedResponse);
        var expectedSkillIds = ResolveSelectedSkillIds(payload, item.ExpectedResponse);
        var availableSkillIds = ResolveAvailableSkillIds(payload, null, expectedSkillIds, []);
        if (expectedSkillIds.Count == 0)
            return OperationResult<SkillSelectionEvaluationExpectation>.Fail("Expected skill ids are required.", "SKILL_SELECTION_EXPECTED_SKILLS_REQUIRED");
        if (availableSkillIds.Count == 0)
            return OperationResult<SkillSelectionEvaluationExpectation>.Fail("Evaluation available skill ids are required.", "SKILL_SELECTION_EVAL_AVAILABLE_SKILLS_REQUIRED");

        var request = new SkillSelectionRequest(
            Prompt: item.Prompt,
            ProjectRoot: "",
            AvailableSkillIds: availableSkillIds,
            HostFacts: payload?.HostFacts ?? new Dictionary<string, string>(),
            UserPromptSourceEntryId: FirstNonEmpty(payload?.SourcePromptEntryId, item.ItemId));

        return OperationResult<SkillSelectionEvaluationExpectation>.Ok(new SkillSelectionEvaluationExpectation(
            request,
            expectedSkillIds));
    }

    private static SkillSelectionPayload? ParseSkillPayload(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.TrimStart().StartsWith("{", StringComparison.Ordinal))
            return null;

        try
        {
            return JsonSerializer.Deserialize<SkillSelectionPayload>(value, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static IReadOnlyList<string> ResolveSelectedSkillIds(SkillSelectionPayload? payload, string fallback)
        => NormalizeSkillIds(payload?.SelectedSkillIds ?? SplitSkillIds(fallback));

    private static IReadOnlyList<string> ResolveAvailableSkillIds(
        SkillSelectionPayload? firstPayload,
        SkillSelectionPayload? secondPayload,
        IReadOnlyList<string> chosenSkillIds,
        IReadOnlyList<string> rejectedSkillIds)
    {
        var payloadAvailable = firstPayload?.AvailableSkillIds ?? secondPayload?.AvailableSkillIds;
        var available = payloadAvailable is { Count: > 0 }
            ? payloadAvailable
            : chosenSkillIds.Concat(rejectedSkillIds).ToArray();
        return NormalizeSkillIds(available);
    }

    private static IReadOnlyList<string> NormalizeSkillIds(IEnumerable<string> skillIds)
        => skillIds
            .Where(skillId => !string.IsNullOrWhiteSpace(skillId))
            .Select(skillId => skillId.Trim())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> SplitSkillIds(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.TrimStart().StartsWith("{", StringComparison.Ordinal))
            return [];

        return value.Split(
                [',', ';', '\n', '\r', '|'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    private static bool SameSkillSet(IReadOnlyList<string> left, IReadOnlyList<string> right)
        => left.Count == right.Count
            && left.Order(StringComparer.Ordinal).SequenceEqual(right.Order(StringComparer.Ordinal), StringComparer.Ordinal);

    private static string MapTrainingAdapterToUnitId(string targetTrainingAdapter)
        => targetTrainingAdapter switch
        {
            PromptUnderstandingTrainingAdapter => PromptUnderstandingUnitId,
            TaskUnderstandingTrainingAdapter => TaskUnderstandingUnitId,
            TaskTypeTrainingAdapter => TaskTypeUnitId,
            RoutingTrainingAdapter => RoutingUnitId,
            SkillCandidatesTrainingAdapter => SkillCandidatesUnitId,
            SkillSelectionTrainingAdapter => SkillSelectionUnitId,
            PromptContextTrainingAdapter => PromptContextUnitId,
            ResultGapTrainingAdapter => ResultGapUnitId,
            OutputValidationTrainingAdapter => OutputValidationUnitId,
            TaskCompletionTrainingAdapter => TaskCompletionUnitId,
            IntermediateChecksTrainingAdapter => IntermediateChecksUnitId,
            _ => SkillSelectionUnitId
        };

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "";

    private sealed record SkillSelectionEvaluationExpectation(
        SkillSelectionRequest Request,
        IReadOnlyList<string> ExpectedSkillIds);

    private sealed class SkillSelectionPayload
    {
        public IReadOnlyList<string>? SelectedSkillIds { get; init; }
        public IReadOnlyList<string>? AvailableSkillIds { get; init; }
        public IReadOnlyDictionary<string, string>? HostFacts { get; init; }
        public string? SourcePromptEntryId { get; init; }
        public string? TargetTrainingAdapter { get; init; }
    }

    private void LearnPair(SkillSelectionMachinePair pair)
    {
        _trainedPairIds.Add(pair.PairId);
        _trainedPromptKeys.Add(HashPromptKey(NormalizePromptKey(pair.Request.Prompt)));
        if (IsPromptContextTrainingPair(pair))
            LearnPromptContextPair(pair);
        else if (IsResultGapTrainingPair(pair))
            LearnResultGapPair(pair);
        else if (IsOutputValidationTrainingPair(pair))
            LearnOutputValidationPair(pair);
        else if (IsTaskCompletionTrainingPair(pair))
            LearnTaskCompletionPair(pair);
        else if (IsIntermediateChecksTrainingPair(pair))
            LearnIntermediateChecksPair(pair);
        else if (IsSkillSelectionTrainingPair(pair))
            LearnSkillSelectionPair(pair);
        else if (IsSkillCandidatesTrainingPair(pair))
            LearnSkillCandidatesPair(pair);
        else if (IsRoutingTrainingPair(pair))
            LearnRoutingPair(pair);
        else if (IsTaskTypeTrainingPair(pair))
            LearnTaskTypePair(pair);
        else if (IsTaskUnderstandingTrainingPair(pair))
            LearnTaskUnderstandingPair(pair);
        else
            LearnPromptUnderstandingPair(pair);

        foreach (var chosen in pair.Chosen.SelectedSkillIds)
        {
            var index = pair.Request.AvailableSkillIds
                .Select((skillId, position) => new { skillId, position })
                .FirstOrDefault(item => string.Equals(item.skillId, chosen, StringComparison.Ordinal))
                ?.position;
            if (index is null)
                continue;
            _candidatePositionWeights[index.Value] = _candidatePositionWeights.GetValueOrDefault(index.Value) + 1;
        }
    }

    public static string NormalizePromptKey(string prompt)
        => string.Join(' ', prompt.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();

    private static bool IsTaskUnderstandingTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, TaskUnderstandingTrainingAdapter, StringComparison.Ordinal);

    private static bool IsTaskTypeTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, TaskTypeTrainingAdapter, StringComparison.Ordinal);

    private static bool IsRoutingTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, RoutingTrainingAdapter, StringComparison.Ordinal);

    private static bool IsSkillCandidatesTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, SkillCandidatesTrainingAdapter, StringComparison.Ordinal);

    private static bool IsSkillSelectionTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, SkillSelectionTrainingAdapter, StringComparison.Ordinal);

    private static bool IsPromptContextTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, PromptContextTrainingAdapter, StringComparison.Ordinal);

    private static bool IsResultGapTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, ResultGapTrainingAdapter, StringComparison.Ordinal);

    private static bool IsOutputValidationTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, OutputValidationTrainingAdapter, StringComparison.Ordinal);

    private static bool IsTaskCompletionTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, TaskCompletionTrainingAdapter, StringComparison.Ordinal);

    private static bool IsIntermediateChecksTrainingPair(SkillSelectionMachinePair pair)
        => string.Equals(pair.TargetTrainingAdapter, IntermediateChecksTrainingAdapter, StringComparison.Ordinal);

    private void LearnPromptUnderstandingPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _promptUnderstandingSkillState.LearnPromptFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithPromptUnderstanding(SkillSelectionRequest request)
    {
        var evidence = new PromptUnderstandingSkillUnit().Evaluate(
            new PromptUnderstandingSkillInput(
                Request: request,
                State: ExportPromptUnderstandingSkillUnitState()));
        if (!evidence.HasDecision)
            return null;

        return new SkillSelectionDecision(
            SelectedSkillIds: evidence.SelectedSkillIds,
            RejectedSkillIds: evidence.RejectedSkillIds,
            EvidenceFeatures: evidence.EvidenceFeatures,
            Confidence: evidence.Confidence,
            RequiresSecondPass: evidence.RequiresSecondPass,
            DecisionText: evidence.DecisionText);
    }

    private void LearnTaskUnderstandingPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _taskUnderstandingSkillState.LearnTaskBoundaryFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithTaskUnderstanding(SkillSelectionRequest request)
    {
        var evidence = new TaskUnderstandingSkillUnit().Evaluate(
            new TaskUnderstandingSkillInput(
                Request: request,
                State: ExportTaskUnderstandingSkillUnitState()));
        if (!evidence.HasDecision)
            return null;

        return new SkillSelectionDecision(
            SelectedSkillIds: evidence.SelectedSkillIds,
            RejectedSkillIds: evidence.RejectedSkillIds,
            EvidenceFeatures: evidence.EvidenceFeatures,
            Confidence: evidence.Confidence,
            RequiresSecondPass: evidence.RequiresSecondPass,
            DecisionText: evidence.DecisionText);
    }

    private void LearnTaskTypePair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _taskTypeSkillState.LearnTaskTypeFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithTaskType(SkillSelectionRequest request)
    {
        var unitResult = new TaskTypeSkillUnit().ExecuteAsync(new LogicalUnitRequest<TaskTypeSkillInput>(
            RunId: "skill-selection-task-type-sync",
            UnitId: TaskTypeSkillUnit.UnitId,
            UnitRole: TaskTypeSkillUnit.Role,
            DomainTag: DomainTag,
            Input: new TaskTypeSkillInput(
                Request: request,
                State: ExportTaskTypeSkillUnitState()),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>())).GetAwaiter().GetResult();
        if (!unitResult.IsSuccess || unitResult.Value is null || !unitResult.Value.Output.HasDecision)
            return null;

        var unitEvidence = unitResult.Value.Output;

        return new SkillSelectionDecision(
            SelectedSkillIds: unitEvidence.SelectedSkillIds,
            RejectedSkillIds: unitEvidence.RejectedSkillIds,
            EvidenceFeatures: unitEvidence.EvidenceFeatures,
            Confidence: unitEvidence.Confidence,
            RequiresSecondPass: unitEvidence.RequiresSecondPass,
            DecisionText: "task_type selected from the typed TaskTypeSkillUnit; candidate-position scaffold was not used as final authority.");
    }

    private void LearnRoutingPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _routingSkillState.LearnRoutingFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithRouting(SkillSelectionRequest request)
    {
        var unitResult = new RoutingSkillUnit().ExecuteAsync(new LogicalUnitRequest<RoutingSkillInput>(
            RunId: "skill-selection-routing-sync",
            UnitId: RoutingSkillUnit.UnitId,
            UnitRole: RoutingSkillUnit.Role,
            DomainTag: DomainTag,
            Input: new RoutingSkillInput(
                Request: request,
                State: ExportRoutingSkillUnitState()),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>())).GetAwaiter().GetResult();
        if (!unitResult.IsSuccess || unitResult.Value is null || !unitResult.Value.Output.HasDecision)
            return null;

        var unitEvidence = unitResult.Value.Output;

        return new SkillSelectionDecision(
            SelectedSkillIds: unitEvidence.SelectedSkillIds,
            RejectedSkillIds: unitEvidence.RejectedSkillIds,
            EvidenceFeatures: unitEvidence.EvidenceFeatures,
            Confidence: unitEvidence.Confidence,
            RequiresSecondPass: unitEvidence.RequiresSecondPass,
            DecisionText: "routing selected from the typed RoutingSkillUnit; candidate-position scaffold was not used as final authority.");
    }

    private void LearnSkillCandidatesPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _skillCandidatesSkillState.LearnCandidateFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithSkillCandidates(SkillSelectionRequest request)
    {
        var unitResult = new SkillCandidatesSkillUnit().ExecuteAsync(new LogicalUnitRequest<SkillCandidatesSkillInput>(
            RunId: "skill-selection-skill-candidates-sync",
            UnitId: SkillCandidatesSkillUnit.UnitId,
            UnitRole: SkillCandidatesSkillUnit.Role,
            DomainTag: DomainTag,
            Input: new SkillCandidatesSkillInput(
                Request: request,
                State: ExportSkillCandidatesSkillUnitState()),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>())).GetAwaiter().GetResult();
        if (!unitResult.IsSuccess || unitResult.Value is null || !unitResult.Value.Output.HasDecision)
            return null;

        var unitEvidence = unitResult.Value.Output;

        return new SkillSelectionDecision(
            SelectedSkillIds: unitEvidence.SelectedSkillIds,
            RejectedSkillIds: unitEvidence.RejectedSkillIds,
            EvidenceFeatures: unitEvidence.EvidenceFeatures,
            Confidence: unitEvidence.Confidence,
            RequiresSecondPass: unitEvidence.RequiresSecondPass,
            DecisionText: "skill_candidates selected from the typed SkillCandidatesSkillUnit; candidate-position scaffold was not used as final authority.");
    }

    private void LearnSkillSelectionPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _skillSelectionSkillState.LearnFinalSelectionFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithSkillSelection(SkillSelectionRequest request)
    {
        var unitResult = new SkillSelectionSkillUnit().ExecuteAsync(new LogicalUnitRequest<SkillSelectionSkillInput>(
            RunId: "skill-selection-final-selection-sync",
            UnitId: SkillSelectionSkillUnit.UnitId,
            UnitRole: SkillSelectionSkillUnit.Role,
            DomainTag: DomainTag,
            Input: new SkillSelectionSkillInput(
                Request: request,
                State: ExportSkillSelectionSkillUnitState()),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>())).GetAwaiter().GetResult();
        if (!unitResult.IsSuccess || unitResult.Value is null || !unitResult.Value.Output.HasDecision)
            return null;

        var unitEvidence = unitResult.Value.Output;

        return new SkillSelectionDecision(
            SelectedSkillIds: unitEvidence.SelectedSkillIds,
            RejectedSkillIds: unitEvidence.RejectedSkillIds,
            EvidenceFeatures: unitEvidence.EvidenceFeatures,
            Confidence: unitEvidence.Confidence,
            RequiresSecondPass: unitEvidence.RequiresSecondPass,
            DecisionText: "skill_selection selected final skills from the typed SkillSelectionSkillUnit; candidate-position scaffold was not used as final authority.");
    }

    private void LearnPromptContextPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptContextSignals(pair.Request);
        _promptContextSkillState.LearnPromptAndContextFeatureWeights(
            pair,
            signals.Where(IsPromptContextPromptSignal).ToArray(),
            signals.Where(IsPromptContextContextSignal).ToArray());
    }

    private SkillSelectionDecision? TryDecideWithPromptContext(SkillSelectionRequest request)
    {
        var unitEvidence = new PromptContextSkillSelectionUnit().Evaluate(new PromptContextSkillInput(
            Request: request,
            State: ExportPromptContextSkillSelectionUnitState()));
        if (!unitEvidence.HasDecision)
            return null;

        return new SkillSelectionDecision(
            SelectedSkillIds: unitEvidence.SelectedSkillIds,
            RejectedSkillIds: unitEvidence.RejectedSkillIds,
            EvidenceFeatures: unitEvidence.EvidenceFeatures,
            Confidence: unitEvidence.Confidence,
            RequiresSecondPass: unitEvidence.RequiresSecondPass,
            DecisionText: "prompt_context selected from the typed PromptContextSkillSelectionUnit; candidate-position scaffold was not used as final authority.");
    }

    private void LearnResultGapPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _resultGapSkillState.LearnResultGapFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithResultGap(SkillSelectionRequest request)
    {
        var unitResult = new ResultGapSkillUnit().ExecuteAsync(new LogicalUnitRequest<ResultGapSkillInput>(
            RunId: "skill-selection-result-gap-sync",
            UnitId: ResultGapSkillUnit.UnitId,
            UnitRole: ResultGapSkillUnit.Role,
            DomainTag: DomainTag,
            Input: new ResultGapSkillInput(
                Request: request,
                State: ExportResultGapSkillUnitState()),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>())).GetAwaiter().GetResult();
        if (!unitResult.IsSuccess || unitResult.Value is null || !unitResult.Value.Output.HasDecision)
            return null;

        var unitEvidence = unitResult.Value.Output;

        return new SkillSelectionDecision(
            SelectedSkillIds: unitEvidence.SelectedSkillIds,
            RejectedSkillIds: unitEvidence.RejectedSkillIds,
            EvidenceFeatures: unitEvidence.EvidenceFeatures,
            Confidence: unitEvidence.Confidence,
            RequiresSecondPass: unitEvidence.RequiresSecondPass,
            DecisionText: "result_gap selected from the typed ResultGapSkillUnit; bootstrap ResultGapMachine and candidate-position scaffold were not used as final authority.");
    }

    private void LearnOutputValidationPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _outputValidationSkillState.LearnValidationFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithOutputValidation(SkillSelectionRequest request)
    {
        var unitResult = new OutputValidationSkillUnit().ExecuteAsync(new LogicalUnitRequest<OutputValidationSkillInput>(
            RunId: "skill-selection-output-validation-sync",
            UnitId: OutputValidationSkillUnit.UnitId,
            UnitRole: OutputValidationSkillUnit.Role,
            DomainTag: DomainTag,
            Input: new OutputValidationSkillInput(
                Request: request,
                State: ExportOutputValidationSkillUnitState()),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>())).GetAwaiter().GetResult();
        if (!unitResult.IsSuccess || unitResult.Value is null || !unitResult.Value.Output.HasDecision)
            return null;

        var unitEvidence = unitResult.Value.Output;

        return new SkillSelectionDecision(
            SelectedSkillIds: unitEvidence.SelectedSkillIds,
            RejectedSkillIds: unitEvidence.RejectedSkillIds,
            EvidenceFeatures: unitEvidence.EvidenceFeatures,
            Confidence: unitEvidence.Confidence,
            RequiresSecondPass: unitEvidence.RequiresSecondPass,
            DecisionText: "output_validation selected from the typed OutputValidationSkillUnit; API/request validators and candidate-position scaffold were not used as final authority.");
    }

    private void LearnTaskCompletionPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _taskCompletionSkillState.LearnCompletionFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithTaskCompletion(SkillSelectionRequest request)
    {
        var unitResult = new TaskCompletionSkillUnit().ExecuteAsync(new LogicalUnitRequest<TaskCompletionSkillInput>(
            RunId: "skill-selection-task-completion-sync",
            UnitId: TaskCompletionSkillUnit.UnitId,
            UnitRole: TaskCompletionSkillUnit.Role,
            DomainTag: DomainTag,
            Input: new TaskCompletionSkillInput(
                Request: request,
                State: ExportTaskCompletionSkillUnitState()),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>())).GetAwaiter().GetResult();
        if (!unitResult.IsSuccess || unitResult.Value is null || !unitResult.Value.Output.HasDecision)
            return null;

        var unitEvidence = unitResult.Value.Output;

        return new SkillSelectionDecision(
            SelectedSkillIds: unitEvidence.SelectedSkillIds,
            RejectedSkillIds: unitEvidence.RejectedSkillIds,
            EvidenceFeatures: unitEvidence.EvidenceFeatures,
            Confidence: unitEvidence.Confidence,
            RequiresSecondPass: unitEvidence.RequiresSecondPass,
            DecisionText: "task_completion selected from the typed TaskCompletionSkillUnit; bootstrap TaskCompletionMachine and candidate-position scaffold were not used as final authority.");
    }

    private void LearnIntermediateChecksPair(SkillSelectionMachinePair pair)
    {
        var signals = ExtractPromptUnderstandingSignals(pair.Request.Prompt);
        _intermediateChecksSkillState.LearnCheckFeatureWeights(pair, signals);
    }

    private SkillSelectionDecision? TryDecideWithIntermediateChecks(SkillSelectionRequest request)
    {
        var unitResult = new IntermediateChecksSkillUnit().ExecuteAsync(new LogicalUnitRequest<IntermediateChecksSkillInput>(
            RunId: "skill-selection-intermediate-checks-sync",
            UnitId: IntermediateChecksSkillUnit.UnitId,
            UnitRole: IntermediateChecksSkillUnit.Role,
            DomainTag: DomainTag,
            Input: new IntermediateChecksSkillInput(
                Request: request,
                State: ExportIntermediateChecksSkillUnitState()),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>())).GetAwaiter().GetResult();
        if (!unitResult.IsSuccess || unitResult.Value is null || !unitResult.Value.Output.HasDecision)
            return null;

        var unitEvidence = unitResult.Value.Output;

        return new SkillSelectionDecision(
            SelectedSkillIds: unitEvidence.SelectedSkillIds,
            RejectedSkillIds: unitEvidence.RejectedSkillIds,
            EvidenceFeatures: unitEvidence.EvidenceFeatures,
            Confidence: unitEvidence.Confidence,
            RequiresSecondPass: unitEvidence.RequiresSecondPass,
            DecisionText: "intermediate_checks selected from the typed IntermediateChecksSkillUnit; candidate-position scaffold was not used as final authority.");
    }

    public PromptUnderstandingSkillUnitState ExportPromptUnderstandingSkillUnitState()
        => _promptUnderstandingSkillState.ExportUnitState(_trainedPairIds);

    private PromptUnderstandingUnitState ExportPromptUnderstandingState()
    {
        var unitState = ExportPromptUnderstandingSkillUnitState();
        return new PromptUnderstandingUnitState(
            UnitId: PromptUnderstandingUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.PromptFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    private TaskUnderstandingUnitState ExportTaskUnderstandingState()
    {
        var unitState = ExportTaskUnderstandingSkillUnitState();
        return new TaskUnderstandingUnitState(
            UnitId: TaskUnderstandingUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.TaskFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public TaskUnderstandingSkillUnitState ExportTaskUnderstandingSkillUnitState()
        => _taskUnderstandingSkillState.ExportUnitState(_trainedPairIds);

    private TaskTypeUnitState ExportTaskTypeState()
    {
        var unitState = ExportTaskTypeSkillUnitState();
        return new TaskTypeUnitState(
            UnitId: TaskTypeUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .OrderBy(profile => profile.SkillId, StringComparer.Ordinal)
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.TaskTypeFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public TaskTypeSkillUnitState ExportTaskTypeSkillUnitState()
        => _taskTypeSkillState.ExportUnitState(_trainedPairIds);

    private RoutingUnitState ExportRoutingState()
    {
        var unitState = ExportRoutingSkillUnitState();
        return new RoutingUnitState(
            UnitId: RoutingUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .OrderBy(profile => profile.SkillId, StringComparer.Ordinal)
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.RoutingFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public RoutingSkillUnitState ExportRoutingSkillUnitState()
        => _routingSkillState.ExportUnitState(_trainedPairIds);

    private SkillCandidatesUnitState ExportSkillCandidatesState()
    {
        var unitState = ExportSkillCandidatesSkillUnitState();
        return new SkillCandidatesUnitState(
            UnitId: SkillCandidatesUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .OrderBy(profile => profile.SkillId, StringComparer.Ordinal)
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.CandidateFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public SkillCandidatesSkillUnitState ExportSkillCandidatesSkillUnitState()
        => _skillCandidatesSkillState.ExportUnitState(_trainedPairIds);

    private SkillSelectionUnitState ExportSkillSelectionState()
    {
        var unitState = ExportSkillSelectionSkillUnitState();
        return new SkillSelectionUnitState(
            UnitId: SkillSelectionUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .OrderBy(profile => profile.SkillId, StringComparer.Ordinal)
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.FinalSelectionFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public SkillSelectionSkillUnitState ExportSkillSelectionSkillUnitState()
        => _skillSelectionSkillState.ExportUnitState(_trainedPairIds);

    private PromptContextUnitState ExportPromptContextState()
    {
        var unitState = ExportPromptContextSkillSelectionUnitState();
        return new PromptContextUnitState(
            UnitId: PromptContextUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .OrderBy(profile => profile.SkillId, StringComparer.Ordinal)
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.PromptFeatureWeights
                        .Concat(profile.ContextFeatureWeights)
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: profile.SourcePairIds
                        .Order(StringComparer.Ordinal)
                        .ToArray()))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public PromptContextSkillSelectionUnitState ExportPromptContextSkillSelectionUnitState()
        => _promptContextSkillState.ExportUnitState(_trainedPairIds);

    private ResultGapUnitState ExportResultGapState()
    {
        var unitState = ExportResultGapSkillUnitState();
        return new ResultGapUnitState(
            UnitId: ResultGapUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .OrderBy(profile => profile.SkillId, StringComparer.Ordinal)
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.ResultGapFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public ResultGapSkillUnitState ExportResultGapSkillUnitState()
        => _resultGapSkillState.ExportUnitState(_trainedPairIds);

    private OutputValidationUnitState ExportOutputValidationState()
    {
        var unitState = ExportOutputValidationSkillUnitState();
        return new OutputValidationUnitState(
            UnitId: OutputValidationUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .OrderBy(profile => profile.SkillId, StringComparer.Ordinal)
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.ValidationFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public OutputValidationSkillUnitState ExportOutputValidationSkillUnitState()
        => _outputValidationSkillState.ExportUnitState(_trainedPairIds);

    private TaskCompletionUnitState ExportTaskCompletionState()
    {
        var unitState = ExportTaskCompletionSkillUnitState();
        return new TaskCompletionUnitState(
            UnitId: TaskCompletionUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .OrderBy(profile => profile.SkillId, StringComparer.Ordinal)
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.CompletionFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public TaskCompletionSkillUnitState ExportTaskCompletionSkillUnitState()
        => _taskCompletionSkillState.ExportUnitState(_trainedPairIds);

    private IntermediateChecksUnitState ExportIntermediateChecksState()
    {
        var unitState = ExportIntermediateChecksSkillUnitState();
        return new IntermediateChecksUnitState(
            UnitId: IntermediateChecksUnitId,
            SchemaVersion: 1,
            SkillProfiles: unitState.SkillProfiles
                .OrderBy(profile => profile.SkillId, StringComparer.Ordinal)
                .Select(profile => new PromptUnderstandingSkillProfile(
                    SkillId: profile.SkillId,
                    LearnedSignals: profile.CheckFeatureWeights,
                    SourcePairIds: profile.SourcePairIds))
                .ToArray(),
            TrainingPairIds: unitState.TrainingPairIds,
            SemanticQualityClaimed: false);
    }

    public IntermediateChecksSkillUnitState ExportIntermediateChecksSkillUnitState()
        => _intermediateChecksSkillState.ExportUnitState(_trainedPairIds);

    private sealed class PromptUnderstandingSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillPromptFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillPromptFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
        }

        public void LearnPromptFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> promptFeatureWeights)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in promptFeatureWeights)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillPromptFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
            }
        }

        public PromptUnderstandingSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.PromptFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new PromptUnderstandingSkillUnitState(
                UnitId: PromptUnderstandingSkillUnit.UnitId,
                SchemaVersion: 1,
                PromptFeatureVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<PromptUnderstandingSkillFeatureProfile> SkillProfiles()
            => _skillPromptFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new PromptUnderstandingSkillFeatureProfile(
                    SkillId: item.Key,
                    PromptFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillPromptFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillPromptFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    private sealed class TaskUnderstandingSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillTaskFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillTaskFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
        }

        public void LearnTaskBoundaryFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> taskBoundaryFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in taskBoundaryFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillTaskFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
            }
        }

        public TaskUnderstandingSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.TaskFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new TaskUnderstandingSkillUnitState(
                UnitId: TaskUnderstandingSkillUnit.UnitId,
                SchemaVersion: 1,
                TaskFeatureVocabulary: vocabulary,
                TaskKindVocabulary: vocabulary,
                DeliverableVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<TaskUnderstandingSkillFeatureProfile> SkillProfiles()
            => _skillTaskFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new TaskUnderstandingSkillFeatureProfile(
                    SkillId: item.Key,
                    TaskFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillTaskFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillTaskFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    private sealed class TaskTypeSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillTaskTypeFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);
        private readonly List<TaskTypeKnnExample> _knnMemory = [];

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillTaskTypeFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
            _knnMemory.Clear();
        }

        public void LearnTaskTypeFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> taskTypeFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in taskTypeFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
                _knnMemory.Add(new TaskTypeKnnExample(
                    ExampleId: $"task-type-example-{_knnMemory.Count + 1:D4}",
                    TaskTypeFeatures: taskTypeFeatures.Order(StringComparer.Ordinal).ToArray(),
                    ChosenSkillId: chosenSkillId,
                    Weight: 1.0));
            }

            var chosenSkillIds = pair.Chosen.SelectedSkillIds.ToHashSet(StringComparer.Ordinal);
            foreach (var rejectedSkillId in pair.Rejected.SelectedSkillIds
                .Where(skillId => pair.Request.AvailableSkillIds.Contains(skillId, StringComparer.Ordinal))
                .Where(skillId => !chosenSkillIds.Contains(skillId)))
            {
                var skillProfile = GetOrCreateSkillProfile(rejectedSkillId);
                foreach (var feature in taskTypeFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) - 1;

                GetOrCreateSourcePairIds(rejectedSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillTaskTypeFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
                var positiveFeatures = profile.LearnedSignals
                    .Where(item => item.Value > 0)
                    .Select(item => item.Key)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (positiveFeatures.Length > 0)
                {
                    _knnMemory.Add(new TaskTypeKnnExample(
                        ExampleId: $"task-type-loaded-example-{_knnMemory.Count + 1:D4}",
                        TaskTypeFeatures: positiveFeatures,
                        ChosenSkillId: profile.SkillId,
                        Weight: 1.0));
                }
            }
        }

        public TaskTypeSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.TaskTypeFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new TaskTypeSkillUnitState(
                UnitId: TaskTypeSkillUnit.UnitId,
                SchemaVersion: 1,
                TaskTypeFeatureVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                KnnMemory: _knnMemory
                    .OrderBy(example => example.ExampleId, StringComparer.Ordinal)
                    .ToArray(),
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<TaskTypeSkillProfile> SkillProfiles()
            => _skillTaskTypeFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new TaskTypeSkillProfile(
                    SkillId: item.Key,
                    TaskTypeFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillTaskTypeFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillTaskTypeFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    private sealed class RoutingSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillRoutingFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);
        private readonly List<RoutingKnnExample> _knnMemory = [];

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillRoutingFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
            _knnMemory.Clear();
        }

        public void LearnRoutingFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> routingFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in routingFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
                _knnMemory.Add(new RoutingKnnExample(
                    ExampleId: $"routing-example-{_knnMemory.Count + 1:D4}",
                    RoutingFeatures: routingFeatures.Order(StringComparer.Ordinal).ToArray(),
                    ChosenSkillId: chosenSkillId,
                    Weight: 1.0));
            }

            var chosenSkillIds = pair.Chosen.SelectedSkillIds.ToHashSet(StringComparer.Ordinal);
            foreach (var rejectedSkillId in pair.Rejected.SelectedSkillIds
                .Where(skillId => pair.Request.AvailableSkillIds.Contains(skillId, StringComparer.Ordinal))
                .Where(skillId => !chosenSkillIds.Contains(skillId)))
            {
                var skillProfile = GetOrCreateSkillProfile(rejectedSkillId);
                foreach (var feature in routingFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) - 1;

                GetOrCreateSourcePairIds(rejectedSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillRoutingFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
                var positiveFeatures = profile.LearnedSignals
                    .Where(item => item.Value > 0)
                    .Select(item => item.Key)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (positiveFeatures.Length > 0)
                {
                    _knnMemory.Add(new RoutingKnnExample(
                        ExampleId: $"routing-loaded-example-{_knnMemory.Count + 1:D4}",
                        RoutingFeatures: positiveFeatures,
                        ChosenSkillId: profile.SkillId,
                        Weight: 1.0));
                }
            }
        }

        public RoutingSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.RoutingFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new RoutingSkillUnitState(
                UnitId: RoutingSkillUnit.UnitId,
                SchemaVersion: 1,
                RoutingFeatureVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                KnnMemory: _knnMemory
                    .OrderBy(example => example.ExampleId, StringComparer.Ordinal)
                    .ToArray(),
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<RoutingSkillProfile> SkillProfiles()
            => _skillRoutingFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new RoutingSkillProfile(
                    SkillId: item.Key,
                    RoutingFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillRoutingFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillRoutingFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    private sealed class SkillCandidatesSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillCandidateFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);
        private readonly List<SkillCandidatesKnnExample> _knnMemory = [];

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillCandidateFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
            _knnMemory.Clear();
        }

        public void LearnCandidateFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> candidateFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in candidateFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
                _knnMemory.Add(new SkillCandidatesKnnExample(
                    ExampleId: $"skill-candidates-example-{_knnMemory.Count + 1:D4}",
                    CandidateFeatures: candidateFeatures.Order(StringComparer.Ordinal).ToArray(),
                    ChosenSkillId: chosenSkillId,
                    Weight: 1.0));
            }

            var chosenSkillIds = pair.Chosen.SelectedSkillIds.ToHashSet(StringComparer.Ordinal);
            foreach (var rejectedSkillId in pair.Rejected.SelectedSkillIds
                .Where(skillId => pair.Request.AvailableSkillIds.Contains(skillId, StringComparer.Ordinal))
                .Where(skillId => !chosenSkillIds.Contains(skillId)))
            {
                var skillProfile = GetOrCreateSkillProfile(rejectedSkillId);
                foreach (var feature in candidateFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) - 1;

                GetOrCreateSourcePairIds(rejectedSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillCandidateFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
                var positiveFeatures = profile.LearnedSignals
                    .Where(item => item.Value > 0)
                    .Select(item => item.Key)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (positiveFeatures.Length > 0)
                {
                    _knnMemory.Add(new SkillCandidatesKnnExample(
                        ExampleId: $"skill-candidates-loaded-example-{_knnMemory.Count + 1:D4}",
                        CandidateFeatures: positiveFeatures,
                        ChosenSkillId: profile.SkillId,
                        Weight: 1.0));
                }
            }
        }

        public SkillCandidatesSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.CandidateFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new SkillCandidatesSkillUnitState(
                UnitId: SkillCandidatesSkillUnit.UnitId,
                SchemaVersion: 1,
                CandidateFeatureVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                KnnMemory: _knnMemory
                    .OrderBy(example => example.ExampleId, StringComparer.Ordinal)
                    .ToArray(),
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<SkillCandidatesSkillProfile> SkillProfiles()
            => _skillCandidateFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new SkillCandidatesSkillProfile(
                    SkillId: item.Key,
                    CandidateFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillCandidateFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillCandidateFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    private sealed class SkillSelectionSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillFinalSelectionFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);
        private readonly List<SkillSelectionKnnExample> _knnMemory = [];

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillFinalSelectionFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
            _knnMemory.Clear();
        }

        public void LearnFinalSelectionFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> finalSelectionFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in finalSelectionFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
                _knnMemory.Add(new SkillSelectionKnnExample(
                    ExampleId: $"skill-selection-example-{_knnMemory.Count + 1:D4}",
                    FinalSelectionFeatures: finalSelectionFeatures.Order(StringComparer.Ordinal).ToArray(),
                    ChosenSkillId: chosenSkillId,
                    Weight: 1.0));
            }

            var chosenSkillIds = pair.Chosen.SelectedSkillIds.ToHashSet(StringComparer.Ordinal);
            foreach (var rejectedSkillId in pair.Rejected.SelectedSkillIds
                .Where(skillId => pair.Request.AvailableSkillIds.Contains(skillId, StringComparer.Ordinal))
                .Where(skillId => !chosenSkillIds.Contains(skillId)))
            {
                var skillProfile = GetOrCreateSkillProfile(rejectedSkillId);
                foreach (var feature in finalSelectionFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) - 1;

                GetOrCreateSourcePairIds(rejectedSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillFinalSelectionFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
                var positiveFeatures = profile.LearnedSignals
                    .Where(item => item.Value > 0)
                    .Select(item => item.Key)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (positiveFeatures.Length > 0)
                {
                    _knnMemory.Add(new SkillSelectionKnnExample(
                        ExampleId: $"skill-selection-loaded-example-{_knnMemory.Count + 1:D4}",
                        FinalSelectionFeatures: positiveFeatures,
                        ChosenSkillId: profile.SkillId,
                        Weight: 1.0));
                }
            }
        }

        public SkillSelectionSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.FinalSelectionFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new SkillSelectionSkillUnitState(
                UnitId: SkillSelectionSkillUnit.UnitId,
                SchemaVersion: 1,
                FinalSelectionFeatureVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                KnnMemory: _knnMemory
                    .OrderBy(example => example.ExampleId, StringComparer.Ordinal)
                    .ToArray(),
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<SkillSelectionSkillProfile> SkillProfiles()
            => _skillFinalSelectionFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new SkillSelectionSkillProfile(
                    SkillId: item.Key,
                    FinalSelectionFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillFinalSelectionFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillFinalSelectionFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    private sealed class PromptContextSkillTrainingState
    {
        private readonly Dictionary<string, PromptContextMutableSkillProfile> _skillProfiles = new(StringComparer.Ordinal);

        public bool SemanticQualityClaimed => false;

        public void Clear()
            => _skillProfiles.Clear();

        public void LearnPromptAndContextFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> promptFeatures,
            IReadOnlyList<string> contextFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var profile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var promptFeature in promptFeatures)
                    profile.PromptFeatureWeights[promptFeature] = profile.PromptFeatureWeights.GetValueOrDefault(promptFeature) + 1;
                foreach (var contextFeature in contextFeatures)
                    profile.ContextFeatureWeights[contextFeature] = profile.ContextFeatureWeights.GetValueOrDefault(contextFeature) + 1;

                profile.SourcePairIds.Add(pair.PairId);
            }

            foreach (var rejectedSkillId in pair.Chosen.RejectedSkillIds)
            {
                var profile = GetOrCreateSkillProfile(rejectedSkillId);
                foreach (var promptFeature in promptFeatures)
                    profile.PromptFeatureWeights[promptFeature] = profile.PromptFeatureWeights.GetValueOrDefault(promptFeature) - 1;

                profile.SourcePairIds.Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                var typedProfile = GetOrCreateSkillProfile(profile.SkillId);
                foreach (var signal in profile.LearnedSignals)
                {
                    if (IsPromptContextPromptSignal(signal.Key))
                        typedProfile.PromptFeatureWeights[signal.Key] = signal.Value;
                    else if (IsPromptContextContextSignal(signal.Key))
                        typedProfile.ContextFeatureWeights[signal.Key] = signal.Value;
                }

                foreach (var sourcePairId in profile.SourcePairIds)
                    typedProfile.SourcePairIds.Add(sourcePairId);
            }
        }

        public PromptContextSkillSelectionUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var promptVocabulary = profiles
                .SelectMany(profile => profile.PromptFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            var contextVocabulary = profiles
                .SelectMany(profile => profile.ContextFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new PromptContextSkillSelectionUnitState(
                UnitId: PromptContextSkillSelectionUnit.UnitId,
                SchemaVersion: 1,
                PromptFeatureVocabulary: promptVocabulary,
                ContextFeatureVocabulary: contextVocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<PromptContextSkillProfile> SkillProfiles()
            => _skillProfiles
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new PromptContextSkillProfile(
                    SkillId: item.Key,
                    PromptFeatureWeights: item.Value.PromptFeatureWeights
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    ContextFeatureWeights: item.Value.ContextFeatureWeights
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: item.Value.SourcePairIds
                        .Order(StringComparer.Ordinal)
                        .ToArray()))
                .ToArray();

        private PromptContextMutableSkillProfile GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillProfiles.TryGetValue(skillId, out var profile))
            {
                profile = new PromptContextMutableSkillProfile();
                _skillProfiles[skillId] = profile;
            }

            return profile;
        }

        private sealed class PromptContextMutableSkillProfile
        {
            public Dictionary<string, int> PromptFeatureWeights { get; } = new(StringComparer.Ordinal);
            public Dictionary<string, int> ContextFeatureWeights { get; } = new(StringComparer.Ordinal);
            public HashSet<string> SourcePairIds { get; } = new(StringComparer.Ordinal);
        }
    }

    private sealed class ResultGapSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillResultGapFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);
        private readonly List<ResultGapKnnExample> _knnMemory = [];

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillResultGapFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
            _knnMemory.Clear();
        }

        public void LearnResultGapFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> resultGapFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in resultGapFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
                _knnMemory.Add(new ResultGapKnnExample(
                    ExampleId: $"result-gap-example-{_knnMemory.Count + 1:D4}",
                    ResultGapFeatures: resultGapFeatures.Order(StringComparer.Ordinal).ToArray(),
                    ChosenSkillId: chosenSkillId,
                    Weight: 1.0));
            }

            var chosenSkillIds = pair.Chosen.SelectedSkillIds.ToHashSet(StringComparer.Ordinal);
            foreach (var rejectedSkillId in pair.Rejected.SelectedSkillIds
                .Where(skillId => pair.Request.AvailableSkillIds.Contains(skillId, StringComparer.Ordinal))
                .Where(skillId => !chosenSkillIds.Contains(skillId)))
            {
                var skillProfile = GetOrCreateSkillProfile(rejectedSkillId);
                foreach (var feature in resultGapFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) - 1;

                GetOrCreateSourcePairIds(rejectedSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillResultGapFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
                var positiveFeatures = profile.LearnedSignals
                    .Where(item => item.Value > 0)
                    .Select(item => item.Key)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (positiveFeatures.Length > 0)
                {
                    _knnMemory.Add(new ResultGapKnnExample(
                        ExampleId: $"result-gap-loaded-example-{_knnMemory.Count + 1:D4}",
                        ResultGapFeatures: positiveFeatures,
                        ChosenSkillId: profile.SkillId,
                        Weight: 1.0));
                }
            }
        }

        public ResultGapSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.ResultGapFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new ResultGapSkillUnitState(
                UnitId: ResultGapSkillUnit.UnitId,
                SchemaVersion: 1,
                ResultGapFeatureVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                KnnMemory: _knnMemory
                    .OrderBy(example => example.ExampleId, StringComparer.Ordinal)
                    .ToArray(),
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<ResultGapSkillProfile> SkillProfiles()
            => _skillResultGapFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new ResultGapSkillProfile(
                    SkillId: item.Key,
                    ResultGapFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillResultGapFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillResultGapFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    private sealed class OutputValidationSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillValidationFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);
        private readonly List<OutputValidationKnnExample> _knnMemory = [];

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillValidationFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
            _knnMemory.Clear();
        }

        public void LearnValidationFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> validationFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in validationFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
                _knnMemory.Add(new OutputValidationKnnExample(
                    ExampleId: $"output-validation-example-{_knnMemory.Count + 1:D4}",
                    ValidationFeatures: validationFeatures.Order(StringComparer.Ordinal).ToArray(),
                    ChosenSkillId: chosenSkillId,
                    Weight: 1.0));
            }

            var chosenSkillIds = pair.Chosen.SelectedSkillIds.ToHashSet(StringComparer.Ordinal);
            foreach (var rejectedSkillId in pair.Rejected.SelectedSkillIds
                .Where(skillId => pair.Request.AvailableSkillIds.Contains(skillId, StringComparer.Ordinal))
                .Where(skillId => !chosenSkillIds.Contains(skillId)))
            {
                var skillProfile = GetOrCreateSkillProfile(rejectedSkillId);
                foreach (var feature in validationFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) - 1;

                GetOrCreateSourcePairIds(rejectedSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillValidationFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
                var positiveFeatures = profile.LearnedSignals
                    .Where(item => item.Value > 0)
                    .Select(item => item.Key)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (positiveFeatures.Length > 0)
                {
                    _knnMemory.Add(new OutputValidationKnnExample(
                        ExampleId: $"output-validation-loaded-example-{_knnMemory.Count + 1:D4}",
                        ValidationFeatures: positiveFeatures,
                        ChosenSkillId: profile.SkillId,
                        Weight: 1.0));
                }
            }
        }

        public OutputValidationSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.ValidationFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new OutputValidationSkillUnitState(
                UnitId: OutputValidationSkillUnit.UnitId,
                SchemaVersion: 1,
                ValidationFeatureVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                KnnMemory: _knnMemory
                    .OrderBy(example => example.ExampleId, StringComparer.Ordinal)
                    .ToArray(),
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<OutputValidationSkillProfile> SkillProfiles()
            => _skillValidationFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new OutputValidationSkillProfile(
                    SkillId: item.Key,
                    ValidationFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillValidationFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillValidationFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    private sealed class TaskCompletionSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillCompletionFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);
        private readonly List<TaskCompletionKnnExample> _knnMemory = [];

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillCompletionFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
            _knnMemory.Clear();
        }

        public void LearnCompletionFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> completionFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in completionFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
                _knnMemory.Add(new TaskCompletionKnnExample(
                    ExampleId: $"task-completion-example-{_knnMemory.Count + 1:D4}",
                    CompletionFeatures: completionFeatures.Order(StringComparer.Ordinal).ToArray(),
                    ChosenSkillId: chosenSkillId,
                    Weight: 1.0));
            }

            var chosenSkillIds = pair.Chosen.SelectedSkillIds.ToHashSet(StringComparer.Ordinal);
            foreach (var rejectedSkillId in pair.Rejected.SelectedSkillIds
                .Where(skillId => pair.Request.AvailableSkillIds.Contains(skillId, StringComparer.Ordinal))
                .Where(skillId => !chosenSkillIds.Contains(skillId)))
            {
                var skillProfile = GetOrCreateSkillProfile(rejectedSkillId);
                foreach (var feature in completionFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) - 1;

                GetOrCreateSourcePairIds(rejectedSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillCompletionFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
                var positiveFeatures = profile.LearnedSignals
                    .Where(item => item.Value > 0)
                    .Select(item => item.Key)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (positiveFeatures.Length > 0)
                {
                    _knnMemory.Add(new TaskCompletionKnnExample(
                        ExampleId: $"task-completion-loaded-example-{_knnMemory.Count + 1:D4}",
                        CompletionFeatures: positiveFeatures,
                        ChosenSkillId: profile.SkillId,
                        Weight: 1.0));
                }
            }
        }

        public TaskCompletionSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.CompletionFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new TaskCompletionSkillUnitState(
                UnitId: TaskCompletionSkillUnit.UnitId,
                SchemaVersion: 1,
                CompletionFeatureVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                KnnMemory: _knnMemory
                    .OrderBy(example => example.ExampleId, StringComparer.Ordinal)
                    .ToArray(),
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<TaskCompletionSkillProfile> SkillProfiles()
            => _skillCompletionFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new TaskCompletionSkillProfile(
                    SkillId: item.Key,
                    CompletionFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillCompletionFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillCompletionFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    private sealed class IntermediateChecksSkillTrainingState
    {
        private readonly Dictionary<string, Dictionary<string, int>> _skillCheckFeatureWeights = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _sourcePairIdsBySkill = new(StringComparer.Ordinal);
        private readonly List<IntermediateChecksKnnExample> _knnMemory = [];

        public bool SemanticQualityClaimed => false;

        public void Clear()
        {
            _skillCheckFeatureWeights.Clear();
            _sourcePairIdsBySkill.Clear();
            _knnMemory.Clear();
        }

        public void LearnCheckFeatureWeights(
            SkillSelectionMachinePair pair,
            IReadOnlyList<string> checkFeatures)
        {
            foreach (var chosenSkillId in pair.Chosen.SelectedSkillIds)
            {
                var skillProfile = GetOrCreateSkillProfile(chosenSkillId);
                foreach (var feature in checkFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) + 1;

                GetOrCreateSourcePairIds(chosenSkillId).Add(pair.PairId);
                _knnMemory.Add(new IntermediateChecksKnnExample(
                    ExampleId: $"intermediate-checks-example-{_knnMemory.Count + 1:D4}",
                    CheckFeatures: checkFeatures.Order(StringComparer.Ordinal).ToArray(),
                    ChosenSkillId: chosenSkillId,
                    Weight: 1.0));
            }

            var chosenSkillIds = pair.Chosen.SelectedSkillIds.ToHashSet(StringComparer.Ordinal);
            foreach (var rejectedSkillId in pair.Rejected.SelectedSkillIds
                .Where(skillId => pair.Request.AvailableSkillIds.Contains(skillId, StringComparer.Ordinal))
                .Where(skillId => !chosenSkillIds.Contains(skillId)))
            {
                var skillProfile = GetOrCreateSkillProfile(rejectedSkillId);
                foreach (var feature in checkFeatures)
                    skillProfile[feature] = skillProfile.GetValueOrDefault(feature) - 1;

                GetOrCreateSourcePairIds(rejectedSkillId).Add(pair.PairId);
            }
        }

        public void LoadProfiles(IReadOnlyList<PromptUnderstandingSkillProfile> skillProfiles)
        {
            Clear();
            foreach (var profile in skillProfiles)
            {
                _skillCheckFeatureWeights[profile.SkillId] = profile.LearnedSignals
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                _sourcePairIdsBySkill[profile.SkillId] = profile.SourcePairIds
                    .ToHashSet(StringComparer.Ordinal);
                var positiveFeatures = profile.LearnedSignals
                    .Where(item => item.Value > 0)
                    .Select(item => item.Key)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (positiveFeatures.Length > 0)
                {
                    _knnMemory.Add(new IntermediateChecksKnnExample(
                        ExampleId: $"intermediate-checks-loaded-example-{_knnMemory.Count + 1:D4}",
                        CheckFeatures: positiveFeatures,
                        ChosenSkillId: profile.SkillId,
                        Weight: 1.0));
                }
            }
        }

        public IntermediateChecksSkillUnitState ExportUnitState(IReadOnlyCollection<string> trainingPairIds)
        {
            var profiles = SkillProfiles();
            var vocabulary = profiles
                .SelectMany(profile => profile.CheckFeatureWeights.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            return new IntermediateChecksSkillUnitState(
                UnitId: IntermediateChecksSkillUnit.UnitId,
                SchemaVersion: 1,
                CheckFeatureVocabulary: vocabulary,
                SkillVocabulary: profiles
                    .Select(profile => profile.SkillId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                SkillProfiles: profiles,
                KnnMemory: _knnMemory
                    .OrderBy(example => example.ExampleId, StringComparer.Ordinal)
                    .ToArray(),
                TrainingPairIds: trainingPairIds.Order(StringComparer.Ordinal).ToArray(),
                SemanticQualityClaimed: SemanticQualityClaimed);
        }

        private IReadOnlyList<IntermediateChecksSkillProfile> SkillProfiles()
            => _skillCheckFeatureWeights
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new IntermediateChecksSkillProfile(
                    SkillId: item.Key,
                    CheckFeatureWeights: item.Value
                        .OrderBy(signal => signal.Key, StringComparer.Ordinal)
                        .ToDictionary(signal => signal.Key, signal => signal.Value),
                    SourcePairIds: _sourcePairIdsBySkill.GetValueOrDefault(item.Key)?
                        .Order(StringComparer.Ordinal)
                        .ToArray() ?? []))
                .ToArray();

        private Dictionary<string, int> GetOrCreateSkillProfile(string skillId)
        {
            if (!_skillCheckFeatureWeights.TryGetValue(skillId, out var skillProfile))
            {
                skillProfile = new Dictionary<string, int>(StringComparer.Ordinal);
                _skillCheckFeatureWeights[skillId] = skillProfile;
            }

            return skillProfile;
        }

        private HashSet<string> GetOrCreateSourcePairIds(string skillId)
        {
            if (!_sourcePairIdsBySkill.TryGetValue(skillId, out var sourcePairIds))
            {
                sourcePairIds = new HashSet<string>(StringComparer.Ordinal);
                _sourcePairIdsBySkill[skillId] = sourcePairIds;
            }

            return sourcePairIds;
        }
    }

    public static IReadOnlyList<string> ExtractPromptUnderstandingSignals(string prompt)
        => prompt
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => new string(token
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray()))
            .Where(token => token.Length >= 4)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    public static IReadOnlyList<string> ExtractPromptContextSignals(SkillSelectionRequest request)
    {
        var signals = new HashSet<string>(StringComparer.Ordinal);
        foreach (var token in ExtractPromptUnderstandingSignals(request.Prompt))
            signals.Add($"prompt:{token}");

        foreach (var fact in request.HostFacts.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            foreach (var token in ExtractPromptUnderstandingSignals(fact.Key).Where(IsSafeContextToken))
                signals.Add($"host_key:{token}");
            foreach (var token in ExtractPromptUnderstandingSignals(fact.Value).Where(IsSafeContextToken))
                signals.Add($"host_value:{token}");
        }

        if (!string.IsNullOrWhiteSpace(request.ProjectRoot))
        {
            signals.Add("project_root_present");
            if (request.ProjectRoot.Contains(@":\", StringComparison.Ordinal)
                || request.ProjectRoot.Contains('/', StringComparison.Ordinal))
                signals.Add("project_root_path_like_redacted");
        }

        if (!string.IsNullOrWhiteSpace(request.UserPromptSourceEntryId))
        {
            signals.Add("provenance_ref_present");
            if (request.UserPromptSourceEntryId.Contains('-', StringComparison.Ordinal))
                signals.Add("provenance_ref_structured");
        }

        return signals.Order(StringComparer.Ordinal).ToArray();
    }

    private static bool IsSafeContextToken(string token)
        => token.Length is >= 4 and <= 32
            && !token.Any(char.IsDigit)
            && !token.Contains("users", StringComparison.OrdinalIgnoreCase)
            && !token.Contains("projects", StringComparison.OrdinalIgnoreCase);

    public static bool IsPromptContextPromptSignal(string signal)
        => signal.StartsWith("prompt:", StringComparison.Ordinal);

    public static bool IsPromptContextContextSignal(string signal)
        => signal.StartsWith("host_key:", StringComparison.Ordinal)
            || signal.StartsWith("host_value:", StringComparison.Ordinal)
            || string.Equals(signal, "project_root_present", StringComparison.Ordinal)
            || string.Equals(signal, "project_root_path_like_redacted", StringComparison.Ordinal)
            || string.Equals(signal, "provenance_ref_present", StringComparison.Ordinal)
            || string.Equals(signal, "provenance_ref_structured", StringComparison.Ordinal);

    private static string HashPromptKey(string normalizedPromptKey)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedPromptKey)));

    private static async Task<OperationResult<SkillSelectionCheckpoint>> ReadCheckpointAsync(
        string checkpointPath,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<SkillSelectionCheckpoint>.Fail("Load cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<SkillSelectionCheckpoint>.Fail("Checkpoint path is required.", "CHECKPOINT_PATH_REQUIRED");
        if (!File.Exists(checkpointPath))
            return OperationResult<SkillSelectionCheckpoint>.Fail("Checkpoint file not found.", "CHECKPOINT_NOT_FOUND");

        try
        {
            var json = await File.ReadAllTextAsync(checkpointPath, System.Text.Encoding.UTF8, ct);
            var checkpoint = JsonSerializer.Deserialize<SkillSelectionCheckpoint>(json, JsonOptions);
            var validation = ValidateCheckpoint(checkpoint);
            return validation.IsSuccess
                ? OperationResult<SkillSelectionCheckpoint>.Ok(checkpoint!)
                : OperationResult<SkillSelectionCheckpoint>.Fail(validation.ErrorMessage, validation.ErrorCode);
        }
        catch (Exception ex)
        {
            return OperationResult<SkillSelectionCheckpoint>.Fail(ex.Message, "SKILL_SELECTION_CHECKPOINT_READ_FAILED");
        }
    }

    private static async Task<OperationResult<SkillSelectionCheckpointManifest>> ReadManifestAsync(
        string manifestPath,
        CancellationToken ct)
    {
        try
        {
            var json = await File.ReadAllTextAsync(manifestPath, System.Text.Encoding.UTF8, ct);
            var manifest = JsonSerializer.Deserialize<SkillSelectionCheckpointManifest>(json, JsonOptions);
            return manifest is null
                ? OperationResult<SkillSelectionCheckpointManifest>.Fail("Manifest deserialized null.", "MANIFEST_DESERIALIZE_FAILED")
                : OperationResult<SkillSelectionCheckpointManifest>.Ok(manifest);
        }
        catch (Exception ex)
        {
            return OperationResult<SkillSelectionCheckpointManifest>.Fail(ex.Message, "SKILL_SELECTION_MANIFEST_READ_FAILED");
        }
    }

    private static OperationResult<bool> ValidateCheckpoint(SkillSelectionCheckpoint? checkpoint)
    {
        if (checkpoint is null)
            return OperationResult<bool>.Fail("Checkpoint deserialized null.", "CHECKPOINT_DESERIALIZE_FAILED");
        if (checkpoint.SchemaVersion != 1)
            return OperationResult<bool>.Fail("Checkpoint schema is unsupported.", "CHECKPOINT_SCHEMA_UNSUPPORTED");
        if (!string.Equals(checkpoint.RouteMode, RouteMode, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Checkpoint route mode is unsupported.", "CHECKPOINT_ROUTE_MODE_UNSUPPORTED");
        if (checkpoint.CandidatePositionWeights is null
            || checkpoint.TrainedPairIds is null
            || checkpoint.TrainedPromptKeys is null
            || checkpoint.PromptUnderstanding is null
            || checkpoint.PromptUnderstanding.SkillProfiles is null
            || checkpoint.TaskUnderstanding is null
            || checkpoint.TaskUnderstanding.SkillProfiles is null
            || checkpoint.TaskType is null
            || checkpoint.TaskType.SkillProfiles is null
            || checkpoint.Routing is null
            || checkpoint.Routing.SkillProfiles is null
            || checkpoint.SkillCandidates is null
            || checkpoint.SkillCandidates.SkillProfiles is null
            || checkpoint.SkillSelection is null
            || checkpoint.SkillSelection.SkillProfiles is null
            || checkpoint.PromptContext is null
            || checkpoint.PromptContext.SkillProfiles is null
            || checkpoint.ResultGap is null
            || checkpoint.ResultGap.SkillProfiles is null
            || checkpoint.OutputValidation is null
            || checkpoint.OutputValidation.SkillProfiles is null
            || checkpoint.TaskCompletion is null
            || checkpoint.TaskCompletion.SkillProfiles is null
            || checkpoint.IntermediateChecks is null
            || checkpoint.IntermediateChecks.SkillProfiles is null)
            return OperationResult<bool>.Fail("Checkpoint is missing trained state sections.", "CHECKPOINT_STATE_SECTIONS_REQUIRED");
        if (!string.Equals(checkpoint.PromptUnderstanding.UnitId, PromptUnderstandingUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Prompt understanding unit id is unsupported.", "PROMPT_UNDERSTANDING_UNIT_UNSUPPORTED");
        if (checkpoint.PromptUnderstanding.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Prompt understanding unit must not claim full semantic quality.", "PROMPT_UNDERSTANDING_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.TaskUnderstanding.UnitId, TaskUnderstandingUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Task understanding unit id is unsupported.", "TASK_UNDERSTANDING_UNIT_UNSUPPORTED");
        if (checkpoint.TaskUnderstanding.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Task understanding unit must not claim full semantic quality.", "TASK_UNDERSTANDING_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.TaskType.UnitId, TaskTypeUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Task type unit id is unsupported.", "TASK_TYPE_UNIT_UNSUPPORTED");
        if (checkpoint.TaskType.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Task type unit must not claim full semantic quality.", "TASK_TYPE_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.Routing.UnitId, RoutingUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Routing unit id is unsupported.", "ROUTING_UNIT_UNSUPPORTED");
        if (checkpoint.Routing.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Routing unit must not claim full semantic quality.", "ROUTING_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.SkillCandidates.UnitId, SkillCandidatesUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Skill candidates unit id is unsupported.", "SKILL_CANDIDATES_UNIT_UNSUPPORTED");
        if (checkpoint.SkillCandidates.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Skill candidates unit must not claim full semantic quality.", "SKILL_CANDIDATES_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.SkillSelection.UnitId, SkillSelectionUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Skill selection unit id is unsupported.", "SKILL_SELECTION_UNIT_UNSUPPORTED");
        if (checkpoint.SkillSelection.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Skill selection unit must not claim full semantic quality.", "SKILL_SELECTION_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.PromptContext.UnitId, PromptContextUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Prompt context unit id is unsupported.", "PROMPT_CONTEXT_UNIT_UNSUPPORTED");
        if (checkpoint.PromptContext.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Prompt context unit must not claim full semantic quality.", "PROMPT_CONTEXT_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.ResultGap.UnitId, ResultGapUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Result gap unit id is unsupported.", "RESULT_GAP_UNIT_UNSUPPORTED");
        if (checkpoint.ResultGap.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Result gap unit must not claim full semantic quality.", "RESULT_GAP_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.OutputValidation.UnitId, OutputValidationUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Output validation unit id is unsupported.", "OUTPUT_VALIDATION_UNIT_UNSUPPORTED");
        if (checkpoint.OutputValidation.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Output validation unit must not claim full semantic quality.", "OUTPUT_VALIDATION_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.TaskCompletion.UnitId, TaskCompletionUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Task completion unit id is unsupported.", "TASK_COMPLETION_UNIT_UNSUPPORTED");
        if (checkpoint.TaskCompletion.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Task completion unit must not claim full semantic quality.", "TASK_COMPLETION_QUALITY_CLAIM_UNSUPPORTED");
        if (!string.Equals(checkpoint.IntermediateChecks.UnitId, IntermediateChecksUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Intermediate checks unit id is unsupported.", "INTERMEDIATE_CHECKS_UNIT_UNSUPPORTED");
        if (checkpoint.IntermediateChecks.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Intermediate checks unit must not claim full semantic quality.", "INTERMEDIATE_CHECKS_QUALITY_CLAIM_UNSUPPORTED");

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateManifest(
        SkillSelectionCheckpointManifest manifest,
        LearningMachineExport export,
        string checkpointPath,
        string manifestPath)
    {
        if (manifest.SchemaVersion != 1)
            return OperationResult<bool>.Fail("Manifest schema is unsupported.", "MANIFEST_SCHEMA_UNSUPPORTED");
        if (!string.Equals(manifest.MachineId, MachineId, StringComparison.Ordinal)
            || !string.Equals(manifest.MachineId, export.MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest machine id mismatch.", "MANIFEST_MACHINE_ID_MISMATCH");
        if (!string.Equals(manifest.DomainTag, DomainTag, StringComparison.Ordinal)
            || !string.Equals(manifest.DomainTag, export.DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest domain tag mismatch.", "MANIFEST_DOMAIN_TAG_MISMATCH");
        if (!string.Equals(manifest.Version, export.Version, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest version mismatch.", "MANIFEST_VERSION_MISMATCH");
        if (manifest.CheckpointSchemaVersion != 1)
            return OperationResult<bool>.Fail("Manifest checkpoint schema is unsupported.", "MANIFEST_CHECKPOINT_SCHEMA_UNSUPPORTED");
        if (string.IsNullOrWhiteSpace(manifest.CheckpointRef))
            return OperationResult<bool>.Fail("Manifest checkpoint ref is required.", "MANIFEST_CHECKPOINT_REF_REQUIRED");
        if (string.IsNullOrWhiteSpace(manifest.ManifestRef))
            return OperationResult<bool>.Fail("Manifest ref is required.", "MANIFEST_REF_REQUIRED");
        var manifestDirectory = Path.GetDirectoryName(manifestPath) ?? string.Empty;
        var resolvedCheckpointRef = ResolveManifestReference(manifestDirectory, manifest.CheckpointRef);
        var resolvedManifestRef = ResolveManifestReference(manifestDirectory, manifest.ManifestRef);
        if (!string.Equals(resolvedCheckpointRef, checkpointPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<bool>.Fail("Manifest checkpoint ref mismatch.", "MANIFEST_CHECKPOINT_REF_MISMATCH");
        if (!string.Equals(resolvedManifestRef, manifestPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<bool>.Fail("Manifest ref mismatch.", "MANIFEST_REF_MISMATCH");
        if (manifest.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("SkillSelection persistence import must not claim full semantic quality.", "SEMANTIC_QUALITY_CLAIM_UNSUPPORTED");

        return OperationResult<bool>.Ok(true);
    }

    private static string ResolveManifestReference(string manifestDirectory, string reference)
        => Path.GetFullPath(Path.IsPathRooted(reference) ? reference : Path.Combine(manifestDirectory, reference));

    private static string CreatePortableManifestReference(string manifestDirectory, string targetPath)
    {
        var relative = Path.GetRelativePath(manifestDirectory, targetPath);
        return relative.StartsWith("..", StringComparison.Ordinal)
            ? targetPath
            : relative.Replace(Path.DirectorySeparatorChar, '/');
    }

    private static void EnsureDirectoryFor(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }

    private static async Task WriteJsonAtomicallyAsync<T>(
        string path,
        T value,
        CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        var tmpPath = path + ".tmp";
        await File.WriteAllTextAsync(tmpPath, json, Utf8NoBom, ct);
        File.Move(tmpPath, path, overwrite: true);
    }
}
