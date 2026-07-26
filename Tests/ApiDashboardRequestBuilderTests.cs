// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SmartSkillsDomain;
using SmartSkillsDomain.Dashboard;

public sealed class ApiDashboardRequestBuilderTests
{
    [Fact]
    public void ApiDashboardRequestBuilder_BuildRequest_FromCatalogRowBuildsContractShapedRequest()
    {
        var catalog = ApiCatalogProvider.BuildCatalog().Value!;
        var row = Assert.Single(catalog, item => item.MethodId == XiigenExtensionApiContract.PromptToTaskMethod);

        var result = ApiDashboardRequestBuilder.BuildRequest(
            row,
            "  Build a dashboard request  ",
            debugEnabled: true,
            correlationId: "corr-test-001",
            hostFacts: new Dictionary<string, string>
            {
                ["workspace"] = "C:/sandbox/llm_mvp_core",
                ["external_ai"] = "true"
            },
            requestedSkills: ["api-shape-verification"],
            availableSkills: ["api-shape-verification", "test-integrity"],
            checkSetInputs: new Dictionary<string, string>
            {
                ["constraint.scope"] = "dashboard-only"
            },
            provenance: new Dictionary<string, string>
            {
                ["source_prompt_entry_id"] = "UPSL-test"
            });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var request = result.Value!;
        Assert.Equal(ApiDashboardRequestBuilder.DashboardHostName, request.HostName);
        Assert.Equal(ApiDashboardRequestBuilder.DashboardHostType, request.HostType);
        Assert.Equal(XiigenExtensionApiContract.PromptToTaskMethod, request.Method);
        Assert.Equal("Build a dashboard request", request.UserPromptOrTaskText);
        Assert.True(request.DebugEnabled);
        Assert.Equal("corr-test-001", request.CorrelationId);
        Assert.Equal("C:/sandbox/llm_mvp_core", request.HostFacts["workspace"]);
        Assert.Equal("false", request.HostFacts["external_ai"]);
        Assert.Equal("false", request.HostFacts["host_semantic_ranking"]);
        Assert.Equal("false", request.HostFacts["host_training"]);
        Assert.Equal("false", request.HostFacts["host_corpus_parsing"]);
        Assert.Equal("false", request.HostFacts["runtime_extension_claimed"]);
        Assert.Equal("false", request.HostFacts["semantic_quality_claimed"]);
        Assert.Equal(["api-shape-verification"], request.RequestedSkills);
        Assert.Equal(["api-shape-verification", "test-integrity"], request.AvailableSkills);
        Assert.Equal("dashboard-only", request.CheckSetInputs["constraint.scope"]);
        Assert.Equal("true", request.CheckSetInputs["dashboard_request_builder"]);
        Assert.Equal("false", request.CheckSetInputs["runtime_extension_claimed"]);
        Assert.Equal("false", request.CheckSetInputs["semantic_quality_claimed"]);
        Assert.Equal("api-dashboard", request.Provenance["source"]);
        Assert.Equal("ApiDashboardRequestBuilder", request.Provenance["dashboard_component"]);
        Assert.Equal("corr-test-001", request.Provenance["correlation_id"]);
    }

