// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;
using AgentInfrastructure;
using SC29 = SemanticCompiler29;

namespace LlmFromScratch.Tests;

// 14.ContinuousLearning — LoRA per-project specialisation, E28 hypothesis test.
// Gate B: dotnet test --filter "ContinuousLearning" → 22 passed, 0 failed
// Acceptance: grep -rn "ISemanticCompiler" 14.ContinuousLearning/ → ≥ 1 line
public class ContinuousLearningTests
{
    private static readonly ISessionStorage Storage = new NullSessionStorageCL();

    // ── Phase 1: Core types ───────────────────────────────────────────────────

    [Fact]
    public void ContinuousLearning_TrainingExample_StoresDomainAndWeight()
    {
        // Gate B: Domain and Weight round-trip unchanged through record construction.
        var ex = new TrainingExample("fix null", "use ?.", "BUG", 1.5, DateTimeOffset.UtcNow);

        Assert.Equal("BUG", ex.Domain);
        Assert.Equal(1.5, ex.Weight);
    }

    [Fact]
    public void ContinuousLearning_DpoPair_StoresChosenAndRejected()
    {
        // Gate B: Chosen and Rejected fields preserved exactly as given.
        var pair = new DpoPair("prompt", "good", "bad", "CODE_Q", DateTimeOffset.UtcNow);

        Assert.Equal("good", pair.Chosen);
        Assert.Equal("bad", pair.Rejected);
    }

    [Fact]
    public void ContinuousLearning_FeedbackSignal_HasThreeValues()
    {
        // Gate B: enum has exactly 3 values — Accepted, Edited, Rejected.
        Assert.Equal(3, Enum.GetValues<FeedbackSignal>().Length);
    }

    // ── Phase 2: ProjectIndexer ───────────────────────────────────────────────

