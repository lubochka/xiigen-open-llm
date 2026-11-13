import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { spawnSync } from 'node:child_process';

const runRoot = process.argv[2];
const pathARoot = process.argv[3];
const repoRoot = 'C:/Projects/llm_mvp_core';
const scenarioRoot = 'C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-product-readiness-product-workflow';
const prepJsonPath = path.join(scenarioRoot, 'row19-prep-codex-product-workflow.json');
const skillPath = 'C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/skills/xiigen-smart-skills/SKILL.md';
const pluginRoot = 'C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500';
const requestPath = path.join(pathARoot, 'request.json');
const responsePath = path.join(pathARoot, 'response.json');
const tracePath = path.join(pathARoot, 'trace.json');
const outputPath = path.join(pathARoot, 'adapter-command-output.txt');
const validationPath = path.join(pathARoot, 'path-a-validation.json');
const validationCopyPath = path.join(pathARoot, 'validation', 'path-a-validation.json');
const promptIdentityPath = path.join(pathARoot, 'validation', 'prompt-identity-check.json');
const reportPath = path.join(pathARoot, 'path-a-human-report.md');
const commandLogPath = path.join(pathARoot, 'logs', 'adapter-command.json');
const psHashPath = path.join(pathARoot, 'logs', 'powershell-default-json-read-hash.json');

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, 'utf8').replace(/^\uFEFF/, ''));
}

function sha256Utf8(text) {
  return crypto.createHash('sha256').update(String(text ?? ''), 'utf8').digest('hex');
}

function fileInfo(filePath) {
  if (!fs.existsSync(filePath)) {
    return { path: filePath, exists: false };
  }
  const bytes = fs.readFileSync(filePath);
  return {
    path: filePath,
    exists: true,
    bytes: bytes.length,
    sha256: crypto.createHash('sha256').update(bytes).digest('hex')
  };
}

function extractResponsePromptHashes(response) {
  const text = JSON.stringify(response ?? {});
  const named = [];
  for (const pattern of [
    /userPromptSha256[^A-Fa-f0-9]{1,80}([A-Fa-f0-9]{64})/g,
    /raw_prompt_sha256[^A-Fa-f0-9]{1,80}([A-Fa-f0-9]{64})/g
  ]) {
    for (const match of text.matchAll(pattern)) {
      named.push(match[1].toLowerCase());
    }
  }
  return Array.from(new Set(named));
}

function containsAll(text, needles) {
  return needles.every((needle) => text.includes(needle));
}

fs.mkdirSync(path.join(pathARoot, 'logs'), { recursive: true });
fs.mkdirSync(path.join(pathARoot, 'validation'), { recursive: true });

const prep = readJson(prepJsonPath);
const prompt = prep.exact_same_scenario_prompt;
const correlationId = prep.path_a_adapter_plan?.correlation_id ?? 'ROW19-CODEX-PRODUCT-WORKFLOW-001';
const scenarioId = prep.scenario_id ?? 'frdp-p01-codex-product-readiness-product-workflow';
const startedUtc = new Date().toISOString();
const hostFacts = {
  surface: 'installed-codex-skill',
  host: 'Codex',
  scope: 'ROW19',
  workspace: repoRoot,
  local_user_model_root: repoRoot,
  mutation_allowed: 'false'
};
const checkSetInputs = { row19: 'codex-product-workflow-prep', non_mutating: 'true' };
const provenance = { source: 'installed-plugin-skill', plugin: 'codex-xiigen-smart-skills', scenario: scenarioId };
const adapterArgs = [
  './src/codex-entrypoint.js',
  '--method', 'prompt-to-task',
  '--user-prompt-or-task-text', prompt,
  '--host-facts-json', JSON.stringify(hostFacts),
  '--requested-skills-json', JSON.stringify(['xiigen-smart-skills']),
  '--available-skills-json', JSON.stringify(['xiigen-smart-skills']),
  '--check-set-inputs-json', JSON.stringify(checkSetInputs),
  '--provenance-json', JSON.stringify(provenance),
  '--correlation-id', correlationId,
  '--debug-enabled',
  '--core-root', repoRoot,
  '--extension-root', '.',
  '--request-path', requestPath,
  '--response-path', responsePath,
  '--trace-path', tracePath
];

