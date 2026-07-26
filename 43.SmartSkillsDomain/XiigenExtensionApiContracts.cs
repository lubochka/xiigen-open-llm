// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

// Request sent by thin Claude/Cursor/Codex host clients to xiigen exe.
public sealed record XiigenExtensionRequest(
    string HostName,
    string HostType,
    string Method,
    string UserPromptOrTaskText,
    IReadOnlyDictionary<string, string> HostFacts,
    bool DebugEnabled,
    IReadOnlyList<string> RequestedSkills,
    IReadOnlyList<string> AvailableSkills,
    IReadOnlyDictionary<string, string> CheckSetInputs,
    IReadOnlyDictionary<string, string> Provenance,
    string CorrelationId,
    XiigenTargetLocator? TargetLocator = null);

public sealed record XiigenTargetLocator(
    string LocatorCandidateId,
    string TargetCandidateId,
    string RollbackCandidateId,
    string UserAdaptiveAssetRoot,
    string RollbackAssetRoot = "",
    string LocatorMode = "target",
    IReadOnlyDictionary<string, string>? LocatorRefs = null);

// Response returned by xiigen exe to thin host clients.
public sealed record XiigenExtensionResponse(
    bool Success,
    string Status,
    string ChatDisplayText,
    IReadOnlyList<string> SelectedSkills,
    IReadOnlyDictionary<string, string> CheckSetResults,
    IReadOnlyList<string> StructuredTaskRefs,
    IReadOnlyList<string> ScenarioRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors,
    XiigenDebugTrace? DebugTrace);

// Optional debug payload for programmer-facing chat display.
public sealed record XiigenDebugTrace(
    string Method,
    string InputPayloadText,
    string OutputPayloadText,
    bool ContainsRawPrivateContent = false,
    bool ContainsSecrets = false);

public sealed record XiigenExtensionRequestValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors);

public sealed record XiigenExtensionApiMethodContract(
    string Method,
    IReadOnlyList<string> AllowedRequestFields,
    IReadOnlyList<string> AllowedResponseFields,
    IReadOnlyList<string> ForbiddenPayloads,
    IReadOnlyList<string> BoundaryRules);

public static class XiigenExtensionApiContract
{
    public const string PromptToTaskMethod = "prompt-to-task";
    public const string OrchestrationPlanMethod = "orchestration-plan";
    public const string SkillSelectionMethod = "skill-selection";
    public const string FeedbackMethod = "feedback";
    public const string DebugTraceMethod = "debug-trace";

    public static readonly IReadOnlyList<string> MethodNames =
    [
        PromptToTaskMethod,
        OrchestrationPlanMethod,
        SkillSelectionMethod,
        FeedbackMethod,
        DebugTraceMethod
    ];

    private static readonly IReadOnlyList<string> CommonForbiddenPayloads =
    [
        "model_internals",
        "raw_corpus_data",
        "raw_private_prompt",
        "host_side_training_logic",
        "external_ai_call",
        "host_runtime_claim",
        "host_side_target_locator_resolution"
    ];

    private static readonly IReadOnlyList<string> CommonBoundaryRules =
    [
        "thin_host_request_only",
        "core_logic_stays_in_llm_mvp_core",
        "no_model_internals_or_corpus_rows_in_response",
        "semantic_quality_requires_trained_model_gate_refs",
        "runtime_extension_claimed_false_until_host_runtime_phase",
        "target_locator_is_request_metadata_only_core_resolves_assets"
    ];

