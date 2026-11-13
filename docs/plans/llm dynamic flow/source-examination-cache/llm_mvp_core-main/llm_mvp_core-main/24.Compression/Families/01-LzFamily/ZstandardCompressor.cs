namespace Compression;

// Simplified Zstandard-style compressor: LZ77 match-finding + Huffman entropy coding.
// Full Zstandard uses FSE (Finite State Entropy / tANS), but this implementation
// substitutes Huffman for correctness while retaining the same overall structure.
// Reference: https://github.com/facebook/zstd (BSD license)
public sealed class ZstandardCompressor : ICompressor
{
    private const int WindowSize     = 131072; // 128 KB
    private const int MinMatchLength = 3;
    private const int HashBits       = 17;
    private const int HashTableSize  = 1 << HashBits;

    public async Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return OperationResult<CompressionResult>.Fail("Empty input.", "EMPTY_INPUT");

        // Stage 1: LZ77 tokenisation — produce literals and back-references.
        var literals   = new List<byte>();
        var sequences  = new List<(int LitLength, int MatchOffset, int MatchLength)>();

        var hashTable = new int[HashTableSize];
        Array.Fill(hashTable, -1);

        int pos      = 0;
        int litStart = 0;

        while (pos + 3 < input.Length)
        {
            int  hash      = ZstdHash(input, pos);
            int  candidate = hashTable[hash];
            hashTable[hash] = pos;

            bool hasMatch = candidate >= 0
                && pos - candidate <= WindowSize
                && input[candidate] == input[pos]
                && input[candidate + 1] == input[pos + 1]
                && input[candidate + 2] == input[pos + 2];

            if (!hasMatch) { pos++; continue; }

            // Extend match.
            int matchLen = 3;
            while (pos + matchLen < input.Length
                && input[candidate + matchLen] == input[pos + matchLen])
                matchLen++;

            int litLen = pos - litStart;
            for (int i = litStart; i < pos; i++) literals.Add(input[i]);

            sequences.Add((litLen, pos - candidate, matchLen));

            pos     += matchLen;
            litStart = pos;
        }

        // Flush remaining literals.
        for (int i = litStart; i < input.Length; i++) literals.Add(input[i]);

        // Stage 2: Huffman-compress the literal stream.
        byte[] compressedLiterals = await HuffmanEncodeAsync(literals.ToArray());

        // Stage 3: Encode sequences using a simple variable-length format.
        var seqBytes = EncodeSequences(sequences);

        // Stage 4: Assemble the frame.
        //   Header: 4-byte original length
        //   + 4-byte compressed-literals length
        //   + compressed literals
        //   + sequences
        var frame = new List<byte>();
        frame.Add((byte)(input.Length));
        frame.Add((byte)(input.Length >> 8));
        frame.Add((byte)(input.Length >> 16));
        frame.Add((byte)(input.Length >> 24));

        frame.Add((byte)(compressedLiterals.Length));
        frame.Add((byte)(compressedLiterals.Length >> 8));
        frame.Add((byte)(compressedLiterals.Length >> 16));
        frame.Add((byte)(compressedLiterals.Length >> 24));

        frame.AddRange(compressedLiterals);
        frame.AddRange(seqBytes);

