namespace OssEquivalents;

// Describes one Machine Control Protocol tool: name, description, and parameter schema.
// This is the .NET equivalent of the tool descriptor in @modelcontextprotocol/sdk.
public sealed record McpToolDefinition(
    string                     Name,
    string                     Description,
    Dictionary<string, object> ParameterSchema);
