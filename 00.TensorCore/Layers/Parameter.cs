// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace TensorCore.Layers;

// Trainable network parameter — a tensor for which the gradient is always computed.
// Analogous to torch.nn.Parameter.
public sealed class Parameter
{
    // Numeric values of the parameter (neural-network weights)
    public Tensor Value { get; }

    // Always set RequiresGrad = true — a parameter is trainable by definition
    public Parameter(Tensor initialValue)
    {
        Value = initialValue;
        Value.RequiresGrad = true;
    }

    // Gradient of the parameter — filled in during backward()
    public Tensor? Grad => Value.Grad;

    // Zero the gradient before a new iteration
    public void ZeroGrad() => Value.ZeroGrad();
}
