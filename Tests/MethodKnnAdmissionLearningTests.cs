// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using SemanticCompiler;
using SemanticCompiler.Prediction;

// FIX-KHE transfer, plan 26-method-knn-training-admission.md, from KH-EXT-01.
//
// MethodKnnPredictor.Train clears the pair table before refilling it. The facade used
// to admit a batch by raw method count, so a batch that yields no adjacent same-file
// pairs still reached Train and wiped memory that was still answering queries. The
// experiment measured the table dropping from four pairs to zero, and with it the
// arm's own correct answers from 4/4 to 0/4.
//
// The transfer admits a batch by the number of pairs the predictor can actually learn
// from. These tests pin that behaviour on both sides: an unusable batch must leave the
// learned table and its answers intact, and a usable batch must still retrain.
//
// Two honest boundaries, both verified here rather than assumed:
//
// 1. The experiment's own exam sources cannot be used to read answers through the real
//    facade. StackDetector.PredictByMarkers scores a source by marker words and returns
//    Generic when none match; those sources contain no C# marker, so Compile never
//    reaches the C# branch and the answer is null whatever the table holds. The historical
//    4/4 readouts came from the experiment's own query wrapper, which bypassed detection.
//    That is asserted below as a fact about the consumer path, not worked around silently.
//    The consumer-visible assertions therefore use a query that the facade does route.
// 2. The training and admission batches are used verbatim from the experiment, because
//    that is where the studied factor lives.
public sealed class MethodKnnAdmissionLearningTests
{
    // T1 verbatim: one file, five methods, four adjacent same-file pairs.
    private const string TrainableBatchSource =
        "class Seed { int Start(){return 1;} void Stage1(){for(int i=0;i<2;i++) {}} "
        + "void Stage2(){for(int j=0;j<3;j++) {}} void Stage3(){for(int k=0;k<4;k++) {}} "
        + "void Stage4(){for(int n=0;n<5;n++) {}} }";

    // Repair R1 of the 27.10 transfer audit, finding F2. The second admissible
    // corpus has the same admission shape as the first — one file, five methods,
    // four adjacent pairs — but different targets: If+Ret is the Guard category,
    // not Loop. Presenting the first corpus twice could not tell retraining apart
    // from an implementation that silently ignores every batch after the first.
    // This is a deliberately single-category corpus for REPLACEMENT; it says
    // nothing about generalisation or overall accuracy.
    private const string SecondTrainableBatchSource =
        "public sealed class GuardSeed { int Start(){return 1;} "
        + "void Stage1(int x){if(x<0)return;} void Stage2(int x){if(x<0)return;} "
        + "void Stage3(int x){if(x<0)return;} void Stage4(int x){if(x<0)return;} }";

    // T2 verbatim: four files with one method each, so four methods but zero pairs.
    private static readonly string[] UnusableBatchSources =
    [
        "class NewA { int OnlyA(){return 10;} }",
        "class NewB { int OnlyB(){return 20;} }",
        "class NewC { int OnlyC(){return 30;} }",
        "class NewD { int OnlyD(){return 40;} }"
    ];

    // EXAM verbatim from the experiment. Kept to document, in executable form, that the
    // real facade cannot answer these at all — see boundary 1 above.
    private static readonly string[] ExperimentExamSources =
    [
        "class QueryA { int BeforeStage(){return 7;} }",
        "class QueryB { void BeforeStage(){for(int x=0;x<7;x++) {}} }",
        "class QueryC { void BeforeStage(){for(int x=0;x<8;x++) {int y=x+1;}} }",
        "class QueryD { void BeforeStage(){foreach(int x in new int[]{1,2}) {}} }"
    ];

    // The same loop-shaped query the experiment asked, written so the facade's stack
    // detector actually routes it to the C# branch. The word "sealed" is a C# marker;
    // without a marker the detector returns Generic and no answer is produced. Only the
    // routing signal is added — the method body that drives the prediction is unchanged.
    private const string RoutableLoopQuery =
        "public sealed class QueryB { void BeforeStage(){for(int x=0;x<7;x++) {}} }";

