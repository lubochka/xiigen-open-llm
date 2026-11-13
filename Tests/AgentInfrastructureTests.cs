// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;

namespace LlmFromScratch.Tests;

// ─── Fakes ────────────────────────────────────────────────────────────────────

file sealed class InMemoryConfigProvider : IConfigurationProvider
{
    private readonly Dictionary<string, string> _files;
    public InMemoryConfigProvider(Dictionary<string, string> files) => _files = files;

    public Task<OperationResult<Dictionary<string, object>>> ReadJsonAsync(string path)
    {
        if (!_files.TryGetValue(path, out var json))
            return Task.FromResult(OperationResult<Dictionary<string, object>>.Fail("Not found", "NOT_FOUND"));
        try
        {
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json)
                       ?? new Dictionary<string, object>();
            return Task.FromResult(OperationResult<Dictionary<string, object>>.Ok(dict));
        }
        catch
        {
            return Task.FromResult(OperationResult<Dictionary<string, object>>.Fail("Bad JSON", "PARSE_ERROR"));
        }
    }

    public Task<OperationResult<string>> ReadTextAsync(string path)
    {
        if (!_files.TryGetValue(path, out var text))
            return Task.FromResult(OperationResult<string>.Fail("Not found", "NOT_FOUND"));
        return Task.FromResult(OperationResult<string>.Ok(text));
    }
}

file sealed class InMemoryCodeReader : ICodeReader
{
    private readonly Dictionary<string, string> _files;
    public InMemoryCodeReader(Dictionary<string, string> files) => _files = files;

    public Task<OperationResult<string>> ReadFileAsync(string path)
    {
        if (!_files.TryGetValue(path, out var content))
            return Task.FromResult(OperationResult<string>.Fail("Not found", "NOT_FOUND"));
        return Task.FromResult(OperationResult<string>.Ok(content));
    }

    public Task<OperationResult<IReadOnlyList<string>>> ListFilesAsync(string directory, string pattern)
    {
        var matches = _files.Keys
            .Where(k => k.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
            .ToList<string>();
        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(matches));
    }
}

file sealed class InMemoryCodeWriter : ICodeWriter
{
    public Dictionary<string, string> Written { get; } = new();

    public Task<OperationResult<bool>> WriteFileAsync(string path, string content)
    {
        Written[path] = content;
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }
}

file sealed class ScriptedLlmClient : ILlmClient
{
    private readonly Queue<LlmResponse> _responses;
    public ScriptedLlmClient(IEnumerable<LlmResponse> responses) => _responses = new Queue<LlmResponse>(responses);

    public Task<OperationResult<LlmResponse>> SendAsync(
        IReadOnlyList<ConversationMessage> history,
        IReadOnlyList<ToolDefinition>? tools,
        CancellationToken ct)
    {
        if (_responses.Count == 0)
            return Task.FromResult(OperationResult<LlmResponse>.Fail("No more responses", "EMPTY"));
        return Task.FromResult(OperationResult<LlmResponse>.Ok(_responses.Dequeue()));
    }
}

file sealed class StubDomainModelRegistry : IDomainModelRegistry
{
    private readonly Dictionary<InternalModelDomain, string> _adapters;
    public List<InternalModelDomain> RequestedDomains { get; } = [];

    public StubDomainModelRegistry(Dictionary<InternalModelDomain, string> adapters)
        => _adapters = adapters;

    public OperationResult<string> ResolveAdapter(InternalModelDomain domain)
    {
        RequestedDomains.Add(domain);
        return _adapters.TryGetValue(domain, out var adapter)
            ? OperationResult<string>.Ok(adapter)
            : OperationResult<string>.Fail("Adapter not found.", "ADAPTER_NOT_FOUND");
    }
}

file sealed class InMemorySessionStorage : ISessionStorage
{
    private readonly Dictionary<string, AgentState> _store = new();

    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
    {
        _store[sessionId] = state;
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
    {
        if (!_store.TryGetValue(sessionId, out var s))
            return Task.FromResult(OperationResult<AgentState>.Fail("Not found", "NOT_FOUND"));
        return Task.FromResult(OperationResult<AgentState>.Ok(s));
    }

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok((IReadOnlyList<string>)_store.Keys.ToList()));
}

