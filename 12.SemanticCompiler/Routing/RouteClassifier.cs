// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler;

// RouteClassifier classifies the domain of a request by predicate profile.
// E18: 95% domain accuracy with K-NN (K-Nearest Neighbours) on predicate sets.
// Domains follow D-04 canonical names: CODE_Q, BUG, PLAN, ARCH, REFACTOR.
public sealed class RouteClassifier
{
    private readonly SemanticCompiler29.RouteClassifier _canonical;

    public RouteClassifier(int k = 5)
    {
        _canonical = new SemanticCompiler29.RouteClassifier(
            new InMemoryCompilerStorage(),
            new NoopCompilerEventLogger(),
            k);
    }

    public void Train(IReadOnlyList<(int[] Predicates, SemanticCompiler29.Domain Domain)> examples)
    {
        _canonical.Train(examples);
    }

    // Compatibility wrapper over the canonical project 29 E18 KNN classifier.
    public OperationResult<SemanticCompiler29.Domain> Classify(int[] predicates)
    {
        var result = _canonical.Classify(predicates);
        return result.IsSuccess
            ? OperationResult<SemanticCompiler29.Domain>.Ok(result.Value!)
            : OperationResult<SemanticCompiler29.Domain>.Fail(result.ErrorMessage, result.ErrorCode);
    }

    // Baseline: keyword matching (fast, no training required)
    public static SemanticCompiler29.Domain ClassifyByKeywords(string prompt)
    {
        return SemanticCompiler29.RouteClassifier.ClassifyByKeywords(prompt);
    }

    // Auto-label a prompt for training data generation
    public static SemanticCompiler29.Domain AutoLabel(string prompt) => ClassifyByKeywords(prompt);

    private sealed class InMemoryCompilerStorage : SemanticCompiler29.ICompilerStorage
    {
        private readonly Dictionary<string, SemanticCompiler29.PipelineOutput> _items = new(StringComparer.Ordinal);

        public void Save(string key, SemanticCompiler29.PipelineOutput output) => _items[key] = output;

        public SemanticCompiler29.PipelineOutput? Load(string key)
        {
            return _items.TryGetValue(key, out var output) ? output : null;
        }
    }

    private sealed class NoopCompilerEventLogger : SemanticCompiler29.ICompilerEventLogger
    {
        public void Log(SemanticCompiler29.StandardEvent evt)
        {
        }
    }
}
