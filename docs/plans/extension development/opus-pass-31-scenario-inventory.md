# Pass P31 — Инвентарь dual-path сценариев + покрытие (scenario inventory + coverage)

Дата прохода: 2026-07-08. Модель: Opus 4.8, xhigh. Слайс: перечислить ВСЕ same-scenario
dual-path сценарии по каждому хосту; для каждого — статус Path A (automated boundary) и
Path B (local desktop); свести счётчики покрытия. Заносить ТОЛЬКО новые дыры.

## Что реально проверено (evidence-bound)

Канонический инвентарь сценариев найден: **`product-readiness-scenario-board.json`** —
существует и в пакете
(`supporting-files/evidence-extension-full-readiness-dual-path/product-readiness-scenario-board.json`,
1138 строк), и в РЕАЛЬНОМ репо
(`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\product-readiness-scenario-board.json`).
Обе копии **идентичны**: один `generated_at_utc: 2026-06-27T00:13:32Z`, один `counts.total: 28`,
`full_joined_pass: 0`, `product_readiness_full_pass: 0`, `remaining: 14`.

Последняя активность цепочки (`next-scenario-selection-after-row28-20260627-012724`) в реальном
репо показывает `decision_status: NEEDS_SCENARIO_REPAIR` и `selected_next.scenario_number: 1`
(claude-product-readiness), т.е. исполнение dual-path не продвинулось за 14 scoped-accepted, а
ушло НАЗАД в planning-repair (пере-декомпозиция Row1). Ничего новее 2026-06-27 сценарии не
двигало. Значит счётчики покрытия ниже отражают текущее состояние, а не устаревший снимок.

Дедуп: не повторяю P06 (три join-пакета в пакете — бумажные), P08 (board мис-локализует
deliverable / provenance), P12 (Claude QA-артефакт HOST-08E — механизм), P16 (Cursor headless
Path B), P20 (Codex нет Path B). Здесь — впервые собранный **перечислимый 28-сценарный
инвентарь с per-host счётчиками покрытия** (total/accepted/api_only/ui_only/blocked/not_started),
которого не было ни в одном проходе.

## Полный инвентарь: 9 типов сценариев × хост (плюс 1 лишний у Codex)

Девять типов сценариев (одинаковы для каждого хоста):
1. product-readiness-product-workflow
2. full-extension-entrypoint
3. token-savings-baseline
4. semantic-quality-heldout
5. learnability-training-continue
6. dpo-quality-provenance
7. asset-lifecycle-roundtrip
8. checkpoint-promotion-reload
9. locator-update-reload

Плюс у Codex ЕСТЬ 10-й тип, которого нет у Claude/Cursor: `frdp-p01-codex-codex-chat-ui-marker`
(row 21). Итого 9 (Claude) + 9 (Cursor) + 10 (Codex) = **28**.

### Полная таблица (row | host | scenario_id-суффикс | board_category | Path A | Path B | joined)

