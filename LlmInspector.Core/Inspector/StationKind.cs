// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

// This enum identifies the runtime role of one station in the inspector trace.
public enum StationKind
{
    // Orchestrator stations select flows and coordinate nested execution.
    Orchestrator,

    // Flow stations execute one logical topology over one request shape.
    Flow,

    // Unit stations perform one logical transformation.
    Unit,

    // Storage stations represent today's concrete storage boundary before registry promotion.
    Storage
}
