// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using TrainableRouter.Contracts;

public sealed class BaselineKeywordCandidateGenerator : IUnitScorer
{
    private readonly IReadOnlyDictionary<string, UnitScoringProfile> _profiles;
    internal const double DomainBoost = 20.0, DirectionBoost = 2.0, MaximumConfidence = 0.99, BaseConfidence = 0.20, // Normal scoring and passive input description share these existing boosts and confidence bounds.
        KeywordConfidenceMultiplier = 0.12, DomainConfidenceMultiplier = 0.02, DirectionConfidenceMultiplier = 0.08; // Describe and scoring share the actual constants.

    public BaselineKeywordCandidateGenerator(IReadOnlyDictionary<string, UnitScoringProfile> profiles)
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

        var text = $"{request.OriginalPrompt} {request.TaskSlice} {request.RequiredDirection} {request.DomainHint}"
            .Replace(RoutingProfileTrainer.CSharpFrom, RoutingProfileTrainer.CSharpTo, StringComparison.OrdinalIgnoreCase) // FIX-KR: read the key in the spelling the lesson wrote it in; the writer already stores "C#" as "csharp".
            .Replace(RoutingProfileTrainer.DotNetFrom, RoutingProfileTrainer.DotNetTo, StringComparison.OrdinalIgnoreCase) // The same replacement the writer applies to ".NET", in the writer's own order, from the writer's own constants.
            .ToLowerInvariant(); // Lowercase last, exactly as before, so an already canonical request is left unchanged.
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
        {
            return new UnitScoringResult(unitId, "unknown", "unknown", 0, 0.01)
            {
                AuthorityRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
            };
        }

        var hits = profile.Keywords.Count(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        var domainBoost = string.Equals(request.DomainHint, profile.DomainTag, StringComparison.OrdinalIgnoreCase) ? DomainBoost : 0.0; // Apply the unchanged domain match boost through the same constant exposed by input description.
        var directionBoost = !string.IsNullOrWhiteSpace(request.RequiredDirection)
            && profile.Keywords.Any(keyword => string.Equals(keyword, request.RequiredDirection, StringComparison.OrdinalIgnoreCase))
                ? DirectionBoost // Apply the unchanged direction keyword boost when the request matches.
                : 0.0;
        var score = profile.BaseScore + hits + domainBoost + directionBoost;
        var confidence = Math.Min(MaximumConfidence, Math.Max(profile.ConfidenceFloor, BaseConfidence + (hits * KeywordConfidenceMultiplier) + (domainBoost * DomainConfidenceMultiplier) + (directionBoost * DirectionConfidenceMultiplier) + profile.BaseScore)); // Preserve the existing confidence formula, floor and cap while sharing every described multiplier.
        return new UnitScoringResult(unitId, profile.UnitRole, profile.DomainTag, score, confidence)
        {
            AuthorityRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
        };
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
