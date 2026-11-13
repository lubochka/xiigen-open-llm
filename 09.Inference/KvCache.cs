// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TransformerModel.Caching;

namespace Inference;

/// <summary>
/// Owns detached decoder KV state. A cache may temporarily be at different
/// sequence lengths per layer while a decoder walks its layers, but every
/// populated layer has the same batch, KV-head and head-dimension layout.
/// </summary>
public class KvCache : ITransformerKvCache, IDisposable
{
    private const string InputRankInvalid = "KV_CACHE_INPUT_RANK_INVALID";
    private const string KeyValueShapeMismatch = "KV_CACHE_KEY_VALUE_SHAPE_MISMATCH";
    private const string LayoutMismatch = "KV_CACHE_LAYOUT_MISMATCH";
    private const string SequenceLengthOverflow = "KV_CACHE_SEQUENCE_LENGTH_OVERFLOW";
    private const string StateInconsistent = "KV_CACHE_STATE_INCONSISTENT";
    private const string LayerSequenceMismatch = "KV_CACHE_LAYER_SEQUENCE_MISMATCH";

    private readonly Tensor?[] _keys;
    private readonly Tensor?[] _values;
    private int? _batchSize;
    private int? _kvHeads;
    private int? _headDimension;
    private int _seqLen;
    private bool _disposed;

    public int LayerCount => _keys.Length;

    /// <summary>Compatibility alias for layer zero only.</summary>
    public int SeqLen
    {
        get
        {
            ThrowIfDisposed();
            return _seqLen;
        }
        private set => _seqLen = value;
    }

    public KvCache(int numLayers)
    {
        if (numLayers <= 0)
            throw new ArgumentOutOfRangeException(nameof(numLayers), "KV cache requires at least one layer.");

        _keys = new Tensor?[numLayers];
        _values = new Tensor?[numLayers];
    }

    public int GetSequenceLength(int layer)
    {
        ThrowIfDisposed();
        ValidateLayer(layer);
        EnsureStateIntegrity();

        return _keys[layer] is null ? 0 : _keys[layer]!.Shape.Dimensions[2];
    }

    public int GetUniformSequenceLength()
    {
        ThrowIfDisposed();
        EnsureStateIntegrity();
        int? uniform = null;
        for (int layer = 0; layer < LayerCount; layer++)
        {
            int current = _keys[layer] is null ? 0 : _keys[layer]!.Shape.Dimensions[2];
            if (uniform is null)
            {
                uniform = current;
                continue;
            }

            if (uniform.Value != current)
                throw new InvalidOperationException(LayerSequenceMismatch);
        }

        return uniform ?? 0;
    }

    /// <summary>
    /// Appends a rank-four [batch, kvHeads, deltaSequence, headDimension]
    /// slice to a layer and returns detached snapshots of the full layer state.
    /// </summary>
    public (Tensor keys, Tensor values) Update(int layer, Tensor newK, Tensor newV)
    {
        ThrowIfDisposed();
        ValidateLayer(layer);
        ArgumentNullException.ThrowIfNull(newK);
        ArgumentNullException.ThrowIfNull(newV);
        ValidateInputPair(newK, newV);
        EnsureStateIntegrity();

        var dimensions = newK.Shape.Dimensions;
        int batchSize = dimensions[0];
        int kvHeads = dimensions[1];
        int deltaSequenceLength = dimensions[2];
        int headDimension = dimensions[3];

        ValidateLayout(batchSize, kvHeads, headDimension);

        Tensor nextKeys;
        Tensor nextValues;
        Tensor? existingKeys = _keys[layer];
        Tensor? existingValues = _values[layer];

        if (existingKeys is null)
        {
            nextKeys = DetachedSnapshot(newK);
            nextValues = DetachedSnapshot(newV);
        }
        else
        {
            int existingSequenceLength = existingKeys.Shape.Dimensions[2];
            int nextSequenceLength;
            try
            {
                nextSequenceLength = checked(existingSequenceLength + deltaSequenceLength);
            }
            catch (OverflowException exception)
            {
                throw new InvalidOperationException(SequenceLengthOverflow, exception);
            }

            nextKeys = AppendAlongSequence(existingKeys, newK, batchSize, kvHeads, existingSequenceLength, deltaSequenceLength, nextSequenceLength, headDimension);
            nextValues = AppendAlongSequence(existingValues!, newV, batchSize, kvHeads, existingSequenceLength, deltaSequenceLength, nextSequenceLength, headDimension);
        }

        // Publish a complete pair only after every validation and allocation succeeds.
        _keys[layer] = nextKeys;
        _values[layer] = nextValues;
        _batchSize = batchSize;
        _kvHeads = kvHeads;
        _headDimension = headDimension;
        if (layer == 0)
            SeqLen = nextKeys.Shape.Dimensions[2];

        return (DetachedSnapshot(nextKeys), DetachedSnapshot(nextValues));
    }

