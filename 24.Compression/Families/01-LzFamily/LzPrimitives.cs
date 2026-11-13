// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// Describes one back-reference: "copy Length bytes starting Distance positions back."
public sealed record LzBackReference(int Distance, int Length);

// One token in the compressed stream — either a raw literal byte or a back-reference.
public readonly struct LzToken
{
    public bool              IsLiteral    { get; init; }
    public byte              LiteralByte  { get; init; }
    public LzBackReference?  BackReference { get; init; }
}

// Scans a byte array backwards for the longest match at the current position.
// This is the inner loop of every Lempel-Ziv compressor.
public static class LzMatchFinder
{
    // Find the longest back-reference starting at currentPosition in the input.
    // Returns null if no match of at least minimumMatchLength bytes was found.
    public static LzBackReference? FindBestMatch(
        ReadOnlySpan<byte> input,
        int currentPosition,
        int windowSize,
        int minimumMatchLength)
    {
        // Furthest back we are allowed to search, bounded by window size and array start.
        int searchStart = Math.Max(0, currentPosition - windowSize);

        int bestDistance = 0;
        int bestLength   = 0;

        for (int candidate = currentPosition - 1; candidate >= searchStart; candidate--)
        {
            // Count how many consecutive bytes match at candidate vs currentPosition.
            int matchLength    = 0;
            int maxMatchLength = input.Length - currentPosition;

            while (matchLength < maxMatchLength
                && input[candidate + matchLength] == input[currentPosition + matchLength])
            {
                matchLength++;
            }

            if (matchLength > bestLength)
            {
                bestLength   = matchLength;
                bestDistance = currentPosition - candidate;
                if (bestLength == maxMatchLength) break; // cannot do better
            }
        }

        return bestLength >= minimumMatchLength
            ? new LzBackReference(bestDistance, bestLength)
            : null;
    }
}
