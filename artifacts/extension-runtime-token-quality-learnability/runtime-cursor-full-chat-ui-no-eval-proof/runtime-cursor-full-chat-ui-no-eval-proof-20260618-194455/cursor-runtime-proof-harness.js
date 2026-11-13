"use strict";

const fs = require("fs");
const path = require("path");
const assert = require("assert");
const vscode = require("vscode");

const runId = "runtime-cursor-full-chat-ui-no-eval-proof-20260618-194455";
const runRoot = path.resolve(__dirname);
const evidenceRoot = path.join(runRoot, "extension-host-evidence");
const reportPath = path.join(runRoot, "proof-packet.json");

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function writeJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, "");
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function fileExists(filePath) {
  return typeof filePath === "string" && fs.existsSync(filePath);
}

function ownKeys(value) {
  if (!value) {
    return [];
  }

  return Array.from(new Set([
    ...Object.keys(value),
    ...Object.getOwnPropertyNames(value)
  ])).sort();
}

function redactDisplay(value) {
  return typeof value === "string" ? value.replace(/SENTINEL_[A-Z0-9_]+_DO_NOT_DISPLAY/gu, "<redacted-sentinel>") : "";
}

function summarizeEntrypoint(label, result, expected) {
  assert(result, `${label} returned no result`);
  const refs = {
    request_ref: result.coreRequestRef,
    response_ref: result.coreResponseRef,
    core_trace_ref: result.coreTraceRef,
    host_trace_ref: result.hostTraceRef,
    entrypoint_response_ref: result.entrypointResponseRef,
    host_panel_ui_ref: result.hostPanelUiRef
  };

  for (const [name, filePath] of Object.entries(refs)) {
    if (name === "host_panel_ui_ref") {
      continue;
    }
    assert(fileExists(filePath), `${label} ${name} missing: ${filePath}`);
  }

  const request = readJson(refs.request_ref);
  const response = readJson(refs.response_ref);
  const hostTrace = readJson(refs.host_trace_ref);
  const entrypointResponse = readJson(refs.entrypoint_response_ref);
  const displayText = typeof result.CursorDisplayText === "string" ? result.CursorDisplayText : "";
  const markdownText = Array.isArray(expected.markdown) ? expected.markdown.join("\n") : "";
  const debugShape = `xiigen ${request.Method} :\ninput:`;
  const sentinel = expected.sentinel || "";
  const leakedRefs = [
    refs.response_ref,
    refs.core_trace_ref,
    refs.host_trace_ref,
    refs.entrypoint_response_ref
  ].filter((filePath) => sentinel && readText(filePath).includes(sentinel));
  const expectedError = expected.expected_error || "";
  const responseErrors = Array.isArray(response.Errors) ? response.Errors : [];
  const responseSuccess = response.Success === true || response.passed === true;
  const negativePassed = expected.negative === true
    ? responseSuccess === false && (responseErrors.includes(expectedError) || JSON.stringify(response).includes(expectedError))
    : null;

  return {
    label,
    surface: expected.surface,
    evidence_classification: expected.classification,
    passed: expected.negative === true ? negativePassed === true : responseSuccess === true,
    expected_negative: expected.negative === true,
    expected_error: expectedError,
    response_success: responseSuccess,
    response_status: response.Status || response.status || "",
    response_errors: responseErrors,
    extension_host_entrypoint_invoked: result.entrypointInvoked === true,
    entrypoint_name: result.entrypointName || request.HostFacts?.host_entrypoint || "",
    entrypoint_type: result.entrypointType || expected.surface,
    method: request.Method,
    debug_enabled_request: request.DebugEnabled === true,
    debug_shape_visible: displayText.includes(debugShape) || markdownText.includes(debugShape),
    display_text_visible: displayText.length > 0,
    display_text_redacted_preview: redactDisplay(displayText).slice(0, 500),
    markdown_visible: markdownText.length > 0,
    markdown_redacted_preview: redactDisplay(markdownText).slice(0, 500),
    panel_output_available_noninteractive: expected.surface === "panel" ? false : null,
    panel_output_note: expected.surface === "panel"
      ? "Panel command executed in Extension Host; WebView content is not directly inspectable in this noninteractive proof, so full panel UI visibility is not claimed."
      : "",
    request_contains_prompt_by_contract: sentinel ? readText(refs.request_ref).includes(sentinel) : null,
    sentinel_absent_from_display_response_core_host_entrypoint_trace: sentinel
      ? leakedRefs.length === 0 && !displayText.includes(sentinel) && !markdownText.includes(sentinel)
      : null,
    correlation_id: request.CorrelationId || response.CorrelationId || response.correlationId || "",
    recorded_utc: response.recordedUtc || response.RecordedUtc || hostTrace.recordedUtc || "",
    refs,
    host_trace_debug_enabled: hostTrace.debugEnabled === true,
    host_trace_external_ai_used: hostTrace.externalAiOrUnrelatedToolUsed === true,
    response_runtime_extension_claimed: response.RuntimeExtensionClaimed === true,
    response_semantic_quality_claimed: response.SemanticQualityClaimed === true,
    entrypoint_response_full_cursor_ui_proof: entrypointResponse.fullCursorExtensionHostUiProof === true
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
  ensureDir(evidenceRoot);
  process.env.PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = evidenceRoot;

  const chatApi = vscode.chat;
  const chatApiKeysBeforeActivation = ownKeys(chatApi);
  const chatRegistrations = [];
  let chatIntercepted = false;
  let chatInterceptError = null;

  if (chatApi && typeof chatApi.createChatParticipant === "function") {
    try {
      const originalCreateChatParticipant = chatApi.createChatParticipant.bind(chatApi);
      chatApi.createChatParticipant = (id, handler) => {
        chatRegistrations.push({
          id,
          handler_type: typeof handler,
          observed_via_createChatParticipant_intercept: true
        });
        return originalCreateChatParticipant(id, handler);
      };
      chatIntercepted = true;
    } catch (error) {
      chatInterceptError = error && error.message ? error.message : String(error);
    }
  }

  const extension = vscode.extensions.getExtension("xiigen.cursor-xiigen-smart-skills")
    || vscode.extensions.getExtension("lubochka.cursor-xiigen-smart-skills");
  assert(extension, "Cursor XIIGen development extension was not found.");
  const activated = await extension.activate();
  assert(activated !== undefined || extension.isActive === true, "Cursor extension activation did not complete.");

  const config = vscode.workspace.getConfiguration("xiigenSmartSkills");
  const debugInspect = config.inspect("debug") || {};
  const previousDebug = debugInspect.globalValue;
  const previousLocatorPath = (config.inspect("targetLocatorPath") || {}).globalValue;
  const previousLocatorJson = (config.inspect("targetLocatorJson") || {}).globalValue;

  const cases = [];

  try {
    await config.update("targetLocatorPath", "", vscode.ConfigurationTarget.Global);
    await config.update("targetLocatorJson", "", vscode.ConfigurationTarget.Global);

    await config.update("debug", false, vscode.ConfigurationTarget.Global);
    const commandOffSentinel = "SENTINEL_CURSOR_COMMAND_DEBUG_OFF_DO_NOT_DISPLAY";
    const commandOff = await vscode.commands.executeCommand("xiigen.smartSkills.select", {
      prompt: `Cursor command debug off proof fixture. ${commandOffSentinel}`,
      method: "prompt-to-task"
    });
    cases.push(summarizeEntrypoint("command_debug_off", commandOff, {
      surface: "command",
      classification: "narrow_live_host_surface",
      sentinel: commandOffSentinel
    }));

    await config.update("debug", true, vscode.ConfigurationTarget.Global);
    const commandOnSentinel = "SENTINEL_CURSOR_COMMAND_DEBUG_ON_DO_NOT_DISPLAY";
    const commandOn = await vscode.commands.executeCommand("xiigen.smartSkills.select", {
      prompt: `Cursor command debug on proof fixture. ${commandOnSentinel}`,
      method: "prompt-to-task"
    });
    cases.push(summarizeEntrypoint("command_debug_on", commandOn, {
      surface: "command",
      classification: "narrow_live_host_surface",
      sentinel: commandOnSentinel
    }));

    const chatSentinel = "SENTINEL_CURSOR_CHAT_HANDLER_DO_NOT_DISPLAY";
    const chatInvocation = await vscode.commands.executeCommand("xiigenSmartSkills.__test.chatHandlerLevel", {
      prompt: `Cursor chat handler-level proof fixture. ${chatSentinel}`,
      method: "orchestration-plan"
    });
    cases.push(summarizeEntrypoint("chat_handler_level_debug_on", chatInvocation.result, {
      surface: "chat",
      classification: "handler/helper/static",
      sentinel: chatSentinel,
      markdown: chatInvocation.markdown
    }));

    await config.update("debug", false, vscode.ConfigurationTarget.Global);
    const panel = await vscode.commands.executeCommand("xiigenSmartSkills.panel");
    cases.push(summarizeEntrypoint("panel_command", panel, {
      surface: "panel",
      classification: "narrow_live_host_surface"
    }));

    const negativeSentinel = "SENTINEL_CURSOR_NEGATIVE_MISSING_LOCATOR_DO_NOT_DISPLAY";
    const negative = await withEnv({
      XIIGEN_TARGET_LOCATOR_REQUIRED: "true",
      XIIGEN_TARGET_LOCATOR_PATH: "",
      XIIGEN_TARGET_LOCATOR_JSON: "",
      XIIGEN_USER_ADAPTIVE_ASSET_ROOT: "",
      XIIGEN_ROLLBACK_ASSET_ROOT: "",
      XIIGEN_LOCATOR_MODE: ""
    }, async () => vscode.commands.executeCommand("xiigen.smartSkills.select", {
      prompt: `Cursor negative missing locator proof fixture. ${negativeSentinel}`,
      method: "prompt-to-task"
    }));
    cases.push(summarizeEntrypoint("negative_missing_locator", negative, {
      surface: "command",
      classification: "narrow_live_host_surface",
      sentinel: negativeSentinel,
      negative: true,
      expected_error: "target_locator_required"
    }));
  } finally {
    if (previousDebug === undefined) {
      await config.update("debug", undefined, vscode.ConfigurationTarget.Global);
    } else {
      await config.update("debug", previousDebug, vscode.ConfigurationTarget.Global);
    }
    if (previousLocatorPath === undefined) {
      await config.update("targetLocatorPath", undefined, vscode.ConfigurationTarget.Global);
    } else {
      await config.update("targetLocatorPath", previousLocatorPath, vscode.ConfigurationTarget.Global);
    }
    if (previousLocatorJson === undefined) {
      await config.update("targetLocatorJson", undefined, vscode.ConfigurationTarget.Global);
    } else {
      await config.update("targetLocatorJson", previousLocatorJson, vscode.ConfigurationTarget.Global);
    }
  }

  const publicChatInvocationCandidates = chatApiKeysBeforeActivation
    .filter((key) => /request|send|invoke|execute/iu.test(key));
  const chatParticipantRegistered = chatRegistrations.some((entry) => entry.id === "xiigen-smart-skills");
  const fullChatUiSafelyInvoked = false;
  const fullChatUiBlockedReason = publicChatInvocationCandidates.length > 0
    ? "A possible chat invocation API key was observed, but this bounded proof did not have a reviewed safe non-AI invocation contract for it; no Cursor AI/chat task-solving was run."
    : "No public noninteractive Cursor/VS Code API was exposed in this Extension Host session to send a request to the xiigen-smart-skills chat participant without entering Cursor AI/chat task-solving.";

  const packet = {
    schema: "xiigen.runtime_cursor_full_chat_ui_no_eval_proof.packet.v1",
    work_order_id: "RUNTIME-CURSOR-FULL-CHAT-UI-NO-EVAL-PROOF-01",
    run_id: runId,
    generated_utc: new Date().toISOString(),
    run_root: runRoot,
    evidence_root: evidenceRoot,
    app_name: vscode.env.appName,
    extension_host_started: true,
    extension_loaded: true,
    extension_activated: extension.isActive === true,
    extension_id: extension.id,
    extension_path: extension.extensionPath,
    package_debug_default_seen: debugInspect.defaultValue,
    evidence_classification: "narrow_live_host_surface",
    full_live_ui_chat_app_proof: false,
    full_chat_ui_safely_invoked: fullChatUiSafelyInvoked,
    external_ai_task_solving: false,
    external_ai_or_model_called: false,
    actual_eval_executed: false,
    training_started: false,
    dpo_created: false,
    checkpoint_locator_target_manifest_mutated: false,
    source_code_changed: false,
    git_writes_used: false,
    raw_private_local_content_read_or_copied: false,
    command_panel_chat_rows: cases,
    chat_participant_api_discovery: {
      vscode_chat_api_exists: !!chatApi,
      keys_before_activation: chatApiKeysBeforeActivation,
      create_chat_participant_intercepted: chatIntercepted,
      create_chat_participant_intercept_error: chatInterceptError,
      chat_participant_registered_observed: chatParticipantRegistered,
      chat_participant_registrations_observed: chatRegistrations,
      public_noninteractive_invocation_candidates: publicChatInvocationCandidates,
      command_path_used_as_full_chat_evidence: false,
      panel_path_used_as_full_chat_evidence: false,
      handler_level_path_used_as_full_chat_evidence: false,
      full_chat_ui_blocked_reason: fullChatUiBlockedReason
    },
    classification_reason: "Cursor Extension Host loaded and activated the development extension; command and panel entrypoints reached llm_mvp_core, and the chat handler-level adapter produced markdown, but a real Cursor chat participant UI request was not invoked noninteractively and is not claimed.",
    what_this_proves: [
      "Cursor Extension Host can load and activate the XIIGen extension in an isolated headless run.",
      "Command and panel entrypoints can create same-run request/response/core trace/host trace artifacts under the allowed proof folder.",
      "The explicit chat handler-level adapter can call the production chat handler and stream visible markdown text, but remains handler-level evidence.",
      "Debug off and debug on behavior were exercised.",
      "A missing target-locator negative path was exercised without external AI, eval, training, source changes, or git writes."
    ],
    what_this_does_not_prove: [
      "Full Cursor chat participant UI/app proof.",
      "Cursor AI/model task-solving behavior; intentionally not invoked.",
      "Token savings, quality improvement, semantic quality, learnability, target promotion, final readiness, or product readiness.",
      "Actual visual WebView panel inspection; only the panel command and returned display trace were observed noninteractively."
    ],
    human_board_ru: {
      vsego: "1 Cursor proof slice: Extension Host/command/panel/chat participant boundary without external AI/eval/training/private data.",
      sdelano: "Extension Host loaded/activated; command debug off/on, panel command, chat handler-level markdown, and missing-locator negative path were exercised with same-run trace refs.",
      seichas: "Classification is narrow_live_host_surface overall; chat row is handler/helper/static; full Cursor chat UI remains blocked.",
      ostalos: "Parent review should accept this packet or issue a repair work order only for a reviewed safe noninteractive chat participant invocation API if one is approved.",
      chto_zakryto: "Closed: host activation, command/panel host rows, handler-level chat row, debug off/on, negative path, correlation/freshness refs, no external AI/eval/training/source/git.",
      chto_ne_zakryto: "Not closed: full Cursor chat UI/app proof, actual Cursor AI chat participant request, visual WebView inspection, token/quality/semantic/learnability/final/product readiness."
    },
    next_action_if_accepted: "RUNTIME-CODEX-FULL-CHAT-UI-NO-EVAL-PROOF-01",
    repair_work_order_if_rejected: "RUNTIME-CURSOR-FULL-CHAT-UI-NO-EVAL-PROOF-REPAIR-01: run only a reviewed safe noninteractive Cursor chat participant invocation path if one is available without external AI/model task-solving; otherwise preserve FULL_CHAT_UI_BLOCKED_BY_API_RUNTIME_GAP and do not substitute command/panel/handler evidence for full chat UI proof."
  };

  packet.passed = cases.every((item) => item.passed === true)
    && packet.extension_activated === true
    && packet.external_ai_task_solving === false
    && packet.full_live_ui_chat_app_proof === false;
  packet.verdict = packet.passed
    ? "PASS_NARROW_LIVE_HOST_SURFACE_FULL_CHAT_UI_BLOCKED"
    : "FAIL_NEEDS_REPAIR";

  writeJson(reportPath, packet);
  if (!packet.passed) {
    process.exitCode = 1;
  }
}

module.exports = { run };