    public void Clear()
    {
        ThrowIfDisposed();

        for (int i = 0; i < LayerCount; i++)
        {
            _keys[i] = null;
            _values[i] = null;
        }

        _batchSize = null;
        _kvHeads = null;
        _headDimension = null;
        SeqLen = 0;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Clear();
        _disposed = true;
    }

    private void ValidateLayer(int layer)
    {
        if (layer < 0 || layer >= LayerCount)
            throw new ArgumentOutOfRangeException(nameof(layer), $"Layer must be in [0, {LayerCount - 1}].");
    }

    private static void ValidateInputPair(Tensor newK, Tensor newV)
    {
        if (newK.Shape.Rank != 4 || newV.Shape.Rank != 4)
            throw new ArgumentException(InputRankInvalid);

        if (!newK.Shape.Dimensions.SequenceEqual(newV.Shape.Dimensions))
            throw new ArgumentException(KeyValueShapeMismatch);

        if (newK.Shape.Dimensions[2] < 1)
            throw new ArgumentException(LayoutMismatch);
    }

    private void ValidateLayout(int batchSize, int kvHeads, int headDimension)
    {
        if (_batchSize is null)
            return;

        if (_batchSize.Value != batchSize ||
            _kvHeads!.Value != kvHeads ||
            _headDimension!.Value != headDimension)
        {
            throw new ArgumentException(LayoutMismatch);
        }
    }

    private void EnsureStateIntegrity()
    {
        bool hasAnyLayer = false;
        for (int layer = 0; layer < LayerCount; layer++)
        {
            Tensor? keys = _keys[layer];
            Tensor? values = _values[layer];
            if ((keys is null) != (values is null))
                throw new InvalidOperationException(StateInconsistent);

            if (keys is null)
                continue;

            hasAnyLayer = true;
            if (keys.Shape.Rank != 4 ||
                values!.Shape.Rank != 4 ||
                !keys.Shape.Dimensions.SequenceEqual(values.Shape.Dimensions) ||
                _batchSize is null ||
                _kvHeads is null ||
                _headDimension is null ||
                keys.Shape.Dimensions[0] != _batchSize.Value ||
                keys.Shape.Dimensions[1] != _kvHeads.Value ||
                keys.Shape.Dimensions[2] < 1 ||
                keys.Shape.Dimensions[3] != _headDimension.Value)
            {
                throw new InvalidOperationException(StateInconsistent);
            }
        }

        if (hasAnyLayer != _batchSize.HasValue ||
            hasAnyLayer != _kvHeads.HasValue ||
            hasAnyLayer != _headDimension.HasValue)
        {
            throw new InvalidOperationException(StateInconsistent);
        }

        int expectedSeqLen = _keys[0] is null ? 0 : _keys[0]!.Shape.Dimensions[2];
        if (_seqLen != expectedSeqLen)
            throw new InvalidOperationException(StateInconsistent);
    }

    private static Tensor AppendAlongSequence(
        Tensor existing,
        Tensor append,
        int batchSize,
        int kvHeads,
        int existingSequenceLength,
        int deltaSequenceLength,
        int nextSequenceLength,
        int headDimension)
    {
        var combined = new float[checked(batchSize * kvHeads * nextSequenceLength * headDimension)];
        int existingBlockLength = checked(existingSequenceLength * headDimension);
        int appendBlockLength = checked(deltaSequenceLength * headDimension);
        int combinedBlockLength = checked(nextSequenceLength * headDimension);

        for (int block = 0; block < checked(batchSize * kvHeads); block++)
        {
            int existingOffset = checked(block * existingBlockLength);
            int appendOffset = checked(block * appendBlockLength);
            int combinedOffset = checked(block * combinedBlockLength);
            Array.Copy(existing.Data, existingOffset, combined, combinedOffset, existingBlockLength);
            Array.Copy(append.Data, appendOffset, combined, combinedOffset + existingBlockLength, appendBlockLength);
        }

        return new Tensor(combined, new TensorShape(batchSize, kvHeads, nextSequenceLength, headDimension));
    }

    private static Tensor DetachedSnapshot(Tensor source) =>
        new(source.Data.ToArray(), new TensorShape(source.Shape.Dimensions.ToArray()), requiresGrad: false);

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(KvCache));
    }
}
