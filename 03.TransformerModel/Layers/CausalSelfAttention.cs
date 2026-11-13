// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Caching;
using TransformerModel.Config;

namespace TransformerModel.Layers;

public interface IAttentionProjectionAdapter
{
    Linear BaseProjection { get; }
    Module RegistrationModule { get; }
    IReadOnlyList<(string Suffix, Parameter Parameter)> AdapterParameters { get; }
    TransformerAdapterRandomState CaptureRandomState()
        => throw new NotSupportedException(
            "This projection adapter does not expose deterministic random state.");
    void RestoreRandomState(TransformerAdapterRandomState state)
        => throw new NotSupportedException(
            "This projection adapter does not expose deterministic random state.");
    Tensor Forward(Tensor input, bool training);
    Tensor Forward(Tensor input, bool training, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Forward(input, training);
    }
}

public sealed record AttentionProjectionBinding(
    string Projection,
    Linear BaseProjection,
    IAttentionProjectionAdapter? Adapter);

// Каузальное (masked) multi-head self-attention с Grouped Query Attention (GQA).
// GQA: Q использует NumHeads голов, K и V делят NumKvHeads голов — экономия памяти.
// "Каузальное" — каждый токен видит только себя и предыдущие токены (no future leakage).
public class CausalSelfAttention : Module
{
    private readonly Linear _qProj;
    private readonly Linear _kProj;
    private readonly Linear _vProj;
    private readonly Linear _oProj;
    private readonly RotaryEmbedding _rope;
    private readonly ModelConfig _cfg;
    private IAttentionProjectionAdapter? _qAdapter;
    private IAttentionProjectionAdapter? _kAdapter;
    private IAttentionProjectionAdapter? _vAdapter;
    private IAttentionProjectionAdapter? _oAdapter;

    internal static readonly string[] CanonicalProjectionNames =
        ["q_proj", "k_proj", "v_proj", "o_proj"];

    public CausalSelfAttention(ModelConfig cfg)
    {
        _cfg = cfg;
        int h  = cfg.HiddenDim;
        int hd = cfg.HeadDim;

        _qProj = new Linear(h, cfg.NumHeads   * hd, useBias: false);
        _kProj = new Linear(h, cfg.NumKvHeads * hd, useBias: false);
        _vProj = new Linear(h, cfg.NumKvHeads * hd, useBias: false);
        _oProj = new Linear(h, h,                  useBias: false);
        _rope  = new RotaryEmbedding(hd, cfg.RopeTheta);

        RegisterModule("q_proj", _qProj);
        RegisterModule("k_proj", _kProj);
        RegisterModule("v_proj", _vProj);
        RegisterModule("o_proj", _oProj);
        RegisterModule("rope",   _rope);
    }

    public IReadOnlyList<AttentionProjectionBinding> ProjectionBindings =>
    [
        new("q_proj", _qProj, _qAdapter),
        new("k_proj", _kProj, _kAdapter),
        new("v_proj", _vProj, _vAdapter),
        new("o_proj", _oProj, _oAdapter)
    ];

    internal AttentionProjectionBinding GetProjectionBinding(string projection)
        => projection switch
        {
            "q_proj" => new(projection, _qProj, _qAdapter),
            "k_proj" => new(projection, _kProj, _kAdapter),
            "v_proj" => new(projection, _vProj, _vAdapter),
            "o_proj" => new(projection, _oProj, _oAdapter),
            _ => throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT")
        };

    internal void AttachProjectionAdapterPrevalidated(
        LlmModel.AdapterMutationLease lease,
        string projection,
        IAttentionProjectionAdapter adapter)
    {
        lease.ValidateActive();
        var binding = GetProjectionBinding(projection);
        if (!ReferenceEquals(binding.BaseProjection, adapter.BaseProjection))
            throw new InvalidOperationException("LORA_PROJECTION_ADAPTER_CONFLICT");
        if (binding.Adapter is not null && !ReferenceEquals(binding.Adapter, adapter))
            throw new InvalidOperationException("LORA_PROJECTION_ADAPTER_CONFLICT");

        switch (projection)
        {
            case "q_proj":
                _qAdapter = adapter;
                break;
            case "k_proj":
                _kAdapter = adapter;
                break;
            case "v_proj":
                _vAdapter = adapter;
                break;
            case "o_proj":
                _oAdapter = adapter;
                break;
            default:
                throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");
        }
    }