    [Fact]
    public void ApiDashboardRequestBuilder_BuildRequest_RejectsEmptyInputBeforeCoreCall()
    {
        var result = ApiDashboardRequestBuilder.BuildRequest(
            XiigenExtensionApiContract.PromptToTaskMethod,
            "   ",
            debugEnabled: false,
            correlationId: "corr-empty");

        Assert.False(result.IsSuccess);
        Assert.Equal("API_DASHBOARD_TEXT_REQUIRED", result.ErrorCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ApiDashboardRequestBuilder_BuildRequest_RejectsUnsupportedMethodBeforeCoreCall()
    {
        var result = ApiDashboardRequestBuilder.BuildRequest(
            "not-a-real-method",
            "Build a request",
            debugEnabled: false,
            correlationId: "corr-unknown");

        Assert.False(result.IsSuccess);
        Assert.Equal("API_DASHBOARD_UNSUPPORTED_METHOD", result.ErrorCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ApiDashboardRequestBuilder_BuildRequest_DefaultsDoNotClaimExternalAiOrRuntimeProof()
    {
        var result = ApiDashboardRequestBuilder.BuildRequest(
            XiigenExtensionApiContract.SkillSelectionMethod,
            "Select skills for this dashboard slice",
            debugEnabled: false,
            correlationId: "corr-safe-defaults");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var request = result.Value!;
        Assert.Equal("false", request.HostFacts["external_ai"]);
        Assert.Equal("false", request.HostFacts["runtime_extension_claimed"]);
        Assert.Equal("false", request.HostFacts["semantic_quality_claimed"]);
        Assert.Equal("false", request.CheckSetInputs["runtime_extension_claimed"]);
        Assert.Equal("false", request.CheckSetInputs["semantic_quality_claimed"]);
    }

    [Fact]
    public void ApiDashboardRequestBuilder_BuildRequest_FromFixtureExampleAddsSafeFixtureBoundaryFields()
    {
        var catalog = ApiCatalogProvider.BuildCatalog().Value!;
        var row = Assert.Single(catalog, item => item.MethodId == XiigenExtensionApiContract.SkillSelectionMethod);
        var fixture = Assert.Single(row.FixtureExamples, item => item.FixtureId == "P36-SKILLSELECTION-01");

        var result = ApiDashboardRequestBuilder.BuildRequest(
            row,
            fixture,
            debugEnabled: true,
            correlationId: "corr-fixture-001");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var request = result.Value!;
        Assert.Equal(XiigenExtensionApiContract.SkillSelectionMethod, request.Method);
        Assert.Equal(fixture.ExampleInputText, request.UserPromptOrTaskText);
        Assert.Equal("true", request.HostFacts["fixture_preview_only"]);
        Assert.Equal(fixture.FixtureId, request.HostFacts["fixture_id"]);
        Assert.Equal(fixture.ScenarioId, request.HostFacts["scenario_id"]);
        Assert.Equal("false", request.HostFacts["training_allowed"]);
        Assert.Equal("false", request.HostFacts["eval_execution_allowed"]);
        Assert.Equal("true", request.HostFacts["not_executed_yet"]);
        Assert.Equal("true", request.CheckSetInputs["fixture_preview_only"]);
        Assert.Equal(fixture.FixtureId, request.CheckSetInputs["fixture_id"]);
        Assert.Equal(fixture.ScenarioId, request.CheckSetInputs["scenario_id"]);
        Assert.Equal(fixture.HeldoutSourceFamily, request.CheckSetInputs["heldout_source_family"]);
        Assert.Equal("false", request.CheckSetInputs["raw_private_text_copied"]);
        Assert.Equal("false", request.CheckSetInputs["semantic_quality_claimed"]);
        Assert.Equal("false", request.CheckSetInputs["runtime_extension_claimed"]);
        Assert.Equal("true", request.Provenance["fixture_preview_only"]);
        Assert.Equal(fixture.FixtureId, request.Provenance["fixture_id"]);
        Assert.Equal("false", request.Provenance["semantic_quality_claimed"]);
        Assert.Equal("false", request.Provenance["runtime_extension_claimed"]);
    }

    [Fact]
    public void ApiDashboardRequestBuilder_BuildRequest_RejectsFixtureFromDifferentApiMethod()
    {
        var catalog = ApiCatalogProvider.BuildCatalog().Value!;
        var promptToTask = Assert.Single(catalog, item => item.MethodId == XiigenExtensionApiContract.PromptToTaskMethod);
        var skillSelection = Assert.Single(catalog, item => item.MethodId == XiigenExtensionApiContract.SkillSelectionMethod);
        var skillFixture = skillSelection.FixtureExamples[0];

        var result = ApiDashboardRequestBuilder.BuildRequest(
            promptToTask,
            skillFixture,
            debugEnabled: false,
            correlationId: "corr-fixture-mismatch");

        Assert.False(result.IsSuccess);
        Assert.Equal("API_DASHBOARD_FIXTURE_METHOD_MISMATCH", result.ErrorCode);
        Assert.Null(result.Value);
    }
}
