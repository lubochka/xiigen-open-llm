// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticNlp;

// ─── Result envelope ────────────────────────────────────────────────────────
// OperationResult<T> wraps every public-method return value with an explicit
// success flag, a value, an error message, and an error code. Callers must
// always inspect IsSuccess instead of catching exceptions for business errors.
public sealed class OperationResult<T>
{
    // True when the operation completed without a business-level failure.
    public bool   IsSuccess    { get; }

    // The value produced by the operation when it succeeded; default(T) on failure.
    public T?     Value        { get; }

    // Plain-language description of what went wrong, empty when IsSuccess is true.
    public string ErrorMessage { get; }

    // Short stable code identifying the failure category (for example "EMPTY_INPUT").
    public string ErrorCode    { get; }

    // Private constructor — callers create instances via the Ok/Fail factory methods.
    private OperationResult(bool ok, T? v, string msg, string code)
        => (IsSuccess, Value, ErrorMessage, ErrorCode) = (ok, v, msg, code);

    // Build a successful result containing the produced value.
    public static OperationResult<T> Ok(T value)
        => new(true, value, "", "");

    // Build a failed result with a human-readable message and a stable code.
    public static OperationResult<T> Fail(string message, string code = "ERROR")
        => new(false, default, message, code);

    // Forward a failure from one result type to another while preserving message and code.
    public OperationResult<TNew> MapError<TNew>()
        => OperationResult<TNew>.Fail(ErrorMessage, ErrorCode);
}

// ─── Standard event envelope (DNA rule 9) ───────────────────────────────────
// Every notification dispatched between components is wrapped in this envelope
// so any receiver can deduplicate by OperationId and route by EventType.
public sealed record StandardEvent(
    // Dot-separated event type, for example "semantic.annotation.completed".
    string EventType,
    // The unique identifier of the operation that caused this event.
    string OperationId,
    // When the event was created — used for ordering and audit.
    DateTimeOffset OccurredAt,
    // Free-form payload as a dictionary so any structure can be carried.
    Dictionary<string, object> Payload);

// ─── Idempotency store (DNA rule 7) ──────────────────────────────────────────
// In-memory cache from operationId to its produced result. Re-running the same
// operationId returns the cached result instead of doing the work twice.
public sealed class IdempotencyStore
{
    // Storage map: operationId → produced result dictionary.
    private readonly Dictionary<string, Dictionary<string, object>> _store = [];

    // Look up a previously recorded result; returns true and sets result if present.
    public bool TryGetResult(string operationId, out Dictionary<string, object> result)
        => _store.TryGetValue(operationId, out result!);

    // Store the produced result so future calls with the same operationId skip the work.
    public void Record(string operationId, Dictionary<string, object> result)
        => _store[operationId] = result;
}

// ─── Base service (DNA rule 4) ───────────────────────────────────────────────
// Every concrete service inherits from BaseService so it gets a storage handle,
// an annotation logger, and a per-instance idempotency store for free.
public abstract class BaseService
{
    // Where to persist annotation outputs — injected via constructor (fabric-first).
    protected readonly ISemanticStorage Storage;

    // Where to record annotation events for audit and observability.
    protected readonly ISemanticEventLogger EventLogger;

    // Per-instance cache that prevents the same operationId from being processed twice.
    protected readonly IdempotencyStore Idempotency = new();

    // Standard constructor — concrete services accept the same fabric pair.
    protected BaseService(ISemanticStorage storage, ISemanticEventLogger eventLogger)
        => (Storage, EventLogger) = (storage, eventLogger);
}
