// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// Internal server-owned entry for one exact deterministic DesignConversion
/// Machine mutation. The caller supplies typed provenance, but cannot supply a
/// content digest, lease, binding, or admission carrier.
/// </summary>
internal static class DesignConversionMachineTrainingAuthority
{
    internal static async Task<OperationResult<LearningBatchResult>>
        ExecuteExactAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority? authority,
            DesignConversionMachine? machine,
            LearningBatch<DesignConversionDomain>? batch,
            IReadOnlyList<TrainingTeacherProvenance?>? provenance,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<LearningBatchResult>.Fail(
                "DesignConversion Machine training cancelled.",
                "CANCELLED");
        }
        if (authority is null || machine is null)
        {
            return OperationResult<LearningBatchResult>.Fail(
                "Server-owned legal training authority is required for DesignConversion Machine mutation.",
                LegalTrainingTeacherAdmissionAuthority
                    .ConfigurationRequiredErrorCode);
        }
        if (batch is null || provenance is null ||
            batch.TrainingItems is null || batch.SourceIds is null ||
            provenance.Count != batch.TrainingItems.Count)
        {
            return Invalid();
        }

        for (var index = 0; index < provenance.Count; index++)
        {
            var row = provenance[index];
            if (row is null ||
                row.SourceKind !=
                    TrainingTeacherSourceKind.DeterministicNoExternalTeacher ||
                row.Family is not null || row.AccessMode is not null ||
                row.HostingProvider is not null || row.ModelLicense is not null ||
                !string.IsNullOrEmpty(row.ModelId) ||
                !string.IsNullOrEmpty(row.ModelVersion) ||
                row.ProviderTermsChecked || row.ProviderTermsAllowTraining ||
                row.ClosedProprietaryModel ||
                row.TeacherOutputUsedForTraining || row.InvocationCount != 0 ||
                string.IsNullOrWhiteSpace(row.RequestId))
            {
                return OperationResult<LearningBatchResult>.Fail(
                    "Deterministic typed provenance is required for exact DesignConversion Machine training.",
                    LegalTrainingTeacherAdmissionAuthority
                        .ProvenanceInvalidErrorCode);
            }
        }

        var finalOptimizer = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                batch.TrainingItems,
                provenance,
                static item => item.Prompt,
                static item => item.ChosenResponse,
                static item => item.RejectedResponse,
                static _ => null,
                static _ => null,
                static item => string.Concat(
                    "design-conversion-machine-exact|item=",
                    item.ItemId));
        if (!finalOptimizer.IsSuccess || finalOptimizer.Value is null)
            return finalOptimizer.MapError<LearningBatchResult>();

        var leaseResult = authority.AcquireTrainingLease(
            finalOptimizer.Value.Content,
            finalOptimizer.Value.Provenance);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<LearningBatchResult>();

        using var lease = leaseResult.Value;
        var admission = DesignConversionMachineTrainingAdmission.Mint(
            finalOptimizer.Value,
            batch,
            lease);
        if (!admission.IsSuccess || admission.Value is null)
            return admission.MapError<LearningBatchResult>();

        return await machine.TrainAdmittedBatchAsync(admission.Value, ct);
    }

    private static OperationResult<LearningBatchResult> Invalid()
        => OperationResult<LearningBatchResult>.Fail(
            "Exact DesignConversion Machine training batch and provenance are required.",
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);
}
