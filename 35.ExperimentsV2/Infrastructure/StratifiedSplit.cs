// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// Stratified split: each class is represented proportionally in train and val.
// Fixes random collapse of rare classes into one split.
public static class StratifiedSplit
{
    public static (List<T> Train, List<T> Val) Split<T, TKey>(
        IReadOnlyList<T> examples,
        Func<T, TKey>    keySelector,
        double           trainRatio = 0.8,
        int              seed       = 42)
        where TKey : notnull
    {
        var rng   = new Random(seed);
        var train = new List<T>();
        var val   = new List<T>();

        var byClass = examples
            .GroupBy(keySelector)
            .ToDictionary(g => g.Key, g => g.OrderBy(_ => rng.Next()).ToList());

        foreach (var (_, items) in byClass)
        {
            int nTrain = Math.Max(1, (int)(items.Count * trainRatio));
            train.AddRange(items.Take(nTrain));
            val.AddRange(items.Skip(nTrain));
        }

        train = [.. train.OrderBy(_ => rng.Next())];
        val   = [.. val.OrderBy(_ => rng.Next())];
        return (train, val);
    }
}
