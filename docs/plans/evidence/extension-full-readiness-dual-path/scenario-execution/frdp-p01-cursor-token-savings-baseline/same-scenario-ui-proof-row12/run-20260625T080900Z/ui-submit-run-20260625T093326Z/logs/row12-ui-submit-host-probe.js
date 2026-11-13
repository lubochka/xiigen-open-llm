const fs = require('fs');
const vscode = require('vscode');
const resultPath = "C:\\Projects\\llm_mvp_core\\docs\\plans\\evidence\\extension-full-readiness-dual-path\\scenario-execution\\frdp-p01-cursor-token-savings-baseline\\same-scenario-ui-proof-row12\\run-20260625T080900Z\\ui-submit-run-20260625T093326Z\\logs\\row12-ui-submit-command-result.json";
const stopPath = "C:\\Projects\\llm_mvp_core\\docs\\plans\\evidence\\extension-full-readiness-dual-path\\scenario-execution\\frdp-p01-cursor-token-savings-baseline\\same-scenario-ui-proof-row12\\run-20260625T080900Z\\ui-submit-run-20260625T093326Z\\logs\\row12-ui-submit-stop.txt";
const packagePath = "C:\\Users\\lubao\\Documents\\xiigen\\extensions reasearch\\cursor-xiigen-smart-skills\\package.json";
const targetCommand = 'xiigenSmartSkills.panel';
const scenario = "frdp-p01-cursor-token-savings-baseline";
const correlationId = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1-ui-submit-panel-webview-submit";
function writeJson(filePath, value) { fs.writeFileSync(filePath, JSON.stringify(value, null, 2) + '\n', 'utf8'); }
function readPackage() {
  const packageJson = JSON.parse(fs.readFileSync(packagePath, 'utf8').replace(/^\uFEFF/, ''));
  return { raw: packageJson, extensionId: packageJson.publisher + '.' + packageJson.name };
}
async function sleep(ms) { await new Promise((resolve) => setTimeout(resolve, ms)); }
async function run() {
  const startedAt = new Date().toISOString();
  const packageInfo = readPackage();
  const extension = vscode.extensions.getExtension(packageInfo.extensionId);
  const before = await vscode.commands.getCommands(true);
  const activation = { attempted: false, succeeded: false, error: null };
  if (extension) {
    activation.attempted = true;
    try { await extension.activate(); activation.succeeded = true; }
    catch (error) { activation.error = error && error.stack ? error.stack : String(error); }
  }
  await sleep(500);
  const after = await vscode.commands.getCommands(true);
  const apiCommand = { command: targetCommand, attempted: after.includes(targetCommand), executed: false, succeeded: false, error: null, started_at_utc: new Date().toISOString(), finished_at_utc: null };
  if (apiCommand.attempted) {
    try {
      await vscode.commands.executeCommand(targetCommand, {
        scenario,
        correlationId,
        prompt: 'A Cursor user opens the XIIGen Smart Skills panel and asks for thin-client host-runtime evidence.',
        method: 'prompt-to-task',
        redactedInput: { marker: "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1", scenario, prompt: '[REDACTED initial panel-open prompt; row12 prompt will be pasted manually]', privateDataIncluded: false },
        requestedSkills: ['prompt-to-task', 'task-to-plan', 'task-to-skills', 'evidence-ledger'],
        availableSkills: ['prompt-to-task', 'task-to-plan', 'task-to-skills', 'evidence-ledger']
      });
      apiCommand.executed = true; apiCommand.succeeded = true;
    } catch (error) { apiCommand.executed = true; apiCommand.error = error && error.stack ? error.stack : String(error); }
  }
  apiCommand.finished_at_utc = new Date().toISOString();
  writeJson(resultPath, { packet_type: 'ROW12_UI_SUBMIT_API_OPEN_COMMAND_EVIDENCE', checked_at_utc: new Date().toISOString(), package_extension_id: packageInfo.extensionId, extension_present: Boolean(extension), extension_summary: extension ? { id: extension.id, extensionPath: extension.extensionPath, isActive: extension.isActive, package_main: extension.packageJSON && extension.packageJSON.main, activationEvents: extension.packageJSON && extension.packageJSON.activationEvents } : null, activation, target_command: targetCommand, target_present_before_activation: before.includes(targetCommand), target_present_after_activation: after.includes(targetCommand), api_command: apiCommand, scenario, correlationId, forbidden_actions: { command_palette_used: false, login_or_browser_recovery_run: false, external_ai_used: false, git_used: false, readiness_claimed: false }, timing: { started_at_utc: startedAt, finished_at_utc: new Date().toISOString() } });
  const deadline = Date.now() + 180000;
  while (Date.now() < deadline && !fs.existsSync(stopPath)) { await sleep(500); }
}
module.exports = { run };
