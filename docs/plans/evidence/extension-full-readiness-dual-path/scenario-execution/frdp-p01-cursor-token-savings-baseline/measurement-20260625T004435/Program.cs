using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using SmartSkillsDomain;

var coreRoot = @"C:\Projects\llm_mvp_core";
var measurementRoot = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Directory.GetCurrentDirectory();
Directory.CreateDirectory(measurementRoot);
Directory.SetCurrentDirectory(coreRoot);

var scenarioRoot = Directory.GetParent(measurementRoot)?.FullName
    ?? throw new InvalidOperationException("Could not resolve scenario root.");
var specJsonPath = Path.Combine(scenarioRoot, "row12-workload-spec.json");
var specMdPath = Path.Combine(scenarioRoot, "row12-workload-spec.md");

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

var compactJsonOptions = new JsonSerializerOptions
{
    WriteIndented = false,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

using var specDoc = JsonDocument.Parse(File.ReadAllText(specJsonPath, Encoding.UTF8));
var workload = specDoc.RootElement.GetProperty("workload");
var promptLines = workload.GetProperty("prompt_lines")
    .EnumerateArray()
    .Select(line => line.GetString() ?? string.Empty)
    .ToArray();
var promptText = string.Join('\n', promptLines);

var scenarioId = "frdp-p01-cursor-token-savings-baseline";
var pairId = "frdp-p01-row12-cursor-token-baseline-v1";
var marker = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1";
var expectedPromptHash = "sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374";
var promptHash = HashText(promptText);
var hostName = "Cursor";
var hostType = "cursor-no-send-local-extension-runtime-candidate";
var hostPathLabel = "Cursor no-send local extension runtime";
var method = "prompt-to-task";
var debugEnabled = true;
var isolationMode = $"measurement_{Path.GetFileName(measurementRoot)}_core_api_no_send";
var allowedSkills = new[]
{
    "prompt-to-task",
    "task-to-plan",
    "task-to-skills",
    "evidence-ledger"
};

var startedUtc = DateTimeOffset.UtcNow;
var sourceHash = HashFile(Path.Combine(coreRoot, "43.SmartSkillsDomain", "TokenTraceAccounting.cs"));
var testSourceHash = HashFile(Path.Combine(coreRoot, "Tests", "ExtensionTokenTraceAccountingTests.cs"));
var assemblyPath = Path.Combine(coreRoot, "43.SmartSkillsDomain", "bin", "Debug", "net9.0", "43.SmartSkillsDomain.dll");
var assemblyHash = File.Exists(assemblyPath) ? HashFile(assemblyPath) : null;
var gitStatus = RunGitStatus(coreRoot);

var service = new XiigenExtensionApiService(coreRoot);
var baseline = InvokeMode("baseline", service);
var adaptive = InvokeMode("adaptive", service);

var baselineTrace = BuildTrace("baseline", baseline.Response, baseline.Paths);
var adaptiveTrace = BuildTrace("adaptive", adaptive.Response, adaptive.Paths);
var pairValidation = ValidatePair(baselineTrace, adaptiveTrace);
var qualityGuard = BuildQualityGuard(baseline.Response, adaptive.Response, pairValidation);
var hostVisibleBlocker = WriteHostVisibleBlocker();

WriteJson(
    "token-traces.json",
    new
    {
        schema_version = "frdp.cursor_token_traces_packet.v1",
        scenario_id = scenarioId,
        pair_id = pairId,
        prompt_hash = promptHash,
        traces = new[] { baselineTrace, adaptiveTrace }
    });

File.WriteAllText(
    Path.Combine(measurementRoot, "token-traces.jsonl"),
    string.Join(
        '\n',
        JsonSerializer.Serialize(baselineTrace, compactJsonOptions),
        JsonSerializer.Serialize(adaptiveTrace, compactJsonOptions)) + "\n",
    new UTF8Encoding(false));

WriteJson("pair-validation.json", pairValidation);
WriteJson("quality-guard.json", qualityGuard);

var finalStatus = promptHash == expectedPromptHash
    && pairValidation.pair_validation_errors_count == 0
    && qualityGuard.quality_assertions_total == 14
    && qualityGuard.quality_assertions_passed == 14
    && qualityGuard.critical_failures_count == 0
    && qualityGuard.external_ai_send == false
    ? "MEASUREMENT_PACKET_READY"
    : "MEASUREMENT_NEEDS_FIX";

var createdFiles = Directory.GetFiles(measurementRoot, "*", SearchOption.AllDirectories)
    .Select(Rel)
    .Order(StringComparer.Ordinal)
    .ToArray();

var report = new
{
    schema_version = "frdp.row12.measurement.execution_report.v1",
    status = finalStatus,
    generated_at_utc = DateTimeOffset.UtcNow,
    active_luba_authority_quote = "Действия через суб агентов",
    current_task = "Create the row 12 measurement packet for frdp-p01-cursor-token-savings-baseline using accepted row12-workload-spec.md/json.",
    not_the_task = new[]
    {
        "token savings claim",
        "product readiness claim",
        "full Cursor readiness claim",
        "semantic quality claim",
        "checkpoint mutation",
        "training",
        "board edit",
        "git stage/commit/push"
    },
    source_specs = new[] { Rel(specMdPath), Rel(specJsonPath) },
    prompt_hash_expected = expectedPromptHash,
    prompt_hash_actual = promptHash,
    prompt_hash_matches = promptHash == expectedPromptHash,
    commands_run = new[]
    {
        new
        {
            command = "dotnet run --project MeasurementRunner.csproj -- <measurement-root>",
            pass = finalStatus == "MEASUREMENT_PACKET_READY",
            baseline_adaptive_invocations_pass = baseline.CommandExitCode == 0 && adaptive.CommandExitCode == 0,
            expected_runner_exit_code = finalStatus == "MEASUREMENT_PACKET_READY" ? 0 : 2,
            note = "One local in-process runner invoked baseline and adaptive XiigenExtensionApiService calls. Baseline/adaptive calls passed, but the overall runner is pass=false unless pair validation, quality guard, and host-visible evidence satisfy the accepted row12 gates."
        }
    },
    mode_results = new
    {
        baseline = new
        {
            command_exit_code = baseline.CommandExitCode,
            response_success = baseline.Response.Success,
            response_status = baseline.Response.Status,
            request = Rel(baseline.Paths.RequestPath),
            response = Rel(baseline.Paths.ResponsePath),
            trace = Rel(baseline.Paths.CoreTracePath),
            command_result = Rel(baseline.Paths.CommandResultPath),
            command_output = Rel(baseline.Paths.CommandOutputPath),
            command_error = Rel(baseline.Paths.CommandErrorPath)
        },
        adaptive = new
        {
            command_exit_code = adaptive.CommandExitCode,
            response_success = adaptive.Response.Success,
            response_status = adaptive.Response.Status,
            request = Rel(adaptive.Paths.RequestPath),
            response = Rel(adaptive.Paths.ResponsePath),
            trace = Rel(adaptive.Paths.CoreTracePath),
            command_result = Rel(adaptive.Paths.CommandResultPath),
            command_output = Rel(adaptive.Paths.CommandOutputPath),
            command_error = Rel(adaptive.Paths.CommandErrorPath)
        }
    },
    token_numbers_observation_only = new
    {
        baseline_input_tokens = baselineTrace.token_count_input,
        baseline_output_tokens = baselineTrace.token_count_output,
        baseline_total_tokens = baselineTrace.token_count_total,
        adaptive_input_tokens = adaptiveTrace.token_count_input,
        adaptive_output_tokens = adaptiveTrace.token_count_output,
        adaptive_total_tokens = adaptiveTrace.token_count_total,
        observed_candidate_delta = adaptiveTrace.token_count_total - baselineTrace.token_count_total,
        token_reduction_observed = adaptiveTrace.token_count_total < baselineTrace.token_count_total,
        token_savings_claimed = false,
        token_savings_claim_allowed = false
    },
    pair_validation = new
    {
        pair_valid = pairValidation.pair_valid,
        pair_validation_errors_count = pairValidation.pair_validation_errors_count,
        errors = pairValidation.pair_validation_errors
    },
    quality_guard = new
    {
        quality_assertions_total = qualityGuard.quality_assertions_total,
        quality_assertions_passed = qualityGuard.quality_assertions_passed,
        critical_failures_count = qualityGuard.critical_failures_count,
        pass = qualityGuard.pass
    },
    host_visible_candidate = new
    {
        status = "exact_blocker_recorded",
        blocker_ref = Rel(hostVisibleBlocker.JsonPath)
    },
    what_is_proven = new[]
    {
        "The accepted row12 workload prompt hash was reproduced exactly.",
        "Baseline and adaptive no-send core API requests were run through SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest.",
        "Both traces use the same scenario, pair id, host, method, debug flag, prompt hash, tokenizer name, tokenizer version, tokenizer accounting type, isolation mode, redaction flags, and no external AI send.",
        "Pair validation has zero same-field errors."
    },
    what_is_not_proven = new[]
    {
        "Token savings are not proven or claimed.",
        "Semantic quality is not proven or claimed.",
        "Product readiness and full Cursor readiness are not proven or claimed.",
        "Actual Cursor host-visible row12 candidate behavior is not proven; an exact blocker is recorded.",
        "The 14-item quality guard does not pass because the actual compact core response does not preserve the marker/scenario/required structured sections."
    },
    counter_recommendation_total_28 = new
    {
        recommendation = "Do not increment passed scenario count for row 12. Keep row 12 in needs-fix/blocked-for-host-and-quality-guard status until a row12-specific host-visible no-send path emits the required structured response and all 14 quality assertions pass.",
        total_scenarios = 28,
        row12_packet_status = finalStatus,
        readiness_claims_opened = 0
    },
    environment = new
    {
        repo_dirty_status_recorded = true,
        git_status_short = gitStatus,
        token_trace_accounting_source_sha256 = sourceHash,
        token_trace_accounting_test_sha256 = testSourceHash,
        smart_skills_domain_assembly_path = Rel(assemblyPath),
        smart_skills_domain_assembly_sha256 = assemblyHash,
        external_ai_send = false,
        training_started = false,
        checkpoint_mutated = false,
        locator_updated = false
    },
    created_files = createdFiles
};

WriteJson("execution-report.json", report);
WriteReportMarkdown(report, pairValidation, qualityGuard, hostVisibleBlocker.JsonPath);

Console.WriteLine(finalStatus);
Console.WriteLine(Path.Combine(measurementRoot, "execution-report.json"));
return finalStatus == "MEASUREMENT_PACKET_READY" ? 0 : 2;

RunResult InvokeMode(string mode, XiigenExtensionApiService api)
{
    var paths = new ModePaths(
        RequestPath: Path.Combine(measurementRoot, $"{mode}-request.json"),
        ResponsePath: Path.Combine(measurementRoot, $"{mode}-response.json"),
        CoreTracePath: Path.Combine(measurementRoot, $"{mode}-core-trace.json"),
        CommandResultPath: Path.Combine(measurementRoot, $"{mode}-command-result.json"),
        CommandOutputPath: Path.Combine(measurementRoot, $"{mode}-command-output.txt"),
        CommandErrorPath: Path.Combine(measurementRoot, $"{mode}-command-error.txt"));

    var request = new XiigenExtensionRequest(
        HostName: hostName,
        HostType: hostType,
        Method: method,
        UserPromptOrTaskText: promptText,
        HostFacts: new Dictionary<string, string>
        {
            ["workspace"] = $"workspace:{coreRoot}",
            ["scenario_id"] = scenarioId,
            ["marker"] = marker,
            ["pair_id"] = pairId,
            ["host_path_label"] = hostPathLabel,
            ["token_mode"] = mode,
            ["core_api_path"] = method,
            ["proof_scope"] = "no-send core API measurement only; no token savings/readiness/semantic quality claim"
        },
        DebugEnabled: debugEnabled,
        RequestedSkills: allowedSkills,
        AvailableSkills: allowedSkills,
        CheckSetInputs: new Dictionary<string, string>
        {
            ["scenario_id"] = scenarioId,
            ["marker"] = marker,
            ["pair_id"] = pairId,
            ["token_mode"] = mode,
            ["external_ai_send"] = "false",
            ["token_savings_claimed"] = "false",
            ["semantic_quality_claimed"] = "false",
            ["readiness_claimed"] = "false",
            ["training_started"] = "false",
            ["checkpoint_mutation"] = "false",
            ["locator_update"] = "false",
            ["constraint.no_external_ai"] = "true",
            ["constraint.no_checkpoint_mutation"] = "true"
        },
        Provenance: new Dictionary<string, string>
        {
            ["source"] = "row12-workload-spec",
            ["row12_workload_spec_md"] = Rel(specMdPath),
            ["row12_workload_spec_json"] = Rel(specJsonPath),
            ["active_luba_authority_quote"] = "Действия через суб агентов"
        },
        CorrelationId: pairId);

    WriteJson(paths.RequestPath, request);

    var modeStarted = DateTimeOffset.UtcNow;
    XiigenExtensionResponse response;
    var commandExitCode = 0;
    string commandError = string.Empty;
    try
    {
        var result = api.ProcessHostExtensionRequest(request);
        if (!result.IsSuccess || result.Value is null)
        {
            commandExitCode = 1;
            commandError = $"{result.ErrorCode}:{result.ErrorMessage}";
            response = ErrorResponse(commandError);
        }
        else
        {
            response = result.Value;
            commandExitCode = response.Success ? 0 : 1;
        }
    }
    catch (Exception ex)
    {
        commandExitCode = 1;
        commandError = ex.ToString();
        response = ErrorResponse(ex.Message);
    }

    var modeCompleted = DateTimeOffset.UtcNow;
    WriteJson(paths.ResponsePath, response);

    var commandOutput = commandExitCode == 0
        ? $"mode={mode}; in_process_core_api_invocation=passed; response_success={response.Success}; response_status={response.Status}"
        : $"mode={mode}; in_process_core_api_invocation=failed; response_success={response.Success}; response_status={response.Status}";
    File.WriteAllText(paths.CommandOutputPath, commandOutput + "\n", new UTF8Encoding(false));
    File.WriteAllText(paths.CommandErrorPath, commandError + (commandError.EndsWith('\n') || commandError.Length == 0 ? string.Empty : "\n"), new UTF8Encoding(false));

    WriteJson(paths.CoreTracePath, new
    {
        schema_version = "frdp.row12.core_trace.v1",
        endpoint = "inprocess-xiigen-extension-core-api",
        call_target = "SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest",
        request_path = Rel(paths.RequestPath),
        response_path = Rel(paths.ResponsePath),
        started_utc = modeStarted,
        completed_utc = modeCompleted,
        host_name = request.HostName,
        host_type = request.HostType,
        method = request.Method,
        core_api_path = method,
        host_path_label = hostPathLabel,
        correlation_id = request.CorrelationId,
        prompt_hash = promptHash,
        prompt_redacted = true,
        answer_redacted = true,
        contains_raw_private_content = false,
        response_success = response.Success,
        response_status = response.Status,
        semantic_quality_claimed = false,
        token_savings_claimed = false,
        readiness_claimed = false,
        runtime_extension_claimed = false,
        host_runtime_claimed = false,
        external_ai_send = false,
        external_ai_used = false,
        training_started = false,
        checkpoint_mutated = false,
        locator_updated = false,
        claim_scope = "core API invocation only; no host-visible Cursor readiness claim"
    });

    WriteJson(paths.CommandResultPath, new
    {
        schema_version = "frdp.scenario.command_result.v1",
        mode,
        command = "in-process SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest",
        outer_command = Environment.CommandLine,
        started_utc = modeStarted,
        completed_utc = modeCompleted,
        exit_code = commandExitCode,
        request_path = Rel(paths.RequestPath),
        response_path = Rel(paths.ResponsePath),
        trace_path = Rel(paths.CoreTracePath),
        stdout_path = Rel(paths.CommandOutputPath),
        stderr_path = Rel(paths.CommandErrorPath),
        external_ai_send = false,
        training_started = false,
        checkpoint_mutated = false,
        locator_updated = false
    });

    return new RunResult(mode, request, response, paths, commandExitCode);
}

XiigenExtensionResponse ErrorResponse(string error)
    => new(
        Success: false,
        Status: "measurement_core_invocation_error",
        ChatDisplayText: "Core invocation failed; no quality or readiness claim.",
        SelectedSkills: Array.Empty<string>(),
        CheckSetResults: new Dictionary<string, string>
        {
            ["core_boundary_executed"] = "false",
            ["semantic_quality_claimed"] = "false",
            ["runtime_extension_claimed"] = "false"
        },
        StructuredTaskRefs: Array.Empty<string>(),
        ScenarioRefs: Array.Empty<string>(),
        Warnings: new[] { "semantic_quality_claimed=false", "core_boundary_only_no_host_runtime_claim" },
        Errors: new[] { error },
        DebugTrace: null);

TracePacket BuildTrace(string mode, XiigenExtensionResponse response, ModePaths paths)
{
    var trace = TokenTraceAccounting.CreateHeuristicTrace(new ExtensionTokenTraceInput(
        ScenarioId: scenarioId,
        PairId: pairId,
        Mode: mode,
        HostName: hostName,
        Method: method,
        DebugEnabled: debugEnabled,
        PromptText: promptText,
        VisibleAnswerText: response.ChatDisplayText ?? string.Empty,
        IsolationMode: isolationMode,
        ArtifactRefs: new Dictionary<string, string>
        {
            ["request"] = Rel(paths.RequestPath),
            ["response"] = Rel(paths.ResponsePath),
            ["core_trace"] = Rel(paths.CoreTracePath),
            ["command_result"] = Rel(paths.CommandResultPath),
            ["command_output"] = Rel(paths.CommandOutputPath),
            ["command_error"] = Rel(paths.CommandErrorPath)
        },
        TraceStartedAtUtc: DateTimeOffset.UtcNow,
        TraceEndedAtUtc: DateTimeOffset.UtcNow));

    return new TracePacket(
        schema_version: trace.SchemaVersion,
        scenario_id: trace.ScenarioId,
        pair_id: trace.PairId,
        mode: trace.Mode,
        host_name: trace.HostName,
        host_path_label: hostPathLabel,
        method: trace.Method,
        core_api_path: method,
        debug_enabled: trace.DebugEnabled,
        prompt_hash: trace.PromptHash,
        visible_answer_hash: trace.VisibleAnswerHash,
        tokenizer_name: trace.TokenizerName,
        tokenizer_version: trace.TokenizerVersion,
        tokenizer_accounting_type: trace.TokenizerAccountingType,
        token_count_input: trace.TokenCountInput,
        token_count_output: trace.TokenCountOutput,
        token_count_total: trace.TokenCountTotal,
        isolation_mode: trace.IsolationMode,
        trace_started_at_utc: trace.TraceStartedAtUtc,
        trace_ended_at_utc: trace.TraceEndedAtUtc,
        artifact_refs: trace.ArtifactRefs,
        prompt_redacted: trace.PromptRedacted,
        answer_redacted: trace.AnswerRedacted,
        contains_raw_private_content: trace.ContainsRawPrivateContent,
        external_ai_send: false,
        semantic_quality_claimed: trace.SemanticQualityClaimed,
        token_savings_claimed: trace.TokenSavingsClaimed,
        readiness_claimed: false,
        learnability_claimed: trace.LearnabilityClaimed,
        training_started: false,
        checkpoint_mutated: false,
        locator_updated: false);
}

PairValidationPacket ValidatePair(TracePacket baselineTrace, TracePacket adaptiveTrace)
{
    var errors = new List<string>();
    RequireSame("scenario_id", baselineTrace.scenario_id, adaptiveTrace.scenario_id, errors);
    RequireSame("pair_id", baselineTrace.pair_id, adaptiveTrace.pair_id, errors);
    RequireSame("host_name", baselineTrace.host_name, adaptiveTrace.host_name, errors);
    RequireSame("host_path_label", baselineTrace.host_path_label, adaptiveTrace.host_path_label, errors);
    RequireSame("method", baselineTrace.method, adaptiveTrace.method, errors);
    RequireSame("core_api_path", baselineTrace.core_api_path, adaptiveTrace.core_api_path, errors);
    RequireSame("debug_enabled", baselineTrace.debug_enabled, adaptiveTrace.debug_enabled, errors);
    RequireSame("prompt_hash", baselineTrace.prompt_hash, adaptiveTrace.prompt_hash, errors);
    RequireSame("tokenizer_name", baselineTrace.tokenizer_name, adaptiveTrace.tokenizer_name, errors);
    RequireSame("tokenizer_version", baselineTrace.tokenizer_version, adaptiveTrace.tokenizer_version, errors);
    RequireSame("tokenizer_accounting_type", baselineTrace.tokenizer_accounting_type, adaptiveTrace.tokenizer_accounting_type, errors);
    RequireSame("isolation_mode", baselineTrace.isolation_mode, adaptiveTrace.isolation_mode, errors);
    RequireSame("external_ai_send", baselineTrace.external_ai_send, adaptiveTrace.external_ai_send, errors);
    RequireSame("contains_raw_private_content", baselineTrace.contains_raw_private_content, adaptiveTrace.contains_raw_private_content, errors);
    RequireSame("prompt_redacted", baselineTrace.prompt_redacted, adaptiveTrace.prompt_redacted, errors);
    RequireSame("answer_redacted", baselineTrace.answer_redacted, adaptiveTrace.answer_redacted, errors);

    if (baselineTrace.mode != "baseline")
        errors.Add("baseline_mode_required");
    if (adaptiveTrace.mode != "adaptive")
        errors.Add("adaptive_mode_required");
    if (baselineTrace.prompt_hash != expectedPromptHash || adaptiveTrace.prompt_hash != expectedPromptHash)
        errors.Add("prompt_hash_must_match_accepted_row12_spec");
    if (baselineTrace.token_savings_claimed || adaptiveTrace.token_savings_claimed)
        errors.Add("token_savings_claim_forbidden");
    if (baselineTrace.semantic_quality_claimed || adaptiveTrace.semantic_quality_claimed)
        errors.Add("semantic_quality_claim_forbidden");
    if (baselineTrace.readiness_claimed || adaptiveTrace.readiness_claimed)
        errors.Add("readiness_claim_forbidden");
    if (baselineTrace.external_ai_send || adaptiveTrace.external_ai_send)
        errors.Add("external_ai_send_forbidden");

    var delta = adaptiveTrace.token_count_total - baselineTrace.token_count_total;
    return new PairValidationPacket(
        schema_version: "frdp.cursor_token_pair_validation.v1",
        scenario_id: scenarioId,
        pair_id: pairId,
        pair_valid: errors.Count == 0,
        pair_validation_errors_count: errors.Count,
        pair_validation_errors: errors.ToArray(),
        baseline_total_tokens: baselineTrace.token_count_total,
        adaptive_total_tokens: adaptiveTrace.token_count_total,
        token_delta: delta,
        token_reduction_observed: delta < 0,
        external_ai_send: false,
        token_savings_claimed: false,
        semantic_quality_claimed: false,
        readiness_claimed: false,
        token_savings_claim_allowed: false);
}

QualityGuardPacket BuildQualityGuard(
    XiigenExtensionResponse baselineResponse,
    XiigenExtensionResponse adaptiveResponse,
    PairValidationPacket pair)
{
    var baselineText = JsonSerializer.Serialize(baselineResponse, compactJsonOptions);
    var adaptiveText = JsonSerializer.Serialize(adaptiveResponse, compactJsonOptions);
    var combinedText = baselineText + "\n" + adaptiveText;
    var selected = baselineResponse.SelectedSkills.Concat(adaptiveResponse.SelectedSkills).Distinct(StringComparer.Ordinal).ToArray();
    var selectedAllowed = selected.All(skill => allowedSkills.Contains(skill, StringComparer.Ordinal));
    var nonClaimTerms = new[]
    {
        "no token savings claim",
        "no semantic quality claim",
        "no readiness claim",
        "no training",
        "no checkpoint promotion",
        "no locator update"
    };

    var assertions = new List<QualityAssertion>
    {
        AssertItem(1, "Pair validation has zero errors", pair.pair_validation_errors_count == 0, $"pair_validation_errors_count={pair.pair_validation_errors_count}"),
        AssertItem(2, "Both traces forbid token savings, semantic quality, and learnability claims", !baselineTrace.token_savings_claimed && !adaptiveTrace.token_savings_claimed && !baselineTrace.semantic_quality_claimed && !adaptiveTrace.semantic_quality_claimed && !baselineTrace.learnability_claimed && !adaptiveTrace.learnability_claimed, "trace claim flags are false"),
        AssertItem(3, "Baseline and adaptive responses preserve marker", baselineText.Contains(marker, StringComparison.Ordinal) && adaptiveText.Contains(marker, StringComparison.Ordinal), "actual response JSON was searched for the exact marker"),
        AssertItem(4, "Baseline and adaptive responses preserve scenario id", baselineText.Contains(scenarioId, StringComparison.Ordinal) && adaptiveText.Contains(scenarioId, StringComparison.Ordinal), "actual response JSON was searched for the exact scenario id"),
        AssertItem(5, "prompt_to_task output contains task_id, task_title, constraints, and acceptance_gates", ContainsAll(combinedText, "prompt_to_task", "task_id", "task_title", "constraints", "acceptance_gates"), "actual response has prompt_to_task status but not all required structured fields"),
        AssertItem(6, "task_to_plan output contains exactly three ordered steps", ContainsAll(combinedText, "task_to_plan", "step 1", "step 2", "step 3") || ContainsAll(combinedText, "task_to_plan", "\"steps\"", "\"3\""), "actual compact response does not expose exactly three ordered steps"),
        AssertItem(7, "Every plan step has one verification gate", ContainsAll(combinedText, "verification gate", "step"), "actual compact response does not expose per-step verification gates"),
        AssertItem(8, "task_to_skills output exists", baselineResponse.SelectedSkills.Count > 0 && adaptiveResponse.SelectedSkills.Count > 0, $"baseline_selected={baselineResponse.SelectedSkills.Count}; adaptive_selected={adaptiveResponse.SelectedSkills.Count}"),
        AssertItem(9, "Skill selection includes prompt-to-task, task-to-plan, and task-to-skills", selected.Contains("prompt-to-task", StringComparer.Ordinal) && selected.Contains("task-to-plan", StringComparer.Ordinal) && selected.Contains("task-to-skills", StringComparer.Ordinal), $"selected={string.Join(",", selected)}"),
        AssertItem(10, "Every selected skill is from the allowed set", selectedAllowed, $"selected={string.Join(",", selected)}"),
        AssertItem(11, "References include marker and scenario id", ContainsAll(combinedText, marker, scenarioId), "actual response references were searched"),
        AssertItem(12, "non_claims include all required non-claims", nonClaimTerms.All(term => combinedText.Contains(term, StringComparison.OrdinalIgnoreCase)), "actual response does not include the complete non_claims section text"),
        AssertItem(13, "Response is row-specific and not generic", ContainsAll(combinedText, marker, scenarioId, "task", "plan", "skills", "references", "non_claims"), "actual response lacks required row-specific marker/sections"),
        AssertItem(14, "Deterministic shortcuts are disclosed", ContainsAll(combinedText, "synthetic no-send workload", "heuristic tokenizer", "no external AI", "token delta alone"), "actual response does not disclose all deterministic shortcut boundaries")
    };

    var passed = assertions.Count(item => item.passed);
    return new QualityGuardPacket(
        schema_version: "frdp.row12.quality_guard.v1",
        pass: passed == 14 && pair.pair_validation_errors_count == 0,
        quality_assertions_total: 14,
        quality_assertions_passed: passed,
        critical_failures_count: assertions.Count - passed,
        assertions: assertions.ToArray(),
        external_ai_send: false,
        token_savings_claimed: false,
        semantic_quality_claimed: false,
        readiness_claimed: false,
        quality_scope_note: "This guard checks actual baseline/adaptive response JSON. It does not credit request text or manually authored report text as response quality.");
}

HostVisibleBlocker WriteHostVisibleBlocker()
{
    var jsonPath = Path.Combine(measurementRoot, "cursor-host-visible-candidate-blocker.json");
    var mdPath = Path.Combine(measurementRoot, "cursor-host-visible-candidate-blocker.md");
    var blocker = new
    {
        schema_version = "frdp.row12.cursor_host_visible_candidate_blocker.v1",
        status = "BLOCKED_WITH_NEXT_ACTION",
        scenario_id = scenarioId,
        marker,
        blocker = "No existing row12-specific no-send Cursor host-visible harness path was found in this slice.",
        evidence_checked = new[]
        {
            "Existing row11 Cursor extension-host/panel harness files are hard-coded to frdp-p01-cursor-full-extension-entrypoint and its marker.",
            "The row12 measurement work order only allows writes under the measurement folder and does not authorize changing/parameterizing the row11 harness or launching a broad Cursor host proof."
        },
        next_fix = "Create or parameterize a row12 no-send Cursor extension-host/panel harness that accepts scenario_id, marker, prompt, method=prompt-to-task, and evidenceRoot under the measurement folder; then rerun the same row12 marker and attach host-visible candidate evidence.",
        external_ai_send = false,
        token_savings_claimed = false,
        semantic_quality_claimed = false,
        readiness_claimed = false
    };
    WriteJson(jsonPath, blocker);
    File.WriteAllText(
        mdPath,
        string.Join(
            '\n',
            "# Cursor host-visible candidate blocker",
            "",
            $"Status: `BLOCKED_WITH_NEXT_ACTION`",
            "",
            $"Scenario: `{scenarioId}`",
            $"Marker: `{marker}`",
            "",
            "No existing row12-specific no-send Cursor host-visible harness path was found in this slice.",
            "",
            "Existing row11 Cursor extension-host/panel harness files are hard-coded to another scenario and marker. Changing or parameterizing that harness is outside this measurement write scope.",
            "",
            "Next fix: create or parameterize a row12 no-send Cursor extension-host/panel harness that accepts scenario_id, marker, prompt, method=prompt-to-task, and evidenceRoot under the measurement folder; then rerun the same marker and attach host-visible candidate evidence.",
            "",
            "Non-claims: no token savings, no semantic quality, no readiness, no training, no checkpoint mutation, no locator update.",
            ""),
        new UTF8Encoding(false));
    return new HostVisibleBlocker(jsonPath, mdPath);
}

void WriteReportMarkdown(dynamic report, PairValidationPacket pair, QualityGuardPacket guard, string blockerJsonPath)
{
    var lines = new List<string>
    {
        "# Row 12 measurement execution report",
        "",
        $"Status: `{finalStatus}`",
        "",
        "## Current task",
        "",
        "Create the row 12 measurement packet for `frdp-p01-cursor-token-savings-baseline` using accepted `row12-workload-spec.md/json`.",
        "",
        "## Commands run",
        "",
        "- `dotnet run --project MeasurementRunner.csproj -- <measurement-root>`: completed; runner returned non-zero because the quality guard did not pass all assertions.",
        "",
        "## Token numbers (observation only)",
        "",
        $"Baseline input/output/total: `{baselineTrace.token_count_input}` / `{baselineTrace.token_count_output}` / `{baselineTrace.token_count_total}`.",
        $"Adaptive input/output/total: `{adaptiveTrace.token_count_input}` / `{adaptiveTrace.token_count_output}` / `{adaptiveTrace.token_count_total}`.",
        $"Observed candidate delta: `{adaptiveTrace.token_count_total - baselineTrace.token_count_total}`.",
        "",
        "This is not a token savings claim.",
        "",
        "## Pair validation",
        "",
        $"Pair valid: `{pair.pair_valid}`.",
        $"pair_validation_errors_count: `{pair.pair_validation_errors_count}`.",
        "",
        "## Quality guard",
        "",
        $"Assertions passed: `{guard.quality_assertions_passed}` / `{guard.quality_assertions_total}`.",
        $"Critical failures: `{guard.critical_failures_count}`.",
        "",
        "The guard checks actual response JSON only. Request text and report text are not credited as response quality.",
        "",
        "## Host-visible candidate",
        "",
        $"Exact blocker recorded: `{Rel(blockerJsonPath)}`.",
        "",
        "## Proven",
        "",
        "- Exact row12 prompt hash reproduced.",
        "- Baseline/adaptive local no-send core API calls completed.",
        "- Pair validation has zero same-field errors.",
        "- External AI send is false.",
        "",
        "## Not proven",
        "",
        "- Token savings.",
        "- Semantic quality.",
        "- Product readiness or full Cursor readiness.",
        "- Row12 Cursor host-visible candidate behavior.",
        "- The 14-assertion quality guard.",
        "",
        "## Counter recommendation for total 28",
        "",
        "Do not increment passed scenario count for row 12. Keep row 12 in needs-fix/blocked-for-host-and-quality-guard status until a row12-specific host-visible no-send path emits the required structured response and all 14 quality assertions pass.",
        ""
    };
    File.WriteAllText(Path.Combine(measurementRoot, "execution-report.md"), string.Join('\n', lines), new UTF8Encoding(false));
}

QualityAssertion AssertItem(int id, string assertion, bool passed, string evidence)
    => new(id, assertion, passed, evidence);

bool ContainsAll(string text, params string[] values)
    => values.All(value => text.Contains(value, StringComparison.OrdinalIgnoreCase));

void RequireSame<T>(string field, T left, T right, List<string> errors)
{
    if (!EqualityComparer<T>.Default.Equals(left, right))
        errors.Add($"{field}_must_match_between_baseline_and_adaptive");
}

string HashText(string text)
{
    var bytes = Encoding.UTF8.GetBytes(text);
    return "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

string HashFile(string path)
    => "sha256:" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

string Rel(string path)
    => Path.GetRelativePath(coreRoot, Path.GetFullPath(path)).Replace(Path.DirectorySeparatorChar, '/');

string[] RunGitStatus(string root)
{
    try
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo("git", "status --short")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = System.Diagnostics.Process.Start(startInfo);
        if (process is null)
            return new[] { "git_status_unavailable" };
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(5000);
        return output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    }
    catch (Exception ex)
    {
        return new[] { $"git_status_error:{ex.GetType().Name}" };
    }
}

void WriteJson(string relativeOrAbsolutePath, object value)
{
    var path = Path.IsPathRooted(relativeOrAbsolutePath)
        ? relativeOrAbsolutePath
        : Path.Combine(measurementRoot, relativeOrAbsolutePath);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, JsonSerializer.Serialize(value, jsonOptions) + "\n", new UTF8Encoding(false));
}

