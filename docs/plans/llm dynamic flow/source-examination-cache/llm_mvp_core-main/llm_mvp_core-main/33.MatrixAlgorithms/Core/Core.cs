namespace MatrixAlgorithms;

// ─── Result envelope (Genie DNA rule 3) ────────────────────────────────────
// OperationResult<T> wraps every public-method return value so that callers
// always check whether the operation succeeded instead of catching exceptions.
// Business errors (singular matrix, wrong dimensions) use Fail(); only true
// programming bugs (null arguments) are allowed to throw.
public sealed class OperationResult<T>
{
    // True when the operation completed without a business-level failure.
    public bool   IsSuccess    { get; }

    // The value produced by the operation; default(T) when IsSuccess is false.
    public T?     Value        { get; }

    // Plain-language description of what went wrong; empty string on success.
    public string ErrorMessage { get; }

    // Short stable code that identifies the failure category, e.g. "SINGULAR_MATRIX".
    public string ErrorCode    { get; }

    // Private constructor — use the Ok and Fail factory methods below to create instances.
    private OperationResult(bool ok, T? value, string message, string code)
        => (IsSuccess, Value, ErrorMessage, ErrorCode) = (ok, value, message, code);

    // Build a result that carries a successfully produced value.
    public static OperationResult<T> Ok(T value)
        => new(true, value, string.Empty, string.Empty);

    // Build a result that carries no value but explains the failure in human language.
    public static OperationResult<T> Fail(string message, string code = "ERROR")
        => new(false, default, message, code);

    // Copy a failure from one result type to another, preserving the message and code.
    // Useful when a service receives a failed inner result and must return a different type.
    public OperationResult<TNew> MapError<TNew>()
        => OperationResult<TNew>.Fail(ErrorMessage, ErrorCode);
}

// ─── Standard event envelope (Genie DNA rule 9) ────────────────────────────
// Every notification dispatched between components is wrapped in this envelope.
// Receivers can deduplicate by OperationId and route by EventType without
// knowing anything about the payload shape.
public sealed record StandardEvent(
    // Dot-separated name of the event, e.g. "matrix.decomposition.completed".
    string EventType,
    // Unique identifier of the operation that produced this event — used for deduplication.
    string OperationId,
    // The exact moment the event was created — used for ordering and audit.
    DateTimeOffset OccurredAt,
    // Free-form payload dictionary so any result data can be carried without a typed class.
    Dictionary<string, object> Payload);

// ─── Idempotency store (Genie DNA rule 7) ──────────────────────────────────
// In-memory cache that maps an operationId to the result already produced for it.
// Any service that writes data or runs an expensive computation checks this cache
// first and returns the cached result instead of doing the work twice.
public sealed class IdempotencyStore
{
    // Internal dictionary: operationId → the payload produced the first time it ran.
    private readonly Dictionary<string, Dictionary<string, object>> _store = [];

    // Try to retrieve a previously stored result for the given operationId.
    // Returns true and sets result if the operationId was already processed; false otherwise.
    public bool TryGetResult(string operationId, out Dictionary<string, object> result)
        => _store.TryGetValue(operationId, out result!);

    // Record the produced result so that future calls with the same operationId get it back.
    public void Record(string operationId, Dictionary<string, object> result)
        => _store[operationId] = result;
}

// ─── Fabric interfaces needed by BaseService ───────────────────────────────

// IMatrixStorage is the persistence interface injected into every service.
// It saves and loads named result payloads without knowing where storage lives.
// Tests supply an in-memory implementation; production wires a file-based one.
public interface IMatrixStorage
{
    // Persist a result payload under the given name key.
    // Returns true when the save succeeded; false on storage failure.
    bool TrySave(string key, Dictionary<string, object> data);

    // Retrieve a previously saved payload by name.
    // Returns true and sets data if the key exists; false when not found.
    bool TryLoad(string key, out Dictionary<string, object> data);
}

// IMatrixEventLogger is the observability interface injected into every service.
// It records StandardEvent notifications so external observers can react or audit.
public interface IMatrixEventLogger
{
    // Record one event — the service calls this after saving its result.
    void Log(StandardEvent evt);
}

// ─── Base service (Genie DNA rule 4) ───────────────────────────────────────
// Every concrete service in this sub-project inherits from BaseService.
// It provides injected storage, an event logger, and a per-instance
// idempotency store without any service needing to construct them itself.
public abstract class BaseService
{
    // Where the service persists its results — injected, never constructed here.
    protected readonly IMatrixStorage    Storage;

    // Where the service emits audit events — injected, never constructed here.
    protected readonly IMatrixEventLogger EventLogger;

    // Per-instance cache ensuring the same operationId is never processed twice.
    protected readonly IdempotencyStore  Idempotency = new();

    // Constructor that every subclass calls via : base(storage, logger).
    protected BaseService(IMatrixStorage storage, IMatrixEventLogger logger)
        => (Storage, EventLogger) = (storage, logger);
}
