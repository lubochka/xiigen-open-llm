import fs from 'node:fs';
import { spawnSync } from 'node:child_process';
import { pathToFileURL } from 'node:url';

function readJsonIfPresent(filePath) {
  if (!fs.existsSync(filePath)) {
    return null;
  }

  return JSON.parse(fs.readFileSync(filePath, 'utf8').replace(/^\uFEFF/, ''));
}

function formatCommand(args) {
  return `node ${args.map((arg) => /[\s"`{}\[\],:]/u.test(arg) ? `"${arg.replaceAll('"', '\\"')}"` : arg).join(' ')}`;
}

const optionsPath = process.argv[2];
const options = JSON.parse(fs.readFileSync(optionsPath, 'utf8').replace(/^\uFEFF/, ''));
const args = [
  options.entrypointPath,
  '--method', options.method,
  '--user-prompt-or-task-text', options.promptText,
  '--host-facts-json', JSON.stringify(options.hostFacts),
  '--requested-skills-json', JSON.stringify(options.requestedSkills),
  '--available-skills-json', JSON.stringify(options.availableSkills),
  '--check-set-inputs-json', JSON.stringify(options.checkSetInputs),
  '--provenance-json', JSON.stringify(options.provenance),
  '--correlation-id', options.correlationId,
  '--debug-enabled',
  '--core-root', options.coreRoot,
  '--extension-root', '.',
  '--request-path', options.requestPath,
  '--response-path', options.responsePath,
  '--trace-path', options.tracePath
];

const child = spawnSync('node', args, {
  cwd: options.pluginRoot,
  encoding: 'utf8',
  windowsHide: true
});
const request = readJsonIfPresent(options.requestPath);
const response = readJsonIfPresent(options.responsePath);
const trace = readJsonIfPresent(options.tracePath);
let adapterVisibleAnswerDisplay = null;
let adapterDisplayError = null;

try {
  const entrypoint = await import(pathToFileURL(options.entrypointPath).href);
  adapterVisibleAnswerDisplay = entrypoint.createVisibleAnswerDisplay({
    method: options.method,
    response,
    exitCode: child.status,
    stderr: child.stderr,
    debugEnabled: true
  });
} catch (error) {
  adapterDisplayError = error instanceof Error ? error.message : String(error);
}

const driverResult = {
  name: options.name,
  exit_code: child.status,
  signal: child.signal,
  plugin_root: options.pluginRoot,
  entrypoint_path: options.entrypointPath,
  child_command: formatCommand(args),
  stdout: child.stdout ?? '',
  stderr: child.stderr ?? '',
  request_path: options.requestPath,
  response_path: options.responsePath,
  trace_path: options.tracePath,
  request,
  response,
  trace,
  adapter_visible_answer_display: adapterVisibleAnswerDisplay,
  adapter_display_error: adapterDisplayError
};

fs.writeFileSync(options.resultPath, `${JSON.stringify(driverResult, null, 2)}\n`, 'utf8');

if (child.stdout) {
  process.stdout.write(child.stdout);
}

if (child.stderr) {
  process.stderr.write(child.stderr);
}

process.exitCode = typeof child.status === 'number' ? child.status : 1;
