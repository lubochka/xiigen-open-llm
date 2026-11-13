// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TransformerModel.Config;

/// <summary>
/// All architecture hyperparameters come from this record — no inline constants in model layers.
/// DNA rule 1: config values must come from config objects, not scattered magic numbers.
/// </summary>
public record ModelConfig
{
    public int VocabSize    { get; init; } = 32_000;
    public int ContextLen   { get; init; } = 2048;
    public int HiddenDim    { get; init; } = 2048;
    public int NumLayers    { get; init; } = 16;
    public int NumHeads     { get; init; } = 16;
    public int NumKvHeads   { get; init; } = 4;    // GQA: fewer KV heads than Q heads
    public int FfnHiddenDim { get; init; } = 5632; // ≈ 2.75 × hidden (SwiGLU convention)
    public float RopeTheta  { get; init; } = 10_000f;
    public float RmsNormEps { get; init; } = 1e-5f;

    public int HeadDim     => HiddenDim / NumHeads;
    public int KvGroupSize => NumHeads / NumKvHeads;

    public static ModelConfig Small => new()
    {
        VocabSize = 32_000, ContextLen = 512, HiddenDim = 512,
        NumLayers = 4, NumHeads = 8, NumKvHeads = 2, FfnHiddenDim = 1408
    };
}
