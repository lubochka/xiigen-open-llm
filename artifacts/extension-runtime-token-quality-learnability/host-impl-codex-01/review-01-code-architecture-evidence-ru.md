# HOST-IMPL-CODEX-01 review: code/architecture/evidence

Verdict: PASS_WITH_FINDINGS_REPAIR_REQUIRED

Reviewer role: bounded REVIEW_INTERNAL sub-agent. No code fixes, no commits, no pushes, no spawned agents.

## Scope reviewed

- Core repo: `C:\Projects\llm_mvp_core`
- Codex plugin repo: `C:\Projects\codex-xiigen-smart-skills`
- Core state: `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- Core evidence root: `artifacts/extension-runtime-token-quality-learnability/host-impl-codex-01`
- Plugin report root: `reports/HOST-IMPL-CODEX-01`
- Installed cache root: `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500`

## Findings

### P1 - Russian human-readable evidence report is mojibake/placeholder text

The required Russian human-readable report is not actually readable Russian in both copied locations. Headings and bullets contain literal `?` replacement characters, for example:

- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\host-impl-codex-01\HOST-IMPL-CODEX-01-evidence-ru.md:3`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\host-impl-codex-01\HOST-IMPL-CODEX-01-evidence-ru.md:4`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\host-impl-codex-01\HOST-IMPL-CODEX-01-evidence-ru.md:48`
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\HOST-IMPL-CODEX-01-summary-ru.md:3`
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\HOST-IMPL-CODEX-01-summary-ru.md:4`
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\HOST-IMPL-CODEX-01-summary-ru.md:48`

Impact: this fails the review requirement that the Russian evidence report clearly explain what was run, examples, input/output, and proven/not proven. The JSON evidence is stronger than the Markdown report, but the user-facing RU artifact needs repair before acceptance/commit.

### P1 - Core repo contains noisy untracked Cursor user-data logs outside the HOST-IMPL-CODEX-01 commit scope

`git status --short -uall` in `C:\Projects\llm_mvp_core` currently enumerates many untracked files under:

`artifacts/extension-runtime-token-quality-learnability/qsg21-live-runtime-target-probes/cursor-extension-host/cursor-user-data/logs/...`

The HOST-IMPL-CODEX-01 summary itself records this noisy untracked directory at:

- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\HOST-IMPL-CODEX-01-summary.json:216`
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\HOST-IMPL-CODEX-01-summary.json:218`

Representative current status paths include `main.log`, `mcpprocess.log`, `telemetry.log`, `renderer.log`, and `tasks.workspaceId-*.log` below that `cursor-user-data/logs` tree. These are outside the requested HOST-IMPL-CODEX-01 artifact root and look like runtime/user-data noise. Do not include them in the phase commit; clean, quarantine, or ignore them before any clean commit/push claim.

## Pass evidence

### Code/architecture boundary

No host-side PromptToTask/OrchestrationReasoning/SkillSelection/model/training/semantic ranking/scoring shortcut was found in the reviewed plugin source/cache evidence.

Supporting refs:

- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js:129` constructs the `XiigenExtensionRequest`.
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js:136` passes `RequestedSkills`.
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js:137` passes `AvailableSkills`.
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js:141` passes `TargetLocator`.
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js:145` through `:165` invokes the core endpoint via PowerShell request/response/trace paths.
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js:373` through `:381` records evidence flags with host ranking/training/model mutation disabled.
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js:460` through `:468` declares debug shape and disabled host-side semantic/training/mutation authority.
- `C:\Projects\codex-xiigen-smart-skills\skills\xiigen-smart-skills\SKILL.md:3` and `:10` state the skill is a thin adapter/core-boundary smoke only.
- `C:\Projects\codex-xiigen-smart-skills\xiigen-extension-contract.json:6` through `:13` declare thin client/core endpoint/no training/no external AI/no semantic quality claim.
- `C:\Projects\codex-xiigen-smart-skills\xiigen-extension-contract.json:63` through `:75` list forbidden host responsibilities.
- `C:\Projects\codex-xiigen-smart-skills\xiigen-extension-contract.json:77` through `:82` limit allowed host responsibilities to collect facts, construct request, write JSON, invoke core, and display/forward redacted core response.

The same boundary strings were present in the installed cache source under `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500`.

### Installed cache evidence

I independently recomputed SHA256 for the 10 files listed in `reports/HOST-IMPL-CODEX-01/installed-cache/20260617-234852/cache-refresh-report.json`. Source and installed-cache hashes matched the report for all 10 files.

Report refs:

- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-cache\20260617-234852\cache-refresh-report.json:5` installed cache root.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-cache\20260617-234852\cache-refresh-report.json:32` through `:39` `src\codex-entrypoint.js` source/cache hash and TargetLocator/debug markers.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-cache\20260617-234852\cache-refresh-report.json:112` all copied files match source.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-cache\20260617-234852\cache-refresh-report.json:113` TargetLocator present after refresh.

### Positive installed-cache runtime smoke

Direct request/response inspection passed:

- `TargetLocator` present: true.
- `RequestedSkills` count: 1.
- `AvailableSkills` count: 5.
- Response success: true.
- `core_boundary_executed`: true.
- `target_locator_bound`: true.
- Debug shape contains `xiigen skill-selection :`, `input:`, and `output:`.

Refs:

- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\runtime-smoke-run-summary.json:10` through `:24` positive installed-cache command and evidence paths.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\positive-target-locator\response.json:2` success true.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\positive-target-locator\response.json:4` visible debug display with `xiigen skill-selection :`, input, and output.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\positive-target-locator\response.json:10` `core_boundary_executed=true`.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\positive-target-locator\response.json:49` through `:56` TargetLocator bound/mode/candidate/root evidence.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\positive-target-locator\trace.json:5` core call target.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\positive-target-locator\trace.json:18` through `:27` success, no semantic/runtime claim, no external AI/training, core-owned endpoint scope.

### Negative missing-TargetLocator smoke

Direct response inspection passed:

- Request had no TargetLocator.
- Response success: false.
- Error contains `target_locator_required`.
- Debug shape contains `xiigen prompt-to-task :`, `input:`, and `output:`.

Refs:

- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\runtime-smoke-run-summary.json:31` through `:45` negative installed-cache command and evidence paths.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\negative-missing-target-locator\response.json:2` success false.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\negative-missing-target-locator\response.json:4` visible debug display with input/output.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\negative-missing-target-locator\response.json:17` through `:18` `target_locator_required`.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017\negative-missing-target-locator\response.json:20` through `:25` debug trace and redaction flags.

### Honest scope and overclaim review

No prohibited broad readiness claim was found in the HOST-IMPL-CODEX-01 summary/state. The evidence records `codex plugin list --json` as failed and explicitly avoids full Codex chat UI/app runtime, send-message execution, token/quality/semantic/learnability/final readiness claims.

Refs:

- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\HOST-IMPL-CODEX-01-summary.json:184` through `:205` `codex plugin list --json` exit 1, `send-message-v2 --help` only, `send_message_invoked=false`.
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\HOST-IMPL-CODEX-01-summary.json:235` through `:239` not-proven claims.
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json:5` through `:7` current phase/status/scope.
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json:18` through `:26` commit-scope non-proven claims.
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json:40` through `:47` current/remaining/next bounded work order.
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json:6440` through `:6463` full UI/app/send-message/list/readiness/token/learnability/final flags.
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json:6490` through `:6499` claims kept closed and next bounded work order.

## Checks run

- `git diff --check` in `C:\Projects\llm_mvp_core`: exit 0.
- `git diff --check` in `C:\Projects\codex-xiigen-smart-skills`: exit 0.
- `git status --short` and `git status --short -uall` in both repos.
- JSON/JSONL parse for Core state and HOST-IMPL-CODEX-01 artifacts: 5 Core JSON/JSONL files parsed successfully in my targeted parse; existing validation covers 39 combined targets.
- JSON/JSONL parse for plugin `reports/HOST-IMPL-CODEX-01`: 34 plugin JSON files parsed successfully.
- Installed cache source/cache SHA256 recomputation for 10 files: all match.
- Direct installed-cache positive/negative smoke request/response/debug-shape inspection.
- `rg` boundary search for `PromptToTask`, `OrchestrationReasoning`, `SkillSelection`, semantic/ranking/scoring/model/training, `TargetLocator`, `RequestedSkills`, `AvailableSkills` in source and installed cache.
- `rg` overclaim search for final readiness, semantic quality claimed, token savings claimed, learnability claimed, marketplace/list/app-runtime/send-message-v2 terms in HOST-IMPL-CODEX-01 evidence/state.

## Commit/push recommendation

Do not commit/push as-is. Repair required first:

1. Regenerate or rewrite both `HOST-IMPL-CODEX-01-evidence-ru.md` / `HOST-IMPL-CODEX-01-summary-ru.md` as readable Russian UTF-8 text with clear run examples, input/output shape, proven/not-proven scope.
2. Remove, quarantine, or ignore the untracked `cursor-user-data/logs` runtime/user-data files outside the HOST-IMPL-CODEX-01 evidence scope.
3. Re-run targeted `git diff --check`, JSON/JSONL parse, and status checks.

After those repairs, the architecture/runtime evidence can be accepted for a parent-owned exact-file commit/push boundary for HOST-IMPL-CODEX-01 artifacts/state/plugin reports only.
