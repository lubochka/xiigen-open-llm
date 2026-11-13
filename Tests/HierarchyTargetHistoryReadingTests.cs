// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using Experiments;

// FIX-HCL transfer, plan 53-hierarchical-local-history.md, from HIERARCHICAL-LOCAL-HISTORY-01.
//
// The two-level predictor learns one transition table per cluster. Reading it
// used to be addressed with the local index of the CURRENT event — a number
// taken from a different cluster's member list. The same integer there does not
// mean the same event. The transfer addresses the target cluster's table with
// the last event actually observed in that target cluster.
//
// The fixture below is the shape the experiment measured: two clusters of
// orchestration events, validation and persistence, alternating. The counts are
// never recomputed here: where the old address is compared, the test reads the
// already trained row through the same public Predict.
public sealed class HierarchyTargetHistoryReadingTests
{
    // Validation events 10,11,12; persistence events 20,21,22.
    private const int ValidateInput = 10, ValidateSchema = 11, ValidatePolicy = 12;
    private const int PersistInput = 20, PersistSchema = 21, PersistPolicy = 22;

    private const int ValidateCluster = 0, PersistCluster = 1;

    // The learned law: inside persistence, policy is followed by input, and
    // input is followed by schema. Validation alternates with persistence.
    private static readonly int[] Train =
    [
        ValidatePolicy, PersistPolicy, ValidateInput, PersistInput, ValidateSchema, PersistSchema,
        ValidatePolicy, PersistPolicy, ValidateInput, PersistInput, ValidateSchema, PersistSchema,
        ValidatePolicy, PersistPolicy, ValidateInput, PersistInput, ValidateSchema, PersistSchema
    ];

    [Fact]
    public void TargetClusterHistoryAddressesTheRow_WhereTheSourceIndexPointedElsewhere()
    {
        var state = Fit(CanonicalMembers());
        var reader = new E05HierarchyReader(state);
        reader.Reset();

        // The sequence actually observed: persistence policy happened, then a
        // validation event. The next event should be persistence input, because
        // inside persistence policy is followed by input.
        reader.Observe(PersistPolicy);
        reader.PredictNext();
        reader.Observe(ValidateInput);
        var read = reader.PredictNext();

        Assert.True(read.IsSuccess);
        Assert.Equal(PersistCluster, read.Value!.PredictedCluster);
        Assert.True(read.Value.HasGlobal);
        Assert.Equal(PersistInput, read.Value.Global);

        // The address used is the target's own last event, not the source's index.
        var persistMembers = state.ClusterMembers[PersistCluster];
        Assert.Equal(persistMembers.IndexOf(PersistPolicy), read.Value.ReaderInputLocal);
        Assert.NotEqual(state.TokenToLocal[ValidateInput], read.Value.ReaderInputLocal);

        // What the old address would have produced, read from the very same
        // trained table at the index the source event carries. It is a different
        // row and a different, wrong answer — so this test cannot pass vacuously.
        var within = state.WithinFor(PersistCluster);
        Assert.NotNull(within);
        var oldAddress = state.TokenToLocal[ValidateInput];
        var oldPrediction = within!.Predict(oldAddress);
        Assert.True(oldPrediction.IsSuccess);
        Assert.NotEqual(PersistInput, persistMembers[oldPrediction.Value]);
    }

    [Fact]
    public void RenumberingTheTargetMembersDoesNotChangeTheAnswer()
    {
        // The same law, the members listed in a different order. A table read by
        // the target's own state moves with the renumbering; a table read by a
        // foreign index does not.
        foreach (var members in new[] { CanonicalMembers(), PermutedMembers() })
        {
            var state = Fit(members);
            var reader = new E05HierarchyReader(state);
            reader.Reset();
            reader.Observe(PersistPolicy);
            reader.PredictNext();
            reader.Observe(ValidateInput);
            var read = reader.PredictNext();

            Assert.True(read.IsSuccess);
            Assert.Equal(PersistInput, read.Value!.Global);
        }
    }

    [Fact]
    public void WithoutTargetHistoryThePreviousBehaviourIsKeptExactly()
    {
        // This branch was never measured by the experiment, so it must not change:
        // with nothing observed yet in the target cluster the reader still uses
        // the source index, as the original code did.
        var state = Fit(CanonicalMembers());
        var reader = new E05HierarchyReader(state);
        reader.Reset();
        reader.Observe(ValidateInput);
        var read = reader.PredictNext();

        Assert.True(read.IsSuccess);
        Assert.Equal(PersistCluster, read.Value!.PredictedCluster);
        Assert.Equal(state.TokenToLocal[ValidateInput], read.Value.ReaderInputLocal);

        var within = state.WithinFor(PersistCluster);
        var expected = within!.Predict(state.TokenToLocal[ValidateInput]);
        Assert.Equal(state.ClusterMembers[PersistCluster][expected.Value], read.Value.Global);
    }

