# Row20 Path A Metadata-Loss Diagnosis

claim_scope: Row20 Path A metadata-loss diagnosis packet only.
evidence_scope: Targeted reads of the primary partial run folder, installed Codex plugin source/manifest/skill files, core endpoint script, and exact core request/service files.
count_claimed: 1 diagnostic packet with 2 new files; 0 source patches; 0 reruns.
independent_evidence_count: 18 read/search/parse records.
subagent_id_or_NONE: Row20Diag-Codex-20260626.
downgrade_needed: yes. This proves a metadata propagation diagnosis only, not Row20 Path A pass or product readiness.

## Work Order Boundary

- Requested artifact type: Row20 Path A metadata loss diagnosis packet.
- Primary run: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836`
- Diagnostic folder: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-metadata-diagnosis-20260626-193604`
- No nested agents spawned.
- No external AI called.
- No host UI/training/eval/DPO/checkpoint/locator mutation run.
- No git stage/commit/push.
- No plugin/core/governance/source/test/existing-run files modified.

## Files Read

- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\request.json`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\response.json`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\trace.json`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\command-result.json`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\row20-path-a-status.json`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\row20-path-a-evidence-packet.md`
- `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js`
- `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\package.json`
- `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\.codex-plugin\plugin.json`
- `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\skills\xiigen-smart-skills\SKILL.md`
- `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`
- `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs`
- `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs`

Note: one read-only `rg` locator command initially searched wider than the preferred exact core files to find the request/service owners. The diagnosis below uses the narrowed exact files.

## Files Changed

- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-metadata-diagnosis-20260626-193604\path-a-metadata-diagnosis.md`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-metadata-diagnosis-20260626-193604\path-a-metadata-diagnosis.json`

## Evidence Findings

`request.json` contains only these metadata keys:

- `HostFacts`: `target_locator_present`, `target_locator_candidate_id`, `target_candidate_id`, `target_locator_mode`
- `CheckSetInputs`: empty
- `Provenance`: `host`, `entrypoint`, `package`, `target_locator_contract`

It does not contain the required Row20 host facts, check-set inputs, or provenance values. The response debug payload mirrors this loss: `hostFactKeys` contains only target-locator defaults, `checkSetInputKeys` is empty, and `provenanceKeys` contains only default plugin provenance keys.

`command-result.json` records the intended flags:

- `--host-facts-json`
- `--check-set-inputs-json`
- `--provenance-json`

But its stored argument values are redacted/placeheld as `<row20-host-facts-json>`, `<row20-check-set-inputs-json>`, and `<row20-provenance-json>`, so the primary run folder does not independently prove that valid JSON strings reached Node.

`codex-entrypoint.js` attempts to parse these exact CLI flags:

- `hostFacts: parseJsonOrDefault(parsed['host-facts-json'], {})`
- `checkSetInputs: parseJsonOrDefault(parsed['check-set-inputs-json'], {})`
- `provenance: parseJsonOrDefault(parsed['provenance-json'], {})`

It then writes the request fields under the core contract names:

- `HostFacts`
- `CheckSetInputs`
- `Provenance`

The parser silently falls back to `{}` on invalid JSON. Therefore invalid quoting, stripped quotes, placeholder values, or an empty/missing argument all produce exactly the observed request shape: default target-locator host facts, empty check-set inputs, and default provenance only.

`XiigenExtensionApiContracts.cs` defines these fields directly on `XiigenExtensionRequest` as `IReadOnlyDictionary<string, string>`:

- `HostFacts`
- `CheckSetInputs`
- `Provenance`

`xiigen-extension-core-api.ps1` deserializes the whole request into `XiigenExtensionRequest` with `PropertyNameCaseInsensitive = true` and passes it to `XiigenExtensionApiService.ProcessHostExtensionRequest`.

`XiigenExtensionApiService.cs` consumes those fields in multiple places:

- `request.HostFacts` is passed into skill selection and prompt context.
- `request.CheckSetInputs` supplies explicit constraints and reference fields such as `marker`, `scenario_id`, and `pair_id`.
- `request.Provenance` supplies prompt provenance/source and provenance context keys.
- debug trace reports `HostFactKeys`, `CheckSetInputKeys`, and `ProvenanceKeys`.

This makes a core ingress drop/rename unlikely for the fields themselves. The evidence supports that the metadata was already absent or invalid before the written `request.json`, or was converted to fallback `{}` by plugin-side JSON parsing before core invocation.

## Required Diagnosis Questions

Did `codex-entrypoint.js` parse `--host-facts-json`, `--check-set-inputs-json`, and `--provenance-json`?

