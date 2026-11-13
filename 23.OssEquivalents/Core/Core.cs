// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OssEquivalents;

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

// ─── Shared message type ──────────────────────────────────────────────────────

public sealed record ConversationMessage(string Role, string Content);
