// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// Run-Length Encoding — replaces consecutive identical values with a (count, value) pair.
// Particularly effective on the MTF output of BWT-compressed data, which contains
// long runs of zeros representing repeated identical characters.
// Reference: bzip2 compress.c (BSD license)
public static class RunLengthEncoder
{
    // Apply RLE to an integer sequence (0-255 range).
    // Each run of identical values is stored as (value, count).
    // Output format: flat array of (value, count) pairs, with 0 as separator.
    public static byte[] Forward(int[] input)
    {
        if (input.Length == 0) return [];

        var output = new List<byte>();
        int i = 0;

        while (i < input.Length)
        {
            int value     = input[i];
            int runLength = 1;

            while (i + runLength < input.Length
                && input[i + runLength] == value
                && runLength < 255)
                runLength++;

            output.Add((byte)value);  // the symbol
            output.Add((byte)runLength); // how many times it repeats

            i += runLength;
        }

        return output.ToArray();
    }

    // Reverse the RLE encoding back to the original integer sequence.
    public static int[] Inverse(byte[] encoded)
    {
        var output = new List<int>();

        for (int i = 0; i + 1 < encoded.Length; i += 2)
        {
            int value     = encoded[i];
            int runLength = encoded[i + 1];
            for (int r = 0; r < runLength; r++)
                output.Add(value);
        }

        return output.ToArray();
    }
}
