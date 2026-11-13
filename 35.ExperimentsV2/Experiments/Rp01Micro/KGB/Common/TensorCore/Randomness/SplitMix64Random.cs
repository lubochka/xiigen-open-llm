// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TensorCore.Randomness;

public readonly record struct SplitMix64RandomState(
    ulong Seed,
    ulong ConsumedCursor);

public sealed class SplitMix64Random
{
    private readonly ulong _seed;
    private ulong _state;
    private ulong _consumedCursor;

    public SplitMix64Random(ulong seed)
    {
        _seed = seed;
        _state = seed;
    }

    public SplitMix64RandomState CaptureState()
        => new(_seed, _consumedCursor);

    public void RestoreState(SplitMix64RandomState state)
    {
        if (state.Seed != _seed)
            throw new InvalidOperationException("DPO_RANDOM_STATE_SEED_MISMATCH");

        unchecked
        {
            _state = state.Seed + state.ConsumedCursor * 0x9E3779B97F4A7C15UL;
        }
        _consumedCursor = state.ConsumedCursor;
    }

    public ulong NextUInt64()
    {
        unchecked
        {
            var value = (_state += 0x9E3779B97F4A7C15UL);
            _consumedCursor = checked(_consumedCursor + 1);
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }

    public double NextUnitDouble()
        => (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);
}
