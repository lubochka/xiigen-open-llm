// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Experiments;

// FIX-HCL transfer, plan 53-hierarchical-local-history.md, from HIERARCHICAL-LOCAL-HISTORY-01.
//
// The two-level predictor learns one transition table per cluster. Reading it
// used to be addressed with the local index of the CURRENT token, which belongs
// to the source cluster - a number from a different list. The same integer does
// not mean the same event there. This reader addresses the target cluster's
// table with the last event actually observed in that target cluster, which is
// a state of that table. The counts are untouched; only the address changes.
//
// The training part below is the same code E05 ran inline (lines 93-123 of
// E05HierarchicalPrediction.cs at the measured slice); it is lifted here so the
// reader can be exercised directly, not reimplemented.

// What one read actually did, recorded for evidence. It does not pick the
// answer and holds no copy of the counts.
internal sealed record E05HierarchyRead(
    bool HasGlobal,
    int Global,
    int SourceCluster,
    int PredictedCluster,
    int ReaderInputLocal,
    bool WithinPredictCalled);

// The trained state: cluster transitions, one within-cluster table per cluster,
// and the fixed numbering of members.
internal sealed class E05HierarchyState
{
    internal E05HierarchyState(
        BigramMarkov clusterBigram,
        BigramMarkov?[] withinBigrams,
        IReadOnlyDictionary<int, int> tokenToCluster,
        IReadOnlyDictionary<int, int> tokenToLocal,
        IReadOnlyDictionary<int, List<int>> clusterMembers)
    {
        ClusterBigram = clusterBigram;
        WithinBigrams = withinBigrams;
        TokenToCluster = tokenToCluster;
        TokenToLocal = tokenToLocal;
        ClusterMembers = clusterMembers;
    }

    internal BigramMarkov ClusterBigram { get; }

    internal BigramMarkov?[] WithinBigrams { get; }

    internal IReadOnlyDictionary<int, int> TokenToCluster { get; }

    internal IReadOnlyDictionary<int, int> TokenToLocal { get; }

    internal IReadOnlyDictionary<int, List<int>> ClusterMembers { get; }

    // The within-cluster table itself, so a check can read the already trained
    // row at a chosen address instead of recomputing the algorithm.
    internal BigramMarkov? WithinFor(int cluster)
        => cluster >= 0 && cluster < WithinBigrams.Length ? WithinBigrams[cluster] : null;
}

internal static class E05LocalHierarchy
{
    // The same training E05 performed inline: cluster sequence, per-cluster
    // filter, local numbering, the >= 2 threshold and BigramMarkov.Train.
    internal static E05HierarchyState Fit(
        IReadOnlyList<int> train,
        IReadOnlyDictionary<int, int> tokenToCluster,
        IReadOnlyDictionary<int, List<int>> clusterMembers,
        int k)
    {
        var trainClusters = train.Select(token => tokenToCluster.GetValueOrDefault(token, 0)).ToList();
        var clusterBigram = new BigramMarkov();
        clusterBigram.Train(trainClusters);

        var tokenToLocal = new Dictionary<int, int>();
        foreach (var (_, members) in clusterMembers)
            for (var index = 0; index < members.Count; index++)
                tokenToLocal[members[index]] = index;

        var withinBigrams = new BigramMarkov?[k];
        for (var cluster = 0; cluster < k; cluster++)
        {
            var localSeq = new List<int>();
            for (var i = 0; i < train.Count; i++)
            {
                if (tokenToCluster.GetValueOrDefault(train[i], -1) == cluster)
                    localSeq.Add(tokenToLocal[train[i]]);
            }

            if (localSeq.Count >= 2)
            {
                var within = new BigramMarkov();
                within.Train(localSeq);
                withinBigrams[cluster] = within;
            }
        }

        return new E05HierarchyState(clusterBigram, withinBigrams, tokenToCluster, tokenToLocal, clusterMembers);
    }
}

// Reads the trained state for one observed sequence. History never crosses
// sequences: a new sequence always starts with Reset.
internal sealed class E05HierarchyReader
{
    private readonly E05HierarchyState _state;
    private readonly Dictionary<int, int> _lastObservedLocal = [];
    private int? _currentToken;

    internal E05HierarchyReader(E05HierarchyState state) => _state = state;

    // Record the event that just happened. Only the current token is seen here;
    // no gold answer and no future part of the sequence.
    internal void Observe(int currentToken)
    {
        _currentToken = currentToken;
        var cluster = _state.TokenToCluster.GetValueOrDefault(currentToken, 0);
        _lastObservedLocal[cluster] = _state.TokenToLocal.GetValueOrDefault(currentToken, 0);
    }

    // Forget the sequence. Nothing carries over to the next one.
    internal void Reset()
    {
        _lastObservedLocal.Clear();
        _currentToken = null;
    }

    internal OperationResult<E05HierarchyRead> PredictNext()
    {
        if (_currentToken is not int currentToken)
            return OperationResult<E05HierarchyRead>.Fail("Нет наблюдённого события.");

        var sourceCluster = _state.TokenToCluster.GetValueOrDefault(currentToken, 0);
        var clusterPrediction = _state.ClusterBigram.Predict(sourceCluster);
        if (!clusterPrediction.IsSuccess)
            return OperationResult<E05HierarchyRead>.Fail("Переход между кластерами не предсказан.");

        var predictedCluster = clusterPrediction.Value;
        var within = _state.WithinFor(predictedCluster);
        var sourceLocal = _state.TokenToLocal.GetValueOrDefault(currentToken, 0);
        var readerInput = sourceLocal;
        var predictedLocal = 0;
        var withinCalled = false;

        if (within is not null)
        {
            // The fix: address the target table by the last event actually seen
            // in that same target cluster. Until such an event exists, keep the
            // previous behaviour exactly - this branch was never measured.
            if (_lastObservedLocal.TryGetValue(predictedCluster, out var targetLocal))
                readerInput = targetLocal;

            var localPrediction = within.Predict(readerInput);
            withinCalled = true;
            if (localPrediction.IsSuccess)
                predictedLocal = localPrediction.Value;
        }

        if (!_state.ClusterMembers.TryGetValue(predictedCluster, out var members))
            return OperationResult<E05HierarchyRead>.Fail("У предсказанного кластера нет членов.");

        var hasGlobal = predictedLocal >= 0 && predictedLocal < members.Count;
        return OperationResult<E05HierarchyRead>.Ok(new E05HierarchyRead(
            hasGlobal,
            hasGlobal ? members[predictedLocal] : -1,
            sourceCluster,
            predictedCluster,
            readerInput,
            withinCalled));
    }
}
