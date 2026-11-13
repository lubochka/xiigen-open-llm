namespace TranslationDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using TensorCore.Core;

public sealed record TranslationDpoTokenizationSettings(
    int VocabSize = 512,
    int ContextLength = 64);

public sealed record TranslationDpoBatchRecord(
    string PairId,
    string LearningUnitId,
    string CapabilityId,
    string Direction,
    string SourceLanguage,
    string TargetLanguage,
    DpoPair SourcePair,
    DpoBatch Batch,
    string PromptFingerprint,
    string ChosenTextFingerprint,
    string RejectedTextFingerprint,
    IReadOnlyList<int> ChosenTokenIds,
    IReadOnlyList<int> RejectedTokenIds);

public sealed record TranslationDpoBatchBuildResult(
    int TrainPairCount,
    int HeldOutExcludedCount,
    IReadOnlyList<DpoBatch> Batches,
    IReadOnlyList<TranslationDpoBatchRecord> Records,
    IReadOnlyList<string> HeldOutEvaluationPairIds,
    IReadOnlyList<string> CapabilityIds,
    IReadOnlyList<string> Directions,
    string NextEvaluationBoundary);

/// <summary>
/// One immutable admission-and-mutation view of the final Translation DPO
/// tensors. The snapshot is created only by <see
/// cref="TranslationDpoTrainerBridge.BuildMutationBatchSnapshot"/> so the
/// content admitted by the server and the <see cref="DpoBatch"/> instances
/// passed to <c>DpoTrainer.StepMany</c> cannot be rebuilt independently.
/// </summary>
public sealed class TranslationDpoMutationBatchSnapshot
{
    internal TranslationDpoMutationBatchSnapshot(
        TranslationDpoTrainingHandoff handoff,
        TranslationDpoTokenizationSettings tokenizationSettings,
        TranslationDpoBatchBuildResult batchBuild,
        DpoTrainingContentBatch trainingContent)
    {
        Handoff = handoff;
        TokenizationSettings = tokenizationSettings;
        BatchBuild = batchBuild;
        TrainingContent = trainingContent;
    }

    internal TranslationDpoTrainingHandoff Handoff { get; }

    public TranslationDpoTokenizationSettings TokenizationSettings { get; }

    public TranslationDpoBatchBuildResult BatchBuild { get; }

    public DpoTrainingContentBatch TrainingContent { get; }
}

public sealed record TranslationDpoEvaluationBatchBuildResult(
    int HeldOutEvaluationCount,
    int TrainExcludedCount,
    IReadOnlyList<DpoBatch> Batches,
    IReadOnlyList<TranslationDpoBatchRecord> Records,
    IReadOnlyList<string> ExcludedTrainPairIds,
    IReadOnlyList<string> CapabilityIds,
    IReadOnlyList<string> Directions,
    string EvaluationBoundary);

public sealed record TranslationDpoTrainerBridgeResult(
    int Step,
    int TrainPairCount,
    int HeldOutExcludedCount,
    DpoMetrics TrainerMetrics,
    IReadOnlyList<string> TrainedPairIds,
    IReadOnlyList<string> HeldOutEvaluationPairIds,
    IReadOnlyList<string> CapabilityIds,
    IReadOnlyList<string> Directions,
    string TrainerCallPath,
    string NextEvaluationBoundary);

public sealed record TranslationDpoHeldOutEvaluationResult(
    int Step,
    int HeldOutEvaluationCount,
    int TrainExcludedCount,
    DpoMetrics EvaluationMetrics,
    IReadOnlyList<string> HeldOutEvaluationPairIds,
    IReadOnlyList<string> ExcludedTrainPairIds,
    IReadOnlyList<string> CapabilityIds,
    IReadOnlyList<string> Directions,
    string TrainerCallPath,
    bool SemanticQualityClaimed,
    string SemanticQualityBoundary);

public sealed class TranslationDpoTextFeaturizer
{
    public const string TensorContextFormatVersion =
        "translation-dpo-final-token-tensors-v2";

    private static readonly string[] ArrowDiagnosticPrefixes =
    [
        "PHRASE:",
        "AGREEMENT:"
    ];

    private const int PadId = 0;
    private const int BosId = 1;
    private const int SeparatorId = 2;
    private const int EosId = 3;
    private const int FirstHashedTokenId = 4;