    public static XiigenExtensionRequestValidationResult ValidateRequest(
        XiigenExtensionRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.HostName))
        {
            errors.Add("host_name_required");
        }

        if (string.IsNullOrWhiteSpace(request.HostType))
        {
            errors.Add("host_type_required");
        }

        if (string.IsNullOrWhiteSpace(request.Method))
        {
            errors.Add("method_required");
        }

        if (string.IsNullOrWhiteSpace(request.CorrelationId))
        {
            errors.Add("correlation_id_required");
        }

        if (IsTruthy(request.CheckSetInputs, "qsg19_target_locator_required", "target_locator_required")
            && request.TargetLocator is null)
        {
            errors.Add("target_locator_required");
        }

        if (request.TargetLocator is not null)
        {
            if (string.IsNullOrWhiteSpace(request.TargetLocator.LocatorCandidateId))
            {
                errors.Add("target_locator_candidate_id_required");
            }

            if (string.IsNullOrWhiteSpace(request.TargetLocator.TargetCandidateId))
            {
                errors.Add("target_locator_target_candidate_id_required");
            }

            var locatorMode = string.IsNullOrWhiteSpace(request.TargetLocator.LocatorMode)
                ? "target"
                : request.TargetLocator.LocatorMode.Trim();
            if (!string.Equals(locatorMode, "target", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(locatorMode, "rollback", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("target_locator_mode_invalid");
            }

            if (string.Equals(locatorMode, "rollback", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.TargetLocator.RollbackAssetRoot))
                {
                    errors.Add("rollback_locator_asset_root_required");
                }
            }
            else if (string.IsNullOrWhiteSpace(request.TargetLocator.UserAdaptiveAssetRoot))
            {
                errors.Add("target_locator_user_adaptive_asset_root_required");
            }
        }

        return new XiigenExtensionRequestValidationResult(errors.Count == 0, errors);
    }

    public static IReadOnlyList<XiigenExtensionApiMethodContract> GetMethodContracts()
        =>
        [
            new(
                Method: PromptToTaskMethod,
                AllowedRequestFields:
                [
                    nameof(XiigenExtensionRequest.HostName),
                    nameof(XiigenExtensionRequest.HostType),
                    nameof(XiigenExtensionRequest.Method),
                    nameof(XiigenExtensionRequest.UserPromptOrTaskText),
                    nameof(XiigenExtensionRequest.HostFacts),
                    nameof(XiigenExtensionRequest.CheckSetInputs),
                    nameof(XiigenExtensionRequest.Provenance),
                    nameof(XiigenExtensionRequest.CorrelationId),
                    nameof(XiigenExtensionRequest.TargetLocator)
                ],
                AllowedResponseFields:
                [
                    nameof(XiigenExtensionResponse.Success),
                    nameof(XiigenExtensionResponse.Status),
                    nameof(XiigenExtensionResponse.ChatDisplayText),
                    nameof(XiigenExtensionResponse.StructuredTaskRefs),
                    nameof(XiigenExtensionResponse.CheckSetResults),
                    nameof(XiigenExtensionResponse.Warnings),
                    nameof(XiigenExtensionResponse.Errors)
                ],
                ForbiddenPayloads: CommonForbiddenPayloads,
                BoundaryRules: CommonBoundaryRules),
            new(
                Method: OrchestrationPlanMethod,
                AllowedRequestFields:
                [
                    nameof(XiigenExtensionRequest.HostName),
                    nameof(XiigenExtensionRequest.HostType),
                    nameof(XiigenExtensionRequest.Method),
                    nameof(XiigenExtensionRequest.HostFacts),
                    nameof(XiigenExtensionRequest.CheckSetInputs),
                    nameof(XiigenExtensionRequest.Provenance),
                    nameof(XiigenExtensionRequest.CorrelationId),
                    nameof(XiigenExtensionRequest.TargetLocator),
                    "structured_task_ref"
                ],
                AllowedResponseFields:
                [
                    nameof(XiigenExtensionResponse.Success),
                    nameof(XiigenExtensionResponse.Status),
                    nameof(XiigenExtensionResponse.ChatDisplayText),
                    nameof(XiigenExtensionResponse.ScenarioRefs),
                    nameof(XiigenExtensionResponse.CheckSetResults),
                    nameof(XiigenExtensionResponse.Warnings),
                    nameof(XiigenExtensionResponse.Errors)
                ],
                ForbiddenPayloads: CommonForbiddenPayloads,
                BoundaryRules: CommonBoundaryRules),
            new(
                Method: SkillSelectionMethod,
                AllowedRequestFields:
                [
                    nameof(XiigenExtensionRequest.HostName),
                    nameof(XiigenExtensionRequest.HostType),
                    nameof(XiigenExtensionRequest.Method),
                    nameof(XiigenExtensionRequest.AvailableSkills),
                    nameof(XiigenExtensionRequest.RequestedSkills),
                    nameof(XiigenExtensionRequest.HostFacts),
                    nameof(XiigenExtensionRequest.Provenance),
                    nameof(XiigenExtensionRequest.CorrelationId),
                    nameof(XiigenExtensionRequest.TargetLocator),
                    "prompt_sha256"
                ],
                AllowedResponseFields:
                [
                    nameof(XiigenExtensionResponse.Success),
                    nameof(XiigenExtensionResponse.Status),
                    nameof(XiigenExtensionResponse.ChatDisplayText),
                    nameof(XiigenExtensionResponse.SelectedSkills),
                    nameof(XiigenExtensionResponse.CheckSetResults),
                    nameof(XiigenExtensionResponse.Warnings),
                    nameof(XiigenExtensionResponse.Errors)
                ],
                ForbiddenPayloads: CommonForbiddenPayloads,
                BoundaryRules: CommonBoundaryRules),
            new(
                Method: FeedbackMethod,
                AllowedRequestFields:
                [
                    nameof(XiigenExtensionRequest.HostName),
                    nameof(XiigenExtensionRequest.HostType),
                    nameof(XiigenExtensionRequest.Method),
                    nameof(XiigenExtensionRequest.Provenance),
                    nameof(XiigenExtensionRequest.CorrelationId),
                    nameof(XiigenExtensionRequest.TargetLocator),
                    "feedback_class",
                    "chosen_ref",
                    "rejected_ref",
                    "prompt_sha256",
                    "response_ref",
                    "correction_ref"
                ],
                AllowedResponseFields:
                [
                    nameof(XiigenExtensionResponse.Success),
                    nameof(XiigenExtensionResponse.Status),
                    nameof(XiigenExtensionResponse.ChatDisplayText),
                    nameof(XiigenExtensionResponse.CheckSetResults),
                    nameof(XiigenExtensionResponse.Warnings),
                    nameof(XiigenExtensionResponse.Errors)
                ],
                ForbiddenPayloads:
                [
                    ..CommonForbiddenPayloads,
                    "common_model_mutation",
                    "raw_private_correction_text"
                ],
                BoundaryRules:
                [
                    ..CommonBoundaryRules,
                    "feedback_routes_refs_only_to_user_local_adaptive_dpo_signal_creation",
                    "feedback_must_not_mutate_common_models",
                    "feedback_must_not_include_raw_private_content"
                ]),
            new(
                Method: DebugTraceMethod,
                AllowedRequestFields:
                [
                    nameof(XiigenExtensionRequest.HostName),
                    nameof(XiigenExtensionRequest.HostType),
                    nameof(XiigenExtensionRequest.Method),
                    nameof(XiigenExtensionRequest.DebugEnabled),
                    nameof(XiigenExtensionRequest.CorrelationId),
                    nameof(XiigenExtensionRequest.TargetLocator),
                    "redacted_input_payload",
                    "redacted_output_payload"
                ],
                AllowedResponseFields:
                [
                    nameof(XiigenExtensionResponse.Success),
                    nameof(XiigenExtensionResponse.Status),
                    nameof(XiigenExtensionResponse.ChatDisplayText),
                    nameof(XiigenExtensionResponse.DebugTrace),
                    nameof(XiigenExtensionResponse.Warnings),
                    nameof(XiigenExtensionResponse.Errors)
                ],
                ForbiddenPayloads:
                [
                    ..CommonForbiddenPayloads,
                    "debug_trace_when_raw_private_or_secret_flags_true"
                ],
                BoundaryRules:
                [
                    ..CommonBoundaryRules,
                    "chat_format_exact:xiigen <method> :\\ninput: <redacted>\\noutput: <redacted>",
                    "debug_trace_suppressed_if_raw_private_content_or_secrets_true"
                ])
        ];

    public static XiigenExtensionApiMethodContract? FindMethodContract(string method)
        => GetMethodContracts().FirstOrDefault(contract => string.Equals(contract.Method, method, StringComparison.Ordinal));

    private static bool IsTruthy(
        IReadOnlyDictionary<string, string>? source,
        params string[] keys)
        => keys.Any(key => source is not null
            && source.TryGetValue(key, out var value)
            && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase));

    public static string FormatDebugTraceForChat(
        bool debugEnabled,
        XiigenDebugTrace? debugTrace)
    {
        if (!debugEnabled || debugTrace is null)
        {
            return string.Empty;
        }

        if (debugTrace.ContainsRawPrivateContent || debugTrace.ContainsSecrets)
        {
            return string.Empty;
        }

        return string.Join(
            Environment.NewLine,
            $"xiigen {debugTrace.Method} :",
            $"input: {debugTrace.InputPayloadText}",
            $"output: {debugTrace.OutputPayloadText}");
    }

    public static string BuildChatDisplayText(
        string chatDisplayText,
        bool debugEnabled,
        XiigenDebugTrace? debugTrace)
    {
        var debugText = FormatDebugTraceForChat(debugEnabled, debugTrace);
        if (string.IsNullOrEmpty(debugText))
        {
            return chatDisplayText;
        }

        if (string.IsNullOrEmpty(chatDisplayText))
        {
            return debugText;
        }

        return string.Join(Environment.NewLine, chatDisplayText, debugText);
    }
}
