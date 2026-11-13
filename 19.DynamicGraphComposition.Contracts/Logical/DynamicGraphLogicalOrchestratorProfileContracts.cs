// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure.Logical;

// Binds one explicitly selected logical sub-phase to its descriptor-only graph node.
public sealed record DynamicGraphLogicalOrchestratorNodeDefinition(
    string LogicalSubPhaseId,
    DynamicGraphNodeDescriptor Descriptor);

// Carries the complete caller-owned bootstrap selection without supplying graph defaults.
public sealed record DynamicGraphLogicalOrchestratorBootstrapProfile(
    string ProfileId,
    string TenantId,
    string DomainTag,
    string ContextId,
    string SignalId,
    string SourceRef,
    string TraceRef,
    DynamicGraphLogicalOrchestratorNodeDefinition[] NodeDefinitions,
    DynamicGraphRegistryEdgeBinding[] EdgeBindings,
    TopologyBuildBudget Budget);
