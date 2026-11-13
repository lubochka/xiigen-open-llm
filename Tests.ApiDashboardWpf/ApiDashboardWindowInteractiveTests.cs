// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AgentInfrastructure;
using SmartSkillsDomain;
using SmartSkillsDomain.Dashboard;

public sealed class ApiDashboardWindowInteractiveTests
{
    private static readonly TimeSpan StaTestTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public void ApiDashboardWindow_UxAutomationExposesHumanReadableSurfaceAndStableLayout()
        => RunOnStaThread(() =>
        {
            var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
            var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
            var window = new ApiDashboardWindow(viewModel);

            try
            {
                window.Show();
                Flush(window);

                var apiListPanel = FindRequired<Border>(window, "ApiListPanel");
                var apiDetailsPanel = FindRequired<Border>(window, "ApiDetailsPanel");
                var resultPanel = FindRequired<Border>(window, "ResultPanel");
                var apiList = FindRequired<ListBox>(window, "ApiListBox");
                var fixtureList = FindRequired<ListBox>(window, "FixtureExampleListBox");
                var fixtureExpectedResultBox = FindRequired<TextBox>(window, "FixtureExpectedResultTextBox");
                var exampleBox = FindRequired<TextBox>(window, "ExampleTextBox");
                var useExampleButton = FindRequired<Button>(window, "UseExampleButton");
                var inputBox = FindRequired<TextBox>(window, "InputTextBox");
                var runButton = FindRequired<Button>(window, "RunButton");
                var rawJsonToggle = FindRequired<ToggleButton>(window, "RawJsonToggle");
                var debugToggle = FindRequired<ToggleButton>(window, "DebugToggle");
                var copyButton = FindRequired<Button>(window, "CopyResultButton");
                var exportButton = FindRequired<Button>(window, "ExportHistoryButton");
                var historyList = FindRequired<ListBox>(window, "HistoryListBox");
                var resultBox = FindRequired<TextBox>(window, "HumanResultTextBox");
                var warningsItems = FindRequired<ItemsControl>(window, "WarningsItemsControl");
                var boundaryItems = FindRequired<ItemsControl>(window, "BoundaryItemsControl");
                var rawJsonBox = FindRequired<TextBox>(window, "RawJsonTextBox");
                var debugTraceBox = FindRequired<TextBox>(window, "DebugTraceTextBox");
                var errorText = FindRequired<TextBlock>(window, "ErrorTextBlock");
                var selectedName = FindRequired<TextBlock>(window, "SelectedApiNameTextBlock");
                var selectedDescription = FindRequired<TextBlock>(window, "SelectedApiDescriptionTextBlock");
                var inputDescription = FindRequired<TextBlock>(window, "SelectedApiInputDescriptionTextBlock");
                var outputPreview = FindRequired<TextBlock>(window, "SelectedApiOutputPreviewTextBlock");
                var safetyBoundary = FindRequired<TextBlock>(window, "SelectedSafetyBoundaryTextBlock");

                Assert.True(window.MinWidth >= 1120);
                Assert.True(window.MinHeight >= 680);
                AssertNonOverlappingColumns(window, apiListPanel, apiDetailsPanel, resultPanel);
                Assert.True(apiListPanel.ActualWidth >= 280);
                Assert.True(apiDetailsPanel.ActualWidth >= 360);
                Assert.True(resultPanel.ActualWidth >= 360);
                Assert.Equal(TextWrapping.Wrap, exampleBox.TextWrapping);
                Assert.Equal(TextWrapping.Wrap, inputBox.TextWrapping);
                Assert.Equal(TextWrapping.Wrap, resultBox.TextWrapping);

                var skillApi = apiList.Items
                    .Cast<ApiDashboardCatalogRow>()
                    .Single(row => row.MethodId == XiigenExtensionApiContract.SkillSelectionMethod);
                SelectListBoxItem(apiList, skillApi);

                Assert.Equal(skillApi.HumanName, selectedName.Text);
                Assert.DoesNotContain(skillApi.MethodId, selectedName.Text, StringComparison.Ordinal);
                Assert.Contains("Use this", selectedDescription.Text, StringComparison.Ordinal);
                Assert.Contains("dashboard sends", inputDescription.Text, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Selected skills", outputPreview.Text, StringComparison.Ordinal);
                Assert.Contains("must not expose", safetyBoundary.Text, StringComparison.OrdinalIgnoreCase);
                Assert.True(fixtureList.Items.Count >= 12);
                Assert.Contains("Fixture preview", fixtureExpectedResultBox.Text, StringComparison.Ordinal);
                Assert.Contains("not executed yet", fixtureExpectedResultBox.Text, StringComparison.OrdinalIgnoreCase);

                AssertUx(apiList, "Available API methods", "Choose the Xiigen API method");
                AssertUx(fixtureList, "Semantic-quality fixture examples", "does not run model eval");
                AssertUx(fixtureExpectedResultBox, "Expected fixture result", "without claiming model execution");
                AssertUx(exampleBox, "Example input for the selected API", "realistic input");
                AssertUx(useExampleButton, "Use example", "Copies the example input");
                AssertUx(inputBox, "Request text input", "Type the prompt");
                AssertUx(runButton, "Run selected API", "dashboard boundary");
                AssertUx(rawJsonToggle, "Show raw JSON", "not host runtime proof");
                AssertUx(debugToggle, "Request debug trace", "redacted Xiigen input and output trace");
                AssertUx(copyButton, "Prepare result copy text", "does not claim clipboard automation");
                AssertUx(exportButton, "Prepare safe history text", "does not claim file export automation");
                AssertUx(historyList, "Safe run history", "prompt hashes");
                AssertUx(resultBox, "Human-readable result", "chat-safe Xiigen response");
                AssertUx(warningsItems, "Core warnings", "proof-boundary warnings");
                AssertUx(boundaryItems, "Evidence boundary", "what this dashboard run proves");
                AssertUx(rawJsonBox, "Raw response JSON", "unsafe debug traces are suppressed");
                AssertUx(debugTraceBox, "Redacted debug trace", "safe redacted Xiigen input/output trace");
                AssertUx(errorText, "Visible error message", "validation or core errors");

                AssertTooltip(useExampleButton);
                AssertTooltip(fixtureList);
                AssertTooltip(fixtureExpectedResultBox);
                AssertTooltip(inputBox);
                AssertTooltip(runButton);
                AssertTooltip(rawJsonToggle);
                AssertTooltip(debugToggle);
                AssertTooltip(copyButton);
                AssertTooltip(exportButton);

                AssertNoPrimaryPathJargon(
                    selectedName.Text,
                    selectedDescription.Text,
                    inputDescription.Text,
                    outputPreview.Text,
                    useExampleButton.Content?.ToString() ?? string.Empty,
                    runButton.Content?.ToString() ?? string.Empty,
                    rawJsonToggle.Content?.ToString() ?? string.Empty,
                    debugToggle.Content?.ToString() ?? string.Empty,
                    copyButton.Content?.ToString() ?? string.Empty,
                    exportButton.Content?.ToString() ?? string.Empty);

                SetText(inputBox, "   ");
                InvokeButton(runButton);

                Assert.Equal(0, gateway.CallCount);
                Assert.Contains("Dashboard API request text is required", errorText.Text, StringComparison.Ordinal);
                Assert.Contains("Select relevant skills for a task", selectedName.Text, StringComparison.Ordinal);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void ApiDashboardWindow_UxAutomationTraversesKeyboardFocusStops()
        => RunOnStaThread(() =>
        {
            var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
            var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
            var window = new ApiDashboardWindow(viewModel);

            try
            {
                window.Show();
                window.Activate();
                Flush(window);

                var apiList = FindRequired<ListBox>(window, "ApiListBox");
                var useExampleButton = FindRequired<Button>(window, "UseExampleButton");
                var inputBox = FindRequired<TextBox>(window, "InputTextBox");
                var runButton = FindRequired<Button>(window, "RunButton");
                var rawJsonToggle = FindRequired<ToggleButton>(window, "RawJsonToggle");
                var debugToggle = FindRequired<ToggleButton>(window, "DebugToggle");
                var copyButton = FindRequired<Button>(window, "CopyResultButton");
                var exportButton = FindRequired<Button>(window, "ExportHistoryButton");
                var historyList = FindRequired<ListBox>(window, "HistoryListBox");

                var skillApi = apiList.Items
                    .Cast<ApiDashboardCatalogRow>()
                    .Single(row => row.MethodId == XiigenExtensionApiContract.SkillSelectionMethod);
                SelectListBoxItem(apiList, skillApi);
                SetText(inputBox, "Validate keyboard focus traversal after a real dashboard run.");
                InvokeButton(runButton);

                Assert.Equal(1, gateway.CallCount);
                AssertKeyboardFocusOrder(
                    window,
                    useExampleButton,
                    inputBox,
                    runButton,
                    rawJsonToggle,
                    debugToggle,
                    copyButton,
                    exportButton,
                    historyList);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void ApiDashboardWindow_AutomationDrivesRealControlsAndShowsDashboardScopedEvidence()
        => RunOnStaThread(() =>
        {
            var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
            var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
            var window = new ApiDashboardWindow(viewModel);

            try
            {
                window.Show();
                Flush(window);

                var apiList = FindRequired<ListBox>(window, "ApiListBox");
                var exampleBox = FindRequired<TextBox>(window, "ExampleTextBox");
                var useExampleButton = FindRequired<Button>(window, "UseExampleButton");
                var inputBox = FindRequired<TextBox>(window, "InputTextBox");
                var debugToggle = FindRequired<ToggleButton>(window, "DebugToggle");
                var rawJsonToggle = FindRequired<ToggleButton>(window, "RawJsonToggle");
                var runButton = FindRequired<Button>(window, "RunButton");
                var copyButton = FindRequired<Button>(window, "CopyResultButton");
                var exportButton = FindRequired<Button>(window, "ExportHistoryButton");
                var resultBox = FindRequired<TextBox>(window, "HumanResultTextBox");
                var rawJsonBox = FindRequired<TextBox>(window, "RawJsonTextBox");
                var debugTraceBox = FindRequired<TextBox>(window, "DebugTraceTextBox");
                var boundaryItems = FindRequired<ItemsControl>(window, "BoundaryItemsControl");
                var warningsItems = FindRequired<ItemsControl>(window, "WarningsItemsControl");
                var historyList = FindRequired<ListBox>(window, "HistoryListBox");
                var errorPanel = FindRequired<StackPanel>(window, "ErrorPanel");
                var errorText = FindRequired<TextBlock>(window, "ErrorTextBlock");
                var selectedName = FindRequired<TextBlock>(window, "SelectedApiNameTextBlock");

                var debugApi = apiList.Items
                    .Cast<ApiDashboardCatalogRow>()
                    .Single(row => row.MethodId == XiigenExtensionApiContract.DebugTraceMethod);
                SelectListBoxItem(apiList, debugApi);

                Assert.Equal(debugApi.HumanName, selectedName.Text);
                Assert.Equal(debugApi.ExampleInputText, exampleBox.Text);

                InvokeButton(useExampleButton);
                Assert.Equal(debugApi.ExampleInputText, inputBox.Text);

                const string typedInput = "Show a safe redacted trace from real WPF control automation.";
                SetText(inputBox, typedInput);
                Toggle(debugToggle);
                Toggle(rawJsonToggle);
                InvokeButton(runButton);

                Assert.Equal(1, gateway.CallCount);
                Assert.NotNull(gateway.LastRequest);
                Assert.Equal(XiigenExtensionApiContract.DebugTraceMethod, gateway.LastRequest!.Method);
                Assert.Equal(typedInput, gateway.LastRequest.UserPromptOrTaskText);
                Assert.True(gateway.LastRequest.DebugEnabled);
                Assert.Contains("Mapped WPF dashboard response.", resultBox.Text);
                Assert.Contains("Mapped WPF dashboard response.", rawJsonBox.Text);
                Assert.Contains("input: safe wpf input", debugTraceBox.Text);
                Assert.Contains("output: safe wpf output", debugTraceBox.Text);
                Assert.Contains(
                    boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
                    flag => flag.Key == "runtime_extension_claimed"
                        && !flag.Claimed
                        && flag.DisplayText.Contains("not Claude, Cursor, or Codex host runtime proof", StringComparison.Ordinal));
                Assert.Contains(
                    warningsItems.Items.Cast<string>(),
                    warning => warning.Contains("core_boundary_only_no_host_runtime_claim", StringComparison.Ordinal));
                Assert.Single(historyList.Items);
                Assert.Equal(Visibility.Collapsed, errorPanel.Visibility);

                InvokeButton(copyButton);
                Assert.Equal("Mapped WPF dashboard response.", viewModel.LastCopiedText);
                InvokeButton(exportButton);
                Assert.Contains(XiigenExtensionApiContract.DebugTraceMethod, viewModel.LastExportText);
                Assert.Contains(XiigenExtensionApiService.PartialBoundaryStatus, viewModel.LastExportText);
                Assert.Contains(ApiDashboardRunner.DashboardEvidenceScope, viewModel.LastExportText);
                Assert.DoesNotContain(typedInput, viewModel.LastExportText);

                SetText(inputBox, "   ");
                InvokeButton(runButton);

                Assert.Equal(1, gateway.CallCount);
                Assert.Equal(Visibility.Visible, errorPanel.Visibility);
                Assert.Contains("API_DASHBOARD_TEXT_REQUIRED", errorText.Text);
                Assert.Contains("Dashboard API request text is required", errorText.Text);
                Assert.Equal(2, historyList.Items.Count);
                var failedHistory = Assert.IsType<ApiDashboardHistoryEntry>(historyList.Items[0]);
                Assert.False(failedHistory.Success);
                Assert.Equal(ApiDashboardRunner.DashboardValidationFailedStatus, failedHistory.Status);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void ApiDashboardWindow_FixtureCatalogSelectionRunsDashboardPreviewRequestWithoutModelClaims()
        => RunOnStaThread(() =>
        {
            var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
            var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
            var window = new ApiDashboardWindow(viewModel);

            try
            {
                window.Show();
                Flush(window);

                var apiList = FindRequired<ListBox>(window, "ApiListBox");
                var fixtureList = FindRequired<ListBox>(window, "FixtureExampleListBox");
                var fixtureExpectedResultBox = FindRequired<TextBox>(window, "FixtureExpectedResultTextBox");
                var exampleBox = FindRequired<TextBox>(window, "ExampleTextBox");
                var useExampleButton = FindRequired<Button>(window, "UseExampleButton");
                var inputBox = FindRequired<TextBox>(window, "InputTextBox");
                var runButton = FindRequired<Button>(window, "RunButton");
                var boundaryItems = FindRequired<ItemsControl>(window, "BoundaryItemsControl");
                var historyList = FindRequired<ListBox>(window, "HistoryListBox");

                var skillApi = apiList.Items
                    .Cast<ApiDashboardCatalogRow>()
                    .Single(row => row.MethodId == XiigenExtensionApiContract.SkillSelectionMethod);
                SelectListBoxItem(apiList, skillApi);

                var fixture = fixtureList.Items
                    .Cast<ApiDashboardFixtureExampleRow>()
                    .Single(row => row.FixtureId == "P36-SKILLSELECTION-01");
                SelectListBoxItem(fixtureList, fixture);

                Assert.Contains("SkillSelection", fixture.HumanName, StringComparison.Ordinal);
                Assert.Contains("Fixture preview", fixtureExpectedResultBox.Text, StringComparison.Ordinal);
                Assert.Equal(fixture.ExampleInputText, exampleBox.Text);

                InvokeButton(useExampleButton);
                Assert.Equal(fixture.ExampleInputText, inputBox.Text);

                InvokeButton(runButton);

                Assert.Equal(1, gateway.CallCount);
                Assert.NotNull(gateway.LastRequest);
                Assert.Equal(XiigenExtensionApiContract.SkillSelectionMethod, gateway.LastRequest!.Method);
                Assert.Equal(fixture.ExampleInputText, gateway.LastRequest.UserPromptOrTaskText);
                Assert.Equal("true", gateway.LastRequest.HostFacts["fixture_preview_only"]);
                Assert.Equal(fixture.FixtureId, gateway.LastRequest.CheckSetInputs["fixture_id"]);
                Assert.Equal(fixture.ScenarioId, gateway.LastRequest.Provenance["scenario_id"]);
                Assert.Contains(
                    boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
                    flag => flag.Key == "semantic_quality_claimed" && !flag.Claimed);
                Assert.Single(historyList.Items);
                var history = Assert.IsType<ApiDashboardHistoryEntry>(historyList.Items[0]);
                Assert.False(history.HostRuntimeProofClaimed);
                Assert.False(history.SemanticQualityOrTrainedModelProofClaimed);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void ApiDashboardWindow_PromptToTaskSemanticQualityScenario_DrivesControlsAndShowsHonestDashboardEvidence()
        => RunOnStaThread(() =>
        {
            const string scenarioId = "dashboard-semantic-quality-prompt-to-task-1";
            const string exampleInput = "Write a plan for improving skill selection; do not implement.";
            const string negativeInput = "Missing PromptToTask checkpoint for plan-only request.";
            var gateway = new RecordingGateway(request => request.UserPromptOrTaskText.Contains("Missing PromptToTask checkpoint", StringComparison.Ordinal)
                ? BuildPromptToTaskNotReadyScenarioResponse(request)
                : BuildPromptToTaskSemanticQualityScenarioResponse(request));
            var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
            var window = new ApiDashboardWindow(viewModel);

            try
            {
                window.Show();
                Flush(window);

                var apiList = FindRequired<ListBox>(window, "ApiListBox");
                var inputBox = FindRequired<TextBox>(window, "InputTextBox");
                var debugToggle = FindRequired<ToggleButton>(window, "DebugToggle");
                var rawJsonToggle = FindRequired<ToggleButton>(window, "RawJsonToggle");
                var runButton = FindRequired<Button>(window, "RunButton");
                var exportButton = FindRequired<Button>(window, "ExportHistoryButton");
                var resultBox = FindRequired<TextBox>(window, "HumanResultTextBox");
                var rawJsonBox = FindRequired<TextBox>(window, "RawJsonTextBox");
                var debugTraceBox = FindRequired<TextBox>(window, "DebugTraceTextBox");
                var boundaryItems = FindRequired<ItemsControl>(window, "BoundaryItemsControl");
                var warningsItems = FindRequired<ItemsControl>(window, "WarningsItemsControl");
                var historyList = FindRequired<ListBox>(window, "HistoryListBox");
                var selectedName = FindRequired<TextBlock>(window, "SelectedApiNameTextBlock");
                var inputDescription = FindRequired<TextBlock>(window, "SelectedApiInputDescriptionTextBlock");

                var promptToTaskApi = apiList.Items
                    .Cast<ApiDashboardCatalogRow>()
                    .Single(row => row.MethodId == XiigenExtensionApiContract.PromptToTaskMethod);
                SelectListBoxItem(apiList, promptToTaskApi);

                Assert.Equal("Turn a user prompt into a structured task", selectedName.Text);
                Assert.DoesNotContain(XiigenExtensionApiContract.PromptToTaskMethod, selectedName.Text, StringComparison.Ordinal);
                Assert.Contains("user prompt text", inputDescription.Text, StringComparison.OrdinalIgnoreCase);

                SetText(inputBox, exampleInput);
                Toggle(debugToggle);
                Toggle(rawJsonToggle);
                InvokeButton(runButton);

                Assert.Equal(1, gateway.CallCount);
                Assert.NotNull(gateway.LastRequest);
                Assert.Equal(XiigenExtensionApiContract.PromptToTaskMethod, gateway.LastRequest!.Method);
                Assert.Equal(exampleInput, gateway.LastRequest.UserPromptOrTaskText);
                Assert.True(gateway.LastRequest.DebugEnabled);
                AssertPromptToTaskScenarioPositiveOutput(
                    scenarioId,
                    exampleInput,
                    resultBox.Text,
                    rawJsonBox.Text,
                    debugTraceBox.Text,
                    warningsItems,
                    boundaryItems);
                Assert.Single(historyList.Items);

                SetText(inputBox, negativeInput);
                InvokeButton(runButton);

                Assert.Equal(2, gateway.CallCount);
                Assert.NotNull(gateway.LastRequest);
                Assert.Equal(XiigenExtensionApiContract.PromptToTaskMethod, gateway.LastRequest!.Method);
                Assert.Equal(negativeInput, gateway.LastRequest.UserPromptOrTaskText);
                AssertPromptToTaskScenarioNotReadyOutput(
                    negativeInput,
                    resultBox.Text,
                    rawJsonBox.Text,
                    debugTraceBox.Text,
                    warningsItems,
                    boundaryItems);
                Assert.Equal(2, historyList.Items.Count);
                var negativeHistory = Assert.IsType<ApiDashboardHistoryEntry>(historyList.Items[0]);
                Assert.False(negativeHistory.Success);
                Assert.Equal("prompt_to_task_not_ready_degraded_dashboard_scenario", negativeHistory.Status);
                Assert.Equal(ComputeSha256(negativeInput), negativeHistory.PromptSha256);
                Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, negativeHistory.EvidenceScope);
                Assert.False(negativeHistory.HostRuntimeProofClaimed);
                Assert.False(negativeHistory.SemanticQualityOrTrainedModelProofClaimed);
                Assert.DoesNotContain(negativeInput, negativeHistory.ToString(), StringComparison.Ordinal);
                Assert.DoesNotContain(exampleInput, negativeHistory.ToString(), StringComparison.Ordinal);
                var positiveHistory = Assert.IsType<ApiDashboardHistoryEntry>(historyList.Items[1]);
                Assert.True(positiveHistory.Success);
                Assert.Equal(XiigenExtensionApiService.PartialBoundaryStatus, positiveHistory.Status);
                Assert.Equal(ComputeSha256(exampleInput), positiveHistory.PromptSha256);
                Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, positiveHistory.EvidenceScope);
                Assert.False(positiveHistory.HostRuntimeProofClaimed);
                Assert.False(positiveHistory.SemanticQualityOrTrainedModelProofClaimed);
                Assert.DoesNotContain(exampleInput, positiveHistory.ToString(), StringComparison.Ordinal);
                Assert.DoesNotContain(negativeInput, positiveHistory.ToString(), StringComparison.Ordinal);

                InvokeButton(exportButton);
                Assert.Contains(XiigenExtensionApiContract.PromptToTaskMethod, viewModel.LastExportText);
                Assert.Contains(ApiDashboardRunner.DashboardEvidenceScope, viewModel.LastExportText);
                Assert.Contains(ComputeSha256(exampleInput), viewModel.LastExportText);
                Assert.Contains(ComputeSha256(negativeInput), viewModel.LastExportText);
                Assert.DoesNotContain(exampleInput, viewModel.LastExportText);
                Assert.DoesNotContain(negativeInput, viewModel.LastExportText);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void ApiDashboardWindow_OrchestrationReasoningSemanticQualityScenario_DrivesControlsAndShowsHonestDashboardEvidence()
        => RunOnStaThread(() =>
        {
            const string scenarioId = "dashboard-semantic-quality-orchestration-1";
            const string exampleInput =
                "Structured task: plan_review_or_plan_repair; gates: plan review, dashboard QA, documentation sync; implementation_authorized=false.";
            const string negativeInput = "Malformed OrchestrationReasoning task input with no structured task ref.";
            var gateway = new RecordingGateway(request => request.UserPromptOrTaskText.Contains("Malformed OrchestrationReasoning", StringComparison.Ordinal)
                ? BuildOrchestrationReasoningNotReadyScenarioResponse(request)
                : BuildOrchestrationReasoningSemanticQualityScenarioResponse(request));
            var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
            var window = new ApiDashboardWindow(viewModel);

            try
            {
                window.Show();
                Flush(window);

                var apiList = FindRequired<ListBox>(window, "ApiListBox");
                var inputBox = FindRequired<TextBox>(window, "InputTextBox");
                var debugToggle = FindRequired<ToggleButton>(window, "DebugToggle");
                var rawJsonToggle = FindRequired<ToggleButton>(window, "RawJsonToggle");
                var runButton = FindRequired<Button>(window, "RunButton");
                var exportButton = FindRequired<Button>(window, "ExportHistoryButton");
                var resultBox = FindRequired<TextBox>(window, "HumanResultTextBox");
                var rawJsonBox = FindRequired<TextBox>(window, "RawJsonTextBox");
                var debugTraceBox = FindRequired<TextBox>(window, "DebugTraceTextBox");
                var boundaryItems = FindRequired<ItemsControl>(window, "BoundaryItemsControl");
                var warningsItems = FindRequired<ItemsControl>(window, "WarningsItemsControl");
                var historyList = FindRequired<ListBox>(window, "HistoryListBox");
                var selectedName = FindRequired<TextBlock>(window, "SelectedApiNameTextBlock");
                var inputDescription = FindRequired<TextBlock>(window, "SelectedApiInputDescriptionTextBlock");

                var orchestrationApi = apiList.Items
                    .Cast<ApiDashboardCatalogRow>()
                    .Single(row => row.MethodId == XiigenExtensionApiContract.OrchestrationPlanMethod);
                SelectListBoxItem(apiList, orchestrationApi);

                Assert.Equal("Find an orchestration plan for a structured task", selectedName.Text);
                Assert.DoesNotContain(XiigenExtensionApiContract.OrchestrationPlanMethod, selectedName.Text, StringComparison.Ordinal);
                Assert.Contains("structured task references", inputDescription.Text, StringComparison.OrdinalIgnoreCase);

                SetText(inputBox, exampleInput);
                Toggle(debugToggle);
                Toggle(rawJsonToggle);
                InvokeButton(runButton);

                Assert.Equal(1, gateway.CallCount);
                Assert.NotNull(gateway.LastRequest);
                Assert.Equal(XiigenExtensionApiContract.OrchestrationPlanMethod, gateway.LastRequest!.Method);
                Assert.Equal(exampleInput, gateway.LastRequest.UserPromptOrTaskText);
                Assert.True(gateway.LastRequest.DebugEnabled);
                AssertOrchestrationReasoningScenarioPositiveOutput(
                    scenarioId,
                    exampleInput,
                    resultBox.Text,
                    rawJsonBox.Text,
                    debugTraceBox.Text,
                    warningsItems,
                    boundaryItems);
                Assert.Single(historyList.Items);

                SetText(inputBox, negativeInput);
                InvokeButton(runButton);

                Assert.Equal(2, gateway.CallCount);
                Assert.NotNull(gateway.LastRequest);
                Assert.Equal(XiigenExtensionApiContract.OrchestrationPlanMethod, gateway.LastRequest!.Method);
                Assert.Equal(negativeInput, gateway.LastRequest.UserPromptOrTaskText);
                AssertOrchestrationReasoningScenarioNotReadyOutput(
                    negativeInput,
                    resultBox.Text,
                    rawJsonBox.Text,
                    debugTraceBox.Text,
                    warningsItems,
                    boundaryItems);
                AssertQ8ScenarioHistoryAndExport(
                    viewModel,
                    exportButton,
                    historyList,
                    XiigenExtensionApiContract.OrchestrationPlanMethod,
                    exampleInput,
                    negativeInput,
                    "orchestration_reasoning_not_ready_degraded_dashboard_scenario");
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void ApiDashboardWindow_SkillSelectionSemanticQualityScenario_DrivesControlsAndShowsHonestDashboardEvidence()
        => RunOnStaThread(() =>
        {
            const string scenarioId = "dashboard-semantic-quality-skill-selection-1";
            const string exampleInput =
                "Task: plan-review dashboard QA; orchestration plan: review gates and docs sync; available skills: ui-testing, test-integrity, implementation-integrity, output-readability-gate, documentation-sync, git-automation.";
            const string negativeInput = "Missing SkillSelection inventory for task and orchestration plan.";
            var gateway = new RecordingGateway(request => request.UserPromptOrTaskText.Contains("Missing SkillSelection inventory", StringComparison.Ordinal)
                ? BuildSkillSelectionNotReadyScenarioResponse(request)
                : BuildSkillSelectionSemanticQualityScenarioResponse(request));
            var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
            var window = new ApiDashboardWindow(viewModel);

            try
            {
                window.Show();
                Flush(window);

                var apiList = FindRequired<ListBox>(window, "ApiListBox");
                var inputBox = FindRequired<TextBox>(window, "InputTextBox");
                var debugToggle = FindRequired<ToggleButton>(window, "DebugToggle");
                var rawJsonToggle = FindRequired<ToggleButton>(window, "RawJsonToggle");
                var runButton = FindRequired<Button>(window, "RunButton");
                var exportButton = FindRequired<Button>(window, "ExportHistoryButton");
                var resultBox = FindRequired<TextBox>(window, "HumanResultTextBox");
                var rawJsonBox = FindRequired<TextBox>(window, "RawJsonTextBox");
                var debugTraceBox = FindRequired<TextBox>(window, "DebugTraceTextBox");
                var boundaryItems = FindRequired<ItemsControl>(window, "BoundaryItemsControl");
                var warningsItems = FindRequired<ItemsControl>(window, "WarningsItemsControl");
                var historyList = FindRequired<ListBox>(window, "HistoryListBox");
                var selectedName = FindRequired<TextBlock>(window, "SelectedApiNameTextBlock");
                var inputDescription = FindRequired<TextBlock>(window, "SelectedApiInputDescriptionTextBlock");

                var skillSelectionApi = apiList.Items
                    .Cast<ApiDashboardCatalogRow>()
                    .Single(row => row.MethodId == XiigenExtensionApiContract.SkillSelectionMethod);
                SelectListBoxItem(apiList, skillSelectionApi);

                Assert.Equal("Select relevant skills for a task", selectedName.Text);
                Assert.DoesNotContain(XiigenExtensionApiContract.SkillSelectionMethod, selectedName.Text, StringComparison.Ordinal);
                Assert.Contains("available skills", inputDescription.Text, StringComparison.OrdinalIgnoreCase);

                SetText(inputBox, exampleInput);
                Toggle(debugToggle);
                Toggle(rawJsonToggle);
                InvokeButton(runButton);

                Assert.Equal(1, gateway.CallCount);
                Assert.NotNull(gateway.LastRequest);
                Assert.Equal(XiigenExtensionApiContract.SkillSelectionMethod, gateway.LastRequest!.Method);
                Assert.Equal(exampleInput, gateway.LastRequest.UserPromptOrTaskText);
                Assert.True(gateway.LastRequest.DebugEnabled);
                AssertSkillSelectionScenarioPositiveOutput(
                    scenarioId,
                    exampleInput,
                    resultBox.Text,
                    rawJsonBox.Text,
                    debugTraceBox.Text,
                    warningsItems,
                    boundaryItems);
                Assert.Single(historyList.Items);

                SetText(inputBox, negativeInput);
                InvokeButton(runButton);

                Assert.Equal(2, gateway.CallCount);
                Assert.NotNull(gateway.LastRequest);
                Assert.Equal(XiigenExtensionApiContract.SkillSelectionMethod, gateway.LastRequest!.Method);
                Assert.Equal(negativeInput, gateway.LastRequest.UserPromptOrTaskText);
                AssertSkillSelectionScenarioNotReadyOutput(
                    negativeInput,
                    resultBox.Text,
                    rawJsonBox.Text,
                    debugTraceBox.Text,
                    warningsItems,
                    boundaryItems);
                AssertQ8ScenarioHistoryAndExport(
                    viewModel,
                    exportButton,
                    historyList,
                    XiigenExtensionApiContract.SkillSelectionMethod,
                    exampleInput,
                    negativeInput,
                    "skill_selection_inventory_missing_degraded_dashboard_scenario");
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void ApiDashboardWindow_AutomationShowsCoreFailureViewer()
        => RunOnStaThread(() =>
        {
            var gateway = new RecordingGateway(_ => OperationResult<XiigenExtensionResponse>.Fail(
                "Injected core failure for WPF automation.",
                "CORE_FAILURE_FOR_WPF_TEST"));
            var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
            var window = new ApiDashboardWindow(viewModel);

            try
            {
                window.Show();
                Flush(window);

                var apiList = FindRequired<ListBox>(window, "ApiListBox");
                var inputBox = FindRequired<TextBox>(window, "InputTextBox");
                var runButton = FindRequired<Button>(window, "RunButton");
                var resultBox = FindRequired<TextBox>(window, "HumanResultTextBox");
                var errorPanel = FindRequired<StackPanel>(window, "ErrorPanel");
                var errorText = FindRequired<TextBlock>(window, "ErrorTextBlock");
                var historyList = FindRequired<ListBox>(window, "HistoryListBox");

                var promptApi = apiList.Items
                    .Cast<ApiDashboardCatalogRow>()
                    .Single(row => row.MethodId == XiigenExtensionApiContract.PromptToTaskMethod);
                SelectListBoxItem(apiList, promptApi);
                SetText(inputBox, "Trigger a controlled core failure through the WPF run button.");
                InvokeButton(runButton);

                Assert.Equal(1, gateway.CallCount);
                Assert.Equal(Visibility.Visible, errorPanel.Visibility);
                Assert.Contains("Dashboard core call failed before a xiigen response was returned.", resultBox.Text);
                Assert.Contains("CORE_FAILURE_FOR_WPF_TEST", errorText.Text);
                Assert.Contains("Injected core failure for WPF automation.", errorText.Text);
                Assert.Single(historyList.Items);
                var failedHistory = Assert.IsType<ApiDashboardHistoryEntry>(historyList.Items[0]);
                Assert.False(failedHistory.Success);
                Assert.Equal(ApiDashboardRunner.DashboardCoreOperationFailedStatus, failedHistory.Status);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void ApiDashboardWindow_AutomationShowsUnknownMethodFromControlledInvalidSelection()
        => RunOnStaThread(() =>
        {
            var gateway = new RecordingGateway(request => BuildSuccessfulResponse(request));
            var viewModel = new ApiDashboardViewModel(new ApiDashboardRunner(gateway));
            var window = new ApiDashboardWindow(viewModel);

            try
            {
                window.Show();
                Flush(window);

                var inputBox = FindRequired<TextBox>(window, "InputTextBox");
                var runButton = FindRequired<Button>(window, "RunButton");
                var errorPanel = FindRequired<StackPanel>(window, "ErrorPanel");
                var errorText = FindRequired<TextBlock>(window, "ErrorTextBlock");
                var selectedName = FindRequired<TextBlock>(window, "SelectedApiNameTextBlock");

                viewModel.SelectedApi = BuildUnsupportedMethodRow();
                Flush(window);
                Assert.Equal("Unsupported method test hook", selectedName.Text);

                SetText(inputBox, "Drive unknown method through the WPF run button.");
                InvokeButton(runButton);

                Assert.Equal(0, gateway.CallCount);
                Assert.Equal(Visibility.Visible, errorPanel.Visibility);
                Assert.Contains("API_DASHBOARD_UNSUPPORTED_METHOD", errorText.Text);
                Assert.Contains("not-a-real-xiigen-method", errorText.Text);
            }
            finally
            {
                window.Close();
            }
        });

    private static void RunOnStaThread(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        })
        {
            IsBackground = true
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!thread.Join(StaTestTimeout))
        {
            throw new TimeoutException(
                $"WPF dashboard automation did not finish within {StaTestTimeout.TotalSeconds:0} seconds. STA thread state: {thread.ThreadState}.");
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private static T FindRequired<T>(
        FrameworkElement root,
        string name)
        where T : class
    {
        if (root.FindName(name) is not T element)
        {
            throw new InvalidOperationException($"Expected WPF element '{name}' of type {typeof(T).Name}.");
        }

        return element;
    }

    private static void AssertUx(
        DependencyObject element,
        string expectedName,
        string expectedHelpSubstring)
    {
        Assert.Equal(expectedName, AutomationProperties.GetName(element));
        Assert.Contains(expectedHelpSubstring, AutomationProperties.GetHelpText(element), StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertTooltip(FrameworkElement element)
        => Assert.False(string.IsNullOrWhiteSpace(element.ToolTip?.ToString()));

    private static void AssertKeyboardFocusOrder(
        Window window,
        params Control[] orderedControls)
    {
        Assert.NotEmpty(orderedControls);
        for (var index = 0; index < orderedControls.Length; index++)
        {
            Assert.Equal(index + 1, orderedControls[index].TabIndex);
            Assert.True(orderedControls[index].Focusable, $"{orderedControls[index].Name} must be a keyboard focus stop.");
        }

        Assert.True(orderedControls[0].Focus(), $"{orderedControls[0].Name} did not accept initial keyboard focus.");
        Flush(window);
        AssertFocusedWithin(orderedControls[0]);

        foreach (var expectedNext in orderedControls.Skip(1))
        {
            var focusedElement = Assert.IsAssignableFrom<UIElement>(Keyboard.FocusedElement);
            Assert.True(
                focusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)),
                $"Could not move keyboard focus from {GetElementName(focusedElement)} to {expectedNext.Name}.");
            Flush(window);
            AssertFocusedWithin(expectedNext);
        }
    }

    private static void AssertFocusedWithin(DependencyObject expectedContainer)
    {
        var focused = Assert.IsAssignableFrom<DependencyObject>(Keyboard.FocusedElement);
        Assert.True(
            ReferenceEquals(expectedContainer, focused) || IsVisualDescendant(expectedContainer, focused),
            $"Expected keyboard focus inside {GetElementName(expectedContainer)}, actual focus was {GetElementName(focused)}.");
    }

    private static bool IsVisualDescendant(
        DependencyObject ancestor,
        DependencyObject descendant)
    {
        for (var current = descendant; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, ancestor))
                return true;
        }

        return false;
    }

    private static string GetElementName(DependencyObject element)
        => element is FrameworkElement frameworkElement && !string.IsNullOrWhiteSpace(frameworkElement.Name)
            ? frameworkElement.Name
            : element.GetType().Name;

    private static void AssertNoPrimaryPathJargon(params string[] values)
    {
        var joined = string.Join(" ", values);
        Assert.DoesNotContain("runtime_extension_claimed", joined, StringComparison.Ordinal);
        Assert.DoesNotContain("semantic_quality_claimed", joined, StringComparison.Ordinal);
        Assert.DoesNotContain("core_boundary_only", joined, StringComparison.Ordinal);
        Assert.DoesNotContain("prompt-to-task", joined, StringComparison.Ordinal);
        Assert.DoesNotContain("orchestration-plan", joined, StringComparison.Ordinal);
        Assert.DoesNotContain("skill-selection", joined, StringComparison.Ordinal);
    }

    private static void AssertNonOverlappingColumns(
        Window window,
        FrameworkElement left,
        FrameworkElement middle,
        FrameworkElement right)
    {
        var leftBounds = GetBounds(window, left);
        var middleBounds = GetBounds(window, middle);
        var rightBounds = GetBounds(window, right);

        Assert.False(leftBounds.IntersectsWith(middleBounds));
        Assert.False(middleBounds.IntersectsWith(rightBounds));
        Assert.True(leftBounds.Right <= middleBounds.Left);
        Assert.True(middleBounds.Right <= rightBounds.Left);
    }

    private static Rect GetBounds(
        Visual root,
        FrameworkElement element)
        => element.TransformToAncestor(root)
            .TransformBounds(new Rect(new Size(element.ActualWidth, element.ActualHeight)));

    private static void SelectListBoxItem(
        ListBox listBox,
        object item)
    {
        listBox.ScrollIntoView(item);
        Flush(listBox);

        if (listBox.ItemContainerGenerator.ContainerFromItem(item) is not ListBoxItem listBoxItem)
        {
            throw new InvalidOperationException("Expected the dashboard API list to generate a selectable ListBoxItem.");
        }

        var selectorPeer = new ListBoxAutomationPeer(listBox);
        var provider = new ListBoxItemAutomationPeer(item, selectorPeer).GetPattern(PatternInterface.SelectionItem) as ISelectionItemProvider;
        if (provider is null)
        {
            throw new InvalidOperationException("Expected the dashboard API item to expose the WPF selection automation pattern.");
        }

        provider.Select();
        listBox.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
        Flush(listBox);
    }

    private static void SetText(
        TextBox textBox,
        string value)
    {
        var provider = new TextBoxAutomationPeer(textBox).GetPattern(PatternInterface.Value) as IValueProvider;
        if (provider is null)
        {
            throw new InvalidOperationException("Expected the dashboard text box to expose the WPF value automation pattern.");
        }

        provider.SetValue(value);
        textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        Flush(textBox);
    }

    private static void InvokeButton(Button button)
    {
        var provider = new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
        if (provider is null)
        {
            throw new InvalidOperationException("Expected the dashboard button to expose the WPF invoke automation pattern.");
        }

        provider.Invoke();
        Flush(button);
    }

    private static void Toggle(ToggleButton toggleButton)
    {
        var provider = new ToggleButtonAutomationPeer(toggleButton).GetPattern(PatternInterface.Toggle) as IToggleProvider;
        if (provider is null)
        {
            throw new InvalidOperationException("Expected the dashboard toggle to expose the WPF toggle automation pattern.");
        }

        provider.Toggle();
        Flush(toggleButton);
    }

    private static void Flush(FrameworkElement element)
    {
        element.UpdateLayout();
        element.Dispatcher.Invoke(() => { }, DispatcherPriority.Background);
        element.UpdateLayout();
    }

    private static OperationResult<XiigenExtensionResponse> BuildSuccessfulResponse(
        XiigenExtensionRequest request)
        => OperationResult<XiigenExtensionResponse>.Ok(
            new XiigenExtensionResponse(
                Success: true,
                Status: XiigenExtensionApiService.PartialBoundaryStatus,
                ChatDisplayText: "Mapped WPF dashboard response.",
                SelectedSkills: ["ui-testing"],
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
                StructuredTaskRefs: ["structured-task:wpf-window-test"],
                ScenarioRefs: ["scenario:wpf-window-test"],
                Warnings: ["semantic_quality_claimed=false", "core_boundary_only_no_host_runtime_claim"],
                Errors: [],
                DebugTrace: request.DebugEnabled
                    ? new XiigenDebugTrace(request.Method, "safe wpf input", "safe wpf output")
                    : null));

    private static OperationResult<XiigenExtensionResponse> BuildPromptToTaskSemanticQualityScenarioResponse(
        XiigenExtensionRequest request)
        => OperationResult<XiigenExtensionResponse>.Ok(
            new XiigenExtensionResponse(
                Success: true,
                Status: XiigenExtensionApiService.PartialBoundaryStatus,
                ChatDisplayText: string.Join(
                    Environment.NewLine,
                    "PromptToTask dashboard scenario result.",
                    "Task type: plan_review_or_plan_repair.",
                    "Goal: improve skill selection planning without starting implementation.",
                    "Constraints: plan only; use review gates; keep quoted transcripts as evidence only.",
                    "Forbidden actions: do not implement; do not train; do not commit.",
                    "Required evidence: dashboard QA scenario, structured task ref, review evidence.",
                    "implementation_authorized=false.",
                    "Structured task ref: structured-task:dashboard-semantic-quality-ptt-1."),
                SelectedSkills: [],
                CheckSetResults: new Dictionary<string, string>
                {
                    ["core_boundary_executed"] = "true",
                    ["scenario_id"] = "dashboard-semantic-quality-prompt-to-task-1",
                    ["evidence_scope"] = "dashboard_api_only_not_model_quality_not_host_runtime",
                    ["implementation_authorized"] = "false",
                    ["constraints"] = "plan_only;review_gates;quoted_transcripts_evidence_only",
                    ["forbidden_actions"] = "do_not_implement;do_not_train;do_not_commit",
                    ["required_evidence"] = "dashboard_qa_scenario;structured_task_ref;review_evidence",
                    ["redacted_input_ref"] = "redacted_input_ref:prompt-to-task-q8-01",
                    ["output_fields"] = "task_type,goal,constraints,forbidden_actions,required_evidence,implementation_authorized",
                    ["raw_private_prompt_copied_as_training_evidence"] = "false",
                    ["runtime_extension_claimed"] = "false",
                    ["semantic_quality_claimed"] = "false",
                    ["trained_model_proof_claimed"] = "false",
                    ["host_runtime_claimed"] = "false",
                    ["script_parity_used"] = "false",
                    ["external_ai"] = "false"
                },
                StructuredTaskRefs: ["structured-task:dashboard-semantic-quality-prompt-to-task-1"],
                ScenarioRefs: ["scenario:dashboard-semantic-quality-prompt-to-task-1"],
                Warnings:
                [
                    "semantic_quality_claimed=false",
                    "trained_model_proof_claimed=false",
                    "host/runtime=false",
                    "dashboard_api_only_not_model_quality_not_host_runtime",
                    "raw_private_prompt_copied_as_training_evidence=false"
                ],
                Errors: [],
                DebugTrace: request.DebugEnabled
                    ? new XiigenDebugTrace(
                        request.Method,
                        "redacted_input_ref:prompt-to-task-q8-01",
                        "output_fields:task_type,goal,constraints,forbidden_actions,required_evidence,implementation_authorized=false")
                    : null));

    private static OperationResult<XiigenExtensionResponse> BuildPromptToTaskNotReadyScenarioResponse(
        XiigenExtensionRequest request)
        => OperationResult<XiigenExtensionResponse>.Ok(
            new XiigenExtensionResponse(
                Success: false,
                Status: "prompt_to_task_not_ready_degraded_dashboard_scenario",
                ChatDisplayText: string.Join(
                    Environment.NewLine,
                    "PromptToTask is not ready for this dashboard scenario.",
                    "Missing or untrained checkpoint is visible as a degraded response.",
                    "No semantic quality or trained-model proof is claimed.",
                    "implementation_authorized=false remains a dashboard-visible safety boundary."),
                SelectedSkills: [],
                CheckSetResults: new Dictionary<string, string>
                {
                    ["core_boundary_executed"] = "true",
                    ["scenario_id"] = "dashboard-semantic-quality-prompt-to-task-1-negative",
                    ["evidence_scope"] = "dashboard_api_only_not_model_quality_not_host_runtime",
                    ["degraded_or_not_ready_visible"] = "true",
                    ["implementation_authorized"] = "false",
                    ["raw_private_prompt_copied_as_training_evidence"] = "false",
                    ["runtime_extension_claimed"] = "false",
                    ["semantic_quality_claimed"] = "false",
                    ["trained_model_proof_claimed"] = "false",
                    ["host_runtime_claimed"] = "false",
                    ["script_parity_used"] = "false",
                    ["external_ai"] = "false"
                },
                StructuredTaskRefs: ["structured-task:dashboard-semantic-quality-prompt-to-task-1-negative"],
                ScenarioRefs: ["scenario:dashboard-semantic-quality-prompt-to-task-1-negative"],
                Warnings:
                [
                    "PromptToTask not ready: missing or untrained checkpoint.",
                    "semantic_quality_claimed=false",
                    "trained_model_proof_claimed=false",
                    "host/runtime=false",
                    "dashboard_api_only_not_model_quality_not_host_runtime"
                ],
                Errors: ["PROMPT_TO_TASK_NOT_READY_FOR_SEMANTIC_QUALITY"],
                DebugTrace: request.DebugEnabled
                    ? new XiigenDebugTrace(
                        request.Method,
                        "redacted_input_ref:prompt-to-task-q8-01-negative",
                        "output_fields:not_ready_status,warning_codes,implementation_authorized=false")
                    : null));

    private static OperationResult<XiigenExtensionResponse> BuildOrchestrationReasoningSemanticQualityScenarioResponse(
        XiigenExtensionRequest request)
        => OperationResult<XiigenExtensionResponse>.Ok(
            new XiigenExtensionResponse(
                Success: true,
                Status: XiigenExtensionApiService.PartialBoundaryStatus,
                ChatDisplayText: string.Join(
                    Environment.NewLine,
                    "OrchestrationReasoning dashboard scenario result.",
                    "Selected orchestration family: plan_review_or_plan_repair.",
                    "Scenario refs: scenario:dashboard-semantic-quality-orchestration-1; scenario:review-gate-boundary.",
                    "Required gates: authority ledger, four review cycles, documentation sync, dashboard QA.",
                    "Implementation is not authorized: warn before executor work.",
                    "Malformed or untrained honesty remains visible; this fixture does not prove model quality."),
                SelectedSkills: [],
                CheckSetResults: new Dictionary<string, string>
                {
                    ["core_boundary_executed"] = "true",
                    ["scenario_id"] = "dashboard-semantic-quality-orchestration-1",
                    ["evidence_scope"] = "dashboard_api_only_not_model_quality_not_host_runtime",
                    ["orchestration_family"] = "plan_review_or_plan_repair",
                    ["selected_scenario_refs"] = "scenario:dashboard-semantic-quality-orchestration-1;scenario:review-gate-boundary",
                    ["required_gates"] = "authority_ledger;four_review_cycles;documentation_sync;dashboard_qa",
                    ["implementation_authorized"] = "false",
                    ["implementation_warning"] = "warn_when_implementation_not_authorized",
                    ["rejected_scenario_reasons"] = "technical_execution_recipe_rejected;host_runtime_claim_rejected",
                    ["redacted_task_ref"] = "redacted_task_ref:orchestration-q8-02",
                    ["output_fields"] = "orchestration_family,scenario_refs,required_gates,warnings,implementation_authorized",
                    ["malformed_or_untrained_honesty"] = "true",
                    ["plan_invented"] = "false",
                    ["raw_private_task_copied_as_training_evidence"] = "false",
                    ["runtime_extension_claimed"] = "false",
                    ["semantic_quality_claimed"] = "false",
                    ["trained_model_proof_claimed"] = "false",
                    ["host_runtime_claimed"] = "false",
                    ["script_parity_used"] = "false",
                    ["external_ai"] = "false"
                },
                StructuredTaskRefs: ["structured-task:dashboard-semantic-quality-orchestration-1"],
                ScenarioRefs:
                [
                    "scenario:dashboard-semantic-quality-orchestration-1",
                    "scenario:review-gate-boundary"
                ],
                Warnings:
                [
                    "implementation_authorized=false: warn before executor work",
                    "semantic_quality_claimed=false",
                    "trained_model_proof_claimed=false",
                    "host/runtime=false",
                    "dashboard_api_only_not_model_quality_not_host_runtime"
                ],
                Errors: [],
                DebugTrace: request.DebugEnabled
                    ? new XiigenDebugTrace(
                        request.Method,
                        "redacted_task_ref:orchestration-q8-02",
                        "output_fields:orchestration_family,scenario_refs,required_gates,warnings,implementation_authorized=false")
                    : null));

    private static OperationResult<XiigenExtensionResponse> BuildOrchestrationReasoningNotReadyScenarioResponse(
        XiigenExtensionRequest request)
        => OperationResult<XiigenExtensionResponse>.Ok(
            new XiigenExtensionResponse(
                Success: false,
                Status: "orchestration_reasoning_not_ready_degraded_dashboard_scenario",
                ChatDisplayText: string.Join(
                    Environment.NewLine,
                    "OrchestrationReasoning is not ready to plan from this dashboard input.",
                    "Empty, malformed, or untrained task input is visible as a degraded response.",
                    "No orchestration plan is invented and no semantic quality proof is claimed.",
                    "implementation_authorized=false remains a dashboard-visible safety boundary."),
                SelectedSkills: [],
                CheckSetResults: new Dictionary<string, string>
                {
                    ["core_boundary_executed"] = "true",
                    ["scenario_id"] = "dashboard-semantic-quality-orchestration-1-negative",
                    ["evidence_scope"] = "dashboard_api_only_not_model_quality_not_host_runtime",
                    ["degraded_or_not_ready_visible"] = "true",
                    ["malformed_input_visible"] = "true",
                    ["implementation_authorized"] = "false",
                    ["plan_invented"] = "false",
                    ["raw_private_task_copied_as_training_evidence"] = "false",
                    ["runtime_extension_claimed"] = "false",
                    ["semantic_quality_claimed"] = "false",
                    ["trained_model_proof_claimed"] = "false",
                    ["host_runtime_claimed"] = "false",
                    ["script_parity_used"] = "false",
                    ["external_ai"] = "false"
                },
                StructuredTaskRefs: ["structured-task:dashboard-semantic-quality-orchestration-1-negative"],
                ScenarioRefs: ["scenario:dashboard-semantic-quality-orchestration-1-negative"],
                Warnings:
                [
                    "OrchestrationReasoning not ready: empty, malformed, or untrained input.",
                    "plan_invented=false",
                    "semantic_quality_claimed=false",
                    "trained_model_proof_claimed=false",
                    "host/runtime=false",
                    "dashboard_api_only_not_model_quality_not_host_runtime"
                ],
                Errors: ["ORCHESTRATION_REASONING_NOT_READY_FOR_SEMANTIC_QUALITY"],
                DebugTrace: request.DebugEnabled
                    ? new XiigenDebugTrace(
                        request.Method,
                        "redacted_task_ref:orchestration-q8-02-negative",
                        "output_fields:not_ready_status,warning_codes,implementation_authorized=false")
                    : null));

    private static OperationResult<XiigenExtensionResponse> BuildSkillSelectionSemanticQualityScenarioResponse(
        XiigenExtensionRequest request)
        => OperationResult<XiigenExtensionResponse>.Ok(
            new XiigenExtensionResponse(
                Success: true,
                Status: XiigenExtensionApiService.PartialBoundaryStatus,
                ChatDisplayText: string.Join(
                    Environment.NewLine,
                    "SkillSelection dashboard scenario result.",
                    "Chosen skills: ui-testing, test-integrity, implementation-integrity, output-readability-gate, documentation-sync.",
                    "Rejected tempting skills: git-automation because commit is forbidden; external-ai-tooling because external AI is forbidden; broad-test-runner because verification is focused.",
                    "Candidate-position scaffold warning: candidate position is not final semantic authority.",
                    "Candidate-position scaffold is not success proof.",
                    "Missing inventory or untrained honesty remains visible; this fixture does not prove model quality."),
                SelectedSkills:
                [
                    "ui-testing",
                    "test-integrity",
                    "implementation-integrity",
                    "output-readability-gate",
                    "documentation-sync"
                ],
                CheckSetResults: new Dictionary<string, string>
                {
                    ["core_boundary_executed"] = "true",
                    ["scenario_id"] = "dashboard-semantic-quality-skill-selection-1",
                    ["evidence_scope"] = "dashboard_api_only_not_model_quality_not_host_runtime",
                    ["chosen_skills"] = "ui-testing;test-integrity;implementation-integrity;output-readability-gate;documentation-sync",
                    ["rejected_tempting_skills"] = "git-automation:commit_forbidden;external-ai-tooling:external_ai_forbidden;broad-test-runner:focused_verification_only",
                    ["candidate_position_scaffold_warning"] = "candidate_position_scaffold_not_final_authority",
                    ["candidate_position_final_authority"] = "false",
                    ["candidate_position_success_proof_claimed"] = "false",
                    ["missing_inventory_untrained_honesty"] = "true",
                    ["redacted_task_plan_ref"] = "redacted_task_plan_ref:skill-selection-q8-02",
                    ["output_fields"] = "chosen_skills,rejected_tempting_skills,candidate_position_scaffold_warning,candidate_position_success_proof_claimed,warnings",
                    ["raw_private_task_plan_copied_as_training_evidence"] = "false",
                    ["runtime_extension_claimed"] = "false",
                    ["semantic_quality_claimed"] = "false",
                    ["trained_model_proof_claimed"] = "false",
                    ["host_runtime_claimed"] = "false",
                    ["script_parity_used"] = "false",
                    ["external_ai"] = "false"
                },
                StructuredTaskRefs: ["structured-task:dashboard-semantic-quality-skill-selection-1"],
                ScenarioRefs: ["scenario:dashboard-semantic-quality-skill-selection-1"],
                Warnings:
                [
                    "candidate_position_scaffold_not_final_authority",
                    "candidate_position_success_proof_claimed=false",
                    "semantic_quality_claimed=false",
                    "trained_model_proof_claimed=false",
                    "host/runtime=false",
                    "dashboard_api_only_not_model_quality_not_host_runtime"
                ],
                Errors: [],
                DebugTrace: request.DebugEnabled
                    ? new XiigenDebugTrace(
                        request.Method,
                        "redacted_task_plan_ref:skill-selection-q8-02",
                        "output_fields:chosen_skills,rejected_tempting_skills,candidate_position_scaffold_warning,candidate_position_final_authority=false,candidate_position_success_proof_claimed=false")
                    : null));

    private static OperationResult<XiigenExtensionResponse> BuildSkillSelectionNotReadyScenarioResponse(
        XiigenExtensionRequest request)
        => OperationResult<XiigenExtensionResponse>.Ok(
            new XiigenExtensionResponse(
                Success: false,
                Status: "skill_selection_inventory_missing_degraded_dashboard_scenario",
                ChatDisplayText: string.Join(
                    Environment.NewLine,
                    "SkillSelection is not ready for this dashboard scenario.",
                    "Missing skill inventory or untrained checkpoint is visible as a degraded response.",
                    "The dashboard does not silently fall back to candidate position as semantic proof.",
                    "Candidate-position scaffold is not success proof.",
                    "No semantic quality or trained-model proof is claimed."),
                SelectedSkills: [],
                CheckSetResults: new Dictionary<string, string>
                {
                    ["core_boundary_executed"] = "true",
                    ["scenario_id"] = "dashboard-semantic-quality-skill-selection-1-negative",
                    ["evidence_scope"] = "dashboard_api_only_not_model_quality_not_host_runtime",
                    ["degraded_or_not_ready_visible"] = "true",
                    ["missing_skill_inventory_visible"] = "true",
                    ["candidate_position_final_authority"] = "false",
                    ["candidate_position_success_proof_claimed"] = "false",
                    ["raw_private_task_plan_copied_as_training_evidence"] = "false",
                    ["runtime_extension_claimed"] = "false",
                    ["semantic_quality_claimed"] = "false",
                    ["trained_model_proof_claimed"] = "false",
                    ["host_runtime_claimed"] = "false",
                    ["script_parity_used"] = "false",
                    ["external_ai"] = "false"
                },
                StructuredTaskRefs: ["structured-task:dashboard-semantic-quality-skill-selection-1-negative"],
                ScenarioRefs: ["scenario:dashboard-semantic-quality-skill-selection-1-negative"],
                Warnings:
                [
                    "SkillSelection not ready: missing inventory or untrained checkpoint.",
                    "candidate_position_final_authority=false",
                    "candidate_position_success_proof_claimed=false",
                    "semantic_quality_claimed=false",
                    "trained_model_proof_claimed=false",
                    "host/runtime=false",
                    "dashboard_api_only_not_model_quality_not_host_runtime"
                ],
                Errors: ["SKILL_SELECTION_INVENTORY_NOT_READY_FOR_SEMANTIC_QUALITY"],
                DebugTrace: request.DebugEnabled
                    ? new XiigenDebugTrace(
                        request.Method,
                        "redacted_task_plan_ref:skill-selection-q8-02-negative",
                        "output_fields:not_ready_status,warning_codes,candidate_position_final_authority=false,candidate_position_success_proof_claimed=false")
                    : null));

    private static void AssertPromptToTaskScenarioPositiveOutput(
        string scenarioId,
        string rawPrompt,
        string resultText,
        string rawJsonText,
        string debugTraceText,
        ItemsControl warningsItems,
        ItemsControl boundaryItems)
    {
        Assert.Contains("Task type: plan_review_or_plan_repair", resultText, StringComparison.Ordinal);
        Assert.Contains("Goal: improve skill selection planning", resultText, StringComparison.Ordinal);
        Assert.Contains("Constraints: plan only", resultText, StringComparison.Ordinal);
        Assert.Contains("Forbidden actions: do not implement", resultText, StringComparison.Ordinal);
        Assert.Contains("Required evidence: dashboard QA scenario", resultText, StringComparison.Ordinal);
        Assert.Contains("implementation_authorized=false", resultText, StringComparison.Ordinal);
        Assert.Contains("structured-task:dashboard-semantic-quality-ptt-1", resultText, StringComparison.Ordinal);
        Assert.DoesNotContain(XiigenExtensionApiContract.PromptToTaskMethod, resultText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrompt, resultText, StringComparison.Ordinal);
        Assert.DoesNotContain("redacted_input_ref", resultText, StringComparison.Ordinal);
        Assert.DoesNotContain("output_fields", resultText, StringComparison.Ordinal);

        Assert.Contains(scenarioId, rawJsonText, StringComparison.Ordinal);
        Assert.Contains(XiigenExtensionApiContract.PromptToTaskMethod, rawJsonText, StringComparison.Ordinal);
        Assert.Contains("redacted_input_ref:prompt-to-task-q8-01", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("output_fields", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("raw_private_prompt_copied_as_training_evidence", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("dashboard_api_only_not_model_quality_not_host_runtime", rawJsonText, StringComparison.Ordinal);
        AssertPromptToTaskCheckSetValue(rawJsonText, "scenario_id", scenarioId);
        AssertPromptToTaskCheckSetValue(rawJsonText, "implementation_authorized", "false");
        AssertPromptToTaskCheckSetValue(rawJsonText, "constraints", "plan_only;review_gates;quoted_transcripts_evidence_only");
        AssertPromptToTaskCheckSetValue(rawJsonText, "forbidden_actions", "do_not_implement;do_not_train;do_not_commit");
        AssertPromptToTaskCheckSetValue(rawJsonText, "required_evidence", "dashboard_qa_scenario;structured_task_ref;review_evidence");
        AssertPromptToTaskCheckSetValue(rawJsonText, "raw_private_prompt_copied_as_training_evidence", "false");
        AssertPromptToTaskCheckSetValue(rawJsonText, "semantic_quality_claimed", "false");
        AssertPromptToTaskCheckSetValue(rawJsonText, "trained_model_proof_claimed", "false");
        AssertPromptToTaskCheckSetValue(rawJsonText, "host_runtime_claimed", "false");
        Assert.DoesNotContain(rawPrompt, rawJsonText, StringComparison.Ordinal);

        Assert.Contains($"xiigen {XiigenExtensionApiContract.PromptToTaskMethod} :", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("input: redacted_input_ref:prompt-to-task-q8-01", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("output: output_fields:task_type,goal,constraints,forbidden_actions,required_evidence,implementation_authorized=false", debugTraceText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrompt, debugTraceText, StringComparison.Ordinal);

        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("trained_model_proof_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("host/runtime=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("dashboard_api_only_not_model_quality_not_host_runtime", StringComparison.Ordinal));
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "semantic_quality_claimed"
                && !flag.Claimed
                && flag.DisplayText.Contains("not semantic quality or trained-model proof", StringComparison.Ordinal));
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "runtime_extension_claimed"
                && !flag.Claimed
                && flag.DisplayText.Contains("not Claude, Cursor, or Codex host runtime proof", StringComparison.Ordinal));
    }

    private static void AssertPromptToTaskScenarioNotReadyOutput(
        string rawPrompt,
        string resultText,
        string rawJsonText,
        string debugTraceText,
        ItemsControl warningsItems,
        ItemsControl boundaryItems)
    {
        Assert.Contains("PromptToTask is not ready", resultText, StringComparison.Ordinal);
        Assert.Contains("Missing or untrained checkpoint", resultText, StringComparison.Ordinal);
        Assert.Contains("No semantic quality or trained-model proof is claimed", resultText, StringComparison.Ordinal);
        Assert.Contains("implementation_authorized=false", resultText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrompt, resultText, StringComparison.Ordinal);

        Assert.Contains("prompt_to_task_not_ready_degraded_dashboard_scenario", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("degraded_or_not_ready_visible", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("PROMPT_TO_TASK_NOT_READY_FOR_SEMANTIC_QUALITY", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("dashboard_api_only_not_model_quality_not_host_runtime", rawJsonText, StringComparison.Ordinal);
        AssertPromptToTaskCheckSetValue(rawJsonText, "scenario_id", "dashboard-semantic-quality-prompt-to-task-1-negative");
        AssertPromptToTaskCheckSetValue(rawJsonText, "degraded_or_not_ready_visible", "true");
        AssertPromptToTaskCheckSetValue(rawJsonText, "implementation_authorized", "false");
        AssertPromptToTaskCheckSetValue(rawJsonText, "raw_private_prompt_copied_as_training_evidence", "false");
        AssertPromptToTaskCheckSetValue(rawJsonText, "semantic_quality_claimed", "false");
        AssertPromptToTaskCheckSetValue(rawJsonText, "trained_model_proof_claimed", "false");
        AssertPromptToTaskCheckSetValue(rawJsonText, "host_runtime_claimed", "false");
        Assert.DoesNotContain(rawPrompt, rawJsonText, StringComparison.Ordinal);

        Assert.Contains($"xiigen {XiigenExtensionApiContract.PromptToTaskMethod} :", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("input: redacted_input_ref:prompt-to-task-q8-01-negative", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("output: output_fields:not_ready_status,warning_codes,implementation_authorized=false", debugTraceText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrompt, debugTraceText, StringComparison.Ordinal);

        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("missing or untrained checkpoint", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "semantic_quality_claimed"
                && !flag.Claimed);
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "runtime_extension_claimed"
                && !flag.Claimed);
    }

    private static void AssertOrchestrationReasoningScenarioPositiveOutput(
        string scenarioId,
        string rawTask,
        string resultText,
        string rawJsonText,
        string debugTraceText,
        ItemsControl warningsItems,
        ItemsControl boundaryItems)
    {
        Assert.Contains("Selected orchestration family: plan_review_or_plan_repair", resultText, StringComparison.Ordinal);
        Assert.Contains("Scenario refs: scenario:dashboard-semantic-quality-orchestration-1", resultText, StringComparison.Ordinal);
        Assert.Contains("scenario:review-gate-boundary", resultText, StringComparison.Ordinal);
        Assert.Contains("Required gates: authority ledger", resultText, StringComparison.Ordinal);
        Assert.Contains("four review cycles", resultText, StringComparison.Ordinal);
        Assert.Contains("documentation sync", resultText, StringComparison.Ordinal);
        Assert.Contains("dashboard QA", resultText, StringComparison.Ordinal);
        Assert.Contains("Implementation is not authorized", resultText, StringComparison.Ordinal);
        Assert.Contains("Malformed or untrained honesty", resultText, StringComparison.Ordinal);
        Assert.DoesNotContain(XiigenExtensionApiContract.OrchestrationPlanMethod, resultText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawTask, resultText, StringComparison.Ordinal);
        Assert.DoesNotContain("redacted_task_ref", resultText, StringComparison.Ordinal);
        Assert.DoesNotContain("output_fields", resultText, StringComparison.Ordinal);

        Assert.Contains(scenarioId, rawJsonText, StringComparison.Ordinal);
        Assert.Contains(XiigenExtensionApiContract.OrchestrationPlanMethod, rawJsonText, StringComparison.Ordinal);
        Assert.Contains("redacted_task_ref:orchestration-q8-02", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("output_fields", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("rejected_scenario_reasons", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("dashboard_api_only_not_model_quality_not_host_runtime", rawJsonText, StringComparison.Ordinal);
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "scenario_id", scenarioId);
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "orchestration_family", "plan_review_or_plan_repair");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "selected_scenario_refs", "scenario:dashboard-semantic-quality-orchestration-1;scenario:review-gate-boundary");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "required_gates", "authority_ledger;four_review_cycles;documentation_sync;dashboard_qa");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "implementation_authorized", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "implementation_warning", "warn_when_implementation_not_authorized");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "raw_private_task_copied_as_training_evidence", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "semantic_quality_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "trained_model_proof_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "host_runtime_claimed", "false");
        Assert.DoesNotContain(rawTask, rawJsonText, StringComparison.Ordinal);

        Assert.Contains($"xiigen {XiigenExtensionApiContract.OrchestrationPlanMethod} :", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("input: redacted_task_ref:orchestration-q8-02", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("output: output_fields:orchestration_family,scenario_refs,required_gates,warnings,implementation_authorized=false", debugTraceText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawTask, debugTraceText, StringComparison.Ordinal);

        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("implementation_authorized=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("trained_model_proof_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("host/runtime=false", StringComparison.Ordinal));
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "semantic_quality_claimed"
                && !flag.Claimed);
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "runtime_extension_claimed"
                && !flag.Claimed);
    }

    private static void AssertOrchestrationReasoningScenarioNotReadyOutput(
        string rawTask,
        string resultText,
        string rawJsonText,
        string debugTraceText,
        ItemsControl warningsItems,
        ItemsControl boundaryItems)
    {
        Assert.Contains("OrchestrationReasoning is not ready", resultText, StringComparison.Ordinal);
        Assert.Contains("Empty, malformed, or untrained task input", resultText, StringComparison.Ordinal);
        Assert.Contains("No orchestration plan is invented", resultText, StringComparison.Ordinal);
        Assert.Contains("implementation_authorized=false", resultText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawTask, resultText, StringComparison.Ordinal);

        Assert.Contains("orchestration_reasoning_not_ready_degraded_dashboard_scenario", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("malformed_input_visible", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("ORCHESTRATION_REASONING_NOT_READY_FOR_SEMANTIC_QUALITY", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("dashboard_api_only_not_model_quality_not_host_runtime", rawJsonText, StringComparison.Ordinal);
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "scenario_id", "dashboard-semantic-quality-orchestration-1-negative");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "degraded_or_not_ready_visible", "true");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "malformed_input_visible", "true");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "implementation_authorized", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "plan_invented", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "raw_private_task_copied_as_training_evidence", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "semantic_quality_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "trained_model_proof_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "host_runtime_claimed", "false");
        Assert.DoesNotContain(rawTask, rawJsonText, StringComparison.Ordinal);

        Assert.Contains($"xiigen {XiigenExtensionApiContract.OrchestrationPlanMethod} :", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("input: redacted_task_ref:orchestration-q8-02-negative", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("output: output_fields:not_ready_status,warning_codes,implementation_authorized=false", debugTraceText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawTask, debugTraceText, StringComparison.Ordinal);

        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("empty, malformed, or untrained", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("plan_invented=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("trained_model_proof_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("host/runtime=false", StringComparison.Ordinal));
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "semantic_quality_claimed"
                && !flag.Claimed);
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "runtime_extension_claimed"
                && !flag.Claimed);
    }

    private static void AssertSkillSelectionScenarioPositiveOutput(
        string scenarioId,
        string rawTaskPlan,
        string resultText,
        string rawJsonText,
        string debugTraceText,
        ItemsControl warningsItems,
        ItemsControl boundaryItems)
    {
        Assert.Contains("Chosen skills: ui-testing, test-integrity, implementation-integrity, output-readability-gate, documentation-sync", resultText, StringComparison.Ordinal);
        Assert.Contains("Rejected tempting skills: git-automation because commit is forbidden", resultText, StringComparison.Ordinal);
        Assert.Contains("external-ai-tooling because external AI is forbidden", resultText, StringComparison.Ordinal);
        Assert.Contains("Candidate-position scaffold warning", resultText, StringComparison.Ordinal);
        Assert.Contains("candidate position is not final semantic authority", resultText, StringComparison.Ordinal);
        Assert.Contains("Candidate-position scaffold is not success proof", resultText, StringComparison.Ordinal);
        Assert.DoesNotContain(XiigenExtensionApiContract.SkillSelectionMethod, resultText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawTaskPlan, resultText, StringComparison.Ordinal);
        Assert.DoesNotContain("redacted_task_plan_ref", resultText, StringComparison.Ordinal);
        Assert.DoesNotContain("output_fields", resultText, StringComparison.Ordinal);

        Assert.Contains(scenarioId, rawJsonText, StringComparison.Ordinal);
        Assert.Contains(XiigenExtensionApiContract.SkillSelectionMethod, rawJsonText, StringComparison.Ordinal);
        Assert.Contains("redacted_task_plan_ref:skill-selection-q8-02", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("chosen_skills", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("rejected_tempting_skills", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("candidate_position_scaffold_warning", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("dashboard_api_only_not_model_quality_not_host_runtime", rawJsonText, StringComparison.Ordinal);
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "scenario_id", scenarioId);
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "chosen_skills", "ui-testing;test-integrity;implementation-integrity;output-readability-gate;documentation-sync");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "rejected_tempting_skills", "git-automation:commit_forbidden;external-ai-tooling:external_ai_forbidden;broad-test-runner:focused_verification_only");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "candidate_position_scaffold_warning", "candidate_position_scaffold_not_final_authority");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "candidate_position_final_authority", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "candidate_position_success_proof_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "raw_private_task_plan_copied_as_training_evidence", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "semantic_quality_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "trained_model_proof_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "host_runtime_claimed", "false");
        Assert.DoesNotContain(rawTaskPlan, rawJsonText, StringComparison.Ordinal);

        Assert.Contains($"xiigen {XiigenExtensionApiContract.SkillSelectionMethod} :", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("input: redacted_task_plan_ref:skill-selection-q8-02", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("output: output_fields:chosen_skills,rejected_tempting_skills,candidate_position_scaffold_warning,candidate_position_final_authority=false,candidate_position_success_proof_claimed=false", debugTraceText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawTaskPlan, debugTraceText, StringComparison.Ordinal);

        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("candidate_position_scaffold_not_final_authority", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("candidate_position_success_proof_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("trained_model_proof_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("host/runtime=false", StringComparison.Ordinal));
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "semantic_quality_claimed"
                && !flag.Claimed);
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "runtime_extension_claimed"
                && !flag.Claimed);
    }

    private static void AssertSkillSelectionScenarioNotReadyOutput(
        string rawTaskPlan,
        string resultText,
        string rawJsonText,
        string debugTraceText,
        ItemsControl warningsItems,
        ItemsControl boundaryItems)
    {
        Assert.Contains("SkillSelection is not ready", resultText, StringComparison.Ordinal);
        Assert.Contains("Missing skill inventory or untrained checkpoint", resultText, StringComparison.Ordinal);
        Assert.Contains("does not silently fall back to candidate position", resultText, StringComparison.Ordinal);
        Assert.Contains("Candidate-position scaffold is not success proof", resultText, StringComparison.Ordinal);
        Assert.Contains("No semantic quality or trained-model proof is claimed", resultText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawTaskPlan, resultText, StringComparison.Ordinal);

        Assert.Contains("skill_selection_inventory_missing_degraded_dashboard_scenario", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("missing_skill_inventory_visible", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("SKILL_SELECTION_INVENTORY_NOT_READY_FOR_SEMANTIC_QUALITY", rawJsonText, StringComparison.Ordinal);
        Assert.Contains("dashboard_api_only_not_model_quality_not_host_runtime", rawJsonText, StringComparison.Ordinal);
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "scenario_id", "dashboard-semantic-quality-skill-selection-1-negative");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "degraded_or_not_ready_visible", "true");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "missing_skill_inventory_visible", "true");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "candidate_position_final_authority", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "candidate_position_success_proof_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "raw_private_task_plan_copied_as_training_evidence", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "semantic_quality_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "trained_model_proof_claimed", "false");
        AssertQ8SemanticScenarioCheckSetValue(rawJsonText, "host_runtime_claimed", "false");
        Assert.DoesNotContain(rawTaskPlan, rawJsonText, StringComparison.Ordinal);

        Assert.Contains($"xiigen {XiigenExtensionApiContract.SkillSelectionMethod} :", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("input: redacted_task_plan_ref:skill-selection-q8-02-negative", debugTraceText, StringComparison.Ordinal);
        Assert.Contains("output: output_fields:not_ready_status,warning_codes,candidate_position_final_authority=false,candidate_position_success_proof_claimed=false", debugTraceText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawTaskPlan, debugTraceText, StringComparison.Ordinal);

        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("missing inventory or untrained checkpoint", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("candidate_position_final_authority=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("candidate_position_success_proof_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("trained_model_proof_claimed=false", StringComparison.Ordinal));
        Assert.Contains(
            warningsItems.Items.Cast<string>(),
            warning => warning.Contains("host/runtime=false", StringComparison.Ordinal));
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "semantic_quality_claimed"
                && !flag.Claimed);
        Assert.Contains(
            boundaryItems.Items.Cast<ApiDashboardBoundaryFlag>(),
            flag => flag.Key == "runtime_extension_claimed"
                && !flag.Claimed);
    }

    private static void AssertQ8ScenarioHistoryAndExport(
        ApiDashboardViewModel viewModel,
        Button exportButton,
        ListBox historyList,
        string expectedMethod,
        string positiveInput,
        string negativeInput,
        string expectedNegativeStatus)
    {
        Assert.Equal(2, historyList.Items.Count);
        var negativeHistory = Assert.IsType<ApiDashboardHistoryEntry>(historyList.Items[0]);
        Assert.False(negativeHistory.Success);
        Assert.Equal(expectedNegativeStatus, negativeHistory.Status);
        Assert.Equal(ComputeSha256(negativeInput), negativeHistory.PromptSha256);
        Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, negativeHistory.EvidenceScope);
        Assert.False(negativeHistory.HostRuntimeProofClaimed);
        Assert.False(negativeHistory.SemanticQualityOrTrainedModelProofClaimed);
        Assert.DoesNotContain(negativeInput, negativeHistory.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(positiveInput, negativeHistory.ToString(), StringComparison.Ordinal);

        var positiveHistory = Assert.IsType<ApiDashboardHistoryEntry>(historyList.Items[1]);
        Assert.True(positiveHistory.Success);
        Assert.Equal(XiigenExtensionApiService.PartialBoundaryStatus, positiveHistory.Status);
        Assert.Equal(ComputeSha256(positiveInput), positiveHistory.PromptSha256);
        Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, positiveHistory.EvidenceScope);
        Assert.False(positiveHistory.HostRuntimeProofClaimed);
        Assert.False(positiveHistory.SemanticQualityOrTrainedModelProofClaimed);
        Assert.DoesNotContain(positiveInput, positiveHistory.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(negativeInput, positiveHistory.ToString(), StringComparison.Ordinal);

        InvokeButton(exportButton);
        Assert.Contains(expectedMethod, viewModel.LastExportText);
        Assert.Contains(ApiDashboardRunner.DashboardEvidenceScope, viewModel.LastExportText);
        Assert.Contains(ComputeSha256(positiveInput), viewModel.LastExportText);
        Assert.Contains(ComputeSha256(negativeInput), viewModel.LastExportText);
        Assert.DoesNotContain(positiveInput, viewModel.LastExportText);
        Assert.DoesNotContain(negativeInput, viewModel.LastExportText);
    }

    private static void AssertQ8SemanticScenarioCheckSetValue(
        string rawJsonText,
        string key,
        string value)
        => Assert.Contains($"\"{key}\": \"{value}\"", rawJsonText, StringComparison.Ordinal);

    private static void AssertPromptToTaskCheckSetValue(
        string rawJsonText,
        string key,
        string value)
        => Assert.Contains($"\"{key}\": \"{value}\"", rawJsonText, StringComparison.Ordinal);

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static ApiDashboardCatalogRow BuildUnsupportedMethodRow()
        => new(
            MethodId: "not-a-real-xiigen-method",
            HumanName: "Unsupported method test hook",
            HumanDescription: "Controlled invalid selection used only by WPF automation.",
            ExampleInputText: "Invalid example",
            InputFieldDescription: "Invalid method input",
            InputFields: [],
            OutputPreview: "No core output expected",
            OutputFields: [],
            Status: "test-only invalid selection",
            SafetyBoundaryNote: "This row must fail before the core boundary is called.",
            IsPrimaryUserFacingAction: false,
            ClaimsExternalAi: false,
            ClaimsRuntimeExtensionProof: false,
            FixtureExamples: []);

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
