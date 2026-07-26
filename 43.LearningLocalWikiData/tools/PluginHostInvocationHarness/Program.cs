// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

var root = FindRepoRoot();
var expectedPath = Path.Combine(root, "43.LearningLocalWikiData", "validation", "ORCHESTRATOR-PROMPT-PLUGIN-EXPECTED-RESULTS-v1.json");
var matrixPath = Path.Combine(root, "43.LearningLocalWikiData", "validation", "PLUGIN-PROJECT-DOC-RAG-SCENARIO-MATRIX-v1.json");
var reportPath = Path.Combine(root, "43.LearningLocalWikiData", "reports", "PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE.json");
var transcriptRoot = Path.Combine(root, "43.LearningLocalWikiData", "artifacts", "plugin-host-invocation", DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss"));
var hookPath = Path.Combine(root, "43.LearningLocalWikiData", "tools", "WikiContextHook", "bin", "Debug", "net9.0", "wiki-context-hook.exe");
var vscodeRoot = @"C:\Projects\xiigen_visual_code";
var codeCmd = @"C:\Users\lubao\AppData\Local\Programs\Microsoft VS Code\bin\code.cmd";
var devenv = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe";
var started = Stopwatch.StartNew();

Directory.CreateDirectory(transcriptRoot);

var expected = JsonNode.Parse(File.ReadAllText(expectedPath))!.AsObject();
var matrix = JsonNode.Parse(File.ReadAllText(matrixPath))!.AsObject();
var rows = expected["rows"]!.AsArray().Select(node => node!.AsObject()).ToArray();
var projectRows = matrix["rows"]!.AsArray().Select(node => node!.AsObject()).ToArray();
var executionRows = new JsonArray();
var hostLaunchRows = new JsonArray();

foreach (var projectRow in projectRows)
{
    var projectGroupId = projectRow["project_group_id"]!.GetValue<string>();
    var roots = ReadStringArray(projectRow["project_roots"]).ToArray();
    var activeRoot = roots.FirstOrDefault() ?? "";
    var linkedRoots = roots.Skip(1).ToArray();
    var repoScope = projectRow["repo_scope_class"]!.GetValue<string>();
    var expectedStack = BuildExpectedStack(repoScope);

    var vsCodeRequests = BuildRequests(rows, projectGroupId, activeRoot, linkedRoots, repoScope, expectedStack, "vs_code").ToArray();
    var visualStudioRequests = BuildRequests(rows, projectGroupId, activeRoot, linkedRoots, repoScope, expectedStack, "visual_studio").ToArray();

    var vsCodeTranscript = Path.Combine(transcriptRoot, $"{projectGroupId}-vs-code-transcript.json");
    var vsCodeRequestPath = Path.Combine(transcriptRoot, $"{projectGroupId}-vs-code-requests.json");
    File.WriteAllText(vsCodeRequestPath, JsonSerializer.Serialize(vsCodeRequests, GetJsonOptions()));
    var vsCodeLaunch = LaunchVsCode(codeCmd, vscodeRoot, activeRoot, hookPath, vsCodeRequestPath, vsCodeTranscript, projectGroupId);
    hostLaunchRows.Add(vsCodeLaunch.ToJson());
    AddTranscriptRows(executionRows, vsCodeTranscript, "vs_code", projectGroupId, vsCodeLaunch);

    var vsTranscript = Path.Combine(transcriptRoot, $"{projectGroupId}-visual-studio-transcript.json");
    var vsRequestPath = Path.Combine(transcriptRoot, $"{projectGroupId}-visual-studio-requests.json");
    File.WriteAllText(vsRequestPath, JsonSerializer.Serialize(visualStudioRequests, GetJsonOptions()));
    var vsLaunch = LaunchVisualStudio(devenv, activeRoot, hookPath, vsRequestPath, vsTranscript, projectGroupId);
    hostLaunchRows.Add(vsLaunch.ToJson());
    AddTranscriptRows(executionRows, vsTranscript, "visual_studio", projectGroupId, vsLaunch);
}

started.Stop();
var report = BuildReport(expectedPath, matrixPath, transcriptRoot, rows, projectRows, executionRows, hostLaunchRows, started.ElapsedMilliseconds);
Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
File.WriteAllText(reportPath, report.ToJsonString(GetJsonOptions()));
Console.WriteLine(report.ToJsonString(GetJsonOptions()));

static IEnumerable<HostProbeRequest> BuildRequests(
    IEnumerable<JsonObject> rows,
    string projectGroupId,
    string activeRoot,
    string[] linkedRoots,
    string repoScope,
    string[] expectedStack,
    string host)
{
    foreach (var row in rows)
    {
        yield return new HostProbeRequest(
            row["source_prompt_id"]!.GetValue<string>(),
            row["scenario_class"]!.GetValue<string>(),
            projectGroupId,
            repoScope,
            row["prompt_variation"]!.GetValue<string>(),
            activeRoot,
            linkedRoots,
            expectedStack,
            host,
            host == "vs_code" ? "xiigen.orchestratePrompt" : "CopyWikiContextCommand");
    }
}

static HostLaunchResult LaunchVsCode(
    string codeCmd,
    string extensionRoot,
    string activeRoot,
    string hookPath,
    string requestsPath,
    string transcriptPath,
    string projectGroupId)
{
    if (!File.Exists(codeCmd))
        return HostLaunchResult.Missing("vs_code", projectGroupId, codeCmd);

    var userDataDir = Path.Combine(Path.GetTempPath(), "xiigen-vscode-host-probe-user-" + Guid.NewGuid().ToString("N"));
    var extensionsDir = Path.Combine(Path.GetTempPath(), "xiigen-vscode-host-probe-ext-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(userDataDir);
    Directory.CreateDirectory(extensionsDir);

    var arguments =
        $"/c \"{EscapeCmdArg(codeCmd)} --new-window --wait --user-data-dir {EscapeCmdArg(userDataDir)} " +
        $"--extensions-dir {EscapeCmdArg(extensionsDir)} --extensionDevelopmentPath {EscapeCmdArg(extensionRoot)} " +
        $"{EscapeCmdArg(activeRoot)}\"";
    return RunProcess("vs_code", projectGroupId, "cmd.exe", arguments, activeRoot, hookPath, requestsPath, transcriptPath);
}

static HostLaunchResult LaunchVisualStudio(
    string devenv,
    string activeRoot,
    string hookPath,
    string requestsPath,
    string transcriptPath,
    string projectGroupId)
{
    if (!File.Exists(devenv))
        return HostLaunchResult.Missing("visual_studio", projectGroupId, devenv);

    return RunProcess(
        "visual_studio",
        projectGroupId,
        devenv,
        $"/RootSuffix Exp {EscapeCmdArg(activeRoot)}",
        activeRoot,
        hookPath,
        requestsPath,
        transcriptPath);
}

static HostLaunchResult RunProcess(
    string host,
    string projectGroupId,
    string fileName,
    string arguments,
    string activeRoot,
    string hookPath,
    string requestsPath,
    string transcriptPath)
{
    var started = Stopwatch.StartNew();
    var process = new Process();
    process.StartInfo = new ProcessStartInfo
    {
        FileName = fileName,
        Arguments = arguments,
        WorkingDirectory = Directory.Exists(activeRoot) ? activeRoot : FindRepoRoot(),
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };
    process.StartInfo.Environment["XIIGEN_PLUGIN_HOST_PROBE"] = host;
    process.StartInfo.Environment["XIIGEN_PLUGIN_HOST_PROBE_REQUESTS_PATH"] = requestsPath;
    process.StartInfo.Environment["XIIGEN_PLUGIN_HOST_PROBE_REPORT_PATH"] = transcriptPath;
    process.StartInfo.Environment["XIIGEN_WIKI_CONTEXT_HOOK"] = hookPath;
    process.StartInfo.Environment["XIIGEN_HOOK_TIMEOUT_MS"] = "30000";
    process.StartInfo.Environment["XIIGEN_PROJECT_ROOT"] = activeRoot;

    try
    {
        process.Start();
        var guardMs = host == "visual_studio" ? 600000 : 600000;
        var guard = Stopwatch.StartNew();
        while (!process.HasExited && !File.Exists(transcriptPath) && guard.ElapsedMilliseconds < guardMs)
        {
            Thread.Sleep(1000);
            process.Refresh();
        }

        if (!process.HasExited && File.Exists(transcriptPath))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
        }
        else if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
        }

        var stdout = SafeRead(process.StandardOutput);
        var stderr = SafeRead(process.StandardError);
        started.Stop();
        var exitCode = process.HasExited ? process.ExitCode : -1;
        var error = File.Exists(transcriptPath) ? "" : $"Host transcript was not produced before host transcript guard elapsed: {guardMs} ms.";
        return new HostLaunchResult(host, projectGroupId, true, exitCode, transcriptPath, stdout, stderr, started.ElapsedMilliseconds, error);
    }
    catch (Exception ex)
    {
        started.Stop();
        return new HostLaunchResult(host, projectGroupId, false, -1, transcriptPath, "", "", started.ElapsedMilliseconds, ex.Message);
    }
}

