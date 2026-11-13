// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.RegularExpressions;

public static class FlowReferenceDetector
{
    private static readonly Regex ExplicitFlowRegex = new(
        @"\bflow\s*-?\s*(\d{1,2})\b|\bflow(\d{1,2})\b|\bFLOW\s*-?\s*(\d{1,2})\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StarFlowRegex = new(
        @"\b(\d{1,2})\s*(?:\*|-\*|-[a-z][a-z0-9_-]*)(?=$|[^\p{L}\p{N}_])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DigitOrdinalFlowRegex = new(
        @"(?:\bthe\s+)?\b(\d{1,2})\s*-?\s*(?:st|nd|rd|th)\s+flow\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ForbiddenBareNumberRegex = new(
        @"\b(\d{1,2})\b\s*(?:not\s+now|not\s+needed|exclude|skip|later|no\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly IReadOnlyDictionary<string, int> Ordinals =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["first"] = 1,
            ["second"] = 2,
            ["third"] = 3,
            ["fourth"] = 4,
            ["fifth"] = 5,
            ["sixth"] = 6,
            ["seventh"] = 7,
            ["eighth"] = 8,
            ["ninth"] = 9,
            ["tenth"] = 10
        };

    public static FlowDetectionResult Detect(string prompt)
    {
        var text = prompt ?? string.Empty;
        var all = new SortedSet<string>(StringComparer.Ordinal);
        var forbidden = new SortedSet<string>(StringComparer.Ordinal);

        foreach (Match match in ExplicitFlowRegex.Matches(text))
        {
            var number = FirstGroup(match, 1, 2, 3);
            AddFlowRef(all, number);
        }

        foreach (Match match in StarFlowRegex.Matches(text))
            AddFlowRef(all, match.Groups[1].Value);

        foreach (Match match in DigitOrdinalFlowRegex.Matches(text))
            AddFlowRef(all, match.Groups[1].Value);

        foreach (var (word, ordinal) in Ordinals)
        {
            if (Regex.IsMatch(text, $@"(?:\bthe\s+)?\b{Regex.Escape(word)}\s+flow\b", RegexOptions.IgnoreCase))
                AddFlowRef(all, ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        foreach (Match match in ForbiddenBareNumberRegex.Matches(text))
        {
            if (TryFormatFlowRef(match.Groups[1].Value, out var flowRef))
                forbidden.Add(flowRef);
        }

        foreach (var flowRef in all)
        {
            if (Regex.IsMatch(text, $@"\b{Regex.Escape(flowRef.Replace("FLOW-", string.Empty, StringComparison.Ordinal))}\b\s*(?:not\s+now|exclude|skip|later|no\b)", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(text, $@"\bflow\s*-?\s*{Regex.Escape(flowRef.Replace("FLOW-", string.Empty, StringComparison.Ordinal))}\b\s*(?:not\s+now|exclude|skip|later|no\b)", RegexOptions.IgnoreCase))
            {
                forbidden.Add(flowRef);
            }
        }

        return new FlowDetectionResult(all.ToArray(), forbidden.ToArray());
    }

    public static bool TryFormatFlowRef(string raw, out string flowRef)
    {
        flowRef = string.Empty;
        if (!int.TryParse(raw, out var number) || number < 1 || number > 99)
            return false;

        flowRef = $"FLOW-{number:D2}";
        return true;
    }

    private static void AddFlowRef(ISet<string> refs, string raw)
    {
        if (TryFormatFlowRef(raw, out var flowRef))
            refs.Add(flowRef);
    }

    private static string FirstGroup(Match match, params int[] groupIndexes)
    {
        foreach (var index in groupIndexes)
        {
            if (match.Groups[index].Success)
                return match.Groups[index].Value;
        }

        return string.Empty;
    }
}
