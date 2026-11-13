// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;

// IProjectIndexer — scans a codebase and produces labelled training examples.
// Implementation injects ISemanticCompiler29 from 29.SemanticCompiler — never reimplements it.
// grep -rn "ISemanticCompiler" 14.ContinuousLearning/ must return ≥ 1 line (Principle 4).
public interface IProjectIndexer
{
    // Scan all .cs / .ts / .py files under rootPath.
    // Returns one TrainingExample per file (prompt=symbol, response=body).
    OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath);
}

// IDomainDatasetCache — persists training examples by domain.
// Backed by JSON via ICodeWriter — no SQLite dependency.
public interface IDomainDatasetCache
{
    // Add one example to the in-memory store for its domain.
    OperationResult<bool> Add(TrainingExample example);
    // Retrieve all examples for one canonical domain string.
    OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain);
    // Total example count across all domains.
    OperationResult<int> TotalCount();
    // Write one JSON file per domain to the configured cache directory.
    Task<OperationResult<bool>> FlushAsync();
}

// IFeedbackRouter — converts user feedback events into training signals.
// ACCEPTED → positive example, EDITED/REJECTED → DPO pair.
public interface IFeedbackRouter
{
    // Process one event: route it to cache or create a DPO pair.
    OperationResult<bool> Route(FeedbackEvent feedback);
    // Return all durable, not-yet-drained DPO pairs across process restarts.
    OperationResult<IReadOnlyList<DpoPair>> GetDpoPairs();
}

/// <summary>
/// Friend-only server transaction seam. The concrete handle is minted only by
/// the durable pair store and is never a public training authority.
/// </summary>
internal interface IDpoPairTransactionCoordinator
{
    OperationResult<FileDpoPairStore.TransactionHandle?> PrepareDpoTransaction(
        IReadOnlyList<string> domains);

}

/// <summary>
/// Optional friend-only capability for consuming exactly one durable atomic
/// teacher group in a non-adapter transaction.
/// </summary>
internal interface IDpoPairExternalConsumptionTransactionCoordinator
{
    OperationResult<FileDpoPairStore.ExternalConsumptionTransactionHandle?>
        PrepareExternalConsumptionTransaction(
            DpoPairAtomicGroupReceipt receipt,
            string domain);
}

internal interface ITransactionalIncrementalTrainer
{
    Task<OperationResult<TrainedAdapterArtifact>> TrainTransactionAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        FileDpoPairStore.TransactionHandle transaction,
        CancellationToken cancellationToken = default);
}

// IIncrementalTrainer — runs LoRA fine-tuning on accumulated examples.
// Delegates to LoraApplicator.Apply from 10.LoRA — never reimplements gradient logic.
/// <summary>
/// Persists and reads back durable DPO preference pairs.
/// </summary>
/// <remarks>
/// Operations return failures through <see cref="OperationResult{T}"/> so callers can fail closed
/// before starting training when persistence or readback does not complete.
/// </remarks>
public interface IDpoPairStore
{
    /// <summary>Adds a preference pair to durable storage and returns its stable identity.</summary>
    OperationResult<string> Add(DpoPair pair);

    /// <summary>
    /// Adds one two-child teacher-authored group as a single durable unit.
    /// Implementations that do not support the atomic-group schema fail closed.
    /// </summary>
    OperationResult<DpoPairAtomicGroupReceipt> AddAtomicGroup(
        DpoPairAtomicGroupEnvelope group)
        => OperationResult<DpoPairAtomicGroupReceipt>.Fail(
            "Atomic DPO pair-group storage is unavailable.",
            "DPO_PAIR_ATOMIC_GROUP_UNAVAILABLE");

    /// <summary>Reads all preference pairs currently retained by the store.</summary>
    OperationResult<IReadOnlyList<DpoPair>> GetAll();
}

public sealed record DpoPairBatchReservation(
    string ReservationId,
    string BatchSha256,
    int PairCount);

/// <summary>
/// Durable, exclusive capacity reservation for an exact DPO pair batch. The
/// reservation is idempotent for the same id and exact semantic batch.
/// </summary>
public interface IDpoPairBatchReservationStore : IDpoPairStore
{
    OperationResult<DpoPairBatchReservation> ReserveBatch(
        string reservationId,
        IReadOnlyList<DpoPair> pairs);

