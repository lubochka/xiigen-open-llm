namespace TranslationDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DPO.Models;

public sealed record TranslationDpoTrainingHandoff(
    string HandoffId,
    string DomainTag,
    string MachineId,
    string SourceBatchId,
    string ExistingDpoPairContract,
    string ExistingTrainerInputContract,
    bool ProducesTensorDpoBatches,
    string NextRealTrainerConnectionGap,
    IReadOnlyList<TranslationDpoTrainingHandoffRecord> TrainingPairs,
    IReadOnlyList<TranslationDpoHeldOutEvaluationRecord> HeldOutEvaluationRows)
{
    public bool SemanticQualityClaimed =>
        TrainingPairs.Any(pair => pair.SemanticQualityClaimed) ||
        HeldOutEvaluationRows.Any(row => row.SemanticQualityClaimed);
}

public sealed record TranslationDpoTrainingHandoffRecord(
    string PairId,
    string LearningUnitId,
    string CapabilityId,
    string Direction,
    string SourceLanguage,
    string TargetLanguage,
    string Prompt,
    string Input,
    string ChosenResponse,
    string RejectedResponse,
    string RejectionReason,
    string RejectionMarker,
    string PairFamily,
    string SourceCategory,
    string License,
    TranslationSeedTeacherProvenance TeacherProvenance,
    bool SemanticQualityClaimed,
    DpoPair TrainerPair)
{
    public DpoTrainerPairAdmission? Admission { get; init; }

    // Для старого seed-пути паспорт пустой: эти строки остаются человеческими
    // User/User. Для repository-owned пути здесь лежит тот же юридический
    // паспорт, который был сохранен рядом с DPO-парой в репозитории.
    public TranslationRepositoryOwnedDpoPairPassport? RepositoryOwnedPassport
    {
        get;
        init;
    }
}

/// <summary>
/// Репозиторный паспорт переводческой DPO-пары. Он нужен мосту к тренеру:
/// бедный DPO.Models.DpoPair несет только три строки, а авторство сторон,
/// открытый учитель и решение допуска должны доехать до последней проверки.
/// </summary>
public sealed record TranslationRepositoryOwnedDpoPairPassport(
    string Domain,
    ContinuousLearning.TrainingTeacherProvenance TeacherProvenance,
    ContinuousLearning.TrainingDataGovernanceDecision GovernanceDecision,
    ContinuousLearning.DpoPairSideAuthorship ChosenAuthorship,
    ContinuousLearning.DpoPairSideAuthorship RejectedAuthorship);

public sealed record TranslationDpoHeldOutEvaluationRecord(
    string PairId,
    string LearningUnitId,
    string CapabilityId,
    string Direction,
    string SourceLanguage,
    string TargetLanguage,
    string Prompt,
    string Input,
    string ExpectedResponse,
    string RejectedResponse,
    string RejectionReason,
    string RejectionMarker,
    string SourceCategory,
    string License,
    TranslationSeedTeacherProvenance TeacherProvenance,
    bool SemanticQualityClaimed);

public static partial class TranslationDpoTrainingHandoffBuilder
{
    public const string ExistingDpoPairContractName = "DPO.Models.DpoPair";
    public const string ExistingTrainerInputContractName = "DPO.Models.DpoBatch";
    public const string RealTrainerConnectionGap =
        "Text preference pairs are mapped to DPO.Models.DpoPair here; TranslationDpoTrainerBridge now tokenizes them into DPO.Models.DpoBatch and calls DPO.DpoTrainer.StepMany without adding held-out rows.";

    private const string SelfAuthoredCategory = "synthetic_self_authored";
    private const string InternalLicense = "project_internal_self_authored";
    private const string NoTeacherValue = "none";

