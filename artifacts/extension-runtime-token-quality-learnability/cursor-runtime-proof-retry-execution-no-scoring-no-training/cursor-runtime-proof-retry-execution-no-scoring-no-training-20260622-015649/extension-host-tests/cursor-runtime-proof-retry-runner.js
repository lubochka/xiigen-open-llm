"use strict";

const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, "");
}

function readJsonIfExists(filePath) {
  if (!filePath || !fs.existsSync(filePath)) {
    return null;
  }
  return JSON.parse(readText(filePath));
}

function writeJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function fileExists(filePath) {
  return typeof filePath === "string" && fs.existsSync(filePath);
}

function summarizeResult(label, result) {
  const request = readJsonIfExists(result && result.coreRequestRef);
  const response = readJsonIfExists(result && result.coreResponseRef);
  const coreTrace = readJsonIfExists(result && result.coreTraceRef);
  const hostTrace = readJsonIfExists(result && result.hostTraceRef);
  const entrypoint = readJsonIfExists(result && result.entrypointResponseRef);
  const redactedTrace = readJsonIfExists(result && result.redactedTraceRef);
  const refs = {
    core_request_ref: result && result.coreRequestRef,
    core_response_ref: result && result.coreResponseRef,
    core_trace_ref: result && result.coreTraceRef,
    host_trace_ref: result && result.hostTraceRef,
    entrypoint_response_ref: result && result.entrypointResponseRef,
    redacted_trace_ref: result && result.redactedTraceRef
  };
  return {
    label,
    command_result_present: !!result,
    entrypoint_invoked: result && result.entrypointInvoked === true,
    entrypoint_name: result && result.entrypointName,
    entrypoint_type: result && result.entrypointType,
    command_passed: result && result.passed === true,
    adapter_functionality_passed: result && result.adapterFunctionalityPassed === true,
    result_runtime_evidence: result && result.runtimeEvidence === true,
    result_core_api_runtime_evidence: result && result.coreApiRuntimeEvidence === true,
    correlation_id_result: result && result.correlationId,
    correlation_id_request: request && request.CorrelationId,
    correlation_id_response: response && (response.correlationId || response.request_correlation_id || response.CorrelationId),
    correlation_id_core_trace: coreTrace && (coreTrace.correlation_id || coreTrace.correlationId),
    correlation_id_host_trace: hostTrace && hostTrace.correlationId,
    correlation_id_redacted_trace: redactedTrace && redactedTrace.correlationId,
    request_host: request && (request.HostType || request.hostKind),
    request_method: request && request.Method,
    requested_skills: request && request.RequestedSkills,
    available_skills: request && request.AvailableSkills,
    request_host_facts: request && request.HostFacts ? {
      host_entrypoint: request.HostFacts.host_entrypoint,
      runtime_proof_scenario: request.HostFacts.runtime_proof_scenario,
      runtime_proof_command: request.HostFacts.runtime_proof_command,
      dashboard_only_shortcut: request.HostFacts.dashboard_only_shortcut,
      same_handler_core_boundary: request.HostFacts.same_handler_core_boundary,
      requested_skills_count: request.HostFacts.requested_skills_count,
      available_skills_count: request.HostFacts.available_skills_count,
      host_skill_selection_binding: request.HostFacts.host_skill_selection_binding
    } : null,
    response_success: response && (response.Success === true || response.passed === true),
    response_status: response && (response.Status || response.status || response.errorCode),
    response_errors: response && response.Errors,
    response_gaps_count: response && Array.isArray(response.gaps) ? response.gaps.length : null,
    core_trace_kind: coreTrace && (coreTrace.traceKind || coreTrace.endpoint || coreTrace.schema_version),
    host_trace_kind: hostTrace && hostTrace.traceKind,
    host_trace_skill_binding: hostTrace && hostTrace.skillBinding,
    host_trace_calls_same_handler_core_boundary: hostTrace && hostTrace.callsSameHandlerCoreBoundary === true,
    host_trace_dashboard_only_shortcut: hostTrace && hostTrace.dashboardOnlyShortcut === true,
    redacted_trace_kind: redactedTrace && redactedTrace.traceKind,
    redacted_trace_reached_llm_mvp_core: redactedTrace && redactedTrace.reachedLlmMvpCore === true,
    redacted_trace_input_present: !!(redactedTrace && redactedTrace.inputRedacted),
    redacted_trace_output_present: !!(redactedTrace && redactedTrace.outputRedacted),
    redacted_trace_negative_error_path_present: !!(redactedTrace && redactedTrace.outputRedacted && redactedTrace.outputRedacted.negativeErrorPathPresent === true),
    refs_exist: Object.fromEntries(Object.entries(refs).map(([name, filePath]) => [name, fileExists(filePath)])),
    refs
  };
}

async function withEnv(values, action) {
  const keys = Object.keys(values);
  const previous = Object.fromEntries(keys.map((key) => [key, process.env[key]]));
  for (const [key, value] of Object.entries(values)) {
    if (value === undefined || value === null) {
      delete process.env[key];
    } else {
      process.env[key] = String(value);
    }
  }
  try {
    return await action();
  } finally {
    for (const key of keys) {
      if (previous[key] === undefined) {
        delete process.env[key];
      } else {
        process.env[key] = previous[key];
      }
    }
  }
}

