// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Config;

namespace TransformerModel.Layers;

// SwiGLU Feed-Forward Network.
// gate = silu(x @ gate_proj^T) — a smooth gate: what to let through
// up   = x @ up_proj^T
// out  = (gate * up) @ down_proj^T
// SwiGLU works better than ReLU in transformers (PaLM, LLaMA, Mistral).
public class SwiGLUFeedForward : Module
{
    private readonly Linear _gateProj;
    private readonly Linear _upProj;
    private readonly Linear _downProj;

    public SwiGLUFeedForward(ModelConfig cfg)
    {
        int h   = cfg.HiddenDim;
        int ffn = cfg.FfnHiddenDim;

        _gateProj = new Linear(h, ffn, useBias: false);
        _upProj   = new Linear(h, ffn, useBias: false);
        _downProj = new Linear(ffn, h, useBias: false);

        RegisterModule("gate_proj", _gateProj);
        RegisterModule("up_proj",   _upProj);
        RegisterModule("down_proj", _downProj);
    }

    public Tensor Forward(Tensor x)
    {
        // gate is activated by SiLU, up passes without activation, multiplied element-wise
        var gate = TensorOps.Silu(_gateProj.Forward(x));
        var up   = _upProj.Forward(x);
        return _downProj.Forward(TensorOps.Multiply(gate, up));
    }
}