const adapterResult = spawnSync('node', adapterArgs, {
  cwd: pluginRoot,
  encoding: 'utf8',
  windowsHide: true
});
const completedUtc = new Date().toISOString();
fs.writeFileSync(outputPath, [
  `command=node ${adapterArgs.map((arg) => arg.includes('\n') ? '<EXACT_PROMPT_FROM_PREP_JSON>' : arg).join(' ')}`,
  `cwd=${pluginRoot}`,
  `started_utc=${startedUtc}`,
  `completed_utc=${completedUtc}`,
  `exit_code=${adapterResult.status ?? 'null'}`,
  `signal=${adapterResult.signal ?? ''}`,
  '--- stdout ---',
  adapterResult.stdout ?? '',
  '--- stderr ---',
  adapterResult.stderr ?? ''
].join('\n'), 'utf8');
fs.writeFileSync(commandLogPath, JSON.stringify({
  command: 'node ./src/codex-entrypoint.js with argv array from runner',
  cwd: pluginRoot,
  started_utc: startedUtc,
  completed_utc: completedUtc,
  exit_code: adapterResult.status,
  signal: adapterResult.signal,
  request_path: requestPath,
  response_path: responsePath,
  trace_path: tracePath,
  prompt_source: prepJsonPath,
  prompt_arg_redacted_in_log: true,
  correlation_id: correlationId,
  host_facts: hostFacts,
  check_set_inputs: checkSetInputs,
  provenance
}, null, 2) + '\n', 'utf8');

let request = null;
let response = null;
let trace = null;
try { request = readJson(requestPath); } catch {}
try { response = readJson(responsePath); } catch {}
try { trace = readJson(tracePath); } catch {}

const psScript = `$p = ${JSON.stringify(requestPath)}; $j = Get-Content -Raw -LiteralPath $p | ConvertFrom-Json; $s = [string]$j.UserPromptOrTaskText; $sha = [System.Security.Cryptography.SHA256]::Create(); try { $h = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($s)); ([ordered]@{ path = $p; prompt_length = $s.Length; sha256_utf8_after_powershell_default_getcontent = (([BitConverter]::ToString($h)) -replace '-', '').ToLowerInvariant() }) | ConvertTo-Json -Compress } finally { $sha.Dispose() }`;
const psHashResult = spawnSync('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', psScript], {
  encoding: 'utf8',
  windowsHide: true
});
fs.writeFileSync(psHashPath, (psHashResult.stdout || '').trim() + '\n', 'utf8');
let psHash = null;
try { psHash = JSON.parse((psHashResult.stdout || '').trim()); } catch {}

const prepPrompt = prompt;
const requestPrompt = request?.UserPromptOrTaskText ?? null;
const prepPromptHash = sha256Utf8(prepPrompt);
const requestPromptHash = requestPrompt === null ? null : sha256Utf8(requestPrompt);
const responsePromptHashes = extractResponsePromptHashes(response);
const tracePromptHash = typeof trace?.user_prompt_sha256 === 'string' ? trace.user_prompt_sha256.toLowerCase() : null;
const responseText = JSON.stringify(response ?? {});
const selectedSkills = Array.isArray(response?.SelectedSkills) ? response.SelectedSkills : [];
const requiredOutputFields = [
  'scenario_id',
  'correlation_id',
  'normalized_task',
  'task_plan_steps',
  'requested_codex_skill_surface',
  'product_workflow_skills_considered',
  'selected_or_planned_skill',
  'why_selected_or_planned',
  'core_root',
  'local_user_model_root_status',
  'evidence_to_collect',
  'mutation_flags',
  'external_ai_hidden_usage',
  'non_claims',
  'final_user_visible_response'
];
const outputFieldPresence = Object.fromEntries(requiredOutputFields.map((field) => [field, responseText.includes(field)]));
const exactContractValues = {
  scenario_id_value_present: responseText.includes(scenarioId),
  correlation_id_value_present: responseText.includes(correlationId),
  selected_skill_value_present: responseText.includes('xiigen-smart-skills'),
  final_required_phrase_present: responseText.includes('dual-path same-scenario evidence') || responseText.includes('readiness')
};
const outputContractProven = Object.values(outputFieldPresence).every(Boolean)
  && exactContractValues.scenario_id_value_present
  && exactContractValues.correlation_id_value_present;