    [Fact]
    public void ProjectIndexer_EmptyDirectory_ReturnsEmpty()
    {
        // Gate B: no files listed → IsSuccess=true, empty Value.
        var indexer = new ProjectIndexer(Storage, new StubSC29CompilerCL(SC29.Domain.CODE_Q), new EmptyCodeReaderCL());
        var result  = indexer.Index("/repo");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public void ProjectIndexer_CSharpFile_ProducesExampleWithCodeQDomain()
    {
        // Gate B: one .cs file → one example with domain CODE_Q from stub compiler.
        var reader  = new SingleFileCodeReaderCL("UserService.cs", "public class UserService { }");
        var indexer = new ProjectIndexer(Storage, new StubSC29CompilerCL(SC29.Domain.CODE_Q), reader);
        var result  = indexer.Index("/repo");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("CODE_Q", result.Value![0].Domain);
    }

    [Fact]
    public void ProjectIndexer_ISemanticCompilerInjected_NotCopied()
    {
        // Gate B: constructor has an ISemanticCompiler29 parameter — no reimplementation.
        var ctor       = typeof(ProjectIndexer).GetConstructors()[0];
        var hasCompiler = ctor.GetParameters()
            .Any(p => p.ParameterType == typeof(SC29.ISemanticCompiler29));

        Assert.True(hasCompiler, "ProjectIndexer must accept ISemanticCompiler29 via constructor.");
    }

    // ── Phase 2: DomainDatasetCache ───────────────────────────────────────────

    [Fact]
    public void DomainDatasetCache_Add_IncrementsTotalCount()
    {
        // Gate B: adding one example makes TotalCount return 1.
        var cache = BuildCache();
        var ex    = new TrainingExample("fix null", "if (x == null)", "BUG", 1.0, DateTimeOffset.UtcNow);

        cache.Add(ex);

        Assert.Equal(1, cache.TotalCount().Value!);
    }

    [Fact]
    public void DomainDatasetCache_GetByDomain_ReturnsOnlyMatchingDomain()
    {
        // Gate B: GetByDomain("BUG") returns only BUG examples, not CODE_Q.
        var cache = BuildCache();
        cache.Add(new TrainingExample("p1", "r1", "BUG",    1.0, DateTimeOffset.UtcNow));
        cache.Add(new TrainingExample("p2", "r2", "CODE_Q", 1.0, DateTimeOffset.UtcNow));

        var bugResults = cache.GetByDomain("BUG").Value!;

        Assert.Single(bugResults);
        Assert.Equal("BUG", bugResults[0].Domain);
    }

    [Fact]
    public void DomainDatasetCache_TotalCount_SumsAcrossDomains()
    {
        // Gate B: 2 BUG + 1 CODE_Q = total 3.
        var cache = BuildCache();
        cache.Add(new TrainingExample("p1", "r1", "BUG",    1.0, DateTimeOffset.UtcNow));
        cache.Add(new TrainingExample("p2", "r2", "BUG",    1.0, DateTimeOffset.UtcNow));
        cache.Add(new TrainingExample("p3", "r3", "CODE_Q", 1.0, DateTimeOffset.UtcNow));

        Assert.Equal(3, cache.TotalCount().Value!);
    }

    // ── Phase 3: FeedbackRouter ───────────────────────────────────────────────

    [Fact]
    public void FeedbackRouter_Accepted_AddsPositiveExample()
    {
        // Gate B: ACCEPTED signal → cache gains one example.
        var cache  = BuildCache();
        var router = new FeedbackRouter(Storage, cache);
        var ev     = new FeedbackEvent("fix null", "if (x == null) throw", null, FeedbackSignal.Accepted, "CODE_Q");

        router.Route(ev);

        Assert.Equal(1, cache.TotalCount().Value!);
    }

    [Fact]
    public void FeedbackRouter_Edited_CreatesDpoPair_WithUserCorrectionAsChosen()
    {
        // Gate B: EDITED signal → DPO pair with UserCorrection as Chosen.
        var cache  = BuildCache();
        var router = new FeedbackRouter(Storage, cache);
        var ev     = new FeedbackEvent("fix null", "return null;", "return Optional.empty();", FeedbackSignal.Edited, "CODE_Q");

        router.Route(ev);
        var pairs = router.GetDpoPairs().Value!;

        Assert.Single(pairs);
        Assert.Equal("return Optional.empty();", pairs[0].Chosen);
        Assert.Equal("return null;", pairs[0].Rejected);
    }

    [Fact]
    public void FeedbackRouter_Rejected_CreatesDpoPair_WithNoPreferredResponse()
    {
        // Gate B: REJECTED signal → DPO pair with "[no preferred response]" as Chosen.
        var cache  = BuildCache();
        var router = new FeedbackRouter(Storage, cache);
        var ev     = new FeedbackEvent("fix null", "return null;", null, FeedbackSignal.Rejected, "BUG");

        router.Route(ev);
        var pairs = router.GetDpoPairs().Value!;

        Assert.Single(pairs);
        Assert.Equal("[no preferred response]", pairs[0].Chosen);
    }

    [Fact]
    public void FeedbackRouter_AcceptedSignalsOnly_DoNotTriggerLearningEventScheduler()
    {
        // Gate B: ACCEPTED signals create positive examples, not DPO pairs.
        // The idle scheduler receives DPO count, so accepted-only feedback must not queue training.
        var cache = BuildCache();
        var router = new FeedbackRouter(Storage, cache);
        var scheduler = new LearningEventScheduler(() => true, minimumExamples: 5);

        for (int i = 0; i < 5; i++)
        {
            var routed = router.Route(new FeedbackEvent(
                $"accepted prompt {i}",
                $"accepted response {i}",
                null,
                FeedbackSignal.Accepted,
                "BUG"));
            Assert.True(routed.IsSuccess, routed.ErrorMessage);
        }

        var dpoPairs = router.GetDpoPairs();
        Assert.True(dpoPairs.IsSuccess, dpoPairs.ErrorMessage);
        Assert.Empty(dpoPairs.Value!);
        Assert.Equal(5, cache.TotalCount().Value!);

        var queued = scheduler.QueueIfReady("BUG", dpoPairs.Value!.Count, "accepted feedback only");

        Assert.True(queued.IsSuccess, queued.ErrorMessage);
        Assert.False(queued.Value);
        Assert.Equal(0, scheduler.QueuedCount);
    }

    [Fact]
    public void FeedbackRouter_ComputeMaxWeight_Formula_1000_5_50_Returns4()
    {
        // Gate B (D-09): 1000 examples / (5 domains × 50 rare) = 4.0 — not a hardcoded ×10.
        Assert.Equal(4.0, FeedbackRouter.ComputeMaxWeight(1000, 5, 50));
    }

    [Fact]
    public void FeedbackRouter_ComputeMaxWeight_ZeroRare_Returns1()
    {
        // Gate B: rareExamples=0 guard → returns 1.0 to avoid division by zero.
        Assert.Equal(1.0, FeedbackRouter.ComputeMaxWeight(100, 5, 0));
    }

    // ── Phase 4: IncrementalTrainer ───────────────────────────────────────────

    [Fact]
    public async Task IncrementalTrainer_EmptyExamples_ReturnsFailure()
    {
        // Gate B: zero examples → Fail with EMPTY_EXAMPLES code before reaching LoRA.
        var trainer = new IncrementalTrainer(Storage);
        var result  = await trainer.TrainAsync("CODE_Q", [], []);

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_EXAMPLES", result.ErrorCode);
    }

    [Fact]
    public void IncrementalTrainer_DefaultConfig_RankIsEight()
    {
        // Gate B: Rank=8 matching existing 10.LoRA experiment baseline.
        Assert.Equal(8, IncrementalTrainer.DefaultConfig.Rank);
    }

    [Fact]
    public void IncrementalTrainer_DefaultConfig_TargetModulesContainQProj()
    {
        // Gate B: q_proj in TargetModules — delegates to LoraApplicator which matches this name.
        Assert.Contains("q_proj", IncrementalTrainer.DefaultConfig.TargetModules);
    }

    [Fact]
    public void IncrementalTrainer_UsesLoraApplicator_NotCustomGradients()
    {
        // Gate B: ContinuousLearning assembly references LoRA — proves delegation,
        // not reimplementation of gradient logic.
        var refs = typeof(IncrementalTrainer).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? "");
        Assert.Contains(refs, n => n.Equals("LoRA", StringComparison.OrdinalIgnoreCase));
    }

