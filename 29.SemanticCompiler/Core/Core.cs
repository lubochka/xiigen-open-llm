// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// ─── Result envelope ──────────────────────────────────────────────────────────
// Every public method returns OperationResult<T>. Never throw for business logic.
// Never return null. Never return bool without context. (Rule 1)
public sealed class OperationResult<T>
{
    // True when the operation completed without a business-level failure.
    public bool   IsSuccess    { get; }

    // The value produced by the operation when it succeeded; default(T) on failure.
    public T?     Value        { get; }

    // Plain-language description of what went wrong, empty when IsSuccess is true.
    public string ErrorMessage { get; }

    // Short stable code identifying the failure category, e.g. "EMPTY_INPUT".
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

    // Forward a failure from one result type to another, preserving message and code.
    public OperationResult<TNew> MapError<TNew>()
        => OperationResult<TNew>.Fail(ErrorMessage, ErrorCode);
}

// ─── Standard event envelope ──────────────────────────────────────────────────
// Every notification dispatched between components is wrapped in this envelope.
public sealed record StandardEvent(
    // Dot-separated event type, e.g. "compiler.compile.completed".
    string EventType,
    // The unique identifier of the operation that caused this event.
    string OperationId,
    // When the event was created — used for ordering and audit.
    DateTimeOffset OccurredAt,
    // Free-form payload as a dictionary so any structure can be carried.
    Dictionary<string, object> Payload);

// ─── Storage and logging contracts ────────────────────────────────────────────
// Fabric-first (Rule 2): external dependencies resolve through interfaces only.

// ICompilerStorage — persists pipeline results for idempotent re-runs.
public interface ICompilerStorage
{
    // Persist the compilation output keyed by its source text hash.
    void Save(string key, PipelineOutput output);

    // Retrieve a previously persisted output; returns null when absent.
    PipelineOutput? Load(string key);
}

// ICompilerEventLogger — records pipeline audit events.
public interface ICompilerEventLogger
{
    // Record one event produced during compilation.
    void Log(StandardEvent evt);
}

// ─── Base service (fabric-first pattern, Rule 2) ───────────────────────────────
// Every concrete service in this sub-project inherits from BaseService.
// Provides injected storage and event logger without any service constructing them.
public abstract class BaseService
{
    // Where the service persists its results — injected, never constructed here.
    protected readonly ICompilerStorage     Storage;

    // Where the service emits audit events — injected, never constructed here.
    protected readonly ICompilerEventLogger EventLogger;

    // Standard constructor: every concrete subclass calls : base(storage, logger).
    protected BaseService(ICompilerStorage storage, ICompilerEventLogger logger)
        => (Storage, EventLogger) = (storage, logger);
}

// ─── Domain enum (DECISIONS-LOCKED D-04) ─────────────────────────────────────
// Short forms in code and enum values. Long forms in comments only.
// CODE_Q=CodeQuestion, BUG=BugFix, PLAN=Planning, ARCH=Architecture, REFACTOR=Refactoring.
// Source: DECISIONS-LOCKED D-04.
public enum Domain { CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN }

// ─── Pipeline input ───────────────────────────────────────────────────────────
// Raw text entering the semantic compiler pipeline.
public sealed record PipelineInput(
    // Raw prompt or code fragment to compile.
    string Text,
    // Optional: source file path used by StackDetector for extension hints.
    string? FilePath);

// ─── Pipeline output ──────────────────────────────────────────────────────────
// Result after all five compression stages (E01→E17).
public sealed record PipelineOutput(
    // E18 RouteClassifier result: which of the five domains.
    Domain   Domain,
    // E17 LogicExtractor result: logical predicate tokens like "VALIDATE_OP(X)".
    string[] Predicates,
    // E20 StackDetector result: "csharp" | "typescript" | "python" | "unknown".
    string   StackLanguage,
    // original_tokens / predicate_count — how much the pipeline compressed the input.
    double   CompressionRatio);
