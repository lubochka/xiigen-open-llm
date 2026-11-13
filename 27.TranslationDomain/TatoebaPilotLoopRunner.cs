// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure.Logical;

public sealed record TatoebaPilotLoopRunOptions(
    string PrivatePairsPath,
    int TrainSetSize = 100,
    int TestSetSize = 100,
    int MaxCycles = 9,
    int SkipCompletedCycles = 0,
    double TargetAccuracy = 0.95,
    string? CheckpointPath = null,           // absolute path where checkpoint is saved/loaded
    bool RequireCheckpointResume = false,    // when true: fail if no checkpoint found, never start fresh
    bool StartNewSourceFromCheckpoint = false, // explicit source rollover; load machine weights, reset source loop only
    TranslationCheckpointCache? Cache = null, // in-memory cache layer (null = no cache, always disk)
    string? LostCandidateDirectory = null); // Явный output; входной корпус не является каталогом записи.

public sealed record TatoebaPilotLoopReport(
    string PrivatePairsPath,
    int TotalPairs,
    int TrainSetSize,
    int TestSetSize,
    int SkipCompletedCycles,
    double TargetAccuracy,
    double BestRuntimeProductAccuracy,
    bool CheckpointLoaded,
    bool CheckpointSaved,
    string ResumeMode,
    int ReplayedCycles,
    bool InitialTrainingSkippedDueCheckpoint,
    string? SourceRolloverFrom,
    string? SourceRolloverTo,
    int? SourceRolloverStartCursor,
    int StartCursor,
    int EndCursor,
    int StartCycle,
    int LastCompletedCycle,
    int AcceptedTrainingRows,
    int SkippedRows,
    bool ReachedTarget,
    IReadOnlyList<TatoebaPilotLoopCycleReport> Cycles);

public sealed record TatoebaPilotLoopCycleReport(
    int Cycle,
    int TrainRows,
    int TestRows,
    int CandidateTestRowsScanned,
    int NoveltyScanSkippedRows,
    string CandidateSelectionMode,
    int FilteredExactOverlapRows,
    int FilteredCheckpointMemoryOverlapRows,
    int FilteredSimilarityOverlapRows,
    int TestSelectionShortfall,
    double NewPatternRate,
    double SourceTokenSeenRate,
    double RuntimeProductAccuracy,
    double RuntimeExactAccuracy,
    double RuntimeBootstrapRate,
    double TargetScriptAccuracy,
    double CopiedSourceRate,
    double AverageTokenSimilarity,
    bool PassesTarget,
    bool PromotedToTraining,
    double PromotionMaxAccuracy,
    double MinimumPromotionNewPatternRate,
    double MaximumPromotionSeenTokenRate,
    string PromotionDecision,
    IReadOnlyList<TatoebaPilotLoopRowResult> Rows);

public sealed record TatoebaPilotLoopRowResult(
    int SourceSentenceId,
    int TargetSentenceId,
    string Direction,
    string SourceText,
    string ExpectedText,
    string ActualText,
    bool ExactMatch,
    bool ProductMatch,
    bool BootstrapFallback,
    bool TargetScriptOk,
    bool CopiedSource,
    double TokenSimilarity);

public static partial class TatoebaPilotLoopRunner
{
    private const double TrainingHistorySimilarityThreshold = 0.70;
    private const double DefaultPromotionMaxAccuracy = 0.78;
    private const int MinimumLexiconSupport = 3;
    private const double MinimumLexiconConfidence = 0.60;
    private const double MinimumLexiconSpecificity = 0.45;
    private const double MinimumLexiconScoreMargin = 0.15;
    private const int MaxLexiconMappingsPerRow = 8;
    private const int MaxPhraseMappingsPerRow = 3;
    private const int MinimumPhraseSupport = 2;
    private const double MinimumPhraseCoverage = 0.60;
    private const double MinimumPhraseSpecificity = 0.50;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly Regex WordRegex = new(@"[\p{L}]+(?:['’][\p{L}]+)?", RegexOptions.Compiled);

