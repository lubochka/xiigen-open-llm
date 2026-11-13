// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace TensorCore.Layers;

// Обучаемый параметр нейросети — тензор для которого всегда вычисляется градиент.
// Аналог torch.nn.Parameter.
public sealed class Parameter
{
    // Числовые значения параметра (веса нейронной сети)
    public Tensor Value { get; }

    // Всегда устанавливаем RequiresGrad = true — параметр по определению обучаемый
    public Parameter(Tensor initialValue)
    {
        Value = initialValue;
        Value.RequiresGrad = true;
    }

    // Градиент параметра — заполняется при backward()
    public Tensor? Grad => Value.Grad;

    // Обнулить градиент перед новой итерацией
    public void ZeroGrad() => Value.ZeroGrad();
}
