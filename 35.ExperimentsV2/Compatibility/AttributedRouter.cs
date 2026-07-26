// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// Compatibility surface for the original E24 dataset-attribution experiment.
// Production prompt segmentation now lives in SemanticCompilerAdapter.AttributedSegmentRouter.
public sealed class AttributedRouter
{
    private readonly SemanticCompiler29.Decomposer _splitter;
    private readonly AgentAlgorithms.PredicateDatasetRouter _router;

    public AttributedRouter(
        AgentAlgorithms.PredicateDatasetRouter trainedRouter,
        double splitThreshold = 0.4,
        int windowSize = 3)
    {
        _ = splitThreshold;
        _ = windowSize;
        var storage = new InMemoryCompilerStorage();
        _splitter = new SemanticCompiler29.Decomposer(storage, new NoopCompilerEventLogger());
        _router = trainedRouter;
    }

    public OperationResult<AttributedRoutingResult> Route(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<AttributedRoutingResult>.Fail("Empty prompt");

        var splitResult = _splitter.Decompose(prompt);
        if (!splitResult.IsSuccess)
            return OperationResult<AttributedRoutingResult>.Fail("Splitter: " + splitResult.ErrorMessage);

        var attribution = new Dictionary<string, List<string>>();
        foreach (var segment in splitResult.Value!)
        {
            var preds = TextToPredicates(segment);
            var dsVector = _router.Predict(preds);

            for (int d = 0; d < AgentAlgorithms.PredicateDatasetRouter.DatasetNames.Length; d++)
            {
                if (!dsVector[d])
                    continue;

                var key = AgentAlgorithms.PredicateDatasetRouter.DatasetNames[d];
                if (!attribution.ContainsKey(key))
                    attribution[key] = [];
                attribution[key].Add(segment);
            }
        }

        if (attribution.Count == 0)
            attribution["Generic"] = [prompt];

        var result = attribution.ToDictionary(
            kv => kv.Key,
            kv => string.Join("\n", kv.Value));

        return OperationResult<AttributedRoutingResult>.Ok(new AttributedRoutingResult
        {
            DatasetToSubprompt = result.AsReadOnly()
        });
    }

    private static int[] TextToPredicates(string text)
    {
        var words = SemanticNormalizer.Normalize(text);
        var families = FamilyCollapser.Collapse(words);

        var preds = new List<int>();
        foreach (var fam in families)
        {
            int pred = fam switch
            {
                "VALIDATE_OP" => LogicExtractor.VALIDATE_OP,
                "GET_OP" => LogicExtractor.GET_CALL,
                "SET_OP" => LogicExtractor.ASSIGN,
                "CREATE_OP" => LogicExtractor.CREATE_OP,
                "DELETE_OP" => LogicExtractor.FAIL,
                "AUTH_OP" => LogicExtractor.VALIDATE_OP,
                "PROCESS_OP" => LogicExtractor.PROCESS_OP,
                "SEND_OP" => LogicExtractor.SEND_OP,
                "LOG_OP" => LogicExtractor.LOG_OP,
                _ => LogicExtractor.UNKNOWN
            };
            if (pred != LogicExtractor.UNKNOWN)
                preds.Add(pred);
        }

        return preds.Count > 0 ? preds.ToArray() : [LogicExtractor.UNKNOWN];
    }

    private sealed class InMemoryCompilerStorage : SemanticCompiler29.ICompilerStorage
    {
        private readonly Dictionary<string, SemanticCompiler29.PipelineOutput> _items = new(StringComparer.Ordinal);

        public void Save(string key, SemanticCompiler29.PipelineOutput output) => _items[key] = output;

        public SemanticCompiler29.PipelineOutput? Load(string key) => _items.TryGetValue(key, out var output) ? output : null;
    }

    private sealed class NoopCompilerEventLogger : SemanticCompiler29.ICompilerEventLogger
    {
        public void Log(SemanticCompiler29.StandardEvent evt) { }
    }
}