    [Fact]
    public void IneligibleNextBatch_PreservesLearnedMethodPredictions()
    {
        var facade = new SemanticCompilerFacade(
            options: new SemanticCompilerOptions { UseMethodKnn = true });

        // Empty start: the reader needs at least four pairs, so there is no answer yet.
        Assert.Equal(0, TrainingSize(facade));
        Assert.Null(Answer(facade, RoutableLoopQuery));

        // T1 is usable: five methods in one file give four adjacent pairs.
        facade.TrainMethodKnn([TrainableBatchSource]);
        Assert.Equal(4, TrainingSize(facade));

        // The trained table now answers through the real consumer path. The expected
        // value is written out: two unknown answers being equal to each other would
        // also hold if both were wrong, or both null.
        var answerAfterTraining = Answer(facade, RoutableLoopQuery);
        Assert.Equal("Loop", answerAfterTraining);

        // T2 is unusable: four methods, each in its own file, so zero pairs. Before the
        // transfer this batch was admitted and Train wiped the table.
        facade.TrainMethodKnn(UnusableBatchSources);

        // The learned table must survive the rejected batch untouched. This single
        // assertion is what fails against the old admission rule: it dropped to zero.
        Assert.Equal(4, TrainingSize(facade));

        // And the consumer still gets the same literal answer as before the batch.
        Assert.Equal("Loop", Answer(facade, RoutableLoopQuery));
    }

    [Fact]
    public void UsableNextBatch_StillRetrainsTheTable()
    {
        // The transfer must not turn admission into "never retrain": a batch that does
        // contain four adjacent pairs is still accepted and still replaces the table.
        var facade = new SemanticCompilerFacade(
            options: new SemanticCompilerOptions { UseMethodKnn = true });

        facade.TrainMethodKnn([TrainableBatchSource]);
        Assert.Equal(4, TrainingSize(facade));
        // The literal answer, not merely "something non-null": the first corpus
        // teaches loop-shaped targets.
        Assert.Equal("Loop", Answer(facade, RoutableLoopQuery));

        // A second admissible batch with DIFFERENT targets. Train clears and refills,
        // so the size stays at four rather than accumulating to eight — and, the part
        // the old version of this test could not see, the answer itself must change.
        facade.TrainMethodKnn([SecondTrainableBatchSource]);
        Assert.Equal(4, TrainingSize(facade));
        Assert.Equal("Guard", Answer(facade, RoutableLoopQuery));
    }

    [Fact]
    public void ExperimentExamSources_AreNotRoutedToTheCSharpBranch()
    {
        // Boundary 1 stated as a check rather than a comment: the experiment's exam
        // texts carry no C# marker word, so the detector labels them Generic and the
        // facade returns no method prediction however well the table is trained. This
        // is why the historical 4/4 readouts cannot be reproduced through Compile.
        var facade = new SemanticCompilerFacade(
            options: new SemanticCompilerOptions { UseMethodKnn = true });
        facade.TrainMethodKnn([TrainableBatchSource]);
        Assert.Equal(4, TrainingSize(facade));

        foreach (var source in ExperimentExamSources)
        {
            Assert.Equal(StackLabel.Generic, StackDetector.PredictByMarkers(source));
            Assert.Null(Answer(facade, source));
        }

        // The same body with a C# marker added is routed and does produce an answer,
        // which isolates stack detection as the only reason for the nulls above.
        Assert.Equal(StackLabel.CSharp, StackDetector.PredictByMarkers(RoutableLoopQuery));
        Assert.NotNull(Answer(facade, RoutableLoopQuery));
    }

    // Drive the real consumer path: compile the source through the facade and read the
    // predicted next method type the pipeline actually produced.
    private static string? Answer(SemanticCompilerFacade facade, string source)
    {
        var compiled = facade.Compile(source);
        Assert.True(compiled.IsSuccess);
        return compiled.Value!.PredictedNextMethodType;
    }

    // The pair-table size is the state the admission decision protects; it is internal
    // to the facade, so the existing reflection pattern of these tests is reused.
    private static int TrainingSize(SemanticCompilerFacade facade)
    {
        var field = facade.GetType().GetField("_methodKnn",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<MethodKnnPredictor>(field!.GetValue(facade)).TrainingSize;
    }
}