// ─── Tests ────────────────────────────────────────────────────────────────────

public sealed class AgentInfrastructureTests
{
    private static IDomainModelRegistry FullRegistry() =>
        new StubDomainModelRegistry(new Dictionary<InternalModelDomain, string>
        {
            [InternalModelDomain.CodeQ] = "code_q.lora",
            [InternalModelDomain.Bug] = "bug.lora",
            [InternalModelDomain.Plan] = "plan.lora",
            [InternalModelDomain.Arch] = "arch.lora",
            [InternalModelDomain.Refactor] = "refactor.lora",
            [InternalModelDomain.Generic] = "generic.lora"
        });
    // ── TokenCounter ──────────────────────────────────────────────────────────

    [Fact]
    public void TokenCounter_ShortPhrase_EstimatesReasonably()
    {
        int tokens = TokenCounter.Estimate("hello world how are you");
        Assert.True(tokens > 0);
    }

    [Fact]
    public void TokenCounter_EmptyString_ReturnsZero()
    {
        Assert.Equal(0, TokenCounter.Estimate(""));
    }

    [Fact]
    public void TokenCounter_History_SumsMessages()
    {
        var history = new List<ConversationMessage>
        {
            new("user", "Hello"),
            new("assistant", "Hi there")
        };
        int tokens = TokenCounter.EstimateHistory(history);
        Assert.True(tokens > 0);
    }

    // ── ContextAssembler ──────────────────────────────────────────────────────

    [Fact]
    public void ContextAssembler_Empty_ReturnsEmpty()
    {
        var assembler = new ContextAssembler(1000);
        var result = assembler.Assemble([]);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public void ContextAssembler_RanksHighestFirst()
    {
        var assembler = new ContextAssembler(100_000);
        var chunks = new[]
        {
            new ContextChunk("a.cs", "low relevance", 0.1),
            new ContextChunk("b.cs", "high relevance", 0.9),
            new ContextChunk("c.cs", "medium relevance", 0.5),
        };
        var result = assembler.Assemble(chunks);
        Assert.True(result.IsSuccess);
        Assert.Equal("b.cs", result.Value![0].FilePath);
    }

    [Fact]
    public void ContextAssembler_RespectsTokenBudget()
    {
        var assembler = new ContextAssembler(5); // very tight budget — fits only small chunks
        // 1000 space-separated words → ~1334 tokens; far exceeds budget of 5
        string bigContent = string.Join(" ", Enumerable.Repeat("word", 1000));
        var chunks = new[]
        {
            new ContextChunk("a.cs", bigContent, 1.0),
            new ContextChunk("b.cs", bigContent, 0.9),
        };
        var result = assembler.Assemble(chunks);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!); // neither fits — both exceed budget
    }

    // ── AgentState ────────────────────────────────────────────────────────────

    [Fact]
    public void AgentState_WithMessage_AppendsAndIncrementsIteration()
    {
        var state = new AgentState { CurrentTask = "task1" };
        var updated = state.WithMessage(new ConversationMessage("user", "hello"));
        Assert.Single(updated.MessageHistory);
        Assert.Equal(1, updated.IterationCount);
    }

    [Fact]
    public void AgentState_WithTodoCompleted_MarksCorrectItem()
    {
        var state = new AgentState
        {
            TodoList = [new TodoItem("task a"), new TodoItem("task b")]
        };
        var updated = state.WithTodoCompleted(0);
        Assert.True(updated.TodoList[0].IsCompleted);
        Assert.False(updated.TodoList[1].IsCompleted);
    }

    [Fact]
    public void AgentState_IsImmutable_OriginalUnchanged()
    {
        var state = new AgentState { CurrentTask = "original" };
        _ = state.WithMessage(new ConversationMessage("user", "test"));
        Assert.Empty(state.MessageHistory);
        Assert.Equal(0, state.IterationCount);
    }

    // ── PermissionGate ────────────────────────────────────────────────────────

    [Fact]
    public async Task InternalLlmGateway_CodeQDomain_LoadsCodeQuestionAdapter()
    {
        var registry = FullRegistry();
        var llm = new ScriptedLlmClient([new LlmResponse("internal code question answer", null, null)]);
        var gateway = new InternalLlmGateway(llm, registry);

        var result = await gateway.SendAsync("What does BpeTokenizer.Encode return for an empty string?");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(InternalModelDomain.CodeQ, result.Value!.Domain);
        Assert.Equal("code_q.lora", result.Value.AdapterPath);
        Assert.NotEmpty(result.Value.Response);
    }

