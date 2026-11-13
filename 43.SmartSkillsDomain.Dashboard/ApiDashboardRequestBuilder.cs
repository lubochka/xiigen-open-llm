// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Dashboard;

using AgentInfrastructure;
using SmartSkillsDomain;

public static class ApiDashboardRequestBuilder
{
    public const string DashboardHostName = "ApiDashboard";
    public const string DashboardHostType = "llm_mvp_core_dashboard";

    public static OperationResult<XiigenExtensionRequest> BuildRequest(
        ApiDashboardCatalogRow selectedRow,
        ApiDashboardFixtureExampleRow selectedFixture,
        bool debugEnabled,
        string? correlationId = null)
    {
        if (selectedRow is null)
        {
            return OperationResult<XiigenExtensionRequest>.Fail(
                "Dashboard API request requires a selected catalog row.",
                "API_DASHBOARD_CATALOG_ROW_REQUIRED");
        }

        if (selectedFixture is null)
        {
            return OperationResult<XiigenExtensionRequest>.Fail(
                "Dashboard fixture request requires a selected fixture example.",
                "API_DASHBOARD_FIXTURE_EXAMPLE_REQUIRED");
        }

        if (!string.Equals(selectedRow.MethodId, selectedFixture.MethodId, StringComparison.Ordinal))
        {
            return OperationResult<XiigenExtensionRequest>.Fail(
                $"Dashboard fixture '{selectedFixture.FixtureId}' belongs to method '{selectedFixture.MethodId}', not '{selectedRow.MethodId}'.",
                "API_DASHBOARD_FIXTURE_METHOD_MISMATCH");
        }

        return BuildRequest(
            selectedRow.MethodId,
            selectedFixture.ExampleInputText,
            debugEnabled,
            correlationId,
            hostFacts: BuildFixtureHostFacts(selectedFixture),
            requestedSkills: null,
            availableSkills: null,
            checkSetInputs: BuildFixtureCheckSetInputs(selectedFixture),
            provenance: BuildFixtureProvenance(selectedFixture));
    }

    public static OperationResult<XiigenExtensionRequest> BuildRequest(
        ApiDashboardCatalogRow selectedRow,
        string textInput,
        bool debugEnabled,
        string? correlationId = null,
        IReadOnlyDictionary<string, string>? hostFacts = null,
        IReadOnlyList<string>? requestedSkills = null,
        IReadOnlyList<string>? availableSkills = null,
        IReadOnlyDictionary<string, string>? checkSetInputs = null,
        IReadOnlyDictionary<string, string>? provenance = null)
    {
        if (selectedRow is null)
        {
            return OperationResult<XiigenExtensionRequest>.Fail(
                "Dashboard API request requires a selected catalog row.",
                "API_DASHBOARD_CATALOG_ROW_REQUIRED");
        }

        return BuildRequest(
            selectedRow.MethodId,
            textInput,
            debugEnabled,
            correlationId,
            hostFacts,
            requestedSkills,
            availableSkills,
            checkSetInputs,
            provenance);
    }

    public static OperationResult<XiigenExtensionRequest> BuildRequest(
        string methodId,
        string textInput,
        bool debugEnabled,
        string? correlationId = null,
        IReadOnlyDictionary<string, string>? hostFacts = null,
        IReadOnlyList<string>? requestedSkills = null,
        IReadOnlyList<string>? availableSkills = null,
        IReadOnlyDictionary<string, string>? checkSetInputs = null,
        IReadOnlyDictionary<string, string>? provenance = null)
    {
        var method = methodId?.Trim() ?? string.Empty;
        if (XiigenExtensionApiContract.FindMethodContract(method) is null)
        {
            return OperationResult<XiigenExtensionRequest>.Fail(
                $"Dashboard API method '{method}' is not supported by the xiigen extension contract.",
                "API_DASHBOARD_UNSUPPORTED_METHOD");
        }

        var text = textInput?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return OperationResult<XiigenExtensionRequest>.Fail(
                "Dashboard API request text is required before the core boundary can be called.",
                "API_DASHBOARD_TEXT_REQUIRED");
        }

        var safeCorrelationId = BuildCorrelationId(method, correlationId);
        var request = new XiigenExtensionRequest(
            HostName: DashboardHostName,
            HostType: DashboardHostType,
            Method: method,
            UserPromptOrTaskText: text,
            HostFacts: BuildHostFacts(hostFacts),
            DebugEnabled: debugEnabled,
            RequestedSkills: SafeList(requestedSkills),
            AvailableSkills: SafeList(availableSkills),
            CheckSetInputs: BuildCheckSetInputs(checkSetInputs),
            Provenance: BuildProvenance(provenance, safeCorrelationId),
            CorrelationId: safeCorrelationId);

