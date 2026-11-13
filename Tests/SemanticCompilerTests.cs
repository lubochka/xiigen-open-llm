// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SemanticCompiler;
using Domain = SemanticCompiler29.Domain;

namespace LlmFromScratch.Tests;

// Gate B: dotnet test --filter "SemanticCompiler"
// All tests must pass (Rule 4 — zero failures).
public class SemanticCompilerTests
{
    // ── StackDetector ─────────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_StackDetector_CSharp_MarkerVoting()
    {
        var label = StackDetector.PredictByMarkers(
            "using linq; Task<IEnumerable<T>> nullable sealed namespace dotnet");
        Assert.Equal(StackLabel.CSharp, label);
    }

    [Fact]
    public void SemanticCompiler_StackDetector_TypeScript_MarkerVoting()
    {
        var label = StackDetector.PredictByMarkers(
            "Promise<T> Injectable decorator angular rxjs observable typescript");
        Assert.Equal(StackLabel.TypeScript, label);
    }

    [Fact]
    public void SemanticCompiler_StackDetector_Generic_WhenNoMarkers()
    {
        var label = StackDetector.PredictByMarkers("some random text with no stack markers");
        Assert.Equal(StackLabel.Generic, label);
    }

    // ── RouteClassifier ───────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_RouteClassifier_Bug_Detected()
    {
        var domain = RouteClassifier.ClassifyByKeywords("fix the null exception error in login");
        Assert.Equal(Domain.BUG, domain);
    }

    [Fact]
    public void SemanticCompiler_RouteClassifier_Plan_Detected()
    {
        var domain = RouteClassifier.ClassifyByKeywords("design the workflow phases for deployment");
        Assert.Equal(Domain.PLAN, domain);
    }

    [Fact]
    public void SemanticCompiler_RouteClassifier_Arch_Detected()
    {
        var domain = RouteClassifier.ClassifyByKeywords(
            "what is the best architecture approach for this service design");
        Assert.Equal(Domain.ARCH, domain);
    }

    [Fact]
    public void SemanticCompiler_RouteClassifier_CodeQ_Fallback()
    {
        var domain = RouteClassifier.ClassifyByKeywords("explain how this method works");
        Assert.Equal(Domain.CODE_Q, domain);
    }

    // ── Decomposer ────────────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_Decomposer_SingleSegment_ShortInput()
    {
        var decomposer = new Decomposer();
        var result = decomposer.Split("fix null check");
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public void SemanticCompiler_Decomposer_EmptyInput_ReturnsSingleSegment()
    {
        var decomposer = new Decomposer();
        var result = decomposer.Split("");
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public void SemanticCompiler_Decomposer_MultiWord_ProducesSegments()
    {
        var decomposer = new Decomposer();
        var result = decomposer.Split(
            "fix the null exception error in login and then refactor the auth service to use dependency injection");
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!);
    }

    // ── StopSignalDetector ────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_StopSignalDetector_Phase_Sequence_Detected()
    {
        int signal = StopSignalDetector.Detect("first implement the model then test it finally deploy");
        Assert.Equal(StopSignalDetector.STOP_PHASES, signal);
    }

    [Fact]
    public void SemanticCompiler_StopSignalDetector_TechTerms_StopsOnTech()
    {
        int signal = StopSignalDetector.Detect("implement async await linq pipeline", false, true);
        Assert.Equal(StopSignalDetector.STOP_TECH, signal);
    }

    [Fact]
    public void SemanticCompiler_StopSignalDetector_Continue_WhenIterative()
    {
        int signal = StopSignalDetector.Detect("review and iterate feedback round", true, false);
        Assert.Equal(StopSignalDetector.CONTINUE, signal);
    }

    // ── DependencyGraph ───────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_DependencyGraph_Default_NoCircle()
    {
        var g = DependencyGraph.BuildDefault();
        // "fetch" is not a stopword → maps to GET_OP → "reader"
        // "create" is not a stopword → maps to CREATE_OP → "coder"
        var segments = new[]
        {
            new PromptSegment("fetch all users from database", 0, 4),
            new PromptSegment("create new user record", 5, 8),
        };
        var result = g.Sort(segments);
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!);
    }

