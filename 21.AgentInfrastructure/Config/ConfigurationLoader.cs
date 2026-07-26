// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

// Stage 01 — Initialization and configuration.
// Reads up to three configuration sources and deep-merges them in priority order:
//   1. Global user config  (~/.config/agent/agent.json)          — lowest priority
//   2. Project config      (<projectRoot>/.agent/agent.json)     — overrides global
//   3. Instruction file    (AGENTS.md or CLAUDE.md in project root) — stored as "systemInstructions"
public sealed class ConfigurationLoader
{
    private readonly IConfigurationProvider _provider;

    public ConfigurationLoader(IConfigurationProvider provider)
        => _provider = provider;

    public async Task<OperationResult<Dictionary<string, object>>> LoadAsync(string projectRootPath)
    {
        var merged = new Dictionary<string, object>(StringComparer.Ordinal);

        // Layer 1 — global user configuration (lowest priority).
        var globalPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config", "agent", "agent.json");

        var globalResult = await _provider.ReadJsonAsync(globalPath);
        if (globalResult.IsSuccess)
            DeepMerge(merged, globalResult.Value!);

        // Layer 2 — project-level configuration (overrides global).
        var projectPath = Path.Combine(projectRootPath, ".agent", "agent.json");
        var projectResult = await _provider.ReadJsonAsync(projectPath);
        if (projectResult.IsSuccess)
            DeepMerge(merged, projectResult.Value!);

        // Layer 3 — instruction file (AGENTS.md preferred; CLAUDE.md as fallback).
        var agentsMdPath = Path.Combine(projectRootPath, "AGENTS.md");
        var claudeMdPath = Path.Combine(projectRootPath, "CLAUDE.md");

        var instrResult = await _provider.ReadTextAsync(agentsMdPath);
        if (!instrResult.IsSuccess)
            instrResult = await _provider.ReadTextAsync(claudeMdPath);

        if (instrResult.IsSuccess)
            merged["systemInstructions"] = instrResult.Value!;

        return OperationResult<Dictionary<string, object>>.Ok(merged);
    }

    // Deep merge: nested Dictionary values are merged recursively; all other types are overwritten.
    private static void DeepMerge(Dictionary<string, object> target, Dictionary<string, object> source)
    {
        foreach (var (key, srcVal) in source)
        {
            if (target.TryGetValue(key, out var tgtVal)
                && tgtVal is Dictionary<string, object> tgtNested
                && srcVal  is Dictionary<string, object> srcNested)
            {
                DeepMerge(tgtNested, srcNested);
            }
            else
            {
                target[key] = srcVal;
            }
        }
    }
}