    public Tensor Forward(Tensor x) => Forward(x, training: false);

    public Tensor Forward(Tensor x, bool training)
        => Forward(x, training, CancellationToken.None);

    public Tensor Forward(Tensor x, bool training, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int batch = x.Shape.Dimensions[0];
        int seq   = x.Shape.Dimensions[1];
        int h     = _cfg.NumHeads;
        int kv    = _cfg.NumKvHeads;
        int hd    = _cfg.HeadDim;

        // Проецируем в Q, K, V и делим на головы
        // [B, T, h*hd] → reshape → [B, h, T, hd]
        var q = Reshape4D(ForwardProjection(_qProj, _qAdapter, x, training, cancellationToken), batch, seq, h,  hd, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var k = Reshape4D(ForwardProjection(_kProj, _kAdapter, x, training, cancellationToken), batch, seq, kv, hd, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var v = Reshape4D(ForwardProjection(_vProj, _vAdapter, x, training, cancellationToken), batch, seq, kv, hd, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        // Применяем RoPE к Q и K
        var (cos, sin) = _rope.GetFrequencies(seq, cancellationToken);
        q = RotaryEmbedding.Apply(q, cos, sin, cancellationToken);
        k = RotaryEmbedding.Apply(k, cos, sin, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        // GQA: расширяем K и V до h голов (повторяем каждую голову KvGroupSize раз)
        if (kv < h)
        {
            int g = _cfg.KvGroupSize;
            k = TensorOps.RepeatInterleave(k, g, dim: 1, cancellationToken);
            v = TensorOps.RepeatInterleave(v, g, dim: 1, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        // Scaled dot-product attention с каузальной маской
        var attnOut = ScaledDotProduct(q, k, v, seq, cancellationToken);

        // [B, h, T, hd] → [B, T, H] → _oProj
        var combined = Reshape3D(attnOut, batch, seq, h * hd, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var projected = ForwardProjection(_oProj, _oAdapter, combined, training, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return projected;
    }

    internal Tensor ForwardWithCache(
        Tensor x,
        ITransformerKvCache cache,
        int layerIndex,
        int positionOffset,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(cache);

        int batch = x.Shape.Dimensions[0];
        int seq = x.Shape.Dimensions[1];
        int h = _cfg.NumHeads;
        int kv = _cfg.NumKvHeads;
        int hd = _cfg.HeadDim;

        if (cache.GetSequenceLength(layerIndex) != positionOffset)
            throw new InvalidOperationException("LLM_KV_CACHE_LAYER_POSITION_MISMATCH");
        if ((long)positionOffset + seq > _cfg.ContextLen)
            throw new InvalidOperationException("LLM_KV_CONTEXT_LENGTH_EXCEEDED");

        var q = Reshape4D(
            ForwardProjection(_qProj, _qAdapter, x, training: false, cancellationToken),
            batch,
            seq,
            h,
            hd,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var k = Reshape4D(
            ForwardProjection(_kProj, _kAdapter, x, training: false, cancellationToken),
            batch,
            seq,
            kv,
            hd,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var v = Reshape4D(
            ForwardProjection(_vProj, _vAdapter, x, training: false, cancellationToken),
            batch,
            seq,
            kv,
            hd,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        var (cos, sin) = _rope.GetFrequencies(seq, positionOffset, cancellationToken);
        q = RotaryEmbedding.Apply(q, cos, sin, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        k = RotaryEmbedding.Apply(k, cos, sin, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        var cached = cache.Update(layerIndex, k, v);
        cancellationToken.ThrowIfCancellationRequested();
        k = cached.keys;
        v = cached.values;

        if (kv < h)
        {
            int g = _cfg.KvGroupSize;
            k = TensorOps.RepeatInterleave(k, g, dim: 1, cancellationToken);
            v = TensorOps.RepeatInterleave(v, g, dim: 1, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        var attnOut = ScaledDotProductWithPast(
            q,
            k,
            v,
            positionOffset,
            cancellationToken);
        var combined = Reshape3D(attnOut, batch, seq, h * hd, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var projected = ForwardProjection(_oProj, _oAdapter, combined, training: false, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return projected;
    }

    private static Tensor ForwardProjection(
        Linear baseProjection,
        IAttentionProjectionAdapter? adapter,
        Tensor input,
        bool training,
        CancellationToken cancellationToken)
        => adapter is null
            ? baseProjection.Forward(input, cancellationToken)
            : adapter.Forward(input, training, cancellationToken);

    // Scaled dot-product attention:
    // scores = Q @ K^T / sqrt(hd), применяем каузальную маску, softmax, @ V
    private static Tensor ScaledDotProduct(
        Tensor q,
        Tensor k,
        Tensor v,
        int seq,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        float scale = MathF.Sqrt(q.Shape.Dimensions[3]);

        // scores: [B, h, T, T]
        var kT     = TensorOps.Transpose(k, -2, -1, cancellationToken); // [B, h, hd, T]
        cancellationToken.ThrowIfCancellationRequested();
        // FIX-CS: только причинно видимые ячейки. Всё выше диагонали следующая
        // строка всё равно превращает в минус бесконечность, поэтому считать её
        // незачем. Для прямоугольного блока с прошлыми ключами это неверно, и
        // там ниже по-прежнему обычное произведение.
        var scores = TensorOps.CausalScores(q, kT, cancellationToken); // [B, h, T, T]
        cancellationToken.ThrowIfCancellationRequested();
        scores     = TensorOps.Scale(scores, 1f / scale, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        // Каузальная маска: позиции выше диагонали заполняем -inf
        scores = TensorOps.ApplyCausalMask(
            scores,
            float.NegativeInfinity,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        // Softmax по последней оси (по ключам)
        var weights = TensorOps.Softmax(scores, dim: -1, cancellationToken); // [B, h, T, T]
        cancellationToken.ThrowIfCancellationRequested();
        return TensorOps.MatMul(weights, v, cancellationToken); // [B, h, T, hd]
    }

    private static Tensor ScaledDotProductWithPast(
        Tensor q,
        Tensor k,
        Tensor v,
        int pastKeyLength,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        float scale = MathF.Sqrt(q.Shape.Dimensions[3]);

        var kT = TensorOps.Transpose(k, -2, -1, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var scores = TensorOps.MatMul(q, kT, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        scores = TensorOps.Scale(scores, 1f / scale, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        scores = TensorOps.ApplyCausalMaskWithPast(
            scores,
            float.NegativeInfinity,
            pastKeyLength,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var weights = TensorOps.Softmax(scores, dim: -1, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return TensorOps.MatMul(weights, v, cancellationToken);
    }

    // Заполнить позиции вне нижнего треугольника -inf
    // [B, T, heads*hd] → [B, heads, T, hd]
    private static Tensor Reshape4D(
        Tensor x,
        int batch,
        int seq,
        int heads,
        int hd,
        CancellationToken cancellationToken)
    {
        // x уже [B, T, heads*hd], переставим в [B, heads, T, hd]
        return TensorOps.Transpose(
            TensorOps.Reshape(x, cancellationToken, batch, seq, heads, hd),
            1,
            2,
            cancellationToken);
    }

    // [B, heads, T, hd] → [B, T, heads*hd]
    private static Tensor Reshape3D(
        Tensor x,
        int batch,
        int seq,
        int total,
        CancellationToken cancellationToken)
    {
        return TensorOps.Reshape(
            TensorOps.Transpose(x, 1, 2, cancellationToken),
            cancellationToken,
            batch,
            seq,
            total);
    }
}
