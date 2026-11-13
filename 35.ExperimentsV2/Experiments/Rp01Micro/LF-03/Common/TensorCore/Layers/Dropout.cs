// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Randomness;

namespace TensorCore.Layers;

// Слой случайного обнуления нейронов (регуляризация).
// Во время обучения каждый нейрон обнуляется с вероятностью p.
// Во время инференса dropout отключён (все значения проходят).
public class Dropout : Module
{
    private readonly float _p;
    private readonly Random? _rng;
    private readonly SplitMix64Random? _deterministicRng;

    // p — вероятность обнуления; seed фиксирует воспроизводимость
    public Dropout(float p = 0.1f, int? seed = null)
    {
        _p   = p;
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public Dropout(float p, ulong deterministicSeed)
    {
        _p = p;
        _deterministicRng = new SplitMix64Random(deterministicSeed);
    }

    public SplitMix64RandomState CaptureDeterministicState()
    {
        if (_p == 0f)
            return new SplitMix64RandomState(0, 0);
        if (_deterministicRng is null)
            throw new InvalidOperationException(
                "DPO_RANDOM_STATE_NONDETERMINISTIC_ADAPTER");
        return _deterministicRng.CaptureState();
    }

    public void RestoreDeterministicState(SplitMix64RandomState state)
    {
        if (_p == 0f)
            return;
        if (_deterministicRng is null)
            throw new InvalidOperationException(
                "DPO_RANDOM_STATE_NONDETERMINISTIC_ADAPTER");
        _deterministicRng.RestoreState(state);
    }

    // training=true — применяем dropout; training=false — пропускаем как есть
    public Tensor Forward(Tensor input, bool training = true)
    {
        if (!training || _p == 0f)
            return input;

        // Создаём маску: 1/(1-p) для выживших нейронов, 0 для обнулённых
        // Масштабирование на 1/(1-p) сохраняет ожидаемое значение активаций
        float scale = 1.0f / (1.0f - _p);
        var maskData = new float[input.Shape.TotalElements];
        for (int i = 0; i < maskData.Length; i++)
            maskData[i] = NextUnitDouble() >= _p ? scale : 0f;

        var mask   = new Tensor(maskData, input.Shape);
        return TensorOps.Multiply(input, mask);
    }

    private double NextUnitDouble()
        => _rng is not null
            ? _rng.NextDouble()
            : _deterministicRng!.NextUnitDouble();
}
