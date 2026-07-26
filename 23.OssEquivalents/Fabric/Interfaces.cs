// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OssEquivalents;

// Calls the language model and validates the response against a declared JSON Schema.
// Retries automatically if the response does not match the schema.
public interface ILlmClientWithSchema
{
    Task<OperationResult<Dictionary<string, object>>> SendWithSchemaAsync(
        IReadOnlyList<ConversationMessage> messageHistory,
        Dictionary<string, object>        expectedSchema,
        int                               maximumRetryCount = 3,
        CancellationToken                 cancellationToken = default);
}

// Executes a named tool with given arguments and returns the result.
public interface IToolExecutor
{
    Task<OperationResult<Dictionary<string, object>>> ExecuteAsync(
        string                     toolName,
        Dictionary<string, object> arguments,
        CancellationToken          cancellationToken = default);
}