async function run() {
  const startedUtc = new Date().toISOString();
  const runRoot = process.env.CURSOR_RUNTIME_PROOF_RETRY_RUN_ROOT;
  const evidenceRoot = process.env.PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR;
  const reportPath = path.join(runRoot, "extension-host-runner-result.json");
  const extensionId = "xiigen.cursor-xiigen-smart-skills";
  const commandId = "xiigenSmartSkills.runtimeProof.invoke";
  const sameRunId = process.env.CURSOR_RUNTIME_PROOF_RETRY_RUN_ID || `cursor-runtime-proof-retry-${Date.now()}`;
  const requestedSkills = ["prompt-to-task", "skill-selection"];
  const availableSkills = ["prompt-to-task", "orchestration-plan", "skill-selection"];
  const coreRoot = "C:\\Projects\\llm_mvp_core";
  const effectiveCoreApiScript = "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1";
  const contractCoreApiScript = "C:\\Projects\\llm_mvp_core\\tools\\phase40-extension-core-api.ps1";
  const report = {
    schema: "xiigen.cursor.runtime_proof_retry.extension_host_runner.v1",
    work_order_id: "DOCS-QA-CURSOR-RUNTIME-PROOF-RETRY-EXECUTION-NO-SCORING-NO-TRAINING-01",
    started_utc: startedUtc,
    selected_host: "Cursor",
    extension_id: extensionId,
    command_id: commandId,
    run_root: runRoot,
    evidence_root: evidenceRoot,
    same_run_id: sameRunId,
    core_root: coreRoot,
    effective_core_api_script: effectiveCoreApiScript,
    contract_core_api_script: contractCoreApiScript,
    extension_visible_to_host: false,
    extension_activated: false,
    commands_observed: [],
    positive_summary: null,
    negative_summary: null,
    errors: [],
    forbidden_claims: {
      token_savings_claimed: false,
      quality_improvement_claimed: false,
      semantic_quality_claimed: false,
      learnability_claimed: false
    },
    forbidden_actions: {
      scoring: false,
      training: false,
      model_mutation: false,
      checkpoint_promotion: false,
      claude_runtime_proof: false,
      codex_runtime_proof: false,
      external_ai_web_model_calls: false
    }
  };

  try {
    ensureDir(evidenceRoot);
    const extension = vscode.extensions.getExtension(extensionId);
    report.extension_visible_to_host = !!extension;
    if (!extension) {
      throw new Error("Cursor XIIGen extension was not visible to the Extension Host.");
    }
    report.extension_path = extension.extensionPath;
    report.extension_main = extension.packageJSON && extension.packageJSON.main;
    await extension.activate();
    report.extension_activated = extension.isActive === true;

    const commands = await vscode.commands.getCommands(true);
    report.commands_observed = commands.filter((name) => /xiigen/i.test(name)).sort();
    if (!report.commands_observed.includes(commandId)) {
      throw new Error(`Command ${commandId} was not registered in the Extension Host.`);
    }

    const config = vscode.workspace.getConfiguration("xiigenSmartSkills");
    await config.update("coreRoot", coreRoot, vscode.ConfigurationTarget.Global);
    await config.update("coreApiScript", effectiveCoreApiScript, vscode.ConfigurationTarget.Global);
    await config.update("debug", false, vscode.ConfigurationTarget.Global);

    const positiveInput = {
      scenario: "positive_request",
      correlationId: `${sameRunId}-positive`,
      prompt: "Redacted Cursor smart-skills runtime proof request for bounded Phase40 work-order routing.",
      evidenceRoot,
      redactedInput: {
        scenario: "positive_request",
        prompt: "[REDACTED Cursor smart-skills routing request]",
        privateDataIncluded: false
      },
      method: "skill-selection",
      requestedSkills,
      availableSkills
    };
    const positiveResult = await vscode.commands.executeCommand(commandId, positiveInput);
    report.positive_summary = summarizeResult("positive_request", positiveResult);

    const negativeInput = {
      scenario: "negative_error_request",
      correlationId: `${sameRunId}-negative`,
      prompt: "",
      evidenceRoot,
      redactedInput: {
        scenario: "negative_error_request",
        prompt: "[REDACTED missing or invalid Cursor smart-skills request]",
        privateDataIncluded: false
      },
      method: "skill-selection",
      requestedSkills,
      availableSkills
    };
    const negativeResult = await withEnv({ XIIGEN_TARGET_LOCATOR_REQUIRED: "true" }, async () => vscode.commands.executeCommand(commandId, negativeInput));
    report.negative_summary = summarizeResult("negative_error_request", negativeResult);
  } catch (error) {
    report.errors.push(error && error.stack ? error.stack : String(error));
  } finally {
    report.ended_utc = new Date().toISOString();
    report.elapsed_ms = new Date(report.ended_utc).getTime() - new Date(startedUtc).getTime();
    writeJson(reportPath, report);
  }
}

module.exports = { run };
