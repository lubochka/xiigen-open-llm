// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Caching;
using TransformerModel.Config;
using TransformerModel.Layers;

namespace TransformerModel;

// Один декодерный блок трансформера:
//   x = x + attention(norm1(x))
//   x = x + ffn(norm2(x))
// Residual connections позволяют градиентам течь через 32+ стека без затухания.
public class TransformerBlock : Module
{
    private readonly RmsNorm             _norm1;
    private readonly CausalSelfAttention _attn;
    private readonly RmsNorm             _norm2;
    private readonly SwiGLUFeedForward   _ffn;

    // Открытый доступ для LoraApplicator
    public CausalSelfAttention Attention => _attn;

    public TransformerBlock(ModelConfig cfg)
    {
        _norm1 = new RmsNorm(cfg.HiddenDim, cfg.RmsNormEps);
        _attn  = new CausalSelfAttention(cfg);
        _norm2 = new RmsNorm(cfg.HiddenDim, cfg.RmsNormEps);
        _ffn   = new SwiGLUFeedForward(cfg);

        RegisterModule("norm1", _norm1);
        RegisterModule("attn",  _attn);
        RegisterModule("norm2", _norm2);
        RegisterModule("ffn",   _ffn);
    }

    public Tensor Forward(Tensor x) => Forward(x, training: false);

    public Tensor Forward(Tensor x, bool training)
        => Forward(x, training, CancellationToken.None);

    public Tensor Forward(Tensor x, bool training, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Pre-norm резидуальный блок: нормируем перед операцией
        var normalizedAttentionInput = _norm1.Forward(x, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        x = TensorOps.Add(
            x,
            _attn.Forward(normalizedAttentionInput, training, cancellationToken),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedFeedForwardInput = _norm2.Forward(x, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        x = TensorOps.Add(
            x,
            _ffn.Forward(normalizedFeedForwardInput, cancellationToken),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return x;
    }

    internal Tensor ForwardWithCache(
        Tensor x,
        ITransformerKvCache cache,
        int layerIndex,
        int positionOffset,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedAttentionInput = _norm1.Forward(x, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        x = TensorOps.Add(
            x,
            _attn.ForwardWithCache(
                normalizedAttentionInput,
                cache,
                layerIndex,
                positionOffset,
                cancellationToken),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedFeedForwardInput = _norm2.Forward(x, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        x = TensorOps.Add(
            x,
            _ffn.Forward(normalizedFeedForwardInput, cancellationToken),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return x;
    }
}
