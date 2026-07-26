// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Text.RegularExpressions;
using AgentInfrastructure;

public sealed class DesignConversionRuntimeImprovementCandidateStrategy : IDesignConversionImprovementCandidateStrategy
{
    public const string StrategyMarker = "design_conversion_runtime_improvement_candidate_strategy";
    public const string ChangedCandidateMarker = "design_conversion_runtime_improvement_changed_candidate";
    public const string NoExternalProviderMarker = "runtime_improvement_candidate_no_external_provider";
    public const string CandidateUnchangedErrorCode = "RUNTIME_IMPROVEMENT_CANDIDATE_UNCHANGED";
    public const string GeneratedBlockAmbiguousErrorCode = "RUNTIME_IMPROVEMENT_GENERATED_BLOCK_AMBIGUOUS";
    private const string LoopImproverClass = "dcm-loop-improved";
    private const string LoopImproverBlockPrefix = ".dcm-loop-improved{";
    private const int MaxFallbackCssScanLength = 1_000_000;
    private static readonly ISet<string> KnownGeneratedFallbackDeclarations = new HashSet<string>(StringComparer.Ordinal)
    {
        "background-color:#ffffff",
        "box-sizing:border-box",
        "color:#111827",
        "font-size:14px",
        "font-size:18px",
        "gap:0",
        "gap:12px",
        "height:auto",
        "line-height:1.5",
        "margin:0",
        "margin:8px",
        "max-height:100%",
        "max-width:100%",
        "min-height:44px",
        "overflow-wrap:anywhere",
        "padding:0",
        "padding:16px",
        "width:100%",
        "width:auto"
    };

    private enum FallbackDirection
    {
        Increase,
        Decrease
    }

    private sealed record FallbackIntent(string Property, FallbackDirection Direction)
    {
        public string Key => $"{Property}:{Direction.ToString().ToLowerInvariant()}";
    }

    private sealed record FallbackDeclarationPlan(
        IReadOnlyList<string> Declarations,
        IReadOnlyList<FallbackIntent> Intents,
        IReadOnlyList<string> Conflicts);

    private sealed record LoopImproverBlock(
        int Start,
        int EndExclusive,
        string Declarations);

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
        var improvedHtml = string.IsNullOrWhiteSpace(evaluation.ImprovedHtml)
            ? current.Html
            : evaluation.ImprovedHtml;
        var improvedCss = string.IsNullOrWhiteSpace(evaluation.ImprovedCss)
            ? current.Css
            : evaluation.ImprovedCss;
        var runtimeHtmlChanged = !string.Equals(improvedHtml, current.Html, StringComparison.Ordinal);
        var runtimeCssChanged = !string.Equals(improvedCss, current.Css, StringComparison.Ordinal);
        var fallbackApplied = false;
        FallbackDeclarationPlan? fallbackPlan = null;

