// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using SemanticCompiler;
using SemanticCompiler.Prediction;

public sealed class E13E14FeatureFlagTests
{
    [Fact]
    public void UseMethodKnnFalseAlwaysLeavesPredictedNextMethodTypeNull()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseMethodKnn = false });

        var compiled = facade.Compile(TwoMethodSource());

        Assert.True(compiled.IsSuccess);
        Assert.Null(compiled.Value!.PredictedNextMethodType);
    }

    [Fact]
    public void UseMethodKnnTrueWithUntrainedPredictorSkipsSilently()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseMethodKnn = true });

        var compiled = facade.Compile(TwoMethodSource());

        Assert.True(compiled.IsSuccess);
        Assert.Null(compiled.Value!.PredictedNextMethodType);
    }

    [Fact]
    public void TrainMethodKnnPopulatesTheFacadePredictor()
    {
        var facade = new SemanticCompilerFacade();

        facade.TrainMethodKnn(MethodTrainingSources());
        var predictor = PrivateField<MethodKnnPredictor>(facade, "_methodKnn");

        Assert.True(predictor.TrainingSize >= 4);
    }

    [Fact]
    public void UseMethodKnnTrueWithTrainedPredictorProducesCanonicalMethodType()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseMethodKnn = true });
        facade.TrainMethodKnn(MethodTrainingSources());

        var compiled = facade.Compile(GuardMethodSource());

        var validTypes = new[] { "Guard", "Loop", "Simple", "Mixed", "ErrorHandling", "Async" };

        Assert.True(compiled.IsSuccess);
        Assert.Contains(compiled.Value!.PredictedNextMethodType, validTypes);
    }

    [Fact]
    public void MethodExtractorReturnsGuardKindBagForNullCheckMethod()
    {
        var extracted = MethodExtractor.Extract(GuardMethodSource());

        Assert.True(extracted.IsSuccess);
        var method = extracted.Value![0];
        Assert.Contains("If", method.KindBag);
        Assert.Contains("Ret", method.KindBag);
        Assert.Equal("Guard", MethodKnnPredictor.CanonicalType(method.KindBag));
    }

    [Fact]
    public void MethodKnnPredictorPredictionIsDeterministicForSameQuery()
    {
        var methods = MethodTrainingSources()
            .SelectMany((source, index) => MethodExtractor.Extract(source, (short)index).Value!)
            .ToArray();
        var predictor = new MethodKnnPredictor();
        predictor.Train(methods);

        var first = predictor.Predict(methods[0]);
        var second = predictor.Predict(methods[0]);

        Assert.Equal(first, second);
    }

    [Fact]
    public void UseFileActionKnnFalseAlwaysLeavesPredictedFileActionNull()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseFileActionKnn = false });

        var compiled = facade.Compile(NullCheckCandidateSource());

        Assert.True(compiled.IsSuccess);
        Assert.Null(compiled.Value!.PredictedFileAction);
    }

    [Fact]
    public void UseFileActionKnnTrueWithUntrainedPredictorSkipsSilently()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseFileActionKnn = true });

        var compiled = facade.Compile(NullCheckCandidateSource());

        Assert.True(compiled.IsSuccess);
        Assert.Null(compiled.Value!.PredictedFileAction);
    }

    [Fact]
    public void ActionLabelGeneratorReturnsAddNullCheckForMemberAccessWithoutGuard()
    {
        var label = ActionLabelGenerator.Label("public sealed class C { public void Process(string s) { var len = s.Length; } }");

        Assert.Equal(ActionLabelGenerator.ADD_NULL_CHECK, label);
    }

    [Fact]
    public void ActionLabelGeneratorReturnsAddLoopForCollectionParameterWithoutLoop()
    {
        var label = ActionLabelGenerator.Label("public sealed class C { public void Process(System.Collections.Generic.IEnumerable<int> items) { var x = 1; } }");

        Assert.Equal(ActionLabelGenerator.ADD_LOOP, label);
    }

    [Fact]
    public void TrainFileActionKnnPopulatesTheFacadePredictor()
    {
        var facade = new SemanticCompilerFacade();

        facade.TrainFileActionKnn(FileActionTrainingSources());
        var predictor = PrivateField<FileActionKnnPredictor>(facade, "_fileActionKnn");

        Assert.True(predictor.TrainingSize >= 4);
    }

    [Fact]
    public void UseFileActionKnnTrueWithTrainedPredictorProducesKnownLabel()
    {
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseFileActionKnn = true });
        facade.TrainFileActionKnn(FileActionTrainingSources());

        var compiled = facade.Compile(NullCheckCandidateSource());

        Assert.True(compiled.IsSuccess);
        Assert.Contains(compiled.Value!.PredictedFileAction, ActionLabelGenerator.Labels);
    }

    private static string TwoMethodSource()
    {
        return """
        public sealed class C
        {
            public void First(System.Collections.Generic.List<int> list) { list.Add(1); }
            public int Second(int x) { if (x > 0) { return x; } return 0; }
        }
        """;
    }

    private static string GuardMethodSource()
    {
        return """
        public sealed class C
        {
            public bool Validate(string s)
            {
                if (s == null) { return false; }
                return true;
            }
        }
        """;
    }

    private static string NullCheckCandidateSource()
    {
        return """
        public sealed class C
        {
            public void Process(string s, System.Collections.Generic.List<int> list)
            {
                var len = s.Length;
                list.Add(len);
            }
        }
        """;
    }

    private static IReadOnlyList<string> MethodTrainingSources()
    {
        return
        [
            """
            public sealed class A
            {
                public void Add(System.Collections.Generic.List<int> list) { list.Add(1); }
                public bool Guard(string s) { if (s == null) { return false; } return true; }
                public int Loop(System.Collections.Generic.List<int> values) { var sum = 0; foreach (var value in values) { sum += value; } return sum; }
            }
            """,
            """
            public sealed class B
            {
                public int First(int x) { if (x > 0) { return x; } return 0; }
                public void Second(System.Collections.Generic.List<int> list) { list.Add(2); }
                public int Third(System.Collections.Generic.List<int> values) { var sum = 0; foreach (var value in values) { sum += value; } return sum; }
            }
            """,
            """
            public sealed class D
            {
                public string First(string s) { if (s == null) { return ""; } return s; }
                public void Second(System.Collections.Generic.List<string> list) { list.Add("x"); }
                public void Third() { try { return; } catch { return; } }
            }
            """
        ];
    }

    private static IReadOnlyList<string> FileActionTrainingSources()
    {
        return
        [
            NullCheckCandidateSource(),
            "public sealed class L { public void Process(System.Collections.Generic.IEnumerable<int> items) { var x = 1; } }",
            "public sealed class M { public void A() { } public void B() { } public void C() { } public void D() { } }",
            "public sealed class N { public int Process(int x) { if (x > 0) { return x; } return 0; } }",
            "public sealed class O { public void Process(System.Collections.Generic.List<int> list) { foreach (var item in list) { var x = item; } } }"
        ];
    }

    private static T PrivateField<T>(object owner, string fieldName)
    {
        var field = owner.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<T>(field.GetValue(owner));
    }
}
