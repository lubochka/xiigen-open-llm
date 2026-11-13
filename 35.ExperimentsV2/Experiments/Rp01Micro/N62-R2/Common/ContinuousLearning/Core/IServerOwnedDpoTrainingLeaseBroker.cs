// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Consumer-only access to the ModelServer-owned DPO mutation-admission
/// authority. The broker cannot create, configure, replace, or revoke the
/// authority; it can only ask that the server bind one exact bounded training
/// batch to one bounded run in a named product domain.
/// </summary>
public interface IServerOwnedDpoTrainingLeaseBroker
{
    OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireTranslationTrainingLease(
            DpoTrainingContentBatch trainingContent,
            string trainingRunId);

    /// <summary>
    /// CodeDocumentation needs a separate door because its DPO rows are not
    /// Translation rows and must not be smuggled through the Translation
    /// namespace. The caller supplies the exact bounded content and run id, but
    /// cannot mint provenance or a mutation capability; only the server-owned
    /// authority can do that.
    /// </summary>
    OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireCodeDocumentationTrainingLease(
            DpoTrainingContentBatch trainingContent,
            string trainingRunId)
        => OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
            "A server-owned CodeDocumentation DPO training lease broker is required.",
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);
}
