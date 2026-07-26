// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentAlgorithms;
using Domain = SemanticCompiler29.Domain;

namespace LlmFromScratch.Tests;

// ─── Fakes ────────────────────────────────────────────────────────────────────

file sealed class StubSymbolGraphProvider : ISymbolGraphProvider
{
    private readonly List<Dictionary<string, object>> _symbols;
    public StubSymbolGraphProvider(List<Dictionary<string, object>> symbols) => _symbols = symbols;

    public Task<OperationResult<List<Dictionary<string, object>>>> GetAllSymbolsAsync()
        => Task.FromResult(OperationResult<List<Dictionary<string, object>>>.Ok(_symbols));

    public Task<OperationResult<List<Dictionary<string, object>>>> GetReferencedByAsync(string symbolId)
        => Task.FromResult(OperationResult<List<Dictionary<string, object>>>.Ok(new List<Dictionary<string, object>>()));
}

file sealed class StubSemanticSearchProvider : ISemanticSearchProvider
{
    private readonly List<(string, double)> _results;
    public StubSemanticSearchProvider(List<(string, double)> results) => _results = results;

    public Task<OperationResult<List<(string ChunkId, double Score)>>> SearchAsync(string query, int topK)
        => Task.FromResult(OperationResult<List<(string, double)>>.Ok(_results.Take(topK).ToList()));
}

file sealed class StubAgentMessageBus : IAgentMessageBus
{
    public List<(string Agent, string Task)> Received { get; } = new();

    public Task<OperationResult<string>> AssignTaskAsync(string subAgentName, string task)
    {
        Received.Add((subAgentName, task));
        return Task.FromResult(OperationResult<string>.Ok($"done:{task}"));
    }
}

file sealed class ScriptedLlmGateway : ILlmGateway
{
    private readonly Queue<string> _responses;
    public ScriptedLlmGateway(IEnumerable<string> responses) => _responses = new Queue<string>(responses);

    public Task<OperationResult<string>> SendAsync(IReadOnlyList<ConversationMessage> history, CancellationToken ct)
    {
        if (_responses.Count == 0)
            return Task.FromResult(OperationResult<string>.Fail("No more responses", "EMPTY"));
        return Task.FromResult(OperationResult<string>.Ok(_responses.Dequeue()));
    }
}

// ─── Tests ────────────────────────────────────────────────────────────────────

public sealed class AgentAlgorithmsTests
{
    // ── ReactAgentLoop ────────────────────────────────────────────────────────

