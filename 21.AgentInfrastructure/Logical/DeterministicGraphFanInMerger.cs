// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;

// This production merger materializes the Plan-06 B-16 deterministic, non-authoritative fan-in baseline.
public sealed class DeterministicGraphFanInMerger : IEdgeFanInMerger
{
    private const string InputNullCode = "FAN_IN_INPUT_NULL";
    private const string InputMalformedCode = "FAN_IN_INPUT_MALFORMED";
    private const string BranchMalformedCode = "FAN_IN_BRANCH_MALFORMED";
    private const string AmbiguousCanonicalKeyCode = "AMBIGUOUS_MERGE_PATH_KEY";
    private const string NoBranchesCode = "NO_BRANCHES_TO_MERGE";
    private const string MergeKeyMismatchCode = "MERGE_KEY_MISMATCH";
    private const string PathOrderRefMissingCode = "PATH_ORDER_REF_MISSING";
    private const string ProvenanceRefMissingCode = "PROVENANCE_REF_MISSING";
    private const string LegalScopeBlockedCode = "LEGAL_SCOPE_BLOCKED";
    private const string ConflictUnresolvedCode = "CONFLICT_UNRESOLVED";
    private const string UnconsumedTargetHandoffCode = "EDGE_FAN_IN_UNCONSUMED_TARGET_HANDOFF";
    private const string AcceptedLegalScope = "LEGAL_SCOPE_ALLOWED";
    private const string ConflictFieldPrefix = "Metrics.";

    private static readonly HashSet<string> ExplicitlyBlockedLegalScopes = new(StringComparer.Ordinal)
    {
        "BLOCK",
        "BLOCKED",
        "DENIED",
        "LEGAL_SCOPE_BLOCKED",
        "QUARANTINE",
        "QUARANTINED",
        "REJECTED"
    };

    // Merge validates and snapshots the complete input before performing canonical worst-of aggregation.
    public OperationResult<GraphFanInMergeResult> Merge(GraphFanInMergeInput input)
    {
        var validation = ValidateInput(input);
        if (!validation.IsSuccess)
            return validation.MapError<GraphFanInMergeResult>();

        var orderedBranches = OrderBranches(input.Branches);
        var summaryTimeTotals = AggregateSummaryTimes(orderedBranches);
        if (!summaryTimeTotals.IsSuccess)
            return summaryTimeTotals.MapError<GraphFanInMergeResult>();

        var orderedRejectedAlternatives = OrderBranches(input.RejectedAlternatives);
        var conflictSnapshots = BuildDerivedMetricConflicts(orderedBranches);
        var merged = BuildMergedResult(
            input,
            orderedBranches,
            conflictSnapshots.Count != 0,
            summaryTimeTotals.Value!);
        var context = BuildContext(input, orderedBranches);
        var provenanceRefs = BuildProvenanceRefs(input, orderedBranches, orderedRejectedAlternatives);
        var traceRefs = BuildTraceRefs(input, orderedBranches, orderedRejectedAlternatives);
        var failureReasonCodes = BuildFailureReasonCodes(
            input,
            orderedBranches,
            orderedRejectedAlternatives,
            conflictSnapshots.Count != 0);

        var result = new GraphFanInMergeResult(
            Merged: merged,
            Context: context,
            MergeRole: FanInMergeRole.DeterministicFanInBaseline,
            MergeOrderIsAuthority: false,
            MergedBranchCount: orderedBranches.Length,
            RejectedAlternatives: Array.AsReadOnly(
                orderedRejectedAlternatives.Select(SnapshotBranch).ToArray()),
            Conflicts: conflictSnapshots,
            ProvenanceRefs: provenanceRefs,
            TraceRefs: traceRefs,
            FailureReasonCodes: failureReasonCodes);

        return OperationResult<GraphFanInMergeResult>.Ok(result);
    }

