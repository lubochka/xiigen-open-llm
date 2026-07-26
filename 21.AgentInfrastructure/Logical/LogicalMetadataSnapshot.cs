// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using DynamicDomains;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

// Creates deep-owned, deterministic snapshots for logical metadata and the result shapes it carries.
public static class LogicalMetadataSnapshot
{
    private const int MaxDepth = 16;

    internal static bool TryCreate(
        IReadOnlyDictionary<string, object>? source,
        out IReadOnlyDictionary<string, object> snapshot)
    {
        snapshot = null!;
        try
        {
            return TryCreate(
                source,
                out snapshot,
                depth: 0,
                new HashSet<object>(ReferenceEqualityComparer.Instance));
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            snapshot = null!;
            return false;
        }
    }

    internal static bool TryCreate(
        LogicalFlowResult? source,
        out LogicalFlowResult snapshot)
    {
        snapshot = null!;
        try
        {
            return TryCreateLogicalFlowResult(source, out snapshot);
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            snapshot = null!;
            return false;
        }
    }

    private static bool TryCreateLogicalFlowResult(
        LogicalFlowResult? source,
        out LogicalFlowResult snapshot)
    {
        snapshot = null!;
        if (source is null
            || source.Summary is null
            || source.Metrics is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignals is null
            || source.FlowId is null
            || source.ScenarioId is null
            || source.OverallVerdict is null
            || source.FinalHumanResult is null
            || source.Summary.SummaryId is null
            || source.Summary.PrimaryMetricName is null
            || source.Summary.NextAction is null
            || source.Summary.ScalingSignalNote is null
            || source.Summary.Notes is null)
        {
            return false;
        }

        var metrics = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var metric in source.Metrics.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (metric.Key is null || metric.Value is null)
                return false;
            if (!metrics.TryAdd(metric.Key, metric.Value))
                return false;
        }

        if (source.EvidenceIds.Any(value => value is null)
            || source.Warnings.Any(value => value is null))
        {
            return false;
        }

        var signals = new PhaseTransitionSignal[source.TransitionSignals.Length];
        for (var index = 0; index < source.TransitionSignals.Length; index++)
        {
            var signal = source.TransitionSignals[index];
            if (signal is null
                || signal.TransitionPrompt is null
                || signal.SourceNodeId is null
                || signal.SourceSubPhaseId is null
                || signal.EvidenceIds is null
                || signal.EvidenceIds.Any(value => value is null))
            {
                return false;
            }

            signals[index] = signal with { EvidenceIds = signal.EvidenceIds.ToArray() };
        }

        DynamicGraphEdgeTargetHandoff? handoff = null;
        if (source.EdgeTargetHandoff is not null)
        {
            var value = source.EdgeTargetHandoff;
            handoff = new DynamicGraphEdgeTargetHandoff(
                value.RunId,
                value.EdgeId,
                value.FromNodeId,
                value.DomainTag,
                value.Kind,
                value.ParallelEligible,
                value.OrderedTargets,
                value.KnowledgeEdge,
                value.AuthoritySource,
                value.UpstreamCandidateRefs,
                value.UpstreamOutputHash,
                value.ProjectionRule);
        }

