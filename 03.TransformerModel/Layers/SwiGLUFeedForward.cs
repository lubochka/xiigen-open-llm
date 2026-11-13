// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Config;

namespace TransformerModel.Layers;

// SwiGLU Feed-Forward Network.
// gate = silu(x @ gate_proj^T) — плавный вентиль: что пропускать
// up   = x @ up_proj^T
// out  = (gate * up) @ down_proj^T
// SwiGLU работает лучше ReLU в трансформерах (PaLM, LLaMA, Mistral).
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
        => Forward(x, CancellationToken.None);

    // Прежняя перегрузка сохраняет прежний путь: признак обучения выключен,
    // поэтому знаменатель плавного вентиля не запоминается.
    public Tensor Forward(Tensor x, CancellationToken cancellationToken)
        => Forward(x, training: false, cancellationToken);

    // FIX-SSR transfer, plan 47-silu-sigmoid-reuse.md, from SILU01.
    // Учебный вызов передаёт уже существующий признак обучения в плавный вентиль,
    // чтобы его обратный проход взял сохранённый знаменатель вместо второй
    // экспоненты. Ничего другого эта перегрузка не меняет.
    public Tensor Forward(Tensor x, bool training, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // gate активируется SiLU, up проходит без активации, перемножаются поэлементно
        var gate = TensorOps.Silu(
            _gateProj.Forward(x, cancellationToken),
            // Запоминание знаменателя включается ровно на учебном проходе: вход
            // здесь — свежий тензор проекции вентиля, и до обратного прохода он
            // не меняется.
            cacheBackwardDenominator: training,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var up   = _upProj.Forward(x, cancellationToken);
        return _downProj.Forward(
            TensorOps.Multiply(gate, up, cancellationToken),
            cancellationToken);
    }
}
