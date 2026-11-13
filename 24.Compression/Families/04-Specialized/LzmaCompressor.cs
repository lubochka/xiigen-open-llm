// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// LZMA compressor: range coding + Markov probability model + LZ77 match finding.
// Achieves very high compression ratios on text by modeling byte probabilities
// with context-dependent adaptive Markov chains.
// Reference: LZMA-SDK (public domain) — simplified single-pass implementation
public sealed class LzmaCompressor : ICompressor
{
    private const int MinMatchLength = 2;
    private const int MaxMatchLength = 273;
    private const int MaxMatchDistance = 1 << 22; // 4 MB window

    public Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return Task.FromResult(
                OperationResult<CompressionResult>.Fail("Empty input.", "EMPTY_INPUT"));

        var model  = new LzmaMarkovModel(lc: 3, lp: 0);
        var coder  = new LzmaRangeCoder();
        // ShiftLow naturally emits a leading 0x00 (initial _cache=0 flushed on first call).

        // 4-byte original-length header (little-endian).
        var header = new byte[4];
        header[0] = (byte)input.Length;
        header[1] = (byte)(input.Length >> 8);
        header[2] = (byte)(input.Length >> 16);
        header[3] = (byte)(input.Length >> 24);

        byte prevByte = 0;
        int  pos      = 0;

        while (pos < input.Length)
        {
            // Try to find a match in the history.
            var match = LzMatchFinder.FindBestMatch(
                input, pos,
                MaxMatchDistance,
                MinMatchLength);

            if (match != null && match.Length >= MinMatchLength)
            {
                int dist = match.Distance;
                int len  = match.Length;

                // Signal "match" with a 1-bit, then encode length + distance.
                ushort matchFlag = 1024;
                coder.EncodeBit(ref matchFlag, 1);

                model.EncodeMatchLength(coder, len);

                // Encode distance as raw bits (22-bit).
                EncodeDistance(coder, dist - 1);

                pos      += len;
                prevByte  = input[pos - 1];
            }
            else
            {
                // Signal "literal" with a 0-bit, then encode byte.
                ushort litFlag = 1024;
                coder.EncodeBit(ref litFlag, 0);

                model.EncodeLiteral(coder, prevByte, pos, input[pos]);
                prevByte = input[pos];
                pos++;
            }
        }

        coder.Finish();

        var output = new byte[header.Length + coder.Output.Count];
        Buffer.BlockCopy(header, 0, output, 0, header.Length);
        var outputSpan = coder.Output.ToArray();
        Buffer.BlockCopy(outputSpan, 0, output, header.Length, outputSpan.Length);

        return Task.FromResult(OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = output,
            OriginalLength  = input.Length,
            AlgorithmName   = "LZMA"
        }));
    }

    public Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length < 4)
            return Task.FromResult(
                OperationResult<byte[]>.Fail("Too short.", "CORRUPT"));

        int origLen = compressed[0] | (compressed[1] << 8) |
                      (compressed[2] << 16) | (compressed[3] << 24);

        var model   = new LzmaMarkovModel(lc: 3, lp: 0);
        var output  = new byte[origLen];
        int outPos  = 0;

        // Initialize range decoder state.
        uint state = 0;
        uint range = 0xFFFFFFFF;
        int  inPos = 4;

        // Prime state with 5 bytes; the leading 0x00 emitted by the encoder shifts out of
        // the uint32 window, leaving exactly 4 bytes of real interval state.
        for (int i = 0; i < 5 && inPos < compressed.Length; i++)
        {
            state = (state << 8) | compressed[inPos++];
        }

        byte prevByte = 0;

        while (outPos < origLen && inPos <= compressed.Length)
        {
            ushort matchFlag = 1024;
            int isMatch = LzmaRangeCoder.DecodeBit(
                ref state, ref range, compressed, ref inPos, ref matchFlag);

            if (isMatch == 0)
            {
                // Literal.
                ushort[] probs  = model.GetLiteralProbs(prevByte, outPos);
                byte lit        = LzmaMarkovModel.DecodeLiteral(probs, ref state, ref range, compressed, ref inPos);
                output[outPos++] = lit;
                prevByte         = lit;
            }
            else
            {
                // Match.
                int len  = LzmaMarkovModel.DecodeMatchLength(model.MatchLenProbs, ref state, ref range, compressed, ref inPos);
                int dist = DecodeDistance(ref state, ref range, compressed, ref inPos) + 1;

                int copyFrom = outPos - dist;
                if (copyFrom < 0)
                    return Task.FromResult(
                        OperationResult<byte[]>.Fail("Invalid back-reference.", "CORRUPT"));

                for (int k = 0; k < len && outPos < origLen; k++)
                    output[outPos++] = output[copyFrom++];

                prevByte = output[outPos - 1];
            }
        }

        return Task.FromResult(OperationResult<byte[]>.Ok(output));
    }

    // Encode a distance as a 22-bit direct value, split across range-coded high bits.
    private static void EncodeDistance(LzmaRangeCoder coder, int distance)
    {
        // Simplified: encode as 22 raw bits using a fixed probability of 1024 (uniform).
        for (int i = 21; i >= 0; i--)
        {
            ushort p   = 1024;
            int    bit = (distance >> i) & 1;
            coder.EncodeBit(ref p, bit);
        }
    }

    private static int DecodeDistance(ref uint state, ref uint range, byte[] input, ref int pos)
    {
        int dist = 0;
        for (int i = 21; i >= 0; i--)
        {
            ushort p   = 1024;
            int    bit = LzmaRangeCoder.DecodeBit(ref state, ref range, input, ref pos, ref p);
            dist = (dist << 1) | bit;
        }
        return dist;
    }
}