    public static TranslationDpoTrainingHandoff BuildSeedCorpusTrainHandoff(
        string handoffId,
        TranslationSeedCorpus corpus,
        TranslationLearningBatch trainBatch,
        IReadOnlyList<TranslationTrainReadyPair>? validatedTrainingDataArtifactPairs = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(handoffId);
        ArgumentNullException.ThrowIfNull(corpus);
        ArgumentNullException.ThrowIfNull(trainBatch);

        if (corpus.TrainRows.Count == 0)
            throw new InvalidOperationException("Translation DPO handoff requires at least one train row.");

        if (trainBatch.Examples.Any(example => example.HeldOut))
            throw new InvalidOperationException("Translation DPO handoff rejects held-out examples before trainer handoff creation.");

        EnsureTrainBatchMatchesSeedTrainRows(corpus, trainBatch);
        var artifactPairsById = BuildArtifactPairIndex(corpus, validatedTrainingDataArtifactPairs);

        var trainingPairs = corpus.TrainRows
            .Select(row => BuildTrainingRecord(
                row,
                artifactPairsById is null ? null : artifactPairsById[row.SeedId]))
            .ToArray();
        var heldOutRows = corpus.HeldOutRows
            .Select(BuildHeldOutRecord)
            .ToArray();

        return new TranslationDpoTrainingHandoff(
            HandoffId: handoffId.Trim(),
            DomainTag: TranslationMachine.DomainTag,
            MachineId: TranslationMachine.MachineId,
            SourceBatchId: trainBatch.BatchId,
            ExistingDpoPairContract: ExistingDpoPairContractName,
            ExistingTrainerInputContract: ExistingTrainerInputContractName,
            ProducesTensorDpoBatches: false,
            NextRealTrainerConnectionGap: RealTrainerConnectionGap,
            TrainingPairs: trainingPairs,
            HeldOutEvaluationRows: heldOutRows);
    }

    public static TranslationDpoTrainingHandoff BuildRepositoryOwnedDpoTrainHandoff(
        string handoffId,
        string sourceBatchId,
        IReadOnlyList<ContinuousLearning.DpoPair> repositoryOwnedTrainingPairs,
        IReadOnlyList<TranslationSeedCorpusRow> heldOutRows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(handoffId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceBatchId);
        ArgumentNullException.ThrowIfNull(repositoryOwnedTrainingPairs);
        ArgumentNullException.ThrowIfNull(heldOutRows);

        if (repositoryOwnedTrainingPairs.Count == 0)
            throw new InvalidOperationException(
                "Translation DPO repository-owned handoff requires at least one sealed training pair.");

        if (heldOutRows.Count == 0)
            throw new InvalidOperationException(
                "Translation DPO repository-owned handoff requires an explicit held-out seed source because common DpoPair rows do not carry held-out expected labels.");

        var trainingPairs = repositoryOwnedTrainingPairs
            .Select(BuildRepositoryOwnedTrainingRecord)
            .ToArray();
        var heldOutEvaluationRows = heldOutRows
            .Select(BuildHeldOutRecord)
            .ToArray();

        return new TranslationDpoTrainingHandoff(
            HandoffId: handoffId.Trim(),
            DomainTag: TranslationMachine.DomainTag,
            MachineId: TranslationMachine.MachineId,
            SourceBatchId: sourceBatchId.Trim(),
            ExistingDpoPairContract: ExistingDpoPairContractName,
            ExistingTrainerInputContract: ExistingTrainerInputContractName,
            ProducesTensorDpoBatches: false,
            NextRealTrainerConnectionGap: RealTrainerConnectionGap,
            TrainingPairs: trainingPairs,
            HeldOutEvaluationRows: heldOutEvaluationRows);
    }

    private static IReadOnlyDictionary<string, TranslationTrainReadyPair>? BuildArtifactPairIndex(
        TranslationSeedCorpus corpus,
        IReadOnlyList<TranslationTrainReadyPair>? validatedTrainingDataArtifactPairs)
    {
        if (validatedTrainingDataArtifactPairs is null)
            return null;

        var byId = new Dictionary<string, TranslationTrainReadyPair>(StringComparer.Ordinal);
        foreach (var pair in validatedTrainingDataArtifactPairs)
        {
            if (!byId.TryAdd(pair.Id, pair))
                throw new InvalidOperationException(
                    $"Translation DPO handoff received duplicate artifact train pair id '{pair.Id}'.");
        }

        var trainIds = corpus.TrainRows
            .Select(row => row.SeedId)
            .ToHashSet(StringComparer.Ordinal);
        var missingTrainIds = trainIds
            .Where(id => !byId.ContainsKey(id))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var extraArtifactIds = byId.Keys
            .Where(id => !trainIds.Contains(id))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (missingTrainIds.Length > 0 || extraArtifactIds.Length > 0)
            throw new InvalidOperationException(
                "Translation DPO handoff requires artifact train pair ids to match seed train rows exactly.");

        return byId;
    }