    private static readonly HashSet<string> EnglishStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "that", "with", "this", "from", "they", "you", "your", "are", "was", "were",
        "have", "has", "had", "not", "but", "his", "her", "she", "him", "our", "their", "there", "here",
        "what", "when", "where", "who", "why", "how", "into", "onto", "about", "than", "then", "them",
        "let", "lets", "did", "does", "done", "been", "being", "will", "would", "can", "could", "should",
        "a", "an", "of", "to", "in", "on", "is", "am", "as", "at", "by", "or", "do", "under", "over"
    };

    private static readonly HashSet<string> RussianStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "что", "это", "как", "для", "или", "она", "они", "оно", "его", "её", "ему", "мне", "тебе",
        "меня", "тебя", "нас", "вас", "уже", "еще", "ещё", "быть", "был", "была", "были", "кто",
        "где", "когда", "почему", "потому", "чтобы", "если", "при", "над", "под", "без", "через",
        "не", "нет", "the", "and", "for", "that", "with"
    };

    private static readonly HashSet<string> NegationWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "not", "no", "never", "none", "n't", "не", "нет", "никогда"
    };

    // Выбор output не делает файловых записей и никогда не выводится из входного корпуса.
    internal static string? ResolveLostCandidateDirectory(TatoebaPilotLoopRunOptions options)
        => !string.IsNullOrWhiteSpace(options.LostCandidateDirectory)
            ? Path.GetFullPath(options.LostCandidateDirectory)
            : options.CheckpointPath is not null
                ? Path.Combine(Path.GetDirectoryName(Path.GetFullPath(options.CheckpointPath))!, "lost-candidates")
                : null;

    public static async Task<TatoebaPilotLoopReport> RunAsync(
        TatoebaPilotLoopRunOptions options,
        Action<TatoebaPilotLoopCycleReport>? onCycleCompleted = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!File.Exists(options.PrivatePairsPath))
            throw new FileNotFoundException("Private Tatoeba pilot pair file not found.", options.PrivatePairsPath);
        if (options.TrainSetSize <= 0 || options.TestSetSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "TrainSetSize and TestSetSize must be positive.");
        if (options.MaxCycles < 0 || options.SkipCompletedCycles < 0)
            throw new ArgumentOutOfRangeException(nameof(options), "MaxCycles and SkipCompletedCycles must not be negative.");

        var rows = await ReadPrivatePairsAsync(options.PrivatePairsPath, ct);
        var cycles = new List<TatoebaPilotLoopCycleReport>();

        // Determine absolute checkpoint path once (null when not configured).
        var absoluteCheckpointPath = options.CheckpointPath is null
            ? null
            : Path.GetFullPath(options.CheckpointPath);

        // Без явно назначенного output сохраняется честный режим без хранилища.
        var candidateDirectory = ResolveLostCandidateDirectory(options);

        // Resolve the machine from cache, disk checkpoint, or fresh start.
        TranslationMachine machine;
        var lastCompletedCycle = 0;       // 0 means no cycles completed; resume will update this.
        int cursor;
        var checkpointLoaded = false;
        var checkpointSaved = false;
        var replayedCycles = 0;
        var initialTrainingSkippedDueCheckpoint = false;
        var resumeMode = "fresh";
        IReadOnlyDictionary<string, HashSet<string>> checkpointExactSourceMemory =
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        List<TatoebaPrivateSentencePair> trainingRows;
        List<TatoebaPilotPairRefCheckpoint> acceptedTrainingRefs;
        List<TatoebaPilotPairRefCheckpoint> skippedRowRefs;
        TranslationMachineLoadResult? loadedCheckpoint = null;
        var bestObservedRuntimeProductAccuracy = DefaultPromotionMaxAccuracy;
        string? sourceRolloverFrom = null;
        string? sourceRolloverTo = null;
        int? sourceRolloverStartCursor = null;

        if (options.StartNewSourceFromCheckpoint && options.SkipCompletedCycles > 0)
        {
            throw new InvalidOperationException(
                "Source rollover cannot be combined with --skip-cycles replay.");
        }

        if (absoluteCheckpointPath is not null
            && options.Cache is not null
            && options.Cache.TryGet(absoluteCheckpointPath, out var cachedResult))
        {
            // Cache hit: use the in-memory machine and restore cursor from the saved loop state.
            loadedCheckpoint = cachedResult!;
        }
        else if (absoluteCheckpointPath is not null && File.Exists(absoluteCheckpointPath))
        {
            // Cache miss but file exists: load from disk and populate cache.
            var loadResult = await TranslationMachine.LoadCheckpointAsync(absoluteCheckpointPath, ct);
            if (!loadResult.IsSuccess)
                throw new InvalidOperationException($"Failed to load checkpoint: {loadResult.ErrorMessage} ({loadResult.ErrorCode})");
            loadedCheckpoint = loadResult.Value!;
            options.Cache?.Store(absoluteCheckpointPath, loadResult.Value!);
        }
        else if (options.RequireCheckpointResume)
        {
            // Checkpoint required but not found: fail hard, do not start fresh.
            throw new FileNotFoundException(
                $"Checkpoint required but not found: {absoluteCheckpointPath}", absoluteCheckpointPath);
        }

        if (loadedCheckpoint is not null)
        {
            checkpointLoaded = true;
            machine = loadedCheckpoint.Machine;
            // Подключаем и disk-load, и cache-hit ДО возможного обучения source-rollover.
            if (candidateDirectory is not null)
                machine.UseLostCandidateStore(new TranslationFileLostCandidateStore(candidateDirectory));
            checkpointExactSourceMemory = BuildCheckpointExactSourceMemory(machine);
            var loopState = RequireLoopState(loadedCheckpoint.TatoebaLoop, absoluteCheckpointPath!);
            bestObservedRuntimeProductAccuracy = loopState.BestRuntimeProductAccuracy ?? DefaultPromotionMaxAccuracy;

            if (options.StartNewSourceFromCheckpoint)
            {
                resumeMode = "source-rollover";
                ValidateLoopStateCanRollover(loopState, options);
                sourceRolloverFrom = Path.GetFullPath(loopState.PrivatePairsPath);
                sourceRolloverTo = Path.GetFullPath(options.PrivatePairsPath);
                sourceRolloverStartCursor = 0;
                lastCompletedCycle = 0;

                var initialTrainingRows = rows.Take(options.TrainSetSize).ToList();
                cursor = initialTrainingRows.Count;
                await TrainRowsAsync(
                    machine,
                    initialTrainingRows,
                    initialTrainingRows,
                    "tatoeba-pilot-source-rollover-initial-train",
                    ct);
                trainingRows = initialTrainingRows;
                acceptedTrainingRefs = initialTrainingRows.Select(ToPairRef).ToList();
                skippedRowRefs = new List<TatoebaPilotPairRefCheckpoint>();

                if (absoluteCheckpointPath is not null)
                {
                    if (await SaveLoopCheckpointAsync(
                        machine,
                        options,
                        absoluteCheckpointPath,
                        lastCompletedCycle,
                        cursor,
                        acceptedTrainingRefs,
                        skippedRowRefs,
                        bestObservedRuntimeProductAccuracy,
                        ct))
                    {
                        checkpointSaved = true;
                    }
                }
            }
            else
            {
                resumeMode = "checkpoint";
                initialTrainingSkippedDueCheckpoint = true;
                ValidateLoopStateMatchesOptions(loopState, options, rows.Count);
                cursor = loopState.Cursor;
                lastCompletedCycle = loopState.LastCompletedCycle;
                acceptedTrainingRefs = loopState.AcceptedTrainingRows.ToList();
                skippedRowRefs = loopState.SkippedRows.ToList();
                trainingRows = ResolvePairRefs(rows, acceptedTrainingRefs, "accepted training");
            }
        }
        else
        {
            if (options.StartNewSourceFromCheckpoint)
            {
                throw new InvalidOperationException(
                    "Source rollover requires an existing checkpoint. Pass --checkpoint with --require-checkpoint-resume.");
            }

            // No checkpoint configured or found: start fresh.
            machine = new TranslationMachine();
            // Первый Train тоже должен сохранить своего кандидата при позднем отказе.
            if (candidateDirectory is not null)
                machine.UseLostCandidateStore(new TranslationFileLostCandidateStore(candidateDirectory));
            var initialTrainingRows = rows.Take(options.TrainSetSize).ToList();
            cursor = initialTrainingRows.Count;
            await TrainRowsAsync(machine, initialTrainingRows, initialTrainingRows, "tatoeba-pilot-initial-train", ct);
            trainingRows = initialTrainingRows;
            acceptedTrainingRefs = initialTrainingRows.Select(ToPairRef).ToList();
            skippedRowRefs = new List<TatoebaPilotPairRefCheckpoint>();

            if (absoluteCheckpointPath is not null)
            {
                if (await SaveLoopCheckpointAsync(
                    machine,
                    options,
                    absoluteCheckpointPath,
                    lastCompletedCycle,
                    cursor,
                    acceptedTrainingRefs,
                    skippedRowRefs,
                    bestObservedRuntimeProductAccuracy,
                    ct))
                {
                    checkpointSaved = true;
                }
            }
        }

        if (!checkpointLoaded && options.SkipCompletedCycles > 0)
        {
            for (var replayIndex = 0; replayIndex < options.SkipCompletedCycles; replayIndex++)
            {
                if (trainingRows.Count < options.TrainSetSize || cursor >= rows.Count)
                    break;

                var promotionThresholds = PromotionThresholdsFromMaxAccuracy(bestObservedRuntimeProductAccuracy);
                var replaySelection = SelectPromotableTestRows(
                    rows,
                    cursor,
                    options.TestSetSize,
                    trainingRows,
                    checkpointExactSourceMemory,
                    promotionThresholds);
                cursor += replaySelection.CandidateRowsScanned;
                if (replaySelection.NoveltySkippedRows.Count > 0)
                    skippedRowRefs.AddRange(replaySelection.NoveltySkippedRows.Select(ToPairRef));
                if (replaySelection.Rows.Count == 0)
                    break;

                var cycleNumber = lastCompletedCycle + 1;
                var promoteToTraining = ShouldPromoteToTraining(replaySelection, promotionThresholds);
                if (promoteToTraining)
                {
                    var cumulativeForLexicon = trainingRows.Concat(replaySelection.Rows).ToArray();
                    await TrainRowsAsync(
                        machine,
                        replaySelection.Rows,
                        cumulativeForLexicon,
                        $"tatoeba-pilot-cycle-{cycleNumber:D2}-resume-promote-test",
                        ct);
                    trainingRows.AddRange(replaySelection.Rows);
                    acceptedTrainingRefs.AddRange(replaySelection.Rows.Select(ToPairRef));
                }
                else
                {
                    skippedRowRefs.AddRange(replaySelection.Rows.Select(ToPairRef));
                }

                lastCompletedCycle = cycleNumber;
                replayedCycles++;

                if (absoluteCheckpointPath is not null)
                {
                    if (await SaveLoopCheckpointAsync(
                        machine,
                        options,
                        absoluteCheckpointPath,
                        lastCompletedCycle,
                        cursor,
                        acceptedTrainingRefs,
                        skippedRowRefs,
                        bestObservedRuntimeProductAccuracy,
                        ct))
                    {
                        checkpointSaved = true;
                    }
                }
            }
        }

        var startCursor = cursor;
        var startCycle = lastCompletedCycle + 1;

        for (var visibleCycle = 0; visibleCycle < options.MaxCycles; visibleCycle++)
        {
            if (trainingRows.Count < options.TrainSetSize || cursor >= rows.Count)
                break;

            var promotionThresholds = PromotionThresholdsFromMaxAccuracy(bestObservedRuntimeProductAccuracy);
            var testSelection = SelectPromotableTestRows(
                rows,
                cursor,
                options.TestSetSize,
                trainingRows,
                checkpointExactSourceMemory,
                promotionThresholds);
            cursor += testSelection.CandidateRowsScanned;
            if (testSelection.NoveltySkippedRows.Count > 0)
                skippedRowRefs.AddRange(testSelection.NoveltySkippedRows.Select(ToPairRef));
            if (testSelection.Rows.Count == 0)
            {
                if (absoluteCheckpointPath is not null)
                {
                    if (await SaveLoopCheckpointAsync(
                        machine,
                        options,
                        absoluteCheckpointPath,
                        lastCompletedCycle,
                        cursor,
                        acceptedTrainingRefs,
                        skippedRowRefs,
                        bestObservedRuntimeProductAccuracy,
                        ct))
                    {
                        checkpointSaved = true;
                    }
                }

                break;
            }

            var cycleNumber = lastCompletedCycle + 1;
            var promoteToTraining = ShouldPromoteToTraining(testSelection, promotionThresholds);

            var report = await RunCycleAsync(
                machine,
                cycleNumber,
                trainingRows.ToArray(),
                testSelection,
                options,
                promoteToTraining,
                promotionThresholds,
                PromotionDecision(testSelection, promoteToTraining, promotionThresholds),
                ct);
            cycles.Add(report);
            bestObservedRuntimeProductAccuracy = Math.Max(bestObservedRuntimeProductAccuracy, report.RuntimeProductAccuracy);
            onCycleCompleted?.Invoke(report);
            lastCompletedCycle = cycleNumber;

            if (report.PassesTarget)
            {
                skippedRowRefs.AddRange(testSelection.Rows.Select(ToPairRef));
                if (absoluteCheckpointPath is not null)
                {
                    if (await SaveLoopCheckpointAsync(
                        machine,
                        options,
                        absoluteCheckpointPath,
                        lastCompletedCycle,
                        cursor,
                        acceptedTrainingRefs,
                        skippedRowRefs,
                        bestObservedRuntimeProductAccuracy,
                        ct))
                    {
                        checkpointSaved = true;
                    }
                }

                break;
            }

            if (promoteToTraining)
            {
                var cumulativeForLexicon = trainingRows.Concat(testSelection.Rows).ToArray();
                await TrainRowsAsync(
                    machine,
                    testSelection.Rows,
                    cumulativeForLexicon,
                    $"tatoeba-pilot-cycle-{cycleNumber:D2}-promote-test",
                    ct);
                trainingRows.AddRange(testSelection.Rows);
                acceptedTrainingRefs.AddRange(testSelection.Rows.Select(ToPairRef));
            }
            else
            {
                skippedRowRefs.AddRange(testSelection.Rows.Select(ToPairRef));
            }

            if (absoluteCheckpointPath is not null)
            {
                if (await SaveLoopCheckpointAsync(
                    machine,
                    options,
                    absoluteCheckpointPath,
                    lastCompletedCycle,
                    cursor,
                    acceptedTrainingRefs,
                    skippedRowRefs,
                    bestObservedRuntimeProductAccuracy,
                    ct))
                {
                    checkpointSaved = true;
                }
            }
        }

        // Save final checkpoint at end of loop run (catches cases where no cycle promoted).
        if (absoluteCheckpointPath is not null)
        {
            if (await SaveLoopCheckpointAsync(
                machine,
                options,
                absoluteCheckpointPath,
                lastCompletedCycle,
                cursor,
                acceptedTrainingRefs,
                skippedRowRefs,
                bestObservedRuntimeProductAccuracy,
                ct))
            {
                checkpointSaved = true;
            }
        }

        return new TatoebaPilotLoopReport(
            PrivatePairsPath: options.PrivatePairsPath,
            TotalPairs: rows.Count,
            TrainSetSize: options.TrainSetSize,
            TestSetSize: options.TestSetSize,
            SkipCompletedCycles: options.SkipCompletedCycles,
            TargetAccuracy: options.TargetAccuracy,
            BestRuntimeProductAccuracy: bestObservedRuntimeProductAccuracy,
            CheckpointLoaded: checkpointLoaded,
            CheckpointSaved: checkpointSaved,
            ResumeMode: resumeMode,
            ReplayedCycles: replayedCycles,
            InitialTrainingSkippedDueCheckpoint: initialTrainingSkippedDueCheckpoint,
            SourceRolloverFrom: sourceRolloverFrom,
            SourceRolloverTo: sourceRolloverTo,
            SourceRolloverStartCursor: sourceRolloverStartCursor,
            StartCursor: startCursor,
            EndCursor: cursor,
            StartCycle: startCycle,
            LastCompletedCycle: lastCompletedCycle,
            AcceptedTrainingRows: acceptedTrainingRefs.Count,
            SkippedRows: skippedRowRefs.Count,
            ReachedTarget: cycles.Any(cycle => cycle.PassesTarget),
            Cycles: cycles);
    }

    public static async Task WriteReportAsync(
        TatoebaPilotLoopReport report,
        string path,
        CancellationToken ct = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(report, new JsonSerializerOptions(JsonOptions) { WriteIndented = true }) + Environment.NewLine,
            Encoding.UTF8,
            ct);
    }

    private static TatoebaPilotLoopCheckpoint RequireLoopState(
        TatoebaPilotLoopCheckpoint? loopState,
        string checkpointPath)
        => loopState ?? throw new InvalidOperationException(
            $"Checkpoint does not contain Tatoeba loop state: {checkpointPath}");

    private static void ValidateLoopStateMatchesOptions(
        TatoebaPilotLoopCheckpoint loopState,
        TatoebaPilotLoopRunOptions options,
        int totalRows)
    {
        var checkpointPairsPath = Path.GetFullPath(loopState.PrivatePairsPath);
        var currentPairsPath = Path.GetFullPath(options.PrivatePairsPath);
        if (!string.Equals(checkpointPairsPath, currentPairsPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Checkpoint Tatoeba loop PrivatePairsPath mismatch. Checkpoint='{checkpointPairsPath}', Current='{currentPairsPath}'.");
        }

        if (loopState.TrainSetSize != options.TrainSetSize)
        {
            throw new InvalidOperationException(
                $"Checkpoint Tatoeba loop TrainSetSize mismatch. Checkpoint={loopState.TrainSetSize}, Current={options.TrainSetSize}.");
        }

        if (loopState.TestSetSize != options.TestSetSize)
        {
            throw new InvalidOperationException(
                $"Checkpoint Tatoeba loop TestSetSize mismatch. Checkpoint={loopState.TestSetSize}, Current={options.TestSetSize}.");
        }

        if (loopState.Cursor < 0 || loopState.Cursor > totalRows)
        {
            throw new InvalidOperationException(
                $"Checkpoint Tatoeba loop Cursor is outside the current pairs file. Cursor={loopState.Cursor}, TotalRows={totalRows}.");
        }

        if (loopState.LastCompletedCycle < 0)
        {
            throw new InvalidOperationException(
                $"Checkpoint Tatoeba loop LastCompletedCycle must not be negative. LastCompletedCycle={loopState.LastCompletedCycle}.");
        }

        if (loopState.AcceptedTrainingRows is null || loopState.SkippedRows is null)
            throw new InvalidOperationException("Checkpoint Tatoeba loop history is missing accepted or skipped row references.");

        if (loopState.TrainRows != loopState.AcceptedTrainingRows.Count)
        {
            throw new InvalidOperationException(
                $"Checkpoint Tatoeba loop TrainRows does not match AcceptedTrainingRows. TrainRows={loopState.TrainRows}, AcceptedTrainingRows={loopState.AcceptedTrainingRows.Count}.");
        }
    }

    private static void ValidateLoopStateCanRollover(
        TatoebaPilotLoopCheckpoint loopState,
        TatoebaPilotLoopRunOptions options)
    {
        var checkpointPairsPath = Path.GetFullPath(loopState.PrivatePairsPath);
        var currentPairsPath = Path.GetFullPath(options.PrivatePairsPath);
        if (string.Equals(checkpointPairsPath, currentPairsPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Source rollover requires a different --private-pairs file. Use normal checkpoint resume for the same source.");
        }

        if (loopState.TrainSetSize != options.TrainSetSize)
        {
            throw new InvalidOperationException(
                $"Checkpoint Tatoeba loop TrainSetSize mismatch. Checkpoint={loopState.TrainSetSize}, Current={options.TrainSetSize}.");
        }

        if (loopState.TestSetSize != options.TestSetSize)
        {
            throw new InvalidOperationException(
                $"Checkpoint Tatoeba loop TestSetSize mismatch. Checkpoint={loopState.TestSetSize}, Current={options.TestSetSize}.");
        }
    }

    private static List<TatoebaPrivateSentencePair> ResolvePairRefs(
        IReadOnlyList<TatoebaPrivateSentencePair> rows,
        IReadOnlyList<TatoebaPilotPairRefCheckpoint> refs,
        string historyName)
    {
        var byRef = rows
            .GroupBy(row => PairRefKey(ToPairRef(row)), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var resolved = new List<TatoebaPrivateSentencePair>(refs.Count);

        foreach (var pairRef in refs)
        {
            var key = PairRefKey(pairRef);
            if (!byRef.TryGetValue(key, out var row))
            {
                throw new InvalidOperationException(
                    $"Checkpoint {historyName} row not found in current private pairs file: {key}.");
            }

            resolved.Add(row);
        }

        return resolved;
    }

    private static async Task<bool> SaveLoopCheckpointAsync(
        TranslationMachine machine,
        TatoebaPilotLoopRunOptions options,
        string absoluteCheckpointPath,
        int lastCompletedCycle,
        int cursor,
        IReadOnlyList<TatoebaPilotPairRefCheckpoint> acceptedTrainingRefs,
        IReadOnlyList<TatoebaPilotPairRefCheckpoint> skippedRowRefs,
        double bestRuntimeProductAccuracy,
        CancellationToken ct)
    {
        var loopState = new TatoebaPilotLoopCheckpoint(
            PrivatePairsPath: Path.GetFullPath(options.PrivatePairsPath),
            TrainSetSize: options.TrainSetSize,
            TestSetSize: options.TestSetSize,
            LastCompletedCycle: lastCompletedCycle,
            Cursor: cursor,
            TrainRows: acceptedTrainingRefs.Count,
            BestRuntimeProductAccuracy: bestRuntimeProductAccuracy,
            AcceptedTrainingRows: acceptedTrainingRefs.ToArray(),
            SkippedRows: skippedRowRefs.ToArray());
        var saveResult = await machine.SaveCheckpointAsync(absoluteCheckpointPath, loopState, ct);
        if (!saveResult.IsSuccess)
            throw new InvalidOperationException($"Failed to save checkpoint: {saveResult.ErrorMessage} ({saveResult.ErrorCode})");

        options.Cache?.Invalidate(absoluteCheckpointPath);
        options.Cache?.Store(absoluteCheckpointPath, new TranslationMachineLoadResult(machine, loopState));
        return true;
    }

    private static TatoebaPilotPairRefCheckpoint ToPairRef(TatoebaPrivateSentencePair row)
        => new(
            Direction: row.Direction,
            SourceSentenceId: row.SourceSentenceId,
            TargetSentenceId: row.TargetSentenceId,
            SourcePattern: row.SourcePattern);

    private static string PairRefKey(TatoebaPilotPairRefCheckpoint pairRef)
        => string.Join(
            "|",
            pairRef.Direction.ToUpperInvariant(),
            pairRef.SourceSentenceId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            pairRef.TargetSentenceId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            pairRef.SourcePattern);

    private static async Task<TatoebaPilotLoopCycleReport> RunCycleAsync(
        TranslationMachine machine,
        int cycle,
        IReadOnlyList<TatoebaPrivateSentencePair> training,
        TatoebaTestSelection testSelection,
        TatoebaPilotLoopRunOptions options,
        bool promotedToTraining,
        PromotionThresholds promotionThresholds,
        string promotionDecision,
        CancellationToken ct)
    {
        var orchestrator = await machine.GetOrchestratorAsync(ct);
        if (!orchestrator.IsSuccess)
            throw new InvalidOperationException(orchestrator.ErrorMessage);

        var rowResults = new List<TatoebaPilotLoopRowResult>();
        foreach (var row in testSelection.Rows)
        {
            var actual = await TranslateAsync(orchestrator.Value!, row, ct);
            rowResults.Add(BuildRowResult(row, actual));
        }

        var exact = Ratio(rowResults, row => row.ExactMatch);
        var product = Ratio(rowResults, row => row.ProductMatch);
        var bootstrap = Ratio(rowResults, row => row.BootstrapFallback);
        var script = Ratio(rowResults, row => row.TargetScriptOk);
        var copied = Ratio(rowResults, row => row.CopiedSource);
        var avgSimilarity = rowResults.Count == 0 ? 0.0 : rowResults.Average(row => row.TokenSimilarity);

        return new TatoebaPilotLoopCycleReport(
            Cycle: cycle,
            TrainRows: training.Count,
            TestRows: testSelection.Rows.Count,
            CandidateTestRowsScanned: testSelection.CandidateRowsScanned,
            NoveltyScanSkippedRows: testSelection.NoveltyScanSkippedRows,
            CandidateSelectionMode: testSelection.CandidateSelectionMode,
            FilteredExactOverlapRows: testSelection.FilteredExactOverlapRows,
            FilteredCheckpointMemoryOverlapRows: testSelection.FilteredCheckpointMemoryOverlapRows,
            FilteredSimilarityOverlapRows: testSelection.FilteredSimilarityOverlapRows,
            TestSelectionShortfall: testSelection.Shortfall,
            NewPatternRate: testSelection.NewPatternRate,
            SourceTokenSeenRate: testSelection.SourceTokenSeenRate,
            RuntimeProductAccuracy: product,
            RuntimeExactAccuracy: exact,
            RuntimeBootstrapRate: bootstrap,
            TargetScriptAccuracy: script,
            CopiedSourceRate: copied,
            AverageTokenSimilarity: avgSimilarity,
            PassesTarget: testSelection.Rows.Count == options.TestSetSize &&
                product >= options.TargetAccuracy &&
                bootstrap <= 0.15 &&
                copied <= 0.05,
            PromotedToTraining: promotedToTraining,
            PromotionMaxAccuracy: promotionThresholds.MaxAccuracy,
            MinimumPromotionNewPatternRate: promotionThresholds.MinimumNewPatternRate,
            MaximumPromotionSeenTokenRate: promotionThresholds.MaximumSeenTokenRate,
            PromotionDecision: promotionDecision,
            Rows: rowResults);
    }

    private static async Task TrainRowsAsync(
        TranslationMachine machine,
        IReadOnlyList<TatoebaPrivateSentencePair> rows,
        IReadOnlyList<TatoebaPrivateSentencePair> lexiconRows,
        string batchId,
        CancellationToken ct)
    {
        if (rows.Count == 0)
            return;

        var lexicon = InduceLexicon(lexiconRows);
        var phraseMappings = InducePhraseMappings(lexiconRows);
        var trainReady = rows
            .Select((row, index) => ToTrainReadyPair(row, index + 1, lexicon, phraseMappings))
            .ToArray();
        var trainBatch = TranslationStructuredBatchBuilder.BuildGeneralBatch(
            batchId,
            trainReady,
            heldOut: false,
            signatureMode: TranslationSignatureMode.Structural);

        var trainResult = await machine.TrainStructuredBatchAsync(trainBatch, ct);
        if (!trainResult.IsSuccess)
            throw new InvalidOperationException(trainResult.ErrorMessage);
    }

    private static bool ShouldPromoteToTraining(TatoebaTestSelection selection, PromotionThresholds thresholds)
    {
        if (selection.Rows.Count != selection.RequestedRows)
            return false;

        return selection.NewPatternRate > thresholds.MinimumNewPatternRate &&
            selection.SourceTokenSeenRate < thresholds.MaximumSeenTokenRate;
    }

    private static string PromotionDecision(
        TatoebaTestSelection selection,
        bool promoteToTraining,
        PromotionThresholds thresholds)
    {
        if (selection.Rows.Count < selection.RequestedRows)
            return "SKIP_SHORTFALL";

        var thresholdText = PromotionThresholdText(thresholds);
        if (promoteToTraining)
            return $"PROMOTE_{thresholdText}";
        return $"SKIP_REQUIRES_{thresholdText}";
    }

    private static PromotionThresholds PromotionThresholdsFromMaxAccuracy(double maxAccuracy)
    {
        var normalizedMaxAccuracy = Math.Clamp(maxAccuracy, 0.0, 1.0);
        return new PromotionThresholds(
            MaxAccuracy: normalizedMaxAccuracy,
            MinimumNewPatternRate: 1.0 - normalizedMaxAccuracy,
            MaximumSeenTokenRate: normalizedMaxAccuracy);
    }

    private static string PromotionThresholdText(PromotionThresholds thresholds)
        => $"NEW_PATTERN_GT_{FormatThresholdPercent(thresholds.MinimumNewPatternRate)}_AND_SEEN_TOKENS_LT_{FormatThresholdPercent(thresholds.MaximumSeenTokenRate)}";

    private static string FormatThresholdPercent(double value)
        => Math.Round(value * 100.0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture);

    private static TatoebaTestSelection SelectUniqueTestRows(
        IReadOnlyList<TatoebaPrivateSentencePair> rows,
        int startIndex,
        int testSetSize,
        IReadOnlyList<TatoebaPrivateSentencePair> training,
        IReadOnlyDictionary<string, HashSet<string>> checkpointExactSourceMemory)
    {
        var trainingHistory = BuildTrainingHistory(training);
        var exactHistory = new HashSet<string>(trainingHistory.ExactSourceKeys, StringComparer.Ordinal);
        var tokenHistory = trainingHistory.SourceTokenSets
            .Select(tokens => tokens.ToHashSet(StringComparer.OrdinalIgnoreCase))
            .ToList();
        var selected = new List<TatoebaPrivateSentencePair>();
        var candidateRowsScanned = 0;
        var filteredExact = 0;
        var filteredCheckpointMemory = 0;
        var filteredSimilarity = 0;

        for (var index = startIndex; index < rows.Count && selected.Count < testSetSize; index++)
        {
            var candidate = rows[index];
            candidateRowsScanned++;

            var sourceKey = NormalizeSourceKey(candidate.SourceText);
            if (exactHistory.Contains(sourceKey))
            {
                filteredExact++;
                continue;
            }

            if (IsKnownByCheckpointExactSourceMemory(candidate, checkpointExactSourceMemory))
            {
                filteredCheckpointMemory++;
                continue;
            }

            var sourceTokens = ContentWords(candidate.SourceText, SourceLanguage(candidate.Direction))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (IsTooSimilarToHistory(sourceTokens, tokenHistory))
            {
                filteredSimilarity++;
                continue;
            }

            selected.Add(candidate);
            exactHistory.Add(sourceKey);
            tokenHistory.Add(sourceTokens);
        }

        var newPatternRate = selected.Count == 0
            ? 0.0
            : selected.Count(row => !trainingHistory.SourcePatterns.Contains(row.SourcePattern)) / (double)selected.Count;
        var selectedSourceTokens = selected
            .SelectMany(row => ContentWords(row.SourceText, SourceLanguage(row.Direction)))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seenTokens = selectedSourceTokens
            .Intersect(trainingHistory.SourceTokens, StringComparer.OrdinalIgnoreCase)
            .Count();
        var sourceTokenSeenRate = selectedSourceTokens.Count == 0
            ? 0.0
            : seenTokens / (double)selectedSourceTokens.Count;

        return new TatoebaTestSelection(
            Rows: selected,
            RequestedRows: testSetSize,
            CandidateRowsScanned: candidateRowsScanned,
            FilteredExactOverlapRows: filteredExact,
            FilteredCheckpointMemoryOverlapRows: filteredCheckpointMemory,
            FilteredSimilarityOverlapRows: filteredSimilarity,
            Shortfall: Math.Max(0, testSetSize - selected.Count),
            NewPatternRate: newPatternRate,
            SourceTokenSeenRate: sourceTokenSeenRate,
            NoveltySkippedRows: Array.Empty<TatoebaPrivateSentencePair>(),
            NoveltyScanSkippedRows: 0,
            CandidateSelectionMode: "sequential_candidate");
    }

    private static TatoebaTestSelection SelectPromotableTestRows(
        IReadOnlyList<TatoebaPrivateSentencePair> rows,
        int startIndex,
        int testSetSize,
        IReadOnlyList<TatoebaPrivateSentencePair> training,
        IReadOnlyDictionary<string, HashSet<string>> checkpointExactSourceMemory,
        PromotionThresholds promotionThresholds)
    {
        var cursor = startIndex;
        var candidateRowsScanned = 0;
        var filteredExact = 0;
        var filteredCheckpointMemory = 0;
        var filteredSimilarity = 0;
        var skippedNoveltyRows = new List<TatoebaPrivateSentencePair>();
        TatoebaTestSelection? pendingRejectedSelection = null;

        while (cursor < rows.Count)
        {
            if (pendingRejectedSelection is not null)
            {
                skippedNoveltyRows.AddRange(pendingRejectedSelection.Rows);
                pendingRejectedSelection = null;
            }

            var selection = SelectUniqueTestRows(
                rows,
                cursor,
                testSetSize,
                training,
                checkpointExactSourceMemory);
            candidateRowsScanned += selection.CandidateRowsScanned;
            filteredExact += selection.FilteredExactOverlapRows;
            filteredCheckpointMemory += selection.FilteredCheckpointMemoryOverlapRows;
            filteredSimilarity += selection.FilteredSimilarityOverlapRows;

            if (selection.Rows.Count < testSetSize)
            {
                return WithAdaptiveScanTotals(
                    selection,
                    candidateRowsScanned,
                    filteredExact,
                    filteredCheckpointMemory,
                    filteredSimilarity,
                    skippedNoveltyRows,
                    skippedNoveltyRows.Count == 0
                        ? "sequential_shortfall"
                        : "adaptive_scan_shortfall");
            }

            if (ShouldPromoteToTraining(selection, promotionThresholds))
            {
                return WithAdaptiveScanTotals(
                    selection,
                    candidateRowsScanned,
                    filteredExact,
                    filteredCheckpointMemory,
                    filteredSimilarity,
                    skippedNoveltyRows,
                    skippedNoveltyRows.Count == 0
                        ? "adaptive_promotable_first_candidate"
                        : "adaptive_promotable_scan");
            }

            pendingRejectedSelection = selection;
            cursor += selection.CandidateRowsScanned;
        }

        if (pendingRejectedSelection is not null)
        {
            return WithAdaptiveScanTotals(
                pendingRejectedSelection,
                candidateRowsScanned,
                filteredExact,
                filteredCheckpointMemory,
                filteredSimilarity,
                skippedNoveltyRows,
                skippedNoveltyRows.Count == 0
                    ? "adaptive_scan_no_promotable"
                    : "adaptive_scan_no_promotable_after_skips");
        }

        return new TatoebaTestSelection(
            Rows: Array.Empty<TatoebaPrivateSentencePair>(),
            RequestedRows: testSetSize,
            CandidateRowsScanned: candidateRowsScanned,
            FilteredExactOverlapRows: filteredExact,
            FilteredCheckpointMemoryOverlapRows: filteredCheckpointMemory,
            FilteredSimilarityOverlapRows: filteredSimilarity,
            Shortfall: testSetSize,
            NewPatternRate: 0.0,
            SourceTokenSeenRate: 0.0,
            NoveltySkippedRows: skippedNoveltyRows.ToArray(),
            NoveltyScanSkippedRows: skippedNoveltyRows.Count,
            CandidateSelectionMode: "adaptive_scan_exhausted");
    }

    private static TatoebaTestSelection WithAdaptiveScanTotals(
        TatoebaTestSelection selection,
        int candidateRowsScanned,
        int filteredExact,
        int filteredCheckpointMemory,
        int filteredSimilarity,
        IReadOnlyList<TatoebaPrivateSentencePair> skippedNoveltyRows,
        string candidateSelectionMode)
        => selection with
        {
            CandidateRowsScanned = candidateRowsScanned,
            FilteredExactOverlapRows = filteredExact,
            FilteredCheckpointMemoryOverlapRows = filteredCheckpointMemory,
            FilteredSimilarityOverlapRows = filteredSimilarity,
            NoveltySkippedRows = skippedNoveltyRows.ToArray(),
            NoveltyScanSkippedRows = skippedNoveltyRows.Count,
            CandidateSelectionMode = candidateSelectionMode
        };

    private static IReadOnlyDictionary<string, HashSet<string>> BuildCheckpointExactSourceMemory(
        TranslationMachine machine)
    {
        var exportResult = machine.ExportCheckpoint();
        if (!exportResult.IsSuccess)
            throw new InvalidOperationException(
                $"Failed to export checkpoint memory: {exportResult.ErrorMessage} ({exportResult.ErrorCode})");

        var memory = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var executor in exportResult.Value!.Executors)
        {
            var direction = executor.Direction.ToString().ToUpperInvariant();
            if (!memory.TryGetValue(direction, out var exactKeys))
            {
                exactKeys = new HashSet<string>(StringComparer.Ordinal);
                memory[direction] = exactKeys;
            }

            // Legacy saved content addressed the exact memory by source text alone. It is still
            // read here so an old checkpoint keeps its known-source set.
            foreach (var entry in executor.ExactSourceMap)
            {
                if (!string.IsNullOrWhiteSpace(entry.Key))
                    exactKeys.Add(entry.Key);
            }

            // Current content addresses the exact memory by the pair (topic, source). This loop
            // collects the source half of every cell, because this set answers only "is this
            // source text already known at all" for candidate de-duplication; without it the
            // pilot loop would treat every already-learned sentence as new after the exact
            // memory moved to its topic-aware address.
            foreach (var entry in executor.TopicSourceMap ?? [])
            {
                if (!string.IsNullOrWhiteSpace(entry.Source))
                    exactKeys.Add(entry.Source);
            }
        }

        return memory;
    }

    private static bool IsKnownByCheckpointExactSourceMemory(
        TatoebaPrivateSentencePair candidate,
        IReadOnlyDictionary<string, HashSet<string>> checkpointExactSourceMemory)
    {
        if (!checkpointExactSourceMemory.TryGetValue(candidate.Direction.ToUpperInvariant(), out var exactKeys))
            return false;

        var checkpointSourceKey = NormalizeCheckpointSourceKey(candidate.SourceText);
        return !string.IsNullOrWhiteSpace(checkpointSourceKey) && exactKeys.Contains(checkpointSourceKey);
    }

    private static TatoebaTrainingHistory BuildTrainingHistory(IReadOnlyList<TatoebaPrivateSentencePair> training)
    {
        var exactSourceKeys = new HashSet<string>(StringComparer.Ordinal);
        var sourceTokenSets = new List<HashSet<string>>();
        var sourcePatterns = new HashSet<string>(StringComparer.Ordinal);
        var sourceTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in training)
        {
            exactSourceKeys.Add(NormalizeSourceKey(row.SourceText));
            var tokens = ContentWords(row.SourceText, SourceLanguage(row.Direction))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            sourceTokenSets.Add(tokens);
            sourcePatterns.Add(row.SourcePattern);
            foreach (var token in tokens)
                sourceTokens.Add(token);
        }

        return new TatoebaTrainingHistory(
            ExactSourceKeys: exactSourceKeys,
            SourceTokenSets: sourceTokenSets,
            SourcePatterns: sourcePatterns,
            SourceTokens: sourceTokens);
    }

    private static bool IsTooSimilarToHistory(
        HashSet<string> sourceTokens,
        IReadOnlyList<HashSet<string>> tokenHistory)
    {
        if (sourceTokens.Count == 0)
            return false;

        foreach (var historyTokens in tokenHistory)
        {
            if (Jaccard(sourceTokens, historyTokens) >= TrainingHistorySimilarityThreshold)
                return true;
        }

        return false;
    }

    private static double Jaccard(HashSet<string> left, HashSet<string> right)
    {
        if (left.Count == 0 && right.Count == 0)
            return 1.0;
        if (left.Count == 0 || right.Count == 0)
            return 0.0;

        var intersection = left.Intersect(right, StringComparer.OrdinalIgnoreCase).Count();
        var union = left.Union(right, StringComparer.OrdinalIgnoreCase).Count();
        return union == 0 ? 0.0 : intersection / (double)union;
    }

    private static string NormalizeSourceKey(string value)
        => string.Join(
            " ",
            WordRegex.Matches(value ?? string.Empty)
                .Select(match => match.Value.Trim().ToLowerInvariant())
                .Where(token => token.Length > 0));

    private static string NormalizeCheckpointSourceKey(string value)
        => string.Join(" ", new string((value ?? string.Empty).Trim()
                .ToLowerInvariant()
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '$' || char.IsWhiteSpace(ch))
                .ToArray())
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static async Task<IReadOnlyList<TatoebaPrivateSentencePair>> ReadPrivatePairsAsync(
        string path,
        CancellationToken ct)
    {
        var rows = new List<TatoebaPrivateSentencePair>();
        await foreach (var line in File.ReadLinesAsync(path, ct))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var row = JsonSerializer.Deserialize<TatoebaPrivateSentencePair>(line, JsonOptions);
            if (row is not null)
                rows.Add(row);
        }

        return rows;
    }

    private static TranslationTrainReadyPair ToTrainReadyPair(
        TatoebaPrivateSentencePair row,
        int ordinal,
        IReadOnlyDictionary<string, string> lexicon,
        IReadOnlyList<InducedPhraseMapping> phraseMappings)
    {
        var sourceWords = ContentWords(row.SourceText, SourceLanguage(row.Direction));
        var mappings = sourceWords
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(lexicon.ContainsKey)
            .Take(MaxLexiconMappingsPerRow)
            .Select(word => $"[AGREEMENT: {word} => {lexicon[word]}]");
        var phraseTags = phraseMappings
            .Where(mapping => SourceContainsPhrase(row.SourceText, mapping.SourcePhrase))
            .OrderByDescending(mapping => mapping.SourceTokenCount)
            .ThenByDescending(mapping => mapping.Score)
            .Take(MaxPhraseMappingsPerRow)
            .Select(mapping => $"[PHRASE: {mapping.SourcePhrase} => {mapping.TargetPhrase}]");
        var rejected = string.Join(" ", mappings
            .Concat(phraseTags)
            .Append("[WRONG: tatoeba_pilot_boundary - plausible literal or copied output would miss the source-governed sentence mapping.]"));

        return new TranslationTrainReadyPair(
            Id: $"tatoeba-pilot-{row.Direction.ToLowerInvariant()}-{ordinal:D5}",
            Source: row.SourceId,
            Direction: row.Direction,
            PairFamily: "tatoeba-pilot-100-loop",
            Prompt: "Train source-governed Tatoeba pilot translation pair.",
            ASide: row.SourceText,
            Chosen: row.TargetText,
            Rejected: rejected,
            ValidationOutcome: "train_ready",
            ValidationErrors: []);
    }

    private static Dictionary<string, string> InduceLexicon(IReadOnlyList<TatoebaPrivateSentencePair> training)
    {
        var counts = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
        var sourceFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var sourceCapitalizedFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var targetFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in training)
        {
            var sourceWords = ContentWords(row.SourceText, SourceLanguage(row.Direction))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var targetWords = ContentWords(row.TargetText, TargetLanguage(row.Direction))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var source in sourceWords)
                sourceFrequency[source] = sourceFrequency.TryGetValue(source, out var sf) ? sf + 1 : 1;

            foreach (var source in CapitalizedContentWords(row.SourceText, SourceLanguage(row.Direction)))
                sourceCapitalizedFrequency[source] = sourceCapitalizedFrequency.TryGetValue(source, out var cf) ? cf + 1 : 1;

            foreach (var target in targetWords)
                targetFrequency[target] = targetFrequency.TryGetValue(target, out var tf) ? tf + 1 : 1;

            foreach (var source in sourceWords)
            {
                if (!counts.TryGetValue(source, out var targetCounts))
                {
                    targetCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    counts[source] = targetCounts;
                }

                foreach (var target in targetWords)
                    targetCounts[target] = targetCounts.TryGetValue(target, out var existing) ? existing + 1 : 1;
            }
        }

        var accepted = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (source, targetCounts) in counts)
        {
            var sourceRows = sourceFrequency.GetValueOrDefault(source);
            if (sourceRows < MinimumLexiconSupport)
                continue;
            if (sourceCapitalizedFrequency.GetValueOrDefault(source) == sourceRows)
                continue;

            var candidates = targetCounts
                .Select(candidate =>
                {
                    var targetRows = targetFrequency.GetValueOrDefault(candidate.Key, 1);
                    var confidence = candidate.Value / (double)Math.Max(1, sourceRows);
                    var specificity = candidate.Value / (double)Math.Max(1, targetRows);
                    var score = confidence * specificity;
                    return new LexiconCandidate(
                        Target: candidate.Key,
                        Support: candidate.Value,
                        Confidence: confidence,
                        Specificity: specificity,
                        Score: score);
                })
                .Where(candidate => candidate.Support >= MinimumLexiconSupport)
                .Where(candidate => candidate.Confidence >= MinimumLexiconConfidence)
                .Where(candidate => candidate.Specificity >= MinimumLexiconSpecificity)
                .Where(candidate => !IsLowInformationTarget(candidate.Target))
                .OrderByDescending(candidate => candidate.Score)
                .ThenByDescending(candidate => candidate.Support)
                .ThenBy(candidate => candidate.Target, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (candidates.Length == 0)
                continue;

            var best = candidates[0];
            var runnerUp = candidates.Skip(1).FirstOrDefault();
            if (runnerUp is not null &&
                best.Score - runnerUp.Score < MinimumLexiconScoreMargin &&
                best.Support <= runnerUp.Support + 1)
            {
                continue;
            }

            accepted[source] = best.Target;
        }

        return accepted;
    }

    private static IReadOnlyList<InducedPhraseMapping> InducePhraseMappings(
        IReadOnlyList<TatoebaPrivateSentencePair> training)
    {
        var sourcePhraseRows = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
        var targetPhraseRows = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
        var rowTargetPhrases = new List<HashSet<string>>(training.Count);

        for (var rowIndex = 0; rowIndex < training.Count; rowIndex++)
        {
            var row = training[rowIndex];
            var sourceTokens = PhraseTokens(row.SourceText);
            var targetTokens = PhraseTokens(row.TargetText);
            var rowTargetSet = BuildNgrams(targetTokens, 1, 4, allowFullSentence: true)
                .Where(IsPlausibleTargetPhrase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            rowTargetPhrases.Add(rowTargetSet);

            foreach (var targetPhrase in rowTargetSet)
                AddRow(targetPhraseRows, targetPhrase, rowIndex);

            foreach (var sourcePhrase in BuildNgrams(sourceTokens, 2, 4, allowFullSentence: false)
                         .Where(IsPlausibleSourcePhrase))
            {
                AddRow(sourcePhraseRows, sourcePhrase, rowIndex);
            }
        }

        var accepted = new List<InducedPhraseMapping>();
        foreach (var (sourcePhrase, sourceRows) in sourcePhraseRows)
        {
            if (sourceRows.Count < MinimumPhraseSupport)
                continue;

            var sourceTokenCount = sourcePhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var candidates = sourceRows
                .SelectMany(rowIndex => rowTargetPhrases[rowIndex])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(targetPhrase =>
                {
                    var support = sourceRows.Count(rowIndex => rowTargetPhrases[rowIndex].Contains(targetPhrase));
                    var globalRows = targetPhraseRows.GetValueOrDefault(targetPhrase)?.Count ?? support;
                    var coverage = support / (double)Math.Max(1, sourceRows.Count);
                    var specificity = support / (double)Math.Max(1, globalRows);
                    var targetTokenCount = targetPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    var lengthCloseness = 1.0 / (1.0 + Math.Abs(targetTokenCount - sourceTokenCount) * 0.25);
                    var score = coverage * specificity * lengthCloseness;
                    return new PhraseCandidate(
                        TargetPhrase: targetPhrase,
                        Support: support,
                        Coverage: coverage,
                        Specificity: specificity,
                        TargetTokenCount: targetTokenCount,
                        Score: score);
                })
                .Where(candidate => candidate.Support >= MinimumPhraseSupport)
                .Where(candidate => candidate.Coverage >= MinimumPhraseCoverage)
                .Where(candidate => candidate.Specificity >= MinimumPhraseSpecificity)
                .OrderByDescending(candidate => candidate.Score)
                .ThenByDescending(candidate => candidate.Support)
                .ThenByDescending(candidate => candidate.TargetTokenCount)
                .ThenBy(candidate => candidate.TargetPhrase, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (candidates.Length == 0)
                continue;

            var best = candidates[0];

            accepted.Add(new InducedPhraseMapping(
                SourcePhrase: sourcePhrase,
                TargetPhrase: best.TargetPhrase,
                SourceTokenCount: sourceTokenCount,
                Score: best.Score));
        }

        return accepted
            .OrderByDescending(mapping => mapping.Score)
            .ThenByDescending(mapping => mapping.SourceTokenCount)
            .ThenBy(mapping => mapping.SourcePhrase, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        static void AddRow(Dictionary<string, HashSet<int>> rowsByPhrase, string phrase, int rowIndex)
        {
            if (!rowsByPhrase.TryGetValue(phrase, out var rows))
            {
                rows = new HashSet<int>();
                rowsByPhrase[phrase] = rows;
            }

            rows.Add(rowIndex);
        }
    }

    private static bool IsLowInformationTarget(string target)
        => string.IsNullOrWhiteSpace(target) ||
           target.Length <= 1 ||
           EnglishStopWords.Contains(target) ||
           RussianStopWords.Contains(target) ||
           NegationWords.Contains(target);

    private static IReadOnlyList<string> PhraseTokens(string text)
        => WordRegex.Matches(text ?? string.Empty)
            .Select(match => match.Value.Trim().ToLowerInvariant())
            .Where(token => token.Length > 0)
            .ToArray();

    private static IEnumerable<string> BuildNgrams(
        IReadOnlyList<string> tokens,
        int minimumLength,
        int maximumLength,
        bool allowFullSentence)
    {
        for (var length = minimumLength; length <= maximumLength; length++)
        {
            if (tokens.Count < length)
                continue;

            if (!allowFullSentence && tokens.Count == length)
                continue;

            for (var start = 0; start + length <= tokens.Count; start++)
                yield return string.Join(" ", tokens.Skip(start).Take(length));
        }
    }

    private static bool IsPlausibleSourcePhrase(string phrase)
    {
        var tokens = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length is < 2 or > 4)
            return false;

        return tokens.Any(token => !EnglishStopWords.Contains(token) && !RussianStopWords.Contains(token));
    }

    private static bool IsPlausibleTargetPhrase(string phrase)
    {
        var tokens = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length is < 1 or > 4)
            return false;

        return tokens.Any(token => !IsLowInformationTarget(token));
    }

    private static bool SourceContainsPhrase(string sourceText, string sourcePhrase)
    {
        var sourceTokens = PhraseTokens(sourceText);
        var phraseTokens = sourcePhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (phraseTokens.Length == 0 || sourceTokens.Count < phraseTokens.Length)
            return false;

        for (var start = 0; start + phraseTokens.Length <= sourceTokens.Count; start++)
        {
            var matched = true;
            for (var offset = 0; offset < phraseTokens.Length; offset++)
            {
                if (!string.Equals(sourceTokens[start + offset], phraseTokens[offset], StringComparison.OrdinalIgnoreCase))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
                return true;
        }

        return false;
    }

    private static async Task<string> TranslateAsync(
        ILogicalOrchestrator orchestrator,
        TatoebaPrivateSentencePair row,
        CancellationToken ct)
    {
        var request = new LogicalOrchestratorRequest(
            RunId: $"tatoeba-pilot-{row.Direction.ToLowerInvariant()}-{row.SourceSentenceId}-{row.TargetSentenceId}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: row.SourceText,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = SourceLanguage(row.Direction),
                ["target_language"] = TargetLanguage(row.Direction),
                ["topic_domain"] = "general"
            });

        var result = await orchestrator.RunAsync(request, ct);
        return result.IsSuccess ? result.Value!.FinalHumanResult : result.ErrorMessage;
    }

    private static TatoebaPilotLoopRowResult BuildRowResult(TatoebaPrivateSentencePair row, string actual)
    {
        var bootstrap = actual.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase);
        var exact = string.Equals(actual, row.TargetText, StringComparison.Ordinal);
        var scriptOk = TargetScriptOk(actual, TargetLanguage(row.Direction));
        var copied = IsCopiedSource(row.SourceText, actual);
        var similarity = TokenSimilarity(row.TargetText, actual, TargetLanguage(row.Direction));
        var extraNegation = HasExtraNegationJunk(row.TargetText, actual);
        var product = scriptOk && !bootstrap && !copied && !extraNegation && (exact || similarity >= 0.50);

        return new TatoebaPilotLoopRowResult(
            SourceSentenceId: row.SourceSentenceId,
            TargetSentenceId: row.TargetSentenceId,
            Direction: row.Direction,
            SourceText: row.SourceText,
            ExpectedText: row.TargetText,
            ActualText: actual,
            ExactMatch: exact,
            ProductMatch: product,
            BootstrapFallback: bootstrap,
            TargetScriptOk: scriptOk,
            CopiedSource: copied,
            TokenSimilarity: similarity);
    }

    private static IReadOnlyList<string> ContentWords(string text, string language)
        => WordRegex.Matches(text ?? string.Empty)
            .Select(match => match.Value.Trim().ToLowerInvariant())
            .Where(word => word.Length >= 2)
            .Where(word => language == "en" ? !EnglishStopWords.Contains(word) : !RussianStopWords.Contains(word))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static IReadOnlyList<string> CapitalizedContentWords(string text, string language)
        => WordRegex.Matches(text ?? string.Empty)
            .Select(match => match.Value.Trim())
            .Where(word => word.Length >= 2 && char.IsUpper(word[0]) && word.Skip(1).Any(char.IsLower))
            .Select(word => word.ToLowerInvariant())
            .Where(word => language == "en" ? !EnglishStopWords.Contains(word) : !RussianStopWords.Contains(word))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static double TokenSimilarity(string expected, string actual, string language)
    {
        var expectedTokens = ContentWords(expected, language).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var actualTokens = ContentWords(actual, language).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (expectedTokens.Count == 0)
            return 0.0;

        return expectedTokens.Intersect(actualTokens, StringComparer.OrdinalIgnoreCase).Count() / (double)expectedTokens.Count;
    }

    private static bool HasExtraNegationJunk(string expected, string actual)
    {
        var expectedNegations = PhraseTokens(expected).Count(token => NegationWords.Contains(token));
        var actualNegations = PhraseTokens(actual).Count(token => NegationWords.Contains(token));
        return actualNegations > expectedNegations;
    }

    private static bool TargetScriptOk(string actual, string targetLanguage)
    {
        if (string.IsNullOrWhiteSpace(actual))
            return false;

        var hasCyrillic = actual.Any(ch => ch is >= '\u0400' and <= '\u04FF');
        var hasLatin = actual.Any(ch => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
        return targetLanguage == "ru"
            ? hasCyrillic && !hasLatin
            : hasLatin && !hasCyrillic;
    }

    private static bool IsCopiedSource(string source, string actual)
    {
        var sourceTokens = ContentWords(source, "mixed").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var actualTokens = ContentWords(actual, "mixed").ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (sourceTokens.Count == 0 || actualTokens.Count == 0)
            return false;

        var overlap = sourceTokens.Intersect(actualTokens, StringComparer.OrdinalIgnoreCase).Count();
        return overlap / (double)Math.Max(sourceTokens.Count, actualTokens.Count) >= 0.80;
    }

    private static string SourceLanguage(string direction)
        => direction.ToUpperInvariant() switch
        {
            "RU_EN" => "ru",
            "EN_RU" => "en",
            "HE_EN" => "he",
            "EN_HE" => "en",
            _ => "en"
        };

    private static string TargetLanguage(string direction)
        => direction.ToUpperInvariant() switch
        {
            "RU_EN" => "en",
            "EN_RU" => "ru",
            "HE_EN" => "en",
            "EN_HE" => "he",
            _ => "en"
        };

    private static double Ratio(IReadOnlyList<TatoebaPilotLoopRowResult> rows, Func<TatoebaPilotLoopRowResult, bool> predicate)
        => rows.Count == 0 ? 0.0 : rows.Count(predicate) / (double)rows.Count;

    private sealed record TatoebaTestSelection(
        IReadOnlyList<TatoebaPrivateSentencePair> Rows,
        int RequestedRows,
        int CandidateRowsScanned,
        int FilteredExactOverlapRows,
        int FilteredCheckpointMemoryOverlapRows,
        int FilteredSimilarityOverlapRows,
        int Shortfall,
        double NewPatternRate,
        double SourceTokenSeenRate,
        IReadOnlyList<TatoebaPrivateSentencePair> NoveltySkippedRows,
        int NoveltyScanSkippedRows,
        string CandidateSelectionMode);

    private sealed record PromotionThresholds(
        double MaxAccuracy,
        double MinimumNewPatternRate,
        double MaximumSeenTokenRate);

    private sealed record TatoebaTrainingHistory(
        HashSet<string> ExactSourceKeys,
        IReadOnlyList<HashSet<string>> SourceTokenSets,
        HashSet<string> SourcePatterns,
        HashSet<string> SourceTokens);

    private sealed record LexiconCandidate(
        string Target,
        int Support,
        double Confidence,
        double Specificity,
        double Score);

    private sealed record PhraseCandidate(
        string TargetPhrase,
        int Support,
        double Coverage,
        double Specificity,
        int TargetTokenCount,
        double Score);

    private sealed record InducedPhraseMapping(
        string SourcePhrase,
        string TargetPhrase,
        int SourceTokenCount,
        double Score);
}
