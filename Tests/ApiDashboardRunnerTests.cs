// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using SmartSkillsDomain;
using SmartSkillsDomain.Dashboard;

public sealed class ApiDashboardRunnerTests
{
    [Fact]
    public void ApiDashboardRunner_Run_SuccessfulServiceResponseMapsToViewModel()
    {
        var gateway = new RecordingGateway(BuildSuccessfulResponse());
        var runner = new ApiDashboardRunner(gateway);

        var result = runner.Run(
            XiigenExtensionApiContract.PromptToTaskMethod,
            "Build an in-process dashboard runner",
            debugEnabled: false,
            correlationId: "corr-run-success",
            availableSkills: ["test-integrity"]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var run = result.Value!;
        Assert.Equal(1, gateway.CallCount);
        Assert.NotNull(gateway.LastRequest);
        Assert.Equal(XiigenExtensionApiContract.PromptToTaskMethod, gateway.LastRequest!.Method);
        Assert.True(run.CoreBoundaryCalled);
        Assert.True(run.ViewModel.Success);
        Assert.Equal("Mapped dashboard response.", run.ViewModel.HumanResult);
        Assert.Contains("test-integrity", run.ViewModel.SelectedSkills);
        Assert.Contains(run.ViewModel.CheckSetResults, row => row.Key == "core_boundary_executed" && row.Value == "true");
        Assert.False(run.RuntimeExtensionProofClaimed);
        Assert.False(run.SemanticQualityOrTrainedModelProofClaimed);
        Assert.False(run.HostRuntimeProofClaimed);
        Assert.False(run.ExternalAiUsed);
        Assert.False(run.ScriptParityUsed);
        Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, run.EvidenceScope);
    }

    [Fact]
    public void ApiDashboardRunner_Run_InvalidBuilderInputReturnsVisibleErrorWithoutCoreCall()
    {
        var gateway = new RecordingGateway(BuildSuccessfulResponse());
        var runner = new ApiDashboardRunner(gateway);

        var result = runner.Run(
            XiigenExtensionApiContract.PromptToTaskMethod,
            "   ",
            debugEnabled: false,
            correlationId: "corr-run-invalid");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var run = result.Value!;
        Assert.Equal(0, gateway.CallCount);
        Assert.False(run.CoreBoundaryCalled);
        Assert.False(run.ViewModel.Success);
        Assert.Equal(ApiDashboardRunner.DashboardValidationFailedStatus, run.ViewModel.Status);
        Assert.Contains("API_DASHBOARD_TEXT_REQUIRED", run.ViewModel.Errors);
        Assert.Contains(run.ViewModel.CheckSetResults, row => row.Key == "core_boundary_called" && row.Value == "false");
        Assert.False(run.RuntimeExtensionProofClaimed);
        Assert.False(run.SemanticQualityOrTrainedModelProofClaimed);
        Assert.False(run.HostRuntimeProofClaimed);
        Assert.False(run.ExternalAiUsed);
        Assert.False(run.ScriptParityUsed);
    }

