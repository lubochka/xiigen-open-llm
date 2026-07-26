// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SmartSkillsDomain;

public sealed class ExtensionTokenTraceAccountingTests
{
    [Fact]
    public void CreateHeuristicTrace_ProducesRequiredStableFieldsAndHashes()
    {
        var trace = CreateTrace(TokenTraceAccounting.BaselineMode, debugEnabled: true);

        Assert.Equal(TokenTraceAccounting.SchemaVersion, trace.SchemaVersion);
        Assert.Equal("scenario-token-001", trace.ScenarioId);
        Assert.Equal("pair-token-001", trace.PairId);
        Assert.Equal("Claude", trace.HostName);
        Assert.Equal("prompt-to-task", trace.Method);
        Assert.True(trace.DebugEnabled);
        Assert.StartsWith("sha256:", trace.PromptHash, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", trace.VisibleAnswerHash, StringComparison.Ordinal);
        Assert.Equal(TokenTraceAccounting.HeuristicTokenizerName, trace.TokenizerName);
        Assert.Equal(TokenTraceAccounting.HeuristicTokenizerVersion, trace.TokenizerVersion);
        Assert.Equal(TokenTraceAccounting.HeuristicAccountingType, trace.TokenizerAccountingType);
        Assert.True(trace.TokenCountInput > 0);
        Assert.True(trace.TokenCountOutput > 0);
        Assert.Equal(trace.TokenCountInput + trace.TokenCountOutput, trace.TokenCountTotal);
        Assert.Null(trace.QualityScore);
        Assert.Null(trace.QualityRegression);
        Assert.Equal("fresh_isolated_runtime", trace.IsolationMode);
        Assert.True(trace.PromptRedacted);
        Assert.True(trace.AnswerRedacted);
        Assert.False(trace.ContainsRawPrivateContent);
        Assert.False(trace.SemanticQualityClaimed);
        Assert.False(trace.TokenSavingsClaimed);
        Assert.False(trace.LearnabilityClaimed);
        Assert.Equal("artifact://positive-trace", trace.ArtifactRefs["positive"]);
    }

    [Fact]
    public void HashAndHeuristicCounts_AreDeterministic()
    {
        var first = CreateTrace(TokenTraceAccounting.BaselineMode, debugEnabled: false);
        var second = CreateTrace(TokenTraceAccounting.BaselineMode, debugEnabled: false);

        Assert.Equal(first.PromptHash, second.PromptHash);
        Assert.Equal(first.VisibleAnswerHash, second.VisibleAnswerHash);
        Assert.Equal(first.TokenCountInput, second.TokenCountInput);
        Assert.Equal(first.TokenCountOutput, second.TokenCountOutput);
        Assert.Equal(6, TokenTraceAccounting.CountHeuristicTokens("Plan API-01, then test."));
    }

    [Fact]
    public void DebugOnOff_IsRepresentedInTraceAndPairingRequiresMatch()
    {
        var baseline = CreateTrace(TokenTraceAccounting.BaselineMode, debugEnabled: true);
        var adaptive = CreateTrace(TokenTraceAccounting.AdaptiveMode, debugEnabled: false);

        var result = TokenTraceAccounting.ValidateBaselineAdaptivePair(baseline, adaptive);

        Assert.False(result.IsValid);
        Assert.Contains("debug_enabled_must_match_between_baseline_and_adaptive", result.Errors);
    }

    [Fact]
    public void BaselineAdaptivePair_RequiresSamePromptHostMethodTokenizerAndDebug()
    {
        var baseline = CreateTrace(TokenTraceAccounting.BaselineMode, debugEnabled: true);
        var adaptive = CreateTrace(TokenTraceAccounting.AdaptiveMode, debugEnabled: true);

        var result = TokenTraceAccounting.ValidateBaselineAdaptivePair(baseline, adaptive);

        Assert.True(result.IsValid);
        Assert.DoesNotContain("host_name_must_match_between_baseline_and_adaptive", result.Errors);
        Assert.DoesNotContain("method_must_match_between_baseline_and_adaptive", result.Errors);
        Assert.DoesNotContain("prompt_hash_must_match_between_baseline_and_adaptive", result.Errors);
        Assert.DoesNotContain("tokenizer_name_must_match_between_baseline_and_adaptive", result.Errors);
        Assert.DoesNotContain("tokenizer_version_must_match_between_baseline_and_adaptive", result.Errors);
        Assert.DoesNotContain("tokenizer_accounting_type_must_match_between_baseline_and_adaptive", result.Errors);
    }

    [Fact]
    public void TokenDeltaAlone_CannotPassQualityOrClaimSavings()
    {
        var baseline = CreateTrace(TokenTraceAccounting.BaselineMode, debugEnabled: true);
        var adaptive = CreateTrace(TokenTraceAccounting.AdaptiveMode, debugEnabled: true) with
        {
            TokenSavingsClaimed = true
        };

        var result = TokenTraceAccounting.ValidateBaselineAdaptivePair(baseline, adaptive);
        var qualityGate = TokenTraceAccounting.EvaluateQualityGate(baseline, adaptive);

        Assert.False(result.IsValid);
        Assert.Contains("token_savings_claim_forbidden_in_trace_schema_phase", result.Errors);
        Assert.False(qualityGate.CanPass);
        Assert.Contains("token_delta_alone_cannot_pass_quality", qualityGate.Reasons);
        Assert.Contains("quality_regression_requires_approved_quality_rubric", qualityGate.Reasons);
    }

    private static ExtensionTokenTrace CreateTrace(string mode, bool debugEnabled)
        => TokenTraceAccounting.CreateHeuristicTrace(new ExtensionTokenTraceInput(
            ScenarioId: "scenario-token-001",
            PairId: "pair-token-001",
            Mode: mode,
            HostName: "Claude",
            Method: "prompt-to-task",
            DebugEnabled: debugEnabled,
            PromptText: "Turn this user prompt into a universal task.",
            VisibleAnswerText: "Structured task created without semantic claim.",
            IsolationMode: "fresh_isolated_runtime",
            ArtifactRefs: new Dictionary<string, string>
            {
                ["positive"] = "artifact://positive-trace"
            },
            TraceStartedAtUtc: DateTimeOffset.Parse("2026-06-16T10:00:00Z"),
            TraceEndedAtUtc: DateTimeOffset.Parse("2026-06-16T10:00:01Z")));
}
