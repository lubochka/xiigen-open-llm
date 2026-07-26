// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DPO;
using DPO.Models;
using TensorCore.Core;
using TransformerModel;
using TransformerModel.Config;

public sealed class ProjectWikiDpoMetricRunner
{
    private const int MiniBatchSize = 5;
    private readonly int _epochCount;
    private readonly float _learningRate;
    private readonly int _vocabSize;
    private readonly int _contextLen;
    private readonly int _hiddenDim;
    private readonly int _numLayers;
    private readonly int _numHeads;
    private readonly int _numKvHeads;
    private readonly int _ffnHiddenDim;
    private readonly string? _initialPolicyCheckpointPath;
    private readonly string? _initialReferenceCheckpointPath;
    private readonly bool _deterministicFreshInitialization;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public ProjectWikiDpoMetricRunner(
        int epochCount = 5,
        float learningRate = 5e-4f,
        int vocabSize = 2048,
        int contextLen = 192,
        int hiddenDim = 64,
        int numLayers = 1,
        int numHeads = 4,
        int numKvHeads = 1,
        int ffnHiddenDim = 128,
        string? initialPolicyCheckpointPath = null,
        string? initialReferenceCheckpointPath = null,
        bool deterministicFreshInitialization = false)
    {
        _epochCount = Math.Max(1, epochCount);
        _learningRate = Math.Max(1e-6f, learningRate);
        _vocabSize = Math.Max(64, vocabSize);
        _contextLen = Math.Max(16, contextLen);
        _hiddenDim = Math.Max(16, hiddenDim);
        _numLayers = Math.Max(1, numLayers);
        _numHeads = Math.Max(1, numHeads);
        _numKvHeads = Math.Max(1, numKvHeads);
        _ffnHiddenDim = Math.Max(32, ffnHiddenDim);
        _initialPolicyCheckpointPath = string.IsNullOrWhiteSpace(initialPolicyCheckpointPath) ? null : initialPolicyCheckpointPath;
        _initialReferenceCheckpointPath = string.IsNullOrWhiteSpace(initialReferenceCheckpointPath) ? null : initialReferenceCheckpointPath;
        _deterministicFreshInitialization = deterministicFreshInitialization;
    }

