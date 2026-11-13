"use strict";

const assert = require("assert");
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const scenarioId = "frdp-p01-cursor-full-extension-entrypoint";
const marker = "FRDP-P01-frdp-p01-cursor-full-extension-entrypoint";
const panelCommand = "xiigenSmartSkills.panel";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const coreApiScript = "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1";
const scenarioRoot = __dirname;
const correlationId = `${marker}-extension-host-no-send`;

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, ""));
}

function writeJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function errorDetails(error) {
  return {
    name: error && error.name ? String(error.name) : null,
    message: error && error.message ? String(error.message) : String(error),
    stack: error && error.stack ? String(error.stack) : null
  };
}

function rel(filePath) {
  return path.relative(coreRoot, filePath).replace(/\\/g, "/");
}

function getExtension() {
  return vscode.extensions.getExtension("xiigen.cursor-xiigen-smart-skills")
    || vscode.extensions.getExtension("lubochka.cursor-xiigen-smart-skills");
}

async function setConfig(name, value) {
  const config = vscode.workspace.getConfiguration("xiigenSmartSkills");
  await config.update(name, value, vscode.ConfigurationTarget.Global);
}

function fieldResult(field, expected, actual) {
  return {
    field,
    expected,
    actual,
    passed: actual === expected
  };
}

function validateHostPanelUiRef(panelArtifact, result) {
  const visibleText = typeof panelArtifact.visibleText === "string" ? panelArtifact.visibleText : "";
  const fieldResults = [
    fieldResult("schemaVersion", 1, panelArtifact.schemaVersion),
    fieldResult("hostKind", "Cursor", panelArtifact.hostKind),
    fieldResult("panelUiCreated", true, panelArtifact.panelUiCreated),
    fieldResult("panelTitle", "XIIGen Smart Skills", panelArtifact.panelTitle),
    fieldResult("correlationId", correlationId, panelArtifact.correlationId),
    fieldResult("runtimeProofScenario", "positive_request", panelArtifact.runtimeProofScenario),
    fieldResult("marker", marker, panelArtifact.marker),
    fieldResult("sameMarkerInPrompt", true, panelArtifact.sameMarkerInPrompt),
    fieldResult("sameMarkerInCorrelation", true, panelArtifact.sameMarkerInCorrelation),
    fieldResult("panelCommandAcceptedRuntimeProofInput", true, panelArtifact.panelCommandAcceptedRuntimeProofInput),
    fieldResult("sameSharedRequestPathAsRuntimeProofInvoke", true, panelArtifact.sameSharedRequestPathAsRuntimeProofInvoke),
    fieldResult("semanticQualityClaimed", false, panelArtifact.semanticQualityClaimed),
    fieldResult("externalAiOrUnrelatedToolUsed", false, panelArtifact.externalAiOrUnrelatedToolUsed),
    fieldResult("responseRef", result.coreResponseRef, panelArtifact.responseRef),
    fieldResult("traceRef", result.hostTraceRef, panelArtifact.traceRef),
    fieldResult("visibleTextNonEmpty", true, visibleText.trim().length > 0)
  ];
  const failures = fieldResults.filter((item) => !item.passed);
  assert.strictEqual(failures.length, 0, `hostPanelUiRef validation failed: ${JSON.stringify(failures)}`);
  return fieldResults;
}

