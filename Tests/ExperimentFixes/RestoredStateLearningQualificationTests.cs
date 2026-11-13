// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using TensorCore.Core;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

// Repair R4 of the 27.10 transfer audit: the restored-state learning
// qualification that rows CS, RRD and MB all wait on.
//
// Those three transfers changed arithmetic the training step runs on: the
// causal score block, the row sum over the last axis, and the backward share
// of the mean over the last axis. Their own tests prove the arithmetic and the
// consumer. What none of them did is the thing the audit keeps naming: start
// from a saved state, run the assigned updates, and look at the whole state
// and the answers before and after. Equal losses can hide a different update.
//
// The plan says that when the shared harness does not exist yet, its bounded
// contract is what gets built first. This is that contract, built from product
// types only: a model, its saved parameters, the real supervised step and the
// real optimizer. No experiment host, no fixtures outside the repository.
public sealed class RestoredStateLearningQualificationTests
{
    // Small on purpose: the numbers below are pinned, so the run has to be
    // short enough to stay exactly reproducible and quick.
    private static readonly ModelConfig Config = new()
    {
        VocabSize = 64, ContextLen = 16, HiddenDim = 16,
        NumLayers = 2, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 32
    };

    // Four lessons, four questions, fixed. The last token of each lesson is
    // what the step is asked to predict.
    private static readonly int[][] Lessons =
    [
        [5, 9, 13, 21, 34],
        [6, 10, 14, 22, 35],
        [7, 11, 15, 23, 36],
        [8, 12, 16, 24, 37],
    ];

    // A state to restore from: the same values every time, written by hand
    // rather than drawn at run time, so "restored" means one exact thing.
    private static float[] SavedState()
    {
        var model = new LlmModel(Config);
        var random = new Random(4242);
        var values = new List<float>();
        foreach (var parameter in model.Parameters())
            for (var index = 0; index < parameter.Value.Data.Length; index++)
            {
                var value = parameter.Value.Shape.Rank == 1
                    ? 1f
                    : (float)(random.NextDouble() - 0.5) * 0.2f;
                values.Add(value);
            }

        return [.. values];
    }

    private static LlmModel Restored(float[] saved)
    {
        var model = new LlmModel(Config);
        var cursor = 0;
        foreach (var parameter in model.Parameters())
            for (var index = 0; index < parameter.Value.Data.Length; index++)
                parameter.Value.Data[index] = saved[cursor++];
        Assert.Equal(saved.Length, cursor);
        return model;
    }

    private static float[] Values(LlmModel model)
    {
        var values = new List<float>();
        foreach (var parameter in model.Parameters())
            values.AddRange(parameter.Value.Data);
        return [.. values];
    }

    // One reading of the model: for each question, which token it thinks comes
    // next at the last position. This is the answer, not a loss.
    private static int[] Readout(LlmModel model)
    {
        var answers = new List<int>();
        foreach (var lesson in Lessons)
        {
            var prompt = lesson[..^1];
            var logits = model.Forward(
                new Tensor([.. prompt.Select(token => (float)token)], new TensorShape(1, prompt.Length)),
                training: false);
            var lastRow = logits.Shape.Dimensions[^2] - 1;
            var width = logits.Shape.Dimensions[^1];
            var best = 0;
            for (var candidate = 1; candidate < width; candidate++)
                if (logits.Data[(lastRow * width) + candidate] > logits.Data[(lastRow * width) + best])
                    best = candidate;
            answers.Add(best);
        }

        return [.. answers];
    }

    // The optimizer keeps its own state beside the weights: two running
    // estimates per parameter and the step counter. Naming every parameter
    // lets that state be read back and compared.
    private static IReadOnlyList<AdamWParameterBinding> Bindings(LlmModel model)
        => [.. model.Parameters().Select((parameter, index) =>
            new AdamWParameterBinding($"p{index:D4}", parameter))];

