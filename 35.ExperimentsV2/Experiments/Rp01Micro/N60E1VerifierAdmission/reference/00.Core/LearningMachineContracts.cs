// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

// This file defines the top-level abstraction for any trainable, exportable,
// orchestrator-backed domain machine. It adds no implementations — only contracts.
// Governed by: TECHNICAL-DEBT-CLOSURE-PHASE-0.md (TD3) and
//              ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION-v1.md.
// Design rule A-LM-01: ILearningMachine sits above ILogicalOrchestrator.
// Design rule A-LM-02: Train() is ONLY on ILearningMachine — never on ILogicalUnit or ILogicalFlow.

using AgentInfrastructure; // OperationResult<T>

// ---------------------------------------------------------------------------
// Training batch types
// ---------------------------------------------------------------------------

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

// ---------------------------------------------------------------------------
// Evaluation set types
// ---------------------------------------------------------------------------

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

// ---------------------------------------------------------------------------
// Export / import type
// ---------------------------------------------------------------------------

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

// ---------------------------------------------------------------------------
// Two-leg adaptive pair coverage types
// ---------------------------------------------------------------------------

// LearningMachineLegContract names one side of a two-leg trainable sphere.
// The common/global leg lives in llm_mvp_core; the personal/adaptive leg lives with the user.
public sealed record LearningMachineLegContract(
    // LegName is the stable human-readable leg label, such as "common/global" or "personal/adaptive".
    string LegName,
    // OwnerScope names where this leg is owned and persisted.
    string OwnerScope,
    // ModelOrStateRef names the checkpoint, manifest, local state, or future adapter reference for this leg.
    string ModelOrStateRef,
    // ImplementationStatus records whether this leg is implemented, partial, planned, or missing.
    string ImplementationStatus);

// LearningMachinePortabilityContract names the user-model portability promises for a trainable sphere.
// It is a contract surface only; runtime backup/restore probes remain separate asset work orders.
public sealed record LearningMachinePortabilityContract(
    // BackupExportPath names the backup/export package or work order that owns portable user state.
    string BackupExportPath,
    // RestoreImportPath names the restore/import path or work order that must load the portable package.
    string RestoreImportPath,
    // CrossDevicePolicy names the fresh-profile or cross-device transfer expectation.
    string CrossDevicePolicy,
    // SharePolicy names the user-approved anonymised share/copy policy.
    string SharePolicy,
    // ImplementationStatus records whether portability is implemented, partial, planned, or missing.
    string ImplementationStatus);

// LearningMachinePairCoverageRow records AD-00 coverage for one trainable sphere.
// It is intentionally descriptive so reviewers can fail missing legs without reading implementation code.
public sealed record LearningMachinePairCoverageRow(
    // SphereName is the trainable sphere, such as PromptToTask or SkillSelection.
    string SphereName,
    // CommonGlobalLeg names the project-owned common/global machine leg.
    LearningMachineLegContract CommonGlobalLeg,
    // PersonalAdaptiveLeg names the user-owned personal/adaptive machine leg.
    LearningMachineLegContract PersonalAdaptiveLeg,
    // Portability names backup/export, restore/import, cross-device, and anonymised-share policy.
    LearningMachinePortabilityContract Portability,
    // DeterministicShortcutSubstituted is false when no regex/BM25/static lookup is being accepted as an adaptive leg.
    bool DeterministicShortcutSubstituted,
    // CurrentImplementationStatus summarizes the current state for this sphere.
    string CurrentImplementationStatus,
    // Gap records the first missing implementation item, or "none" when fully covered.
    string Gap);

// LearningMachinePairCoverageMatrix is the AD-00 report DTO for all trainable spheres.
// The matrix is evidence for the contract boundary, not proof that later AD asset work is complete.
public sealed record LearningMachinePairCoverageMatrix(
    // MatrixId uniquely identifies the coverage report.
    string MatrixId,
    // Rows contains one row for each trainable sphere required by AD-00.
    IReadOnlyList<LearningMachinePairCoverageRow> Rows,
    // Passed is true when every required sphere names both legs and portability policy.
    bool Passed,
    // ClaimScope keeps this contract check from claiming adaptive training or asset portability readiness.
    string ClaimScope);

