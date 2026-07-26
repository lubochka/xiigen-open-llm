// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using ProjectWikiDomain;

var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true
};

if (args.Contains("--orchestrate", StringComparer.OrdinalIgnoreCase))
{
    var request = BuildRequest();
    var adapter = new ProjectWikiPluginOrchestrationAdapter();
    var result = await adapter.OrchestrateAsync(request);
    if (!result.IsSuccess)
    {
        Console.WriteLine(JsonSerializer.Serialize(BuildErrorResult(request, result.ErrorMessage), options));
        return 2;
    }

    Console.WriteLine(JsonSerializer.Serialize(result.Value, options));
    return 0;
}

if (args.Contains("--ensure-ready", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        isReady = true,
        status = "PLUGIN_ORCHESTRATION_HOOK_AVAILABLE",
        documentationVersion = "plugin-runtime",
        entryCount = 1,
        nextAction = "Use --orchestrate --json to request XIIGen plugin orchestration."
    }, options));
    return 0;
}

Console.WriteLine(JsonSerializer.Serialize(new
{
    status = "BLOCKED_WITH_GAPS",
    blockedReason = "Unsupported wiki-context-hook invocation. Use --orchestrate --json.",
    metrics = new { cpuMs = 0, ramMb = GC.GetTotalMemory(false) / 1024 / 1024, elapsedMs = 0 }
}, options));
return 2;

ProjectWikiPluginOrchestrationRequest BuildRequest()
{
    var fromJson = Environment.GetEnvironmentVariable("XIIGEN_ORCHESTRATION_REQUEST");
    if (!string.IsNullOrWhiteSpace(fromJson))
    {
        var parsed = JsonSerializer.Deserialize<ProjectWikiPluginOrchestrationRequest>(fromJson, options);
        if (parsed is not null)
            return parsed;
    }

    var prompt = Environment.GetEnvironmentVariable("HOOK_PROMPT") ?? "";
    var activeRoot = Environment.GetEnvironmentVariable("XIIGEN_PROJECT_ROOT") ?? Environment.CurrentDirectory;
    var linkedRaw = Environment.GetEnvironmentVariable("XIIGEN_LINKED_REPO_PATHS") ?? "[]";
    var linked = JsonSerializer.Deserialize<string[]>(linkedRaw, options) ?? [];
    return new ProjectWikiPluginOrchestrationRequest(
        prompt,
        activeRoot,
        linked,
        linked.Length > 0 ? "linked_repository_group" : "active_repository",
        [],
        $"hook-{Guid.NewGuid():N}",
        Environment.UserName,
        false,
        false,
        "wiki-context-hook",
        Environment.GetEnvironmentVariable("XIIGEN_PLUGIN_ENTRYPOINT") ?? "wiki-context-hook");
}

object BuildErrorResult(ProjectWikiPluginOrchestrationRequest request, string? reason)
    => new
    {
        status = "ERROR",
        orchestrationPlan = Array.Empty<string>(),
        selectedUnits = Array.Empty<string>(),
        wikiQueryPlan = Array.Empty<string>(),
        evidenceRequirements = Array.Empty<string>(),
        missingEvidence = new[] { reason ?? "Unknown plugin orchestration error." },
        blockedReason = reason ?? "Unknown plugin orchestration error.",
        answerPreviewOrPlanSummary = "XIIGen plugin orchestration failed before planning.",
        stateUpdate = "plugin_orchestration_error",
        repoScopeClass = request.RepoScopeClass,
        activeRepoPath = request.ActiveRepoPath,
        linkedRepoPaths = request.LinkedRepoPaths,
        userFacingEntrypoint = request.UserFacingEntrypoint,
        metrics = new { cpuMs = 0, ramMb = GC.GetTotalMemory(false) / 1024 / 1024, elapsedMs = 0 }
    };