    public async Task<OperationResult<ProjectWikiDpoMetricRunResult>> TrainAndEvaluateAsync(
        string unitId,
        string batchId,
        IReadOnlyList<ProjectWikiDpoMetricPair> trainingRows,
        IReadOnlyList<ProjectWikiDpoMetricPair> heldOutRows,
        string artifactRoot,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail("DPO metric run cancelled.", "DPO_METRIC_CANCELLED");

        if (string.IsNullOrWhiteSpace(unitId))
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail("Unit id is required.", "UNIT_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(batchId))
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail("Batch id is required.", "BATCH_ID_REQUIRED");

        if (trainingRows.Count != 50)
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail("DPO metric training batch must contain exactly 50 rows.", "DPO_TRAINING_BATCH_SIZE_NOT_50");

        if (heldOutRows.Count != 50)
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail("DPO metric held-out batch must contain exactly 50 rows.", "DPO_HELDOUT_BATCH_SIZE_NOT_50");

        var validation = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validation.IsSuccess)
            return validation.MapError<ProjectWikiDpoMetricRunResult>();

        var runId = $"{Sanitize(batchId)}-{Sanitize(unitId)}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var resultRoot = Path.Combine(artifactRoot, "dpo-runs", Sanitize(unitId), runId);
        var checkpointRoot = Path.Combine(resultRoot, "checkpoints");
        Directory.CreateDirectory(resultRoot);
        Directory.CreateDirectory(checkpointRoot);

        var sourcePath = Path.Combine(resultRoot, "dpo-pairs.jsonl");
        await File.WriteAllLinesAsync(
            sourcePath,
            trainingRows.Concat(heldOutRows).Select(row => JsonSerializer.Serialize(new
            {
                id = row.PairId,
                prompt = row.Prompt,
                chosen = row.Chosen,
                rejected = row.Rejected
            })),
            Encoding.UTF8,
            ct);

        var config = new ModelConfig
        {
            VocabSize = _vocabSize,
            ContextLen = _contextLen,
            HiddenDim = _hiddenDim,
            NumLayers = _numLayers,
            NumHeads = _numHeads,
            NumKvHeads = _numKvHeads,
            FfnHiddenDim = _ffnHiddenDim
        };

        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        var policyInitialization = "fresh_random_policy";
        var referenceInitialization = "copied_from_fresh_policy";
        if (_deterministicFreshInitialization &&
            _initialPolicyCheckpointPath is null &&
            _initialReferenceCheckpointPath is null)
        {
            InitializeDeterministicWeights(policy, $"project-wiki-dpo-metric:{unitId}");
            policyInitialization = "deterministic_fresh_policy";
        }

        if (_initialPolicyCheckpointPath is not null)
        {
            if (!File.Exists(_initialPolicyCheckpointPath))
                return OperationResult<ProjectWikiDpoMetricRunResult>.Fail($"Initial policy checkpoint not found: {_initialPolicyCheckpointPath}.", "INITIAL_POLICY_CHECKPOINT_NOT_FOUND");

            policy.LoadCheckpoint(_initialPolicyCheckpointPath);
            policyInitialization = _initialPolicyCheckpointPath;
        }

        if (_initialReferenceCheckpointPath is not null)
        {
            if (!File.Exists(_initialReferenceCheckpointPath))
                return OperationResult<ProjectWikiDpoMetricRunResult>.Fail($"Initial reference checkpoint not found: {_initialReferenceCheckpointPath}.", "INITIAL_REFERENCE_CHECKPOINT_NOT_FOUND");

            reference.LoadCheckpoint(_initialReferenceCheckpointPath);
            referenceInitialization = _initialReferenceCheckpointPath;
        }
        else if (_initialPolicyCheckpointPath is null)
        {
            CopyWeights(policy, reference);
            if (_deterministicFreshInitialization)
                referenceInitialization = "copied_from_deterministic_fresh_policy";
        }
        else
        {
            return OperationResult<ProjectWikiDpoMetricRunResult>.Fail(
                "Continued DPO metric runs require the original reference checkpoint.",
                "INITIAL_REFERENCE_CHECKPOINT_REQUIRED_FOR_CONTINUATION");
        }

        var initialPolicyReferenceMaxAbsDelta = MeasureMaxParameterDelta(policy, reference);
        var trainer = new DpoTrainer(policy, reference, checkpointRoot, learningRate: _learningRate);
        var process = Process.GetCurrentProcess();
        var stopwatch = Stopwatch.StartNew();
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var summaries = new List<ProjectWikiDpoMetricBatchSummary>();

        var trainingBatches = trainingRows
            .Select((row, index) => new { row, index })
            .GroupBy(item => item.index / MiniBatchSize)
            .Select(group => group.Select(item => item.row).ToArray())
            .ToArray();

        var step = 0;
        for (var epoch = 0; epoch < _epochCount; epoch++)
        {
            for (var index = 0; index < trainingBatches.Length; index++)
            {
                step++;
                var batch = trainingBatches[index];
                process.Refresh();
                var cpuBefore = process.TotalProcessorTime;
                var memoryBefore = process.WorkingSet64;
                var batchStopwatch = Stopwatch.StartNew();
                var dpoBatches = batch.Select(row => ToBatch(row, config.VocabSize, config.ContextLen)).ToArray();
                var metrics = trainer.StepMany(dpoBatches, step);
                batchStopwatch.Stop();
                process.Refresh();
                peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
                summaries.Add(new ProjectWikiDpoMetricBatchSummary(
                    step,
                    batch.First().PairId,
                    batch.Last().PairId,
                    batch.Length,
                    metrics.Loss,
                    metrics.Accuracy,
                    metrics.RewardMargin,
                    batchStopwatch.Elapsed.TotalMilliseconds,
                    ComputeCpuPercent(cpuBefore, process.TotalProcessorTime, batchStopwatch.Elapsed),
                    BytesToMb(process.WorkingSet64 - memoryBefore)));
            }
        }

        var heldOutMetrics = trainer.EvaluateMany(heldOutRows.Select(row => ToBatch(row, config.VocabSize, config.ContextLen)).ToArray(), step + 1);
        var pairDiagnostics = BuildPairDiagnostics(trainer, trainingRows, heldOutRows, config, step + 2);
        var pairDiagnosticsPath = Path.Combine(resultRoot, "dpo-pair-diagnostics.jsonl");
        await File.WriteAllLinesAsync(
            pairDiagnosticsPath,
            pairDiagnostics.Select(row => JsonSerializer.Serialize(row)),
            Encoding.UTF8,
            ct);
        stopwatch.Stop();
        var policyCheckpointPath = Path.Combine(checkpointRoot, "policy_final.bin");
        var referenceCheckpointPath = Path.Combine(checkpointRoot, "reference_base.bin");
        policy.SaveCheckpoint(policyCheckpointPath);
        reference.SaveCheckpoint(referenceCheckpointPath);

        var trainedPairCount = Math.Max(1, summaries.Sum(item => item.PairCount));
        var runLoss = summaries.Sum(item => item.Loss * item.PairCount) / trainedPairCount;
        var runAccuracy = summaries.Sum(item => item.Accuracy * item.PairCount) / trainedPairCount;
        var runRewardMargin = summaries.Sum(item => item.RewardMargin * item.PairCount) / trainedPairCount;
        var totalCpuTimeMs = summaries.Sum(item => item.CpuPercent / 100.0 * item.ElapsedMs * Math.Max(1, Environment.ProcessorCount));

        var result = new ProjectWikiDpoMetricRunResult(
            runId,
            unitId,
            batchId,
            resultRoot,
            sourcePath,
            checkpointRoot,
            _epochCount,
            _learningRate,
            policyInitialization,
            referenceInitialization,
            initialPolicyReferenceMaxAbsDelta,
            policyCheckpointPath,
            referenceCheckpointPath,
            trainingRows.Count,
            heldOutRows.Count,
            runLoss,
            runAccuracy,
            runRewardMargin,
            heldOutMetrics.Loss,
            heldOutMetrics.Accuracy,
            heldOutMetrics.RewardMargin,
            summaries.Count == 0 ? 0 : summaries.Average(item => item.CpuPercent),
            totalCpuTimeMs,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            pairDiagnosticsPath,
            summaries.ToArray());

        await File.WriteAllTextAsync(
            Path.Combine(resultRoot, "project-wiki-dpo-metric-summary.json"),
            JsonSerializer.Serialize(result, JsonOptions),
            Encoding.UTF8,
            ct);

        return OperationResult<ProjectWikiDpoMetricRunResult>.Ok(result);
    }