    [Fact]
    public void SemanticCompiler_DependencyGraph_CycleDetected()
    {
        var g = new DependencyGraph();
        g.AddDependency("reader", "coder");
        g.AddDependency("coder", "reader"); // circular
        // "fetch" → GET_OP → "reader"; "update" → UPDATE_OP → "coder"
        var segments = new[]
        {
            new PromptSegment("fetch users", 0, 1),
            new PromptSegment("update record", 2, 3),
        };
        var result = g.Sort(segments);
        Assert.False(result.IsSuccess);
        Assert.Equal("DEPENDENCY_CYCLE", result.ErrorCode);
    }

    // ── PatternClassifier ──────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_PatternClassifier_Imperative_VerbFirst()
    {
        // VERB NOUN → IMPERATIVE_V
        int pattern = PatternClassifier.PredictByRules(new[] { 8, 7 });
        Assert.Equal(PatternClassifier.IMPERATIVE_V, pattern);
    }

    [Fact]
    public void SemanticCompiler_PatternClassifier_SVO_NounVerbNoun()
    {
        // NOUN VERB NOUN → SVO
        int pattern = PatternClassifier.PredictByRules(new[] { 7, 8, 7 });
        Assert.Equal(PatternClassifier.SVO, pattern);
    }

    [Fact]
    public void SemanticCompiler_PatternClassifier_SOV_EndsWithVerb()
    {
        // NOUN NOUN VERB → SOV
        int pattern = PatternClassifier.PredictByRules(new[] { 7, 7, 8 });
        Assert.Equal(PatternClassifier.SOV, pattern);
    }

    // ── PatternReorderer ──────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_PatternReorderer_SOV_ReorderedToSVO()
    {
        var words = new List<TaggedWord>
        {
            new("user", 7),
            new("record", 7),
            new("creates", 8),
        };
        var reordered = PatternReorderer.Reorder(words, PatternClassifier.SOV);
        Assert.True(PatternReorderer.IsSVO(reordered));
    }

    [Fact]
    public void SemanticCompiler_PatternReorderer_SVO_Unchanged()
    {
        var words = new List<TaggedWord>
        {
            new("user", 7),
            new("creates", 8),
            new("record", 7),
        };
        var reordered = PatternReorderer.Reorder(words, PatternClassifier.SVO);
        Assert.Equal(words, reordered);
    }

    // ── ContextualDictionary ──────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_ContextualDictionary_Translate_ReturnsString()
    {
        var dict = ContextualDictionary.BuildDefault();
        string result = dict.TranslateWithContext("запрос", new[] { 7 }, Array.Empty<int>());
        Assert.NotEmpty(result);
    }

    [Fact]
    public void SemanticCompiler_ContextualDictionary_Baseline_ReturnsHighestPriority()
    {
        var dict = ContextualDictionary.BuildDefault();
        string result = dict.TranslateBaseline("запис");
        Assert.NotEmpty(result);
    }

    // ── TemplateBeautifier ────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_TemplateBeautifier_NullGuardFail_CSharp_Found()
    {
        var template = TemplateBeautifier.FindBest(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
            TemplateBeautifier.CSHARP);
        Assert.NotNull(template);
        Assert.Contains("null", template!.Code.ToLowerInvariant());
    }

    [Fact]
    public void SemanticCompiler_TemplateBeautifier_AllTemplates_Exist()
    {
        Assert.NotEmpty(TemplateBeautifier.All);
    }

    // ── CSharpBeautifier ─────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_CSharpBeautifier_Beautify_ReturnsLines()
    {
        var result = CSharpBeautifier.Beautify(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
            StackLabel.CSharp);
        Assert.Contains("null guard", result);
        Assert.Contains("return Fail", result);
    }

    [Fact]
    public void SemanticCompiler_CSharpBeautifier_EmptyPredicates_ReturnsComment()
    {
        var result = CSharpBeautifier.Beautify(Array.Empty<int>(), StackLabel.CSharp);
        Assert.Contains("No predicates", result);
    }

    // ── LogicExtractor ────────────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_LogicExtractor_NullGuard_Detected()
    {
        const string code = "if (input == null) return OperationResult<string>.Fail(\"null\");";
        var result = LogicExtractor.Extract(code);
        Assert.True(result.IsSuccess);
        Assert.Contains(LogicExtractor.NULL_GUARD, result.Value!);
        Assert.Contains(LogicExtractor.FAIL, result.Value!);
    }

