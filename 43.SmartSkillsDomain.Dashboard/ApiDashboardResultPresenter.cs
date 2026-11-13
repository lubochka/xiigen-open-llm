// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Dashboard;

using System.Text.Json;
using AgentInfrastructure;
using SmartSkillsDomain;

public sealed record ApiDashboardEvidenceRow(
    string Key,
    string Value);

public sealed record ApiDashboardBoundaryFlag(
    string Key,
    bool Claimed,
    string DisplayText);

public sealed record ApiDashboardResultViewModel(
    bool Success,
    string Status,
    string HumanResult,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> SelectedSkills,
    IReadOnlyList<string> StructuredTaskRefs,
    IReadOnlyList<string> ScenarioRefs,
    IReadOnlyList<ApiDashboardEvidenceRow> CheckSetResults,
    bool IsRawJsonVisible,
    string RawJsonText,
    bool IsDebugTraceVisible,
    string DebugTraceText,
    string DebugTraceSuppressionReason,
    bool AreBoundaryFlagsVisible,
    bool RuntimeExtensionProofClaimed,
    bool SemanticQualityOrTrainedModelProofClaimed,
    IReadOnlyList<ApiDashboardBoundaryFlag> BoundaryFlags);

public static class ApiDashboardResultPresenter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static OperationResult<ApiDashboardResultViewModel> BuildViewModel(
        XiigenExtensionResponse response,
        bool debugRequested,
        bool showRawJson = false)
    {
        if (response is null)
        {
            return OperationResult<ApiDashboardResultViewModel>.Fail(
                "Dashboard result presenter requires a xiigen extension response.",
                "API_DASHBOARD_RESPONSE_REQUIRED");
        }

        var debugTraceText = XiigenExtensionApiContract.FormatDebugTraceForChat(
            debugRequested,
            response.DebugTrace);
        var runtimeProofClaimed = IsExplicitTrue(response.CheckSetResults, "runtime_extension_claimed");
        var semanticProofClaimed = IsExplicitTrue(response.CheckSetResults, "semantic_quality_claimed")
            || IsExplicitTrue(response.CheckSetResults, "trained_model_proof_claimed");
        var viewModel = new ApiDashboardResultViewModel(
            Success: response.Success,
            Status: response.Status,
            HumanResult: response.ChatDisplayText,
            Warnings: SafeList(response.Warnings),
            Errors: SafeList(response.Errors),
            SelectedSkills: SafeList(response.SelectedSkills),
            StructuredTaskRefs: SafeList(response.StructuredTaskRefs),
            ScenarioRefs: SafeList(response.ScenarioRefs),
            CheckSetResults: BuildEvidenceRows(response.CheckSetResults),
            IsRawJsonVisible: showRawJson,
            RawJsonText: BuildRawJsonText(response, showRawJson),
            IsDebugTraceVisible: !string.IsNullOrWhiteSpace(debugTraceText),
            DebugTraceText: debugTraceText,
            DebugTraceSuppressionReason: BuildDebugTraceSuppressionReason(debugRequested, response.DebugTrace, debugTraceText),
            AreBoundaryFlagsVisible: true,
            RuntimeExtensionProofClaimed: runtimeProofClaimed,
            SemanticQualityOrTrainedModelProofClaimed: semanticProofClaimed,
            BoundaryFlags: BuildBoundaryFlags(runtimeProofClaimed, semanticProofClaimed));

        return OperationResult<ApiDashboardResultViewModel>.Ok(viewModel);
    }

    private static IReadOnlyList<ApiDashboardEvidenceRow> BuildEvidenceRows(
        IReadOnlyDictionary<string, string>? checkSetResults)
        => (checkSetResults ?? new Dictionary<string, string>())
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new ApiDashboardEvidenceRow(entry.Key, entry.Value))
            .ToArray();

    private static string BuildRawJsonText(
        XiigenExtensionResponse response,
        bool showRawJson)
    {
        if (!showRawJson)
        {
            return string.Empty;
        }

        var safeResponse = response.DebugTrace is { ContainsRawPrivateContent: true } or { ContainsSecrets: true }
            ? response with
            {
                DebugTrace = new XiigenDebugTrace(
                    response.DebugTrace.Method,
                    "[suppressed unsafe debug trace input]",
                    "[suppressed unsafe debug trace output]",
                    response.DebugTrace.ContainsRawPrivateContent,
                    response.DebugTrace.ContainsSecrets)
            }
            : response;

        return JsonSerializer.Serialize(safeResponse, JsonOptions);
    }

    private static IReadOnlyList<ApiDashboardBoundaryFlag> BuildBoundaryFlags(
        bool runtimeProofClaimed,
        bool semanticProofClaimed)
        =>
        [
            new(
                "runtime_extension_claimed",
                runtimeProofClaimed,
                runtimeProofClaimed
                    ? "Response explicitly claims extension host runtime proof."
                    : "Dashboard result is not Claude, Cursor, or Codex host runtime proof."),
            new(
                "semantic_quality_claimed",
                semanticProofClaimed,
                semanticProofClaimed
                    ? "Response explicitly claims semantic quality or trained-model proof."
                    : "Dashboard result is not semantic quality or trained-model proof.")
        ];

    private static string BuildDebugTraceSuppressionReason(
        bool debugRequested,
        XiigenDebugTrace? debugTrace,
        string debugTraceText)
    {
        if (!string.IsNullOrWhiteSpace(debugTraceText))
        {
            return string.Empty;
        }

        if (!debugRequested)
        {
            return "Debug trace was not requested.";
        }

        if (debugTrace is null)
        {
            return "Core returned no debug trace.";
        }

        if (debugTrace.ContainsRawPrivateContent || debugTrace.ContainsSecrets)
        {
            return "Debug trace was suppressed because it was marked unsafe.";
        }

        return "Debug trace was suppressed.";
    }

    private static bool IsExplicitTrue(
        IReadOnlyDictionary<string, string>? values,
        string key)
        => values is not null
            && values.TryGetValue(key, out var value)
            && bool.TryParse(value, out var parsed)
            && parsed;

    private static IReadOnlyList<string> SafeList(
        IReadOnlyList<string>? source)
        => source?.ToArray() ?? [];
}
