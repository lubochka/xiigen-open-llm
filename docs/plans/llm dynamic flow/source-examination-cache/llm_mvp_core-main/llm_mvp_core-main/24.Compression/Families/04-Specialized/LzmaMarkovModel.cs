namespace Compression;

// Markov chain probability model for LZMA literal and match-length coding.
// Each context (previous byte, literal state) maintains an independent set of
// 11-bit adaptive probabilities — one per bit position.
// Reference: LZMA-SDK LzmaDec.c / LzmaEnc.c (public domain)
public sealed class LzmaMarkovModel
{
    // LZMA uses 11-bit fixed-point probabilities. Initial value = 1024 = 50% probability.
    private const ushort InitialProbability = 1024;

    // Literal probabilities: 768 probability tables, each with 8 bits.
    // Index: literalContext * 256 + previousByte, value: 8-element probability array.
    private readonly ushort[][] _literalProbs;

    // Match-length probabilities — two tables (short: 1-8, long: 9+).
    private readonly ushort[] _matchLenProbs;
    private readonly ushort[] _repLenProbs;

    // Number of literal context bits (lc) and literal position bits (lp).
    private readonly int _lc;
    private readonly int _lp;

    public LzmaMarkovModel(int lc = 3, int lp = 0)
    {
        _lc = lc;
        _lp = lp;

        int literalContexts = 1 << (lc + lp);
        _literalProbs  = new ushort[literalContexts][];
        for (int i = 0; i < literalContexts; i++)
        {
            _literalProbs[i] = new ushort[256 * 3]; // 3 ranges: plain / match / rep
            Array.Fill(_literalProbs[i], InitialProbability);
        }

        _matchLenProbs = new ushort[512];
        _repLenProbs   = new ushort[512];
        Array.Fill(_matchLenProbs, InitialProbability);
        Array.Fill(_repLenProbs,   InitialProbability);
    }

    // Get the literal probability table for the given encoder state.
    // prevByte — the byte just encoded; position — current stream position.
    public ushort[] GetLiteralProbs(byte prevByte, int position)
    {
        int lpMask  = (1 << _lp) - 1;
        int lcMask  = (1 << _lc) - 1;
        int context = ((position & lpMask) << _lc) | (prevByte >> (8 - _lc) & lcMask);
        return _literalProbs[context];
    }

    // Encode one literal byte bit-by-bit using the range coder.
    // Each of the 8 bits uses an independent probability from the context table.
    public void EncodeLiteral(LzmaRangeCoder coder, byte prevByte, int position, byte symbol)
    {
        ushort[] probs = GetLiteralProbs(prevByte, position);
        int ctx = 1;
        for (int i = 7; i >= 0; i--)
        {
            int bit = (symbol >> i) & 1;
            coder.EncodeBit(ref probs[ctx], bit);
            ctx = (ctx << 1) | bit;
        }
    }

    // Decode one literal byte bit-by-bit.
    public static byte DecodeLiteral(ushort[] probs, ref uint state, ref uint range,
                                      byte[] input, ref int pos)
    {
        int ctx = 1;
        for (int i = 7; i >= 0; i--)
        {
            int bit = LzmaRangeCoder.DecodeBit(ref state, ref range, input, ref pos, ref probs[ctx]);
            ctx = (ctx << 1) | bit;
        }
        return (byte)(ctx & 0xFF);
    }

    // Encode a match length (2–273) using a hierarchical probability model.
    // len 2–9  → 3-bit code in _matchLenProbs[0..7]
    // len 10+  → 8-bit code in _matchLenProbs[8..263]
    public void EncodeMatchLength(LzmaRangeCoder coder, int length)
    {
        length -= 2; // normalize to 0-based
        if (length < 8)
        {
            coder.EncodeBit(ref _matchLenProbs[0], 0);
            EncodeDirectBits(coder, length, 3, _matchLenProbs, 1);
        }
        else
        {
            coder.EncodeBit(ref _matchLenProbs[0], 1);
            EncodeDirectBits(coder, Math.Min(length - 8, 255), 8, _matchLenProbs, 9);
        }
    }

    // Decode a match length.
    public static int DecodeMatchLength(ushort[] probs, ref uint state, ref uint range,
                                         byte[] input, ref int pos)
    {
        int bit0 = LzmaRangeCoder.DecodeBit(ref state, ref range, input, ref pos, ref probs[0]);
        if (bit0 == 0)
            return 2 + DecodeDirectBits(3, probs, 1, ref state, ref range, input, ref pos);
        else
            return 10 + DecodeDirectBits(8, probs, 9, ref state, ref range, input, ref pos);
    }

    private static void EncodeDirectBits(LzmaRangeCoder coder, int value, int bits,
                                          ushort[] probs, int probOffset)
    {
        for (int i = bits - 1; i >= 0; i--)
        {
            int bit = (value >> i) & 1;
            coder.EncodeBit(ref probs[probOffset + (value >> (i + 1))], bit);
        }
    }

    private static int DecodeDirectBits(int bits, ushort[] probs, int probOffset,
                                         ref uint state, ref uint range, byte[] input, ref int pos)
    {
        int result = 0;
        for (int i = bits - 1; i >= 0; i--)
        {
            int bit = LzmaRangeCoder.DecodeBit(ref state, ref range, input, ref pos,
                          ref probs[probOffset + (result >> 1)]);
            result = (result << 1) | bit;
        }
        return result;
    }

    public ushort[] MatchLenProbs => _matchLenProbs;
    public ushort[] RepLenProbs   => _repLenProbs;
}
