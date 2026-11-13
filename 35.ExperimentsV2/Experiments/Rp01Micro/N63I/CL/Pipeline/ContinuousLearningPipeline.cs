// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// ContinuousLearningPipeline: orchestrates one complete learning cycle.
// Implements IContinuousLearningPipeline declared in Fabric/Interfaces.cs (Principle 3).
//
// Cycle steps:
//   1. ProjectIndexer.Index(rootPath)       → new TrainingExamples
//   2. DomainDatasetCache.Add(...)          → persist to domain buckets
//   3. FeedbackRouter.GetDpoPairs()         → accumulated corrections since last run
//   4. IncrementalTrainer.TrainAsync(...)   → one LoRA pass per domain with new data
//   5. DomainDatasetCache.FlushAsync()      → durable cache gate
//   6. DomainModelRegistry.Register/Get     → publish and verify adapter readback
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
    public async Task<OperationResult<IReadOnlyList<ModelAdapter>>> RunCycleAsync(
        string rootPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
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
        {
            cancellationToken.ThrowIfCancellationRequested();
            var addResult = _cache.Add(example);
            if (!addResult.IsSuccess)
            {
                return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                    addResult.ErrorMessage,
                    addResult.ErrorCode);
            }
            if (addResult.Value is not true)
            {
                return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                    "Cache add did not complete.",
                    "CACHE_ADD_INCOMPLETE");
            }
        }

        // Step 3: reserve one exact pending pair batch. The transaction handle
        // is minted only by the server-rooted FileDpoPairStore.
        var domains = indexResult.Value!
            .Select(e => e.Domain)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToList();
        if (domains.Count == 0)
        {
            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                "Indexing produced no trainable domains.",
                "EMPTY_EXAMPLES");
        }

        cancellationToken.ThrowIfCancellationRequested();
        FileDpoPairStore.TransactionHandle? transaction = null;
        IReadOnlyList<DpoPair> dpoPairs;
        if (_router is IDpoPairTransactionCoordinator coordinator)
        {
            var prepared = coordinator.PrepareDpoTransaction(domains);
            if (!prepared.IsSuccess)
                return prepared.MapError<IReadOnlyList<ModelAdapter>>();
            transaction = prepared.Value;
            FileDpoPairStore.TransactionSnapshot? exactSnapshot = null;
            if (transaction is not null)
            {
                var transactionSnapshot = transaction.Read();
                if (!transactionSnapshot.IsSuccess
                    || transactionSnapshot.Value is null)
                {
                    return transactionSnapshot
                        .MapError<IReadOnlyList<ModelAdapter>>();
                }
                exactSnapshot = transactionSnapshot.Value;
                if (!exactSnapshot.Receipt.OrderedDomains
                        .SequenceEqual(domains, StringComparer.Ordinal))
                {
                    if (exactSnapshot.MutationStarted)
                    {
                        return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                            "The indexed domain batch changed after durable DPO mutation began; an exact candidate is required for recovery.",
                            "DPO_TRANSACTION_RECOVERY_REQUIRED");
                    }
                    var released = transaction.ReleaseIfUnmutated();
                    if (!released.IsSuccess || released.Value is not true)
                        return released.MapError<IReadOnlyList<ModelAdapter>>();
                    prepared = coordinator.PrepareDpoTransaction(domains);
                    if (!prepared.IsSuccess)
                        return prepared.MapError<IReadOnlyList<ModelAdapter>>();
                    transaction = prepared.Value;
                    exactSnapshot = null;
                    if (transaction is not null)
                    {
                        transactionSnapshot = transaction.Read();
                        if (!transactionSnapshot.IsSuccess
                            || transactionSnapshot.Value is null
                            || !transactionSnapshot.Value.Receipt.OrderedDomains
                                .SequenceEqual(domains, StringComparer.Ordinal))
                        {
                            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                                "The durable DPO transaction could not be rebound to the exact indexed domain batch.",
                                "DPO_TRANSACTION_STORE_CORRUPT");
                        }
                        exactSnapshot = transactionSnapshot.Value;
                    }
                }
            }
            dpoPairs = exactSnapshot is null
                ? Array.Empty<DpoPair>()
                : Array.AsReadOnly(exactSnapshot.Pairs
                    .Select(static value => value.Pair)
                    .ToArray());
        }
        else
        {
            var pending = _router.GetDpoPairs();
            if (!pending.IsSuccess || pending.Value is null)
                return pending.MapError<IReadOnlyList<ModelAdapter>>();
            if (pending.Value.Count != 0)
            {
                return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                    "Durable DPO transaction storage is required before training pending feedback.",
                    "DPO_TRANSACTION_REQUIRED");
            }
            dpoPairs = Array.Empty<DpoPair>();
        }

        // Step 4: resume or train every exact per-domain candidate before
        // entering publication. Newly arriving feedback is not in this handle.
        var candidates = new List<ModelAdapter>(domains.Count);
        foreach (var domain in domains)
        {
            ObserveCancellationBeforeMutation(transaction, cancellationToken);
            var domainExamples = indexResult.Value!
                .Where(e => e.Domain == domain)
                .ToList();
            var domainDpoPairs = dpoPairs
                .Where(pair => StringComparer.Ordinal.Equals(pair.Domain, domain))
                .ToArray();

            TrainedAdapterArtifact artifact;
            if (transaction is not null)
            {
                var current = transaction.Read();
                if (!current.IsSuccess || current.Value is null)
                    return current.MapError<IReadOnlyList<ModelAdapter>>();
                var currentDomain = current.Value.Domains.Single(value =>
                    StringComparer.Ordinal.Equals(value.Domain, domain));
                if (currentDomain.Artifact is not null)
                {
                    artifact = currentDomain.Artifact;
                }
                else
                {
                    if (_trainer is not ITransactionalIncrementalTrainer
                        transactionalTrainer)
                    {
                        ReleaseTransactionIfSafe(transaction);
                        return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                            "The incremental trainer does not support durable DPO transaction recovery.",
                            "DPO_TRANSACTION_TRAINER_REQUIRED");
                    }
                    OperationResult<TrainedAdapterArtifact> trainResult;
                    try
                    {
                        trainResult = await transactionalTrainer
                            .TrainTransactionAsync(
                                domain,
                                domainExamples,
                                domainDpoPairs,
                                transaction,
                                HasTransactionMutationStarted(transaction)
                                    ? CancellationToken.None
                                    : cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        ReleaseTransactionIfSafe(transaction);
                        throw;
                    }
                    if (!trainResult.IsSuccess)
                    {
                        ReleaseTransactionIfSafe(transaction);
                        return trainResult
                            .MapError<IReadOnlyList<ModelAdapter>>();
                    }
                    if (trainResult.Value is null)
                    {
                        ReleaseTransactionIfSafe(transaction);
                        return IncompleteAdapterCheckpoint();
                    }
                    artifact = trainResult.Value;
                }
            }
            else
            {
                var trainResult = await _trainer.TrainAsync(
                    domain,
                    domainExamples,
                    domainDpoPairs,
                    cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                if (!trainResult.IsSuccess)
                    return trainResult.MapError<IReadOnlyList<ModelAdapter>>();
                if (trainResult.Value is null)
                    return IncompleteAdapterCheckpoint();
                artifact = trainResult.Value;
            }

            if (string.IsNullOrWhiteSpace(artifact.AdapterPath)
                || artifact.Provenance is null)
            {
                ReleaseTransactionIfSafe(transaction);
                return IncompleteAdapterCheckpoint();
            }
            var trainedAt = DateTimeOffset.UtcNow;
            var trainingExamples = domainExamples.Count;
            if (transaction is not null)
            {
                var completed = transaction.Read();
                if (!completed.IsSuccess || completed.Value is null)
                    return completed.MapError<IReadOnlyList<ModelAdapter>>();
                var completedDomain = completed.Value.Domains.Single(value =>
                    StringComparer.Ordinal.Equals(value.Domain, domain));
                trainedAt = completedDomain.TrainedAt;
                trainingExamples = completedDomain.TrainingExamples;
            }
            candidates.Add(new ModelAdapter(
                domain,
                artifact.AdapterPath,
                trainingExamples,
                trainedAt,
                artifact.Provenance));
        }

        // Step 5: durably flush the cache before publishing any adapter.
        var flushResult = await _cache.FlushAsync();
        if (!flushResult.IsSuccess)
        {
            ReleaseTransactionIfSafe(transaction);
            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                flushResult.ErrorMessage,
                flushResult.ErrorCode);
        }
        if (flushResult.Value is not true)
        {
            ReleaseTransactionIfSafe(transaction);
            return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                "Cache flush did not complete.",
                "CACHE_FLUSH_INCOMPLETE");
        }

        // This is the final cancellable boundary. Once the first adapter is
        // registered, registry publication and readback must complete as one
        // caller-visible operation even if cancellation is requested.
        ObserveCancellationBeforeMutation(transaction, cancellationToken);

        // Step 6: publish every trained adapter and verify its exact readback.
        if (transaction is not null)
        {
            if (_registry is not IDpoRegistryPublicationTransaction publisher)
            {
                return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                    "The registry does not support an atomic durable DPO publication transaction.",
                    "DPO_REGISTRY_TRANSACTION_REQUIRED");
            }

            // No pair-store lock is held on entry. The registry owns its
            // in-process and cross-process boundary first, then invokes this
            // callback to acquire the transaction lock (registry -> DPO).
            var publication = publisher.PublishBatchAndCommit(
                Array.AsReadOnly(candidates
                    .OrderBy(static value => value.Domain, StringComparer.Ordinal)
                    .ToArray()),
                published =>
                {
                    foreach (var adapter in published)
                    {
                        var recorded = transaction.RecordPublished(
                            adapter.Domain,
                            adapter.Provenance!);
                        if (!recorded.IsSuccess)
                            return recorded;
                    }
                    return transaction.CommitDrain();
                });
            return publication.IsSuccess && publication.Value is not null
                ? publication
                : publication.MapError<IReadOnlyList<ModelAdapter>>();
        }

        var newAdapters = new List<ModelAdapter>(candidates.Count);
        foreach (var adapter in candidates)
        {
            var domain = adapter.Domain;
            var existing = _registry.GetAdapter(domain);
            if (!existing.IsSuccess)
            {
                return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                    existing.ErrorMessage,
                    existing.ErrorCode);
            }
            if (!AdapterEquals(existing.Value, adapter))
            {
                var registration = _registry.Register(adapter);
                if (!registration.IsSuccess)
                {
                    return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                        registration.ErrorMessage,
                        registration.ErrorCode);
                }
                if (registration.Value is not true)
                {
                    return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                        "Adapter registration did not complete.",
                        "ADAPTER_REGISTRATION_INCOMPLETE");
                }
            }
            var readback = _registry.GetAdapter(domain);
            if (!readback.IsSuccess || !AdapterEquals(readback.Value, adapter))
            {
                return OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
                    "Adapter registration readback did not match the exact transaction candidate.",
                    "ADAPTER_REGISTRATION_INCOMPLETE");
            }
            newAdapters.Add(readback.Value!);
        }

        return OperationResult<IReadOnlyList<ModelAdapter>>.Ok(newAdapters);
    }

    private static OperationResult<IReadOnlyList<ModelAdapter>>
        IncompleteAdapterCheckpoint()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Fail(
            "Training did not publish a complete adapter checkpoint.",
            "ADAPTER_CHECKPOINT_INCOMPLETE");

    private static bool HasTransactionMutationStarted(
        FileDpoPairStore.TransactionHandle? transaction)
    {
        if (transaction is null)
            return false;
        var snapshot = transaction.Read();
        // An unreadable durable state is conservatively treated as mutated:
        // cancellation and cleanup must never downgrade corruption into a
        // releasable reservation.
        return !snapshot.IsSuccess
               || snapshot.Value is null
               || snapshot.Value.MutationStarted;
    }

    private static void ReleaseTransactionIfSafe(
        FileDpoPairStore.TransactionHandle? transaction)
    {
        if (transaction is null || HasTransactionMutationStarted(transaction))
            return;
        _ = transaction.ReleaseIfUnmutated();
    }

    private static void ObserveCancellationBeforeMutation(
        FileDpoPairStore.TransactionHandle? transaction,
        CancellationToken cancellationToken)
    {
        if (HasTransactionMutationStarted(transaction))
            return;
        if (!cancellationToken.IsCancellationRequested)
            return;
        ReleaseTransactionIfSafe(transaction);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static bool AdapterEquals(ModelAdapter? left, ModelAdapter right)
    {
        if (left is null)
            return false;
        try
        {
            return StringComparer.Ordinal.Equals(left.Domain, right.Domain)
                   && StringComparer.Ordinal.Equals(
                       Path.GetFullPath(left.AdapterPath),
                       Path.GetFullPath(right.AdapterPath))
                   && left.TrainingExamples == right.TrainingExamples
                   && left.TrainedAt.Equals(right.TrainedAt)
                   && ProvenanceEquals(left.Provenance, right.Provenance);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool ProvenanceEquals(
        ModelAdapterProvenance? left,
        ModelAdapterProvenance? right)
        => left is not null
           && right is not null
           && left.TrainingSources is not null
           && right.TrainingSources is not null
           && left.Model is not null
           && right.Model is not null
           && left.Lora is not null
           && right.Lora is not null
           && left.Lora.OrderedTargetModules is not null
           && right.Lora.OrderedTargetModules is not null
           && StringComparer.Ordinal.Equals(left.AdapterSha256, right.AdapterSha256)
           && StringComparer.Ordinal.Equals(left.BaseCheckpointSha256, right.BaseCheckpointSha256)
           && StringComparer.Ordinal.Equals(left.TokenizerSha256, right.TokenizerSha256)
           && left.Model == right.Model
           && StringComparer.Ordinal.Equals(
               left.TrainingSources.Sha256,
               right.TrainingSources.Sha256)
           && left.TrainingSources.SourceIds.SequenceEqual(
               right.TrainingSources.SourceIds,
               StringComparer.Ordinal)
           && left.Lora.Rank == right.Lora.Rank
           && left.Lora.Alpha.Equals(right.Lora.Alpha)
           && left.Lora.Scaling.Equals(right.Lora.Scaling)
           && left.Lora.Dropout.Equals(right.Lora.Dropout)
           && StringComparer.Ordinal.Equals(left.Lora.Sha256, right.Lora.Sha256)
           && left.Lora.OrderedTargetModules.SequenceEqual(
               right.Lora.OrderedTargetModules,
               StringComparer.Ordinal)
           && DpoPublicationEquals(
               left.DpoPublication,
               right.DpoPublication);

    private static bool DpoPublicationEquals(
        DpoAdapterPublicationReceipt? left,
        DpoAdapterPublicationReceipt? right)
        => left is null && right is null
           || left is not null
              && right is not null
              && left.OrderedPairIds is not null
              && right.OrderedPairIds is not null
              && left.OrderedDomains is not null
              && right.OrderedDomains is not null
              && StringComparer.Ordinal.Equals(
                  left.TransactionId,
                  right.TransactionId)
              && StringComparer.Ordinal.Equals(
                  left.PairBatchSha256,
                  right.PairBatchSha256)
              && left.OrderedPairIds.SequenceEqual(
                  right.OrderedPairIds,
                  StringComparer.Ordinal)
              && left.OrderedDomains.SequenceEqual(
                  right.OrderedDomains,
                  StringComparer.Ordinal);
}