| # | Host | Тип (суффикс) | board_category | Path A (automated_core_api) | Path B (local_real_program) | joined |
|---|------|---------------|----------------|------------------------------|------------------------------|--------|
| 1 | Claude | product-readiness-product-workflow | blocked_with_next_action | Needs concrete work (insufficient) | Blocked (no safe local route) | no joined pass |
| 2 | Claude | full-extension-entrypoint | blocked_with_next_action | Accepted boundary-only | Blocked (no marker route) | no joined pass |
| 3 | Claude | token-savings-baseline | blocked_with_next_action | Accepted guard-only (savings not claimed) | Blocked (no host access) | no joined pass |
| 4 | Claude | semantic-quality-heldout | blocked_with_next_action | Accepted guard-only (no improvement claimed) | Blocked (host exec unavailable) | no joined pass |
| 5 | Claude | learnability-training-continue | blocked_with_next_action | Blocked (no non-mutating cmd) | Blocked | no joined pass |
| 6 | Claude | dpo-quality-provenance | blocked_with_next_action | Insufficient partial (loader only) | Blocked | no joined pass |
| 7 | Claude | asset-lifecycle-roundtrip | blocked_with_next_action | Blocked (no lifecycle cmd) | Blocked | no joined pass |
| 8 | Claude | checkpoint-promotion-reload | blocked_with_next_action | Accepted guard-only | Blocked | no joined pass |
| 9 | Claude | locator-update-reload | blocked_with_next_action | Insufficient partial | Blocked | no joined pass |
| 10 | Cursor | product-readiness-product-workflow | not_moved | Needs concrete work | Not run locally (row11 marker only) | not moved |
| 11 | Cursor | full-extension-entrypoint | partial | panel launch + core-response (mojibake) | clean narrow visible (screenshot) | Partial, formal_joined_pass=false |
| 12 | Cursor | token-savings-baseline | partial | Accepted narrow (delta 0, reduction false) | narrow webview submit/result | narrow accepted, formal_joined_pass=false |
| 13 | Cursor | semantic-quality-heldout | accepted_narrow_scoped | scoped (0/4→4/4 staged; perm 12/12) | scoped (TargetLocator UI rendered) | ROW13_SCOPED_ACCEPTED, joined=false |
| 14 | Cursor | learnability-training-continue | accepted_narrow_scoped | scoped (backup/DPO/continue review PASS) | scoped (heldout eval, no UI run) | ROW14_SCOPED_ACCEPTED, joined=false |
| 15 | Cursor | dpo-quality-provenance | scoped_accepted | scoped (audit consistent, 10 rows) | Not run locally | ROW15_SCOPED_ACCEPTED, joined=false |
| 16 | Cursor | asset-lifecycle-roundtrip | partial | scoped review PASS | (per row16 status) | ROW16_SCOPED_ACCEPTED, joined=false |
| 17 | Cursor | checkpoint-promotion-reload | partial | scoped review PASS | (per row17 status) | ROW17_SCOPED_ACCEPTED, joined=false |
| 18 | Cursor | locator-update-reload | partial | scoped review PASS | (per row18 status) | ROW18_SCOPED_ACCEPTED, joined=false |
| 19 | Codex | product-readiness-product-workflow | partial | scoped workflow-boundary comparison | (partial) | ROW19_SCOPED_ACCEPTED, joined=false |
| 20 | Codex | full-extension-entrypoint | partial | scoped execution review | (partial) | ROW20_SCOPED_ACCEPTED, joined=false |
| 21 | Codex | codex-chat-ui-marker (ЛИШНИЙ тип) | scoped_accepted | scoped | scoped blocker | PASS_SCOPED_BLOCKER, joined=false |
| 22 | Codex | token-savings-baseline | scoped_accepted | narrow local no-send token measurement | marker-only (downgrades) | ROW22_ACCEPTED_WITH_DOWNGRADES, joined=false |
| 23 | Codex | semantic-quality-heldout | scoped_accepted | runner-contract only | blocked_not_proven (no approved input) | ROW23_SCOPED_BLOCKER, joined=false |
| 24 | Codex | learnability-training-continue | blocked_with_next_action | Blocked | Blocked | no joined pass |
| 25 | Codex | dpo-quality-provenance | blocked_with_next_action | Blocked | Blocked | no joined pass |
| 26 | Codex | asset-lifecycle-roundtrip | blocked_with_next_action | Blocked | Blocked | no joined pass |
| 27 | Codex | checkpoint-promotion-reload | scoped_accepted | temp-staging checkpoint reload+rollback only | UI path blocker | ROW27_SCOPED_ACCEPTED, joined=false |
| 28 | Codex | locator-update-reload | scoped_accepted | dry-run staging locator update+rollback only | UI path blocker | ROW28_SCOPED_ACCEPTED, joined=false |

## Счётчики покрытия (реально пересчитанные по строкам board_category)

| Категория | Claude (1-9) | Cursor (10-18) | Codex (19-28) | ИТОГО /28 |
|---|---|---|---|---|
| full_joined_pass (Path A+Path B same-scenario) | 0/9 | 0/9 | 0/10 | **0/28** |
| product_readiness_full_pass | 0/9 | 0/9 | 0/10 | **0/28** |
| blocked_with_next_action | 9 | 0 | 3 (24,25,26) | 12 |
| not_moved | 0 | 1 (10) | 0 | 1 |
| partial | 0 | 5 (11,12,16,17,18) | 2 (19,20) | 7 |
| accepted_narrow_scoped | 0 | 2 (13,14) | 0 | 2 |
| scoped_accepted | 0 | 1 (15) | 5 (21,22,23,27,28) | 6 |
| Path B (local desktop) с ЛЮБЫМ визуальным доказательством | 0/9 | ~4/9 (screenshot 11; webview 12,13; +?) | 0/10 (0 pixel в репо, P20) | << 28 |

