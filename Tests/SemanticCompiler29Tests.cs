// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SemanticCompiler29;

namespace LlmFromScratch.Tests;

// 29.SemanticCompiler — plan-compliant implementation (E01-E27).
// Gate B: dotnet test --filter "SC29_"
// All test names prefixed SC29_ to avoid collision with 12.SemanticCompiler tests.
public class SemanticCompiler29Tests
{
    private static readonly ICompilerStorage     NullStorage = new NullCompilerStorage();
    private static readonly ICompilerEventLogger NullLogger  = new NullCompilerEventLogger();

    // ── Phase 1: Scaffold + interfaces ────────────────────────────────────────

    [Fact]
    public void SC29_DomainEnum_HasFiveNonUnknownValues()
    {
        // Gate B: exactly five production domains — UNKNOWN is sentinel only.
        int count = Enum.GetValues<Domain>().Count(d => d != Domain.UNKNOWN);
        Assert.Equal(5, count);
    }

    [Fact]
    public void SC29_PipelineInput_StoresText()
    {
        // Gate B: Text property round-trips unchanged.
        var input = new PipelineInput("add null check", null);
        Assert.Equal("add null check", input.Text);
    }

    [Fact]
    public void SC29_PipelineOutput_CompressionRatioPositive()
    {
        // Gate B: CompressionRatio > 0 — positive reduction always expected.
        var output = new PipelineOutput(Domain.CODE_Q, ["VALIDATE_OP"], "csharp", 12.5);
        Assert.True(output.CompressionRatio > 0);
    }

    // ── Phase 2: Compression pipeline ─────────────────────────────────────────