    [Fact]
    public void ApiDashboardRunner_Run_CoreOperationResultFailureMapsToVisibleError()
    {
        var gateway = new RecordingGateway(
            OperationResult<XiigenExtensionResponse>.Fail(
                "Injected core gateway failed.",
                "INJECTED_CORE_FAILURE"));
        var runner = new ApiDashboardRunner(gateway);

        var result = runner.Run(
            XiigenExtensionApiContract.SkillSelectionMethod,
            "Select dashboard skills",
            debugEnabled: false,
            correlationId: "corr-run-core-fail",
            availableSkills: ["api-shape-verification"]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var run = result.Value!;
        Assert.Equal(1, gateway.CallCount);
        Assert.True(run.CoreBoundaryCalled);
        Assert.False(run.ViewModel.Success);
        Assert.Equal(ApiDashboardRunner.DashboardCoreOperationFailedStatus, run.ViewModel.Status);
        Assert.Contains("INJECTED_CORE_FAILURE", run.ViewModel.Errors);
        Assert.Contains("Injected core gateway failed.", run.ViewModel.Errors);
        Assert.Contains(run.ViewModel.CheckSetResults, row => row.Key == "core_boundary_called" && row.Value == "true");
        Assert.Contains(run.ViewModel.CheckSetResults, row => row.Key == "core_boundary_executed" && row.Value == "false");
        Assert.False(run.RuntimeExtensionProofClaimed);
        Assert.False(run.SemanticQualityOrTrainedModelProofClaimed);
        Assert.False(run.HostRuntimeProofClaimed);
        Assert.False(run.ExternalAiUsed);
        Assert.False(run.ScriptParityUsed);
    }

    [Fact]
    public void ApiDashboardRunner_Run_RealInProcessServiceKeepsProofFlagsFalse()
    {
        var service = new XiigenExtensionApiService();
        var runner = new ApiDashboardRunner(new XiigenExtensionApiServiceGateway(service));

        var result = runner.Run(
            XiigenExtensionApiContract.PromptToTaskMethod,
            "Create a bounded API dashboard runner integration test.",
            debugEnabled: false,
            correlationId: "corr-run-real-service",
            availableSkills: ["api-shape-verification"]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var run = result.Value!;
        Assert.True(run.CoreBoundaryCalled);
        Assert.True(run.ViewModel.Success);
        Assert.Contains(run.ViewModel.CheckSetResults, row => row.Key == "core_boundary_executed" && row.Value == "true");
        Assert.Contains(run.ViewModel.CheckSetResults, row => row.Key == "runtime_extension_claimed" && row.Value == "false");
        Assert.Contains(run.ViewModel.CheckSetResults, row => row.Key == "semantic_quality_claimed" && row.Value == "false");
        Assert.False(run.RuntimeExtensionProofClaimed);
        Assert.False(run.SemanticQualityOrTrainedModelProofClaimed);
        Assert.False(run.HostRuntimeProofClaimed);
        Assert.False(run.ExternalAiUsed);
        Assert.False(run.ScriptParityUsed);
        Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, run.EvidenceScope);
    }

    [Fact]
    public void ApiDashboardRunner_Run_DoesNotUseScriptParityOrHostRuntimeProofByDefault()
    {
        var gateway = new RecordingGateway(BuildSuccessfulResponse());
        var runner = new ApiDashboardRunner(gateway);

        var result = runner.Run(
            XiigenExtensionApiContract.OrchestrationPlanMethod,
            "Plan the dashboard-to-core path",
            debugEnabled: true,
            correlationId: "corr-run-no-script",
            availableSkills: ["implementation-integrity"]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var run = result.Value!;
        Assert.Equal(1, gateway.CallCount);
        Assert.False(run.ScriptParityUsed);
        Assert.False(run.HostRuntimeProofClaimed);
        Assert.False(run.RuntimeExtensionProofClaimed);
        Assert.False(run.SemanticQualityOrTrainedModelProofClaimed);
        Assert.False(run.ExternalAiUsed);
        Assert.DoesNotContain(run.ViewModel.CheckSetResults, row => row.Key == "host_runtime_claimed" && row.Value == "true");
        Assert.DoesNotContain(run.ViewModel.CheckSetResults, row => row.Key == "script_parity_used" && row.Value == "true");
    }

    [Fact]
    public void ApiDashboardRunner_Run_FixtureExampleCallsGatewayWithFixturePreviewBoundary()
    {
        var gateway = new RecordingGateway(BuildSuccessfulResponse());
        var runner = new ApiDashboardRunner(gateway);
        var catalog = ApiCatalogProvider.BuildCatalog().Value!;
        var row = Assert.Single(catalog, item => item.MethodId == XiigenExtensionApiContract.OrchestrationPlanMethod);
        var fixture = Assert.Single(row.FixtureExamples, item => item.FixtureId == "P36-ORCHESTRATIONREASONING-01");

        var result = runner.Run(
            row,
            fixture,
            debugEnabled: true,
            showRawJson: true,
            correlationId: "corr-run-fixture");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var run = result.Value!;
        Assert.Equal(1, gateway.CallCount);
        Assert.NotNull(gateway.LastRequest);
        Assert.Equal(XiigenExtensionApiContract.OrchestrationPlanMethod, gateway.LastRequest!.Method);
        Assert.Equal(fixture.ExampleInputText, gateway.LastRequest.UserPromptOrTaskText);
        Assert.Equal("true", gateway.LastRequest.HostFacts["fixture_preview_only"]);
        Assert.Equal(fixture.FixtureId, gateway.LastRequest.CheckSetInputs["fixture_id"]);
        Assert.Equal(fixture.ScenarioId, gateway.LastRequest.Provenance["scenario_id"]);
        Assert.True(run.CoreBoundaryCalled);
        Assert.True(run.ViewModel.IsRawJsonVisible);
        Assert.False(run.RuntimeExtensionProofClaimed);
        Assert.False(run.SemanticQualityOrTrainedModelProofClaimed);
        Assert.False(run.HostRuntimeProofClaimed);
        Assert.False(run.ExternalAiUsed);
        Assert.False(run.ScriptParityUsed);
    }

    private static OperationResult<XiigenExtensionResponse> BuildSuccessfulResponse()
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
                StructuredTaskRefs: ["structured-task:runner-test"],
                ScenarioRefs: ["scenario:runner-test"],
                Warnings: ["semantic_quality_claimed=false", "core_boundary_only_no_host_runtime_claim"],
                Errors: [],
                DebugTrace: null));

    private sealed class RecordingGateway : IXiigenExtensionApiGateway
    {
        private readonly OperationResult<XiigenExtensionResponse> _result;

        public RecordingGateway(OperationResult<XiigenExtensionResponse> result)
            => _result = result;

        public int CallCount { get; private set; }

        public XiigenExtensionRequest? LastRequest { get; private set; }

        public OperationResult<XiigenExtensionResponse> ProcessHostExtensionRequest(
            XiigenExtensionRequest request)
        {
            CallCount++;
            LastRequest = request;
            return _result;
        }
    }
}