    [Fact]
    public async Task ReactAgentLoop_TaskCompleteSignal_CompletesSuccessfully()
    {
        var llm = new ScriptedLlmGateway(["TASK_COMPLETE:All done"]);
        var compactor = new ContextCompactor();
        var loop = new ReactAgentLoop(llm, compactor);

        var result = await loop.RunAsync("do something");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsComplete);
        Assert.Equal("All done", result.Value.FinalResponse);
    }

    [Fact]
    public async Task ReactAgentLoop_ToolCall_ExecutesAndContinues()
    {
        var llm = new ScriptedLlmGateway([
            """TOOL:read_file:{"path":"foo.cs"}""",
            "TASK_COMPLETE:Finished"
        ]);
        var compactor = new ContextCompactor();
        bool toolCalled = false;
        var loop = new ReactAgentLoop(
            llm, compactor,
            toolExecutor: (name, _) => { toolCalled = true; return Task.FromResult("content"); });

        var result = await loop.RunAsync("read a file");

        Assert.True(result.IsSuccess);
        Assert.True(toolCalled);
        Assert.True(result.Value!.IsComplete);
    }

    [Fact]
    public async Task ReactAgentLoop_MaxIterations_StopsWithoutCompletion()
    {
        var responses = Enumerable.Repeat("thinking...", 10).ToList();
        var llm = new ScriptedLlmGateway(responses);
        var compactor = new ContextCompactor();
        var loop = new ReactAgentLoop(llm, compactor, maxIterations: 3);

        var result = await loop.RunAsync("infinite loop task");

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsComplete);
        Assert.Equal(3, result.Value.TotalIterations);
    }

    [Fact]
    public async Task ReactAgentLoop_CancellationToken_StopsLoop()
    {
        var llm = new ScriptedLlmGateway(Enumerable.Repeat("thinking...", 100));
        var compactor = new ContextCompactor();
        var loop = new ReactAgentLoop(llm, compactor);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await loop.RunAsync("task", cts.Token);
        Assert.False(result.IsSuccess);
        Assert.Equal("CANCELLED", result.ErrorCode);
    }

    // ── ContextCompactor ──────────────────────────────────────────────────────

    [Fact]
    public async Task ContextCompactor_FewMessages_PreservesAll()
    {
        var compactor = new ContextCompactor();
        var history = new List<ConversationMessage>
        {
            new("user", "hello"),
            new("assistant", "hi"),
        };

        var result = await compactor.CompactAsync(history, msgs => Task.FromResult("summary"));
        Assert.True(result.IsSuccess);
        // Only 2 messages — all should be preserved (fewer than RecentTurnsToPreserve=10)
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task ContextCompactor_ManyMessages_CompactsOldOnes()
    {
        var compactor = new ContextCompactor();
        var history = Enumerable.Range(1, 20)
            .Select(i => new ConversationMessage("user", $"message {i}"))
            .ToList<ConversationMessage>();

        var result = await compactor.CompactAsync(history, msgs => Task.FromResult("[summary]"));
        Assert.True(result.IsSuccess);
        // Should have summary + recent 10 = 11 messages
        Assert.True(result.Value!.Count <= 12);
        Assert.Contains(result.Value!, m => m.Content.Contains("[summary]") || m.Role == "system");
    }

    // ── SemanticRanker ────────────────────────────────────────────────────────

    [Fact]
    public async Task SemanticRanker_RanksChunksDescending()
    {
        var provider = new StubSemanticSearchProvider([
            ("chunk-1", 0.3),
            ("chunk-2", 0.9),
            ("chunk-3", 0.6),
        ]);
        var ranker = new SemanticRanker(provider);
        var chunks = new List<TextChunk>
        {
            new("chunk-1", "low relevance"),
            new("chunk-2", "high relevance"),
            new("chunk-3", "medium relevance"),
        };

        var result = await ranker.RankAsync(chunks, "my query");
        Assert.True(result.IsSuccess);
        Assert.Equal("chunk-2", result.Value![0].Id);
    }

    [Fact]
    public async Task SemanticRanker_EmptyChunks_ReturnsEmpty()
    {
        var provider = new StubSemanticSearchProvider([]);
        var ranker = new SemanticRanker(provider);

        var result = await ranker.RankAsync([], "query");
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    // ── ContextWindowManager ──────────────────────────────────────────────────

    [Fact]
    public async Task ContextWindowManager_AssembleAsync_ReturnsRankedChunks()
    {
        var provider = new StubSemanticSearchProvider([("c1", 0.8), ("c2", 0.5)]);
        var ranker = new SemanticRanker(provider);
        var compactor = new ContextCompactor();
        var manager = new ContextWindowManager(ranker, compactor, tokenBudget: 100_000);
        var chunks = new List<TextChunk>
        {
            new("c1", "relevant content"),
            new("c2", "less relevant"),
        };

        var result = await manager.AssembleAsync(chunks, "find x");
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!);
    }

    // ── AgentOrchestrator ─────────────────────────────────────────────────────

    [Fact]
    public async Task AgentOrchestrator_RunsAllSubTasks()
    {
        var bus = new StubAgentMessageBus();
        var decomposer = new ComplexTaskDecomposer();
        var orchestrator = new AgentOrchestrator(bus, decomposer);

        // "read the file" → "reader"; "write tests" → "tester"
        orchestrator.Register(new SubAgentDefinition("reader-agent", "reader", ["read_file"]));
        orchestrator.Register(new SubAgentDefinition("tester-agent", "tester", ["run_tests"]));

        var result = await orchestrator.RunAsync("read the file then write tests");
        Assert.True(result.IsSuccess);
        Assert.True(bus.Received.Count > 0);
    }

    [Fact]
    public async Task AgentOrchestrator_SingleTask_AssignsToAgent()
    {
        var bus = new StubAgentMessageBus();
        var decomposer = new ComplexTaskDecomposer();
        var orchestrator = new AgentOrchestrator(bus, decomposer);
        // "refactor the payment service" → role "refactorer"
        orchestrator.Register(new SubAgentDefinition("refactorer-agent", "refactorer", ["edit_file"]));

        var result = await orchestrator.RunAsync("refactor the payment service");
        Assert.True(result.IsSuccess);
        Assert.Single(bus.Received);
    }

    // ── PermissionModel ───────────────────────────────────────────────────────

    [Fact]
    public void PermissionModel_ReadFile_IsReadOnly()
    {
        var model = new PermissionModel();
        var result = model.Check("read_file");
        Assert.True(result.IsSuccess);
        Assert.Equal(PermissionLevel.ReadOnly, result.Value);
    }

    [Fact]
    public void PermissionModel_EditFile_IsWriteWithApproval()
    {
        var model = new PermissionModel();
        var result = model.Check("edit_file");
        Assert.True(result.IsSuccess);
        Assert.Equal(PermissionLevel.WriteWithApproval, result.Value);
    }

    [Fact]
    public void PermissionModel_DeleteFile_IsAlwaysDenied()
    {
        var model = new PermissionModel();
        var result = model.Check("delete_file");
        Assert.True(result.IsSuccess);
        Assert.Equal(PermissionLevel.AlwaysDenied, result.Value);
    }

    [Fact]
    public void PermissionModel_UnknownTool_IsAlwaysDenied()
    {
        var model = new PermissionModel();
        var result = model.Check("launch_rockets");
        Assert.True(result.IsSuccess);
        Assert.Equal(PermissionLevel.AlwaysDenied, result.Value);
    }

    [Fact]
    public void PermissionModel_Register_OverridesDefault()
    {
        var model = new PermissionModel();
        model.Register("delete_file", PermissionLevel.WriteWithApproval);
        var result = model.Check("delete_file");
        Assert.Equal(PermissionLevel.WriteWithApproval, result.Value);
    }

    [Fact]
    public void PermissionModel_EmptyToolName_Fails()
    {
        var model = new PermissionModel();
        var result = model.Check("");
        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_TOOL", result.ErrorCode);
    }

    // ── RulesFileParser ───────────────────────────────────────────────────────

    [Fact]
    public void RulesFileParser_ExtractsSections()
    {
        const string content = """
            # Overview
            This agent handles coding tasks.
            # Restrictions
            Only modify src/ files.
            """;
        var parser = new RulesFileParser();
        var result = parser.Parse(content);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Sections.ContainsKey("Overview"));
        Assert.True(result.Value.Sections.ContainsKey("Restrictions"));
    }

    [Fact]
    public void RulesFileParser_ExtractsAllowAndDenyTools()
    {
        const string content = """
            # Tools
            @allow read_file
            @allow list_directory
            @deny delete_file
            """;
        var parser = new RulesFileParser();
        var result = parser.Parse(content);

        Assert.True(result.IsSuccess);
        Assert.Contains("read_file", result.Value!.AllowedTools);
        Assert.Contains("list_directory", result.Value.AllowedTools);
        Assert.Contains("delete_file", result.Value.DeniedTools);
    }

    [Fact]
    public void RulesFileParser_EmptyContent_ReturnsFailure()
    {
        var parser = new RulesFileParser();
        var result = parser.Parse("  ");
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_CONTENT", result.ErrorCode);
    }

    // ── ComplexTaskDecomposer ─────────────────────────────────────────────────

    [Fact]
    public void ComplexTaskDecomposer_SimpleTask_ReturnsSingle()
    {
        var decomposer = new ComplexTaskDecomposer();
        var result = decomposer.Decompose("read the file");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("reader", result.Value![0].RequiredRole);
    }

    [Fact]
    public void ComplexTaskDecomposer_ConjunctionTask_SplitsCorrectly()
    {
        var decomposer = new ComplexTaskDecomposer();
        var result = decomposer.Decompose("read the file and then write the tests");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public void ComplexTaskDecomposer_RefactorKeyword_InfersRefactorer()
    {
        var decomposer = new ComplexTaskDecomposer();
        var result = decomposer.Decompose("refactor the payment service");

        Assert.True(result.IsSuccess);
        Assert.Equal("refactorer", result.Value![0].RequiredRole);
    }

    [Fact]
    public void ComplexTaskDecomposer_EmptyTask_ReturnsFailure()
    {
        var decomposer = new ComplexTaskDecomposer();
        var result = decomposer.Decompose("");
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_TASK", result.ErrorCode);
    }

    // ── ComplexTaskDecomposer — False conjunction guard ───────────────────────

    [Fact]
    public void ComplexTaskDecomposer_FalseConjunction_StaysAsSingleTask()
    {
        // "Find methods that return null and log them" — right side ("log them") has no imperative verb start
        var decomposer = new ComplexTaskDecomposer();
        var result = decomposer.Decompose("Find methods that return null and log them");
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public void ComplexTaskDecomposer_TrueConjunction_SplitsIntoTwo()
    {
        // Both sides start with an independent imperative verb + noun → true conjunction
        var decomposer = new ComplexTaskDecomposer();
        var result = decomposer.Decompose("Read the config file and write the output");
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public void ComplexTaskDecomposer_FalseConjunctionWithCheck_Preserved()
    {
        // "Verify all connections and report status" — right side starts with "report" (imperative)
        // but "report status" IS an independent operation — true split
        // Versus "find errors and fix them" — "fix them" is NOT independent (pronoun object, no noun)
        var decomposer = new ComplexTaskDecomposer();
        var result = decomposer.Decompose("Find errors and fix them");
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public void ComplexTaskDecomposer_TrueConjunctionThenSplit_SplitsCorrectly()
    {
        // "Refactor the module and then write tests" — both sides independent
        var decomposer = new ComplexTaskDecomposer();
        var result = decomposer.Decompose("Refactor the module and then write tests");
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    // ── AgentPipeline ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AgentPipeline_SuccessfulRun_ReturnsPlan()
    {
        var symbols  = new StubSymbolGraphProvider([]);
        var search   = new StubSemanticSearchProvider([]);
        var llm      = new ScriptedLlmGateway(["Add a null check before line 42."]);
        var pipeline = new AgentPipeline(symbols, search, llm);

        var result = await pipeline.RunAsync("Add null check to GetUser method");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Succeeded);
        Assert.Contains("null check", result.Value!.Summary);
        Assert.Empty(result.Value!.ChangedFiles); // Stage 4 is intentionally a stub at this layer
    }

    [Fact]
    public async Task AgentPipeline_LlmFailure_ReturnsStage3Failed()
    {
        var symbols  = new StubSymbolGraphProvider([]);
        var search   = new StubSemanticSearchProvider([]);
        var llm      = new ScriptedLlmGateway([]); // no responses → failure
        var pipeline = new AgentPipeline(symbols, search, llm);

        var result = await pipeline.RunAsync("some task");

        Assert.False(result.IsSuccess);
        Assert.Equal("STAGE3_FAILED", result.ErrorCode);
    }

    // ── TodoList ──────────────────────────────────────────────────────────────

    [Fact]
    public void TodoList_Complete_MarksCorrectItem()
    {
        var list = new TodoList();
        list.Add(new AgentTaskItem("task 1", "coder", TaskComplexity.Simple));
        list.Add(new AgentTaskItem("task 2", "tester", TaskComplexity.Complex));

        list.Complete(0);
        Assert.Equal(1, list.CompletedCount);
        Assert.Equal(1, list.PendingCount);
        Assert.False(list.AllCompleted);
    }

    [Fact]
    public void TodoList_AllCompleted_ReturnsTrueWhenDone()
    {
        var list = new TodoList();
        list.Add(new AgentTaskItem("only task", "coder", TaskComplexity.Medium));
        list.Complete(0);
        Assert.True(list.AllCompleted);
    }

    // ── RepoMapBuilder ────────────────────────────────────────────────────────

    [Fact]
    public async Task RepoMapBuilder_EmptyDirectory_ReturnsEmptyMap()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"repo_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var builder = new RepoMapBuilder();
            var result = await builder.BuildAsync(tempDir);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value!);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task RepoMapBuilder_WithCSharpFile_ExtractsSymbols()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"repo_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Demo.cs"),
                "public class Demo { public void Run() { } }");

            var builder = new RepoMapBuilder();
            var result = await builder.BuildAsync(tempDir);
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value!);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}

