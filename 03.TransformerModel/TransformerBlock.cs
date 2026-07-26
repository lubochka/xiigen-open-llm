// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Config;
using TransformerModel.Layers;

namespace TransformerModel;

// A single transformer decoder block:
//   x = x + attention(norm1(x))
//   x = x + ffn(norm2(x))
// Residual connections let gradients flow through 32+ stacked blocks without vanishing.
public class TransformerBlock : Module
{
    private readonly RmsNorm             _norm1;
    private readonly CausalSelfAttention _attn;
    private readonly RmsNorm             _norm2;
    private readonly SwiGLUFeedForward   _ffn;

    // Public access for LoraApplicator
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

    public Tensor Forward(Tensor x)
    {
        // Pre-norm residual block: normalize before the operation
        x = TensorOps.Add(x, _attn.Forward(_norm1.Forward(x)));
        x = TensorOps.Add(x, _ffn.Forward(_norm2.Forward(x)));
        return x;
    }
}
