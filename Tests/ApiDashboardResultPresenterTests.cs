// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SmartSkillsDomain;
using SmartSkillsDomain.Dashboard;

public sealed class ApiDashboardResultPresenterTests
{
    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_ShowsWarningsAndErrors()
    {
        var response = BuildResponse(
            success: false,
            warnings: ["semantic_quality_claimed=false"],
            errors: ["validation failed"]);

        var result = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: false);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var viewModel = result.Value!;
        Assert.False(viewModel.Success);
        Assert.Equal("Human result", viewModel.HumanResult);
        Assert.Contains("semantic_quality_claimed=false", viewModel.Warnings);
        Assert.Contains("validation failed", viewModel.Errors);
    }

    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_HidesRawJsonByDefault()
    {
        var response = BuildResponse();

        var viewModel = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: false).Value!;

        Assert.False(viewModel.IsRawJsonVisible);
        Assert.Equal(string.Empty, viewModel.RawJsonText);
    }

    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_ShowsRawJsonOnlyWhenRequested()
    {
        var response = BuildResponse();

        var viewModel = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: false,
            showRawJson: true).Value!;

        Assert.True(viewModel.IsRawJsonVisible);
        Assert.Contains("Human result", viewModel.RawJsonText);
    }

    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_RedactsUnsafeDebugTraceFromRawJson()
    {
        var response = BuildResponse(
            debugTrace: new XiigenDebugTrace(
                XiigenExtensionApiContract.DebugTraceMethod,
                "raw private input should not leak",
                "raw private output should not leak",
                ContainsRawPrivateContent: true));

        var viewModel = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: true,
            showRawJson: true).Value!;

        Assert.True(viewModel.IsRawJsonVisible);
        Assert.Contains("[suppressed unsafe debug trace input]", viewModel.RawJsonText);
        Assert.Contains("[suppressed unsafe debug trace output]", viewModel.RawJsonText);
        Assert.DoesNotContain("raw private input should not leak", viewModel.RawJsonText);
        Assert.DoesNotContain("raw private output should not leak", viewModel.RawJsonText);
        Assert.False(viewModel.IsDebugTraceVisible);
        Assert.Equal("Debug trace was suppressed because it was marked unsafe.", viewModel.DebugTraceSuppressionReason);
    }

    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_RedactsSecretDebugTraceFromRawJson()
    {
        var response = BuildResponse(
            debugTrace: new XiigenDebugTrace(
                XiigenExtensionApiContract.DebugTraceMethod,
                "secret input should not leak",
                "secret output should not leak",
                ContainsRawPrivateContent: false,
                ContainsSecrets: true));

        var viewModel = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: true,
            showRawJson: true).Value!;

        Assert.True(viewModel.IsRawJsonVisible);
        Assert.Contains("[suppressed unsafe debug trace input]", viewModel.RawJsonText);
        Assert.Contains("[suppressed unsafe debug trace output]", viewModel.RawJsonText);
        Assert.DoesNotContain("secret input should not leak", viewModel.RawJsonText);
        Assert.DoesNotContain("secret output should not leak", viewModel.RawJsonText);
        Assert.False(viewModel.IsDebugTraceVisible);
        Assert.Equal("Debug trace was suppressed because it was marked unsafe.", viewModel.DebugTraceSuppressionReason);
    }

    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_SuppressesDebugTraceWhenNotRequested()
    {
        var response = BuildResponse(
            debugTrace: new XiigenDebugTrace(
                XiigenExtensionApiContract.PromptToTaskMethod,
                "input",
                "output"));

        var viewModel = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: false).Value!;

        Assert.False(viewModel.IsDebugTraceVisible);
        Assert.Equal(string.Empty, viewModel.DebugTraceText);
        Assert.Equal("Debug trace was not requested.", viewModel.DebugTraceSuppressionReason);
    }

    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_ShowsDebugTraceWhenRequestedAndSafe()
    {
        var response = BuildResponse(
            debugTrace: new XiigenDebugTrace(
                XiigenExtensionApiContract.PromptToTaskMethod,
                "safe input",
                "safe output"));

        var viewModel = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: true).Value!;

        Assert.True(viewModel.IsDebugTraceVisible);
        Assert.Contains("xiigen prompt-to-task :", viewModel.DebugTraceText);
        Assert.Contains("input: safe input", viewModel.DebugTraceText);
        Assert.Contains("output: safe output", viewModel.DebugTraceText);
        Assert.Equal(string.Empty, viewModel.DebugTraceSuppressionReason);
    }

    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_SuppressesUnsafeDebugTrace()
    {
        var response = BuildResponse(
            debugTrace: new XiigenDebugTrace(
                XiigenExtensionApiContract.DebugTraceMethod,
                "raw",
                "output",
                ContainsRawPrivateContent: true));

        var viewModel = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: true).Value!;

        Assert.False(viewModel.IsDebugTraceVisible);
        Assert.Equal("Debug trace was suppressed because it was marked unsafe.", viewModel.DebugTraceSuppressionReason);
    }

    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_MakesBoundaryFlagsVisible()
    {
        var response = BuildResponse();

        var viewModel = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: false).Value!;

        Assert.True(viewModel.AreBoundaryFlagsVisible);
        Assert.False(viewModel.RuntimeExtensionProofClaimed);
        Assert.False(viewModel.SemanticQualityOrTrainedModelProofClaimed);
        Assert.Contains(viewModel.BoundaryFlags, flag => flag.Key == "runtime_extension_claimed" && !flag.Claimed);
        Assert.Contains(viewModel.BoundaryFlags, flag => flag.Key == "semantic_quality_claimed" && !flag.Claimed);
        Assert.Contains(viewModel.CheckSetResults, row => row.Key == "runtime_extension_claimed" && row.Value == "false");
        Assert.Contains(viewModel.CheckSetResults, row => row.Key == "semantic_quality_claimed" && row.Value == "false");
    }

    [Fact]
    public void ApiDashboardResultPresenter_BuildViewModel_DoesNotClaimExternalAiOrRuntimeExtensionProof()
    {
        var response = BuildResponse(
            checkSetResults: new Dictionary<string, string>
            {
                ["runtime_extension_claimed"] = "false",
                ["semantic_quality_claimed"] = "false",
                ["external_ai"] = "false"
            });

        var viewModel = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested: false).Value!;

        Assert.False(viewModel.RuntimeExtensionProofClaimed);
        Assert.False(viewModel.SemanticQualityOrTrainedModelProofClaimed);
        Assert.Contains(viewModel.CheckSetResults, row => row.Key == "external_ai" && row.Value == "false");
    }

    private static XiigenExtensionResponse BuildResponse(
        bool success = true,
        IReadOnlyList<string>? warnings = null,
        IReadOnlyList<string>? errors = null,
        IReadOnlyDictionary<string, string>? checkSetResults = null,
        XiigenDebugTrace? debugTrace = null)
        => new(
            Success: success,
            Status: success ? XiigenExtensionApiService.PartialBoundaryStatus : XiigenExtensionApiService.InvalidRequestStatus,
            ChatDisplayText: "Human result",
            SelectedSkills: ["api-shape-verification"],
            CheckSetResults: checkSetResults ?? new Dictionary<string, string>
            {
                ["runtime_extension_claimed"] = "false",
                ["semantic_quality_claimed"] = "false"
            },
            StructuredTaskRefs: ["structured-task:test"],
            ScenarioRefs: ["scenario:test"],
            Warnings: warnings ?? [],
            Errors: errors ?? [],
            DebugTrace: debugTrace);
}
