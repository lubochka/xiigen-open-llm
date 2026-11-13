// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LoRA;

public record LoraConfig
{
    public int    Rank        { get; init; } = 8;
    public float  Alpha       { get; init; } = 16f;   // scaling = alpha / rank
    public float  Dropout     { get; init; } = 0.05f;

    // Which module name substrings to apply LoRA to
    public string[] TargetModules { get; init; } = ["q_proj", "v_proj"];

    public float Scaling => Alpha / Rank;
}