    OperationResult<bool> ReleaseBatch(
        DpoPairBatchReservation reservation);
}

/// <summary>Closed, non-model target identity for an atomic teacher group.</summary>
public enum DpoPairAtomicGroupRole
{
    CriticUnit = 0,
    ImproverUnit = 1,
}

/// <summary>One exact child of an atomic teacher-authored preference group.</summary>
public sealed record DpoPairAtomicGroupChild(
    int Ordinal,
    DpoPairAtomicGroupRole Role,
    string PairId,
    string ContentSha256,
    DpoPair Pair);

/// <summary>
/// Versioned parent envelope. The commitment is over framed child identities
/// and digests; the child pairs remain the store's sole raw text carrier.
/// </summary>
public sealed record DpoPairAtomicGroupEnvelope(
    int SchemaVersion,
    string GroupCommitmentSha256,
    string RequestId,
    int InvocationCount,
    IReadOnlyList<DpoPairAtomicGroupChild> Children)
{
    public const int CurrentSchemaVersion = 1;

    public static OperationResult<DpoPairAtomicGroupEnvelope> Create(
        DpoPair critic,
        DpoPair improver)
        => FileDpoPairStore.CreateAtomicGroupEnvelope(critic, improver);
}

/// <summary>Durable receipt for one indivisible two-child pair group.</summary>
public sealed record DpoPairAtomicGroupReceipt(
    string GroupCommitmentSha256,
    string RequestId,
    IReadOnlyList<string> OrderedPairIds);

/// <summary>
/// Closed friend-only identity for a non-adapter consumer of one exact atomic
/// pair group.  The value is persisted; it is never supplied as training
/// authority by itself.
/// </summary>
internal enum DpoExternalConsumerKind
{
    DesignConversionDualTargetTeacherAggregate = 0,
}

/// <summary>
/// Bounded, non-model result identity supplied after an externally authorized
/// mutation.  Both values are canonical SHA-256 digests; raw checkpoints or
/// model output never enter the pair-store transaction carrier.
/// </summary>
internal sealed record DpoExternalConsumptionResult(
    DpoExternalConsumerKind ConsumerKind,
    string ResultSha256,
    string CheckpointSha256);

public interface IIncrementalTrainer
{
    // Run one LoRA training pass on examples for the given domain.
    // Returns the saved adapter plus provenance measured at the training boundary.
    Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair>         dpoPairs,
        CancellationToken cancellationToken = default);
}

public interface IIncrementalTrainingSessionFactory
{
    OperationResult<IncrementalTrainingSession> Create(string domain);

    OperationResult<IncrementalTrainingSession> Create(
        string domain,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Create(domain);
    }
}

// IDomainModelRegistry — loads and saves LoRA adapters per domain.
public interface IDomainModelRegistry
{
    // Register a newly-trained adapter.
    OperationResult<bool> Register(ModelAdapter adapter);
    // Get the adapter for a domain; Value is null when none trained yet (still IsSuccess).
    OperationResult<ModelAdapter?> GetAdapter(string domain);
    // List all registered adapters.
    OperationResult<IReadOnlyList<ModelAdapter>> ListAll();
}

// Holds the registry's in-process and cross-process publication boundary while
// the caller records the exact DPO transaction publication and drains its pair
// receipt. The required lock order is registry boundary -> DPO pair transaction.
internal interface IDpoRegistryPublicationTransaction
{
    OperationResult<IReadOnlyList<ModelAdapter>> PublishBatchAndCommit(
        IReadOnlyList<ModelAdapter> adapters,
        Func<IReadOnlyList<ModelAdapter>, OperationResult<bool>> commit);
}

// IContinuousLearningPipeline — fabric interface for 16.AutonomousLearning.
// 16.AutonomousLearning must inject this, not copy the implementation (Principle 3).
public interface IContinuousLearningPipeline
{
    // Run one full learning cycle: index → cache → train → register → flush.
    Task<OperationResult<IReadOnlyList<ModelAdapter>>> RunCycleAsync(
        string rootPath,
        CancellationToken cancellationToken = default);
}
