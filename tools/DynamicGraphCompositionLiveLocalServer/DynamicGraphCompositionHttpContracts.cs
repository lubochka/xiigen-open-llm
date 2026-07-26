// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using System.Text.Json.Serialization;
using OrchestratorModel.Logical;

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer;

internal static class DynamicGraphCompositionJson
{
    internal static readonly JsonSerializerOptions HttpOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    internal static readonly JsonSerializerOptions CheckpointOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };
}

public sealed record CreateCompositionSessionHttpRequest(
    [property: JsonPropertyName("profile")] DynamicGraphLogicalOrchestratorBootstrapProfile? Profile,
    [property: JsonPropertyName("trained_input")] object? TrainedInput = null);

public sealed record CompositionEvidenceHttpResponse(
    [property: JsonPropertyName("profile_id")] string ProfileId,
    [property: JsonPropertyName("revision")] long Revision,
    [property: JsonPropertyName("source_kind")] string SourceKind,
    [property: JsonPropertyName("node_count")] int NodeCount,
    [property: JsonPropertyName("edge_count")] int EdgeCount,
    [property: JsonPropertyName("initial_revision_advanced")] bool InitialRevisionAdvanced);

public sealed record CompositionSessionHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("created_at_utc")] DateTimeOffset CreatedAtUtc,
    [property: JsonPropertyName("expires_at_utc")] DateTimeOffset ExpiresAtUtc,
    [property: JsonPropertyName("result")] CompositionEvidenceHttpResponse Result,
    [property: JsonPropertyName("boundaries")] CompositionBoundaryHttpResponse Boundaries);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RunCompositionSessionHttpRequest(
    [property: JsonPropertyName("user_request")] string? UserRequest);

public sealed record AssistantRunResultHttpResponse(
    [property: JsonPropertyName("scenario_id")] string ScenarioId,
    [property: JsonPropertyName("selected_flow_id")] string SelectedFlowId,
    [property: JsonPropertyName("overall_verdict")] string OverallVerdict,
    [property: JsonPropertyName("final_human_result")] string FinalHumanResult,
    [property: JsonPropertyName("metrics")] IReadOnlyDictionary<string, string> Metrics,
    [property: JsonPropertyName("evidence_ids")] string[] EvidenceIds);

public sealed record AssistantRunHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("result")] AssistantRunResultHttpResponse Result,
    [property: JsonPropertyName("boundaries")] CompositionBoundaryHttpResponse Boundaries);

public sealed record CompositionBoundaryHttpResponse(
    [property: JsonPropertyName("loopback_only")] bool LoopbackOnly = true,
    [property: JsonPropertyName("tenant_binding_is_authentication")] bool TenantBindingIsAuthentication = false,
    [property: JsonPropertyName("persistent")] bool Persistent = false,
    [property: JsonPropertyName("executes_assistant")] bool ExecutesAssistant = true,
    [property: JsonPropertyName("applies_graph")] bool AppliesGraph = false,
    [property: JsonPropertyName("performs_training")] bool PerformsTraining = false);

public sealed record SafeErrorHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message);

public sealed record ReadinessHttpResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason_code")] string? ReasonCode = null,
    [property: JsonPropertyName("loopback_only")] bool LoopbackOnly = true,
    [property: JsonPropertyName("tenant_binding_is_authentication")] bool TenantBindingIsAuthentication = false,
    [property: JsonPropertyName("persistent")] bool Persistent = false,
    [property: JsonPropertyName("executes_assistant")] bool ExecutesAssistant = true,
    [property: JsonPropertyName("applies_graph")] bool AppliesGraph = false,
    [property: JsonPropertyName("performs_training")] bool PerformsTraining = false);