    private static readonly Regex TokenRegex = new(
        @"[\p{L}\p{N}_$][\p{L}\p{N}'_\-$]*|[^\s]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public TranslationDpoTextFeaturizer(TranslationDpoTokenizationSettings settings)
    {
        Settings = settings;
    }

    public TranslationDpoTokenizationSettings Settings { get; }

    public OperationResult<TranslationDpoBatchRecord> BuildRecord(TranslationDpoTrainingHandoffRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var validation = TranslationDpoTrainerBridge.ValidateTrainingRecord(record);
        if (!validation.IsSuccess)
            return validation.MapError<TranslationDpoBatchRecord>();

        return BuildRecord(new TranslationDpoTextRecordShape(
            RowKind: "TRAIN",
            PairId: record.PairId,
            LearningUnitId: record.LearningUnitId,
            CapabilityId: record.CapabilityId,
            Direction: record.Direction,
            SourceLanguage: record.SourceLanguage,
            TargetLanguage: record.TargetLanguage,
            Pair: record.TrainerPair,
            Admission: record.Admission));
    }

    public OperationResult<TranslationDpoBatchRecord> BuildRecord(TranslationDpoHeldOutEvaluationRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var validation = TranslationDpoTrainerBridge.ValidateHeldOutRecord(record);
        if (!validation.IsSuccess)
            return validation.MapError<TranslationDpoBatchRecord>();

        return BuildRecord(new TranslationDpoTextRecordShape(
            RowKind: "HELDOUT",
            PairId: record.PairId,
            LearningUnitId: record.LearningUnitId,
            CapabilityId: record.CapabilityId,
            Direction: record.Direction,
            SourceLanguage: record.SourceLanguage,
            TargetLanguage: record.TargetLanguage,
            Pair: new DpoPair
            {
                Prompt = $"{record.Prompt}\nInput: {record.Input}",
                Chosen = record.ExpectedResponse,
                Rejected = BuildHeldOutRejectedText(record)
            },
            Admission: null));
    }

    private OperationResult<TranslationDpoBatchRecord> BuildRecord(TranslationDpoTextRecordShape record)
    {
        // The audit pair intentionally stays raw, but the model-visible text must not
        // contain teacher diagnostics such as wrong-answer or phrase-agreement hints.
        var chosenTextForModel = StripReservedDiagnosticsForModel(record.Pair.Chosen);
        if (string.IsNullOrWhiteSpace(chosenTextForModel))
            return EmptyAfterDiagnosticStrip(record.RowKind, "CHOSEN", record.PairId);

        var rejectedTextForModel = StripReservedDiagnosticsForModel(record.Pair.Rejected);
        if (string.IsNullOrWhiteSpace(rejectedTextForModel))
            return EmptyAfterDiagnosticStrip(record.RowKind, "REJECTED", record.PairId);

        var chosen = EncodeSide(record, chosenTextForModel, "chosen");
        var rejected = EncodeSide(record, rejectedTextForModel, "rejected");
        var batch = new DpoBatch
        {
            ChosenIds = ToTensor(chosen.TokenIds),
            ChosenMask = ToMask(chosen.TokenIds.Count),
            RejectedIds = ToTensor(rejected.TokenIds),
            RejectedMask = ToMask(rejected.TokenIds.Count),
            Admission = record.Admission
        };

        return OperationResult<TranslationDpoBatchRecord>.Ok(new TranslationDpoBatchRecord(
            PairId: record.PairId,
            LearningUnitId: record.LearningUnitId,
            CapabilityId: record.CapabilityId,
            Direction: record.Direction,
            SourceLanguage: record.SourceLanguage,
            TargetLanguage: record.TargetLanguage,
            SourcePair: record.Pair,
            Batch: batch,
            PromptFingerprint: StableSha256(BuildPromptPrefix(record)),
            ChosenTextFingerprint: chosen.TextFingerprint,
            RejectedTextFingerprint: rejected.TextFingerprint,
            ChosenTokenIds: chosen.TokenIds,
            RejectedTokenIds: rejected.TokenIds));
    }

    private static string BuildHeldOutRejectedText(TranslationDpoHeldOutEvaluationRecord record)
    {
        var marker = string.IsNullOrWhiteSpace(record.RejectionMarker)
            ? "heldout_translation_quality_miss"
            : record.RejectionMarker.Trim();
        var reason = string.IsNullOrWhiteSpace(record.RejectionReason)
            ? marker
            : record.RejectionReason.Trim();
        return $"{record.RejectedResponse} [WRONG: {marker} - {reason}]";
    }

    private EncodedSide EncodeSide(
        TranslationDpoTextRecordShape record,
        string response,
        string side)
    {
        var prefix = BuildPromptPrefix(record);
        var prefixTokens = Tokenize(prefix);
        var responseTokens = Tokenize(response);

        var maxBodyTokens = Settings.ContextLength - 3;
        var prefixBudget = Math.Min(prefixTokens.Count, Math.Max(1, maxBodyTokens / 2));
        var responseBudget = Math.Max(1, maxBodyTokens - prefixBudget);
        if (prefixBudget + responseBudget > maxBodyTokens)
            prefixBudget = Math.Max(1, maxBodyTokens - responseBudget);

        var ids = new List<int>(Settings.ContextLength) { BosId };
        ids.AddRange(prefixTokens.Take(prefixBudget).Select(ToStableTokenId));
        ids.Add(SeparatorId);
        ids.AddRange(responseTokens.Take(responseBudget).Select(ToStableTokenId));
        ids.Add(EosId);

        return new EncodedSide(ids, StableSha256($"{side}\n{prefix}\n{response}"));
    }

    private static OperationResult<TranslationDpoBatchRecord> EmptyAfterDiagnosticStrip(
        string rowKind,
        string side,
        string pairId)
        => OperationResult<TranslationDpoBatchRecord>.Fail(
            $"Translation DPO {rowKind.ToLowerInvariant()} {side.ToLowerInvariant()} text for row '{pairId}' becomes empty after removing audit-only diagnostics.",
            $"{rowKind}_{side}_TEXT_EMPTY_AFTER_DIAGNOSTIC_STRIP");

    private static string StripReservedDiagnosticsForModel(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var builder = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length;)
        {
            if (value[index] == '[')
            {
                var end = value.IndexOf(']', index + 1);
                var nestedStart = value.IndexOf('[', index + 1);
                if (end >= 0 && (nestedStart < 0 || nestedStart > end))
                {
                    var content = value.Substring(index + 1, end - index - 1);
                    if (IsReservedDiagnosticBlock(content))
                    {
                        index = end + 1;
                        continue;
                    }
                }
            }

            // Ordinary brackets and unclosed brackets are translation content, not
            // teacher audit grammar, so they remain visible to the model.
            builder.Append(value[index]);
            index++;
        }

        return NormalizeModelWhitespace(builder.ToString());
    }

    private static bool IsReservedDiagnosticBlock(string bracketContent)
    {
        var trimmed = (bracketContent ?? string.Empty).Trim();
        if (trimmed.StartsWith("WRONG:", StringComparison.OrdinalIgnoreCase))
            return true;

        // Only the active audit markers that carry source=>target hints are removed;
        // ordinary bracket text with arrows still belongs to the translation sample.
        return trimmed.Contains("=>", StringComparison.Ordinal) &&
               ArrowDiagnosticPrefixes.Any(prefix =>
                   trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeModelWhitespace(string value)
        => Regex.Replace(
                (value ?? string.Empty).Normalize(NormalizationForm.FormC),
                @"\s+",
                " ")
            .Trim();

    private string BuildPromptPrefix(TranslationDpoTextRecordShape record)
    {
        // Текст source для training берётся только из допущенного TrainerPair.Prompt.
        // record.Input остаётся metadata/leakage key и не меняет байты модели.
        return string.Join('\n',
        [
            $"domain:{TranslationMachine.DomainTag}",
            $"machine:{TranslationMachine.MachineId}",
            $"direction:{record.Direction}",
            $"capability:{record.CapabilityId}",
            $"unit:{record.LearningUnitId}",
            $"source:{record.SourceLanguage}",
            $"target:{record.TargetLanguage}",
            $"prompt:{record.Pair.Prompt}"
        ]);
    }

    private IReadOnlyList<string> Tokenize(string value)
        => TokenRegex
            .Matches((value ?? string.Empty).Normalize(NormalizationForm.FormC).ToLowerInvariant())
            .Select(match => match.Value)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();

    private int ToStableTokenId(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token ?? string.Empty));
        var value = BitConverter.ToUInt32(hash, 0);
        var range = Settings.VocabSize - FirstHashedTokenId;
        return FirstHashedTokenId + (int)(value % (uint)range);
    }

