// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

// This record represents one inspectable property in a station input or output tree.
public sealed record PropertyNode(
    string Name,
    string? Value,
    string? TypeHint,
    IReadOnlyList<PropertyNode> Children);
