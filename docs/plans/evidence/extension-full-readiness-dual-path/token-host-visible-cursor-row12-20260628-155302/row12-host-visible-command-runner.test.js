"use strict";

const crypto = require("crypto");
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const evidenceRoot = __dirname;
const reportPath = path.join(evidenceRoot, "row12-host-visible-runner-report.json");
const commandName = "xiigenSmartSkills.runtimeProof.invoke";
const extensionId = "xiigen.cursor-xiigen-smart-skills";
const expectedPromptHash = "sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374";
const scenarioId = "frdp-p01-cursor-token-savings-baseline";
const marker = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1";
const pairId = "frdp-p01-row12-cursor-token-baseline-v1";
const prompt = `[${marker}]
Host: Cursor no-send local extension runtime.
Scenario: ${scenarioId}.
User request: Convert this synthetic request into a bounded Smart Skills execution plan without calling external AI.
Requirements:
1. Preserve the marker and scenario id in references.
2. Produce a prompt_to_task result with task_id, task_title, constraints, and acceptance_gates.
3. Produce a task_to_plan result with three ordered steps and one verification gate per step.
4. Produce a task_to_skills result selecting skills from this allowed set only: prompt-to-task, task-to-plan, task-to-skills, evidence-ledger.
5. State non_claims: no token savings claim, no semantic quality claim, no readiness claim, no training, no checkpoint promotion, no locator update.
6. Return deterministic structured text with sections: task, plan, skills, references, non_claims.`;

