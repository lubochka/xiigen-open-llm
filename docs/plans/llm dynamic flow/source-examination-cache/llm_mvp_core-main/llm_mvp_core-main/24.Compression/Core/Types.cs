namespace Compression;

// Configurable parameters for compression algorithms.
public sealed record CompressionOptions
{
    // Higher levels trade speed for better compression ratio. 1 is always fastest.
    public int CompressionLevel { get; init; } = 1;

    // Pre-trained dictionary path — only used by Zstandard and OpenZL.
    public string? DictionaryPath { get; init; }

    // Maximum memory the compressor may allocate, in megabytes.
    public int MaxMemoryMegabytes { get; init; } = 64;
}

// Wraps compressed bytes with metadata needed for decompression.
public sealed record CompressionResult
{
    public byte[] CompressedBytes { get; init; } = [];

    // Original uncompressed byte count — stored in header for buffer pre-allocation.
    public int OriginalLength { get; init; }

    // Name of the algorithm that produced this result.
    public string AlgorithmName { get; init; } = string.Empty;

    // Original size divided by compressed size.
    public double CompressionRatio => OriginalLength / (double)Math.Max(1, CompressedBytes.Length);
}

// Timing and ratio measurements from one benchmark run.
public sealed record BenchmarkResult(
    string AlgorithmName,
    double CompressionRatio,
    double CompressionThroughputMbps,
    double DecompressionThroughputMbps);
