# Phase 00/01 Human-Readable Status Board

Русский статус для Luba: мы не доказывали продукт и не запускали host UI. Мы
поставили план на рельсы выполнения и затем починили контракт отчетности, чтобы
дальше нельзя было писать только opaque codes вроде `Phase 01.01 Scenario And
Claim Inventory` без человеческого объяснения.

Фаза 2 из 17: Phase 01 - Scenario and claim inventory. Смысл фазы:
превратить широкие readiness claims по Claude/Cursor/Codex в конкретные
host/scenario/claim rows.

Шаг 01.00 из 01.02: Human-readable status and reporting contract repair. Смысл
шага: сделать текущую и будущую отчетность понятной Luba: что решает фаза, что
делает шаг, сколько сделано, что нашел review, что исправлено, что осталось, и
какое bounded действие идет дальше.

Статус: DONE. Reporting contract repair completed for this slice; Phase 01.01
scenario/claim inventory remains NOT_STARTED.

Сделано: 1 из 17 фаз полностью (Phase 00) и 1 из 2 tracked Phase 01 steps
(`01.00` status/reporting model and repair). В процессе: Phase 01 active, but
no Phase 01.01 executor is running in this packet. Осталось: 15 full phases
(Phase 02-Phase 16) plus 1 Phase 01 step (`01.01` inventory).

Review нашёл: прежняя board/log/state wording still allowed machine-only labels
and did not require `Фаза X из Y`, `Смысл фазы`, `Шаг X.Y`, `Смысл шага`,
actual counts, review/fix result, next action, or commit/continuation boundary.

Что исправлено: this board, state/log, plan template, skills, AGENTS/CLAUDE, and
next bounded work order now require the full Russian-first contract before
opaque codes.

Что осталось: parent review of this repair; then Phase 01.01 inventory can be
issued as a bounded sub-agent work order. Product/runtime/host/training/
checkpoint/locator work remains outside this slice.

Следующее действие: parent reviews this reporting repair and, if accepted,
sends the bounded Phase 01.01 inventory work order already stored in
`phase-01-next-bounded-work-order.md`.

Commit/continuation allowed: no git write/ref actions are allowed here.
Continuation into Phase 01.01 is not performed here and is allowed only through
the bounded sub-agent work order after parent review.

opaque codes: allowed only after the human explanation above, never instead of
it.

## Current step

| Step | Что решает | Status | Evidence path |
|---|---|---|---|
| Phase 00.01 | Проверяет, что work starts from current Luba authority, not old packets or stale plans. | done | `docs/plans/evidence/extension-full-readiness-dual-path/phase-00-01-startup-evidence.md` |
| Phase 00.02 | Загружает plan, ledger, AGENTS, CLAUDE and relevant startup/claim/plugin/session skills. | done | `docs/plans/evidence/extension-full-readiness-dual-path/phase-00-01-startup-evidence.md` |
| Phase 00.03 | Устанавливает no-overclaim boundary: startup evidence is not readiness evidence. | done | `docs/plans/evidence/extension-full-readiness-dual-path/phase-00-01-startup-evidence.md` |
| Phase 01.00 | Создает понятный board and reporting contract: сколько фаз/шагов, что сделано, что сейчас, что осталось, что нашел review, что исправлено, что дальше, и можно ли продолжать/commit. | DONE | this file |
| Phase 01.01 | Создает настоящую scenario/claim inventory matrix for Claude/Cursor/Codex and required claim families. | NOT_STARTED; next bounded work order only | `docs/plans/evidence/extension-full-readiness-dual-path/phase-01-next-bounded-work-order.md` |

## Phase board

| # | Phase | Что решает | Status |
|---|---|---|---|
| 1 | Phase 00 | Удерживает authority, scope, skills and no-git boundary before execution. | done |
| 2 | Phase 01 | Делает inventory of hosts, scenarios and claims. Opening/status/reporting-contract part is done; full inventory remains. | IN_PROGRESS: 01.00 DONE, 01.01 NOT_STARTED |
| 3 | Phase 02 | Создает evidence contract/schema so future evidence can be joined. | remaining |
| 4 | Phase 03 | Проверяет automated boundary before host UI. | remaining |
| 5 | Phase 04 | Проверяет real local Claude/Cursor/Codex paths or records blockers. | remaining |
| 6 | Phase 05 | Соединяет evidence into product readiness matrix. | remaining |
| 7 | Phase 06 | Проверяет full extension readiness separately for each host. | remaining |
| 8 | Phase 07 | Проверяет real Codex chat UI path or honest downgrade. | remaining |
| 9 | Phase 08 | Проверяет token savings with stable workload and quality guard. | remaining |
| 10 | Phase 09 | Проверяет semantic quality on held-out rubric cases. | remaining |
| 11 | Phase 10 | Проверяет learnability/training/DPO quality without provenance leakage. | remaining |
| 12 | Phase 11 | Проверяет lifecycle: backup/export/import/share/continue-training/rollback. | remaining |
| 13 | Phase 12 | Проверяет checkpoint mutation/promotion and locator update. | remaining |
| 14 | Phase 13 | Проверяет, что improvements are not hidden deterministic shortcuts. | remaining |
| 15 | Phase 14 | Делает readable reports/dashboard linked to evidence. | remaining |
| 16 | Phase 15 | Выполняет future execution review/fix closure when real evidence exists. | remaining |
| 17 | Phase 16 | Propagates accepted evidence-backed requirements to docs/skills/guides/agent files. | remaining |

## next bounded work order

Следующее действие: Phase 01.01 scenario and claim inventory as a bounded
sub-agent work order. Оно должно создать матрицу сценариев и claims, но все
boundary/local evidence rows должны остаться `NOT_RUN` или
`NEEDS_VERIFICATION` до Phase 02+.
