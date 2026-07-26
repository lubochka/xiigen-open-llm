// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DPO.Models;

public record DpoMetrics
{
    public int    Step          { get; init; }
    public float  Loss          { get; init; }
    public float  ChosenReward  { get; init; }
    public float  RejectedReward { get; init; }
    public float  RewardMargin  { get; init; }
    public float  Accuracy      { get; init; }  // fraction where chosen > rejected
}
