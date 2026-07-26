// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// Describes a sub-agent: its role, the tools it may use, and how to route tasks to it.
public sealed record SubAgentDefinition(
    string Name,
    string Role,
    IReadOnlyList<string> AllowedTools);