    private Tensor ToTensor(IReadOnlyList<int> tokenIds)
    {
        var data = new float[Settings.ContextLength];
        for (var index = 0; index < data.Length; index++)
            data[index] = index < tokenIds.Count ? tokenIds[index] : PadId;

        return new Tensor(data, new TensorShape(1, Settings.ContextLength));
    }

    private Tensor ToMask(int tokenCount)
    {
        var data = new float[Settings.ContextLength];
        for (var index = 0; index < Math.Min(tokenCount, data.Length); index++)
            data[index] = 1f;

        return new Tensor(data, new TensorShape(1, Settings.ContextLength));
    }

    private static string StableSha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant();

    private sealed record EncodedSide(IReadOnlyList<int> TokenIds, string TextFingerprint);

    private sealed record TranslationDpoTextRecordShape(
        string RowKind,
        string PairId,
        string LearningUnitId,
        string CapabilityId,
        string Direction,
        string SourceLanguage,
        string TargetLanguage,
        DpoPair Pair,
        DpoTrainerPairAdmission? Admission);
}

public static class TranslationDpoTrainerBridge
{
    public const string TrainerCallPath = "DPO.DpoTrainer.StepMany";
    public const string EvaluationCallPath = "DPO.DpoTrainer.EvaluateMany";
    public const string NextEvaluationBoundary =
        "Evaluate TranslationDpoTrainingHandoff.HeldOutEvaluationRows after training; do not pass held-out rows to DpoTrainer.StepMany.";
    public const string HeldOutEvaluationBoundary =
        "Held-out rows are converted to evaluation-only DpoBatch values and passed to DpoTrainer.EvaluateMany; train rows stay excluded.";
    public const string SemanticQualityNotReadyBoundary =
        "SemanticQualityClaimed=false: held-out DPO metrics are evaluation evidence only and do not claim semantic quality readiness.";
    internal const string DirectTrainerTrainWithStepManyBoundaryMessage =
        "TranslationDpoTrainerBridge.TrainWithStepMany direct DpoTrainer overloads are a compatibility/testing surface; production composition uses TranslationDpoTrainEvaluateRunner with IServerOwnedDpoTrainingLeaseBroker-issued DpoTrainerMutationAdmissionCapability and the final fail-closed mutation gate.";

    private const string SelfAuthoredCategory = "synthetic_self_authored";
    private const string InternalLicense = "project_internal_self_authored";
    private const string NoTeacherValue = "none";
    private const string RepositoryOwnedTrainingPairFamily =
        "repository-owned-dpo-training-pair-v1";

    public static OperationResult<TranslationDpoBatchBuildResult> BuildTrainDpoBatches(
        TranslationDpoTrainingHandoff handoff,
        TranslationDpoTokenizationSettings? settings = null)
    {
        var normalizedSettings = ValidateSettings(settings ?? new TranslationDpoTokenizationSettings());
        if (!normalizedSettings.IsSuccess)
            return normalizedSettings.MapError<TranslationDpoBatchBuildResult>();

        var validation = ValidateHandoffForTraining(handoff);
        if (!validation.IsSuccess)
            return validation.MapError<TranslationDpoBatchBuildResult>();

        var featurizer = new TranslationDpoTextFeaturizer(normalizedSettings.Value!);
        var records = new List<TranslationDpoBatchRecord>(handoff.TrainingPairs.Count);
        foreach (var pair in handoff.TrainingPairs)
        {
            var record = featurizer.BuildRecord(pair);
            if (!record.IsSuccess || record.Value is null)
                return record.MapError<TranslationDpoBatchBuildResult>();
            records.Add(record.Value);
        }

        return OperationResult<TranslationDpoBatchBuildResult>.Ok(
            new TranslationDpoBatchBuildResult(
                TrainPairCount: records.Count,
                HeldOutExcludedCount: handoff.HeldOutEvaluationRows.Count,
                Batches: Array.AsReadOnly(
                    records.Select(record => record.Batch).ToArray()),
                Records: Array.AsReadOnly(records.ToArray()),
                HeldOutEvaluationPairIds: Array.AsReadOnly(handoff.HeldOutEvaluationRows
                    .Select(row => row.PairId)
                    .Order(StringComparer.Ordinal)
                    .ToArray()),
                CapabilityIds: Array.AsReadOnly(records
                    .Select(record => record.CapabilityId)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()),
                Directions: Array.AsReadOnly(records
                    .Select(record => record.Direction)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()),
                NextEvaluationBoundary: NextEvaluationBoundary));
    }

    public static OperationResult<TranslationDpoMutationBatchSnapshot>
        BuildMutationBatchSnapshot(
            TranslationDpoTrainingHandoff handoff,
            TranslationDpoTokenizationSettings? settings = null)
    {
        var normalizedSettings = ValidateSettings(
            settings ?? new TranslationDpoTokenizationSettings());
        if (!normalizedSettings.IsSuccess)
            return normalizedSettings
                .MapError<TranslationDpoMutationBatchSnapshot>();

        var batchResult = BuildTrainDpoBatches(
            handoff,
            normalizedSettings.Value!);
        if (!batchResult.IsSuccess || batchResult.Value is null)
            return batchResult.MapError<TranslationDpoMutationBatchSnapshot>();

        var content = ProjectMutationTrainingContent(
            handoff,
            batchResult.Value,
            normalizedSettings.Value!);
        if (!content.IsSuccess || content.Value is null)
            return content.MapError<TranslationDpoMutationBatchSnapshot>();

        return OperationResult<TranslationDpoMutationBatchSnapshot>.Ok(
            new TranslationDpoMutationBatchSnapshot(
                handoff,
                normalizedSettings.Value!,
                batchResult.Value,
                content.Value));
    }

