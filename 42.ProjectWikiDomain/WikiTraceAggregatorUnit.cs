// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class WikiTraceAggregatorUnit
{
    public const string UnitId = "wiki-trace-aggregator-unit";
    public const string ScenarioId = "project-wiki-trace-aggregation";

    public OperationResult<WikiTraceAggregationResult> Aggregate(
        string batchId,
        IReadOnlyList<WikiQueryTrace> queryTraces,
        IReadOnlyList<WikiUnitTrace> unitTraces)
    {
        if (string.IsNullOrWhiteSpace(batchId))
            return OperationResult<WikiTraceAggregationResult>.Fail("Batch id is required.", "BATCH_ID_REQUIRED");

        if (queryTraces is null)
            return OperationResult<WikiTraceAggregationResult>.Fail("Query traces are required.", "QUERY_TRACES_REQUIRED");

        if (unitTraces is null)
            return OperationResult<WikiTraceAggregationResult>.Fail("Unit traces are required.", "UNIT_TRACES_REQUIRED");

        var failedUnits = unitTraces
            .SelectMany(trace => trace.FailedUnits)
            .Concat(queryTraces.SelectMany(trace => trace.UnitResults.Where(unit => !string.Equals(unit.PassOrFail, "PASS", StringComparison.Ordinal)).Select(unit => unit.UnitId)))
            .Where(unit => !string.IsNullOrWhiteSpace(unit))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var remediation = unitTraces
            .SelectMany(trace => trace.RemediationBatchesRequired)
            .Concat(queryTraces.SelectMany(trace => trace.UnitResults.Where(unit => !string.Equals(unit.PassOrFail, "PASS", StringComparison.Ordinal)).Select(unit => unit.RemediationAction)))
            .Where(action => !string.IsNullOrWhiteSpace(action))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var queryPassRate = queryTraces.Count == 0
            ? 0
            : queryTraces.Count(trace => string.Equals(trace.FinalStatus, "PASS", StringComparison.Ordinal)) / (double)queryTraces.Count;
        var stopBlindTraining = unitTraces.Any(trace => trace.StopBlindTraining) ||
                                (queryTraces.Count > 0 && queryPassRate < 0.75);
        var warnings = queryTraces.Count == 0 && unitTraces.Count == 0
            ? ["no_traces_to_aggregate"]
            : Array.Empty<string>();
        var confidence = warnings.Length == 0 ? Math.Min(0.98, 0.70 + queryTraces.Count * 0.02 + unitTraces.Count * 0.03) : 0.35;

        return OperationResult<WikiTraceAggregationResult>.Ok(new WikiTraceAggregationResult(
            batchId,
            queryTraces.Count,
            unitTraces.Count,
            queryPassRate,
            failedUnits,
            remediation,
            stopBlindTraining,
            confidence,
            warnings));
    }

    public OperationResult<ProjectWikiNextTrainingAction> SelectNextTrainingAction(
        WikiTraceAggregationResult aggregation,
        ProjectWikiTrainingAssetRegistry registry)
    {
        if (aggregation is null)
            return OperationResult<ProjectWikiNextTrainingAction>.Fail("Trace aggregation result is required.", "TRACE_AGGREGATION_REQUIRED");

        if (registry is null)
            return OperationResult<ProjectWikiNextTrainingAction>.Fail("Training asset registry is required.", "TRAINING_ASSET_REGISTRY_REQUIRED");

        var continuationGate = registry.ValidateTrainingMayContinue();
        if (!continuationGate.IsSuccess)
            return OperationResult<ProjectWikiNextTrainingAction>.Ok(new ProjectWikiNextTrainingAction(
                "NONE",
                continuationGate.ErrorMessage,
                [],
                true));

        var target = aggregation.FailedUnits.FirstOrDefault(unit => !string.IsNullOrWhiteSpace(unit)) ??
                     registry.Entries
                         .Where(IsWeakRegistrySignal)
                         .GroupBy(entry => entry.UnitName, StringComparer.Ordinal)
                         .OrderByDescending(WeakSignalScore)
                         .ThenByDescending(group => group.Count())
                         .Select(group => group.Key)
                         .FirstOrDefault(unit => !string.Equals(unit, "UNKNOWN", StringComparison.Ordinal)) ??
                     "ProjectWikiRequestRouterUnit";
        var evidence = registry.EntriesForUnit(target)
            .Take(10)
            .Select(entry => entry.BatchId)
            .ToArray();
        var reason = aggregation.FailedUnits.Contains(target, StringComparer.Ordinal)
            ? "Selected from failed unit traces."
            : "Selected from registry statuses because no failed unit trace was available.";

        return OperationResult<ProjectWikiNextTrainingAction>.Ok(new ProjectWikiNextTrainingAction(
            target,
            reason,
            evidence,
            aggregation.StopBlindTraining));
    }

    private static bool IsWeakRegistrySignal(ProjectWikiTrainingAssetEntry entry)
        => string.Equals(entry.Status, "rejected", StringComparison.Ordinal) ||
           string.Equals(entry.Status, "blocked", StringComparison.Ordinal) ||
           string.Equals(entry.Status, "experimental_no_authority", StringComparison.Ordinal);

    private static int WeakSignalScore(IGrouping<string, ProjectWikiTrainingAssetEntry> group)
        => group.Sum(entry =>
            string.Equals(entry.Status, "rejected", StringComparison.Ordinal) ? 3 :
            string.Equals(entry.Status, "blocked", StringComparison.Ordinal) ? 2 :
            string.Equals(entry.Status, "experimental_no_authority", StringComparison.Ordinal) ? 1 :
            0);
}
