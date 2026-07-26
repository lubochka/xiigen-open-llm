// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using SmartSkillsDomain;
using SmartSkillsDomain.Dashboard;

public sealed class ApiDashboardViewModelTests
{
    [Fact]
    public void ApiDashboardViewModel_SelectsApiUsesExampleRunsAndShowsResult()
    {
        var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
        var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
        var selectedApi = viewModel.ApiRows.Single(row => row.MethodId == XiigenExtensionApiContract.PromptToTaskMethod);

        viewModel.SelectedApi = selectedApi;
        viewModel.UseExampleCommand.Execute(null);
        viewModel.InputText = "Turn this prompt into a dashboard test.";
        viewModel.RunCommand.Execute(null);

        Assert.Equal(selectedApi.ExampleInputText, viewModel.SelectedExampleText);
        Assert.Equal(1, gateway.CallCount);
        Assert.NotNull(gateway.LastRequest);
        Assert.Equal(XiigenExtensionApiContract.PromptToTaskMethod, gateway.LastRequest!.Method);
        Assert.Equal("Turn this prompt into a dashboard test.", gateway.LastRequest.UserPromptOrTaskText);
        Assert.NotNull(viewModel.Result);
        Assert.True(viewModel.Result!.Success);
        Assert.Equal("Mapped dashboard response.", viewModel.HumanResultText);
        Assert.Contains("test-integrity", viewModel.Result.SelectedSkills);
        Assert.Single(viewModel.History);
        Assert.False(viewModel.History[0].HostRuntimeProofClaimed);
        Assert.False(viewModel.History[0].SemanticQualityOrTrainedModelProofClaimed);
    }