        if (!runtimeHtmlChanged && !runtimeCssChanged && retryFindings.Length > 0)
        {
            fallbackPlan = BuildFallbackDeclarationPlan(retryFindings);
            if (fallbackPlan.Declarations.Count > 0)
            {
                var fallbackCss = ApplyLoopImproverCss(current.Css, fallbackPlan);
                if (!fallbackCss.IsSuccess)
                    return Task.FromResult(fallbackCss.MapError<DesignConversionImprovementCandidate>());

                improvedHtml = EnsureLoopImproverClass(current.Html);
                improvedCss = fallbackCss.Value!;
                fallbackApplied = true;
            }
        }

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
                ChangeReasons: BuildChangeReasons(retryFindings, fallbackApplied, fallbackPlan),
                EvidenceRefs: evaluation.RuntimeFlowResult.EvidenceRefs
                    .Concat(retryFindings.Select(finding => $"visual-correction-finding:{finding.FindingId}"))
                    .Concat((fallbackPlan?.Intents ?? []).Select(intent => $"visual-fallback-intent:{intent.Key}"))
                    .Concat((fallbackPlan?.Conflicts ?? []).Select(conflict => $"visual-fallback-conflict:{conflict}"))
                    .Append(StrategyMarker)
                    .Append(ChangedCandidateMarker)
                    .Append(NoExternalProviderMarker)
                    .Append("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE")
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(reference => reference, StringComparer.Ordinal)
                    .ToArray(),
                Metadata: new Dictionary<string, string>
                {
                    ["candidate_strategy"] = StrategyMarker,
                    ["candidate_changed"] = bool.TrueString,
                    ["candidate_html_changed"] = htmlChanged.ToString(),
                    ["candidate_css_changed"] = cssChanged.ToString(),
                    ["runtime_flow_candidate_used"] = (!fallbackApplied).ToString(),
                    ["visual_finding_fallback_applied"] = fallbackApplied.ToString(),
                    ["visual_finding_fallback_intents"] = fallbackPlan is null
                        ? string.Empty
                        : string.Join("|", fallbackPlan.Intents.Select(intent => intent.Key)),
                    ["visual_finding_fallback_conflicts"] = fallbackPlan is null
                        ? string.Empty
                        : string.Join("|", fallbackPlan.Conflicts),
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
        IReadOnlyList<DesignConversionVisualCorrectionFinding> retryFindings,
        bool fallbackApplied,
        FallbackDeclarationPlan? fallbackPlan)
    {
        var reasons = new List<string>
        {
            "Runtime improvement loop produced a changed local HTML/CSS candidate from visual findings; no external provider or ProductReady claim."
        };

        if (fallbackApplied && fallbackPlan is not null)
        {
            reasons.Add("Fallback visible loop class/CSS applied because runtime flow output did not change.");
            reasons.AddRange(fallbackPlan.Intents.Select(intent =>
                $"Fallback visual intent {intent.Property}:{intent.Direction.ToString().ToLowerInvariant()} applied with bounded CSS target."));
        }

        if (fallbackPlan is not null)
        {
            reasons.AddRange(fallbackPlan.Conflicts.Select(conflict =>
                $"Fallback visual intent {conflict} was skipped because issue direction was conflicting."));
        }

        reasons.AddRange(retryFindings.Select(finding =>
            $"Visual retry finding category {SanitizeMetadataToken(finding.Category)} considered without raw issue text."));
        return reasons
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string EnsureLoopImproverClass(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return $"<div class=\"{LoopImproverClass}\"></div>";

        if (html.Contains(LoopImproverClass, StringComparison.Ordinal))
            return html;

        const string classPattern = "(<[^>\\s]+[^>]*\\bclass\\s*=\\s*\")([^\"]*)(\")";
        if (Regex.IsMatch(html, classPattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)))
        {
            return Regex.Replace(
                html,
                classPattern,
                match => $"{match.Groups[1].Value}{match.Groups[2].Value} {LoopImproverClass}{match.Groups[3].Value}",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(100));
        }

        const string tagPattern = "(<[a-zA-Z][a-zA-Z0-9:-]*)(\\s|>)";
        return Regex.Replace(
            html,
            tagPattern,
            match => string.Equals(match.Groups[2].Value, ">", StringComparison.Ordinal)
                ? $"{match.Groups[1].Value} class=\"{LoopImproverClass}\">"
                : $"{match.Groups[1].Value} class=\"{LoopImproverClass}\"{match.Groups[2].Value}",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(100));
    }

    private static OperationResult<string> ApplyLoopImproverCss(
        string css,
        FallbackDeclarationPlan fallbackPlan)
    {
        var sourceCss = css ?? string.Empty;
        var block = BuildCanonicalLoopImproverBlock(fallbackPlan);
        var existingBlock = FindExistingLoopImproverBlock(sourceCss);
        if (!existingBlock.IsSuccess)
            return existingBlock.MapError<string>();

        if (existingBlock.Value is null)
        {
            return OperationResult<string>.Ok(
                $"{sourceCss}{Environment.NewLine}{block}{Environment.NewLine}");
        }

        var existing = existingBlock.Value;
        if (!IsKnownGeneratedLoopImproverDeclarations(existing.Declarations))
        {
            return OperationResult<string>.Fail(
                "DesignConversion runtime improvement candidate strategy found an ambiguous existing dcm-loop-improved CSS block and refused to overwrite it.",
                GeneratedBlockAmbiguousErrorCode);
        }

        var declarations = BuildCanonicalLoopImproverDeclarations(fallbackPlan);
        if (string.Equals(existing.Declarations, declarations, StringComparison.Ordinal))
        {
            return OperationResult<string>.Fail(
                "DesignConversion runtime improvement candidate strategy found equivalent generated fallback CSS and stopped before emitting a duplicate candidate.",
                CandidateUnchangedErrorCode);
        }

        return OperationResult<string>.Ok(
            string.Concat(sourceCss.AsSpan(0, existing.Start), block, sourceCss.AsSpan(existing.EndExclusive)));
    }

