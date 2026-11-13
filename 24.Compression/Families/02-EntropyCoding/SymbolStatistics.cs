// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// Counts how often each possible byte value (0-255) appears in a data block.
// Every entropy coder uses this table to decide which symbols get short codes.
public static class SymbolStatistics
{
    // Count occurrences of each byte value. Returns an array of 256 counts.
    public static long[] CountFrequencies(ReadOnlySpan<byte> input)
    {
        var counts = new long[256];
        foreach (byte b in input) counts[b]++;
        return counts;
    }

    // Compute Shannon entropy in bits per symbol.
    // Values near 8.0 mean near-random data (hard to compress).
    // Values near 0.0 mean highly repetitive data (easy to compress).
    public static double ComputeShannonEntropy(long[] frequencies)
    {
        long total = frequencies.Sum();
        if (total == 0) return 0;

        double entropy = 0;
        foreach (long count in frequencies)
        {
            if (count == 0) continue;
            double probability = (double)count / total;
            entropy -= probability * Math.Log2(probability);
        }
        return entropy;
    }

    // Estimate compressed size in bytes for a given entropy and input length.
    public static double EstimatedCompressedBytes(long[] frequencies, long inputLength)
        => Math.Ceiling(inputLength * ComputeShannonEntropy(frequencies) / 8.0);
}