        return OperationResult<XiigenExtensionRequest>.Ok(request);
    }

    private static string BuildCorrelationId(
        string method,
        string? correlationId)
    {
        var provided = correlationId?.Trim();
        return string.IsNullOrWhiteSpace(provided)
            ? $"api-dashboard-{method}-{Guid.NewGuid():N}"
            : provided;
    }

    private static IReadOnlyDictionary<string, string> BuildHostFacts(
        IReadOnlyDictionary<string, string>? hostFacts)
    {
        var facts = CopyDictionary(hostFacts);
        facts["host_repo"] = "llm_mvp_core";
        facts["host_entrypoint"] = "43.SmartSkillsDomain.Dashboard";
        facts["host_boundary"] = "dashboard_request_builder_only";
        facts["host_semantic_ranking"] = "false";
        facts["host_training"] = "false";
        facts["host_corpus_parsing"] = "false";
        facts["fixture_preview_only"] = facts.TryGetValue("fixture_preview_only", out var fixturePreview)
            ? fixturePreview
            : "false";
        facts["external_ai"] = "false";
        facts["runtime_extension_claimed"] = "false";
        facts["semantic_quality_claimed"] = "false";
        return facts;
    }

    private static IReadOnlyDictionary<string, string> BuildCheckSetInputs(
        IReadOnlyDictionary<string, string>? checkSetInputs)
    {
        var inputs = CopyDictionary(checkSetInputs);
        inputs["dashboard_request_builder"] = "true";
        inputs["fixture_preview_only"] = inputs.TryGetValue("fixture_preview_only", out var fixturePreview)
            ? fixturePreview
            : "false";
        inputs["runtime_extension_claimed"] = "false";
        inputs["semantic_quality_claimed"] = "false";
        return inputs;
    }

    private static IReadOnlyDictionary<string, string> BuildProvenance(
        IReadOnlyDictionary<string, string>? provenance,
        string correlationId)
    {
        var values = CopyDictionary(provenance);
        values["source"] = "api-dashboard";
        values["dashboard_component"] = "ApiDashboardRequestBuilder";
        values["prompt_payload_location"] = "request_only_redacted_in_trace";
        values["raw_prompt_trace_policy"] = "redacted_in_trace";
        values["fixture_preview_only"] = values.TryGetValue("fixture_preview_only", out var fixturePreview)
            ? fixturePreview
            : "false";
        values["correlation_id"] = correlationId;
        return values;
    }

    private static IReadOnlyDictionary<string, string> BuildFixtureHostFacts(
        ApiDashboardFixtureExampleRow selectedFixture)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["fixture_preview_only"] = "true",
            ["fixture_machine"] = selectedFixture.Machine,
            ["fixture_id"] = selectedFixture.FixtureId,
            ["scenario_id"] = selectedFixture.ScenarioId,
            ["training_allowed"] = selectedFixture.TrainingAllowed.ToString().ToLowerInvariant(),
            ["eval_execution_allowed"] = selectedFixture.EvalExecutionAllowed.ToString().ToLowerInvariant(),
            ["not_executed_yet"] = selectedFixture.NotExecutedYet.ToString().ToLowerInvariant()
        };

    private static IReadOnlyDictionary<string, string> BuildFixtureCheckSetInputs(
        ApiDashboardFixtureExampleRow selectedFixture)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["fixture_preview_only"] = "true",
            ["fixture_id"] = selectedFixture.FixtureId,
            ["scenario_id"] = selectedFixture.ScenarioId,
            ["heldout_source_family"] = selectedFixture.HeldoutSourceFamily,
            ["fixture_expected_visible_result"] = selectedFixture.ExpectedVisibleResult,
            ["training_allowed"] = selectedFixture.TrainingAllowed.ToString().ToLowerInvariant(),
            ["eval_execution_allowed"] = selectedFixture.EvalExecutionAllowed.ToString().ToLowerInvariant(),
            ["not_executed_yet"] = selectedFixture.NotExecutedYet.ToString().ToLowerInvariant(),
            ["raw_private_text_copied"] = selectedFixture.RawPrivateTextCopied.ToString().ToLowerInvariant()
        };

    private static IReadOnlyDictionary<string, string> BuildFixtureProvenance(
        ApiDashboardFixtureExampleRow selectedFixture)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["fixture_preview_only"] = "true",
            ["fixture_id"] = selectedFixture.FixtureId,
            ["scenario_id"] = selectedFixture.ScenarioId,
            ["fixture_source"] = "p36/p37 semantic-quality fixture catalog",
            ["evidence_scope"] = selectedFixture.EvidenceScope,
            ["semantic_quality_claimed"] = "false",
            ["runtime_extension_claimed"] = "false"
        };

    private static Dictionary<string, string> CopyDictionary(
        IReadOnlyDictionary<string, string>? source)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (source is null)
        {
            return values;
        }

        foreach (var entry in source)
        {
            values[entry.Key] = entry.Value;
        }

        return values;
    }

    private static IReadOnlyList<string> SafeList(
        IReadOnlyList<string>? source)
        => source?.ToArray() ?? [];
}