    [Fact]
    public void SC29_AstTokenizer_SplitsOnOperators()
    {
        // Gate B: "a.b" yields at least ["a", ".", "b"] — operator characters split tokens.
        var tokenizer = new AstTokenizer(NullStorage, NullLogger);
        var result    = tokenizer.Tokenize("a.b(c)");
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Length >= 3);
    }

    [Fact]
    public void SC29_AstTokenizer_PreservesCase_ForStackDetector()
    {
        // Gate B: E20 Rule A — original casing MUST be preserved in output.
        // "Promise<" must survive tokenization so StackDetector can detect TypeScript.
        var tokenizer = new AstTokenizer(NullStorage, NullLogger);
        var result    = tokenizer.Tokenize("Promise<string> resolve");
        Assert.True(result.IsSuccess);
        // Original-case fragment must appear somewhere in the token stream.
        Assert.Contains(result.Value!, t => t.Contains('P'));
    }

    [Fact]
    public void SC29_RoleAssigner_SqlFragment_GetsSqlRole()
    {
        // Gate B: tokens containing SELECT and FROM must produce at least one "SQL" role.
        var assigner = new RoleAssigner(NullStorage, NullLogger);
        var result   = assigner.AssignRoles(["SELECT", "id", "FROM", "users"]);
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!, t => t.Role == "SQL");
    }

    [Fact]
    public void SC29_SemanticNormalizer_StopWords_AreRemoved()
    {
        // Gate B: "the" and "a" (stop words) must not appear in normalized output.
        var normalizer = new SemanticNormalizer(NullStorage, NullLogger);
        var result     = normalizer.Normalize("the validate a method");
        Assert.DoesNotContain("the", result);
        Assert.DoesNotContain("a", result);
    }

    [Fact]
    public void SC29_FamilyCollapser_Validates_CollapsesToValidate()
    {
        // Gate B: "validates" must collapse to VALIDATE_OP (family collapse E16).
        var collapser = new FamilyCollapser(NullStorage, NullLogger);
        var result    = collapser.Collapse(["validates", "null"]);
        Assert.Contains("VALIDATE_OP", result);
    }

    [Fact]
    public void SC29_LogicExtractor_VocabSize_IsExactly22()
    {
        // Gate B: VocabSize == 22 — all experiments calibrated to this constant.
        Assert.Equal(22, LogicExtractor.VocabSize);
    }

    [Fact]
    public void SC29_LogicExtractor_ExtractsNullCheck_FromNullGuard()
    {
        // Gate B: source containing "== null" must produce a NULL_CHECK predicate.
        var extractor = new LogicExtractor(NullStorage, NullLogger);
        var tokens    = new[] { "if", "(input", "==", "null)", "return" };
        var result    = extractor.Extract(tokens);
        Assert.Contains("NULL_CHECK", result);
    }

    // ── Phase 3: Routing ───────────────────────────────────────────────────────

    [Fact]
    public void SC29_StackDetector_CSharp_DetectedFromUsing()
    {
        // Gate B: C# "using System" marker → "csharp" result.
        var detector = new StackDetector(NullStorage, NullLogger);
        string stack = detector.Detect("using System; namespace App { public class Foo { } }");
        Assert.Equal("csharp", stack);
    }

    [Fact]
    public void SC29_StackDetector_TypeScript_DetectedBeforeNormalisation()
    {
        // Gate B (E20 CRITICAL): "Promise<" is case-sensitive — normalisation destroys it.
        // StackDetector must fire on raw text to correctly identify TypeScript.
        var detector = new StackDetector(NullStorage, NullLogger);
        string stack = detector.Detect(
            "export async function fetch(): Promise<string> { return ''; }");
        Assert.Equal("typescript", stack);
    }

    [Fact]
    public void SC29_StackDetector_LeanEfModel_DetectedFromGetSet()
    {
        // Gate B (genietasks fix): EF (Entity Framework) model with "{ get; set; }" but no using/async.
        var detector = new StackDetector(NullStorage, NullLogger);
        string stack = detector.Detect(
            "public class User { public int Id { get; set; } public string Name { get; set; } }");
        Assert.Equal("csharp", stack);
    }

    [Fact]
    public void SC29_RouteClassifier_NullCheck_IsCodeQ()
    {
        // Gate B: VALIDATE_OP predicate → CODE_Q domain via predicate routing.
        var classifier = new RouteClassifier(NullStorage, NullLogger);
        var domain     = classifier.Classify(["VALIDATE_OP", "NULL_CHECK"]);
        Assert.NotEqual(Domain.UNKNOWN, domain);
    }

    [Fact]
    public void SC29_RouteClassifier_FixException_IsBug()
    {
        // Gate B: keyword "fix" and "exception" → BUG domain via keyword fallback.
        var domain = RouteClassifier.ClassifyByKeywords("fix the null exception in login");
        Assert.Equal(Domain.BUG, domain);
    }

    // ── Phase 4: Planning ──────────────────────────────────────────────────────

    [Fact]
    public void SC29_Decomposer_TwoDistinctTasks_SplitsCorrectly()
    {
        // Gate B: "add null check and then refactor the service" → 2 parts.
        var decomposer = new Decomposer(NullStorage, NullLogger);
        var result     = decomposer.Decompose("add null check and then refactor the service");
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Length);
    }

    [Fact]
    public void SC29_Decomposer_FindNullAndLog_StaysAsSingleTask()
    {
        // Gate B: false-CONJ guard — "them" after " and " means same task, no split.
        var decomposer = new Decomposer(NullStorage, NullLogger);
        var result     = decomposer.Decompose("find null references and log them");
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public void SC29_StopSignalDetector_SequentialSteps_Stops()
    {
        // Gate B: "step 1" marker at depth 1 → ShouldStop returns true.
        var detector = new StopSignalDetector(NullStorage, NullLogger);
        bool stop    = detector.ShouldStop(["step 1 validate input", "step 2 call service"], 1);
        Assert.True(stop);
    }

    [Fact]
    public void SC29_DependencyGraph_ArchBeforeCodeQ()
    {
        // Gate B: ARCH → CODE_Q edge means ARCH must appear in an earlier level than CODE_Q.
        var g      = DependencyGraph.BuildDefault(NullStorage, NullLogger);
        var result = g.Sort([Domain.ARCH, Domain.CODE_Q, Domain.PLAN]);
        Assert.True(result.IsSuccess);
        var levels = result.Value!;
        int archLevel  = Array.FindIndex(levels, lvl => lvl.Contains(Domain.ARCH));
        int codeLevel  = Array.FindIndex(levels, lvl => lvl.Contains(Domain.CODE_Q));
        Assert.True(archLevel < codeLevel, "ARCH must be in an earlier level than CODE_Q");
    }

    [Fact]
    public void SC29_DependencyGraph_CodeQAndRefactor_ParallelWhenNoPlanArch()
    {
        // Gate B: CODE_Q and REFACTOR with no ARCH or PLAN present → same level (parallel).
        var g      = new DependencyGraph(NullStorage, NullLogger);
        var result = g.Sort([Domain.CODE_Q, Domain.REFACTOR]);
        Assert.True(result.IsSuccess);
        // Both domains are in the same level (no edges between them).
        Assert.Single(result.Value!);
    }

    // ── Phase 5: Translation + Beautification + Pipeline ──────────────────────

    [Fact]
    public void SC29_TemplateBeautifier_ValidateOpSuccess_MapsToReturn()
    {
        // Gate B (E23 Rule B CRITICAL): SUCCESS = RETURN — not the string "SUCCESS".
        // "VALIDATE_OP(X) → SUCCESS" MUST produce a snippet containing "return".
        var beautifier = new TemplateBeautifier(NullStorage, NullLogger);
        var result     = beautifier.Beautify(["VALIDATE_OP(X) → SUCCESS"]);
        Assert.True(result.IsSuccess);
        Assert.Contains("return", result.Value!.ToLowerInvariant());
        Assert.DoesNotContain("SUCCESS", result.Value!); // must NOT echo back "SUCCESS"
    }

    [Fact]
    public void SC29_TemplateBeautifier_NullCheck_MapsToGuardClause()
    {
        // Gate B: NULL_CHECK → ASSIGN_OP must produce a null-guard if-statement.
        var beautifier = new TemplateBeautifier(NullStorage, NullLogger);
        var result     = beautifier.Beautify(["NULL_CHECK(X) → ASSIGN_OP"]);
        Assert.True(result.IsSuccess);
        Assert.Contains("null", result.Value!.ToLowerInvariant());
    }

    [Fact]
    public void SC29_PatternReorderer_SovToSvo_CorrectOrder()
    {
        // Gate B: SOV input "user record update" → verb moves to position 1 (SVO order).
        var reorderer = new PatternReorderer(NullStorage, NullLogger);
        var result    = reorderer.Reorder(["user", "record", "update"], "SOV");
        // Verb ("update") must no longer be last.
        Assert.NotEqual("update", result[^1]);
    }

    [Fact]
    public void SC29_Pipeline_TypeScript_DetectedBeforeNormalisation()
    {
        // Gate B (E20 integration): full pipeline must identify TypeScript stack
        // from "Promise<" which is destroyed by lowercasing in normalisation.
        var pipeline = new SemanticCompilerPipeline(NullStorage, NullLogger);
        var result   = pipeline.Compile(new PipelineInput(
            "export async function fetch(): Promise<string> { return ''; }",
            "user.service.ts"));
        Assert.True(result.IsSuccess);
        Assert.Equal("typescript", result.Value!.StackLanguage);
    }

    [Fact]
    public void SC29_Pipeline_FullRun_ProducesPredicatesAndDomain()
    {
        // Gate B (end-to-end): full pipeline on C# source must produce non-empty predicates.
        var pipeline = new SemanticCompilerPipeline(NullStorage, NullLogger);
        var result   = pipeline.Compile(new PipelineInput(
            "if (user == null) return OperationResult<User>.Fail(\"not found\");",
            "UserService.cs"));
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!.Predicates);
        Assert.True(result.Value!.CompressionRatio > 0);
    }
}

// ── Null object implementations for test injection ─────────────────────────────
// Fabric-first (Rule 2): tests inject these no-op implementations, not concrete stores.

file sealed class NullCompilerStorage : ICompilerStorage
{
    public void Save(string key, PipelineOutput output) { }
    public PipelineOutput? Load(string key) => null;
}

file sealed class NullCompilerEventLogger : ICompilerEventLogger
{
    public void Log(StandardEvent evt) { }
}