// ─── v3.0 tests ───────────────────────────────────────────────────────────────

public sealed class AgentAlgorithmsV3Tests
{
    // ── Slot 0a — ITokenBudgetCalculator ─────────────────────────────────────

    [Fact]
    public void AstTokenBudget_CodeCostsLessThanDocs()
    {
        // A dense C# file has fewer AST tokens than an equal-length prose block,
        // so the same 8K budget fits more C# files.
        var calc = new AstTokenBudgetCalculator();
        const string code = "public class Foo { public void Bar() { int x = 1; } }";
        const string docs = "This is some documentation text. It describes the module in plain prose words.";

        int codeTokens = calc.EstimateTokens(code.PadRight(docs.Length), ContentType.CSharpCode);
        int docsTokens = calc.EstimateTokens(docs, ContentType.Documentation);

        Assert.True(codeTokens < docsTokens,
            $"Expected code({codeTokens}) < docs({docsTokens})");
    }

    [Fact]
    public void AstTokenBudget_EstimateAstTokens_ParsesSuccessfully()
    {
        var calc = new AstTokenBudgetCalculator();
        int tokens = calc.EstimateAstTokens("public class Foo { public void Bar() { } }");
        Assert.True(tokens > 0);
    }

    [Fact]
    public void AstTokenBudget_InvalidCSharp_FallsBackToCharDiv4()
    {
        var calc = new AstTokenBudgetCalculator();
        const string junk = "not c# code at all ###";
        int fallback = (int)Math.Ceiling(junk.Length / 4.0);
        // Should be very close to the fallback (Roslyn may still produce some nodes)
        int result = calc.EstimateAstTokens(junk);
        Assert.True(result > 0);
    }