        snapshot = new LogicalFlowResult(
            source.FlowId,
            source.ScenarioId,
            source.OverallVerdict,
            source.FinalHumanResult,
            source.Summary with { },
            new ReadOnlyDictionary<string, string>(metrics),
            source.EvidenceIds.ToArray(),
            source.Warnings.ToArray())
        {
            TransitionSignals = signals,
            EdgeTargetHandoff = handoff
        };
        return true;
    }

    internal static bool TryCreate(
        LogicalFlowResult[]? source,
        out LogicalFlowResult[] snapshot)
    {
        snapshot = null!;
        if (source is null)
            return false;

        var owned = new LogicalFlowResult[source.Length];
        for (var index = 0; index < source.Length; index++)
        {
            if (!TryCreate(source[index], out owned[index]))
                return false;
        }

        snapshot = owned;
        return true;
    }

    public static bool TryCreateCopies(
        IReadOnlyDictionary<string, object>? source,
        int count,
        out IReadOnlyDictionary<string, object>[] snapshots)
    {
        snapshots = null!;
        if (count < 1 || !TryCreate(source, out var authority))
            return false;

        var owned = new IReadOnlyDictionary<string, object>[count];
        for (var index = 0; index < owned.Length; index++)
        {
            if (!TryCreate(authority, out owned[index]))
                return false;
        }

        snapshots = owned;
        return true;
    }

    private static bool TryCreate(
        IReadOnlyDictionary<string, object>? source,
        out IReadOnlyDictionary<string, object> snapshot,
        int depth,
        HashSet<object> activeReferences)
    {
        snapshot = null!;
        if (source is null
            || depth > MaxDepth
            || !activeReferences.Add(source))
        {
            return false;
        }

        try
        {
            var owned = new SortedDictionary<string, object>(StringComparer.Ordinal);
            foreach (var item in source.OrderBy(item => item.Key, StringComparer.Ordinal))
            {
                if (item.Key is null
                    || item.Value is null
                    || !TryCreateValue(
                        item.Value,
                        out var stableValue,
                        depth + 1,
                        activeReferences))
                {
                    return false;
                }

                if (!owned.TryAdd(item.Key, stableValue))
                    return false;
            }

            snapshot = new ReadOnlyDictionary<string, object>(owned);
            return true;
        }
        finally
        {
            activeReferences.Remove(source);
        }
    }

    // Clone only explicitly supported metadata shapes; unknown references fail closed rather than being shared.
    private static bool TryCreateValue(
        object source,
        out object snapshot,
        int depth,
        HashSet<object> activeReferences)
    {
        snapshot = null!;
        if (depth > MaxDepth)
            return false;

        switch (source)
        {
            case string:
            case bool:
            case byte:
            case sbyte:
            case short:
            case ushort:
            case int:
            case uint:
            case long:
            case ulong:
            case float:
            case double:
            case decimal:
            case char:
            case DateTime:
            case DateTimeOffset:
            case DateOnly:
            case TimeOnly:
            case TimeSpan:
            case Guid:
            case IntPtr:
            case UIntPtr:
            case Enum:
                snapshot = source;
                return true;

            case string[] values:
                if (values.Any(value => value is null))
                    return false;
                snapshot = values.ToArray();
                return true;

            case int[] values:
                snapshot = values.ToArray();
                return true;

            case double[] values:
                snapshot = values.ToArray();
                return true;

            case bool[] values:
                snapshot = values.ToArray();
                return true;

            case Domain[] values:
                snapshot = values.ToArray();
                return true;

            case LogicalContextPacket packet:
                if (!TryCreateLogicalContextPacket(packet, out var stablePacket))
                    return false;
                snapshot = stablePacket;
                return true;

            case ContextSignals signals:
                if (!TryCreateContextSignals(signals, out var stableSignals))
                    return false;
                snapshot = stableSignals;
                return true;

            case LogicalCapabilityTrainingResult trainingResult:
                if (!TryCreateLogicalCapabilityTrainingResult(trainingResult, out var stableTrainingResult))
                    return false;
                snapshot = stableTrainingResult;
                return true;

            case LogicalFlowResult[] results:
                if (!TryCreate(results, out var stableResults))
                    return false;
                snapshot = stableResults;
                return true;

            case IReadOnlyDictionary<string, string> stringMap:
                if (!TryCreateStringDictionary(stringMap, out var stableStringMap))
                    return false;
                snapshot = stableStringMap;
                return true;

            case IReadOnlyDictionary<string, object> objectMap:
                if (!TryCreate(
                        objectMap,
                        out var stableObjectMap,
                        depth,
                        activeReferences))
                {
                    return false;
                }

                snapshot = stableObjectMap;
                return true;

            default:
                return false;
        }
    }

    private static bool TryCreateLogicalCapabilityTrainingResult(
        LogicalCapabilityTrainingResult source,
        out LogicalCapabilityTrainingResult snapshot)
    {
        snapshot = null!;
        if (source.RegisteredFlowIds is null
            || source.RegisteredSubPhaseIds is null
            || source.RegisteredUnitIds is null
            || source.EvidenceIds is null
            || source.BranchTimings is null
            || source.Notes is null
            || source.Domain is null
            || source.RegisteredFlowIds.Any(value => value is null)
            || source.RegisteredSubPhaseIds.Any(value => value is null)
            || source.RegisteredUnitIds.Any(value => value is null)
            || source.EvidenceIds.Any(value => value is null))
        {
            return false;
        }

        var branchTimings = new LogicalCapabilityTrainingBranchTiming[source.BranchTimings.Length];
        for (var index = 0; index < source.BranchTimings.Length; index++)
        {
            var timing = source.BranchTimings[index];
            if (timing is null
                || timing.SubPhaseId is null
                || timing.UnitId is null)
            {
                return false;
            }

            branchTimings[index] = new LogicalCapabilityTrainingBranchTiming(
                timing.SubPhaseId,
                timing.UnitId,
                timing.ParentUnitId,
                timing.TrainingStepCount,
                timing.UsedWarmCache,
                timing.BuildStackMs,
                timing.ReferenceModelMs,
                timing.DpoMs,
                timing.SaveArtifactsMs,
                timing.WrapUnitMs,
                timing.SaveManifestMs,
                timing.RegisterUnitMs,
                timing.RegisterFlowMs,
                timing.TotalMs);
        }

        snapshot = new LogicalCapabilityTrainingResult(
            source.CapabilityRegistered,
            source.RegisteredFlowIds.ToArray(),
            source.RegisteredSubPhaseIds.ToArray(),
            source.RegisteredUnitIds.ToArray(),
            source.TrainingTimeMs,
            source.EvidenceIds.ToArray(),
            source.Notes)
        {
            Domain = source.Domain,
            BatchedBranchCount = source.BatchedBranchCount,
            BatchedDpoStepCount = source.BatchedDpoStepCount,
            BranchTimings = branchTimings
        };
        return true;
    }

    private static bool TryCreateStringDictionary(
        IReadOnlyDictionary<string, string>? source,
        out IReadOnlyDictionary<string, string> snapshot)
    {
        snapshot = null!;
        if (source is null)
            return false;

        var owned = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in source.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (item.Key is null || item.Value is null)
                return false;
            if (!owned.TryAdd(item.Key, item.Value))
                return false;
        }

        snapshot = new ReadOnlyDictionary<string, string>(owned);
        return true;
    }

    private static bool TryCreateLogicalContextPacket(
        LogicalContextPacket source,
        out LogicalContextPacket snapshot)
    {
        snapshot = null!;
        if (source.RunId is null
            || source.SourceFlowIds is null
            || source.SourceScenarioIds is null
            || source.Fragments is null
            || source.SourceFlowIds.Any(value => value is null)
            || source.SourceScenarioIds.Any(value => value is null))
        {
            return false;
        }

        var fragments = new LogicalContextFragment[source.Fragments.Length];
        for (var index = 0; index < source.Fragments.Length; index++)
        {
            var fragment = source.Fragments[index];
            if (fragment is null
                || fragment.FlowId is null
                || fragment.ScenarioId is null
                || fragment.OverallVerdict is null
                || fragment.FinalHumanResult is null
                || fragment.EvidenceIds is null
                || fragment.Warnings is null
                || fragment.EvidenceIds.Any(value => value is null)
                || fragment.Warnings.Any(value => value is null)
                || !TryCreateStringDictionary(fragment.Metrics, out var stableMetrics))
            {
                return false;
            }

            fragments[index] = new LogicalContextFragment(
                fragment.FlowId,
                fragment.ScenarioId,
                fragment.OverallVerdict,
                fragment.FinalHumanResult,
                fragment.PhaseId,
                fragment.SubPhaseId,
                stableMetrics,
                fragment.EvidenceIds.ToArray(),
                fragment.Warnings.ToArray());
        }

        snapshot = new LogicalContextPacket(
            source.RunId,
            source.SourceFlowIds.ToArray(),
            source.SourceScenarioIds.ToArray(),
            fragments,
            source.CreatedAtUtc);
        return true;
    }

    private static bool TryCreateContextSignals(
        ContextSignals source,
        out ContextSignals snapshot)
    {
        snapshot = null!;
        if (source.PriorDomains is null
            || source.CompletedSubPhases is null
            || source.ActiveKnowledgeEdges is null
            || source.PriorConfidenceMap is null
            || source.CompletedSubPhases.Any(value => value is null)
            || source.ActiveKnowledgeEdges.Any(value => value is null))
        {
            return false;
        }

        // Domain's numeric order is the canonical serialization/enumeration order across graph boundaries.
        var confidenceMap = new SortedDictionary<Domain, double>();
        foreach (var item in source.PriorConfidenceMap.OrderBy(item => (int)item.Key))
        {
            if (!confidenceMap.TryAdd(item.Key, item.Value))
                return false;
        }

        snapshot = new ContextSignals(
            source.PriorDomains.ToArray(),
            source.CompletedSubPhases.ToArray(),
            source.ActiveKnowledgeEdges.ToArray(),
            new ReadOnlyDictionary<Domain, double>(confidenceMap));
        return true;
    }

    private static bool IsFatal(Exception exception)
        => exception is OutOfMemoryException
            or StackOverflowException
            or AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException
            || string.Equals(
                exception.GetType().FullName,
                "System.Threading.ThreadAbortException",
                StringComparison.Ordinal);
}
