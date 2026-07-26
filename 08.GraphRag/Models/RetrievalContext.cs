// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace GraphRag.Models;

public record RetrievalContext
{
    public string Query          { get; init; } = "";
    public List<SearchResult> Results { get; init; } = [];
    public string ContextText    { get; init; } = "";
    public int    TokenCount     { get; init; }
}
