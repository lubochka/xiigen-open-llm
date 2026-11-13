// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using CodeOptimizer;
using Microsoft.CodeAnalysis.CSharp;

namespace LlmFromScratch.Tests;

// Unit + integration tests for sub-project 13 (CodeOptimizer).
// All assertions use in-memory C# source strings — no external service parity.
public class CodeOptimizerTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static OptimizationPipeline MakePipeline(params IOptimizationPass[] passes)
    {
        var reader   = new InMemoryCodeReader([]);
        var reporter = new NullReporter();
        return new OptimizationPipeline(passes, reader, reporter);
    }

    private static OperationResult<OptimizationReport> Analyze(
        string sourceCode,
        OptimizationContext? ctx = null,
        params IOptimizationPass[] passes)
    {
        var pipeline = MakePipeline(passes);
        return pipeline.AnalyzeSource(sourceCode, "test.cs", ctx ?? new OptimizationContext());
    }

    // ──────────────────────────────────────────────────────────────────────────
    // OperationResult<T>
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void OperationResult_Success_HasValue()
    {
        var r = OperationResult<int>.Success(42);
        Assert.True(r.IsSuccess);
        Assert.Equal(42, r.Value);
        Assert.Empty(r.ErrorMessage);
    }

    [Fact]
    public void OperationResult_Failure_HasError()
    {
        var r = OperationResult<int>.Failure("oops");
        Assert.False(r.IsSuccess);
        Assert.Equal("oops", r.ErrorMessage);
    }

    [Fact]
    public void OperationResult_MapError_PreservesMessage()
    {
        var r    = OperationResult<int>.Failure("bad");
        var r2   = r.MapError<string>();
        Assert.False(r2.IsSuccess);
        Assert.Equal("bad", r2.ErrorMessage);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // ConstantFolder
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ConstantFolder_DetectsIntegerFolding()
    {
        const string src = @"
class C { void M() { int x = 2 + 2; } }";
        var result = Analyze(src, passes: new ConstantFolder());
        Assert.True(result.IsSuccess);
        var findings = result.Value!.Findings;
        Assert.Contains(findings, f => f.OriginalCode.Contains("2 + 2"));
        Assert.Contains(findings, f => f.SuggestedReplacement == "4");
    }

    [Fact]
    public void ConstantFolder_NonConstant_NotFlagged()
    {
        const string src = @"
class C { void M(int x) { int y = x + 2; } }";
        var result = Analyze(src, passes: new ConstantFolder());
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Findings);
    }

    [Fact]
    public void ConstantFolder_DisabledByContext_ReturnsEmpty()
    {
        const string src = @"
class C { void M() { int x = 2 + 2; } }";
        var ctx = new OptimizationContext { EnableConstantFolding = false };
        var result = Analyze(src, ctx, new ConstantFolder());
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Findings);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // StrengthReducer
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void StrengthReducer_DetectsMultiplyByPowerOfTwo()
    {
        const string src = @"
class C { void M(int x) { int y = x * 4; } }";
        var result = Analyze(src, passes: new StrengthReducer());
        Assert.True(result.IsSuccess);
        var findings = result.Value!.Findings;
        Assert.Contains(findings, f => f.OriginalCode.Contains("* 4"));
        Assert.Contains(findings, f => f.SuggestedReplacement!.Contains("<< 2"));
    }

    [Fact]
    public void StrengthReducer_NonPowerOfTwo_NotFlagged()
    {
        const string src = @"
class C { void M(int x) { int y = x * 3; } }";
        var result = Analyze(src, passes: new StrengthReducer());
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Findings);
    }

    [Fact]
    public void StrengthReducer_MultiplyBy8_Suggests_Shift3()
    {
        const string src = @"
class C { void M(int count) { int y = count * 8; } }";
        var result = Analyze(src, passes: new StrengthReducer());
        Assert.True(result.IsSuccess);
        var f = result.Value!.Findings.Single();
        Assert.Contains("<< 3", f.SuggestedReplacement);
    }

    [Fact]
    public void StrengthReducer_DisabledByContext_ReturnsEmpty()
    {
        const string src = @"
class C { void M(int x) { int y = x * 2; } }";
        var ctx = new OptimizationContext { EnableStrengthReduction = false };
        var result = Analyze(src, ctx, new StrengthReducer());
        Assert.Empty(result.Value!.Findings);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DeadCodeEliminator
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void DeadCode_AfterReturn_IsDetected()
    {
        const string src = @"
class C {
    void M() {
        return;
        int x = 1;
    }
}";
        var result = Analyze(src, passes: new DeadCodeEliminator());
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!.Findings, f => f.OptimizationType.Contains("unreachable"));
    }

    [Fact]
    public void DeadCode_NormalFlow_NothingFlagged()
    {
        const string src = @"
class C {
    int M() {
        int x = 1;
        return x;
    }
}";
        var result = Analyze(src, passes: new DeadCodeEliminator());
        Assert.True(result.IsSuccess);
        // No unreachable-after-jump findings
        Assert.DoesNotContain(result.Value!.Findings, f => f.OptimizationType.Contains("unreachable"));
    }

    [Fact]
    public void DeadCode_DisabledByContext_ReturnsEmpty()
    {
        const string src = @"
class C { void M() { return; int x = 1; } }";
        var ctx = new OptimizationContext { EnableDeadCodeDetection = false };
        var result = Analyze(src, ctx, new DeadCodeEliminator());
        Assert.Empty(result.Value!.Findings);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CommonSubexpressionEliminator
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CSE_RepeatedCall_IsDetected()
    {
        const string src = @"
class C {
    int M(int[] items) {
        int a = items.Length;
        int b = items.Length;
        return a + b;
    }
}";
        var ctx    = new OptimizationContext { MinimumConfidenceThreshold = 0.5 };
        var result = Analyze(src, ctx, new CommonSubexpressionEliminator());
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!.Findings, f => f.OriginalCode.Contains("Length"));
    }

    [Fact]
    public void CSE_SingleOccurrence_NotFlagged()
    {
        const string src = @"
class C {
    int M(int[] items) { return items.Length; }
}";
        var result = Analyze(src, passes: new CommonSubexpressionEliminator());
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Findings);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LoopOptimizer
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void LoopOptimizer_CountInForCondition_IsDetected()
    {
        const string src = @"
using System.Collections.Generic;
class C {
    void M(List<int> items) {
        for (int i = 0; i < items.Count; i++) { }
    }
}";
        var result = Analyze(src, passes: new LoopOptimizer());
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!.Findings, f => f.OptimizationType.Contains("Count"));
    }

    [Fact]
    public void LoopOptimizer_PreCachedCount_NothingFlagged()
    {
        const string src = @"
class C {
    void M(int[] items) {
        int n = items.Length;
        for (int i = 0; i < n; i++) { }
    }
}";
        // The condition uses 'n', not Count/Length — no finding expected
        var result = Analyze(src, passes: new LoopOptimizer());
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Value!.Findings, f => f.OptimizationType.Contains("Count"));
    }

    [Fact]
    public void LoopOptimizer_DisabledByContext_ReturnsEmpty()
    {
        const string src = @"
using System.Collections.Generic;
class C { void M(List<int> items) { for (int i = 0; i < items.Count; i++) { } } }";
        var ctx = new OptimizationContext { EnableLoopOptimization = false };
        var result = Analyze(src, ctx, new LoopOptimizer());
        Assert.Empty(result.Value!.Findings);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // OptimizationPipeline integration
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Pipeline_AllPasses_CollectsFindings()
    {
        const string src = @"
class C {
    void M(int x) {
        int a = 2 + 2;
        int b = x * 4;
        return;
        int dead = 99;
    }
}";
        var result = Analyze(src, null,
            new ConstantFolder(),
            new StrengthReducer(),
            new DeadCodeEliminator());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Findings.Count >= 3,
            $"Expected >=3 findings, got {result.Value!.Findings.Count}");
    }

    [Fact]
    public void Pipeline_ParsedSuccessfully_TrueForValidCode()
    {
        const string src = "class C { void M() { } }";
        var result = Analyze(src, passes: new ConstantFolder());
        Assert.True(result.Value!.ParsedSuccessfully);
    }

    [Fact]
    public void Pipeline_FindingsOrderedByLine()
    {
        const string src = @"
class C {
    void M(int x) {
        int a = 2 + 3;
        int b = 4 + 5;
    }
}";
        var result = Analyze(src, passes: new ConstantFolder());
        var lines  = result.Value!.Findings.Select(f => f.LineNumber).ToList();
        for (int i = 1; i < lines.Count; i++)
            Assert.True(lines[i] >= lines[i - 1]);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // FileSystemCodeAccess
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void FileSystemCodeAccess_ReadSource_MissingFile_ReturnsFail()
    {
        var access = new FileSystemCodeAccess();
        var result = access.ReadSource("/no/such/file.cs");
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ErrorMessage);
    }

    [Fact]
    public void FileSystemCodeAccess_ListSourceFiles_MissingDir_ReturnsFail()
    {
        var access = new FileSystemCodeAccess();
        var result = access.ListSourceFiles("/no/such/dir");
        Assert.False(result.IsSuccess);
    }
}

// ─── Test helpers ─────────────────────────────────────────────────────────────

file sealed class InMemoryCodeReader(Dictionary<string, string> files) : ICodeReader
{
    public OperationResult<string> ReadSource(string path)
        => files.TryGetValue(path, out var src)
            ? OperationResult<string>.Success(src)
            : OperationResult<string>.Failure($"not found: {path}");

    public OperationResult<IReadOnlyList<string>> ListSourceFiles(string _)
        => OperationResult<IReadOnlyList<string>>.Success([.. files.Keys]);
}

file sealed class NullReporter : IOptimizationReporter
{
    public void ReportFinding(OptimizationFinding _) { }
    public void ReportFileSummary(OptimizationReport _) { }
}
