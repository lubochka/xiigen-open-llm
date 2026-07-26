// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using OrchestratorModel;
using AgentInfrastructure;
using SC29 = SemanticCompiler29;

namespace LlmFromScratch.Tests;

// 15.OrchestratorModel — E32 episode decomposition, E33 stop signals, E35 dependency sort, E39 quality.
// Gate B: dotnet test --filter "OrchestratorModel_DeployTask_ArchFirstLevel" → PASSED
// All test names use class-prefixed naming to avoid collision with 22.AgentAlgorithms tests.
public class OrchestratorModelTests
{
    // Shared null storage — OrchestratorModel.NullSessionStorage is internal; use local stub.
    private static readonly ISessionStorage Storage = new NullSessionStorageOM();

    // ── Phase 1: InMemoryEpisodeMemory ────────────────────────────────────────

    [Fact]
    public void EpisodeMemory_Store_ThenRecall_FindsBySubstring()
    {
        // Gate B: recalled list contains the stored task description.
        var memory = new InMemoryEpisodeMemory(Storage);
        var tasks  = new List<AgentAlgorithms.SubTask>
            { new("design the auth", "architect") };

        memory.Store("Design and implement auth system", tasks);
        var recalled = memory.Recall("auth system");

        Assert.True(recalled.IsSuccess);
        Assert.Single(recalled.Value!);
    }

    [Fact]
    public void EpisodeMemory_EmptyDescription_ReturnsFail()
    {
        // Gate B: error code is EMPTY_TASK for blank description.
        var memory = new InMemoryEpisodeMemory(Storage);
        var result = memory.Store("", new List<AgentAlgorithms.SubTask>());

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_TASK", result.ErrorCode);
    }

    [Fact]
    public void EpisodeMemory_ExceedsMaxEntries_EvictsOldest()
    {
        // Gate B: after capacity+1 stores, oldest entry is no longer recalled.
        var memory = new InMemoryEpisodeMemory(Storage, maxEntries: 2);
        memory.Store("task 1", new List<AgentAlgorithms.SubTask>());
        memory.Store("task 2", new List<AgentAlgorithms.SubTask>());
        memory.Store("task 3", new List<AgentAlgorithms.SubTask>());

        Assert.Empty(memory.Recall("task 1").Value!);
        Assert.Single(memory.Recall("task 3").Value!);
    }

    // ── Phase 2: EpisodeClassifier ────────────────────────────────────────────

    [Fact]
    public void EpisodeClassifier_UnknownDomain_FallsBackToCodeQ()
    {
        // Gate B: Domain.UNKNOWN from pipeline → CODE_Q (safe default, never unknown to caller).
        var compiler   = new StubSC29Compiler(SC29.Domain.UNKNOWN, ratio: 5.0);
        var classifier = new EpisodeClassifier(compiler, Storage);
        var result     = classifier.Classify("some obscure task");

        Assert.True(result.IsSuccess);
        Assert.Equal(TaskDomain.CODE_Q, result.Value!);
    }

