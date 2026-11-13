const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const probeDir = __dirname;
const packagePath = "C:\\Users\\lubao\\Documents\\xiigen\\extensions reasearch\\cursor-xiigen-smart-skills\\package.json";
const jsonPath = path.join(probeDir, "extension-host-api-probe.json");
const mdPath = path.join(probeDir, "extension-host-api-probe.md");
const targetCommand = "xiigenSmartSkills.panel";

function readPackage() {
  const packageJson = JSON.parse(fs.readFileSync(packagePath, "utf8").replace(/^\uFEFF/, ""));
  return {
    raw: packageJson,
    extensionId: `${packageJson.publisher}.${packageJson.name}`
  };
}

function summarizeExtension(extension) {
  if (!extension) {
    return null;
  }

  return {
    id: extension.id,
    extensionPath: extension.extensionPath,
    isActive: extension.isActive,
    packageJSON: {
      name: extension.packageJSON && extension.packageJSON.name,
      publisher: extension.packageJSON && extension.packageJSON.publisher,
      displayName: extension.packageJSON && extension.packageJSON.displayName,
      version: extension.packageJSON && extension.packageJSON.version,
      main: extension.packageJSON && extension.packageJSON.main,
      activationEvents: extension.packageJSON && extension.packageJSON.activationEvents,
      contributesCommands: extension.packageJSON && extension.packageJSON.contributes && extension.packageJSON.contributes.commands
    }
  };
}

function commandFragments(commands) {
  const fragments = ["xiigen", "smart", "skill", "skills", "panel", "open"];
  return commands
    .filter((command) => {
      const lower = command.toLowerCase();
      return fragments.some((fragment) => lower.includes(fragment));
    })
    .sort();
}

function titleishFragmentsFromExtensions(extensions) {
  const rows = [];
  for (const extension of extensions) {
    const contributed = extension.packageJSON && extension.packageJSON.contributes && extension.packageJSON.contributes.commands;
    if (!Array.isArray(contributed)) {
      continue;
    }
    for (const command of contributed) {
      const commandId = String(command.command || "");
      const title = String(command.title || "");
      const category = String(command.category || "");
      const haystack = `${commandId} ${title} ${category}`.toLowerCase();
      if (haystack.includes("xiigen") || haystack.includes("smart") || haystack.includes("skill") || haystack.includes("panel")) {
        rows.push({
          extensionId: extension.id,
          command: commandId,
          title,
          category
        });
      }
    }
  }
  return rows;
}

function matchingExtensions(extensions, extensionId, packageJson) {
  const wantedId = extensionId.toLowerCase();
  const wantedName = String(packageJson.name || "").toLowerCase();
  const wantedPublisher = String(packageJson.publisher || "").toLowerCase();
  return extensions
    .filter((extension) => {
      const extensionName = String(extension.packageJSON && extension.packageJSON.name || "").toLowerCase();
      const publisher = String(extension.packageJSON && extension.packageJSON.publisher || "").toLowerCase();
      const displayName = String(extension.packageJSON && extension.packageJSON.displayName || "").toLowerCase();
      return extension.id.toLowerCase() === wantedId
        || extensionName === wantedName
        || (extensionName.includes("xiigen") || displayName.includes("xiigen"))
        || (publisher === wantedPublisher && wantedPublisher.length > 0);
    })
    .map(summarizeExtension);
}

function statusFor(extensionPresent, commandPresent) {
  if (extensionPresent && commandPresent) {
    return "API_PROBE_EXTENSION_AND_COMMAND_PRESENT";
  }
  if (extensionPresent) {
    return "API_PROBE_EXTENSION_PRESENT_COMMAND_MISSING";
  }
  return "API_PROBE_EXTENSION_NOT_PRESENT";
}