    private static void EnsureTrainBatchMatchesSeedTrainRows(
        TranslationSeedCorpus corpus,
        TranslationLearningBatch trainBatch)
    {
        var trainIds = corpus.TrainRows
            .Select(row => row.SeedId)
            .ToHashSet(StringComparer.Ordinal);
        var heldOutIds = corpus.HeldOutRows
            .Select(row => row.SeedId)
            .ToHashSet(StringComparer.Ordinal);
        var batchSourceIds = trainBatch.SourceIds.ToHashSet(StringComparer.Ordinal);

        var leakedHeldOutIds = batchSourceIds
            .Where(heldOutIds.Contains)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (leakedHeldOutIds.Length > 0)
            throw new InvalidOperationException(
                $"Translation DPO handoff rejects held-out source ids in the train batch: {string.Join(", ", leakedHeldOutIds)}.");

        var missingTrainIds = trainIds
            .Where(id => !batchSourceIds.Contains(id))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var extraBatchIds = batchSourceIds
            .Where(id => !trainIds.Contains(id))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (missingTrainIds.Length > 0 || extraBatchIds.Length > 0)
            throw new InvalidOperationException(
                "Translation DPO handoff requires train batch SourceIds to match seed train rows exactly.");
    }

    private static TranslationDpoTrainingHandoffRecord BuildTrainingRecord(
        TranslationSeedCorpusRow row,
        TranslationTrainReadyPair? validatedTrainingDataArtifactPair = null)
    {
        var rowAuthor = ValidateLegalHandoffRow(row, TranslationSeedCorpusSplit.Train);

        var dpoPair = TranslationSeedCorpusLoader.ToDpoPair(row);
        var trainReady = validatedTrainingDataArtifactPair ?? TranslationSeedCorpusLoader.ToTrainReadyPair(row);
        var trainerPrompt = BuildTrainerPrompt(trainReady);
        var admitted = DpoTrainerPairCustoms.CreateAdmitted(
            trainerPrompt,
            trainReady.Chosen,
            trainReady.Rejected,
            rowAuthor,
            rowAuthor,
            nameof(TranslationDpoTrainingHandoffBuilder));

        return new TranslationDpoTrainingHandoffRecord(
            PairId: row.SeedId,
            LearningUnitId: LearningUnitIdFor(row),
            CapabilityId: CapabilityIdFor(row),
            Direction: row.Direction,
            SourceLanguage: row.SourceLanguage,
            TargetLanguage: row.TargetLanguage,
            Prompt: trainReady.Prompt,
            Input: row.InputText,
            ChosenResponse: validatedTrainingDataArtifactPair?.Chosen ?? row.ChosenTranslation,
            RejectedResponse: validatedTrainingDataArtifactPair?.Rejected ?? row.RejectedTranslation,
            RejectionReason: dpoPair.RejectedReason,
            RejectionMarker: dpoPair.WrongMarker,
            PairFamily: trainReady.PairFamily,
            SourceCategory: row.SourceCategory,
            License: row.License,
            TeacherProvenance: row.TeacherProvenance,
            SemanticQualityClaimed: false,
            TrainerPair: admitted.Pair)
        {
            Admission = admitted.Admission
        };
    }

