// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace GraphEngine.Models;

public record GraphEdge
{
    public string From         { get; init; } = "";
    public string To           { get; init; } = "";
    public string RelationType { get; init; } = ""; // Calls | Defines | Inherits | Implements
    public double Weight       { get; init; } = 1.0;
}
