namespace Compression;

// Google's Snappy compression algorithm — trades ratio for extreme speed.
// Uses a 16 KB hash table to find 4-byte matches quickly.
// Format: varint(originalLength) followed by a sequence of literal-run and copy entries.
// Reference: https://github.com/google/snappy (BSD-3-Clause, ~500 lines of C++)
public sealed class SnappyCompressor : ICompressor
{
    // Hash table size: 16,384 entries (2^14). One entry per hash bucket (no chaining).
    private const int HashTableSize = 1 << 14;

    // Minimum match length — back-references shorter than 4 bytes cost more bits than they save.
    private const int MinMatchLength = 4;

    // Maximum look-back distance. Snappy uses 32 KB to keep offsets in 2 bytes.
    private const int MaxMatchDistance = 32768;

    public Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return Task.FromResult(OperationResult<CompressionResult>.Fail(
                "Snappy requires at least one byte of input.", "EMPTY_INPUT"));

        // Worst-case output is slightly larger than input due to format overhead.
        var output = new byte[input.Length + (input.Length / 6) + 32];
        int outPos = 0;

        // Write original length as a varint so the decompressor can pre-allocate.
        outPos += WriteVarint(output, outPos, input.Length);

        var hashTable = new int[HashTableSize];
        Array.Fill(hashTable, -1);

        int inPos      = 0;
        int litStart   = 0;

        while (inPos + 4 <= input.Length)
        {
            // Hash 4 bytes at the current position to a table slot.
            int hash      = Hash4(input, inPos);
            int candidate = hashTable[hash];
            hashTable[hash] = inPos;

            bool matched = candidate >= 0
                && inPos - candidate < MaxMatchDistance
                && input[candidate]     == input[inPos]
                && input[candidate + 1] == input[inPos + 1]
                && input[candidate + 2] == input[inPos + 2]
                && input[candidate + 3] == input[inPos + 3];

            if (!matched) { inPos++; continue; }

            // Flush accumulated literal bytes before emitting the back-reference.
            if (inPos > litStart)
                outPos += EmitLiterals(output, outPos, input, litStart, inPos - litStart);

            // Extend the 4-byte match as far as possible.
            int matchLen = 4;
            while (inPos + matchLen < input.Length
                && input[candidate + matchLen] == input[inPos + matchLen])
                matchLen++;

            int distance = inPos - candidate;
            outPos += EmitCopy(output, outPos, distance, matchLen);

            inPos    += matchLen;
            litStart  = inPos;
        }

        // Flush any trailing literal bytes.
        if (litStart < input.Length)
            outPos += EmitLiterals(output, outPos, input, litStart, input.Length - litStart);

