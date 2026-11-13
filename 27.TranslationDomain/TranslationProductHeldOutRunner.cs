// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record TranslationProductHeldOutCase(
    [property: JsonPropertyName("case_id")] string CaseId,
    [property: JsonPropertyName("direction")] string Direction,
    [property: JsonPropertyName("source_language")] string SourceLanguage,
    [property: JsonPropertyName("target_language")] string TargetLanguage,
    [property: JsonPropertyName("topic_domain")] string TopicDomain,
    [property: JsonPropertyName("input_text")] string InputText,
    [property: JsonPropertyName("expected_translation")] string ExpectedTranslation,
    [property: JsonPropertyName("required_markers")] IReadOnlyList<string> RequiredMarkers,
    [property: JsonPropertyName("forbidden_markers")] IReadOnlyList<string>? ForbiddenMarkers,
    [property: JsonPropertyName("emoji_markers")] IReadOnlyList<string>? EmojiMarkers,
    [property: JsonPropertyName("named_entities")] IReadOnlyList<string>? NamedEntities);

public sealed record TranslationProductHeldOutCaseResult(
    string CaseId,
    string Direction,
    string SourceText,
    string ExpectedTranslation,
    string ActualTranslation,
    bool ExactMatch,
    bool ProductMatch,
    bool BootstrapFallback,
    bool TargetScriptOk,
    bool CopiedSource,
    bool WrongDirection,
    bool EmojiPreserved,
    bool NamedEntitiesPreserved,
    double Bleu1,
    double ChrF1,
    IReadOnlyList<string> MissingRequiredMarkers,
    IReadOnlyList<string> ForbiddenMarkersFound);

public sealed record TranslationProductHeldOutRunReport(
    string BatchId,
    string TrainingPath,
    string HeldOutPath,
    int TrainingRows,
    int HeldOutRows,
    int UnitExamplesTrained,
    double RuntimeExactAccuracy,
    double RuntimeProductAccuracy,
    double RuntimeBootstrapRate,
    double TargetScriptAccuracy,
    double CopiedSourceRate,
    double WrongDirectionRate,
    double EmojiPreservationAccuracy,
    double NamedEntityPreservationAccuracy,
    double Bleu1,
    double ChrF1,
    bool MeetsAccuracyTargets,
    double? RewardMargin,
    bool RewardMarginAvailable,
    int RewardMarginEvidenceCount,
    string? RewardMarginSource,
    long CpuMs,
    long PeakRamMb,
    long ElapsedMs,
    string CheckpointRef,
    string ManifestRef,
    bool PassesGate,
    bool PassesPromotionGate,
    string GateNote,
    IReadOnlyList<TranslationProductHeldOutCaseResult> CaseResults);