Итог покрытия: **0 из 28** сценариев достигли full joined pass или product_readiness_full_pass.
14 «accepted» рядов (12,13,14,15,16,17,18,19,20,21,22,23,27,28) — ВСЕ `formal_joined_pass=false`,
narrow/scoped. Claude — 0 из 9 по обеим осям (Path A частично guard-only, Path B = 0). Codex — 0
видимого desktop Path B во всём репо (P20). Настоящая dual-path готовность = **0/28**.

## Расхождение summary-счётчиков board vs строки

Объект `counts` (строки 75-116 board.json) заявляет: `partial: 14`, `blocked_with_next_action: 14`,
`scoped_accepted: 14`, `accepted_narrow_scoped: 14`, `remaining: 14`. Это НЕ совпадает с
фактическими board_category по строкам: blocked=12, partial=7, scoped_accepted=6,
accepted_narrow_scoped=2, not_moved=1. Одновременно 14 partial + 14 blocked + 14 scoped_accepted
не может разложить 28. Плюс объект раздут ~40 кумулятивно-историческими ключами вида
`row12_row13_..._row28_accepted_narrow_scoped: 14`. Это count-carrier-дефект (не нормализованный
вектор покрытия).

## НОВЫЕ дыры (G31-1 … G31-7)

Ниже — сведены в мастер `opus-PROJECT-CLOSURE-PLAN.md` под `## Pass P31`. Каждая с file:line.

- **G31-1** — Dual-path покрытие всех 28 сценариев = 0 accepted (0 full_joined_pass /
  0 product_readiness_full_pass) по СОБСТВЕННОМУ счёту board; 14 «accepted» — все
  formal_joined_pass=false.
- **G31-2** — Канонический 28-сценарный инвентарь не выведен в пакетный status-board 2026-07-08
  (свёрнут в 3 фазовых ряда 5/6/7 «PARTIAL»); per-host вектор покрытия
  (total/accepted/api_only/ui_only/blocked/not_started) отсутствует; сам инвентарь устарел
  (2026-06-27) и застрял в NEEDS_SCENARIO_REPAIR.
- **G31-3** — summary `counts` объект внутренне противоречив (14/14/14 vs строки 12/7/6/2/1) и
  денормализован ~40 кумулятивными ключами.
- **G31-4** — Claude: 0/9 по обеим осям; Path B = 0/9 (все 9 blocked). Перечислимый per-host
  счёт (новая гранулярность vs P12).
- **G31-5** — «same-scenario» не выполнен даже в accepted рядах: board определяет `required_match`
  (тот же промпт / то же поведение / сравнение результата A↔B), но НИ ОДИН ряд не содержит
  поля сравнения Path A↔Path B; «accepted» Cursor ряды сравнивают разное (baseline-only
  measurement vs TargetLocator UI).
- **G31-6** — Асимметричная матрица host×type: Codex=10 (лишний `codex-codex-chat-ui-marker`),
  Claude/Cursor=9; «видимый host UI» тип есть только у Codex → знаменатели покрытия не
  сопоставимы между хостами.
- **G31-7** — Инвентарь mojibake-повреждён в рядах 6, 11, 15 (Windows-1251 «крокозябры» в
  what_scenario_checks/status), source-of-truth сценариев частично нечитаем.

## Что нужно для закрытия слайса

1. Выполнить хотя бы ОДИН настоящий full joined pass на хост: Path A ЧЕРЕЗ команду собранного
   расширения (не in-process fallback) + Path B видимый тот же сценарий, с полем сравнения A↔B.
2. Ввести нормализованный вектор покрытия в пакетный board и синхронизировать дату.
3. Зафиксировать канонический host×type матрикс с фиксированным знаменателем; лишние/дубли
   сделать под-рядами.
4. Пере-кодировать инвентарь в UTF-8; регенерировать повреждённые ряды.