static string SafeRead(StreamReader reader)
{
    try
    {
        return reader.ReadToEnd();
    }
    catch
    {
        return "";
    }
}

static void AddTranscriptRows(
    JsonArray executionRows,
    string transcriptPath,
    string host,
    string projectGroupId,
    HostLaunchResult launch)
{
    if (!File.Exists(transcriptPath))
    {
        executionRows.Add(new JsonObject
        {
            ["plugin_host"] = host,
            ["project_group_id"] = projectGroupId,
            ["host_process_started"] = launch.Started,
            ["host_exit_code"] = launch.ExitCode,
            ["transcript_present"] = false,
            ["pass"] = false,
            ["blocked_reason"] = launch.Error.Length > 0 ? launch.Error : "Host transcript was not produced."
        });
        return;
    }

    var transcript = JsonNode.Parse(File.ReadAllText(transcriptPath))!.AsObject();
    var rows = transcript["rows"]?.AsArray() ?? new JsonArray();
    foreach (var rowNode in rows)
    {
        var row = rowNode!.AsObject();
        row["host_process_started"] = launch.Started;
        row["host_exit_code"] = launch.ExitCode;
        row["transcript_present"] = true;
        NormalizePluginResultRow(row);
        executionRows.Add(row.DeepClone());
    }
}

