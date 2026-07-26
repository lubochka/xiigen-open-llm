// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MachineLearning;

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

// ─── Idempotency store ───────────────────────────────────────────────────────

public sealed class IdempotencyStore
{
    private readonly Dictionary<string, Dictionary<string, object>> _store = [];

    public bool TryGetResult(string operationId, out Dictionary<string, object> result)
        => _store.TryGetValue(operationId, out result!);

    public void Record(string operationId, Dictionary<string, object> result)
        => _store[operationId] = result;
}

// ─── Base service ────────────────────────────────────────────────────────────

public abstract class BaseService
{
    protected readonly IModelStateStorage  Storage;
    protected readonly IPredictionLogger   Logger;
    protected readonly IdempotencyStore    Idempotency = new();

    protected BaseService(IModelStateStorage storage, IPredictionLogger logger)
        => (Storage, Logger) = (storage, logger);
}
