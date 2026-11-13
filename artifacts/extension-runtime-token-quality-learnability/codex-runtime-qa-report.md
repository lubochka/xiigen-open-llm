# RUNTIME-04 Codex Runtime QA Report

Verdict: **PASS_RUNTIME_PROOF_CANDIDATE_WITH_LIMITATIONS**

## What This Proves

- Codex CLI runtime is available: `codex-cli 0.140.0-alpha.2`.
- `codex exec` ran fresh Codex runtime sessions from `C:\Projects\codex-xiigen-smart-skills`.
- The runtime sessions executed `runtime04-positive-probe.ps1` and `runtime04-error-probe.ps1`.
- Those scripts invoked the active Codex JS thin adapter CLI: `src/codex-entrypoint.js`.
- The adapter called `llm_mvp_core\tools\xiigen-extension-core-api.ps1` and produced `XiigenExtensionResponse` evidence.
- Positive path produced visible answer/debug output with the required redacted debug shape:

```text
xiigen prompt-to-task :
input: <redacted>
output: <redacted>
```

- Controlled error path produced a missing-core error artifact with redacted visible error text.

## What This Does Not Prove

- It does not prove full Codex plugin chat UI invocation.
- It does not prove token savings, quality improvement, semantic quality, trained-model readiness, DPO generation, checkpoint mutation, fresh-load, continue-training, or rollback.
- It does not prove SkillSelection end-to-end through Codex yet, because this runtime pass found a Codex adapter parser bug.

## Exact Runtime Path

```text
C:\Users\lubao\AppData\Local\OpenAI\Codex\bin\f1c7ee7a13db5fed\codex.exe exec --json --dangerously-bypass-approvals-and-sandbox -C C:\Projects\codex-xiigen-smart-skills --add-dir C:\Projects\llm_mvp_core -o <last-message> -
```

Inside Codex runtime:

```text
powershell.exe -NoProfile -ExecutionPolicy Bypass -File <run>\runtime04-positive-probe.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File <run>\runtime04-error-probe.ps1
```

Inside each probe script:

```text
node.exe .\src\codex-entrypoint.js ... --core-root C:\Projects\llm_mvp_core ...
```

## Evidence

Run folder: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342`

Positive scenario:

- correlation id: `codex-runtime04-positive-scripted-20260616-123342`
- request: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\positive-scripted\xiigen-codex-core-request.json`
- response: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\positive-scripted\xiigen-codex-core-response.json`
- trace: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\positive-scripted\xiigen-codex-core-trace.json`
- visible answer: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\positive-scripted\visible-answer.txt`
- visible debug: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\positive-scripted\visible-debug.txt`
- Codex JSONL stream: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\codex-runtime04-positive-scripted-rerun.stdout.jsonl`

Controlled error scenario:

- correlation id: `codex-runtime04-error-scripted-20260616-123342`
- request: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\error-scripted\xiigen-codex-core-request.json`
- visible error: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\error-scripted\visible-error.txt`
- Codex JSONL stream: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-123342\codex-runtime04-error-scripted-rerun.stdout.jsonl`

## Boundary Review

- Core logic stayed inside `llm_mvp_core` for this probe.
- Codex host side acted as a thin adapter plus evidence script.
- No host-side semantic ranking, DPO generation, training, or checkpoint mutation was observed.
- External AI/model-backed Codex execution was used: **true**, with prior authorization for Codex runtime probing.

## Finding Found During Runtime QA

High severity: `src/codex-entrypoint.js` does not map `--available-skills-json` in `optionsFromCliArgs`. The probe script passed `available-skills-json`, but the produced `XiigenExtensionRequest` still had `AvailableSkills: []`.

Human meaning: the Codex adapter can reach core, but it cannot yet honestly test SkillSelection through the CLI path because the available skills are dropped before core sees them.

Required next repair: add `availableSkills: parseJsonOrDefault(parsed['available-skills-json'], [])` to the Codex CLI parser, rerun adapter tests, then rerun RUNTIME-04. This phase did not change production code because RUNTIME-04 scope allowed runtime QA artifacts/state only.

## Next Independent Track

Recommended before token/quality runs: **HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER**. If not repaired first, TOKEN-01 can still draft the scenario pack, but Codex SkillSelection runtime rows must be marked invalid until the parser repair passes.
