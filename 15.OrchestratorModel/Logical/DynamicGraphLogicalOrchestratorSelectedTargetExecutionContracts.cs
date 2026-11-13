// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Carries one shallow, caller-owned request for direct execution of an already-selected target.
/// Validation and ownership transfer remain the responsibility of the retained runtime boundary.
/// </summary>
public sealed record DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest(
    string RunId,
    string ScenarioId,
    string UserRequest,
    DynamicGraphMeasurementTarget? Target,
    EdgeAuthorityExecutionResult? AuthorityResult,
    DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? NoveltyRequest);

/// <summary>
/// Carries trusted in-process association evidence that has already passed admission. This
/// carrier does not prove provenance, re-measure evidence, or expose an HTTP or client boundary,
/// and it makes no aggregate-quality, DPO, training, promotion, or lifecycle claim.
/// </summary>
public sealed record DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
    string RunId,
    string ScenarioId,
    string UserRequest,
    DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? Association);

/// <summary>
/// Exposes one selected-target execution operation without publishing runtime, registry, metadata,
/// execution-flag, trust-policy, training, or mutation authority.
/// </summary>
public interface IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession
{
    Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>> RunAsync(
        DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Exposes one trusted in-process already-admitted execution operation. It does not establish
/// provenance, re-measure evidence, or publish an HTTP, client, aggregate-quality, DPO, training,
/// promotion, or lifecycle boundary.
/// </summary>
public interface IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession
{
    Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
        RunAdmittedAsync(
            DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest request,
            CancellationToken cancellationToken = default);
}