    private static string BuildCanonicalLoopImproverBlock(FallbackDeclarationPlan fallbackPlan)
        => $"{LoopImproverBlockPrefix}{BuildCanonicalLoopImproverDeclarations(fallbackPlan)}}}";

    private static string BuildCanonicalLoopImproverDeclarations(FallbackDeclarationPlan fallbackPlan)
        => string.Join(";", fallbackPlan.Declarations) + ";";

    private static OperationResult<LoopImproverBlock?> FindExistingLoopImproverBlock(string css)
    {
        if (css.Length > MaxFallbackCssScanLength)
        {
            return OperationResult<LoopImproverBlock?>.Fail(
                "DesignConversion runtime improvement candidate strategy refused to scan an oversized CSS fallback payload.",
                GeneratedBlockAmbiguousErrorCode);
        }

        var selectorMarker = $".{LoopImproverClass}";
        var selectorIndex = css.IndexOf(selectorMarker, StringComparison.Ordinal);
        if (selectorIndex < 0)
            return OperationResult<LoopImproverBlock?>.Ok(null);

        var blockStart = css.IndexOf(LoopImproverBlockPrefix, StringComparison.Ordinal);
        if (blockStart < 0)
        {
            return OperationResult<LoopImproverBlock?>.Fail(
                "DesignConversion runtime improvement candidate strategy found a non-canonical dcm-loop-improved selector and refused to append over it.",
                GeneratedBlockAmbiguousErrorCode);
        }

        if (selectorIndex != blockStart)
        {
            return OperationResult<LoopImproverBlock?>.Fail(
                "DesignConversion runtime improvement candidate strategy found an ambiguous dcm-loop-improved selector before the generated block.",
                GeneratedBlockAmbiguousErrorCode);
        }

        var declarationsStart = blockStart + LoopImproverBlockPrefix.Length;
        var blockEnd = css.IndexOf('}', declarationsStart);
        if (blockEnd <= declarationsStart)
        {
            return OperationResult<LoopImproverBlock?>.Fail(
                "DesignConversion runtime improvement candidate strategy found a malformed generated fallback CSS block.",
                GeneratedBlockAmbiguousErrorCode);
        }

        var nextSelectorIndex = css.IndexOf(selectorMarker, blockEnd + 1, StringComparison.Ordinal);
        if (nextSelectorIndex >= 0)
        {
            return OperationResult<LoopImproverBlock?>.Fail(
                "DesignConversion runtime improvement candidate strategy found multiple dcm-loop-improved CSS blocks and refused to choose one.",
                GeneratedBlockAmbiguousErrorCode);
        }

        return OperationResult<LoopImproverBlock?>.Ok(new LoopImproverBlock(
            Start: blockStart,
            EndExclusive: blockEnd + 1,
            Declarations: css[declarationsStart..blockEnd]));
    }

    private static bool IsKnownGeneratedLoopImproverDeclarations(string declarations)
    {
        if (string.IsNullOrWhiteSpace(declarations) ||
            !declarations.EndsWith(';') ||
            declarations.Contains('{', StringComparison.Ordinal) ||
            declarations.Contains('}', StringComparison.Ordinal) ||
            declarations.Contains('\n', StringComparison.Ordinal) ||
            declarations.Contains('\r', StringComparison.Ordinal))
        {
            return false;
        }

        var parsed = declarations
            .Split(';', StringSplitOptions.None)
            .ToArray();
        if (parsed.Length < 2 || !string.IsNullOrEmpty(parsed[^1]))
            return false;

        var items = parsed[..^1];
        if (items.Any(string.IsNullOrWhiteSpace) ||
            items.Any(item => item.Any(char.IsWhiteSpace)) ||
            items.Any(item => !KnownGeneratedFallbackDeclarations.Contains(item)))
        {
            return false;
        }

        var canonical = string.Join(";", items.OrderBy(item => item, StringComparer.Ordinal)) + ";";
        return string.Equals(declarations, canonical, StringComparison.Ordinal);
    }

    private static FallbackDeclarationPlan BuildFallbackDeclarationPlan(
        IReadOnlyList<DesignConversionVisualCorrectionFinding> retryFindings)
    {
        var directionsByProperty = new Dictionary<string, HashSet<FallbackDirection>>(StringComparer.Ordinal);
        var declarations = new SortedSet<string>(StringComparer.Ordinal);
        var conflicts = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var finding in retryFindings)
        {
            foreach (var intent in ClassifyFallbackIntents(finding))
            {
                if (!directionsByProperty.TryGetValue(intent.Property, out var directions))
                {
                    directions = [];
                    directionsByProperty[intent.Property] = directions;
                }

                directions.Add(intent.Direction);
            }
        }