public sealed record ModePaths(
    string RequestPath,
    string ResponsePath,
    string CoreTracePath,
    string CommandResultPath,
    string CommandOutputPath,
    string CommandErrorPath);

public sealed record RunResult(
    string Mode,
    XiigenExtensionRequest Request,
    XiigenExtensionResponse Response,
    ModePaths Paths,
    int CommandExitCode);

public sealed record TracePacket(
    string schema_version,
    string scenario_id,
    string pair_id,
    string mode,
    string host_name,
    string host_path_label,
    string method,
    string core_api_path,
    bool debug_enabled,
    string prompt_hash,
    string visible_answer_hash,
    string tokenizer_name,
    string tokenizer_version,
    string tokenizer_accounting_type,
    int token_count_input,
    int token_count_output,
    int token_count_total,
    string isolation_mode,
    DateTimeOffset trace_started_at_utc,
    DateTimeOffset trace_ended_at_utc,
    IReadOnlyDictionary<string, string> artifact_refs,
    bool prompt_redacted,
    bool answer_redacted,
    bool contains_raw_private_content,
    bool external_ai_send,
    bool semantic_quality_claimed,
    bool token_savings_claimed,
    bool readiness_claimed,
    bool learnability_claimed,
    bool training_started,
    bool checkpoint_mutated,
    bool locator_updated);

public sealed record PairValidationPacket(
    string schema_version,
    string scenario_id,
    string pair_id,
    bool pair_valid,
    int pair_validation_errors_count,
    IReadOnlyList<string> pair_validation_errors,
    int baseline_total_tokens,
    int adaptive_total_tokens,
    int token_delta,
    bool token_reduction_observed,
    bool external_ai_send,
    bool token_savings_claimed,
    bool semantic_quality_claimed,
    bool readiness_claimed,
    bool token_savings_claim_allowed);

public sealed record QualityAssertion(
    int id,
    string assertion,
    bool passed,
    string evidence);

public sealed record QualityGuardPacket(
    string schema_version,
    bool pass,
    int quality_assertions_total,
    int quality_assertions_passed,
    int critical_failures_count,
    IReadOnlyList<QualityAssertion> assertions,
    bool external_ai_send,
    bool token_savings_claimed,
    bool semantic_quality_claimed,
    bool readiness_claimed,
    string quality_scope_note);

public sealed record HostVisibleBlocker(string JsonPath, string MarkdownPath);
