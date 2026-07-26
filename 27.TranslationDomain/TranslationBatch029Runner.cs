// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record TranslationBatch029RunReport(
    string BatchId,
    string SourcePath,
    int SourceRows,
    int CorpusTrainingRows,
    int FilteredBatchTrainRows,
    int TrainRows,
    int HeldOutRows,
    int UnitExamplesTrained,
    int UnitExamplesHeldOut,
    double UnitAccuracyBefore,
    double UnitAccuracyAfter,
    double RuntimeExactAccuracy,
    double RuntimeProductAccuracy,
    double RuntimeBootstrapRate,
    double RewardMargin,
    long CpuMs,
    long PeakRamMb,
    long ElapsedMs,
    string CheckpointRef,
    string ManifestRef,
    bool PassesGate,
    string GateNote,
    IReadOnlyList<TranslationHeldOutRowResult> HeldOutResults);

public sealed record TranslationHeldOutRowResult(
    string PairId,
    string SourceText,
    string ExpectedTranslation,
    string ActualTranslation,
    bool ExactMatch,
    string ProductMarker,
    bool ProductMatch,
    bool BootstrapFallback);

public sealed record TranslationContaminationReport(
    string HeldOutId,
    string TrainId,
    double Similarity);

public static class TranslationBatch029Runner
{
    public static async Task<OperationResult<TranslationBatch029RunReport>> RunAsync(
        string sourcePath,
        int trainRows = 40,
        int heldOutRows = 10,
        string? additionalTrainingRoot = null,
        IReadOnlyList<TranslationTrainReadyPair>? supplementalTrainingPairs = null,
        string? supplementalTrainingPath = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<TranslationBatch029RunReport>.Fail("Batch-029 runner cancelled.", "CANCELLED");

        if (string.IsNullOrWhiteSpace(sourcePath))
            return OperationResult<TranslationBatch029RunReport>.Fail("Source path is required.", "SOURCE_PATH_REQUIRED");

        if (!File.Exists(sourcePath))
            return OperationResult<TranslationBatch029RunReport>.Fail($"Source file not found: {sourcePath}", "SOURCE_NOT_FOUND");

        var startMs = Environment.TickCount64;
        var jsonl = await File.ReadAllTextAsync(sourcePath, Encoding.UTF8, ct);
        var rows = TranslationStructuredBatchBuilder.ParseJsonl(jsonl);
        if (rows.Count < trainRows + heldOutRows)
            return OperationResult<TranslationBatch029RunReport>.Fail(
                $"Source file has {rows.Count} rows, but {trainRows + heldOutRows} are required.",
                "INSUFFICIENT_ROWS");

        var trainPairs = rows.Take(trainRows).ToArray();
        var heldOutPairs = rows.Skip(trainRows).Take(heldOutRows).ToArray();
        var splitContaminationError = BuildTrainHeldOutOverlapError(trainPairs, heldOutPairs);
        if (splitContaminationError is not null)
            return OperationResult<TranslationBatch029RunReport>.Fail(splitContaminationError, "TRAIN_HELDOUT_OVERLAP");

        var corpusPairs = await LoadAdditionalTrainingPairsAsync(additionalTrainingRoot, sourcePath, ct);
        var supplementalFilePairs = await LoadSupplementalTrainingPairsAsync(supplementalTrainingPath, ct);
        var supplementalPairs = (supplementalTrainingPairs ?? [])
            .Concat(supplementalFilePairs)
            .Where(IsUsableRuEnPair)
            .ToArray();

        // Filter new batch train pairs against corpus training history to remove near-duplicates.
        // Pairs with >= 70% token overlap to any already-trained a_side are skipped because
        // they provide no new learning signal and inflate memorisation accuracy.
        var historyASides = corpusPairs.Select(p => p.ASide).ToList();
        var filteredBatchTrainPairs = historyASides.Count > 0
            ? TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(trainPairs, historyASides)
            : (IReadOnlyList<TranslationTrainReadyPair>)trainPairs;

        var effectiveBatchTrainPairs = filteredBatchTrainPairs;
        if (corpusPairs.Count == 0 && supplementalPairs.Length == 0 && effectiveBatchTrainPairs.Count == 0)
            return OperationResult<TranslationBatch029RunReport>.Fail(
                "All candidate training rows overlap more than 70% with the training history. Provide a fresher batch before training.",
                "NO_UNIQUE_TRAIN_ROWS");

        var allTrainPairs = MergeTrainingPairs(
            corpusPairs.Concat(supplementalPairs).ToArray(),
            effectiveBatchTrainPairs);
        var allTrainContaminationError = BuildTrainHeldOutOverlapError(allTrainPairs, heldOutPairs);
        if (allTrainContaminationError is not null)
            return OperationResult<TranslationBatch029RunReport>.Fail(allTrainContaminationError, "TRAIN_HELDOUT_OVERLAP");

        var similarityContamination = FindSimilarityContamination(allTrainPairs, heldOutPairs, 0.70);
        if (similarityContamination.Count > 0)
            return OperationResult<TranslationBatch029RunReport>.Fail(
                BuildSimilarityContaminationError(similarityContamination),
                "TRAIN_HELDOUT_SIMILARITY_OVERLAP");

        // Build the split train batch from the uniqueness-filtered pairs.
        // Null guard: if all pairs were filtered and corpus is the only source, skip the split batch.
        var splitTrainBatch = effectiveBatchTrainPairs.Count > 0
            ? TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
                "ru-en-general-l1-b029-heldout-train-structural",
                effectiveBatchTrainPairs,
                heldOut: false,
                signatureMode: TranslationSignatureMode.Structural)
            : null;

