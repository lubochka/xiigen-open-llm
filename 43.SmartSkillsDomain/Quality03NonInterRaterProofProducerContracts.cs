// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

public sealed record Quality03NonInterRaterProofProducerOptions
{
    public string EvidencePackDirectory { get; init; } = string.Empty;
    public string SourceProofContractPackDirectory { get; init; } = string.Empty;
    public string OutputDirectory { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public string WorkOrderId { get; init; } = "QUALITY-03-NON-INTER-RATER-PROOF-DATA-AND-RUNNERS-10A";
    public DateTimeOffset GeneratedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public bool WriteReports { get; init; } = true;
    public string ApprovedProofBundleRoot { get; init; } = string.Empty;
    public string ApprovedTrainSourceCorpusManifestRef { get; init; } = string.Empty;
    public string ApprovedHeldOutSplitManifestRef { get; init; } = string.Empty;
    public string ApprovedRawPrivateCorpusRef { get; init; } = string.Empty;
}

public sealed record Quality03NonInterRaterProofProducerCategoryCounts(
    int TotalRows,
    int PassProduced,
    int RiskOrDeferred);

public sealed record Quality03NonInterRaterProofProducerReport(
    string Schema,
    string WorkOrderId,
    string RunId,
    string GeneratedAtUtc,
    string EvidencePackDirectory,
    string SourceProofContractPackDirectory,
    string OutputDirectory,
    string ApprovedProofBundleRoot,
    bool ValidProofBundleLoaded,
    IReadOnlyList<string> ProofBundleValidationMessages,
    int TotalQualityRows,
    int TotalProofRefs,
    int PassBeforeWork,
    int NonInterRaterProofRefsTotal,
    int NonInterRaterProofRefsPassProduced,
    int NonInterRaterProofRefsRiskOrDeferred,
    int InterRaterProofRefsCarriedForward,
    int BlockingProofRefsPassAfterProducer,
    int RunnerReadyRows,
    int QualityPassRows,
    bool RunnerReadyEvalInputsCreated,
    bool ExternalAiModelCalled,
    bool TrainingDpoCheckpointModelLocatorMutation,
    IReadOnlyDictionary<string, Quality03NonInterRaterProofProducerCategoryCounts> CountsByCategory,
    IReadOnlyList<string> OutputRefs,
    string DeterministicRunnerLabel,
    string WhatThisProves,
    string WhatThisDoesNotProve,
    string NextBoundedWorkOrder);
