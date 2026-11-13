// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DPO;

// Detects DPO (Direct Preference Optimization) convergence by comparing the
// loss of the current observation with the loss of the previous observation.
// `windowSteps` is the observation interval - how many optimizer steps the
// caller runs between two ShouldStop calls - and NOT a rolling-average window:
// this type keeps no loss buffer and averages nothing.
public sealed class DpoConvergenceMonitor
{
    // Largest absolute loss change that still counts as one stable comparison.
    private readonly float _threshold;

    // Шагов между наблюдениями, а не окно скользящего среднего: это число
    // только прибавляется к StepsTaken и ни одной потери не усредняет.
    private readonly int _windowSteps;

    // Loss seen at the previous call; MaxValue means "no previous call yet".
    private float _previousLoss = float.MaxValue;

    // How many consecutive comparisons stayed under the threshold so far.
    private int _stableWindowCount;

    // Store the stable-change threshold and the observation interval.
    public DpoConvergenceMonitor(float threshold = 0.002f, int windowSteps = 25)
    {
        // A negative threshold is clamped to zero: then only an exact repeat is stable.
        _threshold = MathF.Max(0f, threshold);
        // A zero or negative interval is clamped to one step per observation.
        _windowSteps = Math.Max(1, windowSteps);
    }

    // Return true once two consecutive comparisons stayed under the threshold.
    // The first call only records the starting loss, so the earliest possible
    // true is the THIRD call. A caller that asks this monitor once every
    // `windowSteps` optimizer steps therefore needs a step budget of at least
    // three observation intervals before convergence can ever be detected, and
    // at exactly three intervals the true arrives on the final step, so it
    // detects convergence without saving a single step.
    public bool ShouldStop(float currentLoss)
    {
        // Accounting only: add the steps that this one observation stands for.
        StepsTaken += _windowSteps;
        // The first observation has nothing to compare against yet.
        if (_previousLoss == float.MaxValue)
        {
            // Remember the starting loss and report "do not stop".
            _previousLoss = currentLoss;
            return false;
        }

        // One comparison against the previous observation, not against an average.
        if (MathF.Abs(_previousLoss - currentLoss) < _threshold)
            // This observation moved less than the threshold: the stable run grows.
            _stableWindowCount++;
        else
            // Any change at or above the threshold restarts the stable run.
            _stableWindowCount = 0;

        // The current loss becomes the baseline for the next observation.
        _previousLoss = currentLoss;
        // Two consecutive stable comparisons are required before stopping.
        return _stableWindowCount >= 2;
    }

    // Steps the caller reported through the observation interval; accounting
    // only - it is the number of ShouldStop calls times `windowSteps`.
    public int StepsTaken { get; private set; }
}
