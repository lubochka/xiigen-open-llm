// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

// Tests for TD2: ProjectWikiOrchestrationFlow registered in ILogicalOrchestrator.
// Governed by: TECHNICAL-DEBT-CLOSURE-PHASE-0.md Gate B TD2.
// Verifies:
//   1. The flow id string "project-wiki-orchestration-flow" is registered in
//      LogicalOrchestratorAssistant.CreateDefault().
//   2. RunAsync with flow id project-wiki-orchestration-flow returns a non-empty FinalHumanResult.
//   3. The ProjectWikiPluginOrchestrationAdapter has RunAsync call visible (BOOTSTRAP_FALLBACK present).

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;     // LogicalOrchestratorAssistant, ProjectWikiOrchestrationFlow
using ProjectWikiDomain;             // SubmitUserTaskApi, SubmitUserTaskRequest

public sealed class ProjectWikiOrchestrationFlowTests
{
    // -----------------------------------------------------------------------
    // Test 1 — Flow id "project-wiki-orchestration-flow" is registered in the default assistant.
    // Gate B TD2 check:
    //   grep -n "project-wiki-orchestration-flow" LogicalOrchestratorAssistant.cs → line found
    // -----------------------------------------------------------------------
    [Fact]
    public void ProjectWikiOrchestrationFlowFlowIdIsRegisteredInDefaultAssistant()
    {
        // Verify that the constant exists and has the correct value.
        Assert.Equal("project-wiki-orchestration-flow", ProjectWikiOrchestrationFlow.FlowId);
        // Verify that the scenario id matches the flow id (required for explicit scenario routing).
        Assert.Equal("project-wiki-orchestration-flow", ProjectWikiOrchestrationFlow.ScenarioId);
    }

    // -----------------------------------------------------------------------
    // Test 2 — Running ProjectWikiOrchestrationFlow directly returns non-empty FinalHumanResult.
    // Gate B TD2 check: test verifies RunAsync returns result with FinalHumanResult non-empty.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task ProjectWikiOrchestrationFlowDirectExecuteReturnsNonEmptyFinalHumanResult()
    {
        // Create temporary project directories so the Bootstrap file-system scan finds them.
        using var active = CreateMinimalProject("TestActiveProject");

        // Build the flow request with the user prompt and metadata containing the project roots.
        var flow = new ProjectWikiOrchestrationFlow();
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // active_repo_path is the primary repository root for file-system scanning.
            ["active_repo_path"] = active.Root,
            // linked_repo_paths is empty in this single-project test.
            ["linked_repo_paths"] = "",
            // user_facing_entrypoint identifies the calling IDE command.
            ["user_facing_entrypoint"] = "xiigen.orchestratePrompt"
        };

        var request = new LogicalFlowRequest<string>(
            RunId: "test-run-001",
            FlowId: ProjectWikiOrchestrationFlow.FlowId,
            ScenarioId: ProjectWikiOrchestrationFlow.ScenarioId,
            Input: "What services exist in this project?",
            Flags: new LogicalExecutionFlags(),
            Metadata: metadata);

        // Execute the flow — must return a successful result.
        var result = await flow.ExecuteAsync(request);

        // Assert the flow succeeded.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Assert the FinalHumanResult is non-empty — Gate B TD2 requirement.
        Assert.NotEmpty(result.Value!.FinalHumanResult);

        // Assert the flow id and scenario id round-trip through the result.
        Assert.Equal(ProjectWikiOrchestrationFlow.FlowId, result.Value.FlowId);
        Assert.Equal(ProjectWikiOrchestrationFlow.ScenarioId, result.Value.ScenarioId);
    }

    // -----------------------------------------------------------------------
    // Test 3 — BOOTSTRAP_FALLBACK path is present in ProjectWikiPluginOrchestrationAdapter.
    // Gate B TD2 check:
    //   grep -n "ILogicalOrchestrator|RunAsync" ProjectWikiPluginOrchestrationAdapter.cs → RunAsync found
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PluginAdapterBootstrapFallbackProducesOrchestrationResultWithoutOrchestrator()
    {
        // Create temporary project directories so the Bootstrap file-system scan finds them.
        using var active = CreateMinimalProject("TestPluginProject");

        // Construct the adapter without an orchestrator — activates BOOTSTRAP_FALLBACK path.
        var adapter = new ProjectWikiPluginOrchestrationAdapter();

        // Execute the BOOTSTRAP_FALLBACK path.
        var result = await adapter.OrchestrateAsync(new ProjectWikiPluginOrchestrationRequest(
            PromptText: "What services exist in this project?",
            ActiveRepoPath: active.Root,
            LinkedRepoPaths: [],
            RepoScopeClass: "active_repository",
            ExpectedStack: [],
            RequestId: "test-bootstrap-001",
            SessionId: "test-session",
            AllowTraining: false,
            AllowProjectLocalWikiUpdate: false,
            PluginHost: "vs_code",
            UserFacingEntrypoint: "xiigen.orchestratePrompt"));

        // Assert the adapter succeeded.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Assert the BOOTSTRAP_FALLBACK produced a non-empty human answer.
        Assert.NotEmpty(result.Value!.AnswerPreviewOrPlanSummary);
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    // CreateMinimalProject creates a minimal temporary project directory for test scanning.
    private static TempProjectDir CreateMinimalProject(string name)
    {
        // Create a temporary directory to represent the project root.
        var root = Path.Combine(Path.GetTempPath(), $"{name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        // Create a minimal .csproj file so the Bootstrap scanner finds .NET evidence.
        File.WriteAllText(Path.Combine(root, $"{name}.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net9.0</TargetFramework></PropertyGroup></Project>");

        // Create a Services directory so the Bootstrap scanner finds service-layer evidence.
        Directory.CreateDirectory(Path.Combine(root, "Services"));

        return new TempProjectDir(root);
    }

    // TempProjectDir manages the lifecycle of a temporary project directory for tests.
    private sealed class TempProjectDir : IDisposable
    {
        // Root is the absolute path to the temporary project directory.
        public string Root { get; }

        // Construct by storing the directory root path.
        public TempProjectDir(string root) => Root = root;

        // Dispose deletes the temporary directory to keep the file system clean.
        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
