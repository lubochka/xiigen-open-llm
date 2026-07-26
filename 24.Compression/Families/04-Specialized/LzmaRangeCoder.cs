// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Compression;

// Range coder for LZMA — implements arithmetic range coding with carry propagation.
// Uses the LZMA-SDK carry-save mechanism: a cached byte absorbs backward carry
// so previously-written bytes never need to be patched.
// The encoder naturally emits a leading 0x00 byte (from the initial cache=0 flush).
// The decoder must therefore consume 5 initialization bytes so the 0x00 shifts out.
// Reference: LZMA-SDK LzmaDec.c / LzmaEnc.c (public domain)
public sealed class LzmaRangeCoder
{
    private ulong _low       = 0;
    private uint  _range     = 0xFFFFFFFF;
    private byte  _cache     = 0;      // last byte pending output (may get carry added)
    private int   _cacheSize = 1;      // how many 0xFF-bytes follow _cache (for carry chains)

    private const uint MinimumRange = 1u << 24;

    public List<byte> Output { get; } = new();

    public void EncodeBit(ref ushort probability, int actualBit)
    {
        uint mid = (_range >> 11) * probability;

        if (actualBit == 0)
        {
            _range     = mid;
            probability += (ushort)((2048 - probability) >> 5);
        }
        else
        {
            _low   += mid;
            _range -= mid;
            probability -= (ushort)(probability >> 5);
        }

        while (_range < MinimumRange)
        {
            ShiftLow();
            _range <<= 8;
        }
    }

    // Flush the remaining state — emits _cache + 4 more bytes.
    public void Finish()
    {
        for (int i = 0; i < 5; i++)
        {
            ShiftLow();
            _range <<= 8;
        }
    }

    // Carry-save flush: delays output by one byte so a future carry can increment it.
    // Mirrors LZMA-SDK RangeEnc_ShiftLow exactly.
    private void ShiftLow()
    {
        uint lowBits32 = (uint)_low;           // low 32 bits
        byte carry     = (byte)(_low >> 32);   // 0 or 1 (carry out of bit 31)

        if (lowBits32 < 0xFF000000u || carry != 0)
        {
            // Safe to flush: carry chain resolves here.
            Output.Add((byte)(_cache + carry));
            byte fill = (byte)(0xFF + carry); // carry=0 → 0xFF; carry=1 → 0x00
            for (int i = 1; i < _cacheSize; i++)
                Output.Add(fill);
            _cache     = (byte)(lowBits32 >> 24); // new pending byte
            _cacheSize = 1;
        }
        else
        {
            // lowBits32 ∈ [0xFF000000, 0xFFFFFFFF] and no carry yet — this byte
            // might absorb a future carry, so buffer it.
            _cacheSize++;
        }

        // Advance _low: discard the high byte, shift remaining 24 bits up by 8.
        // Keep result as uint (bits 32+ cleared) stored in ulong.
        _low = (ulong)((lowBits32 & 0x00FFFFFF) << 8);
    }

    // Decode one bit from the input stream using the same probability model.
    public static int DecodeBit(ref uint state, ref uint range, byte[] input, ref int pos,
                                 ref ushort probability)
    {
        while (range < MinimumRange && pos < input.Length)
        {
            state  = (state << 8) | input[pos++];
            range <<= 8;
        }

        uint mid = (range >> 11) * probability;

        int bit;
        if (state < mid)
        {
            range      = mid;
            probability += (ushort)((2048 - probability) >> 5);
            bit = 0;
        }
        else
        {
            state  -= mid;
            range  -= mid;
            probability -= (ushort)(probability >> 5);
            bit = 1;
        }

        return bit;
    }
}
