// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using OssEquivalents;

namespace LlmFromScratch.Tests;

public sealed class OssEquivalentsTests
{
    // ── FrontmatterParser ─────────────────────────────────────────────────────

    [Fact]
    public void FrontmatterParser_WithFrontmatter_ExtractsMetadataAndContent()
    {
        const string md = """
            ---
            name: my-skill
            version: 1
            ---
            # Hello World
            This is the content.
            """;

        var parser = new FrontmatterParser();
        var result = parser.Parse(md);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Metadata.ContainsKey("name"));
        Assert.Contains("Hello World", result.Value.Content);
    }

    [Fact]
    public void FrontmatterParser_NoFrontmatter_ReturnsEmptyMetadata()
    {
        const string md = "# Just content\nNo YAML here.";
        var parser = new FrontmatterParser();
        var result = parser.Parse(md);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Metadata);
        Assert.Contains("Just content", result.Value.Content);
    }

    [Fact]
    public void FrontmatterParser_EmptyString_ReturnsEmpty()
    {
        var parser = new FrontmatterParser();
        var result = parser.Parse("");
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Metadata);
    }

    [Fact]
    public void FrontmatterParser_UnclosedFrontmatter_ReturnsMalformed()
    {
        const string md = "---\nname: bad\n# No closing delimiter";
        var parser = new FrontmatterParser();
        var result = parser.Parse(md);
        Assert.False(result.IsSuccess);
        Assert.Equal("MALFORMED_FRONTMATTER", result.ErrorCode);
    }

    [Fact]
    public void FrontmatterParser_MultipleMetadataFields_AllExtracted()
    {
        const string md = "---\ntitle: test\nauthor: alice\ntags: [a, b]\n---\ncontent";
        var parser = new FrontmatterParser();
        var result = parser.Parse(md);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Metadata.ContainsKey("title"));
        Assert.True(result.Value.Metadata.ContainsKey("author"));
    }

    // ── SkillFileLoader ───────────────────────────────────────────────────────

    [Fact]
    public async Task SkillFileLoader_LoadsMarkdownFiles_ExtractsMetadata()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"skills_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "skill1.md"), "---\nname: skill-one\ndescription: Does X\n---\nContent.");
            File.WriteAllText(Path.Combine(tempDir, "skill2.md"), "---\nname: skill-two\ndescription: Does Y\n---\nOther.");

            var loader = new SkillFileLoader(new FrontmatterParser());
            var result = await loader.LoadDirectoryAsync(tempDir);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.Count);
            Assert.Contains(result.Value!, s => s.Name == "skill-one");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task SkillFileLoader_LoadFullContent_ReturnsBody()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"skills_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "my-skill.md"), "---\nname: my-skill\n---\nThis is the skill body.");

            var loader = new SkillFileLoader(new FrontmatterParser());
            await loader.LoadDirectoryAsync(tempDir);
            var content = await loader.LoadFullContentAsync("my-skill");

            Assert.True(content.IsSuccess);
            Assert.Contains("skill body", content.Value!);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task SkillFileLoader_EmptyDirectory_ReturnsEmpty()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"skills_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var loader = new SkillFileLoader(new FrontmatterParser());
            var result = await loader.LoadDirectoryAsync(tempDir);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value!);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    // ── McpToolDispatcher ─────────────────────────────────────────────────────

    [Fact]
    public async Task McpToolDispatcher_RegisteredTool_DispatchesSuccessfully()
    {
        var dispatcher = new McpToolDispatcher();
        var definition = new McpToolDefinition("echo", "Echoes input", new Dictionary<string, object>());
        dispatcher.Register(definition, args =>
            Task.FromResult(new Dictionary<string, object> { ["output"] = args["input"] }));

        var request = JsonSerializer.Serialize(new
        {
            ToolName = "echo",
            Arguments = new Dictionary<string, object> { ["input"] = "hello" }
        });

        var result = await dispatcher.DispatchAsync(request);
        Assert.True(result.IsSuccess);

        var response = JsonSerializer.Deserialize<McpResponse>(result.Value!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(response);
        Assert.True(response!.Success);
    }

    [Fact]
    public async Task McpToolDispatcher_UnknownTool_ReturnsErrorInResponse()
    {
        var dispatcher = new McpToolDispatcher();
        var request = JsonSerializer.Serialize(new
        {
            ToolName = "nonexistent",
            Arguments = new Dictionary<string, object>()
        });

        var result = await dispatcher.DispatchAsync(request);
        Assert.True(result.IsSuccess); // envelope is ok, but response indicates failure
        var response = JsonSerializer.Deserialize<McpResponse>(result.Value!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.False(response!.Success);
    }

    [Fact]
    public async Task McpToolDispatcher_InvalidJson_ReturnsParseError()
    {
        var dispatcher = new McpToolDispatcher();
        var result = await dispatcher.DispatchAsync("not json at all");
        Assert.False(result.IsSuccess);
        Assert.Equal("PARSE_ERROR", result.ErrorCode);
    }

    [Fact]
    public void McpToolDispatcher_Definitions_ReturnsAll()
    {
        var dispatcher = new McpToolDispatcher();
        dispatcher.Register(new McpToolDefinition("t1", "d1", []), _ => Task.FromResult(new Dictionary<string, object>()));
        dispatcher.Register(new McpToolDefinition("t2", "d2", []), _ => Task.FromResult(new Dictionary<string, object>()));
        Assert.Equal(2, dispatcher.Definitions.Count);
    }

    // ── SchemaValidator ───────────────────────────────────────────────────────

    [Fact]
    public void SchemaValidator_ValidData_ReturnsSuccess()
    {
        var validator = new SchemaValidator();
        var data   = new Dictionary<string, object> { ["name"] = "Alice", ["age"] = 30 };
        var schema = new Dictionary<string, object> { ["name"] = "string", ["age"] = "number" };

        var result = validator.Validate(data, schema);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void SchemaValidator_MissingRequiredField_ReturnsFailure()
    {
        var validator = new SchemaValidator();
        var data   = new Dictionary<string, object> { ["name"] = "Bob" };
        var schema = new Dictionary<string, object> { ["name"] = "string", ["age"] = "number" };

        var result = validator.Validate(data, schema);
        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_FIELD", result.ErrorCode);
    }

    [Fact]
    public void SchemaValidator_OptionalMissingField_IsOk()
    {
        var validator = new SchemaValidator();
        var data   = new Dictionary<string, object> { ["name"] = "Carol" };
        var schema = new Dictionary<string, object> { ["name"] = "string", ["age"] = "number?" };

        var result = validator.Validate(data, schema);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void SchemaValidator_WrongType_ReturnsTypeMismatch()
    {
        var validator = new SchemaValidator();
        var data   = new Dictionary<string, object> { ["age"] = "not-a-number" };
        var schema = new Dictionary<string, object> { ["age"] = "number" };

        var result = validator.Validate(data, schema);
        Assert.False(result.IsSuccess);
        Assert.Equal("TYPE_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public void SchemaValidator_NullData_ReturnsNullData()
    {
        var validator = new SchemaValidator();
        var schema = new Dictionary<string, object> { ["x"] = "string" };
        var result = validator.Validate(null, schema);
        Assert.False(result.IsSuccess);
        Assert.Equal("NULL_DATA", result.ErrorCode);
    }

    // ── StructuredLlmClient / InMemoryStructuredLlmClient ─────────────────────

    [Fact]
    public async Task InMemoryStructuredLlmClient_ValidResponse_ReturnsData()
    {
        var responses = new[] { """{"name":"Alice","score":99}""" };
        var client = new InMemoryStructuredLlmClient(responses);
        var schema = new Dictionary<string, object> { ["name"] = "string" };
        var history = new List<ConversationMessage> { new("user", "go") };

        var result = await client.SendWithSchemaAsync(history, schema);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task InMemoryStructuredLlmClient_InvalidThenValid_RetriesAndSucceeds()
    {
        var responses = new[]
        {
            "bad json",
            """{"name":"Bob"}"""
        };
        var client = new InMemoryStructuredLlmClient(responses);
        var schema = new Dictionary<string, object> { ["name"] = "string" };
        var history = new List<ConversationMessage> { new("user", "go") };

        var result = await client.SendWithSchemaAsync(history, schema, maximumRetryCount: 3);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, client.CallCount);
    }

    [Fact]
    public async Task InMemoryStructuredLlmClient_AllBad_ReturnsExhausted()
    {
        var responses = new[] { "not json", "also bad", "still bad" };
        var client = new InMemoryStructuredLlmClient(responses);
        var schema = new Dictionary<string, object> { ["x"] = "string" };
        var history = new List<ConversationMessage> { new("user", "go") };

        var result = await client.SendWithSchemaAsync(history, schema, maximumRetryCount: 3);

        Assert.False(result.IsSuccess);
        Assert.Equal("SCHEMA_VALIDATION_EXHAUSTED", result.ErrorCode);
    }

    [Fact]
    public async Task StructuredLlmClient_DelegatesToInnerClient()
    {
        var inner = new InMemoryStructuredLlmClient(["""{"ok":true}"""]);
        var validator = new SchemaValidator();
        var client = new StructuredLlmClient(inner, validator);
        var schema = new Dictionary<string, object> { ["ok"] = "boolean" };

        var result = await client.SendAsync(
            new List<ConversationMessage> { new("user", "go") },
            schema);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task InMemoryStructuredLlmClient_MissingSchemaField_Fails()
    {
        var responses = new[] { """{"name":"Alice"}""" };
        var client = new InMemoryStructuredLlmClient(responses);
        var schema = new Dictionary<string, object> { ["name"] = "string", ["required_field"] = "number" };
        var history = new List<ConversationMessage> { new("user", "go") };

        var result = await client.SendWithSchemaAsync(history, schema, maximumRetryCount: 1);
        Assert.False(result.IsSuccess);
    }

    // ── AgentStateMachine + StateCheckpointer ─────────────────────────────────

    [Fact]
    public async Task AgentStateMachine_RunsToTerminal_ReturnsSuccess()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"sm_{Guid.NewGuid()}.db");
        try
        {
            await using var checkpointer = new StateCheckpointer(dbPath);
            var machine = new AgentStateMachine(checkpointer);

            machine.AddNode(new AgentStateNode("start",
                state => Task.FromResult(new Dictionary<string, object>(state) { ["step"] = 1 })));

            machine.AddConditionalEdge("start", _ => true, "end");

            var initial = new Dictionary<string, object> { ["task"] = "hello" };
            var result = await machine.RunAsync(initial, "start", "end");

            Assert.True(result.IsSuccess);
            Assert.Equal(1, (int)result.Value!["step"]);
            Assert.Equal(1, machine.CheckpointsSaved);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task AgentStateMachine_MultipleTransitions_CheckpointsSavedForEach()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"sm_{Guid.NewGuid()}.db");
        try
        {
            await using var checkpointer = new StateCheckpointer(dbPath);
            var machine = new AgentStateMachine(checkpointer);

            machine.AddNode(new AgentStateNode("a",
                s => Task.FromResult(new Dictionary<string, object>(s) { ["a"] = true })));
            machine.AddNode(new AgentStateNode("b",
                s => Task.FromResult(new Dictionary<string, object>(s) { ["b"] = true })));

            machine.AddConditionalEdge("a", _ => true, "b");
            machine.AddConditionalEdge("b", _ => true, "done");

            var result = await machine.RunAsync(new Dictionary<string, object>(), "a", "done");
            Assert.True(result.IsSuccess);
            Assert.Equal(2, machine.CheckpointsSaved);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task AgentStateMachine_NoTransitionMatch_ReturnsFailure()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"sm_{Guid.NewGuid()}.db");
        try
        {
            await using var checkpointer = new StateCheckpointer(dbPath);
            var machine = new AgentStateMachine(checkpointer);

            machine.AddNode(new AgentStateNode("start",
                s => Task.FromResult(new Dictionary<string, object>(s))));
            machine.AddConditionalEdge("start", _ => false, "end"); // never matches

            var result = await machine.RunAsync(new Dictionary<string, object>(), "start", "end");
            Assert.False(result.IsSuccess);
            Assert.Equal("NO_TRANSITION", result.ErrorCode);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task AgentStateMachine_UnknownState_ReturnsUnknownState()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"sm_{Guid.NewGuid()}.db");
        try
        {
            await using var checkpointer = new StateCheckpointer(dbPath);
            var machine = new AgentStateMachine(checkpointer);

            var result = await machine.RunAsync(new Dictionary<string, object>(), "ghost", "end");
            Assert.False(result.IsSuccess);
            Assert.Equal("UNKNOWN_STATE", result.ErrorCode);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    // ── StateCheckpointer ─────────────────────────────────────────────────────

    [Fact]
    public async Task AgentStateMachine_KnowledgeEdge_RoutesToExternalApprovalState()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"sm_{Guid.NewGuid()}.db");
        try
        {
            await using var checkpointer = new StateCheckpointer(dbPath);
            var machine = new AgentStateMachine(checkpointer);

            machine.AddNode(new AgentStateNode("examine",
                state => Task.FromResult(new Dictionary<string, object>(state)
                {
                    [ExternalAiStateGuards.ExaminerConfidenceKey] = 0.31,
                    [ExternalAiStateGuards.MissingCompetenceKey] = "biometric passport verification"
                })));
            machine.AddNode(new AgentStateNode("approval",
                state => Task.FromResult(new Dictionary<string, object>(state)
                {
                    [ExternalAiStateGuards.ApprovalRequestedKey] = true
                })));

            machine.AddConditionalEdge("examine", state => ExternalAiStateGuards.ShouldRequestExternalAiApproval(state), "approval");
            machine.AddConditionalEdge("approval", _ => true, "done");

            var result = await machine.RunAsync([], "examine", "done");

            Assert.True(result.IsSuccess);
            Assert.True((bool)result.Value![ExternalAiStateGuards.ApprovalRequestedKey]);
            Assert.Equal(2, machine.CheckpointsSaved);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public void ExternalAiStateGuards_ApprovalAlreadyRequested_DoesNotFireAgain()
    {
        var state = new Dictionary<string, object>
        {
            [ExternalAiStateGuards.ExaminerConfidenceKey] = 0.21,
            [ExternalAiStateGuards.MissingCompetenceKey] = "biometric passport verification",
            [ExternalAiStateGuards.ApprovalRequestedKey] = true
        };

        Assert.False(ExternalAiStateGuards.ShouldRequestExternalAiApproval(state));
    }

    [Fact]
    public void ExternalAiStateGuards_HighConfidence_ReturnsFalse()
    {
        var state = new Dictionary<string, object>
        {
            [ExternalAiStateGuards.ExaminerConfidenceKey] = 0.87,
            [ExternalAiStateGuards.MissingCompetenceKey] = "biometric passport verification"
        };

        Assert.False(ExternalAiStateGuards.ShouldRequestExternalAiApproval(state));
    }

    [Fact]
    public void ExternalAiStateGuards_MissingCompetenceEmpty_ReturnsFalse()
    {
        var state = new Dictionary<string, object>
        {
            [ExternalAiStateGuards.ExaminerConfidenceKey] = 0.21,
            [ExternalAiStateGuards.MissingCompetenceKey] = ""
        };

        Assert.False(ExternalAiStateGuards.ShouldRequestExternalAiApproval(state));
    }

    [Fact]
    public async Task StateCheckpointer_SaveAndLoadLatest_RoundTrips()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"cp_{Guid.NewGuid()}.db");
        try
        {
            await using var checkpointer = new StateCheckpointer(dbPath);
            var data = new Dictionary<string, object> { ["key"] = "value" };

            var saveResult = await checkpointer.SaveAsync("state-a", data);
            Assert.True(saveResult.IsSuccess);

            var loadResult = await checkpointer.LoadLatestAsync();
            Assert.True(loadResult.IsSuccess);
            Assert.Equal("state-a", loadResult.Value.StateName);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task StateCheckpointer_LoadLatest_EmptyDb_ReturnsEmpty()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"cp_{Guid.NewGuid()}.db");
        try
        {
            await using var checkpointer = new StateCheckpointer(dbPath);
            var result = await checkpointer.LoadLatestAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal("EMPTY", result.ErrorCode);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task StateCheckpointer_MultipleSaves_LoadsLatest()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"cp_{Guid.NewGuid()}.db");
        try
        {
            await using var checkpointer = new StateCheckpointer(dbPath);
            await checkpointer.SaveAsync("first", new Dictionary<string, object> { ["x"] = 1 });
            await checkpointer.SaveAsync("second", new Dictionary<string, object> { ["x"] = 2 });

            var result = await checkpointer.LoadLatestAsync();
            Assert.True(result.IsSuccess);
            Assert.Equal("second", result.Value.StateName);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    // ── RoslynCodeAnalyzer + CallGraphBuilder ─────────────────────────────────

    [Fact]
    public async Task RoslynCodeAnalyzer_CSharpFile_ExtractsMethods()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"roslyn_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Service.cs"),
                "public class Service { public void Execute() { } private void Helper() { } }");

            var analyzer = new RoslynCodeAnalyzer();
            var result = await analyzer.AnalyzeDirectoryAsync(tempDir);

            Assert.True(result.IsSuccess);
            Assert.Contains(result.Value!, sym => sym["name"].ToString() == "Execute");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void CallGraphBuilder_BuildsCalledByRelationship()
    {
        var symbols = new List<Dictionary<string, object>>
        {
            new() { ["name"] = "A", ["kind"] = "Method", ["calls"] = new List<string> { "B" }, ["calledBy"] = new List<string>() },
            new() { ["name"] = "B", ["kind"] = "Method", ["calls"] = new List<string>(),       ["calledBy"] = new List<string>() },
        };

        var builder = new CallGraphBuilder();
        builder.Build(symbols);

        var b = symbols.First(s => s["name"].ToString() == "B");
        var calledBy = (List<string>)b["calledBy"];
        Assert.Contains("A", calledBy);
    }
}

// ─── v4.0 tests — TemplateBeautifier + PromptTranslator ─────────────────────

public sealed class OssEquivalentsV4Tests
{
    // ── TemplateBeautifier (E23) ──────────────────────────────────────────────

    [Fact]
    public void TemplateBeautifier_NullCheckReturn_ExactMatch()
    {
        // E23: SUCCESS=return mapping is the key fix — must produce exact C# output
        var beautifier = new TemplateBeautifier();
        var result = beautifier.Beautify("IF(NULL_CHECK(X)) → RETURN_FAIL", BeautifyTarget.CSharp);
        Assert.True(result.IsSuccess);
        Assert.Equal("if ({X} == null) return false;", result.Value);
    }

    [Fact]
    public void TemplateBeautifier_ValidateSuccess_ExactMatch()
    {
        var beautifier = new TemplateBeautifier();
        var result = beautifier.Beautify("VALIDATE_OP(X) → SUCCESS", BeautifyTarget.CSharp);
        Assert.True(result.IsSuccess);
        Assert.Equal("return Validate({X});", result.Value);
    }

    [Fact]
    public void TemplateBeautifier_UnknownPredicate_ReturnsFailure()
    {
        var beautifier = new TemplateBeautifier();
        var result = beautifier.Beautify("UNKNOWN_OP(X) → ???", BeautifyTarget.CSharp);
        Assert.False(result.IsSuccess);
        Assert.Equal("NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public void TemplateBeautifier_TypeScript_NullCheck()
    {
        var beautifier = new TemplateBeautifier();
        var result = beautifier.Beautify("IF(NULL_CHECK(X)) → RETURN_FAIL", BeautifyTarget.TypeScript);
        Assert.True(result.IsSuccess);
        Assert.Equal("if ({X} === null) return false;", result.Value);
    }

    // ── PromptTranslator (E25 + E26 + E27) ───────────────────────────────────

    [Fact]
    public void PromptTranslator_Russian_Imperative_SOV()
    {
        // E25+E27: Russian imperative "Добавь проверку null" → "Add null check"
        var translator = new PromptTranslator();
        var result = translator.ToEnglish("Добавь проверку null");
        Assert.True(result.IsSuccess);
        Assert.Equal("Add null check", result.Value);
    }

    [Fact]
    public void PromptTranslator_Hebrew_Imperative()
    {
        // E25+E27: Hebrew imperative → same English phrase
        var translator = new PromptTranslator();
        var result = translator.ToEnglish("הוסף בדיקת null");
        Assert.True(result.IsSuccess);
        Assert.Equal("Add null check", result.Value);
    }

    [Fact]
    public void PromptTranslator_English_PassThrough()
    {
        // English input is returned unchanged
        var translator = new PromptTranslator();
        var result = translator.ToEnglish("Add null check");
        Assert.True(result.IsSuccess);
        Assert.Equal("Add null check", result.Value);
    }

    [Fact]
    public void PromptTranslator_ContextDict_Stream_vs_Flow()
    {
        // E26: "потоком" (ADJ before) → "stream" (not "flow") via POS context
        // Direct TranslationDictionary test to verify E26 disambiguation
        var dict = TranslationDictionary.BuildDefault();
        string stream = dict.TranslateWithContext("потоком", new[] { 9 }, Array.Empty<int>());
        string flow   = dict.TranslateWithContext("потоки",  Array.Empty<int>(), new[] { 7 });
        Assert.Equal("stream", stream);
        Assert.Equal("flow", flow);
    }
}

// ── RoslynCodeAnalyzer ────────────────────────────────────────────────────────

public sealed class RoslynCodeAnalyzerTests
{
    [Fact]
    public async Task RoslynCodeAnalyzer_WithClassAndMethod_ExtractsBoth()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "Sample.cs"), """
                namespace Demo
                {
                    /// <summary>A sample service.</summary>
                    public class SampleService
                    {
                        /// <summary>Gets a value.</summary>
                        public string GetValue() => "hello";
                    }
                }
                """);

            var analyzer = new RoslynCodeAnalyzer();
            var result   = await analyzer.AnalyzeDirectoryAsync(tempDir);

            Assert.True(result.IsSuccess);
            Assert.Contains(result.Value!, s => (string)s["name"] == "SampleService"
                                             && (string)s["kind"] == "Class");
            Assert.Contains(result.Value!, s => (string)s["name"] == "GetValue"
                                             && (string)s["kind"] == "Method");
        }
        finally { Directory.Delete(tempDir, recursive: true); }
    }

    [Fact]
    public async Task RoslynCodeAnalyzer_XmlDocComment_ExtractedForClass()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "Doc.cs"), """
                namespace Demo
                {
                    /// <summary>Documented class.</summary>
                    public class DocClass { }
                }
                """);

            var analyzer = new RoslynCodeAnalyzer();
            var result   = await analyzer.AnalyzeDirectoryAsync(tempDir);
            Assert.True(result.IsSuccess);

            var classEntry = result.Value!.First(s => (string)s["name"] == "DocClass");
            var doc        = (string)classEntry["documentation"];
            Assert.Contains("Documented class", doc);
        }
        finally { Directory.Delete(tempDir, recursive: true); }
    }

    [Fact]
    public async Task RoslynCodeAnalyzer_MethodCallsExtracted_PopulatesCallsList()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "Caller.cs"), """
                namespace Demo
                {
                    public class CallerService
                    {
                        public void Run() { Helper(); }
                        private void Helper() { }
                    }
                }
                """);

            var analyzer = new RoslynCodeAnalyzer();
            var result   = await analyzer.AnalyzeDirectoryAsync(tempDir);
            Assert.True(result.IsSuccess);

            var runMethod = result.Value!
                .First(s => (string)s["name"] == "Run" && (string)s["kind"] == "Method");
            var calls = (List<string>)runMethod["calls"];
            Assert.Contains("Helper", calls);
        }
        finally { Directory.Delete(tempDir, recursive: true); }
    }

    [Fact]
    public async Task RoslynCodeAnalyzer_NonexistentDirectory_ReturnsFailure()
    {
        var analyzer = new RoslynCodeAnalyzer();
        var result   = await analyzer.AnalyzeDirectoryAsync("/path/that/does/not/exist");
        Assert.False(result.IsSuccess);
        Assert.Equal("DIR_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task RoslynCodeAnalyzer_EmptyDirectory_ReturnsEmptyList()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var analyzer = new RoslynCodeAnalyzer();
            var result   = await analyzer.AnalyzeDirectoryAsync(tempDir);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value!);
        }
        finally { Directory.Delete(tempDir, recursive: true); }
    }
}
