// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using SemanticCompiler;
using SemanticCompiler.Prediction;

// FIX-KS transfer, plan 35-method-knn-target-scope.md, from METHOD-KNN-TARGET-SCOPE-01.
//
// The predictor guesses the category of the method that follows a query. That category
// used to be derived from every nested node of the target method, so a local function
// or lambda that is merely declared inside it — never called — decided the answer. Two
// of the four training targets are built exactly that way: one hides a guard in an
// uncalled local function, the other hides a try/catch in an uncalled lambda. Both
// methods really just return a number.
//
// The transfer trains the target side from the method's OUTER body instead. The first
// test below measures the effect the way the experiment did, by running both rules over
// the same verbatim material: the old rule answers two of four, the new rule four of
// four. That comparison lives inside the test, so it cannot pass by agreeing with the
// new implementation alone.
//
// Sources are taken verbatim from the experiment's inputs; k = 1 is the mode the
// experiment confirmed, which is why the facade now accepts a predictor.
public sealed class MethodKnnTargetScopeLearningTests
{
    // Four files, two methods each: a "before" method, then the target to be learned.
    private static readonly string[] TrainingSources =
    [
        "class LessonOne { void BeforeFor(){for(int i=0;i<2;i++){}} "
            + "int Target(){int Inner(int x){if(x<0)return 0;return x;}return 7;} }",
        "class LessonTwo { void BeforeWhile(){while(false){}} "
            + "int Target(){System.Func<int,int> f=x=>{try{return x;}catch{return 0;}};return 8;} }",
        "class LessonThree { void BeforeIf(){if(true)return;} int Target(int x){if(x<0)return 0;return x;} }",
        "class LessonFour { void BeforeTry(){try{}catch{}} int Target(){try{return 1;}catch{return 0;}} }"
    ];

    private static readonly string[] QuerySources =
    [
        "class QueryOne { void Query(){for(int q=0;q<3;q++){}} }",
        "class QueryTwo { void Query(){while(false){}} }",
        "class QueryThree { void Query(){if(true)return;} }",
        "class QueryFour { void Query(){try{}catch{}} }"
    ];

    // The same four questions as a real consumer sees them: a C# marker so the
    // facade reads the file as C#, otherwise the answer would come back empty for
    // a reason that has nothing to do with the learned memory.
    private static readonly string[] ConsumerQuerySources =
    [
        "using System; public sealed class QueryOne { public void Query(){for(int q=0;q<3;q++){}} }",
        "using System; public sealed class QueryTwo { public void Query(){while(false){}} }",
        "using System; public sealed class QueryThree { public void Query(){if(true)return;} }",
        "using System; public sealed class QueryFour { public void Query(){try{}catch{}} }"
    ];

    // What the targets really do. The first two are plain returns; the uncalled helper
    // inside them must not colour the answer. The last two are genuine controls: their
    // guard and handler are in the outer body and must survive the change.
    private static readonly string[] Expected = ["Mixed", "Mixed", "Guard", "ErrorHandling"];

    [Fact]
    public void TargetScope_ExcludesUncalledHelpers_AndRaisesCorrectAnswersFromTwoToFour()
    {
        var legacy = new MethodKnnPredictor(k: 1);
        legacy.Train(ExtractLegacy());
        Assert.Equal(4, legacy.TrainingSize);
        var legacyAnswers = QuerySources.Select(source => legacy.Predict(QueryMethod(source))).ToArray();

        var scoped = new MethodKnnPredictor(k: 1);
        var trained = scoped.TrainWithTargetScope(ScopedMaterial());
        Assert.True(trained.IsSuccess);
        Assert.Equal(4, trained.Value);
        var scopedAnswers = QuerySources.Select(source => scoped.Predict(QueryMethod(source))).ToArray();

        // The old rule is wrong exactly where the uncalled helper lives: the loop query
        // inherits the hidden guard, the while query inherits the hidden handler.
        Assert.Equal("Guard", legacyAnswers[0]);
        Assert.Equal("ErrorHandling", legacyAnswers[1]);

        // The new rule answers all four, controls included.
        Assert.Equal(Expected, scopedAnswers);

        var legacyCorrect = legacyAnswers.Where((answer, i) => answer == Expected[i]).Count();
        var scopedCorrect = scopedAnswers.Where((answer, i) => answer == Expected[i]).Count();
        Assert.Equal(2, legacyCorrect);
        Assert.Equal(4, scopedCorrect);
    }