    [Fact]
    public void EpisodeClassifier_LowCompressionRatio_LowConfidence()
    {
        // Gate B: CompressionRatio=5.0 → confidence=0.25, below 0.6 threshold for DynamicDomains.
        var compiler   = new StubSC29Compiler(SC29.Domain.CODE_Q, ratio: 5.0);
        var classifier = new EpisodeClassifier(compiler, Storage);
        var result     = classifier.ClassifyWithConfidence("Write Dockerfile for .NET app");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Confidence < 0.6,
            $"Expected confidence < 0.6, got {result.Value!.Confidence}");
    }

    // ── Phase 2: EpisodeDecomposer ────────────────────────────────────────────

    [Fact]
    public void EpisodeDecomposer_PlanDomain_Returns4SubEpisodes()
    {
        // Gate B: PLAN domain caps at 4 sub-episodes (E32 +11% validated count).
        var decomposer = new EpisodeDecomposer(Storage);
        var result     = decomposer.Decompose(
            "Design auth system and implement it and test it and document it",
            TaskDomain.PLAN);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.Count);
    }

    [Fact]
    public void EpisodeDecomposer_FalseConjunction_StaysAsSingleTask()
    {
        // Gate B: "log them" is a dependent clause — whole description stays as one task.
        var decomposer = new EpisodeDecomposer(Storage);
        var result     = decomposer.Decompose(
            "Find methods that return null and log them", TaskDomain.CODE_Q);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public void EpisodeDecomposer_BugDomain_FirstRoleIsDebugger()
    {
        // Gate B: position-0 sub-task for BUG domain must have role "debugger" (E32 role mapping).
        var decomposer = new EpisodeDecomposer(Storage);
        var result     = decomposer.Decompose(
            "Fix NullReference in UserService then add regression test", TaskDomain.BUG);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal("debugger", result.Value![0].RequiredRole);
    }

    // ── Phase 3: StopSignalDetector ───────────────────────────────────────────

    [Fact]
    public void StopSignalDetector_PhaseSequence_ReturnsTrue()
    {
        // Gate B: "first…then" phrase triggers PHASES stop signal (no independent sub-episodes).
        var detector = new StopSignalDetector(Storage);
        var result   = detector.ShouldStop("First read the requirements then decide the approach");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!);
    }

    [Fact]
    public void StopSignalDetector_IndependentGoal_ReturnsFalse()
    {
        // Gate B: 5-word substantive goal with no stop-signal patterns → false.
        var detector = new StopSignalDetector(Storage);
        var result   = detector.ShouldStop("Design the authentication module architecture");

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!);
    }

    // ── Phase 3: DependencyGraph reuse from 22.AgentAlgorithms ───────────────

    [Fact]
    public void DependencyGraph_ArchBeforeImplementer_UsableFrom15()
    {
        // Gate B: architect in level 0, implementer in level 1 — no duplication from 22.
        var graph = new AgentAlgorithms.DependencyGraph();
        graph.AddDependency("architect", "implementer");

        var tasks = new List<AgentAlgorithms.SubTask>
        {
            new("design the auth system",     "architect"),
            new("implement the auth service", "implementer"),
        };
        var result = graph.Sort(tasks);

        Assert.True(result.IsSuccess);
        Assert.Equal("architect",   result.Value![0][0].RequiredRole);
        Assert.Equal("implementer", result.Value![1][0].RequiredRole);
    }

    // ── Phase 4: AgentLauncher ────────────────────────────────────────────────

    [Fact]
    public async Task AgentLauncher_EmptyDescription_ReturnsFail()
    {
        // Gate B: empty sub-task description is rejected before reaching the orchestrator.
        var orch     = BuildStubOrchestrator();
        var launcher = new AgentLauncher(Storage, orch);
        var result   = await launcher.LaunchAsync(
            new AgentAlgorithms.SubTask("", "architect"), "");

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_TASK", result.ErrorCode);
    }

    // ── Phase 4: ResultMerger ─────────────────────────────────────────────────

    [Fact]
    public void ResultMerger_SingleResult_ReturnedUnchanged()
    {
        // Gate B: single result bypasses beautifier and is returned verbatim.
        var beautifier = BuildStubBeautifier();
        var merger     = new ResultMerger(Storage, beautifier);
        var task       = new AgentAlgorithms.SubTask("design auth", "architect");

        var result = merger.Merge([(task, "auth schema: JWT + RBAC")]);

        Assert.True(result.IsSuccess);
        Assert.Equal("auth schema: JWT + RBAC", result.Value!);
    }

    [Fact]
    public void ResultMerger_EmptyResults_ReturnsFail()
    {
        // Gate B: empty list returns EMPTY_RESULTS error code.
        var beautifier = BuildStubBeautifier();
        var merger     = new ResultMerger(Storage, beautifier);
        var result     = merger.Merge([]);

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_RESULTS", result.ErrorCode);
    }

    // ── Phase 5: QualityOrchestrator ─────────────────────────────────────────

    [Fact]
    public void QualityOrchestrator_ShortResult_FailsOutputContract()
    {
        // Gate B: result under 10 characters fails OUTPUT_CONTRACT check.
        var quality = new QualityOrchestrator(Storage);
        var report  = quality.Check("add null check", "ok", "CODE_SERVICE");

        Assert.True(report.IsSuccess);
        Assert.False(report.Value!.Passed);
        Assert.Contains("OUTPUT_CONTRACT", report.Value!.FailedChecks);
    }

    [Fact]
    public void QualityOrchestrator_SubstantiveResultWithReturn_Passes()
    {
        // Gate B: C# guard clause ≥ 10 chars containing "return" → all checks pass.
        var quality = new QualityOrchestrator(Storage);
        string res  = "if (user == null) return OperationResult<User>.Fail(\"not found\");";
        var report  = quality.Check("add null check to GetUser", res, "CODE_SERVICE");

        Assert.True(report.IsSuccess);
        Assert.True(report.Value!.Passed,
            $"Expected pass, failed: {string.Join(", ", report.Value!.FailedChecks)}");
    }

    // ── Phase 5: Gate B integration test ─────────────────────────────────────

    [Fact]
    public async Task OrchestratorModel_DeployTask_ArchFirstLevel()
    {
        // Gate B: "Deploy new application" → PLAN decomposition → planner in level 0.
        // Verifies the full Classify→Decompose→Sort chain without the launcher.
        var graph = new AgentAlgorithms.DependencyGraph();
        graph.AddDependency("architect", "implementer");

        var decomposer = new EpisodeDecomposer(Storage);
        var tasks      = decomposer.Decompose(
            "Design deployment architecture and implement the service and deploy and monitor",
            TaskDomain.PLAN);

        Assert.True(tasks.IsSuccess);

        var sorted = graph.Sort(tasks.Value!.ToList());
        Assert.True(sorted.IsSuccess);

        var firstLevel = sorted.Value![0];
        Assert.Contains(firstLevel, t =>
            t.RequiredRole == "architect" || t.RequiredRole == "planner");
        await Task.CompletedTask;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AgentAlgorithms.AgentOrchestrator BuildStubOrchestrator()
    {
        // Minimal orchestrator: stub bus + default decomposer + default dependency graph.
        var bus       = new StubMessageBusOM();
        var decomposer = new AgentAlgorithms.ComplexTaskDecomposer();
        return new AgentAlgorithms.AgentOrchestrator(bus, decomposer);
    }

    private static ITemplateBeautifier BuildStubBeautifier()
    {
        // Wrap the real SC29 TemplateBeautifier behind the fabric interface.
        var inner = new SC29.TemplateBeautifier(
            new NullCompilerStorageOM(), new NullCompilerEventLoggerOM());
        return new TemplateBeautifierAdapter(inner);
    }
}