async function run() {
  const outputRoot = process.env.FRDP_P01_CURSOR_PANEL_UI_PROOF_DIR
    || path.join(scenarioRoot, "panel-ui-proof");
  const evidenceRoot = process.env.FRDP_P01_CURSOR_PANEL_UI_EVIDENCE_ROOT
    || path.join(scenarioRoot, "extension-host-proof", "extension-host-evidence");
  const validationJsonPath = path.join(outputRoot, "frdp-p01-cursor-panel-ui-proof-validation.json");
  const harnessStartedJsonPath = path.join(outputRoot, "frdp-p01-cursor-panel-ui-proof-harness-started.json");
  const harnessFailureJsonPath = path.join(outputRoot, "frdp-p01-cursor-panel-ui-proof-harness-failure.json");
  ensureDir(outputRoot);
  ensureDir(evidenceRoot);

  writeJson(harnessStartedJsonPath, {
    schema_version: "frdp-p01-cursor-panel-ui-proof-harness-started/v1",
    started_utc: new Date().toISOString(),
    current_task: "Validate only xiigenSmartSkills.panel hostPanelUiRef fields for FRDP-P01 without a readiness claim.",
    not_the_task: "Cursor product readiness, full UI screenshot proof, online prompt send, stage, commit, or push.",
    command_invoked: panelCommand,
    payload_correlation_id: correlationId,
    payload_marker: marker,
    payload_evidence_root: rel(evidenceRoot),
    validation_json_ref: rel(validationJsonPath),
    product_readiness_claimed: false,
    online_prompt_or_message_sent_by_harness: false
  });

  try {
    const extension = getExtension();
    assert(extension, "Cursor XIIGen development extension was not found.");
    await extension.activate();

    await setConfig("coreRoot", coreRoot);
    await setConfig("coreApiScript", coreApiScript);
    await setConfig("debug", false);

    const prompt = [
      "Plan one bounded product-readiness scenario execution for the Cursor extension entrypoint.",
      `Scenario id: ${scenarioId}.`,
      `Marker: ${marker}.`,
      "Keep host logic thin, route through llm_mvp_core, preserve evidence links, and do not claim readiness without joined local desktop proof.",
      "This is a local no-send panel hostPanelUiRef proof; do not use external AI."
    ].join(" ");

    const payload = {
      scenario: "positive_request",
      correlationId,
      prompt,
      evidenceRoot,
      method: "orchestration-plan",
      requestedSkills: ["extension-runtime", "plan-review", "runtime-QA", "test-integrity"],
      availableSkills: ["authority-governance", "extension-runtime", "plan-review", "privacy-redaction", "runtime-QA", "shortcut-audit", "test-integrity"],
      redactedInput: {
        scenario_id: scenarioId,
        marker,
        prompt_private_data_included: false,
        no_send_panel_ui_ref_proof: true
      }
    };

    const result = await vscode.commands.executeCommand(panelCommand, payload);
    assert(result, "panel command returned no result");
    assert.strictEqual(typeof result.hostPanelUiRef, "string", "hostPanelUiRef ref missing");
    assert(fs.existsSync(result.hostPanelUiRef), `hostPanelUiRef file missing: ${result.hostPanelUiRef}`);

    const panelArtifact = readJson(result.hostPanelUiRef);
    const fieldResults = validateHostPanelUiRef(panelArtifact, result);

    writeJson(validationJsonPath, {
      schema_version: "frdp-p01-cursor-panel-ui-proof-validation/v1",
      status: "validated",
      current_task: "Validate only xiigenSmartSkills.panel hostPanelUiRef fields for FRDP-P01 without a readiness claim.",
      not_the_task: "Cursor product readiness, full UI screenshot proof, online prompt send, stage, commit, or push.",
      command_invoked: panelCommand,
      payload_correlation_id: correlationId,
      payload_marker: marker,
      payload_evidence_root: rel(evidenceRoot),
      host_panel_ui_ref: rel(result.hostPanelUiRef),
      validation_json_ref: rel(validationJsonPath),
      field_results: fieldResults,
      all_passed: true,
      product_readiness_claimed: false,
      online_prompt_or_message_sent_by_harness: false
    });
  } catch (error) {
    writeJson(harnessFailureJsonPath, {
      schema_version: "frdp-p01-cursor-panel-ui-proof-harness-failure/v1",
      failed_utc: new Date().toISOString(),
      current_task: "Validate only xiigenSmartSkills.panel hostPanelUiRef fields for FRDP-P01 without a readiness claim.",
      not_the_task: "Cursor product readiness, full UI screenshot proof, online prompt send, stage, commit, or push.",
      command_invoked: panelCommand,
      payload_correlation_id: correlationId,
      payload_marker: marker,
      payload_evidence_root: rel(evidenceRoot),
      validation_json_ref: rel(validationJsonPath),
      error: errorDetails(error),
      product_readiness_claimed: false,
      online_prompt_or_message_sent_by_harness: false
    });
    throw error;
  }
}

module.exports = { run };