        return OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = frame.ToArray(),
            OriginalLength  = input.Length,
            AlgorithmName   = "Zstandard"
        });
    }

    public async Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length < 8)
            return OperationResult<byte[]>.Fail("Frame too short.", "CORRUPT");

        int inPos   = 0;
        int origLen = ReadInt32LE(compressed, ref inPos);
        int litCompressedLen = ReadInt32LE(compressed, ref inPos);

        if (inPos + litCompressedLen > compressed.Length)
            return OperationResult<byte[]>.Fail("Truncated literal section.", "CORRUPT");

        byte[] litCompressed = compressed[inPos..(inPos + litCompressedLen)];
        inPos += litCompressedLen;

        // Decompress literals with Huffman.
        var litResult = await HuffmanDecodeAsync(litCompressed);
        if (!litResult.IsSuccess) return litResult;
        byte[] literals = litResult.Value!;

        // Decode sequences.
        var sequences = DecodeSequences(compressed, inPos);

        // Reconstruct original data.
        var output    = new byte[origLen];
        int outPos    = 0;
        int litOffset = 0;

        foreach (var (litLen, matchOffset, matchLen) in sequences)
        {
            // Copy literals.
            int litsToCopy = Math.Min(litLen, literals.Length - litOffset);
            Array.Copy(literals, litOffset, output, outPos, litsToCopy);
            litOffset += litLen;
            outPos    += litsToCopy;

            // Copy match.
            int srcPos = outPos - matchOffset;
            if (srcPos >= 0)
            {
                for (int i = 0; i < matchLen && outPos < origLen; i++)
                    output[outPos++] = output[srcPos++];
            }
        }

        // Copy any remaining literals.
        int remaining = Math.Min(literals.Length - litOffset, origLen - outPos);
        if (remaining > 0)
            Array.Copy(literals, litOffset, output, outPos, remaining);

        return OperationResult<byte[]>.Ok(output);
    }

    // ─── Huffman sub-routines ─────────────────────────────────────────────────

    // Encode a byte array using Huffman coding.
    private static Task<byte[]> HuffmanEncodeAsync(byte[] data)
    {
        if (data.Length == 0) return Task.FromResult(new byte[] { 0 });

        var tree = new HuffmanTree();
        var freq = new long[256];
        foreach (byte b in data) freq[b]++;
        tree.Build(freq);

        // Derive bit-reversed canonical codes for LSB-first packing.
        var canonCodes = HuffmanCompressor.DeriveLsbFirstCodes(tree.CodeLengths);

        // Write: 256-byte code-length table + bit-packed encoded data.
        var output = new List<byte>(256 + data.Length);

        // Code lengths header (one byte per symbol, 256 symbols).
        for (int i = 0; i < 256; i++)
            output.Add((byte)tree.CodeLengths[i]);

        // Bit-pack the encoded symbols.
        uint  bitBuffer = 0;
        int   bitsInBuffer = 0;

        foreach (byte b in data)
        {
            if (tree.CodeLengths[b] == 0) continue; // symbol not present (shouldn't happen)
            bitBuffer    |= canonCodes[b] << bitsInBuffer;
            bitsInBuffer += tree.CodeLengths[b];

            while (bitsInBuffer >= 8)
            {
                output.Add((byte)(bitBuffer & 0xFF));
                bitBuffer    >>= 8;
                bitsInBuffer  -= 8;
            }
        }

        // Flush remaining bits.
        if (bitsInBuffer > 0)
            output.Add((byte)(bitBuffer & 0xFF));

        // Append original data length (4 bytes) so the decoder knows when to stop.
        output.Add((byte)(data.Length));
        output.Add((byte)(data.Length >> 8));
        output.Add((byte)(data.Length >> 16));
        output.Add((byte)(data.Length >> 24));

        return Task.FromResult(output.ToArray());
    }

    // Decode Huffman-encoded data.
    private static Task<OperationResult<byte[]>> HuffmanDecodeAsync(byte[] compressed)
    {
        if (compressed.Length < 260) // 256 (code-length table) + 4 (origLen)
            return Task.FromResult(OperationResult<byte[]>.Ok([]));

        // Read code lengths.
        var codeLengths = new int[256];
        for (int i = 0; i < 256; i++) codeLengths[i] = compressed[i];

        // Rebuild bit-reversed canonical codes for LSB-first extraction.
        var codes = HuffmanCompressor.DeriveLsbFirstCodes(codeLengths);

        // Build reverse lookup: code+length → symbol.
        var lookup = new Dictionary<(uint code, int len), byte>();
        for (int sym = 0; sym < 256; sym++)
        {
            if (codeLengths[sym] > 0)
                lookup[(codes[sym], codeLengths[sym])] = (byte)sym;
        }

        // Read original length from last 4 bytes.
        int end     = compressed.Length;
        int origLen = compressed[end - 4]
                    | (compressed[end - 3] << 8)
                    | (compressed[end - 2] << 16)
                    | (compressed[end - 1] << 24);

        var output = new byte[origLen];
        int outPos = 0;

        // Decode bit stream.
        uint bitBuffer   = 0;
        int  bitsInBuf   = 0;
        int  bytePos     = 256;

        while (outPos < origLen)
        {
            // Fill bit buffer.
            while (bitsInBuf < 16 && bytePos < end - 4)
            {
                bitBuffer   |= (uint)compressed[bytePos++] << bitsInBuf;
                bitsInBuf   += 8;
            }

            // Try each code length until we find a match.
            bool found = false;
            for (int len = 1; len <= 16 && len <= bitsInBuf; len++)
            {
                uint testCode = bitBuffer & ((1u << len) - 1);
                if (lookup.TryGetValue((testCode, len), out byte sym))
                {
                    output[outPos++] = sym;
                    bitBuffer  >>= len;
                    bitsInBuf   -= len;
                    found = true;
                    break;
                }
            }

            if (!found) break; // corrupted or done
        }

        return Task.FromResult(OperationResult<byte[]>.Ok(output));
    }

    // ─── Sequence encoding ─────────────────────────────────────────────────────

    // Encode LZ77 sequences as: litLen (2B) + matchOffset (3B) + matchLen (2B).
    private static byte[] EncodeSequences(List<(int LitLen, int Offset, int MatchLen)> seqs)
    {
        var output = new List<byte>();
        // Count header.
        output.Add((byte)(seqs.Count));
        output.Add((byte)(seqs.Count >> 8));
        output.Add((byte)(seqs.Count >> 16));
        output.Add((byte)(seqs.Count >> 24));

        foreach (var (litLen, offset, matchLen) in seqs)
        {
            output.Add((byte)(litLen & 0xFF));
            output.Add((byte)((litLen >> 8) & 0xFF));
            output.Add((byte)(offset & 0xFF));
            output.Add((byte)((offset >> 8) & 0xFF));
            output.Add((byte)((offset >> 16) & 0xFF));
            output.Add((byte)(matchLen & 0xFF));
            output.Add((byte)((matchLen >> 8) & 0xFF));
        }
        return output.ToArray();
    }

    private static List<(int LitLen, int Offset, int MatchLen)> DecodeSequences(byte[] data, int startPos)
    {
        var seqs = new List<(int, int, int)>();
        if (startPos + 4 > data.Length) return seqs;

        int count = data[startPos] | (data[startPos + 1] << 8)
                  | (data[startPos + 2] << 16) | (data[startPos + 3] << 24);
        int pos = startPos + 4;

        for (int i = 0; i < count && pos + 7 <= data.Length; i++)
        {
            int litLen   = data[pos] | (data[pos + 1] << 8);
            int offset   = data[pos + 2] | (data[pos + 3] << 8) | (data[pos + 4] << 16);
            int matchLen = data[pos + 5] | (data[pos + 6] << 8);
            pos += 7;
            seqs.Add((litLen, offset, matchLen));
        }
        return seqs;
    }

    private static int ReadInt32LE(byte[] data, ref int pos)
    {
        int v = data[pos] | (data[pos + 1] << 8) | (data[pos + 2] << 16) | (data[pos + 3] << 24);
        pos += 4;
        return v;
    }

    private static int ZstdHash(byte[] data, int pos)
    {
        uint v = (uint)(data[pos] | (data[pos + 1] << 8) | (data[pos + 2] << 16) | (data[pos + 3] << 24));
        return (int)(((v * 2654435761u) >> (32 - HashBits)) & (HashTableSize - 1));
    }
}
