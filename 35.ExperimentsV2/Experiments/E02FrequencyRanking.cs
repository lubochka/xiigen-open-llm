// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E02: frequency-based method ranking against alphabetical method ordering.
public sealed class E02FrequencyRanking
{
    // Run the experiment on one or more C# source documents.
    public OperationResult<ExperimentV2Result> Run(IReadOnlyList<string> sources, int tier = 1)
    {
        if (sources.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No source documents for E02");

        var started = Stopwatch();
        var allCalls = new List<(string Receiver, string Method)>();
        foreach (var source in sources)
        {
            var extracted = ExtractMethodCalls(source);
            if (extracted.IsSuccess)
                allCalls.AddRange(extracted.Value!);
        }

        if (allCalls.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No method invocations found in sources");

        var frequencyTable = new MethodFrequencyTable();
        frequencyTable.Train(sources);
        var receiverGroups = allCalls
            .GroupBy(call => call.Receiver, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(call => call.Method).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        var baselineMs = ElapsedMs(started);
        var enhancedStarted = Stopwatch();
        var baselineCorrect = 0;
        var enhancedCorrect = 0;
        foreach (var (receiver, method) in allCalls)
        {
            if (!receiverGroups.TryGetValue(receiver, out var methods))
                continue;

            var alphabetical = methods.OrderBy(candidate => candidate, StringComparer.OrdinalIgnoreCase).ToArray();
            if (alphabetical.Length > 0 && string.Equals(alphabetical[0], method, StringComparison.OrdinalIgnoreCase))
                baselineCorrect++;

            var ranked = frequencyTable.RankMethods(receiver, methods);
            if (ranked.Count > 0 && string.Equals(ranked[0], method, StringComparison.OrdinalIgnoreCase))
                enhancedCorrect++;
        }

        var total = allCalls.Count;
        var baselineTop1 = baselineCorrect / (double)total;
        var enhancedTop1 = enhancedCorrect / (double)total;
        var enhancedMs = ElapsedMs(enhancedStarted);
        var methodCounts = allCalls
            .GroupBy(call => $"{call.Receiver}.{call.Method}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Count())
            .ToArray();
        var confidencePairs = methodCounts.Count(count => count >= 10);
        var confidenceRatio = frequencyTable.TotalPairs > 0 ? confidencePairs / (double)frequencyTable.TotalPairs : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId = "E02-FREQUENCY-RANKING",
            Tier = tier,
            BaselinePrimary = baselineTop1,
            EnhancedPrimary = enhancedTop1,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter = false,
            ScalingSignal = confidenceRatio,
            ScalingSignalNote = "confidence_ratio (pairs with >=10 obs / total pairs)",
            BaselineTimeMs = baselineMs,
            EnhancedTimeMs = enhancedMs,
            Notes = $"totalCalls={total}, uniqueReceivers={receiverGroups.Count}, confRatio={confidenceRatio:P1}",
            ShouldScaleUp = confidenceRatio == 0
        });
    }

    // Keep the experiment API stable while delegating extraction to production.
    public static OperationResult<List<(string Receiver, string Method)>> ExtractMethodCalls(string source)
    {
        var extracted = MethodFrequencyTable.ExtractMethodCalls(source);
        return extracted.IsSuccess
            ? OperationResult<List<(string Receiver, string Method)>>.Ok(extracted.Value!)
            : OperationResult<List<(string Receiver, string Method)>>.Fail(extracted.ErrorMessage);
    }

    private static long Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();

    private static double ElapsedMs(long started)
    {
        return (System.Diagnostics.Stopwatch.GetTimestamp() - started)
            / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
    }
}