    // ── Facade (full pipeline) ─────────────────────────────────────────────────

    [Fact]
    public void SemanticCompiler_Facade_Compile_CSharpSource_Succeeds()
    {
        var compiler = new SemanticCompilerFacade();
        const string source = @"
            namespace MyApp;
            public class UserService
            {
                public OperationResult<User> GetUser(int id)
                {
                    if (id <= 0) return OperationResult<User>.Fail(""invalid id"");
                    var user = _repo.Find(id);
                    if (user == null) return OperationResult<User>.Fail(""not found"");
                    return OperationResult<User>.Ok(user);
                }
            }";

        var result = compiler.Compile(source);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value!.Predicates);
        Assert.NotEmpty(result.Value!.Segments);
    }

    [Fact]
    public void SemanticCompiler_Facade_Compile_EmptySource_Fails()
    {
        var compiler = new SemanticCompilerFacade();
        var result = compiler.Compile("   ");
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_SOURCE", result.ErrorCode);
    }

    [Fact]
    public void SemanticCompiler_Facade_DetectStack_CSharpSource_ReturnsCSharp()
    {
        var compiler = new SemanticCompilerFacade();
        var result = compiler.DetectStack("Task<IEnumerable<T>> nullable linq sealed namespace");
        Assert.True(result.IsSuccess);
        Assert.Equal(StackLabel.CSharp, result.Value);
    }

    [Fact]
    public void SemanticCompiler_Facade_ClassifyDomain_BugPrompt_ReturnsBug()
    {
        var compiler = new SemanticCompilerFacade();
        var result = compiler.ClassifyDomain("fix the null exception in auth service");
        Assert.True(result.IsSuccess);
        Assert.Equal(Domain.BUG, result.Value);
    }

    [Fact]
    public void SemanticCompiler_Facade_BeautifyPredicates_FindsTemplate()
    {
        var compiler = new SemanticCompilerFacade();
        var result = compiler.BeautifyPredicates(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
            StackLabel.CSharp);
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!);
    }

    // E10 RegexPreprocessor

    [Fact]
    public void RegexPreprocessor_Normalize_RemovesXmlDocs()
    {
        const string input = "/// <summary>Does a thing</summary>\npublic void Foo() {}";
        var result = RegexPreprocessor.Normalize(input);
        Assert.DoesNotContain("///", result);
        Assert.Contains("public void Foo()", result);
    }

    [Fact]
    public void RegexPreprocessor_Normalize_RemovesBlockComments()
    {
        const string input = "int x /* inline comment */ = 1;";
        var result = RegexPreprocessor.Normalize(input);
        Assert.DoesNotContain("/*", result);
        Assert.Contains("int x", result);
        Assert.Contains("= 1;", result);
    }

    [Fact]
    public void RegexPreprocessor_Normalize_EmptiesStringLiterals()
    {
        const string input = "var s = \"hello world\";";
        var result = RegexPreprocessor.Normalize(input);
        Assert.Contains("var s = \"\";", result);
    }

    [Fact]
    public void RegexPreprocessor_Normalize_EmptyInput_ReturnsInput()
    {
        Assert.Equal("", RegexPreprocessor.Normalize(""));
        Assert.Equal("   ", RegexPreprocessor.Normalize("   "));
    }

    [Fact]
    public void SemanticCompiler_Facade_Compile_CSharpWithComments_StripsBefore_Normalise()
    {
        const string source = """
            namespace MyApp;

            /// <summary>User service</summary>
            public class UserService
            {
                // get the user by id
                public OperationResult<User> GetById(User user)
                {
                    if (user == null) return OperationResult<User>.Fail("missing");
                    return OperationResult<User>.Ok(user);
                }
            }
            """;
        var compiler = new SemanticCompilerFacade();
        var result = compiler.Compile(source);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.DoesNotContain(result.Value!.Segments, segment => segment.Text.Contains("summary", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SemanticCompiler_Facade_Compile_NaturalLanguage_SkipsRegexPreprocessor()
    {
        const string prompt = "explain how /// works in C# documentation";
        var compiler = new SemanticCompilerFacade();
        var result = compiler.Compile(prompt);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains(result.Value!.Segments, segment => segment.Text.Contains("///", StringComparison.Ordinal));
    }
}
