# TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-BASELINE-COMPANION-GENERATE-OR-DEFER-01

## Цель

Получить или безопасно сгенерировать только missing aseline_common_no_adaptive row для TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001 / TOKEN-SCENARIO-PROMPT-TO-TASK-001. Если exact row нельзя получить без нарушения authority, вернуть deferral/missing-input ledger и не создавать pair.

## Preconditions

- Нужна свежая текущая Luba authority на bounded no-external-AI/no-model-call local capture. Старые approvals или state
ext_action не подходят.
- Нельзя запускать Claude model/chat solving, external AI/model calls, training, DPO, quality scoring, checkpoint/model/locator mutation или git writes.
- Если runtime/core-boundary capture не разрешён текущей authority, работа должна завершиться deferral.

## Exact safe method

1. Read TOKEN-01 scenario ledger, TOKEN-02 accounting schema, existing TOKEN-03 adaptive row and current state.
2. Reuse the same redacted prompt contract as the adaptive row: pair id, scenario id, host Claude, method prompt-to-task, evidence class
arrow_live_host_surface, debug_enabled=true, tokenizer/accounting and redaction policy unchanged.
3. Build the baseline request as common/no-adaptive only: model_mode=common_baseline, daptive_enabled=false, user_local_checkpoint_id=null, no adaptive target locator, no adaptive manifest, no target checkpoint, no fresh-load ref.
4. Persist raw/private refs as private refs only; commit only redacted refs and hashes.
5. Persist per-span counts and totals for host_user_prompt_input, xiigen_core_request_payload, xiigen_core_response_payload, debug_trace_visible_payload, and host_visible_output using the same accounting policy.
6. Run same-pair checks against the existing adaptive row. Create 	oken-pair-001.json and 	oken-pair-001.md only if all checks pass.
7. Keep 	oken_savings_claim_allowed=false and product savings claim false until a later same-pair quality non-regression gate passes.

## Required outputs

- If exact row is captured and checks pass: 	oken-pair-001.json, 	oken-pair-001.md, updated matrix/counting/no-overclaim/human/checks/report artifacts.
- If not: 	oken-first-pair-missing-inputs.json, deferral reason, and another bounded next work order.

## Forbidden

No external AI/model calls, no full host UI readiness claim, no quality scoring, no training/DPO, no checkpoint/model/locator mutation, no source/test/docs/skills/AGENTS/CLAUDE edits, and no git add/commit/push.
