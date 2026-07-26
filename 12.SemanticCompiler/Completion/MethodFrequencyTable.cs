// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler.Completion;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Ranks method candidates by observed call frequency per receiver.
public sealed class MethodFrequencyTable
{
    private readonly Dictionary<string, Dictionary<string, int>> _frequency = new(StringComparer.OrdinalIgnoreCase);

    // Train the frequency table from one or more C# sources.
    public OperationResult<bool> Train(IReadOnlyList<string> csharpSources)
    {
        if (csharpSources.Count == 0)
            return OperationResult<bool>.Fail("At least one source is required.", "EMPTY_SOURCE_SET");

        _frequency.Clear();
        foreach (var source in csharpSources)
        {
            var extracted = ExtractMethodCalls(source);
            if (!extracted.IsSuccess)
                continue;

            foreach (var (receiver, method) in extracted.Value!)
            {
                if (!_frequency.TryGetValue(receiver, out var methods))
                {
                    methods = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    _frequency[receiver] = methods;
                }

                methods[method] = methods.GetValueOrDefault(method, 0) + 1;
            }
        }

        TotalPairs = _frequency.Values.Sum(methods => methods.Count);
        return OperationResult<bool>.Ok(true);
    }

    // Return candidates sorted by descending observed frequency for this receiver.
    public IReadOnlyList<string> RankMethods(string receiver, IReadOnlyList<string> candidates)
    {
        if (string.IsNullOrWhiteSpace(receiver) || candidates.Count == 0)
            return candidates.ToArray();
        if (!_frequency.TryGetValue(receiver.Trim(), out var methods))
            return candidates.ToArray();

        return candidates
            .Select((method, index) => (method, index))
            .OrderByDescending(pair => methods.GetValueOrDefault(pair.method, 0))
            .ThenBy(pair => pair.index)
            .Select(pair => pair.method)
            .ToArray();
    }

    // Parse C# invocations into (receiver, method) observations.
    public static OperationResult<List<(string Receiver, string Method)>> ExtractMethodCalls(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return OperationResult<List<(string, string)>>.Fail("Empty source.", "EMPTY_SOURCE");

        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var results = new List<(string Receiver, string Method)>();
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                continue;

            var method = memberAccess.Name.Identifier.Text;
            var receiver = ExtractLeafIdentifier(memberAccess.Expression);
            if (!string.IsNullOrWhiteSpace(receiver) && !string.IsNullOrWhiteSpace(method))
                results.Add((receiver, method));
        }

        return OperationResult<List<(string, string)>>.Ok(results);
    }

    // Return the left-most receiver identifier for member-access expressions.
    private static string ExtractLeafIdentifier(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            MemberAccessExpressionSyntax memberAccess => ExtractLeafIdentifier(memberAccess.Expression),
            ThisExpressionSyntax => "this",
            _ => string.Empty
        };
    }

    // Count distinct receiver/method pairs seen in training.
    public int TotalPairs { get; private set; }
}
