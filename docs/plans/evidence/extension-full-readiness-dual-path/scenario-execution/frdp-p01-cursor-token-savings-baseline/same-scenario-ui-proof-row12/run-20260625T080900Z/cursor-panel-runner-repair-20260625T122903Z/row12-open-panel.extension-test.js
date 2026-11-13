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

async function run() {
  const started = new Date();
  const reportPath = process.env.ROW12_PANEL_HOST_REPORT;
  const markerPath = process.env.ROW12_PANEL_READY_MARKER;
  const holdMs = Number(process.env.ROW12_PANEL_HOLD_MS || "120000");
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

    const result = await vscode.commands.executeCommand(commandName);
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
      error: report.errors[0],
      report_path: reportPath
    });
    throw error;
  }
}

module.exports = { run };
