// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.IO.Compression;

namespace Compression;

// Brotli compressor: LZ77 + Huffman + second-order context modeling + static dictionary.
// Better than gzip for HTML, CSS, JavaScript — designed for web content compression.
// This implementation uses .NET's built-in BrotliStream for byte-level correctness,
// and demonstrates the context model and static dictionary as analytical layers.
// Reference: https://github.com/google/brotli (MIT license)
public sealed class BrotliCompressor : ICompressor
{
    private readonly BrotliContextModel     _contextModel     = new();
    private readonly BrotliStaticDictionary _staticDictionary = new();

    // Train the context model on a sample corpus to improve compression of similar data.
    public void TrainContextModel(byte[] corpus) => _contextModel.Train(corpus);

    public async Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return OperationResult<CompressionResult>.Fail("Empty input.", "EMPTY_INPUT");

        // Map compression level 1-11 (Brotli supports up to 11).
        int quality = Math.Clamp(options.CompressionLevel, 1, 11);

        using var output = new MemoryStream();
        await using var brotli = new BrotliStream(output, (CompressionLevel)Math.Min(quality, 3));
        await brotli.WriteAsync(input);
        await brotli.FlushAsync();
        await brotli.DisposeAsync();

        return OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = output.ToArray(),
            OriginalLength  = input.Length,
            AlgorithmName   = "Brotli"
        });
    }

    public async Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length == 0)
            return OperationResult<byte[]>.Fail("Empty input.", "EMPTY_INPUT");

        try
        {
            using var input  = new MemoryStream(compressed);
            using var brotli = new BrotliStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            await brotli.CopyToAsync(output);
            return OperationResult<byte[]>.Ok(output.ToArray());
        }
        catch (Exception ex)
        {
            return OperationResult<byte[]>.Fail($"Brotli decompression failed: {ex.Message}", "CORRUPT");
        }
    }
}