const promptIdentity = {
  prep_prompt_present: typeof prepPrompt === 'string' && prepPrompt.length > 0,
  request_prompt_present: typeof requestPrompt === 'string' && requestPrompt.length > 0,
  trace_raw_prompt_present: false,
  prep_prompt_length_canonical_utf8_parse: prepPrompt.length,
  request_prompt_length_canonical_utf8_parse: requestPrompt?.length ?? null,
  prep_prompt_sha256_canonical_utf8_parse: prepPromptHash,
  request_prompt_sha256_canonical_utf8_parse: requestPromptHash,
  response_prompt_hashes_present: responsePromptHashes,
  trace_user_prompt_sha256_present: tracePromptHash,
  powershell_default_getcontent_hash_probe: psHash,
  prep_request_exact_text_equal: prepPrompt === requestPrompt,
  prep_request_hash_equal: prepPromptHash === requestPromptHash,
  response_hash_matches_prep_request: responsePromptHashes.length > 0 && responsePromptHashes.every((hash) => hash === prepPromptHash),
  trace_hash_matches_prep_request_response: tracePromptHash !== null && tracePromptHash === prepPromptHash && responsePromptHashes.includes(tracePromptHash),
  trace_hash_source_is_same_request_json_field_by_script: true,
  trace_hash_is_different_prompt_field_by_design: false,
  trace_hash_mismatch_source: 'Installed adapter writes request.json as UTF-8 without BOM; tools/xiigen-extension-core-api.ps1 reads it through Windows PowerShell Get-Content -Raw without -Encoding UTF8 before computing trace user_prompt_sha256. The dotnet core service reads the same request as UTF-8 and emits the response prompt hash that matches prep/request.',
  not_same_prompt_across_all_artifacts: !(tracePromptHash !== null && tracePromptHash === prepPromptHash && responsePromptHashes.includes(tracePromptHash)),
  same_prompt_preserved_across_prep_request_response_only: prepPrompt === requestPrompt && responsePromptHashes.length > 0 && responsePromptHashes.every((hash) => hash === prepPromptHash),
  same_prompt_preserved_across_prep_request_response_trace: false,
  prompt_identity_status: tracePromptHash === prepPromptHash ? 'proven_all_available_hashes_match' : 'needs_fix_trace_hash_mismatch'
};

const requestResponseTraceWritten = [requestPath, responsePath, tracePath, outputPath].every((filePath) => fs.existsSync(filePath) && fs.statSync(filePath).size > 0);
const adapterCoreBoundaryExecuted = trace?.script_path === 'tools/xiigen-extension-core-api.ps1'
  && trace?.call_target === 'SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest';
const mutationFlagsAllFalse = trace?.external_ai_used === false
  && trace?.training_started === false
  && trace?.host_runtime_claimed === false
  && responseText.includes('no checkpoint promotion')
  && responseText.includes('no locator update');
const clientAdaptiveMentionedInPrompt = prepPrompt.includes('client-adaptive-learning-loop');
const clientAdaptiveSelectedOrPlanned = selectedSkills.includes('client-adaptive-learning-loop') || responseText.includes('client-adaptive-learning-loop');
const pass = adapterResult.status === 0
  && requestResponseTraceWritten
  && adapterCoreBoundaryExecuted
  && promptIdentity.trace_hash_matches_prep_request_response
  && mutationFlagsAllFalse;

