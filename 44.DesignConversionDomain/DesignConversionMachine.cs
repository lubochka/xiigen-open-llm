// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DesignConversionMachine : ILearningMachine<DesignConversionDomain>
{
    public const string MachineId = "design-conversion-machine";
    public const string DomainTag = "DesignConversionDomain";
    public const int CheckpointSchemaVersion = 1;
    public const string RuntimeScaffoldEvidence = "DESIGN_CONVERSION_MACHINE_RUNTIME_SCAFFOLD";
    public const string CheckpointLifecycleEvidence = "DESIGN_CONVERSION_MACHINE_CHECKPOINT_LIFECYCLE";
    public const string LearnedWeightsPersistedEvidence = "DESIGN_CONVERSION_LOCAL_CHECKPOINT_FEATURE_WEIGHTS_PERSISTED_NOT_TRAINED_MODEL";
    public const string LearnedWeightsSanitizedFieldsEvidence = "DESIGN_CONVERSION_LOCAL_CHECKPOINT_FEATURE_WEIGHTS_DERIVED_FROM_SANITIZED_FIELDS_NOT_TRAINED_MODEL";
    public const string LearnedInferenceAppliedEvidence = "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_NOT_LEARNED_AUTHORITY";
    public const string LearnedInferenceNoRawPayloadEvidence = "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_NO_RAW_PAYLOAD_NOT_TRAINED_MODEL";
    public const string LearnedInferenceCriticAdjustmentEvidence = "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_CRITIC_NOT_TRAINED_MODEL";
    public const string LearnedInferenceImproverEvidence = "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_IMPROVER_NOT_TRAINED_MODEL";
    public const string LocalCheckpointFeatureAdjustmentEvidence = "DESIGN_CONVERSION_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_NOT_TRAINED_AUTHORITY";
    public const string DpoTrainerPolicyCheckpointFreshLoadedEvidence = "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_FRESH_LOADED";
    public const string DpoTrainerPolicyCheckpointConvertedEvidence = "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_CONVERTED_TO_RUNTIME_WEIGHTS";
    public const string DpoTrainerPolicyCheckpointRuntimeScoringEvidence = "DESIGN_CONVERSION_RUNTIME_DPO_TRAINER_POLICY_CHECKPOINT_CONVERTED_SCORING";
    public const string CheckpointWriteConflictCode = "CHECKPOINT_WRITE_CONFLICT";
    public const string RuntimeScaffoldNotTrainedModelWarning = "RUNTIME_SCAFFOLD_NOT_TRAINED_MODEL";
    public const string HeldOutAccuracyNotMeasuredEvidence = "DESIGN_CONVERSION_HELD_OUT_ACCURACY_NOT_MEASURED_DURING_TRAINBATCH";
    public const string LocalPreferenceFitSignalOnlyEvidence = "DESIGN_CONVERSION_LOCAL_PREFERENCE_FIT_SIGNAL_NOT_HELD_OUT_ACCURACY";

    private const string VersionUnsupportedCode = "VERSION_UNSUPPORTED";
    private const double LocalCheckpointRuntimeAcceptanceThreshold = 0.85;
    private const double LocalCheckpointAdjustmentScale = 2.0;
    private const double LocalCheckpointAdjustmentClamp = 0.5;
    private static readonly Func<string, CancellationToken, Task<byte[]>> DefaultCheckpointByteReader = File.ReadAllBytesAsync;
    private static readonly AsyncLocal<Func<string, CancellationToken, Task<byte[]>>?> CheckpointByteReaderOverride = new();

    private readonly DesignConversionRoutingFlow _flow;
    private readonly List<LearningBatchResult> _trainingHistory = [];
    private readonly Dictionary<string, Dictionary<string, LearnedWeightAccumulator>> _learnedWeightsByUnit = new(StringComparer.Ordinal);

    private string _currentVersion = "0.1.0";
    private double _accuracyAfterLastBatch;
    private bool _hasTrainingScaffoldHistory;

    public DesignConversionMachine() : this(new DesignConversionRoutingFlow()) { }

    public DesignConversionMachine(DesignConversionRoutingFlow flow)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
    }

    public DesignConversionRoutingFlow Flow => _flow;

    public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<DesignConversionDomain> batch,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Training cancelled.", "CANCELLED"));

        if (batch is null)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Training batch is required.", "BATCH_REQUIRED"));

        if (!string.Equals(batch.DomainTag, DomainTag, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                $"Training batch domain tag '{batch.DomainTag}' does not match '{DomainTag}'.",
                "DOMAIN_TAG_MISMATCH"));

        if (batch.TrainingItems.Count == 0)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                "Training batch must contain at least one item.",
                "BATCH_EMPTY"));

        if (!TryIncrementPatchVersion(_currentVersion, out var nextVersion))
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                $"Current machine version '{_currentVersion}' cannot be incremented.",
                VersionUnsupportedCode));

        var startMs = Environment.TickCount64;
        var accuracyBefore = _accuracyAfterLastBatch;
        var learnedBatch = BuildLearnedWeightBatch(batch);
        foreach (var delta in learnedBatch.Deltas)
            ApplyLearnedWeightDelta(delta);

        var accuracyAfter = CarryForwardAccuracyWithoutHeldOutMeasurement(accuracyBefore, learnedBatch);
        var elapsedMs = Math.Max(0, Environment.TickCount64 - startMs);
        var result = new LearningBatchResult(
            BatchId: batch.BatchId,
            RowsTrained: batch.TrainingItems.Count,
            AccuracyBefore: accuracyBefore,
            AccuracyAfter: accuracyAfter,
            RewardMargin: learnedBatch.AverageRewardMargin,
            CpuMs: elapsedMs,
            PeakRamMb: GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024),
            ElapsedMs: elapsedMs,
            PromotedUnitIds: []);

        _hasTrainingScaffoldHistory = true;
        _accuracyAfterLastBatch = accuracyAfter;
        _trainingHistory.Add(result);
        _currentVersion = nextVersion;

        return Task.FromResult(OperationResult<LearningBatchResult>.Ok(result));
    }

    public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<DesignConversionDomain> evalSet,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail("Evaluation cancelled.", "CANCELLED"));

        if (evalSet is null)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail("Evaluation set is required.", "EVAL_SET_REQUIRED"));

        if (!string.Equals(evalSet.DomainTag, DomainTag, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(
                $"Evaluation set domain tag '{evalSet.DomainTag}' does not match '{DomainTag}'.",
                "DOMAIN_TAG_MISMATCH"));

        if (evalSet.EvaluationItems.Count == 0)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(
                "Evaluation set must contain at least one item.",
                "EVAL_SET_EMPTY"));

        var result = new LearningEvaluationResult(
            EvaluationId: evalSet.EvaluationId,
            RowsEvaluated: evalSet.EvaluationItems.Count,
            Accuracy: _accuracyAfterLastBatch,
            RewardMargin: 0.0,
            PassesGate: false,
            GateNote: "DesignConversionMachine lifecycle scaffold does not perform learned visual evaluation yet.");

        return Task.FromResult(OperationResult<LearningEvaluationResult>.Ok(result));
    }

    public Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningMachineExport>.Fail("Export cancelled.", "CANCELLED"));

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

    public Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<bool>.Fail("Import cancelled.", "CANCELLED"));

        if (export is null)
            return Task.FromResult(OperationResult<bool>.Fail("Export record is required.", "EXPORT_REQUIRED"));

        if (!string.Equals(export.MachineId, MachineId, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<bool>.Fail(
                $"Export machine id '{export.MachineId}' does not match '{MachineId}'.",
                "MACHINE_ID_MISMATCH"));

        if (!string.Equals(export.DomainTag, DomainTag, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<bool>.Fail(
                $"Export domain tag '{export.DomainTag}' does not match '{DomainTag}'.",
                "DOMAIN_TAG_MISMATCH"));

        if (!IsSupportedVersion(export.Version))
            return Task.FromResult(OperationResult<bool>.Fail(
                $"Export version '{export.Version}' is not supported.",
                VersionUnsupportedCode));

        _accuracyAfterLastBatch = export.AccuracyAtExport;
        _currentVersion = export.Version;

        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public OperationResult<DesignConversionMachineCheckpoint> ExportCheckpoint()
    {
        var checkpoint = new DesignConversionMachineCheckpoint(
            SchemaVersion: CheckpointSchemaVersion,
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: _currentVersion,
            AccuracyAfterLastBatch: _accuracyAfterLastBatch,
            HasTrainingScaffoldHistory: _hasTrainingScaffoldHistory,
            ExportedAtUtc: DateTimeOffset.UtcNow,
            CriticUnit: BuildRuntimeUnitCheckpoint(
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldCriticUnit.Role,
                DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
                "runtime_scaffold_input_evidence_only",
                [
                    RuntimeScaffoldEvidence,
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ]),
            ImproverUnit: BuildRuntimeUnitCheckpoint(
                DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.Role,
                DesignConversionRuntimeScaffoldImproverUnit.DomainTag,
                "runtime_scaffold_candidate_shell",
                [
                    RuntimeScaffoldEvidence,
                    "DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE"
                ]),
            TrainingHistory: _trainingHistory
                .Select(history => new DesignConversionTrainingHistoryEntryCheckpoint(
                    BatchId: history.BatchId,
                    RowsTrained: history.RowsTrained,
                    AccuracyBefore: history.AccuracyBefore,
                    AccuracyAfter: history.AccuracyAfter,
                    RewardMargin: history.RewardMargin,
                    CpuMs: history.CpuMs,
                    PeakRamMb: history.PeakRamMb,
                    ElapsedMs: history.ElapsedMs,
                    PromotedUnitIds: history.PromotedUnitIds))
                .ToArray(),
            EvidenceRefs:
            [
                RuntimeScaffoldEvidence,
                CheckpointLifecycleEvidence,
                HeldOutAccuracyNotMeasuredEvidence,
                LocalPreferenceFitSignalOnlyEvidence,
                "NO_DPO_GENERATOR_OR_MODEL_PROMOTION_IN_SLICE_3",
                "DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE"
            ],
            DeterministicLogicClaimedAsLearning: false);

        return OperationResult<DesignConversionMachineCheckpoint>.Ok(checkpoint);
    }

    public OperationResult<bool> ImportCheckpoint(DesignConversionMachineCheckpoint checkpoint)
    {
        if (checkpoint is null)
            return OperationResult<bool>.Fail("Checkpoint is required.", "CHECKPOINT_REQUIRED");

        if (checkpoint.SchemaVersion != CheckpointSchemaVersion)
            return OperationResult<bool>.Fail(
                $"Checkpoint schema version {checkpoint.SchemaVersion} is not supported.",
                "CHECKPOINT_SCHEMA_UNSUPPORTED");

        if (!string.Equals(checkpoint.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Checkpoint machine id '{checkpoint.MachineId}' does not match '{MachineId}'.",
                "MACHINE_ID_MISMATCH");

        if (!string.Equals(checkpoint.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Checkpoint domain tag '{checkpoint.DomainTag}' does not match '{DomainTag}'.",
                "DOMAIN_TAG_MISMATCH");

        if (!IsSupportedVersion(checkpoint.Version))
            return OperationResult<bool>.Fail(
                $"Checkpoint version '{checkpoint.Version}' is not supported.",
                VersionUnsupportedCode);

        _currentVersion = checkpoint.Version;
        _accuracyAfterLastBatch = checkpoint.AccuracyAfterLastBatch;
        _hasTrainingScaffoldHistory = checkpoint.HasTrainingScaffoldHistory;
        _trainingHistory.Clear();
        _trainingHistory.AddRange(checkpoint.TrainingHistory.Select(history => new LearningBatchResult(
            BatchId: history.BatchId,
            RowsTrained: history.RowsTrained,
            AccuracyBefore: history.AccuracyBefore,
            AccuracyAfter: history.AccuracyAfter,
            RewardMargin: history.RewardMargin,
            CpuMs: history.CpuMs,
            PeakRamMb: history.PeakRamMb,
            ElapsedMs: history.ElapsedMs,
            PromotedUnitIds: history.PromotedUnitIds)));
        var learnedWeightsImport = ImportLearnedWeights(checkpoint);
        if (!learnedWeightsImport.IsSuccess)
            return learnedWeightsImport;

        return OperationResult<bool>.Ok(true);
    }

    public static bool CheckpointHasRealLearnedWeights(DesignConversionMachineCheckpoint? checkpoint)
        => RuntimeUnitHasRealLearnedWeights(checkpoint?.CriticUnit) ||
           RuntimeUnitHasRealLearnedWeights(checkpoint?.ImproverUnit);

    public static int CountRealLearnedWeightArtifacts(DesignConversionMachineCheckpoint? checkpoint)
        => CountRealLearnedWeightArtifacts(checkpoint?.CriticUnit) +
           CountRealLearnedWeightArtifacts(checkpoint?.ImproverUnit);

    public static int CountRealLearnedWeightObservations(DesignConversionMachineCheckpoint? checkpoint)
        => CountRealLearnedWeightObservations(checkpoint?.CriticUnit) +
           CountRealLearnedWeightObservations(checkpoint?.ImproverUnit);

    public DesignConversionRoutingFlow CreateRuntimeRoutingFlow()
    {
        var criticWeights = ExportLearnedWeights(DesignConversionRuntimeScaffoldCriticUnit.UnitId);
        var improverWeights = ExportLearnedWeights(DesignConversionRuntimeScaffoldImproverUnit.UnitId);
        if (criticWeights.Count == 0 && improverWeights.Count == 0)
            return _flow;

        return new DesignConversionRoutingFlow(
            criticWeights.Count > 0 ? new LearnedWeightCriticUnit(criticWeights) : null,
            improverWeights.Count > 0 ? new LearnedWeightImproverUnit(improverWeights) : null);
    }

    public async Task<OperationResult<LearningMachineExport>> SaveCheckpointAsync(
        string checkpointPath,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");

        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<LearningMachineExport>.Fail(
                "Checkpoint path is required.",
                "CHECKPOINT_PATH_REQUIRED");

        string fullCheckpointPath;
        try
        {
            fullCheckpointPath = Path.GetFullPath(checkpointPath);
        }
        catch (Exception ex)
        {
            return OperationResult<LearningMachineExport>.Fail(ex.Message, "CHECKPOINT_WRITE_FAILED");
        }

        var checkpointResult = ExportCheckpoint();
        if (!checkpointResult.IsSuccess)
            return OperationResult<LearningMachineExport>.Fail(checkpointResult.ErrorMessage, checkpointResult.ErrorCode);

        string? tmpPath = null;
        var checkpointMoved = false;
        try
        {
            var dir = Path.GetDirectoryName(fullCheckpointPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var writerLock = TryAcquireCheckpointWriterLock(fullCheckpointPath);
            if (!writerLock.IsSuccess)
                return OperationResult<LearningMachineExport>.Fail(writerLock.ErrorMessage, writerLock.ErrorCode);

            await using var lockStream = writerLock.Value!;
            var json = JsonSerializer.Serialize(checkpointResult.Value!, DesignConversionCheckpointJsonOptions.Standard);
            tmpPath = BuildCheckpointTempPath(fullCheckpointPath);
            await File.WriteAllTextAsync(tmpPath, json, System.Text.Encoding.UTF8, ct);
            File.Move(tmpPath, fullCheckpointPath, overwrite: true);
            checkpointMoved = true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");
        }
        catch (Exception ex)
        {
            return OperationResult<LearningMachineExport>.Fail(ex.Message, "CHECKPOINT_WRITE_FAILED");
        }
        finally
        {
            if (!checkpointMoved && tmpPath is not null)
                TryDeleteExactTempFile(tmpPath);
        }

        var export = new LearningMachineExport(
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: _currentVersion,
            CheckpointRef: fullCheckpointPath,
            ManifestRef: $"{MachineId}/{_currentVersion}/manifest.json",
            AccuracyAtExport: _accuracyAfterLastBatch,
            ExportedAtUtc: DateTimeOffset.UtcNow);

        return OperationResult<LearningMachineExport>.Ok(export);
    }

    public static string GetCheckpointWriterLockPath(string checkpointPath)
    {
        if (string.IsNullOrWhiteSpace(checkpointPath))
            throw new ArgumentException("Checkpoint path is required.", nameof(checkpointPath));

        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        var dir = Path.GetDirectoryName(fullCheckpointPath);
        var fileName = Path.GetFileName(fullCheckpointPath);
        return Path.Combine(dir ?? string.Empty, $".{fileName}.write.lock");
    }

    private static OperationResult<FileStream> TryAcquireCheckpointWriterLock(string fullCheckpointPath)
    {
        FileStream? lockStream = null;
        try
        {
            lockStream = new FileStream(
                GetCheckpointWriterLockPath(fullCheckpointPath),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 1,
                FileOptions.Asynchronous);
            lockStream.SetLength(0);
            return OperationResult<FileStream>.Ok(lockStream);
        }
        catch (IOException ex) when (IsFileSharingConflict(ex))
        {
            lockStream?.Dispose();
            return OperationResult<FileStream>.Fail(
                "Checkpoint writer lock is already held for this path.",
                CheckpointWriteConflictCode);
        }
        catch (Exception ex)
        {
            lockStream?.Dispose();
            return OperationResult<FileStream>.Fail(ex.Message, "CHECKPOINT_WRITE_FAILED");
        }
    }

    private static string BuildCheckpointTempPath(string fullCheckpointPath)
    {
        var dir = Path.GetDirectoryName(fullCheckpointPath) ?? string.Empty;
        var fileName = Path.GetFileName(fullCheckpointPath);
        return Path.Combine(dir, $".{fileName}.{Guid.NewGuid():N}.tmp");
    }

    private static bool IsFileSharingConflict(IOException ex)
    {
        const int errorSharingViolation = 32;
        const int errorLockViolation = 33;
        var errorCode = ex.HResult & 0xFFFF;
        return errorCode is errorSharingViolation or errorLockViolation;
    }

    private static void TryDeleteExactTempFile(string tmpPath)
    {
        try
        {
            if (File.Exists(tmpPath))
                File.Delete(tmpPath);
        }
        catch
        {
            // Best-effort cleanup only for this call's unique temp file.
        }
    }

    public static async Task<OperationResult<DesignConversionMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        CancellationToken ct = default)
        => await LoadCheckpointAsync(
            checkpointPath,
            CheckpointByteReaderOverride.Value ?? DefaultCheckpointByteReader,
            ct);

    internal static IDisposable UseCheckpointByteReaderForCurrentAsyncFlow(
        Func<string, CancellationToken, Task<byte[]>> checkpointByteReader)
    {
        ArgumentNullException.ThrowIfNull(checkpointByteReader);

        var previous = CheckpointByteReaderOverride.Value;
        CheckpointByteReaderOverride.Value = checkpointByteReader;
        return new CheckpointByteReaderOverrideScope(previous);
    }

    internal static async Task<OperationResult<DesignConversionMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        Func<string, CancellationToken, Task<byte[]>> checkpointByteReader,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionMachineLoadResult>.Fail("Load cancelled.", "CANCELLED");

        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<DesignConversionMachineLoadResult>.Fail(
                "Checkpoint path is required.",
                "CHECKPOINT_PATH_REQUIRED");

        if (!File.Exists(checkpointPath))
            return OperationResult<DesignConversionMachineLoadResult>.Fail(
                $"Checkpoint file not found: {checkpointPath}",
                "CHECKPOINT_NOT_FOUND");

        DesignConversionMachineCheckpoint checkpoint;
        string checkpointSha256;
        try
        {
            var checkpointBytes = await checkpointByteReader(checkpointPath, ct);
            checkpointSha256 = Convert.ToHexString(SHA256.HashData(checkpointBytes)).ToLowerInvariant();
            var json = DecodeCheckpointJsonSnapshot(checkpointBytes);
            var parsed = JsonSerializer.Deserialize<DesignConversionMachineCheckpoint>(
                json,
                DesignConversionCheckpointJsonOptions.Standard);
            if (parsed is null)
                return OperationResult<DesignConversionMachineLoadResult>.Fail(
                    "Checkpoint deserialisation returned null.",
                    "CHECKPOINT_DESERIALIZE_FAILED");
            checkpoint = parsed;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<DesignConversionMachineLoadResult>.Fail("Load cancelled.", "CANCELLED");
        }
        catch (Exception ex)
        {
            return OperationResult<DesignConversionMachineLoadResult>.Fail(ex.Message, "CHECKPOINT_READ_FAILED");
        }

        var machine = new DesignConversionMachine();
        var importResult = machine.ImportCheckpoint(checkpoint);
        if (!importResult.IsSuccess)
            return OperationResult<DesignConversionMachineLoadResult>.Fail(importResult.ErrorMessage, importResult.ErrorCode);

        return OperationResult<DesignConversionMachineLoadResult>.Ok(new DesignConversionMachineLoadResult(machine, checkpoint, checkpointSha256));
    }

    private static string DecodeCheckpointJsonSnapshot(byte[] checkpointBytes)
    {
        var json = Encoding.UTF8.GetString(checkpointBytes);
        return json.Length > 0 && json[0] == '\uFEFF'
            ? json[1..]
            : json;
    }

    private sealed class CheckpointByteReaderOverrideScope(
        Func<string, CancellationToken, Task<byte[]>>? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            CheckpointByteReaderOverride.Value = previous;
            _disposed = true;
        }
    }

    private DesignConversionRuntimeUnitCheckpoint BuildRuntimeUnitCheckpoint(
        string unitId,
        string unitRole,
        string domainTag,
        string implementationStatus,
        IReadOnlyList<string> baseEvidenceRefs)
    {
        var learnedWeights = ExportLearnedWeights(unitId);
        var hasLearnedWeights = learnedWeights.Count > 0;
        return new DesignConversionRuntimeUnitCheckpoint(
            UnitId: unitId,
            UnitRole: unitRole,
            DomainTag: domainTag,
            ImplementationStatus: hasLearnedWeights
                ? $"{implementationStatus}_with_local_learned_weight_artifact"
                : implementationStatus,
            EvidenceRefs: baseEvidenceRefs
                .Concat(hasLearnedWeights
                    ? [LearnedWeightsPersistedEvidence, LearnedWeightsSanitizedFieldsEvidence]
                    : [])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            LearnedWeightsPresent: hasLearnedWeights,
            LearnedWeights: learnedWeights);
    }

    private IReadOnlyList<DesignConversionLearnedWeightCheckpoint> ExportLearnedWeights(string checkpointUnitId)
    {
        if (!_learnedWeightsByUnit.TryGetValue(checkpointUnitId, out var byFeature))
            return [];

        return byFeature.Values
            .Where(accumulator => accumulator.ObservationCount > 0)
            .OrderBy(accumulator => accumulator.FeatureKey, StringComparer.Ordinal)
            .Select(accumulator => accumulator.ToCheckpoint())
            .Where(IsValidLearnedWeight)
            .ToArray();
    }

    private OperationResult<bool> ImportLearnedWeights(DesignConversionMachineCheckpoint checkpoint)
    {
        _learnedWeightsByUnit.Clear();
        var critic = ImportLearnedWeights(checkpoint.CriticUnit);
        if (!critic.IsSuccess)
            return critic;

        var improver = ImportLearnedWeights(checkpoint.ImproverUnit);
        if (!improver.IsSuccess)
            return improver;

        return OperationResult<bool>.Ok(true);
    }

    private OperationResult<bool> ImportLearnedWeights(DesignConversionRuntimeUnitCheckpoint unit)
    {
        var weights = unit.LearnedWeights ?? [];
        if (unit.LearnedWeightsPresent && !RuntimeUnitHasRealLearnedWeights(unit))
        {
            return OperationResult<bool>.Fail(
                $"Checkpoint unit '{unit.UnitId}' claims learned weights without a valid persisted artifact.",
                "CHECKPOINT_LEARNED_WEIGHTS_INVALID");
        }

        if (weights.Count == 0)
            return OperationResult<bool>.Ok(true);

        var byFeature = GetUnitAccumulatorMap(unit.UnitId);
        foreach (var weight in weights.Where(IsValidLearnedWeight))
        {
            byFeature[weight.FeatureKey] = LearnedWeightAccumulator.FromCheckpoint(weight);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool RuntimeUnitHasRealLearnedWeights(DesignConversionRuntimeUnitCheckpoint? unit)
        => unit?.LearnedWeightsPresent == true &&
           CountRealLearnedWeightArtifacts(unit) > 0;

    private static int CountRealLearnedWeightArtifacts(DesignConversionRuntimeUnitCheckpoint? unit)
        => unit?.LearnedWeights?
            .Count(IsValidLearnedWeight) ?? 0;

    private static int CountRealLearnedWeightObservations(DesignConversionRuntimeUnitCheckpoint? unit)
        => unit?.LearnedWeights?
            .Where(IsValidLearnedWeight)
            .Sum(weight => weight.ObservationCount) ?? 0;

    private static bool IsValidLearnedWeight(DesignConversionLearnedWeightCheckpoint? weight)
        => weight is not null &&
           !string.IsNullOrWhiteSpace(weight.FeatureKey) &&
           IsAllowedLearnedWeightFeatureKey(weight.FeatureKey) &&
           double.IsFinite(weight.Weight) &&
           weight.ObservationCount > 0 &&
           !string.IsNullOrWhiteSpace(weight.LastBatchId) &&
           !string.IsNullOrWhiteSpace(weight.Derivation) &&
           weight.SourceIdentityHashes is not null &&
           weight.SourceIdentityHashes.Count > 0 &&
           weight.SourceIdentityHashes.All(IsSha256Hex) &&
           weight.EvidenceRefs is not null &&
           weight.EvidenceRefs.Contains(LearnedWeightsSanitizedFieldsEvidence, StringComparer.Ordinal);

    private static bool IsAllowedLearnedWeightFeatureKey(string featureKey)
        => !string.IsNullOrWhiteSpace(featureKey) &&
           !featureKey.Contains("usefulness", StringComparison.OrdinalIgnoreCase) &&
           !featureKey.Contains("review_priority", StringComparison.OrdinalIgnoreCase) &&
           !featureKey.Contains("accepted_delta", StringComparison.OrdinalIgnoreCase) &&
           !featureKey.Contains("score_margin", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(featureKey, "visual:chosen_score", StringComparison.Ordinal) &&
           !string.Equals(featureKey, "visual:rejected_score_inverse", StringComparison.Ordinal);

    private void ApplyLearnedWeightDelta(LearnedWeightDelta delta)
    {
        var byFeature = GetUnitAccumulatorMap(delta.CheckpointUnitId);
        if (!byFeature.TryGetValue(delta.FeatureKey, out var accumulator))
        {
            accumulator = new LearnedWeightAccumulator(
                delta.FeatureKey,
                delta.Derivation,
                delta.BatchId);
            byFeature[delta.FeatureKey] = accumulator;
        }

        accumulator.Apply(delta);
    }

    private Dictionary<string, LearnedWeightAccumulator> GetUnitAccumulatorMap(string checkpointUnitId)
    {
        if (!_learnedWeightsByUnit.TryGetValue(checkpointUnitId, out var byFeature))
        {
            byFeature = new Dictionary<string, LearnedWeightAccumulator>(StringComparer.Ordinal);
            _learnedWeightsByUnit[checkpointUnitId] = byFeature;
        }

        return byFeature;
    }

    private static LearnedWeightBatch BuildLearnedWeightBatch(LearningBatch<DesignConversionDomain> batch)
    {
        var deltas = new List<LearnedWeightDelta>();
        var margins = new List<double>();
        var batchSourceHashes = (batch.SourceIds ?? [])
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Select(HashText)
            .ToArray();

        foreach (var item in batch.TrainingItems ?? [])
        {
            var itemDeltas = BuildLearnedWeightDeltas(
                batch.BatchId,
                item,
                batchSourceHashes,
                out var margin);
            deltas.AddRange(itemDeltas);
            if (margin.HasValue)
                margins.Add(margin.Value);
        }

        return new LearnedWeightBatch(
            Deltas: deltas,
            AverageRewardMargin: margins.Count == 0
                ? 0.0
                : Math.Round(margins.Average(), 6));
    }

    private static IReadOnlyList<LearnedWeightDelta> BuildLearnedWeightDeltas(
        string batchId,
        LearningTrainingItem item,
        IReadOnlyList<string> batchSourceHashes,
        out double? rewardMargin)
    {
        rewardMargin = null;
        if (!TryParseJsonObject(item.Prompt, out var prompt))
            return [];

        using (prompt)
        {
            if (!TryParseJsonObject(item.ChosenResponse, out var chosen) ||
                !TryParseJsonObject(item.RejectedResponse, out var rejected))
            {
                return [];
            }

            using (chosen)
            using (rejected)
            {
                var promptRoot = prompt.RootElement;
                var chosenRoot = chosen.RootElement;
                var rejectedRoot = rejected.RootElement;
                var unitId = NormalizeCheckpointUnitId(ReadString(promptRoot, "unit_id"));
                if (string.IsNullOrWhiteSpace(unitId))
                    return [];

                if (!TryReadDouble(chosenRoot, "visual_score", out var chosenScore) ||
                    !TryReadDouble(rejectedRoot, "visual_score", out var rejectedScore))
                {
                    return [];
                }

                var sourceHashes = new SortedSet<string>(batchSourceHashes, StringComparer.Ordinal);
                AddHashIfPresent(sourceHashes, ReadString(promptRoot, "source_manifest_hash"));
                AddHashIfPresent(sourceHashes, ReadString(promptRoot, "source_identity_hash"));
                AddHashIfPresent(sourceHashes, ReadString(promptRoot, "source_structure_hash"));
                if (sourceHashes.Count == 0)
                    return [];

                var deltas = new List<LearnedWeightDelta>();
                var margin = Math.Round(chosenScore - rejectedScore, 6);
                rewardMargin = margin;
                var vector = ReadDoubleArray(promptRoot, "internal_vector");
                if (vector.Count > 0)
                {
                    AddDelta(deltas, unitId, batchId, "internal_vector:mean", vector.Average(), "internal_vector_mean", sourceHashes);
                    AddDelta(deltas, unitId, batchId, "internal_vector:dimension", vector.Count, "internal_vector_dimension", sourceHashes);
                }

                var sourceKind = SanitizeFeatureToken(ReadString(promptRoot, "source_kind"));
                if (!string.IsNullOrWhiteSpace(sourceKind))
                    AddDelta(deltas, unitId, batchId, $"source_kind:{sourceKind}", 1.0, "source_kind_presence", sourceHashes);

                var pairFamily = SanitizeFeatureToken(ReadString(promptRoot, "pair_family"));
                if (!string.IsNullOrWhiteSpace(pairFamily))
                    AddDelta(deltas, unitId, batchId, $"pair_family:{pairFamily}", 1.0, "negative_pair_family_presence", sourceHashes);

                AddCorrectionCategoryDeltas(deltas, unitId, batchId, chosenRoot, rejectedRoot, sourceHashes);
                AddDelta(deltas, unitId, batchId, "source_manifest:hash_diversity", sourceHashes.Count, "source_manifest_hash_diversity", sourceHashes);

                return deltas;
            }
        }
    }

    private static void AddCorrectionCategoryDeltas(
        List<LearnedWeightDelta> deltas,
        string unitId,
        string batchId,
        JsonElement chosenRoot,
        JsonElement rejectedRoot,
        IReadOnlyCollection<string> sourceHashes)
    {
        if (!string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal))
            return;

        var chosenRules = ReadCorrectionRuleIds(chosenRoot);
        var rejectedRules = ReadCorrectionRuleIds(rejectedRoot)
            .Except(chosenRules, StringComparer.Ordinal)
            .ToArray();

        foreach (var ruleId in chosenRules)
        {
            AddDelta(
                deltas,
                unitId,
                batchId,
                $"correction_category:{ruleId}",
                1.0,
                "chosen_correction_category_preference",
                sourceHashes);
        }

        foreach (var ruleId in rejectedRules)
        {
            AddDelta(
                deltas,
                unitId,
                batchId,
                $"correction_category:{ruleId}",
                -1.0,
                "rejected_correction_category_penalty",
                sourceHashes);
        }
    }

    private static IReadOnlyList<string> ReadCorrectionRuleIds(JsonElement element)
    {
        var rules = new SortedSet<string>(StringComparer.Ordinal);
        AddCorrectionRuleIfPresent(rules, ReadString(element, "correction_category"));
        AddCorrectionRuleIfPresent(rules, ReadString(element, "selected_correction_category"));
        AddCorrectionRuleIfPresent(rules, ReadString(element, "correction_rule"));
        AddCorrectionRuleIfPresent(rules, ReadString(element, "selected_correction_rule"));
        AddCorrectionRuleIfPresent(rules, ReadString(element, "defect_category"));
        AddCorrectionRuleArray(rules, element, "correction_categories");
        AddCorrectionRuleArray(rules, element, "defect_categories");

        if (element.TryGetProperty("metadata", out var metadata) &&
            metadata.ValueKind == JsonValueKind.Object)
        {
            AddCorrectionRuleIfPresent(rules, ReadString(metadata, "correction_category"));
            AddCorrectionRuleIfPresent(rules, ReadString(metadata, "selected_correction_category"));
            AddCorrectionRuleArray(rules, metadata, "correction_categories");
            AddCorrectionRuleArray(rules, metadata, "defect_categories");
        }

        return rules.ToArray();
    }

    private static void AddCorrectionRuleArray(SortedSet<string> rules, JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var child in property.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.String)
                AddCorrectionRuleIfPresent(rules, child.GetString());
        }
    }

    private static void AddCorrectionRuleIfPresent(SortedSet<string> rules, string? signal)
    {
        var ruleId = DesignConversionRuntimeScaffoldImproverUnit.NormalizeCorrectionRuleId(signal ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(ruleId))
            rules.Add(ruleId);
    }

    private static void AddDelta(
        List<LearnedWeightDelta> deltas,
        string checkpointUnitId,
        string batchId,
        string featureKey,
        double contribution,
        string derivation,
        IReadOnlyCollection<string> sourceHashes)
    {
        if (!double.IsFinite(contribution))
            return;
        if (!IsAllowedLearnedWeightFeatureKey(featureKey))
            return;

        deltas.Add(new LearnedWeightDelta(
            CheckpointUnitId: checkpointUnitId,
            FeatureKey: featureKey,
            Contribution: Math.Round(contribution, 6),
            BatchId: batchId,
            Derivation: derivation,
            SourceIdentityHashes: sourceHashes.ToArray()));
    }

    // TrainBatchAsync does not perform held-out measurement; it carries forward
    // existing accuracy while checkpoint evidence records NOT_MEASURED markers.
    private static double CarryForwardAccuracyWithoutHeldOutMeasurement(
        double accuracyBefore,
        LearnedWeightBatch learnedBatch)
    {
        _ = learnedBatch;
        return accuracyBefore;
    }

    private static bool TryParseJsonObject(string? value, out JsonDocument document)
    {
        document = null!;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
                return true;

            document.Dispose();
            document = null!;
            return false;
        }
        catch (JsonException)
        {
            document = null!;
            return false;
        }
    }

    private static string ReadString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool TryReadDouble(JsonElement element, string propertyName, out double value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var property))
            return false;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out value))
            return double.IsFinite(value);

        if (property.ValueKind == JsonValueKind.String &&
            double.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return double.IsFinite(value);
        }

        return false;
    }

    private static bool TryReadBoolean(JsonElement element, string propertyName, out bool value)
    {
        value = false;
        if (!element.TryGetProperty(propertyName, out var property))
            return false;

        if (property.ValueKind == JsonValueKind.True)
        {
            value = true;
            return true;
        }

        if (property.ValueKind == JsonValueKind.False)
        {
            value = false;
            return true;
        }

        if (property.ValueKind == JsonValueKind.String &&
            bool.TryParse(property.GetString(), out value))
        {
            return true;
        }

        return false;
    }

    private static IReadOnlyList<double> ReadDoubleArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var values = new List<double>();
        foreach (var child in property.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.Number &&
                child.TryGetDouble(out var value) &&
                double.IsFinite(value))
            {
                values.Add(value);
            }
        }

        return values;
    }

    private static string NormalizeCheckpointUnitId(string unitId)
    {
        if (string.Equals(unitId, DesignConversionDpoUnits.CriticUnit, StringComparison.Ordinal) ||
            string.Equals(unitId, DesignConversionRuntimeScaffoldCriticUnit.UnitId, StringComparison.Ordinal))
        {
            return DesignConversionRuntimeScaffoldCriticUnit.UnitId;
        }

        if (string.Equals(unitId, DesignConversionDpoUnits.ImproverUnit, StringComparison.Ordinal) ||
            string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal))
        {
            return DesignConversionRuntimeScaffoldImproverUnit.UnitId;
        }

        return string.Empty;
    }

    private static string SanitizeFeatureToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new StringBuilder();
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(ch))
                builder.Append(ch);
            else if (ch is '_' or '-' or ':')
                builder.Append('_');
        }

        return builder.ToString().Trim('_');
    }

    private static void AddHashIfPresent(SortedSet<string> hashes, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return;

        var normalized = hash.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? hash["sha256:".Length..]
            : hash;
        if (IsSha256Hex(normalized))
            hashes.Add(normalized.ToLowerInvariant());
    }

    private static bool IsSha256Hex(string value)
        => value.Length == 64 &&
           value.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');

    private static string HashText(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ILogicalOrchestrator>.Fail(
                "GetOrchestrator cancelled.",
                "CANCELLED"));

        var runtimeFlow = CreateRuntimeRoutingFlow();
        ILogicalOrchestrator orchestrator = new InlineDesignConversionOrchestrator(
            runtimeFlow,
            localCheckpointWeightsConfigured: !ReferenceEquals(runtimeFlow, _flow));
        return Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(orchestrator));
    }

    private static bool IsSupportedVersion(string? version)
        => TryParseSupportedVersion(version, out _, out _, out _);

    private static bool TryIncrementPatchVersion(string? version, out string nextVersion)
    {
        nextVersion = string.Empty;
        if (!TryParseSupportedVersion(version, out var major, out var minor, out var patch) ||
            patch == int.MaxValue)
        {
            return false;
        }

        nextVersion = string.Join(
            ".",
            major.ToString(CultureInfo.InvariantCulture),
            minor.ToString(CultureInfo.InvariantCulture),
            (patch + 1).ToString(CultureInfo.InvariantCulture));
        return true;
    }

    private static bool TryParseSupportedVersion(
        string? version,
        out int major,
        out int minor,
        out int patch)
    {
        major = 0;
        minor = 0;
        patch = 0;

        if (string.IsNullOrWhiteSpace(version))
            return false;

        var parts = version.Split('.');
        if (parts.Length != 3)
            return false;

        if (!TryParseVersionComponent(parts[0], out major) ||
            !TryParseVersionComponent(parts[1], out minor) ||
            !TryParseVersionComponent(parts[2], out patch))
        {
            return false;
        }

        return major is 0 or 1;
    }

    private static bool TryParseVersionComponent(string component, out int value)
        => int.TryParse(
            component,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out value);

    private sealed record LearnedWeightBatch(
        IReadOnlyList<LearnedWeightDelta> Deltas,
        double AverageRewardMargin);

    private sealed record LearnedWeightDelta(
        string CheckpointUnitId,
        string FeatureKey,
        double Contribution,
        string BatchId,
        string Derivation,
        IReadOnlyList<string> SourceIdentityHashes);

    private sealed record LocalCheckpointFeatureContribution(
        string FeatureKey,
        double FeatureValue,
        double Weight,
        int ObservationCount,
        string Derivation,
        double WeightedContribution,
        IReadOnlyList<string> EvidenceRefs);

    private sealed class LearnedWeightAccumulator
    {
        private readonly SortedSet<string> _sourceIdentityHashes = new(StringComparer.Ordinal);
        private readonly SortedSet<string> _evidenceRefs = new(StringComparer.Ordinal)
        {
            LearnedWeightsPersistedEvidence,
            LearnedWeightsSanitizedFieldsEvidence,
            "DESIGN_CONVERSION_LEARNED_WEIGHT_NO_RAW_TREE_HTML_SCREENSHOT"
        };
        private double _weightedSum;

        public LearnedWeightAccumulator(
            string featureKey,
            string derivation,
            string lastBatchId)
        {
            FeatureKey = featureKey;
            Derivation = derivation;
            LastBatchId = lastBatchId;
        }

        public string FeatureKey { get; }

        public string Derivation { get; private set; }

        public string LastBatchId { get; private set; }

        public int ObservationCount { get; private set; }

        public void Apply(LearnedWeightDelta delta)
        {
            _weightedSum += delta.Contribution;
            ObservationCount++;
            LastBatchId = delta.BatchId;
            Derivation = delta.Derivation;
            foreach (var hash in delta.SourceIdentityHashes)
                _sourceIdentityHashes.Add(hash);
        }

        public DesignConversionLearnedWeightCheckpoint ToCheckpoint()
            => new(
                FeatureKey: FeatureKey,
                Weight: Math.Round(_weightedSum / ObservationCount, 6),
                ObservationCount: ObservationCount,
                LastBatchId: LastBatchId,
                Derivation: Derivation,
                SourceIdentityHashes: _sourceIdentityHashes.ToArray(),
                EvidenceRefs: _evidenceRefs.ToArray());

        public static LearnedWeightAccumulator FromCheckpoint(DesignConversionLearnedWeightCheckpoint checkpoint)
        {
            var accumulator = new LearnedWeightAccumulator(
                checkpoint.FeatureKey,
                checkpoint.Derivation,
                checkpoint.LastBatchId)
            {
                _weightedSum = checkpoint.Weight * checkpoint.ObservationCount,
                ObservationCount = checkpoint.ObservationCount
            };

            foreach (var hash in checkpoint.SourceIdentityHashes ?? [])
                accumulator._sourceIdentityHashes.Add(hash);
            foreach (var evidenceRef in checkpoint.EvidenceRefs ?? [])
            {
                if (!string.IsNullOrWhiteSpace(evidenceRef))
                    accumulator._evidenceRefs.Add(evidenceRef);
            }

            return accumulator;
        }
    }

    private sealed class LearnedWeightCriticUnit(
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> learnedWeights) : IDesignConversionCriticUnit
    {
        private readonly DesignConversionRuntimeScaffoldCriticUnit _inner = new();
        private readonly IReadOnlyList<DesignConversionLearnedWeightCheckpoint> _learnedWeights =
            (learnedWeights ?? [])
            .Where(IsValidLearnedWeight)
            .ToArray();

        public async Task<OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionCriticUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            var baseline = await _inner.ExecuteAsync(request, cancellationToken);
            if (!baseline.IsSuccess || _learnedWeights.Count == 0 || baseline.Value is null)
                return baseline;

            var output = baseline.Value.Output;
            var contributions = BuildLocalCheckpointFeatureContributions(
                _learnedWeights,
                BuildCriticRuntimeFeatures(request.Input, output.Verdict));
            if (contributions.Count == 0)
                return baseline;

            var adjustment = CalculateLocalCheckpointAdjustment(contributions);
            var adjustedScore = Clamp01(Math.Round(output.Verdict.Score + adjustment, 6));
            var evidenceRefs = BuildLocalCheckpointFeatureAdjustmentEvidenceRefs(
                "critic",
                contributions,
                LearnedInferenceCriticAdjustmentEvidence);
            var adjustedVerdict = output.Verdict with
            {
                VerdictId = $"{output.Verdict.VerdictId}:local-checkpoint:{HashText(string.Join("|", evidenceRefs))[..12]}",
                Score = adjustedScore,
                Passed = output.Verdict.Passed || adjustedScore >= LocalCheckpointRuntimeAcceptanceThreshold,
                EvidenceRefs = output.Verdict.EvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            };
            var adjustedOutput = output with
            {
                Verdict = adjustedVerdict,
                QaEvidenceRefs = output.QaEvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            };

            return OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Ok(baseline.Value with
            {
                Output = adjustedOutput,
                Confidence = adjustedScore,
                PredicateIds = baseline.Value.PredicateIds
                    .Append("design_conversion_critic_local_checkpoint_feature_adjustment")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                EvidenceIds = baseline.Value.EvidenceIds
                    .Concat(evidenceRefs)
                    .Append(BuildLocalCheckpointAdjustmentSummary("critic", output.Verdict.Score, adjustedScore, adjustment, contributions))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Warnings = BuildLocalCheckpointAdjustmentWarnings(
                    baseline.Value.Warnings,
                    "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_CRITIC_NOT_TRAINED_AUTHORITY")
            });
        }
    }

    private sealed class LearnedWeightImproverUnit(
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> learnedWeights) : IDesignConversionImproverUnit
    {
        private readonly DesignConversionRuntimeScaffoldImproverUnit _inner = new();
        private readonly IReadOnlyList<DesignConversionLearnedWeightCheckpoint> _learnedWeights =
            (learnedWeights ?? [])
            .Where(IsValidLearnedWeight)
            .ToArray();

        public async Task<OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionImproverUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            if (request is null || request.Input is null)
                return await _inner.ExecuteAsync(request!, cancellationToken);

            var contributions = BuildLocalCheckpointFeatureContributions(
                _learnedWeights,
                BuildImproverRuntimeFeatures(request.Input));
            var correctionPreferences = BuildLearnedCorrectionPreferences(contributions);
            var baseline = correctionPreferences.Count == 0
                ? await _inner.ExecuteAsync(request, cancellationToken)
                : await _inner.ExecuteWithLearnedCorrectionPreferencesAsync(request, correctionPreferences, cancellationToken);
            if (!baseline.IsSuccess || _learnedWeights.Count == 0 || baseline.Value is null)
                return baseline;

            var output = baseline.Value.Output;
            if (contributions.Count == 0)
                return baseline;

            var adjustment = CalculateLocalCheckpointAdjustment(contributions);
            var evidenceRefs = BuildLocalCheckpointFeatureAdjustmentEvidenceRefs(
                "improver",
                contributions,
                LearnedInferenceImproverEvidence);
            var summary = BuildLocalCheckpointAdjustmentSummary(
                "improver",
                request.Input.CriticVerdict.Score,
                Clamp01(request.Input.CriticVerdict.Score + adjustment),
                adjustment,
                contributions);
            var candidate = output.Candidate with
            {
                ChangeReasons = output.Candidate.ChangeReasons
                    .Append("Local checkpoint feature weights contributed sanitized runtime evidence for improver selection; this is not trained model authority.")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                EvidenceRefs = output.Candidate.EvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Metadata = output.Candidate.Metadata
                    .Concat(new Dictionary<string, string>
                    {
                        ["LearnedInferenceApplied"] = bool.TrueString,
                        ["LearnedFeatureContributionCount"] = contributions.Count.ToString(CultureInfo.InvariantCulture),
                        ["LocalCheckpointFeatureAdjustmentApplied"] = bool.TrueString,
                        ["TrainedModelAuthority"] = bool.FalseString,
                        ["LocalCheckpointFeatureContributionCount"] = contributions.Count.ToString(CultureInfo.InvariantCulture),
                        ["LocalCheckpointFeatureAdjustmentSummary"] = summary,
                        ["LocalCheckpointFeatureAdjustmentEvidenceRefs"] = string.Join("|", evidenceRefs),
                        ["local_checkpoint_feature_adjustment_applied"] = bool.TrueString,
                        [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
                        [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue,
                        ["local_checkpoint_weights_source"] = "local_checkpoint_sanitized_features_not_trained_model"
                    })
                    .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal)
            };
            var improvedOutput = output.Output with
            {
                CandidateId = candidate.CandidateId,
                EvidenceRefs = output.Output.EvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            };
            var adjustedUnitOutput = output with
            {
                Candidate = candidate,
                Output = improvedOutput,
                QaEvidenceRefs = output.QaEvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            };

            return OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Ok(baseline.Value with
            {
                Output = adjustedUnitOutput,
                Confidence = Clamp01(Math.Round(baseline.Value.Confidence + adjustment, 6)),
                PredicateIds = baseline.Value.PredicateIds
                    .Append("design_conversion_improver_local_checkpoint_feature_adjustment")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                EvidenceIds = baseline.Value.EvidenceIds
                    .Concat(evidenceRefs)
                    .Append(summary)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Warnings = BuildLocalCheckpointAdjustmentWarnings(
                    baseline.Value.Warnings,
                    "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_IMPROVER_NOT_TRAINED_AUTHORITY")
            });
        }
    }

    private static IReadOnlyDictionary<string, double> BuildCriticRuntimeFeatures(
        DesignConversionCriticUnitInput input,
        VisualVerdict baselineVerdict)
    {
        _ = baselineVerdict;
        var features = new Dictionary<string, double>(StringComparer.Ordinal);

        var sourceKind = SanitizeFeatureToken(input.Request.SourceKind.ToString());
        if (!string.IsNullOrWhiteSpace(sourceKind))
            features[$"source_kind:{sourceKind}"] = 1.0;

        if (input.ComparisonArtifacts.Count > 0)
            features["source_manifest:hash_diversity"] = input.ComparisonArtifacts
                .SelectMany(artifact => new[] { artifact.Expected.Sha256, artifact.Actual.Sha256, artifact.DiffImageSha256 })
                .Select(NormalizeRuntimeSourceIdentityKey)
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

        return features;
    }

    private static string NormalizeRuntimeSourceIdentityKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim();
        normalized = normalized.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? normalized["sha256:".Length..]
            : normalized;
        if (string.IsNullOrWhiteSpace(normalized))
            return string.Empty;

        return IsSha256Hex(normalized)
            ? normalized.ToLowerInvariant()
            : HashText(normalized);
    }

    private static IReadOnlyDictionary<string, double> BuildImproverRuntimeFeatures(
        DesignConversionImproverUnitInput input)
    {
        var features = new Dictionary<string, double>(StringComparer.Ordinal);

        var sourceKind = SanitizeFeatureToken(input.Request.SourceKind.ToString());
        if (!string.IsNullOrWhiteSpace(sourceKind))
            features[$"source_kind:{sourceKind}"] = 1.0;

        foreach (var ruleId in DesignConversionRuntimeScaffoldImproverUnit.FindLocalCorrectionRuleIds(input.CriticVerdict.Defects))
            features[$"correction_category:{ruleId}"] = 1.0;

        return features;
    }

    private static IReadOnlyList<LocalCheckpointFeatureContribution> BuildLocalCheckpointFeatureContributions(
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> learnedWeights,
        IReadOnlyDictionary<string, double> runtimeFeatures)
        => learnedWeights
            .Where(IsValidLearnedWeight)
            .Where(weight => runtimeFeatures.TryGetValue(weight.FeatureKey, out var value) && double.IsFinite(value))
            .Select(weight =>
            {
                var featureValue = runtimeFeatures[weight.FeatureKey];
                return new LocalCheckpointFeatureContribution(
                    FeatureKey: weight.FeatureKey,
                    FeatureValue: Math.Round(featureValue, 6),
                    Weight: Math.Round(weight.Weight, 6),
                    ObservationCount: weight.ObservationCount,
                    Derivation: weight.Derivation,
                    WeightedContribution: Math.Round(featureValue * weight.Weight, 6),
                    EvidenceRefs: weight.EvidenceRefs ?? []);
            })
            .ToArray();

    private static IReadOnlyList<DesignConversionLearnedCorrectionPreference> BuildLearnedCorrectionPreferences(
        IReadOnlyList<LocalCheckpointFeatureContribution> contributions)
    {
        const string prefix = "correction_category:";
        return contributions
            .Where(contribution => contribution.FeatureKey.StartsWith(prefix, StringComparison.Ordinal))
            .Select(contribution => new DesignConversionLearnedCorrectionPreference(
                RuleId: contribution.FeatureKey[prefix.Length..],
                Weight: contribution.WeightedContribution,
                ObservationCount: contribution.ObservationCount,
                FeatureKey: contribution.FeatureKey,
                Derivation: contribution.Derivation))
            .ToArray();
    }

    private static double CalculateLocalCheckpointAdjustment(IReadOnlyList<LocalCheckpointFeatureContribution> contributions)
    {
        if (contributions.Count == 0)
            return 0.0;

        var rawAdjustment = contributions.Sum(contribution => contribution.WeightedContribution);
        var scaledAdjustment = Math.Tanh(rawAdjustment / LocalCheckpointAdjustmentScale) * LocalCheckpointAdjustmentClamp;
        return Math.Round(Math.Clamp(scaledAdjustment, -LocalCheckpointAdjustmentClamp, LocalCheckpointAdjustmentClamp), 6);
    }

    private static string[] BuildLocalCheckpointFeatureAdjustmentEvidenceRefs(
        string unit,
        IReadOnlyList<LocalCheckpointFeatureContribution> contributions,
        string unitEvidence)
    {
        var evidenceRefs = new[]
            {
                LearnedInferenceAppliedEvidence,
                LearnedInferenceNoRawPayloadEvidence,
                LocalCheckpointFeatureAdjustmentEvidence,
                unitEvidence,
                $"learned-inference:{unit}"
            }
            .Concat(contributions.SelectMany(contribution => contribution.EvidenceRefs ?? []))
            .Concat(contributions.Select(contribution =>
                $"local-checkpoint-feature-adjustment:{unit}:feature={SanitizeFeatureToken(contribution.FeatureKey)}:obs={contribution.ObservationCount}"));

        if (HasDpoTrainerPolicyCheckpointEvidence(contributions))
            evidenceRefs = evidenceRefs.Append(DpoTrainerPolicyCheckpointRuntimeScoringEvidence);

        return evidenceRefs
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildLocalCheckpointAdjustmentSummary(
        string unit,
        double beforeScore,
        double afterScore,
        double adjustment,
        IReadOnlyList<LocalCheckpointFeatureContribution> contributions)
    {
        var authority = HasDpoTrainerPolicyCheckpointEvidence(contributions)
            ? "dpo_trainer_policy_checkpoint_converted_not_product_ready"
            : "not_trained_model";
        return string.Join(
            ";",
            "LocalCheckpointFeatureAdjustmentSummary",
            $"unit={unit}",
            $"features={contributions.Count.ToString(CultureInfo.InvariantCulture)}",
            $"adjustment={adjustment.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"score_before={beforeScore.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"score_after={afterScore.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"acceptance_threshold={LocalCheckpointRuntimeAcceptanceThreshold.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"adjustment_scale=tanh_bounded_{LocalCheckpointAdjustmentClamp.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"authority={authority}");
    }

    private static bool HasDpoTrainerPolicyCheckpointEvidence(IReadOnlyList<LocalCheckpointFeatureContribution> contributions)
        => contributions.Any(contribution =>
            contribution.EvidenceRefs.Contains(DpoTrainerPolicyCheckpointConvertedEvidence, StringComparer.Ordinal) ||
            contribution.EvidenceRefs.Contains(DpoTrainerPolicyCheckpointFreshLoadedEvidence, StringComparer.Ordinal));

    private static string[] BuildLocalCheckpointAdjustmentWarnings(
        IReadOnlyList<string> warnings,
        string localAdjustmentWarning)
        => (warnings ?? [])
            .Append(RuntimeScaffoldNotTrainedModelWarning)
            .Append(localAdjustmentWarning)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static double Clamp01(double value)
        => Math.Clamp(value, 0.0, 1.0);

    private sealed class InlineDesignConversionOrchestrator(
        DesignConversionRoutingFlow flow,
        bool localCheckpointWeightsConfigured) : ILogicalOrchestrator
    {
        public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
            LogicalOrchestratorRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalOrchestratorResult>.Fail(
                    "DesignConversion orchestrator cancelled.",
                    "CANCELLED");

            if (request is null)
                return OperationResult<LogicalOrchestratorResult>.Fail(
                    "Orchestrator request is required.",
                    "REQUEST_REQUIRED");

            var input = ReadInput(request.Metadata);
            if (input is null)
                return OperationResult<LogicalOrchestratorResult>.Fail(
                    "DesignConversionRuntimeFlowInput must be supplied in orchestrator metadata under 'runtime_flow_input' or 'design_conversion_input'.",
                    "DESIGN_CONVERSION_INPUT_REQUIRED");

            var flags = request.Flags ?? new LogicalExecutionFlags();
            var flowResult = await flow.ExecuteAsync(new LogicalFlowRequest<DesignConversionRuntimeFlowInput>(
                RunId: request.RunId,
                FlowId: DesignConversionRoutingFlow.FlowId,
                ScenarioId: DesignConversionRoutingFlow.ScenarioId,
                Input: input,
                Flags: flags,
                Metadata: request.Metadata), cancellationToken);

            if (!flowResult.IsSuccess)
                return OperationResult<LogicalOrchestratorResult>.Fail(flowResult.ErrorMessage, flowResult.ErrorCode);

            var normalizedFlowResult = NormalizeLocalCheckpointAdjustedFlowResult(
                flowResult.Value!,
                localCheckpointWeightsConfigured);
            return OperationResult<LogicalOrchestratorResult>.Ok(new LogicalOrchestratorResult(
                ScenarioId: request.ScenarioId,
                SelectedFlowId: DesignConversionRoutingFlow.FlowId,
                OverallVerdict: normalizedFlowResult.OverallVerdict,
                FinalHumanResult: normalizedFlowResult.FinalHumanResult,
                AppliedFlags: flags,
                FlowResult: normalizedFlowResult)
            {
                Warnings = BuildOrchestratorWarnings(normalizedFlowResult, localCheckpointWeightsConfigured)
            });
        }

        private static LogicalFlowResult NormalizeLocalCheckpointAdjustedFlowResult(
            LogicalFlowResult result,
            bool localCheckpointWeightsConfigured)
        {
            var localCheckpointAdjustmentApplied = result.EvidenceIds.Contains(LearnedInferenceAppliedEvidence, StringComparer.Ordinal);
            if (!localCheckpointAdjustmentApplied)
                return result;

            var contributionCount = result.EvidenceIds
                .Count(id => id.StartsWith("local-checkpoint-feature-adjustment:", StringComparison.Ordinal));
            var dpoTrainerPolicyCheckpointConverted = result.EvidenceIds.Contains(DpoTrainerPolicyCheckpointConvertedEvidence, StringComparer.Ordinal);
            var dpoTrainerPolicyCheckpointFreshLoaded = result.EvidenceIds.Contains(DpoTrainerPolicyCheckpointFreshLoadedEvidence, StringComparer.Ordinal);
            var localCheckpointWeightsSource = dpoTrainerPolicyCheckpointConverted
                ? "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted"
                : "local_checkpoint_sanitized_features_not_trained_model";
            var metrics = result.Metrics
                .Concat(new Dictionary<string, string>
                {
                    ["LocalCheckpointFeatureAdjustmentApplied"] = bool.TrueString,
                    ["LocalCheckpointFeatureContributionCount"] = contributionCount.ToString(CultureInfo.InvariantCulture),
                    ["local_checkpoint_weights_configured"] = localCheckpointWeightsConfigured.ToString(),
                    ["local_checkpoint_weights_source"] = localCheckpointWeightsSource,
                    ["local_checkpoint_feature_adjustment_applied"] = bool.TrueString,
                    ["dpo_trainer_policy_checkpoint_converted"] = dpoTrainerPolicyCheckpointConverted.ToString(),
                    ["dpo_trainer_policy_checkpoint_fresh_loaded"] = dpoTrainerPolicyCheckpointFreshLoaded.ToString(),
                    [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
                    [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue
                })
                .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
            var warnings = result.Warnings
                .Append("DESIGN_CONVERSION_RUNTIME_SCAFFOLD_NOT_LEARNED_OUTPUT")
                .Append(RuntimeScaffoldNotTrainedModelWarning)
                .Append("DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_NOT_TRAINED_AUTHORITY")
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var summary = result.Summary with
            {
                PrimaryMetricName = "critic_local_checkpoint_feature_adjusted_score",
                ScalingSignalNote = dpoTrainerPolicyCheckpointConverted
                    ? "DpoTrainer policy checkpoint was fresh-loaded then converted into sanitized runtime feature weights; this is not product-ready trained-model authority."
                    : "Local checkpoint feature weights adjusted critic confidence; this is not trained model authority.",
                Notes = dpoTrainerPolicyCheckpointConverted
                    ? "DesignConversion flow used a fresh-loaded 11.DPO/DpoTrainer policy checkpoint converted into sanitized runtime feature-weight wrappers; ProductReady/model training remains false; evidence contains sanitized feature keys and counts only."
                    : "DesignConversion flow used local checkpoint feature-weight wrappers over Critic and Improver scaffold contracts; ProductReady/model training remains false; evidence contains sanitized feature keys and counts only."
            };

            return result with
            {
                Summary = summary,
                Metrics = metrics,
                EvidenceIds = result.EvidenceIds
                    .Append(LocalCheckpointFeatureAdjustmentEvidence)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Warnings = warnings
            };
        }

        private static string[] BuildOrchestratorWarnings(
            LogicalFlowResult result,
            bool localCheckpointWeightsConfigured)
        {
            var localCheckpointAdjustmentApplied = result.EvidenceIds.Contains(LearnedInferenceAppliedEvidence, StringComparer.Ordinal);
            if (localCheckpointAdjustmentApplied)
            {
                return
                [
                    "DESIGN_CONVERSION_MACHINE_RUNTIME_SCAFFOLD_NOT_LEARNED",
                    RuntimeScaffoldNotTrainedModelWarning,
                    "DESIGN_CONVERSION_MACHINE_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_NOT_TRAINED_AUTHORITY",
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ];
            }

            return localCheckpointWeightsConfigured
                ?
                [
                    "DESIGN_CONVERSION_MACHINE_RUNTIME_LOCAL_CHECKPOINT_WEIGHTS_CONFIGURED_NO_MATCHING_RUNTIME_FEATURE_NOT_TRAINED_MODEL",
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ]
                :
                [
                    "DESIGN_CONVERSION_MACHINE_RUNTIME_SCAFFOLD_NOT_LEARNED",
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ];
        }

        private static DesignConversionRuntimeFlowInput? ReadInput(IReadOnlyDictionary<string, object> metadata)
        {
            if (metadata.TryGetValue("runtime_flow_input", out var runtimeInput) &&
                runtimeInput is DesignConversionRuntimeFlowInput typedRuntimeInput)
                return typedRuntimeInput;

            if (metadata.TryGetValue("design_conversion_input", out var designInput) &&
                designInput is DesignConversionRuntimeFlowInput typedDesignInput)
                return typedDesignInput;

            return null;
        }
    }
}