    [Obsolete(DirectTrainerTrainWithStepManyBoundaryMessage, false)]
    public static OperationResult<TranslationDpoTrainerBridgeResult> TrainWithStepMany(
        TranslationDpoTrainingHandoff handoff,
        DpoTrainer trainer,
        int step,
        TranslationDpoTokenizationSettings? settings = null)
    {
        if (trainer is null)
            return OperationResult<TranslationDpoTrainerBridgeResult>.Fail(
                "Translation DPO trainer bridge requires a DpoTrainer instance.",
                "DPO_TRAINER_REQUIRED");

        var snapshot = BuildMutationBatchSnapshot(handoff, settings);
        if (!snapshot.IsSuccess || snapshot.Value is null)
            return snapshot.MapError<TranslationDpoTrainerBridgeResult>();

        return TrainAdmittedWithStepMany(handoff, snapshot.Value, trainer, step);
    }

    [Obsolete(DirectTrainerTrainWithStepManyBoundaryMessage, false)]
    public static OperationResult<TranslationDpoTrainerBridgeResult> TrainWithStepMany(
        TranslationDpoTrainingHandoff handoff,
        TranslationDpoMutationBatchSnapshot snapshot,
        DpoTrainer trainer,
        int step)
    {
        return TrainAdmittedWithStepMany(handoff, snapshot, trainer, step);
    }

    // Brokered runners enter this internal seam only after the server lease broker has issued the trainer and mutation capability.
    internal static OperationResult<TranslationDpoTrainerBridgeResult> TrainAdmittedWithStepMany(
        TranslationDpoTrainingHandoff handoff,
        TranslationDpoMutationBatchSnapshot snapshot,
        DpoTrainer trainer,
        int step)
    {
        if (trainer is null)
            return OperationResult<TranslationDpoTrainerBridgeResult>.Fail(
                "Translation DPO trainer bridge requires a DpoTrainer instance.",
                "DPO_TRAINER_REQUIRED");
        if (snapshot is null || !ReferenceEquals(snapshot.Handoff, handoff))
            return OperationResult<TranslationDpoTrainerBridgeResult>.Fail(
                "Translation DPO final tensor snapshot does not belong to this training handoff.",
                "DPO_TRAINING_SNAPSHOT_HANDOFF_MISMATCH");

        var batchResult = snapshot.BatchBuild;
        var integrity = ProjectMutationTrainingContent(
            handoff,
            batchResult,
            snapshot.TokenizationSettings);
        if (!integrity.IsSuccess || integrity.Value is null ||
            !string.Equals(
                integrity.Value.Sha256,
                snapshot.TrainingContent.Sha256,
                StringComparison.Ordinal))
        {
            return OperationResult<TranslationDpoTrainerBridgeResult>.Fail(
                "Translation DPO final tensor snapshot changed after mutation admission.",
                "DPO_TRAINING_SNAPSHOT_INTEGRITY_FAILED");
        }

        DpoMetrics metrics;
        try
        {
            metrics = trainer.StepMany(batchResult.Batches, step);
        }
        catch (DpoMutationAdmissionException ex)
        {
            return OperationResult<TranslationDpoTrainerBridgeResult>.Fail(
                "Translation DPO mutation admission is not live.",
                ex.ErrorCode);
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationDpoTrainerBridgeResult>.Fail(
                $"DpoTrainer.StepMany failed: {ex.Message}",
                "DPO_TRAINER_STEP_MANY_FAILED");
        }

        return OperationResult<TranslationDpoTrainerBridgeResult>.Ok(
            new TranslationDpoTrainerBridgeResult(
                Step: step,
                TrainPairCount: batchResult.TrainPairCount,
                HeldOutExcludedCount: batchResult.HeldOutExcludedCount,
                TrainerMetrics: metrics,
                TrainedPairIds: batchResult.Records
                    .Select(record => record.PairId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                HeldOutEvaluationPairIds: batchResult.HeldOutEvaluationPairIds,
                CapabilityIds: batchResult.CapabilityIds,
                Directions: batchResult.Directions,
                TrainerCallPath: TrainerCallPath,
                NextEvaluationBoundary: NextEvaluationBoundary));
    }

    private static OperationResult<DpoTrainingContentBatch>
        ProjectMutationTrainingContent(
            TranslationDpoTrainingHandoff handoff,
            TranslationDpoBatchBuildResult batches,
            TranslationDpoTokenizationSettings settings)
    {
        try
        {
            if (batches.Records.Count != handoff.TrainingPairs.Count ||
                batches.Batches.Count != handoff.TrainingPairs.Count)
            {
                return OperationResult<DpoTrainingContentBatch>.Fail(
                    "Translation DPO final tensor snapshot does not match the training handoff.",
                    "DPO_TRAINING_SNAPSHOT_HANDOFF_MISMATCH");
            }

            var rows = new DpoTrainingContentRowInput[batches.Records.Count];
            for (var ordinal = 0; ordinal < batches.Records.Count; ordinal++)
            {
                var record = batches.Records[ordinal];
                var source = handoff.TrainingPairs[ordinal];
                if (!ReferenceEquals(record.Batch, batches.Batches[ordinal]) ||
                    !string.Equals(record.PairId, source.PairId, StringComparison.Ordinal) ||
                    !string.Equals(record.LearningUnitId, source.LearningUnitId, StringComparison.Ordinal) ||
                    !string.Equals(record.CapabilityId, source.CapabilityId, StringComparison.Ordinal) ||
                    !string.Equals(record.Direction, source.Direction, StringComparison.Ordinal) ||
                    !string.Equals(record.SourceLanguage, source.SourceLanguage, StringComparison.Ordinal) ||
                    !string.Equals(record.TargetLanguage, source.TargetLanguage, StringComparison.Ordinal))
                {
                    return OperationResult<DpoTrainingContentBatch>.Fail(
                        "Translation DPO final tensor snapshot identity does not match the training handoff.",
                        "DPO_TRAINING_SNAPSHOT_HANDOFF_MISMATCH");
                }

                rows[ordinal] = ToMutationTrainingContentRow(
                    handoff,
                    source,
                    record.Batch,
                    settings,
                    ordinal);
            }

            return BoundedDpoTrainingContentProjector.Project(rows);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DpoTrainingContentBatch>.Fail(
                "Translation DPO final tensor snapshot could not be projected.",
                BoundedDpoTrainingContentProjector.InvalidErrorCode);
        }
    }

    private static DpoTrainingContentRowInput ToMutationTrainingContentRow(
        TranslationDpoTrainingHandoff handoff,
        TranslationDpoTrainingHandoffRecord source,
        DpoBatch batch,
        TranslationDpoTokenizationSettings settings,
        int ordinal)
        => new(
            Prompt: SerializeTensorBits("chosen_ids", batch.ChosenIds),
            Chosen: SerializeTensorBits("chosen_mask", batch.ChosenMask),
            Rejected: string.Concat(
                SerializeTensorBits("rejected_ids", batch.RejectedIds),
                "\n",
                SerializeTensorBits("rejected_mask", batch.RejectedMask)),
            TeachingVector: null,
            Context: string.Join(
                "\n",
                TranslationDpoTextFeaturizer.TensorContextFormatVersion,
                CanonicalContextField("domain", handoff.DomainTag),
                CanonicalContextField("machine", handoff.MachineId),
                CanonicalContextField("handoff", handoff.HandoffId),
                CanonicalContextField("batch", handoff.SourceBatchId),
                $"ordinal={ordinal.ToString(CultureInfo.InvariantCulture)}",
                CanonicalContextField("pair", source.PairId),
                CanonicalContextField("learning_unit", source.LearningUnitId),
                CanonicalContextField("capability", source.CapabilityId),
                CanonicalContextField("direction", source.Direction),
                CanonicalContextField("source_language", source.SourceLanguage),
                CanonicalContextField("target_language", source.TargetLanguage),
                $"vocab_size={settings.VocabSize.ToString(CultureInfo.InvariantCulture)}",
                $"context_len={settings.ContextLength.ToString(CultureInfo.InvariantCulture)}"));

    private static string CanonicalContextField(string name, string value)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        return string.Concat(
            name,
            "=",
            byteCount.ToString(CultureInfo.InvariantCulture),
            ":",
            value);
    }