        return Task.FromResult(OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = output[..outPos],
            OriginalLength  = input.Length,
            AlgorithmName   = "Snappy"
        }));
    }

    public Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length == 0)
            return Task.FromResult(OperationResult<byte[]>.Fail("Empty compressed input.", "EMPTY_INPUT"));

        int inPos = 0;

        // Read original length from the varint header.
        int origLen = ReadVarint(compressed, ref inPos);
        var output  = new byte[origLen];
        int outPos  = 0;

        while (inPos < compressed.Length && outPos < origLen)
        {
            byte tag    = compressed[inPos++];
            int  tagType = tag & 0x03;

            if (tagType == 0) // literal run
            {
                int litLen;
                int lenCode = (tag >> 2) & 0x3F;
                if (lenCode < 60)
                {
                    litLen = lenCode + 1;
                }
                else
                {
                    // Extra bytes encode the literal length.
                    int extraBytes = lenCode - 59;
                    litLen = 0;
                    for (int b = 0; b < extraBytes; b++)
                        litLen |= compressed[inPos++] << (b * 8);
                    litLen++;
                }

                if (inPos + litLen > compressed.Length)
                    return Task.FromResult(OperationResult<byte[]>.Fail("Truncated literal run.", "CORRUPT"));

                Array.Copy(compressed, inPos, output, outPos, litLen);
                inPos  += litLen;
                outPos += litLen;
            }
            else // copy (back-reference)
            {
                int copyLen;
                int copyOffset;

                if (tagType == 1) // 1-byte offset copy
                {
                    copyLen    = ((tag >> 2) & 0x07) + 4;
                    copyOffset = ((tag & 0xE0) << 3) | compressed[inPos++];
                }
                else if (tagType == 2) // 2-byte offset copy
                {
                    copyLen    = ((tag >> 2) & 0x3F) + 1;
                    copyOffset = compressed[inPos] | (compressed[inPos + 1] << 8);
                    inPos += 2;
                }
                else // 4-byte offset copy (tagType == 3)
                {
                    copyLen    = ((tag >> 2) & 0x3F) + 1;
                    copyOffset = compressed[inPos]
                               | (compressed[inPos + 1] << 8)
                               | (compressed[inPos + 2] << 16)
                               | (compressed[inPos + 3] << 24);
                    inPos += 4;
                }

                if (copyOffset <= 0 || copyOffset > outPos)
                    return Task.FromResult(OperationResult<byte[]>.Fail("Invalid back-reference.", "CORRUPT"));

                // Copy byte-by-byte to support overlapping matches (run expansions).
                int srcPos = outPos - copyOffset;
                for (int i = 0; i < copyLen && outPos < origLen; i++)
                    output[outPos++] = output[srcPos++];
            }
        }

        return Task.FromResult(OperationResult<byte[]>.Ok(output));
    }

    // ─── Encoding helpers ─────────────────────────────────────────────────────

    // Write a variable-length integer (varint) to the output buffer.
    // Groups of 7 bits; the high bit of each byte indicates "more bytes follow."
    private static int WriteVarint(byte[] buf, int pos, int value)
    {
        int written = 0;
        do
        {
            byte b = (byte)(value & 0x7F);
            value >>= 7;
            if (value > 0) b |= 0x80; // set continuation bit
            buf[pos + written++] = b;
        } while (value > 0);
        return written;
    }

    // Read a varint from the compressed buffer, advancing the position.
    private static int ReadVarint(byte[] buf, ref int pos)
    {
        int result = 0;
        int shift  = 0;
        byte b;
        do
        {
            b      = buf[pos++];
            result |= (b & 0x7F) << shift;
            shift  += 7;
        } while ((b & 0x80) != 0);
        return result;
    }

    // Hash 4 bytes at a position to a table index using a multiplicative hash.
    private static int Hash4(byte[] data, int pos)
    {
        uint v = (uint)(data[pos] | (data[pos + 1] << 8) | (data[pos + 2] << 16) | (data[pos + 3] << 24));
        return (int)((v * 0x1e35a7bd) >> 18) & (HashTableSize - 1);
    }

    // Emit a run of literal bytes in Snappy format.
    private static int EmitLiterals(byte[] output, int outPos, byte[] input, int start, int length)
    {
        int written = 0;
        int offset  = 0;

        while (offset < length)
        {
            // Emit in chunks of up to 256 literals at a time.
            int chunkLen = Math.Min(length - offset, 256);
            int lenCode  = chunkLen - 1;

            if (lenCode < 60)
            {
                output[outPos + written++] = (byte)(lenCode << 2); // tag byte: type=0 (literal)
            }
            else
            {
                // Use one extra byte to encode the length.
                output[outPos + written++] = (byte)((60 << 2)); // 60 means 1 extra byte follows
                output[outPos + written++] = (byte)(lenCode - 1);
            }

            Array.Copy(input, start + offset, output, outPos + written, chunkLen);
            written += chunkLen;
            offset  += chunkLen;
        }

        return written;
    }

    // Emit a back-reference (copy) entry.
    private static int EmitCopy(byte[] output, int outPos, int distance, int length)
    {
        int written = 0;

        // Emit in chunks of at most 64 bytes per copy tag.
        while (length > 0)
        {
            int chunkLen = Math.Min(length, 64);

            if (distance < 2048 && chunkLen >= 4 && chunkLen <= 11)
            {
                // 1-byte offset copy: tag type 1
                output[outPos + written++] = (byte)(((chunkLen - 4) << 2) | 0x01 | ((distance >> 3) & 0xE0));
                output[outPos + written++] = (byte)(distance & 0xFF);
            }
            else
            {
                // 2-byte offset copy: tag type 2
                output[outPos + written++] = (byte)(((chunkLen - 1) << 2) | 0x02);
                output[outPos + written++] = (byte)(distance & 0xFF);
                output[outPos + written++] = (byte)((distance >> 8) & 0xFF);
            }

            length -= chunkLen;
        }

        return written;
    }
}