    [Fact]
    public async Task InternalLlmGateway_BugDomain_LoadsBugAdapter_ForCompoundV06Prompt()
    {
        var registry = FullRegistry();
        var llm = new ScriptedLlmClient([new LlmResponse("internal bug answer", null, null)]);
        var gateway = new InternalLlmGateway(llm, registry);

        var result = await gateway.SendAsync("The tokenizer crashes on emoji input why and how to fix it?");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(InternalModelDomain.Bug, result.Value!.Domain);
        Assert.Equal("bug.lora", result.Value.AdapterPath);
    }

    [Fact]
    public async Task InternalLlmGateway_UnknownDomain_FallsBackToGenericAdapter()
    {
        var registry = FullRegistry();
        var llm = new ScriptedLlmClient([new LlmResponse("generic internal answer", null, null)]);
        var gateway = new InternalLlmGateway(llm, registry);

        var result = await gateway.SendAsync("gentle words without a project signal");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(InternalModelDomain.Generic, result.Value!.Domain);
        Assert.Equal("generic.lora", result.Value.AdapterPath);
    }

    [Fact]
    public async Task InternalLlmGateway_RegistryMissForDomain_StillSucceeds()
    {
        var registry = new StubDomainModelRegistry(new Dictionary<InternalModelDomain, string>
        {
            [InternalModelDomain.Generic] = "generic.lora"
        });
        var llm = new ScriptedLlmClient([new LlmResponse("fallback internal answer", null, null)]);
        var gateway = new InternalLlmGateway(llm, registry);

        var result = await gateway.SendAsync("Architect a plugin system so tokenizers can be swapped at runtime.");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("generic.lora", result.Value!.AdapterPath);
        Assert.True(result.Value.UsedFallbackAdapter);
    }

