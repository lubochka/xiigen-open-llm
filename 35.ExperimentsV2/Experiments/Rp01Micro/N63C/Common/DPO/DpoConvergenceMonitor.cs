// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DPO;

// Detects DPO convergence from window-level loss changes.
public sealed class DpoConvergenceMonitor
{
    private readonly float _threshold;
    private readonly int _windowSteps;
    private float _previousLoss = float.MaxValue;
    private int _stableWindowCount;

    public DpoConvergenceMonitor(float threshold = 0.002f, int windowSteps = 25)
    {
        _threshold = MathF.Max(0f, threshold);
        _windowSteps = Math.Max(1, windowSteps);
    }

    // Return true after two consecutive windows move less than the threshold.
    public bool ShouldStop(float currentLoss)
    {
        StepsTaken += _windowSteps;
        if (_previousLoss == float.MaxValue)
        {
            _previousLoss = currentLoss;
            return false;
        }

        if (MathF.Abs(_previousLoss - currentLoss) < _threshold)
            _stableWindowCount++;
        else
            _stableWindowCount = 0;

        _previousLoss = currentLoss;
        return _stableWindowCount >= 2;
    }

    public int StepsTaken { get; private set; }
}