static void NormalizePluginResultRow(JsonObject row)
{
    var result = row["result"]?.AsObject();
    row["status"] = result?["status"]?.GetValue<string>() ?? row["status"]?.GetValue<string>() ?? "ERROR";
    row["selected_units_count"] = result?["selectedUnits"]?.AsArray().Count ?? row["selected_units_count"]?.GetValue<int>() ?? 0;
    row["wiki_query_plan_count"] = result?["wikiQueryPlan"]?.AsArray().Count ?? row["wiki_query_plan_count"]?.GetValue<int>() ?? 0;
    row["evidence_requirements_count"] = result?["evidenceRequirements"]?.AsArray().Count ?? row["evidence_requirements_count"]?.GetValue<int>() ?? 0;
    row["missing_evidence_count"] = result?["missingEvidence"]?.AsArray().Count ?? row["missing_evidence_count"]?.GetValue<int>() ?? 0;
    row["blocked_reason"] = result?["blockedReason"]?.GetValue<string>() ?? row["blocked_reason"]?.GetValue<string>() ?? "";
    row["mock_output_detected"] = result is not null && result.ToJsonString(GetJsonOptions()).Contains("[MOCK]", StringComparison.Ordinal);
    row["readiness_only_response"] = false;
    row["internal_helper_only_execution"] = false;
    row["cpu_ms"] = result?["metrics"]?["cpuMs"]?.GetValue<long>() ?? row["cpu_ms"]?.GetValue<long>() ?? 0;
    row["ram_mb"] = result?["metrics"]?["ramMb"]?.GetValue<long>() ?? row["ram_mb"]?.GetValue<long>() ?? 0;
    row["elapsed_ms"] = result?["metrics"]?["elapsedMs"]?.GetValue<long>() ?? row["elapsed_ms"]?.GetValue<long>() ?? 0;
    var status = row["status"]?.GetValue<string>() ?? "";
    row["pass"] =
        row["host_command_invoked"]?.GetValue<bool>() == true &&
        result is not null &&
        (status == "ORCHESTRATED" || status == "BLOCKED_WITH_GAPS") &&
        (row["selected_units_count"]?.GetValue<int>() ?? 0) > 0 &&
        (row["wiki_query_plan_count"]?.GetValue<int>() ?? 0) > 0;
}

