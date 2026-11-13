namespace Compression;

// rANS (range Asymmetric Numeral Systems) entropy coder.
// Unlike Huffman, rANS assigns fractional bit lengths to symbols, reaching Shannon entropy.
// The encoder works backwards (last symbol first); the decoder works forwards.
// Reference: https://github.com/rygorous/ryg_rans (rans_byte.h, public domain, ~100 lines)
public sealed class RansCompressor : ICompressor
{
    // ANS scaling factor — all frequencies normalized to sum to this value.
    // Must be a power of 2; 1<<16 means frequencies are 16-bit integers.
    private const uint ScaleBits = 16;
    private const uint Scale     = 1u << (int)ScaleBits;

    // The rANS state range is [LowerBound, 4*LowerBound).
    // When encoding we flush bits to keep the state in this range.
    private const uint LowerBound = 1u << 23;

    public Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return Task.FromResult(OperationResult<CompressionResult>.Fail("Empty input.", "EMPTY_INPUT"));

        // Count frequencies and normalize to sum to Scale.
        var rawFreq  = SymbolStatistics.CountFrequencies(input);
        var (normalizedFreq, cumFreq) = NormalizeFrequencies(rawFreq, input.Length);

        // Encode backwards — rANS encodes in reverse, decoder reads forwards.
        var outputBytes = new List<byte>();
        uint state      = LowerBound;

        for (int i = input.Length - 1; i >= 0; i--)
        {
            byte sym = input[i];
            uint freq = normalizedFreq[sym];
            if (freq == 0) continue; // symbol not in training set

            // Renormalize: flush low bytes until state fits in the encoding range.
            uint upperBound = ((LowerBound >> (int)ScaleBits) << 8) * freq;
            while (state >= upperBound)
            {
                outputBytes.Add((byte)(state & 0xFF));
                state >>= 8;
            }

            // Encode the symbol: new_state = (state / freq) * Scale + cumFreq[sym] + (state % freq)
            state = (state / freq) * Scale + cumFreq[sym] + (state % freq);
        }

        // Flush the final state in little-endian; after Reverse() it will be big-endian
        // at the start of the buffer so the decoder can read it as a 4-byte big-endian value.
        outputBytes.Add((byte)(state & 0xFF));
        outputBytes.Add((byte)((state >> 8)  & 0xFF));
        outputBytes.Add((byte)((state >> 16) & 0xFF));
        outputBytes.Add((byte)((state >> 24) & 0xFF));

        // Reverse: renorm bytes (accumulated in encoding order) become decoder order;
        // state bytes become big-endian at position 0.
        outputBytes.Reverse();

        // Build the header: original length (4B) + frequency table (256 × 4B).
        // 4-byte entries are required because Scale = 65536 overflows 2 bytes.
        var header = new List<byte>();
        header.Add((byte)(input.Length));
        header.Add((byte)(input.Length >> 8));
        header.Add((byte)(input.Length >> 16));
        header.Add((byte)(input.Length >> 24));

        for (int sym = 0; sym < 256; sym++)
        {
            header.Add((byte)(normalizedFreq[sym] & 0xFF));
            header.Add((byte)((normalizedFreq[sym] >> 8) & 0xFF));
            header.Add((byte)((normalizedFreq[sym] >> 16) & 0xFF));
            header.Add((byte)((normalizedFreq[sym] >> 24) & 0xFF));
        }

        header.AddRange(outputBytes);

        return Task.FromResult(OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = header.ToArray(),
            OriginalLength  = input.Length,
            AlgorithmName   = "rANS"
        }));
    }

    public Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length < 4 + 1024 + 4)
            return Task.FromResult(OperationResult<byte[]>.Fail("Too short.", "CORRUPT"));

        int pos     = 0;
        int origLen = compressed[pos] | (compressed[pos + 1] << 8)
                    | (compressed[pos + 2] << 16) | (compressed[pos + 3] << 24);
        pos += 4;

        // Read frequency table (4 bytes per symbol).
        var freq    = new uint[256];
        var cumFreq = new uint[257];
        for (int sym = 0; sym < 256; sym++)
        {
            freq[sym] = (uint)(compressed[pos]
                             | (compressed[pos + 1] << 8)
                             | (compressed[pos + 2] << 16)
                             | (compressed[pos + 3] << 24));
            pos += 4;
        }

        // Rebuild cumulative frequencies.
        cumFreq[0] = 0;
        for (int sym = 0; sym < 256; sym++)
            cumFreq[sym + 1] = cumFreq[sym] + freq[sym];

        // Build reverse lookup: cumulative index → symbol.
        var symLookup = new byte[Scale];
        for (int sym = 0; sym < 256; sym++)
        {
            for (uint j = cumFreq[sym]; j < cumFreq[sym + 1] && j < Scale; j++)
                symLookup[j] = (byte)sym;
        }

        // Read initial state from the first 4 bytes of the encoded data.
        uint state = ((uint)compressed[pos] << 24)
                   | ((uint)compressed[pos + 1] << 16)
                   | ((uint)compressed[pos + 2] << 8)
                   | compressed[pos + 3];
        pos += 4;

        var output = new byte[origLen];
        for (int i = 0; i < origLen; i++)
        {
            // Decode symbol from state.
            uint slot = state & (Scale - 1);
            byte sym  = symLookup[slot];

            // Advance state.
            uint symFreq = freq[sym];
            if (symFreq == 0) break;

            state = symFreq * (state >> (int)ScaleBits) + slot - cumFreq[sym];

            // Renormalize: refill state from encoded bytes.
            while (state < LowerBound && pos < compressed.Length)
            {
                state = (state << 8) | compressed[pos++];
            }

            output[i] = sym;
        }

        return Task.FromResult(OperationResult<byte[]>.Ok(output));
    }

    // Normalize raw frequencies so they sum to exactly Scale, with minimum freq = 1.
    private static (uint[] freq, uint[] cumFreq) NormalizeFrequencies(long[] raw, int total)
    {
        var freq = new uint[256];
        long sum  = 0;

        // Assign proportional frequencies.
        for (int sym = 0; sym < 256; sym++)
        {
            if (raw[sym] == 0) { freq[sym] = 0; continue; }
            freq[sym] = (uint)Math.Max(1, (raw[sym] * Scale) / total);
            sum += freq[sym];
        }

        // Adjust to make the sum exactly Scale.
        long delta = (long)Scale - sum;
        for (int sym = 0; sym < 256 && delta != 0; sym++)
        {
            if (freq[sym] == 0) continue;
            if (delta > 0) { freq[sym]++; delta--; }
            else if (freq[sym] > 1) { freq[sym]--; delta++; }
        }

        // Build cumulative frequency array.
        var cumFreq  = new uint[257];
        cumFreq[0] = 0;
        for (int sym = 0; sym < 256; sym++)
            cumFreq[sym + 1] = cumFreq[sym] + freq[sym];

        return (freq, cumFreq);
    }
}