    private static (int[] Before, int[] After, float[] Losses, float[] Final, byte[] Optimizer, int Step) RunProgramme(float[] saved)
    {
        var model = Restored(saved);
        var before = Readout(model);

        var optimizer = new AdamW(0.9f, 0.95f, 0f, 1e-8f);
        var trainer = new Trainer(new TrainingConfig
        {
            BatchSize = 1, SeqLen = Config.ContextLen, WarmupSteps = 0, TotalSteps = 4,
            MaxLr = 0.05f, MinLr = 0.05f, GradAccumSteps = 1, GradClipNorm = 1f, WeightDecay = 0f
        });

        var losses = Lessons
            .Select(lesson => trainer.TrainSupervisedNextTokenStep(model, optimizer, lesson, 1f))
            .ToArray();

        var optimizerState = optimizer.ExportPortableState(Bindings(model));
        return (before, Readout(model), losses, Values(model), optimizerState.ToBytes(), optimizerState.Step);
    }

    [Fact]
    public void RestoredState_FourAssignedUpdates_ChangeTheAnswersAndTheWholeState()
    {
        var saved = SavedState();

        // Restoring must give back exactly what was saved, otherwise everything
        // below measures something else.
        Assert.Equal(saved, Values(Restored(saved)));

        var run = RunProgramme(saved);

        // Training really happened: every step reported a finite loss, and the
        // parameters are no longer the saved ones.
        Assert.Equal(4, run.Losses.Length);
        Assert.All(run.Losses, loss => Assert.True(float.IsFinite(loss) && loss > 0, $"потеря {loss}"));
        Assert.NotEqual(saved, run.Final);

        // The optimizer counted every assigned update, and its running
        // estimates are no longer the empty ones a fresh optimizer carries.
        // A skipped update would show here even if the weights looked plausible.
        Assert.Equal(4, run.Step);
        var fresh = new AdamW(0.9f, 0.95f, 0f, 1e-8f).ExportPortableState(Bindings(Restored(saved)));
        Assert.Equal(0, fresh.Step);
        Assert.NotEqual(fresh.ToBytes(), run.Optimizer);

        // Whether four updates move the answers is measured, not demanded: the
        // plan says this programme is not obliged to learn the rule in four
        // steps, and claiming it did would be the overclaim the audit is about.
        // Measured here: they do move, and they collapse to a single constant,
        // which is recorded in the pinned case below rather than celebrated.
        Assert.Equal(Lessons.Length, run.After.Length);
        Assert.NotEqual(run.Before, run.After);
    }

    [Fact]
    public void TheSameSavedStateAndTheSameProgrammeGiveTheSameNumbersTwice()
    {
        // The qualification is only worth something if it is exactly
        // reproducible: otherwise a later difference could always be blamed on
        // the run rather than on the change.
        var saved = SavedState();
        var first = RunProgramme(saved);
        var second = RunProgramme(saved);

        Assert.Equal(first.Before, second.Before);
        Assert.Equal(first.After, second.After);
        Assert.Equal(first.Losses, second.Losses);
        Assert.Equal(first.Final, second.Final);

        // The optimizer state repeats too, byte for byte. That also covers the
        // random draws the step makes: a different draw would move the weights
        // and with them these bytes, so repeatability here is the statement
        // about the generator that this contract can honestly make.
        Assert.Equal(first.Step, second.Step);
        Assert.Equal(first.Optimizer, second.Optimizer);
    }

    [Fact]
    public void TheProgrammesOwnNumbersArePinned()
    {
        // The four losses of the assigned programme, observed and recorded.
        // They are the number that moves when the arithmetic the step runs on
        // changes — the causal score block, the row sum over the last axis, or
        // the backward share of the mean. Equal answers would not show that;
        // these do.
        var run = RunProgramme(SavedState());

        Assert.Equal(4.114400f, run.Losses[0], 6);
        Assert.Equal(4.238668f, run.Losses[1], 6);
        Assert.Equal(4.180397f, run.Losses[2], 6);
        Assert.Equal(4.341100f, run.Losses[3], 6);

        // The answers, pinned as observed. They move — and they collapse to one
        // constant. That is written down as it is: four updates changed the
        // model, they did not teach it the four lessons, and a constant answer
        // is the opposite of acquisition.
        Assert.Equal([42, 37, 43, 46], run.Before);
        Assert.Equal([22, 22, 22, 22], run.After);
        Assert.Single(run.After.Distinct());
    }
}
