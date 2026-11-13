namespace AgentAlgorithms;

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

// ─── Conversation types ───────────────────────────────────────────────────────

public sealed record ConversationMessage(string Role, string Content);
