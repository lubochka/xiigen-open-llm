// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// Huffman entropy compressor.
// Assigns shorter bit sequences to frequent bytes and longer ones to rare bytes.
// Format: 256-byte code-length table + 4-byte original length + LSB-first bit-packed data.
// Canonical codes are bit-reversed so they are prefix-free under LSB-first packing.
// Reference: zlib trees.c (canonical Huffman implementation)
public sealed class HuffmanCompressor : ICompressor
{
    public Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return Task.FromResult(OperationResult<CompressionResult>.Fail(
                "Empty input.", "EMPTY_INPUT"));

        var freq = SymbolStatistics.CountFrequencies(input);
        var tree = new HuffmanTree();
        tree.Build(freq);

        // Derive LSB-first canonical codes (bit-reversed canonical Huffman).
        var codes = DeriveLsbFirstCodes(tree.CodeLengths);

        var output = new List<byte>(260 + input.Length / 2);

        // Header: code length per symbol (256 bytes).
        for (int sym = 0; sym < 256; sym++)
            output.Add((byte)tree.CodeLengths[sym]);

        // Original length (4 bytes LE).
        output.Add((byte)(input.Length));
        output.Add((byte)(input.Length >> 8));
        output.Add((byte)(input.Length >> 16));
        output.Add((byte)(input.Length >> 24));

        // LSB-first bit packing.
        uint bitBuffer    = 0;
        int  bitsInBuffer = 0;

        foreach (byte b in input)
        {
            uint code    = codes[b];
            int  codeLen = tree.CodeLengths[b];

            bitBuffer    |= code << bitsInBuffer;
            bitsInBuffer += codeLen;

            while (bitsInBuffer >= 8)
            {
                output.Add((byte)(bitBuffer & 0xFF));
                bitBuffer    >>= 8;
                bitsInBuffer  -= 8;
            }
        }

        if (bitsInBuffer > 0)
            output.Add((byte)(bitBuffer & 0xFF));

        return Task.FromResult(OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = output.ToArray(),
            OriginalLength  = input.Length,
            AlgorithmName   = "Huffman"
        }));
    }

    public Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length < 260)
            return Task.FromResult(OperationResult<byte[]>.Fail("Too short for Huffman header.", "CORRUPT"));

        var codeLengths = new int[256];
        for (int i = 0; i < 256; i++) codeLengths[i] = compressed[i];

        int origLen = compressed[256]
                    | (compressed[257] << 8)
                    | (compressed[258] << 16)
                    | (compressed[259] << 24);

        var codes = DeriveLsbFirstCodes(codeLengths);

        // Build reverse lookup: (lsb-first code, length) → symbol.
        var reverseLookup = new Dictionary<(uint, int), byte>();
        for (int sym = 0; sym < 256; sym++)
        {
            if (codeLengths[sym] > 0)
                reverseLookup[(codes[sym], codeLengths[sym])] = (byte)sym;
        }

        var output    = new byte[origLen];
        int outPos    = 0;
        int bytePos   = 260;
        uint bitBuf   = 0;
        int bitsAvail = 0;

        while (outPos < origLen)
        {
            while (bitsAvail < 24 && bytePos < compressed.Length)
            {
                bitBuf    |= (uint)compressed[bytePos++] << bitsAvail;
                bitsAvail += 8;
            }

            bool decoded = false;
            for (int len = 1; len <= 24 && len <= bitsAvail; len++)
            {
                uint testCode = bitBuf & ((1u << len) - 1);
                if (reverseLookup.TryGetValue((testCode, len), out byte sym))
                {
                    output[outPos++] = sym;
                    bitBuf    >>= len;
                    bitsAvail  -= len;
                    decoded = true;
                    break;
                }
            }

            if (!decoded) break;
        }

        return Task.FromResult(OperationResult<byte[]>.Ok(output));
    }

    // Derive bit-reversed canonical codes from code lengths.
    // Canonical codes are assigned MSB-first, then reversed for LSB-first packing.
    // Bit-reversed canonical codes are prefix-free under LSB-first extraction.
    internal static uint[] DeriveLsbFirstCodes(int[] codeLengths)
    {
        // Step 1: assign standard canonical codes.
        var canonical = new uint[256];
        uint next = 0;
        for (int len = 1; len <= 32; len++)
        {
            for (int sym = 0; sym < 256; sym++)
            {
                if (codeLengths[sym] == len)
                    canonical[sym] = next++;
            }
            next <<= 1;
        }

        // Step 2: bit-reverse each code for LSB-first packing.
        var lsbFirst = new uint[256];
        for (int sym = 0; sym < 256; sym++)
        {
            if (codeLengths[sym] == 0) continue;
            lsbFirst[sym] = ReverseBits(canonical[sym], codeLengths[sym]);
        }

        return lsbFirst;
    }

    private static uint ReverseBits(uint code, int length)
    {
        uint result = 0;
        for (int i = 0; i < length; i++)
        {
            result = (result << 1) | (code & 1);
            code >>= 1;
        }
        return result;
    }
}
