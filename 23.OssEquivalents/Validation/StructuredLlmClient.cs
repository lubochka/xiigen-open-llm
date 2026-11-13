// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;

namespace OssEquivalents;

// Wraps any ILlmClientWithSchema implementation to guarantee a validated structured response.
// If the model returns output that doesn't match the schema, sends an error-correction
// message and retries — up to maximumRetryCount times.
// This is the .NET equivalent of 'instructor' from the OSS plan.
public sealed class StructuredLlmClient
{
    private readonly ILlmClientWithSchema _rawClient;
    private readonly SchemaValidator      _validator;

    public StructuredLlmClient(ILlmClientWithSchema rawClient, SchemaValidator validator)
    {
        _rawClient = rawClient;
        _validator = validator;
    }

    // Call the language model and return a validated structured response.
    public async Task<OperationResult<Dictionary<string, object>>> SendAsync(
        IReadOnlyList<ConversationMessage> messageHistory,
        Dictionary<string, object>        expectedSchema,
        int                               maximumRetryCount = 3,
        CancellationToken                 cancellationToken = default)
    {
        return await _rawClient.SendWithSchemaAsync(
            messageHistory, expectedSchema, maximumRetryCount, cancellationToken);
    }
}

// ─── In-memory structured LLM client (used for testing) ──────────────────────

// Wraps a queue of pre-set responses. Each call dequeues one response and validates it.
public sealed class InMemoryStructuredLlmClient : ILlmClientWithSchema
{
    private readonly Queue<string>   _responses;
    private readonly SchemaValidator _validator;

    public int CallCount { get; private set; }

    public InMemoryStructuredLlmClient(IEnumerable<string> responses, SchemaValidator? validator = null)
    {
        _responses = new Queue<string>(responses);
        _validator  = validator ?? new SchemaValidator();
    }

    public async Task<OperationResult<Dictionary<string, object>>> SendWithSchemaAsync(
        IReadOnlyList<ConversationMessage> messageHistory,
        Dictionary<string, object>        expectedSchema,
        int                               maximumRetryCount = 3,
        CancellationToken                 cancellationToken = default)
    {
        int attempt = 0;
        var history = new List<ConversationMessage>(messageHistory);

        while (attempt < maximumRetryCount)
        {
            if (_responses.Count == 0)
                break;

            string rawJson = _responses.Dequeue();
            CallCount++;
            await Task.Yield(); // simulate async

            Dictionary<string, object>? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<Dictionary<string, object>>(rawJson);
            }
            catch
            {
                parsed = null;
            }

            if (parsed is not null)
            {
                var validation = _validator.Validate(parsed, expectedSchema);
                if (validation.IsSuccess)
                    return OperationResult<Dictionary<string, object>>.Ok(parsed);
            }

            attempt++;
        }

        return OperationResult<Dictionary<string, object>>.Fail(
            $"Did not produce a valid response after {maximumRetryCount} attempts.",
            "SCHEMA_VALIDATION_EXHAUSTED");
    }
}
