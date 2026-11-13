const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function writeJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function summarizeTabInput(input) {
  if (!input) {
    return null;
  }

  const summary = {
    constructorName: input.constructor && input.constructor.name ? input.constructor.name : null
  };

  if (input.uri && typeof input.uri.toString === "function") {
    summary.uri = input.uri.toString();
  }
  if (input.modified && input.modified.uri && typeof input.modified.uri.toString === "function") {
    summary.modifiedUri = input.modified.uri.toString();
  }
  if (input.original && input.original.uri && typeof input.original.uri.toString === "function") {
    summary.originalUri = input.original.uri.toString();
  }
  if (Object.prototype.hasOwnProperty.call(input, "viewType")) {
    summary.viewType = input.viewType;
  }
  if (Object.prototype.hasOwnProperty.call(input, "notebookType")) {
    summary.notebookType = input.notebookType;
  }

  return summary;
}

function summarizeTab(tab, index) {
  const input = summarizeTabInput(tab.input);
  return {
    index,
    label: tab.label,
    isActive: tab.isActive,
    isDirty: tab.isDirty,
    isPinned: tab.isPinned,
    input,
    viewType: input && Object.prototype.hasOwnProperty.call(input, "viewType") ? input.viewType : null
  };
}

function collectWindowDiagnostics() {
  const activeEditor = vscode.window.activeTextEditor;
  const diagnostics = {
    active_editor: activeEditor ? {
      document_file_name: activeEditor.document && activeEditor.document.fileName ? activeEditor.document.fileName : null,
      document_uri: activeEditor.document && activeEditor.document.uri ? activeEditor.document.uri.toString() : null,
      document_language_id: activeEditor.document ? activeEditor.document.languageId : null,
      document_is_dirty: activeEditor.document ? activeEditor.document.isDirty : null,
      view_column: activeEditor.viewColumn
    } : null,
    active_window_state: vscode.window.state ? {
      focused: vscode.window.state.focused,
      active: vscode.window.state.active
    } : null,
    tab_groups_available: !!(vscode.window.tabGroups && Array.isArray(vscode.window.tabGroups.all)),
    tab_groups: []
  };

  if (diagnostics.tab_groups_available) {
    diagnostics.tab_groups = vscode.window.tabGroups.all.map((group, groupIndex) => {
      const tabs = group.tabs.map((tab, tabIndex) => summarizeTab(tab, tabIndex));
      const activeTab = tabs.find((tab) => tab.isActive) || null;
      return {
        group_index: groupIndex,
        is_active: group.isActive,
        view_column: group.viewColumn,
        active_tab_label: activeTab ? activeTab.label : null,
        active_tab_input: activeTab ? activeTab.input : null,
        active_tab_view_type: activeTab ? activeTab.viewType : null,
        active_tab_is_active: activeTab ? activeTab.isActive : null,
        tabs
      };
    });
  }

  return diagnostics;
}

