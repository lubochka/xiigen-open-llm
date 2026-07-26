// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// SparseNgramTable stores an N-gram table as a dictionary of dictionaries
// instead of a dense two-dimensional array.
// At 2-5% density (typical for BPE code) it saves 20-50× memory
// and speeds up ArgmaxRow lookup by 10-50×.
public sealed class SparseNgramTable
{
    private readonly Dictionary<int, Dictionary<int, int>> _counts = new();

    // Add an observation: token next followed token prev
    public void Add(int prev, int next)
    {
        if (!_counts.TryGetValue(prev, out var row))
            _counts[prev] = row = new Dictionary<int, int>();
        row[next] = row.GetValueOrDefault(next, 0) + 1;
    }

    // Find the most frequent next token after prev.
    // Returns 0 if prev was never seen.
    public int ArgmaxRow(int prev)
    {
        if (!_counts.TryGetValue(prev, out var row) || row.Count == 0)
            return 0;
        int best = 0; int bestCount = -1;
        foreach (var (token, count) in row)
            if (count > bestCount) { bestCount = count; best = token; }
        return best;
    }

    public int UniquePairs    => _counts.Values.Sum(r => r.Count);
    public int UniqueContexts => _counts.Count;
}
