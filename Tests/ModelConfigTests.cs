// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public class ModelConfigTests
{
    [Fact]
    public void SmallConfig_ComputedPropertiesAreConsistent()
    {
        var cfg = ModelConfig.Small;

        Assert.Equal(cfg.HiddenDim / cfg.NumHeads, cfg.HeadDim);
        Assert.Equal(cfg.NumHeads / cfg.NumKvHeads, cfg.KvGroupSize);
    }

    [Fact]
    public void SmallConfig_HeadDimIsPositive()
    {
        Assert.True(ModelConfig.Small.HeadDim > 0);
    }
}
