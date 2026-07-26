// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace GraphEngine.Models;

public record GraphNode
{
    public string Id            { get; init; } = "";
    public string Name          { get; init; } = "";
    public string Type          { get; init; } = ""; // Class | Method | File | Interface
    public string FilePath      { get; init; } = "";
    public string Signature     { get; init; } = "";
    public string RawText       { get; init; } = "";
    public float[] Embedding    { get; init; } = [];
    public double PageRankScore { get; set; }
}