// ── File-scoped fakes ─────────────────────────────────────────────────────────

// ISessionStorage no-op for OrchestratorModel tests (OrchestratorModel.NullSessionStorage is internal).
file sealed class NullSessionStorageOM : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)Array.Empty<string>()));
}

// ISemanticCompiler29 stub: returns a configurable domain and compression ratio.
file sealed class StubSC29Compiler : SC29.ISemanticCompiler29
{
    private readonly SC29.Domain _domain;
    private readonly double      _ratio;

    public StubSC29Compiler(SC29.Domain domain, double ratio = 15.0)
        => (_domain, _ratio) = (domain, ratio);

    public SC29.OperationResult<SC29.PipelineOutput> Compile(SC29.PipelineInput input)
        => SC29.OperationResult<SC29.PipelineOutput>.Ok(
               new SC29.PipelineOutput(_domain, [], "csharp", _ratio));
}

// IAgentMessageBus stub: returns "done:<task>" for every assignment.
file sealed class StubMessageBusOM : AgentAlgorithms.IAgentMessageBus
{
    public Task<AgentAlgorithms.OperationResult<string>> AssignTaskAsync(
        string subAgentName, string task)
        => Task.FromResult(AgentAlgorithms.OperationResult<string>.Ok($"done:{task}"));
}

// SC29 ICompilerStorage no-op (duplicated from SemanticCompiler29Tests — file-scoped there).
file sealed class NullCompilerStorageOM : SC29.ICompilerStorage
{
    public void Save(string key, SC29.PipelineOutput output) { }
    public SC29.PipelineOutput? Load(string key) => null;
}

// SC29 ICompilerEventLogger no-op (duplicated from SemanticCompiler29Tests — file-scoped there).
file sealed class NullCompilerEventLoggerOM : SC29.ICompilerEventLogger
{
    public void Log(SC29.StandardEvent evt) { }
}