    // ── Phase 5: DomainModelRegistry ─────────────────────────────────────────

    [Fact]
    public void DomainModelRegistry_Register_StoresAdapter()
    {
        // Gate B: Register then GetAdapter returns the same domain.
        var reg     = BuildRegistry();
        var adapter = new ModelAdapter("BUG", "adapters/bug.lora", 120, DateTimeOffset.UtcNow);

        reg.Register(adapter);
        var result = reg.GetAdapter("BUG");

        Assert.True(result.IsSuccess);
        Assert.Equal("BUG", result.Value!.Domain);
    }

    [Fact]
    public void DomainModelRegistry_GetAdapter_UnknownDomain_ReturnsNull()
    {
        // Gate B: missing domain → IsSuccess=true, Value is null (not Fail).
        var reg    = BuildRegistry();
        var result = reg.GetAdapter("UNKNOWN_DOMAIN");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void DomainModelRegistry_ListAll_ReturnsAllRegistered()
    {
        // Gate B: 3 registered adapters → ListAll returns count 3.
        var reg = BuildRegistry();
        reg.Register(new ModelAdapter("BUG",      "a/bug.lora",      10, DateTimeOffset.UtcNow));
        reg.Register(new ModelAdapter("CODE_Q",   "a/codeq.lora",    20, DateTimeOffset.UtcNow));
        reg.Register(new ModelAdapter("REFACTOR", "a/refactor.lora", 30, DateTimeOffset.UtcNow));

        Assert.Equal(3, reg.ListAll().Value!.Count);
    }

    // ── Phase 5: IContinuousLearningPipeline interface ────────────────────────

    [Fact]
    public void ContinuousLearning_IContinuousLearningPipeline_InterfaceExists()
    {
        // Gate B: IContinuousLearningPipeline declared in Fabric/Interfaces.cs for 16.AutonomousLearning.
        var type = typeof(IContinuousLearningPipeline);
        Assert.NotNull(type);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DomainDatasetCache BuildCache()
        => new(Storage, new NullCodeWriterCL(), "test-cache");

    private static DomainModelRegistry BuildRegistry()
        => new(Storage, new NullCodeWriterCL(), "test-adapters");
}

// ── File-scoped fakes ─────────────────────────────────────────────────────────

file sealed class NullSessionStorageCL : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)Array.Empty<string>()));
}

// ICodeWriter no-op — FlushAsync and registry persist calls succeed silently.
file sealed class NullCodeWriterCL : ICodeWriter
{
    public Task<OperationResult<bool>> WriteFileAsync(string path, string content)
        => Task.FromResult(OperationResult<bool>.Ok(true));
}

// ISemanticCompiler29 stub: returns a configurable domain and default ratio.
file sealed class StubSC29CompilerCL : SC29.ISemanticCompiler29
{
    private readonly SC29.Domain _domain;

    public StubSC29CompilerCL(SC29.Domain domain) => _domain = domain;

    public SC29.OperationResult<SC29.PipelineOutput> Compile(SC29.PipelineInput input)
        => SC29.OperationResult<SC29.PipelineOutput>.Ok(
               new SC29.PipelineOutput(_domain, [], "csharp", 15.0));
}

// ICodeReader stub that returns an empty file list.
file sealed class EmptyCodeReaderCL : ICodeReader
{
    public Task<OperationResult<IReadOnlyList<string>>> ListFilesAsync(string dir, string pattern)
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)Array.Empty<string>()));

    public Task<OperationResult<string>> ReadFileAsync(string path)
        => Task.FromResult(OperationResult<string>.Fail("Not found.", "NOT_FOUND"));
}

// ICodeReader stub that returns exactly one file and its content.
file sealed class SingleFileCodeReaderCL : ICodeReader
{
    private readonly string _file;
    private readonly string _content;

    public SingleFileCodeReaderCL(string file, string content)
        => (_file, _content) = (file, content);

    public Task<OperationResult<IReadOnlyList<string>>> ListFilesAsync(string dir, string pattern)
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)new[] { _file }));

    public Task<OperationResult<string>> ReadFileAsync(string path)
        => Task.FromResult(path == _file
               ? OperationResult<string>.Ok(_content)
               : OperationResult<string>.Fail("Not found.", "NOT_FOUND"));
}
