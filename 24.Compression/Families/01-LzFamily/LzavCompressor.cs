// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// LZAV — Avaneev's 2024 single-header LZ compressor.
// Uses a 4-byte hash with chaining to find longer matches than Snappy's single-entry table.
// Better compression ratio than LZ4 on most data; very simple implementation.
// Reference: https://github.com/avaneev/lzav (MIT license)
public sealed class LzavCompressor : ICompressor
{
    // Window size: how far back we search for matches.
    private const int WindowSize = 65536;

    // Minimum match length — shorter back-references are not worth encoding.
    private const int MinMatchLength = 4;

    // Maximum chain length — how many hash collisions we follow before giving up.
    private const int MaxChainLength = 16;

    public Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return Task.FromResult(OperationResult<CompressionResult>.Fail(
                "LZAV requires at least one byte.", "EMPTY_INPUT"));

        // Output format: 4-byte original length header + compressed data.
        var output = new List<byte>(input.Length);

        // Write original length in little-endian order.
        output.Add((byte)(input.Length));
        output.Add((byte)(input.Length >> 8));
        output.Add((byte)(input.Length >> 16));
        output.Add((byte)(input.Length >> 24));

        // Hash table: each slot holds a chain of recent positions with the same hash.
        var hashTable = new List<int>[65536];

        int position = 0;
        int litStart = 0;

        while (position < input.Length)
        {
            if (position + 4 > input.Length)
            {
                position++;
                continue;
            }

            int hash      = Hash4Bytes(input, position);
            var chain     = hashTable[hash] ??= new List<int>();

            // Find the longest match in the chain.
            int bestLength   = 0;
            int bestDistance = 0;

            foreach (int candidate in chain)
            {
                int distance = position - candidate;
                if (distance > WindowSize) break; // chain is sorted by recency

                int matchLen    = 0;
                int maxMatchLen = Math.Min(input.Length - position, 255 + MinMatchLength);

                while (matchLen < maxMatchLen
                    && input[candidate + matchLen] == input[position + matchLen])
                    matchLen++;

                if (matchLen > bestLength)
                {
                    bestLength   = matchLen;
                    bestDistance = distance;
                }

                if (bestLength >= 64) break; // good enough
            }

            // Prepend current position to the chain (most-recent first).
            chain.Insert(0, position);
            if (chain.Count > MaxChainLength) chain.RemoveAt(chain.Count - 1);

            if (bestLength >= MinMatchLength)
            {
                // Flush literals before emitting back-reference.
                FlushLiterals(output, input, litStart, position - litStart);

                // Emit back-reference: 1-byte length, 2-byte distance.
                output.Add((byte)(bestLength - MinMatchLength)); // adjusted length
                output.Add((byte)(bestDistance & 0xFF));
                output.Add((byte)((bestDistance >> 8) & 0xFF));
                output.Add(0xFF); // marker byte distinguishing copy from literal

                position += bestLength;
                litStart  = position;
            }
            else
            {
                position++;
            }
        }

        // Flush any remaining literals.
        FlushLiterals(output, input, litStart, input.Length - litStart);

        return Task.FromResult(OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = output.ToArray(),
            OriginalLength  = input.Length,
            AlgorithmName   = "LZAV"
        }));
    }

    public Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length < 4)
            return Task.FromResult(OperationResult<byte[]>.Fail("Compressed data too short.", "CORRUPT"));

        int origLen = compressed[0] | (compressed[1] << 8) | (compressed[2] << 16) | (compressed[3] << 24);
        var output  = new byte[origLen];
        int outPos  = 0;
        int inPos   = 4;

        while (inPos < compressed.Length && outPos < origLen)
        {
            // Peek ahead: if the 4th byte is the marker (0xFF), it's a copy.
            if (inPos + 3 < compressed.Length && compressed[inPos + 3] == 0xFF)
            {
                // Back-reference: length (1 byte) + distance (2 bytes) + marker
                int copyLen      = compressed[inPos] + MinMatchLength;
                int copyDistance = compressed[inPos + 1] | (compressed[inPos + 2] << 8);
                inPos += 4;

                if (copyDistance <= 0 || copyDistance > outPos)
                    return Task.FromResult(OperationResult<byte[]>.Fail("Invalid back-reference.", "CORRUPT"));

                int srcPos = outPos - copyDistance;
                for (int i = 0; i < copyLen && outPos < origLen; i++)
                    output[outPos++] = output[srcPos++];
            }
            else
            {
                // Literal block: 1-byte count + raw bytes.
                int litLen = compressed[inPos++];
                if (inPos + litLen > compressed.Length)
                    return Task.FromResult(OperationResult<byte[]>.Fail("Truncated literals.", "CORRUPT"));

                Array.Copy(compressed, inPos, output, outPos, Math.Min(litLen, origLen - outPos));
                outPos += litLen;
                inPos  += litLen;
            }
        }

        return Task.FromResult(OperationResult<byte[]>.Ok(output));
    }

    // Flush a run of literal bytes into the output with a length prefix.
    private static void FlushLiterals(List<byte> output, byte[] input, int start, int length)
    {
        int offset = 0;
        while (offset < length)
        {
            int chunkLen = Math.Min(length - offset, 255);
            output.Add((byte)chunkLen);
            for (int i = 0; i < chunkLen; i++)
                output.Add(input[start + offset + i]);
            offset += chunkLen;
        }
    }

    // Hash 4 bytes at a position to a 16-bit table index.
    private static int Hash4Bytes(byte[] data, int pos)
    {
        uint v = (uint)(data[pos] | (data[pos + 1] << 8) | (data[pos + 2] << 16) | (data[pos + 3] << 24));
        return (int)(((v * 0x9e3779b9) >> 16) & 0xFFFF);
    }
}
