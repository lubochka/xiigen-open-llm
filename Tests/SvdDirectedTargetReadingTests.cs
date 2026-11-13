// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using Experiments;

// FIX-SVD transfer, plan 53-hierarchical-history-and-directed-svd.md,
// from SVD-DIRECTED-TARGET-01.
//
// The factorisation M ≈ UΣVᵀ has two sides: L = U√Σ for the source role and
// R = V√Σ for the target role. The code kept only L and scored a candidate with
// L[source]·L[candidate]. That product is symmetric, and a symmetric matrix
// cannot represent a directed transition table: for any symmetric S,
// ‖M−S‖ ≥ ‖M−Mᵀ‖/2. So the direction was not approximated badly — it was
// unrepresentable. Keeping R and scoring L[source]·R[candidate] restores it.
//
// The training below is a directed cycle of orchestration events: receive is
// always followed by validate, validate by persist, persist by receive. Nothing
// ever goes backwards, which is exactly what a symmetric score cannot express.
public sealed class SvdDirectedTargetReadingTests
{
    private const int Receive = 1, Validate = 2, Persist = 3;

    private static IReadOnlyList<int> DirectedCycle()
    {
        var tokens = new List<int>();
        for (var repeat = 0; repeat < 200; repeat++)
        {
            tokens.Add(Receive);
            tokens.Add(Validate);
            tokens.Add(Persist);
        }

        return tokens;
    }

    [Fact]
    public void DirectedReaderFollowsTheCycle_WhereTheSymmetricScoreCannot()
    {
        var factors = E10SvdPmiEmbedding.FitFixedTraining(DirectedCycle());

        // The directed reader answers the directed question: what comes next.
        var directed = E10SvdPmiEmbedding.EvaluateFixedPanel(factors, [Receive, Validate, Persist], directed: true);
        Assert.Equal([Validate, Persist, Receive], directed);

        // The old score on the very same trained factors. It is kept under its
        // own name, and here it is read to show it gives different answers — so
        // this test cannot pass by agreeing with itself.
        var similarity = E10SvdPmiEmbedding.EvaluateFixedPanel(factors, [Receive, Validate, Persist], directed: false);
        Assert.NotEqual(directed, similarity);
    }

    [Fact]
    public void TheSymmetricScoreCannotTellForwardFromBackward()
    {
        var factors = E10SvdPmiEmbedding.FitFixedTraining(DirectedCycle());

        // Why the old reader was wrong in principle, shown on the numbers it
        // actually produces: the source-source score is the same in both
        // directions, so it holds no information about which way time runs.
        var forward = Dot(factors.SourceFactors, factors.SourceFactors, factors, Receive, Validate);
        var backward = Dot(factors.SourceFactors, factors.SourceFactors, factors, Validate, Receive);
        Assert.Equal(forward, backward, 12);

        // The directed score distinguishes them: receive → validate is learned,
        // validate → receive never happens in the training sequence.
        var directedForward = Dot(factors.SourceFactors, factors.TargetFactors, factors, Receive, Validate);
        var directedBackward = Dot(factors.SourceFactors, factors.TargetFactors, factors, Validate, Receive);
        Assert.True(directedForward > directedBackward,
            $"ожидался перевес прямого перехода: {directedForward} против {directedBackward}");
    }

    [Fact]
    public void BothSidesAreStoredAndAreNotTheSameArray()
    {
        var factors = E10SvdPmiEmbedding.FitFixedTraining(DirectedCycle());

        Assert.Equal(500, factors.Vocab);
        Assert.Equal(30, factors.Rank);
        Assert.Equal(factors.Vocab * factors.Rank, factors.SourceFactors.Length);
        Assert.Equal(factors.Vocab * factors.Rank, factors.TargetFactors.Length);

        // The second array is the cost of the fix, named plainly: one more
        // [vocab × rank] of doubles. If the two sides were identical, the whole
        // change would be pointless, so that is asserted too.
        Assert.NotSame(factors.SourceFactors, factors.TargetFactors);
        Assert.False(factors.SourceFactors.SequenceEqual(factors.TargetFactors));
    }

    [Fact]
    public void TheRealRunLearnsTheCycleThroughTheSharedCoreAndTheDirectedReader()
    {
        // Repair R3 of the 27.10 transfer audit, finding F4. Every other case
        // here goes through the fitted-panel seam. None of them touched the
        // real Run, so the run could have kept its own copy of the training
        // core, or quietly gone back to the symmetric reader, and all of them
        // would still have been green.
        //
        // The input is the one the repair plan assigns: exactly 500 identifiers
        // cycling 0 → 1 → 2 → 0. Run splits them 400 / 100 by its own rule, so
        // it trains on 400 and answers 99 questions with each algorithm.
        var tokens = Enumerable.Range(0, 500).Select(i => i % 3).ToList();

        var run = new E10SvdPmiEmbedding().Run(tokens);
        Assert.True(run.IsSuccess, run.ErrorMessage);

        // A fixed cycle is fully learnable, so anything below a perfect score
        // means the consumer is not using what was learned. The expectation is
        // written from the data, not lowered to whatever the run returns.
        Assert.Equal(500, run.Value!.Baseline.DatasetTokenCount);
        Assert.Equal(1.0, run.Value!.Baseline.AccuracyFraction, 6);
        Assert.Equal(1.0, run.Value!.Enhanced.AccuracyFraction, 6);
    }

    [Fact]
    public void SimilarityReaderKeepsItsOldMeaningOnTheSameFactors()
    {
        // Compatibility contract: the previous score is still available and is
        // still computed from the source side on both arguments.
        var factors = E10SvdPmiEmbedding.FitFixedTraining(DirectedCycle());
        var panel = E10SvdPmiEmbedding.EvaluateFixedPanel(factors, [Receive, Validate, Persist], directed: false);

        for (var index = 0; index < panel.Length; index++)
        {
            int query = new[] { Receive, Validate, Persist }[index];
            var expected = Argmax(factors.SourceFactors, factors.SourceFactors, factors, query);
            Assert.Equal(expected, panel[index]);
        }
    }

    private static double Dot(double[] left, double[] right, E10Factorization factors, int a, int b)
    {
        double dot = 0;
        for (var k = 0; k < factors.Rank; k++)
            dot += left[(a * factors.Rank) + k] * right[(b * factors.Rank) + k];
        return dot;
    }

    private static int Argmax(double[] left, double[] right, E10Factorization factors, int cur)
    {
        var best = 0;
        var bestScore = double.MinValue;
        for (var b = 0; b < factors.Vocab; b++)
        {
            var dot = Dot(left, right, factors, cur, b);
            if (dot > bestScore)
            {
                bestScore = dot;
                best = b;
            }
        }

        return best;
    }
}
