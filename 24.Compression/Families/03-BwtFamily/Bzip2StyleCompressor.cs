// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// bzip2-style compressor: applies the full BWT pipeline.
//   Input → BWT → MTF → RLE → Huffman → compressed output
//
// This achieves better compression ratio than LZ-family algorithms on most text data
// because BWT sorts the entire block rather than using a sliding window.
// Reference: bzip2 compress.c + blocksort.c (BSD license)
public sealed class Bzip2StyleCompressor : ICompressor
{
    // Maximum block size in bytes — bzip2 processes data in blocks.
    private const int BlockSize = 100_000;

    private readonly HuffmanCompressor _huffman = new();

    public async Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return OperationResult<CompressionResult>.Fail("Empty input.", "EMPTY_INPUT");

        var allCompressed = new List<byte>();

        // Write total original length as 4-byte header.
        allCompressed.Add((byte)(input.Length));
        allCompressed.Add((byte)(input.Length >> 8));
        allCompressed.Add((byte)(input.Length >> 16));
        allCompressed.Add((byte)(input.Length >> 24));

        // Process data in blocks.
        int offset = 0;
        while (offset < input.Length)
        {
            int blockLen  = Math.Min(BlockSize, input.Length - offset);
            byte[] block  = input[offset..(offset + blockLen)];
            offset       += blockLen;

            // Stage 1: Burrows-Wheeler Transform.
            var (bwtData, originalRow) = BurrowsWheelerTransform.Forward(block);

            // Store the originalRow index (4 bytes) so decompressor can reverse BWT.
            allCompressed.Add((byte)(originalRow));
            allCompressed.Add((byte)(originalRow >> 8));
            allCompressed.Add((byte)(originalRow >> 16));
            allCompressed.Add((byte)(originalRow >> 24));

            // Store block length (4 bytes).
            allCompressed.Add((byte)(blockLen));
            allCompressed.Add((byte)(blockLen >> 8));
            allCompressed.Add((byte)(blockLen >> 16));
            allCompressed.Add((byte)(blockLen >> 24));

            // Stage 2: Move-To-Front transform.
            int[] mtfData = MoveToFrontTransform.Forward(bwtData);

            // Stage 3: Run-Length Encoding.
            byte[] rleData = RunLengthEncoder.Forward(mtfData);

            // Stage 4: Huffman entropy coding.
            var huffResult = await _huffman.CompressAsync(rleData, options);
            if (!huffResult.IsSuccess)
                return OperationResult<CompressionResult>.Fail(huffResult.ErrorMessage, huffResult.ErrorCode);

            byte[] compressed = huffResult.Value!.CompressedBytes;

            // Store compressed block length (4 bytes) + compressed data.
            allCompressed.Add((byte)(compressed.Length));
            allCompressed.Add((byte)(compressed.Length >> 8));
            allCompressed.Add((byte)(compressed.Length >> 16));
            allCompressed.Add((byte)(compressed.Length >> 24));
            allCompressed.AddRange(compressed);
        }

        return OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = allCompressed.ToArray(),
            OriginalLength  = input.Length,
            AlgorithmName   = "bzip2-style"
        });
    }

    public async Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length < 4)
            return OperationResult<byte[]>.Fail("Too short.", "CORRUPT");

        int pos     = 0;
        int origLen = ReadInt32LE(compressed, ref pos);

        var output = new List<byte>(origLen);

        while (pos < compressed.Length && output.Count < origLen)
        {
            if (pos + 12 > compressed.Length)
                return OperationResult<byte[]>.Fail("Truncated block header.", "CORRUPT");

            int originalRow      = ReadInt32LE(compressed, ref pos);
            int blockLen         = ReadInt32LE(compressed, ref pos);
            int compressedLen    = ReadInt32LE(compressed, ref pos);

            if (pos + compressedLen > compressed.Length)
                return OperationResult<byte[]>.Fail("Truncated block data.", "CORRUPT");

            byte[] compressedBlock = compressed[pos..(pos + compressedLen)];
            pos += compressedLen;

            // Stage 4 → 3 → 2 → 1 (reverse pipeline).
            var huffResult = await _huffman.DecompressAsync(compressedBlock, 0);
            if (!huffResult.IsSuccess)
                return OperationResult<byte[]>.Fail(huffResult.ErrorMessage, huffResult.ErrorCode);

            int[]  mtfData = RunLengthEncoder.Inverse(huffResult.Value!);
            byte[] bwtData = MoveToFrontTransform.Inverse(mtfData);
            byte[] block   = BurrowsWheelerTransform.Inverse(bwtData, originalRow);

            output.AddRange(block);
        }

        return OperationResult<byte[]>.Ok(output.ToArray());
    }

    private static int ReadInt32LE(byte[] data, ref int pos)
    {
        int v = data[pos] | (data[pos + 1] << 8) | (data[pos + 2] << 16) | (data[pos + 3] << 24);
        pos += 4;
        return v;
    }
}
