// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class OrchestrationPlanMachine
{
    public const string MachineId = "orchestration-plan-machine";
    public const string DomainTag = "ProjectWikiDomain.OrchestrationPlan";
    private const int CurrentCheckpointSchemaVersion = 3;
    private const double EvaluationAccuracyGateThreshold = 0.75;
    private const string BaselineUnverifiedRefSegment = "baseline_unverified";

    private readonly PromptFlowGroundingModel _promptFlowGroundingModel = new();
    private readonly OrchestrationKnnStore _mode2Knn = new();
    private readonly OrchestrationKnnStore _humanExplanationKnn = new();
    private readonly TrainedRouteComposerUnit _routeComposer = new();
    private readonly HashSet<string> _trainedPairIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _mode1PromotedPairIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _mode2PromotedPairIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _mode3PromotedPairIds = new(StringComparer.Ordinal);
    private readonly List<LearningBatchResult> _trainingHistory = [];

    private string _currentVersion = "0.1.0";
    private double _trainingProgressScore;
    private OrchestrationPlanEvaluationGate? _lastEvaluationGate;

    public int TrainOrchestrationBatchCallCount { get; private set; }

    public double TrainingProgressScore => _trainingProgressScore;

    public OrchestrationPlanEvaluationGate? LastEvaluationGate => _lastEvaluationGate;

    public IReadOnlyCollection<string> TrainedPairIds => _trainedPairIds;

    public IReadOnlyCollection<string> Mode1PromotedPairIds => _mode1PromotedPairIds;

    public IReadOnlyCollection<string> Mode2PromotedPairIds => _mode2PromotedPairIds;

    public IReadOnlyCollection<string> Mode3PromotedPairIds => _mode3PromotedPairIds;

    public int Mode1FlowGroundingKnnSize => _promptFlowGroundingModel.KnnSize;

    public int Mode1FlowGroundingWeightsSize => _promptFlowGroundingModel.WeightsSize;

    public int Mode2KnnStoreSize => _mode2Knn.Count;

    public int Mode2PatternMapSize => _mode2Knn.ExportPairs().Select(pair => pair.SourceBundleId).Distinct(StringComparer.Ordinal).Count();

    public int Mode3KnnStoreSize => _humanExplanationKnn.Count;

    public int Mode3HumanExplanationPatternMapSize => ExportPatternMap(_humanExplanationKnn).Count;

    public Task<OperationResult<OrchestrationGenerationResult>> GeneratePlanAsync(
        string prompt,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<OrchestrationGenerationResult>.Fail("Generation cancelled.", "CANCELLED"));

        var contract = HumanIntentBoundaryClassifier.Classify(prompt);
        var warnings = new List<string>();

        if (contract.Kind == IntentKind.SkillLoadSessionInit)
        {
            return Task.FromResult(OperationResult<OrchestrationGenerationResult>.Ok(new OrchestrationGenerationResult(
                "",
                GenerationRouteMode.Passthrough,
                BoundarySource.Classifier,
                false,
                contract.Kind,
                null,
                [])));
        }

        if (contract.RequiresApprovalGate)
            warnings.Add("PLAN_ONLY_PENDING_APPROVAL");

        if (contract.Kind == IntentKind.HumanOrchestrationExplanation)
        {
            var nearest = _humanExplanationKnn.FindNearest(prompt, PairMode.Mode3HumanExplanation);
            var text = _routeComposer.RenderHumanExplanation(nearest, contract, prompt);
            return Task.FromResult(OperationResult<OrchestrationGenerationResult>.Ok(new OrchestrationGenerationResult(
                text,
                GenerationRouteMode.Mode3HumanExplanation,
                BoundarySource.Classifier,
                true,
                contract.Kind,
                null,
                warnings)));
        }

        var grounding = _promptFlowGroundingModel.TryGround(prompt, contract);
        if (grounding is not null &&
            grounding.Confidence >= 0.75 &&
            AllowsLearnedGrounding(grounding, contract))
        {
            var learned = _promptFlowGroundingModel.FindLearnedPair(prompt, contract, grounding);
            if (learned is not null)
            {
                var text = _routeComposer.RenderLearnedFlowPlan(learned, grounding, contract, prompt);
                return Task.FromResult(OperationResult<OrchestrationGenerationResult>.Ok(new OrchestrationGenerationResult(
                    text,
                    GenerationRouteMode.LearnedFlowGrounding,
                    BoundarySource.Classifier,
                    true,
                    contract.Kind,
                    grounding,
                    warnings)));
            }
        }

        if (contract.AllowedFlowScope.Any())
            warnings.Add("learned_grounding_low_confidence_fallback_to_mode2");

        var nearestPlan = _mode2Knn.FindNearest(prompt, PairMode.Mode2Generalization);
        var planText = _routeComposer.RenderMode2Plan(nearestPlan, contract, prompt);
        return Task.FromResult(OperationResult<OrchestrationGenerationResult>.Ok(new OrchestrationGenerationResult(
            planText,
            GenerationRouteMode.Mode2Generalization,
            BoundarySource.Classifier,
            true,
            contract.Kind,
            null,
            warnings)));
    }

    public async Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<OrchestrationPlanDomain> batch,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningBatchResult>.Fail("Training cancelled.", "CANCELLED");
        if (batch is null)
            return OperationResult<LearningBatchResult>.Fail("Training batch is required.", "BATCH_REQUIRED");

        var pairs = new List<OrchestrationTrainingPair>();
        foreach (var item in batch.TrainingItems)
        {
            var pair = JsonSerializer.Deserialize<OrchestrationTrainingPair>(
                item.ChosenResponse,
                OrchestrationPlanJsonOptions.Standard);
            if (pair is not null)
                pairs.Add(pair);
        }

        var accuracyBefore = GetExportAccuracyFromGate();
        var result = await TrainOrchestrationBatchAsync(new OrchestrationTrainingBatch(batch.BatchId, pairs), ct);
        if (!result.IsSuccess)
            return OperationResult<LearningBatchResult>.Fail(result.ErrorMessage, result.ErrorCode);
        var accuracyAfter = GetExportAccuracyFromGate();

        var batchResult = new LearningBatchResult(
            batch.BatchId,
            result.Value!.RowsTrained,
            accuracyBefore,
            accuracyAfter,
            0.03,
            result.Value.CpuMs,
            result.Value.PeakRamMb,
            result.Value.ElapsedMs,
            ["prompt-flow-grounding-model", "trained-route-composer"]);
        return OperationResult<LearningBatchResult>.Ok(batchResult);
    }

    public Task<OperationResult<OrchestrationTrainingBatchResult>> TrainOrchestrationBatchAsync(
        OrchestrationTrainingBatch batch,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<OrchestrationTrainingBatchResult>.Fail("Training cancelled.", "CANCELLED"));
        if (batch is null)
            return Task.FromResult(OperationResult<OrchestrationTrainingBatchResult>.Fail("Training batch is required.", "BATCH_REQUIRED"));

        var start = Environment.TickCount64;
        TrainOrchestrationBatchCallCount++;
        var trained = new List<string>();
        var mode1 = 0;
        var mode2 = 0;
        var mode3 = 0;
        var accuracyBefore = GetExportAccuracyFromGate();

        foreach (var pair in batch.Pairs)
        {
            if (_trainedPairIds.Contains(pair.PairId))
                continue;

            if (pair.Mode == PairMode.Mode1LearnedFlowGrounding)
            {
                _promptFlowGroundingModel.Train(pair);
                _mode1PromotedPairIds.Add(pair.PairId);
                mode1++;
            }
            else if (pair.Mode == PairMode.Mode2Generalization)
            {
                _mode2Knn.Add(pair);
                _mode2PromotedPairIds.Add(pair.PairId);
                mode2++;
            }
            else if (pair.Mode == PairMode.Mode3HumanExplanation)
            {
                _humanExplanationKnn.Add(pair);
                _mode3PromotedPairIds.Add(pair.PairId);
                mode3++;
            }

            _trainedPairIds.Add(pair.PairId);
            trained.Add(pair.PairId);
        }

        if (trained.Count > 0)
        {
            _trainingProgressScore = Math.Min(1.0, _trainingProgressScore + 0.05 + trained.Count * 0.001);
            _lastEvaluationGate = null;
        }

        IncrementPatchVersion();

        var elapsed = Environment.TickCount64 - start;
        var accuracyAfter = GetExportAccuracyFromGate();
        var result = new OrchestrationTrainingBatchResult(
            batch.BatchId,
            trained.Count,
            mode1,
            mode2,
            mode3,
            trained,
            elapsed,
            GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024),
            elapsed);

        _trainingHistory.Add(new LearningBatchResult(
            batch.BatchId,
            trained.Count,
            accuracyBefore,
            accuracyAfter,
            0.03,
            result.CpuMs,
            result.PeakRamMb,
            result.ElapsedMs,
            ["prompt-flow-grounding-model", "trained-route-composer"]));

        return Task.FromResult(OperationResult<OrchestrationTrainingBatchResult>.Ok(result));
    }

    public async Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<OrchestrationPlanDomain> evalSet,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningEvaluationResult>.Fail("Evaluation cancelled.", "CANCELLED");
        if (evalSet is null)
            return OperationResult<LearningEvaluationResult>.Fail("Evaluation set is required.", "EVAL_SET_REQUIRED");
        if (evalSet.EvaluationItems.Count == 0)
            return OperationResult<LearningEvaluationResult>.Fail("Evaluation set must contain rows.", "EVAL_SET_EMPTY");

        var correct = 0;
        foreach (var item in evalSet.EvaluationItems)
        {
            var generated = await GeneratePlanAsync(item.Prompt, ct);
            if (generated.IsSuccess &&
                generated.Value!.PlanText.Contains(item.ExpectedResponse, StringComparison.OrdinalIgnoreCase))
            {
                correct++;
            }
        }

        var accuracy = correct / (double)evalSet.EvaluationItems.Count;
        var passesGate = accuracy >= EvaluationAccuracyGateThreshold;
        var gateNote = passesGate
            ? $"orchestration gate passed: accuracy={accuracy:F4} >= {EvaluationAccuracyGateThreshold:F4}"
            : $"orchestration gate failed: accuracy={accuracy:F4} < {EvaluationAccuracyGateThreshold:F4}";
        _lastEvaluationGate = new OrchestrationPlanEvaluationGate(
            evalSet.EvaluationId,
            evalSet.EvaluationItems.Count,
            accuracy,
            passesGate,
            gateNote,
            DateTimeOffset.UtcNow,
            false);
        var result = new LearningEvaluationResult(
            evalSet.EvaluationId,
            evalSet.EvaluationItems.Count,
            accuracy,
            0.03,
            passesGate,
            gateNote);
        return OperationResult<LearningEvaluationResult>.Ok(result);
    }

    public Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningMachineExport>.Fail("Export cancelled.", "CANCELLED"));

        var exportedAtUtc = DateTimeOffset.UtcNow;
        return Task.FromResult(OperationResult<LearningMachineExport>.Ok(new LearningMachineExport(
            MachineId,
            DomainTag,
            _currentVersion,
            BuildPortableCheckpointRef(),
            BuildPortableManifestRef(),
            GetExportAccuracyFromGate(),
            exportedAtUtc)));
    }

    public Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<bool>.Fail("Import cancelled.", "CANCELLED"));
        if (export is null)
            return Task.FromResult(OperationResult<bool>.Fail("Export is required.", "EXPORT_REQUIRED"));
        if (!string.Equals(export.MachineId, MachineId, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<bool>.Fail("Machine id mismatch.", "MACHINE_ID_MISMATCH"));

        _currentVersion = export.Version;
        _trainingProgressScore = 0.0;
        _lastEvaluationGate = CreateImportedEvaluationGate(export);
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public OrchestrationPlanMachineCheckpoint ExportCheckpoint(OrchestrationLoopCheckpoint? loop = null)
        => new(
            CurrentCheckpointSchemaVersion,
            MachineId,
            DomainTag,
            _currentVersion,
            DateTimeOffset.UtcNow,
            _promptFlowGroundingModel.ExportPairs(),
            _promptFlowGroundingModel.ExportWeights(),
            _mode2Knn.ExportPairs(),
            ExportPatternMap(_mode2Knn),
            _humanExplanationKnn.ExportPairs(),
            ExportPatternMap(_humanExplanationKnn),
            _trainedPairIds.Order(StringComparer.Ordinal).ToArray(),
            _mode1PromotedPairIds.Order(StringComparer.Ordinal).ToArray(),
            _mode2PromotedPairIds.Order(StringComparer.Ordinal).ToArray(),
            _mode3PromotedPairIds.Order(StringComparer.Ordinal).ToArray(),
            loop,
            _trainingProgressScore,
            _lastEvaluationGate);

    public OperationResult<bool> ImportCheckpoint(OrchestrationPlanMachineCheckpoint checkpoint)
    {
        if (checkpoint is null)
            return OperationResult<bool>.Fail("Checkpoint is required.", "CHECKPOINT_REQUIRED");
        if (checkpoint.SchemaVersion is not 2 and not CurrentCheckpointSchemaVersion)
            return OperationResult<bool>.Fail("Checkpoint schema is unsupported.", "CHECKPOINT_SCHEMA_UNSUPPORTED");
        if (!string.Equals(checkpoint.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Machine id mismatch.", "MACHINE_ID_MISMATCH");

        _currentVersion = checkpoint.Version;
        _trainingProgressScore = checkpoint.TrainingProgressScore;
        _lastEvaluationGate = checkpoint.LastEvaluationGate is null
            ? null
            : checkpoint.LastEvaluationGate with { SemanticQualityClaimed = false };
        _promptFlowGroundingModel.ImportState(checkpoint.FlowGroundingKnn, checkpoint.FlowGroundingWeights);
        _mode2Knn.ImportPairs(checkpoint.KnnStore);
        _humanExplanationKnn.ImportPairs(checkpoint.HumanExplanationKnn);
        Replace(_trainedPairIds, checkpoint.TrainedPairIds);
        Replace(_mode1PromotedPairIds, checkpoint.Mode1PromotedPairIds);
        Replace(_mode2PromotedPairIds, checkpoint.Mode2PromotedPairIds);
        Replace(_mode3PromotedPairIds, checkpoint.Mode3PromotedPairIds);
        return OperationResult<bool>.Ok(true);
    }

    public async Task<OperationResult<LearningMachineExport>> SaveCheckpointAsync(
        string checkpointPath,
        OrchestrationLoopCheckpoint? loop = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<LearningMachineExport>.Fail("Checkpoint path is required.", "CHECKPOINT_PATH_REQUIRED");

        try
        {
            var directory = Path.GetDirectoryName(checkpointPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(ExportCheckpoint(loop), OrchestrationPlanJsonOptions.Standard);
            var tmp = checkpointPath + ".tmp";
            await File.WriteAllTextAsync(tmp, json, System.Text.Encoding.UTF8, ct);
            File.Move(tmp, checkpointPath, overwrite: true);
        }
        catch (Exception ex)
        {
            return OperationResult<LearningMachineExport>.Fail(ex.Message, "CHECKPOINT_WRITE_FAILED");
        }

        var checkpointFullPath = Path.GetFullPath(checkpointPath);
        return OperationResult<LearningMachineExport>.Ok(new LearningMachineExport(
            MachineId,
            DomainTag,
            _currentVersion,
            checkpointFullPath,
            BuildSavedManifestRef(checkpointFullPath),
            GetExportAccuracyFromGate(),
            DateTimeOffset.UtcNow));
    }

    public static async Task<OperationResult<OrchestrationPlanMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<OrchestrationPlanMachineLoadResult>.Fail("Load cancelled.", "CANCELLED");
        if (!File.Exists(checkpointPath))
            return OperationResult<OrchestrationPlanMachineLoadResult>.Fail("Checkpoint not found.", "CHECKPOINT_NOT_FOUND");

        try
        {
            var json = await File.ReadAllTextAsync(checkpointPath, System.Text.Encoding.UTF8, ct);
            var checkpoint = JsonSerializer.Deserialize<OrchestrationPlanMachineCheckpoint>(
                json,
                OrchestrationPlanJsonOptions.Standard);
            if (checkpoint is null)
                return OperationResult<OrchestrationPlanMachineLoadResult>.Fail("Checkpoint deserialized null.", "CHECKPOINT_DESERIALIZE_FAILED");

            var machine = new OrchestrationPlanMachine();
            var import = machine.ImportCheckpoint(checkpoint);
            if (!import.IsSuccess)
                return OperationResult<OrchestrationPlanMachineLoadResult>.Fail(import.ErrorMessage, import.ErrorCode);

            return OperationResult<OrchestrationPlanMachineLoadResult>.Ok(new OrchestrationPlanMachineLoadResult(
                machine,
                checkpoint.OrchestrationLoop));
        }
        catch (Exception ex)
        {
            return OperationResult<OrchestrationPlanMachineLoadResult>.Fail(ex.Message, "CHECKPOINT_READ_FAILED");
        }
    }

    private static bool AllowsLearnedGrounding(FlowGroundingResult grounding, HumanIntentContract contract)
    {
        if (contract.Kind is IntentKind.DocumentationMigration or IntentKind.PhaseContinuation or IntentKind.ArchitectureReview or IntentKind.TestingPlan)
            return false;
        if (contract.ForbiddenFlowScope.Contains(grounding.FlowRef, StringComparer.Ordinal))
            return false;
        if (contract.AllowedFlowScope.Count > 0 && !contract.AllowedFlowScope.Contains(grounding.FlowRef, StringComparer.Ordinal))
            return false;
        return true;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ExportPatternMap(OrchestrationKnnStore store)
        => store.ExportPairs()
            .GroupBy(pair => pair.SourceBundleId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(pair => pair.PairId).Order(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

    private static void Replace(HashSet<string> target, IReadOnlyList<string> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }

    private void IncrementPatchVersion()
    {
        var parts = _currentVersion.Split('.');
        if (parts.Length == 3 && int.TryParse(parts[2], out var patch))
            _currentVersion = $"{parts[0]}.{parts[1]}.{patch + 1}";
    }

    private double GetExportAccuracyFromGate()
        => _lastEvaluationGate is { PassesGate: true } gate
            ? gate.Accuracy
            : 0.0;

    private string BuildPortableCheckpointRef()
    {
        var gate = GetLastPassedEvaluationGate();
        if (gate is null)
            return $"{MachineId}/{_currentVersion}/{BaselineUnverifiedRefSegment}/checkpoint";

        return $"{MachineId}/{_currentVersion}/eval/{SanitizeRefSegment(gate.EvaluationId)}/checkpoint";
    }

    private string BuildPortableManifestRef()
    {
        var gate = GetLastPassedEvaluationGate();
        if (gate is null)
            return $"{MachineId}/{_currentVersion}/{BaselineUnverifiedRefSegment}/manifest.json";

        return $"{MachineId}/{_currentVersion}/eval/{SanitizeRefSegment(gate.EvaluationId)}/manifest.json";
    }

    private string BuildSavedManifestRef(string checkpointFullPath)
    {
        var gate = GetLastPassedEvaluationGate();
        if (gate is null)
            return $"{checkpointFullPath}#{BaselineUnverifiedRefSegment}";

        return $"{checkpointFullPath}#eval/{SanitizeRefSegment(gate.EvaluationId)}";
    }

    private OrchestrationPlanEvaluationGate? GetLastPassedEvaluationGate()
        => _lastEvaluationGate is { PassesGate: true } gate
            ? gate
            : null;

    private static OrchestrationPlanEvaluationGate? CreateImportedEvaluationGate(LearningMachineExport export)
    {
        if (export.AccuracyAtExport <= 0.0)
            return null;
        if (ContainsBaselineUnverifiedRef(export.CheckpointRef) || ContainsBaselineUnverifiedRef(export.ManifestRef))
            return null;
        if (!TryExtractEvaluationId(export.ManifestRef, out var evaluationId) &&
            !TryExtractEvaluationId(export.CheckpointRef, out evaluationId))
        {
            return null;
        }

        return new OrchestrationPlanEvaluationGate(
            evaluationId,
            0,
            export.AccuracyAtExport,
            true,
            $"imported export gate: accuracy={export.AccuracyAtExport:F4}",
            export.ExportedAtUtc,
            false);
    }

    private static bool ContainsBaselineUnverifiedRef(string reference)
        => reference.Contains(BaselineUnverifiedRefSegment, StringComparison.OrdinalIgnoreCase);

    private static bool TryExtractEvaluationId(string reference, out string evaluationId)
    {
        evaluationId = string.Empty;
        var marker = reference.Contains("#eval/", StringComparison.Ordinal)
            ? "#eval/"
            : "/eval/";
        var markerIndex = reference.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
            return false;

        var start = markerIndex + marker.Length;
        var end = reference.IndexOfAny(new[] { '/', '#', '?' }, start);
        if (end < 0)
            end = reference.Length;

        evaluationId = reference[start..end];
        return !string.IsNullOrWhiteSpace(evaluationId);
    }

    private static string SanitizeRefSegment(string value)
    {
        var chars = value.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.'
            ? ch
            : '-').ToArray();
        var sanitized = new string(chars).Trim('-');
        return string.IsNullOrWhiteSpace(sanitized)
            ? "evaluation"
            : sanitized;
    }
}