    [Fact]
    public void ResetDropsTheHistorySoSequencesDoNotLeakIntoEachOther()
    {
        var state = Fit(CanonicalMembers());
        var reader = new E05HierarchyReader(state);

        reader.Reset();
        reader.Observe(PersistPolicy);
        reader.PredictNext();
        reader.Observe(ValidateInput);
        var carried = reader.PredictNext();

        reader.Reset();
        reader.Observe(ValidateInput);
        var fresh = reader.PredictNext();

        // The same query, once with target history and once without, must give
        // the two different addresses — otherwise Reset did nothing.
        Assert.True(carried.IsSuccess && fresh.IsSuccess);
        Assert.NotEqual(carried.Value!.ReaderInputLocal, fresh.Value!.ReaderInputLocal);
        Assert.Equal(state.TokenToLocal[ValidateInput], fresh.Value.ReaderInputLocal);
    }

    [Fact]
    public void ReadingWithinTheSameClusterIsNotBroken()
    {
        // The experiment's own control: when source and target are the same
        // cluster, the target's last event is the current one, so the answer is
        // exactly what it always was.
        int[] sameCluster = [PersistPolicy, PersistInput, PersistSchema, PersistPolicy, PersistInput, PersistSchema];
        var members = new Dictionary<int, List<int>>
        {
            [ValidateCluster] = [ValidateInput],
            [PersistCluster] = [PersistInput, PersistSchema, PersistPolicy]
        };
        var state = E05LocalHierarchy.Fit(sameCluster, ClusterMap(), members, k: 2);
        var reader = new E05HierarchyReader(state);
        reader.Reset();
        reader.Observe(PersistPolicy);
        var read = reader.PredictNext();

        Assert.True(read.IsSuccess);
        Assert.Equal(state.TokenToLocal[PersistPolicy], read.Value!.ReaderInputLocal);
        Assert.Equal(PersistInput, read.Value.Global);
    }

    private static E05HierarchyState Fit(Dictionary<int, List<int>> members)
        => E05LocalHierarchy.Fit(Train, ClusterMap(), members, k: 2);

    private static Dictionary<int, int> ClusterMap() => new()
    {
        [ValidateInput] = ValidateCluster,
        [ValidateSchema] = ValidateCluster,
        [ValidatePolicy] = ValidateCluster,
        [PersistInput] = PersistCluster,
        [PersistSchema] = PersistCluster,
        [PersistPolicy] = PersistCluster
    };

    private static Dictionary<int, List<int>> CanonicalMembers() => new()
    {
        [ValidateCluster] = [ValidateInput, ValidateSchema, ValidatePolicy],
        [PersistCluster] = [PersistInput, PersistSchema, PersistPolicy]
    };

    private static Dictionary<int, List<int>> PermutedMembers() => new()
    {
        [ValidateCluster] = [ValidateInput, ValidateSchema, ValidatePolicy],
        [PersistCluster] = [PersistPolicy, PersistInput, PersistSchema]
    };

    [Fact]
    public void TheRealRunUsesTheRepairedReaderAndItsAccuracyIsPinned()
    {
        // Repair R4 of the 27.10 transfer audit, HCL row. Every other case here
        // drives the fitted state directly. The audit asks for the real run, and
        // a counterexample showed the gap is genuine: putting the reader back to
        // the source index leaves every existing run case green, because they
        // only check that the accuracy lies between zero and one.
        //
        // A fixed input and a pinned number close that. The value was observed,
        // not chosen.
        var tokens = Enumerable.Range(0, 1000).Select(index => index % 100).ToList();
        var run = new Experiments.E05HierarchicalPrediction().Run(tokens);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.Equal(0.8693467, run.Value!.Enhanced.AccuracyFraction, 6);

        // And the hierarchy is doing something: the plain baseline is not the
        // same number, so the pinned value is not simply the baseline passed
        // through.
        Assert.NotEqual(run.Value.Baseline.AccuracyFraction, run.Value.Enhanced.AccuracyFraction, 6);
    }
}
