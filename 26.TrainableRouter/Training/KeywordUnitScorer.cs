// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using TrainableRouter.Contracts;

public sealed class KeywordUnitScorer : IUnitScorer
{
    private readonly IReadOnlyDictionary<string, UnitScoringProfile> _profiles;

    public KeywordUnitScorer(IReadOnlyDictionary<string, UnitScoringProfile> profiles)
    {
        _profiles = profiles;
    }

    public Task<OperationResult<UnitScoringResult[]>> ScoreAsync(
        RoutingRequest request,
        CancellationToken ct = default)
    {
        if (request is null)
            return Task.FromResult(OperationResult<UnitScoringResult[]>.Fail("Routing request is required.", "ROUTING_REQUEST_REQUIRED"));

        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<UnitScoringResult[]>.Fail("Routing score was cancelled.", "ROUTING_SCORE_CANCELLED"));

        if (request.CandidateUnitIds.Length == 0)
            return Task.FromResult(OperationResult<UnitScoringResult[]>.Fail("At least one candidate unit is required.", "NO_CANDIDATE_UNITS"));

        var text = $"{request.OriginalPrompt} {request.TaskSlice} {request.RequiredDirection} {request.DomainHint}".ToLowerInvariant();
        var scored = request.CandidateUnitIds
            .Select(unitId => Score(unitId, text, request))
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.UnitId, StringComparer.Ordinal)
            .ToArray();

        return Task.FromResult(OperationResult<UnitScoringResult[]>.Ok(scored));
    }

    private UnitScoringResult Score(string unitId, string text, RoutingRequest request)
    {
        if (!_profiles.TryGetValue(unitId, out var profile))
            return new UnitScoringResult(unitId, "unknown", "unknown", 0, 0.01);

        var hits = profile.Keywords.Count(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        var domainBoost = string.Equals(request.DomainHint, profile.DomainTag, StringComparison.OrdinalIgnoreCase) ? 20.0 : 0.0;
        var directionBoost = !string.IsNullOrWhiteSpace(request.RequiredDirection)
            && profile.Keywords.Any(keyword => string.Equals(keyword, request.RequiredDirection, StringComparison.OrdinalIgnoreCase))
                ? 2.0
                : 0.0;
        var score = profile.BaseScore + hits + domainBoost + directionBoost;
        var confidence = Math.Min(0.99, Math.Max(profile.ConfidenceFloor, 0.20 + (hits * 0.12) + (domainBoost * 0.02) + (directionBoost * 0.08) + profile.BaseScore));
        return new UnitScoringResult(unitId, profile.UnitRole, profile.DomainTag, score, confidence);
    }
}

public sealed record UnitScoringProfile(
    string UnitId,
    string UnitRole,
    string DomainTag,
    string[] Keywords,
    string[] AllowedInputs,
    double ConfidenceFloor,
    string MetadataSourceRef,
    double BaseScore = 0.0);
