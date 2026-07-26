// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TrainingLoop.Schedulers;

namespace LlmFromScratch.Tests;

public class SchedulerTests
{
    [Fact]
    public void Warmup_StartsNearZeroAndRisesToMaxLr()
    {
        var sched = new CosineWithWarmup(maxLr: 1e-3f, minLr: 1e-4f, warmupSteps: 100, totalSteps: 1000);

        float step0 = sched.GetLr(0);
        float step50 = sched.GetLr(50);
        float step100 = sched.GetLr(100);

        Assert.True(step0 < step50);
        Assert.True(step50 < step100);
        Assert.Equal(1e-3f, step100, precision: 6);
    }

    [Fact]
    public void Decay_DropsBelowMaxLrAfterWarmup()
    {
        var sched = new CosineWithWarmup(maxLr: 1e-3f, minLr: 1e-4f, warmupSteps: 10, totalSteps: 100);

        float peak = sched.GetLr(10);
        float mid  = sched.GetLr(55);
        float end  = sched.GetLr(100);

        Assert.True(mid < peak);
        Assert.True(end <= mid);
        Assert.True(end >= 1e-4f - 1e-8f);
    }
}