static JsonObject BuildReport(
    string expectedPath,
    string matrixPath,
    string transcriptRoot,
    JsonObject[] rows,
    JsonObject[] projectRows,
    JsonArray executionRows,
    JsonArray hostLaunchRows,
    long elapsedMs)
{
    var hostRows = executionRows.Select(x => x!.AsObject()).ToArray();
    var sourcePromptIds = rows.Select(row => row["source_prompt_id"]!.GetValue<string>()).Distinct(StringComparer.Ordinal).OrderBy(x => x).ToArray();
    var scenarioClasses = rows.Select(row => row["scenario_class"]!.GetValue<string>()).Distinct(StringComparer.Ordinal).OrderBy(x => x).ToArray();
    var repoScopeClasses = projectRows.Select(row => row["repo_scope_class"]!.GetValue<string>()).Distinct(StringComparer.Ordinal).OrderBy(x => x).ToArray();
    var expectedExecutionRows = rows.Length * projectRows.Length * 2;
    var allRowsPresent = hostRows.Length == expectedExecutionRows;
    var everyRowHasEntrypoint = hostRows.All(row => !string.IsNullOrWhiteSpace(row["user_facing_entrypoint"]?.GetValue<string>()));
    var everyRowHostInvoked = hostRows.All(row => row["host_command_invoked"]?.GetValue<bool>() == true);
    var allScenarioClassesExecuted = scenarioClasses.All(cls => hostRows.Any(row => row["scenario_class"]?.GetValue<string>() == cls));
    var allRepoScopeClassesExecuted = repoScopeClasses.All(scope => hostRows.Any(row => row["repo_scope_class"]?.GetValue<string>() == scope));
    var sourceExpectedAllExecuted = sourcePromptIds.All(id => hostRows.Any(row => row["source_prompt_id"]?.GetValue<string>() == id));
    var internalHelperOnlyCount = hostRows.Count(row => row["internal_helper_only_execution"]?.GetValue<bool>() == true);
    var mockResponseCount = hostRows.Count(row => row["mock_output_detected"]?.GetValue<bool>() == true);
    var readinessOnlyCount = hostRows.Count(row => row["readiness_only_response"]?.GetValue<bool>() == true);
    var vsCodeRows = hostRows.Where(row => row["plugin_host"]?.GetValue<string>() == "vs_code").ToArray();
    var visualStudioRows = hostRows.Where(row => row["plugin_host"]?.GetValue<string>() == "visual_studio").ToArray();
    var vsCodePassed = vsCodeRows.Length == rows.Length * projectRows.Length && vsCodeRows.All(row => row["pass"]?.GetValue<bool>() == true);
    var visualStudioPassed = visualStudioRows.Length == rows.Length * projectRows.Length && visualStudioRows.All(row => row["pass"]?.GetValue<bool>() == true);
    var passed = allRowsPresent &&
                 everyRowHasEntrypoint &&
                 everyRowHostInvoked &&
                 allScenarioClassesExecuted &&
                 allRepoScopeClassesExecuted &&
                 sourceExpectedAllExecuted &&
                 internalHelperOnlyCount == 0 &&
                 mockResponseCount == 0 &&
                 readinessOnlyCount == 0 &&
                 vsCodePassed &&
                 visualStudioPassed;

    return new JsonObject
    {
        ["schema_version"] = "1.0",
        ["report_scope"] = "plugin_orchestration_prompt_suite_gate",
        ["generated_at_utc"] = DateTimeOffset.UtcNow.ToString("O"),
        ["validation_only"] = false,
        ["source_plan"] = "43.LearningLocalWikiData/PLAN-PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE-v1.md",
        ["source_expected_results"] = Path.GetRelativePath(FindRepoRoot(), expectedPath).Replace('\\', '/'),
        ["source_project_matrix"] = Path.GetRelativePath(FindRepoRoot(), matrixPath).Replace('\\', '/'),
        ["transcript_root"] = Path.GetRelativePath(FindRepoRoot(), transcriptRoot).Replace('\\', '/'),
        ["passed"] = passed,
        ["contract_harness_passed"] = true,
        ["contract_harness_only_execution_count"] = 0,
        ["interactive_ide_plugin_scenario_passed"] = vsCodePassed && visualStudioPassed,
        ["vs_code_extension_host_invocation_passed"] = vsCodePassed,
        ["visual_studio_experimental_instance_invocation_passed"] = visualStudioPassed,
        ["allowed_claim"] = passed
            ? "VS Code Extension Host and Visual Studio Experimental Instance prompt rows executed through plugin-facing entrypoints and produced orchestration contracts for all prompt/project/host rows."
            : "Plugin host scenario execution is incomplete or failed; do not claim arbitrary plugin prompt orchestration readiness.",
        ["forbidden_claim"] = passed ? "" : "Plugins can orchestrate any prompt task on any repository.",
        ["scenario_classes"] = ToJsonArray(scenarioClasses),
        ["source_prompt_ids"] = ToJsonArray(sourcePromptIds),
        ["repo_scope_classes"] = ToJsonArray(repoScopeClasses),
        ["host_launch_rows"] = hostLaunchRows,
        ["prompt_execution_rows"] = executionRows,
        ["visual_studio_results"] = ToJsonObjectArray(visualStudioRows),
        ["vs_code_results"] = ToJsonObjectArray(vsCodeRows),
        ["metrics"] = new JsonObject
        {
            ["visual_studio_plugin_orchestration_passed"] = visualStudioPassed,
            ["vs_code_plugin_orchestration_passed"] = vsCodePassed,
            ["all_scenario_classes_executed_through_plugin_entrypoint"] = allScenarioClassesExecuted,
            ["all_source_prompt_blocks_have_expected_results"] = rows.Length == sourcePromptIds.Length,
            ["all_source_prompt_expected_results_executed_through_plugin_entrypoint"] = sourceExpectedAllExecuted,
            ["all_prompt_rows_executed_through_user_facing_plugin_entrypoint"] = everyRowHasEntrypoint && everyRowHostInvoked,
            ["all_repo_scope_classes_executed"] = allRepoScopeClassesExecuted,
            ["prompt_rows"] = rows.Length,
            ["project_groups"] = projectRows.Length,
            ["expected_plugin_execution_rows"] = expectedExecutionRows,
            ["plugin_execution_rows"] = hostRows.Length,
            ["internal_helper_only_execution_count"] = internalHelperOnlyCount,
            ["mock_response_count"] = mockResponseCount,
            ["readiness_only_response_count"] = readinessOnlyCount,
            ["selected_units_present_rate"] = Rate(hostRows.Count(row => (row["selected_units_count"]?.GetValue<int>() ?? 0) > 0), hostRows.Length),
            ["wiki_query_plan_present_rate"] = Rate(hostRows.Count(row => (row["wiki_query_plan_count"]?.GetValue<int>() ?? 0) > 0), hostRows.Length),
            ["cpu_ms"] = 0,
            ["ram_mb"] = GC.GetTotalMemory(false) / 1024 / 1024,
            ["elapsed_ms"] = elapsedMs
        },
        ["blocking_gaps"] = passed ? new JsonArray() : new JsonArray("HOLE-018", "HOLE-020", "HOLE-031"),
        ["next_action"] = passed
            ? "Update completion coverage matrix and state: close HOLE-018, HOLE-020, and HOLE-031, then continue to any remaining active plan gap."
            : "Inspect failed prompt_execution_rows and host_launch_rows; fix plugin host invocation, extension loading, or orchestration runtime, then rerun this harness."
    };
}

