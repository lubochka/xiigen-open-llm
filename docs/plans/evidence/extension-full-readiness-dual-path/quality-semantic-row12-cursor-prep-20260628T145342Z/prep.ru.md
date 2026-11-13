# Подготовка semantic-quality eval для Cursor row12

Статус: `PASS_PREP_READY_TO_SCORE`.

Папка подготовки не запускает eval, не вызывает внешнюю AI, не меняет код, governance, skills, checkpoint или locator, не делает commit/push.

## Вопрос eval

Оценить узкое качество уже существующего Row12 Cursor Path A core-output для того же сценария:

- prompt -> universal task: синтетический prompt должен быть преобразован в универсальную задачу с `task_id`, `task_title`, `task_type`, constraints и acceptance gates.
- task -> universal orchestration plan: задача должна стать универсальным orchestration plan с 3 ordered steps и verification gate для каждого шага.
- task/plan -> skills: выбранные skills должны быть допустимым подмножеством разрешенной surface: `prompt-to-task`, `task-to-plan`, `task-to-skills`, `evidence-ledger`.

Это не вопрос "доказана ли полная semantic quality". Полная semantic quality не заявляется. Eval может дать только узкий verdict по одному Row12 Cursor output и должен отдельно записать ограничения: нет held-out/source-disjoint набора, нет before/after quality comparison, нет ablation/no-memorization proof, нет training или checkpoint mutation.

## Ожидаемые выходы и rubric

Eval worker должен создать отдельный bounded eval packet и поставить один из verdict:

- `PASS_NARROW_ROW12_SEMANTIC_EVAL`: все структурные и смысловые checks ниже проходят для Path A core-response.
- `FAIL_ROW12_SEMANTIC_EVAL`: core-response есть, но хотя бы один обязательный check не проходит.
- `BLOCKED_MISSING_OUTPUT`: отсутствует нужный source JSON или nested JSON не парсится.

Pass/fail checks:

1. Mapping: `scenario_id`, `pair_id`, marker и prompt hash совпадают с join/token/status board.
2. Source boundary: scoring uses Path A core request/response/trace only for semantic output; Path B screenshots are route evidence only.
3. Prompt-to-task: nested `prompt_to_task` JSON parses and has task id, title, type, constraints, deliverables, acceptance gates, raw prompt redaction/hash, and explicit non-claims.
4. Task type: `task_type` is compatible with the synthetic request. For current evidence, expected narrow type is `plan_only_or_approval_gated_task`; a different type requires a written failure or rationale.
5. Universal plan: nested `task_to_plan` JSON parses, has exactly 3 ordered steps, each step has `uses` and one verification gate, and the plan is universal/approval-gated rather than a Cursor-only implementation script.
6. Skills: nested `task_to_skills` JSON parses; selected skills are exactly from the allowed set and include the required route skills. No unapproved skill may appear.
7. Non-claims: output keeps `semantic_quality_claimed=false`, `token_savings_claimed=false`, `readiness_claimed=false`, `training_started=false`, `checkpoint_mutation=false`, `locator_update=false`, `external_ai_used=false`.
8. No deterministic shortcut or position leakage: deterministic, baseline, nearest-overlap, candidate-order, candidate-position, regex/dictionary/counter/KNN/BM25/static-fixture evidence may be accepted only when labeled as diagnostic/baseline/eval/internal and not final semantic authority. Fail if hidden dictionary/scorer, exact prompt replay, candidate position, or candidate order is treated as semantic authority.
9. No masquerade: token delta, Path B UI route, screenshot presence, existing checkpoint, or status-board row may not be scored as semantic quality.
10. Overclaim lock: report must say the evidence supports only a narrow Row12 Cursor semantic-output eval, not full semantic quality, token savings, training, common readiness, target promotion, all-extension readiness, or full product readiness.

## Источники использовать

Use as scoring/mapping sources:

- `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-frdp-p01-row12-cursor-token-baseline-v1_FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1_frdp-p01-cursor-token-savings-baseline-positive_request-request-2026-06-28T13-50-15-212Z.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-frdp-p01-row12-cursor-token-baseline-v1_FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1_frdp-p01-cursor-token-savings-baseline-positive_request-core-response-2026-06-28T13-50-15-212Z.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-frdp-p01-row12-cursor-token-baseline-v1_FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1_frdp-p01-cursor-token-savings-baseline-positive_request-core-trace-2026-06-28T13-50-15-212Z.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/row12-host-visible-runner-report.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-joined-path-a-b-20260628T142044Z/join-report.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/token-row12-cursor-review-20260628T143616Z/review-report.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/status-after-row12-token-review-20260628-174621/status-board.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-path-b-real-ui-20260628T140709Z/path-b-ui-report.json` only for Path B route boundary and non-submit fact.

Do not use:

- external AI, internet, model/vendor calls, hidden evaluator service, or raw external AI output;
- hidden deterministic dictionary/scorer while calling it semantic quality;
- Path B screenshots as core semantic output;
- token delta as quality proof;
- old ledgers/checklists/status rows as completed eval cycles;
- training/checkpoint/locator artifacts as quality proof.

## Следующее executable bounded action

Worker: `QUALITY-SEMANTIC-ROW12-CURSOR-EVAL-01`.

Allowed scope: read the sources above, write one eval folder under `docs/plans/evidence/extension-full-readiness-dual-path/quality-semantic-row12-cursor-eval-<timestamp>/`, and run read-only JSON parsing plus rubric checks. No code/governance/skills edits. No commit/push. No external AI. No training/checkpoint/locator mutation.

Obvious starting commands:

```powershell
$Core = Get-Content -LiteralPath 'docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-frdp-p01-row12-cursor-token-baseline-v1_FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1_frdp-p01-cursor-token-savings-baseline-positive_request-core-response-2026-06-28T13-50-15-212Z.json' -Raw | ConvertFrom-Json
$Task = $Core.CheckSetResults.prompt_to_task | ConvertFrom-Json
$Plan = $Core.CheckSetResults.task_to_plan | ConvertFrom-Json
$Skills = $Core.CheckSetResults.task_to_skills | ConvertFrom-Json
$Task | ConvertTo-Json -Depth 20
$Plan | ConvertTo-Json -Depth 20
$Skills | ConvertTo-Json -Depth 20
```

If these nested parses fail, verdict is `BLOCKED_MISSING_OUTPUT` with exact field/file. If they pass, apply the rubric above and write `eval.ru.md` plus `eval.json`.

## Overclaim locks

- This prep does not prove semantic quality.
- This prep does not execute semantic eval.
- This prep does not prove token savings; token review showed `6209 -> 6209`, delta `0`.
- This prep does not prove readiness, training, checkpoint promotion, locator update, learnability, all-extension readiness, or full product readiness.
- `READY_TO_SCORE` means only that Path A core-response appears available for the next bounded scoring worker.

## Mapping to 8-group status board

Current board remains:

1. Cursor row12 dual-path same-scenario coverage: `done_narrow`.
2. All-extensions readiness: `remaining`.
3. Token savings: `reviewed_not_proven`.
4. Semantic quality: still `remaining`, with this prep as next-action evidence only.
5. Training and learnability: `remaining`.
6. Backup/export/import/share/continue-training lifecycle: `remaining`.
7. Checkpoint promotion and locator update: `remaining`.
8. Full product readiness: `remaining`.

If the next eval passes, only group 4 may move to a narrow row12-scored status such as `reviewed_narrow_row12_only`; it still must not become full semantic quality without held-out/source-disjoint scoring, before/after comparison, ablation/no-leakage/no-memorization controls, and regression guard.
