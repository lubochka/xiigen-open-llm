// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

// ─── Tool handler delegate ────────────────────────────────────────────────────

public delegate Task<OperationResult<string>> ToolHandler(Dictionary<string, object> arguments);

// ─── Stage 05 — Tool registry ─────────────────────────────────────────────────

// Stores all available tools and dispatches calls to the correct handler.
// Tools are registered by name; the registry checks permissions before executing.
public sealed class ToolRegistry
{
    private readonly Dictionary<string, (ToolDefinition Definition, ToolHandler Handler)> _tools
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly PermissionGate _gate;

    public ToolRegistry(PermissionGate gate)
        => _gate = gate;

    // Register a tool with its definition (name, description, schema) and its handler.
    public void Register(ToolDefinition definition, ToolHandler handler)
        => _tools[definition.Name] = (definition, handler);

    // Return all registered tool definitions (sent to the language model as the tool list).
    public IReadOnlyList<ToolDefinition> AllToolDefinitions()
        => _tools.Values.Select(t => t.Definition).ToList();

    // Execute a tool call after checking permissions.
    public async Task<OperationResult<string>> ExecuteAsync(
        string toolName,
        Dictionary<string, object> arguments)
    {
        if (!_tools.TryGetValue(toolName, out var entry))
            return OperationResult<string>.Fail($"Tool '{toolName}' is not registered.", "UNKNOWN_TOOL");

        var permission = _gate.Check(toolName, arguments);
        if (!permission.IsSuccess)
            return OperationResult<string>.Fail(permission.ErrorMessage, permission.ErrorCode);

        if (permission.Value == PermissionDecision.Denied)
            return OperationResult<string>.Fail($"Tool '{toolName}' is denied.", "PERMISSION_DENIED");

        // RequiresApproval is granted automatically in the library (the UI layer handles the dialog).
        return await entry.Handler(arguments);
    }
}
