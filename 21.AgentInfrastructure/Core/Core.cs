// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

public sealed record StandardEvent(
    string EventType,
    string OperationId,
    DateTimeOffset OccurredAt,
    Dictionary<string, object> Payload);

public sealed record ConversationMessage(string Role, string Content);

public sealed record LlmResponse(
    string ResponseText,
    string? ToolCallName,
    Dictionary<string, object>? ToolCallArguments);

public sealed record ToolDefinition(
    string Name,
    string Description,
    Dictionary<string, object> ParameterSchema);

public abstract class BaseService
{
    protected readonly ISessionStorage Storage;

    protected BaseService(ISessionStorage storage)
        => Storage = storage;
}