function writeJson(filePath, value) {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function fileExists(filePath) {
  return typeof filePath === "string" && filePath.length > 0 && fs.existsSync(filePath);
}

async function run() {
  const startedUtc = new Date().toISOString();
  const promptHash = `sha256:${crypto.createHash("sha256").update(prompt, "utf8").digest("hex")}`;
  const report = {
    schema_version: "frdp.row12.cursor_host_visible_runner_report.v1",
    status: "STARTED",
    started_utc: startedUtc,
    host_kind: "Cursor",
    evidence_root: evidenceRoot,
    command_name: commandName,
    command_path_used: "vscode.commands.executeCommand('xiigenSmartSkills.runtimeProof.invoke', row12Input)",
    extension_id: extensionId,
    scenario_id: scenarioId,
    marker,
    pair_id: pairId,
    prompt_hash: promptHash,
    expected_prompt_hash: expectedPromptHash,
    same_prompt_hash_as_previous_synthetic_pair: promptHash === expectedPromptHash,
    method: "prompt-to-task",
    requested_skills: ["prompt-to-task", "task-to-plan", "task-to-skills", "evidence-ledger"],
    available_skills: ["prompt-to-task", "task-to-plan", "task-to-skills", "evidence-ledger"],
    external_ai_send: false,
    external_ai_used_claimed: false,
    token_savings_claimed: false,
    semantic_quality_claimed: false,
    readiness_claimed: false,
    training_started: false,
    checkpoint_mutated: false,
    errors: []
  };

  try {
    if (promptHash !== expectedPromptHash) {
      throw new Error(`Prompt hash mismatch: ${promptHash}`);
    }

    const extension = vscode.extensions.getExtension(extensionId);
    report.extension_visible_to_host = !!extension;
    if (extension) {
      report.extension_path = extension.extensionPath;
      report.extension_is_active_before = extension.isActive;
      await extension.activate();
      report.extension_is_active_after_activate = extension.isActive;
    }

    const commandsBefore = await vscode.commands.getCommands(true);
    report.xiigen_commands_before = commandsBefore.filter((name) => name.toLowerCase().includes("xiigen"));
    report.command_registered_before_execute = commandsBefore.includes(commandName);

    const result = await vscode.commands.executeCommand(commandName, {
      scenario: "positive_request",
      correlationId: `${pairId}|${marker}|${scenarioId}`,
      prompt,
      evidenceRoot,
      method: "prompt-to-task",
      requestedSkills: report.requested_skills,
      availableSkills: report.available_skills,
      redactedInput: {
        scenario: "row12_cursor_host_visible_no_send",
        marker,
        scenario_id: scenarioId,
        pair_id: pairId,
        prompt_hash: promptHash,
        privateDataIncluded: false
      }
    });

    report.command_result_type = typeof result;
    report.command_returned = !!result;
    report.result_passed = result && result.passed === true;
    report.result_runtime_evidence = result && result.runtimeEvidence === true;
    report.result_entrypoint_invoked = result && result.entrypointInvoked === true;
    report.result_entrypoint_name = result && result.entrypointName;
    report.result_entrypoint_type = result && result.entrypointType;
    report.result_evidence_scope = result && result.evidenceScope;
    report.result_core_api_runtime_evidence = result && result.coreApiRuntimeEvidence === true;
    report.result_external_ai_or_unrelated_tool_used = result && result.externalAiOrUnrelatedToolUsed === true;
    report.result_semantic_quality_claimed = result && result.semanticQualityClaimed === true;
    report.result_token_savings_claimed = result && result.tokenSavingsClaimed === true;
    report.refs = result ? {
      core_request_ref: result.coreRequestRef,
      core_response_ref: result.coreResponseRef,
      core_trace_ref: result.coreTraceRef,
      host_trace_ref: result.hostTraceRef,
      entrypoint_response_ref: result.entrypointResponseRef,
      redacted_trace_ref: result.redactedTraceRef,
      latest_ref: path.join(evidenceRoot, "xiigen-cursor-host-runtime-latest.json")
    } : {};
    report.refs_exist = Object.fromEntries(Object.entries(report.refs).map(([name, filePath]) => [name, fileExists(filePath)]));

    const requestText = fileExists(report.refs.core_request_ref)
      ? fs.readFileSync(report.refs.core_request_ref, "utf8")
      : "";
    const redactionTargets = [
      report.refs.core_response_ref,
      report.refs.core_trace_ref,
      report.refs.host_trace_ref,
      report.refs.entrypoint_response_ref,
      report.refs.redacted_trace_ref
    ].filter(fileExists);
    const promptLeaks = redactionTargets.filter((filePath) => fs.readFileSync(filePath, "utf8").includes(prompt));
    const displayText = result && typeof result.CursorDisplayText === "string" ? result.CursorDisplayText : "";
    report.redaction = {
      prompt_present_in_request_payload_by_contract: requestText.includes(prompt),
      full_prompt_absent_from_response_trace_display: promptLeaks.length === 0 && !displayText.includes(prompt),
      files_with_full_prompt_outside_request: promptLeaks,
      marker_may_appear_in_redacted_metadata: true
    };

    const commandsAfter = await vscode.commands.getCommands(true);
    report.xiigen_commands_after = commandsAfter.filter((name) => name.toLowerCase().includes("xiigen"));
    report.command_registered_after_execute = commandsAfter.includes(commandName);

    report.status = report.command_returned
      && report.result_entrypoint_invoked
      && report.result_entrypoint_name === commandName
      && report.result_external_ai_or_unrelated_tool_used === false
      && report.result_semantic_quality_claimed === false
      && report.result_token_savings_claimed === false
      && Object.values(report.refs_exist).every(Boolean)
      && report.redaction.prompt_present_in_request_payload_by_contract
      && report.redaction.full_prompt_absent_from_response_trace_display
        ? "CURSOR_HOST_VISIBLE_ROW12_TOKEN_TRACE_READY"
        : "BLOCKED_CURSOR_HOST_VISIBLE_HARNESS_FAILED";
  } catch (error) {
    report.status = "BLOCKED_CURSOR_HOST_VISIBLE_HARNESS_FAILED";
    report.errors.push({
      stage: "row12-host-visible-command-runner",
      message: error && error.message ? error.message : String(error),
      stack: error && error.stack ? String(error.stack) : null
    });
  } finally {
    report.ended_utc = new Date().toISOString();
    report.elapsed_ms = new Date(report.ended_utc).getTime() - new Date(startedUtc).getTime();
    writeJson(reportPath, report);
    if (report.status !== "CURSOR_HOST_VISIBLE_ROW12_TOKEN_TRACE_READY") {
      throw new Error(`Row12 Cursor host-visible command runner did not pass: ${report.status}`);
    }
  }
}

module.exports = { run };
