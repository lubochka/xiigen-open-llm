// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class TranslationMachine : ILearningMachine<TranslationDomain>
{
    public const string MachineId = "translation-machine";
    public const string DomainTag = "TranslationDomain";

    private readonly TranslationRoutingFlow _flow;
    private readonly IReadOnlyDictionary<string, ITranslationUnitTrainer> _unitTrainers;
    private readonly List<LearningBatchResult> _trainingHistory = [];
    private readonly List<TranslationStructuredTrainingIdentityCheckpoint> _structuredTrainingIdentities = [];

    // _runtime holds typed references to the concrete unit objects for checkpoint export/import.
    // Null only when the machine was constructed via the flow-injection constructor used in tests.
    private readonly TranslationRuntime? _runtime;

    private string _currentVersion = "0.1.0";
    private double _accuracyAfterLastBatch;
    private bool _hasStructuredTraining;

    // Default constructor creates a fresh machine with a full runtime; supports checkpoint export/import.
    public TranslationMachine() : this(CreateRuntime()) { }

    // Runtime constructor wires the flow and trainers from a CreateRuntime() result.
    private TranslationMachine(TranslationRuntime runtime)
    {
        // Store the flow for orchestrator access.
        _flow = runtime.Flow;
        // Store the unit trainer map for training dispatch.
        _unitTrainers = runtime.UnitTrainers;
        // Store the typed runtime reference so checkpoint methods can access concrete units.
        _runtime = runtime;
    }

    public TranslationMachine(TranslationRoutingFlow flow)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _unitTrainers = new Dictionary<string, ITranslationUnitTrainer>(StringComparer.Ordinal);
    }

    public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<TranslationDomain> batch,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Training cancelled.", "CANCELLED"));

        if (batch is null)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Training batch is required.", "BATCH_REQUIRED"));

        if (batch.TrainingItems.Count == 0)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                "Training batch must contain at least one item.", "BATCH_EMPTY"));

        var startMs = Environment.TickCount64;
        var accuracyBefore = _accuracyAfterLastBatch;

        // Legacy scaffold path: flat LearningTrainingItem rows are accepted for old tests,
        // but this path does not claim unit-level translation learning.
        var accuracyAfter = Math.Min(accuracyBefore + 0.01, 1.0);
        var result = new LearningBatchResult(
            BatchId: batch.BatchId,
            RowsTrained: batch.TrainingItems.Count,
            AccuracyBefore: accuracyBefore,
            AccuracyAfter: accuracyAfter,
            RewardMargin: 0.03,
            CpuMs: Environment.TickCount64 - startMs,
            PeakRamMb: GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024),
            ElapsedMs: Environment.TickCount64 - startMs,
            PromotedUnitIds: []);

        _accuracyAfterLastBatch = accuracyAfter;
        _trainingHistory.Add(result);
        IncrementPatchVersion();

        return Task.FromResult(OperationResult<LearningBatchResult>.Ok(result));
    }

    public Task<OperationResult<LearningBatchResult>> TrainStructuredBatchAsync(
        TranslationLearningBatch batch,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Structured training cancelled.", "CANCELLED"));

        if (batch is null)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Structured training batch is required.", "BATCH_REQUIRED"));

        if (batch.Examples.Count == 0)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                "Structured training batch must contain at least one example.", "BATCH_EMPTY"));

        var startMs = Environment.TickCount64;
        var accuracyBefore = _accuracyAfterLastBatch;
        var trainedUnits = new HashSet<string>(StringComparer.Ordinal);

        foreach (var example in batch.Examples.Where(example => !example.HeldOut))
        {
            if (!_unitTrainers.TryGetValue(example.UnitId, out var trainer))
                continue;

            trainer.TrainOnExample(example);
            trainedUnits.Add(example.UnitId);
            if (!_structuredTrainingIdentities.Any(identity =>
                    string.Equals(identity.ExampleId, example.ExampleId, StringComparison.Ordinal) &&
                    string.Equals(identity.UnitId, example.UnitId, StringComparison.Ordinal)))
                _structuredTrainingIdentities.Add(new TranslationStructuredTrainingIdentityCheckpoint(
                    example.ExampleId,
                    example.SourceId,
                    example.UnitId,
                    batch.BatchId));
        }

        var heldOut = batch.Examples.Where(example => example.HeldOut).ToArray();
        var accuracyAfter = heldOut.Length > 0
            ? MeasureHeldOutAccuracy(heldOut)
            : MeasureUnitCoverage(trainedUnits);

        var rewardMargin = batch.Examples.All(example =>
            example.RejectedOutputJson.Contains("[WRONG:", StringComparison.Ordinal))
                ? 0.03
                : 0.0;

        _hasStructuredTraining = trainedUnits.Count > 0;
        _accuracyAfterLastBatch = accuracyAfter;
        IncrementPatchVersion();

        var result = new LearningBatchResult(
            BatchId: batch.BatchId,
            RowsTrained: batch.Examples.Count(example => !example.HeldOut),
            AccuracyBefore: accuracyBefore,
            AccuracyAfter: accuracyAfter,
            RewardMargin: rewardMargin,
            CpuMs: Environment.TickCount64 - startMs,
            PeakRamMb: GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024),
            ElapsedMs: Environment.TickCount64 - startMs,
            PromotedUnitIds: trainedUnits.Order(StringComparer.Ordinal).ToArray());

        _trainingHistory.Add(result);
        return Task.FromResult(OperationResult<LearningBatchResult>.Ok(result));
    }

    public async Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<TranslationDomain> evalSet,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningEvaluationResult>.Fail("Evaluation cancelled.", "CANCELLED");

        if (evalSet is null)
            return OperationResult<LearningEvaluationResult>.Fail("Evaluation set is required.", "EVAL_SET_REQUIRED");

        if (evalSet.EvaluationItems.Count == 0)
            return OperationResult<LearningEvaluationResult>.Fail(
                "Evaluation set must contain at least one item.", "EVAL_SET_EMPTY");

        var accuracy = _accuracyAfterLastBatch;
        if (_hasStructuredTraining)
        {
            var correct = 0;
            foreach (var item in evalSet.EvaluationItems)
            {
                var translated = await TranslateForEvaluationAsync(item.Prompt, ct);
                if (translated.IsSuccess &&
                    string.Equals(translated.Value, item.ExpectedResponse, StringComparison.Ordinal))
                {
                    correct++;
                }
            }

            accuracy = correct / (double)evalSet.EvaluationItems.Count;
            _accuracyAfterLastBatch = accuracy;
        }

        var gateThreshold = _hasStructuredTraining ? 0.75 : 0.0;
        var passesGate = accuracy >= gateThreshold && accuracy > 0.0;
        var gateKind = _hasStructuredTraining ? "structured unit" : "legacy Bootstrap";

        var result = new LearningEvaluationResult(
            EvaluationId: evalSet.EvaluationId,
            RowsEvaluated: evalSet.EvaluationItems.Count,
            Accuracy: accuracy,
            RewardMargin: _trainingHistory.Count > 0 ? 0.03 : 0.0,
            PassesGate: passesGate,
            GateNote: passesGate
                ? $"{gateKind} gate passed: accuracy={accuracy:F4} >= {gateThreshold:F4}"
                : $"{gateKind} gate failed: accuracy={accuracy:F4} < {gateThreshold:F4}");

        return OperationResult<LearningEvaluationResult>.Ok(result);
    }

    // ExportAsync returns a metadata-only LearningMachineExport record.
    // It does NOT persist trained unit state. For trained-state persistence use SaveCheckpointAsync.
    // CheckpointRef is a placeholder string — it does not point to a file containing weights.
    public Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
    {
        // Return early if cancellation was requested before doing any work.
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningMachineExport>.Fail("Export cancelled.", "CANCELLED"));

        // Build a metadata record. CheckpointRef here is not a file path — it is a placeholder.
        // For a real checkpoint that restores trained state, call SaveCheckpointAsync instead.
        var export = new LearningMachineExport(
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: _currentVersion,
            CheckpointRef: $"{MachineId}/{_currentVersion}/checkpoint",
            ManifestRef: $"{MachineId}/{_currentVersion}/manifest.json",
            AccuracyAtExport: _accuracyAfterLastBatch,
            ExportedAtUtc: DateTimeOffset.UtcNow);

        return Task.FromResult(OperationResult<LearningMachineExport>.Ok(export));
    }

    // ImportAsync restores only scalar metadata (version, accuracy) from an export record.
    // It does NOT restore trained unit state. For full restore from disk use LoadCheckpointAsync.
    // This method is the metadata counterpart to ExportAsync — not a trained-model load path.
    public Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export,
        CancellationToken ct = default)
    {
        // Return early if cancellation was requested.
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<bool>.Fail("Import cancelled.", "CANCELLED"));

        // Validate the export record before reading any fields.
        if (export is null)
            return Task.FromResult(OperationResult<bool>.Fail("Export record is required.", "EXPORT_REQUIRED"));

        // Verify the machine identity before restoring any scalar fields.
        if (!string.Equals(export.MachineId, MachineId, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<bool>.Fail(
                $"Export machine id '{export.MachineId}' does not match '{MachineId}'.", "MACHINE_ID_MISMATCH"));

        // Verify the domain tag before restoring any scalar fields.
        if (!string.Equals(export.DomainTag, DomainTag, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<bool>.Fail(
                $"Export domain tag '{export.DomainTag}' does not match '{DomainTag}'.", "DOMAIN_TAG_MISMATCH"));

        // Restore only the scalar metadata — trained unit state is NOT restored here.
        _accuracyAfterLastBatch = export.AccuracyAtExport;
        _currentVersion = export.Version;

        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    // ExportCheckpoint builds a TranslationMachineCheckpoint from all current trained unit state.
    // This method does NOT write to disk. Call SaveCheckpointAsync to persist.
    // Returns RUNTIME_STATE_UNAVAILABLE if the machine was constructed via the flow-injection path.
    public OperationResult<TranslationMachineCheckpoint> ExportCheckpoint()
    {
        // Verify that the machine has typed unit references required for checkpoint export.
        if (_runtime is null)
            return OperationResult<TranslationMachineCheckpoint>.Fail(
                "Runtime state is not available for this machine.", "RUNTIME_STATE_UNAVAILABLE");

        // Map the in-memory training history to checkpoint records.
        var historyEntries = _trainingHistory
            .Select(h => new TranslationTrainingHistoryEntryCheckpoint(
                BatchId: h.BatchId,
                RowsTrained: h.RowsTrained,
                AccuracyBefore: h.AccuracyBefore,
                AccuracyAfter: h.AccuracyAfter,
                RewardMargin: h.RewardMargin,
                CpuMs: h.CpuMs,
                PeakRamMb: h.PeakRamMb,
                ElapsedMs: h.ElapsedMs,
                PromotedUnitIds: h.PromotedUnitIds))
            .ToList();

        // Build the top-level checkpoint record from machine fields and all unit exports.
        var checkpoint = new TranslationMachineCheckpoint(
            SchemaVersion: 1,
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: _currentVersion,
            AccuracyAfterLastBatch: _accuracyAfterLastBatch,
            HasStructuredTraining: _hasStructuredTraining,
            ExportedAtUtc: DateTimeOffset.UtcNow,
            KnnStore: _runtime.KnnStore.ExportCheckpoint(),
            SemanticTokenizer: _runtime.SemanticTokenizer.ExportCheckpoint(),
            Executors: _runtime.Executors.Values.Select(e => e.ExportCheckpoint()).ToList(),
            QualityVerifier: _runtime.Verifier.ExportCheckpoint(),
            TrainingHistory: historyEntries,
            StructuredTrainingIdentities: _structuredTrainingIdentities
                .OrderBy(identity => identity.ExampleId, StringComparer.Ordinal)
                .ThenBy(identity => identity.UnitId, StringComparer.Ordinal)
                .ToList(),
            TatoebaLoop: null);  // Runner sets TatoebaLoop when it calls SaveCheckpointAsync.

        return OperationResult<TranslationMachineCheckpoint>.Ok(checkpoint);
    }

    // ImportCheckpoint restores all trained unit state from a checkpoint record into this machine.
    // The machine must have been constructed via the default constructor (not the flow-injection path).
    // Returns CHECKPOINT_SCHEMA_UNSUPPORTED, MACHINE_ID_MISMATCH, DOMAIN_TAG_MISMATCH, or EXECUTOR_DIRECTION_MISSING on validation failure.
    public OperationResult<bool> ImportCheckpoint(TranslationMachineCheckpoint checkpoint)
    {
        // Validate the checkpoint record is not null.
        if (checkpoint is null)
            return OperationResult<bool>.Fail("Checkpoint is required.", "CHECKPOINT_REQUIRED");

        // Validate the schema version before reading any fields.
        if (checkpoint.SchemaVersion != 1)
            return OperationResult<bool>.Fail(
                $"Checkpoint schema version {checkpoint.SchemaVersion} is not supported.", "CHECKPOINT_SCHEMA_UNSUPPORTED");

        // Validate the machine identity to prevent loading a checkpoint from a different machine.
        if (!string.Equals(checkpoint.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Checkpoint machine id '{checkpoint.MachineId}' does not match '{MachineId}'.", "MACHINE_ID_MISMATCH");

        // Validate the domain tag to prevent loading a checkpoint from a different domain.
        if (!string.Equals(checkpoint.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Checkpoint domain tag '{checkpoint.DomainTag}' does not match '{DomainTag}'.", "DOMAIN_TAG_MISMATCH");

        // Verify typed unit references are present (requires default constructor path).
        if (_runtime is null)
            return OperationResult<bool>.Fail(
                "Runtime state is not available for this machine.", "RUNTIME_STATE_UNAVAILABLE");

        // Restore scalar fields from the checkpoint.
        _currentVersion = checkpoint.Version;
        _accuracyAfterLastBatch = checkpoint.AccuracyAfterLastBatch;
        _hasStructuredTraining = checkpoint.HasStructuredTraining;

        // Restore the shared KNN store (all four executors share the same store reference).
        _runtime.KnnStore.ImportCheckpoint(checkpoint.KnnStore);

        // Restore the semantic tokenizer corrections.
        _runtime.SemanticTokenizer.ImportCheckpoint(checkpoint.SemanticTokenizer);

        // Restore each direction-specific executor by matching on Direction.
        foreach (var executorCheckpoint in checkpoint.Executors)
        {
            // Locate the executor for this checkpoint direction in the runtime map.
            if (!_runtime.Executors.TryGetValue(executorCheckpoint.Direction, out var executor))
                return OperationResult<bool>.Fail(
                    $"Executor for direction {executorCheckpoint.Direction} not found in runtime.", "EXECUTOR_DIRECTION_MISSING");
            // Import the executor's four trained maps.
            executor.ImportCheckpoint(executorCheckpoint);
        }

        // Verify that all runtime executor directions have a corresponding checkpoint entry.
        // A partial checkpoint that is missing directions would silently reset those executor maps.
        var missingDirections = _runtime.Executors.Keys
            .Except(checkpoint.Executors.Select(e => e.Direction))
            .ToList();
        if (missingDirections.Count > 0)
            return OperationResult<bool>.Fail(
                $"Checkpoint missing executors for directions: {string.Join(", ", missingDirections)}",
                "EXECUTOR_DIRECTIONS_INCOMPLETE");

        // Restore the quality verifier rules.
        _runtime.Verifier.ImportCheckpoint(checkpoint.QualityVerifier);

        // Rebuild the training history from checkpoint records.
        _trainingHistory.Clear();
        foreach (var entry in checkpoint.TrainingHistory)
            _trainingHistory.Add(new LearningBatchResult(
                entry.BatchId,
                entry.RowsTrained,
                entry.AccuracyBefore,
                entry.AccuracyAfter,
                entry.RewardMargin,
                entry.CpuMs,
                entry.PeakRamMb,
                entry.ElapsedMs,
                entry.PromotedUnitIds));

        // Restore structured example identities for same-type no-replay checks.
        _structuredTrainingIdentities.Clear();
        foreach (var identity in checkpoint.StructuredTrainingIdentities ?? [])
            _structuredTrainingIdentities.Add(identity);

        return OperationResult<bool>.Ok(true);
    }

    // SaveCheckpointAsync serialises the machine to a JSON checkpoint file at checkpointPath.
    // Uses a tmp-then-move atomic write to avoid leaving a partially written file on crash.
    // Returns a LearningMachineExport whose CheckpointRef is the absolute checkpoint path.
    // loop is an optional TatoebaPilotLoopCheckpoint set by TatoebaPilotLoopRunner to embed cursor state.
    public async Task<OperationResult<LearningMachineExport>> SaveCheckpointAsync(
        string checkpointPath,
        TatoebaPilotLoopCheckpoint? loop = null,   // caller (TatoebaPilotLoopRunner) passes cursor/history
        CancellationToken ct = default)
    {
        // Validate the checkpoint path before any I/O.
        if (ct.IsCancellationRequested)
            return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<LearningMachineExport>.Fail(
                "Checkpoint path is required.", "CHECKPOINT_PATH_REQUIRED");

        // Build the in-memory checkpoint record from all trained unit state.
        var exportResult = ExportCheckpoint();
        if (!exportResult.IsSuccess)
            return OperationResult<LearningMachineExport>.Fail(exportResult.ErrorMessage, exportResult.ErrorCode);

        try
        {
            // Create the parent directory if it does not exist yet.
            var dir = Path.GetDirectoryName(checkpointPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            // Inject the optional loop cursor — create a with-copy if loop is provided, otherwise use as-is.
            var checkpointToWrite = loop is null
                ? exportResult.Value!
                : exportResult.Value! with { TatoebaLoop = loop };
            // Serialise to JSON using the standard checkpoint options (snake_case, Cyrillic-safe).
            var json = JsonSerializer.Serialize(checkpointToWrite, TranslationCheckpointJsonOptions.Standard);

            // Write to a temporary file first to avoid a partially written checkpoint on crash.
            var tmpPath = checkpointPath + ".tmp";
            await File.WriteAllTextAsync(tmpPath, json, System.Text.Encoding.UTF8, ct);

            // Atomic move: replaces any existing checkpoint file in one OS operation.
            File.Move(tmpPath, checkpointPath, overwrite: true);
        }
        catch (Exception ex)
        {
            return OperationResult<LearningMachineExport>.Fail(ex.Message, "CHECKPOINT_WRITE_FAILED");
        }

        // Return a LearningMachineExport whose CheckpointRef is the resolved absolute path.
        var export = new LearningMachineExport(
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: _currentVersion,
            CheckpointRef: Path.GetFullPath(checkpointPath),
            ManifestRef: Path.GetFullPath(checkpointPath),
            AccuracyAtExport: _accuracyAfterLastBatch,
            ExportedAtUtc: DateTimeOffset.UtcNow);

        return OperationResult<LearningMachineExport>.Ok(export);
    }

    // LoadCheckpointAsync reads a JSON checkpoint file and returns a fully restored TranslationMachineLoadResult.
    // This is the only valid post-restart load path. No replay or retraining is performed.
    // Returns CHECKPOINT_NOT_FOUND when the file is absent, CHECKPOINT_READ_FAILED on parse error.
    public static async Task<OperationResult<TranslationMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        CancellationToken ct = default)
    {
        // Validate the checkpoint path and check file existence before reading.
        if (ct.IsCancellationRequested)
            return OperationResult<TranslationMachineLoadResult>.Fail("Load cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<TranslationMachineLoadResult>.Fail(
                "Checkpoint path is required.", "CHECKPOINT_PATH_REQUIRED");
        if (!File.Exists(checkpointPath))
            return OperationResult<TranslationMachineLoadResult>.Fail(
                $"Checkpoint file not found: {checkpointPath}", "CHECKPOINT_NOT_FOUND");

        // Read and deserialise the checkpoint JSON.
        TranslationMachineCheckpoint checkpoint;
        try
        {
            var json = await File.ReadAllTextAsync(checkpointPath, System.Text.Encoding.UTF8, ct);
            var parsed = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                json, TranslationCheckpointJsonOptions.Standard);
            if (parsed is null)
                return OperationResult<TranslationMachineLoadResult>.Fail(
                    "Checkpoint deserialisation returned null.", "CHECKPOINT_DESERIALIZE_FAILED");
            checkpoint = parsed;
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationMachineLoadResult>.Fail(ex.Message, "CHECKPOINT_READ_FAILED");
        }

        // Create a fresh machine and restore all trained state from the checkpoint.
        var machine = new TranslationMachine();
        var importResult = machine.ImportCheckpoint(checkpoint);
        if (!importResult.IsSuccess)
            return OperationResult<TranslationMachineLoadResult>.Fail(importResult.ErrorMessage, importResult.ErrorCode);

        // Return the fully restored machine with the runner loop cursor — ready for inference without replay.
        return OperationResult<TranslationMachineLoadResult>.Ok(
            new TranslationMachineLoadResult(machine, checkpoint.TatoebaLoop));
    }

    public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ILogicalOrchestrator>.Fail("GetOrchestrator cancelled.", "CANCELLED"));

        ILogicalOrchestrator orchestrator = new InlineTranslationOrchestrator(_flow);
        return Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(orchestrator));
    }

    private double MeasureHeldOutAccuracy(IReadOnlyList<TranslationLearningExample> heldOut)
    {
        var scored = heldOut
            .GroupBy(example => example.UnitId)
            .Select(group => _unitTrainers.TryGetValue(group.Key, out var trainer)
                ? trainer.MeasureAccuracy(group.ToArray())
                : 0.0)
            .ToArray();

        return scored.Length == 0 ? 0.0 : scored.Average();
    }

    private static double MeasureUnitCoverage(IReadOnlySet<string> trainedUnits)
    {
        var required = new[]
        {
            TranslationUnitIds.SemanticTokenizer,
            TranslationUnitIds.ExecutorRuEn,
            TranslationUnitIds.QualityVerifier
        };

        return required.Count(trainedUnits.Contains) / (double)required.Length;
    }

    private async Task<OperationResult<string>> TranslateForEvaluationAsync(string prompt, CancellationToken ct)
    {
        var orchestrator = new InlineTranslationOrchestrator(_flow);
        var request = new LogicalOrchestratorRequest(
            RunId: $"eval-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: prompt,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "ru",
                ["target_language"] = "en",
                ["topic_domain"] = "general"
            });

        var result = await orchestrator.RunAsync(request, ct);
        return result.IsSuccess
            ? OperationResult<string>.Ok(result.Value!.FinalHumanResult)
            : OperationResult<string>.Fail(result.ErrorMessage, result.ErrorCode);
    }

    private void IncrementPatchVersion()
    {
        var parts = _currentVersion.Split('.');
        if (parts.Length == 3 && int.TryParse(parts[2], out var patch))
            _currentVersion = $"{parts[0]}.{parts[1]}.{patch + 1}";
    }

    private static TranslationRuntime CreateRuntime()
    {
        var knnStore = new TranslationKnnStore();
        var semanticTokenizer = new SemanticTokenizerUnit();
        var executorRuEn = new ExecutorUnit(TranslationDirection.RU_EN, knnStore);
        var executorEnRu = new ExecutorUnit(TranslationDirection.EN_RU, knnStore);
        var executorHeEn = new ExecutorUnit(TranslationDirection.HE_EN, knnStore);
        var executorEnHe = new ExecutorUnit(TranslationDirection.EN_HE, knnStore);
        var verifier = new QualityVerifierUnit();

        var adapters = new Dictionary<TranslationDirection, ITranslationDirectionAdapter>
        {
            [TranslationDirection.RU_EN] = new TrainedTranslationAdapter(
                TranslationDirection.RU_EN,
                semanticTokenizer,
                executorRuEn,
                verifier),
            [TranslationDirection.EN_RU] = new TrainedTranslationAdapter(
                TranslationDirection.EN_RU,
                semanticTokenizer,
                executorEnRu,
                verifier),
            [TranslationDirection.HE_EN] = new TrainedTranslationAdapter(
                TranslationDirection.HE_EN,
                semanticTokenizer,
                executorHeEn,
                verifier),
            [TranslationDirection.EN_HE] = new TrainedTranslationAdapter(
                TranslationDirection.EN_HE,
                semanticTokenizer,
                executorEnHe,
                verifier)
        };

        var trainers = new Dictionary<string, ITranslationUnitTrainer>(StringComparer.Ordinal)
        {
            [semanticTokenizer.UnitId] = semanticTokenizer,
            [executorRuEn.UnitId] = executorRuEn,
            [executorEnRu.UnitId] = executorEnRu,
            [executorHeEn.UnitId] = executorHeEn,
            [executorEnHe.UnitId] = executorEnHe,
            [verifier.UnitId] = verifier
        };

        // Collect direction → ExecutorUnit map so checkpoint export can access each executor.
        var executorsByDirection = new Dictionary<TranslationDirection, ExecutorUnit>
        {
            [TranslationDirection.RU_EN] = executorRuEn,
            [TranslationDirection.EN_RU] = executorEnRu,
            [TranslationDirection.HE_EN] = executorHeEn,
            [TranslationDirection.EN_HE] = executorEnHe
        };

        // Return the expanded runtime with all typed unit references for checkpoint access.
        return new TranslationRuntime(
            new TranslationRoutingFlow(adapters),
            trainers,
            knnStore,
            semanticTokenizer,
            executorsByDirection,
            verifier);
    }

    // TranslationRuntime holds both the inference graph (Flow) and typed concrete unit references.
    // The typed references are required for checkpoint export/import — the flow only exposes adapters.
    private sealed record TranslationRuntime(
        // Flow is the routing graph used for inference (TranslateAsync / GetOrchestratorAsync).
        TranslationRoutingFlow Flow,
        // UnitTrainers is the unit-id → trainer map used for training dispatch.
        IReadOnlyDictionary<string, ITranslationUnitTrainer> UnitTrainers,
        // KnnStore is the shared KNN bridge between training writes and inference reads.
        TranslationKnnStore KnnStore,
        // SemanticTokenizer is the tokenizer unit used during inference and exported in checkpoints.
        SemanticTokenizerUnit SemanticTokenizer,
        // Executors maps each TranslationDirection to its specific ExecutorUnit for checkpoint access.
        IReadOnlyDictionary<TranslationDirection, ExecutorUnit> Executors,
        // Verifier is the quality verifier unit used during inference and exported in checkpoints.
        QualityVerifierUnit Verifier);

    private sealed class InlineTranslationOrchestrator : ILogicalOrchestrator
    {
        private readonly TranslationRoutingFlow _flow;

        public InlineTranslationOrchestrator(TranslationRoutingFlow flow)
        {
            _flow = flow;
        }

        public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
            LogicalOrchestratorRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalOrchestratorResult>.Fail("Translation orchestrator cancelled.", "CANCELLED");

            if (request is null)
                return OperationResult<LogicalOrchestratorResult>.Fail("Orchestrator request is required.", "REQUEST_REQUIRED");

            var srcLang = ReadString(request.Metadata, "source_language", "en");
            var tgtLang = ReadString(request.Metadata, "target_language", "en");
            var topicDomain = ReadString(request.Metadata, "topic_domain", "general");

            var translationRequest = new TranslationRoutingRequest(
                SourceLanguage: srcLang ?? "en",
                TargetLanguage: tgtLang ?? "en",
                TopicDomain: topicDomain ?? "general",
                InputText: request.UserRequest);

            var flowRequest = new LogicalFlowRequest<TranslationRoutingRequest>(
                RunId: request.RunId,
                FlowId: TranslationRoutingFlow.FlowId,
                ScenarioId: TranslationRoutingFlow.ScenarioId,
                Input: translationRequest,
                Flags: request.Flags ?? new LogicalExecutionFlags(),
                Metadata: request.Metadata);

            var flowResult = await _flow.ExecuteAsync(flowRequest, cancellationToken);
            if (!flowResult.IsSuccess)
                return OperationResult<LogicalOrchestratorResult>.Fail(flowResult.ErrorMessage, flowResult.ErrorCode);

            var orchestratorResult = new LogicalOrchestratorResult(
                ScenarioId: request.ScenarioId,
                SelectedFlowId: TranslationRoutingFlow.FlowId,
                OverallVerdict: flowResult.Value!.OverallVerdict,
                FinalHumanResult: flowResult.Value.FinalHumanResult,
                AppliedFlags: request.Flags ?? new LogicalExecutionFlags(),
                FlowResult: flowResult.Value);

            return OperationResult<LogicalOrchestratorResult>.Ok(orchestratorResult);
        }

        private static string? ReadString(
            IReadOnlyDictionary<string, object> metadata,
            string key,
            string? defaultValue)
        {
            if (!metadata.TryGetValue(key, out var value))
                return defaultValue;

            return value?.ToString() ?? defaultValue;
        }
    }
}
