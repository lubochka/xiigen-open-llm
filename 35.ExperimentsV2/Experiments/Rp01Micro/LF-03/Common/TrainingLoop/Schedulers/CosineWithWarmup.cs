// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainingLoop.Schedulers;

/// <summary>
/// Learning-rate schedule: linear warmup then cosine decay.
/// Warmup prevents early large gradients from destroying randomly-initialised weights.
/// All rates come from config — no inline constants (DNA rule 1).
/// </summary>
public class CosineWithWarmup
{
    private readonly int _warmupSteps;
    private readonly int _totalSteps;
    private readonly float _maxLr;
    private readonly float _minLr;

    public CosineWithWarmup(int warmupSteps, int totalSteps, float maxLr, float minLr)
    {
        _warmupSteps = warmupSteps;
        _totalSteps  = totalSteps;
        _maxLr       = maxLr;
        _minLr       = minLr;
    }

    public float GetLr(int step)
    {
        if (step < _warmupSteps)
            return _maxLr * step / Math.Max(1, _warmupSteps);

        float progress = (float)(step - _warmupSteps) / Math.Max(1, _totalSteps - _warmupSteps);
        float cosine   = 0.5f * (1f + MathF.Cos(MathF.PI * progress));
        return _minLr + cosine * (_maxLr - _minLr);
    }
}