Evidence supports: it maps and attempts to parse those exact flags. The actual primary run did not preserve their intended values, so the actual parse result was fallback/empty or the arguments were missing/invalid by the time Node parsed them.

Did it write those fields into `request.json` under the names expected by core?

Evidence supports: source writes `HostFacts`, `CheckSetInputs`, and `Provenance`, which match the core request contract. The primary `request.json` contains those field names but lacks the Row20 values, which means the entrypoint wrote fallback/default values for this run.

Did the core API accept/drop/rename those fields?

Evidence supports: core accepts those exact field names and consumes them. No source evidence shows core dropping or renaming the dictionaries at ingress. The response debug key lists match the already-empty/default values in `request.json`, so the loss is upstream of core behavior.

Is the gap likely PowerShell/command quoting, plugin parser mapping, request schema mismatch, or core serializer behavior?

Classification: likely command argument quoting/value substitution plus silent plugin parser fallback. Plugin parser mapping and core schema names are correct. Core serializer/deserializer behavior is not the likely root cause. A secondary source-design risk is that `parseJsonOrDefault` silently hides invalid metadata JSON instead of failing closed for required diagnostic metadata.

Can a no-source command-only rerun fix it?

`true`, conditionally. If the next worker passes valid JSON strings directly as PowerShell variables, avoids placeholder/literal redaction values, writes to a fresh run folder, and then verifies `request.json` before making any pass claim, the existing source should preserve the metadata. This packet did not rerun the command.

## Exact Safe Command Shape For Next Worker

The next worker should create a new run folder under the same scenario folder, then run from the installed plugin root with JSON strings built by `ConvertTo-Json -Compress` and passed as variables:

```powershell
$pluginRoot = 'C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500'
$scenarioRoot = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint'
$runDir = Join-Path $scenarioRoot ('path-a-run-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Force -Path $runDir | Out-Null

$hostFactsJson = [ordered]@{
  surface = 'installed-codex-skill'
  row = '20'
  scenario_id = 'frdp-p01-codex-full-extension-entrypoint'
  path = 'A'
} | ConvertTo-Json -Compress

$checkSetInputsJson = [ordered]@{
  row = '20'
  scenario_id = 'frdp-p01-codex-full-extension-entrypoint'
  marker = 'row20-path-a-codex-full-extension-entrypoint'
  nonmutation_flags = 'no-training,no-eval,no-dpo,no-checkpoint,no-locator'
} | ConvertTo-Json -Compress

$provenanceJson = [ordered]@{
  source = 'row20-path-a-rerun'
  plugin = 'codex-xiigen-smart-skills'
  scenario_id = 'frdp-p01-codex-full-extension-entrypoint'
} | ConvertTo-Json -Compress

Push-Location $pluginRoot
try {
  & node .\src\codex-entrypoint.js `
    --method prompt-to-task `
    --user-prompt-or-task-text 'Row20 non-mutating Codex installed-plugin entrypoint smoke: route this harmless planning request through XIIGen Smart Skills using prompt-to-task, request only xiigen-smart-skills, keep debug output redacted, do not train, do not evaluate, do not mutate checkpoint or locator, do not call external AI, and return the selected skill/plan boundary result.' `
    --host-facts-json $hostFactsJson `
    --requested-skills-json '["xiigen-smart-skills"]' `
    --available-skills-json '["xiigen-smart-skills"]' `
    --check-set-inputs-json $checkSetInputsJson `
    --provenance-json $provenanceJson `
    --correlation-id 'ROW20-CODEX-FULL-EXTENSION-ENTRYPOINT-NONMUTATING-SMOKE' `
    --debug-enabled `
    --core-root 'C:\Projects\llm_mvp_core' `
    --extension-root '.' `
    --request-path (Join-Path $runDir 'request.json') `
    --response-path (Join-Path $runDir 'response.json') `
    --trace-path (Join-Path $runDir 'trace.json')
}
finally {
  Pop-Location
}
```

Immediate post-rerun gate:

- Parse `request.json`.
- Require `HostFacts.surface`, `HostFacts.row`, and `HostFacts.scenario_id`.
- Require nonempty `CheckSetInputs` with `scenario_id` and `marker`.
- Require `Provenance.source`, `Provenance.plugin`, and `Provenance.scenario_id`.
- If any are missing, stop with metadata propagation failure and do not claim Row20 Path A pass.

## Exact Patch Scope If Needed

No source patch is proven necessary before one command-only rerun with non-placeholder JSON variables.

If that rerun still loses fields, the minimal future patch scope is:

