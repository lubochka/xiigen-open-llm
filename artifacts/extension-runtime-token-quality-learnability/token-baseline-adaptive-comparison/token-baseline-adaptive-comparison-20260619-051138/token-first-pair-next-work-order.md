# TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-BASELINE-COMPANION-01

## Цель

Закрыть только недостающий baseline_common_no_adaptive counterpart для `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001`, если это можно сделать безопасно и честно. Если exact counterpart не найден или не разрешен текущей authority, вернуть missing-input ledger и не создавать complete pair.

## Allowed inputs

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-051138/*`
- `artifacts/extension-runtime-token-quality-learnability/token-01-scenario-pairing-prep/token-01-scenario-pairing-prep-20260618-221337/scenario-pair-ledger.json`
- `artifacts/extension-runtime-token-quality-learnability/token-02-trace-accounting-schema/token-02-trace-accounting-schema-20260618-222556/trace-accounting-schema.json`
- `artifacts/extension-runtime-token-quality-learnability/token-03-narrow-trace-capture-or-defer/token-03-narrow-trace-capture-or-defer-20260618-224207/token-traces.jsonl`
- Claude runtime evidence packet refs already named by TOKEN-03.

## Allowed write scope

- New timestamped folder under `artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Required outputs

- `token-pair-001.json` and `token-pair-001.md` only if the baseline counterpart and existing adaptive row pass all same-pair checks.
- Otherwise `token-first-pair-missing-inputs.json` and a repaired next work order.
- Updated `token-comparison-matrix.json`.
- `token-counting-method.json`, `token-no-overclaim.json`, `human-status-board.json`, `checks-report.json`.

## Acceptance checks for pair creation

- Same `pair_id`: `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001`.
- Same `scenario_id`: `TOKEN-SCENARIO-PROMPT-TO-TASK-001`.
- Same host, method, evidence class, debug setting, tokenizer/accounting version, counted spans policy and redaction policy.
- Baseline side is truly common/no-adaptive: `model_mode=common_baseline`, `adaptive_enabled=false`, no user-local checkpoint.
- Adaptive side is the existing current TOKEN-03 row or a successor with equal or stronger evidence tied to the same pair id.
- Raw/private refs, redacted refs, per-span counts and total counts are persisted.
- `token_savings_claim_allowed=false` unless a later same-pair quality gate passes. Pair creation alone is not a product savings claim.

## Forbidden

- No external AI/model calls.
- No training, DPO generation, checkpoint/model/locator/target mutation.
- No source/test/docs/skills/AGENTS/CLAUDE edits.
- No quality scoring in this work order.
- No git add/commit/push.
- No product savings, runtime readiness, quality, learnability, target promotion, locator update, final readiness or product readiness claim.

## Verification

- Parse every created/updated JSON plus state JSON.
- Run `git diff --check -- <new folder> docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
- Scan updated state human status fields and new artifacts for the four-question-mark mojibake sequence.
- Scan for positive savings/readiness wording or claim locks set true.
- Ensure only allowed paths changed.