// ---------------------------------------------------------------------------
// Top-level machine interface
// ---------------------------------------------------------------------------

// ILearningMachine<TDomain> is the top-level contract for any trainable, exportable,
// orchestrator-backed domain machine. It sits above ILogicalOrchestrator in the hierarchy.
// It does not replace any existing interface — it adds a new outer ring.
//
// Hierarchy:
//   ILearningMachine<TDomain>                ← this interface (new)
//     └── GetOrchestratorAsync()
//           └── ILogicalOrchestrator         ← existing (00.Core)
//                 └── ILogicalFlow<T>        ← existing (00.Core)
//                       └── ILogicalUnit<TIn,TOut> ← existing (00.Core)
//
// Training path (separate from execution, per A-LM-02):
//   ILearningMachine.TrainBatchAsync()
//     └── internally calls ILogicalCapabilityTrainer.TrainAndRegisterAsync()
//           └── uses existing *DpoGenerator + *TrainingRunner per unit
public interface ILearningMachine<TDomain>
{
    // TrainBatchAsync performs one incremental DPO training step for the domain.
    // Returns before/after accuracy metrics so the caller can observe the learning delta.
    Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<TDomain> batch, CancellationToken ct = default);

    // EvaluateAsync measures accuracy on the held-out set without modifying any weights.
    // Returns accuracy, reward margin, and whether the gate threshold was reached.
    Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<TDomain> evalSet, CancellationToken ct = default);

    // ExportAsync serialises the current trained checkpoint into a portable transfer record.
    // The record's CheckpointRef and ManifestRef describe 25.ModelServer publication state;
    // shared remote transport contracts live under 21.AgentInfrastructure/ModelServer.
    Task<OperationResult<LearningMachineExport>> ExportAsync(
        CancellationToken ct = default);

    // ImportAsync merges an external checkpoint into this machine.
    // Returns true when the import was applied successfully and the machine is ready to use.
    Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export, CancellationToken ct = default);

    // GetOrchestratorAsync returns a runtime ILogicalOrchestrator backed by the current trained state.
    // The orchestrator is the entry point for all execution-time requests to this domain.
    Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(
        CancellationToken ct = default);
}

// ---------------------------------------------------------------------------
// Personal + global machine pair interface (affiliate use case)
// ---------------------------------------------------------------------------

// ILearningMachinePair<TPersonal, TGlobal> owns a personal machine and a global machine.
// The personal machine lives on the affiliate's device.
// The global machine lives on the server (25.ModelServer + AffiliateGlobalSyncService).
// Per A-LM-08: SyncPersonalToGlobalAsync sends only anonymised training signals —
// no raw user content, no personally identifying data leaves the personal machine.
public interface ILearningMachinePair<TPersonal, TGlobal>
{
    // Personal is the on-device machine trained on the affiliate's own data.
    ILearningMachine<TPersonal> Personal { get; }

    // Global is the shared server-side machine trained on aggregated anonymised signals.
    ILearningMachine<TGlobal> Global { get; }

    // SyncPersonalToGlobalAsync sends only anonymised training signal DTOs to the global machine.
    // No raw user content, no conversation text, and no personally identifying information
    // is included in the sync payload. The payload must be verifiable by inspection.
    Task<OperationResult<bool>> SyncPersonalToGlobalAsync(CancellationToken ct = default);

    // PullGlobalToPersonalAsync applies the latest global checkpoint to the personal machine.
    // After a successful pull the personal machine benefits from all other affiliates' signals.
    Task<OperationResult<bool>> PullGlobalToPersonalAsync(CancellationToken ct = default);
}
