// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// Every compression algorithm implements this two-method contract.
// The caller does not need to know whether it is Snappy, LZ4, or LZMA.
public interface ICompressor
{
    // Compress input bytes and return the compressed representation.
    Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input,
        CompressionOptions options);

    // Restore the original bytes from a compressed block.
    Task<OperationResult<byte[]>> DecompressAsync(
        byte[] compressed,
        int    expectedOriginalLength);
}

// Hides where compressed blocks are saved.
// Concrete implementations: InMemoryCompressionStorage (tests), DiskCompressionStorage (production).
public interface ICompressionStorage
{
    Task<OperationResult<bool>>              SaveAsync(string name, CompressionResult compressed);
    Task<OperationResult<CompressionResult>> LoadAsync(string name);
}

// Runs all registered compressors on the same test data and returns metrics.
public interface ICompressionBenchmark
{
    Task<OperationResult<List<BenchmarkResult>>> RunAllAsync(byte[] testData);
}
