// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SemanticCompiler;
using SemanticCompiler.Prediction;

public sealed class E09E10FeatureFlagTests
{
    [Fact]
    public void UseRegexPreprocessorTrueStripsXmlDocCommentsBeforeCompilation()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseRegexPreprocessor = true });

        var compiled = facade.Compile("""
        /// <summary>Remove me before parse.</summary>
        public sealed class Foo
        {
            public void Bar(System.Collections.Generic.List<int> list) { list.Add(1); }
        }
        """);

        Assert.True(compiled.IsSuccess);
        Assert.DoesNotContain("summary", compiled.Value!.NormalizedWords);
    }

    [Fact]
    public void UseRegexPreprocessorFalseKeepsMoreNoiseThanEnabledPath()
    {
        var source = """
        /* keep me only in baseline */
        public sealed class Foo
        {
            public void Bar(System.Collections.Generic.List<int> list) { list.Add(1); var text = "literal words"; }
        }
        """;
        var withFlag = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseRegexPreprocessor = true });
        var withoutFlag = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseRegexPreprocessor = false });

        var stripped = withFlag.Compile(source);
        var baseline = withoutFlag.Compile(source);

        Assert.True(stripped.IsSuccess);
        Assert.True(baseline.IsSuccess);
        Assert.True(stripped.Value!.NormalizedWords.Count <= baseline.Value!.NormalizedWords.Count);
    }

    [Fact]
    public void UseRegexPreprocessorDoesNotBreakGenericNaturalLanguagePrompts()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseRegexPreprocessor = true });

        var compiled = facade.Compile("explain how /// works in C# documentation");

        Assert.True(compiled.IsSuccess);
        Assert.Equal(StackLabel.Generic, compiled.Value!.Stack);
    }

    [Fact]
    public void RegexPreprocessorReducesCommentHeavyCSharpSourceByAtLeastTenPercent()
    {
        var source = """
        /// <summary>Line one should disappear before parsing.</summary>
        /// <remarks>Line two should disappear before parsing.</remarks>
        /// <example>Line three should disappear before parsing.</example>
        public sealed class Foo
        {
            /* block comment should disappear as well */
            public string Bar() { return "literal"; }
        }
        """;

        var normalized = RegexPreprocessor.Normalize(source);

        Assert.True(normalized.Length < source.Length * 0.90);
    }

    [Fact]
    public void RegexPreprocessorKeepsCSharpCompilationSuccessfulAcrossCommentedSnippets()
    {
        var snippets = new[]
        {
            "/// <summary>A</summary>\npublic sealed class A { public void M(System.Collections.Generic.List<int> list) { list.Add(1); } }",
            "public sealed class B { // comment\n public int M(int x, System.Collections.Generic.List<int> list) { list.Add(x); if (x > 0) { return x; } return 1; } }",
            "public sealed class C { /* comment */ public string M(System.Collections.Generic.List<string> list) { list.Add(\"x\"); return \"x\"; } }",
            "/// <summary>D</summary>\npublic sealed class D { public bool M(bool x, System.Collections.Generic.List<int> list) { list.Add(1); if (x) { return true; } return false; } }",
            "public sealed class E { public void M(System.Collections.Generic.List<string> list) { list.Add(\"comment-like // text\"); } }"
        };
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseRegexPreprocessor = true });

        var results = snippets.Select(facade.Compile).ToArray();

        Assert.All(results, result => Assert.True(result.IsSuccess));
    }

    [Fact]
    public void BaselineOnlyDisablesRegexAndStructuralKnnFlags()
    {
        var options = SemanticCompilerOptions.BaselineOnly;

        Assert.False(options.UseRegexPreprocessor);
        Assert.False(options.UseStructuralKnn);
    }

    [Fact]
    public void UseStructuralKnnFalseProducesEmptyPredictedNextTokens()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseStructuralKnn = false });

        var compiled = facade.Compile(StructuralSource());

        Assert.True(compiled.IsSuccess);
        Assert.Empty(compiled.Value!.PredictedNextTokens);
    }

    [Fact]
    public void UseStructuralKnnTrueWithUntrainedPredictorStillSucceedsAndSkipsPrediction()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseStructuralKnn = true });

        var compiled = facade.Compile(StructuralSource());

        Assert.True(compiled.IsSuccess);
        Assert.Empty(compiled.Value!.PredictedNextTokens);
    }

    [Fact]
    public void StructuralKnnPredictor_TrainFromSourcePopulatesTrainingPairs()
    {
        var predictor = new StructuralKnnPredictor();

        predictor.TrainFromSource(StructuralSource());

        Assert.True(predictor.TrainingSize >= 10);
    }

    [Fact]
    public void UseStructuralKnnTrueWithTrainedPredictorPopulatesPredictions()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseStructuralKnn = true });
        facade.TrainStructuralKnn([StructuralSource()]);

        var compiled = facade.Compile(StructuralSource());

        Assert.True(compiled.IsSuccess);
        Assert.NotEmpty(compiled.Value!.PredictedNextTokens);
        Assert.All(compiled.Value.PredictedNextTokens, token => Assert.True(token >= 0));
    }

    [Fact]
    public void StructuralVectorExtractor_ExtractReturnsNonDefaultMethodVector()
    {
        var method = CSharpSyntaxTree.ParseText("public sealed class Foo { public void Bar() { } }")
            .GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();

        var vector = StructuralVectorExtractor.Extract(method);

        Assert.NotEqual(0, vector.AstNodeType);
        Assert.NotEqual(0, vector.Parent1Type);
    }

    [Fact]
    public void StructuralKnnPredictor_PredictIsDeterministicForIdenticalQuery()
    {
        var root = CSharpSyntaxTree.ParseText(StructuralSource()).GetRoot();
        var nodes = root.DescendantNodes().ToArray();
        var pairs = nodes
            .Take(20)
            .Select((node, index) => (StructuralVectorExtractor.Extract(node), index + 1))
            .ToArray();
        var predictor = new StructuralKnnPredictor();
        predictor.Train(pairs);

        var first = predictor.Predict(pairs[3].Item1);
        var second = predictor.Predict(pairs[3].Item1);

        Assert.Equal(first, second);
    }

    private static string StructuralSource()
    {
        return """
        using System.Collections.Generic;
        public sealed class Foo
        {
            public int Bar(List<int> values)
            {
                var total = 0;
                foreach (var value in values)
                {
                    if (value > 0) { total += value; }
                }
                return total;
            }
        }
        """;
    }
}