    private static TranslationDpoTrainingHandoffRecord BuildRepositoryOwnedTrainingRecord(
        ContinuousLearning.DpoPair pair)
    {
        ArgumentNullException.ThrowIfNull(pair);
        var prompt = ParseRepositoryOwnedTrainingPrompt(pair.Prompt);
        var direction = TranslationSeedCorpusLoader.ToDirectionCode(
            prompt.SourceLanguage,
            prompt.TargetLanguage);
        if (!Enum.TryParse<TranslationDirection>(
                direction,
                ignoreCase: false,
                out var parsedDirection))
        {
            throw new InvalidOperationException(
                $"Repository-owned translation DPO pair has unsupported direction '{direction}'.");
        }

        var pairId = ComputeRepositoryOwnedPairId(pair);
        var passport = BuildRepositoryOwnedPassport(pair);
        var admitted = DpoTrainerPairCustoms.CreateAdmitted(
            pair.Prompt,
            pair.Chosen,
            pair.Rejected,
            passport.ChosenAuthorship.AuthorKind,
            passport.RejectedAuthorship.AuthorKind,
            nameof(TranslationDpoTrainingHandoffBuilder));

        return new TranslationDpoTrainingHandoffRecord(
            PairId: pairId,
            LearningUnitId: TranslationUnitIds.ExecutorFor(parsedDirection),
            CapabilityId: $"{TranslationMachine.MachineId}:{TranslationRoutingFlow.FlowId}:{direction}",
            Direction: direction,
            SourceLanguage: prompt.SourceLanguage,
            TargetLanguage: prompt.TargetLanguage,
            Prompt: prompt.Instruction,
            Input: prompt.Input,
            ChosenResponse: pair.Chosen,
            RejectedResponse: pair.Rejected,
            RejectionReason: ExtractRepositoryOwnedRejectionReason(pair.Rejected),
            RejectionMarker: ExtractRepositoryOwnedRejectionMarker(pair.Rejected),
            PairFamily: "repository-owned-dpo-training-pair-v1",
            SourceCategory: SelfAuthoredCategory,
            License: InternalLicense,
            TeacherProvenance: ToRepositoryOwnedTranslationProvenance(
                pairId,
                passport.TeacherProvenance),
            SemanticQualityClaimed: false,
            TrainerPair: admitted.Pair)
        {
            Admission = admitted.Admission,
            RepositoryOwnedPassport = passport
        };
    }

    private static TranslationRepositoryOwnedDpoPairPassport
        BuildRepositoryOwnedPassport(ContinuousLearning.DpoPair pair)
    {
        if (string.IsNullOrWhiteSpace(pair.Domain) ||
            pair.TeacherProvenance is null ||
            pair.GovernanceDecision is null ||
            pair.ChosenAuthorship is null ||
            pair.RejectedAuthorship is null)
        {
            throw new InvalidOperationException(
                "Repository-owned translation DPO pair requires domain, teacher provenance, governance and per-side authorship.");
        }

        var governance = ContinuousLearning.TrainingDataGovernanceAdmission
            .Validate([pair]);
        if (!governance.IsSuccess)
        {
            throw new InvalidOperationException(
                "Repository-owned translation DPO pair governance is not admissible.");
        }

        var rebound = ContinuousLearning.DpoPairTrainingContentAuthority
            .Bind(pair);
        if (!rebound.IsSuccess || rebound.Value is null ||
            !Equals(rebound.Value.TeacherProvenance, pair.TeacherProvenance) ||
            !Equals(rebound.Value.ChosenAuthorship, pair.ChosenAuthorship) ||
            !Equals(rebound.Value.RejectedAuthorship, pair.RejectedAuthorship))
        {
            throw new InvalidOperationException(
                "Repository-owned translation DPO pair passport does not match the stored training content.");
        }

        return new TranslationRepositoryOwnedDpoPairPassport(
            pair.Domain,
            pair.TeacherProvenance,
            pair.GovernanceDecision,
            pair.ChosenAuthorship,
            pair.RejectedAuthorship);
    }