- `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js`

Patch intent:

- Replace silent fallback for required diagnostic JSON flags with explicit parse diagnostics or fail-closed behavior for `--host-facts-json`, `--check-set-inputs-json`, and `--provenance-json`.
- Preserve the existing core request field names: `HostFacts`, `CheckSetInputs`, `Provenance`.
- Do not patch `tools\xiigen-extension-core-api.ps1` or `43.SmartSkillsDomain` for this metadata-loss symptom unless new evidence shows valid metadata in `request.json` and loss after core deserialization.

## Verification Commands Run

- `Get-ChildItem -Force <primary-run-folder>`
- `Get-Content -Raw <primary-run-folder>\request.json`
- `Get-Content -Raw <primary-run-folder>\response.json`
- `Get-Content -Raw <primary-run-folder>\trace.json`
- `Get-Content -Raw <primary-run-folder>\command-result.json`
- `Get-Content -Raw <primary-run-folder>\row20-path-a-status.json`
- `Get-Content -Raw <primary-run-folder>\row20-path-a-evidence-packet.md`
- `Get-Content -Raw <plugin-root>\src\codex-entrypoint.js`
- `Get-Content -Raw <plugin-root>\package.json`
- `Get-Content -Raw <plugin-root>\.codex-plugin\plugin.json`
- `Get-Content -Raw <plugin-root>\skills\xiigen-smart-skills\SKILL.md`
- `Get-Content -Raw C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`
- `rg --files C:\Projects\llm_mvp_core\43.SmartSkillsDomain | rg 'XiigenExtension|ExtensionApi|Request'`
- Targeted line reads from `XiigenExtensionApiContracts.cs`, `XiigenExtensionApiService.cs`, and `codex-entrypoint.js`
- `ConvertFrom-Json` parse of primary `request.json` to list `HostFactKeys`, `CheckSetInputKeys`, and `ProvenanceKeys`

## Verification Results

- Primary run command exit code: 0, from existing `command-result.json`.
- Primary response success: true, from existing `response.json`.
- Metadata preservation: failed/partial. Required Row20 metadata keys are absent in `request.json`.
- Core contract accepts `HostFacts`, `CheckSetInputs`, and `Provenance`.
- Core service consumes those fields and reports their key lists.
- Root cause class: likely CLI JSON argument value/quoting/substitution plus silent plugin fallback, not core schema mismatch.

## Mutation Boundaries Kept

- Created one new diagnostic folder only.
- Wrote only `path-a-metadata-diagnosis.md` and `path-a-metadata-diagnosis.json`.
- Did not patch plugin source.
- Did not patch core source.
- Did not modify governance files, scenario board, action plan, dashboards, product code, tests, existing run folders, checkpoints, locators, or state files.
- Did not run host UI/training/eval/DPO/checkpoint/locator mutation.
- Did not stage, commit, or push git changes.
- Did not spawn nested agents.
- Did not call external AI.

## Non-Claims

- I did not verify product readiness.
- I did not run a Row20 rerun.
- I did not run host UI.
- I did not run training, eval, DPO, checkpoint promotion, or locator mutation.
- I did not patch source.
- I did not prove full Codex chat/app readiness.
- I did not prove token savings, semantic quality, learnability, or checkpoint promotion.
- This packet is diagnosis only, not a completed Row20 Path A pass.
- This packet is not evidence that 3 extensions are product ready.

## Return Packet

- subagent_id_or_nickname: `Row20Diag-Codex-20260626`
- files_read: listed above
- files_changed: the two diagnostic files in this folder
- diagnosis_summary: Row20 metadata flags are mapped by the plugin and accepted by core contract names, but the primary run request contains fallback/default metadata only. The loss likely happened before request serialization through invalid/missing/placeholder JSON CLI argument values, masked by silent parser fallback.
- root_cause_classification: `likely_cli_json_argument_value_or_quoting_plus_silent_plugin_parse_fallback`
- no_source_rerun_possible: `true`
- exact_patch_scope_if_needed: `src\codex-entrypoint.js` only, and only after one command-only rerun with valid JSON variables still fails.
- exact_next_work_order: Run one bounded no-source Row20 Path A command-only rerun from installed plugin root using the safe command shape above, writing only to a fresh `path-a-run-<yyyyMMdd-HHmmss>` folder, then verify metadata keys in `request.json` before any pass claim.
- verification_commands_run: listed above
- verification_results: listed above
- mutation_boundaries_kept: listed above
- non_claims: listed above
- parent_will_perform_commit_push_after_review: `true`
