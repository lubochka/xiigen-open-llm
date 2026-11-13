# Row20 Path A evidence packet

claim_scope: Row20 Path A installed-plugin boundary evidence only.
evidence_scope: required Row20 prep/status packets, targeted AGENTS honesty/sub-agent/product-readiness rules, xiigen-smart-skills skill instructions, installed plugin metadata/source checks, command attempts from the installed plugin root, generated JSON parsing, and targeted verification checks.
count_claimed: 2 command attempts; 1 focused retry; 0 host UI passes; 0 joined passes; 0 product-ready extensions.
independent_evidence_count: 11 inspectable evidence records or record groups.
subagent_id_or_NONE: ROW20_PATH_A_SUBAGENT.
downgrade_needed: yes. The evidence supports installed-plugin JS entrypoint to core boundary execution, but not a clean Row20 Path A PASS because required Row20 host-facts/provenance metadata did not persist in request.json.

## Scenario

- row: 20
- scenario_id: frdp-p01-codex-full-extension-entrypoint
- correlation_id used: ROW20-CODEX-FULL-EXTENSION-ENTRYPOINT-NONMUTATING-SMOKE
- target: Codex installed plugin / xiigen-smart-skills entrypoint boundary only
- run folder: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836

## Files read

- C:\Projects\llm_mvp_core\AGENTS.md targeted honesty/no-overclaim/product-readiness/sub-agent rules.
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\row20-route-or-blocker-evidence-packet.md
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\row20-route-or-blocker-status.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\row20-prep-codex-full-extension-entrypoint.md
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\row20-prep-codex-full-extension-entrypoint.json
- C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\skills\xiigen-smart-skills\SKILL.md
- C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\package.json
- C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\.codex-plugin\plugin.json
- C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js targeted parser/invocation lines.

## Files changed

- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192703\request.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192703\response.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192703\trace.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192703\command-result.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\request.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\response.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\trace.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\command-result.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\row20-path-a-evidence-packet.md
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-192836\row20-path-a-status.json

## Command run or blocker

Command run from installed plugin root:

```text
node src\codex-entrypoint.js --method prompt-to-task --user-prompt-or-task-text <redacted-row20-harmless-prompt> --host-facts-json <row20-host-facts> --requested-skills-json ["xiigen-smart-skills"] --available-skills-json ["xiigen-smart-skills"] --check-set-inputs-json <row20-nonmutation-flags> --provenance-json <row20-provenance> --correlation-id ROW20-CODEX-FULL-EXTENSION-ENTRYPOINT-NONMUTATING-SMOKE --debug-enabled --core-root C:\Projects\llm_mvp_core --extension-root . --request-path <runDir>\request.json --response-path <runDir>\response.json --trace-path <runDir>\trace.json
```

There were two attempts. The first run used bad PowerShell JSON quoting for host-facts/check-set/provenance. A single focused retry kept the same command path and scenario but corrected the intended quoting. The focused retry also exited 0, but the generated request still did not persist the required Row20 host-facts/provenance values. No broader retry was performed.

## Evidence found

- `command-result.json` shows `cwd` was the installed plugin root and the executable path was `node src\codex-entrypoint.js`.
- `request.json` shows method `prompt-to-task`, requested/available skill `xiigen-smart-skills`, correlation `ROW20-CODEX-FULL-EXTENSION-ENTRYPOINT-NONMUTATING-SMOKE`, and `TargetLocator: null`.
- `request.json` does not show `HostFacts.surface=installed-codex-skill`, `HostFacts.row=20`, `HostFacts.scenario_id=frdp-p01-codex-full-extension-entrypoint`, or the requested `CheckSetInputs`/`Provenance` values.
- `response.json` shows `Success: true`, status `core_boundary_partial_no_semantic_quality_claim`, selected skill `xiigen-smart-skills`, and redacted display output.
- `trace.json` shows endpoint `xiigen-extension-core-api`, method `prompt-to-task`, the Row20 correlation id, `response_success: true`, `external_ai_used: false`, `training_started: false`, `host_runtime_claimed: false`, and claim scope `core-owned endpoint invocation only; no host runtime claim`.

## Verification commands run

- Listed files in the retry run folder.
- Parsed `request.json`, `response.json`, `trace.json`, and `command-result.json` with PowerShell `ConvertFrom-Json`.
- Ran targeted `rg` inside the retry run folder for external/training/eval/DPO/checkpoint/locator/mutation/correlation/skill/status terms.
- Ran targeted `Select-String` against the exact plugin skill file for smoke and non-claim rules.
- Read targeted parser/invocation lines from `src\codex-entrypoint.js`.

## Verification results

- Command exit code: 0.
- Core response success: true.
- Adapter/core boundary executed: true.
- Installed plugin JS entrypoint invocation from installed root: true.
- Required Row20 host-facts metadata persisted in `request.json`: false.
- Required Row20 provenance/check-set metadata persisted in `request.json`: false.
- Hidden external AI observed: false.
- Training observed: false.
- Eval observed: false.
- DPO generation observed: false.
- Checkpoint mutation observed: false.
- Locator mutation observed: false.
- Host UI observed: false.

## Path A result

`PARTIAL_BOUNDARY_EVIDENCE_METADATA_GAP`.

The evidence supports only this narrow claim: the installed package's current JS entrypoint was invoked from the installed plugin root and reached the local `llm_mvp_core` adapter/core boundary for the Row20 prompt/correlation, with a successful response and no observed external AI/training/eval/checkpoint/locator mutation. It does not support a clean Row20 Path A acceptance pass because the request did not preserve the required Row20 host-facts/provenance metadata.

## Mutation boundaries kept

- No host UI execution or screenshots.
- No training/eval/DPO generation.
- No checkpoint or locator mutation.
- No product code edits.
- No tests/builds beyond the installed-plugin boundary command.
- No board/counter/status updates outside the run folder.
- No git stage/commit/push.
- No hidden external AI observed.

## Non-claims

- I did not verify product readiness.
- I did not run host UI.
- I did not prove a formal joined pass.
- I did not prove full Codex chat/app readiness.
- I did not prove token savings.
- I did not prove semantic quality.
- I did not prove learnability or training readiness.
- I did not prove DPO quality.
- I did not prove checkpoint promotion.
- I did not prove locator update.
- This is not evidence that 3 extensions are product ready.

## Exact next action

A future bounded Path A rerun or entrypoint/CLI argument review should preserve `--host-facts-json`, `--check-set-inputs-json`, and `--provenance-json` values in `request.json`, then rerun from the installed plugin root. Path B host UI remains unproven and product readiness remains false.

parent_will_perform_commit_push_after_review=true
