// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel;

using AgentInfrastructure;

// Launches a sub-agent for one episode via AgentAlgorithms.AgentOrchestrator.
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class AgentLauncher : BaseService, IAgentLauncher
{
    // AgentOrchestrator from 22.AgentAlgorithms — injected (Fabric First, Rule 2).
    private readonly AgentAlgorithms.AgentOrchestrator _orchestrator;

    public AgentLauncher(ISessionStorage storage,
                         AgentAlgorithms.AgentOrchestrator orchestrator)
        : base(storage)
        => _orchestrator = orchestrator;

    // Launch the sub-task, optionally enriched with context from earlier levels.
    public async Task<OperationResult<string>> LaunchAsync(
        AgentAlgorithms.SubTask task, string context)
    {
        if (string.IsNullOrWhiteSpace(task.Description))
            return OperationResult<string>.Fail(
                "Sub-task description cannot be empty.", "EMPTY_TASK");

        // Append context from earlier pipeline levels so later agents benefit from prior work.
        string enriched = string.IsNullOrWhiteSpace(context)
            ? task.Description
            : $"{task.Description}\n\nContext from earlier steps:\n{context}";

        // AgentOrchestrator returns AgentAlgorithms.OperationResult<string> — convert to
        // AgentInfrastructure.OperationResult<string> so the caller sees a uniform envelope.
        var r = await _orchestrator.RunAsync(enriched);
        return r.IsSuccess
            ? OperationResult<string>.Ok(r.Value!)
            : OperationResult<string>.Fail(r.ErrorMessage, r.ErrorCode);
    }
}
