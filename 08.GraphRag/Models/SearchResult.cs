// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace GraphRag.Models;

public record SearchResult
{
    public string NodeId    { get; init; } = "";
    public string Name      { get; init; } = "";
    public string Type      { get; init; } = "";
    public string RawText   { get; init; } = "";
    public float  VectorScore    { get; init; }
    public double GraphScore     { get; init; }
    public double CombinedScore  { get; init; }
    public string Source    { get; init; } = ""; // "local" | "global" | "hybrid"
}
