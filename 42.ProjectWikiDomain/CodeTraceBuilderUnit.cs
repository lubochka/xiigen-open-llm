// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.RegularExpressions;
using AgentInfrastructure;

public sealed partial class CodeTraceBuilderUnit
{
    public const string UnitId = "code-trace-builder-unit";
    public const string ScenarioId = "project-wiki-code-trace-builder";

    public OperationResult<CodeTraceBuilderAnalysis> Analyze(
        string projectId,
        string sourceId,
        string sourceText)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            return OperationResult<CodeTraceBuilderAnalysis>.Fail("Project id is required.", "PROJECT_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(sourceId))
            return OperationResult<CodeTraceBuilderAnalysis>.Fail("Source id is required.", "SOURCE_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(sourceText))
            return OperationResult<CodeTraceBuilderAnalysis>.Fail("Source text is required.", "SOURCE_TEXT_REQUIRED");

        var methods = ExtractMethods(sourceText).ToArray();
        var callEdges = BuildCallEdges(methods).Distinct(StringComparer.Ordinal).ToArray();
        var valueFlows = BuildValueFlows(methods).Distinct(StringComparer.Ordinal).ToArray();
        var warnings = methods.Length == 0 ? ["no_methods_detected"] : Array.Empty<string>();
        var confidence = methods.Length == 0 ? 0.35 : callEdges.Length == 0 ? 0.82 : 0.94;

        return OperationResult<CodeTraceBuilderAnalysis>.Ok(new CodeTraceBuilderAnalysis(
            projectId,
            sourceId,
            methods.Select(method => method.Symbol).Distinct(StringComparer.Ordinal).ToArray(),
            callEdges,
            valueFlows,
            confidence,
            warnings));
    }

    private static IEnumerable<TraceMethod> ExtractMethods(string sourceText)
    {
        foreach (Match match in MethodPattern().Matches(sourceText))
        {
            var name = match.Groups["name"].Value;
            if (IsControlKeyword(name))
                continue;

            var openBrace = sourceText.IndexOf('{', match.Index);
            if (openBrace < 0)
                continue;

            var closeBrace = FindMatchingBrace(sourceText, openBrace);
            if (closeBrace <= openBrace)
                continue;

            var className = FindNearestClassName(sourceText, match.Index);
            var body = sourceText.Substring(openBrace + 1, closeBrace - openBrace - 1);
            var symbol = string.IsNullOrWhiteSpace(className) ? name : $"{className}.{name}";
            yield return new TraceMethod(symbol, name, body);
        }
    }

    private static IEnumerable<string> BuildCallEdges(IReadOnlyList<TraceMethod> methods)
    {
        foreach (var caller in methods)
        {
            foreach (var callee in methods)
            {
                if (string.Equals(caller.Symbol, callee.Symbol, StringComparison.Ordinal))
                    continue;

                if (FindCallExpression(caller.Body, callee.Name).Length > 0)
                    yield return $"{caller.Symbol}->{callee.Symbol}";
            }
        }
    }

    private static IEnumerable<string> BuildValueFlows(IReadOnlyList<TraceMethod> methods)
    {
        foreach (var caller in methods)
        {
            foreach (var callee in methods)
            {
                if (string.Equals(caller.Symbol, callee.Symbol, StringComparison.Ordinal))
                    continue;

                var expression = FindCallExpression(caller.Body, callee.Name);
                if (expression.Length == 0)
                    continue;

                var args = ExtractArguments(expression);
                var usage = expression.Contains("return ", StringComparison.Ordinal)
                    ? "returned"
                    : expression.Contains("=", StringComparison.Ordinal) ? "assigned" : "side_effect";
                yield return $"{caller.Symbol}->{callee.Symbol}({string.Join("|", args)})->{usage}";
            }
        }
    }

    private static string FindNearestClassName(string text, int methodIndex)
    {
        var prefix = text[..methodIndex];
        var matches = ClassPattern().Matches(prefix);
        return matches.Count == 0 ? "" : matches[^1].Groups["name"].Value;
    }

    private static string FindCallExpression(string body, string methodName)
    {
        var match = Regex.Match(body, $@"(?<expr>(?:return\s+)?(?:[A-Za-z_][A-Za-z0-9_]*\s*=\s*)?(?:[A-Za-z_][A-Za-z0-9_]*\.)?{Regex.Escape(methodName)}\s*\((?<args>[^;]*)\))");
        return match.Success ? match.Groups["expr"].Value.Trim() : "";
    }

    private static string[] ExtractArguments(string callExpression)
    {
        var match = Regex.Match(callExpression, @"\((?<args>.*)\)");
        return match.Success
            ? match.Groups["args"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];
    }

    private static int FindMatchingBrace(string text, int openBrace)
    {
        var depth = 0;
        for (var i = openBrace; i < text.Length; i++)
        {
            if (text[i] == '{')
                depth++;
            else if (text[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    private static bool IsControlKeyword(string name)
        => name is "if" or "for" or "foreach" or "while" or "switch" or "catch" or "using" or "lock";

    [GeneratedRegex(@"(?<return>[A-Za-z_][A-Za-z0-9_<>\[\],.?]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\((?<parameters>[^)]*)\)\s*(?:where\s+[^{]+)?\{", RegexOptions.Multiline)]
    private static partial Regex MethodPattern();

    [GeneratedRegex(@"\b(class|struct|record)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)")]
    private static partial Regex ClassPattern();

    private sealed record TraceMethod(
        string Symbol,
        string Name,
        string Body);
}
