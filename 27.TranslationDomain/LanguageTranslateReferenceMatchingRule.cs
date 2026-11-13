// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Globalization;
using AgentInfrastructure;
using ContinuousLearning;

/// <summary>
/// Translation-only adapter from the generic frozen-reference calibration
/// contract to the existing language-translate held-out scorer. It measures
/// candidate/reference agreement; it is not a DPO judge and never turns paid
/// reference bytes into trainable chosen/rejected sides.
/// </summary>
public static class LanguageTranslateReferenceMatchingRule
{
    public const string RuleId = "language-translate-heldout-scorer-reference-match";
    public const string RuleVersion = "v1";

    public static LanguageTranslateReferenceMatchingPolicy StrictPolicy
        => LanguageTranslateReferenceMatchingPolicy.Strict;

    public static OperationResult<ReferenceMatchingRuleIdentity> CreateIdentity()
        => CreateIdentity(StrictPolicy);

    public static OperationResult<ReferenceMatchingRuleIdentity> CreateIdentity(
        LanguageTranslateReferenceMatchingPolicy? policy)
    {
        policy ??= StrictPolicy;
        return ReferenceMatchingRuleIdentity.Create(
            LanguageTranslateHeldOutScorer.DomainTag,
            RuleId,
            BuildRuleVersion(policy),
            BuildHumanDescription(policy));
    }

    public static ReferenceMatchingRulePredicate CreatePredicate()
        => CreatePredicate(StrictPolicy);

    public static ReferenceMatchingRulePredicate CreatePredicate(
        LanguageTranslateReferenceMatchingPolicy? policy)
    {
        policy ??= StrictPolicy;
        return (candidateAnswer, referenceAnswer) =>
            IsMatch(candidateAnswer, referenceAnswer, policy);
    }

    public static bool IsMatch(
        string candidateAnswer,
        string referenceAnswer)
        => IsMatch(candidateAnswer, referenceAnswer, StrictPolicy);

    public static bool IsMatch(
        string candidateAnswer,
        string referenceAnswer,
        LanguageTranslateReferenceMatchingPolicy? policy)
    {
        policy ??= StrictPolicy;

        // The generic delegate order is (candidate, reference). The held-out
        // scorer order is (expected reference, predicted candidate), so the
        // call is intentionally reversed at this boundary.
        var score = LanguageTranslateHeldOutScorer.ScoreOne(
            referenceAnswer,
            candidateAnswer);
        if (!score.IsSuccess || score.Value is null)
            return false;

        return double.IsFinite(score.Value.CandidateScore)
            && score.Value.CandidateScore >= policy.MinimumCandidateScore
            && (!policy.RequireExactMatch || score.Value.ExactMatch);
    }

    public static OperationResult<ReferenceMatchingRuleCalibrationResult>
        EvaluateCalibration(ReferenceMatchingCalibrationInput input)
        => EvaluateCalibration(input, StrictPolicy);

