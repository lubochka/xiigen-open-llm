// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AgentAlgorithms;

// ─── Symbol summary ───────────────────────────────────────────────────────────

// Compact description of one symbol for the repo map (no function bodies).
public sealed record SymbolSummary(
    string   Name,
    string   Kind,
    string   Signature,
    string   FilePath,
    double   InDegreeScore,    // normalized in-degree (callers / max_callers) — not iterative PageRank
    IReadOnlyList<string> CalledBy);

// ─── Repo map builder ─────────────────────────────────────────────────────────

// Builds a compact structural map of a C# codebase (static indexing approach, used by Aider).
// Extracts function names and signatures — but NOT function bodies — to keep token count low.
// Runs simplified PageRank so widely-called symbols appear higher in the map.
public sealed class RepoMapBuilder
{
    // Build the repo map for all .cs files under the given directory.
    public async Task<OperationResult<IReadOnlyList<SymbolSummary>>> BuildAsync(string projectDirectory)
    {
        if (!Directory.Exists(projectDirectory))
            return OperationResult<IReadOnlyList<SymbolSummary>>.Fail(
                $"Directory not found: {projectDirectory}", "DIR_NOT_FOUND");

        var files = Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);
        var allSymbols  = new List<SymbolSummary>();
        var callEdges   = new Dictionary<string, List<string>>(StringComparer.Ordinal); // callee → list of callers

        foreach (var filePath in files)
        {
            var source = await File.ReadAllTextAsync(filePath);
            var tree   = CSharpSyntaxTree.ParseText(source);
            var root   = tree.GetRoot();

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                string name = method.Identifier.Text;
                string sig  = BuildSignature(method);

                // Collect the names of methods this method calls (to build call edges).
                var callees = method.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Select(inv => inv.Expression.ToString().Split('.').Last())
                    .Distinct()
                    .ToList();

                // Record caller → callee relationships.
                foreach (var callee in callees)
                {
                    if (!callEdges.ContainsKey(callee))
                        callEdges[callee] = [];
                    callEdges[callee].Add(name);
                }

                allSymbols.Add(new SymbolSummary(name, "Method", sig, filePath, 0.0, []));
            }
        }

        // Compute normalized in-degree: callers / max_callers.
        // This is not iterative PageRank. For full PageRank use SymbolGraph.ComputePageRank()
        // from CodebaseIndexer in project 21.
        int max = callEdges.Values.Select(v => v.Count).DefaultIfEmpty(0).Max();

        var ranked = allSymbols.Select(s =>
        {
            double score = max == 0 ? 0.0
                : (callEdges.TryGetValue(s.Name, out var callers) ? (double)callers.Count / max : 0.0);
            var callerList = callEdges.TryGetValue(s.Name, out var c) ? (IReadOnlyList<string>)c : [];
            return s with { InDegreeScore = score, CalledBy = callerList };
        }).ToList();

        return OperationResult<IReadOnlyList<SymbolSummary>>.Ok(ranked);
    }

    // Build a compact signature string: "ReturnType MethodName(Params)".
    private static string BuildSignature(MethodDeclarationSyntax method)
    {
        string ret    = method.ReturnType.ToString();
        string name   = method.Identifier.Text;
        string pars   = string.Join(", ", method.ParameterList.Parameters.Select(p => p.ToString()));
        return $"{ret} {name}({pars})";
    }
}