        var corpusTrainBatch = corpusPairs.Count == 0
            ? null
            : TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
                "ru-en-general-l1-corpus-train-exact",
                corpusPairs,
                heldOut: false,
                signatureMode: TranslationSignatureMode.ExactPair);

        var supplementalTrainBatch = supplementalPairs.Length == 0
            ? null
            : TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
                "ru-en-general-l1-supplemental-repair-structural",
                supplementalPairs,
                heldOut: false,
                signatureMode: TranslationSignatureMode.Structural);

        var trainBatchExamples = new[]
            {
                corpusTrainBatch?.Examples ?? [],
                splitTrainBatch?.Examples ?? [],
                supplementalTrainBatch?.Examples ?? []
            }
            .SelectMany(examples => examples)
            .ToArray();

        var trainBatch = new TranslationLearningBatch(
            BatchId: "ru-en-general-l1-b029-heldout-train",
            DomainTag: TranslationMachine.DomainTag,
            Examples: trainBatchExamples,
            SourceIds: allTrainPairs
                .Select(row => string.IsNullOrWhiteSpace(row.Source) ? row.Id : row.Source)
                .Distinct(StringComparer.Ordinal)
                .ToArray());

        // Count of batch train pairs that passed the uniqueness filter (for transparency in the report).
        var filteredBatchTrainRowsCount = filteredBatchTrainPairs.Count;

        var heldOutBatch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "ru-en-general-l1-b029-heldout-eval",
            heldOutPairs,
            heldOut: true,
            signatureMode: TranslationSignatureMode.Structural);

        var machine = new TranslationMachine();
        var trainResult = await machine.TrainStructuredBatchAsync(trainBatch, ct);
        if (!trainResult.IsSuccess)
            return OperationResult<TranslationBatch029RunReport>.Fail(trainResult.ErrorMessage, trainResult.ErrorCode);

        var orchestrator = await machine.GetOrchestratorAsync(ct);
        if (!orchestrator.IsSuccess)
            return OperationResult<TranslationBatch029RunReport>.Fail(orchestrator.ErrorMessage, orchestrator.ErrorCode);

        var markerAlternatives = BuildMarkerAlternatives(allTrainPairs);
        var heldOutResults = new List<TranslationHeldOutRowResult>(heldOutPairs.Length);
        foreach (var row in heldOutPairs)
        {
            var request = new LogicalOrchestratorRequest(
                RunId: $"batch029-heldout-{row.Id}",
                ScenarioId: TranslationRoutingFlow.ScenarioId,
                UserRequest: row.ASide,
                Flags: new LogicalExecutionFlags(),
                Metadata: new Dictionary<string, object>
                {
                    ["source_language"] = "ru",
                    ["target_language"] = "en",
                    ["topic_domain"] = "general"
                });

            var runtime = await orchestrator.Value!.RunAsync(request, ct);
            var actual = runtime.IsSuccess ? runtime.Value!.FinalHumanResult : runtime.ErrorMessage;
            var expectedMarker = TranslationStructuredBatchBuilder.ExtractProductMarker(row.Chosen);
            var acceptableMarkers = GetAcceptableMarkers(row, expectedMarker, markerAlternatives);
            var bootstrapFallback = actual?.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase) == true;
            var productMatch = !bootstrapFallback && acceptableMarkers.Any(marker =>
                !string.IsNullOrWhiteSpace(marker) &&
                (actual ?? string.Empty).Contains(marker, StringComparison.OrdinalIgnoreCase));

            heldOutResults.Add(new TranslationHeldOutRowResult(
                PairId: row.Id,
                SourceText: row.ASide,
                ExpectedTranslation: row.Chosen,
                ActualTranslation: actual ?? string.Empty,
                ExactMatch: string.Equals(actual, row.Chosen, StringComparison.Ordinal),
                ProductMarker: expectedMarker,
                ProductMatch: productMatch,
                BootstrapFallback: bootstrapFallback));
        }

        var runtimeExactAccuracy = heldOutResults.Count == 0
            ? 0.0
            : heldOutResults.Count(row => row.ExactMatch) / (double)heldOutResults.Count;
        var runtimeProductAccuracy = heldOutResults.Count == 0
            ? 0.0
            : heldOutResults.Count(row => row.ProductMatch) / (double)heldOutResults.Count;
        var runtimeBootstrapRate = heldOutResults.Count == 0
            ? 0.0
            : heldOutResults.Count(row => row.BootstrapFallback) / (double)heldOutResults.Count;

        var export = await machine.ExportAsync(ct);
        if (!export.IsSuccess)
            return OperationResult<TranslationBatch029RunReport>.Fail(export.ErrorMessage, export.ErrorCode);

        var passesGate =
            trainResult.Value!.AccuracyAfter >= trainResult.Value.AccuracyBefore + 0.02
            && trainResult.Value.RewardMargin >= 0.025
            && runtimeProductAccuracy >= 0.95
            && runtimeBootstrapRate <= 0.15;

        var report = new TranslationBatch029RunReport(
            BatchId: "ru-en-general-l1-b029-heldout-run",
            SourcePath: sourcePath,
            SourceRows: rows.Count,
            CorpusTrainingRows: corpusPairs.Count,
            FilteredBatchTrainRows: filteredBatchTrainRowsCount,
            TrainRows: trainRows,
            HeldOutRows: heldOutRows,
            UnitExamplesTrained: trainBatch.Examples.Count,
            UnitExamplesHeldOut: heldOutBatch.Examples.Count,
            UnitAccuracyBefore: trainResult.Value.AccuracyBefore,
            UnitAccuracyAfter: trainResult.Value.AccuracyAfter,
            RuntimeExactAccuracy: runtimeExactAccuracy,
            RuntimeProductAccuracy: runtimeProductAccuracy,
            RuntimeBootstrapRate: runtimeBootstrapRate,
            RewardMargin: trainResult.Value.RewardMargin,
            CpuMs: trainResult.Value.CpuMs,
            PeakRamMb: trainResult.Value.PeakRamMb,
            ElapsedMs: Environment.TickCount64 - startMs,
            CheckpointRef: export.Value!.CheckpointRef,
            ManifestRef: export.Value.ManifestRef,
            PassesGate: passesGate,
            GateNote: passesGate
                ? "Held-out runtime gate passed."
                : "Held-out runtime gate failed; do not promote this translation adapter.",
            HeldOutResults: heldOutResults);

        return OperationResult<TranslationBatch029RunReport>.Ok(report);
    }

    private static async Task<IReadOnlyList<TranslationTrainReadyPair>> LoadAdditionalTrainingPairsAsync(
        string? additionalTrainingRoot,
        string heldOutSourcePath,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(additionalTrainingRoot) || !Directory.Exists(additionalTrainingRoot))
            return [];

        var heldOutFullPath = Path.GetFullPath(heldOutSourcePath);
        var rows = new List<TranslationTrainReadyPair>();
        foreach (var file in Directory.EnumerateFiles(additionalTrainingRoot, "training-pairs-train-ready.jsonl", SearchOption.AllDirectories))
        {
            var fullPath = Path.GetFullPath(file);
            if (string.Equals(fullPath, heldOutFullPath, StringComparison.OrdinalIgnoreCase))
                continue;

            if ((Path.GetDirectoryName(fullPath) ?? string.Empty).Contains("batch-029", StringComparison.OrdinalIgnoreCase))
                continue;

            var jsonl = await File.ReadAllTextAsync(fullPath, Encoding.UTF8, ct);
            foreach (var row in TranslationStructuredBatchBuilder.ParseJsonl(jsonl))
                if (IsUsableRuEnPair(row))
                    rows.Add(row);
        }

        return rows;
    }

    private static async Task<IReadOnlyList<TranslationTrainReadyPair>> LoadSupplementalTrainingPairsAsync(
        string? supplementalTrainingPath,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(supplementalTrainingPath))
            return [];

        if (!File.Exists(supplementalTrainingPath))
            return [];

        var jsonl = await File.ReadAllTextAsync(supplementalTrainingPath, Encoding.UTF8, ct);
        return TranslationStructuredBatchBuilder.ParseJsonl(jsonl)
            .Where(IsUsableRuEnPair)
            .ToArray();
    }

    private static IReadOnlyList<TranslationTrainReadyPair> MergeTrainingPairs(
        IReadOnlyList<TranslationTrainReadyPair> corpusPairs,
        IReadOnlyList<TranslationTrainReadyPair> batchTrainPairs)
    {
        var byId = new Dictionary<string, TranslationTrainReadyPair>(StringComparer.Ordinal);
        foreach (var row in corpusPairs.Concat(batchTrainPairs))
        {
            if (!IsUsableRuEnPair(row))
                continue;

            byId[row.Id] = row;
        }

        return byId.Values.ToArray();
    }

    private static bool IsUsableRuEnPair(TranslationTrainReadyPair row)
        => row is not null
           && string.Equals(row.Direction, "RU_EN", StringComparison.Ordinal)
           && string.Equals(row.ValidationOutcome, "train_ready", StringComparison.Ordinal)
           && row.ValidationErrors is not { Count: > 0 }
           && !string.IsNullOrWhiteSpace(row.Id)
           && !string.IsNullOrWhiteSpace(row.ASide)
           && !string.IsNullOrWhiteSpace(row.Chosen);

    private static string? BuildTrainHeldOutOverlapError(
        IReadOnlyList<TranslationTrainReadyPair> trainPairs,
        IReadOnlyList<TranslationTrainReadyPair> heldOutPairs)
    {
        var trainByInput = trainPairs
            .Where(IsUsableRuEnPair)
            .Select(row => (Key: NormalizeForOverlap(row.ASide), row.Id))
            .Where(row => !string.IsNullOrWhiteSpace(row.Key))
            .GroupBy(row => row.Key, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => string.Join(",", group.Select(row => row.Id).Distinct(StringComparer.Ordinal).Take(3)),
                StringComparer.Ordinal);

        var overlaps = heldOutPairs
            .Where(IsUsableRuEnPair)
            .Select(row => (
                row.Id,
                TrainIds: trainByInput.GetValueOrDefault(NormalizeForOverlap(row.ASide))))
            .Where(row => !string.IsNullOrWhiteSpace(row.TrainIds))
            .ToArray();

        if (overlaps.Length == 0)
            return null;

        var examples = string.Join("; ",
            overlaps.Take(5).Select(row => $"{row.Id} duplicates train row(s) {row.TrainIds}"));
        return $"Train/held-out contamination detected: {overlaps.Length} held-out input(s) are also present in training. {examples}. Use a non-overlapping 100+100 split before claiming TranslationDomain accuracy.";
    }

    private static string NormalizeForOverlap(string value)
        => string.Join(" ", (value ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static IReadOnlyList<TranslationContaminationReport> FindSimilarityContamination(
        IReadOnlyList<TranslationTrainReadyPair> trainPairs,
        IReadOnlyList<TranslationTrainReadyPair> heldOutPairs,
        double threshold)
    {
        var trainFingerprints = trainPairs
            .Where(IsUsableRuEnPair)
            .Select(row => (row.Id, Tokens: TokenizeForSimilarity(row.ASide)))
            .Where(row => row.Tokens.Count > 0)
            .ToArray();

        var overlaps = new List<TranslationContaminationReport>();
        foreach (var heldOut in heldOutPairs.Where(IsUsableRuEnPair))
        {
            var heldTokens = TokenizeForSimilarity(heldOut.ASide);
            if (heldTokens.Count == 0)
                continue;

            var closest = trainFingerprints
                .Select(train => new TranslationContaminationReport(
                    heldOut.Id,
                    train.Id,
                    Jaccard(heldTokens, train.Tokens)))
                .OrderByDescending(row => row.Similarity)
                .FirstOrDefault();

            if (closest is not null && closest.Similarity > threshold)
                overlaps.Add(closest);
        }

        return overlaps;
    }

    private static string BuildSimilarityContaminationError(
        IReadOnlyList<TranslationContaminationReport> contamination)
    {
        var examples = string.Join("; ",
            contamination.Take(5).Select(row =>
                $"{row.HeldOutId} is {row.Similarity:0.00} similar to train row {row.TrainId}"));
        return $"Train/held-out similarity contamination detected: {contamination.Count} held-out input(s) have more than 70% token overlap with training history. {examples}. Replace those held-out rows before claiming TranslationDomain accuracy.";
    }

    private static HashSet<string> TokenizeForSimilarity(string value)
        => (value ?? string.Empty)
            .ToLowerInvariant()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => new string(token.Where(char.IsLetterOrDigit).ToArray()))
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToHashSet(StringComparer.Ordinal);

    private static double Jaccard(IReadOnlySet<string> left, IReadOnlySet<string> right)
    {
        if (left.Count == 0 && right.Count == 0)
            return 1.0;

        var intersection = left.Count(right.Contains);
        var union = left.Count + right.Count - intersection;
        return union == 0 ? 0.0 : intersection / (double)union;
    }

    private static Dictionary<string, HashSet<string>> BuildMarkerAlternatives(
        IReadOnlyList<TranslationTrainReadyPair> trainPairs)
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in trainPairs)
        {
            var sourceMarker = ExtractSourceMarker(row.ASide);
            var targetMarker = TranslationStructuredBatchBuilder.ExtractProductMarker(row.Chosen);
            if (string.IsNullOrWhiteSpace(sourceMarker) || string.IsNullOrWhiteSpace(targetMarker))
                continue;

            if (!result.TryGetValue(sourceMarker, out var markers))
            {
                markers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                result[sourceMarker] = markers;
            }

            markers.Add(targetMarker);
        }

        return result;
    }

    private static IReadOnlyList<string> GetAcceptableMarkers(
        TranslationTrainReadyPair row,
        string expectedMarker,
        IReadOnlyDictionary<string, HashSet<string>> markerAlternatives)
    {
        var markers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(expectedMarker))
            markers.Add(expectedMarker);

        var sourceMarker = ExtractSourceMarker(row.ASide);
        if (!string.IsNullOrWhiteSpace(sourceMarker) &&
            markerAlternatives.TryGetValue(sourceMarker, out var alternatives))
            foreach (var marker in alternatives)
                markers.Add(marker);

        return markers.ToArray();
    }

    private static string ExtractSourceMarker(string sourceText)
    {
        var tokenizer = new TranslationSemanticTokenizer();
        var sequence = tokenizer.Tokenize(sourceText, "ru");
        var marker = sequence.Tokens.FirstOrDefault(token => token.Pos == TranslationPos.ADV)
            ?? sequence.Tokens.FirstOrDefault(token =>
                token.Pos is not TranslationPos.PREP
                    and not TranslationPos.PRON
                    and not TranslationPos.CODE_IDENTIFIER
                    and not TranslationPos.VERB &&
                token.Concept == SemanticConcept.UNKNOWN &&
                (sequence.RuleTags.Count == 0 || LooksLikeMannerMarker(token.Word) || LooksLikeMannerMarker(token.Lemma)));

        return NormalizeMarker(marker?.Lemma ?? marker?.Word ?? string.Empty);
    }

    private static string NormalizeMarker(string value)
        => string.Concat((value ?? string.Empty)
            .Trim()
            .Trim('\'', '"', '`', '.', ',', '!', '?', ';', ':', '«', '»', '“', '”', '(', ')', '[', ']')
            .ToLowerInvariant()
            .Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '$'));

    private static bool LooksLikeMannerMarker(string value)
    {
        var marker = NormalizeMarker(value);
        return marker.Length > 2 &&
            (marker.EndsWith("Ð¾", StringComparison.Ordinal) ||
             marker.EndsWith("Ð½Ð¾", StringComparison.Ordinal) ||
             marker.EndsWith("ÐºÐ¾", StringComparison.Ordinal) ||
             marker.EndsWith("Ð»Ð¾", StringComparison.Ordinal) ||
             marker.EndsWith("ÃÂ¾", StringComparison.Ordinal) ||
             marker.EndsWith("ÃÂ½ÃÂ¾", StringComparison.Ordinal) ||
             marker.EndsWith("ly", StringComparison.OrdinalIgnoreCase));
    }

    public static async Task WriteReportAsync(
        TranslationBatch029RunReport report,
        string reportPath,
        CancellationToken ct = default)
    {
        if (report is null)
            throw new ArgumentNullException(nameof(report));

        var directory = Path.GetDirectoryName(reportPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(reportPath, json, Encoding.UTF8, ct);
    }
}
