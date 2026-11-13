// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AutonomousLearning;

using AgentInfrastructure;
using ContinuousLearning;

public sealed record AutonomousTrainingBatch(
    string Domain,
    IReadOnlyList<TrainingExample> NewExamples,
    IReadOnlyList<TrainingExample> MixedExamples,
    IReadOnlyList<DpoPair> DpoPairs,
    TrainingSourceManifest TrainingSources);

// AutonomousTrainer — wraps IncrementalTrainer with E43 replay-buffer mixing.
// Implements the core training step of the 8-step autonomous learning cycle:
//   Step 7: IncrementalTrainer → LoRA rank=8, alpha=16 (from 14.ContinuousLearning).
// IncrementalTrainer is injected — never reimplemented here (Principle 4).
public sealed class AutonomousTrainer : BaseService
{
    // MinExamples — minimum number of new examples required to start a training cycle.
    // Fewer than this means the domain dataset is not yet statistically meaningful.
    public const int MinExamples = 5;

    // _trainer — injected IIncrementalTrainer from 14.ContinuousLearning.
    // Performs the actual LoRA gradient steps — not reimplemented here.
    private readonly IIncrementalTrainer _trainer;

    // _buffer — E43 ReplayBuffer that maintains the 70/30 new/old mix.
    // Shared across training cycles to accumulate domain history.
    private readonly ReplayBuffer _buffer;

    // Constructor: fabric-first injection of trainer and replay buffer.
    public AutonomousTrainer(
        ISessionStorage    storage,
        IIncrementalTrainer trainer,
        ReplayBuffer        buffer)
        : base(storage)
    {
        // Store trainer reference — called during TrainAsync after replay mixing.
        _trainer = trainer;
        // Store buffer reference — sampled before training and updated after.
        _buffer  = buffer;
    }

