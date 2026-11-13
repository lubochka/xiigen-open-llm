// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// N61 / RETENTION-M1-01: the seven records of 00.Core/LearningMachineContracts.cs that the assigned
// source projection needs, copied verbatim. The interfaces of that file are outside this projection.

// LearningBatch<TDomain> carries one incremental training batch for a specific domain.
// The TDomain type parameter is a sealed marker class that names the domain (e.g. TranslationDomain).
public sealed record LearningBatch<TDomain>(
    // BatchId uniquely identifies this batch within the domain training history.
    string BatchId,
    // DomainTag is the string name of TDomain — used for routing and reporting.
    string DomainTag,
    // TrainingItems holds the DPO (Direct Preference Optimization) preference pairs for this batch.
    IReadOnlyList<LearningTrainingItem> TrainingItems,
    // SourceIds tracks which corpus sources contributed rows to this batch.
    IReadOnlyList<string> SourceIds
);

// LearningTrainingItem is one DPO preference triple: prompt, chosen response, rejected response.
public sealed record LearningTrainingItem(
    // ItemId uniquely identifies this training row.
    string ItemId,
    // Prompt is the input text that both responses were generated for.
    string Prompt,
    // ChosenResponse is the preferred (winning) response.
    string ChosenResponse,
    // RejectedResponse is the non-preferred (losing) response.
    string RejectedResponse
);

// LearningBatchResult is the metrics record returned after one training batch completes.
// All numeric fields are required — no placeholder or UNKNOWN values in production.
public sealed record LearningBatchResult(
    // BatchId identifies which batch produced this result.
    string BatchId,
    // RowsTrained is the count of DPO pairs that were processed in this batch.
    int RowsTrained,
    // AccuracyBefore is the held-out accuracy measured before this batch was applied.
    double AccuracyBefore,
    // AccuracyAfter is the held-out accuracy measured after this batch was applied.
    double AccuracyAfter,
    // RewardMargin is the average margin between chosen and rejected log-probabilities.
    double RewardMargin,
    // CpuMs is the CPU (Central Processing Unit) time consumed by this batch in milliseconds.
    long CpuMs,
    // PeakRamMb is the peak RAM (Random Access Memory) usage during this batch in megabytes.
    long PeakRamMb,
    // ElapsedMs is the wall-clock elapsed time for this batch in milliseconds.
    long ElapsedMs,
    // PromotedUnitIds lists the unit ids whose checkpoints were promoted to 25.ModelServer after this batch.
    IReadOnlyList<string> PromotedUnitIds
);

// LearningEvaluationSet<TDomain> carries the held-out evaluation set for a domain.
public sealed record LearningEvaluationSet<TDomain>(
    // EvaluationId uniquely identifies this evaluation run.
    string EvaluationId,
    // DomainTag is the string name of TDomain — must match the domain being evaluated.
    string DomainTag,
    // EvaluationItems holds the held-out rows used to measure accuracy without training.
    IReadOnlyList<LearningEvaluationItem> EvaluationItems
);

// LearningEvaluationItem is one held-out row: a prompt paired with its expected response.
public sealed record LearningEvaluationItem(
    // ItemId uniquely identifies this evaluation row.
    string ItemId,
    // Prompt is the input to be evaluated.
    string Prompt,
    // ExpectedResponse is the reference answer the model is scored against.
    string ExpectedResponse
);

// LearningEvaluationResult is the metrics record returned after held-out evaluation completes.
// PassesGate is the trainable equivalent of the runner gate in ORCHESTRATION-TRAINING-PROTOCOL-v1.md.
public sealed record LearningEvaluationResult(
    // EvaluationId identifies which evaluation run produced this result.
    string EvaluationId,
    // RowsEvaluated is the count of held-out items that were scored.
    int RowsEvaluated,
    // Accuracy is the fraction of held-out rows where the model matched the expected response.
    double Accuracy,
    // RewardMargin is the average margin between model output and expected output log-probabilities.
    double RewardMargin,
    // PassesGate is true when Accuracy meets or exceeds the domain-specific acceptance threshold.
    bool PassesGate,
    // GateNote explains the threshold value and pass/fail reason for this evaluation.
    string GateNote
);

// LearningMachineExport is the portable transfer record for a trained machine checkpoint.
// CheckpointRef points to the model weights published by 25.ModelServer. ManifestRef points
// to the same server-owned publication record, while the shared RemoteUnitManifest transport
// type is physically stored under 21.AgentInfrastructure/ModelServer so clients can read it
// without depending on the server executable.
// AccuracyAtExport must be a real measured value — not a placeholder.
public sealed record LearningMachineExport(
    // MachineId is the stable identifier for this machine across versions.
    string MachineId,
    // DomainTag identifies the domain this checkpoint belongs to.
    string DomainTag,
    // Version is the semver (Semantic Versioning) string of this checkpoint (e.g. "1.0.0").
    string Version,
    // CheckpointRef is the storage key pointing to model weights published by 25.ModelServer.
    string CheckpointRef,
    // ManifestRef is the storage key for the shared remote-unit passport: 25 owns
    // publication/checkpoint authority, and 21 owns the transport DTO used by readers.
    string ManifestRef,
    // AccuracyAtExport is the held-out accuracy that was current when this checkpoint was exported.
    double AccuracyAtExport,
    // ExportedAtUtc is the UTC (Coordinated Universal Time) timestamp when ExportAsync was called.
    DateTimeOffset ExportedAtUtc
);