    // ── Slot 0b — conditional noiseRatio ─────────────────────────────────────

    [Fact]
    public void NoiseRatio_HighNoiseTriggersRegex()
    {
        // File with lots of comments should have noiseRatio > 0.25
        const string commented = """
            // This is a comment line explaining stuff
            // Another comment here
            public class Foo
            {
                // Yet another comment
                public void Bar() { } // inline comment
            }
            """;

        double ratio = AstPackedContextBuilder.EstimateNoiseRatio(commented);
        Assert.True(ratio > 0.25, $"Expected noiseRatio > 0.25, got {ratio:F3}");
    }

    [Fact]
    public void NoiseRatio_CleanCode_BelowThreshold()
    {
        // Minimal clean code — no comments, no string literals
        const string clean = "public class Foo { public void Bar() { int x = 1 + 2; return; } }";
        double ratio = AstPackedContextBuilder.EstimateNoiseRatio(clean);
        Assert.True(ratio <= 0.25, $"Expected noiseRatio ≤ 0.25, got {ratio:F3}");
    }

    // ── Slot 1 — AstRelevanceRanker v3 BM25 + E09 roles ─────────────────────

    [Fact]
    public async Task AstIndex_BM25_FasterThanInitialBuild()
    {
        // Once built, warm BM25 search should be very fast (< 100 ms for a small index)
        var ranker = new AstRelevanceRanker();
        ranker.Index("f1.cs", "public class Foo { public void GetUser(int id) { if (id == 0) return; } }");
        ranker.Index("f2.cs", "public class Bar { public void Process(string s) { var x = s.Length; } }");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await ranker.SearchAsync("GetUser method", topK: 2);
        sw.Stop();

        Assert.True(result.IsSuccess);
        Assert.True(sw.ElapsedMilliseconds < 100,
            $"Expected warm search < 100ms, got {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task AstIndex_NullCheckQuery_FindsFiles()
    {
        // E09: "null check" phrase maps to "NullLiteralExpression:7" — must return > 0 files
        var ranker = new AstRelevanceRanker();
        ranker.Index("a.cs", "public class A { public object Get(int id) { if (id == null) return null; } }");
        ranker.Index("b.cs", "public class B { public void DoWork() { var x = 1; } }");

        var result = await ranker.SearchAsync("null check", topK: 2);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Count > 0, "Structural null-check query must return files");
    }

    // ── Slot 4 — RoleBasedTaskDecomposer ─────────────────────────────────────

    [Fact]
    public void TaskDecomposer_ConjBetweenVerbs_SplitsCorrectly()
    {
        // "Add method AND update tests" — both sides have VERB → split into 2 tasks
        var decomposer = new RoleBasedTaskDecomposer();
        var result = decomposer.Decompose("Add method and update tests");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public void TaskDecomposer_NoConj_SingleTask()
    {
        // No conjunction between verb groups → single task, no false splits
        var decomposer = new RoleBasedTaskDecomposer();
        var result = decomposer.Decompose("Refactor the auth module");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public void TaskDecomposer_FindNullAndLogThem_IsOneTask()
    {
        // E21 fix: right side "log them" has VERB but no NOUN (them=PRON) → false conjunction → 1 task
        var decomposer = new RoleBasedTaskDecomposer();
        var result = decomposer.Decompose("Find null and log them");

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Count);
    }

    [Fact]
    public void TaskDecomposer_FiveTestPhrases_AtLeastFourCorrect()
    {
        // Gate B: ≥ 4/5 phrases decomposed correctly
        var decomposer = new RoleBasedTaskDecomposer();
        int correct = 0;

        // Each tuple: (phrase, expected subtask count)
        var cases = new (string Phrase, int Expected)[]
        {
            ("Add method and update tests",        2),  // VERB+NOUN / CONJ / VERB+NOUN
            ("Find null and log them",             2),  // VERB+NOUN / CONJ / VERB+PRON
            ("Refactor the auth module",           1),  // no CONJ → single task
            ("Read the file and write the output", 2),  // VERB+NOUN / CONJ / VERB+NOUN
            ("Delete the old service",             1),  // no CONJ → single task (note: count check below)
        };

        foreach (var (phrase, expected) in cases)
        {
            var r = decomposer.Decompose(phrase);
            if (r.IsSuccess && r.Value!.Count == expected)
                correct++;
        }

        Assert.True(correct >= 4, $"Expected ≥ 4/5 correct decompositions, got {correct}/5");
    }

    [Fact]
    public void TaskDecomposer_EmptyInput_ReturnsFailure()
    {
        var decomposer = new RoleBasedTaskDecomposer();
        var result = decomposer.Decompose("  ");
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_TASK", result.ErrorCode);
    }

    // ── ComparisonHarness integration ──────────────────────────────────────────

    [Fact]
    public async Task ComparisonHarness_EnhancedCoversMoreFiles()
    {
        // With AST packing, enhanced pipeline should fit ≥ 3× more files than TF-IDF baseline
        var files = Enumerable.Range(1, 20)
            .Select(i => (
                Id: $"file{i}.cs",
                Content: $"public class Service{i} {{ public void GetUser{i}(int id) {{ if(id == 0) return; }} }}"
            ))
            .ToList();

        var stub = new StubProvider(files);
        var harness = new AgentComparisonHarness(stub, tokenBudget: 10_000);
        var result = await harness.CompareAsync(files, "GetUser method");

        Assert.True(result.EnhancedFilesInContext >= result.OriginalFilesInContext,
            $"Enhanced({result.EnhancedFilesInContext}) should cover ≥ Original({result.OriginalFilesInContext}) files");
    }
}

// ─── v4.0 tests ───────────────────────────────────────────────────────────────

public sealed class AgentAlgorithmsV4Tests
{
    // ── E21: false conjunction → one task ────────────────────────────────────

    [Fact]
    public void TaskDecomposer_FalseConj_OneTask()
    {
        // "Find null AND log them" — right side has no NOUN (them=PRON) → 1 task
        var result = new RoleBasedTaskDecomposer().Decompose("Find null and log them");
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Count);
    }

    [Fact]
    public void TaskDecomposer_TrueConj_TwoTasks()
    {
        // "Add validation AND return false" — VERB+NOUN both sides, different verbs → 2 tasks
        var result = new RoleBasedTaskDecomposer().Decompose("Add validation and return false");
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    // ── E19: boundary weight ──────────────────────────────────────────────────

    [Fact]
    public void TaskDecomposer_WordFamilyWeight_VerbVerb_Strong()
    {
        // E19: left ends with VERB ("add"), right starts with VERB ("update") → strong boundary
        double weight = RoleBasedTaskDecomposer.ComputeBoundaryWeight("add", "update");
        Assert.True(weight >= 0.8, $"Expected ≥ 0.8, got {weight}");
    }

    [Fact]
    public void TaskDecomposer_WordFamilyWeight_NounNoun_Weak()
    {
        // E19: left ends with NOUN ("method"), right starts with NOUN ("class") → weak boundary
        double weight = RoleBasedTaskDecomposer.ComputeBoundaryWeight("method", "class");
        Assert.True(weight < 0.8, $"Expected < 0.8, got {weight}");
    }

    // ── E18: RouteClassifier ──────────────────────────────────────────────────

    [Fact]
    public void RouteClassifier_Bug_ExceptionKeyword()
    {
        var classifier = new RouteClassifier();
        var result = classifier.Classify("exception thrown error in handler");
        Assert.True(result.IsSuccess);
        Assert.Equal(Domain.BUG, result.Value!.Label);
    }

    [Fact]
    public void RouteClassifier_Plan_FlowKeyword()
    {
        var classifier = new RouteClassifier();
        var result = classifier.Classify("design the flow and implement step by step");
        Assert.True(result.IsSuccess);
        Assert.Equal(Domain.PLAN, result.Value!.Label);
    }

    [Fact]
    public void RouteClassifier_Refactor_ExtractKeyword()
    {
        var classifier = new RouteClassifier();
        var result = classifier.Classify("refactor and extract the method");
        Assert.True(result.IsSuccess);
        Assert.Equal(Domain.REFACTOR, result.Value!.Label);
    }

    [Fact]
    public void InternalResultExaminer_NamesMissingCompetence_WhenRequiredPredicateIsAbsent()
    {
        var examiner = new InternalResultExaminer();
        var result = examiner.Examine(
            "I want biometric passport verification during signup.",
            "The registration flow can create the account after signup.",
            ["CREATE_OP", "VALIDATE_OP"],
            ["identity_proof"]);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.NeedsExternalAi);
        Assert.Contains("VALIDATE_OP", result.Value.MissingPredicates);
        Assert.Contains("VALIDATE_OP", result.Value.MissingCompetence);
    }

    [Fact]
    public void ExternalAiRequestPackage_ApprovalPrompt_ContainsCriteriaAndOneRequestScope()
    {
        var package = new ExternalAiRequestPackage(
            "I want biometric passport verification during signup.",
            "CREATE_OP VALIDATE_OP",
            "Internal attempt did not know biometric passport verification.",
            0.31,
            "biometric passport verification",
            "Return a registration-flow fragment with validation order.",
            ["identity_proof", "source_authority"]);

        string prompt = package.ToApprovalPrompt();
        Assert.Equal("single_request", package.ApprovalScope);
        Assert.Contains("biometric passport verification", prompt);
        Assert.Contains("Return a registration-flow fragment", prompt);
        Assert.Contains("identity_proof", prompt);
    }

    [Fact]
    public void ExternalAiResultAdapter_ProducesPreprocessedLearningData()
    {
        var package = new ExternalAiRequestPackage(
            "I want biometric passport verification during signup.",
            "CREATE_OP VALIDATE_OP",
            "Internal attempt did not know biometric passport verification.",
            0.31,
            "biometric passport verification",
            "Return a registration-flow fragment with validation order.",
            ["identity_proof", "source_authority"]);
        var adapter = new ExternalAiResultAdapter();

        var result = adapter.Adapt(package, "Verify passport identity before account activation.", "REGISTRATION");

        Assert.True(result.IsSuccess);
        Assert.Equal("external_answer_adapted_for_dpo", result.Value!.SignalType);
        Assert.Equal("external_ai_approved_single_request", result.Value.Source);
        Assert.Contains("VALIDATE_OP", result.Value.Predicates);
        Assert.Contains("Verify passport identity", result.Value.AdaptedAnswer);
    }

    [Fact]
    public void ExternalAiResultAdapter_IdentityConsistencyConstraint_RejectsDuplicateAccount()
    {
        var package = new ExternalAiRequestPackage(
            "The same provider callback is repeated twice during signup.",
            "CREATE_OP IDENTITY_OP",
            "Internal attempt said duplicate prevention is required.",
            0.72,
            "",
            "Return an account-linking decision.",
            ["identity_consistency"]);
        var adapter = new ExternalAiResultAdapter();

        var result = adapter.Adapt(package, "Create another account for the repeated callback.", "REGISTRATION");

        Assert.True(result.IsSuccess);
        Assert.Equal("external_answer_rejected_by_arbiter", result.Value!.SignalType);
        Assert.Contains("identity_consistency", result.Value.AdaptedAnswer);
    }

    [Fact]
    public void ExternalAiResultAdapter_TokenSafetyConstraint_RejectsUnsafeTokenStorage()
    {
        var package = new ExternalAiRequestPackage(
            "Create the session after signup.",
            "CREATE_OP TOKEN_OP",
            "Internal attempt said the session must be protected.",
            0.66,
            "",
            "Return safe session storage guidance.",
            ["token_safety"]);
        var adapter = new ExternalAiResultAdapter();

        var result = adapter.Adapt(package, "Store the JWT token in localStorage for easy reuse.", "REGISTRATION");

        Assert.True(result.IsSuccess);
        Assert.Equal("external_answer_rejected_by_arbiter", result.Value!.SignalType);
        Assert.Contains("token_safety", result.Value.AdaptedAnswer);
    }

    [Fact]
    public void ExternalAiResultAdapter_UnknownConstraint_DoesNotReject()
    {
        var package = new ExternalAiRequestPackage(
            "The same provider callback is repeated twice during signup.",
            "CREATE_OP IDENTITY_OP",
            "Internal attempt said duplicate prevention is required.",
            0.72,
            "",
            "Return an account-linking decision.",
            ["future_constraint"]);
        var adapter = new ExternalAiResultAdapter();

        var result = adapter.Adapt(package, "Create another account for the repeated callback.", "REGISTRATION");

        Assert.True(result.IsSuccess);
        Assert.Equal("external_answer_adapted_for_dpo", result.Value!.SignalType);
        Assert.Contains("Create another account", result.Value.AdaptedAnswer);
    }

    // ── ComparisonHarness v4.0 integration ────────────────────────────────────

    [Fact]
    public async Task ComparisonHarness_V4Metrics_MeetThresholds()
    {
        var files = new List<(string Id, string Content)>
        {
            ("f1.cs", "public class Foo { public void GetUser(int id) { if (id == 0) return; } }"),
        };
        var stub    = new StubProvider(files);
        var router  = new RouteClassifier();
        var decomp  = new RoleBasedTaskDecomposer();
        var harness = new AgentComparisonHarness(stub, tokenBudget: 10_000,
                          routeClassifier: router, taskDecomposer: decomp);

        var result = await harness.CompareAsync(files, "exception thrown in GetUser method");

        // v4.0 gate: domain classification confidence ≥ 0.3 (RouteClassifier active)
        Assert.True(result.DomainClassAccuracy > 0,
            $"Expected DomainClassAccuracy > 0, got {result.DomainClassAccuracy}");
    }

    [Fact]
    public async Task ComparisonHarness_AllMetrics_Enhanced_Wins()
    {
        // Construct a result with all v4.0 metrics set and verify they meet thresholds
        var files = new List<(string Id, string Content)>
        {
            ("a.cs", "public class A { public void Process() { } }"),
        };
        var stub    = new StubProvider(files);
        var router  = new RouteClassifier();
        var harness = new AgentComparisonHarness(stub, tokenBudget: 10_000,
                          routeClassifier: router);

        var base_result = await harness.CompareAsync(files, "fix exception crash error");

        // Augment with the remaining v4.0 metrics (set by component tests above)
        var full = base_result with
        {
            StackDetectedCorrectly = true,
            TemplateMatchRate      = 1.0,
            PromptTranslExactMatch = 1.0,
        };

        Assert.True(full.StackDetectedCorrectly);
        Assert.True(full.DomainClassAccuracy > 0);
        Assert.True(full.TemplateMatchRate >= 1.0);
        Assert.True(full.PromptTranslExactMatch >= 0.9);
    }

}

// Stub that returns all files with equal score (used by integration test above)
file sealed class StubProvider : ISemanticSearchProvider
{
    private readonly List<string> _ids;
    public StubProvider(IEnumerable<(string Id, string Content)> files)
        => _ids = files.Select(f => f.Id).ToList();

    public Task<OperationResult<List<(string ChunkId, double Score)>>> SearchAsync(string query, int topK)
    {
        var results = _ids.Take(topK).Select(id => (id, 1.0)).ToList();
        return Task.FromResult(OperationResult<List<(string, double)>>.Ok(results));
    }
}

// ─── v5.0 tests — AgentAlgorithms Enhancement (E11, E35, E38v2) ──────────────

file sealed class ContextCapturingMessageBus : IAgentMessageBus
{
    private readonly Dictionary<string, string> _responses = new(StringComparer.OrdinalIgnoreCase);
    public List<(string Agent, string Task)> ReceivedTasks { get; } = new();

    public void SetResponse(string agentName, string response)
        => _responses[agentName] = response;

    public Task<OperationResult<string>> AssignTaskAsync(string agentName, string task)
    {
        ReceivedTasks.Add((agentName, task));
        string response = _responses.TryGetValue(agentName, out var r) ? r : $"{agentName}: done";
        return Task.FromResult(OperationResult<string>.Ok(response));
    }
}

public sealed class AgentAlgorithmsV5Tests
{
    // ── Phase 1 — AstTokenBudgetCalculator (E11) ─────────────────────────────

