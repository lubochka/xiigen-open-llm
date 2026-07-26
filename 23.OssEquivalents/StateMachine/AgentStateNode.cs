// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OssEquivalents;

// One node in the agent state machine.
// A node is an async function that receives the current state dictionary
// and returns an updated state dictionary.
// This mirrors LangGraph's "node" concept from the OSS plan.
public sealed record AgentStateNode(
    string                                                        Name,
    Func<Dictionary<string, object>, Task<Dictionary<string, object>>> Execute);
