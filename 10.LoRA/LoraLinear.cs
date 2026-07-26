// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;

namespace LoRA;

// LoRA adapter on top of a frozen Linear layer.
// Output = frozen_linear(x) + scaling * (x @ A^T) @ B^T
// A is initialized with Kaiming; B = 0 (the adapter starts as a zero correction).
public class LoraLinear : Module
{
    private readonly Linear    _base;
    private readonly Parameter _loraA;
    private readonly Parameter _loraB;
    private readonly Dropout   _dropout;
    private readonly float     _scaling;

    public LoraLinear(Linear baseLinear, LoraConfig config)
    {
        _base    = baseLinear;
        _scaling = config.Scaling;
        _dropout = new Dropout(config.Dropout);

        int inDim  = baseLinear.InFeatures;
        int outDim = baseLinear.OutFeatures;

        // A — the projection matrix into the low-rank space
        var aData = Initializers.KaimingUniform(config.Rank, inDim);
        _loraA = new Parameter(new Tensor(aData, new TensorShape(config.Rank, inDim)));

        // B — the matrix that expands back, initialized with zeros
        var bData = Initializers.Zeros(outDim, config.Rank);
        _loraB = new Parameter(new Tensor(bData, new TensorShape(outDim, config.Rank)));

        // Freeze the base weights
        baseLinear.Weight.Value.RequiresGrad = false;
        if (baseLinear.Bias != null)
            baseLinear.Bias.Value.RequiresGrad = false;

        RegisterModule("base",    _base);
        RegisterModule("dropout", _dropout);
        RegisterParameter("lora_a", _loraA);
        RegisterParameter("lora_b", _loraB);
    }

    // output = base(x) + scaling * dropout(x) @ A^T @ B^T
    public Tensor Forward(Tensor x)
    {
        var baseOut = _base.Forward(x);

        // Low-rank path: x → dropout → @ A^T → @ B^T → * scaling
        var xDrop = _dropout.Forward(x, training: true);
        var aT    = TensorOps.Transpose(_loraA.Value, 0, 1); // [in, rank]
        var bT    = TensorOps.Transpose(_loraB.Value, 0, 1); // [rank, out]
        var lora  = TensorOps.MatMul(TensorOps.MatMul(xDrop, aT), bT);
        lora      = TensorOps.Scale(lora, _scaling);

        return TensorOps.Add(baseOut, lora);
    }
}