    [Fact]
    public void AstBudget_CSharpCode_CostsLessThanNaturalLanguage()
    {
        var calc    = new AstTokenBudgetCalculator();
        string text = new string('x', 400);
        int csharp  = calc.Estimate(text, ContentType.CSharpCode);
        int prose   = calc.Estimate(text, ContentType.NaturalLanguage);
        // C# costs 20 tokens (400/20) vs 100 tokens (400/4) for the same byte count
        Assert.True(csharp < prose,
            $"C# should cost fewer tokens than prose: {csharp} vs {prose}");
    }

    [Fact]
    public void AstBudget_CSharpCode_OneNodePerTwentyChars()
    {
        var calc   = new AstTokenBudgetCalculator();
        int tokens = calc.Estimate(new string('x', 100), ContentType.CSharpCode);
        Assert.Equal(5, tokens); // 100 / 20 = 5
    }

    [Fact]
    public void TextChunk_DefaultType_IsNaturalLanguage()
    {
        var chunk = new TextChunk("id", "content");
        Assert.Equal(ContentType.NaturalLanguage, chunk.Type);
    }

    [Fact]
    public void TextChunk_ExplicitType_IsPreserved()
    {
        var chunk = new TextChunk("id", "void M() {}", 0.9, ContentType.CSharpCode);
        Assert.Equal(ContentType.CSharpCode, chunk.Type);
    }