    // ValidateInput applies fail-closed validation in a stable field order and never relies on enumeration order.
    private static OperationResult<bool> ValidateInput(GraphFanInMergeInput input)
    {
        if (input is null)
            return OperationResult<bool>.Fail("Fan-in input is required.", InputNullCode);

        if (string.IsNullOrWhiteSpace(input.RunId))
            return OperationResult<bool>.Fail("Fan-in RunId is required.", InputMalformedCode);

        if (input.Branches is null || input.Branches.Length == 0)
            return OperationResult<bool>.Fail("At least one fan-in branch is required.", NoBranchesCode);

        if (string.IsNullOrWhiteSpace(input.MergeKey))
            return OperationResult<bool>.Fail("Fan-in MergeKey is required.", MergeKeyMismatchCode);

        if (string.IsNullOrWhiteSpace(input.PathOrderRef))
            return OperationResult<bool>.Fail("Fan-in PathOrderRef is required.", PathOrderRefMissingCode);

        // Plan-06:984/1594 requires LegalScope and :1604 names the blocked failure, but defines no
        // accepted-value vocabulary. Normalize once and admit only this narrow local sentinel until
        // that owner gap is resolved; every explicit blocked or unknown value fails closed.
        var normalizedLegalScope = NormalizeLegalScope(input.LegalScope);
        if (ExplicitlyBlockedLegalScopes.Contains(normalizedLegalScope))
            return OperationResult<bool>.Fail("Fan-in legal scope is explicitly blocked.", LegalScopeBlockedCode);

        if (!string.Equals(normalizedLegalScope, AcceptedLegalScope, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Fan-in legal scope is missing or not explicitly allowed.", LegalScopeBlockedCode);

        var sourceValidation = ValidateReferenceArray(input.SourceRefs, "SourceRefs", requireValue: true);
        if (!sourceValidation.IsSuccess)
            return OperationResult<bool>.Fail(sourceValidation.ErrorMessage, ProvenanceRefMissingCode);

        var provenanceValidation = ValidateReferenceArray(
            input.ProvenanceRefs,
            "ProvenanceRefs",
            requireValue: true);
        if (!provenanceValidation.IsSuccess)
            return OperationResult<bool>.Fail(provenanceValidation.ErrorMessage, ProvenanceRefMissingCode);

        var traceValidation = ValidateReferenceArray(input.TraceRefs, "TraceRefs", requireValue: true);
        if (!traceValidation.IsSuccess)
            return traceValidation;

        if (input.RejectedAlternatives is null || input.Conflicts is null || input.FailureReasonCodes is null)
            return OperationResult<bool>.Fail(
                "RejectedAlternatives, Conflicts, and FailureReasonCodes must be materialized.",
                InputMalformedCode);

        var failureValidation = ValidateReferenceArray(
            input.FailureReasonCodes,
            "FailureReasonCodes",
            requireValue: false);
        if (!failureValidation.IsSuccess)
            return failureValidation;

        var branchValidation = ValidateBranchSet(input.Branches, input.MergeKey, "Branches");
        if (!branchValidation.IsSuccess)
            return branchValidation;

        var rejectedValidation = ValidateBranchSet(
            input.RejectedAlternatives,
            input.MergeKey,
            "RejectedAlternatives");
        if (!rejectedValidation.IsSuccess)
            return rejectedValidation;

        var unionValidation = ValidateUnionIdentity(input.Branches, input.RejectedAlternatives);
        if (!unionValidation.IsSuccess)
            return unionValidation;

        var activeHandoffValidation = ValidateNoUnconsumedTargetHandoffs(input.Branches);
        if (!activeHandoffValidation.IsSuccess)
            return activeHandoffValidation;

        var rejectedHandoffValidation = ValidateNoUnconsumedTargetHandoffs(input.RejectedAlternatives);
        if (!rejectedHandoffValidation.IsSuccess)
            return rejectedHandoffValidation;

        return ValidateConflictEvidence(input.Conflicts, OrderBranches(input.Branches));
    }

    // ValidateBranchSet validates typed carriers and rejects duplicate owner canonical keys without a tie-break.
    private static OperationResult<bool> ValidateBranchSet(
        IReadOnlyList<GraphAsyncBranchResult> branches,
        string expectedMergeKey,
        string fieldName)
    {
        var canonicalKeys = new HashSet<(string MergeKey, string PathOrderRef)>();
        var branchIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var branch in branches
                     .OrderBy(item => item?.MergeKey ?? string.Empty, StringComparer.Ordinal)
                     .ThenBy(item => item?.PathOrderRef ?? string.Empty, StringComparer.Ordinal)
                     .ThenBy(item => item?.BranchId ?? string.Empty, StringComparer.Ordinal))
        {
            var validation = ValidateBranch(branch, expectedMergeKey, fieldName);
            if (!validation.IsSuccess)
                return validation;

            if (!canonicalKeys.Add((branch.MergeKey, branch.PathOrderRef)))
            {
                return OperationResult<bool>.Fail(
                    $"{fieldName} contains a duplicate canonical (MergeKey, PathOrderRef).",
                    AmbiguousCanonicalKeyCode);
            }

            if (!branchIds.Add(branch.BranchId))
            {
                return OperationResult<bool>.Fail(
                    $"{fieldName} contains an ambiguous duplicate BranchId.",
                    AmbiguousCanonicalKeyCode);
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    // Fan-in cannot consume, select, merge, or safely discard an edge target handoff from any branch.
    private static OperationResult<bool> ValidateNoUnconsumedTargetHandoffs(
        IReadOnlyList<GraphAsyncBranchResult> branches)
    {
        foreach (var branch in branches
                     .OrderBy(item => item.MergeKey, StringComparer.Ordinal)
                     .ThenBy(item => item.PathOrderRef, StringComparer.Ordinal)
                     .ThenBy(item => item.BranchId, StringComparer.Ordinal))
        {
            if (branch.Inner.EdgeTargetHandoff is not null)
            {
                return OperationResult<bool>.Fail(
                    "Fan-in input contains an unconsumed edge target handoff.",
                    UnconsumedTargetHandoffCode);
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    // ValidateBranch rejects malformed branch payloads before any aggregation or snapshot construction occurs.
    private static OperationResult<bool> ValidateBranch(
        GraphAsyncBranchResult branch,
        string expectedMergeKey,
        string fieldName)
    {
        if (branch is null)
            return OperationResult<bool>.Fail($"{fieldName} contains a null branch.", BranchMalformedCode);

        if (string.IsNullOrWhiteSpace(branch.CombinationId)
            || string.IsNullOrWhiteSpace(branch.BranchId)
            || string.IsNullOrWhiteSpace(branch.OverallVerdict))
        {
            return OperationResult<bool>.Fail(
                $"{fieldName} contains a branch with missing identity or verdict.",
                BranchMalformedCode);
        }

        if (!IsKnownVerdict(branch.OverallVerdict))
        {
            return OperationResult<bool>.Fail(
                $"{fieldName} contains a verdict outside the current orchestrator vocabulary.",
                BranchMalformedCode);
        }

        if (!string.Equals(branch.MergeKey, expectedMergeKey, StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                $"{fieldName} branch MergeKey does not match the fan-in MergeKey.",
                MergeKeyMismatchCode);
        }

        if (string.IsNullOrWhiteSpace(branch.PathOrderRef))
            return OperationResult<bool>.Fail($"{fieldName} branch PathOrderRef is required.", PathOrderRefMissingCode);

        if (!double.IsFinite(branch.Confidence) || branch.Confidence < 0d || branch.Confidence > 1d)
            return OperationResult<bool>.Fail($"{fieldName} branch Confidence must be in [0,1].", BranchMalformedCode);

        var sourceValidation = ValidateReferenceArray(branch.SourceHashes, "SourceHashes", requireValue: true);
        if (!sourceValidation.IsSuccess)
            return OperationResult<bool>.Fail(sourceValidation.ErrorMessage, ProvenanceRefMissingCode);

        var provenanceValidation = ValidateReferenceArray(
            branch.ProvenanceRefs,
            "ProvenanceRefs",
            requireValue: true);
        if (!provenanceValidation.IsSuccess)
            return OperationResult<bool>.Fail(provenanceValidation.ErrorMessage, ProvenanceRefMissingCode);

        var traceValidation = ValidateReferenceArray(branch.TraceRefs, "TraceRefs", requireValue: true);
        if (!traceValidation.IsSuccess)
            return traceValidation;

        var errorsValidation = ValidateReferenceArray(branch.ErrorCodes, "ErrorCodes", requireValue: false);
        if (!errorsValidation.IsSuccess)
            return errorsValidation;

        if (!IsValidInner(branch.Inner))
            return OperationResult<bool>.Fail($"{fieldName} contains a malformed Inner result.", BranchMalformedCode);

        if (!string.Equals(branch.OverallVerdict, branch.Inner.OverallVerdict, StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                $"{fieldName} branch verdict disagrees with Inner.OverallVerdict.",
                BranchMalformedCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    // IsValidInner checks every nested collection needed by the deep snapshot and context builders.
    private static bool IsValidInner(LogicalFlowResult inner)
    {
        if (inner is null
            || string.IsNullOrWhiteSpace(inner.FlowId)
            || string.IsNullOrWhiteSpace(inner.ScenarioId)
            || string.IsNullOrWhiteSpace(inner.OverallVerdict)
            || inner.FinalHumanResult is null
            || inner.Summary is null
            || inner.Metrics is null
            || inner.EvidenceIds is null
            || inner.Warnings is null
            || inner.TransitionSignals is null)
        {
            return false;
        }

        if (inner.Metrics.Any(pair => string.IsNullOrWhiteSpace(pair.Key) || pair.Value is null)
            || inner.EvidenceIds.Any(string.IsNullOrWhiteSpace)
            || inner.Warnings.Any(value => value is null)
            || !IsValidSummary(inner.Summary))
        {
            return false;
        }

        return inner.TransitionSignals.All(signal =>
            signal is not null
            && !string.IsNullOrWhiteSpace(signal.TransitionPrompt)
            && !string.IsNullOrWhiteSpace(signal.SourceNodeId)
            && !string.IsNullOrWhiteSpace(signal.SourceSubPhaseId)
            && signal.EvidenceIds is not null
            && signal.EvidenceIds.All(value => !string.IsNullOrWhiteSpace(value)));
    }

    // IsValidSummary rejects malformed identifiers and every non-JSON-safe numeric carrier.
    // Advisory text may be blank, but cannot violate its non-nullable owner contract.
    private static bool IsValidSummary(LogicalFlowSummary summary)
        => !string.IsNullOrWhiteSpace(summary.SummaryId)
            && !string.IsNullOrWhiteSpace(summary.PrimaryMetricName)
            && summary.NextAction is not null
            && summary.ScalingSignalNote is not null
            && summary.Notes is not null
            && double.IsFinite(summary.BaselinePrimary)
            && double.IsFinite(summary.EnhancedPrimary)
            && double.IsFinite(summary.ScalingSignal)
            && double.IsFinite(summary.BaselineTimeMs)
            && double.IsFinite(summary.EnhancedTimeMs);

    // ValidateReferenceArray ensures callers cannot defer enumeration or inject null/blank evidence entries.
    private static OperationResult<bool> ValidateReferenceArray(
        IReadOnlyList<string> values,
        string fieldName,
        bool requireValue)
    {
        if (values is null || (requireValue && values.Count == 0))
        {
            return OperationResult<bool>.Fail(
                $"{fieldName} must be a materialized non-empty collection.",
                InputMalformedCode);
        }

        if (values.Any(string.IsNullOrWhiteSpace))
        {
            return OperationResult<bool>.Fail(
                $"{fieldName} cannot contain null or blank values.",
                InputMalformedCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    // ValidateUnionIdentity rejects overlap across active and rejected sets; neither identity has an owner tie-break.
    private static OperationResult<bool> ValidateUnionIdentity(
        IReadOnlyList<GraphAsyncBranchResult> branches,
        IReadOnlyList<GraphAsyncBranchResult> rejectedAlternatives)
    {
        var canonicalKeys = new HashSet<(string MergeKey, string PathOrderRef)>();
        var branchIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var branch in branches
                     .Concat(rejectedAlternatives)
                     .OrderBy(item => item.MergeKey, StringComparer.Ordinal)
                     .ThenBy(item => item.PathOrderRef, StringComparer.Ordinal)
                     .ThenBy(item => item.BranchId, StringComparer.Ordinal))
        {
            if (!canonicalKeys.Add((branch.MergeKey, branch.PathOrderRef)))
            {
                return OperationResult<bool>.Fail(
                    "Active and rejected branches overlap on canonical (MergeKey, PathOrderRef).",
                    AmbiguousCanonicalKeyCode);
            }

            if (!branchIds.Add(branch.BranchId))
            {
                return OperationResult<bool>.Fail(
                    "Active and rejected branches overlap on BranchId.",
                    AmbiguousCanonicalKeyCode);
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    // ValidateConflictEvidence requires exact correspondence with conflicts derived from active payload metrics.
    private static OperationResult<bool> ValidateConflictEvidence(
        IReadOnlyList<GraphFanInConflict> conflicts,
        IReadOnlyList<GraphAsyncBranchResult> orderedBranches)
    {
        var suppliedByField = new Dictionary<string, GraphFanInConflict>(StringComparer.Ordinal);
        foreach (var conflict in conflicts.OrderBy(item => item?.FieldName ?? string.Empty, StringComparer.Ordinal))
        {
            if (conflict is null
                || string.IsNullOrWhiteSpace(conflict.FieldName)
                || conflict.ValuesByBranchId is null
                || !suppliedByField.TryAdd(conflict.FieldName, conflict))
            {
                return OperationResult<bool>.Fail(
                    "Conflict evidence contains a malformed or duplicate field carrier.",
                    ConflictUnresolvedCode);
            }
        }

        var derived = BuildDerivedMetricConflicts(orderedBranches);
        if (suppliedByField.Count != derived.Count)
        {
            return OperationResult<bool>.Fail(
                "Conflict evidence omits a derived conflict or supplies a forged conflict.",
                ConflictUnresolvedCode);
        }

        foreach (var expected in derived)
        {
            if (!suppliedByField.TryGetValue(expected.FieldName, out var supplied)
                || supplied.ValuesByBranchId.Count != expected.ValuesByBranchId.Count)
            {
                return OperationResult<bool>.Fail(
                    "Conflict evidence field or branch coverage does not match active payloads.",
                    ConflictUnresolvedCode);
            }

            foreach (var expectedValue in expected.ValuesByBranchId)
            {
                if (!supplied.ValuesByBranchId.TryGetValue(expectedValue.Key, out var suppliedValue)
                    || !string.Equals(suppliedValue, expectedValue.Value, StringComparison.Ordinal))
                {
                    return OperationResult<bool>.Fail(
                        "Conflict evidence value does not match the active branch payload.",
                        ConflictUnresolvedCode);
                }
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    // NormalizeLegalScope is the sole legal token normalization point for this merger.
    private static string NormalizeLegalScope(string legalScope)
        => legalScope?.Trim().ToUpperInvariant() ?? string.Empty;

    // BuildDerivedMetricConflicts is the sole conflict source; caller-supplied maps never define truth.
    private static IReadOnlyList<GraphFanInConflict> BuildDerivedMetricConflicts(
        IReadOnlyList<GraphAsyncBranchResult> orderedBranches)
    {
        var conflicts = new List<GraphFanInConflict>();
        var metricKeys = orderedBranches
            .SelectMany(branch => branch.Inner.Metrics.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal);

        foreach (var metricKey in metricKeys)
        {
            var states = orderedBranches
                .Select(branch => branch.Inner.Metrics.TryGetValue(metricKey, out var value)
                    ? (IsPresent: true, Value: value)
                    : (IsPresent: false, Value: string.Empty))
                .ToArray();
            if (states.Distinct().Count() == 1)
                continue;

            var valuesByBranchId = new SortedDictionary<string, string>(StringComparer.Ordinal);
            for (var index = 0; index < orderedBranches.Count; index++)
            {
                valuesByBranchId.Add(
                    orderedBranches[index].BranchId,
                    FormatConflictValue(states[index]));
            }

            conflicts.Add(new GraphFanInConflict(
                $"{ConflictFieldPrefix}{metricKey}",
                new ReadOnlyDictionary<string, string>(valuesByBranchId)));
        }

        return Array.AsReadOnly(conflicts.ToArray());
    }

    // FormatConflictValue distinguishes missing metrics from any literal string, including marker-like text.
    private static string FormatConflictValue((bool IsPresent, string Value) state)
        => state.IsPresent ? $"P:{state.Value.Length}:{state.Value}" : "M:0:";

    // OrderBranches is the sole branch-order rule: exact ordinal owner key, never caller enumeration order.
    private static GraphAsyncBranchResult[] OrderBranches(IEnumerable<GraphAsyncBranchResult> branches)
        => branches
            .OrderBy(branch => branch.MergeKey, StringComparer.Ordinal)
            .ThenBy(branch => branch.PathOrderRef, StringComparer.Ordinal)
            .ToArray();

    // AggregateSummaryTimes processes the canonical branch order and fails at the first non-finite sum.
    private static OperationResult<SummaryTimeTotals> AggregateSummaryTimes(
        IReadOnlyList<GraphAsyncBranchResult> orderedBranches)
    {
        var baselineTimeMs = 0d;
        var enhancedTimeMs = 0d;
        foreach (var branch in orderedBranches)
        {
            baselineTimeMs += branch.Inner.Summary.BaselineTimeMs;
            if (!double.IsFinite(baselineTimeMs))
            {
                return OperationResult<SummaryTimeTotals>.Fail(
                    "Fan-in BaselineTimeMs aggregation overflowed the finite double contract.",
                    BranchMalformedCode);
            }

            enhancedTimeMs += branch.Inner.Summary.EnhancedTimeMs;
            if (!double.IsFinite(enhancedTimeMs))
            {
                return OperationResult<SummaryTimeTotals>.Fail(
                    "Fan-in EnhancedTimeMs aggregation overflowed the finite double contract.",
                    BranchMalformedCode);
            }
        }

        return OperationResult<SummaryTimeTotals>.Ok(
            new SummaryTimeTotals(baselineTimeMs, enhancedTimeMs));
    }

    // BuildMergedResult returns a deep clone for degenerate fan-in and a conservative aggregate for N > 1.
    private static LogicalFlowResult BuildMergedResult(
        GraphFanInMergeInput input,
        IReadOnlyList<GraphAsyncBranchResult> orderedBranches,
        bool hasConflicts,
        SummaryTimeTotals summaryTimeTotals)
    {
        if (orderedBranches.Count == 1)
        {
            var branch = orderedBranches[0];
            var snapshot = SnapshotFlowResult(branch.Inner);
            var passed = snapshot.Summary.Passed
                && branch.ErrorCodes.Length == 0
                && input.FailureReasonCodes.Length == 0
                && !IsFailureVerdict(snapshot.OverallVerdict)
                && !hasConflicts;
            return snapshot with
            {
                Summary = snapshot.Summary with
                {
                    Passed = passed,
                    ShouldScaleUp = passed && snapshot.Summary.ShouldScaleUp,
                    BaselineTimeMs = summaryTimeTotals.BaselineTimeMs,
                    EnhancedTimeMs = summaryTimeTotals.EnhancedTimeMs
                }
            };
        }

        var worstVerdict = orderedBranches
            .Select(branch => branch.OverallVerdict)
            .Aggregate(SelectWorseVerdict);
        var mergedPassed = orderedBranches.All(branch =>
            branch.Inner.Summary.Passed && branch.ErrorCodes.Length == 0)
            && input.FailureReasonCodes.Length == 0
            && !IsFailureVerdict(worstVerdict)
            && !hasConflicts;
        var minimumConfidence = orderedBranches.Min(branch => branch.Confidence);
        var lowerIsBetterValues = orderedBranches
            .Select(branch => branch.Inner.Summary.LowerIsBetter)
            .Distinct()
            .ToArray();
        var lowerIsBetter = lowerIsBetterValues.Length == 1 && lowerIsBetterValues[0];

        var summary = new LogicalFlowSummary(
            SummaryId: $"fanin:{input.MergeKey}",
            Tier: orderedBranches.Max(branch => branch.Inner.Summary.Tier),
            BaselinePrimary: SelectConservativeMetric(
                orderedBranches.Select(branch => branch.Inner.Summary.BaselinePrimary),
                lowerIsBetterValues),
            EnhancedPrimary: SelectConservativeMetric(
                orderedBranches.Select(branch => branch.Inner.Summary.EnhancedPrimary),
                lowerIsBetterValues),
            PrimaryMetricName: JoinDistinct(
                orderedBranches.Select(branch => branch.Inner.Summary.PrimaryMetricName),
                " | "),
            LowerIsBetter: lowerIsBetter,
            Passed: mergedPassed,
            ShouldScaleUp: mergedPassed && orderedBranches.All(branch => branch.Inner.Summary.ShouldScaleUp),
            NextAction: JoinDistinct(orderedBranches.Select(branch => branch.Inner.Summary.NextAction), " | "),
            ScalingSignal: minimumConfidence,
            ScalingSignalNote: "minimum branch confidence; diagnostic only",
            Notes: JoinDistinct(orderedBranches.Select(branch => branch.Inner.Summary.Notes), " | "),
            BaselineTimeMs: summaryTimeTotals.BaselineTimeMs,
            EnhancedTimeMs: summaryTimeTotals.EnhancedTimeMs);

        return new LogicalFlowResult(
            FlowId: JoinDistinct(orderedBranches.Select(branch => branch.Inner.FlowId), "+"),
            ScenarioId: JoinDistinct(orderedBranches.Select(branch => branch.Inner.ScenarioId), "+"),
            OverallVerdict: worstVerdict,
            FinalHumanResult: BuildFinalHumanResult(orderedBranches),
            Summary: summary,
            Metrics: BuildMergedMetrics(orderedBranches),
            EvidenceIds: StableDistinct(
                orderedBranches.SelectMany(branch => branch.Inner.EvidenceIds)),
            Warnings: StableDistinct(
                orderedBranches.SelectMany(branch =>
                    branch.Inner.Warnings.Concat(branch.ErrorCodes))))
        {
            TransitionSignals = SnapshotTransitionSignals(
                orderedBranches.SelectMany(branch => branch.Inner.TransitionSignals))
        };
    }

    // SelectConservativeMetric chooses the directionally worse value only when direction is unambiguous.
    private static double SelectConservativeMetric(
        IEnumerable<double> values,
        IReadOnlyList<bool> lowerIsBetterValues)
    {
        var snapshot = values.ToArray();
        if (lowerIsBetterValues.Count == 1)
            return lowerIsBetterValues[0] ? snapshot.Max() : snapshot.Min();

        return snapshot.Min();
    }

    // BuildFinalHumanResult deduplicates identical typed output content while retaining every source branch id.
    private static string BuildFinalHumanResult(IReadOnlyList<GraphAsyncBranchResult> orderedBranches)
        => string.Join(
            Environment.NewLine,
            orderedBranches
                .GroupBy(branch => branch.Inner.FinalHumanResult, StringComparer.Ordinal)
                .Select(group =>
                    $"[{string.Join(",", group.Select(branch => branch.BranchId).OrderBy(id => id, StringComparer.Ordinal))}] {group.Key}"));

    // BuildMergedMetrics retains a shared value directly and length-prefixes every conflicting branch value.
    private static IReadOnlyDictionary<string, string> BuildMergedMetrics(
        IReadOnlyList<GraphAsyncBranchResult> orderedBranches)
    {
        var keys = orderedBranches
            .SelectMany(branch => branch.Inner.Metrics.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal);
        var merged = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var key in keys)
        {
            var values = orderedBranches
                .Select(branch => branch.Inner.Metrics.TryGetValue(key, out var value)
                    ? (IsPresent: true, Value: value)
                    : (IsPresent: false, Value: string.Empty))
                .ToArray();
            var distinctValues = values.Distinct().ToArray();
            merged[key] = distinctValues.Length == 1 && distinctValues[0].IsPresent
                ? distinctValues[0].Value
                : FormatBranchValueMap(orderedBranches, values);
        }

        return new ReadOnlyDictionary<string, string>(merged);
    }

    // FormatBranchValueMap is reversible and deterministic even when ids or values contain delimiters.
    private static string FormatBranchValueMap(
        IReadOnlyList<GraphAsyncBranchResult> orderedBranches,
        IReadOnlyList<(bool IsPresent, string Value)> values)
        => string.Join(
            ";",
            orderedBranches.Select((branch, index) =>
                $"{branch.BranchId.Length}:{branch.BranchId}="
                + (values[index].IsPresent
                    ? $"P:{values[index].Value.Length}:{values[index].Value}"
                    : "M:0:")));

    // BuildContext keeps branch fragments in canonical order and uses a fixed timestamp for structural equality.
    private static LogicalContextPacket BuildContext(
        GraphFanInMergeInput input,
        IReadOnlyList<GraphAsyncBranchResult> orderedBranches)
    {
        var fragments = input.UseContextCarryForward
            ? orderedBranches.Select(BuildContextFragment).ToArray()
            : [];

        return new LogicalContextPacket(
            RunId: input.RunId,
            SourceFlowIds: StableDistinct(orderedBranches.Select(branch => branch.Inner.FlowId)),
            SourceScenarioIds: StableDistinct(orderedBranches.Select(branch => branch.Inner.ScenarioId)),
            Fragments: fragments,
            CreatedAtUtc: DateTimeOffset.UnixEpoch);
    }

    // BuildContextFragment snapshots branch-local context and embeds its evidence envelope without overwrites.
    private static LogicalContextFragment BuildContextFragment(GraphAsyncBranchResult branch)
    {
        var metrics = SnapshotDictionary(branch.Inner.Metrics);
        var phaseId = ReadOptionalMetric(metrics, "routed_phase_id");
        var subPhaseId = ReadOptionalMetric(metrics, "routed_sub_phase_id")
            ?? ReadOptionalMetric(metrics, "sub_phase_id");
        var evidence = StableDistinct(
            branch.Inner.EvidenceIds
                .Concat(branch.SourceHashes)
                .Concat(branch.ProvenanceRefs)
                .Concat(branch.TraceRefs));

        return new LogicalContextFragment(
            FlowId: branch.Inner.FlowId,
            ScenarioId: branch.Inner.ScenarioId,
            OverallVerdict: branch.Inner.OverallVerdict,
            FinalHumanResult: branch.Inner.FinalHumanResult,
            PhaseId: phaseId,
            SubPhaseId: subPhaseId,
            Metrics: metrics,
            EvidenceIds: evidence,
            Warnings: StableDistinct(branch.Inner.Warnings.Concat(branch.ErrorCodes)));
    }

    // ReadOptionalMetric maps blank routing metadata to no context phase rather than inventing a value.
    private static string? ReadOptionalMetric(IReadOnlyDictionary<string, string> metrics, string key)
        => metrics.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    // BuildProvenanceRefs preserves input, branch, rejected-alternative, and source-hash evidence stably.
    private static IReadOnlyList<string> BuildProvenanceRefs(
        GraphFanInMergeInput input,
        IEnumerable<GraphAsyncBranchResult> branches,
        IEnumerable<GraphAsyncBranchResult> rejectedAlternatives)
        => Array.AsReadOnly(
            StableDistinct(
                input.SourceRefs
                    .Concat(input.ProvenanceRefs)
                    .Concat(branches.SelectMany(branch => branch.SourceHashes))
                    .Concat(branches.SelectMany(branch => branch.ProvenanceRefs))
                    .Concat(rejectedAlternatives.SelectMany(branch => branch.SourceHashes))
                    .Concat(rejectedAlternatives.SelectMany(branch => branch.ProvenanceRefs))));

    // BuildTraceRefs preserves every trace reference from the merge envelope and typed branch carriers.
    private static IReadOnlyList<string> BuildTraceRefs(
        GraphFanInMergeInput input,
        IEnumerable<GraphAsyncBranchResult> branches,
        IEnumerable<GraphAsyncBranchResult> rejectedAlternatives)
        => Array.AsReadOnly(
            StableDistinct(
                input.TraceRefs
                    .Concat(branches.SelectMany(branch => branch.TraceRefs))
                    .Concat(rejectedAlternatives.SelectMany(branch => branch.TraceRefs))));

    // BuildFailureReasonCodes retains branch and envelope errors and marks preserved conflicts unresolved.
    private static IReadOnlyList<string> BuildFailureReasonCodes(
        GraphFanInMergeInput input,
        IEnumerable<GraphAsyncBranchResult> branches,
        IEnumerable<GraphAsyncBranchResult> rejectedAlternatives,
        bool hasConflicts)
    {
        var values = input.FailureReasonCodes
            .Concat(branches.SelectMany(branch => branch.ErrorCodes))
            .Concat(rejectedAlternatives.SelectMany(branch => branch.ErrorCodes));
        if (hasConflicts)
            values = values.Append(ConflictUnresolvedCode);

        return Array.AsReadOnly(StableDistinct(values));
    }

    // SnapshotBranch severs every caller-owned array, dictionary, record, and transition-signal reference.
    private static GraphAsyncBranchResult SnapshotBranch(GraphAsyncBranchResult branch)
        => new(
            CombinationId: branch.CombinationId,
            BranchId: branch.BranchId,
            MergeKey: branch.MergeKey,
            PathOrderRef: branch.PathOrderRef,
            OverallVerdict: branch.OverallVerdict,
            Confidence: branch.Confidence,
            SourceHashes: StableDistinct(branch.SourceHashes),
            ProvenanceRefs: StableDistinct(branch.ProvenanceRefs),
            TraceRefs: StableDistinct(branch.TraceRefs),
            ErrorCodes: StableDistinct(branch.ErrorCodes),
            Inner: SnapshotFlowResult(branch.Inner));

    // SnapshotFlowResult deep-copies the existing logical result carrier without retaining mutable collections.
    private static LogicalFlowResult SnapshotFlowResult(LogicalFlowResult source)
        => new(
            FlowId: source.FlowId,
            ScenarioId: source.ScenarioId,
            OverallVerdict: source.OverallVerdict,
            FinalHumanResult: source.FinalHumanResult,
            Summary: source.Summary with { },
            Metrics: SnapshotDictionary(source.Metrics),
            EvidenceIds: StableDistinct(source.EvidenceIds),
            Warnings: StableDistinct(source.Warnings))
        {
            TransitionSignals = SnapshotTransitionSignals(source.TransitionSignals)
        };

    // SnapshotTransitionSignal deep-copies the signal's evidence array while retaining its typed domain value.
    private static PhaseTransitionSignal SnapshotTransitionSignal(PhaseTransitionSignal signal)
        => signal with { EvidenceIds = StableDistinct(signal.EvidenceIds) };

    // SnapshotTransitionSignals treats identical advisory signals as a set and emits a stable deep snapshot.
    private static PhaseTransitionSignal[] SnapshotTransitionSignals(
        IEnumerable<PhaseTransitionSignal> signals)
    {
        var snapshots = new SortedDictionary<string, PhaseTransitionSignal>(StringComparer.Ordinal);
        foreach (var signal in signals.Select(SnapshotTransitionSignal))
            snapshots.TryAdd(BuildTransitionSignalKey(signal), signal);

        return snapshots.Values.ToArray();
    }

    // BuildTransitionSignalKey length-prefixes every component so ordering and deduplication are collision-safe.
    private static string BuildTransitionSignalKey(PhaseTransitionSignal signal)
    {
        static string Part(string value) => $"{value.Length}:{value}";
        var evidence = string.Concat(signal.EvidenceIds.Select(Part));
        return string.Concat(
            Part(signal.SuggestedDomain.ToString()),
            Part(signal.TransitionPrompt),
            Part(signal.SourceNodeId),
            Part(signal.SourceSubPhaseId),
            Part(evidence));
    }

    // SnapshotDictionary creates an ordinal read-only dictionary in canonical key order.
    private static IReadOnlyDictionary<string, string> SnapshotDictionary(
        IReadOnlyDictionary<string, string> source)
    {
        var snapshot = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in source.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            snapshot.Add(pair.Key, pair.Value);

        return new ReadOnlyDictionary<string, string>(snapshot);
    }

    // StableDistinct canonicalizes evidence-like sets without using caller enumeration as output order.
    private static string[] StableDistinct(IEnumerable<string> values)
        => values.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();

    // JoinDistinct builds stable aggregate scalar text without picking one branch as authoritative.
    private static string JoinDistinct(IEnumerable<string> values, string separator)
        => string.Join(separator, StableDistinct(values));

    // VerdictRank is the exact existing orchestrator worst-of ordering, including its unknown-verdict fallback.
    private static int VerdictRank(string verdict)
        => verdict switch
        {
            "REJECT" => 6,
            "REPROCESS" => 5,
            "KNOWLEDGE_EDGE" => 4,
            "PASS_WITH_GAPS" => 3,
            "PASS_WITH_CONCERNS" => 2,
            "CONCERN" => 2,
            "PASS" => 1,
            _ => 0
        };

    // IsKnownVerdict pins the accepted status vocabulary to AdaptiveLogicalOrchestrator:913-925.
    private static bool IsKnownVerdict(string verdict)
        => VerdictRank(verdict) != 0;

    // The current owner ranks these three statuses as rejection/reprocess/knowledge failures, not passes.
    private static bool IsFailureVerdict(string verdict)
        => VerdictRank(verdict) >= 4;

    // SummaryTimeTotals carries the exact finite values preflighted for BuildMergedResult.
    private sealed record SummaryTimeTotals(
        double BaselineTimeMs,
        double EnhancedTimeMs);

    // SelectWorseVerdict is commutative: severity wins, and equal severity uses ordinal content only.
    private static string SelectWorseVerdict(string left, string right)
    {
        var leftRank = VerdictRank(left);
        var rightRank = VerdictRank(right);
        if (leftRank != rightRank)
            return leftRank > rightRank ? left : right;

        return string.CompareOrdinal(left, right) <= 0 ? left : right;
    }
}
