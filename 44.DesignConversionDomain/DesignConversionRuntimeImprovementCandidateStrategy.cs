// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using AgentInfrastructure;

public sealed class DesignConversionRuntimeImprovementCandidateStrategy : IDesignConversionImprovementCandidateStrategy
{
    public const string StrategyMarker = "design_conversion_runtime_improvement_candidate_strategy";
    public const string ChangedCandidateMarker = "design_conversion_runtime_improvement_changed_candidate";
    public const string NoExternalProviderMarker = "runtime_improvement_candidate_no_external_provider";
    public const string CandidateUnchangedErrorCode = "RUNTIME_IMPROVEMENT_CANDIDATE_UNCHANGED";

    public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
        DesignConversionCandidateUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "DesignConversion runtime improvement candidate strategy cancelled.",
                "CANCELLED"));
        }

        var validation = Validate(request);
        if (!validation.IsSuccess)
            return Task.FromResult(validation.MapError<DesignConversionImprovementCandidate>());

        var current = request.CurrentRequest;
        var evaluation = request.CurrentEvaluation;
        var retryFindings = evaluation.VisualQaBoundaryTrace.VisualDecision.CorrectionFindings
            .Where(finding => finding.CanDriveRetry)
            .ToArray();
        var improvedHtml = evaluation.ImprovedHtml;
        var improvedCss = evaluation.ImprovedCss;
        var htmlChanged = !string.Equals(improvedHtml, current.Html, StringComparison.Ordinal);
        var cssChanged = !string.Equals(improvedCss, current.Css, StringComparison.Ordinal);
        if (!htmlChanged && !cssChanged)
        {
            return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "DesignConversion runtime improvement candidate strategy found no visual retry finding that produced a changed candidate.",
                CandidateUnchangedErrorCode));
        }

        var nextAttempt = request.CompletedAttemptNumber + 1;
        var candidateId = $"{SanitizeIdentifier(request.LoopId)}:attempt-{nextAttempt.ToString(CultureInfo.InvariantCulture)}:runtime-improvement-candidate";
        var requestId = $"{SanitizeIdentifier(current.RequestId)}:runtime-improvement:{nextAttempt.ToString(CultureInfo.InvariantCulture)}";
        var categories = retryFindings
            .Select(finding => finding.Category)
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(category => category, StringComparer.Ordinal)
            .ToArray();

        return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Ok(
            new DesignConversionImprovementCandidate(
                CandidateId: candidateId,
                RequestId: requestId,
                Html: improvedHtml,
                Css: improvedCss,
                ChangeReasons: BuildChangeReasons(retryFindings),
                EvidenceRefs: evaluation.RuntimeFlowResult.EvidenceRefs
                    .Concat(retryFindings.Select(finding => $"visual-correction-finding:{finding.FindingId}"))
                    .Append(StrategyMarker)
                    .Append(ChangedCandidateMarker)
                    .Append(NoExternalProviderMarker)
                    .Append("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE")
                    .Append("DETERMINISTIC_VISUAL_FINDING_FALLBACK_DISABLED_TRUE")
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(reference => reference, StringComparer.Ordinal)
                    .ToArray(),
                Metadata: new Dictionary<string, string>
                {
                    ["candidate_strategy"] = StrategyMarker,
                    ["candidate_changed"] = bool.TrueString,
                    ["candidate_html_changed"] = htmlChanged.ToString(),
                    ["candidate_css_changed"] = cssChanged.ToString(),
                    ["runtime_flow_candidate_used"] = bool.TrueString,
                    ["visual_finding_fallback_applied"] = bool.FalseString,
                    ["visual_finding_fallback_intents"] = string.Empty,
                    ["visual_finding_fallback_conflicts"] = string.Empty,
                    ["deterministic_visual_finding_fallback_disabled"] = bool.TrueString,
                    ["visual_retry_finding_count"] = retryFindings.Length.ToString(CultureInfo.InvariantCulture),
                    ["visual_retry_finding_categories"] = string.Join("|", categories),
                    ["external_provider_called"] = bool.FalseString,
                    ["network_call_executed"] = bool.FalseString,
                    ["learned_improvement_claimed"] = bool.FalseString,
                    ["deterministic_logic_claimed_as_learning"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString,
                    ["raw_html_included"] = bool.FalseString,
                    ["raw_css_included"] = bool.FalseString
                })));
    }

    private static OperationResult<bool> Validate(DesignConversionCandidateUpdateRequest request)
    {
        if (request is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion candidate update request is required.",
                "CANDIDATE_UPDATE_REQUEST_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.LoopId))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion candidate update loop id is required.",
                "CANDIDATE_UPDATE_LOOP_ID_REQUIRED");
        }

        if (request.CurrentRequest is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion candidate update current request is required.",
                "CANDIDATE_UPDATE_CURRENT_REQUEST_REQUIRED");
        }

        if (request.CurrentEvaluation is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion candidate update current evaluation is required.",
                "CANDIDATE_UPDATE_CURRENT_EVALUATION_REQUIRED");
        }

        if (request.CurrentEvaluation.RuntimeFlowResult is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion candidate update runtime flow result is required.",
                "CANDIDATE_UPDATE_RUNTIME_FLOW_REQUIRED");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static IReadOnlyList<string> BuildChangeReasons(
        IReadOnlyList<DesignConversionVisualCorrectionFinding> retryFindings)
    {
        var reasons = new List<string>
        {
            "Runtime improvement loop accepted changed runtime-flow HTML/CSS bytes; no external provider, trained learning, or ProductReady claim."
        };

        reasons.AddRange(retryFindings.Select(finding =>
            $"Visual retry finding category {SanitizeMetadataToken(finding.Category)} considered without raw issue text."));
        return reasons
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string SanitizeMetadataToken(string value)
    {
        var chars = (value ?? string.Empty)
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '_' or '.' or ':' or '-' ? ch : '_')
            .ToArray();
        var sanitized = new string(chars).Trim('_');
        return string.IsNullOrWhiteSpace(sanitized) ? "unspecified" : sanitized;
    }

    private static string SanitizeIdentifier(string value)
    {
        var chars = (value ?? string.Empty)
            .Trim()
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or ':' ? ch : '-')
            .ToArray();
        var sanitized = new string(chars).Trim('-');
        return string.IsNullOrWhiteSpace(sanitized) ? "design-conversion-loop" : sanitized;
    }
}