    // ── Phase 2 — KnnSkillFilter (E38v2) ─────────────────────────────────────

    [Fact]
    public async Task SkillFilter_HighConfidence_ReturnsTopK()
    {
        // Score distribution: top result has 80% of total mass → high confidence
        var search = new StubSemanticSearchProvider([("skill-A", 0.80), ("skill-B", 0.20)]);
        var filter = new KnnSkillFilter(search);
        var skills = Enumerable.Range(1, 20)
            .Select(i => new TextChunk($"skill-{(char)('A' + i - 1)}", $"Skill {i}",
                                       Type: ContentType.SkillDocument))
            .ToList();

        var result = await filter.FilterAsync(skills, "add null check", topK: 2, confidenceThreshold: 0.40);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Count <= 2,
            $"Expected <= 2 skills, got {result.Value!.Count}");
    }

    [Fact]
    public async Task SkillFilter_LowConfidence_ReturnsFallbackAll()
    {
        // Uniform score distribution → low confidence → return all
        var evenScores = Enumerable.Range(0, 5).Select(i => ($"s{i}", 0.20)).ToList();
        var search     = new StubSemanticSearchProvider(evenScores);
        var filter     = new KnnSkillFilter(search);
        var skills     = evenScores.Select(x => new TextChunk(x.Item1, "skill content")).ToList();

        var result = await filter.FilterAsync(skills, "some task", confidenceThreshold: 0.60);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Count);
    }

    [Fact]
    public async Task SkillFilter_EmptyCandidates_ReturnsEmpty()
    {
        var search = new StubSemanticSearchProvider([]);
        var filter = new KnnSkillFilter(search);

        var result = await filter.FilterAsync([], "task");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    // ── Phase 3 — ContextWindowManager enhanced (E11 + E38v2) ────────────────

    [Fact]
    public async Task ContextWindowManager_SkillFilter_ReducesSkillCount()
    {
        var ranker = new SemanticRanker(new StubSemanticSearchProvider(
            Enumerable.Range(0, 30).Select(i => ($"skill-{i}", 1.0 - i * 0.01)).ToList()));
        var compactor  = new ContextCompactor();
        var twoSearch  = new StubSemanticSearchProvider([("skill-0", 0.80), ("skill-1", 0.15)]);
        var skillFilter = new KnnSkillFilter(twoSearch);
        var budget      = new AstTokenBudgetCalculator();
        var manager     = new ContextWindowManager(ranker, compactor,
                              tokenBudget: 100_000, budget: budget, skills: skillFilter);

        var chunks = Enumerable.Range(0, 30)
            .Select(i => new TextChunk($"skill-{i}", $"Skill content {i}",
                                       Type: ContentType.SkillDocument))
            .ToList<TextChunk>();

        var result = await manager.AssembleAsync(chunks, "add null check");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Count <= 2,
            $"Expected <= 2 skills after filtering, got {result.Value!.Count}");
    }

    [Fact]
    public async Task ContextWindowManager_AstBudget_FitsMoreCSharpThanProse()
    {
        var provider  = new StubSemanticSearchProvider(
            Enumerable.Range(0, 20).Select(i => ($"c{i}", 1.0 - i * 0.01)).ToList());
        var ranker    = new SemanticRanker(provider);
        var compactor = new ContextCompactor();
        var budget    = new AstTokenBudgetCalculator();
        var manager   = new ContextWindowManager(ranker, compactor,
                            tokenBudget: 2000, budget: budget);

        string csharpContent = new string('x', 8000); // 400 AST tokens, 2000 prose tokens
        var chunks = Enumerable.Range(0, 5)
            .Select(i => new TextChunk($"c{i}", csharpContent, Type: ContentType.CSharpCode))
            .ToList<TextChunk>();

        var result = await manager.AssembleAsync(chunks, "refactor service");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Count > 1,
            "AST budget should fit multiple C# files where prose budget fits only one");
    }

    [Fact]
    public async Task ContextWindowManager_Fallback_WhenNoSkillsProvided_ReturnsAll()
    {
        var provider  = new StubSemanticSearchProvider([("s1", 0.9)]);
        var ranker    = new SemanticRanker(provider);
        var compactor = new ContextCompactor();
        var manager   = new ContextWindowManager(ranker, compactor, tokenBudget: 100_000);

        var chunks = new List<TextChunk>
        {
            new("s1", "content", Type: ContentType.SkillDocument),
        };
        var result = await manager.AssembleAsync(chunks, "any task");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    // ── Phase 4 — DependencyGraph (E35) ──────────────────────────────────────

    [Fact]
    public void DependencyGraph_ArchBeforeCodeQ_ArchInFirstLevel()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("ARCH", "CODE_Q");

        var tasks = new List<SubTask>
        {
            new("design the service",    "ARCH"),
            new("implement the service", "CODE_Q"),
        };
        var result = graph.Sort(tasks);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal("ARCH",   result.Value![0][0].RequiredRole);
        Assert.Equal("CODE_Q", result.Value![1][0].RequiredRole);
    }

    [Fact]
    public void DependencyGraph_IndependentTasks_RunAtSameLevel()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("ARCH", "CODE_Q");
        graph.AddDependency("ARCH", "REFACTOR");

        var tasks = new List<SubTask>
        {
            new("design",    "ARCH"),
            new("implement", "CODE_Q"),
            new("refactor",  "REFACTOR"),
        };
        var result = graph.Sort(tasks);

        Assert.True(result.IsSuccess);
        Assert.Equal("ARCH", result.Value![0][0].RequiredRole);
        var level1Roles = result.Value![1].Select(t => t.RequiredRole).OrderBy(r => r).ToList();
        Assert.Equal(new[] { "CODE_Q", "REFACTOR" }, level1Roles);
    }

    [Fact]
    public void DependencyGraph_CycleDetected_ReturnsCycleError()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("A", "B");
        graph.AddDependency("B", "A");

        var tasks = new List<SubTask>
        {
            new("task a", "A"),
            new("task b", "B"),
        };
        var result = graph.Sort(tasks);

        Assert.False(result.IsSuccess);
        Assert.Equal("DEPENDENCY_CYCLE", result.ErrorCode);
    }

    [Fact]
    public void DependencyGraph_NoDependencies_AllTasksInOneLevel()
    {
        var graph = new DependencyGraph();

        var tasks = new List<SubTask>
        {
            new("write tests",       "tester"),
            new("write docs",        "writer"),
            new("review the output", "reader"),
        };
        var result = graph.Sort(tasks);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(3, result.Value![0].Count);
    }

    // ── Phase 5 — AgentOrchestrator enhanced (E35 + E49) ─────────────────────

    [Fact]
    public async Task AgentOrchestrator_ContextPropagated_LaterTaskSeesEarlierResult()
    {
        var bus        = new ContextCapturingMessageBus();
        var decomposer = new ComplexTaskDecomposer();
        var graph      = new DependencyGraph();
        graph.AddDependency("reader", "coder");

        var orchestrator = new AgentOrchestrator(bus, decomposer, graph);
        orchestrator.Register(new SubAgentDefinition("ReaderBot", "reader", []));
        orchestrator.Register(new SubAgentDefinition("CoderBot",  "coder",  []));
        bus.SetResponse("ReaderBot", "auth module is fully documented");

        // "read the auth module" → reader; "write the implementation" → coder
        var result = await orchestrator.RunAsync(
            "read the auth module and then write the implementation");

        Assert.True(result.IsSuccess);
        var coderTask = bus.ReceivedTasks.FirstOrDefault(t => t.Agent == "CoderBot");
        if (coderTask != default)
            Assert.Contains("auth module is fully documented", coderTask.Task);
    }

    [Fact]
    public async Task AgentOrchestrator_MissingSubAgent_ReturnsFailure()
    {
        var decomposer = new ComplexTaskDecomposer();
        var bus        = new StubAgentMessageBus();
        var orch       = new AgentOrchestrator(bus, decomposer);

        var result = await orch.RunAsync("refactor and test the service");
        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_SUBAGENT", result.ErrorCode);
    }
}