    // TrainAsync — run one autonomous training cycle for a single domain.
    // Pipeline:
    //   1. Guard: minimum example count (MinExamples) — fail fast if insufficient data.
    //   2. ReplayBuffer.Sample — mix 70% new + 30% old (E43 algorithm).
    //   3. IIncrementalTrainer.TrainAsync — run LoRA fine-tuning on the mixed batch.
    //   4. ReplayBuffer.Add — store new examples for future replay.
    // Returns the adapter artifact and measured provenance on success.
    public async Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string                         domain,
        IReadOnlyList<TrainingExample>  newExamples,
        IReadOnlyList<DpoPair>          dpoPairs,
        CancellationToken               cancellationToken = default)
    {
        var batchResult = PrepareBatch(domain, newExamples, dpoPairs, cancellationToken);
        if (!batchResult.IsSuccess || batchResult.Value is null)
            return batchResult.MapError<TrainedAdapterArtifact>();
        return await TrainPreparedAsync(batchResult.Value, cancellationToken);
    }

    public OperationResult<AutonomousTrainingBatch> PrepareBatch(
        string domain,
        IReadOnlyList<TrainingExample> newExamples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(newExamples);
        ArgumentNullException.ThrowIfNull(dpoPairs);
        if (newExamples.Count < MinExamples)
            return OperationResult<AutonomousTrainingBatch>.Fail(
                $"At least {MinExamples} new examples required. Got {newExamples.Count}.",
                "INSUFFICIENT_EXAMPLES");

        if (string.IsNullOrWhiteSpace(domain)
            || !StringComparer.Ordinal.Equals(domain, domain.Trim())
            || newExamples.Any(example =>
                example is null
                || !StringComparer.Ordinal.Equals(example.Domain, domain)
                || string.IsNullOrWhiteSpace(example.Prompt)
                || string.IsNullOrWhiteSpace(example.Response)
                || !double.IsFinite(example.Weight)
                || example.Weight <= 0d
                || example.Weight > float.MaxValue))
        {
            return OperationResult<AutonomousTrainingBatch>.Fail(
                "The autonomous training batch is invalid for the requested domain.",
                "INVALID_TRAINING_EXAMPLE");
        }

        var mixedBatch = _buffer.Sample(newExamples).ToArray();
        var manifestResult = TrainingSourceProvenance.BuildManifest(
            mixedBatch.Select(static example => example.SourceId));
        if (!manifestResult.IsSuccess || manifestResult.Value is null)
            return manifestResult.MapError<AutonomousTrainingBatch>();

        return OperationResult<AutonomousTrainingBatch>.Ok(
            new AutonomousTrainingBatch(
                domain,
                Array.AsReadOnly(newExamples.ToArray()),
                Array.AsReadOnly(mixedBatch),
                Array.AsReadOnly(dpoPairs.ToArray()),
                manifestResult.Value));
    }

    public async Task<OperationResult<TrainedAdapterArtifact>> TrainPreparedAsync(
        AutonomousTrainingBatch batch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(batch.Domain)
            || !StringComparer.Ordinal.Equals(batch.Domain, batch.Domain.Trim())
            || batch.NewExamples is null
            || batch.MixedExamples is null
            || batch.DpoPairs is null
            || batch.TrainingSources?.SourceIds is null
            || batch.NewExamples.Count < MinExamples
            || batch.NewExamples.Any(example => !IsValidExample(example, batch.Domain))
            || batch.MixedExamples.Any(example => !IsValidExample(example, batch.Domain)))
        {
            return OperationResult<TrainedAdapterArtifact>.Fail(
                "The frozen autonomous training batch is invalid.",
                "INVALID_TRAINING_EXAMPLE");
        }

        var actualBatchManifest = TrainingSourceProvenance.BuildManifest(
            batch.MixedExamples.Select(static example => example.SourceId));
        if (!actualBatchManifest.IsSuccess
            || actualBatchManifest.Value is null
            || !StringComparer.Ordinal.Equals(
                actualBatchManifest.Value.Sha256,
                batch.TrainingSources.Sha256)
            || !actualBatchManifest.Value.SourceIds.SequenceEqual(
                batch.TrainingSources.SourceIds,
                StringComparer.Ordinal)
            || batch.NewExamples.Any(example =>
                !actualBatchManifest.Value.SourceIds.Contains(
                    example.SourceId,
                    StringComparer.Ordinal)))
        {
            return OperationResult<TrainedAdapterArtifact>.Fail(
                "The declared training manifest does not identify the frozen batch.",
                "CANDIDATE_TRAINING_MANIFEST_MISMATCH");
        }

        // Step 3: delegate training to IncrementalTrainer from 14.ContinuousLearning.
        // IncrementalTrainer runs LoRA with rank=8, alpha=16 — no gradient logic here.
        var trainResult = await _trainer.TrainAsync(
            batch.Domain,
            batch.MixedExamples,
            batch.DpoPairs,
            cancellationToken);

        // If training failed, return the error immediately — do not update replay buffer.
        if (!trainResult.IsSuccess)
            return trainResult;

        if (trainResult.Value?.Provenance?.TrainingSources is not { } trainedSources
            || string.IsNullOrWhiteSpace(trainResult.Value.AdapterPath))
        {
            return OperationResult<TrainedAdapterArtifact>.Fail(
                "Training did not publish a complete adapter artifact.",
                "ADAPTER_CHECKPOINT_INCOMPLETE");
        }

        var rebuiltSources = TrainingSourceProvenance.BuildManifest(
            trainedSources.SourceIds);
        if (!rebuiltSources.IsSuccess
            || rebuiltSources.Value is null
            || !StringComparer.Ordinal.Equals(
                rebuiltSources.Value.Sha256,
                trainedSources.Sha256)
            || batch.TrainingSources.SourceIds.Any(source =>
                !trainedSources.SourceIds.Contains(source, StringComparer.Ordinal)))
        {
            return OperationResult<TrainedAdapterArtifact>.Fail(
                "Trainer provenance does not cover the frozen training batch.",
                "CANDIDATE_TRAINING_MANIFEST_MISMATCH");
        }

        // Step 4: add new examples to the replay buffer ONLY on successful training.
        // This prevents polluting the buffer with examples that produced a failed adapter.
        foreach (var ex in batch.NewExamples)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _buffer.Add(ex);
        }

        // Return the adapter path reported by IncrementalTrainer.
        return trainResult;
    }

    private static bool IsValidExample(TrainingExample? example, string domain)
        => example is not null
           && StringComparer.Ordinal.Equals(example.Domain, domain)
           && !string.IsNullOrWhiteSpace(example.Prompt)
           && !string.IsNullOrWhiteSpace(example.Response)
           && double.IsFinite(example.Weight)
           && example.Weight > 0d
           && example.Weight <= float.MaxValue;
}