    private static string SerializeTensorBits(string name, Tensor tensor)
    {
        var builder = new StringBuilder(name.Length + 64 + tensor.Data.Length * 8);
        builder.Append(name);
        builder.Append(";f32;shape=");
        for (var index = 0; index < tensor.Shape.Dimensions.Length; index++)
        {
            if (index > 0)
                builder.Append(',');
            builder.Append(tensor.Shape.Dimensions[index]
                .ToString(CultureInfo.InvariantCulture));
        }
        builder.Append(";bits=");
        foreach (var value in tensor.Data)
        {
            builder.Append(BitConverter.SingleToInt32Bits(value)
                .ToString("x8", CultureInfo.InvariantCulture));
        }
        return builder.ToString();
    }

    public static OperationResult<TranslationDpoEvaluationBatchBuildResult> BuildHeldOutEvaluationDpoBatches(
        TranslationDpoTrainingHandoff handoff,
        TranslationDpoTokenizationSettings? settings = null)
    {
        var normalizedSettings = ValidateSettings(settings ?? new TranslationDpoTokenizationSettings());
        if (!normalizedSettings.IsSuccess)
            return normalizedSettings.MapError<TranslationDpoEvaluationBatchBuildResult>();

        var validation = ValidateHandoffForEvaluation(handoff);
        if (!validation.IsSuccess)
            return validation.MapError<TranslationDpoEvaluationBatchBuildResult>();

        var featurizer = new TranslationDpoTextFeaturizer(normalizedSettings.Value!);
        var records = new List<TranslationDpoBatchRecord>(handoff.HeldOutEvaluationRows.Count);
        foreach (var row in handoff.HeldOutEvaluationRows)
        {
            var record = featurizer.BuildRecord(row);
            if (!record.IsSuccess || record.Value is null)
                return record.MapError<TranslationDpoEvaluationBatchBuildResult>();
            records.Add(record.Value);
        }

        return OperationResult<TranslationDpoEvaluationBatchBuildResult>.Ok(
            new TranslationDpoEvaluationBatchBuildResult(
                HeldOutEvaluationCount: records.Count,
                TrainExcludedCount: handoff.TrainingPairs.Count,
                Batches: records.Select(record => record.Batch).ToArray(),
                Records: records.ToArray(),
                ExcludedTrainPairIds: handoff.TrainingPairs
                    .Select(row => row.PairId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                CapabilityIds: records
                    .Select(record => record.CapabilityId)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                Directions: records
                    .Select(record => record.Direction)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                EvaluationBoundary: HeldOutEvaluationBoundary));
    }

    public static OperationResult<TranslationDpoHeldOutEvaluationResult> EvaluateHeldOutWithEvaluateMany(
        TranslationDpoTrainingHandoff handoff,
        DpoTrainer trainer,
        int step,
        TranslationDpoTokenizationSettings? settings = null)
    {
        if (trainer is null)
            return OperationResult<TranslationDpoHeldOutEvaluationResult>.Fail(
                "Translation DPO held-out evaluation requires a DpoTrainer instance.",
                "DPO_TRAINER_REQUIRED");

        var batchResult = BuildHeldOutEvaluationDpoBatches(handoff, settings);
        if (!batchResult.IsSuccess)
            return batchResult.MapError<TranslationDpoHeldOutEvaluationResult>();

        DpoMetrics metrics;
        try
        {
            metrics = trainer.EvaluateMany(batchResult.Value!.Batches, step);
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationDpoHeldOutEvaluationResult>.Fail(
                $"DpoTrainer.EvaluateMany failed: {ex.Message}",
                "DPO_TRAINER_EVALUATE_MANY_FAILED");
        }

        return OperationResult<TranslationDpoHeldOutEvaluationResult>.Ok(
            new TranslationDpoHeldOutEvaluationResult(
                Step: step,
                HeldOutEvaluationCount: batchResult.Value.HeldOutEvaluationCount,
                TrainExcludedCount: batchResult.Value.TrainExcludedCount,
                EvaluationMetrics: metrics,
                HeldOutEvaluationPairIds: batchResult.Value.Records
                    .Select(record => record.PairId)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                ExcludedTrainPairIds: batchResult.Value.ExcludedTrainPairIds,
                CapabilityIds: batchResult.Value.CapabilityIds,
                Directions: batchResult.Value.Directions,
                TrainerCallPath: EvaluationCallPath,
                SemanticQualityClaimed: false,
                SemanticQualityBoundary: SemanticQualityNotReadyBoundary));
    }

    private static OperationResult<TranslationDpoTokenizationSettings> ValidateSettings(
        TranslationDpoTokenizationSettings settings)
    {
        if (settings.VocabSize < 16)
            return OperationResult<TranslationDpoTokenizationSettings>.Fail(
                "Translation DPO tokenization requires vocab size >= 16.",
                "DPO_TOKENIZER_VOCAB_TOO_SMALL");

        if (settings.ContextLength < 8)
            return OperationResult<TranslationDpoTokenizationSettings>.Fail(
                "Translation DPO tokenization requires context length >= 8.",
                "DPO_TOKENIZER_CONTEXT_TOO_SMALL");

        return OperationResult<TranslationDpoTokenizationSettings>.Ok(settings);
    }

    private static OperationResult<bool> ValidateHandoffForTraining(
        TranslationDpoTrainingHandoff handoff)
    {
        if (handoff is null)
            return OperationResult<bool>.Fail(
                "Translation DPO handoff is required.",
                "HANDOFF_REQUIRED");

        if (!string.Equals(handoff.ExistingDpoPairContract, TranslationDpoTrainingHandoffBuilder.ExistingDpoPairContractName, StringComparison.Ordinal) ||
            !string.Equals(handoff.ExistingTrainerInputContract, TranslationDpoTrainingHandoffBuilder.ExistingTrainerInputContractName, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                "Translation DPO handoff contracts do not match DPO.Models.DpoPair -> DPO.Models.DpoBatch.",
                "HANDOFF_CONTRACT_MISMATCH");

        if (handoff.TrainingPairs is null || handoff.TrainingPairs.Count == 0)
            return OperationResult<bool>.Fail(
                "Translation DPO handoff has no train preference pairs.",
                "HANDOFF_EMPTY");

        if (handoff.HeldOutEvaluationRows is null)
            return OperationResult<bool>.Fail(
                "Translation DPO handoff held-out evaluation rows are required.",
                "HELDOUT_ROWS_REQUIRED");

        if (handoff.SemanticQualityClaimed)
            return OperationResult<bool>.Fail(
                "Translation DPO bridge refuses rows with semantic quality claims before evaluation.",
                "SEMANTIC_QUALITY_CLAIMED");

        var duplicateTrainId = handoff.TrainingPairs
            .GroupBy(pair => pair.PairId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateTrainId is not null)
            return OperationResult<bool>.Fail(
                $"Translation DPO handoff has duplicate train pair id '{duplicateTrainId.Key}'.",
                "DUPLICATE_TRAIN_PAIR_ID");

        var trainIds = handoff.TrainingPairs
            .Select(pair => pair.PairId)
            .ToHashSet(StringComparer.Ordinal);
        var trainInputs = handoff.TrainingPairs
            .Select(pair => NormalizeComparable(pair.Input))
            .Where(input => input.Length > 0)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var heldOut in handoff.HeldOutEvaluationRows)
        {
            var heldOutValidation = ValidateHeldOutRecord(heldOut);
            if (!heldOutValidation.IsSuccess)
                return heldOutValidation;

            if (trainIds.Contains(heldOut.PairId) ||
                trainInputs.Contains(NormalizeComparable(heldOut.Input)))
                return OperationResult<bool>.Fail(
                    $"Translation DPO bridge rejects held-out leakage for row '{heldOut.PairId}'.",
                    "HELDOUT_LEAKAGE");
        }

        var duplicateHeldOutId = handoff.HeldOutEvaluationRows
            .GroupBy(pair => pair.PairId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateHeldOutId is not null)
            return OperationResult<bool>.Fail(
                $"Translation DPO handoff has duplicate held-out pair id '{duplicateHeldOutId.Key}'.",
                "DUPLICATE_HELDOUT_PAIR_ID");

        foreach (var record in handoff.TrainingPairs)
        {
            var rowValidation = ValidateTrainingRecord(record);
            if (!rowValidation.IsSuccess)
                return rowValidation;
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateHandoffForEvaluation(
        TranslationDpoTrainingHandoff handoff)
    {
        var validation = ValidateHandoffForTraining(handoff);
        if (!validation.IsSuccess)
            return validation;

        if (handoff.HeldOutEvaluationRows.Count == 0)
            return OperationResult<bool>.Fail(
                "Translation DPO handoff has no held-out evaluation rows.",
                "HELDOUT_EMPTY");

        return OperationResult<bool>.Ok(true);
    }

    internal static OperationResult<bool> ValidateTrainingRecord(
        TranslationDpoTrainingHandoffRecord record)
    {
        if (record is null)
            return OperationResult<bool>.Fail("Translation DPO training record is required.", "TRAIN_RECORD_REQUIRED");

        var familyValidation = ValidateTrainingRecordFamily(record);
        if (!familyValidation.IsSuccess)
            return familyValidation;

        var required = ValidateCommonFields(
            record.PairId,
            record.LearningUnitId,
            record.CapabilityId,
            record.Direction,
            record.SourceLanguage,
            record.TargetLanguage,
            record.SourceCategory,
            record.License,
            record.TeacherProvenance,
            record.SemanticQualityClaimed,
            record.RepositoryOwnedPassport);
        if (!required.IsSuccess)
            return required;

        if (string.IsNullOrWhiteSpace(record.Input) ||
            string.IsNullOrWhiteSpace(record.TrainerPair?.Prompt) ||
            string.IsNullOrWhiteSpace(record.TrainerPair.Chosen) ||
            string.IsNullOrWhiteSpace(record.TrainerPair.Rejected))
            return OperationResult<bool>.Fail(
                $"Translation DPO training record '{record.PairId}' requires prompt, input, chosen and rejected text.",
                "TRAIN_RECORD_TEXT_REQUIRED");

        var admission = ValidateTrainingPairAdmission(record);
        if (!admission.IsSuccess)
            return admission;

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateTrainingPairAdmission(
        TranslationDpoTrainingHandoffRecord record)
    {
        var admission = record.Admission;
        if (admission is null)
        {
            return OperationResult<bool>.Fail(
                $"Translation DPO training record '{record.PairId}' is missing pair admission.",
                DpoTrainerPairAdmission.MissingErrorCode);
        }

        if (!string.Equals(
                admission.IssuerName,
                nameof(TranslationDpoTrainingHandoffBuilder),
                StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                $"Translation DPO training record '{record.PairId}' has pair admission from an unexpected issuer.",
                "DPO_PAIR_ADMISSION_ISSUER_MISMATCH");
        }

        var authorshipValidation = record.RepositoryOwnedPassport is null
            ? ValidateLegacyTrainingPairAdmission(record, admission)
            : ValidateRepositoryOwnedTrainingPairAdmission(
                record,
                admission,
                record.RepositoryOwnedPassport);
        if (!authorshipValidation.IsSuccess)
            return authorshipValidation;

        var rebound = DpoTrainerPairCustoms.CreateAdmitted(
            record.TrainerPair.Prompt,
            record.TrainerPair.Chosen,
            record.TrainerPair.Rejected,
            admission.ChosenAuthor,
            admission.RejectedAuthor,
            admission.IssuerName);
        if (!string.Equals(
                rebound.Admission.ContentFingerprintSha256,
                admission.ContentFingerprintSha256,
                StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                $"Translation DPO training record '{record.PairId}' pair admission does not match the raw Prompt/Chosen/Rejected text.",
                "DPO_PAIR_ADMISSION_CONTENT_MISMATCH");
        }

        // Эта проверка стоит после юридического паспорта строки и до построения
        // batch: mutation через record with должна падать до tokenization.
        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateTrainingRecordFamily(
        TranslationDpoTrainingHandoffRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.PairFamily))
        {
            return OperationResult<bool>.Fail(
                $"Translation DPO training record '{record.PairId}' requires a pair family.",
                "TRAIN_PAIR_FAMILY_REQUIRED");
        }

        var isRepositoryOwned = string.Equals(
            record.PairFamily,
            RepositoryOwnedTrainingPairFamily,
            StringComparison.Ordinal);

        if (isRepositoryOwned && record.RepositoryOwnedPassport is null)
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned DPO training record '{record.PairId}' is missing the preserved repository passport.",
                "DPO_PAIR_REPOSITORY_PASSPORT_MISSING");
        }

        if (!isRepositoryOwned && record.RepositoryOwnedPassport is not null)
        {
            return OperationResult<bool>.Fail(
                $"Translation legacy DPO training record '{record.PairId}' must not carry a repository-owned passport.",
                "DPO_PAIR_REPOSITORY_PASSPORT_UNEXPECTED");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateLegacyTrainingPairAdmission(
        TranslationDpoTrainingHandoffRecord record,
        DpoTrainerPairAdmission admission)
    {
        if (admission.ChosenAuthor is not DpoPairSideAuthorKind.User ||
            admission.RejectedAuthor is not DpoPairSideAuthorKind.User)
        {
            return OperationResult<bool>.Fail(
                $"Translation legacy DPO training record '{record.PairId}' must carry user authorship on both admitted sides.",
                "DPO_PAIR_ADMISSION_TRANSLATION_AUTHOR_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool>
        ValidateRepositoryOwnedTrainingPairAdmission(
            TranslationDpoTrainingHandoffRecord record,
            DpoTrainerPairAdmission admission,
            TranslationRepositoryOwnedDpoPairPassport passport)
    {
        var passportValidation =
            ValidateRepositoryOwnedTrainingPassport(record, passport);
        if (!passportValidation.IsSuccess)
            return passportValidation;

        if (admission.HasPaidExternalModelSide)
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned DPO training record '{record.PairId}' contains a paid-model side in the admission.",
                DpoTrainerPairAdmission.PaidExternalModelSideErrorCode);
        }

        if (admission.ChosenAuthor != passport.ChosenAuthorship.AuthorKind ||
            admission.RejectedAuthor != passport.RejectedAuthorship.AuthorKind)
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned DPO training record '{record.PairId}' must preserve the stored side authorship in the trainer admission.",
                "DPO_PAIR_ADMISSION_TRANSLATION_AUTHOR_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateRepositoryOwnedTrainingPassport(
        TranslationDpoTrainingHandoffRecord record,
        TranslationRepositoryOwnedDpoPairPassport passport)
    {
        if (passport is null ||
            string.IsNullOrWhiteSpace(passport.Domain) ||
            passport.TeacherProvenance is null ||
            passport.GovernanceDecision is null ||
            passport.ChosenAuthorship is null ||
            passport.RejectedAuthorship is null)
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned DPO training record '{record.PairId}' has an incomplete repository passport.",
                "DPO_PAIR_REPOSITORY_PASSPORT_MISSING");
        }

        var preservesExistingSingleTeacherShape =
            passport.ChosenAuthorship.AuthorKind is
                DpoPairSideAuthorKind.OpenTeacher &&
            passport.RejectedAuthorship.AuthorKind is
                DpoPairSideAuthorKind.LocalModel &&
            Equals(
                passport.ChosenAuthorship.OpenTeacherPassport,
                passport.TeacherProvenance) &&
            passport.RejectedAuthorship.OpenTeacherPassport is null;
        var preservesCleanMissQwenShape =
            passport.ChosenAuthorship.AuthorKind is
                DpoPairSideAuthorKind.OpenTeacher &&
            passport.RejectedAuthorship.AuthorKind is
                DpoPairSideAuthorKind.OpenTeacher &&
            Equals(
                passport.ChosenAuthorship.OpenTeacherPassport,
                passport.TeacherProvenance) &&
            Equals(
                passport.RejectedAuthorship.OpenTeacherPassport,
                passport.TeacherProvenance);
        if (!preservesExistingSingleTeacherShape &&
            !preservesCleanMissQwenShape)
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned DPO training record '{record.PairId}' must preserve either the existing one-teacher shape or two sides bound to the same admitted local Qwen passport.",
                "DPO_PAIR_REPOSITORY_PASSPORT_AUTHORSHIP_MISMATCH");
        }

        var commonPair = new ContinuousLearning.DpoPair(
            record.TrainerPair.Prompt,
            record.TrainerPair.Chosen,
            record.TrainerPair.Rejected,
            passport.Domain,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport.TeacherProvenance,
            GovernanceDecision = passport.GovernanceDecision,
            ChosenAuthorship = passport.ChosenAuthorship,
            RejectedAuthorship = passport.RejectedAuthorship
        };

        var governance = ContinuousLearning.TrainingDataGovernanceAdmission
            .Validate([commonPair]);
        if (!governance.IsSuccess)
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned DPO training record '{record.PairId}' has invalid governance evidence: {governance.ErrorMessage}",
                governance.ErrorCode ?? "DPO_PAIR_REPOSITORY_GOVERNANCE_INVALID");
        }

        // Используем общий authority повторно: он пересчитывает digest ровно
        // по Prompt/Chosen/Rejected и проверяет, что OpenTeacher-паспорт связан
        // с тем же содержимым, а не заново придуман переводческим мостом.
        var rebound = ContinuousLearning.DpoPairTrainingContentAuthority
            .Bind(commonPair);
        if (!rebound.IsSuccess || rebound.Value is null ||
            !Equals(rebound.Value.TeacherProvenance, passport.TeacherProvenance) ||
            !Equals(rebound.Value.GovernanceDecision, passport.GovernanceDecision) ||
            !Equals(rebound.Value.ChosenAuthorship, passport.ChosenAuthorship) ||
            !Equals(rebound.Value.RejectedAuthorship, passport.RejectedAuthorship))
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned DPO training record '{record.PairId}' has a repository passport that no longer matches the trainer text.",
                rebound.ErrorCode ?? "DPO_PAIR_REPOSITORY_PASSPORT_CONTENT_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    internal static OperationResult<bool> ValidateHeldOutRecord(
        TranslationDpoHeldOutEvaluationRecord record)
    {
        if (record is null)
            return OperationResult<bool>.Fail("Translation DPO held-out record is required.", "HELDOUT_RECORD_REQUIRED");

        var required = ValidateCommonFields(
            record.PairId,
            record.LearningUnitId,
            record.CapabilityId,
            record.Direction,
            record.SourceLanguage,
            record.TargetLanguage,
            record.SourceCategory,
            record.License,
            record.TeacherProvenance,
            record.SemanticQualityClaimed);
        if (!required.IsSuccess)
            return required;

        if (string.IsNullOrWhiteSpace(record.Input) ||
            string.IsNullOrWhiteSpace(record.ExpectedResponse) ||
            string.IsNullOrWhiteSpace(record.RejectedResponse))
            return OperationResult<bool>.Fail(
                $"Translation DPO held-out record '{record.PairId}' requires input, expected response and rejected response.",
                "HELDOUT_RECORD_TEXT_REQUIRED");

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateCommonFields(
        string pairId,
        string learningUnitId,
        string capabilityId,
        string direction,
        string sourceLanguage,
        string targetLanguage,
        string sourceCategory,
        string license,
        TranslationSeedTeacherProvenance provenance,
        bool semanticQualityClaimed,
        TranslationRepositoryOwnedDpoPairPassport? repositoryOwnedPassport = null)
    {
        if (string.IsNullOrWhiteSpace(pairId) ||
            string.IsNullOrWhiteSpace(learningUnitId) ||
            string.IsNullOrWhiteSpace(capabilityId) ||
            string.IsNullOrWhiteSpace(direction) ||
            string.IsNullOrWhiteSpace(sourceLanguage) ||
            string.IsNullOrWhiteSpace(targetLanguage))
            return OperationResult<bool>.Fail(
                "Translation DPO row requires identity, direction, language and capability metadata.",
                "ROW_METADATA_REQUIRED");

        if (!Enum.TryParse<TranslationDirection>(direction, ignoreCase: false, out _))
            return OperationResult<bool>.Fail(
                $"Translation DPO row has unsupported direction '{direction}'.",
                "ROW_DIRECTION_UNSUPPORTED");

        if (semanticQualityClaimed)
            return OperationResult<bool>.Fail(
                $"Translation DPO row '{pairId}' must keep semantic quality unclaimed before held-out evaluation.",
                "SEMANTIC_QUALITY_CLAIMED");

        if (!string.Equals(sourceCategory, SelfAuthoredCategory, StringComparison.Ordinal) ||
            !string.Equals(license, InternalLicense, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Translation DPO row '{pairId}' has illegal source category or license.",
                "ILLEGAL_SOURCE_OR_LICENSE");

        var provenanceValidation = repositoryOwnedPassport is null
            ? ValidateLegacyTranslationProvenance(pairId, provenance)
            : ValidateRepositoryOwnedTranslationProvenance(
                pairId,
                provenance,
                repositoryOwnedPassport);
        if (!provenanceValidation.IsSuccess)
            return provenanceValidation;

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateLegacyTranslationProvenance(
        string pairId,
        TranslationSeedTeacherProvenance provenance)
    {
        if (provenance is null ||
            string.IsNullOrWhiteSpace(provenance.Author) ||
            !string.Equals(
                provenance.SourceType,
                SelfAuthoredCategory,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelProvider,
                NoTeacherValue,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelName,
                NoTeacherValue,
                StringComparison.Ordinal) ||
            provenance.ExternalApiUsed ||
            provenance.ClosedProprietaryTeacherUsed)
        {
            return OperationResult<bool>.Fail(
                $"Translation DPO row '{pairId}' must be self-authored with a named author and must not use external or closed teacher provenance.",
                "ILLEGAL_TEACHER_PROVENANCE");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool>
        ValidateRepositoryOwnedTranslationProvenance(
            string pairId,
            TranslationSeedTeacherProvenance provenance,
            TranslationRepositoryOwnedDpoPairPassport passport)
    {
        if (passport.TeacherProvenance is null)
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned DPO row '{pairId}' is missing teacher provenance in its repository passport.",
                "DPO_PAIR_REPOSITORY_PASSPORT_MISSING");
        }

        var teacher = passport.TeacherProvenance;
        var expectedProvider = teacher.HostingProvider?.ToString() ??
                               NoTeacherValue;
        var expectedModel = string.IsNullOrWhiteSpace(teacher.ModelId)
            ? NoTeacherValue
            : teacher.ModelId;
        var expectedExternalApiUsed = teacher.AccessMode is not
            ContinuousLearning.TrainingTeacherAccessMode.LocalHostedOpenWeight;

        if (provenance is null ||
            string.IsNullOrWhiteSpace(provenance.Author) ||
            !string.Equals(
                provenance.SourceType,
                teacher.SourceKind.ToString(),
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.Author,
                $"repository-owned-open-teacher:{teacher.RequestId}",
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelProvider,
                expectedProvider,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelName,
                expectedModel,
                StringComparison.Ordinal) ||
            provenance.ExternalApiUsed != expectedExternalApiUsed ||
            provenance.ClosedProprietaryTeacherUsed !=
                teacher.ClosedProprietaryModel ||
            string.IsNullOrWhiteSpace(provenance.ProvenanceNote) ||
            !provenance.ProvenanceNote.Contains(pairId, StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned DPO row '{pairId}' must expose the same open-teacher provenance that is stored in its repository passport.",
                "DPO_PAIR_REPOSITORY_PROVENANCE_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static string NormalizeComparable(string value)
        => Regex.Replace((value ?? string.Empty).Normalize(NormalizationForm.FormC), @"\s+", " ").Trim();
}