    [Fact]
    public void ApiDashboardViewModel_EmptyInputShowsVisibleErrorWithoutCoreCall()
    {
        var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
        var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));

        viewModel.SelectedApi = viewModel.ApiRows.Single(row => row.MethodId == XiigenExtensionApiContract.PromptToTaskMethod);
        viewModel.InputText = "   ";
        viewModel.RunCommand.Execute(null);

        Assert.Equal(0, gateway.CallCount);
        Assert.NotNull(viewModel.Result);
        Assert.False(viewModel.Result!.Success);
        Assert.True(viewModel.HasVisibleError);
        Assert.Contains("API_DASHBOARD_TEXT_REQUIRED", viewModel.VisibleErrorText);
        Assert.Contains("Dashboard API request text is required", viewModel.VisibleErrorText);
    }

    [Fact]
    public void ApiDashboardViewModel_RawJsonToggleUsesSameRunCommandPath()
    {
        var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
        var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));

        viewModel.SelectedApi = viewModel.ApiRows.Single(row => row.MethodId == XiigenExtensionApiContract.SkillSelectionMethod);
        viewModel.InputText = "Select the skills for this dashboard view-model test.";
        viewModel.ToggleRawJsonCommand.Execute(null);
        viewModel.RunCommand.Execute(null);

        Assert.True(viewModel.IsRawJsonRequested);
        Assert.True(viewModel.IsRawJsonVisible);
        Assert.Contains("Mapped dashboard response.", viewModel.RawJsonText);

        viewModel.ToggleRawJsonCommand.Execute(null);

        Assert.False(viewModel.IsRawJsonRequested);
        Assert.False(viewModel.IsRawJsonVisible);
        Assert.Equal(string.Empty, viewModel.RawJsonText);
    }

    [Fact]
    public void ApiDashboardViewModel_DebugToggleSendsDebugRequestAndShowsSafeTrace()
    {
        var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
        var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));

        viewModel.SelectedApi = viewModel.ApiRows.Single(row => row.MethodId == XiigenExtensionApiContract.DebugTraceMethod);
        viewModel.InputText = "Show a safe redacted trace.";
        viewModel.ToggleDebugCommand.Execute(null);
        viewModel.RunCommand.Execute(null);

        Assert.True(viewModel.IsDebugRequested);
        Assert.NotNull(gateway.LastRequest);
        Assert.True(gateway.LastRequest!.DebugEnabled);
        Assert.True(viewModel.IsDebugTraceVisible);
        Assert.Contains("input: safe input", viewModel.DebugTraceText);
        Assert.Contains("output: safe output", viewModel.DebugTraceText);
    }

    [Fact]
    public void ApiDashboardViewModel_CopyAndExportAreShellCommandsOverSafeViewState()
    {
        var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
        var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));

        viewModel.SelectedApi = viewModel.ApiRows.Single(row => row.MethodId == XiigenExtensionApiContract.OrchestrationPlanMethod);
        viewModel.InputText = "Build orchestration plan evidence.";
        viewModel.RunCommand.Execute(null);
        viewModel.CopyResultCommand.Execute(null);
        viewModel.ExportHistoryCommand.Execute(null);

        Assert.Equal("Mapped dashboard response.", viewModel.LastCopiedText);
        Assert.Contains(XiigenExtensionApiContract.OrchestrationPlanMethod, viewModel.LastExportText);
        Assert.DoesNotContain("Build orchestration plan evidence.", viewModel.LastExportText);
        Assert.Contains(ApiDashboardRunner.DashboardEvidenceScope, viewModel.LastExportText);
    }

    [Fact]
    public void ApiDashboardViewModel_SelectedFixtureExamplePopulatesInputAndRunsFixtureRequest()
    {
        var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
        var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
        var selectedApi = viewModel.ApiRows.Single(row => row.MethodId == XiigenExtensionApiContract.PromptToTaskMethod);
        var fixture = selectedApi.FixtureExamples.Single(row => row.FixtureId == "P36-PROMPTTOTASK-04");

        viewModel.SelectedApi = selectedApi;
        viewModel.SelectedFixture = fixture;
        viewModel.UseExampleCommand.Execute(null);
        viewModel.RunCommand.Execute(null);

        Assert.Equal(12, viewModel.SelectedFixtureExamples.Count);
        Assert.Equal(fixture.HumanName, viewModel.SelectedFixtureName);
        Assert.Equal(fixture.ExpectedVisibleResult, viewModel.SelectedFixtureExpectedResult);
        Assert.Equal(fixture.ExampleInputText, viewModel.InputText);
        Assert.Equal(1, gateway.CallCount);
        Assert.NotNull(gateway.LastRequest);
        Assert.Equal(fixture.ExampleInputText, gateway.LastRequest!.UserPromptOrTaskText);
        Assert.Equal("true", gateway.LastRequest.HostFacts["fixture_preview_only"]);
        Assert.Equal(fixture.FixtureId, gateway.LastRequest.CheckSetInputs["fixture_id"]);
        Assert.Equal(fixture.ScenarioId, gateway.LastRequest.Provenance["scenario_id"]);
        Assert.False(viewModel.History[0].HostRuntimeProofClaimed);
        Assert.False(viewModel.History[0].SemanticQualityOrTrainedModelProofClaimed);
    }

    [Fact]
    public void ApiDashboardViewModel_CustomTextWithFixtureSelectedUsesNormalDashboardRequest()
    {
        var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
        var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
        var selectedApi = viewModel.ApiRows.Single(row => row.MethodId == XiigenExtensionApiContract.SkillSelectionMethod);

        viewModel.SelectedApi = selectedApi;
        viewModel.InputText = "Custom skill-selection text that is not the selected fixture example.";
        viewModel.RunCommand.Execute(null);

        Assert.Equal(1, gateway.CallCount);
        Assert.NotNull(gateway.LastRequest);
        Assert.Equal("false", gateway.LastRequest!.HostFacts["fixture_preview_only"]);
        Assert.Equal("false", gateway.LastRequest.CheckSetInputs["fixture_preview_only"]);
        Assert.Equal("Custom skill-selection text that is not the selected fixture example.", gateway.LastRequest.UserPromptOrTaskText);
    }

    private static OperationResult<XiigenExtensionResponse> BuildSuccessfulResponse(
        XiigenExtensionRequest request)
        => OperationResult<XiigenExtensionResponse>.Ok(
            new XiigenExtensionResponse(
                Success: true,
                Status: XiigenExtensionApiService.PartialBoundaryStatus,
                ChatDisplayText: "Mapped dashboard response.",
                SelectedSkills: ["test-integrity"],
                CheckSetResults: new Dictionary<string, string>
                {
                    ["core_boundary_executed"] = "true",
                    ["runtime_extension_claimed"] = "false",
                    ["semantic_quality_claimed"] = "false",
                    ["trained_model_proof_claimed"] = "false",
                    ["host_runtime_claimed"] = "false",
                    ["script_parity_used"] = "false",
                    ["external_ai"] = "false"
                },
                StructuredTaskRefs: ["structured-task:view-model-test"],
                ScenarioRefs: ["scenario:view-model-test"],
                Warnings: ["semantic_quality_claimed=false", "core_boundary_only_no_host_runtime_claim"],
                Errors: [],
                DebugTrace: request.DebugEnabled
                    ? new XiigenDebugTrace(request.Method, "safe input", "safe output")
                    : null));

    private sealed class RecordingGateway : IXiigenExtensionApiGateway
    {
        private readonly Func<XiigenExtensionRequest, OperationResult<XiigenExtensionResponse>> _handler;

        public RecordingGateway(Func<XiigenExtensionRequest, OperationResult<XiigenExtensionResponse>> handler)
            => _handler = handler;

        public int CallCount { get; private set; }

        public XiigenExtensionRequest? LastRequest { get; private set; }

        public OperationResult<XiigenExtensionResponse> ProcessHostExtensionRequest(
            XiigenExtensionRequest request)
        {
            CallCount++;
            LastRequest = request;
            return _handler(request);
        }
    }
}
