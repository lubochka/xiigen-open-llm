// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Inference;
using TensorCore.Core;
using TransformerModel.Caching;

namespace LlmFromScratch.Tests;

public sealed class KvCacheStateIntegrityTests
{
    [Fact]
    public void ModelFacingContract_HasNoInferenceReferenceAndCanUpdateCache()
    {
        ITransformerKvCache cache = new KvCache(numLayers: 2);

        Assert.Equal(0, cache.GetUniformSequenceLength());
        var (keys, values) = cache.Update(0, Slice(1f), Slice(101f));

        Assert.Equal(2, cache.LayerCount);
        Assert.Equal(1, cache.GetSequenceLength(0));
        Assert.Equal(new[] { 1f, 2f, 3f, 4f }, keys.Data);
        Assert.Equal(new[] { 101f, 102f, 103f, 104f }, values.Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsNonPositiveLayerCount(int layerCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new KvCache(layerCount));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void StateMethods_ValidateLayerBeforeArrayAccess(int layer)
    {
        using var cache = new KvCache(2);

        Assert.Throws<ArgumentOutOfRangeException>(() => cache.GetSequenceLength(layer));
        Assert.Throws<ArgumentOutOfRangeException>(() => cache.Update(layer, Slice(1f), Slice(101f)));
    }

    [Fact]
    public void Update_PrefillThenDecode_ConcatenatesKAndVInSequenceOrder()
    {
        using var cache = new KvCache(1);
        var prefillK = Slice(1f, sequenceLength: 2);
        var prefillV = Slice(101f, sequenceLength: 2);
        var decodeK = Slice(9f);
        var decodeV = Slice(109f);

        cache.Update(0, prefillK, prefillV);
        var (keys, values) = cache.Update(0, decodeK, decodeV);

        Assert.Equal(new[] { 1, 1, 3, 4 }, keys.Shape.Dimensions);
        Assert.Equal(new[] { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f }, keys.Data);
        Assert.Equal(new[] { 101f, 102f, 103f, 104f, 105f, 106f, 107f, 108f, 109f, 110f, 111f, 112f }, values.Data);
        Assert.Equal(3, cache.SeqLen);
        Assert.Equal(3, cache.GetSequenceLength(0));
    }

    [Fact]
    public void Update_RejectsRankAndKvsShapeMismatchWithoutMutatingState()
    {
        using var cache = new KvCache(1);
        cache.Update(0, Slice(1f), Slice(101f));

        var rankError = Assert.Throws<ArgumentException>(() =>
            cache.Update(0, new Tensor(new float[4], new TensorShape(1, 1, 4)), Slice(201f)));
        Assert.Equal("KV_CACHE_INPUT_RANK_INVALID", rankError.Message);

        var shapeError = Assert.Throws<ArgumentException>(() =>
            cache.Update(0, Slice(2f), Slice(202f, headDimension: 2)));
        Assert.Equal("KV_CACHE_KEY_VALUE_SHAPE_MISMATCH", shapeError.Message);

        Assert.Equal(1, cache.GetSequenceLength(0));
        Assert.Equal(1, cache.SeqLen);
    }

    [Fact]
    public void Update_RejectsWithinAndCrossLayerLayoutMismatchWithoutMutatingState()
    {
        using var cache = new KvCache(2);
        cache.Update(0, Slice(1f, batch: 2, heads: 2), Slice(101f, batch: 2, heads: 2));

        AssertLayoutRejected(() => cache.Update(0, Slice(2f, batch: 1, heads: 2), Slice(102f, batch: 1, heads: 2)));
        AssertLayoutRejected(() => cache.Update(0, Slice(2f, batch: 2, heads: 1), Slice(102f, batch: 2, heads: 1)));
        AssertLayoutRejected(() => cache.Update(1, Slice(2f, batch: 2, heads: 2, headDimension: 2), Slice(102f, batch: 2, heads: 2, headDimension: 2)));

        Assert.Equal(1, cache.GetSequenceLength(0));
        Assert.Equal(0, cache.GetSequenceLength(1));
    }

    [Fact]
    public void Update_ValidatesBothSidesBeforePublishingEitherCacheTensor()
    {
        using var cache = new KvCache(1);
        cache.Update(0, Slice(1f), Slice(101f));

        var exception = Assert.Throws<ArgumentException>(() => cache.Update(0, Slice(5f), Slice(105f, headDimension: 2)));
        Assert.Equal("KV_CACHE_KEY_VALUE_SHAPE_MISMATCH", exception.Message);
        var (keys, values) = cache.Update(0, Slice(9f), Slice(109f));

        Assert.Equal(new[] { 1f, 2f, 3f, 4f, 9f, 10f, 11f, 12f }, keys.Data);
        Assert.Equal(new[] { 101f, 102f, 103f, 104f, 109f, 110f, 111f, 112f }, values.Data);
    }

    [Fact]
    public void StateQueries_FailClosedForHalfPublishedKeyValueState()
    {
        using var cache = new KvCache(1);
        cache.Update(0, Slice(1f), Slice(101f));
        var values = Assert.IsType<Tensor?[]>(typeof(KvCache)
            .GetField("_values", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(cache));
        values[0] = null;

        var exception = Assert.Throws<InvalidOperationException>(() => cache.GetSequenceLength(0));

        Assert.Equal("KV_CACHE_STATE_INCONSISTENT", exception.Message);
    }

    [Fact]
    public void Update_AndReturnedSnapshots_DoNotAliasInputsDataOrShapes()
    {
        using var cache = new KvCache(1);
        var keyData = new[] { 1f, 2f, 3f, 4f };
        var valueData = new[] { 101f, 102f, 103f, 104f };
        var keyShape = new TensorShape(1, 1, 1, 4);
        var valueShape = new TensorShape(1, 1, 1, 4);
        var keys = new Tensor(keyData, keyShape, requiresGrad: true);
        var values = new Tensor(valueData, valueShape, requiresGrad: true);

        var (returnedKeys, returnedValues) = cache.Update(0, keys, values);
        keys.Data[0] = -1f;
        values.Data[0] = -101f;
        keyShape.Dimensions[3] = 999;
        valueShape.Dimensions[3] = 999;
        returnedKeys.Data[0] = -2f;
        returnedValues.Data[0] = -102f;
        returnedKeys.Shape.Dimensions[3] = 777;
        returnedValues.Shape.Dimensions[3] = 777;

        var (storedKeys, storedValues) = cache.Update(0, Slice(9f), Slice(109f));

        Assert.Equal(new[] { 1, 1, 2, 4 }, storedKeys.Shape.Dimensions);
        Assert.Equal(new[] { 1, 1, 2, 4 }, storedValues.Shape.Dimensions);
        Assert.Equal(new[] { 1f, 2f, 3f, 4f, 9f, 10f, 11f, 12f }, storedKeys.Data);
        Assert.Equal(new[] { 101f, 102f, 103f, 104f, 109f, 110f, 111f, 112f }, storedValues.Data);
    }

    [Fact]
    public void Update_DetachesAutogradStateFromInputsAndSnapshots()
    {
        using var cache = new KvCache(1);
        var source = new Tensor(new[] { 1f, 2f, 3f, 4f }, new TensorShape(1, 1, 1, 4), requiresGrad: true);
        var value = new Tensor(new[] { 101f, 102f, 103f, 104f }, new TensorShape(1, 1, 1, 4), requiresGrad: true);

        var (keys, values) = cache.Update(0, source, value);

        Assert.False(keys.RequiresGrad);
        Assert.False(values.RequiresGrad);
        Assert.Null(keys.Grad);
        Assert.Null(values.Grad);
        Assert.Empty(keys.Children);
        Assert.Empty(values.Children);
    }

    [Fact]
    public void UniformLength_RejectsTransientSkewAndRecoversWhenLayersCatchUp()
    {
        using var cache = new KvCache(2);
        cache.Update(0, Slice(1f), Slice(101f));

        var mismatch = Assert.Throws<InvalidOperationException>(() => cache.GetUniformSequenceLength());
        Assert.Equal("KV_CACHE_LAYER_SEQUENCE_MISMATCH", mismatch.Message);

        cache.Update(1, Slice(5f), Slice(105f));

        Assert.Equal(1, cache.GetUniformSequenceLength());
    }

    [Fact]
    public void Clear_DropsLayoutAndAllowsDifferentValidReuse()
    {
        using var cache = new KvCache(2);
        cache.Update(0, Slice(1f, batch: 2, heads: 2), Slice(101f, batch: 2, heads: 2));
        cache.Clear();

        cache.Update(1, Slice(5f, batch: 1, heads: 3, headDimension: 2), Slice(105f, batch: 1, heads: 3, headDimension: 2));

        Assert.Equal(0, cache.SeqLen);
        Assert.Equal(0, cache.GetSequenceLength(0));
        Assert.Equal(1, cache.GetSequenceLength(1));
        Assert.Throws<InvalidOperationException>(() => cache.GetUniformSequenceLength());
    }

    [Fact]
    public void Dispose_IsIdempotentAndAllStateMethodsFailClosedAfterDisposal()
    {
        var cache = new KvCache(1);
        cache.Dispose();
        cache.Dispose();

        Assert.Equal(1, cache.LayerCount);
        Assert.Throws<ObjectDisposedException>(() => _ = cache.SeqLen);
        Assert.Throws<ObjectDisposedException>(() => cache.GetSequenceLength(0));
        Assert.Throws<ObjectDisposedException>(() => cache.GetUniformSequenceLength());
        Assert.Throws<ObjectDisposedException>(() => cache.Update(0, Slice(1f), Slice(101f)));
        Assert.Throws<ObjectDisposedException>(() => cache.Clear());
    }

    private static void AssertLayoutRejected(Action action)
    {
        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("KV_CACHE_LAYOUT_MISMATCH", exception.Message);
    }

    private static Tensor Slice(
        float start,
        int batch = 1,
        int heads = 1,
        int sequenceLength = 1,
        int headDimension = 4)
    {
        var data = Enumerable.Range(0, checked(batch * heads * sequenceLength * headDimension))
            .Select(index => start + index)
            .ToArray();
        return new Tensor(data, new TensorShape(batch, heads, sequenceLength, headDimension));
    }
}