function writeOutputs(result) {
  fs.mkdirSync(probeDir, { recursive: true });
  fs.writeFileSync(jsonPath, `${JSON.stringify(result, null, 2)}\n`, "utf8");
  const md = [
    "# Cursor Extension Host API Probe",
    "",
    `status: ${result.status}`,
    `checked_at_utc: ${result.checked_at_utc}`,
    `extension_id_checked: ${result.extension_id_checked}`,
    `extension_present: ${result.api_probe_result.extension_present}`,
    `target_command: ${targetCommand}`,
    `target_command_present_after_activation: ${result.api_probe_result.commands_after_activation.target_present}`,
    "",
    "## Activation",
    "",
    `attempted: ${result.api_probe_result.activation.attempted}`,
    `succeeded: ${result.api_probe_result.activation.succeeded}`,
    `error: ${result.api_probe_result.activation.error || ""}`,
    "",
    "## What This Proves",
    "",
    result.what_was_proven,
    "",
    "## What This Does Not Prove",
    "",
    result.what_is_not_proven,
    ""
  ].join("\n");
  fs.writeFileSync(mdPath, md, "utf8");
}

async function run() {
  const startedAt = new Date().toISOString();
  const packageInfo = readPackage();
  const allBefore = vscode.extensions.all;
  const extensionBefore = vscode.extensions.getExtension(packageInfo.extensionId);
  const commandsBefore = await vscode.commands.getCommands(true);
  const activation = {
    attempted: false,
    succeeded: false,
    error: null
  };

  if (extensionBefore) {
    activation.attempted = true;
    try {
      await extensionBefore.activate();
      activation.succeeded = true;
    } catch (error) {
      activation.error = error && error.stack ? error.stack : String(error);
    }
  }

  await new Promise((resolve) => setTimeout(resolve, 500));

  const allAfter = vscode.extensions.all;
  const extensionAfter = vscode.extensions.getExtension(packageInfo.extensionId);
  const commandsAfter = await vscode.commands.getCommands(true);
  const extensionPresent = Boolean(extensionAfter);
  const commandPresent = commandsAfter.includes(targetCommand);
  const result = {
    packet_type: "CURSOR_EXTENSION_HOST_API_PROBE_PACKET",
    status: statusFor(extensionPresent, commandPresent),
    checked_at_utc: new Date().toISOString(),
    run_folder: path.dirname(probeDir),
    probe_dir: probeDir,
    package_path: packagePath,
    extension_id_checked: packageInfo.extensionId,
    target_command: targetCommand,
    api_probe_result: {
      package_extension_id: packageInfo.extensionId,
      extension_present: extensionPresent,
      extension_before_activation: summarizeExtension(extensionBefore),
      extension_after_activation: summarizeExtension(extensionAfter),
      matching_extensions_before_activation: matchingExtensions(allBefore, packageInfo.extensionId, packageInfo.raw),
      matching_extensions_after_activation: matchingExtensions(allAfter, packageInfo.extensionId, packageInfo.raw),
      commands_before_activation: {
        total_count: commandsBefore.length,
        target_present: commandsBefore.includes(targetCommand),
        matching_command_ids: commandFragments(commandsBefore)
      },
      activation,
      commands_after_activation: {
        total_count: commandsAfter.length,
        target_present: commandPresent,
        matching_command_ids: commandFragments(commandsAfter)
      },
      contributed_titleish_fragments_before_activation: titleishFragmentsFromExtensions(allBefore),
      contributed_titleish_fragments_after_activation: titleishFragmentsFromExtensions(allAfter)
    },
    forbidden_actions: {
      row12_submit_run: false,
      panel_command_executed: false,
      login_or_browser_recovery_run: false,
      external_ai_used: false,
      git_used: false,
      readiness_claimed: false
    },
    what_was_proven: extensionPresent
      ? "The Cursor/VS Code extension-host API can see the development extension by package id. Command presence is reported separately from the same API run."
      : "The Cursor/VS Code extension-host API did not expose the development extension by package id in this test run.",
    what_is_not_proven: "This is not row12 submit, not manual UI proof, not login/browser recovery, not product readiness, and not evidence that the panel can submit a prompt.",
    timing: {
      started_at_utc: startedAt,
      finished_at_utc: new Date().toISOString()
    }
  };

  writeOutputs(result);
}

module.exports = { run };