public static class TranslationProductHeldOutRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public static async Task<OperationResult<TranslationProductHeldOutRunReport>> RunRuEnAsync(
        string trainingPath,
        string heldOutPath,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Product held-out runner cancelled.", "CANCELLED");

        if (string.IsNullOrWhiteSpace(trainingPath))
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Training path is required.", "TRAINING_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(heldOutPath))
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Held-out path is required.", "HELDOUT_PATH_REQUIRED");

        if (!File.Exists(trainingPath))
            return OperationResult<TranslationProductHeldOutRunReport>.Fail($"Training file not found: {trainingPath}", "TRAINING_FILE_NOT_FOUND");

        if (!File.Exists(heldOutPath))
            return OperationResult<TranslationProductHeldOutRunReport>.Fail($"Held-out file not found: {heldOutPath}", "HELDOUT_FILE_NOT_FOUND");

        var startMs = Environment.TickCount64;
        var trainRows = TranslationStructuredBatchBuilder.ParseJsonl(
                await File.ReadAllTextAsync(trainingPath, Encoding.UTF8, ct))
            .Where(IsUsableRuEnPair)
            .ToArray();
        if (trainRows.Length == 0)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Training file has no usable RU_EN train-ready rows.", "NO_TRAINING_ROWS");

        var cases = ParseCases(await File.ReadAllTextAsync(heldOutPath, Encoding.UTF8, ct))
            .Where(row => string.Equals(row.Direction, "RU_EN", StringComparison.Ordinal))
            .ToArray();
        if (cases.Length == 0)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Held-out file has no RU_EN product cases.", "NO_HELDOUT_ROWS");

        var contaminationError = BuildTrainHeldOutOverlapError(trainRows, cases);
        if (contaminationError is not null)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail(contaminationError, "TRAIN_HELDOUT_OVERLAP");

        var batchId = Path.GetFileNameWithoutExtension(trainingPath);
        var batch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            $"{batchId}-train",
            trainRows,
            heldOut: false,
            signatureMode: TranslationSignatureMode.Structural);

        var machine = new TranslationMachine();
        var trainResult = await machine.TrainStructuredBatchAsync(batch, ct);
        if (!trainResult.IsSuccess)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail(trainResult.ErrorMessage, trainResult.ErrorCode);

        var orchestrator = await machine.GetOrchestratorAsync(ct);
        if (!orchestrator.IsSuccess)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail(orchestrator.ErrorMessage, orchestrator.ErrorCode);

        var caseResults = new List<TranslationProductHeldOutCaseResult>(cases.Length);
        foreach (var testCase in cases)
        {
            var request = new LogicalOrchestratorRequest(
                RunId: $"translation-product-heldout-{testCase.CaseId}",
                ScenarioId: TranslationRoutingFlow.ScenarioId,
                UserRequest: testCase.InputText,
                Flags: new LogicalExecutionFlags(),
                Metadata: new Dictionary<string, object>
                {
                    ["source_language"] = testCase.SourceLanguage,
                    ["target_language"] = testCase.TargetLanguage,
                    ["topic_domain"] = testCase.TopicDomain
                });

            var runtime = await orchestrator.Value!.RunAsync(request, ct);
            var actual = runtime.IsSuccess ? runtime.Value!.FinalHumanResult : runtime.ErrorMessage ?? string.Empty;
            var bootstrapFallback = actual.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase);
            var missingMarkers = (testCase.RequiredMarkers ?? [])
                .Where(marker => !ContainsIgnoreCase(actual, marker))
                .ToArray();
            var forbiddenFound = (testCase.ForbiddenMarkers ?? [])
                .Where(marker => ContainsIgnoreCase(actual, marker))
                .ToArray();
            var targetScriptOk = IsTargetScriptOk(actual, testCase.TargetLanguage);
            var copiedSource = IsCopiedSource(actual, testCase.InputText, testCase.TargetLanguage);
            var wrongDirection = !targetScriptOk;
            var emojiPreserved = (testCase.EmojiMarkers ?? []).All(marker => actual.Contains(marker, StringComparison.Ordinal));
            var namedEntitiesPreserved = (testCase.NamedEntities ?? []).All(marker => ContainsIgnoreCase(actual, marker));
            var exact = string.Equals(NormalizeComparable(actual), NormalizeComparable(testCase.ExpectedTranslation), StringComparison.Ordinal);
            var productMatch =
                !bootstrapFallback &&
                missingMarkers.Length == 0 &&
                forbiddenFound.Length == 0 &&
                targetScriptOk &&
                !copiedSource &&
                !wrongDirection &&
                emojiPreserved &&
                namedEntitiesPreserved;

            caseResults.Add(new TranslationProductHeldOutCaseResult(
                CaseId: testCase.CaseId,
                Direction: testCase.Direction,
                SourceText: testCase.InputText,
                ExpectedTranslation: testCase.ExpectedTranslation,
                ActualTranslation: actual,
                ExactMatch: exact,
                ProductMatch: productMatch,
                BootstrapFallback: bootstrapFallback,
                TargetScriptOk: targetScriptOk,
                CopiedSource: copiedSource,
                WrongDirection: wrongDirection,
                EmojiPreserved: emojiPreserved,
                NamedEntitiesPreserved: namedEntitiesPreserved,
                Bleu1: ComputeBleu1(actual, testCase.ExpectedTranslation),
                ChrF1: ComputeChrF1(actual, testCase.ExpectedTranslation),
                MissingRequiredMarkers: missingMarkers,
                ForbiddenMarkersFound: forbiddenFound));
        }

        var export = await machine.ExportAsync(ct);
        if (!export.IsSuccess)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail(export.ErrorMessage, export.ErrorCode);

        var runtimeExactAccuracy = Ratio(caseResults, row => row.ExactMatch);
        var runtimeProductAccuracy = Ratio(caseResults, row => row.ProductMatch);
        var runtimeBootstrapRate = Ratio(caseResults, row => row.BootstrapFallback);
        var targetScriptAccuracy = Ratio(caseResults, row => row.TargetScriptOk);
        var copiedSourceRate = Ratio(caseResults, row => row.CopiedSource);
        var wrongDirectionRate = Ratio(caseResults, row => row.WrongDirection);
        var emojiCases = caseResults.Where(row => HasAnyEmoji(row.SourceText)).ToArray();
        var casesById = cases.ToDictionary(row => row.CaseId, StringComparer.Ordinal);
        var namedEntityCases = caseResults
            .Where(row => casesById.TryGetValue(row.CaseId, out var testCase) &&
                          testCase.NamedEntities is { Count: > 0 })
            .ToArray();
        var emojiAccuracy = emojiCases.Length == 0 ? 1.0 : Ratio(emojiCases, row => row.EmojiPreserved);
        var namedEntityAccuracy = namedEntityCases.Length == 0 ? 1.0 : Ratio(namedEntityCases, row => row.NamedEntitiesPreserved);
        var bleu1 = caseResults.Count == 0 ? 0.0 : caseResults.Average(row => row.Bleu1);
        var chrf1 = caseResults.Count == 0 ? 0.0 : caseResults.Average(row => row.ChrF1);

        var meetsAccuracyTargets =
            trainResult.Value!.AccuracyAfter >= trainResult.Value.AccuracyBefore + 0.02 &&
            runtimeProductAccuracy >= 0.95 &&
            runtimeBootstrapRate <= 0.15 &&
            targetScriptAccuracy >= 1.0 &&
            copiedSourceRate <= 0.0 &&
            wrongDirectionRate <= 0.0 &&
            emojiAccuracy >= 1.0 &&
            namedEntityAccuracy >= 1.0;

        var report = new TranslationProductHeldOutRunReport(
            BatchId: batchId,
            TrainingPath: trainingPath,
            HeldOutPath: heldOutPath,
            TrainingRows: trainRows.Length,
            HeldOutRows: cases.Length,
            UnitExamplesTrained: batch.Examples.Count,
            RuntimeExactAccuracy: runtimeExactAccuracy,
            RuntimeProductAccuracy: runtimeProductAccuracy,
            RuntimeBootstrapRate: runtimeBootstrapRate,
            TargetScriptAccuracy: targetScriptAccuracy,
            CopiedSourceRate: copiedSourceRate,
            WrongDirectionRate: wrongDirectionRate,
            EmojiPreservationAccuracy: emojiAccuracy,
            NamedEntityPreservationAccuracy: namedEntityAccuracy,
            Bleu1: bleu1,
            ChrF1: chrf1,
            MeetsAccuracyTargets: meetsAccuracyTargets,
            RewardMargin: null,
            RewardMarginAvailable: false,
            RewardMarginEvidenceCount: 0,
            RewardMarginSource: null,
            CpuMs: trainResult.Value.CpuMs,
            PeakRamMb: trainResult.Value.PeakRamMb,
            ElapsedMs: Environment.TickCount64 - startMs,
            CheckpointRef: export.Value!.CheckpointRef,
            ManifestRef: export.Value.ManifestRef,
            PassesGate: meetsAccuracyTargets,
            PassesPromotionGate: false,
            GateNote:
                "REWARD_MARGIN_UNAVAILABLE: preference comparison was not evaluated; " +
                $"accuracy_targets_met={meetsAccuracyTargets}; " +
                $"unit_accuracy_before={trainResult.Value.AccuracyBefore:F4}; " +
                $"unit_accuracy_after={trainResult.Value.AccuracyAfter:F4}; " +
                $"runtime_product_accuracy={runtimeProductAccuracy:F4}",
            CaseResults: caseResults);

        return OperationResult<TranslationProductHeldOutRunReport>.Ok(report);
    }

    public static async Task<OperationResult<TranslationProductHeldOutRunReport>> RunAllDirectionsAsync(
        string trainingPath,
        string heldOutPath,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Product held-out runner cancelled.", "CANCELLED");

        if (string.IsNullOrWhiteSpace(trainingPath))
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Training path is required.", "TRAINING_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(heldOutPath))
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Held-out path is required.", "HELDOUT_PATH_REQUIRED");

        if (!File.Exists(trainingPath))
            return OperationResult<TranslationProductHeldOutRunReport>.Fail($"Training file not found: {trainingPath}", "TRAINING_FILE_NOT_FOUND");

        if (!File.Exists(heldOutPath))
            return OperationResult<TranslationProductHeldOutRunReport>.Fail($"Held-out file not found: {heldOutPath}", "HELDOUT_FILE_NOT_FOUND");

        var startMs = Environment.TickCount64;
        var trainRows = TranslationStructuredBatchBuilder.ParseJsonl(
                await File.ReadAllTextAsync(trainingPath, Encoding.UTF8, ct))
            .Where(IsUsablePair)
            .ToArray();
        if (trainRows.Length == 0)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Training file has no usable train-ready rows.", "NO_TRAINING_ROWS");

        var cases = ParseCases(await File.ReadAllTextAsync(heldOutPath, Encoding.UTF8, ct)).ToArray();
        if (cases.Length == 0)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail("Held-out file has no product cases.", "NO_HELDOUT_ROWS");

        var contaminationError = BuildTrainHeldOutOverlapError(trainRows, cases);
        if (contaminationError is not null)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail(contaminationError, "TRAIN_HELDOUT_OVERLAP");

        var batchId = Path.GetFileNameWithoutExtension(trainingPath);
        var batch = TranslationStructuredBatchBuilder.BuildGeneralBatch(
            $"{batchId}-train",
            trainRows,
            heldOut: false,
            signatureMode: TranslationSignatureMode.Structural);

        var machine = new TranslationMachine();
        var trainResult = await machine.TrainStructuredBatchAsync(batch, ct);
        if (!trainResult.IsSuccess)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail(trainResult.ErrorMessage, trainResult.ErrorCode);

        var orchestrator = await machine.GetOrchestratorAsync(ct);
        if (!orchestrator.IsSuccess)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail(orchestrator.ErrorMessage, orchestrator.ErrorCode);

        var caseResults = new List<TranslationProductHeldOutCaseResult>(cases.Length);
        foreach (var testCase in cases)
        {
            var request = new LogicalOrchestratorRequest(
                RunId: $"translation-product-heldout-{testCase.CaseId}",
                ScenarioId: TranslationRoutingFlow.ScenarioId,
                UserRequest: testCase.InputText,
                Flags: new LogicalExecutionFlags(),
                Metadata: new Dictionary<string, object>
                {
                    ["source_language"] = testCase.SourceLanguage,
                    ["target_language"] = testCase.TargetLanguage,
                    ["topic_domain"] = testCase.TopicDomain
                });

            var runtime = await orchestrator.Value!.RunAsync(request, ct);
            var actual = runtime.IsSuccess ? runtime.Value!.FinalHumanResult : runtime.ErrorMessage ?? string.Empty;
            var bootstrapFallback = actual.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase);
            var missingMarkers = (testCase.RequiredMarkers ?? [])
                .Where(marker => !ContainsIgnoreCase(actual, marker))
                .ToArray();
            var forbiddenFound = (testCase.ForbiddenMarkers ?? [])
                .Where(marker => ContainsIgnoreCase(actual, marker))
                .ToArray();
            var targetScriptOk = IsTargetScriptOk(actual, testCase.TargetLanguage);
            var copiedSource = IsCopiedSource(actual, testCase.InputText, testCase.TargetLanguage);
            var wrongDirection = !targetScriptOk;
            var emojiPreserved = (testCase.EmojiMarkers ?? []).All(marker => actual.Contains(marker, StringComparison.Ordinal));
            var namedEntitiesPreserved = (testCase.NamedEntities ?? []).All(marker => ContainsIgnoreCase(actual, marker));
            var exact = string.Equals(NormalizeComparable(actual), NormalizeComparable(testCase.ExpectedTranslation), StringComparison.Ordinal);
            var productMatch =
                !bootstrapFallback &&
                missingMarkers.Length == 0 &&
                forbiddenFound.Length == 0 &&
                targetScriptOk &&
                !copiedSource &&
                !wrongDirection &&
                emojiPreserved &&
                namedEntitiesPreserved;

            caseResults.Add(new TranslationProductHeldOutCaseResult(
                CaseId: testCase.CaseId,
                Direction: testCase.Direction,
                SourceText: testCase.InputText,
                ExpectedTranslation: testCase.ExpectedTranslation,
                ActualTranslation: actual,
                ExactMatch: exact,
                ProductMatch: productMatch,
                BootstrapFallback: bootstrapFallback,
                TargetScriptOk: targetScriptOk,
                CopiedSource: copiedSource,
                WrongDirection: wrongDirection,
                EmojiPreserved: emojiPreserved,
                NamedEntitiesPreserved: namedEntitiesPreserved,
                Bleu1: ComputeBleu1(actual, testCase.ExpectedTranslation),
                ChrF1: ComputeChrF1(actual, testCase.ExpectedTranslation),
                MissingRequiredMarkers: missingMarkers,
                ForbiddenMarkersFound: forbiddenFound));
        }

        var export = await machine.ExportAsync(ct);
        if (!export.IsSuccess)
            return OperationResult<TranslationProductHeldOutRunReport>.Fail(export.ErrorMessage, export.ErrorCode);

        var runtimeExactAccuracy = Ratio(caseResults, row => row.ExactMatch);
        var runtimeProductAccuracy = Ratio(caseResults, row => row.ProductMatch);
        var runtimeBootstrapRate = Ratio(caseResults, row => row.BootstrapFallback);
        var targetScriptAccuracy = Ratio(caseResults, row => row.TargetScriptOk);
        var copiedSourceRate = Ratio(caseResults, row => row.CopiedSource);
        var wrongDirectionRate = Ratio(caseResults, row => row.WrongDirection);
        var casesById = cases.ToDictionary(row => row.CaseId, StringComparer.Ordinal);
        var emojiCases = caseResults
            .Where(row => casesById.TryGetValue(row.CaseId, out var testCase) &&
                          testCase.EmojiMarkers is { Count: > 0 })
            .ToArray();
        var namedEntityCases = caseResults
            .Where(row => casesById.TryGetValue(row.CaseId, out var testCase) &&
                          testCase.NamedEntities is { Count: > 0 })
            .ToArray();
        var emojiAccuracy = emojiCases.Length == 0 ? 1.0 : Ratio(emojiCases, row => row.EmojiPreserved);
        var namedEntityAccuracy = namedEntityCases.Length == 0 ? 1.0 : Ratio(namedEntityCases, row => row.NamedEntitiesPreserved);
        var bleu1 = caseResults.Count == 0 ? 0.0 : caseResults.Average(row => row.Bleu1);
        var chrf1 = caseResults.Count == 0 ? 0.0 : caseResults.Average(row => row.ChrF1);

        var meetsAccuracyTargets =
            trainResult.Value!.AccuracyAfter >= trainResult.Value.AccuracyBefore + 0.02 &&
            runtimeProductAccuracy >= 0.95 &&
            runtimeBootstrapRate <= 0.15 &&
            targetScriptAccuracy >= 1.0 &&
            copiedSourceRate <= 0.0 &&
            wrongDirectionRate <= 0.0 &&
            emojiAccuracy >= 1.0 &&
            namedEntityAccuracy >= 1.0;

        var report = new TranslationProductHeldOutRunReport(
            BatchId: batchId,
            TrainingPath: trainingPath,
            HeldOutPath: heldOutPath,
            TrainingRows: trainRows.Length,
            HeldOutRows: cases.Length,
            UnitExamplesTrained: batch.Examples.Count,
            RuntimeExactAccuracy: runtimeExactAccuracy,
            RuntimeProductAccuracy: runtimeProductAccuracy,
            RuntimeBootstrapRate: runtimeBootstrapRate,
            TargetScriptAccuracy: targetScriptAccuracy,
            CopiedSourceRate: copiedSourceRate,
            WrongDirectionRate: wrongDirectionRate,
            EmojiPreservationAccuracy: emojiAccuracy,
            NamedEntityPreservationAccuracy: namedEntityAccuracy,
            Bleu1: bleu1,
            ChrF1: chrf1,
            MeetsAccuracyTargets: meetsAccuracyTargets,
            RewardMargin: null,
            RewardMarginAvailable: false,
            RewardMarginEvidenceCount: 0,
            RewardMarginSource: null,
            CpuMs: trainResult.Value.CpuMs,
            PeakRamMb: trainResult.Value.PeakRamMb,
            ElapsedMs: Environment.TickCount64 - startMs,
            CheckpointRef: export.Value!.CheckpointRef,
            ManifestRef: export.Value.ManifestRef,
            PassesGate: meetsAccuracyTargets,
            PassesPromotionGate: false,
            GateNote:
                "REWARD_MARGIN_UNAVAILABLE: preference comparison was not evaluated; " +
                $"accuracy_targets_met={meetsAccuracyTargets}; " +
                $"unit_accuracy_before={trainResult.Value.AccuracyBefore:F4}; " +
                $"unit_accuracy_after={trainResult.Value.AccuracyAfter:F4}; " +
                $"runtime_product_accuracy={runtimeProductAccuracy:F4}",
            CaseResults: caseResults);

        return OperationResult<TranslationProductHeldOutRunReport>.Ok(report);
    }

    public static async Task WriteReportAsync(
        TranslationProductHeldOutRunReport report,
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

    private static IReadOnlyList<TranslationProductHeldOutCase> ParseCases(string jsonl)
    {
        if (string.IsNullOrWhiteSpace(jsonl))
            return [];

        var rows = new List<TranslationProductHeldOutCase>();
        foreach (var line in jsonl.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries))
        {
            var row = JsonSerializer.Deserialize<TranslationProductHeldOutCase>(line, JsonOptions);
            if (row is not null)
                rows.Add(row);
        }

        return rows;
    }

    private static bool IsUsableRuEnPair(TranslationTrainReadyPair row)
        => string.Equals(row.Direction, "RU_EN", StringComparison.Ordinal)
           && IsUsablePair(row);

    private static bool IsUsablePair(TranslationTrainReadyPair row)
        => Enum.TryParse<TranslationDirection>(row.Direction, ignoreCase: true, out _)
           && string.Equals(row.ValidationOutcome, "train_ready", StringComparison.Ordinal)
           && row.ValidationErrors is not { Count: > 0 }
           && !string.IsNullOrWhiteSpace(row.Id)
           && !string.IsNullOrWhiteSpace(row.ASide)
           && !string.IsNullOrWhiteSpace(row.Chosen);

    private static string? BuildTrainHeldOutOverlapError(
        IReadOnlyList<TranslationTrainReadyPair> trainRows,
        IReadOnlyList<TranslationProductHeldOutCase> cases)
    {
        var trainByInput = trainRows
            .Select(row => (Key: NormalizeForOverlap(row.ASide), row.Id))
            .Where(row => !string.IsNullOrWhiteSpace(row.Key))
            .GroupBy(row => row.Key, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => string.Join(",", group.Select(row => row.Id).Distinct(StringComparer.Ordinal).Take(3)),
                StringComparer.Ordinal);

        var overlaps = cases
            .Select(testCase => (
                testCase.CaseId,
                TrainIds: trainByInput.GetValueOrDefault(NormalizeForOverlap(testCase.InputText))))
            .Where(row => !string.IsNullOrWhiteSpace(row.TrainIds))
            .ToArray();

        if (overlaps.Length == 0)
            return null;

        var examples = string.Join("; ",
            overlaps.Take(5).Select(row => $"{row.CaseId} duplicates train row(s) {row.TrainIds}"));
        return $"Train/held-out contamination detected: {overlaps.Length} held-out input(s) are also present in the training file. {examples}. Use a non-overlapping held-out file before claiming product accuracy.";
    }

    private static string NormalizeForOverlap(string value)
        => Regex.Replace(value ?? string.Empty, @"\s+", " ")
            .Trim()
            .ToLowerInvariant();

    private static bool ContainsIgnoreCase(string haystack, string needle)
        => string.IsNullOrWhiteSpace(needle) ||
           (haystack ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase);

    private static bool IsTargetScriptOk(string actual, string targetLanguage)
    {
        if (targetLanguage.Equals("en", StringComparison.OrdinalIgnoreCase))
            return HasLatinLetter(actual) && !HasCyrillic(actual) && !HasHebrew(actual);

        if (targetLanguage.Equals("ru", StringComparison.OrdinalIgnoreCase))
            return HasCyrillic(actual) && !HasHebrew(actual);

        if (targetLanguage.Equals("he", StringComparison.OrdinalIgnoreCase))
            return HasHebrew(actual) && !HasCyrillic(actual);

        return true;
    }

    private static bool IsCopiedSource(string actual, string source, string targetLanguage)
    {
        var normalizedActual = NormalizeComparable(actual);
        var normalizedSource = NormalizeComparable(source);
        if (!string.IsNullOrEmpty(normalizedSource) && normalizedActual.Contains(normalizedSource, StringComparison.Ordinal))
            return true;

        if (targetLanguage.Equals("en", StringComparison.OrdinalIgnoreCase))
            return HasCyrillic(actual) || HasHebrew(actual);

        if (targetLanguage.Equals("ru", StringComparison.OrdinalIgnoreCase))
            return !HasCyrillic(actual) || HasHebrew(actual);

        if (targetLanguage.Equals("he", StringComparison.OrdinalIgnoreCase))
            return !HasHebrew(actual) || HasCyrillic(actual);

        return false;
    }

    private static bool HasLatinLetter(string value)
        => (value ?? string.Empty).Any(ch => (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'));

    private static bool HasCyrillic(string value)
        => (value ?? string.Empty).Any(ch => ch >= '\u0400' && ch <= '\u04FF');

    private static bool HasHebrew(string value)
        => (value ?? string.Empty).Any(ch => ch >= '\u0590' && ch <= '\u05FF');

    private static bool HasAnyEmoji(string value)
        => (value ?? string.Empty).Any(ch => char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.OtherSymbol);

    private static double Ratio<T>(IReadOnlyList<T> rows, Func<T, bool> predicate)
        => rows.Count == 0 ? 0.0 : rows.Count(predicate) / (double)rows.Count;

    private static string NormalizeComparable(string value)
        => string.Join(" ", Regex.Replace(value ?? string.Empty, @"\s+", " ")
                .Trim()
                .Trim('.', '!', '?')
                .ToLowerInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static double ComputeBleu1(string actual, string expected)
    {
        var actualTokens = TokenizeEnglish(actual);
        var expectedTokens = TokenizeEnglish(expected);
        if (actualTokens.Length == 0 || expectedTokens.Length == 0)
            return 0.0;

        var expectedCounts = expectedTokens
            .GroupBy(token => token, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        var matches = 0;
        foreach (var token in actualTokens)
            if (expectedCounts.TryGetValue(token, out var count) && count > 0)
            {
                matches++;
                expectedCounts[token] = count - 1;
            }

        return matches / (double)actualTokens.Length;
    }

    private static double ComputeChrF1(string actual, string expected)
    {
        var actualChars = CharCounts(actual);
        var expectedChars = CharCounts(expected);
        if (actualChars.Count == 0 || expectedChars.Count == 0)
            return 0.0;

        var overlap = actualChars.Sum(kv => Math.Min(kv.Value, expectedChars.GetValueOrDefault(kv.Key)));
        var precision = overlap / (double)actualChars.Values.Sum();
        var recall = overlap / (double)expectedChars.Values.Sum();
        return precision + recall == 0.0 ? 0.0 : 2 * precision * recall / (precision + recall);
    }

    private static string[] TokenizeEnglish(string value)
        => Regex.Matches(value ?? string.Empty, @"[\p{L}\p{N}']+")
            .Select(match => match.Value.ToLowerInvariant())
            .ToArray();

    private static Dictionary<char, int> CharCounts(string value)
    {
        var counts = new Dictionary<char, int>();
        foreach (var ch in (value ?? string.Empty).ToLowerInvariant())
        {
            if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch))
                continue;

            counts[ch] = counts.GetValueOrDefault(ch) + 1;
        }

        return counts;
    }
}
