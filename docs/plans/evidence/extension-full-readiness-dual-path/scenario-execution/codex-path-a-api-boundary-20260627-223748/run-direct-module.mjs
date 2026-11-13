import fs from 'node:fs';
import path from 'node:path';
import { invokeXiigenCore } from 'file:///C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/src/codex-entrypoint.js';

const evidenceDir = 'C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/codex-path-a-api-boundary-20260627-223748';
const scenarioPath = 'C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/scenario-prep/codex-full-extension-runtime-20260627-090817/scenario.json';
const scenario = JSON.parse(fs.readFileSync(scenarioPath, 'utf8'));
const correlationId = 'SCENARIO-COVERAGE-CODEX-PATH-A-DIRECT-20260627-223748';

const result = invokeXiigenCore({
  method: 'prompt-to-task',
  userPromptOrTaskText: scenario.exactPrompt,
  hostFacts: {
    surface: 'installed-codex-skill',
    scope: 'scenario-coverage-path-a',
    scenarioId: scenario.scenarioId,
    path: 'Path A',
    samePrompt: true,
    invocation: 'direct exported adapter function to avoid CLI quote loss'
  },
  requestedSkills: ['xiigen-smart-skills'],
  availableSkills: ['xiigen-smart-skills'],
  checkSetInputs: {
    scenarioCoverage: 'Path A',
    samePrompt: true,
    scenarioId: scenario.scenarioId,
    requestedProof: 'Codex API/boundary only',
    invocation: 'direct exported adapter function'
  },
  provenance: {
    source: 'scenario-prep',
    scenarioPath,
    plugin: 'codex-xiigen-smart-skills',
    path: 'Path A',
    invocation: 'direct exported adapter function'
  },
  correlationId,
  debugEnabled: true,
  coreRoot: 'C:/Projects/llm_mvp_core',
  extensionRoot: 'C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500',
  requestPath: path.join(evidenceDir, 'request-direct.json'),
  responsePath: path.join(evidenceDir, 'response-direct.json'),
  tracePath: path.join(evidenceDir, 'trace-direct.json')
});

fs.writeFileSync(path.join(evidenceDir, 'direct-module-result.json'), JSON.stringify({
  success: result.response?.Success === true,
  status: result.response?.Status ?? null,
  correlationId,
  requestPath: path.join(evidenceDir, 'request-direct.json'),
  responsePath: path.join(evidenceDir, 'response-direct.json'),
  tracePath: path.join(evidenceDir, 'trace-direct.json'),
  boundary: 'Path A only; Path B local-agent UI path remains not_started'
}, null, 2), 'utf8');

console.log(JSON.stringify({ ok: true, correlationId, status: result.response?.Status ?? null }));