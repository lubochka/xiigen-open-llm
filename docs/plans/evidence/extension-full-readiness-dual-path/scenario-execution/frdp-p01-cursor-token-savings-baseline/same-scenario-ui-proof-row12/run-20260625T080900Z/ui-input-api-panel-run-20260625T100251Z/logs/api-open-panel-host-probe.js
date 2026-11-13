const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const probeDir = __dirname;
const packagePath = "C:\\Users\\lubao\\Documents\\xiigen\\extensions reasearch\\cursor-xiigen-smart-skills\\package.json";
const resultPath = path.join(probeDir, "api-open-panel-command-result.json");
const stopPath = path.join(probeDir, "api-open-panel-stop.txt");
const targetCommand = "xiigenSmartSkills.panel";

function writeJson(filePath, value) {
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

async function sleep(ms) {
  await new Promise((resolve) => setTimeout(resolve, ms));
}

async function run() {
  const startedAt = new Date().toISOString();
  const packageJson = JSON.parse(fs.readFileSync(packagePath, "utf8").replace(/^\uFEFF/, ""));
  const extensionId = `${packageJson.publisher}.${packageJson.name}`;
  const extension = vscode.extensions.getExtension(extensionId);
  const commandsBeforeActivation = await vscode.commands.getCommands(true);
  const activation = { attempted: false, succeeded: false, error: null };

  if (extension) {
    activation.attempted = true;
    try {
      await extension.activate();
      activation.succeeded = true;
    } catch (error) {
      activation.error = error && error.stack ? error.stack : String(error);
    }
  }

  await sleep(500);
  const commandsAfterActivation = await vscode.commands.getCommands(true);
  const apiCommand = {
    command: targetCommand,
    attempted: commandsAfterActivation.includes(targetCommand),
    executed: false,
    succeeded: false,
    error: null,
    started_at_utc: new Date().toISOString(),
    finished_at_utc: null
  };

  if (apiCommand.attempted) {
    try {
      await vscode.commands.executeCommand(targetCommand);
      apiCommand.executed = true;
      apiCommand.succeeded = true;
    } catch (error) {
      apiCommand.executed = true;
      apiCommand.error = error && error.stack ? error.stack : String(error);
    }
  }
  apiCommand.finished_at_utc = new Date().toISOString();

  writeJson(resultPath, {
    packet_type: "ROW12_API_OPEN_PANEL_COMMAND_EVIDENCE",
    checked_at_utc: new Date().toISOString(),
    package_extension_id: extensionId,
    extension_present: Boolean(extension),
    extension_summary: extension ? {
      id: extension.id,
      extensionPath: extension.extensionPath,
      isActive: extension.isActive,
      package_main: extension.packageJSON && extension.packageJSON.main,
      activationEvents: extension.packageJSON && extension.packageJSON.activationEvents
    } : null,
    activation,
    target_command: targetCommand,
    target_present_before_activation: commandsBeforeActivation.includes(targetCommand),
    target_present_after_activation: commandsAfterActivation.includes(targetCommand),
    api_command: apiCommand,
    forbidden_actions: {
      command_palette_used: false,
      row12_submit_run: false,
      login_or_browser_recovery_run: false,
      external_ai_used: false,
      git_used: false,
      readiness_claimed: false
    },
    timing: {
      started_at_utc: startedAt,
      finished_at_utc: new Date().toISOString()
    }
  });

  const deadline = Date.now() + 120000;
  while (Date.now() < deadline && !fs.existsSync(stopPath)) {
    await sleep(500);
  }
}

module.exports = { run };