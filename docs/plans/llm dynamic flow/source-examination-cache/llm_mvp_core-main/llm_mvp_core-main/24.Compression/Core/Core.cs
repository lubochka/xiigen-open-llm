namespace Compression;

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
}

// ─── Standard event ──────────────────────────────────────────────────────────

public sealed record StandardEvent(
    string EventType,
    string OperationId,
    DateTimeOffset OccurredAt,
    Dictionary<string, object> Payload);

// ─── Base service ─────────────────────────────────────────────────────────────

public abstract class BaseService
{
    protected readonly ICompressionStorage Storage;
    protected BaseService(ICompressionStorage storage) => Storage = storage;
}
