// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// LZ4 compression — designed so the decompressor runs at memory-bandwidth speed.
// Format: each sequence = 1 token byte + optional extra length bytes + literals + 2-byte offset.
// Reference: https://github.com/lz4/lz4 (BSD-2-Clause)
public sealed class Lz4Compressor : ICompressor
{
    private const int HashBits        = 16;
    private const int HashTableSize   = 1 << HashBits;
    private const int MinMatchLength  = 4;
    private const int WindowSize      = 65535;
    private const int MfLimit         = 5; // minimum bytes needed to start a new sequence

    public Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return Task.FromResult(OperationResult<CompressionResult>.Fail(
                "LZ4 requires at least one byte.", "EMPTY_INPUT"));

        // Write 4-byte little-endian original length header.
        var output  = new byte[input.Length + (input.Length / 255) + 16];
        int outPos  = 0;

        output[outPos++] = (byte)(input.Length);
        output[outPos++] = (byte)(input.Length >> 8);
        output[outPos++] = (byte)(input.Length >> 16);
        output[outPos++] = (byte)(input.Length >> 24);

        var hashTable = new int[HashTableSize];
        Array.Fill(hashTable, -1);

        int inPos    = 0;
        int litStart = 0;

        // Main loop: process until too few bytes remain for a full sequence.
        while (inPos + MfLimit < input.Length)
        {
            int hash      = Lz4Hash(input, inPos);
            int candidate = hashTable[hash];
            hashTable[hash] = inPos;

            // Check if we have a valid 4-byte match within the window.
            bool hasMatch = candidate >= 0
                && inPos - candidate <= WindowSize
                && input[candidate]     == input[inPos]
                && input[candidate + 1] == input[inPos + 1]
                && input[candidate + 2] == input[inPos + 2]
                && input[candidate + 3] == input[inPos + 3];

            if (!hasMatch) { inPos++; continue; }

            // Extend the match.
            int matchLen = 4;
            while (inPos + matchLen < input.Length
                && input[candidate + matchLen] == input[inPos + matchLen])
                matchLen++;

            int litLen   = inPos - litStart;
            int offset   = inPos - candidate;
            int adjMatch = matchLen - MinMatchLength;

            // Encode the token byte: high nibble = literal count, low nibble = match length.
            byte token = (byte)((Math.Min(15, litLen) << 4) | Math.Min(15, adjMatch));
            output[outPos++] = token;

            // Encode extra literal length bytes if litLen >= 15.
            if (litLen >= 15)
            {
                int extra = litLen - 15;
                while (extra >= 255) { output[outPos++] = 255; extra -= 255; }
                output[outPos++] = (byte)extra;
            }

            // Write literal bytes.
            Array.Copy(input, litStart, output, outPos, litLen);
            outPos += litLen;

            // Write 2-byte little-endian match offset.
            output[outPos++] = (byte)(offset & 0xFF);
            output[outPos++] = (byte)((offset >> 8) & 0xFF);

            // Encode extra match length bytes if adjMatch >= 15.
            if (adjMatch >= 15)
            {
                int extra = adjMatch - 15;
                while (extra >= 255) { output[outPos++] = 255; extra -= 255; }
                output[outPos++] = (byte)extra;
            }

            inPos    += matchLen;
            litStart  = inPos;
        }

        // Emit the last literal sequence (no match follows it).
        {
            int litLen = input.Length - litStart;
            byte token = (byte)(Math.Min(15, litLen) << 4);
            output[outPos++] = token;

            if (litLen >= 15)
            {
                int extra = litLen - 15;
                while (extra >= 255) { output[outPos++] = 255; extra -= 255; }
                output[outPos++] = (byte)extra;
            }

            Array.Copy(input, litStart, output, outPos, litLen);
            outPos += litLen;
        }

        return Task.FromResult(OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = output[..outPos],
            OriginalLength  = input.Length,
            AlgorithmName   = "LZ4"
        }));
    }

    public Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length < 4)
            return Task.FromResult(OperationResult<byte[]>.Fail("Data too short.", "CORRUPT"));

        int origLen = compressed[0] | (compressed[1] << 8) | (compressed[2] << 16) | (compressed[3] << 24);
        var output  = new byte[origLen];
        int outPos  = 0;
        int inPos   = 4;

        while (inPos < compressed.Length)
        {
            byte token = compressed[inPos++];

            // Read literal length.
            int litLen = (token >> 4) & 0x0F;
            if (litLen == 15)
            {
                byte extra;
                do { extra = compressed[inPos++]; litLen += extra; }
                while (extra == 255 && inPos < compressed.Length);
            }

            // Copy literals.
            if (litLen > 0)
            {
                int copyCount = Math.Min(litLen, origLen - outPos);
                Array.Copy(compressed, inPos, output, outPos, copyCount);
                inPos  += litLen;
                outPos += copyCount;
            }

            // End of stream: no match after the last literal block.
            if (inPos >= compressed.Length) break;

            // Read 2-byte match offset.
            int offset = compressed[inPos] | (compressed[inPos + 1] << 8);
            inPos += 2;

            if (offset == 0)
                return Task.FromResult(OperationResult<byte[]>.Fail("Zero offset.", "CORRUPT"));

            // Read match length.
            int matchLen = (token & 0x0F) + MinMatchLength;
            if ((token & 0x0F) == 15)
            {
                byte extra;
                do { extra = compressed[inPos++]; matchLen += extra; }
                while (extra == 255 && inPos < compressed.Length);
            }

            // Copy match bytes (byte-by-byte to support overlapping runs).
            int srcPos = outPos - offset;
            if (srcPos < 0)
                return Task.FromResult(OperationResult<byte[]>.Fail("Invalid offset.", "CORRUPT"));

            for (int i = 0; i < matchLen && outPos < origLen; i++)
                output[outPos++] = output[srcPos++];
        }

        return Task.FromResult(OperationResult<byte[]>.Ok(output));
    }

    // LZ4 uses a multiplicative hash on 4 bytes.
    private static int Lz4Hash(byte[] data, int pos)
    {
        uint v = (uint)(data[pos] | (data[pos + 1] << 8) | (data[pos + 2] << 16) | (data[pos + 3] << 24));
        return (int)(((v * 2654435761u) >> (32 - HashBits)) & (HashTableSize - 1));
    }
}