    [Fact]
    public void TargetScope_LeavesTheQuerySideAndPairCountUntouched()
    {
        // Only the target side changes. The methods themselves, their order and their
        // own feature bags must be identical to what the ordinary extractor produces.
        var legacyMethods = ExtractLegacy();
        var material = ScopedMaterial();

        Assert.Equal(legacyMethods.Count, material.Methods.Count);
        for (var index = 0; index < legacyMethods.Count; index++)
        {
            Assert.Equal(legacyMethods[index].PackedString, material.Methods[index].PackedString);
            Assert.Equal(legacyMethods[index].FileId, material.Methods[index].FileId);
            Assert.Equal(legacyMethods[index].IndexInFile, material.Methods[index].IndexInFile);
            Assert.Equal(legacyMethods[index].KindBag, material.Methods[index].KindBag);
        }
    }

    [Fact]
    public void RejectedMaterial_KeepsThePreviouslyLearnedTable()
    {
        // Same protection the admission fix gives the legacy path: a material that does
        // not validate must not replace memory that is still answering.
        var predictor = new MethodKnnPredictor(k: 1);
        Assert.True(predictor.TrainWithTargetScope(ScopedMaterial()).IsSuccess);
        var answersBefore = QuerySources.Select(source => predictor.Predict(QueryMethod(source))).ToArray();

        var good = ScopedMaterial();

        var wrongVersion = new MethodScopeMaterial(
            good.Methods, good.TargetScopes, "some-future-projection/v9");
        AssertRejected(predictor, wrongVersion, "UNSUPPORTED_TARGET_SCOPE_VERSION", answersBefore);

        var duplicated = new MethodScopeMaterial(
            [.. good.Methods, good.Methods[0]], good.TargetScopes);
        AssertRejected(predictor, duplicated, "DUPLICATE_METHOD_KEY", answersBefore);

        var missingScope = new MethodScopeMaterial(
            good.Methods,
            good.TargetScopes.Where(pair => pair.Key != (good.Methods[1].FileId, good.Methods[1].IndexInFile))
                .ToDictionary(pair => pair.Key, pair => pair.Value));
        AssertRejected(predictor, missingScope, "TARGET_SCOPE_MISSING_FOR_METHOD", answersBefore);
    }

    [Fact]
    public void EmptyTargetScope_IsAcceptedButMissingOneIsNot()
    {
        // A method with no body legitimately projects to nothing. An empty set and a
        // missing entry are different things and must stay different.
        var predictor = new MethodKnnPredictor(k: 1);
        var good = ScopedMaterial();
        var emptied = good.TargetScopes.ToDictionary(
            pair => pair.Key,
            pair => pair.Key == (good.Methods[1].FileId, good.Methods[1].IndexInFile)
                ? new HashSet<string>(StringComparer.Ordinal)
                : pair.Value);

        var trained = predictor.TrainWithTargetScope(new MethodScopeMaterial(good.Methods, emptied));
        Assert.True(trained.IsSuccess);
        Assert.Equal(4, trained.Value);

        // An empty target set is the "Simple" category, not a failure.
        Assert.Equal("Simple", predictor.Predict(QueryMethod(QuerySources[0])));
    }