    private static TranslationSeedTeacherProvenance
        ToRepositoryOwnedTranslationProvenance(
            string pairId,
            ContinuousLearning.TrainingTeacherProvenance provenance)
    {
        var provider = provenance.HostingProvider?.ToString() ?? NoTeacherValue;
        var model = string.IsNullOrWhiteSpace(provenance.ModelId)
            ? NoTeacherValue
            : provenance.ModelId;

        // Старый TranslationSeedTeacherProvenance остается читабельной витриной.
        // Истина не выдумывается: provider/model/API/closed flags берутся из
        // общего TeacherProvenance, который уже связан с сохраненной DPO-парой.
        return new TranslationSeedTeacherProvenance(
            SourceType: provenance.SourceKind.ToString(),
            Author: $"repository-owned-open-teacher:{provenance.RequestId}",
            ModelProvider: provider,
            ModelName: model,
            ExternalApiUsed: provenance.AccessMode is not
                ContinuousLearning.TrainingTeacherAccessMode
                    .LocalHostedOpenWeight,
            ClosedProprietaryTeacherUsed: provenance.ClosedProprietaryModel,
            ProvenanceNote:
                $"Training text reopened from sealed repository-owned DPO pair {pairId}; open teacher {model} supplied the preferred side and held-out labels stay from the explicit seed source.");
    }

    private static TranslationDpoHeldOutEvaluationRecord BuildHeldOutRecord(TranslationSeedCorpusRow row)
    {
        ValidateLegalHandoffRow(row, TranslationSeedCorpusSplit.HeldOut);

        var dpoPair = TranslationSeedCorpusLoader.ToDpoPair(row);
        return new TranslationDpoHeldOutEvaluationRecord(
            PairId: row.SeedId,
            LearningUnitId: LearningUnitIdFor(row),
            CapabilityId: CapabilityIdFor(row),
            Direction: row.Direction,
            SourceLanguage: row.SourceLanguage,
            TargetLanguage: row.TargetLanguage,
            Prompt: $"Translate from {row.SourceLanguage} to {row.TargetLanguage}.",
            Input: row.InputText,
            ExpectedResponse: row.ExpectedLabel ?? row.ChosenTranslation,
            RejectedResponse: row.RejectedTranslation,
            RejectionReason: dpoPair.RejectedReason,
            RejectionMarker: dpoPair.WrongMarker,
            SourceCategory: row.SourceCategory,
            License: row.License,
            TeacherProvenance: row.TeacherProvenance,
            SemanticQualityClaimed: false);
    }

    private static string BuildTrainerPrompt(TranslationTrainReadyPair pair)
        => $"{pair.Prompt}\nInput: {pair.ASide}";

    private static RepositoryOwnedTrainingPrompt ParseRepositoryOwnedTrainingPrompt(
        string trainerPrompt)
    {
        if (string.IsNullOrWhiteSpace(trainerPrompt))
            throw new InvalidOperationException(
                "Repository-owned translation DPO pair requires a trainer prompt.");

        var normalized = trainerPrompt.Replace("\r\n", "\n");
        var split = normalized.Split(["\nInput: "], 2, StringSplitOptions.None);
        if (split.Length != 2 ||
            string.IsNullOrWhiteSpace(split[0]) ||
            string.IsNullOrWhiteSpace(split[1]))
        {
            throw new InvalidOperationException(
                "Repository-owned translation DPO pair prompt must be '<instruction>\\nInput: <source text>'.");
        }

        var match = TranslationPromptRegex().Match(split[0].Trim());
        if (!match.Success)
            throw new InvalidOperationException(
                "Repository-owned translation DPO pair prompt must name source and target languages.");

        return new RepositoryOwnedTrainingPrompt(
            split[0].Trim(),
            match.Groups["source"].Value.Trim().ToLowerInvariant(),
            match.Groups["target"].Value.Trim().ToLowerInvariant(),
            split[1].Trim());
    }

    private static string ComputeRepositoryOwnedPairId(
        ContinuousLearning.DpoPair pair)
        => ComputeSha256(string.Join(
            "\n",
            "translation-repository-owned-dpo-pair-v1",
            pair.Domain,
            pair.Prompt,
            pair.Chosen,
            pair.Rejected));

    private static string ExtractRepositoryOwnedRejectionMarker(string rejected)
    {
        var match = WrongDiagnosticRegex().Match(rejected ?? string.Empty);
        return match.Success
            ? match.Groups["marker"].Value.Trim()
            : "repository_owned_dpo_rejected";
    }