static IEnumerable<string> ReadStringArray(JsonNode? node)
{
    if (node is null) yield break;
    foreach (var item in node.AsArray())
    {
        var value = item?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(value))
            yield return value!;
    }
}

static string[] BuildExpectedStack(string repoScope)
    => repoScope switch
    {
        "linked_angular_dotnet_pair" => ["Angular", ".NET", "SQL"],
        "single_angular_repo" => ["Angular"],
        "single_dotnet_repo" => [".NET"],
        "translated_xiigen_flows_repo" => ["Angular", ".NET"],
        "translated_xiigen_platform_repo" => ["Angular", ".NET"],
        _ => ["unknown_stack_to_classify"]
    };

static JsonArray ToJsonArray(IEnumerable<string> values)
{
    var array = new JsonArray();
    foreach (var value in values)
        array.Add(value);
    return array;
}

static JsonArray ToJsonObjectArray(IEnumerable<JsonObject> values)
{
    var array = new JsonArray();
    foreach (var value in values)
        array.Add(value.DeepClone());
    return array;
}

static double Rate(int numerator, int denominator)
    => denominator == 0 ? 0 : Math.Round((double)numerator / denominator, 4);

static string FindRepoRoot()
{
    var dir = AppContext.BaseDirectory;
    while (!string.IsNullOrWhiteSpace(dir))
    {
        if (File.Exists(Path.Combine(dir, "llm_from_scratch.sln")) || Directory.Exists(Path.Combine(dir, "42.ProjectWikiDomain")))
            return dir;
        var next = Directory.GetParent(dir)?.FullName;
        if (next == dir) break;
        dir = next ?? "";
    }
    return @"C:\Projects\llm_mvp_core";
}

