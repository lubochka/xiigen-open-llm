// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;

// Supplies one materialized node-output snapshot for one adaptive graph invocation.
public interface IDynamicGraphNodeOutputSource
{
    OperationResult<DynamicGraphNodeOutputSnapshot> ReadForInvocation(string runId);
}

// Owns one invocation's node-output candidates without retaining caller-owned arrays or list views.
public sealed class DynamicGraphNodeOutputSnapshot
{
    private readonly GraphNodeCandidate[] _candidates;

    public DynamicGraphNodeOutputSnapshot(
        string runId,
        long topologyRevision,
        string sourceNodeId,
        IReadOnlyList<GraphNodeCandidate> candidates)
    {
        if (candidates is null)
            throw new ArgumentNullException(nameof(candidates));

        RunId = runId;
        TopologyRevision = topologyRevision;
        SourceNodeId = sourceNodeId;

        _candidates = new GraphNodeCandidate[candidates.Count];
        for (var index = 0; index < candidates.Count; index++)
        {
            var candidate = candidates[index]
                ?? throw new ArgumentNullException(nameof(candidates), "Dynamic graph node-output candidates cannot contain null entries.");
            _candidates[index] = CopyCandidate(candidate);
        }
    }

    // Scalar values remain unnormalized so the runtime boundary can fail closed on invalid source data.
    public string RunId { get; }

    public long TopologyRevision { get; }

    public string SourceNodeId { get; }

    // Each read receives a fresh deep copy, including every nested candidate array.
    public IReadOnlyList<GraphNodeCandidate> Candidates
    {
        get
        {
            var snapshots = new GraphNodeCandidate[_candidates.Length];
            for (var index = 0; index < _candidates.Length; index++)
                snapshots[index] = CopyCandidate(_candidates[index]);

            return new ReadOnlyCollection<GraphNodeCandidate>(snapshots);
        }
    }

    private static GraphNodeCandidate CopyCandidate(GraphNodeCandidate source)
        => new(
            source.CandidateId,
            source.UnitId,
            CopyArray(source.UnitIds),
            source.LogicalUnitRef,
            CopyArray(source.OutputRefs),
            source.OutputType,
            source.Confidence,
            source.Score,
            CopyArray(source.ProvenanceRefs),
            source.PathOrderRef,
            source.LegalStatus,
            source.Status,
            CopyArray(source.ErrorCodes),
            CopyArray(source.SourceIds),
            source.TraceRef,
            CopyArray(source.TraceRefs),
            CopyArray(source.EvidenceIds),
            source.B1TeacherJudgeRef,
            source.B7GraphOrderRef,
            source.B11AsyncBatchRef,
            source.B12WinnerSelectionRef,
            source.B6UnitDpoBuilderRef,
            source.SemanticQualityClaimed);

    // Preserve malformed null nested carriers for downstream validation without exposing caller arrays.
    private static string[] CopyArray(string[]? source)
        => source is null ? null! : source.ToArray();
}
