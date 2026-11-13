// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// ─── Permission levels ────────────────────────────────────────────────────────

public enum PermissionLevel { ReadOnly, WriteWithApproval, AlwaysDenied }

// ─── Permission model ─────────────────────────────────────────────────────────

// Rule-based permission checking for tool calls.
// Each tool is assigned a permission level; the model returns the level for a given name.
public sealed class PermissionModel
{
    private readonly Dictionary<string, PermissionLevel> _rules
        = new(StringComparer.OrdinalIgnoreCase);

    // Register a default set of rules covering common agent tools.
    public PermissionModel()
    {
        // Read-only tools are always permitted automatically.
        Register("read_file",      PermissionLevel.ReadOnly);
        Register("list_directory", PermissionLevel.ReadOnly);
        Register("search_code",    PermissionLevel.ReadOnly);
        Register("get_file_info",  PermissionLevel.ReadOnly);

        // Write tools require explicit approval from the user.
        Register("edit_file",   PermissionLevel.WriteWithApproval);
        Register("write_file",  PermissionLevel.WriteWithApproval);
        Register("apply_patch", PermissionLevel.WriteWithApproval);

        // Destructive operations are denied unless the user unlocks them explicitly.
        Register("delete_file",    PermissionLevel.AlwaysDenied);
        Register("run_command",    PermissionLevel.AlwaysDenied);
        Register("execute_script", PermissionLevel.AlwaysDenied);
    }

    // Add or overwrite a rule for a tool.
    public void Register(string toolName, PermissionLevel level)
        => _rules[toolName] = level;

    // Return the permission level for a tool.
    // Unknown tools default to AlwaysDenied (safe default).
    public OperationResult<PermissionLevel> Check(string toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName))
            return OperationResult<PermissionLevel>.Fail("Tool name cannot be empty.", "INVALID_TOOL");

        var level = _rules.TryGetValue(toolName, out var l) ? l : PermissionLevel.AlwaysDenied;
        return OperationResult<PermissionLevel>.Ok(level);
    }
}
