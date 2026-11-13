# Codex available-skills parser repair report

## Verdict

PASS - the narrow Codex adapter parser boundary is repaired and proven against the real RUNTIME-04 failure mode.

## True root cause

RUNTIME-04 did pass `--available-skills-json` and `--requested-skills-json` in `runtime04-positive-probe.ps1`.

The old `src/codex-entrypoint.js` also already had mapping lines for both fields. The bug was not a missing mapping line.

The actual failure was Windows PowerShell/native Node argument quoting: JSON values such as `["architect","planning","plan-review"]` reached Node as `[architect,planning,plan-review]`. That is not valid JSON, so `JSON.parse` failed and `parseJsonOrDefault(..., [])` returned `[]`.

That is why the RUNTIME-04 request artifact had empty `AvailableSkills`, `RequestedSkills`, `HostFacts`, and default-only `Provenance` even though the probe script showed those arguments.

## What changed

`C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js` now uses `parseJsonArrayOrDefault` for `--requested-skills-json` and `--available-skills-json`.

The helper first accepts real JSON arrays. If JSON parsing fails, it accepts only a safe PowerShell-stripped bracket-list form such as `[architect,planning,plan-review]`. Each item must match a conservative skill-id pattern. Unsafe or non-array values still fall back to `[]`.

This is not skill-selection logic. The Codex host remains a thin adapter: it forwards explicit request fields to `llm_mvp_core` and does not choose, rank, train, or mutate skills locally.

## What this proves

- Codex adapter CLI accepts `--available-skills-json` when it arrives as valid JSON.
- Codex adapter CLI also accepts the real PowerShell-stripped bracket-list form observed in the RUNTIME-04 failure path.
- `AvailableSkills` reaches the `XiigenExtensionRequest` JSON sent to `llm_mvp_core`.
- Observed `AvailableSkills`: `architect`, `planning`, `plan-review`.
- `RequestedSkills` still reaches the same request. Observed `RequestedSkills`: `planning`.
- No semantic ranking, training, DPO generation, or checkpoint mutation happens in the Codex host.

## What this does not prove

- Semantic SkillSelection quality.
- Token savings or quality improvement.
- Trained-model readiness, fresh-load, continue-training, DPO generation, or checkpoint mutation.
- Full Codex plugin chat UI runtime invocation.
- Full completion of the extension runtime/token/quality/learnability plan.

## Evidence

- RUNTIME-04 probe script that passed the options: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\runtime04-positive-probe.ps1`
- RUNTIME-04 stale request that showed the failure: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\positive-scripted\xiigen-codex-core-request.json`
- Source repair: `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js`
- Focused probe script: `C:\Projects\codex-xiigen-smart-skills\test\host-qa\HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER.js`
- Request artifact for real bug condition: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER\powershell-stripped-bracket-list\xiigen-codex-core-request.json`
- Request artifact for normal JSON case: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER\valid-json-array\xiigen-codex-core-request.json`
- JSON report: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-available-skills-parser-repair-report.json`
- Component QA report: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER\HOST-08F-codex-plugin-app-qa.json`

## Checks run

- `node.exe test/host-qa/HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER.js --repo-root C:\Projects\codex-xiigen-smart-skills --core-root C:\Projects\llm_mvp_core --output-root C:\Projects\codex-xiigen-smart-skills\reports\HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER` - PASS.
- `valid-json-array` case - PASS.
- `powershell-stripped-bracket-list` case - PASS. This is the focused regression for the real bug condition.
- Source syntax checks for Codex entrypoint and focused QA scripts - PASS.
- `npm run source-wiring-check` - PASS.

## Next independent track

Parent review can accept this parser repair. After that, Codex SkillSelection/token/quality runtime rows may use non-empty `AvailableSkills` evidence instead of the stale RUNTIME-04 request where `AvailableSkills` was empty.
