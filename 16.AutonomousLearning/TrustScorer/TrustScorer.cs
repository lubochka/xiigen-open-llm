// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AutonomousLearning;

using AgentInfrastructure;
using CoreSourceFeatures = AgentInfrastructure.Logical.SourceFeatures;
using CoreTrustLevel = AgentInfrastructure.Logical.TrustLevel;
using CoreTrustScorerOutput = AgentInfrastructure.Logical.TrustScorerOutput;

// SourceFeatures is the legacy project-16 feature record kept for existing callers.
public sealed record SourceFeatures(
    bool IsInternal,
    bool HasCIPassing,
    bool HasTests,
    int UpdateDaysAgo,
    int PopularityScore,
    bool IsOfficialDocs,
    bool DomainMatch);

// TrustScorerOutput is the legacy project-16 output record kept for existing callers.
public sealed record TrustScorerOutput(double Score, TrustLevel Level);

// TrustScorer implements the shared E40V2 contract while preserving project-16 callers.
public sealed class TrustScorer : BaseService, AgentInfrastructure.Logical.ITrustScorer
{
    private const double W_IsInternal = 0.30;
    private const double W_HasCIPassing = 0.20;
    private const double W_HasTests = 0.15;
    private const double W_IsOfficialDocs = 0.35;
    private const double W_DomainMatch = 0.10;
    private const double W_PopularityUnit = 0.05;
    private const double W_UpdateDayUnit = 0.001;
    private const double HighThreshold = 0.70;
    private const double MediumThreshold = 0.30;

    public TrustScorer(ISessionStorage storage) : base(storage) { }

    // Score the legacy project-16 feature record.
    public OperationResult<TrustScorerOutput> Score(SourceFeatures features)
    {
        var scored = ScoreCore(
            features.IsInternal,
            features.HasCIPassing,
            features.HasTests,
            features.UpdateDaysAgo,
            features.PopularityScore,
            features.IsOfficialDocs,
            features.DomainMatch);

        var level = scored.Level switch
        {
            CoreTrustLevel.High => TrustLevel.High,
            CoreTrustLevel.Medium => TrustLevel.Medium,
            _ => TrustLevel.Low
        };

        return OperationResult<TrustScorerOutput>.Ok(new TrustScorerOutput(scored.Score, level));
    }

    // Score the shared core feature record consumed by project 21.
    public OperationResult<CoreTrustScorerOutput> Score(CoreSourceFeatures features)
    {
        var scored = ScoreCore(
            features.IsInternal,
            features.HasCIPassing,
            features.HasTests,
            features.UpdateDaysAgo,
            features.PopularityScore,
            features.IsOfficialDocs,
            features.DomainMatch);

        return OperationResult<CoreTrustScorerOutput>.Ok(new CoreTrustScorerOutput(scored.Score, scored.Level));
    }

    private static (double Score, CoreTrustLevel Level) ScoreCore(
        bool isInternal,
        bool hasCIPassing,
        bool hasTests,
        int updateDaysAgo,
        int popularityScore,
        bool isOfficialDocs,
        bool domainMatch)
    {
        var score = 0.0;
        if (isInternal) score += W_IsInternal;
        if (hasCIPassing) score += W_HasCIPassing;
        if (hasTests) score += W_HasTests;
        if (isOfficialDocs) score += W_IsOfficialDocs;
        if (domainMatch) score += W_DomainMatch;
        score += popularityScore * W_PopularityUnit;
        score -= updateDaysAgo * W_UpdateDayUnit;
        score = Math.Clamp(score, 0.0, 1.0);

        var level = score >= HighThreshold ? CoreTrustLevel.High :
                    score >= MediumThreshold ? CoreTrustLevel.Medium :
                    CoreTrustLevel.Low;
        return (score, level);
    }
}
