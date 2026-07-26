// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// Second-order context model for Brotli compression.
// Predicts the next byte based on the two most-recently seen bytes.
// A standard compressor uses only the previous byte (first-order);
// using two bytes ("qu" almost always precedes a vowel in English) gives
// better predictions for text and HTML.
// Reference: https://github.com/google/brotli (MIT license)
public sealed class BrotliContextModel
{
    // contextFrequencies[prev2][prev1][nextByte] = count of observations.
    // 256 × 256 × 256 = 16,777,216 entries — kept as a flat array for efficiency.
    private readonly long[] _contextFrequencies = new long[256 * 256 * 256];

    // Record that byte 'current' was seen after the two-byte context (prev2, prev1).
    public void RecordObservation(byte prev2, byte prev1, byte current)
        => _contextFrequencies[prev2 * 65536 + prev1 * 256 + current]++;

    // Get the frequency distribution for what follows context (prev2, prev1).
    public long[] GetDistribution(byte prev2, byte prev1)
    {
        var dist = new long[256];
        int baseIndex = prev2 * 65536 + prev1 * 256;
        for (int next = 0; next < 256; next++)
            dist[next] = _contextFrequencies[baseIndex + next];
        return dist;
    }

    // Train the model on a sample corpus.
    public void Train(byte[] corpus)
    {
        if (corpus.Length < 3) return;
        for (int i = 2; i < corpus.Length; i++)
            RecordObservation(corpus[i - 2], corpus[i - 1], corpus[i]);
    }
}
