// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler.Prediction;

using Microsoft.CodeAnalysis.CSharp;

// Restricts N-gram next-token predictions to AST continuations observed in C# grammar contexts.
public sealed class GrammarMaskFilter
{
    private Dictionary<(int Prev2, int Prev1), HashSet<int>> _table = new();
    private NgramPredictor? _predictor;

    // Train the predictor and grammar table from token ids and the source corpus that produced them.
    public OperationResult<bool> Train(
        IReadOnlyList<int> tokens,
        IReadOnlyList<string> allSources,
        Dictionary<string, int> kindToIdx)
    {
        if (tokens.Count == 0)
            return OperationResult<bool>.Fail("Token sequence is required.", "EMPTY_TOKENS");

        _predictor = new NgramPredictor();
        var trained = _predictor.Train(tokens);
        if (!trained.IsSuccess)
            return trained;

        _table = BuildGrammarTable(allSources, kindToIdx);
        ContextCount = _table.Count;
        return OperationResult<bool>.Ok(true);
    }

    // Predict the next token, constrained by the grammar table when this context is known.
    public int PredictNext(int prev2, int prev1)
    {
        if (_predictor is null)
            return 0;

        if (!_table.TryGetValue((prev2, prev1), out var mask) || mask.Count == 0)
            return _predictor.PredictNext(prev1);

        return mask
            .OrderByDescending(token => _predictor.Probability(prev1, token))
            .ThenBy(token => token)
            .First();
    }

    // Check whether a context has a grammar mask.
    public bool HasContext(int prev2, int prev1)
    {
        return _table.ContainsKey((prev2, prev1));
    }

    // Return the observed continuation set for tests and diagnostics.
    public IReadOnlySet<int> ValidContinuations(int prev2, int prev1)
    {
        return _table.TryGetValue((prev2, prev1), out var set)
            ? set
            : new HashSet<int>();
    }

    // Convert one source into AST kind ids using an existing kind vocabulary.
    public static IReadOnlyList<int> TokenizeAstKinds(string source, IReadOnlyDictionary<string, int> kindToIdx)
    {
        if (string.IsNullOrWhiteSpace(source))
            return [];

        return CSharpSyntaxTree.ParseText(source)
            .GetRoot()
            .DescendantNodes()
            .Select(node => node.Kind().ToString())
            .Where(kindToIdx.ContainsKey)
            .Select(kind => kindToIdx[kind])
            .ToArray();
    }

    // Build a kind vocabulary and combined token stream from a source corpus.
    public static OperationResult<(List<int> Tokens, Dictionary<string, int> KindToIdx)> BuildVocabulary(IReadOnlyList<string> sources)
    {
        if (sources.Count == 0)
            return OperationResult<(List<int>, Dictionary<string, int>)>.Fail("At least one source is required.", "EMPTY_SOURCE_SET");

        var kindNames = sources
            .SelectMany(source => CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().Select(node => node.Kind().ToString()))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(kind => kind, StringComparer.Ordinal)
            .ToArray();
        if (kindNames.Length == 0)
            return OperationResult<(List<int>, Dictionary<string, int>)>.Fail("No AST kinds were found.", "NO_AST_KINDS");

        var kindToIdx = kindNames
            .Select((kind, index) => (kind, index))
            .ToDictionary(pair => pair.kind, pair => pair.index, StringComparer.Ordinal);
        var tokens = sources
            .SelectMany(source => TokenizeAstKinds(source, kindToIdx))
            .ToList();

        return OperationResult<(List<int>, Dictionary<string, int>)>.Ok((tokens, kindToIdx));
    }

    // Build (prev2, prev1) -> possible next ids from all known C# sources.
    private static Dictionary<(int Prev2, int Prev1), HashSet<int>> BuildGrammarTable(
        IReadOnlyList<string> sources,
        IReadOnlyDictionary<string, int> kindToIdx)
    {
        var table = new Dictionary<(int Prev2, int Prev1), HashSet<int>>();
        foreach (var source in sources)
        {
            var tokens = TokenizeAstKinds(source, kindToIdx);
            for (var i = 2; i < tokens.Count; i++)
            {
                var key = (tokens[i - 2], tokens[i - 1]);
                if (!table.TryGetValue(key, out var continuations))
                {
                    continuations = [];
                    table[key] = continuations;
                }

                continuations.Add(tokens[i]);
            }
        }

        return table;
    }

    // Number of observed two-token grammar contexts.
    public int ContextCount { get; private set; }
}