    private static ProjectWikiDpoMetricPairDiagnostic[] BuildPairDiagnostics(
        DpoTrainer trainer,
        IReadOnlyList<ProjectWikiDpoMetricPair> trainingRows,
        IReadOnlyList<ProjectWikiDpoMetricPair> heldOutRows,
        ModelConfig config,
        int step)
    {
        return trainingRows.Select(row => BuildPairDiagnostic(trainer, row, "train", config, step))
            .Concat(heldOutRows.Select(row => BuildPairDiagnostic(trainer, row, "heldout", config, step)))
            .ToArray();
    }

    private static ProjectWikiDpoMetricPairDiagnostic BuildPairDiagnostic(
        DpoTrainer trainer,
        ProjectWikiDpoMetricPair row,
        string split,
        ModelConfig config,
        int step)
    {
        var metrics = trainer.EvaluateMany([ToBatch(row, config.VocabSize, config.ContextLen)], step);
        var chosenTokens = RegexLikeWords(row.Chosen).ToHashSet(StringComparer.Ordinal);
        var rejectedTokens = RegexLikeWords(row.Rejected).ToHashSet(StringComparer.Ordinal);
        var overlap = chosenTokens.Count == 0 || rejectedTokens.Count == 0
            ? 0
            : chosenTokens.Intersect(rejectedTokens, StringComparer.Ordinal).Count() / (double)Math.Max(chosenTokens.Count, rejectedTokens.Count);
        return new ProjectWikiDpoMetricPairDiagnostic(
            row.PairId,
            split,
            ExtractRejectedType(row.Rejected),
            metrics.Loss,
            metrics.Accuracy,
            metrics.RewardMargin,
            overlap,
            chosenTokens.Count,
            rejectedTokens.Count);
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<ProjectWikiDpoMetricPair> rows)
    {
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.PairId) ||
                string.IsNullOrWhiteSpace(row.Prompt) ||
                string.IsNullOrWhiteSpace(row.Chosen) ||
                string.IsNullOrWhiteSpace(row.Rejected))
                return OperationResult<bool>.Fail("DPO metric rows require id, prompt, chosen, and rejected.", "INVALID_DPO_METRIC_ROW");

            if (string.Equals(row.Chosen.Trim(), row.Rejected.Trim(), StringComparison.Ordinal))
                return OperationResult<bool>.Fail($"chosen equals rejected for {row.PairId}.", "CHOSEN_EQUALS_REJECTED");

            if (!row.Rejected.Contains("[WRONG:", StringComparison.Ordinal))
                return OperationResult<bool>.Fail($"rejected must contain [WRONG:] for {row.PairId}.", "MISSING_WRONG_ANNOTATION");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static string ExtractRejectedType(string rejected)
    {
        var marker = rejected.IndexOf("[WRONG:", StringComparison.Ordinal);
        if (marker < 0)
            return "missing_wrong_annotation";
        var start = marker + "[WRONG:".Length;
        while (start < rejected.Length && char.IsWhiteSpace(rejected[start]))
            start++;
        var end = start;
        while (end < rejected.Length && !char.IsWhiteSpace(rejected[end]) && rejected[end] != '-' && rejected[end] != ']')
            end++;
        return end > start ? rejected[start..end].Trim().Trim('"', '\'') : "unknown_wrong_type";
    }

    private static DpoBatch ToBatch(ProjectWikiDpoMetricPair pair, int vocabSize, int contextLen)
    {
        var prefix = $"{pair.Prompt}\n\nAnswer:\n";
        var chosen = TokenizeWithAnswerMask(prefix, pair.Chosen, vocabSize, contextLen);
        var rejected = TokenizeWithAnswerMask(prefix, pair.Rejected, vocabSize, contextLen);
        return new DpoBatch
        {
            ChosenIds = new Tensor(chosen.Ids, new TensorShape(1, contextLen)),
            RejectedIds = new Tensor(rejected.Ids, new TensorShape(1, contextLen)),
            ChosenMask = new Tensor(chosen.Mask, new TensorShape(1, contextLen)),
            RejectedMask = new Tensor(rejected.Mask, new TensorShape(1, contextLen))
        };
    }

    private static TokenizedText TokenizeWithAnswerMask(string promptPrefix, string answer, int vocabSize, int contextLen)
    {
        var promptBudget = Math.Max(8, contextLen / 4);
        var promptTokens = RegexLikeWords(promptPrefix)
            .Select(token => 1 + (StableHash(token) % (vocabSize - 1)))
            .Take(promptBudget)
            .ToArray();
        var answerBudget = contextLen - promptTokens.Length;
        var answerTokens = TakeAnswerTokens(RegexLikeWords(answer)
            .Select(token => 1 + (StableHash(token) % (vocabSize - 1)))
            .ToArray(), answerBudget);
        var tokens = promptTokens.Concat(answerTokens).ToArray();
        if (tokens.Length < 2)
            tokens = [1, 2];

        var ids = new float[contextLen];
        var mask = new float[contextLen];
        var length = Math.Min(tokens.Length, contextLen);
        for (var index = 0; index < length; index++)
        {
            ids[index] = tokens[index];
            mask[index] = index >= promptTokens.Length ? 1f : 0f;
        }

        return new TokenizedText(ids, mask);
    }

    private static int[] TakeAnswerTokens(int[] tokens, int budget)
    {
        if (tokens.Length <= budget)
            return tokens;

        if (budget <= 4)
            return tokens.Take(budget).ToArray();

        var headCount = Math.Max(2, budget / 2);
        var tailCount = budget - headCount;
        return tokens.Take(headCount)
            .Concat(tokens.TakeLast(tailCount))
            .ToArray();
    }

    private static IEnumerable<string> RegexLikeWords(string text)
    {
        var builder = new StringBuilder();
        foreach (var character in text)
        {
            if (char.IsLetterOrDigit(character) || character == '_' || character == '-')
            {
                builder.Append(char.ToLowerInvariant(character));
                continue;
            }

            if (builder.Length == 0)
                continue;

            yield return builder.ToString();
            builder.Clear();
        }

        if (builder.Length > 0)
            yield return builder.ToString();
    }

    private static int StableHash(string value)
    {
        var hash = 2166136261u;
        foreach (var character in value)
        {
            hash ^= character;
            hash *= 16777619u;
        }

        return (int)(hash & 0x7fffffff);
    }

    private static string Sanitize(string value)
        => string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

    private static void CopyWeights(LlmModel source, LlmModel target)
    {
        var sourceParameters = source.Parameters().ToArray();
        var targetParameters = target.Parameters().ToArray();
        var count = Math.Min(sourceParameters.Length, targetParameters.Length);
        for (var parameterIndex = 0; parameterIndex < count; parameterIndex++)
        {
            var sourceData = sourceParameters[parameterIndex].Value.Data;
            var targetData = targetParameters[parameterIndex].Value.Data;
            Array.Copy(sourceData, targetData, Math.Min(sourceData.Length, targetData.Length));
        }
    }

    private static void InitializeDeterministicWeights(LlmModel model, string seedText)
    {
        var seed = StableHash(seedText);
        var parameters = model.Parameters().ToArray();
        for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
        {
            var tensor = parameters[parameterIndex].Value;
            if (tensor.Shape.Rank < 2)
                continue;

            for (var valueIndex = 0; valueIndex < tensor.Data.Length; valueIndex++)
                tensor.Data[valueIndex] = DeterministicWeight(seed, parameterIndex, valueIndex);
        }
    }

    private static float DeterministicWeight(int seed, int parameterIndex, int valueIndex)
    {
        unchecked
        {
            var hash = (uint)seed;
            hash ^= (uint)(parameterIndex + 1) * 16777619u;
            hash *= 2166136261u;
            hash ^= (uint)(valueIndex + 1) * 2246822519u;
            hash *= 3266489917u;
            var normalized = (hash & 0xffff) / 65535.0f;
            return (normalized - 0.5f) * 0.04f;
        }
    }

    private static double MeasureMaxParameterDelta(LlmModel source, LlmModel target)
    {
        var sourceParameters = source.Parameters().ToArray();
        var targetParameters = target.Parameters().ToArray();
        var count = Math.Min(sourceParameters.Length, targetParameters.Length);
        var maxDelta = 0.0;
        for (var parameterIndex = 0; parameterIndex < count; parameterIndex++)
        {
            var sourceData = sourceParameters[parameterIndex].Value.Data;
            var targetData = targetParameters[parameterIndex].Value.Data;
            var valueCount = Math.Min(sourceData.Length, targetData.Length);
            for (var valueIndex = 0; valueIndex < valueCount; valueIndex++)
                maxDelta = Math.Max(maxDelta, Math.Abs(sourceData[valueIndex] - targetData[valueIndex]));
        }

        return maxDelta;
    }

    private static double ComputeCpuPercent(TimeSpan before, TimeSpan after, TimeSpan elapsed)
    {
        if (elapsed.TotalMilliseconds <= 0)
            return 0;

        return Math.Max(0, (after - before).TotalMilliseconds / elapsed.TotalMilliseconds / Math.Max(1, Environment.ProcessorCount) * 100.0);
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;

    private sealed record TokenizedText(float[] Ids, float[] Mask);
}
