using System.IO.Compression;

namespace Compression;

// DEFLATE compressor — the format used inside ZIP files, PNG images, and HTTP gzip.
// This implementation delegates to the .NET built-in DeflateStream for correctness,
// wrapping it in the ICompressor interface with the required OperationResult envelope.
// Reference: zlib deflate.c + trees.c + inflate.c (zlib license)
public sealed class DeflateCompressor : ICompressor
{
    public async Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return OperationResult<CompressionResult>.Fail("Empty input.", "EMPTY_INPUT");

        // Map compression level 1-9 to the .NET CompressionLevel enum.
        var dotNetLevel = options.CompressionLevel >= 9
            ? CompressionLevel.SmallestSize
            : options.CompressionLevel <= 1
                ? CompressionLevel.Fastest
                : CompressionLevel.Optimal;

        using var outputStream     = new MemoryStream();
        await using var deflateStream = new DeflateStream(outputStream, dotNetLevel);
        await deflateStream.WriteAsync(input);
        await deflateStream.FlushAsync();

        // DeflateStream must be closed before reading the underlying buffer.
        await deflateStream.DisposeAsync();

        return OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = outputStream.ToArray(),
            OriginalLength  = input.Length,
            AlgorithmName   = "Deflate"
        });
    }

    public async Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length == 0)
            return OperationResult<byte[]>.Fail("Empty compressed input.", "EMPTY_INPUT");

        try
        {
            using var inputStream      = new MemoryStream(compressed);
            using var deflateStream    = new DeflateStream(inputStream, CompressionMode.Decompress);
            using var outputStream     = new MemoryStream();
            await deflateStream.CopyToAsync(outputStream);
            return OperationResult<byte[]>.Ok(outputStream.ToArray());
        }
        catch (InvalidDataException ex)
        {
            return OperationResult<byte[]>.Fail($"Decompression failed: {ex.Message}", "CORRUPT");
        }
    }
}