    private static string ExtractRepositoryOwnedRejectionReason(string rejected)
    {
        var match = WrongDiagnosticRegex().Match(rejected ?? string.Empty);
        return match.Success
            ? match.Groups["reason"].Value.Trim()
            : "repository-owned DPO rejected response";
    }

    private static string ComputeSha256(string value)
        => Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static DpoPairSideAuthorKind ValidateLegalHandoffRow(
        TranslationSeedCorpusRow row,
        TranslationSeedCorpusSplit expectedSplit)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (row.Split != expectedSplit)
            throw new InvalidOperationException(
                $"Translation DPO handoff expected {expectedSplit} row but got {row.Split} for '{row.SeedId}'.");

        Require(row.SeedId, nameof(row.SeedId));
        Require(row.SourceLanguage, nameof(row.SourceLanguage));
        Require(row.TargetLanguage, nameof(row.TargetLanguage));
        Require(row.InputText, nameof(row.InputText));
        Require(row.ChosenTranslation, nameof(row.ChosenTranslation));
        Require(row.RejectedTranslation, nameof(row.RejectedTranslation));

        if (string.IsNullOrWhiteSpace(row.RejectedReason) && string.IsNullOrWhiteSpace(row.WrongMarker))
            throw new InvalidOperationException(
                $"Translation DPO handoff row '{row.SeedId}' requires a rejection reason or wrong marker.");

        if (!string.Equals(row.SourceCategory, SelfAuthoredCategory, StringComparison.Ordinal) ||
            !string.Equals(row.License, InternalLicense, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Translation DPO handoff row '{row.SeedId}' has illegal source category or license.");

        var provenance = row.TeacherProvenance;
        if (provenance is null ||
            string.IsNullOrWhiteSpace(provenance.Author) ||
            !string.Equals(provenance.SourceType, SelfAuthoredCategory, StringComparison.Ordinal) ||
            !string.Equals(provenance.ModelProvider, NoTeacherValue, StringComparison.Ordinal) ||
            !string.Equals(provenance.ModelName, NoTeacherValue, StringComparison.Ordinal) ||
            provenance.ExternalApiUsed ||
            provenance.ClosedProprietaryTeacherUsed)
            throw new InvalidOperationException(
                $"Translation DPO handoff row '{row.SeedId}' must be self-authored with a named author and must not use an external API or closed proprietary teacher.");

        if (row.SemanticQualityClaimed)
            throw new InvalidOperationException(
                $"Translation DPO handoff row '{row.SeedId}' must keep semantic_quality_claimed=false.");

        if (expectedSplit == TranslationSeedCorpusSplit.HeldOut)
            Require(row.ExpectedLabel, nameof(row.ExpectedLabel));

        // Автор User появляется только после полного паспорта строки; неизвестный
        // или внешний источник выше падает, а не получает удобный default.
        return DpoPairSideAuthorKind.User;
    }

    private static string LearningUnitIdFor(TranslationSeedCorpusRow row)
    {
        if (!Enum.TryParse<TranslationDirection>(row.Direction, ignoreCase: false, out var direction))
            throw new InvalidOperationException(
                $"Translation DPO handoff row '{row.SeedId}' has unsupported direction '{row.Direction}'.");

        return TranslationUnitIds.ExecutorFor(direction);
    }

    private static string CapabilityIdFor(TranslationSeedCorpusRow row)
        => $"{TranslationMachine.MachineId}:{TranslationRoutingFlow.FlowId}:{row.Direction}";

    private static void Require(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Translation DPO handoff requires non-empty {fieldName}.");
    }

    private sealed record RepositoryOwnedTrainingPrompt(
        string Instruction,
        string SourceLanguage,
        string TargetLanguage,
        string Input);

    [GeneratedRegex(
        @"^Translate from (?<source>[A-Za-z]{2,12}) to (?<target>[A-Za-z]{2,12})\.$",
        RegexOptions.CultureInvariant)]
    private static partial Regex TranslationPromptRegex();

    [GeneratedRegex(
        @"\[WRONG:\s*(?<marker>[a-zA-Z0-9_\-]+)\s*-\s*(?<reason>[^\]]+)\]",
        RegexOptions.CultureInvariant)]
    private static partial Regex WrongDiagnosticRegex();
}
