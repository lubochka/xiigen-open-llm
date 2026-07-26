// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace Inference;

// Per-layer key/value cache for autoregressive generation.
// At each decoding step we append one token and concatenate it with the cache.
public class KvCache : IDisposable
{
    private readonly Tensor?[] _keys;
    private readonly Tensor?[] _values;
    private bool _disposed;

    public int SeqLen { get; private set; }

    public KvCache(int numLayers)
    {
        _keys   = new Tensor?[numLayers];
        _values = new Tensor?[numLayers];
    }

    // Append a new K/V slice and return the full accumulated cache.
    // newK/newV: [batch, heads, 1, headDim]
    // Returns: [batch, heads, seqLen, headDim]
    public (Tensor keys, Tensor values) Update(int layer, Tensor newK, Tensor newV)
    {
        if (_keys[layer] is null)
        {
            // First token — just copy the data
            _keys[layer]   = new Tensor(newK.Data.ToArray(), newK.Shape);
            _values[layer] = new Tensor(newV.Data.ToArray(), newV.Shape);
        }
        else
        {
            // Concatenate along axis 2 (the sequence axis)
            _keys[layer]   = TensorOps.Concat(_keys[layer]!,   newK, axis: 2);
            _values[layer] = TensorOps.Concat(_values[layer]!, newV, axis: 2);
        }

        // SeqLen is tracked from layer 0
        if (layer == 0) SeqLen = _keys[0]!.Shape.Dimensions[2];

        return (_keys[layer]!, _values[layer]!);
    }

    // Clear the cache (start a new generation)
    public void Clear()
    {
        for (int i = 0; i < _keys.Length; i++)
        {
            _keys[i]   = null;
            _values[i] = null;
        }
        SeqLen = 0;
    }

    public void Dispose()
    {
        if (_disposed) return;
        Clear();
        _disposed = true;
    }
}