    [Fact]
    public void FacadeIngress_AppliesAdmissionAndDoesNotWipeMemoryOnRefusal()
    {
        // Repair R1 of the 27.10 transfer audit, finding F1.
        //
        // The earlier version of this test finished by training again and checking
        // that the second training succeeded. That would have passed even if the
        // refusal had wiped the table first: the repair fit would simply have put
        // the four pairs back. The memory is now read immediately after the
        // refusal, before anything can restore it.
        var predictor = new MethodKnnPredictor(k: 1);
        var facade = new SemanticCompilerFacade(
            options: new SemanticCompilerOptions { UseMethodKnn = true },
            methodKnnPredictor: predictor);

        var trained = facade.TrainMethodKnnWithTargetScope(TrainingSources);
        Assert.True(trained.IsSuccess);
        Assert.Equal(4, trained.Value);

        // The correct categories come from the fixture above, not from asking the
        // predictor what it happens to think now.
        var answersBefore = QuerySources.Select(source => predictor.Predict(QueryMethod(source))).ToArray();
        Assert.Equal(Expected, answersBefore);
        var pairsBefore = LearnedPairs(predictor);
        Assert.Equal(4, pairsBefore.Count);

        // Four single-method files give zero adjacent pairs, so the batch is refused.
        var refused = facade.TrainMethodKnnWithTargetScope(
        [
            "class NewA { int OnlyA(){return 10;} }",
            "class NewB { int OnlyB(){return 20;} }",
            "class NewC { int OnlyC(){return 30;} }",
            "class NewD { int OnlyD(){return 40;} }"
        ]);
        Assert.False(refused.IsSuccess);
        Assert.Equal("INSUFFICIENT_ADJACENT_METHOD_PAIRS", refused.ErrorCode);

        // No repair fit. A guard that cleared the table and only then returned the
        // refusal is caught right here, by the pairs and by the answers themselves.
        Assert.Equal(pairsBefore, LearnedPairs(predictor));
        Assert.Equal(answersBefore, QuerySources.Select(source => predictor.Predict(QueryMethod(source))).ToArray());

        // And through the real consumer, not only the predictor in isolation. The
        // queries carry a C# marker on purpose: a bare class/int/void file can be
        // read as another stack, which would silently turn the answer into null.
        for (var index = 0; index < ConsumerQuerySources.Length; index++)
        {
            var compiled = facade.Compile(ConsumerQuerySources[index]);
            Assert.True(compiled.IsSuccess);
            Assert.Equal(StackLabel.CSharp, compiled.Value!.Stack);
            Assert.Equal(Expected[index], compiled.Value!.PredictedNextMethodType);
        }
    }

    // Read-only view of what the predictor actually remembers. There is no public
    // snapshot, and the point of this check is exactly that a count is not enough:
    // the pairs themselves must be the same pairs, in the same order.
    private static List<(PackedMethod Current, PackedMethod Next)> LearnedPairs(MethodKnnPredictor predictor)
    {
        var field = typeof(MethodKnnPredictor).GetField("_pairs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.True(field is not null, "не найдено поле памяти _pairs у MethodKnnPredictor");
        var value = field!.GetValue(predictor);
        Assert.True(value is List<(PackedMethod Current, PackedMethod Next)>,
            $"поле памяти имеет неожиданный тип {value?.GetType().Name}");
        return [.. (List<(PackedMethod Current, PackedMethod Next)>)value!];
    }

    private static void AssertRejected(
        MethodKnnPredictor predictor,
        MethodScopeMaterial material,
        string expectedCode,
        string[] answersBefore)
    {
        var result = predictor.TrainWithTargetScope(material);
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);

        // The refusal must be total: the table still answers exactly as it did.
        var answersAfter = QuerySources.Select(source => predictor.Predict(QueryMethod(source))).ToArray();
        Assert.Equal(answersBefore, answersAfter);
    }

    private static List<PackedMethod> ExtractLegacy()
    {
        var methods = new List<PackedMethod>();
        for (short fileId = 0; fileId < TrainingSources.Length; fileId++)
        {
            var extracted = MethodExtractor.Extract(TrainingSources[fileId], fileId);
            Assert.True(extracted.IsSuccess);
            methods.AddRange(extracted.Value!);
        }

        return methods;
    }

    private static MethodScopeMaterial ScopedMaterial()
    {
        var methods = new List<PackedMethod>();
        var scopes = new Dictionary<(short FileId, int IndexInFile), HashSet<string>>();
        for (short fileId = 0; fileId < TrainingSources.Length; fileId++)
        {
            var extracted = MethodExtractor.ExtractWithTargetScope(TrainingSources[fileId], fileId);
            Assert.True(extracted.IsSuccess);
            methods.AddRange(extracted.Value!.Methods);
            foreach (var (key, scope) in extracted.Value!.TargetScopes)
                scopes[key] = scope;
        }

        return new MethodScopeMaterial(methods, scopes);
    }

    private static PackedMethod QueryMethod(string source)
    {
        var extracted = MethodExtractor.Extract(source);
        Assert.True(extracted.IsSuccess);
        return extracted.Value![^1];
    }
}
