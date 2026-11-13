const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const probeDir = __dirname;
const packagePath = "C:\\Users\\lubao\\Documents\\xiigen\\extensions reasearch\\cursor-xiigen-smart-skills\\package.json";
const activationPath = path.join(probeDir, "panel-activation-wait-result.json");
const requestPath = path.join(probeDir, "panel-api-open-request.json");
const responsePath = path.join(probeDir, "panel-api-open-result.json");
const stopPath = path.join(probeDir, "panel-activation-wait-stop.txt");
const targetCommand = "xiigenSmartSkills.panel";

function writeJson(filePath, value) {
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function readPackage() {
  const packageJson = JSON.parse(fs.readFileSync(packagePath, "utf8").replace(/^\uFEFF/, ""));
  return {
    raw: packageJson,
    extensionId: `${packageJson.publisher}.${packageJson.name}`
  };
}

async function sleep(ms) {
  await new Promise((resolve) => setTimeout(resolve, ms));
}

async function run() {
  const startedAt = new Date().toISOString();
  const packageInfo = readPackage();
  const extension = vscode.extensions.getExtension(packageInfo.extensionId);
  const beforeCommands = await vscode.commands.getCommands(true);
  const activation = {
    attempted: false,
    succeeded: false,
    error: null
  };

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
  const afterCommands = await vscode.commands.getCommands(true);
  writeJson(activationPath, {
    packet_type: "ROW12_PANEL_ACTIVATION_WAIT_RESULT",
    checked_at_utc: new Date().toISOString(),
    package_extension_id: packageInfo.extensionId,
    extension_present: Boolean(extension),
    activation,
    target_command: targetCommand,
    target_present_before_activation: beforeCommands.includes(targetCommand),
    target_present_after_activation: afterCommands.includes(targetCommand),
    command_fragments_after_activation: afterCommands.filter((command) => command.toLowerCase().includes("xiigen") || command.toLowerCase().includes("skill") || command.toLowerCase().includes("panel")).sort(),
    forbidden_actions: {
      row12_submit_run: false,
      login_or_browser_recovery_run: false,
      external_ai_used: false,
      git_used: false,
      readiness_claimed: false
    },
    timing: {
      started_at_utc: startedAt,
      activated_at_utc: new Date().toISOString()
    }
  });

  const deadline = Date.now() + 120000;
  let apiOpenHandled = false;
  while (Date.now() < deadline) {
    if (fs.existsSync(stopPath)) {
      break;
    }

    if (!apiOpenHandled && fs.existsSync(requestPath)) {
      apiOpenHandled = true;
      const openedAt = new Date().toISOString();
      const result = {
        packet_type: "ROW12_PANEL_API_OPEN_RESULT",
        requested_at_utc: openedAt,
        command: targetCommand,
        executed: false,
        succeeded: false,
        error: null
      };
      try {
        await vscode.commands.executeCommand(targetCommand);
        result.executed = true;
        result.succeeded = true;
      } catch (error) {
        result.executed = true;
        result.error = error && error.stack ? error.stack : String(error);
      }
      result.finished_at_utc = new Date().toISOString();
      writeJson(responsePath, result);
    }

    await sleep(500);
  }
}

module.exports = { run };
