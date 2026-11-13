namespace AgentInfrastructure;

// ─── Result envelope ────────────────────────────────────────────────────────

public sealed class OperationResult<T>
{
    public bool   IsSuccess    { get; }
    public T?     Value        { get; }
    public string ErrorMessage { get; }
    public string ErrorCode    { get; }

    private OperationResult(bool ok, T? v, string msg, string code)
        => (IsSuccess, Value, ErrorMessage, ErrorCode) = (ok, v, msg, code);

    public static OperationResult<T> Ok(T value)
        => new(true, value, "", "");
    public static OperationResult<T> Fail(string message, string code = "ERROR")
        => new(false, default, message, code);

    public OperationResult<TNew> MapError<TNew>()
        => OperationResult<TNew>.Fail(ErrorMessage, ErrorCode);
}

// ─── Standard event ──────────────────────────────────────────────────────────

public sealed record StandardEvent(
    string EventType,
    string OperationId,
    DateTimeOffset OccurredAt,
    Dictionary<string, object> Payload);

// ─── Conversation types ───────────────────────────────────────────────────────

public sealed record ConversationMessage(string Role, string Content);

public sealed record LlmResponse(string ResponseText, string? ToolCallName, Dictionary<string, object>? ToolCallArguments);

public sealed record ToolDefinition(string Name, string Description, Dictionary<string, object> ParameterSchema);

// ─── Base service ────────────────────────────────────────────────────────────

public abstract class BaseService
{
    protected readonly ISessionStorage Storage;

    protected BaseService(ISessionStorage storage)
        => Storage = storage;
}