    public static OperationResult<ReferenceMatchingRuleCalibrationResult>
        EvaluateCalibration(
            ReferenceMatchingCalibrationInput input,
            LanguageTranslateReferenceMatchingPolicy? policy)
    {
        if (input is null)
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Language-translate reference matching calibration input is required.",
                "LANGUAGE_TRANSLATE_REFERENCE_MATCHING_CALIBRATION_INPUT_REQUIRED");
        }

        policy ??= StrictPolicy;
        var expectedIdentity = CreateIdentity(policy);
        if (!expectedIdentity.IsSuccess || expectedIdentity.Value is null)
            return expectedIdentity.MapError<ReferenceMatchingRuleCalibrationResult>();

        if (!StringComparer.Ordinal.Equals(
                input.RuleIdentity.IdentitySha256,
                expectedIdentity.Value.IdentitySha256))
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Calibration input rule identity does not match the language-translate reference matching policy.",
                "LANGUAGE_TRANSLATE_REFERENCE_MATCHING_RULE_IDENTITY_MISMATCH");
        }

        return ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
            input,
            CreatePredicate(policy));
    }

    private static string BuildRuleVersion(
        LanguageTranslateReferenceMatchingPolicy policy)
        => string.Join(
            '.',
            RuleVersion,
            "norm-" + IdentityToken(LanguageTranslateHeldOutScorer.NormalizationProfile),
            "metric-" + IdentityToken(LanguageTranslateHeldOutScorer.MetricProfile),
            "policy-" + policy.PolicyId,
            "threshold-" + policy.MinimumCandidateScoreInvariant,
            policy.RequireExactMatch ? "exact-required" : "exact-optional");

    private static string BuildHumanDescription(
        LanguageTranslateReferenceMatchingPolicy policy)
        => "Translation-only reference matching adapter for domain " +
           LanguageTranslateHeldOutScorer.DomainTag +
           ". Uses LanguageTranslateHeldOutScorer normalization profile " +
           LanguageTranslateHeldOutScorer.NormalizationProfile +
           " and metric profile " +
           LanguageTranslateHeldOutScorer.MetricProfile +
           ". Policy " +
           policy.PolicyId +
           " requires candidate score >= " +
           policy.MinimumCandidateScoreInvariant +
           (policy.RequireExactMatch ? " and exact match." : " without exact-match requirement.") +
           " This is a frozen-reference calibration instrument, not a universal domain rule, not training, and not DPO.";

    private static string IdentityToken(string value)
        => string.Concat(value.Select(static character =>
            char.IsAsciiLetterOrDigit(character) ||
            character is '_' or '-' or '.' or ':'
                ? character.ToString()
                : "-"));
}

/// <summary>
/// Explicit matching policy for the translation adapter. The default policy is
/// deliberately strict; looser thresholds are opt-in calibration experiments,
/// not production proof of translation quality.
/// </summary>
public sealed record LanguageTranslateReferenceMatchingPolicy
{
    public const string StrictPolicyId = "strict-exact-score-gte-1";
    public const double StrictMinimumCandidateScore = 1D;
    public const bool StrictRequireExactMatch = true;

    public static LanguageTranslateReferenceMatchingPolicy Strict { get; } =
        new(
            StrictPolicyId,
            StrictMinimumCandidateScore,
            StrictRequireExactMatch);

    private LanguageTranslateReferenceMatchingPolicy(
        string policyId,
        double minimumCandidateScore,
        bool requireExactMatch)
    {
        PolicyId = policyId;
        MinimumCandidateScore = minimumCandidateScore;
        RequireExactMatch = requireExactMatch;
        MinimumCandidateScoreInvariant = FormatThreshold(minimumCandidateScore);
    }

    public string PolicyId { get; }

    public double MinimumCandidateScore { get; }

    public bool RequireExactMatch { get; }

    public string MinimumCandidateScoreInvariant { get; }

    public static OperationResult<LanguageTranslateReferenceMatchingPolicy> Create(
        string policyId,
        double minimumCandidateScore,
        bool requireExactMatch)
    {
        if (!IsSafePolicyId(policyId) ||
            !double.IsFinite(minimumCandidateScore) ||
            minimumCandidateScore < 0D ||
            minimumCandidateScore > 1D)
        {
            return OperationResult<LanguageTranslateReferenceMatchingPolicy>.Fail(
                "Language-translate reference matching policy must carry a safe id and a finite threshold in [0,1].",
                "LANGUAGE_TRANSLATE_REFERENCE_MATCHING_POLICY_INVALID");
        }

        return OperationResult<LanguageTranslateReferenceMatchingPolicy>.Ok(
            new LanguageTranslateReferenceMatchingPolicy(
                policyId,
                minimumCandidateScore,
                requireExactMatch));
    }

    private static string FormatThreshold(double value)
        => value.ToString("0.################", CultureInfo.InvariantCulture);

    private static bool IsSafePolicyId(string value)
        => !string.IsNullOrEmpty(value)
            && value.Length <= 64
            && char.IsAsciiLetterOrDigit(value[0])
            && value.All(static character =>
                char.IsAsciiLetterOrDigit(character)
                || character is '_' or '-' or '.' or ':');
}