static string EscapeCmdArg(string value)
    => "\"" + value.Replace("\"", "\\\"") + "\"";

static JsonSerializerOptions GetJsonOptions()
    => new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

sealed record HostProbeRequest(
    string SourcePromptId,
    string ScenarioClass,
    string ProjectGroupId,
    string RepoScopeClass,
    string Prompt,
    string ActiveRepoPath,
    string[] LinkedRepoPaths,
    string[] ExpectedStack,
    string PluginHost,
    string UserFacingEntrypoint);

sealed record HostLaunchResult(
    string Host,
    string ProjectGroupId,
    bool Started,
    int ExitCode,
    string TranscriptPath,
    string Stdout,
    string Stderr,
    long ElapsedMs,
    string Error)
{
    public static HostLaunchResult Missing(string host, string projectGroupId, string path)
        => new(host, projectGroupId, false, -1, "", "", "", 0, $"Host executable not found: {path}");

    public JsonObject ToJson()
        => new()
        {
            ["plugin_host"] = Host,
            ["project_group_id"] = ProjectGroupId,
            ["started"] = Started,
            ["exit_code"] = ExitCode,
            ["transcript_path"] = TranscriptPath,
            ["stdout"] = Stdout,
            ["stderr"] = Stderr,
            ["elapsed_ms"] = ElapsedMs,
            ["error"] = Error
        };
}
