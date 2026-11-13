using System.Text.Json;

namespace OssEquivalents;

// ─── MCP request / response ───────────────────────────────────────────────────

public sealed record McpRequest(string ToolName, Dictionary<string, object> Arguments);
public sealed record McpResponse(bool Success, Dictionary<string, object>? Result, string? Error);

// ─── MCP tool dispatcher ──────────────────────────────────────────────────────

// Routes incoming Machine Control Protocol tool-call messages to the correct handler.
// Handlers are registered by tool name; unknown tools return an error response.
public sealed class McpToolDispatcher
{
    private readonly Dictionary<string, Func<Dictionary<string, object>, Task<Dictionary<string, object>>>> _handlers
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<McpToolDefinition> _definitions = [];

    // Register a tool handler under the given definition.
    public void Register(
        McpToolDefinition definition,
        Func<Dictionary<string, object>, Task<Dictionary<string, object>>> handler)
    {
        _definitions.Add(definition);
        _handlers[definition.Name] = handler;
    }

    public IReadOnlyList<McpToolDefinition> Definitions => _definitions;

    // Dispatch a raw JSON message string to the correct handler.
    // Returns a McpResponse serialized as JSON.
    public async Task<OperationResult<string>> DispatchAsync(string jsonMessage)
    {
        McpRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<McpRequest>(jsonMessage,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            return OperationResult<string>.Fail($"Invalid JSON: {ex.Message}", "PARSE_ERROR");
        }

        if (request is null)
            return OperationResult<string>.Fail("Request was null after parsing.", "NULL_REQUEST");

        if (!_handlers.TryGetValue(request.ToolName, out var handler))
        {
            var err = new McpResponse(false, null, $"Tool '{request.ToolName}' not registered.");
            return OperationResult<string>.Ok(JsonSerializer.Serialize(err));
        }

        try
        {
            var result   = await handler(request.Arguments);
            var response = new McpResponse(true, result, null);
            return OperationResult<string>.Ok(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            var errResponse = new McpResponse(false, null, ex.Message);
            return OperationResult<string>.Ok(JsonSerializer.Serialize(errResponse));
        }
    }
}
