// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;

namespace SmartSkillsDomain;

public sealed record ExtensionTokenTraceInput(
    string ScenarioId,
    string PairId,
    string Mode,
    string HostName,
    string Method,
    bool DebugEnabled,
    string PromptText,
    string VisibleAnswerText,
    string IsolationMode,
    IReadOnlyDictionary<string, string> ArtifactRefs,
    DateTimeOffset TraceStartedAtUtc,
    DateTimeOffset TraceEndedAtUtc);

public sealed record ExtensionTokenTrace(
    string SchemaVersion,
    string ScenarioId,
    string PairId,
    string Mode,
    string HostName,
    string Method,
    bool DebugEnabled,
    string PromptHash,
    string VisibleAnswerHash,
    string TokenizerName,
    string TokenizerVersion,
    string TokenizerAccountingType,
    int TokenCountInput,
    int TokenCountOutput,
    int TokenCountTotal,
    double? QualityScore,
    bool? QualityRegression,
    string IsolationMode,
    DateTimeOffset TraceStartedAtUtc,
    DateTimeOffset TraceEndedAtUtc,
    IReadOnlyDictionary<string, string> ArtifactRefs,
    bool PromptRedacted,
    bool AnswerRedacted,
    bool ContainsRawPrivateContent,
    bool SemanticQualityClaimed,
    bool TokenSavingsClaimed,
    bool LearnabilityClaimed);

public sealed record TokenTracePairValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors);

public sealed record TokenTraceQualityGateResult(
    bool CanPass,
    IReadOnlyList<string> Reasons);

public static class TokenTraceAccounting
{
    public const string SchemaVersion = "xiigen.extension_token_trace.v1";
    public const string HeuristicTokenizerName = "xiigen-heuristic-word-punctuation";
    public const string HeuristicTokenizerVersion = "v1";
    public const string HeuristicAccountingType = "heuristic";
    public const string BaselineMode = "baseline";
    public const string AdaptiveMode = "adaptive";

    public static ExtensionTokenTrace CreateHeuristicTrace(ExtensionTokenTraceInput input)
    {
        var inputCount = CountHeuristicTokens(input.PromptText);
        var outputCount = CountHeuristicTokens(input.VisibleAnswerText);

        return new ExtensionTokenTrace(
            SchemaVersion: SchemaVersion,
            ScenarioId: input.ScenarioId,
            PairId: input.PairId,
            Mode: input.Mode,
            HostName: input.HostName,
            Method: input.Method,
            DebugEnabled: input.DebugEnabled,
            PromptHash: HashText(input.PromptText),
            VisibleAnswerHash: HashText(input.VisibleAnswerText),
            TokenizerName: HeuristicTokenizerName,
            TokenizerVersion: HeuristicTokenizerVersion,
            TokenizerAccountingType: HeuristicAccountingType,
            TokenCountInput: inputCount,
            TokenCountOutput: outputCount,
            TokenCountTotal: inputCount + outputCount,
            QualityScore: null,
            QualityRegression: null,
            IsolationMode: input.IsolationMode,
            TraceStartedAtUtc: input.TraceStartedAtUtc,
            TraceEndedAtUtc: input.TraceEndedAtUtc,
            ArtifactRefs: input.ArtifactRefs,
            PromptRedacted: true,
            AnswerRedacted: true,
            ContainsRawPrivateContent: false,
            SemanticQualityClaimed: false,
            TokenSavingsClaimed: false,
            LearnabilityClaimed: false);
    }

    public static int CountHeuristicTokens(string text)
    {
        var count = 0;
        var inToken = false;

        foreach (var character in text)
        {
            if (char.IsLetterOrDigit(character) || character == '_' || character == '-')
            {
                if (!inToken)
                {
                    count++;
                    inToken = true;
                }

                continue;
            }

            inToken = false;
            if (!char.IsWhiteSpace(character))
            {
                count++;
            }
        }

        return count;
    }

    public static string HashText(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return "sha256:" + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static TokenTracePairValidationResult ValidateBaselineAdaptivePair(
        ExtensionTokenTrace baseline,
        ExtensionTokenTrace adaptive)
    {
        var errors = new List<string>();

        RequireEqual(errors, "scenario_id", baseline.ScenarioId, adaptive.ScenarioId);
        RequireEqual(errors, "pair_id", baseline.PairId, adaptive.PairId);
        RequireEqual(errors, "host_name", baseline.HostName, adaptive.HostName);
        RequireEqual(errors, "method", baseline.Method, adaptive.Method);
        RequireEqual(errors, "debug_enabled", baseline.DebugEnabled, adaptive.DebugEnabled);
        RequireEqual(errors, "prompt_hash", baseline.PromptHash, adaptive.PromptHash);
        RequireEqual(errors, "tokenizer_name", baseline.TokenizerName, adaptive.TokenizerName);
        RequireEqual(errors, "tokenizer_version", baseline.TokenizerVersion, adaptive.TokenizerVersion);
        RequireEqual(errors, "tokenizer_accounting_type", baseline.TokenizerAccountingType, adaptive.TokenizerAccountingType);

        if (!string.Equals(baseline.Mode, BaselineMode, StringComparison.Ordinal))
        {
            errors.Add("baseline_mode_required");
        }

        if (!string.Equals(adaptive.Mode, AdaptiveMode, StringComparison.Ordinal))
        {
            errors.Add("adaptive_mode_required");
        }

        if (baseline.TokenSavingsClaimed || adaptive.TokenSavingsClaimed)
        {
            errors.Add("token_savings_claim_forbidden_in_trace_schema_phase");
        }

        if (baseline.SemanticQualityClaimed || adaptive.SemanticQualityClaimed)
        {
            errors.Add("semantic_quality_claim_forbidden_in_trace_schema_phase");
        }

        if (baseline.LearnabilityClaimed || adaptive.LearnabilityClaimed)
        {
            errors.Add("learnability_claim_forbidden_in_trace_schema_phase");
        }

        return new TokenTracePairValidationResult(errors.Count == 0, errors);
    }

    public static TokenTraceQualityGateResult EvaluateQualityGate(
        ExtensionTokenTrace baseline,
        ExtensionTokenTrace adaptive)
    {
        var reasons = new List<string>();
        var pairValidation = ValidateBaselineAdaptivePair(baseline, adaptive);
        if (!pairValidation.IsValid)
        {
            reasons.AddRange(pairValidation.Errors);
        }

        if (baseline.QualityScore is null || adaptive.QualityScore is null)
        {
            reasons.Add("token_delta_alone_cannot_pass_quality");
        }

        if (baseline.QualityRegression is null || adaptive.QualityRegression is null)
        {
            reasons.Add("quality_regression_requires_approved_quality_rubric");
        }

        return new TokenTraceQualityGateResult(reasons.Count == 0, reasons);
    }

    private static void RequireEqual<T>(List<string> errors, string fieldName, T baseline, T adaptive)
    {
        if (!EqualityComparer<T>.Default.Equals(baseline, adaptive))
        {
            errors.Add($"{fieldName}_must_match_between_baseline_and_adaptive");
        }
    }
}
