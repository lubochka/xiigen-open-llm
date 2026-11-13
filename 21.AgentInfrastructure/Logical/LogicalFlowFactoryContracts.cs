// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This request lets a flow factory construct a flow with the right unit variants for one run.
public sealed record LogicalFlowFactoryRequest(
    string RunId,
    LogicalExecutionFlags Flags,
    InMemoryLogicalUnitRegistry Units,
    IReadOnlyDictionary<string, object> Metadata,
    ILogicalTopologyRegistry? Topology = null)
{
    // Trusted is the product default; tests may lower this to probationary.
    public LogicalUnitTrustState MinimumTrustState { get; init; } = LogicalUnitTrustState.Trusted;
}

// This interface creates a logical flow at execution time so learned units can be selected before construction.
public interface ILogicalFlowFactory<TInput>
{
    // Create returns a flow instance that is ready for one execution.
    OperationResult<ILogicalFlow<TInput>> Create(LogicalFlowFactoryRequest request);
}