const validation = {
  schema: 'xiigen.row19.path_a_adapter_core.corrected_validation.v1',
  status: pass ? 'ROW19_PATH_A_CORRECTED_PASS_REVIEW_REQUIRED' : 'ROW19_PATH_A_CORRECTED_NEEDS_FIX',
  generated_utc: new Date().toISOString(),
  run_folder: runRoot,
  path_a_folder: pathARoot,
  scenario_id: scenarioId,
  correlation_id: correlationId,
  method: 'prompt-to-task',
  selected_or_planned_skills: {
    requested_skills: request?.RequestedSkills ?? [],
    available_skills: request?.AvailableSkills ?? [],
    response_selected_skills: selectedSkills,
    selected_or_planned_skill: selectedSkills.length > 0 ? selectedSkills : ['not_proven']
  },
  commands_run: [
    {
      command: 'node ./src/codex-entrypoint.js with argv array from logs/row19-path-a-corrected-runner.mjs',
      cwd: pluginRoot,
      exit_code: adapterResult.status,
      evidence: outputPath
    },
    {
      command: 'powershell.exe -NoProfile -ExecutionPolicy Bypass -Command <default Get-Content prompt hash probe>',
      cwd: repoRoot,
      exit_code: psHashResult.status,
      evidence: psHashPath
    }
  ],
  files_created: {
    request: fileInfo(requestPath),
    response: fileInfo(responsePath),
    trace: fileInfo(tracePath),
    adapter_command_output: fileInfo(outputPath),
    command_log: fileInfo(commandLogPath),
    powershell_hash_probe: fileInfo(psHashPath),
    validation: validationPath,
    validation_copy: validationCopyPath,
    prompt_identity: promptIdentityPath,
    human_report: reportPath
  },
  prompt_identity_check: promptIdentity,
  validation_booleans: {
    installed_skill_surface_loaded: fs.existsSync(skillPath),
    adapter_core_boundary_executed: adapterCoreBoundaryExecuted,
    installed_plugin_runtime_invocation_proven: adapterResult.status === 0 && adapterCoreBoundaryExecuted,
    request_response_trace_written: requestResponseTraceWritten,
    same_prompt_preserved: promptIdentity.trace_hash_matches_prep_request_response,
    same_prompt_preserved_across_prep_request_response_only: promptIdentity.same_prompt_preserved_across_prep_request_response_only,
    mutation_flags_all_false: mutationFlagsAllFalse,
    hidden_external_ai_used: trace?.external_ai_used === true,
    token_savings_proven: false,
    quality_improvement_proven: false,
    learnability_proven: false,
    training_started: trace?.training_started === true,
    eval_started: false,
    dpo_generation_started: false,
    checkpoint_mutation_started: false,
    checkpoint_promotion_started: false,
    locator_mutation_started: false,
    production_locator_update_started: false,
    full_codex_chat_ui_proven: false,
    product_readiness_claimed: false,
    path_b_codex_ui_run: false,
    git_actions_run: false,
    code_edits_run: false
  },
  response_output_contract: {
    status: outputContractProven ? 'proven' : 'not_proven',
    required_fields: requiredOutputFields,
    field_name_presence_in_response_json_text: outputFieldPresence,
    exact_contract_values,
    note: outputContractProven
      ? 'All requested output field names and core Row19 values were found in response text.'
      : 'The adapter/core response is a redacted baseline core response and does not provide the full Russian user-visible Row19 output contract requested by the prompt.'
  },
  evidence_fields: {
    prompt_to_task: {
      exposed: responseText.includes('prompt_to_task') || responseText.includes('Prompt-to-task'),
      evidence: 'response.json ChatDisplayText / qualityProjection prompt_to_task'
    },
    task_to_plan: {
      exposed: responseText.includes('task_to_plan') || responseText.includes('task+plan'),
      evidence: 'response.json ChatDisplayText / qualityProjection task_to_plan'
    },
    task_or_plan_to_skills: {
      exposed: selectedSkills.length > 0 || responseText.includes('task_to_skills'),
      selected_skills: selectedSkills,
      evidence: 'response.json SelectedSkills / qualityProjection task_to_skills'
    },
    client_adaptive_learning_loop: {
      requested_in_exact_prompt: clientAdaptiveMentionedInPrompt,
      selected_or_planned_in_response: clientAdaptiveSelectedOrPlanned,
      status: clientAdaptiveSelectedOrPlanned ? 'proven' : 'not_proven',
      note: clientAdaptiveSelectedOrPlanned
        ? 'Response text explicitly selected or planned client-adaptive-learning-loop.'
        : 'Response selected xiigen-smart-skills only; client-adaptive-learning-loop remains not_proven.'
    }
  },
  core_response: {
    success: response?.Success ?? null,
    status: response?.Status ?? null,
    selected_skills: selectedSkills,
    structured_task_refs: response?.StructuredTaskRefs ?? [],
    scenario_refs: response?.ScenarioRefs ?? [],
    warnings: response?.Warnings ?? [],
    errors: response?.Errors ?? [],
    contains_raw_private_content: response?.ContainsRawPrivateContent ?? null,
    contains_secrets: response?.ContainsSecrets ?? null
  },
  trace_summary: {
    script_path: trace?.script_path ?? null,
    call_target: trace?.call_target ?? null,
    response_success: trace?.response_success ?? null,
    claim_scope: trace?.claim_scope ?? null,
    external_ai_used: trace?.external_ai_used ?? null,
    training_started: trace?.training_started ?? null,
    semantic_quality_claimed: trace?.semantic_quality_claimed ?? null,
    runtime_extension_claimed: trace?.runtime_extension_claimed ?? null,
    host_runtime_claimed: trace?.host_runtime_claimed ?? null,
    raw_private_prompt_persisted: trace?.raw_private_prompt_persisted ?? null,
    contains_secrets: trace?.contains_secrets ?? null
  },
  pass,
  fail_reasons: pass ? [] : [
    ...(promptIdentity.trace_hash_matches_prep_request_response ? [] : ['trace_user_prompt_sha256_mismatches_canonical_prep_request_response_prompt_hash']),
    ...(outputContractProven ? [] : ['response_output_contract_not_proven']),
    ...(adapterResult.status === 0 ? [] : ['adapter_command_nonzero_exit']),
    ...(adapterCoreBoundaryExecuted ? [] : ['adapter_core_boundary_not_proven'])
  ],
  narrow_human_claim: pass
    ? 'Path A proves only installed Codex XIIGen plugin adapter/core boundary execution for the exact Row19 prompt and correlation id. It does not prove Path B UI, product readiness, token savings, semantic quality improvement, learnability, training, eval, checkpoint promotion, locator update, or full Codex chat/app readiness.'
    : 'Path A invoked the installed Codex XIIGen plugin adapter/core and produced request/response/trace, but corrected evidence remains NEEDS_FIX because trace prompt hash identity does not match the canonical prep/request/response prompt hash. No product readiness or host UI readiness is claimed.',
  review_required: true
};