async function run() {
  const started = new Date();
  const reportPath = process.env.ROW12_PANEL_HOST_REPORT;
  const markerPath = process.env.ROW12_PANEL_READY_MARKER;
  const generatedRunnerHoldMs = Number("60000");
  const holdMsEnvRaw = process.env.ROW12_PANEL_HOLD_MS;
  const holdMs = holdMsEnvRaw ? Number(holdMsEnvRaw) : generatedRunnerHoldMs;
  if (!Number.isFinite(holdMs) || holdMs <= 5000) {
    throw new Error(`ROW12 panel hold must be explicitly propagated from the runner and greater than 5000ms; received ${holdMsEnvRaw || generatedRunnerHoldMs}`);
  }
  const holdMsSource = holdMsEnvRaw ? "ROW12_PANEL_HOLD_MS" : "generated_runner_hold_ms";
  const commandName = "xiigenSmartSkills.panel";
  const report = {
    schema: "xiigen.row12.cursor_panel.extension_host_open.v1",
    started_utc: started.toISOString(),
    host_kind: "Cursor",
    extension_development_host: true,
    command_invoked: commandName,
    targets_real_panel_command: true,
    test_only_handler_used: false,
    generic_chat_used: false,
    no_submit_performed: true,
    panel_title_expected: "XIIGen Smart Skills",
    prompt_textarea_selector_expected: "textarea#xiigen-smart-skills-prompt",
    submit_button_not_pressed: true,
    panel_hold_ms_generated_from_runner: generatedRunnerHoldMs,
    panel_hold_ms_env_raw: holdMsEnvRaw || null,
    panel_hold_ms_effective: holdMs,
    panel_hold_ms_source: holdMsSource,
    panel_hold_fallback_5000_used: false,
    product_readiness_claimed: false,
    errors: []
  };

  try {
    const extension =
      vscode.extensions.getExtension("xiigen.cursor-xiigen-smart-skills") ||
      vscode.extensions.getExtension("lubochka.cursor-xiigen-smart-skills");
    report.extension_visible_to_host = !!extension;
    if (!extension) {
      throw new Error("Cursor XIIGen development extension was not visible to the Extension Host.");
    }

    report.extension_id = extension.id;
    report.extension_path = extension.extensionPath;
    report.extension_was_active_before = extension.isActive;
    await extension.activate();
    report.extension_active_after_activate = extension.isActive;

    const result = await vscode.commands.executeCommand(commandName, {
      prompt: "",
      method: "skill-selection",
      requestedSkills: ["self-verification"],
      availableSkills: ["self-verification", "test-integrity", "output-skepticism", "implementation-integrity"],
      evidenceRoot: process.env.PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR,
      correlationId: "ROW13_3_LIVE_CURSOR_UI_DUPLICATE_SELF_VERIFICATION",
      redactedInput: {
        scenario: "row13_3_live_cursor_ui_duplicate",
        testedSkill: "self-verification",
        privateDataIncluded: false
      }
    });
    report.command_return_value = typeof result === "undefined" ? { type: "undefined" } : result;
    report.panelAction = result && Object.prototype.hasOwnProperty.call(result, "panelAction") ? result.panelAction : null;
    report.window_diagnostics_after_command = collectWindowDiagnostics();
    report.window_title_after_command = null;
    report.window_title_after_command_note = "The extension host API does not expose the native window title; the PowerShell runner records Cursor MainWindowTitle after the command.";
    report.command_resolved = true;
    report.command_result = result;
    report.open_only_result = result && result.panelOpenOnly === true;
    report.core_path_invoked = result && result.corePathInvoked === true;
    report.panel_submission_path_invoked = result && result.panelSubmissionPathInvoked === true;
    report.prompt_focus_requested = result && result.promptFocusRequested === true;
    report.no_submit_performed = report.core_path_invoked !== true && report.panel_submission_path_invoked !== true;
    report.ended_utc = new Date().toISOString();
    report.elapsed_ms = new Date(report.ended_utc).getTime() - started.getTime();
    writeJson(reportPath, report);
    writeJson(markerPath, {
      ready: true,
      ready_utc: new Date().toISOString(),
      command_invoked: commandName,
      no_submit_performed: report.no_submit_performed,
      panel_hold_ms_effective: holdMs,
      panel_hold_ms_source: holdMsSource,
      panel_hold_fallback_5000_used: false,
      report_path: reportPath
    });
    await new Promise((resolve) => setTimeout(resolve, holdMs));
  } catch (error) {
    report.command_resolved = false;
    report.errors.push({
      message: error && error.message ? error.message : String(error),
      stack: error && error.stack ? String(error.stack) : null
    });
    report.ended_utc = new Date().toISOString();
    report.elapsed_ms = new Date(report.ended_utc).getTime() - started.getTime();
    writeJson(reportPath, report);
    writeJson(markerPath, {
      ready: false,
      ready_utc: new Date().toISOString(),
      command_invoked: commandName,
      panel_hold_ms_effective: holdMs,
      panel_hold_ms_source: holdMsSource,
      panel_hold_fallback_5000_used: false,
      error: report.errors[0],
      report_path: reportPath
    });
    throw error;
  }
}

module.exports = { run };