    [Fact]
    public async Task InternalLlmGateway_DoesNotCallExternalAi()
    {
        var registry = FullRegistry();
        var llm = new ScriptedLlmClient([new LlmResponse("internal only", null, null)]);
        var gateway = new InternalLlmGateway(llm, registry);

        var result = await gateway.SendAsync("fix the crash");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, gateway.ExternalCallCount);
        Assert.Equal(InternalModelDomain.Bug, result.Value!.Domain);
    }

    [Fact]
    public void InternalLlmGateway_InspectRouting_BugDominantBoundary_LoadsBugAdapter()
    {
        var gateway = new InternalLlmGateway(
            new ScriptedLlmClient([]),
            FullRegistry());

        var route = gateway.InspectRouting("explain the fix pattern for avoiding null crashes in general");

        Assert.True(route.IsSuccess, route.ErrorMessage);
        Assert.Equal(InternalModelDomain.Bug, route.Value!.Domain);
        Assert.Equal("bug.lora", route.Value.AdapterPath);
    }

    [Fact]
    public void InternalLlmGateway_InspectRouting_QuestionDominantBoundary_LoadsCodeQuestionAdapter()
    {
        var gateway = new InternalLlmGateway(
            new ScriptedLlmClient([]),
            FullRegistry());

        var route = gateway.InspectRouting("what pattern should I use to avoid null crashes");

        Assert.True(route.IsSuccess, route.ErrorMessage);
        Assert.Equal(InternalModelDomain.CodeQ, route.Value!.Domain);
        Assert.Equal("code_q.lora", route.Value.AdapterPath);
    }

    [Fact]
    public void PermissionGate_ReadTool_IsAllowed()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var result = gate.Check("read_file", new Dictionary<string, object> { ["path"] = @"C:\projects\myapp\src\foo.cs" });
        Assert.True(result.IsSuccess);
        Assert.Equal(PermissionDecision.Allowed, result.Value);
    }

    [Fact]
    public void PermissionGate_WriteTool_RequiresApproval()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var result = gate.Check("edit_file", new Dictionary<string, object> { ["path"] = @"C:\projects\myapp\src\foo.cs" });
        Assert.True(result.IsSuccess);
        Assert.Equal(PermissionDecision.RequiresApproval, result.Value);
    }

    [Fact]
    public void PermissionGate_ExternalAiCall_RequiresApproval()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var result = gate.Check("call_external_ai", new Dictionary<string, object>
        {
            ["missing_competence"] = "biometric passport verification"
        });
        Assert.True(result.IsSuccess);
        Assert.Equal(PermissionDecision.RequiresApproval, result.Value);
    }

    [Fact]
    public void PermissionGate_ExternalAiCall_SequentialChecks_BothRequireApproval()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var args = new Dictionary<string, object>
        {
            ["missing_competence"] = "biometric passport verification"
        };

        var first = gate.Check("call_external_ai", args);
        var second = gate.Check("call_external_ai", args);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(PermissionDecision.RequiresApproval, first.Value);
        Assert.Equal(PermissionDecision.RequiresApproval, second.Value);
    }

    [Fact]
    public void PermissionGate_PathOutsideRoot_IsDenied()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var result = gate.Check("read_file", new Dictionary<string, object> { ["path"] = @"C:\secret\passwords.txt" });
        Assert.True(result.IsSuccess);
        Assert.Equal(PermissionDecision.Denied, result.Value);
    }

    [Fact]
    public void PermissionGate_UnknownTool_IsDenied()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var result = gate.Check("unknown_tool", new Dictionary<string, object>());
        Assert.True(result.IsSuccess);
        Assert.Equal(PermissionDecision.Denied, result.Value);
    }

    [Fact]
    public void PermissionGate_EmptyToolName_Fails()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var result = gate.Check("", new Dictionary<string, object>());
        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_TOOL", result.ErrorCode);
    }

    [Fact]
    public void LearningEventScheduler_DrainsOnlyWhenThresholdAndIdle()
    {
        var idle = false;
        var scheduler = new LearningEventScheduler(() => idle, minimumExamples: 5);

        var tooSmall = scheduler.QueueIfReady("BUG", 4, "not enough pairs");
        Assert.True(tooSmall.IsSuccess);
        Assert.False(tooSmall.Value);
        Assert.Equal(0, scheduler.QueuedCount);

        var queued = scheduler.QueueIfReady("BUG", 5, "new DPO pairs from external adaptation");
        Assert.True(queued.IsSuccess);
        Assert.True(queued.Value);

        var blocked = scheduler.DrainIfIdle();
        Assert.True(blocked.IsSuccess);
        Assert.False(blocked.Value!.DidRun);

        idle = true;
        var drained = scheduler.DrainIfIdle();
        Assert.True(drained.IsSuccess);
        Assert.True(drained.Value!.DidRun);
        Assert.Single(drained.Value.Events);
        Assert.Equal(0, scheduler.QueuedCount);
    }

    // ── ToolRegistry ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ToolRegistry_RegisteredTool_Executes()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var registry = new ToolRegistry(gate);
        var def = new ToolDefinition("list_directory", "List files", new Dictionary<string, object>());
        registry.Register(def, args => Task.FromResult(OperationResult<string>.Ok("file1.cs")));

        var result = await registry.ExecuteAsync("list_directory", new Dictionary<string, object>());
        Assert.True(result.IsSuccess);
        Assert.Equal("file1.cs", result.Value);
    }

    [Fact]
    public async Task ToolRegistry_UnknownTool_ReturnsUnknownToolError()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var registry = new ToolRegistry(gate);
        var result = await registry.ExecuteAsync("nonexistent", new Dictionary<string, object>());
        Assert.False(result.IsSuccess);
        Assert.Equal("UNKNOWN_TOOL", result.ErrorCode);
    }

    [Fact]
    public void ToolRegistry_AllToolDefinitions_ReturnsRegistered()
    {
        var gate = new PermissionGate(@"C:\projects\myapp");
        var registry = new ToolRegistry(gate);
        registry.Register(new ToolDefinition("read_file", "Read", []), args => Task.FromResult(OperationResult<string>.Ok("")));
        registry.Register(new ToolDefinition("list_directory", "List", []), args => Task.FromResult(OperationResult<string>.Ok("")));
        Assert.Equal(2, registry.AllToolDefinitions().Count);
    }

    // ── FileChangeApplier ─────────────────────────────────────────────────────

    [Fact]
    public async Task FileChangeApplier_ValidPatch_AppliesReplacement()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "int x = 1;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        var result = await applier.ApplyPatchAsync("src/foo.cs", "int x = 1;", "int x = 42;");
        Assert.True(result.IsSuccess);
        Assert.Equal("int x = 42;", writer.Written["src/foo.cs"]);
    }

    [Fact]
    public async Task FileChangeApplier_SearchNotFound_ReturnsNotFound()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "int x = 1;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        var result = await applier.ApplyPatchAsync("src/foo.cs", "int y = 99;", "int y = 0;");
        Assert.False(result.IsSuccess);
        Assert.Equal("PATCH_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task FileChangeApplier_AmbiguousMatch_ReturnsAmbiguous()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "x x x"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        var result = await applier.ApplyPatchAsync("src/foo.cs", "x", "y");
        Assert.False(result.IsSuccess);
        Assert.Equal("PATCH_AMBIGUOUS", result.ErrorCode);
    }

    [Fact]
    public async Task FileChangeApplier_EmptySearch_ReturnsEmptySearch()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string> { ["f"] = "content" });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        var result = await applier.ApplyPatchAsync("f", "", "replacement");
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_SEARCH", result.ErrorCode);
    }

    // ── FileChangeApplier — Fuzzy matching ───────────────────────────────────

    [Fact]
    public async Task FileChangeApplier_TrailingSpaceInSearch_MatchesFuzzy()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "int x = 1;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        // searchText has trailing space that the file content does not
        var result = await applier.ApplyPatchFuzzyAsync("src/foo.cs", "int x = 1; ", "int x = 42;");
        Assert.True(result.IsSuccess);
        Assert.Equal("int x = 42;", writer.Written["src/foo.cs"]);
    }

    [Fact]
    public async Task FileChangeApplier_CrlfVsLf_MatchesFuzzy()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "line1\nline2\nline3"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        // searchText uses CRLF, file uses LF — fuzzy should still match
        var result = await applier.ApplyPatchFuzzyAsync("src/foo.cs", "line1\r\nline2", "REPLACED");
        Assert.True(result.IsSuccess);
        Assert.Contains("REPLACED", writer.Written["src/foo.cs"]);
    }

    [Fact]
    public async Task FileChangeApplier_IndentationMismatch_MatchesFuzzy()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "    int x = 1;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        // searchText has no leading spaces; file has 4 spaces — fuzzy normalizes both
        var result = await applier.ApplyPatchFuzzyAsync("src/foo.cs", "int x = 1;", "int x = 42;");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task FileChangeApplier_FuzzyAmbiguous_ReturnsAmbiguous()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "int x = 1;\nint x = 1;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        // Two identical lines — fuzzy should report PATCH_AMBIGUOUS
        var result = await applier.ApplyPatchFuzzyAsync("src/foo.cs", "int x = 1; ", "int x = 42;");
        Assert.False(result.IsSuccess);
        Assert.Equal("PATCH_AMBIGUOUS", result.ErrorCode);
    }

    [Fact]
    public async Task FileChangeApplier_FuzzyExactFirst_ExactWins()
    {
        // File has exact match — Pass 1 should succeed without needing fuzzy Pass 2
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "int x = 1;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        var result = await applier.ApplyPatchFuzzyAsync("src/foo.cs", "int x = 1;", "int x = 42;");
        Assert.True(result.IsSuccess);
        Assert.Equal("int x = 42;", writer.Written["src/foo.cs"]);
    }

    // ── FileChangeApplier — Multi-patch ───────────────────────────────────────

    [Fact]
    public async Task FileChangeApplier_TwoPatches_BothApplied()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "int x = 1;\nint y = 2;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        var patches = new List<(string Search, string Replacement)>
        {
            ("int x = 1;", "int x = 10;"),
            ("int y = 2;", "int y = 20;"),
        };

        var result = await applier.ApplyManyPatchesAsync("src/foo.cs", patches);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.AppliedCount);
        Assert.Equal(-1, result.Value.FirstFailedIndex);
        Assert.Contains("int x = 10;", writer.Written["src/foo.cs"]);
        Assert.Contains("int y = 20;", writer.Written["src/foo.cs"]);
    }

    [Fact]
    public async Task FileChangeApplier_ThreePatches_SecondFails_StopsAndReverts()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "int x = 1;\nint y = 2;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        var patches = new List<(string Search, string Replacement)>
        {
            ("int x = 1;",  "int x = 10;"),
            ("int z = 99;", "int z = 0;"),  // not found — should fail
            ("int y = 2;",  "int y = 20;"),
        };

        var result = await applier.ApplyManyPatchesAsync("src/foo.cs", patches);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.AppliedCount);
        Assert.Equal(1, result.Value.FirstFailedIndex);
        Assert.Equal("PATCH_NOT_FOUND", result.Value.FirstFailedErrorCode);
        // File not written — original unchanged
        Assert.False(writer.Written.ContainsKey("src/foo.cs"));
    }

    [Fact]
    public async Task FileChangeApplier_SequentialPatches_SecondUsesResultOfFirst()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "int x = 1;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        // After first patch, "int x = 10;" exists; second patch depends on that
        var patches = new List<(string Search, string Replacement)>
        {
            ("int x = 1;",  "int x = 10;"),
            ("int x = 10;", "int x = 100;"),
        };

        var result = await applier.ApplyManyPatchesAsync("src/foo.cs", patches);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.AppliedCount);
        Assert.Equal("int x = 100;", writer.Written["src/foo.cs"]);
    }

    [Fact]
    public async Task FileChangeApplier_EmptyPatchList_SucceedsWithZero()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/foo.cs"] = "int x = 1;"
        });
        var writer = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        var result = await applier.ApplyManyPatchesAsync("src/foo.cs", new List<(string, string)>());
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.AppliedCount);
        Assert.Equal(-1, result.Value.FirstFailedIndex);
    }

    // ── AgentLoop ─────────────────────────────────────────────────────────────

    // ── AgentLoop — compaction ────────────────────────────────────────────────

    [Fact]
    public async Task AgentLoop_WithSummaryGenerator_SummarizesOldHistory()
    {
        // 15 plain responses accumulate tokens past compactionThreshold=10, then TASK_COMPLETE
        var manyMessages = Enumerable.Range(0, 15)
            .Select(i => new LlmResponse($"thinking step {i}", null, null))
            .Concat([new LlmResponse("TASK_COMPLETE:done!", null, null)]);
        var llm = new ScriptedLlmClient(manyMessages);

        string capturedSummary = "";
        var gate     = new PermissionGate(@"C:\projects");
        var registry = new ToolRegistry(gate);
        var loop     = new AgentLoop(llm, registry,
            maxIterations: 50,
            compactionThreshold: 10,
            recentTurnsToPreserve: 5,
            summaryGenerator: msgs =>
            {
                capturedSummary = $"summarized {msgs.Count} messages";
                return Task.FromResult(capturedSummary);
            });

        var state  = new AgentState { CurrentTask = "Summarize this task" };
        var result = await loop.RunAsync(state);

        Assert.NotEmpty(capturedSummary);
    }

    [Fact]
    public async Task AgentLoop_WithoutSummaryGenerator_FallbackCompactionWorks()
    {
        var llm = new ScriptedLlmClient([
            new LlmResponse("TASK_COMPLETE:done!", null, null)
        ]);
        var gate     = new PermissionGate(@"C:\projects");
        var registry = new ToolRegistry(gate);
        var loop     = new AgentLoop(llm, registry); // no summaryGenerator
        var result   = await loop.RunAsync(new AgentState { CurrentTask = "simple task" });
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsComplete);
    }

    // ── AgentLoop — existing ──────────────────────────────────────────────────

    [Fact]
    public async Task AgentLoop_TaskCompleteSignal_CompletesSuccessfully()
    {
        var llm = new ScriptedLlmClient([
            new LlmResponse("TASK_COMPLETE:Done!", null, null)
        ]);
        var gate = new PermissionGate(@"C:\projects");
        var registry = new ToolRegistry(gate);
        var loop = new AgentLoop(llm, registry);

        var state = new AgentState { CurrentTask = "test task" };
        var result = await loop.RunAsync(state);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsComplete);
        Assert.Equal("Done!", result.Value.FinalResponse);
    }

    [Fact]
    public async Task AgentLoop_ToolCall_ExecutesToolAndContinues()
    {
        var llm = new ScriptedLlmClient([
            new LlmResponse("", "read_file", new Dictionary<string, object> { ["path"] = @"C:\projects\f.cs" }),
            new LlmResponse("TASK_COMPLETE:Read complete", null, null)
        ]);
        var gate = new PermissionGate(@"C:\projects");
        var registry = new ToolRegistry(gate);
        registry.Register(
            new ToolDefinition("read_file", "Read", []),
            _ => Task.FromResult(OperationResult<string>.Ok("file content")));

        var loop = new AgentLoop(llm, registry);
        var state = new AgentState { CurrentTask = "read something" };
        var result = await loop.RunAsync(state);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsComplete);
    }

    [Fact]
    public async Task AgentLoop_MaxIterations_StopsGracefully()
    {
        var responses = Enumerable.Repeat(new LlmResponse("thinking...", null, null), 60).ToList();
        var llm = new ScriptedLlmClient(responses);
        var gate = new PermissionGate(@"C:\projects");
        var registry = new ToolRegistry(gate);
        var loop = new AgentLoop(llm, registry, maxIterations: 3);

        var state = new AgentState { CurrentTask = "infinite task" };
        var result = await loop.RunAsync(state);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsComplete);
        Assert.Equal("MAX_ITERATIONS_EXCEEDED", result.Value.FailureReason);
    }

    // ── CodebaseIndexer — class References fix ────────────────────────────────

    [Fact]
    public async Task CodebaseIndexer_ClassNode_ReferencesContainsDeclaredMethods()
    {
        const string source = """
            namespace Demo
            {
                public class UserService
                {
                    public void GetUser() { }
                    public void SaveUser() { }
                }
            }
            """;
        var reader  = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/UserService.cs"] = source
        });
        var indexer = new CodebaseIndexer(reader);
        var result  = await indexer.IndexDirectoryAsync("src");
        Assert.True(result.IsSuccess);

        var classNode = result.Value!.AllNodes
            .First(n => n.Kind == "Class" && n.Name == "UserService");

        Assert.Contains("GetUser",  classNode.References);
        Assert.Contains("SaveUser", classNode.References);
    }

    [Fact]
    public async Task CodebaseIndexer_ClassNode_DoesNotContainNestedCallTargets()
    {
        // A class that calls external methods — those must NOT appear as class References
        const string source = """
            namespace Demo
            {
                public class OrderService
                {
                    public void PlaceOrder()
                    {
                        _repo.Save();
                        _validator.Validate();
                    }
                }
            }
            """;
        var reader  = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/OrderService.cs"] = source
        });
        var indexer = new CodebaseIndexer(reader);
        var result  = await indexer.IndexDirectoryAsync("src");

        var classNode = result.Value!.AllNodes
            .FirstOrDefault(n => n.Kind == "Class" && n.Name == "OrderService");
        if (classNode is null) return;

        Assert.DoesNotContain("Save",     classNode.References);
        Assert.DoesNotContain("Validate", classNode.References);
    }

    // ── CodebaseIndexer ───────────────────────────────────────────────────────

    [Fact]
    public async Task CodebaseIndexer_CSharpSource_ExtractsClasses()
    {
        const string src = """
            namespace Foo;
            public class MyClass {
                public void DoWork() { Helper(); }
                private void Helper() { }
            }
            """;

        var reader = new InMemoryCodeReader(new Dictionary<string, string>
        {
            ["src/MyClass.cs"] = src
        });

        var indexer = new CodebaseIndexer(reader);
        var result = await indexer.IndexDirectoryAsync("src");

        Assert.True(result.IsSuccess);
        var nodes = result.Value!.AllNodes;
        Assert.Contains(nodes, n => n.Name == "MyClass" && n.Kind == "Class");
    }

    [Fact]
    public async Task CodebaseIndexer_EmptyDirectory_ReturnsEmptyGraph()
    {
        var reader = new InMemoryCodeReader(new Dictionary<string, string>());
        var indexer = new CodebaseIndexer(reader);
        var result = await indexer.IndexDirectoryAsync("src");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.AllNodes);
    }

    [Fact]
    public async Task CodebaseIndexer_ComputesPageRank_ScoresPositive()
    {
        const string src = "public class A { void B(){C();} void C(){} }";
        var reader = new InMemoryCodeReader(new Dictionary<string, string> { ["s/f.cs"] = src });
        var indexer = new CodebaseIndexer(reader);
        var result = await indexer.IndexDirectoryAsync("s");

        Assert.True(result.IsSuccess);
        var graph = result.Value!;
        // All symbols should have a PageRank score
        foreach (var node in graph.AllNodes)
            Assert.True(graph.GetScore(node.Name) >= 0);
    }

    // ── SessionStateStore (SQLite) ────────────────────────────────────────────

    [Fact]
    public async Task SessionStateStore_SaveAndLoad_RoundTrips()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        try
        {
            var store = new SessionStateStore(dbPath);
            var state = new AgentState
            {
                SessionId   = "sess-1",
                CurrentTask = "build feature X",
                MessageHistory = [
                    new ConversationMessage("user", "start"),
                    new ConversationMessage("assistant", "ok")
                ],
                TodoList = [new TodoItem("Write tests")]
            };

            var saveResult = await store.SaveAsync("sess-1", state);
            Assert.True(saveResult.IsSuccess);

            var loadResult = await store.LoadAsync("sess-1");
            Assert.True(loadResult.IsSuccess);
            Assert.Equal("build feature X", loadResult.Value!.CurrentTask);
            Assert.Equal(2, loadResult.Value.MessageHistory.Count);
            Assert.Single(loadResult.Value.TodoList);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task SessionStateStore_ListSessions_ReturnsAllIds()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        try
        {
            var store = new SessionStateStore(dbPath);
            await store.SaveAsync("sess-a", new AgentState { SessionId = "sess-a" });
            await store.SaveAsync("sess-b", new AgentState { SessionId = "sess-b" });

            var listResult = await store.ListSessionsAsync();
            Assert.True(listResult.IsSuccess);
            Assert.Contains("sess-a", listResult.Value!);
            Assert.Contains("sess-b", listResult.Value!);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task SessionStateStore_LoadUnknownSession_ReturnsFailure()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        try
        {
            var store = new SessionStateStore(dbPath);
            var result = await store.LoadAsync("does-not-exist");
            Assert.False(result.IsSuccess);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }
}

// ─── v4.0 tests — StackDetector (E20 Scenario B) ────────────────────────────

public sealed class AgentInfrastructureV4Tests
{
    [Fact]
    public void StackDetector_TypeScript_BeforeNormalization()
    {
        // E20: TypeScript-only markers (export, interface, Promise<, const, => ) — no C# markers
        const string src = "export interface IFetcher { getData(): Promise<void>; } const fn = () => null;";
        var detector = new StackDetector();
        var result   = detector.Detect(src);
        Assert.True(result.IsSuccess);
        Assert.Equal(StackLanguage.TypeScript, result.Value!.Language);
    }

    [Fact]
    public void StackDetector_CSharp_TaskKeyword()
    {
        // E20: "Task<" marker → CSharp; normalization would turn it into "task" and miss it
        var detector = new StackDetector();
        var result   = detector.Detect("public async Task<bool> ValidateAsync() { await Task.Delay(1); return true; }");
        Assert.True(result.IsSuccess);
        Assert.Equal(StackLanguage.CSharp, result.Value!.Language);
    }

    [Fact]
    public void StackDetector_Python_DefKeyword()
    {
        var detector = new StackDetector();
        var result   = detector.Detect("def process(self, data):\n    import json\n    print(data)");
        Assert.True(result.IsSuccess);
        Assert.Equal(StackLanguage.Python, result.Value!.Language);
    }

    [Fact]
    public void StackDetector_Unknown_EmptySource()
    {
        var detector = new StackDetector();
        var result   = detector.Detect("hello world foo bar");
        Assert.True(result.IsSuccess);
        Assert.Equal(StackLanguage.Unknown, result.Value!.Language);
    }

    [Fact]
    public void StackDetector_Mixed_PrefersHighestScore()
    {
        // Source has both C# and TypeScript markers but more C# ones → CSharp
        const string src = "namespace MyApp; using System; async Task<bool> fn() { await Task.Delay(0); }";
        var detector = new StackDetector();
        var result   = detector.Detect(src);
        Assert.True(result.IsSuccess);
        Assert.Equal(StackLanguage.CSharp, result.Value!.Language);
    }
}