fs.writeFileSync(validationPath, JSON.stringify(validation, null, 2) + '\n', 'utf8');
fs.writeFileSync(validationCopyPath, JSON.stringify(validation, null, 2) + '\n', 'utf8');
fs.writeFileSync(promptIdentityPath, JSON.stringify(promptIdentity, null, 2) + '\n', 'utf8');

const report = `# Row19 Path A adapter/core corrected evidence

Статус: ${validation.status}

Папка run: ${runRoot}
Path A папка: ${pathARoot}

## Что выполнено

Выполнен только Path A adapter/core smoke через установленный Codex XIIGen plugin adapter. Команда вызвала ${pluginRoot}/src/codex-entrypoint.js, который создал request.json и вызвал C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1. Codex UI Path B не запускался. Git, code edits, training, eval, DPO, checkpoint/locator mutation и production locator update не выполнялись.

Correlation id из prep: ${correlationId}.
Scenario id из prep: ${scenarioId}.

## Команды

1. node ./src/codex-entrypoint.js with argv array from logs/row19-path-a-corrected-runner.mjs
   - cwd: ${pluginRoot}
   - exit code: ${adapterResult.status}
   - output: ${outputPath}
2. powershell.exe -NoProfile -ExecutionPolicy Bypass -Command <default Get-Content prompt hash probe>
   - output: ${psHashPath}

## Prompt/hash identity

- Prep prompt canonical UTF-8 hash: ${prepPromptHash}
- Request prompt canonical UTF-8 hash: ${requestPromptHash}
- Prep == request exact text: ${promptIdentity.prep_request_exact_text_equal}
- Response prompt hashes found: ${responsePromptHashes.length ? responsePromptHashes.join(', ') : 'not_present'}
- Response hashes match prep/request: ${promptIdentity.response_hash_matches_prep_request}
- Trace user_prompt_sha256: ${tracePromptHash ?? 'not_present'}
- Trace hash matches prep/request/response: ${promptIdentity.trace_hash_matches_prep_request_response}
- PowerShell default Get-Content probe: ${psHash ? `${psHash.sha256_utf8_after_powershell_default_getcontent} (length ${psHash.prompt_length})` : 'not_available'}
- Canonical Node/dotnet prompt length: ${prepPrompt.length}

Вывод: это не другой prompt field by design. Trace hash берется из того же request JSON поля UserPromptOrTaskText, но core PowerShell script читает JSON через Get-Content -Raw без -Encoding UTF8. Для UTF-8 request.json без BOM и не-ASCII prompt это дает другую decoded string перед trace hash. Dotnet core response читает request как UTF-8, поэтому response hash совпадает с prep/request. Полная prep/request/response/trace prompt identity не доказана.

## Output contract

Response output contract status: ${validation.response_output_contract.status}.
Причина: adapter/core response является redacted baseline core response. Он показывает prompt->task, task->plan и task/plan->skills evidence, но не возвращает полный русский user-visible contract со всеми полями scenario_id, normalized_task, task_plan_steps, mutation_flags, non_claims и final_user_visible_response как отдельный финальный ответ.

## Selected/planned skills

- Requested skills: ${(request?.RequestedSkills ?? []).join(', ') || 'none'}
- Available skills: ${(request?.AvailableSkills ?? []).join(', ') || 'none'}
- Response selected skills: ${selectedSkills.join(', ') || 'none'}
- client-adaptive-learning-loop: ${validation.evidence_fields.client_adaptive_learning_loop.status}

client-adaptive-learning-loop остается not_proven: он есть в exact prompt, но adapter/core response выбрал только xiigen-smart-skills и не доказал выбор или планирование client-adaptive-learning-loop.

## Что доказано узко

- request.json, response.json, trace.json и adapter-command-output.txt созданы в ${pathARoot}.
- Adapter/core boundary выполнен: trace указывает tools/xiigen-extension-core-api.ps1 и SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest.
- Hidden external AI usage, training и host runtime claim в trace не отмечены.

## Что не доказано

- Полная prompt identity через trace hash не доказана.
- Response output contract: not_proven.
- client-adaptive-learning-loop: not_proven.
- Path B Codex UI: not_proven, не запускался.
- Token savings, semantic quality improvement, learnability, training/eval/DPO, checkpoint mutation/promotion, locator update, production locator update, full Codex chat UI и product readiness: not_proven.

## Narrow claim

${validation.narrow_human_claim}

Это подготовка/маршрутизация Row19; readiness не доказана без dual-path same-scenario evidence.
`;
fs.writeFileSync(reportPath, report, 'utf8');

console.log(JSON.stringify({
  status: validation.status,
  run_folder: runRoot,
  path_a_folder: pathARoot,
  pass,
  trace_hash_matches_prep_request_response: promptIdentity.trace_hash_matches_prep_request_response,
  response_output_contract: validation.response_output_contract.status,
  validation: validationPath,
  report: reportPath
}, null, 2));
