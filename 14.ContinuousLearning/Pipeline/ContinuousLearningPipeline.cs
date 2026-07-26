// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;

// ContinuousLearningPipeline: orchestrates one complete learning cycle.
// Implements IContinuousLearningPipeline declared in Fabric/Interfaces.cs (Principle 3).
//
// Cycle steps:
//   1. ProjectIndexer.Index(rootPath)       → new TrainingExamples
//   2. DomainDatasetCache.Add(...)          → persist to domain buckets
//   3. FeedbackRouter.GetDpoPairs()         → accumulated corrections since last run
//   4. IncrementalTrainer.TrainAsync(...)   → one LoRA pass per domain with new data
//   5. DomainModelRegistry.Register(...)   → save new adapters
//   6. DomainDatasetCache.FlushAsync()      → write JSON to disk
//
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class ContinuousLearningPipeline : BaseService, IContinuousLearningPipeline
{
    private readonly IProjectIndexer     _indexer;
    private readonly IDomainDatasetCache _cache;
    private readonly IFeedbackRouter     _router;
    private readonly IIncrementalTrainer _trainer;
    private readonly IDomainModelRegistry _registry;

    public ContinuousLearningPipeline(
        ISessionStorage       storage,
        IProjectIndexer       indexer,
        IDomainDatasetCache   cache,
        IFeedbackRouter       router,
        IIncrementalTrainer   trainer,
        IDomainModelRegistry  registry)
        : base(storage)
    {
        _indexer  = indexer;
        _cache    = cache;
        _router   = router;
        _trainer  = trainer;
        _registry = registry;
    }

    // Run one full learning cycle: returns all new adapters produced.
    public async Task<OperationResult<IReadOnlyList<ModelAdapter>>> RunCycleAsync(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                "Root path cannot be empty.", "EMPTY_PATH");

        // Step 1: index the codebase.
        var indexResult = _indexer.Index(rootPath);
        if (!indexResult.IsSuccess)
            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                indexResult.ErrorMessage, "INDEX_FAILED");

        // Step 2: add all indexed examples to the domain cache.
        foreach (var example in indexResult.Value!)
            _cache.Add(example);

        // Step 3: retrieve DPO pairs accumulated from user feedback.
        var dpoPairsResult = _router.GetDpoPairs();
        var dpoPairs = dpoPairsResult.IsSuccess
            ? dpoPairsResult.Value!
            : (IReadOnlyList<DpoPair>)Array.Empty<DpoPair>();

        // Step 4: train one LoRA adapter per domain that has new examples.
        var domains = indexResult.Value!
            .Select(e => e.Domain)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var newAdapters = new List<ModelAdapter>();
        foreach (var domain in domains)
        {
            var domainExamples = indexResult.Value!
                .Where(e => e.Domain == domain)
                .ToList();

            var trainResult = await _trainer.TrainAsync(domain, domainExamples, dpoPairs);
            if (!trainResult.IsSuccess) continue;

            // Step 5: register the new adapter.
            var adapter = new ModelAdapter(
                domain, trainResult.Value!, domainExamples.Count, DateTimeOffset.UtcNow);
            _registry.Register(adapter);
            newAdapters.Add(adapter);
        }

        // Step 6: flush cache to disk.
        await _cache.FlushAsync();

        return OperationResult<IReadOnlyList<ModelAdapter>>.Ok(newAdapters);
    }
}
