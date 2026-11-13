namespace AgentInfrastructure;

// ─── Permission decision ──────────────────────────────────────────────────────

public enum PermissionDecision { Allowed, RequiresApproval, Denied }

// ─── Permission gate ──────────────────────────────────────────────────────────

// Decides whether a tool call is permitted before it executes.
// Three tiers:
//   Allowed        — read-only tools (read_file, list_directory, search_code)
//   RequiresApproval — write tools (edit_file, write_file)
//   Denied         — destructive or out-of-project-root tools
public sealed class PermissionGate
{
    private readonly string _projectRootPath;

    // readOnlyTools — tool names that are always auto-approved.
    private static readonly HashSet<string> _readOnly = new(StringComparer.OrdinalIgnoreCase)
        { "read_file", "list_directory", "search_code" };

    // writeTools — require explicit user approval.
    private static readonly HashSet<string> _writeWithApproval = new(StringComparer.OrdinalIgnoreCase)
        { "edit_file", "write_file", "apply_patch" };

    public PermissionGate(string projectRootPath)
        => _projectRootPath = Path.GetFullPath(projectRootPath);

    // Check the permission level for a named tool call.
    public OperationResult<PermissionDecision> Check(
        string toolName,
        Dictionary<string, object> toolArguments)
    {
        if (string.IsNullOrWhiteSpace(toolName))
            return OperationResult<PermissionDecision>.Fail("Tool name cannot be empty.", "INVALID_TOOL");

        // For file-targeting tools, verify the target path is inside the project root.
        if (toolArguments.TryGetValue("path", out var pathObj) && pathObj is string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);
            if (!fullPath.StartsWith(_projectRootPath, StringComparison.OrdinalIgnoreCase))
                return OperationResult<PermissionDecision>.Ok(PermissionDecision.Denied);
        }

        if (_readOnly.Contains(toolName))
            return OperationResult<PermissionDecision>.Ok(PermissionDecision.Allowed);

        if (_writeWithApproval.Contains(toolName))
            return OperationResult<PermissionDecision>.Ok(PermissionDecision.RequiresApproval);

        // Unknown tools are denied by default.
        return OperationResult<PermissionDecision>.Ok(PermissionDecision.Denied);
    }
}