        foreach (var pair in directionsByProperty.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (pair.Value.Count != 1)
            {
                conflicts.Add(pair.Key);
                continue;
            }

            AddDeclarations(declarations, pair.Key, pair.Value.Single());
        }

        AddNonDirectionalDeclarations(declarations, retryFindings);

        var intents = directionsByProperty
            .Where(pair => !conflicts.Contains(pair.Key))
            .Select(pair => new FallbackIntent(pair.Key, pair.Value.Single()))
            .OrderBy(intent => intent.Key, StringComparer.Ordinal)
            .ToArray();

        return new FallbackDeclarationPlan(
            Declarations: declarations.ToArray(),
            Intents: intents,
            Conflicts: conflicts.ToArray());
    }

    private static IReadOnlyList<FallbackIntent> ClassifyFallbackIntents(DesignConversionVisualCorrectionFinding finding)
    {
        var intents = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var clause in SplitSignalClauses(finding))
        {
            var directions = DetectDirections(clause);
            if (directions.Count == 0)
                continue;

            foreach (var property in DetectProperties(clause))
            {
                foreach (var direction in directions)
                    intents.Add($"{property}:{direction}");
            }
        }

        return intents
            .Select(value =>
            {
                var parts = value.Split(':', 2);
                return new FallbackIntent(parts[0], Enum.Parse<FallbackDirection>(parts[1]));
            })
            .ToArray();
    }

    private static IReadOnlyList<string> SplitSignalClauses(DesignConversionVisualCorrectionFinding finding)
    {
        var signal = string.Join(
            " ",
            new[]
            {
                finding.Category,
                finding.Severity,
                finding.Description,
                finding.SuggestedAction
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return Regex.Split(signal, @"[.;:\n\r]+|\bbut\b|\bwhile\b|\bthen\b", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100))
            .Select(NormalizeSignal)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

    private static IReadOnlyList<FallbackDirection> DetectDirections(string clause)
    {
        var directions = new List<FallbackDirection>();
        if (ContainsAny(
            clause,
            "reduce",
            "decrease",
            "less",
            "reduce spacing",
            "too much",
            "excessive",
            "overly",
            "too large",
            "too big",
            "too wide",
            "too tall",
            "smaller",
            "shrink",
            "tighten",
            "compress"))
        {
            directions.Add(FallbackDirection.Decrease);
        }

        if (ContainsAny(
            clause,
            "increase",
            "add",
            "not enough",
            "too little",
            "too close",
            "needs more",
            "need more",
            "more space",
            "more spacing",
            "larger",
            "expand",
            "raise",
            "crowded"))
        {
            directions.Add(FallbackDirection.Increase);
        }

        return directions
            .Distinct()
            .ToArray();
    }

    private static IReadOnlyList<string> DetectProperties(string clause)
    {
        var properties = new SortedSet<string>(StringComparer.Ordinal);
        var hasSpecificSpacingProperty = ContainsAny(
            clause,
            "padding",
            "padded",
            "inner spacing",
            "gap",
            "space between",
            "margin",
            "outer spacing");

        if (ContainsAny(clause, "padding", "padded", "inner spacing") ||
            (!hasSpecificSpacingProperty && ContainsAny(clause, "spacing", "too close", "crowded")))
            properties.Add("padding");
        if (ContainsAny(clause, "gap", "space between") ||
            (!hasSpecificSpacingProperty && ContainsAny(clause, "spacing", "too close", "crowded")))
            properties.Add("gap");
        if (ContainsAny(clause, "margin", "outer spacing"))
            properties.Add("margin");
        if (ContainsAny(clause, "width", "wide", "narrow"))
            properties.Add("width");
        if (ContainsAny(clause, "height", "tall", "short"))
            properties.Add("height");
        if (ContainsAny(clause, "font size", "font-size", "font", "text"))
            properties.Add("font-size");

        return properties.ToArray();
    }

    private static void AddDeclarations(
        ISet<string> declarations,
        string property,
        FallbackDirection direction)
    {
        switch (property, direction)
        {
            case ("padding", FallbackDirection.Decrease):
                declarations.Add("padding:0");
                break;
            case ("padding", FallbackDirection.Increase):
                declarations.Add("padding:16px");
                break;
            case ("gap", FallbackDirection.Decrease):
                declarations.Add("gap:0");
                break;
            case ("gap", FallbackDirection.Increase):
                declarations.Add("gap:12px");
                break;
            case ("margin", FallbackDirection.Decrease):
                declarations.Add("margin:0");
                break;
            case ("margin", FallbackDirection.Increase):
                declarations.Add("margin:8px");
                break;
            case ("width", FallbackDirection.Decrease):
                declarations.Add("box-sizing:border-box");
                declarations.Add("max-width:100%");
                declarations.Add("width:auto");
                break;
            case ("width", FallbackDirection.Increase):
                declarations.Add("max-width:100%");
                declarations.Add("width:100%");
                break;
            case ("height", FallbackDirection.Decrease):
                declarations.Add("height:auto");
                declarations.Add("max-height:100%");
                break;
            case ("height", FallbackDirection.Increase):
                declarations.Add("min-height:44px");
                break;
            case ("font-size", FallbackDirection.Decrease):
                declarations.Add("font-size:14px");
                break;
            case ("font-size", FallbackDirection.Increase):
                declarations.Add("font-size:18px");
                declarations.Add("line-height:1.5");
                break;
        }
    }

    private static void AddNonDirectionalDeclarations(
        ISet<string> declarations,
        IReadOnlyList<DesignConversionVisualCorrectionFinding> retryFindings)
    {
        var categorySignal = string.Join(
            " ",
            retryFindings.Select(finding => $"{finding.Category} {finding.Severity}"));
        var fullSignal = string.Join(
            " ",
            retryFindings.Select(finding => $"{finding.Category} {finding.Severity} {finding.Description} {finding.SuggestedAction}"));

        if (ContainsAny(categorySignal, "contrast", "color", "foreground", "background") ||
            ContainsAny(fullSignal, "contrast", "foreground contrast", "background contrast"))
        {
            declarations.Add("background-color:#ffffff");
            declarations.Add("color:#111827");
        }

        if (ContainsAny(fullSignal, "overflow", "clip", "clipping", "truncate", "truncated", "truncation", "wrap"))
        {
            declarations.Add("box-sizing:border-box");
            declarations.Add("max-width:100%");
            declarations.Add("overflow-wrap:anywhere");
        }

        if (ContainsAny(fullSignal, "typography", "readability"))
            declarations.Add("line-height:1.5");
    }

    private static string NormalizeSignal(string value)
    {
        return Regex.Replace(
            value ?? string.Empty,
            @"\s+",
            " ",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(100)).Trim().ToLowerInvariant();
    }

    private static bool ContainsAny(string signal, params string[] phrases)
    {
        if (string.IsNullOrWhiteSpace(signal))
            return false;

        var signalWords = TokenizeWords(signal);
        return phrases
            .Select(TokenizeWords)
            .Any(phraseWords => ContainsPhrase(signalWords, phraseWords));
    }

    private static IReadOnlyList<string> TokenizeWords(string value)
    {
        var words = new List<string>();
        var start = -1;
        for (var index = 0; index < value.Length; index++)
        {
            if (char.IsLetterOrDigit(value[index]))
            {
                if (start < 0)
                    start = index;
                continue;
            }

            AddToken(value, words, start, index);
            start = -1;
        }

        AddToken(value, words, start, value.Length);
        return words;
    }

    private static void AddToken(string value, ICollection<string> words, int start, int end)
    {
        if (start < 0 || end <= start)
            return;

        words.Add(value[start..end].ToLowerInvariant());
    }

    private static bool ContainsPhrase(IReadOnlyList<string> signalWords, IReadOnlyList<string> phraseWords)
    {
        if (phraseWords.Count == 0 || phraseWords.Count > signalWords.Count)
            return false;

        for (var signalIndex = 0; signalIndex <= signalWords.Count - phraseWords.Count; signalIndex++)
        {
            var matched = true;
            for (var phraseIndex = 0; phraseIndex < phraseWords.Count; phraseIndex++)
            {
                if (!string.Equals(signalWords[signalIndex + phraseIndex], phraseWords[phraseIndex], StringComparison.Ordinal))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
                return true;
        }

        return false;
    }

    private static string SanitizeMetadataToken(string value)
    {
        var sanitized = Regex.Replace(
            value ?? string.Empty,
            @"[^a-zA-Z0-9_.:-]+",
            "_",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(100)).Trim('_');
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
