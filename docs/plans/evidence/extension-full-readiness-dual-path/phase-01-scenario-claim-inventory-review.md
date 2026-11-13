# Phase 01.01 - Scenario And Claim Inventory Review

CURRENT TASK: Review Phase 01.01 scenario/claim inventory as a bounded review sub-agent.
active_task_sentence_source: Luba current unquoted instruction: "Review Phase 01.01 scenario/claim inventory as a bounded review sub-agent."

NOT THE TASK: no runtime/host UI/code/tests/training/checkpoint/locator execution; no inventory edit except typo/whitespace-only issues; no git add/commit/push/checkout/reset/clean; no readiness declaration.

EVIDENCE-ONLY: plan, execution state, execution log, and inventory files are review inputs only. They do not authorize runtime continuation or readiness claims.

Фаза 2 из 17: инвентаризация сценариев и claims. Смысл фазы: превратить broad readiness claims по Claude, Cursor и Codex в проверяемые host/scenario/claim rows, не открывая readiness claims до двухконтурной проверки.
Шаг 01.01 из 01.02: review матрицы сценариев. Смысл шага: проверить, что inventory не скрывает readiness claim и содержит оба пути проверки.
Статус: NEEDS_FIX. Основная форма inventory проходит: 28 строк есть, Claude/Cursor/Codex распределены как 9/9/10, обязательные поля присутствуют в каждой строке, оба пути проверки представлены, runtime/readiness доказательства не заявлены. Но inventory не даёт достаточно явного моста в Phase 02: в нём есть только общая фраза про будущий `Phase 02 schema work`, без row-derived schema/artifact contract, который Phase 02 должна реализовать по этим 28 строкам.
Сделано: прочитаны план, execution state, execution log и inventory; выполнены targeted counts и содержательная проверка по 6 критериям.
В процессе: ничего не запущено; это report-only review.
Осталось: отдать Phase 01.01 на bounded fix, чтобы добавить в inventory явный bridge-to-Phase-02 section. После этого review можно повторить.
Review нашёл: один blocking finding, потому что критерий 6 не закрыт явно в самом inventory artifact.
Что исправлено: создан только этот review report. Inventory не изменялся, потому что дефект не typo/whitespace-only.
Следующее действие: parent should not launch Phase 02 yet; next bounded action is a Phase 01.01 inventory repair limited to adding the explicit Phase 02 bridge section.

## Review Verdict

NEEDS_FIX

Parent can launch Phase 02: NO.

Reason: Phase 02 would need to infer its schema/artifact contract from row fields and the main plan instead of receiving an explicit bridge from the Phase 01.01 inventory artifact.

## Findings

### BLOCKING-01 - Missing explicit Phase 02 schema/artifact bridge

Inventory lines found for Phase 02 are limited to high-level continuation language, including that the next bounded step should turn the list into work orders or `Phase 02 schema work`. The inventory does not explicitly state what contract Phase 02 must implement from the rows.

Required fix in inventory:

- Add a dedicated `Bridge To Phase 02 Evidence Contract` section.
- State that Phase 02 must consume all 28 rows by `scenario_id`, `host`, `claim_id`, and `marker`.
- Define two packet types per row: `automated_boundary_packet` and `local_desktop_packet`.
- Require common fields copied from every row: `scenario_id`, `host`, `claim_id`, `marker`, `path_status`, `evidence_refs`, `remaining`, `downgraded_claim`, `blocker_or_none`, and `redaction_status`.
- Require automated-boundary slots for request/response/trace/token/quality/training/DPO/lifecycle/checkpoint/locator artifacts as applicable to the row's `claim_id`.
- Require local-desktop slots for activation method, visible marker, screenshot/video/log evidence, blocker details, privacy redaction status, and remaining local work.
- Require EvidenceJoiner keys: `scenario_id` plus `marker`, with host and claim mismatch rejection.
- Require status vocabulary to remain `NOT_RUN`, `NEEDS_VERIFICATION`, `NOT_DISCOVERED_YET`, `NOT_CAPTURED`, or an explicit future blocker/downgrade until real evidence exists.
- State that schema existence does not prove product readiness, full extension readiness, Codex chat UI readiness, token savings, semantic quality, learnability/training, DPO quality, checkpoint promotion, or locator update.

This is not a typo/whitespace issue, so this review did not edit the inventory.

## Passing Criteria

| Criterion | Verdict | Evidence |
|---|---|---|
| 1. Inventory has 28 rows: Claude 9, Cursor 9, Codex 10 | PASS | Targeted counts: row_headings 28; Claude 9; Cursor 9; Codex 10. |
| 2. Claim families include all required families, Codex-only chat UI | PASS | product-readiness 3, full-extension 3, codex-chat-ui 1, token-savings 3, semantic-quality 3, learnability-training 3, dpo-quality 3, asset-lifecycle 3, checkpoint-promotion 3, locator-update 3. |
| 3. Every row has required fields | PASS | Each required field count is 28: scenario_id, host, claim_id, marker, Automated boundary status, Automated boundary remaining, Local desktop status, Local desktop activation, Local desktop evidence required, Local desktop remaining, evidence, remaining, downgraded_claim. |
| 4. Rows remain NOT_RUN/NEEDS_VERIFICATION/NOT_DISCOVERED_YET; no readiness opened | PASS | Every row has `Automated boundary status: NOT_RUN`, `Local desktop status: NOT_RUN`, `Local desktop activation` with `NOT_DISCOVERED_YET`, and non-claim downgraded wording. |
| 5. Human-readable enough for Luba | PASS | Russian-first status block includes phase/step meaning, counts, done/in-process/remaining, review result, fixed/remaining/next-action text. |
| 6. Useful bridge to Phase 02 | NEEDS_FIX | Inventory mentions future `Phase 02 schema work`, but lacks explicit schema/artifact contract derived from the rows. |

## Verification Output

Targeted counts:

```text
row_headings: 28
claude_headings: 9
cursor_headings: 9
codex_headings: 10
scenario_id_fields: 28
host_fields: 28
claim_id_fields: 28
marker_fields: 28
automated_boundary_status: 28
automated_boundary_remaining: 28
local_desktop_status: 28
local_desktop_activation: 28
local_desktop_evidence_required: 28
local_desktop_remaining: 28
evidence_fields: 28
remaining_fields: 28
downgraded_claim_fields: 28
product-readiness: 3
full-extension: 3
codex-chat-ui: 1
token-savings: 3
semantic-quality: 3
learnability-training: 3
dpo-quality: 3
asset-lifecycle: 3
checkpoint-promotion: 3
locator-update: 3
```

Human-readable search:

```text
Фаза 2 из 17: FOUND
Смысл фазы: FOUND
Шаг 01.01 из 01.02: FOUND
Смысл шага: FOUND
Сделано: FOUND
В процессе: FOUND
Осталось: FOUND
Review нашёл: FOUND
Что исправлено: FOUND
Следующее действие: FOUND
Phase 02 schema: FOUND
Automated boundary: FOUND
Local desktop: FOUND
```

Phase 02 bridge search result:

```text
Inventory contains high-level mentions of `Phase 02 schema work`, `Automated boundary`, and `Local desktop`.
Inventory does not contain a dedicated row-derived schema/artifact contract for Phase 02.
```

`git diff --check`:

```text
No output. Exit code 0.
```

## Changed Files

- `docs/plans/evidence/extension-full-readiness-dual-path/phase-01-scenario-claim-inventory-review.md`

No inventory file was changed. No git write/ref action was used.
