// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompilerAdapter;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SemanticCompiler29;

public sealed record AttributedSegmentInput(
    string SegmentId,
    string Text,
    string[] PredicateIds,
    Domain? ExistingDomain = null);

public sealed record AttributedSegmentDocument(
    string SegmentId,
    string Text,
    string[] PredicateIds,
    Domain Domain,
    double DomainConfidence,
    string[] Evidence);

public sealed class AttributedSegmentRouter : ILogicalUnit<DevelopmentPromptCompilation, AttributedSegmentDocument[]>
{
    public const string Id = "unit-attributed-segment-router-v1";
    public const string Role = "attributed-segment-router";
    public const string DomainTag = "whole_development_area";

    private readonly DomainClassifierUnit _classifier;

    public AttributedSegmentRouter(DomainClassifierUnit? classifier = null)
    {
        _classifier = classifier ?? new DomainClassifierUnit();
    }

    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<AttributedSegmentDocument[]>>> ExecuteAsync(
        LogicalUnitRequest<DevelopmentPromptCompilation> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<AttributedSegmentDocument[]>>.Fail("Cancelled.", "CANCELLED"));

        var segments = request.Input.Segments.Length <= 1
            ? [new AttributedSegmentInput(
                $"{request.RunId}-segment-1",
                request.Input.Text,
                request.Input.DevelopmentPredicateIds,
                request.Input.Domain)]
            : request.Input.Segments
                .Select((segment, index) => new AttributedSegmentInput(
                    $"{request.RunId}-segment-{index + 1}",
                    segment.Text,
                    segment.DevelopmentPredicateIds,
                    segment.Domain))
                .ToArray();

        var routed = Route(segments, request.Metadata);
        var result = new LogicalUnitResult<AttributedSegmentDocument[]>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            routed,
            routed.Length == 0 ? 0.20 : routed.Min(segment => segment.DomainConfidence),
            routed.SelectMany(segment => segment.PredicateIds).Distinct(StringComparer.Ordinal).ToArray(),
            routed.Select(segment => segment.Domain.ToString()).Distinct(StringComparer.Ordinal).ToArray(),
            routed.Select(segment => segment.SegmentId).ToArray(),
            []);

        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<AttributedSegmentDocument[]>>.Ok(result));
    }

    public AttributedSegmentDocument[] Route(
        IReadOnlyList<AttributedSegmentInput> segments,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        if (segments.Count == 0)
            return [];

        return segments
            .Select(segment =>
            {
                var classified = _classifier.Classify(new DomainClassifierRequest(
                    segment.Text,
                    Predicates: DomainClassifierUnit.EncodePredicateProfile(segment.PredicateIds),
                    Metadata: metadata));
                var domain = segment.ExistingDomain is { } existing && existing != Domain.UNKNOWN
                    ? existing
                    : classified.Label;
                return new AttributedSegmentDocument(
                    segment.SegmentId,
                    segment.Text,
                    segment.PredicateIds,
                    domain,
                    classified.Confidence,
                    classified.Evidence);
            })
            .ToArray();
    }
}
