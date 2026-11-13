# TOKEN-01-SCENARIO-PAIRING-PREP-01 - scenario/pair ledger

Generated: 2026-06-18T22:13:37+03:00
Branch: codex/chat-gpt-5.5
Run root: artifacts/extension-runtime-token-quality-learnability/token-01-scenario-pairing-prep/token-01-scenario-pairing-prep-20260618-221337

## Доска: всего / сделано / сейчас / осталось

| Поле | Статус |
|---|---|
| Всего | Нужно доказать token before/after для Claude/Cursor/Codex только через валидные same-pair baseline/adaptive строки и quality non-regression для того же pair_id. |
| Сделано | Подготовлен scenario/pair ledger: 9 строк, по 3 метода на Claude/Cursor/Codex, с baseline/adaptive ids, evidence-class требованиями, доступными refs, missing prereqs, target/adaptive refs needed, accounting field, redaction и quality gate. Runtime/eval/training не запускались. |
| Сейчас | Token savings claim закрыт. Claude и Codex можно готовить только как narrow adapter/core-boundary accounting prep; Cursor остается ledger/schema-only до safe launch contract. |
| Осталось | Следующий bounded work order: TOKEN-02-TRACE-ACCOUNTING-SCHEMA-01, freeze tokenizer/accounting/counting spans/redaction без runtime/eval/training. После этого отдельный prereq check решит, какие host pairs можно capture. |

## Результат ledger

Создан scenario-pair-ledger.json со строками для Claude, Cursor и Codex по методам prompt-to-task, orchestration-plan, skill-selection. Для каждой строки есть pair_id, baseline_pair_id, adaptive_pair_id, placeholder сценария, evidence-class requirement, available evidence refs, missing prerequisites, target/adaptive refs needed, token accounting version field, redaction requirement и quality non-regression prerequisite.

## Host status

| Host | Текущий scope | Статус пары | Что можно делать сейчас |
|---|---|---|---|
| Claude | narrow_live_host_surface | Full Claude Desktop/chat UI не доказан. | Только narrow direct hook/core-boundary accounting prep; без live UI savings claim. |
| Cursor | blocked_deferred_cursor_relaunch | Safe launch/no-backend/no-model-metadata contract отсутствует; Cursor full UI proof deferred/blocked. | Только ledger/schema prep; runtime capture и narrow accounting capture не открыты. |
| Codex | installed_skill_surface_plus_direct_installed_plugin_cache_adapter_to_llm_mvp_core_core_boundary | Full Codex desktop chat UI/app не доказан. | Только scoped installed skill/plugin-cache adapter -> core-boundary accounting prep; без full Codex UI savings claim. |

## Claim locks

Все readiness и improvement claims остаются false: token savings, quality improvement, semantic quality, learnability, live/full host readiness, target promotion, locator update, final readiness и product readiness. Token delta, если появится позже, будет только cost/trace metric и не станет claim до same-pair quality non-regression PASS.

## Next bounded work order

TOKEN-02-TRACE-ACCOUNTING-SCHEMA-01: freeze trace/accounting schema, tokenizer/accounting version, counted spans and redaction policy. Запрещены runtime host execution, eval, quality scoring, training, DPO, checkpoint/locator mutation, external AI/model calls and git.
